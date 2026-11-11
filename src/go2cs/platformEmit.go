// platformEmit.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the MULTI-PLATFORM EMISSION — the run that PRODUCES layout L3 (increment 2 of
// docs/phase4/DESIGN-multiplatform-corpus.md; platformLayout.go owns what L3 IS, and every rule for
// honoring it afterwards).
//
//	go2cs -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64,linux/arm64 -go2cspath <repo>\src
//
// converts once per target and merges the emissions into ONE tree, per L3: an artifact that is
// byte-identical on every target is written flat in its package directory, and anything else — a
// name whose content varies, or one only some targets emit at all — is written to the key folders
// of that package the layout lattice homes it at (platformLattice.go).
//
// **It is increment 1's census with a write step, deliberately.** The staging, the seeding ritual,
// the sentinel-modification-time "did this run emit it", the hand-own marker gate and the four-way
// classification are all reused verbatim from platformCensus.go / platformManifest.go — because the
// question a merge has to answer ("does this file vary across platforms?") is exactly the question
// the census already answers, and answering it twice in two places is how the two would drift. The
// design's §4.2 requirement rides along for free: the axis is computed from what the converter WROTE
// for each target, never from `go list` file sets, which are wrong in both directions.
//
// Three things this file is careful about:
//
//  1. **The seed root and the destination are the same tree.** Every read of the corpus (the seed
//     hashes, the hand-own census, the per-target staging copies) completes before the first byte of
//     the merge is written, so the run is a read-then-write, not a read-while-write.
//
//  2. **The merge is additive plus TARGETED removal.** It writes only the artifacts the runs
//     emitted, and removes only the OTHER candidate locations of those same artifacts (the flat copy
//     of a file that became per-GOOS, or the per-GOOS copies of one that became shared). Nothing
//     else in the corpus is touched — not hand-owned files, not committed test sources, not
//     `.gitignore`, not the root attribution files.
//
//  3. **It is idempotent.** Running it twice over the same corpus is a no-op: the staging roots are
//     seeded from an already-L3 tree, layout adoption reproduces that tree file for file, the
//     classification is taken over LOGICAL paths (a per-GOOS file classifies under its package, not
//     under `<pkg>/<goos>`), and every write goes through needToWriteFile.

package main

import (
	"bytes"
	"errors"
	"fmt"
	"io/fs"
	"os"
	"path"
	"path/filepath"
	"runtime"
	"sort"
	"strconv"
	"strings"
	"syscall"
)

// runPlatformEmission is the entry point for a `-platforms <two or more targets>` conversion: stage
// each target, classify the emissions, and merge them into the -go2cspath corpus as layout L3.
func runPlatformEmission(options Options, stageDir string, packageFilter []string) error {
	targets := options.targetPlatforms

	if len(targets) < 2 {
		return fmt.Errorf("multi-platform emission needs at least two -platforms targets (got %d)", len(targets))
	}

	// The merge homes every file by the layout lattice of the targets it is given (platformLattice.go),
	// and a single-target conversion reads the tree against platformUniverse. So the request must be
	// exactly that universe: a strict subset would re-home every file the absent targets share as
	// though they did not exist, and a target outside it would enter a layout no single-target
	// conversion reproduces. A repeated target has no key that tells its two emissions apart.
	if err := platformTargetsMatchUniverse(targets, platformUniverse); err != nil {
		return err
	}

	rootPath, err := filepath.Abs(options.go2csPath)

	if err != nil {
		return fmt.Errorf("failed to resolve output root %q: %w", options.go2csPath, err)
	}

	// The root is BOTH the seed every staging root is copied from and the destination the merge
	// writes into, so it has to be a real converted tree — an empty root would seed nothing, which
	// is the failure mode CLAUDE.md's reconvert ritual exists to prevent (every hand-owned file
	// would emit as a plain .cs).
	if !isGo2CSRoot(rootPath) {
		return fmt.Errorf("output root %q is not a converted go2cs tree (no %s under it); "+
			"a multi-platform emission seeds each target's staging root from it before merging back into it",
			rootPath, filepath.Join("core", "golib", "golib.csproj"))
	}

	stageDir, releaseStage, reclaimed, err := resolveEmissionStageDir(stageDir, rootPath)

	if err != nil {
		return err
	}

	defer releaseStage()

	fmt.Printf("\nMulti-platform emission (layout L3)\n")
	fmt.Printf("  corpus root: %s\n", rootPath)
	fmt.Printf("  staging dir: %s\n", stageDir)

	if reclaimed.files > 0 {
		fmt.Printf("  reclaimed:   %d files (%.1f MB) left in the staging dir by a run that ended before its cleanup\n",
			reclaimed.files, float64(reclaimed.bytes)/(1024*1024))
	}

	fmt.Printf("  targets:     %s\n\n", strings.Join(targets, ", "))

	seed, err := readCensusSeed(filepath.Join(rootPath, "core"))

	if err != nil {
		return err
	}

	fmt.Printf("  seed holds %d files, %d of them hand-owned ([module: GoManualConversion])\n\n",
		len(seed.hashes), len(seed.markedFiles))

	// STRICTLY SEQUENTIAL, one fresh root each — the r41 never-convert-twice-into-one-root rule.
	emissions, err := runTargetsSkippingUnmatched(targets, packageFilter, "emission", func(target string) (*platformEmission, error) {
		emission, err := runCensusTarget(options, rootPath, stageDir, target, packageFilter, seed)

		if err != nil {
			return nil, fmt.Errorf("emission target %s failed: %w", target, err)
		}

		// A staging root whose seeding did not take measures a corpus that does not exist, and here
		// it would be MERGED into the real one. Fatal, not a footnote.
		if len(emission.markerGateViolations) > 0 {
			return nil, fmt.Errorf("target %s emitted %d hand-owned file(s) as plain .cs — the staging root was not seeded correctly; "+
				"first offender: %s", target, len(emission.markerGateViolations), emission.markerGateViolations[0])
		}

		return emission, nil
	})

	if err != nil {
		return err
	}

	// The merge classifies over the targets that EMITTED; a target skipped for an empty filter match has
	// no emission to merge and must not stand in the target list.
	emittedTargets := make([]string, 0, len(emissions))

	for _, emission := range emissions {
		emittedTargets = append(emittedTargets, emission.target)
	}

	return mergePlatformEmissions(rootPath, emittedTargets, emissions)
}

// emissionStageDirName is the default staging directory of a multi-platform emission, made BESIDE the
// corpus under the output root (`<root>/.go2cs-platform-stage`), never inside `core`, and ignored by
// the repository's .gitignore. It used to be a fresh `go2cs-platform-emit-*` directory under the
// system temporary directory, removed by a deferred call; a run that ended through log.Fatal, os.Exit
// or a kill skipped that call and left a full per-target copy of the corpus behind where nothing
// would ever find it again. A fixed path is found again: the next run reclaims it.
const emissionStageDirName = ".go2cs-platform-stage"

// emissionStageLockName is the file inside the default staging directory that holds the process ID of
// the run using it, so a second run into the same output root refuses instead of emptying a stage a
// live run is still writing (CLAUDE.md safety floor 1: never two conversions into one output root).
const emissionStageLockName = "owner.pid"

// stageReclaim is what a run removed from a leftover staging directory before it started.
type stageReclaim struct {
	files int
	bytes int64
}

// resolveEmissionStageDir resolves the staging directory: the caller's -platform-stage when given
// (kept afterwards, so a corpus-wide run can be inspected), otherwise emissionStageDirName under
// rootPath, which this run locks, empties of any leftover, and removes again through the returned
// release function.
func resolveEmissionStageDir(stageDir string, rootPath string) (string, func(), stageReclaim, error) {
	keep := func() {}

	if stageDir = strings.TrimSpace(stageDir); len(stageDir) > 0 {
		resolved, err := filepath.Abs(stageDir)

		if err != nil {
			return "", keep, stageReclaim{}, fmt.Errorf("failed to resolve staging directory %q: %w", stageDir, err)
		}

		if err := os.MkdirAll(resolved, 0755); err != nil {
			return "", keep, stageReclaim{}, fmt.Errorf("failed to create staging directory %q: %w", resolved, err)
		}

		return resolved, keep, stageReclaim{}, nil
	}

	resolved := filepath.Join(rootPath, emissionStageDirName)

	if err := os.MkdirAll(resolved, 0755); err != nil {
		return "", keep, stageReclaim{}, fmt.Errorf("failed to create staging directory %q: %w", resolved, err)
	}

	if err := lockEmissionStage(resolved); err != nil {
		return "", keep, stageReclaim{}, err
	}

	// Locked: whatever else is here was left by a run that ended before its cleanup.
	reclaimed, err := reclaimEmissionStage(resolved)

	if err != nil {
		os.Remove(filepath.Join(resolved, emissionStageLockName))
		return "", keep, stageReclaim{}, err
	}

	return resolved, func() { os.RemoveAll(resolved) }, reclaimed, nil
}

