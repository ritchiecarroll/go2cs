// platformManifest.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the CLASSIFICATION the multi-platform census produces, and the manifest that
// records it — increment 1's whole deliverable (docs/phase4/DESIGN-multiplatform-corpus.md §12).
//
// Every emitted artifact name in the union of the targets' emissions lands in exactly one of four
// buckets, and the partition is by how many targets emitted the NAME first, content second:
//
//	identical   emitted by every target, byte-identical everywhere      → layout L3 writes it FLAT
//	variant     emitted by every target, at least two contents differ   → one copy per GOOS folder
//	partial     emitted by some but not all targets (the shared unix    → one copy per emitting GOOS
//	            variants, when the target set is windows/linux/darwin)
//	exclusive   emitted by exactly one target                           → one copy, in its GOOS folder
//
// That ordering matters: a file emitted by only two of three targets is `partial` even if those two
// contents differ, because L3's cost for it is two copies either way. The design's §8 pricing table
// is exactly Σ|emitting targets| over the three non-identical buckets, so the manifest computes it
// that way rather than from hard-coded multipliers.
//
// The manifest also carries the per-PAIR view (§4's Windows↔Linux / Windows↔macOS / Linux↔macOS
// table), because the pair distances are what make the third platform cheap, and the arch axis (§5)
// is nothing but a two-target census of the same shape.
//
// Nothing here writes into a corpus. The manifest is a report; increment 2 and 3 are what act on it.

package main

import (
	"encoding/json"
	"fmt"
	"os"
	"path"
	"sort"
	"strings"
)

// Artifact classification buckets, spelled once so the manifest and the summary agree.
const (
	artifactIdentical = "identical"
	artifactVariant   = "variant"
	artifactPartial   = "partial"
	artifactExclusive = "exclusive"
)

// platformManifest is the census artifact. Field names are lowerCamel JSON so the file reads the way
// the design's tables do.
type platformManifest struct {
	Schema     string                `json:"schema"`
	SeedRoot   string                `json:"seedRoot"`
	CensusDir  string                `json:"censusDir"`
	Targets    []string              `json:"targets"`
	BuildTags  []string              `json:"buildTags"`
	Comments   bool                  `json:"comments"`
	GoRoot     string                `json:"goRoot"`
	TargetInfo []targetManifestEntry `json:"targetDetail"`

	Summary  censusSummary        `json:"summary"`
	Pairwise []pairwiseComparison `json:"pairwise"`
	Csproj   csprojSummary        `json:"csproj"`

	VariantFiles   []string `json:"variantFiles"`
	PartialFiles   []string `json:"partialFiles"`
	ExclusiveFiles []string `json:"exclusiveFiles"`

	VariantSourceFiles      []string `json:"variantSourceFiles"`
	VariantPackageInfoFiles []string `json:"variantPackageInfoFiles"`
	VariantPackageInitFiles []string `json:"variantPackageInitFiles"`

	PackagesWithDelta []string `json:"packagesWithDelta"`
}

// targetManifestEntry is one target's own accounting — including the marker gate, which is the
// census's proof that the staging root really was seeded.
type targetManifestEntry struct {
	Target string `json:"target"`
	GOOS   string `json:"goos"`
	GOARCH string `json:"goarch"`
	Root   string `json:"root"`

	PackagesQueued int      `json:"packagesQueued"`
	PackagesFailed int      `json:"packagesFailed"`
	FailedPackages []string `json:"failedPackages,omitempty"`

	EmittedCs     int `json:"emittedCs"`
	EmittedCsproj int `json:"emittedCsproj"`
	EmittedReadme int `json:"emittedReadme"`
	EmittedTotal  int `json:"emittedTotal"`

	// The control's honesty check: an emission that reproduces the seed proves the diff instrument
	// reads zero on a null input, so every delta reported elsewhere is a real platform effect rather
	// than converter non-determinism. The middle count is the autocrlf phantom — corpus files holding
	// a multi-line string literal, whose in-string LFs the working tree has smudged to CRLF — kept
	// visible rather than folded away, because how much of the tree it touches is itself useful.
	EmittedCsMatchingSeed       int `json:"emittedCsMatchingSeed"`
	EmittedCsLineEndingOnlySeed int `json:"emittedCsLineEndingOnlyVsSeed"`
	EmittedCsDifferingSeed      int `json:"emittedCsDifferingFromSeed"`

	SeedManualConversionFiles int      `json:"seedManualConversionFiles"`
	MarkerGateViolations      []string `json:"markerGateViolations"`
}

