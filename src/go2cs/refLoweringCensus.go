// refLoweringCensus.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the ж-box arc's CORPUS-WIDE A1 CENSUS instrument — the `-ref-census` flag
// (docs/phase4/DESIGN-zh-box-reduction.md §9 stage A1). It is the corpus-scale companion of the
// per-package `-debug` census refLoweringAnalysisOperations.go prints during any conversion.
//
// What it does, and what it deliberately does NOT do (the -platform-census discipline):
//
//	DOES     load the standard library ONCE PER TARGET with full syntax (the same `std` pattern,
//	         GO111MODULE=off, GOOS/GOARCH environment and purego-default build tags the -stdlib
//	         scanner itself uses), run the ref-lowering classification over every package, resolve
//	         the Phase-A and A′-world fixed points globally, census the §3.3 argument shapes and
//	         the address-taken-local reversions, cross-reference candidates against the hand-owned
//	         file set (read-only), diff classifications across GOOS targets, and write ONE JSON
//	         report plus a stdout summary.
//	DOES NOT write a single corpus byte. There is no emission anywhere on this path — no .cs, no
//	         .csproj, no README, no staging root. -go2cspath is read only to locate src/core for
//	         the hand-own cross-reference scan.
//
// Analysis-only is deliberate and design-sanctioned: §9's A1 row is "classification pass
// (analysis only) + a -debug census", and a census that never emits cannot violate the seeded
// -reconvert ritual because it never converts. The per-GOOS runs (§9 item d) are plain re-loads
// under a different GOOS — no per-target staging roots to seed or wipe.
//
// Toolchain caveat, carried in the report itself: the census reads GOROOT's sources through
// go/types, so its numbers describe the TOOLCHAIN the run resolved (recorded in goVersion), not
// the corpus pin. A census taken on a machine whose toolchain differs from the corpus's pinned Go
// release is developmental; re-derive on the pinned machine with the same one command.
//
// Invocation (the one-command re-derivation):
//
//	go2cs -stdlib -ref-census <out.json> -platforms windows/amd64,linux/amd64,darwin/amd64 -go2cspath <repo>\src
//
// Optional positionals narrow the package set exactly as `-stdlib <pkgs>` does (iteration only —
// a narrowed run under-counts cross-package traffic and skips nothing else).

package main

import (
	"encoding/json"
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"time"

	"golang.org/x/tools/go/packages"
)

const (
	// refCensusSchema versions the report shape (the platform-manifest convention).
	refCensusSchema = "go2cs.ref-lowering-census/1"
)

// refCensusReport is the JSON artifact — the whole census, all targets, one file.
type refCensusReport struct {
	Schema      string   `json:"schema"`
	GoVersion   string   `json:"goVersion"`
	GeneratedAt string   `json:"generatedAt"`
	Targets     []string `json:"targets"`
	// Packages filter applied, when the run was narrowed (developmental runs only).
	PackageFilter []string `json:"packageFilter,omitempty"`

	PerTarget map[string]*refCensusTarget `json:"perTarget"`

	HandOwn *refCensusHandOwn `json:"handOwn,omitempty"`

	// GoosDeltas lists every parameter position whose Phase-A verdict differs between targets on
	// which the function exists — the layout-L3 propagation census (§9 item d).
	GoosDeltas *refCensusGoosDeltas `json:"goosDeltas,omitempty"`
}

// refCensusTarget is one GOOS/GOARCH target's full classification.
type refCensusTarget struct {
	Target   string                       `json:"target"`
	Packages map[string]*refCensusPackage `json:"packages"`
	Totals   refCensusTotals              `json:"totals"`
	// ExportedCandidates is §9 item f: exported package-level functions with ≥1 parameter lowered
	// in the A′-world fixed point, with their corpus call-site traffic.
	ExportedCandidates []refCensusExportedCandidate `json:"exportedCandidates,omitempty"`
}