// lockEmissionStage takes the stage's lock: a file holding this process's ID. The ID is written into a
// file of this run's own first and that file is then linked into place, so the lock appears whole or
// not at all, and a link refuses to replace a lock another run already holds. A lock naming a process
// that is no longer running is stale and is replaced. A lock naming a live process refuses the run,
// and so does one that names no process at all: no run ever presents an empty lock, so that is not
// evidence of a stale one. A known limit: two runs that start together over a dead run's lock can both
// judge it stale, and the later one's remove can take the lock the earlier one has just linked.
func lockEmissionStage(stageDir string) error {
	lockPath := filepath.Join(stageDir, emissionStageLockName)
	pending, err := os.CreateTemp(stageDir, emissionStageLockName+".*")

	if err != nil {
		return fmt.Errorf("failed to create staging lock in %q: %w", stageDir, err)
	}

	defer os.Remove(pending.Name())

	_, err = fmt.Fprintf(pending, "%d\n", os.Getpid())

	if closeErr := pending.Close(); err == nil {
		err = closeErr
	}

	if err != nil {
		return fmt.Errorf("failed to write staging lock %q: %w", pending.Name(), err)
	}

	for attempt := 0; ; attempt++ {
		err := os.Link(pending.Name(), lockPath)

		if err == nil {
			return nil
		}

		if !errors.Is(err, fs.ErrExist) || attempt > 0 {
			return fmt.Errorf("failed to create staging lock %q: %w", lockPath, err)
		}

		holder := readStageLockHolder(lockPath)

		if holder == 0 {
			if _, err := os.Lstat(lockPath); errors.Is(err, fs.ErrNotExist) {
				continue // its holder released it between the link and the read
			}

			return fmt.Errorf("staging directory %q has a lock (%s) that names no process, so it may belong to a live run: "+
				"two emissions into one output root are never run at once; if no go2cs run is using it, remove the directory",
				stageDir, emissionStageLockName)
		}

		if processIsRunning(holder) {
			return fmt.Errorf("staging directory %q is held by running process %d (%s): two emissions into one output root "+
				"are never run at once; wait for that run to finish, or, if that process is not a go2cs run, remove the directory",
				stageDir, holder, emissionStageLockName)
		}

		if err := os.Remove(lockPath); err != nil && !errors.Is(err, fs.ErrNotExist) {
			return fmt.Errorf("failed to remove stale staging lock %q: %w", lockPath, err)
		}
	}
}

// readStageLockHolder reads the process ID a staging lock holds, or 0 when it holds none: the lock is
// missing, empty, or not a process ID.
func readStageLockHolder(lockPath string) int {
	content, err := os.ReadFile(lockPath)

	if err != nil {
		return 0
	}

	pid, err := strconv.Atoi(strings.TrimSpace(string(content)))

	if err != nil || pid <= 0 {
		return 0
	}

	return pid
}

// processIsRunning reports whether a process with the given ID is running. On Windows FindProcess
// opens the process and fails when there is none; elsewhere it always succeeds, and signal 0 asks
// without signalling (EPERM means the process exists under another user). A known limit, which fails
// closed: Windows reuses process IDs, so a stale lock whose ID now belongs to an unrelated process
// reads as live and the run refuses rather than proceeding; removing the directory clears it.
func processIsRunning(pid int) bool {
	process, err := os.FindProcess(pid)

	if err != nil {
		return false
	}

	defer process.Release()

	if runtime.GOOS == "windows" {
		return true
	}

	err = process.Signal(syscall.Signal(0))

	return err == nil || errors.Is(err, syscall.EPERM)
}

// reclaimEmissionStage removes everything in a locked staging directory except its lock, and reports
// how many files and bytes it removed.
func reclaimEmissionStage(stageDir string) (stageReclaim, error) {
	var reclaimed stageReclaim

	entries, err := os.ReadDir(stageDir)

	if err != nil {
		return reclaimed, fmt.Errorf("failed to read staging directory %q: %w", stageDir, err)
	}

	for _, entry := range entries {
		if entry.Name() == emissionStageLockName {
			continue
		}

		leftover := filepath.Join(stageDir, entry.Name())

		filepath.WalkDir(leftover, func(_ string, item fs.DirEntry, err error) error {
			if err == nil && !item.IsDir() {
				reclaimed.files++

				if info, err := item.Info(); err == nil {
					reclaimed.bytes += info.Size()
				}
			}

			return nil
		})

		if err := os.RemoveAll(leftover); err != nil {
			return reclaimed, fmt.Errorf("failed to reclaim leftover %q from staging directory: %w", leftover, err)
		}
	}

	return reclaimed, nil
}

// mergedArtifact is one logical artifact's merge plan: where each emitting target's copy was staged,
// and the key folder each emitting target's copy is homed in.
type mergedArtifact struct {
	class   string
	staged  map[string]string // target -> absolute path of that target's staged copy
	emitted []string          // emitting targets, in target order
	homes   map[string]string // emitting target -> the key folder its build compiles the copy from ("" = flat)
}

// mergedPlacement is one copy a merge plan writes: a content class's bytes in one of its home keys.
type mergedPlacement struct {
	folder string // the package-relative key folder; "" = flat
	source string // the staged copy of the class's first member
}

// coversAnyOf reports whether a key's cover holds any of the given targets.
func coversAnyOf(cover []string, targets []string) bool {
	for _, covered := range cover {
		for _, target := range targets {
			if covered == target {
				return true
			}
		}
	}

	return false
}

// refuseNestedPackagesNamedLikeKeys refuses a merge that would give a package a layout block while it
// holds a NESTED PACKAGE in a folder named like one of the lattice's keys (design §2.4): the project
// file's `$(GoTargetOS)/*.cs` (or arch, or pair) glob would compile the child's sources into the
// parent for the targets that key selects. The three nested packages named for a key today
// (internal/runtime/syscall/{linux,windows}, internal/syscall/windows) have no package for a parent.
func refuseNestedPackagesNamedLikeKeys(coreDir string, lattice *platformLattice, plans map[string]mergedArtifact, placements map[string][]mergedPlacement) error {
	layoutPackages := map[string]bool{}

	for logical := range plans {
		for _, home := range placements[logical] {
			if pkg := artifactPackage(logical); pkg != "." && len(home.folder) > 0 {
				layoutPackages[pkg] = true
			}
		}
	}

	var offenders []string

	for _, pkg := range sortedKeys(layoutPackages) {
		packageDir := filepath.Join(coreDir, filepath.FromSlash(pkg))

		for _, folder := range lattice.candidateFolders() {
			if isNestedPackageFolder(packageDir, folder) {
				offenders = append(offenders, path.Join("core", pkg, folder))
			}
		}
	}

	if len(offenders) == 0 {
		return nil
	}

	return fmt.Errorf("multi-platform merge: %d package(s) would carry the layout block while holding a nested package in a "+
		"folder named like a layout key, whose sources that block's <key>/*.cs glob would compile into the parent; "+
		"rename or move the nested package first:\n  %s", len(offenders), strings.Join(offenders, "\n  "))
}