// censusSummary is the N-way view — §4's three-way table, and §5's two-way one, from one shape.
type censusSummary struct {
	TargetCount int `json:"targetCount"`

	UnionCs               int `json:"unionEmittedCs"`
	IdenticalOnAllTargets int `json:"identicalOnAllTargets"`
	VariantSameName       int `json:"sameNameDifferentContent"`
	PartialEmitted        int `json:"emittedBySomeButNotAll"`
	ExclusiveEmitted      int `json:"platformExclusive"`

	PackagesEmitted      int `json:"packagesEmitted"`
	PackagesWithAnyDelta int `json:"packagesWithAnyDelta"`

	VariantByKind map[string]int `json:"variantByKind"`

	FlatSharedFiles    int     `json:"l3FlatSharedFiles"`
	PerGoosFiles       int     `json:"l3PerGoosFiles"`
	UnionTreeTotal     int     `json:"l3UnionTreeTotal"`
	SinglePlatformTree int     `json:"singlePlatformTreeFiles"`
	GrowthPercent      float64 `json:"l3GrowthPercent"`
}

// pairwiseComparison is one row of §4's pair table.
type pairwiseComparison struct {
	A string `json:"a"`
	B string `json:"b"`

	EmittedByBoth   int `json:"emittedByBoth"`
	Identical       int `json:"identical"`
	Differ          int `json:"differ"`
	AOnly           int `json:"aOnly"`
	BOnly           int `json:"bOnly"`
	PackagesTouched int `json:"packagesTouched"`
}

// csprojSummary tracks the reference-block axis: which runs rewrote a project file at all, and how
// many packages end up with a different direct import set per platform.
type csprojSummary struct {
	RewrittenPerTarget map[string]int `json:"rewrittenPerTarget"`
	DifferingPerTarget map[string]int `json:"differingFromFirstTargetPerTarget"`

	PackagesWithDifferingCsproj int      `json:"packagesWithDifferingCsproj"`
	Packages                    []string `json:"packages"`
}

// artifactClass is one artifact name's verdict across the target set.
type artifactClass struct {
	class    string
	emitters []string // targets that emitted this name, in target order
}

// classifyPlatformEmissions partitions the union of the targets' emitted `.cs` names into the four
// buckets. emissions[i] maps an artifact path to its content hash for targets[i]; a path absent from
// a map was not emitted by that target.
//
// This is the design's §4.2 requirement in code: the input is what the converter WROTE, never what
// `go list` reported, because four packages emit differently from identical Go source and three emit
// identically from differing Go source.
func classifyPlatformEmissions(targets []string, emissions []map[string]string) map[string]artifactClass {
	classes := map[string]artifactClass{}
	names := map[string]bool{}

	for _, emission := range emissions {
		for name := range emission {
			names[name] = true
		}
	}

	for name := range names {
		var emitters []string
		var hashes []string

		for i, target := range targets {
			if hash, ok := emissions[i][name]; ok {
				emitters = append(emitters, target)
				hashes = append(hashes, hash)
			}
		}

		class := artifactExclusive

		switch {
		case len(emitters) == len(targets):
			class = artifactIdentical

			for _, hash := range hashes[1:] {
				if hash != hashes[0] {
					class = artifactVariant
					break
				}
			}
		case len(emitters) > 1:
			class = artifactPartial
		}

		classes[name] = artifactClass{class: class, emitters: emitters}
	}

	return classes
}

// comparePlatformPair produces one row of the pairwise table for two targets' emissions.
func comparePlatformPair(a, b string, emissionA, emissionB map[string]string) pairwiseComparison {
	row := pairwiseComparison{A: a, B: b}
	touched := map[string]bool{}

	for name, hashA := range emissionA {
		if hashB, ok := emissionB[name]; ok {
			row.EmittedByBoth++

			if hashA == hashB {
				row.Identical++
			} else {
				row.Differ++
				touched[artifactPackage(name)] = true
			}
		} else {
			row.AOnly++
			touched[artifactPackage(name)] = true
		}
	}

	for name := range emissionB {
		if _, ok := emissionA[name]; !ok {
			row.BOnly++
			touched[artifactPackage(name)] = true
		}
	}

	row.PackagesTouched = len(touched)

	return row
}

