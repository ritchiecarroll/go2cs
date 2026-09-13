// platformCensus.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the MULTI-PLATFORM EMISSION CENSUS — increment 1 of
// docs/phase4/DESIGN-multiplatform-corpus.md (ACCEPTED 2026-08-08: layout L3 + packaging (a)).
//
// What it does, and what it deliberately does NOT do:
//
//	DOES     teach -platforms a LIST of os/arch targets; run the standard-library pipeline once per
//	         target into its own isolated, SEEDED staging root; classify every emitted artifact as
//	         shared / variant / partial / exclusive by comparing the EMISSIONS; write one manifest.
//	DOES NOT change a single byte of any emitter. No corpus file is written, no .cs moves, no csproj
//	         gains a conditioned item. The layout change that consumes this classification is
//	         increment 2/3; this increment exists so the classification can be PROVEN against the
//	         design's measured numbers before a corpus file moves.
//
// Three decisions in here are load-bearing and each has a reason the design states explicitly:
//
//  1. **The axis is computed from the EMISSION, never from the Go file sets** (design §4.2). Four
//     packages have identical Go source on all three platforms and different emitted C# (constant
//     folding, escape analysis, cross-file collision renaming, dead-branch folding); three have
//     differing Go source and identical Windows↔Linux emission. A census over `go list` output would
//     be wrong in both directions, so this one hashes what the converter actually wrote.
//
//  2. **Each staging root is SEEDED** — `core` + `version.props` + `docs/validation`, mirroring the
//     repository's `src/` layout — exactly as CLAUDE.md's reconvert ritual requires. An unseeded root
//     gives the `[module: GoManualConversion]` detector nothing to detect, so all 41 hand-owned files
//     emit as plain `.cs` (16 of them displacing a `.cs.auto` sibling) and a fully hand-owned package
//     additionally emits a `.csproj`/`package_info.cs`/`README.md` it must not. That is a different
//     corpus, and its census numbers would be meaningless. The seeding is done HERE rather than by a
//     calling script so the ritual cannot be skipped, and the marker gate below proves it took.
//
//  3. **Emitted is distinguished from seeded by a SENTINEL MODIFICATION TIME**, not by content. Every
//     seeded file is stamped to a fixed instant before the run; anything the run wrote carries a
//     different one. Content cannot serve here: the whole point of the Windows control is that its
//     emission is byte-identical to the seed, so a content diff would report it as having emitted
//     nothing. (The converter's own project-file writer skips an unchanged .csproj — that write-skip
//     is a real signal, and this is how the census sees it.)
//
// The r41 corruption rule is mechanized rather than remembered: a staging root is removed and
// re-seeded for every run, and the per-target conversions run strictly sequentially in this one
// process, so two converter passes can never overlap in one root.

package main

import (
	"bytes"
	"crypto/sha256"
	"encoding/hex"
	"fmt"
	"io/fs"
	"os"
	"path/filepath"
	"sort"
	"strings"
	"time"
)

const (
	// platformManifestFileName is the census artifact — the ONLY thing increment 1 produces.
	platformManifestFileName = "platform-manifest.json"

	// platformManifestSchema versions the manifest shape so a later increment that consumes it can
	// tell which producer wrote the file it is reading.
	platformManifestSchema = "go2cs.platform-census/1"
)

// censusSeedSentinel is the modification time every seeded file is stamped with. Any file whose
// modification time differs after a conversion was written BY that conversion. The value is
// arbitrary but must be a fixed instant no write can coincidentally reproduce.
var censusSeedSentinel = time.Date(2000, 1, 1, 0, 0, 0, 0, time.UTC)