// mergePlatformEmissions writes the classified emissions into the corpus as layout L3, keyed by the
// lattice of the targets that emitted (platformLattice.go).
func mergePlatformEmissions(rootPath string, targets []string, emissions []*platformEmission) error {
	coreDir := filepath.Join(rootPath, "core")

	// The lattice of the targets that EMITTED, not of the declared universe: a target skipped for an
	// empty filter match has no emission to place, and its folders are left as they are.
	lattice, err := newPlatformLattice(targets)

	if err != nil {
		return fmt.Errorf("multi-platform merge: %w", err)
	}

	// Classification is over LOGICAL paths — `<pkg>/<file>`, with any per-GOOS folder segment
	// already folded away by snapshotConvertedRoot — so an already-L3 corpus reclassifies to exactly
	// what it reclassified to when it was flat. Without that folding a second run would nest
	// `<pkg>/windows/windows/<file>`.
	hashesByTarget := make([]map[string]string, 0, len(targets))
	stagedByTarget := make([]map[string]string, 0, len(targets))

	for _, emission := range emissions {
		hashes := map[string]string{}
		staged := map[string]string{}

		for _, rawPath := range emission.emittedByExtension(".cs") {
			state := emission.artifacts[rawPath]
			logical := state.logicalPath(rawPath)
			hashes[logical] = state.hash
			staged[logical] = filepath.Join(emission.root, "core", filepath.FromSlash(rawPath))
		}

		hashesByTarget = append(hashesByTarget, hashes)
		stagedByTarget = append(stagedByTarget, staged)
	}

	classes := classifyPlatformEmissions(targets, hashesByTarget)
	plans := make(map[string]mergedArtifact, len(classes))

	// Each class of identical bytes is homed by the lattice (exact hashes, as the classification above
	// compares them): every target in a class compiles the one copy its key holds. Over one target
	// per GOOS this is the L3 rule as it began — shared by all ⟹ flat, anything else ⟹ one copy per
	// emitting target's GOOS folder.
	placements := make(map[string][]mergedPlacement, len(classes))

	for logical, class := range classes {
		plan := mergedArtifact{class: class.class, staged: map[string]string{}, emitted: class.emitters, homes: map[string]string{}}
		hashes := map[string]string{}

		for i, target := range targets {
			if stagedPath, ok := stagedByTarget[i][logical]; ok {
				plan.staged[target] = stagedPath
				hashes[target] = hashesByTarget[i][logical]
			}
		}

		for _, contentClass := range partitionContentClasses(targets, hashes) {
			for _, home := range lattice.homes(contentClass) {
				placements[logical] = append(placements[logical], mergedPlacement{folder: home.folder, source: plan.staged[contentClass[0]]})

				for _, target := range home.cover {
					plan.homes[target] = home.folder
				}
			}
		}

		plans[logical] = plan
	}

	// A package whose project file will select `<key>/*.cs` must not hold a NESTED PACKAGE in a folder
	// of that name: the glob would compile the child's sources into the parent (design §2.4). Refused
	// before the first write, so a refused merge leaves the corpus as it found it.
	if err := refuseNestedPackagesNamedLikeKeys(coreDir, lattice, plans, placements); err != nil {
		return err
	}

	written, removed, unchanged := 0, 0, 0
	layoutPackages := map[string]bool{}
	foldedPackages := map[string]bool{}

	for _, logical := range sortedKeys(func() map[string]bool {
		keys := map[string]bool{}
		for logical := range plans {
			keys[logical] = true
		}
		return keys
	}()) {
		plan := plans[logical]
		pkg := artifactPackage(logical)

		// The root attribution files (core/VERSION, core/LICENSE, core/README.md …) belong to no
		// package and are re-copied verbatim by every conversion; they are not platform artifacts
		// and the merge leaves the corpus's own copies alone.
		if pkg == "." {
			continue
		}

		fileName := path.Base(logical)
		packageDir := filepath.Join(coreDir, filepath.FromSlash(pkg))
		destinations := map[string]bool{}

		for _, home := range placements[logical] {
			destinations[home.folder] = true

			if len(home.folder) > 0 {
				layoutPackages[pkg] = true
			}

			changed, err := copyMergedFile(home.source, filepath.Join(packageDir, home.folder, fileName))

			if err != nil {
				return err
			}

			countWrite(changed, &written, &unchanged)
		}

		// Then the name goes from every other place a build could compile it from: flat, and each key
		// folder of the lattice, admitted or not, whose cover holds a target that emitted it (that
		// target now compiles the copy written above, so another would be a second body). A folder
		// serving only targets that did NOT emit the name is left alone, as the per-GOOS folder of a
		// non-emitting target always was: what it holds there is that target's own business (a
		// per-GOOS hand-own its conversion diverted), not a stale copy of this emission. A nested
		// package is never a key folder, whatever its name.
		if !destinations[""] {
			gone, err := removeIfPresent(filepath.Join(packageDir, fileName))

			if err != nil {
				return err
			}

			if gone {
				removed++
			}
		}

		for _, folder := range lattice.candidateFolders() {
			if destinations[folder] || !coversAnyOf(lattice.folderCover(folder), plan.emitted) || isNestedPackageFolder(packageDir, folder) {
				continue
			}

			gone, err := removeIfPresent(filepath.Join(packageDir, folder, fileName))

			if err != nil {
				return err
			}

			if gone {
				removed++
				foldedPackages[pkg] = true
			}
		}
	}

	emittedLogical := make(map[string]bool, len(plans))

	for logical := range plans {
		emittedLogical[logical] = true
	}

	initsRetired, err := retireUnemittedPackageInitFiles(coreDir, targets, emissions, emittedLogical)

	if err != nil {
		return err
	}

	removed += initsRetired

	conditioned, residual, companionWritten, err := mergeCompanionArtifacts(coreDir, targets, emissions)

	if err != nil {
		return err
	}

	written += companionWritten

	// The per-file warning entries: each target derived its own facts into its staging root, and the
	// corpus file is their composition, anchored where the plans above put each file.
	entriesWritten, err := mergeWarningEntries(coreDir, targets, emissions, plans)

	if err != nil {
		return err
	}

	written += entriesWritten

	// The hand-owned files come last, because their routing READS the layout the two steps above just
	// finished writing: a companion belongs in the platform set of its principal, and the principal's
	// placement is only knowable once it has landed (platformHandOwn.go).
	handOwnPackages, handOwnWritten, handOwnRemoved, err := mergeHandOwnedArtifacts(coreDir, targets, emissions)

	if err != nil {
		return err
	}

	written += handOwnWritten
	removed += handOwnRemoved

	// With every artifact landed, assert the one-location invariant over the whole corpus — the
	// paths NO target rewrote included, which is the only place it can already be broken.
	duplicatesRemoved, err := reconcileLayoutDuplicates(coreDir, targets)

	removed += duplicatesRemoved

	if err != nil {
		return err
	}

	// Every package that ended up with per-GOOS sources needs the conditioned <Compile Include>.
	// A single-target conversion adds it from the tree's own shape (projectFileWriter), but this
	// run is what CREATES that shape, so the first time a package becomes L3 the block is applied
	// here — after the sources have moved, so the predicate reads the finished layout.
	blockAdded := 0

	for _, pkg := range sortedKeys(layoutPackages) {
		added, err := applyLayoutBlockToProjectFile(filepath.Join(coreDir, filepath.FromSlash(pkg)))

		if err != nil {
			return err
		}

		if added {
			blockAdded++
		}
	}

	// The converse (seat #49): a package whose LAST per-GOOS files this run folded flat no longer carries the layout,
	// so its project file drops the block, as every single-target writer renders it for the package, and its emptied
	// per-GOOS folders go, which no checkout carries anyway. Without this the seed's block survived the fold (os/signal
	// and time/tzdata, whose targets' package_info.cs #41's alias drop made identical), and the next single-target
	// writer (a -tests convert, a reconvert from a fresh checkout) dropped it: the emission had two fixed points.
	blockRemoved := 0

	for _, pkg := range sortedKeys(foldedPackages) {
		dropped, err := dropLayoutBlockFromProjectFile(filepath.Join(coreDir, filepath.FromSlash(pkg)), targets)

		if err != nil {
			return err
		}

		if dropped {
			blockRemoved++
		}
	}

	// A multi-platform emission is the one run that can produce the UNION project set: the
	// platform-exclusive packages (internal/syscall/windows on Windows, internal/syscall/unix and
	// internal/runtime/syscall on the unix side, crypto/x509/internal/macos on macOS …) exist in
	// only one target's tree, so a single-target solution can never list them all. Regenerating from
	// the MERGED corpus — after every source and project file has landed — is what makes
	// `dotnet build go2cs-stdlib.slnx -p:GoTargetOS=linux` able to resolve a Linux-only reference at
	// all. The generator is reused unchanged: it already derives its project list from the tree.
	// graph stays nil on purpose: with no convert-set graph, collectConvertedProjects recovers every
	// referenced-but-unemitted package (the hand-owned `unsafe`/`testing` case), which is exactly
	// right when the tree being scanned IS the finished corpus rather than one run's output.
	solutionConverter := &StdLibConverter{go2csPath: rootPath}

	if err := solutionConverter.GenerateSolutionFile(); err != nil {
		return err
	}

	fmt.Printf("\n=== Layout L3 merge ===\n\n")
	fmt.Printf("Packages carrying per-GOOS sources   %6d\n", len(layoutPackages))
	fmt.Printf("Packages with conditioned references %6d\n", len(conditioned))
	fmt.Printf("Artifacts written                    %6d\n", written)
	fmt.Printf("Artifacts already current            %6d\n", unchanged)
	fmt.Printf("Stale copies removed                 %6d\n", removed)
	fmt.Printf("Project files given the L3 block     %6d\n", blockAdded)
	fmt.Printf("Project files folded out of L3       %6d\n", blockRemoved)
	fmt.Printf("Packages with routed hand-owns       %6d\n", len(handOwnPackages))

	fmt.Printf("\nPer-GOOS sources:\n")

	for _, pkg := range sortedKeys(layoutPackages) {
		fmt.Printf("  %s\n", pkg)
	}

	if len(handOwnPackages) > 0 {
		fmt.Printf("\nHand-owned files routed to their principal's platform set:\n")

		for _, pkg := range handOwnPackages {
			fmt.Printf("  %s\n", pkg)
		}
	}

	if len(conditioned) > 0 {
		fmt.Printf("\nConditioned <ProjectReference> groups (direct import set differs by GOOS):\n")

		for _, pkg := range conditioned {
			fmt.Printf("  %s\n", pkg)
		}
	}

	// A project file that differs across targets in something OTHER than its reference list is
	// outside what layout L3 expresses (the design measured the .csproj delta as reference lists —
	// §4). The merge still lands the first target's remainder, but the difference is named rather
	// than absorbed: it is either a genuine design gap or a measurement worth recording.
	if len(residual) > 0 {
		fmt.Printf("\n⚠ %d package(s) emitted a flat companion that differs by target in a way layout L3 does not express\n", len(residual))
		fmt.Printf("  (a project file outside its <ProjectReference> lists, or a review sibling with no source folder to hold it),\n")
		fmt.Printf("  so %s's copy was kept:\n", goosOfTarget(targets[0]))

		for _, pkg := range sortedKeys(func() map[string]bool {
			keys := map[string]bool{}
			for pkg := range residual {
				keys[pkg] = true
			}
			return keys
		}()) {
			fmt.Printf("  %s — %s\n", pkg, residual[pkg])
		}
	}

	return nil
}