// refCensusPackage carries one package's summary plus the full per-function verdicts (the report
// reader drills into fiat/nistec/edwards25519 from here).
type refCensusPackage struct {
	Summary refLoweringSummary         `json:"summary"`
	Funcs   map[string]*refFuncVerdict `json:"funcs,omitempty"`
	Locals  []refLocalVerdict          `json:"locals,omitempty"`
	// Row5Sites lists the pointer-conversion argument sites (file:line) — the §10.3 hoisted-temp
	// rule's exact constituency, priced not discovered.
	Row5Sites []refCallArg `json:"row5Sites,omitempty"`
	// OtherVetoSites lists every argument shape the §3.3 rows could NOT classify — must be empty
	// or each entry re-opens the emission-shape table.
	OtherVetoSites []refCallArg `json:"otherVetoSites,omitempty"`
	// Methods carries the B′ §4.1 verdicts with their R3 arms (the 2026-09-02 amendment) — the S0
	// report reads the two target packages' per-method classification from here.
	Methods map[string]*refMethodVerdict `json:"methods,omitempty"`
}

// refCensusTotals is the corpus-wide roll-up per target.
type refCensusTotals struct {
	Packages               int            `json:"packages"`
	Funcs                  int            `json:"funcs"`
	CandidateFuncs         int            `json:"candidateFuncs"`
	PtrParams              int            `json:"ptrParams"`
	LoweredParamsA         int            `json:"loweredParamsA"`
	LoweredParamsAPrime    int            `json:"loweredParamsAPrime"`
	LocalsAddressTaken     int            `json:"localsAddressTaken"`
	LocalsRevert           int            `json:"localsRevert"`
	ShapeCounts            map[string]int `json:"shapeCounts"`
	VetoCounts             map[string]int `json:"vetoCounts"`
	KeptLocalReasons       map[string]int `json:"keptLocalReasons"`
	MethodPtrParams        int            `json:"methodPtrParams"`
	ExportedCandidateFuncs int            `json:"exportedCandidateFuncs"`
	// ExportedReturnShaped counts exported functions with ≥1 pointer parameter vetoed X2-return —
	// the "constructor-shaped" bucket of the design's 2026-08-10 measurement (§9 item f).
	ExportedReturnShaped int `json:"exportedReturnShaped"`
	// B′ §4.1 receiver eligibility (S0a): analysis only, nothing emission-side reads it.
	MethodsSeen          int            `json:"methodsSeen"`
	MethodsEligible      int            `json:"methodsEligible"`
	MethodRecvVetoCounts map[string]int `json:"methodRecvVetoCounts"`
}

// refCensusExportedCandidate is one §9(f) exported candidate with its measured call-site traffic.
type refCensusExportedCandidate struct {
	PkgPath   string `json:"pkg"`
	Func      string `json:"func"`
	Params    int    `json:"loweredParams"`
	CallSites int    `json:"callSites"` // corpus-wide argument records into its lowered positions
}

// refCensusHandOwn is the §3.5 hand-own cross-reference: the re-measured marker census plus every
// candidate referenced from a hand-owned file of its own package.
type refCensusHandOwn struct {
	CorpusRoot      string   `json:"corpusRoot"`
	MarkedFiles     []string `json:"markedFiles"`
	ImplFiles       []string `json:"implFiles"`
	MarkedFileCount int      `json:"markedFileCount"`
	ImplFileCount   int      `json:"implFileCount"`
	// References: lowered candidates whose name appears (word-anchored) in a hand-owned file of
	// the SAME package — the set A2 must resolve per instance before any regen. Cross-package
	// references cannot bind a lowered candidate (unexported ⇒ C# internal, no InternalsVisibleTo
	// between corpus assemblies; the linkname escape hatch is already X5), so same-package is the
	// complete exposure set.
	References []refCensusHandOwnRef `json:"references,omitempty"`
}

// refCensusHandOwnRef is one candidate-to-hand-own textual match, resolved per instance in the
// census report.
type refCensusHandOwnRef struct {
	PkgPath string `json:"pkg"`
	Func    string `json:"func"`
	File    string `json:"file"`
	Count   int    `json:"count"`
}

