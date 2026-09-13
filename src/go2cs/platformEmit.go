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
//	go2cs -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 -go2cspath <repo>\src
//
// converts once per target and merges the three emissions into ONE tree, per L3: an artifact that is
// byte-identical on every target is written flat in its package directory, and anything else — a
// name whose content varies, or one only some targets emit at all — is written to that package's
// `<goos>/` subfolder, once per emitting target.
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
	"fmt"
	"io/fs"
	"os"
	"path"
	"path/filepath"
	"sort"
	"strings"
)

// runPlatformEmission is the entry point for a `-platforms <two or more targets>` conversion: stage
// each target, classify the emissions, and merge them into the -go2cspath corpus as layout L3.
func runPlatformEmission(options Options, stageDir string, packageFilter []string) error {
	targets := options.targetPlatforms

	if len(targets) < 2 {
		return fmt.Errorf("multi-platform emission needs at least two -platforms targets (got %d)", len(targets))
	}

	// L3 keys its source folders by GOOS, so two targets sharing one GOOS would write into the same
	// folder and the second would silently overwrite the first. The GOARCH axis is real (design §5:
	// 16 packages) and is increment 5's subject; refuse rather than emit a tree that looks fine.
	seenGOOS := map[string]string{}

	for _, target := range targets {
		goos := goosOfTarget(target)

		if previous, exists := seenGOOS[goos]; exists {
			return fmt.Errorf("targets %q and %q share GOOS %q: layout L3 keys its source folders by GOOS, "+
				"so one emission per GOOS is the most this can write (the GOARCH axis is a later increment)",
				previous, target, goos)
		}

		seenGOOS[goos] = target
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

	stageDir, removeStage, err := resolveEmissionStageDir(stageDir)

	if err != nil {
		return err
	}

	if removeStage {
		defer os.RemoveAll(stageDir)
	}

	fmt.Printf("\nMulti-platform emission (layout L3)\n")
	fmt.Printf("  corpus root: %s\n", rootPath)
	fmt.Printf("  staging dir: %s\n", stageDir)
	fmt.Printf("  targets:     %s\n\n", strings.Join(targets, ", "))

	seed, err := readCensusSeed(filepath.Join(rootPath, "core"))

	if err != nil {
		return err
	}

	fmt.Printf("  seed holds %d files, %d of them hand-owned ([module: GoManualConversion])\n\n",
		len(seed.hashes), len(seed.markedFiles))

	emissions := make([]*platformEmission, 0, len(targets))

	// STRICTLY SEQUENTIAL, one fresh root each — the r41 never-convert-twice-into-one-root rule.
	for i, target := range targets {
		fmt.Printf("=== [%d/%d] emission target %s\n", i+1, len(targets), target)

		emission, err := runCensusTarget(options, rootPath, stageDir, target, packageFilter, seed)

		if err != nil {
			return fmt.Errorf("emission target %s failed: %w", target, err)
		}

		// A staging root whose seeding did not take measures a corpus that does not exist, and here
		// it would be MERGED into the real one. Fatal, not a footnote.
		if len(emission.markerGateViolations) > 0 {
			return fmt.Errorf("target %s emitted %d hand-owned file(s) as plain .cs — the staging root was not seeded correctly; "+
				"first offender: %s", target, len(emission.markerGateViolations), emission.markerGateViolations[0])
		}

		emissions = append(emissions, emission)
	}

	return mergePlatformEmissions(rootPath, targets, emissions)
}

// resolveEmissionStageDir resolves the staging directory: the caller's -platform-stage when given
// (kept afterwards, so a corpus-wide run can be inspected), otherwise a fresh temporary directory
// this run owns and removes. Reports whether the caller owns cleanup.
func resolveEmissionStageDir(stageDir string) (string, bool, error) {
	if stageDir = strings.TrimSpace(stageDir); len(stageDir) > 0 {
		resolved, err := filepath.Abs(stageDir)

		if err != nil {
			return "", false, fmt.Errorf("failed to resolve staging directory %q: %w", stageDir, err)
		}

		if err := os.MkdirAll(resolved, 0755); err != nil {
			return "", false, fmt.Errorf("failed to create staging directory %q: %w", resolved, err)
		}

		return resolved, false, nil
	}

	resolved, err := os.MkdirTemp("", "go2cs-platform-emit-")

	if err != nil {
		return "", false, fmt.Errorf("failed to create a staging directory: %w", err)
	}

	return resolved, true, nil
}

// mergedArtifact is one logical artifact's merge plan: where each emitting target's copy was staged.
type mergedArtifact struct {
	class   string
	staged  map[string]string // target -> absolute path of that target's staged copy
	emitted []string          // emitting targets, in target order
}

// mergePlatformEmissions writes the classified emissions into the corpus as layout L3.
func mergePlatformEmissions(rootPath string, targets []string, emissions []*platformEmission) error {
	coreDir := filepath.Join(rootPath, "core")

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

	for logical, class := range classes {
		plan := mergedArtifact{class: class.class, staged: map[string]string{}, emitted: class.emitters}

		for i, target := range targets {
			if stagedPath, ok := stagedByTarget[i][logical]; ok {
				plan.staged[target] = stagedPath
			}
		}

		plans[logical] = plan
	}

	written, removed, unchanged := 0, 0, 0
	layoutPackages := map[string]bool{}

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
		flatPath := filepath.Join(coreDir, filepath.FromSlash(logical))

		if plan.class == artifactIdentical {
			// Shared on every target: one flat copy, and the per-GOOS copies (if this artifact used
			// to vary) go away.
			changed, err := copyMergedFile(plan.staged[targets[0]], flatPath)

			if err != nil {
				return err
			}

			countWrite(changed, &written, &unchanged)

			for _, target := range targets {
				gone, err := removeIfPresent(filepath.Join(coreDir, filepath.FromSlash(pkg), goosOfTarget(target), fileName))

				if err != nil {
					return err
				}

				if gone {
					removed++
				}
			}

			continue
		}

		// Varies, or is emitted by only some targets: one copy per emitting target, in that
		// target's GOOS folder — and the flat copy, if the artifact used to be shared, goes away.
		for _, target := range plan.emitted {
			goos := goosOfTarget(target)
			changed, err := copyMergedFile(plan.staged[target], filepath.Join(coreDir, filepath.FromSlash(pkg), goos, fileName))

			if err != nil {
				return err
			}

			countWrite(changed, &written, &unchanged)
		}

		layoutPackages[pkg] = true

		gone, err := removeIfPresent(flatPath)

		if err != nil {
			return err
		}

		if gone {
			removed++
		}
	}

	conditioned, residual, companionWritten, err := mergeCompanionArtifacts(coreDir, targets, emissions)

	if err != nil {
		return err
	}

	written += companionWritten

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
		fmt.Printf("\n⚠ %d package(s) emitted project files that differ OUTSIDE their <ProjectReference>\n", len(residual))
		fmt.Printf("  lists. Layout L3 conditions references only, so %s's remainder was kept:\n", goosOfTarget(targets[0]))

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
func reconcileLayoutDuplicates(coreDir string, targets []string) (int, error) {
	platformFolders := map[string]bool{}

	for _, target := range targets {
		platformFolders[goosOfTarget(target)] = true
	}

	removed := 0
	var conflicts []string

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

		for _, goos := range sortedKeys(platformFolders) {
			// isPlatformSourceFolder, not a bare directory-name test: `internal/syscall/windows` is
			// a real PACKAGE whose directory happens to be named for a GOOS, and its sources are
			// not a platform variant of its parent's.
			if !isPlatformSourceFolder(dirPath, goos) {
				continue
			}

			platformDir := filepath.Join(dirPath, goos)
			platformEntries, err := os.ReadDir(platformDir)

			if err != nil {
				return err
			}

			for _, platformEntry := range platformEntries {
				if platformEntry.IsDir() {
					continue
				}

				flatPath := filepath.Join(dirPath, platformEntry.Name())
				flatInfo, err := os.Stat(flatPath)

				if err != nil || !flatInfo.Mode().IsRegular() {
					continue
				}

				platformPath := filepath.Join(platformDir, platformEntry.Name())
				identical, err := filesHaveIdenticalBytes(platformPath, flatPath)

				if err != nil {
					return err
				}

				if !identical {
					conflicts = append(conflicts, fmt.Sprintf("  %s\n  %s",
						corpusRelativePath(coreDir, flatPath), corpusRelativePath(coreDir, platformPath)))

					continue
				}

				gone, err := removeIfPresent(platformPath)

				if err != nil {
					return err
				}

				if gone {
					removed++
				}
			}
		}

		return nil
	})

	if err != nil {
		return removed, err
	}

	if len(conflicts) > 0 {
		sort.Strings(conflicts)

		return removed, fmt.Errorf("layout L3: %d artifact(s) exist BOTH flat and under a per-GOOS "+
			"folder with DIFFERING contents, so the affected target would compile both copies "+
			"(CS0579/CS1537). No target re-emitted them, so the merge cannot tell which platform "+
			"owns the flat copy — resolve by hand, or delete both and reconvert:\n%s",
			len(conflicts), strings.Join(conflicts, "\n\n"))
	}

	return removed, nil
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

	for _, emission := range emissions {
		for rawPath, state := range emission.artifacts {
			if state.emitted && filepath.Ext(rawPath) != ".cs" && artifactPackage(rawPath) != "." {
				names[rawPath] = true
			}
		}
	}

	conditioned := map[string]bool{}
	residual := map[string]string{}

	for _, rawPath := range sortedKeys(names) {
		destination := filepath.Join(coreDir, filepath.FromSlash(rawPath))

		if strings.EqualFold(filepath.Ext(rawPath), ".csproj") && companionDiffersAcrossTargets(rawPath, emissions) {
			merged, note, err := mergePlatformProjectFile(rawPath, targets, emissions)

			if err != nil {
				return nil, nil, written, err
			}

			if len(note) > 0 {
				residual[artifactPackage(rawPath)] = note
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
		source := ""

		for _, emission := range emissions {
			if _, ok := emission.artifacts[rawPath]; ok {
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

// mergePlatformProjectFile composes ONE project file out of the several a package's targets emitted,
// expressing their differing import sets as layout L3's conditioned <ProjectReference> groups.
//
// The second result is a note (empty when there is nothing to say) describing a difference the
// merge could NOT express — two targets' project files disagreeing outside their reference lists.
func mergePlatformProjectFile(rawPath string, targets []string, emissions []*platformEmission) (string, string, error) {
	sets := map[string][]string{}

	base, baseTarget, baseRemainder, note := "", "", "", ""
	needsUnsafe := false

	for i, target := range targets {
		if _, ok := emissions[i].artifacts[rawPath]; !ok {
			// This target has no such package at all (a platform-exclusive one). Nothing to read,
			// and nothing to condition — its absence is expressed by the exclusive package's own
			// sources living in a per-GOOS folder, not by this file.
			continue
		}

		contents, err := os.ReadFile(filepath.Join(emissions[i].root, "core", filepath.FromSlash(rawPath)))

		if err != nil {
			return "", "", fmt.Errorf("failed to read staged project file %q for target %s: %w", rawPath, target, err)
		}

		goos := goosOfTarget(target)
		references, ok := platformFullReferenceSet(string(contents), goos)

		if !ok {
			// Not a project file this machinery recognizes (a hand-owned one, or a template without
			// the golib anchor). Merging it would be guesswork; copy the first target's verbatim.
			if len(base) == 0 {
				base = string(contents)
			}

			return base, fmt.Sprintf("project file has no recognizable <ProjectReference> group; %s's copy was kept", goosOfTarget(targets[0])), nil
		}

		sets[goos] = references

		if strings.Contains(string(contents), unsafeBlocksEnabled) {
			needsUnsafe = true
		}

		remainder := stripPlatformReferences(unsafeBlocksUnion(string(contents), true))

		if len(base) == 0 {
			base, baseTarget, baseRemainder = string(contents), target, remainder
			continue
		}

		if remainder != baseRemainder && len(note) == 0 {
			note = fmt.Sprintf("%s and %s disagree outside their reference lists", baseTarget, target)
		}
	}

	shared, deltas, _ := splitPlatformReferenceSets(sets)
	merged, err := renderPlatformReferences(unsafeBlocksUnion(base, needsUnsafe), shared, deltas)

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

		updated := []byte(applyPlatformLayoutBlocks(string(contents), projectFileName))

		if !needToWriteFile(projectFileName, updated) {
			return false, nil
		}

		if err := os.WriteFile(projectFileName, updated, 0644); err != nil {
			return false, fmt.Errorf("failed to write project file %q: %w", projectFileName, err)
		}

		return true, nil
	}

	return false, nil
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

// normalizeArtifactLogicalPaths folds a per-GOOS folder segment out of every artifact path, so the
// classification and the merge speak in `<pkg>/<file>` regardless of which layout the tree they were
// snapshotted from is in. A path is folded only when its directory holds no project file and its
// PARENT does — `internal/syscall/windows` is a converted package whose own name is a GOOS, and
// folding it would move its sources into `internal/syscall`.
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

		if !packageDirs[dir] && isKnownGOOS(path.Base(dir)) && packageDirs[path.Dir(dir)] {
			state.logical = path.Join(path.Dir(dir), path.Base(relPath))
		}

		artifacts[relPath] = state
	}
}