// reconcileLayoutDuplicates asserts layout L3's one-location invariant across the WHOLE corpus,
// not merely the artifacts this run re-emitted.
//
// The plan loop resolves every logical path some target REWROTE: shared artifacts land flat and
// their per-GOOS copies are removed, varying ones land per-GOOS and the flat copy is removed. But
// "emitted" means "the bytes changed" — needToWriteFile skips an identical write, which is what
// keeps a reconvert's timestamps meaningful and what the sentinel comparison rests on — so a file
// every target reproduces exactly appears in NO plan, and nothing above ever looks at it. If the
// corpus already holds that file in both places, the violation simply survives every run.
//
// That state is reachable without anyone doing anything wrong: an earlier regen routes a file
// per-GOOS, later work restores the flat copy (the standing restore rituals around closure files
// and CRLF phantoms do exactly that), and from then on both persist. It stays invisible until the
// affected target is built, because the emitted csproj carries BOTH `<Compile Include="*.cs" />`
// and `<Compile Include="$(GoTargetOS)/*.cs" />` — so the duplicate joins its own compilation:
// duplicate [assembly: GoPackage] (CS0579) and duplicate global usings (CS1537). Found on darwin
// at vendor/golang.org/x/sys/cpu, whose package_info.cs sat in both places with identical bytes.
//
// Identical copies are unambiguous: the artifact is shared, so the per-GOOS copies retire and the
// flat one stays. Copies that DIFFER are not, and are deliberately NOT guessed at — the merge has
// no emission data for a path no target rewrote, so it cannot tell which target owns the flat copy,
// and either choice silently breaks a platform. Those are named and fail the run.
//
// Generalised from "flat versus GOOS" to every pair of key folders by their COVERS (the targets whose
// build compiles each; platformLattice.folderCover, design §2.4):
//
//	disjoint covers                 nothing to do: no target compiles both
//	nested covers, identical bytes  the copy in the finer key goes (the narrower cover; on equal covers
//	                                the later of flat, GOOS, arch, pair)
//	nested covers, differing bytes  conflict: the shared targets would compile two bodies
//	overlapping covers              conflict even when identical: each copy serves a target the other
//	                                does not, so no removal repairs it
//
// Flat contains every key folder, a folder whose name is no target of this lattice included (a GOOS
// the merge did not convert keeps the flat-versus-folder check it always had), and two folders that
// both cover nothing here are disjoint.
func reconcileLayoutDuplicates(coreDir string, targets []string) (int, error) {
	lattice := mustPlatformLattice(targets)
	removed := 0
	var conflicts, nestedConflicts, overlaps []string

	err := filepath.WalkDir(coreDir, func(dirPath string, entry fs.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if !entry.IsDir() {
			return nil
		}

		// Build outputs hold copies of everything and are not part of the layout.
		if isBuildOutputDirectory(entry.Name()) {
			return filepath.SkipDir
		}

		entries, err := os.ReadDir(dirPath)

		if err != nil {
			return err
		}

		// Every key folder of this directory, coarsest first. isPlatformSourceFolder, not a bare
		// directory-name test: `internal/syscall/windows` is a real PACKAGE whose directory happens
		// to be named for a GOOS, and its sources are not a platform variant of its parent's.
		var folders []string

		for _, child := range entries {
			if child.IsDir() && isPlatformSourceFolder(dirPath, child.Name()) {
				folders = append(folders, child.Name())
			}
		}

		if len(folders) == 0 {
			return nil
		}

		sort.SliceStable(folders, func(i, j int) bool {
			if rankI, rankJ := folderRank(folders[i]), folderRank(folders[j]); rankI != rankJ {
				return rankI < rankJ
			}

			return folders[i] < folders[j]
		})

		// The locations of each name a key folder holds: flat ("") first when a regular file of that
		// name sits there too, then the folders in the order above.
		locations := map[string][]string{}

		for _, folder := range folders {
			folderEntries, err := os.ReadDir(filepath.Join(dirPath, folder))

			if err != nil {
				return err
			}

			for _, folderEntry := range folderEntries {
				if folderEntry.IsDir() {
					continue
				}

				name := folderEntry.Name()

				if _, seen := locations[name]; !seen {
					locations[name] = nil

					if flatInfo, err := os.Stat(filepath.Join(dirPath, name)); err == nil && flatInfo.Mode().IsRegular() {
						locations[name] = append(locations[name], "")
					}
				}

				locations[name] = append(locations[name], folder)
			}
		}

		for _, name := range sortedMapKeys(locations) {
			held := locations[name]
			gone := map[string]bool{}

			for i := 0; i < len(held); i++ {
				for j := i + 1; j < len(held); j++ {
					if gone[held[i]] || gone[held[j]] {
						continue
					}

					coarser, finer, relation := relateLayoutFolders(lattice, held[i], held[j])

					if relation == coversDisjoint {
						continue
					}

					coarserPath := filepath.Join(dirPath, coarser, name)
					finerPath := filepath.Join(dirPath, finer, name)
					pair := fmt.Sprintf("  %s\n  %s", corpusRelativePath(coreDir, coarserPath), corpusRelativePath(coreDir, finerPath))

					if relation == coversOverlapping {
						overlaps = append(overlaps, pair)
						continue
					}

					identical, err := filesHaveIdenticalBytes(finerPath, coarserPath)

					if err != nil {
						return err
					}

					if !identical && len(coarser) == 0 {
						conflicts = append(conflicts, pair)
						continue
					}

					if !identical {
						nestedConflicts = append(nestedConflicts, pair)
						continue
					}

					removedNow, err := removeIfPresent(finerPath)

					if err != nil {
						return err
					}

					if removedNow {
						removed++
						gone[finer] = true
					}
				}
			}
		}

		return nil
	})

	if err != nil {
		return removed, err
	}

	var problems []string

	if len(conflicts) > 0 {
		sort.Strings(conflicts)

		problems = append(problems, fmt.Sprintf("layout L3: %d artifact(s) exist BOTH flat and under a per-GOOS "+
			"folder with DIFFERING contents, so the affected target would compile both copies "+
			"(CS0579/CS1537). No target re-emitted them, so the merge cannot tell which platform "+
			"owns the flat copy — resolve by hand, or delete both and reconvert:\n%s",
			len(conflicts), strings.Join(conflicts, "\n\n")))
	}

	if len(nestedConflicts) > 0 {
		sort.Strings(nestedConflicts)

		problems = append(problems, fmt.Sprintf("layout L3: %d artifact(s) exist under two key folders, one serving "+
			"every target the other does, with DIFFERING contents, so the targets they share would compile both copies. "+
			"No target re-emitted them, so the merge cannot tell which copy is current — resolve by hand, or delete both "+
			"and reconvert:\n%s", len(nestedConflicts), strings.Join(nestedConflicts, "\n\n")))
	}

	if len(overlaps) > 0 {
		sort.Strings(overlaps)

		problems = append(problems, fmt.Sprintf("layout L3: %d artifact(s) exist under two key folders whose "+
			"targets overlap without either containing the other, so the targets they share would compile both "+
			"copies and removing either would leave a target without one — resolve by hand, or delete both and "+
			"reconvert:\n%s", len(overlaps), strings.Join(overlaps, "\n\n")))
	}

	if len(problems) > 0 {
		return removed, errors.New(strings.Join(problems, "\n\n"))
	}

	return removed, nil
}