// refCensusGoosDeltas is §9 item d — the per-GOOS classification delta.
type refCensusGoosDeltas struct {
	// Positions whose Phase-A lowered verdict differs across the targets that DECLARE the
	// function. A function present on a single target is platform-exclusive already and creates
	// no new variance.
	Positions []refCensusGoosDelta `json:"positions,omitempty"`
	// Packages summarizes the delta at package granularity, with the package's CURRENT layout-L3
	// membership (read from the corpus, read-only) so the merge churn is priced.
	Packages []refCensusGoosDeltaPackage `json:"packages,omitempty"`
}

type refCensusGoosDelta struct {
	Position refPosKey         `json:"position"`
	Verdicts map[string]string `json:"verdicts"` // target → "lowered" | "vetoed" | "absent"
}

type refCensusGoosDeltaPackage struct {
	PkgPath   string `json:"pkg"`
	Positions int    `json:"positions"`
	AlreadyL3 bool   `json:"alreadyL3"`
}

// runRefLoweringCensus is the -ref-census entry point (main.go dispatches here under -stdlib).
func runRefLoweringCensus(options Options, outputPath string, packageFilter []string) error {
	report := &refCensusReport{
		Schema:        refCensusSchema,
		GoVersion:     censusGoVersion(),
		GeneratedAt:   time.Now().UTC().Format(time.RFC3339),
		Targets:       options.targetPlatforms,
		PackageFilter: packageFilter,
		PerTarget:     map[string]*refCensusTarget{},
	}

	fmt.Printf("ж-box A1 ref-lowering census: %d target(s), toolchain %s\n", len(options.targetPlatforms), report.GoVersion)

	for _, target := range options.targetPlatforms {
		fmt.Printf("\n=== Target %s ===\n", target)

		targetCensus, err := censusOneTarget(options, target, packageFilter)

		if err != nil {
			return fmt.Errorf("census of target %s failed: %w", target, err)
		}

		report.PerTarget[platformTargetTag(target)] = targetCensus
	}

	// The hand-own cross-reference runs once, against the primary target's lowered set (the
	// hand-owned FILE set is a property of the corpus, not of a target; per-GOOS routing of the
	// files themselves is listed as-is).
	primary := report.PerTarget[platformTargetTag(options.targetPlatforms[0])]

	if handOwn, err := crossReferenceHandOwns(options.go2csPath, primary); err != nil {
		showWarning("hand-own cross-reference incomplete: %v", err)
	} else {
		report.HandOwn = handOwn
	}

	if len(options.targetPlatforms) > 1 {
		report.GoosDeltas = diffGoosClassifications(options.go2csPath, report)
	}

	if err := writeRefCensusReport(report, outputPath); err != nil {
		return err
	}

	printRefCensusSummary(report)
	fmt.Printf("\nCensus written: %s\n", outputPath)

	return nil
}

// censusGoVersion resolves the toolchain version the loader will actually run — the report's
// prominent caveat when it differs from the corpus pin.
func censusGoVersion() string {
	if version, err := getGoEnv("GOVERSION"); err == nil && version != "" {
		return version
	}

	return "unknown"
}