// parsePlatformList parses a -platforms value into an ordered, deduplicated list of `os/arch`
// targets. Commas and whitespace both separate (matching -tags), duplicates collapse to their first
// occurrence so the FIRST target stays the primary one every single-platform consumer sees, and each
// entry is validated against go/build's own syslist tables — a typo like `darwin/arm` or `linux-amd64`
// is rejected here rather than silently loading the host platform three hours into a census.
func parsePlatformList(value string) ([]string, error) {
	fields := parseBuildTags(value) // same comma/whitespace splitting, no empty fields

	if len(fields) == 0 {
		return nil, fmt.Errorf("no target platform given: -platforms wants at least one os/arch value")
	}

	seen := map[string]bool{}
	targets := make([]string, 0, len(fields))

	for _, field := range fields {
		parts := strings.Split(field, "/")

		if len(parts) != 2 || parts[0] == "" || parts[1] == "" {
			return nil, fmt.Errorf("invalid -platforms value %q: expected os/arch", field)
		}

		if !isKnownGOOS(parts[0]) {
			return nil, fmt.Errorf("invalid -platforms value %q: %q is not a known GOOS", field, parts[0])
		}

		if !isKnownGOARCH(parts[1]) {
			return nil, fmt.Errorf("invalid -platforms value %q: %q is not a known GOARCH", field, parts[1])
		}

		if seen[field] {
			continue
		}

		seen[field] = true
		targets = append(targets, field)
	}

	return targets, nil
}

// platformTargetTag renders a target as the directory-safe `<goos>-<goarch>` name a staging root and
// (from increment 3 on) a per-GOOS source folder are keyed by.
func platformTargetTag(target string) string {
	return strings.ReplaceAll(target, "/", "-")
}

// artifactState is one file's post-conversion state in a staging root: what it holds, and whether
// this run is what put it there.
//
// Two hashes, for two different questions. `hash` is the exact bytes and is what the CROSS-TARGET
// classification compares — both sides there are fresh emissions of the same deterministic converter,
// so any difference between them is a real platform effect. `normalizedHash` strips carriage returns
// and is what the SEED comparison uses, because the seed is a git working tree: `core.autocrlf`
// rewrites the in-string LFs the converter deliberately preserves inside multi-line string literals,
// so a committed file and its own re-emission legitimately differ by line endings alone (CLAUDE.md's
// CRLF phantom; the behavioral golden comparison normalizes for the same reason).
// `logical` is the artifact's path with any per-GOOS folder segment folded away — `<pkg>/<file>`
// whether the tree it was snapshotted from is flat or already in layout L3 (platformLayout.go). The
// CLASSIFICATION is keyed by it, so a census of an L3 corpus reproduces the census of the flat one
// it was built from; the seed comparison and the hand-own marker gate stay on the RAW path, because
// those ask about a file at a location.
type artifactState struct {
	hash           string
	normalizedHash string
	logical        string
	emitted        bool
}

// logicalPath returns the artifact's folded `<pkg>/<file>` path, falling back to the raw path it is
// keyed by. Only snapshotConvertedRoot folds — it is the one producer that can see the whole tree,
// which is what the fold needs to tell a per-GOOS source folder from a nested package. A state built
// anywhere else (the seed census, a synthetic corpus in a test) describes a flat tree, and there the
// raw path IS the logical one.
func (s artifactState) logicalPath(rawPath string) string {
	if len(s.logical) == 0 {
		return rawPath
	}

	return s.logical
}

// hashPair returns the exact and carriage-return-stripped digests of a file's contents.
func hashPair(contents []byte) (exact string, normalized string) {
	sum := sha256.Sum256(contents)
	normalizedSum := sha256.Sum256(stripCarriageReturns(contents))

	return hex.EncodeToString(sum[:]), hex.EncodeToString(normalizedSum[:])
}

// stripCarriageReturns removes every CR byte, so CRLF and LF text hash alike.
func stripCarriageReturns(contents []byte) []byte {
	if !bytes.ContainsRune(contents, '\r') {
		return contents
	}

	return bytes.ReplaceAll(contents, []byte("\r"), nil)
}

// platformEmission is everything one target's conversion contributed to the census.
type platformEmission struct {
	target    string
	goos      string
	goarch    string
	root      string                   // staging root — the -go2cspath of this target's conversion
	artifacts map[string]artifactState // path relative to <root>/core, forward slashes

	packagesQueued int
	packagesFailed int
	failedPackages []string

	// The control column: of the `.cs` this target emitted, how many reproduce the SEED — exactly,
	// modulo line endings only (the autocrlf phantom), or not at all. For a census seeded with the
	// committed corpus the control target must report ZERO genuinely differing; that is the reading
	// that proves the diff instrument is measuring platform effects rather than converter
	// non-determinism (design §2, "the control validates the instrument").
	seedMatchingCs       int
	seedLineEndingOnlyCs int
	seedDifferingCs      int

	seedMarkedFileCount  int      // hand-owned files the seed carried into this root
	markerGateViolations []string // hand-owned paths this run emitted as a plain .cs — must be empty
}