// relateLayoutFolders relates two key folders of one package directory holding the same name ("" is
// flat) by their covers in the lattice, and orders them coarser first (reconcileLayoutDuplicates).
// Flat contains every folder, whatever it covers here; two folders that cover nothing here are
// disjoint.
func relateLayoutFolders(lattice *platformLattice, a string, b string) (string, string, coverRelation) {
	coverA, coverB := lattice.folderCover(a), lattice.folderCover(b)

	switch {
	case len(a) == 0:
		return a, b, coversNested
	case len(b) == 0:
		return b, a, coversNested
	case len(coverA) == 0 && len(coverB) == 0:
		return a, b, coversDisjoint
	}

	relation := relateCovers(coverA, coverB)

	switch {
	case relation != coversNested:
		return a, b, relation
	case len(coverA) > len(coverB):
		return a, b, relation
	case len(coverB) > len(coverA):
		return b, a, relation
	case folderRank(b) < folderRank(a):
		return b, a, relation
	default:
		return a, b, relation
	}
}

// filesHaveIdenticalBytes compares two files byte for byte. Deliberately exact rather than
// line-ending-insensitive: these are two copies of ONE emitted artifact, and the converter emits
// CRLF unconditionally, so any difference at all is a real one.
func filesHaveIdenticalBytes(first string, second string) (bool, error) {
	firstBytes, err := os.ReadFile(first)

	if err != nil {
		return false, err
	}

	secondBytes, err := os.ReadFile(second)

	if err != nil {
		return false, err
	}

	return bytes.Equal(firstBytes, secondBytes), nil
}

// corpusRelativePath renders a path for a message as `core/<pkg>/<file>`, falling back to the
// absolute path when it does not lie under the corpus.
func corpusRelativePath(coreDir string, filePath string) string {
	relPath, err := filepath.Rel(coreDir, filePath)

	if err != nil {
		return filePath
	}

	return path.Join("core", filepath.ToSlash(relPath))
}

// countWrite tallies one merge write as changed or already-current.
func countWrite(changed bool, written *int, unchanged *int) {
	if changed {
		*written++
	} else {
		*unchanged++
	}
}

// mergeCompanionArtifacts merges the non-`.cs` artifacts a conversion emits — the project file, the
// per-package README, the packaging icons, and the `.cs.auto` review siblings. These stay FLAT: one
// package has one project file, one README and one icon regardless of platform.
//
// The project file is the one that can genuinely disagree between targets, because a package's
// DIRECT IMPORT SET can differ by GOOS (design §4: 24 packages). Such a file is not copied from one
// target — it is COMPOSED, by reading every target's reference set and splitting it into the shared
// list plus per-GOOS conditioned groups (platformProject.go). That composition goes through the same
// renderer a single-target reconvert uses, which is what makes the reconvert reproduce it.
//
// Returns the packages given conditioned reference groups, any package whose project files differ
// OUTSIDE their reference lists (mapped to a description — layout L3 does not express that, so it
// is reported rather than absorbed), and the number of files written.
func mergeCompanionArtifacts(coreDir string, targets []string, emissions []*platformEmission) ([]string, map[string]string, int, error) {
	names := map[string]bool{}
	written := 0
	converted := make([]map[string]bool, len(emissions))

	for i, emission := range emissions {
		converted[i] = convertedPackages(emission)
	}

	for _, emission := range emissions {
		for rawPath, state := range emission.artifacts {
			// The warning entries are composed from every target by mergeWarningEntries, never
			// copied from one.
			if filepath.Ext(rawPath) == ".cs" || artifactPackage(rawPath) == "." || path.Base(rawPath) == warningEntriesFileName {
				continue
			}

			// A project file that carries a per-GOOS block is merged even when no target REWROTE it: the
			// writer that recomposes the seed's bytes skips the write (needToWriteFile), so "emitted" is false
			// in every stage exactly when the stale-axis case arises (carriesConvertedPlatformAxis).
			if state.emitted || (!names[rawPath] && strings.EqualFold(filepath.Ext(rawPath), ".csproj") && carriesConvertedPlatformAxis(rawPath, emissions, converted)) {
				names[rawPath] = true
			}
		}
	}

	conditioned := map[string]bool{}
	residual := map[string]string{}

	siblingsWritten, err := mergeReviewSiblings(coreDir, mustPlatformLattice(targets), emissions, names, residual)

	if err != nil {
		return nil, nil, written, err
	}

	written += siblingsWritten

	for _, rawPath := range sortedKeys(names) {
		destination := filepath.Join(coreDir, filepath.FromSlash(rawPath))

		if strings.EqualFold(filepath.Ext(rawPath), ".csproj") &&
			(companionDiffersAcrossTargets(rawPath, emissions) || carriesConvertedPlatformAxis(rawPath, emissions, converted)) {
			merged, note, err := mergePlatformProjectFile(rawPath, targets, emissions, converted)

			if err != nil {
				return nil, nil, written, err
			}

			if len(note) > 0 {
				addResidualNote(residual, artifactPackage(rawPath), note)
			}

			if strings.Contains(merged, platformReferenceBlockHeader) {
				conditioned[artifactPackage(rawPath)] = true
			}

			changed, err := writeMergedContents(destination, merged)

			if err != nil {
				return nil, nil, written, err
			}

			if changed {
				written++
			}

			continue
		}

		// The FIRST TARGET that has this artifact, not the first that re-emitted it. Those differ,
		// and the difference matters: "emitted" is decided by modification time, so a target whose
		// emission reproduced the seed byte for byte is not marked as emitting — which made the
		// source depend on which target happened to have something to rewrite. Measured, that is not
		// hypothetical: two companions genuinely vary by platform (os/exec/internal/fdtest's README,
		// whose package doc comes from a file Windows excludes, and runtime/runtime2.cs.auto), and
		// they were picking up whichever flavor emitted first.
		//
		// Target order is the caller's -platforms order, whose head is the reference flavor
		// ($(GoTargetOS)'s default). So this is deterministic AND it keeps the Windows lane's
		// shipped bytes exactly as they are, while a platform-exclusive package — absent from the
		// earlier targets entirely — still lands from the only target that has it.
		//
		// A review sibling in a key folder is the exception: only a target that folder serves can
		// emit it, and every other stage merely holds the seed copy it was staged with, so "first
		// target that has it" would hand the root back its own seed bytes (see
		// perGOOSReviewSiblingOwner).
		//
		// "Has it" is also too weak for the targets themselves. Every stage is a whole-tree copy of
		// the seed, so a target that never CONVERTED the package still holds the seed's copy of its
		// companions — and, first in order, handed the root back those seed bytes. Measured (H5,
		// 2026-10-15): crypto/x509/internal/macos is darwin-only, and its README kept the seeded
		// `macOS_package` from the Windows stage while the darwin stage emitted `macos_package`. So
		// the first pass takes the first target that converted the package (convertedPackages), and
		// only when none did does the second fall back to the first that has it. "Converted", not
		// "emitted this companion": emission is read from modification time, a byte-identical
		// rewrite does not move it, and os/exec/internal/fdtest's README — which Windows emits in its
		// own flavor — would then go to whichever later target happened to differ.
		source := ""
		owner := perGOOSReviewSiblingOwner(rawPath, emissions)
		pkg := companionPackage(rawPath, emissions)

		for pass := 0; pass < 2 && len(source) == 0; pass++ {
			for i, emission := range emissions {
				if _, ok := emission.artifacts[rawPath]; !ok || (len(owner) != 0 && !layoutFolderServes(owner, emission.target)) {
					continue
				}

				if pass == 0 && !converted[i][pkg] {
					continue
				}

				source = filepath.Join(emission.root, "core", filepath.FromSlash(rawPath))
				break
			}
		}

		if len(source) == 0 {
			continue
		}

		changed, err := copyMergedFile(source, destination)

		if err != nil {
			return nil, nil, written, err
		}

		if changed {
			written++
		}
	}

	return sortedKeys(conditioned), residual, written, nil
}