// censusOneTarget loads the standard library once (full syntax) for one GOOS/GOARCH and runs the
// classification + census over every convert-set package.
func censusOneTarget(options Options, target string, packageFilter []string) (*refCensusTarget, error) {
	targetParts := strings.Split(target, "/")

	if len(targetParts) != 2 {
		return nil, fmt.Errorf("invalid target %q: expected os/arch", target)
	}

	srcPath := filepath.Join(options.goRoot, "src")

	if _, err := os.Stat(srcPath); os.IsNotExist(err) {
		return nil, fmt.Errorf("standard library source not found at %s", srcPath)
	}

	// The exact loader shape scanStdLib uses (GO111MODULE=off, `std`), widened to full syntax:
	// one load per target instead of one per package — the census's whole cost is this call.
	loadConfig := &packages.Config{
		Mode:       packages.LoadAllSyntax,
		Dir:        srcPath,
		BuildFlags: options.loaderBuildFlags(),
		Env: append(os.Environ(), "GO111MODULE=off",
			fmt.Sprintf("GOOS=%s", targetParts[0]), fmt.Sprintf("GOARCH=%s", targetParts[1])),
	}

	fmt.Println("Loading standard library packages with full syntax (this may take a while)...")
	loadStart := time.Now()

	pkgs, err := packages.Load(loadConfig, "std")

	if err != nil {
		return nil, fmt.Errorf("failed to load standard library packages: %w", err)
	}

	fmt.Printf("Loaded %d packages in %s\n", len(pkgs), time.Since(loadStart).Round(time.Second))

	filterSet := map[string]bool{}

	for _, pkg := range packageFilter {
		filterSet[pkg] = true
	}

	targetCensus := &refCensusTarget{
		Target:   target,
		Packages: map[string]*refCensusPackage{},
		Totals: refCensusTotals{
			ShapeCounts:          map[string]int{},
			VetoCounts:           map[string]int{},
			KeptLocalReasons:     map[string]int{},
			MethodRecvVetoCounts: map[string]int{},
		},
	}

	// Per-package classification (package-local Phase-A fixed point + locals census inside).
	results := map[string]*refLoweringPackageResult{}

	for _, pkg := range pkgs {
		if strings.HasSuffix(pkg.PkgPath, "_test") || isNonConvertedStdLibPackage(pkg.PkgPath) {
			continue
		}

		if len(filterSet) > 0 && !filterSet[pkg.PkgPath] {
			continue
		}

		if len(pkg.Errors) > 0 {
			showWarning("package %s did not load cleanly; census skips it: %v", pkg.PkgPath, pkg.Errors[0])
			continue
		}

		if pkg.Types == nil || pkg.TypesInfo == nil {
			continue
		}

		// Belt-and-suspenders _test.go filter (a `std` load carries none) + the package's own
		// linkname handle scan (the census must not touch the conversion drivers' package state).
		// The X5-hand-owned arm probes the corpus (read-only) for each Go file's emission target
		// carrying the manual-conversion marker — the same per-file probe the conversion driver
		// runs, routed through layout L3 exactly as the driver routes it.
		syntax := make([]*ast.File, 0, len(pkg.Syntax))
		manualFiles := map[*ast.File]bool{}
		pkgCorpusDir := filepath.Join(options.go2csPath, "core", filepath.FromSlash(pkg.PkgPath))

		for _, file := range pkg.Syntax {
			filename := pkg.Fset.Position(file.Pos()).Filename

			if strings.HasSuffix(strings.ToLower(filename), "_test.go") {
				continue
			}

			syntax = append(syntax, file)

			outputFile := platformLayoutPath(pkgCorpusDir, targetParts[0],
				strings.TrimSuffix(filepath.Base(filename), ".go")+".cs")

			if manual, probeErr := containsManualConversionMarker(outputFile); probeErr == nil && manual {
				manualFiles[file] = true
			}
		}

		// B′ §4.2: a receiver-position use's kept-reason turns on what the CALLEE's receiver is
		// emitted as (direct-ж vs `[GoRecv] this ref T`), so the census has to know the same
		// direct-ж set the conversion driver computes — otherwise the instrument and the emitter
		// disagree about which locals keep a box, which is precisely the disagreement this stage
		// exists to remove. Mirror the driver's ordering (collectCaptureModeMethods →
		// performRefLoweringAnalysis, conversionDriver.go) against FRESH per-package maps, so the
		// census still leaves the drivers' package state exactly as it found it.
		censusCaptureModeMethods := packageCaptureModeMethods
		censusDirectBoxMethods := packageDirectBoxReceiverMethods
		censusRefReturnPrimaries := packageRefReturnPrimaryMethods

		packageCaptureModeMethods = make(map[*types.Func]bool)
		packageDirectBoxReceiverMethods = make(map[*types.Func]bool)
		packageRefReturnPrimaryMethods = make(map[*types.Func]bool)

		collectCaptureModeMethods(pkg)

		handles := censusLinknameHandles(syntax)
		results[pkg.PkgPath] = analyzeRefLowering(pkg.Fset, syntax, pkg.Types, pkg.TypesInfo, handles, manualFiles)

		packageCaptureModeMethods = censusCaptureModeMethods
		packageDirectBoxReceiverMethods = censusDirectBoxMethods
		packageRefReturnPrimaryMethods = censusRefReturnPrimaries
	}

	// A′-world resolution: the union of every package's records, exported candidates included.
	// (The Phase-A verdicts are already final — an unexported function's call sites are all
	// package-local, so the per-package fixed point IS the global one for scope A.)
	unionFuncs := map[string]*refFuncVerdict{}
	var unionCallArgs []refCallArg

	for pkgPath, result := range results {
		for name, verdict := range result.Funcs {
			unionFuncs[pkgPath+"|"+name] = verdict
		}

		unionCallArgs = append(unionCallArgs, result.CallArgs...)
	}

	loweredAPrime, strippedAPrime := resolveRefLoweringFixedPoint(unionFuncs, unionCallArgs, true)
	applyRefLoweringVerdicts(unionFuncs, loweredAPrime, strippedAPrime, true)

	// Call-site traffic per A′ exported candidate (the §10.1 decision input).
	callTraffic := map[refPosKey]int{}

	for _, callArg := range unionCallArgs {
		if loweredAPrime[callArg.Target] {
			callTraffic[callArg.Target]++
		}
	}

	// Aggregate.
	for pkgPath, result := range results {
		summary := summarizeRefLowering(result)
		censusPackage := &refCensusPackage{
			Summary: summary,
			Funcs:   result.Funcs,
			Locals:  result.Locals,
			Methods: result.Methods,
		}

		loweredPositions := map[refPosKey]bool{}

		for _, verdict := range result.Funcs {
			for _, param := range verdict.Params {
				if param.LoweredA {
					loweredPositions[refPosKey{PkgPath: verdict.PkgPath, Func: verdict.Name, Index: param.Index}] = true
				}
			}
		}

		for _, callArg := range result.CallArgs {
			switch callArg.Shape {
			case refShapePtrConv:
				censusPackage.Row5Sites = append(censusPackage.Row5Sites, callArg)
			case refShapeOtherVeto:
				censusPackage.OtherVetoSites = append(censusPackage.OtherVetoSites, callArg)
			}
		}

		targetCensus.Packages[pkgPath] = censusPackage

		totals := &targetCensus.Totals
		totals.Packages++
		totals.Funcs += summary.Funcs
		totals.CandidateFuncs += summary.CandidateFuncs
		totals.PtrParams += summary.PtrParams
		totals.LoweredParamsA += summary.LoweredParamsA
		totals.LocalsAddressTaken += summary.LocalsAddressTaken
		totals.LocalsRevert += summary.LocalsRevert
		totals.MethodPtrParams += result.MethodPtrParams
		totals.MethodsSeen += summary.MethodsSeen
		totals.MethodsEligible += summary.MethodsEligible

		for veto, count := range summary.MethodRecvVetoCounts {
			totals.MethodRecvVetoCounts[veto] += count
		}

		for shape, count := range summary.ShapeCounts {
			totals.ShapeCounts[shape] += count
		}

		for veto, count := range summary.VetoCounts {
			totals.VetoCounts[veto] += count
		}

		for reason, count := range summary.KeptLocalReasons {
			totals.KeptLocalReasons[reason] += count
		}
	}

	// Exported candidates + the A′ totals.
	for _, verdict := range unionFuncs {
		returnShaped := false

		for _, param := range verdict.Params {
			if param.LoweredAPrime {
				targetCensus.Totals.LoweredParamsAPrime++
			}

			for _, veto := range param.Vetoes {
				if veto == refVetoX2Return {
					returnShaped = true
				}
			}
		}

		if verdict.Exported && returnShaped && len(verdict.Params) > 0 {
			targetCensus.Totals.ExportedReturnShaped++
		}

		if !verdict.ExportedCandidate {
			continue
		}

		targetCensus.Totals.ExportedCandidateFuncs++
		candidate := refCensusExportedCandidate{PkgPath: verdict.PkgPath, Func: verdict.Name}

		for _, param := range verdict.Params {
			if param.LoweredAPrime {
				candidate.Params++
				candidate.CallSites += callTraffic[refPosKey{PkgPath: verdict.PkgPath, Func: verdict.Name, Index: param.Index}]
			}
		}

		targetCensus.ExportedCandidates = append(targetCensus.ExportedCandidates, candidate)
	}

	sort.Slice(targetCensus.ExportedCandidates, func(i, j int) bool {
		a, b := targetCensus.ExportedCandidates[i], targetCensus.ExportedCandidates[j]

		if a.PkgPath != b.PkgPath {
			return a.PkgPath < b.PkgPath
		}

		return a.Func < b.Func
	})

	return targetCensus, nil
}