// emittedByExtension returns the emitted paths whose extension matches ext (".cs", ".csproj", …),
// sorted. `.cs.auto` review siblings do not match ".cs" — their extension is ".auto".
func (e *platformEmission) emittedByExtension(ext string) []string {
	var paths []string

	for relPath, state := range e.artifacts {
		if state.emitted && filepath.Ext(relPath) == ext {
			paths = append(paths, relPath)
		}
	}

	sort.Strings(paths)

	return paths
}

// runPlatformCensus is the -platform-census entry point: seed one staging root per target, convert
// into each, classify the emissions against one another, and write the manifest. Returns an error
// only for a census-level fault (a bad seed root, an unwritable staging area); a package that fails
// to convert is recorded per target in the manifest, exactly as a single-platform run records it.
func runPlatformCensus(options Options, censusDir string, packageFilter []string) error {
	targets := options.targetPlatforms

	if len(targets) < 2 {
		return fmt.Errorf("-platform-census needs at least two -platforms targets to compare (got %d)", len(targets))
	}

	seedRoot, err := filepath.Abs(options.go2csPath)

	if err != nil {
		return fmt.Errorf("failed to resolve seed root %q: %w", options.go2csPath, err)
	}

	// A census against an unseeded root measures a different corpus (see the file header), so the
	// seed root must be a real go2cs tree. Fail before spending an hour proving nothing.
	if !isGo2CSRoot(seedRoot) {
		return fmt.Errorf("seed root %q is not a converted go2cs tree (no %s under it); "+
			"-platform-census seeds every staging root from it, so it must be the repository's src root",
			seedRoot, filepath.Join("core", "golib", "golib.csproj"))
	}

	if censusDir, err = filepath.Abs(censusDir); err != nil {
		return fmt.Errorf("failed to resolve census directory: %w", err)
	}

	if err = os.MkdirAll(censusDir, 0755); err != nil {
		return fmt.Errorf("failed to create census directory %q: %w", censusDir, err)
	}

	fmt.Printf("\nMulti-platform emission census\n")
	fmt.Printf("  seed root:  %s\n", seedRoot)
	fmt.Printf("  census dir: %s\n", censusDir)
	fmt.Printf("  targets:    %s\n\n", strings.Join(targets, ", "))

	// The seed is read ONCE: every staging root is a copy of it, so its content hashes and its
	// hand-owned census are the same for every target, and both are needed BEFORE the first
	// conversion overwrites anything.
	seed, err := readCensusSeed(filepath.Join(seedRoot, "core"))

	if err != nil {
		return err
	}

	fmt.Printf("  seed holds %d files, %d of them hand-owned ([module: GoManualConversion])\n\n",
		len(seed.hashes), len(seed.markedFiles))

	emissions := make([]*platformEmission, 0, len(targets))

	// STRICTLY SEQUENTIAL, one fresh root each — the r41 rule, mechanized.
	for i, target := range targets {
		fmt.Printf("=== [%d/%d] census target %s\n", i+1, len(targets), target)

		emission, err := runCensusTarget(options, seedRoot, censusDir, target, packageFilter, seed)

		if err != nil {
			return fmt.Errorf("census target %s failed: %w", target, err)
		}

		emissions = append(emissions, emission)
	}

	manifest := buildPlatformManifest(options, seedRoot, censusDir, emissions)
	manifestPath := filepath.Join(censusDir, platformManifestFileName)

	if err := writePlatformManifest(manifestPath, manifest); err != nil {
		return err
	}

	printPlatformCensusSummary(manifest, manifestPath)

	return nil
}