// mergeReviewSiblings places the `.cs.auto` review siblings of FLAT whole-file hand-owns, and removes them from names
// so the per-artifact rule below does not see them. A review sibling exists to show a hand-own's drift from what the
// converter emits, and a flat hand-own's emission can differ by GOOS: runtime/runtime2.go's m carries "sigmask" in its
// [GoValueClone] list on linux only, and g is `public partial struct g` there (R's end-regen F2). One flat sibling,
// taken from the first target, hid the other flavours (COORD's S5 ruling (a)). So, per sibling:
//
//   - the targets that EMITTED it are read, each from the copy it wrote: flat, or in one of its own key folders once
//     the tree holds the sibling there (reviewSiblingLayoutPath). A census stage writes every sibling, changed or not
//     (alwaysWriteSources), so "emitted" is exact here, unlike for a README;
//   - when every flavour agrees, one flat sibling, and the key folders' copies go;
//   - when they differ, each class of identical flavours goes to the keys the lattice homes it at (one per emitting
//     GOOS folder over one target per GOOS), and the flat copy goes. A sibling is never compiled, so the key folder
//     need not be a source folder: crypto/internal/fips140/subtle holds no key source at all and keeps
//     xor_generic.cs.auto in amd64/ and linux-arm64/, folders a single-target reconvert finds again by the sibling
//     itself (heldReviewSiblingPath). Such a folder adds nothing to the project file, which conditions source
//     folders only;
//   - when they differ and one of those keys is a NESTED PACKAGE of the same name (`internal/syscall/windows`), whose
//     directory is never written into, the first target's flavour stays flat and the package is NAMED.
//
// A sibling whose hand-own is not flat in the root (a per-GOOS hand-own's, or one with no hand-own) keeps the
// per-artifact rule, which already takes a per-GOOS sibling from its own target (perGOOSReviewSiblingOwner).
func mergeReviewSiblings(coreDir string, lattice *platformLattice, emissions []*platformEmission, names map[string]bool, residual map[string]string) (int, error) {
	groups := map[string][]string{}

	for rawPath := range names {
		if strings.HasSuffix(rawPath, ".cs.auto") {
			logical := companionLogicalPath(rawPath, emissions)
			groups[logical] = append(groups[logical], rawPath)
		}
	}

	written := 0

	for _, logical := range sortedMapKeys(groups) {
		handOwn, err := os.Stat(filepath.Join(coreDir, filepath.FromSlash(strings.TrimSuffix(logical, ".auto"))))

		if err != nil || !handOwn.Mode().IsRegular() {
			continue
		}

		type flavour struct {
			target   string
			contents []byte
		}

		var flavours []flavour

		for _, emission := range emissions {
			for _, rawPath := range sortedStrings(groups[logical]) {
				state, ok := emission.artifacts[rawPath]

				if !ok || !state.emitted || (rawPath != logical && !layoutFolderServes(path.Base(path.Dir(rawPath)), emission.target)) {
					continue
				}

				contents, err := os.ReadFile(filepath.Join(emission.root, "core", filepath.FromSlash(rawPath)))

				if err != nil {
					return written, fmt.Errorf("failed to read staged review sibling %q for target %s: %w", rawPath, emission.target, err)
				}

				flavours = append(flavours, flavour{emission.target, contents})
				break
			}
		}

		for _, rawPath := range groups[logical] {
			delete(names, rawPath)
		}

		if len(flavours) == 0 {
			continue
		}

		pkg, name := path.Dir(logical), path.Base(logical)
		packageDir := filepath.Join(coreDir, filepath.FromSlash(pkg))
		agree, splittable := true, true

		for _, f := range flavours {
			agree = agree && bytes.Equal(f.contents, flavours[0].contents)
		}

		// The split: each class of identical flavours to the keys the lattice homes it at.
		split := map[string][]byte{}

		if !agree {
			var flavourTargets []string
			hashes := map[string]string{}
			contentsOf := map[string][]byte{}

			for _, f := range flavours {
				flavourTargets = append(flavourTargets, f.target)
				hashes[f.target] = string(f.contents)
				contentsOf[f.target] = f.contents
			}

			for _, class := range partitionContentClasses(flavourTargets, hashes) {
				for _, folder := range lattice.homeFolders(class) {
					splittable = splittable && len(folder) > 0 && !isNestedPackageFolder(packageDir, folder)
					split[filepath.Join(packageDir, folder, name)] = contentsOf[class[0]]
				}
			}
		}

		destinations := map[string][]byte{}

		if agree || !splittable {
			destinations[filepath.Join(packageDir, name)] = flavours[0].contents

			if !agree {
				addResidualNote(residual, pkg, fmt.Sprintf("review sibling %s differs by target, and a flavour's key "+
					"folder is a nested package, so %s's was kept flat", name, flavours[0].target))
			}
		} else {
			destinations = split
		}

		// Every place the sibling can be: flat, and each key folder that is the package's own (not a nested package
		// named for a GOOS, internal/syscall/windows), whether it holds source or the sibling alone.
		candidates := []string{filepath.Join(packageDir, name)}

		for _, folder := range lattice.candidateFolders() {
			if !isNestedPackageFolder(packageDir, folder) {
				candidates = append(candidates, filepath.Join(packageDir, folder, name))
			}
		}

		for _, destination := range sortedMapKeys(destinations) {
			changed, err := writeMergedContents(destination, string(destinations[destination]))

			if err != nil {
				return written, err
			}

			if changed {
				written++
			}
		}

		for _, candidate := range candidates {
			if _, keep := destinations[candidate]; keep {
				continue
			}

			gone, err := removeIfPresent(candidate)

			if err != nil {
				return written, err
			}

			if gone {
				written++

				// A key folder that held the sibling alone is now empty and goes with it. One holding anything else
				// is not empty, and this removal fails, as it should.
				if dir := filepath.Dir(candidate); dir != packageDir {
					_ = os.Remove(dir)
				}
			}
		}
	}

	return written, nil
}

// companionLogicalPath returns a companion's logical path (`<pkg>/<file>`, any per-GOOS folder folded away), read
// through the first emission that holds it, as companionPackage reads its package.
func companionLogicalPath(rawPath string, emissions []*platformEmission) string {
	for _, emission := range emissions {
		if state, ok := emission.artifacts[rawPath]; ok {
			return state.logicalPath(rawPath)
		}
	}

	return rawPath
}

// addResidualNote records a difference the merge could not express for a package, keeping any note already there.
func addResidualNote(residual map[string]string, pkg string, note string) {
	if existing, ok := residual[pkg]; ok && len(existing) > 0 {
		residual[pkg] = existing + "; " + note
		return
	}

	residual[pkg] = note
}

// sortedMapKeys returns a map's keys in sorted order.
func sortedMapKeys[V any](values map[string]V) []string {
	keys := make([]string, 0, len(values))

	for key := range values {
		keys = append(keys, key)
	}

	sort.Strings(keys)
	return keys
}

func sortedStrings(values []string) []string {
	sorted := append([]string(nil), values...)
	sort.Strings(sorted)
	return sorted
}

// convertedPackages returns the packages (logical `<pkg>` paths) a target CONVERTED this run: those
// with at least one `.cs` it emitted. A census stage writes every source whether or not its bytes
// changed (alwaysWriteSources), so for `.cs` emission is exact, unlike for a companion, which the
// converter may leave untouched when it would rewrite the same bytes. A package the target does not
// build — or a hand-owned one, which the conversion never writes as a plain `.cs` — is absent.
func convertedPackages(emission *platformEmission) map[string]bool {
	packages := map[string]bool{}

	for rawPath, state := range emission.artifacts {
		if state.emitted && filepath.Ext(rawPath) == ".cs" {
			packages[path.Dir(state.logicalPath(rawPath))] = true
		}
	}

	return packages
}

// companionPackage returns the logical package a companion belongs to, read through the first
// emission that holds it so a per-GOOS folder is folded exactly as convertedPackages folds it.
func companionPackage(rawPath string, emissions []*platformEmission) string {
	for _, emission := range emissions {
		if state, ok := emission.artifacts[rawPath]; ok {
			return path.Dir(state.logicalPath(rawPath))
		}
	}

	return path.Dir(rawPath)
}

// perGOOSReviewSiblingOwner returns the GOOS whose folder a `.cs.auto` review sibling sits in, or ""
// for anything else: a flat sibling, or any other companion. Every staging root is a whole-tree copy
// of the L3 seed (snapshotConvertedRoot), so a sibling under `<pkg>/linux/` is PRESENT in the
// Windows stage too, but only as its seed copy. Only the Linux stage can rewrite it. The folder is
// read through the snapshot's own fold, so a package whose name is a GOOS (`internal/syscall/windows`)
// is never mistaken for a per-GOOS folder.
//
// Measured (H0, 2026-10-14): siginfo_linux.cs.auto and exec_unix.cs.auto were rewritten in the Linux
// stage and kept their seed bytes in the root, because the Windows stage, first in -platforms order,
// "had" them.
func perGOOSReviewSiblingOwner(rawPath string, emissions []*platformEmission) string {
	if !strings.HasSuffix(rawPath, ".cs.auto") {
		return ""
	}

	for _, emission := range emissions {
		if state, ok := emission.artifacts[rawPath]; ok {
			if state.logicalPath(rawPath) == rawPath {
				return ""
			}

			return path.Base(path.Dir(rawPath))
		}
	}

	return ""
}