// censusLinknameHandles is collectLinknameHandles without the package-global side effect: the
// census scans its own per-package handle set so a census run can never perturb conversion state.
func censusLinknameHandles(files []*ast.File) HashSet[string] {
	handles := NewHashSet([]string{})

	for _, file := range files {
		for _, group := range file.Comments {
			for _, comment := range group.List {
				fields := strings.Fields(comment.Text)

				if len(fields) == 2 && fields[0] == "//go:linkname" {
					handles.Add(fields[1])
				}
			}
		}
	}

	return handles
}

// handOwnMarkerPattern is the LINE-ANCHORED [module: GoManualConversion] scan CLAUDE.md's corpus
// mechanics mandate — an unanchored grep counts the bodyless-partial placeholder comments that
// merely MENTION the marker and inflates the census (~63 vs the real count).
var handOwnMarkerPattern = regexp.MustCompile(`(?m)^\s*\[module:\s*(go\.)?GoManualConversion\]`)

// crossReferenceHandOwns re-measures the hand-owned file census (never asserted from memory — the
// count moves) and cross-references every Phase-A lowered candidate against the hand-owned files
// of its own package (§3.5's audit obligation).
func crossReferenceHandOwns(go2csPath string, target *refCensusTarget) (*refCensusHandOwn, error) {
	coreRoot := filepath.Join(go2csPath, "core")

	if _, err := os.Stat(coreRoot); err != nil {
		return nil, fmt.Errorf("corpus root %s not readable (pass -go2cspath <repo>\\src): %w", coreRoot, err)
	}

	handOwn := &refCensusHandOwn{CorpusRoot: coreRoot}

	// One walk collects both censuses: line-anchored marked files and *_impl.cs companions.
	handOwnByDir := map[string][]string{}

	walkErr := filepath.WalkDir(coreRoot, func(path string, entry os.DirEntry, err error) error {
		if err != nil {
			return err
		}

		if entry.IsDir() {
			name := entry.Name()

			// bin/obj noise never holds hand-owns; golib is hand-written runtime, not a converted
			// package (its members are not candidate callees).
			if name == "bin" || name == "obj" || name == "Generated" {
				return filepath.SkipDir
			}

			return nil
		}

		lower := strings.ToLower(entry.Name())

		if !strings.HasSuffix(lower, ".cs") || strings.HasSuffix(lower, ".cs.auto") {
			return nil
		}

		isImpl := strings.HasSuffix(lower, "_impl.cs")
		content, readErr := os.ReadFile(path)

		if readErr != nil {
			return readErr
		}

		isMarked := handOwnMarkerPattern.Match(content)

		if !isMarked && !isImpl {
			return nil
		}

		rel := relPathForCensus(coreRoot, path)

		if isMarked {
			handOwn.MarkedFiles = append(handOwn.MarkedFiles, rel)
		}

		if isImpl {
			handOwn.ImplFiles = append(handOwn.ImplFiles, rel)
		}

		handOwnByDir[filepath.Dir(path)] = append(handOwnByDir[filepath.Dir(path)], path)

		return nil
	})

	if walkErr != nil {
		return nil, walkErr
	}

	sort.Strings(handOwn.MarkedFiles)
	sort.Strings(handOwn.ImplFiles)
	handOwn.MarkedFileCount = len(handOwn.MarkedFiles)
	handOwn.ImplFileCount = len(handOwn.ImplFiles)

	// Cross-reference: candidates against the hand-owned files of their OWN package directory —
	// flat and per-GOOS subfolders both (layout L3 routes hand-owns into <pkg>/<goos>/).
	for pkgPath, censusPackage := range target.Packages {
		pkgDir := filepath.Join(coreRoot, filepath.FromSlash(pkgPath))
		var pkgHandOwns []string

		for dir, files := range handOwnByDir {
			if dir == pkgDir || filepath.Dir(dir) == pkgDir {
				pkgHandOwns = append(pkgHandOwns, files...)
			}
		}

		if len(pkgHandOwns) == 0 {
			continue
		}

		for name, verdict := range censusPackage.Funcs {
			lowered := false

			for _, param := range verdict.Params {
				if param.LoweredA {
					lowered = true
					break
				}
			}

			if !lowered {
				continue
			}

			namePattern, err := regexp.Compile(`\b` + regexp.QuoteMeta(name) + `\b`)

			if err != nil {
				continue
			}

			for _, handOwnPath := range pkgHandOwns {
				content, readErr := os.ReadFile(handOwnPath)

				if readErr != nil {
					continue
				}

				if matches := namePattern.FindAll(content, -1); len(matches) > 0 {
					handOwn.References = append(handOwn.References, refCensusHandOwnRef{
						PkgPath: pkgPath,
						Func:    name,
						File:    relPathForCensus(coreRoot, handOwnPath),
						Count:   len(matches),
					})
				}
			}
		}
	}

	sort.Slice(handOwn.References, func(i, j int) bool {
		a, b := handOwn.References[i], handOwn.References[j]

		if a.PkgPath != b.PkgPath {
			return a.PkgPath < b.PkgPath
		}

		if a.Func != b.Func {
			return a.Func < b.Func
		}

		return a.File < b.File
	})

	return handOwn, nil
}