// artifactPackage returns the converted package an artifact path belongs to — its directory under
// `core`, which IS the Go import path. A root-level file belongs to the pseudo-package ".".
func artifactPackage(artifactPath string) string {
	return path.Dir(artifactPath)
}

// variantKind buckets a varying artifact by what it is: the two generated metadata files are named
// exactly, everything else is a converted Go source file. §4.3's table is this split.
func variantKind(artifactPath string) string {
	switch path.Base(artifactPath) {
	case PackageInfoFileName:
		return PackageInfoFileName
	case PackageInitFileName:
		return PackageInitFileName
	}

	return "source"
}

// buildPlatformManifest assembles the census artifact from the per-target emissions.
func buildPlatformManifest(options Options, seedRoot, censusDir string, emissions []*platformEmission) *platformManifest {
	targets := make([]string, 0, len(emissions))
	csEmissions := make([]map[string]string, 0, len(emissions))

	manifest := &platformManifest{
		Schema:    platformManifestSchema,
		SeedRoot:  seedRoot,
		CensusDir: censusDir,
		BuildTags: options.buildTags,
		Comments:  options.includeComments,
		GoRoot:    options.goRoot,
		Csproj: csprojSummary{
			RewrittenPerTarget: map[string]int{},
			DifferingPerTarget: map[string]int{},
		},
	}

	for _, emission := range emissions {
		targets = append(targets, emission.target)

		csFiles := emission.emittedByExtension(".cs")
		csprojFiles := emission.emittedByExtension(".csproj")
		readmeFiles := emission.emittedByExtension(".md")

		hashes := make(map[string]string, len(csFiles))

		// Keyed by the LOGICAL path, so a package already in layout L3 classifies under its package
		// directory rather than under `<pkg>/<goos>` — the census of an L3 corpus reproduces the
		// census of the flat corpus it was built from.
		for _, name := range csFiles {
			hashes[emission.artifacts[name].logicalPath(name)] = emission.artifacts[name].hash
		}

		csEmissions = append(csEmissions, hashes)

		emittedTotal := 0

		for _, state := range emission.artifacts {
			if state.emitted {
				emittedTotal++
			}
		}

		entry := targetManifestEntry{
			Target:                      emission.target,
			GOOS:                        emission.goos,
			GOARCH:                      emission.goarch,
			Root:                        emission.root,
			PackagesQueued:              emission.packagesQueued,
			PackagesFailed:              emission.packagesFailed,
			FailedPackages:              emission.failedPackages,
			EmittedCs:                   len(csFiles),
			EmittedCsproj:               len(csprojFiles),
			EmittedReadme:               len(readmeFiles),
			EmittedTotal:                emittedTotal,
			EmittedCsMatchingSeed:       emission.seedMatchingCs,
			EmittedCsLineEndingOnlySeed: emission.seedLineEndingOnlyCs,
			EmittedCsDifferingSeed:      emission.seedDifferingCs,
			SeedManualConversionFiles:   emission.seedMarkedFileCount,
			MarkerGateViolations:        emission.markerGateViolations,
		}

		if entry.MarkerGateViolations == nil {
			entry.MarkerGateViolations = []string{}
		}

		manifest.TargetInfo = append(manifest.TargetInfo, entry)
		manifest.Csproj.RewrittenPerTarget[emission.target] = len(csprojFiles)
	}

	manifest.Targets = targets

	classes := classifyPlatformEmissions(targets, csEmissions)

	summary := censusSummary{
		TargetCount:        len(targets),
		UnionCs:            len(classes),
		VariantByKind:      map[string]int{},
		SinglePlatformTree: manifest.TargetInfo[0].EmittedCs,
	}

	packagesEmitted := map[string]bool{}
	packagesWithDelta := map[string]bool{}
	perGoosFiles := 0

	for name, class := range classes {
		pkg := artifactPackage(name)
		packagesEmitted[pkg] = true

		switch class.class {
		case artifactIdentical:
			summary.IdenticalOnAllTargets++
		case artifactVariant:
			summary.VariantSameName++
			summary.VariantByKind[variantKind(name)]++
			manifest.VariantFiles = append(manifest.VariantFiles, name)
		case artifactPartial:
			summary.PartialEmitted++
			manifest.PartialFiles = append(manifest.PartialFiles, name)
		case artifactExclusive:
			summary.ExclusiveEmitted++
			manifest.ExclusiveFiles = append(manifest.ExclusiveFiles, name)
		}

		if class.class != artifactIdentical {
			packagesWithDelta[pkg] = true
			perGoosFiles += len(class.emitters)
		}
	}

	summary.PackagesEmitted = len(packagesEmitted)
	summary.PackagesWithAnyDelta = len(packagesWithDelta)
	summary.FlatSharedFiles = summary.IdenticalOnAllTargets
	summary.PerGoosFiles = perGoosFiles
	summary.UnionTreeTotal = summary.FlatSharedFiles + perGoosFiles

	if summary.SinglePlatformTree > 0 {
		summary.GrowthPercent = float64(summary.UnionTreeTotal-summary.SinglePlatformTree) /
			float64(summary.SinglePlatformTree) * 100
	}

	manifest.Summary = summary

	sort.Strings(manifest.VariantFiles)
	sort.Strings(manifest.PartialFiles)
	sort.Strings(manifest.ExclusiveFiles)

	for _, name := range manifest.VariantFiles {
		switch variantKind(name) {
		case PackageInfoFileName:
			manifest.VariantPackageInfoFiles = append(manifest.VariantPackageInfoFiles, name)
		case PackageInitFileName:
			manifest.VariantPackageInitFiles = append(manifest.VariantPackageInitFiles, name)
		default:
			manifest.VariantSourceFiles = append(manifest.VariantSourceFiles, name)
		}
	}

	manifest.PackagesWithDelta = sortedKeys(packagesWithDelta)

	for i := 0; i < len(targets); i++ {
		for j := i + 1; j < len(targets); j++ {
			manifest.Pairwise = append(manifest.Pairwise,
				comparePlatformPair(targets[i], targets[j], csEmissions[i], csEmissions[j]))
		}
	}

	buildCsprojSummary(manifest, emissions)

	return manifest
}