// mergePlatformProjectFile composes ONE project file out of the several a package's targets emitted,
// expressing their differing import sets as layout L3's conditioned <ProjectReference> groups.
//
// The second result is a note (empty when there is nothing to say) describing a difference the
// merge could NOT express — two targets' project files disagreeing outside their reference lists.
//
// Only the targets that CONVERTED the package are read (converted, per target, from
// convertedPackages), as mergeCompanionArtifacts reads its companions. Every stage is a whole-tree
// copy of the seed, so a target that does not build the package still HAS its project file, as the
// seed's bytes: reading that copy kept the seeded template as the base of a package only darwin or
// linux converts, and took its reference list for an import set, conditioning references by a GOOS
// that never builds the package. Measured at the hop's fixed-point regen (2026-10-23): 4 such packages
// missed the interceptor opt-in, and internal/syscall/windows kept a seeded split. Only when no target
// converted the package (a hand-owned one) are all the targets that hold it read.
func mergePlatformProjectFile(rawPath string, targets []string, emissions []*platformEmission, converted []map[string]bool) (string, string, error) {
	sets := map[string][]string{}

	base, baseTarget, baseRemainder, note := "", "", "", ""
	needsUnsafe, needsFriend := false, false

	pkg := companionPackage(rawPath, emissions)
	converts := func(i int) bool { return i < len(converted) && converted[i][pkg] }
	anyConverts := false

	for i := range targets {
		if _, ok := emissions[i].artifacts[rawPath]; ok && converts(i) {
			anyConverts = true
			break
		}
	}

	for i, target := range targets {
		if _, ok := emissions[i].artifacts[rawPath]; !ok {
			// This target has no such package at all (a platform-exclusive one). Nothing to read,
			// and nothing to condition — its absence is expressed by the exclusive package's own
			// sources living in a per-GOOS folder, not by this file.
			continue
		}

		if anyConverts && !converts(i) {
			// The seed's copy, in a stage that never converted the package: not this target's emission.
			continue
		}

		contents, err := os.ReadFile(filepath.Join(emissions[i].root, "core", filepath.FromSlash(rawPath)))

		if err != nil {
			return "", "", fmt.Errorf("failed to read staged project file %q for target %s: %w", rawPath, target, err)
		}

		references, ok := platformFullReferenceSet(string(contents), target)

		if !ok {
			// Not a project file this machinery recognizes (a hand-owned one, or a template without
			// the golib anchor). Merging it would be guesswork; copy the first target's verbatim.
			if len(base) == 0 {
				base = string(contents)
			}

			return base, fmt.Sprintf("project file has no recognizable <ProjectReference> group; %s's copy was kept", goosOfTarget(targets[0])), nil
		}

		sets[target] = references

		if strings.Contains(string(contents), unsafeBlocksEnabled) {
			needsUnsafe = true
		}

		if strings.Contains(string(contents), friendAssemblyAccessGroup) {
			needsFriend = true
		}

		remainder := stripPlatformReferences(friendAccessUnion(unsafeBlocksUnion(string(contents), true), true))

		if len(base) == 0 {
			base, baseTarget, baseRemainder = string(contents), target, remainder
			continue
		}

		if remainder != baseRemainder && len(note) == 0 {
			note = fmt.Sprintf("%s and %s disagree outside their reference lists", baseTarget, target)
		}
	}

	// Split over the lattice of the merge's own targets, the ones whose sets were just read.
	shared, deltas, _ := splitPlatformReferenceSetsOver(mustPlatformLattice(targets), sets)
	merged, err := renderPlatformReferences(friendAccessUnion(unsafeBlocksUnion(base, needsUnsafe), needsFriend), shared, deltas)

	if err != nil {
		return base, note, fmt.Errorf("failed to compose conditioned references for %q: %w", rawPath, err)
	}

	return merged, note, nil
}

const (
	unsafeBlocksEnabled  = "<AllowUnsafeBlocks>true</AllowUnsafeBlocks>"
	unsafeBlocksDisabled = "<AllowUnsafeBlocks>false</AllowUnsafeBlocks>"
)

// unsafeBlocksUnion raises a merged project file's <AllowUnsafeBlocks> to the UNION across the
// targets, because `usesUnsafeCode` is a per-package emission fact that can differ by platform —
// measured at 2 packages, `os/user` and `syscall`, both unsafe on Windows and not on the unix side.
//
// The union rather than the first target's value, and it is not a stylistic preference: a .csproj is
// ONE file serving every platform, so a `false` inherited from a target that happens to be first
// makes the platform that DOES need unsafe uncompilable (CS0227). The property is permissive — it
// grants a capability, it does not use one — so raising it is inert for a platform whose emission
// contains no unsafe code, and no IL moves.
//
// It is also, measurably, byte-neutral for the corpus as it stands: in both diverging packages the
// permissive side IS Windows, so the union equals what the Windows lane already ships. The rule is
// here so that stays true by construction rather than by which target the merge read first.
func unsafeBlocksUnion(contents string, needsUnsafe bool) string {
	if !needsUnsafe {
		return contents
	}

	return strings.Replace(contents, unsafeBlocksDisabled, unsafeBlocksEnabled, 1)
}

// friendAccessUnion gives a merged project file the friend-access group (insertFriendAssemblyAccess) when ANY
// converting target emitted it. The writer emits the group when the target being converted sees in-package test
// files, and those can be GOOS-constrained: log/syslog's only one, syslog_test.go, is `!windows`, so linux and
// darwin emitted the group and windows did not, and the merge kept windows' copy (BOARD 2026-11-03, G). A
// .csproj is ONE file serving every platform, so this takes the union for the reason unsafeBlocksUnion does:
// InternalsVisibleTo grants access and uses none, so it is inert where nothing needs it, while its absence
// leaves the linux and darwin packages without the go2cs.SynthesizedStructs grant reflect.StructOf relies on.
func friendAccessUnion(contents string, needsFriend bool) string {
	if !needsFriend || strings.Contains(contents, friendAssemblyAccessGroup) {
		return contents
	}

	return insertFriendAssemblyAccess(contents)
}

// stripPlatformReferences returns a project file with every reference removed — the unconditioned
// list emptied and the conditioned block dropped — so two targets' project files can be compared for
// differences that are NOT about references.
func stripPlatformReferences(contents string) string {
	stripped, err := renderPlatformReferences(contents, nil, nil)

	if err != nil {
		return contents
	}

	return stripped
}

// writeMergedContents writes composed (rather than copied) project-file bytes to their merged
// destination, skipping the write when they are already there. Reports whether anything was written.
func writeMergedContents(destination string, contents string) (bool, error) {
	if !needToWriteFile(destination, []byte(contents)) {
		return false, nil
	}

	if err := os.MkdirAll(filepath.Dir(destination), 0755); err != nil {
		return false, fmt.Errorf("failed to create output directory %q: %w", filepath.Dir(destination), err)
	}

	if err := os.WriteFile(destination, []byte(contents), 0644); err != nil {
		return false, fmt.Errorf("failed to write merged project file %q: %w", destination, err)
	}

	return true, nil
}

// companionDiffersAcrossTargets reports whether the targets that hold this artifact hold different
// bytes for it. Seeded (un-emitted) copies count: a target that did not rewrite its project file
// still HAS the correct one, which is exactly the reading buildCsprojSummary takes.
func companionDiffersAcrossTargets(rawPath string, emissions []*platformEmission) bool {
	first := ""

	for _, emission := range emissions {
		state, ok := emission.artifacts[rawPath]

		if !ok {
			continue
		}

		if len(first) == 0 {
			first = state.hash
			continue
		}

		if state.hash != first {
			return true
		}
	}

	return false
}