// diffGoosClassifications computes §9 item d: parameter positions whose Phase-A verdict differs
// between targets that declare the function, rolled up to packages with their current layout-L3
// membership (a read-only corpus probe).
func diffGoosClassifications(go2csPath string, report *refCensusReport) *refCensusGoosDeltas {
	deltas := &refCensusGoosDeltas{}

	// position → target → verdict
	verdicts := map[refPosKey]map[string]string{}

	for _, target := range report.Targets {
		targetCensus := report.PerTarget[platformTargetTag(target)]

		if targetCensus == nil {
			continue
		}

		for _, censusPackage := range targetCensus.Packages {
			for _, verdict := range censusPackage.Funcs {
				for _, param := range verdict.Params {
					key := refPosKey{PkgPath: verdict.PkgPath, Func: verdict.Name, Index: param.Index}

					if verdicts[key] == nil {
						verdicts[key] = map[string]string{}
					}

					state := "vetoed"

					if param.LoweredA {
						state = "lowered"
					}

					verdicts[key][target] = state
				}
			}
		}
	}

	packagePositions := map[string]int{}

	keys := make([]refPosKey, 0, len(verdicts))

	for key := range verdicts {
		keys = append(keys, key)
	}

	sort.Slice(keys, func(i, j int) bool { return keys[i].String() < keys[j].String() })

	for _, key := range keys {
		perTarget := verdicts[key]

		// Positions on fewer than two targets are platform-exclusive, not variance.
		if len(perTarget) < 2 {
			continue
		}

		first := ""
		varies := false

		for _, target := range report.Targets {
			state, present := perTarget[target]

			if !present {
				continue
			}

			if first == "" {
				first = state
			} else if state != first {
				varies = true
			}
		}

		if !varies {
			continue
		}

		full := map[string]string{}

		for _, target := range report.Targets {
			if state, present := perTarget[target]; present {
				full[target] = state
			} else {
				full[target] = "absent"
			}
		}

		deltas.Positions = append(deltas.Positions, refCensusGoosDelta{Position: key, Verdicts: full})
		packagePositions[key.PkgPath]++
	}

	packages := make([]string, 0, len(packagePositions))

	for pkgPath := range packagePositions {
		packages = append(packages, pkgPath)
	}

	sort.Strings(packages)

	for _, pkgPath := range packages {
		deltas.Packages = append(deltas.Packages, refCensusGoosDeltaPackage{
			PkgPath:   pkgPath,
			Positions: packagePositions[pkgPath],
			AlreadyL3: packageIsLayoutL3(go2csPath, pkgPath),
		})
	}

	return deltas
}