// buildCsprojSummary compares the project files ON DISK after each run rather than only the ones a
// run rewrote: the converter skips an unchanged .csproj, so a target that rewrote nothing still has
// the correct project file — the seeded one. The rewrite COUNT is reported separately because it is
// itself a measurement (the Windows control rewrites zero).
func buildCsprojSummary(manifest *platformManifest, emissions []*platformEmission) {
	names := map[string]bool{}

	for _, emission := range emissions {
		for name := range emission.artifacts {
			if strings.HasSuffix(name, ".csproj") {
				names[name] = true
			}
		}
	}

	differing := map[string]bool{}

	for name := range names {
		var hashes []string

		for _, emission := range emissions {
			if state, ok := emission.artifacts[name]; ok {
				hashes = append(hashes, state.hash)
			} else {
				hashes = append(hashes, "")
			}
		}

		for _, hash := range hashes[1:] {
			if hash != hashes[0] {
				differing[artifactPackage(name)] = true
				break
			}
		}
	}

	for i, emission := range emissions {
		if i == 0 {
			manifest.Csproj.DifferingPerTarget[emission.target] = 0
			continue
		}

		count := 0

		for _, name := range emission.emittedByExtension(".csproj") {
			if first, ok := emissions[0].artifacts[name]; !ok || first.hash != emission.artifacts[name].hash {
				count++
			}
		}

		manifest.Csproj.DifferingPerTarget[emission.target] = count
	}

	manifest.Csproj.Packages = sortedKeys(differing)
	manifest.Csproj.PackagesWithDifferingCsproj = len(manifest.Csproj.Packages)
}