// carriesConvertedPlatformAxis reports whether a project file that every target holds identically must still
// go through mergePlatformProjectFile: it carries a per-GOOS reference block, and at least one target converted
// its package. Identical copies prove nothing about the block, because the single-target writer recovers the
// other platforms' sets from the file it overwrites (adoptPlatformReferences): a GOOS that never converts the
// package keeps its recorded group forever. Measured at the hop's freeze regen (2026-10-23):
// internal/syscall/windows selects Go files on Windows alone, yet kept darwin and linux groups, and the emission
// had two fixed points for the file, split or flat, depending on whether an unrelated change made the stage
// copies differ. Re-rendering from the converting stages is idempotent for a genuinely multi-platform package.
func carriesConvertedPlatformAxis(rawPath string, emissions []*platformEmission, converted []map[string]bool) bool {
	pkg := companionPackage(rawPath, emissions)
	contents := ""

	for i, emission := range emissions {
		if _, ok := emission.artifacts[rawPath]; !ok {
			continue
		}

		if i < len(converted) && converted[i][pkg] && len(contents) == 0 {
			data, err := os.ReadFile(filepath.Join(emission.root, "core", filepath.FromSlash(rawPath)))

			if err != nil {
				return false
			}

			contents = string(data)
		}
	}

	return strings.Contains(contents, platformReferenceBlockHeader)
}

// applyLayoutBlockToProjectFile adds the L3 blocks to a package's project file when the package now
// carries per-GOOS sources and the file does not already say so. Reports whether it changed.
func applyLayoutBlockToProjectFile(packageDir string) (bool, error) {
	if !packageCarriesPlatformLayout(packageDir) {
		return false, nil
	}

	entries, err := os.ReadDir(packageDir)

	if err != nil {
		return false, fmt.Errorf("failed to read package directory %q: %w", packageDir, err)
	}

	for _, entry := range entries {
		if entry.IsDir() || !strings.EqualFold(filepath.Ext(entry.Name()), ".csproj") {
			continue
		}

		projectFileName := filepath.Join(packageDir, entry.Name())
		contents, err := os.ReadFile(projectFileName)

		if err != nil {
			return false, fmt.Errorf("failed to read project file %q: %w", projectFileName, err)
		}

		updated := applyPlatformLayoutBlocks(string(contents), projectFileName)

		// The arch items follow the package's arch and pair folders both ways: added when it holds one,
		// and taken out again when its last one has gone while its GOOS folders stay, which is the file
		// a single-target writer renders for it either way (writeProjectFile).
		if packageCarriesArchLayout(packageDir) {
			updated = applyPlatformArchLayoutBlocks(updated, projectFileName)
		} else {
			updated = removePlatformArchLayoutBlocks(updated)
		}

		if !needToWriteFile(projectFileName, []byte(updated)) {
			return false, nil
		}

		if err := os.WriteFile(projectFileName, []byte(updated), 0644); err != nil {
			return false, fmt.Errorf("failed to write project file %q: %w", projectFileName, err)
		}

		return true, nil
	}

	return false, nil
}

// dropLayoutBlockFromProjectFile takes the L3 blocks back out of a package's project file once the package carries no
// per-GOOS sources, and removes the per-GOOS folders the fold emptied. A package that is still L3 is left alone.
// Reports whether the project file changed.
func dropLayoutBlockFromProjectFile(packageDir string, targets []string) (bool, error) {
	if packageCarriesPlatformLayout(packageDir) {
		return false, nil
	}

	for _, name := range mustPlatformLattice(targets).candidateFolders() {
		folder := filepath.Join(packageDir, name)

		if entries, err := os.ReadDir(folder); err == nil && len(entries) == 0 {
			if err := os.Remove(folder); err != nil {
				return false, fmt.Errorf("failed to remove emptied per-GOOS folder %q: %w", folder, err)
			}
		}
	}

	entries, err := os.ReadDir(packageDir)

	if err != nil {
		return false, fmt.Errorf("failed to read package directory %q: %w", packageDir, err)
	}

	dropped := false

	for _, entry := range entries {
		if entry.IsDir() || !strings.EqualFold(filepath.Ext(entry.Name()), ".csproj") || strings.HasSuffix(strings.ToLower(entry.Name()), ".tests.csproj") {
			continue
		}

		projectFileName := filepath.Join(packageDir, entry.Name())
		contents, err := os.ReadFile(projectFileName)

		if err != nil {
			return false, fmt.Errorf("failed to read project file %q: %w", projectFileName, err)
		}

		updated := []byte(removePlatformLayoutBlocks(string(contents)))

		if !needToWriteFile(projectFileName, updated) {
			continue
		}

		if err := os.WriteFile(projectFileName, updated, 0644); err != nil {
			return false, fmt.Errorf("failed to write project file %q: %w", projectFileName, err)
		}

		dropped = true
	}

	return dropped, nil
}

// copyMergedFile copies a staged artifact to its merged destination, creating directories as
// needed and skipping the write when the bytes are already there (so timestamps stay meaningful and
// a re-run is a genuine no-op). Reports whether anything was written.
func copyMergedFile(source string, destination string) (bool, error) {
	contents, err := os.ReadFile(source)

	if err != nil {
		return false, fmt.Errorf("failed to read staged artifact %q: %w", source, err)
	}

	if !needToWriteFile(destination, contents) {
		return false, nil
	}

	if err := os.MkdirAll(filepath.Dir(destination), 0755); err != nil {
		return false, fmt.Errorf("failed to create output directory %q: %w", filepath.Dir(destination), err)
	}

	if err := os.WriteFile(destination, contents, 0644); err != nil {
		return false, fmt.Errorf("failed to write merged artifact %q: %w", destination, err)
	}

	return true, nil
}

// retireUnemittedPackageInitFiles removes the corpus's generated package_init.cs, flat and per-GOOS, of every package a
// target converted when NO target emitted one. A conversion that relocates nothing retires its stale package_init.cs
// (retireStalePackageInitFile), but in its own staging root: a file no target emitted is in no merge plan, so the corpus
// kept a static constructor calling init methods the package no longer declares (CS0103). Only a file carrying the
// generated header goes; a package no target converted is left alone. emitted holds the merge plans' logical paths.
func retireUnemittedPackageInitFiles(coreDir string, targets []string, emissions []*platformEmission, emitted map[string]bool) (int, error) {
	lattice := mustPlatformLattice(targets)
	converted := map[string]bool{}

	for _, emission := range emissions {
		for pkg := range convertedPackages(emission) {
			converted[pkg] = true
		}
	}

	removed := 0

	for _, pkg := range sortedKeys(converted) {
		if emitted[path.Join(pkg, PackageInitFileName)] {
			continue
		}

		packageDir := filepath.Join(coreDir, filepath.FromSlash(pkg))
		dirs := []string{packageDir}

		// Every key folder of the lattice, admitted or not, that is a source folder rather than a
		// nested package of the same name (whose package_init.cs is its own).
		for _, folder := range lattice.candidateFolders() {
			if isPlatformSourceFolder(packageDir, folder) {
				dirs = append(dirs, filepath.Join(packageDir, folder))
			}
		}

		for _, dir := range dirs {
			initPath := filepath.Join(dir, PackageInitFileName)

			if _, err := os.Stat(initPath); err != nil {
				continue
			}

			if err := retireStalePackageInitFile(dir); err != nil {
				return removed, err
			}

			if _, err := os.Stat(initPath); errors.Is(err, os.ErrNotExist) {
				removed++
			}
		}
	}

	return removed, nil
}

// removeIfPresent deletes a stale copy of an artifact that moved between the flat and per-GOOS
// locations. Reports whether anything was removed.
func removeIfPresent(filePath string) (bool, error) {
	if _, err := os.Stat(filePath); err != nil {
		if os.IsNotExist(err) {
			return false, nil
		}

		return false, fmt.Errorf("failed to inspect %q: %w", filePath, err)
	}

	if err := os.Remove(filePath); err != nil {
		return false, fmt.Errorf("failed to remove stale artifact %q: %w", filePath, err)
	}

	return true, nil
}

// normalizeArtifactLogicalPaths folds a layout key folder segment (GOOS, GOARCH or `<goos>-<arch>`)
// out of every artifact path, so the classification and the merge speak in `<pkg>/<file>`
// regardless of which layout the tree they were snapshotted from is in. A path is folded only when
// its directory holds no project file and its PARENT does — `internal/syscall/windows` is a
// converted package whose own name is a GOOS, and folding it would move its sources into
// `internal/syscall`.
func normalizeArtifactLogicalPaths(artifacts map[string]artifactState) {
	packageDirs := map[string]bool{}

	for relPath := range artifacts {
		if strings.EqualFold(path.Ext(relPath), ".csproj") {
			packageDirs[path.Dir(relPath)] = true
		}
	}

	for relPath, state := range artifacts {
		state.logical = relPath
		dir := path.Dir(relPath)

		if !packageDirs[dir] && isLayoutKeyFolderName(path.Base(dir)) && packageDirs[path.Dir(dir)] {
			state.logical = path.Join(path.Dir(dir), path.Base(relPath))
		}

		artifacts[relPath] = state
	}
}