// packageIsLayoutL3 probes (read-only) whether the corpus already holds per-GOOS subfolders for a
// package — the census prices NEW L3 membership against existing membership.
func packageIsLayoutL3(go2csPath, pkgPath string) bool {
	pkgDir := filepath.Join(go2csPath, "core", filepath.FromSlash(pkgPath))

	for _, goos := range []string{"windows", "linux", "darwin"} {
		if info, err := os.Stat(filepath.Join(pkgDir, goos)); err == nil && info.IsDir() {
			return true
		}
	}

	return false
}

// writeRefCensusReport writes the JSON artifact (pretty-printed — the report is read by people
// before it is read by tools).
func writeRefCensusReport(report *refCensusReport, outputPath string) error {
	if dir := filepath.Dir(outputPath); dir != "" && dir != "." {
		if err := os.MkdirAll(dir, 0755); err != nil {
			return fmt.Errorf("failed to create census output directory %s: %w", dir, err)
		}
	}

	data, err := json.MarshalIndent(report, "", "  ")

	if err != nil {
		return fmt.Errorf("failed to marshal census report: %w", err)
	}

	if err := os.WriteFile(outputPath, data, 0644); err != nil {
		return fmt.Errorf("failed to write census report %s: %w", outputPath, err)
	}

	return nil
}