// runCensusTarget seeds a fresh staging root for one target, converts the standard library into it,
// and returns the artifact state the classification reads.
func runCensusTarget(options Options, seedRoot, censusDir, target string, packageFilter []string, seed *censusSeed) (*platformEmission, error) {
	parts := strings.Split(target, "/")
	stageRoot := filepath.Join(censusDir, platformTargetTag(target))
	outputRoot := filepath.Join(stageRoot, "src")

	// Never convert twice into the same temp root (r41): the previous contents go, wholesale.
	if err := os.RemoveAll(stageRoot); err != nil {
		return nil, fmt.Errorf("failed to clear staging root %q: %w", stageRoot, err)
	}

	seeded, err := seedCensusRoot(seedRoot, stageRoot)

	if err != nil {
		return nil, err
	}

	fmt.Printf("  seeded %d files into %s\n", seeded, stageRoot)

	if err := stampSeedTimes(stageRoot); err != nil {
		return nil, err
	}

	targetOptions := options
	targetOptions.targetPlatform = target
	targetOptions.targetPlatforms = []string{target}
	targetOptions.go2csPath = outputRoot

	converter := NewStdLibConverter(targetOptions)

	if err := converter.ScanAndConvertFiltered(packageFilter); err != nil {
		return nil, fmt.Errorf("standard library conversion failed: %w", err)
	}

	artifacts, err := snapshotConvertedRoot(filepath.Join(outputRoot, "core"))

	if err != nil {
		return nil, err
	}

	emission := &platformEmission{
		target:              target,
		goos:                parts[0],
		goarch:              parts[1],
		root:                outputRoot,
		artifacts:           artifacts,
		packagesQueued:      converter.totalCount,
		failedPackages:      readFailedPackages(outputRoot),
		seedMarkedFileCount: len(seed.markedFiles),
	}

	emission.packagesFailed = len(emission.failedPackages)
	emission.markerGateViolations = markerGateViolations(seed.markedFiles, artifacts)
	emission.seedMatchingCs, emission.seedLineEndingOnlyCs, emission.seedDifferingCs =
		compareEmissionToSeed(artifacts, seed.hashes)

	fmt.Printf("  emitted %d .cs (%d reproduce the seed, %d line-endings-only, %d differ), %d .csproj; marker-gate violations %d\n\n",
		len(emission.emittedByExtension(".cs")), emission.seedMatchingCs, emission.seedLineEndingOnlyCs,
		emission.seedDifferingCs, len(emission.emittedByExtension(".csproj")), len(emission.markerGateViolations))

	return emission, nil
}

// censusSeed is the state of the tree every staging root is copied from, read once before the first
// conversion overwrites any of it.
type censusSeed struct {
	hashes      map[string]artifactState // path relative to core, forward slashes; emitted is unused here
	markedFiles []string                 // the hand-owned ([module: GoManualConversion]) subset
}

// readCensusSeed hashes the seed tree and takes its hand-owned census in one walk. The marker test
// is the converter's OWN predicate — the same call the conversion makes when it decides whether to
// divert a file's emission to `.cs.auto` — so the gate asks exactly the question the conversion
// asked, not a lookalike regex. (A line-anchored grep answers it too; an unanchored one reports the
// bodyless-partial placeholder COMMENTS in reflect/value.cs and internal/reflectlite/value.cs as
// hand-owns and turns the gate into a false alarm.)
func readCensusSeed(coreDir string) (*censusSeed, error) {
	seed := &censusSeed{hashes: map[string]artifactState{}}

	err := filepath.WalkDir(coreDir, func(filePath string, entry fs.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if entry.IsDir() || !entry.Type().IsRegular() {
			return nil
		}

		contents, err := os.ReadFile(filePath)

		if err != nil {
			return err
		}

		relPath, err := filepath.Rel(coreDir, filePath)

		if err != nil {
			return err
		}

		relPath = filepath.ToSlash(relPath)
		exact, normalized := hashPair(contents)
		seed.hashes[relPath] = artifactState{hash: exact, normalizedHash: normalized}

		if filepath.Ext(filePath) != ".cs" {
			return nil
		}

		isManual, err := containsManualConversionMarker(filePath)

		if err != nil {
			return err
		}

		if isManual {
			seed.markedFiles = append(seed.markedFiles, relPath)
		}

		return nil
	})

	sort.Strings(seed.markedFiles)

	return seed, err
}