// sortedKeys returns a map's keys in sorted order, so every list the manifest carries is stable.
func sortedKeys(set map[string]bool) []string {
	keys := make([]string, 0, len(set))

	for key := range set {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	return keys
}

// writePlatformManifest renders the manifest as indented JSON. No timestamp is recorded on purpose:
// two censuses of the same corpus and target set must produce byte-identical manifests, so a
// re-run is itself a determinism check.
func writePlatformManifest(manifestPath string, manifest *platformManifest) error {
	contents, err := json.MarshalIndent(manifest, "", "  ")

	if err != nil {
		return fmt.Errorf("failed to render platform manifest: %w", err)
	}

	if err := os.WriteFile(manifestPath, append(contents, '\n'), 0644); err != nil {
		return fmt.Errorf("failed to write platform manifest %q: %w", manifestPath, err)
	}

	return nil
}

// printPlatformCensusSummary prints the manifest's headline numbers in the shape of the design's own
// tables, so a census run can be read against the document without opening the JSON.
func printPlatformCensusSummary(manifest *platformManifest, manifestPath string) {
	summary := manifest.Summary

	fmt.Printf("\n=== Multi-platform emission census (%d targets) ===\n\n", summary.TargetCount)

	fmt.Printf("%-20s %8s %7s %9s %10s %11s %8s %9s %6s\n",
		"Target", "queued", "failed", "emit .cs", "emit proj", "hand-owned", "gate", "=seed/crlf", "≠seed")

	for _, entry := range manifest.TargetInfo {
		fmt.Printf("%-20s %8d %7d %9d %10d %11d %8d %4d/%-4d %6d\n",
			entry.Target, entry.PackagesQueued, entry.PackagesFailed, entry.EmittedCs,
			entry.EmittedCsproj, entry.SeedManualConversionFiles, len(entry.MarkerGateViolations),
			entry.EmittedCsMatchingSeed, entry.EmittedCsLineEndingOnlySeed, entry.EmittedCsDifferingSeed)
	}

	fmt.Printf("\nUnion of emitted .cs names          %6d\n", summary.UnionCs)
	fmt.Printf("Byte-identical on all targets       %6d\n", summary.IdenticalOnAllTargets)
	fmt.Printf("Same name, different content        %6d\n", summary.VariantSameName)
	fmt.Printf("Emitted by some but not all         %6d\n", summary.PartialEmitted)
	fmt.Printf("Platform-exclusive                  %6d\n", summary.ExclusiveEmitted)
	fmt.Printf("Packages with any delta             %6d  (of %d emitting)\n",
		summary.PackagesWithAnyDelta, summary.PackagesEmitted)

	fmt.Printf("\nVariant artifacts by kind:\n")

	for _, kind := range sortedKeys(func() map[string]bool {
		kinds := map[string]bool{}
		for kind := range summary.VariantByKind {
			kinds[kind] = true
		}
		return kinds
	}()) {
		fmt.Printf("  %-24s %6d\n", kind, summary.VariantByKind[kind])
	}

	fmt.Printf("\nL3 pricing: flat %d + per-GOOS %d = %d (single-platform tree %d, +%.1f%%)\n",
		summary.FlatSharedFiles, summary.PerGoosFiles, summary.UnionTreeTotal,
		summary.SinglePlatformTree, summary.GrowthPercent)

	fmt.Printf("\n%-22s %-22s %10s %10s %8s %8s %8s %10s\n",
		"A", "B", "both", "identical", "differ", "A-only", "B-only", "packages")

	for _, row := range manifest.Pairwise {
		fmt.Printf("%-22s %-22s %10d %10d %8d %8d %8d %10d\n",
			row.A, row.B, row.EmittedByBoth, row.Identical, row.Differ, row.AOnly, row.BOnly, row.PackagesTouched)
	}

	fmt.Printf("\nProject files rewritten per target: %s\n", renderCountMap(manifest.Csproj.RewrittenPerTarget, manifest.Targets))
	fmt.Printf("  of those, differing from %s: %s\n", manifest.Targets[0], renderCountMap(manifest.Csproj.DifferingPerTarget, manifest.Targets))
	fmt.Printf("Packages whose .csproj differs across targets: %d\n", manifest.Csproj.PackagesWithDifferingCsproj)

	fmt.Printf("\nManifest written: %s\n", manifestPath)
}

// renderCountMap renders a per-target count map in target order.
func renderCountMap(counts map[string]int, targets []string) string {
	parts := make([]string, 0, len(targets))

	for _, target := range targets {
		parts = append(parts, fmt.Sprintf("%s=%d", target, counts[target]))
	}

	return strings.Join(parts, ", ")
}