// printRefCensusSummary prints the human summary — the numbers the census report doc quotes.
func printRefCensusSummary(report *refCensusReport) {
	fmt.Printf("\n=== ж-box A1 census summary (toolchain %s) ===\n", report.GoVersion)

	for _, target := range report.Targets {
		targetCensus := report.PerTarget[platformTargetTag(target)]

		if targetCensus == nil {
			continue
		}

		totals := targetCensus.Totals
		fmt.Printf("\n[%s] packages %d, package-level funcs %d (candidates %d)\n", target, totals.Packages, totals.Funcs, totals.CandidateFuncs)
		fmt.Printf("  pointer params %d: lowered(A) %d, lowered(A') %d; method ptr-params (B' context) %d\n",
			totals.PtrParams, totals.LoweredParamsA, totals.LoweredParamsAPrime, totals.MethodPtrParams)
		fmt.Printf("  address-taken locals %d: revert %d (%.1f%%)\n",
			totals.LocalsAddressTaken, totals.LocalsRevert, percentOf(totals.LocalsRevert, totals.LocalsAddressTaken))
		fmt.Printf("  exported candidates (A' flag) %d; exported return-shaped %d\n",
			totals.ExportedCandidateFuncs, totals.ExportedReturnShaped)
		printCountMap("  call-arg shapes at lowered positions", totals.ShapeCounts)
		printCountMap("  veto reasons", totals.VetoCounts)
		printCountMap("  kept-local reasons", totals.KeptLocalReasons)
		fmt.Printf("  [B'] methods seen %d: receiver-eligible %d (%.1f%%)\n",
			totals.MethodsSeen, totals.MethodsEligible, percentOf(totals.MethodsEligible, totals.MethodsSeen))
		printCountMap("  [B'] receiver vetoes", totals.MethodRecvVetoCounts)
	}

	if report.HandOwn != nil {
		fmt.Printf("\nHand-own census (re-measured): %d marked files, %d *_impl.cs companions; %d candidate references to resolve\n",
			report.HandOwn.MarkedFileCount, report.HandOwn.ImplFileCount, len(report.HandOwn.References))
	}

	if report.GoosDeltas != nil {
		fmt.Printf("Per-GOOS classification deltas: %d positions across %d packages\n",
			len(report.GoosDeltas.Positions), len(report.GoosDeltas.Packages))
	}
}

// printCountMap prints a name→count map sorted by key.
func printCountMap(label string, counts map[string]int) {
	if len(counts) == 0 {
		return
	}

	keys := make([]string, 0, len(counts))

	for key := range counts {
		keys = append(keys, key)
	}

	sort.Strings(keys)
	parts := make([]string, 0, len(keys))

	for _, key := range keys {
		parts = append(parts, fmt.Sprintf("%s=%d", key, counts[key]))
	}

	fmt.Printf("%s: %s\n", label, strings.Join(parts, " "))
}

// percentOf renders a/b as a percentage, zero-safe.
func percentOf(a, b int) float64 {
	if b == 0 {
		return 0
	}

	return float64(a) / float64(b) * 100
}