// compareEmissionToSeed splits a run's emitted `.cs` three ways against the tree it was seeded from:
// reproduced exactly, reproduced except for line endings, and genuinely different (a file the seed
// did not hold counts as different — it is new output).
//
// The middle bucket is not noise to be hidden: the seed is a git working tree, and `core.autocrlf`
// rewrites the LFs the converter deliberately preserves inside multi-line string literals, so a
// corpus file holding one differs from its own faithful re-emission by carriage returns alone. Naming
// that count keeps the CONTROL reading honest — a target whose emission reproduces the committed
// corpus reports ZERO in the third bucket, which is the reading that certifies the whole instrument —
// while still showing how much of the tree the phantom touches.
func compareEmissionToSeed(artifacts map[string]artifactState, seed map[string]artifactState) (matching int, lineEndingOnly int, differing int) {
	for relPath, state := range artifacts {
		if !state.emitted || filepath.Ext(relPath) != ".cs" {
			continue
		}

		seedState, ok := seed[relPath]

		switch {
		case ok && seedState.hash == state.hash:
			matching++
		case ok && seedState.normalizedHash == state.normalizedHash:
			lineEndingOnly++
		default:
			differing++
		}
	}

	return matching, lineEndingOnly, differing
}

// seedCensusRoot reproduces CLAUDE.md's reconvert seeding ritual into a staging root, MIRRORING the
// repository's own layout — `<stage>/src/core`, `<stage>/src/version.props`, `<stage>/docs/validation`
// — because that is the shape the converter's two upward walks expect (the project-reference root is
// the directory holding `core/golib`; the README validation badge reads `version.props` from that
// root and `docs/validation/current` from its SIBLING `docs`). Returns the number of files copied.
func seedCensusRoot(seedRoot, stageRoot string) (int, error) {
	outputRoot := filepath.Join(stageRoot, "src")
	copied := 0

	n, err := copyDirTree(filepath.Join(seedRoot, "core"), filepath.Join(outputRoot, "core"))

	if err != nil {
		return copied, fmt.Errorf("failed to seed core tree: %w", err)
	}

	copied += n

	// version.props pins the published version the badge's proof URL carries; without it the badge
	// is omitted and every README differs from the committed one for a reason that has nothing to do
	// with the platform.
	n, err = copyFileIfPresent(filepath.Join(seedRoot, versionPropsFileName), filepath.Join(outputRoot, versionPropsFileName))

	if err != nil {
		return copied, fmt.Errorf("failed to seed %s: %w", versionPropsFileName, err)
	}

	copied += n

	n, err = copyDirTree(filepath.Join(filepath.Dir(seedRoot), "docs", validationDocsDirName, validationCurrentDirName),
		filepath.Join(stageRoot, "docs", validationDocsDirName, validationCurrentDirName))

	if err != nil {
		return copied, fmt.Errorf("failed to seed validation proof counts: %w", err)
	}

	copied += n

	return copied, nil
}

// copyDirTree copies a directory tree verbatim, creating destination directories as needed. A
// missing source is not an error (the seed root may legitimately lack the validation tree); it
// simply copies nothing.
func copyDirTree(src, dst string) (int, error) {
	info, err := os.Stat(src)

	if err != nil {
		if os.IsNotExist(err) {
			return 0, nil
		}

		return 0, err
	}

	if !info.IsDir() {
		return 0, fmt.Errorf("%q is not a directory", src)
	}

	copied := 0

	err = filepath.WalkDir(src, func(sourcePath string, entry fs.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		relPath, err := filepath.Rel(src, sourcePath)

		if err != nil {
			return err
		}

		targetPath := filepath.Join(dst, relPath)

		if entry.IsDir() {
			// BUILD OUTPUTS are not part of the seed. A seeded staging root exists so the converter
			// can read the tree it is about to rewrite — the hand-own markers it must not clobber,
			// the `package_info.cs` files it mints `<ImportedTypeAliases>` from, and the existing
			// bytes `needToWriteFile` compares against to decide whether a file changed. None of
			// that lives in bin/obj/Generated, and nothing downstream reads them: the emitted-vs-
			// seeded classification only considers files a target WROTE, and a staging root is
			// never built in.
			//
			// Measured before excluding them: a seed of a worked-in clone copied 60,931 files
			// against a tracked corpus of roughly 4,000 — about 57k build artifacts, copied once
			// per target, three times per multi-platform regen. It was the single largest cost in
			// the run and bought nothing.
			if isBuildOutputDirectory(entry.Name()) {
				return filepath.SkipDir
			}

			return os.MkdirAll(targetPath, 0755)
		}

		if !entry.Type().IsRegular() {
			return nil
		}

		contents, err := os.ReadFile(sourcePath)

		if err != nil {
			return err
		}

		if err := os.WriteFile(targetPath, contents, 0644); err != nil {
			return err
		}

		copied++

		return nil
	})

	return copied, err
}

// isBuildOutputDirectory reports whether a directory name is one MSBuild owns rather than the
// conversion: `bin` and `obj` are its outputs, and `Generated` is where every converted project
// routes the source generators' emissions (CompilerGeneratedFilesOutputPath, excluded from the
// compilation by the csproj's own `<Compile Remove>`). No converted Go package is named for any of
// them, so the test is safe as a plain name match.
func isBuildOutputDirectory(name string) bool {
	switch name {
	case "bin", "obj", "Generated":
		return true
	}

	return false
}

// copyFileIfPresent copies one file when it exists, creating the destination directory, and reports
// how many files it copied (0 or 1). A missing source is not an error.
func copyFileIfPresent(src, dst string) (int, error) {
	contents, err := os.ReadFile(src)

	if err != nil {
		if os.IsNotExist(err) {
			return 0, nil
		}

		return 0, err
	}

	if err := os.MkdirAll(filepath.Dir(dst), 0755); err != nil {
		return 0, err
	}

	if err := os.WriteFile(dst, contents, 0644); err != nil {
		return 0, err
	}

	return 1, nil
}

// stampSeedTimes marks every seeded file with censusSeedSentinel, which is what makes "emitted by
// this run" an exact, content-independent question afterwards.
func stampSeedTimes(root string) error {
	return filepath.WalkDir(root, func(filePath string, entry fs.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if entry.IsDir() || !entry.Type().IsRegular() {
			return nil
		}

		return os.Chtimes(filePath, censusSeedSentinel, censusSeedSentinel)
	})
}

// snapshotConvertedRoot hashes every file under a converted `core` tree and records whether this run
// emitted it (modification time moved off the seed sentinel). Paths are returned relative to coreDir
// with forward slashes, so `internal/goos/goos.cs` is both the artifact key and, by its directory,
// the package it belongs to.
func snapshotConvertedRoot(coreDir string) (map[string]artifactState, error) {
	artifacts := map[string]artifactState{}

	err := filepath.WalkDir(coreDir, func(filePath string, entry fs.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if entry.IsDir() || !entry.Type().IsRegular() {
			return nil
		}

		info, err := entry.Info()

		if err != nil {
			return err
		}

		contents, err := os.ReadFile(filePath)

		if err != nil {
			return err
		}

		relPath, err := filepath.Rel(coreDir, filePath)

		if err != nil {
			return err
		}

		exact, normalized := hashPair(contents)

		artifacts[filepath.ToSlash(relPath)] = artifactState{
			hash:           exact,
			normalizedHash: normalized,
			emitted:        !info.ModTime().Equal(censusSeedSentinel),
		}

		return nil
	})

	normalizeArtifactLogicalPaths(artifacts)

	return artifacts, err
}

// markerGateViolations is CLAUDE.md's hard pre-overlay gate, made mechanical: for every hand-owned
// file the seed carried, this run must NOT have emitted a plain `.cs` at that path. A non-empty
// result means the seeding did not take, and every number the census would report is measured
// against a corpus that does not exist.
func markerGateViolations(marked []string, artifacts map[string]artifactState) []string {
	var violations []string

	for _, relPath := range marked {
		if state, ok := artifacts[relPath]; ok && state.emitted {
			violations = append(violations, relPath)
		}
	}

	return violations
}

// readFailedPackages reads the resume list a conversion writes when packages fail; an absent file
// means none did.
func readFailedPackages(root string) []string {
	contents, err := os.ReadFile(filepath.Join(root, "failed_packages.txt"))

	if err != nil {
		return nil
	}

	var failed []string

	for _, line := range strings.Split(string(contents), "\n") {
		if line = strings.TrimSpace(line); line != "" {
			failed = append(failed, line)
		}
	}

	sort.Strings(failed)

	return failed
}
