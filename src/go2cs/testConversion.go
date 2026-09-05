// testConversion.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"bytes"
	"context"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"go/types"
	"io/fs"
	"os"
	"os/exec"
	"path"
	"path/filepath"
	"regexp"
	"runtime"
	"runtime/debug"
	"sort"
	"strconv"
	"strings"
	"sync"
	"time"
	"unicode"
	"unicode/utf8"

	"golang.org/x/tools/go/packages"
)

// Phase-4 test conversion: converts a package's _test.go variants (in-package and external
// package_test) into a runnable, self-registering C# test project driven by the hand-owned
// go.testing runtime (src/core/testing), plus a machine-readable manifest and a `go test -json`
// differential oracle. Ported from the codex/testing-infrastructure branch (097c94d70) onto the
// shared per-package helpers in packageStateOperations.go and the shared writePackageInfoFile —
// the branch's private copies of that machinery are gone by design (they drifted; see the port
// review in docs/phase4/BranchReview-codex-testing-infrastructure.md).

const (
	testPackageInfoFileName = "package_test_info.cs"
	testHostFileName        = "go2cs_test_host.cs"
	testManifestFileName    = "go2cs_test_manifest.json"

	// The package's HAND-OWNED disclosed-divergence manifest (see testDisclosure). Unlike the
	// go2cs_test_* artifacts above, this file is never generated: it is authored by hand,
	// committed beside the converted package, and reviewed like source.
	testDisclosureFileName = "go2cs_test_disclosures.json"

	// The EXTERNAL test package's metadata anchor (B4/B5) — the compilation unit hosting the
	// GoImplement/GoImplicitConv attributes whose generated adapters/partials must anchor to
	// the <name>_test package class. "external test package" is Go's own term for `package
	// <name>_test`, matching the vocabulary used throughout this file.
	//
	// ⚠ The `_test.cs` SUFFIX IS LOAD-BEARING — it is the exclusion mechanism: the production
	// csproj's committed `*_test.cs` Compile Remove and productionCSFiles both skip this file by
	// that glob alone, WITHOUT a shared-csproj-template edit (which would churn every behavioral
	// csproj on re-transpile). Any future rename must keep the suffix or pay that churn.
	//
	// Renamed from the original `package_info_test.cs` (2026-07-21): a near-anagram of
	// testPackageInfoFileName above, the two sorted adjacent to `package_info.cs` in every
	// converted package directory, and nothing in either name said which class it anchors to.
	externalTestPackageInfoFileName = "package_info_external_test.cs"
	internalTestPackageInfoFileName = "package_info_internal_test.cs"
)

// stdLibImportPathOf reports the standard-library import path a directory denotes, or "" when the
// directory is not under GOROOT/src. The spelling is Go's — forward slashes — because that is what
// isNonConvertedStdLibPackage and every other import-path predicate compare against.
//
// The comparison is path-normalized rather than textual for the reason main.go's checkGoRootSpelling
// documents: a forward-slash GOROOT on Windows is a spelling `go` itself accepts, and a prefix test
// against the backslash form silently answers "not stdlib" — which here would silently answer "not
// hand-owned" and wave the refusal through, the exact failure the guard exists to stop.
func stdLibImportPathOf(dir string, goRoot string) string {
	if dir == "" || goRoot == "" {
		return ""
	}

	sourceRoot := filepath.Join(goRoot, "src")

	if !isPathUnder(dir, sourceRoot) {
		return ""
	}

	relative, err := filepath.Rel(sourceRoot, dir)

	if err != nil {
		return ""
	}

	if relative == "." {
		return ""
	}

	return filepath.ToSlash(relative)
}

// requireConvertibleTestTarget refuses a -tests run whose target is a package the -stdlib queue
// deliberately skips, unless -test-allow-handown says the caller means it.
//
// WHY THIS EXISTS, and why the skip list alone did not cover it. isNonConvertedStdLibPackage gates
// the -stdlib QUEUE — its callers are scanStdLib and the ref-lowering census, and nothing on the
// -tests path ever consulted it. So `go2cs -tests <GOROOT>/src/testing <repo>/src/core/testing`,
// which is the shape every other row is converted with and therefore the shape a hand or a script
// reaches for, ran happily and converted Go's production testing.go, benchmark.go, match.go,
// allocs.go, cover.go, example.go, fuzz.go, newcover.go and run_example.go INTO the directory the
// hand-owned Phase-4 test host lives in.
//
// MEASURED 2026-09-03, both halves, in an isolated worktree:
//
//   - Without the file-level marker: testing.cs's 685 hand-written lines were replaced by Go's
//     converted testing.go (+2622/-560), and the publish then failed CS0117/CS1929 in the host's
//     own TestHost.cs and TestExecution.cs on M.Runner, M.Run and T.Execution — members Go's
//     testing.go does not declare.
//   - With the marker (now committed on all ten host files): testing.cs survived byte-identical
//     and a testing.cs.auto sibling appeared, but the other nine converted files still landed in
//     the same testing_package and the publish failed with 56 errors, 25 of them CS0111 duplicate
//     members plus CS0260/CS0102/CS1537 — the F15b "ONE testing package, period" collision, exactly
//     as isNonConvertedStdLibPackage's comment predicts.
//
// So the marker is necessary but not sufficient: it protects the one file whose path collides, and
// nothing protects the assembly. This guard is what makes the mistyped command inert.
//
// The predicate is isNonConvertedStdLibPackage itself rather than a second list of names, so a
// package added to the skip list tomorrow is refused here the same day without anyone remembering
// to update a twin.
//
// The OVERRIDE is deliberate, not a courtesy. The census that produced the measurements above is a
// legitimate and repeatable thing to want — it is how the admission arithmetic for `testing`'s own
// suite was established — and refusing it outright would push the next person to comment the guard
// out instead. What the flag does NOT do is make such a run bankable: the collision above is
// structural, so the emission is only ever something to read and throw away, and the caller is told
// to point it at a scratch root.
func requireConvertibleTestTarget(inputPath, outputPath string, options Options) (testTargetKind, error) {
	importPath := stdLibImportPathOf(inputPath, options.goRoot)

	if importPath == "" || !isNonConvertedStdLibPackage(importPath) {
		return testTargetConvertible, nil
	}

	// The sanctioned census, unchanged: -test-allow-handown means "show me what the conversion
	// WOULD produce", and it produces it — production sources and all — wherever it is pointed.
	// It is checked BEFORE the host mode deliberately, so every behavior this flag had on
	// 2026-09-03 it still has, including the destructive one the train-18 control measures.
	if options.testAllowHandOwn {
		return testTargetConvertible, nil
	}

	if handOwnHostTestTarget(inputPath, outputPath) {
		return testTargetHandOwnHost, nil
	}

	reason := fmt.Sprintf("%q is deliberately kept out of the conversion queue", importPath)
	damage := "converting it would write a second copy of the package into the tree its hand-written counterpart lives in"

	switch {
	case importPath == "testing":
		reason = "`testing` is ENTIRELY HAND-OWNED: src/core/testing is the Phase-4 test host, go2cs " +
			"machinery standing in for a state machine over Go's goroutine scheduler, not a transcription of it"
		damage = "the conversion's natural output path IS that host's directory, so this run would overwrite " +
			"the hand-written testing.cs with Go's converted testing.go and emit its converted siblings " +
			"(benchmark.cs, match.cs, allocs.cs, ...) beside the host files, producing the F15b " +
			"\"ONE testing package, period\" collision the skip list names"
	case importPath == "unsafe" || importPath == "builtin":
		reason = fmt.Sprintf("`%s` is a compiler intrinsic with no convertible source", importPath)
		damage = "there is nothing to convert, so the run can only damage what is already there"
	case importPath == "cmd" || strings.HasPrefix(importPath, "cmd/"):
		reason = fmt.Sprintf("%q is the Go toolchain, not the standard library", importPath)
		damage = "the corpus has no counterpart for it, so the emission would have nothing to compile against"
	}

	return testTargetConvertible, fmt.Errorf(
		"-tests refuses %s: %s, and %s. "+
			"The -stdlib queue has skipped it since isNonConvertedStdLibPackage was written; this guard is the "+
			"-tests half of that same decision, which was missing until 2026-09-03. "+
			"If you are deliberately measuring what the conversion WOULD produce, pass -test-allow-handown and "+
			"give the run a SCRATCH output root (a second positional argument) so the emission lands somewhere "+
			"you can throw away -- it is a census, never a row: the collision above is structural and no such "+
			"run can bank. "+
			"(A hand-owned host whose C# counterpart ALREADY EXISTS at the output path, and whose Go package "+
			"has a _test.go suite, is converted TESTS-ONLY instead of refused -- see handOwnHostTestTarget. "+
			"Reaching this message for such a package means the output path is not that counterpart's directory.)",
		importPath, reason, damage)
}

// testTargetKind is what requireConvertibleTestTarget decided about a -tests target: an ordinary
// package whose production sources convert ahead of its tests, or a hand-owned HOST whose C#
// counterpart is already on disk and must not be re-emitted over.
type testTargetKind int

const (
	// testTargetConvertible is every ordinary package: convert production, then its tests.
	testTargetConvertible testTargetKind = iota

	// testTargetHandOwnHost is a package the -stdlib queue skips BECAUSE a hand-written C#
	// counterpart already stands in for it, whose Go package nevertheless has a test suite worth
	// running against that counterpart. The run converts the EXTERNAL test variant only and emits
	// no production file at all — see processTestConversion's handling and DESIGN notes in
	// docs/phase4/CENSUS-testing-osuser-rows.md §2.4 (Option 1, owner-ruled 2026-08-30).
	testTargetHandOwnHost
)

// handOwnHostTestTarget reports whether inputPath's package is a hand-owned host that the -tests
// pipeline can measure by converting its TESTS ONLY into outputPath.
//
// EVIDENCE-BASED, not a name list, and each clause is load-bearing:
//
//   - a project file at outputPath — the counterpart exists as a real compilable project, so the
//     converted tests have something to take a ProjectReference on. A scratch root has none, which
//     is what keeps `-test-allow-handown <scratch>` on its documented census path.
//   - at least one [module: GoManualConversion] file there — the counterpart is HAND-OWNED rather
//     than a stale converted copy. The scan is line-anchored and reads whole files: reflect and
//     internal/reflectlite MENTION the marker inside placeholder comments, and a head-window scan
//     misses markers that sit below long license blocks (both traps are in CLAUDE.md).
//   - a *_test.go in the Go package — `unsafe` and `builtin` are compiler intrinsics with a
//     hand-owned counterpart and NO suite, so this clause is what keeps them refused rather than
//     silently writing a no-tests manifest into their hand-owned directories.
func handOwnHostTestTarget(inputPath, outputPath string) bool {
	if outputPath == "" {
		return false
	}

	projects, err := filepath.Glob(filepath.Join(outputPath, "*.csproj"))
	if err != nil || len(projects) == 0 {
		return false
	}

	tests, err := filepath.Glob(filepath.Join(inputPath, "*_test.go"))
	if err != nil || len(tests) == 0 {
		return false
	}

	sources, err := filepath.Glob(filepath.Join(outputPath, "*.cs"))
	if err != nil {
		return false
	}

	for _, source := range sources {
		content, err := os.ReadFile(source)
		if err != nil {
			continue
		}

		if manualConversionMarker.Match(content) {
			return true
		}
	}

	return false
}

// manualConversionMarker matches a [module: GoManualConversion] attribute on its own line, with or
// without the `go.` qualifier. LINE-ANCHORED on purpose: an unanchored search reports every file
// that merely names the marker in a comment (63 against a real 40, corpus-wide, CLAUDE.md).
var manualConversionMarker = regexp.MustCompile(`(?m)^\s*\[module:\s*(go\.)?GoManualConversion\]`)

// Markers substituted into test-csproj-template.xml by writeTestProject (embedded-resource
// template, following the csproj-template.xml precedent — never a hardcoded csproj string).
const (
	TestRootNamespaceMarker     = ">>MARKER:TEST_ROOT_NAMESPACE<<"
	TestAssemblyNameMarker      = ">>MARKER:TEST_ASSEMBLY_NAME<<"
	TestGo2CSRelativePathMarker = ">>MARKER:TEST_GO2CS_RELATIVE_PATH<<"
	TestCompileItemsMarker      = ">>MARKER:TEST_COMPILE_ITEMS<<"
	TestFixtureItemsMarker      = ">>MARKER:TEST_FIXTURE_ITEMS<<"
	TestProjectReferencesMarker = ">>MARKER:TEST_PROJECT_REFERENCES<<"
)

const unsupportedCapabilityReasonPrefix = "requires unsupported testing capabilities: "

// testProjectModel selects how the generated test project binds the PRODUCTION package.
type testProjectModel int

const (
	// testProjectRecompile compiles the production .cs INTO the test assembly alongside the
	// converted test sources (the original -tests model). Retained as a fallback when converted
	// test metadata would have to add operators to a closed production type.
	testProjectRecompile testProjectModel = iota

	// testProjectWhiteboxReference references the production project while internal test files
	// emit into a friend-assembly bridge class. Production remains the sole identity for its types.
	testProjectWhiteboxReference

	// testProjectReference references the colocated production csproj instead of recompiling
	// its sources, so the production ASSEMBLY stays the single identity for the production
	// types. A black-box (external-only) suite touches only the package's exported API, which
	// resolves cross-assembly exactly as it does for every other converted consumer — while a
	// recompile there DUPLICATES the production types: a referenced stdlib assembly whose API
	// mentions a production type (strings.ToLowerSpecial(unicode.SpecialCase, …)) names the
	// type in the PRODUCTION assembly, and the test assembly's recompiled copy is a distinct
	// type — CS0012 (unicode's letter_test). Applies to black-box-only packages
	// (unicode, unicode/utf8, path, …); mixed/internal suites use whitebox-reference.
	testProjectReference
)

func (m testProjectModel) String() string {
	switch m {
	case testProjectWhiteboxReference:
		return "whitebox-reference"
	case testProjectReference:
		return "reference"
	default:
		return "recompile"
	}
}

func (m testProjectModel) referencesProduction() bool {
	return m == testProjectReference || m == testProjectWhiteboxReference
}

// selectTestProjectModel references production for both suite shapes: black-box-only suites use
// the ordinary reference model; a suite with an internal variant uses the friend-assembly bridge.
// Either reference model can still fall back when converted records require a real mutation of a
// closed production type (errProductionAnchoredRecords — see processTestConversion).
func selectTestProjectModel(internal, external *packages.Package) testProjectModel {
	if internal != nil {
		return testProjectWhiteboxReference
	}
	if external != nil {
		return testProjectReference
	}

	return testProjectRecompile
}

// testVariantOptions derives the per-variant conversion options from the base options, the selected
// model and which variant is about to convert.
//
// Which variant is under conversion is a fact about the SOURCES, not about the model: the external
// half composes Go's package-qualified spelling of a package-under-test declaration under EVERY
// model, and testDeclaredAliasSpelledBare has to unmake it under recompile as well as under
// white-box reference. The flag was set only for the white-box model until net/netip took the
// recompile fallback and landed on the CS0426 that spelling produces.
//
// Nothing else reads testExternalVariant outside the white-box path — whiteboxBridgeDeclaredType is
// reachable only through testOwnedAdapterRef, which returns early unless the model is white-box
// reference — so widening it moves exactly the one rule that needed it.
//
// The bridge overrides remain white-box-only: they name the friend-assembly class that owns internal
// test declarations, which no other model has.
func testVariantOptions(base Options, model testProjectModel, isExternal bool, internalBridgeName string) Options {
	base.testExternalVariant = isExternal

	// Whether this variant's emission may NAME production's internal declarations — the question
	// productionLiftReuseReachable asks, and the one the variant flag above cannot answer, because
	// accessibility is decided by the ASSEMBLY the C# lands in and not by the Go package the source
	// came from. Model by model:
	//
	//   - RECOMPILE: the production `.cs` are compile items of the test assembly, so every
	//     production declaration is same-assembly for BOTH variants.
	//   - WHITEBOX REFERENCE: chosen exactly when an internal `_test.go` exists
	//     (selectTestProjectModel), which is the SAME fact that makes the production conversion emit
	//     `InternalsVisibleTo $(AssemblyName).tests` (hasSiblingInternalTestFiles ->
	//     insertFriendAssemblyAccess, projectFileWriter.go). Both variants emit into that one
	//     `.tests` project, so the external half has package-private sight of production too — and
	//     it already relies on it: the internal bridge names production's internal lifts directly
	//     (runtime's export_test.cs `Func<ifaceHash_i, …>`), so a MISSING grant would fail the build
	//     before any dedup decision was reached. The grant is a CONSEQUENCE of the model, never an
	//     input to it, which is why this is decidable here without reading the csproj back.
	//   - REFERENCE: chosen only when there is NO internal test file, so the production csproj
	//     carries no grant at all and the external suite compiles into a plain referencing assembly.
	//     `errors` is that package (all four of its test files are `package errors_test`).
	base.testProductionInternalsVisible = !isExternal || model != testProjectReference

	if model == testProjectWhiteboxReference && !isExternal {
		base.testClassNameOverride = internalBridgeName
		base.testInlineTypeAccess = true
	}

	return base
}

// errProductionAnchoredRecords signals that a reference-model conversion attempt collected
// GoImplement/GoImplicitConv records whose GENERATED code must anchor to the production
// package class (a partial struct merged into a production type declaration, or conversion
// operators on one) — impossible across an assembly boundary, where the referenced production
// types are closed. The caller falls back to the recompile model, which reconverts with the
// production types local.
var errProductionAnchoredRecords = errors.New("test variant records production-anchored metadata")

// recordsRequireProductionAnchor reports whether the LIVE record globals — the just-converted
// external variant's collected records — contain any entry that must anchor to the production
// class, evaluated with the recompile-model partition predicates (isTestAnchoredImplementRecord /
// isTestAnchoredConversionRecord). Under the reference model nothing is seeded and
// testLocalTypePrefixes stays empty, so every production type renders package-qualified, and any
// record landing in the production partition is one whose generated partial/adapter/operator
// would need to merge with a production declaration. A record that renders a production type
// through its imported ꓸ type-alias form (`<pkg>ꓸ<Type>`, TypeAliasDot) is likewise treated as
// production-anchored — conservatively, since the partition predicates cannot see the production
// qualifier inside the alias identifier.
func recordsRequireProductionAnchor(productionClassName, productionPackageName string, handOwnHost bool) bool {
	_, productionAnchored := splitExternalVariantRecords(productionClassName, handOwnHost)

	if !productionAnchored.isEmpty() {
		return true
	}

	aliasPrefix := getSanitizedIdentifier(productionPackageName) + TypeAliasDot
	names := make([]string, 0)

	for ifaceName, implementations := range interfaceImplementations {
		names = append(names, ifaceName)
		names = append(names, implementations.Keys()...)
	}

	for ifaceName, implementations := range promotedInterfaceImplementations {
		names = append(names, ifaceName)
		names = append(names, implementations.Keys()...)
	}

	for _, proxy := range constraintProxies {
		names = append(names, proxy[0], proxy[1])
	}

	for _, conversions := range []map[string]HashSet[string]{implicitConversions, invertedImplicitConversions, indirectImplicitConversions} {
		for sourceType, targetTypes := range conversions {
			names = append(names, sourceType)
			names = append(names, targetTypes.Keys()...)
		}
	}

	for _, conversions := range []map[string]map[string]string{numericConversions, indirectNumericConversions} {
		for sourceType, targetTypes := range conversions {
			names = append(names, sourceType)

			for targetType := range targetTypes {
				names = append(names, targetType)
			}
		}
	}

	for _, name := range names {
		if strings.Contains(name, aliasPrefix) {
			return true
		}
	}

	return false
}

// recordsRequireProductionMutation reports records that a white-box reference project cannot
// relocate into its test-owned metadata anchor. Interface implementation records are relocatable:
// qualified production structs are foreign to the test compilation, so go2cs-gen emits value or
// pointer adapter classes in the test anchor instead of partial production structs. Structural
// conversions involving a production type still require a partial conversion operator on that
// closed type — except for the pointer-boxing route, whose operator the generator never hosts at
// all (pointerBoxRecordEitherOrientation). Numeric conversions can relocate to the test-local
// operand, but not when both operands belong to production.
func recordsRequireProductionMutation(productionClassName, productionPackageName string) bool {
	aliasPrefix := getSanitizedIdentifier(productionPackageName) + TypeAliasDot
	shadowAliasPrefix := ShadowVarMarker + getSanitizedIdentifier(productionPackageName) + "."
	normalize := func(name string) string {
		return strings.TrimPrefix(name, "global::")
	}
	isProductionType := func(name string) bool {
		if trimmed, ok := strings.CutPrefix(name, PointerPrefix+"<"); ok {
			name = strings.TrimSuffix(trimmed, ">")
		}
		name = normalize(name)
		return strings.Contains(name, productionClassName+".") || strings.Contains(name, aliasPrefix) ||
			strings.Contains(name, shadowAliasPrefix)
	}

	for _, conversions := range []map[string]HashSet[string]{implicitConversions, invertedImplicitConversions, indirectImplicitConversions} {
		for sourceType, targetTypes := range conversions {
			for targetType := range targetTypes {
				if pointerBoxRecordEitherOrientation(sourceType, targetType) {
					// T -> ж<T> is the shared Go pointer-boxing route. The generator intentionally
					// emits no type-owned operator for a foreign T, so it does not mutate production.
					// Same predicate conversionRecordHasLocalOperand reads to admit the record at all.
					//
					// The exemption belongs to the SHAPE, not to the map that happens to hold it.
					// It lived on indirectImplicitConversions alone until 2026-08-26, which read as
					// sufficient because a conversion SITE routes a pointer target there — but the
					// dynamic-struct implicit-cast site records into implicitConversions whatever the
					// target's pointer-ness, so the identical pair can arrive in a direct map. That
					// omission is what forced `crypto/x509` off the reference model: its ONE
					// production-typed record is `Certificate -> ж<Certificate>` in
					// implicitConversions, and the recompile fallback then compiled a SECOND
					// Certificate into the test assembly while `crypto/tls` — a referenced assembly —
					// kept returning the production one, splitting the type's identity (CS0012 x4 +
					// CS1929 x2 in hybrid_pool_test.cs, the one file that reaches x509 through tls).
					continue
				}
				if isProductionType(sourceType) || isProductionType(targetType) {
					return true
				}
			}
		}
	}

	for _, conversions := range []map[string]map[string]string{numericConversions, indirectNumericConversions} {
		for sourceType, targetTypes := range conversions {
			for targetType := range targetTypes {
				if isProductionType(sourceType) && isProductionType(targetType) {
					return true
				}
			}
		}
	}

	return false
}

// nominalConstraintsRequireProductionMutation reports whether this variant emitted a nominal C#
// constraint (`where P : I`) binding a PRODUCTION type argument to a test-declared interface. Such
// a constraint is checked against the type argument itself, so the only thing that can satisfy it
// is the argument's own base list — a partial declaration on a closed referenced type, which is
// precisely the production mutation the reference model exists to avoid. Unlike a conversion
// record it cannot be relocated to an adapter, so the suite must take the recompile model.
func nominalConstraintsRequireProductionMutation() bool {
	packageLock.Lock()
	defer packageLock.Unlock()

	return !nominalProductionConstraints.IsEmpty()
}

// isGo2CSRoot reports whether dir is a go2cs project-reference root — the directory the
// $(go2csPath) MSBuild property points at, identified by the shared runtime living at
// core\golib\golib.csproj beneath it.
func isGo2CSRoot(dir string) bool {
	if dir == "" {
		return false
	}

	_, err := os.Stat(filepath.Join(dir, "core", "golib", "golib.csproj"))
	return err == nil
}

// findGo2CSRootAbove walks dir's ancestor chain (inclusive) and returns the first go2cs
// project-reference root, or "" when none exists above dir.
func findGo2CSRootAbove(dir string) string {
	for current := dir; ; {
		if isGo2CSRoot(current) {
			return current
		}

		parent := filepath.Dir(current)

		if parent == current {
			return ""
		}

		current = parent
	}
}

type testDeclaration struct {
	Name                 string   `json:"name"`
	Kind                 string   `json:"kind"`
	PackageName          string   `json:"packageName"`
	CSharpClassName      string   `json:"-"`
	Source               string   `json:"source"`
	Line                 int      `json:"line"`
	Status               string   `json:"status"`
	Reason               string   `json:"reason,omitempty"`
	RequiredCapabilities []string `json:"requiredCapabilities,omitempty"`
}

type testSource struct {
	Path   string `json:"path"`
	Kind   string `json:"kind"`
	Status string `json:"status"`
	Reason string `json:"reason,omitempty"`
}

type testManifest struct {
	SchemaVersion           int               `json:"schemaVersion"`
	CapabilitiesVersion     int               `json:"capabilitiesVersion"`
	PackageImportPath       string            `json:"packageImportPath"`
	ProjectName             string            `json:"projectName"`
	TestProject             string            `json:"testProject"`
	GoVersion               string            `json:"goVersion"`
	TargetGOOS              string            `json:"targetGOOS"`
	TargetGOARCH            string            `json:"targetGOARCH"`
	SourceRevision          string            `json:"sourceRevision,omitempty"`
	ConverterRevision       string            `json:"converterRevision"`
	InputDigest             string            `json:"inputDigest"`
	TestProjectModel        string            `json:"testProjectModel,omitempty"`
	ProductionFiles         []string          `json:"productionFiles"`
	TestSources             []testSource      `json:"testSources"`
	Fixtures                []string          `json:"fixtures"`
	FixtureDirectories      []string          `json:"fixtureDirectories"`
	// Fixture directories staged as a LINK into the real GOROOT directory rather than as file
	// copies, so the Go toolchain accepts their `internal/…` imports (linkStagedFixtureDirs).
	// Their files appear in NEITHER Fixtures nor the csproj: the host creates one link each at
	// sandbox construction. `omitempty` keeps every package that has none byte-identical to what a
	// converter without this capability wrote.
	FixtureLinks            []string          `json:"fixtureLinks,omitempty"`
	Tests                   []testDeclaration `json:"tests"`
	TestMain                *testDeclaration  `json:"testMain,omitempty"`
	Dependencies            []string          `json:"dependencies"`
	Capabilities            []string          `json:"capabilities"`
	RequiredCapabilities    []string          `json:"requiredCapabilities"`
	UnsupportedCapabilities []string          `json:"unsupportedCapabilities"`
}

func processTestConversion(inputPath, outputPath string, options Options) error {
	// The sibling declarator names steer the PRODUCTION pass only (see siblingTestFuncMethodNames);
	// that pass is complete by the time this runs. Each variant's own analysis then computes the
	// shadow set from its own universe — the in-package variant already contains these names, and
	// the external variant's declarations live in a different C# class, so leaving them set would
	// only over-qualify the external half's package idents.
	siblingTestFuncMethodNames = nil

	// Likewise for the addressed-global seed: each variant's universe already CONTAINS the
	// `_test.go` files, so its own collectAddressedGlobals sees `&g` directly. The seed exists only
	// for the production pass, which cannot.
	siblingTestAddressedGlobalNames = nil

	inputPath, err := filepath.Abs(inputPath)
	if err != nil {
		return err
	}

	outputPath, err = filepath.Abs(outputPath)
	if err != nil {
		return err
	}

	targetParts := strings.Split(options.targetPlatform, "/")
	if len(targetParts) != 2 {
		return fmt.Errorf("invalid target platform format %q", options.targetPlatform)
	}

	cfg := &packages.Config{
		Mode:       packages.LoadAllSyntax,
		Dir:        inputPath,
		Tests:      true,
		BuildFlags: options.loaderBuildFlags(),
		Env: append(os.Environ(),
			fmt.Sprintf("GOOS=%s", targetParts[0]),
			fmt.Sprintf("GOARCH=%s", targetParts[1])),
	}

	loaded, err := packages.Load(cfg, ".")
	if err != nil {
		return fmt.Errorf("load test package variants: %w", err)
	}

	production := findProductionPackage(loaded, inputPath)
	if production == nil {
		return fmt.Errorf("go/packages did not return a production package for %q", inputPath)
	}

	if len(production.Errors) > 0 {
		return fmt.Errorf("production package load failed: %v", production.Errors)
	}

	// External package tests import the package under test. Bind that import to the
	// production partial class compiled into the same test assembly, never to a
	// project reference back to the production DLL.
	options.testPackagePath = production.PkgPath
	options.testPackageName = production.Name

	internal, external := findTestVariants(loaded, production)
	if internal == nil && external == nil {
		return writeNoTestsManifest(production, inputPath, outputPath, targetParts, options)
	}

	// Phase-4D file exclusion (option-a ruling): drop Example/Benchmark-only test files from the
	// compile set (both models honor it below). Computed once from both variants — a cross-variant
	// reference keeps a file compiled — and reused across the reference→recompile fallback.
	compileExcluded := selectCompileExcludedTestFiles(internal, external)

	projectName, projectNamespace := getProjectName(inputPath, options)
	supported := NewHashSet(supportedTestCapabilities())
	testInfoPath := filepath.Join(outputPath, testPackageInfoFileName)

	model := selectTestProjectModel(internal, external)

	// A HAND-OWNED HOST row is external-only by construction, and both halves of that are stated
	// here rather than left to fall out of selectTestProjectModel's nil check.
	//
	// The MODEL is forced rather than derived because `internal` is non-nil and must stay so:
	// discovery walks it, which is what puts its declarations in the manifest with their own
	// capability statuses instead of leaving 20 names the oracle produces unaccounted. Only its
	// EMISSION is suppressed, by the exclusion pass below.
	//
	// testProjectReference is then the correct model on its own terms: the production package is a
	// separate assembly the test project takes a colocated ProjectReference on — which for a host
	// row is the hand-written counterpart itself. There is no white-box bridge to build, because
	// there are no internal test files to put in one.
	handOwnHostExcluded := map[string]bool{}

	if options.testHandOwnHost {
		model = testProjectReference
		handOwnHostExcluded = markHandOwnHostExcludedTestFiles(internal, external, compileExcluded)
	}
	conversion, err := convertTestVariants(model, production, internal, external, compileExcluded, inputPath, outputPath, projectNamespace, supported, options)

	if errors.Is(err, errProductionAnchoredRecords) {
		// A HAND-OWNED HOST has no recompile to fall back to, and falling through silently would be
		// the worst available outcome: the recompile model makes the production .cs COMPILE ITEMS of
		// the test assembly, and for a host row those are the hand-written host sources — a second
		// copy of every host type in a second assembly, which is the F15b collision arriving by the
		// back door after the front one was locked. There is no correct automatic remedy (the
		// records need a production type this run must not mutate), so this is a loud stop naming
		// the package and what would have happened.
		if options.testHandOwnHost {
			return fmt.Errorf(
				"-tests on the hand-owned host %q collected metadata that must anchor to the production class, "+
					"which only the recompile model can host -- and recompiling would make the HAND-WRITTEN host "+
					"sources compile items of the test assembly, producing a second copy of every host type. "+
					"Refusing rather than falling back: %w",
				production.PkgPath, err)
		}

		// The suite records metadata that must mutate a production type — only a same-assembly
		// recompile can host it. Reconvert under the recompile model: conversion is deterministic
		// and the expensive go/packages load above is reused, so fallback costs one emission pass.
		model = testProjectRecompile
		conversion, err = convertTestVariants(model, production, internal, external, compileExcluded, inputPath, outputPath, projectNamespace, supported, options)
	}

	if err != nil {
		return err
	}

	declarations := conversion.declarations
	testMain := conversion.testMain
	outputFiles := conversion.outputFiles
	allImports := conversion.allImports
	requiredCapabilities := conversion.requiredCapabilities
	includedSources := conversion.includedSources

	// The FLAVOR gap: a `_test.go` the conversion's build tags exclude but a plain `go test` on
	// the same platform includes declares tests that are real on exactly one side of the
	// differential oracle. Declare them disclosed-unsupported so the F6 census accounts for every
	// name go test runs (see flavorExcludedTestDeclarations).
	declarations = append(declarations, flavorExcludedTestDeclarations(inputPath, options, declarations)...)

	// A declaration whose SOURCE FILE was dropped from the compile set cannot be `included`: the
	// generated host REGISTERS every included test by name, and a name in a file nobody compiled is
	// a CS0117 against the test package class. Statused here rather than at discovery because
	// discovery deliberately runs over every file (that is what keeps an excluded file's
	// declarations in the manifest at all) and the compile set is not final until the exclusion
	// passes above have run.
	//
	// The case could not arise before the hand-owned-host mode: Phase-4D only ever drops files whose
	// runnable declarations are Example/Benchmark, and those are already `unsupported` by KIND, so
	// the host skipped them for a different reason and the gap never showed. The host rule drops a
	// file for what it REFERENCES, so a perfectly ordinary `test` declaration can now live in an
	// uncompiled file -- measured: `testing`'s TestPrettyPrint, in benchmark_test.go, which the
	// export_test.go edge excludes, and which the pipeline admitted because its only testing.* touch
	// is the package-level VAR testing.PrettyPrint that the capability analysis does not key on.
	//
	// Written as a general rule rather than a host-mode one: the invariant "the host may name only
	// what the compilation contains" is true of every model, and a future Phase-4D widening that
	// drops a file holding a Test would meet it the same way.
	for i := range declarations {
		if declarations[i].Status != "included" {
			continue
		}

		sourcePath := filepath.Clean(filepath.Join(inputPath, declarations[i].Source))

		if !compileExcluded[sourcePath] {
			continue
		}

		declarations[i].Status = "unsupported"

		if handOwnHostExcluded[sourcePath] {
			declarations[i].Reason = handOwnHostExcludedSourceReason
		} else {
			declarations[i].Reason = compileExcludedSourceReason
		}
	}

	sort.Slice(declarations, func(i, j int) bool {
		if declarations[i].Name == declarations[j].Name {
			return declarations[i].PackageName < declarations[j].PackageName
		}
		return declarations[i].Name < declarations[j].Name
	})

	// Hand-owned companions of TEST-file conversions — `*_impl_test.cs`
	// (internal/reflectlite's export_impl_test.cs is the pattern's first instance). They are
	// committed beside the package exactly like the production `*_impl.cs` companions and are
	// compiled into the TEST project: the `_test.cs` suffix keeps them under the production
	// side's existing test-artifact exclusion (csproj template and productionCSFiles both),
	// so no production emission changes. Globbed FRESH (F7) so a companion appearing or
	// disappearing re-shapes the project without a recorded list; testInputDigest globs the
	// same pattern so editing one invalidates a prior comparison.
	testImplCompanions, err := filepath.Glob(filepath.Join(outputPath, "*_impl_test.cs"))
	if err != nil {
		return err
	}
	for _, companion := range testImplCompanions {
		name := filepath.Base(companion)
		if !containsString(outputFiles, name) {
			outputFiles = append(outputFiles, name)
		}
	}

	sort.Strings(outputFiles)

	fixtures, fixtureLinks, err := copyTestFixtures(inputPath, outputPath)
	if err != nil {
		return err
	}

	fixtureDirectories, err := testFixtureDirectories(inputPath)
	if err != nil {
		return err
	}

	productionFiles, err := productionCSFiles(outputPath, goosOfTarget(options.targetPlatform))
	if err != nil {
		return err
	}

	if err := writeTestHost(outputPath, projectNamespace, production.PkgPath, declarations, testMain, fixtures, fixtureDirectories, fixtureLinks); err != nil {
		return err
	}

	dependencies := allImports.Keys()
	dependencies = removeString(dependencies, production.PkgPath)
	dependencies = removeString(dependencies, "testing")
	sort.Strings(dependencies)

	referenceImports := append(append([]string{}, dependencies...), aliasReferenceImports(
		testProjectAliasScanFiles(model, outputPath, testInfoPath, outputFiles, productionFiles),
		production.PkgPath, dependencies)...)

	// Close the reference set under the C# DECLARATION edges the converter emits
	// (declarationClosureImports): binding a type the compilation NAMES needs the assemblies its
	// own declaration names — an interface's base interfaces (hash's `Hash : io.Writer` reached by
	// every `GoImplement<…, hash_package.Hash64>` record, io/fs's `File : io.ReadCloser`) and a
	// struct's field types (testing/quick's `Config` holds a `*rand.Rand`) — and those edges belong
	// to the DECLARING package's import graph, so no test import and no alias `using` names them
	// (CS0012). Both models feed the same walk; the already-referenced set it subtracts carries the
	// production package's own path so an edge landing there never becomes a self-reference.
	//
	// The package's OWN package_info.cs — written moments ago by the production half of this same
	// run — supplies the fourth edge's gate: the VALUE-form `[assembly: GoImplement<T, I>]` records
	// go2cs-gen realizes as base lists on the production types the test half binds members on.
	referenceImports = append(referenceImports, declarationClosureImports(
		[]*packages.Package{production, internal, external}, compileExcluded,
		append([]string{production.PkgPath}, referenceImports...),
		packageImplementBases(platformPackageInfoPath(outputPath, goosOfTarget(options.targetPlatform))),
		foreignImplementBasesResolver(options))...)

	testProjectName := projectFileBaseName(projectName) + ".tests.csproj"
	if err := writeTestProject(filepath.Join(outputPath, testProjectName), projectName, projectNamespace, model, productionFiles, outputFiles, fixtures, referenceImports, options); err != nil {
		return err
	}

	sources, err := classifyTestSources(inputPath, includedSources, compileExcluded, handOwnHostExcluded, external)
	if err != nil {
		return err
	}

	capabilities := supportedTestCapabilities()
	required := requiredCapabilities.Keys()
	sort.Strings(required)
	unsupported := NewHashSet(required)
	unsupported.ExceptWith(capabilities)
	unsupportedList := unsupported.Keys()
	sort.Strings(unsupportedList)

	manifest := testManifest{
		SchemaVersion:           1,
		CapabilitiesVersion:     1,
		PackageImportPath:       production.PkgPath,
		ProjectName:             projectName,
		TestProject:             testProjectName,
		GoVersion:               runtime.Version(),
		TargetGOOS:              targetParts[0],
		TargetGOARCH:            targetParts[1],
		SourceRevision:          gitRevision(inputPath),
		ConverterRevision:       converterRevision(),
		TestProjectModel:        model.String(),
		ProductionFiles:         productionFiles,
		TestSources:             sources,
		Fixtures:                fixtures,
		FixtureDirectories:      fixtureDirectories,
		FixtureLinks:            fixtureLinks,
		Tests:                   declarations,
		TestMain:                testMain,
		Dependencies:            dependencies,
		Capabilities:            capabilities,
		RequiredCapabilities:    required,
		UnsupportedCapabilities: unsupportedList,
	}

	manifest.InputDigest, err = testInputDigest(inputPath, outputPath, options, manifest.ConverterRevision)
	if err != nil {
		return err
	}

	return writeJSONFile(filepath.Join(outputPath, testManifestFileName), manifest)
}

// testVariantConversionResult carries everything one convertTestVariants pass produced — the
// model-dependent conversion state a reference→recompile fallback re-run rebuilds from scratch.
type testVariantConversionResult struct {
	declarations         []testDeclaration
	testMain             *testDeclaration
	outputFiles          []string
	allImports           HashSet[string]
	requiredCapabilities HashSet[string]
	includedSources      HashSet[string]
}

// convertTestVariants converts the package's test variants under the given test-project model:
// seeds the package_test_info.cs anchor, discovers and converts each variant, and merges the
// collected metadata into the model's anchor file(s). A reference model returns
// errProductionAnchoredRecords when records require a closed production-type mutation; the caller
// then re-runs the pass under testProjectRecompile (the go/packages load remains shared).
func convertTestVariants(model testProjectModel, production, internal, external *packages.Package, compileExcluded map[string]bool, inputPath, outputPath, projectNamespace string, supported HashSet[string], options Options) (testVariantConversionResult, error) {
	internalUnitListed := false

	result := testVariantConversionResult{
		declarations:         make([]testDeclaration, 0),
		outputFiles:          make([]string, 0),
		allImports:           HashSet[string]{},
		requiredCapabilities: HashSet[string]{},
		includedSources:      HashSet[string]{},
	}

	// Collected across BOTH variants (result.outputFiles carries csproj <Compile> names, not
	// paths) so the deferred adapter names can be resolved once, after the merged metadata file
	// makes the record set final.
	testAdapterResolveNames = nil
	emittedAdapterPairAnchors = nil

	// A model change between runs (or a recompile fallback) must not leave a stale bridge anchor
	// on disk: it is merge-preserving, and a superseded record set would silently resurrect.
	// The models that need it re-seed it below; everything else keeps the directory clean.
	_ = os.Remove(filepath.Join(outputPath, internalTestPackageInfoFileName))

	internalBridgeName := getSanitizedImport(production.Name + "_internal_test" + PackageSuffix)
	testClassName := internalBridgeName
	testPackageName := production.Name
	if external != nil {
		testClassName = getSanitizedImport(external.Name + PackageSuffix)
		testPackageName = external.Name
	}

	if model.referencesProduction() {
		// The recompile model's external-test anchor is superseded under a reference model; a
		// copy left by a previous recompile conversion is merge-preserving and would resurrect
		// stale records on the next fallback re-run.
		_ = os.Remove(filepath.Join(outputPath, externalTestPackageInfoFileName))
	}

	// The bridge's declared-name set drives the white-box record split: a BARE record name in
	// this set is a bridge-declared type whose generated partial must merge inside the bridge.
	whiteboxBridgeTypeNames = HashSet[string]{}
	if model == testProjectWhiteboxReference {
		whiteboxBridgeTypeNames = collectWhiteboxBridgeTypeNames(internal)
	}

	if model.referencesProduction() {
		options.testProductionPath = options.testPackagePath
		options.testProductionName = options.testPackageName
		options.testMetadataAnchorName = testClassName
		if model == testProjectWhiteboxReference {
			options.testWhiteboxReference = true
			options.testInternalBridgeName = internalBridgeName
		}

		// The production package binds as an ORDINARY imported package: its exported metadata
		// (type aliases, implements) loads from the colocated package_info.cs like any other
		// dependency's, its types render package-qualified, and isSameAssemblyPkg answers false
		// so cast sites compose the same foreign adapter names go2cs-gen generates for a
		// project-referenced package. Clearing the self-import binding is what flips all of it
		// (visitImportSpec's isPackageUnderTest, convertTestVariant's testLocalTypePrefixes and
		// loadPackageImplements are each gated on these fields).
		options.testPackagePath = ""
		options.testPackageName = ""
	}

	// Session-scoped, not per-variant (B2/B9): both variants come from the ONE load the caller
	// performed, so the external variant's references to an internal-variant-renamed method (the
	// export_test pattern) resolve by object identity to entries registered during the internal
	// pass — resetPackageState deliberately does not clear this map.
	testMethodRenames = make(map[types.Object]bool)
	testTypeRenames = make(map[types.Object]bool)
	whiteboxInternalTestObjects = collectWhiteboxInternalTestObjects(internal)

	whiteboxBridgeDeclaredNames = HashSet[string]{}
	if model == testProjectWhiteboxReference {
		whiteboxBridgeDeclaredNames = collectWhiteboxBridgeDeclaredNames(internal)
	}

	// The naming state the PRODUCTION conversion of this package left standing. It ran in this same
	// process moments ago (processConversion converts the production sources, then calls
	// processTestConversion), so its live claims are still here — captured BEFORE the first
	// variant's resetPackageState clears them. Only the INTERNAL variant is seeded with it: its
	// test files emit into the production package class, where those names are already taken.
	// See productionSeed for what each member pins and why. Captured by REFERENCE deliberately:
	// resetPackageState replaces each of these globals with a fresh instance rather than clearing
	// the one in place, so the captured production state stays pristine while the variant claims
	// into its own.
	internalSeed := productionSeed{
		liftedTypeNames:      packageLiftedTypeNames,
		dynamicTypeNames:     packageDynamicTypeNames,
		hoistedConstOrdinals: packageHoistedConstOrdinals,
		globalTempVarCounts:  globalTempVarCount,
		importForces:    packageImportForces,
		initFuncs:            initFuncCounter,
	}

	// The simple type names BOTH variant classes declare (see testAmbiguousLocalTypeNames). Both
	// `using static` directives are in scope in the merged metadata, so these must emit
	// class-qualified. Computed from the loaded variants before either converts, and session-scoped
	// — resetPackageState does not clear it.
	testAmbiguousLocalTypeNames = ambiguousVariantTypeNames(internal, external)

	productionAnchor := metadataClassPrefix(projectNamespace, production.Name)
	internalAnchor := projectNamespace + "." + internalBridgeName
	testAnchor := projectNamespace + "." + testClassName

	testInfoPath := filepath.Join(outputPath, testPackageInfoFileName)

	// Claim the test-info position-map keys for THIS convertTestVariants call: the recompile-model
	// fallback re-invokes the whole function over the same output path, and without the claim every
	// record from the abandoned reference-model attempt would still be standing. Claimed here, once,
	// rather than per variant, because the two variants accumulate into these same files.
	claimPositionMapTarget(testInfoPath)
	claimPositionMapTarget(filepath.Join(outputPath, internalTestPackageInfoFileName))

	if model.referencesProduction() {
		// The reference model must NOT declare the production package class: the production
		// types' single identity is the referenced production assembly, and a local partial
		// declaration (or generated code anchored to one) would re-introduce exactly the
		// duplicate-type shadow the model exists to eliminate. Seed a test-class-only anchor
		// instead of the production package_info.cs.
		seedArgs := []string{}
		if model == testProjectWhiteboxReference {
			seedArgs = append(seedArgs, internalBridgeName)
		}
		seed := referenceModelTestPackageInfoSeed(projectNamespace, testClassName, testPackageName, getSanitizedImport(production.Name+PackageSuffix), seedArgs...)

		if err := os.WriteFile(testInfoPath, []byte(seed), 0644); err != nil {
			return result, fmt.Errorf("seed test package metadata: %w", err)
		}
	} else {
		// Seed package_test_info.cs from the production package_info.cs so the production
		// assembly-level metadata carries over verbatim; each converted variant's ADDITIONS are
		// then merged in by the shared writePackageInfoFile (identical emission semantics to
		// production — pointer-form unwrapping, dedup, pruning — because it IS the production
		// writer).
		// Layout L3: an L3 package's production package_info.cs lives in its per-GOOS folder, and it
		// is the SEED this test conversion's own metadata is merged into — asking flat would fail
		// the "convert the package itself before its tests" check on a package that HAS been
		// converted (design §4.3).
		productionInfoPath := platformPackageInfoPath(outputPath, goosOfTarget(options.targetPlatform))
		productionInfo, err := os.ReadFile(productionInfoPath)
		if err != nil {
			return result, fmt.Errorf("read production package metadata (convert the package itself before its tests): %w", err)
		}

		if err := os.WriteFile(testInfoPath, productionInfo, 0644); err != nil {
			return result, fmt.Errorf("seed test package metadata: %w", err)
		}

		// The production sources recompile into the test assembly, so their imports are test
		// project references too. Under the reference model the production ASSEMBLY carries its
		// own dependencies, and the test project references only what the test files import
		// (plus the alias-scan additions — B2c).
		for importPath := range production.Imports {
			result.allImports.Add(importPath)
		}
	}

	for _, variant := range []*packages.Package{internal, external} {
		if variant == nil {
			continue
		}

		if len(variant.Errors) > 0 {
			return result, fmt.Errorf("test package variant %q failed to load: %v", variant.ID, variant.Errors)
		}

		entries := testFileEntries(variant)
		if len(entries) == 0 {
			continue
		}

		for _, entry := range entries {
			result.includedSources.Add(filepath.Clean(entry.filePath))
		}

		// DISCOVERY runs over EVERY test file (below), so an excluded file's Example/Benchmark
		// declarations still reach the manifest under their disclosed-unsupported status. EMISSION
		// runs over the non-excluded files only — the excluded file's C# is never written and it is
		// never a csproj compile item (Phase-4D file-exclusion ruling, selectCompileExcludedTestFiles).
		emitEntries := make([]FileEntry, 0, len(entries))
		for _, entry := range entries {
			if compileExcluded[filepath.Clean(entry.filePath)] {
				continue
			}
			emitEntries = append(emitEntries, entry)
		}

		capabilities := analyzeTestingCapabilities(variant)
		found, foundMain := discoverTestDeclarations(variant, entries, inputPath, capabilities, supported)
		if model == testProjectWhiteboxReference && variant == internal {
			for i := range found {
				found[i].CSharpClassName = internalBridgeName
			}
			if foundMain != nil {
				foundMain.CSharpClassName = internalBridgeName
			}
		}
		result.declarations = append(result.declarations, found...)

		// Package-level capability reporting aggregates over RUNNABLE declaration kinds only
		// (tests + TestMain) — benchmark/fuzz/example requirements must not block the package,
		// they are excluded-disclosed by their own status (F4: attribution is per-test).
		for _, declaration := range found {
			if declaration.Kind == "test" {
				result.requiredCapabilities.UnionWith(declaration.RequiredCapabilities)
			}
		}

		if foundMain != nil {
			if result.testMain != nil {
				return result, fmt.Errorf("multiple valid TestMain declarations: %s and %s", result.testMain.Source, foundMain.Source)
			}
			result.testMain = foundMain
			result.requiredCapabilities.UnionWith(foundMain.RequiredCapabilities)
		}

		// HAND-OWNED HOST: the internal variant is DISCOVERED and never EMITTED. Everything above
		// this line has already run for it — its files are in includedSources, its declarations are
		// in the manifest with the capability statuses their own analysis assigned — and everything
		// below is emission and the metadata merge that serves emission.
		//
		// Stated as a skip rather than left to fall out of an empty emitEntries list, which the
		// exclusion pass does produce. The two are not equivalent: convertTestVariant does more than
		// walk its entry list, and at least one of those things WRITES. A variant whose analysis
		// records relocated package-var initializers emits `package_init_internal_test.cs` after the
		// convert loop regardless of how many files it converted, and for a host row that lands in
		// the hand-owned directory — the one outcome this whole mode exists to make impossible. An
		// empty list would also leave the variant's collected metadata globals to merge into a
		// project that compiles none of it.
		//
		// So the rule is the accurate one: for a host target there is no internal EMISSION at all,
		// and the code says that instead of arranging for it to happen to produce nothing.
		if options.testHandOwnHost && variant == internal {
			continue
		}

		// The seed is the INTERNAL variant's under the RECOMPILE model alone — that is exactly the
		// case where the production `.cs` are compile items of this assembly and share the emitted
		// class. Under the reference model production is a separate assembly; the external variant
		// has a class of its own. Both may reuse every seeded name.
		var seed productionSeed

		if variant == internal && !model.referencesProduction() {
			seed = internalSeed
		}

		variantOptions := testVariantOptions(options, model, variant == external, internalBridgeName)

		// This variant's position-map records go to the info file of the compilation that compiles
		// its emissions. Every test emission lands in the ONE tests assembly, so any of its info
		// files would satisfy the assembly-scoped lookup; the records follow their ANCHOR, exactly
		// as the GoImplement records do -- the mixed white-box model's internal variant into the
		// bridge unit, everything else into package_test_info.cs.
		variantOptions.positionMapTarget = testInfoPath

		if model == testProjectWhiteboxReference && variant == internal && external != nil {
			variantOptions.positionMapTarget = filepath.Join(outputPath, internalTestPackageInfoFileName)
		}

		variantOutputs, imports, err := convertTestVariant(variant, emitEntries, outputPath, projectNamespace, seed, variantOptions)
		if err != nil {
			return result, err
		}

		// A FUNCTION-LOCAL type declared by an internal _test.go emits under its LIFTED
		// package-level name (`TestEncoderDecoder_r`), which the go/types declared-name scan above
		// cannot know: it sees the Go-source name (`r`). The record split keys on the EMITTED
		// name, so union the internal variant's live lift claims in before the split runs — they
		// are still standing here, the next variant's resetPackageState is what clears them.
		// Without this the record anchors in the EXTERNAL test class and the generator declares a
		// PHANTOM empty type there instead of merging with the bridge's real declaration
		// (encoding/hex: CS0103 on the phantom's missing embed, CS0034 from the phantom's missing
		// TypeGenerator `==`, and CS1503 at the cast site — see splitWhiteboxVariantRecords).
		if model == testProjectWhiteboxReference && variant == internal {
			whiteboxBridgeTypeNames.UnionWithSet(packageLiftedTypeNames)
		}

		// Merge this variant's collected metadata globals while they are still live (the next
		// variant's conversion resets them). Under the RECOMPILE model the EXTERNAL variant's
		// records are split across TWO anchor files (B4/B5): records whose generated code must
		// live in the test package class go to package_info_external_test.cs; production-
		// anchored records stay in package_test_info.cs. Under the REFERENCE model there is a
		// single anchor — the test package class — and a record that would need the production
		// anchor triggers the recompile fallback instead.
		//
		// A nominal generic CONSTRAINT over a production type is the second, non-record reason the
		// reference model can fail to serve a suite — see nominalConstraintsRequireProductionMutation.
		if model == testProjectWhiteboxReference && (recordsRequireProductionMutation(getSanitizedImport(production.Name+PackageSuffix), production.Name) ||
			nominalConstraintsRequireProductionMutation()) {
			return result, errProductionAnchoredRecords
		}

		if model == testProjectWhiteboxReference && external != nil {
			// A MIXED white-box suite has two owning classes in one assembly; each variant's
			// records split between the bridge anchor and the test anchor by declared-name set.
			unitName, err := writeWhiteboxVariantMetadata(testInfoPath, outputPath,
				getSanitizedImport(production.Name+PackageSuffix), internalBridgeName,
				production.Name, internalAnchor, testAnchor, whiteboxBridgeTypeNames, variant == internal)
			if err != nil {
				return result, err
			}

			if unitName != "" && !internalUnitListed {
				result.outputFiles = append(result.outputFiles, unitName)
				internalUnitListed = true
			}
		} else if variant == external {
			if model.referencesProduction() {
				if model == testProjectReference && recordsRequireProductionAnchor(getSanitizedImport(production.Name+PackageSuffix), production.Name, options.testHandOwnHost) {
					return result, errProductionAnchoredRecords
				}

				// Reference model: the seeded package_test_info.cs declares the TEST class as its
				// first — and only — class, so that is its anchor.
				metadataAnchorClassPrefix = testAnchor
				metadataAnchorLocalTypes = true
				writePackageInfoFile(testInfoPath, true)
			} else {
				unitName, err := writeExternalVariantMetadata(testInfoPath, outputPath, production.Name, productionAnchor, testAnchor, options.testHandOwnHost)
				if err != nil {
					return result, err
				}

				if unitName != "" {
					result.outputFiles = append(result.outputFiles, unitName)
				}
			}
		} else {
			// An INTERNAL-ONLY white-box suite has one owning class — the bridge, which is also
			// the seeded first class of package_test_info.cs — so a single anchored write suffices.
			metadataAnchorClassPrefix = productionAnchor
			metadataAnchorLocalTypes = false
			if model == testProjectWhiteboxReference {
				metadataAnchorClassPrefix = internalAnchor
				metadataAnchorLocalTypes = true
			}
			writePackageInfoFile(testInfoPath, true)
		}

		result.outputFiles = append(result.outputFiles, variantOutputs...)
		result.allImports.UnionWithSet(imports)
	}

	// The reference-model seed already declares the attribute-bearing test package class as its
	// first — and only — class; the append is a recompile-model concern (the production-seeded
	// file needs the test class and its widened `using static` scope added).
	if !model.referencesProduction() {
		if err := appendExternalTestPackageClass(testInfoPath, projectNamespace, production.Name, external); err != nil {
			return result, err
		}
	}

	// Resolve the deferred pointer-adapter names for the test outputs against the FINISHED
	// metadata file — read back rather than taken from the last writePackageInfoFile capture,
	// because the variants reach it by different paths (the recompile model's external variant
	// goes through writeExternalVariantMetadata instead) and only the file on disk is what
	// go2cs-gen will actually read. The test closure is where collisions surface at all: its
	// extra casts are what let one struct reach two same-simple-named interfaces.
	if model == testProjectWhiteboxReference {
		// TWO anchor files can exist under white-box; capture both so the pair set is the
		// assembly's full record set and each pair remembers which class its adapter lives in.
		unitPath := filepath.Join(outputPath, internalTestPackageInfoFileName)
		if _, err := os.Stat(unitPath); err == nil {
			captureAdapterPairsFromInfoFile(unitPath, internalBridgeName)
		}
		captureAdapterPairsFromInfoFile(testInfoPath, testClassName)
		resolveAdapterNameMarkers(testAdapterResolveNames, options.testMetadataAnchorName)
	} else {
		captureAdapterPairsFromInfoFile(testInfoPath)
		resolveAdapterNameMarkers(testAdapterResolveNames)
	}

	return result, nil
}

// metadataClassPrefix renders the fully-qualified C# class a converted Go package emits into —
// the anchor a metadata file's bare local type references bind to.
func metadataClassPrefix(namespace, goPackageName string) string {
	return namespace + "." + getSanitizedImport(goPackageName+PackageSuffix)
}

// ambiguousVariantTypeNames returns the simple type names declared by BOTH `-tests` variants: the
// package under test (production + its internal `_test.go` files) and the external `<pkg>_test`
// suite. Both classes are `using static`-imported by the merged metadata, so a bare reference to
// one of these names cannot bind (CS0104) — see testAmbiguousLocalTypeNames. The Go name and its
// core-sanitized C# spelling are both recorded: membership is tested against an EMITTED name, and
// an entry that can never be emitted is inert. Empty unless BOTH variants exist.
func ambiguousVariantTypeNames(internal, external *packages.Package) HashSet[string] {
	ambiguous := HashSet[string]{}

	if internal == nil || external == nil || internal.Types == nil || external.Types == nil {
		return ambiguous
	}

	externalTypeNames := HashSet[string]{}

	for _, name := range external.Types.Scope().Names() {
		if _, ok := external.Types.Scope().Lookup(name).(*types.TypeName); ok {
			externalTypeNames.Add(name)
		}
	}

	for _, name := range internal.Types.Scope().Names() {
		if _, ok := internal.Types.Scope().Lookup(name).(*types.TypeName); !ok {
			continue
		}

		if !externalTypeNames.Contains(name) {
			continue
		}

		ambiguous.Add(name)
		ambiguous.Add(getCoreSanitizedIdentifier(name))
	}

	return ambiguous
}

// referenceModelTestPackageInfoSeed composes package_test_info.cs for a production-reference test
// project. The structure mirrors package_info-template.txt (the shared writer requires all four
// marker sections); the FIRST — and only — class declaration is the test metadata anchor,
// which is where go2cs-gen anchors generated adapters and partials
// (GetFirstClassName), carrying [GoPackage] directly (no second partial exists to make that a
// CS0579). Deliberately absent, versus the recompile model's production-seeded file: the
// production class declaration and every production-anchored record — the referenced production
// assembly already owns them, and a local shadow would duplicate its types.
func referenceModelTestPackageInfoSeed(projectNamespace, testClassName, goPackageName, productionClassName string, additionalStaticClasses ...string) string {
	var b strings.Builder

	b.WriteString("// go2cs metadata anchor for a production-reference test project: the test assembly\r\n")
	b.WriteString("// REFERENCES the colocated production project instead of\r\n")
	b.WriteString("// recompiling its sources, so the production assembly is the single identity for the\r\n")
	b.WriteString("// production types and no production class partial may be declared here. The first —\r\n")
	b.WriteString("// and only — class is the test metadata class the go2cs-gen generators anchor\r\n")
	b.WriteString("// generated adapters and partials to.\r\n")
	b.WriteString(fmt.Sprintf("global using static global::%s.%s;\r\n", projectNamespace, productionClassName))
	for _, className := range additionalStaticClasses {
		// An internal-only suite names the bridge as BOTH the test class and the additional
		// class — the file-scoped `using static` below already imports it, and a second,
		// global import of the same class is CS8933.
		if className != "" && className != productionClassName && className != testClassName {
			b.WriteString(fmt.Sprintf("global using static global::%s.%s;\r\n", projectNamespace, className))
		}
	}
	b.WriteString("\r\n")
	b.WriteString("// <ImportedTypeAliases>\r\n")
	b.WriteString("// </ImportedTypeAliases>\r\n")
	b.WriteString("\r\n")
	b.WriteString("using go;\r\n")

	staticClasses := []string{testClassName}
	seenStatic := HashSet[string]{}
	for _, className := range staticClasses {
		if className == "" || seenStatic.Contains(className) {
			continue
		}
		seenStatic.Add(className)
		b.WriteString(fmt.Sprintf("using static global::%s.%s;\r\n", projectNamespace, className))
	}
	b.WriteString("\r\n")
	b.WriteString("// <ExportedTypeAliases>\r\n")
	b.WriteString("// </ExportedTypeAliases>\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <InterfaceImplementations>\r\n")
	b.WriteString("// </InterfaceImplementations>\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <ImplicitConversions>\r\n")
	b.WriteString("// </ImplicitConversions>\r\n")
	b.WriteString("\r\n")
	b.WriteString(fmt.Sprintf("namespace %s;\r\n", projectNamespace))
	b.WriteString("\r\n")
	b.WriteString(fmt.Sprintf("[GoPackage(\"%s\")]\r\n", goPackageName))
	b.WriteString(fmt.Sprintf("public static partial class %s\r\n{\r\n", testClassName))
	b.WriteString(productionInitForcingHook(projectNamespace, productionClassName))
	b.WriteString("}\r\n")

	return b.String()
}

// packageProductionInitHookMethod names the production-init forcing hook productionInitForcingHook
// emits. Deliberately NOT in the shared symbol table beside PackageTestInitHookMethod: that table
// carries the names BOTH sides need, and `initᴛᴛtests` is there because go2cs-gen's
// PartialStubGenerator must recognize it and never stub it. This hook is emitted whole, with a
// body, so no generator ever sees an unimplemented partial to stub and no C# projection of the
// name is owed. The doubled temp marker keeps it clear of the relocated-package-var method space
// ("init" + marker + <var name>, initOrderOperations.go), and the `production` word keeps it clear
// of both `initᴛᴛtests` and the per-import forcing hooks importInitName builds.
const packageProductionInitHookMethod = "init" + TempVarMarker + TempVarMarker + "production"

// productionInitForcingHook renders the `[GoInit]` method that runs the REFERENCED production
// assembly's module constructor, i.e. the package-under-test's own `init` functions, before
// anything else in the test module runs.
//
// Go's contract is that every `init` in the package - the PRODUCTION files' included - has run
// before the first test does. A converted `init` is `[GoInit]` = .NET's [ModuleInitializer],
// which makes the weaker per-MODULE guarantee writeImportInit already documents: it runs at first
// access to something in its OWN module. Under the two production-REFERENCE models the test
// assembly is a separate module that REFERENCES the production assembly of the SAME Go package,
// so the production `init` runs at the first TOUCH of a production symbol - which may be the
// second test, or the tenth, or never.
//
// The per-import hooks writeImportInit emits do not reach it, and the reason is structural rather
// than an oversight: they are emitted from an import SPEC, and the case that needs forcing here
// has no import to hang one on. An EXTERNAL test file (`package foo_test`) does import the
// package under test and so is already covered by that machinery; an INTERNAL one
// (`package foo`) is IN the package and imports nothing of it, which is exactly the shape
// net/http/pprof has. Its `init` installs the whole /debug/pprof mux, and TestDeltaProfile -
// the one test that goes through a real server rather than calling handlers directly - got a 404
// whenever it ran before any test that touched a production symbol. Measured both directions
// over eight shuffled runs, 100% determined by test order.
//
// Emitting it unconditionally, rather than gating on "does the production package initialize
// anything", is deliberate and follows the same reasoning noInitPseudoPackages already applies:
// running an empty module constructor is a guaranteed no-op, the runtime runs one at most once,
// and this is ONE hook per test project rather than one per import - so the gate would buy
// nothing and could only mis-answer. The RECOMPILE model needs no hook at all and never seeds
// this file: there the production sources ARE compile items of the test assembly, so their
// `[GoInit]`s are already this module's own.
//
// One ordering nuance, stated rather than engineered around: Roslyn orders a module's
// initializers lexically, and the tests csproj sorts its compile items by name, so a test file
// sorting before `package_test_info.cs` has its own `init` run before this hook. That matches the
// guarantee the converter already declines to make ACROSS files of one package (writeImportInit),
// and it does not touch the property this hook exists for - every module initializer runs before
// `Main`, hence before the first test.
//
// The `typeof` target is spelled `global::`-rooted, the same collision-proof form the seed's own
// `global using static` line uses and globalQualifyForcingTarget composes for a shadowed import
// hook, so no name in scope can occlude it.
func productionInitForcingHook(projectNamespace, productionClassName string) string {
	var b strings.Builder

	b.WriteString("    // Go runs every `init` in the package under test - the production files' included -\r\n")
	b.WriteString("    // before the first test. The production package is a REFERENCED assembly here, whose\r\n")
	b.WriteString("    // module constructor .NET would not run until something in it is touched, so that\r\n")
	b.WriteString("    // initialization is forced before anything else in this test module runs.\r\n")
	b.WriteString(fmt.Sprintf("    [GoInit] internal static void %s() {\r\n", packageProductionInitHookMethod))
	b.WriteString(fmt.Sprintf("        builtin.initPackage(typeof(global::%s.%s));\r\n", projectNamespace, productionClassName))
	b.WriteString("    }\r\n")

	return b.String()
}

// collectSiblingTestClosure populates siblingClosureImportPaths with the transitive import closure
// of the package's _test.go variants so package-wide declaration analysis sees the complete test
// assembly. The closure also supplies the production half when mutation forces recompile fallback.
// Declarator names are collected separately and cheaply per package by
// collectSiblingTestFuncMethodNames, including for ordinary conversion, so reference spelling does
// not depend on whether -tests was requested. Metadata load only (no syntax/types for dependencies),
// so it costs a fraction of the LoadAllSyntax pass processTestConversion does later. Best-effort: a
// load failure leaves the closure empty and the production conversion behaves exactly as before —
// processTestConversion reports the real error moments later.
func collectSiblingTestClosure(inputPath string, options Options) {
	siblingClosureImportPaths = nil
	targetParts := strings.Split(options.targetPlatform, "/")
	if len(targetParts) != 2 {
		return
	}

	absolute, err := filepath.Abs(inputPath)
	if err != nil {
		return
	}

	loaded, err := packages.Load(&packages.Config{
		Mode:       packages.NeedName | packages.NeedImports | packages.NeedDeps | packages.NeedCompiledGoFiles,
		Dir:        absolute,
		Tests:      true,
		BuildFlags: options.loaderBuildFlags(),
		Env: append(os.Environ(),
			fmt.Sprintf("GOOS=%s", targetParts[0]),
			fmt.Sprintf("GOARCH=%s", targetParts[1])),
	}, ".")

	if err != nil {
		return
	}

	closure := HashSet[string]{}

	var walk func(pkg *packages.Package)

	walk = func(pkg *packages.Package) {
		for path, imported := range pkg.Imports {
			if closure.Contains(path) {
				continue
			}

			closure.Add(path)
			walk(imported)
		}
	}

	// Only the TEST variants contribute: the production package's own closure is already walked by
	// computeImportAliasRenames from the loaded types, and the synthetic `<pkg>.test` main package
	// is not part of the emitted assembly.
	for _, pkg := range loaded {
		if !strings.Contains(pkg.ID, "[") {
			continue
		}

		walk(pkg)
	}

	siblingClosureImportPaths = closure.Keys()
	sort.Strings(siblingClosureImportPaths)
}

func findProductionPackage(pkgs []*packages.Package, inputPath string) *packages.Package {
	for _, pkg := range pkgs {
		if pkg.Name == "main" || strings.Contains(pkg.ID, "[") {
			continue
		}

		if samePath(pkg.Dir, inputPath) {
			return pkg
		}
	}

	return nil
}

func findTestVariants(pkgs []*packages.Package, production *packages.Package) (internal, external *packages.Package) {
	testID := production.PkgPath + ".test]"

	for _, pkg := range pkgs {
		if !strings.HasSuffix(pkg.ID, testID) || !strings.Contains(pkg.ID, "[") {
			continue
		}

		switch {
		case pkg.PkgPath == production.PkgPath && pkg.Name == production.Name:
			internal = pkg
		case pkg.Name == production.Name+"_test":
			external = pkg
		}
	}

	return internal, external
}

func testFileEntries(pkg *packages.Package) []FileEntry {
	entries := make([]FileEntry, 0)

	for i, file := range pkg.Syntax {
		if i >= len(pkg.CompiledGoFiles) {
			break
		}

		path := pkg.CompiledGoFiles[i]
		if strings.HasSuffix(strings.ToLower(filepath.Base(path)), "_test.go") {
			entries = append(entries, newFileEntry(file, path, false))
		}
	}

	return entries
}

// The manifest TestSources status/reason for a _test.go file dropped from the compile set by the
// Phase-4D Example/Benchmark-only file-exclusion policy (selectCompileExcludedTestFiles). Distinct
// from "platform-excluded" (a file build-constraints deselect): this file WAS selected for the
// target but declares nothing the run registry admits, so its compilation is deferred alongside
// its declarations' execution.
const (
	compileExcludedSourceStatus = "example-benchmark-only"
	compileExcludedSourceReason = "file declares only Phase-4D-deferred Example/Benchmark functions; its compilation is deferred to Phase 4D along with their execution"
)

// isPhase4DExcludedTestFunc reports whether a top-level function declaration is a Phase-4D-deferred
// Example or Benchmark — the EXACT classification discoverTestDeclarations applies for its
// "example"/"benchmark" (status "unsupported") cases: no receiver, no results, no type params, and
// either a zero-parameter func Example* or a single-*testing.B-parameter func Benchmark*. A
// Test/TestMain/Fuzz func, a method, or a mis-signatured Example/Benchmark returns false. TestMain
// and Fuzz are DELIBERATELY not treated as excluded here — the ruling scopes the file predicate to
// Example/Benchmark (conservative by design), so a file declaring either stays in the compile set.
func isPhase4DExcludedTestFunc(fn *ast.FuncDecl, info *types.Info) bool {
	if fn.Recv != nil || fn.Name == nil {
		return false
	}

	obj, ok := info.Defs[fn.Name].(*types.Func)
	if !ok {
		return false
	}

	sig, ok := obj.Type().(*types.Signature)
	if !ok || sig.TypeParams().Len() != 0 || sig.Results().Len() != 0 {
		return false
	}

	name := fn.Name.Name

	if sig.Params().Len() == 0 {
		return isGoTestName(name, "Example")
	}

	return sig.Params().Len() == 1 && isGoTestName(name, "Benchmark") && isTestingPointer(sig.Params().At(0).Type(), "B")
}

// testFileExclusionInfo holds the go/types facts the Phase-4D file-exclusion predicate needs for
// one _test.go file: whether every top-level declaration it contributes is a Phase-4D-deferred
// Example/Benchmark function (condition 1), the objects it declares (the reference targets of
// condition 2), and every object it references (so a candidate promoted back to RETAINED can, in
// turn, pull a file IT references back into the compile set — the condition-2 fixpoint).
type testFileExclusionInfo struct {
	path      string
	qualifies bool                  // condition (1): declares only Example/Benchmark functions
	declared  []types.Object        // top-level objects the file declares
	used      map[types.Object]bool // objects the file references (go/types Uses)
}

// classifyTestFileForExclusion evaluates condition (1) for one test file and captures the go/types
// objects condition (2) needs. A file qualifies when every RUNNABLE declaration it contributes is a
// Phase-4D-deferred Example/Benchmark function, plus — since the crypto/tls measurement, 2026-08-15 —
// the pure TYPE declarations and METHODS such a function needs to express itself.
//
// Why types and methods joined, and why nothing else did. The original predicate accepted a file
// whose declarations were EXCLUSIVELY Example/Benchmark funcs, which is the shape go/token's
// example_test.go happens to have. crypto/tls's is the same file in every way that matters — it is
// the package's ONLY black-box file and every runnable thing in it is an Example — except that its
// Examples need an io.Reader to hand `Config.Rand`, so it declares `type zeroSource struct{}` and one
// `Read` method on it. That one helper type kept the whole file compiled, and under the recompile
// model a compiled black-box Example is exactly what the ruling exists to prevent: `http.Transport`'s
// `TLSClientConfig` field names `tls_package.Config` in the PRODUCTION assembly while the test
// assembly recompiles its own, so the field is unnameable — CS0012 ×3 at example_test.cs 88/99/198,
// two of `crypto/tls`'s four build errors. Adding the production reference cannot fix it (the two
// `Config`s stay distinct types and CS0012 merely becomes CS0029), so the file must not be compiled.
//
// A type declaration and its methods are admissible because they have no RUN-TIME behavior of their
// own: nothing executes at package init, and any use by a retained file is a reference condition (2)
// already resolves by go/types object identity — which is why the type and method objects are now
// recorded in `declared`, without which widening condition (1) would silently disarm condition (2).
// Everything else stays disqualifying, deliberately: a `var`/`const` initializer can carry side
// effects, and a plain helper func can be `init()`, neither of which any reference edge would reveal.
func classifyTestFileForExclusion(file *ast.File, info *types.Info, path string) *testFileExclusionInfo {
	result := &testFileExclusionInfo{path: path, used: make(map[types.Object]bool)}

	qualifies := true
	hasExcludedFunc := false

	declare := func(name *ast.Ident) {
		if name == nil {
			return
		}

		if object := info.Defs[name]; object != nil {
			result.declared = append(result.declared, object)
		}
	}

	for _, decl := range file.Decls {
		switch typed := decl.(type) {
		case *ast.GenDecl:
			if typed.Tok == token.IMPORT {
				continue // imports are not declarations for this predicate
			}

			if typed.Tok != token.TYPE {
				qualifies = false // a top-level var/const disqualifies the file
				continue
			}

			// A pure type declaration is admissible; record it so condition (2) can see a
			// retained file's reference to it.
			for _, spec := range typed.Specs {
				if typeSpec, ok := spec.(*ast.TypeSpec); ok {
					declare(typeSpec.Name)
				}
			}
		case *ast.FuncDecl:
			switch {
			case isPhase4DExcludedTestFunc(typed, info):
				hasExcludedFunc = true
				declare(typed.Name)
			case typed.Recv != nil:
				// A method on a file-local type: admissible with its receiver type, and
				// recorded for the same condition-(2) reason.
				declare(typed.Name)
			default:
				qualifies = false // a Test/TestMain/Fuzz func, an init, or a plain helper disqualifies
			}
		default:
			qualifies = false
		}
	}

	result.qualifies = qualifies && hasExcludedFunc

	// Every referenced object, for the condition-(2) fixpoint. Collected for ALL files: a retained
	// file's references are the exclusion driver, and a candidate's are needed once it is promoted.
	// Defining idents resolve through Defs (not Uses) and are skipped, so a file's own Example name
	// does not count as a reference to itself.
	ast.Inspect(file, func(node ast.Node) bool {
		if ident, ok := node.(*ast.Ident); ok {
			if object := info.Uses[ident]; object != nil {
				result.used[object] = true
			}
		}
		return true
	})

	return result
}

// selectCompileExcludedTestFiles applies the user-approved Phase-4D file-exclusion ruling
// ("option a", 2026-07-24): a _test.go file is dropped from the -tests conversion/compile set iff
//
//	(1) every RUNNABLE declaration it contributes is a Phase-4D-deferred declaration — the file
//	    declares at least one func Example* / func Benchmark* and, apart from those, only pure TYPE
//	    declarations and METHODS (imports do not count as declarations; any var/const, or any other
//	    plain func — a Test/TestMain/Fuzz func, an init, or a mis-signatured Example/Benchmark —
//	    disqualifies the file, conservative by design; see classifyTestFileForExclusion for why the
//	    type/method admission is safe and why nothing beyond it is), AND
//	(2) no RETAINED test file references any object the file declares (resolved by go/types object
//	    identity across the loaded variant set, never by filename or text).
//
// Phase-4D already excludes Example/Benchmark DECLARATIONS from the run registry uniformly, so a
// file that contributes nothing to the run contributes nothing to the compile. This unblocks the
// compile-poisoning external Example-only files (go/token's example_test.go, whose whitebox+blackbox
// recompile drags cross-assembly type identity into CS0012) WITHOUT touching the differential
// oracle: discovery is left intact, so the excluded file's declarations still appear in the manifest
// under their existing disclosed-unsupported status — the F6 census gate stays truthful and every
// already-filtered Example/Benchmark stays filtered. Only the file's EMISSION and csproj
// compile-membership are dropped. Returns the set of cleaned file paths to exclude.
// classifyTestFiles evaluates every variant's _test.go files once, returning them in walk order and
// keyed by cleaned path. Shared by the Phase-4D exclusion below and by the hand-owned-host exclusion
// beside it so the two rules read the SAME classification — two independent walks of the same files
// is the shape that drifts apart without a conflict.
func classifyTestFiles(variants ...*packages.Package) ([]*testFileExclusionInfo, map[string]*testFileExclusionInfo) {
	infos := make([]*testFileExclusionInfo, 0)
	byPath := make(map[string]*testFileExclusionInfo)

	for _, variant := range variants {
		if variant == nil {
			continue
		}

		for i, file := range variant.Syntax {
			if i >= len(variant.CompiledGoFiles) {
				break
			}

			path := variant.CompiledGoFiles[i]
			if !strings.HasSuffix(strings.ToLower(filepath.Base(path)), "_test.go") {
				continue
			}

			cleaned := filepath.Clean(path)
			if _, seen := byPath[cleaned]; seen {
				continue
			}

			info := classifyTestFileForExclusion(file, variant.TypesInfo, cleaned)
			infos = append(infos, info)
			byPath[cleaned] = info
		}
	}

	return infos, byPath
}

func selectCompileExcludedTestFiles(variants ...*packages.Package) map[string]bool {
	infos, byPath := classifyTestFiles(variants...)

	// Seed the excluded set with every condition-(1) qualifier, then relax it: a qualifier a
	// RETAINED file references must stay compiled (condition 2). Promotion is a fixpoint — a
	// newly-retained file's own references can pull further qualifiers back in — over a set that
	// only ever shrinks, so it converges.
	excluded := make(map[string]bool)
	for _, info := range infos {
		if info.qualifies {
			excluded[info.path] = true
		}
	}

	for changed := true; changed; {
		changed = false

		usedByRetained := make(map[types.Object]bool)
		for _, info := range infos {
			if excluded[info.path] {
				continue
			}
			for object := range info.used {
				usedByRetained[object] = true
			}
		}

		for path := range excluded {
			for _, object := range byPath[path].declared {
				if usedByRetained[object] {
					delete(excluded, path)
					changed = true
					break
				}
			}
		}
	}

	return excluded
}

// The manifest TestSources status/reason for a _test.go file dropped from the compile set because
// the package under test is a HAND-OWNED HOST. Distinct from the Phase-4D status beside it: nothing
// about these declarations is deferred — their subject is a representation the host structurally
// replaced, so there is no C# symbol for the assertion to name and never will be.
const (
	handOwnHostExcludedSourceStatus = "hand-own-host-internal"
	handOwnHostExcludedSourceReason = "the package under test is a hand-owned host, so its INTERNAL test variant asserts against Go's own unexported state machine (common, matcher, chattyPrinter, tRunner) — a representation the host replaces rather than implements, leaving no symbol for the assertion to name"
)

// markHandOwnHostExcludedTestFiles adds to excluded every _test.go a hand-owned-host row cannot
// compile, and it propagates in the OPPOSITE DIRECTION from the Phase-4D rule above.
//
// Phase-4D relaxes: a deferred file that a RETAINED file references is pulled back into the compile
// set, because the referencing file is the one that matters. Here the internal variant can never be
// compiled — its universe is the replaced representation — so the exclusion is unconditional and it
// is the REFERENCING file that must go instead. `testing`'s own suite is the worked example:
// export_test.go (internal) publishes `PrettyPrint`/`HighPrecisionTime` as aliases of unexported
// symbols the host does not have, and benchmark_test.go (EXTERNAL) reads `testing.PrettyPrint` — so
// excluding the internal variant alone leaves the external half unable to compile, which is exactly
// the state a prior census measured and read as "bucket D cannot compile at all".
//
// The propagation is a fixpoint over a set that only ever GROWS: a newly excluded external file's
// own declarations may in turn be referenced by another external file. It terminates because each
// pass either adds a file or stops, and the file set is finite.
//
// Every excluded file's DECLARATIONS still reach the manifest — discovery runs over the full entry
// list and only emission is filtered (convertTestVariants) — so the F6 census still accounts for
// every name `go test` produces, each with the capability status its own analysis assigned.
func markHandOwnHostExcludedTestFiles(internal, external *packages.Package, excluded map[string]bool) map[string]bool {
	added := map[string]bool{}

	if internal == nil {
		return added
	}

	_, byPath := classifyTestFiles(internal, external)

	// The DECLARED set is collected here rather than reused from classifyTestFileForExclusion,
	// because that one is Phase-4D-shaped and cannot serve this rule. It records TYPE declarations,
	// Example/Benchmark funcs and methods — and a top-level VAR does not merely go unrecorded there,
	// it sets `qualifies = false` and is skipped, which is exactly right for a predicate whose whole
	// question is "does this file declare only deferred functions".
	//
	// MEASURED, and it is why this exists rather than a one-line reuse: `testing`'s export_test.go
	// declares `var PrettyPrint = prettyPrint` — a VAR — and benchmark_test.go (external) reads
	// `testing.PrettyPrint`. With the Phase-4D declared set the propagation could not see that edge
	// at all, and a convert-only probe emitted benchmark_test.cs against a symbol the host does not
	// have. Two of the three names export_test.go publishes are vars and only HighPrecisionTime is
	// a type, so a type-shaped view of that file misses most of what it exports.
	//
	// The USED set is reused, because it is already complete: classifyTestFileForExclusion walks
	// every ident through TypesInfo.Uses with no kind filter at all.
	declaredByPath := map[string][]types.Object{}

	collectDeclared := func(variant *packages.Package) {
		if variant == nil {
			return
		}

		for i, file := range variant.Syntax {
			if i >= len(variant.CompiledGoFiles) {
				break
			}

			path := filepath.Clean(variant.CompiledGoFiles[i])

			if !strings.HasSuffix(strings.ToLower(filepath.Base(path)), "_test.go") {
				continue
			}

			if _, seen := declaredByPath[path]; seen {
				continue
			}

			objects := make([]types.Object, 0)

			declare := func(name *ast.Ident) {
				if name == nil {
					return
				}

				if object := variant.TypesInfo.Defs[name]; object != nil {
					objects = append(objects, object)
				}
			}

			for _, decl := range file.Decls {
				switch typed := decl.(type) {
				case *ast.GenDecl:
					if typed.Tok == token.IMPORT {
						continue
					}

					for _, spec := range typed.Specs {
						switch valueOrType := spec.(type) {
						case *ast.TypeSpec:
							declare(valueOrType.Name)
						case *ast.ValueSpec:
							for _, name := range valueOrType.Names {
								declare(name)
							}
						}
					}
				case *ast.FuncDecl:
					declare(typed.Name)
				}
			}

			declaredByPath[path] = objects
		}
	}

	collectDeclared(internal)
	collectDeclared(external)

	for _, path := range internal.CompiledGoFiles {
		if !strings.HasSuffix(strings.ToLower(filepath.Base(path)), "_test.go") {
			continue
		}

		excluded[filepath.Clean(path)] = true
		added[filepath.Clean(path)] = true
	}

	if external == nil {
		return added
	}

	for changed := true; changed; {
		changed = false

		// Objects declared by anything already excluded. Recomputed each pass so a file excluded
		// by the previous pass contributes its own declarations to the next.
		gone := make(map[types.Object]bool)
		for path := range excluded {
			for _, object := range declaredByPath[path] {
				gone[object] = true
			}
		}

		for path, info := range byPath {
			if excluded[path] {
				continue
			}

			for object := range info.used {
				if gone[object] {
					excluded[path] = true
					added[path] = true
					changed = true
					break
				}
			}
		}
	}

	return added
}

// seedProductionAliasLifts makes the production conversion's package-scope ALIAS LIFTS reachable
// from the test compilation — both halves of "reachable", which is why they are seeded together.
//
// Go's `type CorpusEntry = struct{Parent string; Path string; …}` (internal/fuzz's fuzz.go) has no
// C# spelling of its own, so the production conversion LIFTS the anonymous struct to a real nested
// type and reaches it through a compilation-scoped `global using CorpusEntry = …CorpusEntryᴛ1;`.
// `global using` is scoped to ONE compilation and a reference-model test project is a second one,
// so neither the name nor the lift crossed: every test-side reference fell through to `t.String()`
// and emitted raw GO syntax into a C# file — `Func<struct{Parent string; …}, error>` in
// minimize_test.cs and worker_test.cs, CS1031/CS1525/CS1003, with all 52 of internal/fuzz's
// verdicts behind it.
//
// The production package's OWN package_info.cs is the authority for both halves: it publishes
// `[assembly: GoTypeAlias("CorpusEntry", "go.@internal.fuzz_package.CorpusEntryᴛ1")]`, which gives
// the exact target the production compilation uses. Recording it in importedTypeAliases re-emits
// the `global using` into the test metadata file, and recording the TYPE in
// productionAliasLiftedTypes makes every renderer spell the alias (see liftedNameFor).
//
// Two kinds of alias are seeded, and they need DIFFERENT halves of "reachable":
//
//  1. An ANONYMOUS struct/interface RHS has no other spelling at all, so both halves are seeded —
//     the `global using` AND the type→name record that makes every renderer spell the alias.
//
//  2. A NAMED RHS (`type FuncMap = template.FuncMap`, html/template) needs the NAME half only. The
//     original narrowing skipped it entirely, on the reasoning that "a named RHS already renders
//     through its own qualified name". Measured, that does not hold: the alias is declared as a
//     `global using FuncMap = go.text.template_package.FuncMap;` in the PRODUCTION FILE that
//     declares it (template.cs), the renderer spells the bare alias name in production and test
//     alike, and a reference-model test project compiles `*_test.cs` only — so the declaring file
//     is not in that compilation and the name resolves nowhere. CS0246 ×6 across clone_test,
//     escape_test and exec_test, with all 243 of html/template's verdicts behind it. The type
//     record is deliberately NOT seeded for this kind: the RHS has its own qualified spelling and
//     is already rendered through it, so recording it would re-spell references that already
//     compile.
//
// And only an alias the production package_info PUBLISHES is seeded, so a type is never rendered
// under a name the test compilation cannot resolve; an unexported alias to an anonymous struct
// publishes nothing and keeps the pre-existing route. That precondition is also what bounds the
// named-RHS set to Go's EXPORTED type aliases — a Δ-renamed defined type is not an alias at all
// (IsAlias is false) and never reaches here.
func seedProductionAliasLifts(pkg *packages.Package, productionInfoPath string) {
	if pkg == nil || pkg.Types == nil {
		return
	}

	published, err := parseExportedTypeAliases(productionInfoPath)

	if err != nil || len(published) == 0 {
		return
	}

	targets := make(map[string]string, len(published))

	for _, entry := range published {
		targets[entry[0]] = entry[1]
	}

	scope := pkg.Types.Scope()

	for _, name := range scope.Names() {
		typeName, ok := scope.Lookup(name).(*types.TypeName)

		if !ok || !typeName.IsAlias() {
			continue
		}

		target, published := targets[name]

		if !published {
			continue
		}

		// A test file's own alias lifts normally in THIS conversion; only the production
		// declarations are missing one.
		if strings.HasSuffix(pkg.Fset.Position(typeName.Pos()).Filename, "_test.go") {
			continue
		}

		resolved := types.Unalias(typeName.Type())

		// recordType distinguishes the two kinds above: an anonymous RHS needs every renderer
		// redirected onto the alias name, a named one only needs that name to RESOLVE.
		recordType := true

		switch underlying := resolved.(type) {
		case *types.Struct:
			if isEmptyStructType(underlying) {
				continue
			}
		case *types.Interface:
			if underlying.Empty() {
				continue
			}
		case *types.Named:
			recordType = false
		default:
			continue
		}

		packageLock.Lock()

		if recordType {
			if productionAliasLiftedTypes == nil {
				productionAliasLiftedTypes = map[types.Type]string{}
			}

			productionAliasLiftedTypes[resolved] = name
		}

		importedTypeAliases[name] = target
		packageLock.Unlock()
	}
}

// seedProductionDynamicTypeLifts makes the production conversion's purely-anonymous (no Go-level
// alias) struct/interface lifts reachable from the test compilation — the sibling of
// seedProductionAliasLifts for the registrations GoTypeAlias/exportedTypeAliases does not cover.
//
// `export_test.go` (an INTERNAL test file — package runtime, not runtime_test — whose whole reason
// to exist is exposing an already-lifted production field or var for a test to read) references an
// anonymous type production's OWN conversion already gave a name like `ifaceHash_i` or
// `pageAlloc_scav`. Under the reference model that internal file converts as a SEPARATE bridge
// compilation unit, so nothing here re-visits alg.go/mgcsweep.go/mgc.go and nothing re-registers
// the lift — deferredDynamicTypeName/dynamicStructTypeName (dynamicTypeOperations.go) find no
// per-file claim and no package-registry entry (packageDynamicTypeNames was reset for this pass)
// and fall to a deferred marker the post-barrier resolution ALSO cannot resolve, since it consults
// the very same reset registry. The result: the raw Go type signature emitted as unbuildable C#,
// and on the `-tests` path, the W2b gate failing the whole conversion (docs/phase4/
// CENSUS-runtime-first-contact.md, W2a).
//
// Unlike seedProductionAliasLifts this needs no go/types scope walk: productionDynamicTypeNames is
// keyed by structural SIGNATURE, exactly what GoDynamicTypeLift already publishes, so the parsed
// records populate it directly.
func seedProductionDynamicTypeLifts(productionInfoPath string) {
	published, err := parseDynamicTypeLifts(productionInfoPath)

	if err != nil || len(published) == 0 {
		return
	}

	packageLock.Lock()

	if productionDynamicTypeNames == nil {
		productionDynamicTypeNames = map[string]string{}
	}

	for _, entry := range published {
		productionDynamicTypeNames[entry[0]] = entry[1]
	}

	packageLock.Unlock()
}

// seedProductionInterfaceAliases makes the production conversion's DEFINED-OVER-INTERFACE types
// reachable from the test compilation — the second kind of package-level declaration that has a
// `global using` and no class member, alongside the anonymous-RHS alias lifts above.
//
// `type Token any` (encoding/xml) and `type Reader io.Reader` are DEFINED types in Go: each has its
// own identity and its own name. But each also has EXACTLY the right-hand interface's method set and
// can carry no methods of its own, so visitTypeSpec emits it as a compilation-scoped
// `global using ΔToken = object;` rather than as a member of the `<pkg>_package` class (see the
// definedOverInterface arm — a struct wrapper over `any` admits no implicit conversion from a
// concrete value, so the wrapper form was CS0029 at every assignment).
//
// A `-tests` conversion under a REFERENCE model is a SECOND compilation that declares no such alias,
// and its renderers reach the type as an ordinary production named type:
// `global::go.encoding.xml_package.ΔToken`, which qualifies an assembly-scoped alias as a type
// member. That is CS0426 — 36 of them in encoding/xml, its ONLY build error, with all 386 of the
// package's verdicts behind it. The production conversion never produces that spelling because it
// references the alias BARE, which is why the defect is invisible outside a `-tests` run.
//
// Both halves are seeded, exactly as seedProductionAliasLifts seeds them: the NAME into
// productionAliasLiftedTypes so every renderer spells the alias (liftedNameFor is consulted ahead of
// the white-box class qualifiers), and the TARGET into importedTypeAliases so the `global using` is
// re-emitted into the test metadata file and the name resolves there. Recording one without the
// other would render a name the test compilation cannot bind.
//
// The production package_info.cs is the authority for both, and its TWO-HOP chain is followed to the
// end — `GoTypeAlias("Token", "ΔToken")` then `GoTypeAlias("ΔToken", "object")`, because a type whose
// name collides with a method name is Δ-renamed and the alias the production compilation actually
// declares is the renamed one. This is the same chain a cross-package consumer already follows
// (loadImportedTypeAliases' localAliases), so the two readers of one published record agree.
//
// Excluded: the RECOMPILE model, for the one reason that matters — there the production `.cs` ARE
// compile items of the test assembly, so the alias is already declared in this compilation and
// re-declaring it would be the defect rather than the fix. Gated on testProductionPath, which is set
// only by the models that REFERENCE production.
func seedProductionInterfaceAliases(pkg *packages.Package, productionInfoPath string, options Options) {
	if pkg == nil || pkg.Types == nil || options.testProductionPath == "" {
		return
	}

	names := definedOverInterfaceTypeNames(pkg)

	if len(names) == 0 {
		return
	}

	published, err := parseExportedTypeAliases(productionInfoPath)

	if err != nil || len(published) == 0 {
		return
	}

	targets := make(map[string]string, len(published))

	for _, entry := range published {
		targets[entry[0]] = entry[1]
	}

	scope := pkg.Types.Scope()

	for _, name := range names {
		typeName, ok := scope.Lookup(name).(*types.TypeName)

		if !ok || typeName.IsAlias() {
			continue
		}

		named, isNamed := typeName.Type().(*types.Named)

		if !isNamed {
			continue
		}

		aliasName, target, resolved := followPublishedAliasChain(targets, name)

		if !resolved {
			continue
		}

		packageLock.Lock()

		if productionAliasLiftedTypes == nil {
			productionAliasLiftedTypes = map[types.Type]string{}
		}

		productionAliasLiftedTypes[named] = aliasName
		importedTypeAliases[aliasName] = target
		packageLock.Unlock()
	}
}

// definedOverInterfaceTypeNames returns the package-level type names the PRODUCTION files declare as
// a defined type over a NAMED interface — visitTypeSpec's definedOverInterface predicate, read from
// the same syntax that pass reads it from.
//
// The predicate needs the AST and cannot be recovered from go/types: `type X any` and
// `type X interface{}` are the same *types.Named over the same empty *types.Interface, yet the first
// emits a `global using` and the second emits a C# interface that IS a class member. Only the
// right-hand SYNTAX separates them, and convertTestVariant's package carries every production file's
// syntax because the whole variant feeds the package-wide analyses.
//
// `_test.go` declarations are excluded: a test file's own alias emits its `global using` into THIS
// compilation and needs no seeding.
func definedOverInterfaceTypeNames(pkg *packages.Package) []string {
	if pkg == nil || pkg.Types == nil || pkg.Fset == nil {
		return nil
	}

	scope := pkg.Types.Scope()
	names := []string{}

	for _, file := range pkg.Syntax {
		if strings.HasSuffix(strings.ToLower(pkg.Fset.Position(file.Pos()).Filename), "_test.go") {
			continue
		}

		for _, decl := range file.Decls {
			genDecl, isGen := decl.(*ast.GenDecl)

			if !isGen || genDecl.Tok != token.TYPE {
				continue
			}

			for _, spec := range genDecl.Specs {
				typeSpec, isType := spec.(*ast.TypeSpec)

				if !isType || typeSpec.Assign.IsValid() {
					continue
				}

				switch typeSpec.Type.(type) {
				case *ast.Ident, *ast.SelectorExpr:
				default:
					continue
				}

				obj, isTypeName := scope.Lookup(typeSpec.Name.Name).(*types.TypeName)

				if !isTypeName || obj.Type() == nil {
					continue
				}

				if _, isInterface := obj.Type().Underlying().(*types.Interface); isInterface {
					names = append(names, typeSpec.Name.Name)
				}
			}
		}
	}

	return names
}

// followPublishedAliasChain resolves a Go type name through the production package_info.cs's
// exported-alias records to the alias name that compilation DECLARES and that alias's target.
//
// A type whose name collides with a method name publishes TWO records — the rename
// (`"Token"` → `"ΔToken"`) and the alias itself (`"ΔToken"` → `"object"`) — so the name to spell is
// the last key in the chain, never the first. A type with no collision publishes one record and the
// chain ends immediately. The visited set bounds a malformed or self-referential published set,
// which is read from a file this run did not necessarily write.
func followPublishedAliasChain(targets map[string]string, name string) (string, string, bool) {
	target, published := targets[name]

	if !published {
		return "", "", false
	}

	visited := NewHashSet([]string{name})

	for {
		next, chained := targets[target]

		if !chained || visited.Contains(target) {
			return name, target, true
		}

		visited.Add(target)
		name, target = target, next
	}
}

// productionSeed carries the naming state the PRODUCTION conversion of this package left standing
// when it finished, so a `-tests` variant that emits into the SAME `<pkg>_package` class can
// continue from it rather than start over.
//
// Every member exists for one reason: under the RECOMPILE model the production `.cs` on disk are
// compile items of the test assembly and are NOT regenerated, so every package-scope name they
// already declare is immutable. A converter counter or claim set that restarts for the test
// emission pass re-mints one of those names and the class declares it twice. The seed is therefore
// the INTERNAL variant's alone (its files land in the production class); the EXTERNAL variant's
// `<pkg>_test_package` is a separate scope that may reuse every one of these names freely, and
// seeding it would only churn names for no compile-level reason.
//
// A zero value is the "no production half to continue from" case — an external variant, the
// reference model (production is a separate ASSEMBLY there, so nothing it declares can collide),
// and every direct unit-test call.
type productionSeed struct {
	// liftedTypeNames — the anonymous-struct/interface lifts already nested in the class.
	liftedTypeNames HashSet[string]

	// dynamicTypeNames — production's packageDynamicTypeNames (signature -> lifted name) for a
	// purely-anonymous struct/interface with no Go-level alias declaration. See
	// productionDynamicTypeNames (packageGlobalState.go) for why this is a separate registry from
	// liftedTypeNames (a name-collision SET, not a signature->name MAP) and from
	// productionAliasLiftedTypes (aliases only).
	dynamicTypeNames map[string]string

	// hoistedConstOrdinals — per Go const name, the `<name>ᶜ[ordinal]` big-constant fields claimed.
	hoistedConstOrdinals map[string]int

	// globalTempVarCounts — the package-scope generated-name counters (getGlobalTempVarName): the
	// blank identifier `_` (a blank package var, const or func becomes `_ᴛNʗ` / `_ᴛN`, since C#
	// has no package-scope discard) and the hidden `tupleᴛNʗ` holders. crypto/x509's pem_decrypt.cs
	// declares `_ᴛ1ʗ` for a blank const in an iota block, and oid_test.cs's
	// `var _ encoding.BinaryMarshaler = OID{}` re-minted the very same name into the very same
	// class — CS0102, one of the three roots that stood between that package and any verdict at all.
	globalTempVarCounts map[string]int

	// importForces — the imported paths a `[GoInit] initᴛᴛimportꓸ…` hook was already emitted for.
	// The hook forces an imported package's module constructor and is idempotent by construction:
	// exactly one per (assembly, imported package). A test file repeating a production import is
	// the ordinary case, not an exotic one — `x509.go` and `x509_test.go` both blank-import
	// `crypto/sha256` and `crypto/sha512`, and each half emitted the same hook into the same
	// partial class (CS0111 ×2). Since the hook covers NAMED imports too, that overlap is now the
	// rule rather than the exception: nearly every test file re-imports something its production
	// half already forced. The PRODUCTION half owns the hook whenever its file is in the
	// compilation, because that file is the one this run cannot rewrite.
	importForces HashSet[string]

	// initFuncs — how many Go `func init()` declarations the class already carries. Go allows any
	// number per package and C# needs a distinct name for each, so the first takes `init` and the
	// rest `initΔN`. The counter restarting for the test emission pass gives the test half's own
	// `func init()` the bare `init` a production file already declares: `crypto/x509`'s
	// windows/root_windows.cs and x509_test.go, CS0111 again. Same shape as globalTempVarCounts —
	// a per-class name supply that one emission pass must not restart.
	initFuncs int
}

// convertTestVariant converts one test package variant's _test.go files into C# in outputPath.
// The whole variant (production + test files) feeds the package-wide analyses so the test files
// convert with complete state, but only the test files are EMITTED here. The production .cs already
// exist from normal conversion and are either referenced or included later by recompile fallback.
//
// Files convert SEQUENTIALLY in pkg.Syntax order for byte-reproducible output, mirroring
// processConversion (the per-file visitors share package-level state claimed at visit time; the
// branch's concurrent goroutines reproduced exactly the nondeterminism master removed).
func convertTestVariant(pkg *packages.Package, testEntries []FileEntry, outputPath, projectNamespace string, seed productionSeed, options Options) ([]string, HashSet[string], error) {
	resetPackageState(pkg)
	packageNamespace = projectNamespace

	// The lifted type names the production conversion already claimed (see
	// productionLiftedTypeNames). Non-nil for the INTERNAL variant only — its test files emit into
	// the production `<pkg>_package` class, whose on-disk `.cs` are not regenerated here, so a lift
	// that reuses one of those names declares the nested type twice.
	productionLiftedTypeNames = seed.liftedTypeNames

	// Same production-pinned seeding for purely-anonymous types (no Go-level alias) that
	// production's own conversion already lifted — see productionDynamicTypeNames.
	productionDynamicTypeNames = seed.dynamicTypeNames

	// Same production-pinned seeding for the hoisted big-constant field ordinals (see
	// productionHoistedConstOrdinals); claimHoistedConstFieldName folds it in on first claim.
	productionHoistedConstOrdinals = seed.hoistedConstOrdinals

	// The counters and claim sets that have no production-pinned mirror of their own are seeded
	// straight into the live state resetPackageState just cleared: nothing else reads them, so a
	// second global would carry no information the live set cannot. All three say the same thing —
	// this emission pass CONTINUES the production one rather than restarting it (see productionSeed).
	for prefix, count := range seed.globalTempVarCounts {
		globalTempVarCount[prefix] = count
	}

	for importPath := range seed.importForces {
		packageImportForces.Add(importPath)
	}

	initFuncCounter = seed.initFuncs

	// The package under test is RECOMPILED into this assembly, so a record naming one of its types
	// through its fully-qualified class (how an external `<name>_test` variant renders it, having
	// reached it by import path) is naming a LOCAL type — and must emit in the same bare form the
	// seeded production metadata uses, or the two spellings of one resolved pair survive as two
	// GoImplement records and go2cs-gen defines the adapter twice. See stripLocalTypeQualifier.
	if options.testPackageName != "" {
		testLocalTypePrefixes = []string{packageNamespace + "." + getSanitizedImport(options.testPackageName+PackageSuffix)}
	}

	// Load the PRODUCTION package's own GoImplement pairs from its (colocated, already-seeded)
	// package_info.cs (B4/B5): visitImportSpec skips the package-under-test alias load — its
	// types bind locally — which also skipped these sets, so an EXTERNAL test file's cast of a
	// production type could not see the seeded adapters and re-recorded the pair. Must run per
	// variant: resetPackageState above just cleared the sets.
	//
	// A REFERENCE model clears testPackageName/Path (that is what makes production bind as an
	// ordinary import), so the name comes from testProductionName there. The INTERNAL white-box
	// bridge is why this cannot be left to visitImportSpec: it is the SAME Go package, so it never
	// imports production, yet production is a referenced assembly whose own partials already
	// realize its records. Without them the bridge re-records every production value cast and
	// constructs a redundant ᴠ adapter — a DIFFERENT identity than the value production's own code
	// returns, which encoding/hex catches at `err != tt.err`. The external variant loads these
	// through its import too; the sets are idempotent.
	productionName := options.testPackageName

	if productionName == "" {
		productionName = options.testProductionName
	}

	if productionName != "" {
		productionInfoPath := platformPackageInfoPath(outputPath, goosOfTarget(options.targetPlatform))
		loadPackageImplements(productionInfoPath, productionName)
		seedProductionAliasLifts(pkg, productionInfoPath)
		seedProductionInterfaceAliases(pkg, productionInfoPath, options)
		seedProductionDynamicTypeLifts(productionInfoPath)
	}

	allEntries := make([]FileEntry, 0, len(pkg.Syntax))
	entryByPath := make(map[string]*FileEntry, len(pkg.Syntax))

	for i, file := range pkg.Syntax {
		if i >= len(pkg.CompiledGoFiles) {
			break
		}

		allEntries = append(allEntries, newFileEntry(file, pkg.CompiledGoFiles[i], false))
		entryByPath[filepath.Clean(pkg.CompiledGoFiles[i])] = &allEntries[len(allEntries)-1]
	}

	selected := make([]FileEntry, 0, len(testEntries))
	for _, requested := range testEntries {
		entry := entryByPath[filepath.Clean(requested.filePath)]
		if entry == nil {
			continue
		}

		outputFile := filepath.Join(outputPath, strings.TrimSuffix(filepath.Base(entry.filePath), ".go")+".cs")
		manual, err := containsManualConversionMarker(outputFile)
		if err != nil {
			return nil, nil, err
		}

		// A hand-owned (GoManualConversion-marked) test `.cs` is never overwritten, but its
		// source stays in the convert set — its visit feeds package-wide emission state that
		// sibling files depend on; only its EMISSION redirects, to the non-compiled `.cs.auto`
		// review sibling. Same semantics as processConversion's marked-file flow — dropping the
		// visit corrupts sibling emission.
		// The copy shares the analysis maps with the allEntries element (maps are references).
		selectedEntry := *entry
		selectedEntry.manualConversion = manual
		selected = append(selected, selectedEntry)
	}

	if len(selected) == 0 {
		return nil, HashSet[string]{}, nil
	}

	// A `_test.go` in the variant's syntax that the caller did NOT select for emission is a
	// Phase-4D compile-excluded file (Example/Benchmark-only, selectCompileExcludedTestFiles):
	// it participates in analysis but renders no C#, so it must never claim a hoisted literal
	// field (see FileEntry.emissionExcluded). Production files are already claim-fenced by the
	// hoist collector's seeded-mode emitted check.
	selectedPaths := make(map[string]bool, len(selected))
	for _, entry := range selected {
		selectedPaths[filepath.Clean(entry.filePath)] = true
	}

	for i := range allEntries {
		path := filepath.Clean(allEntries[i].filePath)

		if strings.HasSuffix(strings.ToLower(path), "_test.go") && !selectedPaths[path] {
			allEntries[i].emissionExcluded = true
		}
	}

	globalIdentNames := make(map[*ast.Ident]string)
	globalScope := map[string]*types.Var{}

	// Mirror processConversion's package-wide analysis sequence — a test file is an ordinary Go
	// file and needs the same emission inputs (collectMovedInitVars runs below, after the hoist
	// collection whose reader set feeds its dependency graph — same ordering as processConversion).
	performNameCollisionAnalysis(pkg)

	for _, entry := range allEntries {
		performGlobalVariableAnalysis(entry.file.Decls, pkg.TypesInfo, globalIdentNames, globalScope)
	}

	// Over allEntries, not prodEntries: a _test.go file can call runtime.Caller/Callers directly
	// (io/multi_test.go's flatten-depth assertions are the measured case) and needs the same
	// protection production code does — see callerInliningAnalysis.go.
	needsNoInlining := computeNoInliningClosure(allEntries, pkg.Types, pkg.TypesInfo)

	collectCaptureModeMethods(pkg)
	collectTypeSpecRHS(pkg)

	// The production-file entry list (manual-conversion flags resolved against the production
	// `.cs` on disk) — input to the ref-lowering classification, the hoist seed, and every other
	// production-only sub-pass below.
	prodEntries := make([]FileEntry, 0, len(allEntries))

	for _, entry := range allEntries {
		if strings.HasSuffix(strings.ToLower(entry.filePath), "_test.go") {
			continue
		}

		prodEntry := entry
		prodOutput := filepath.Join(outputPath, strings.TrimSuffix(filepath.Base(entry.filePath), ".go")+".cs")

		if manual, err := containsManualConversionMarker(prodOutput); err == nil {
			prodEntry.manualConversion = manual
		}

		prodEntries = append(prodEntries, prodEntry)
	}

	// ж-box A2 (three-driver rule, DESIGN-zh-box-reduction §3.5): the ref-lowering classification
	// runs in the -tests driver too — over the PRODUCTION files only (prodEntries; the entry point
	// additionally filters `_test.go` structurally), so the merged white-box package's test-side
	// func-value aliases can never desynchronize this classification from the -stdlib emission's
	// (§3.5's determinism invariant). The EXTERNAL variant carries no production files and records
	// an empty result. Runs BEFORE escape analysis, which consults the reversion verdicts; the
	// signature/call-site emission reads the lowered sets during the visits.
	performRefLoweringAnalysis(prodEntries, pkg.Types, pkg.TypesInfo, options)

	performEscapeAnalysis(allEntries, pkg.Fset, pkg.Types, pkg.TypesInfo)
	collectAddressedGlobals(allEntries, pkg.Types, pkg.TypesInfo)
	computeImportAliasRenames(allEntries, pkg.Types, packageNamespace)
	collectPublicizedTypes(pkg.Types)

	// Bind the //go:cgo_import_dynamic pragmas here too, and not only because the sequence is
	// mirrored: a -tests conversion RECOMPILES the production sources into the test assembly, so
	// that assembly declares the same trampolines and needs the same records to resolve them. It is
	// also the direction that fails badly if skipped -- applyCgoDynamicImports rewrites an existing
	// section from what THIS run bound, so a driver that never binds would silently empty a section
	// the -stdlib emission had populated. The EXTERNAL variant carries no production files, binds
	// nothing, and correctly emits nothing: it declares no trampolines either.
	collectCgoDynamicImportsFromEntries(allEntries)

	preloadImportedTypeAliases(allEntries, options)

	// Tier C hoisted string literals (§4.4's `-tests` invariants). The INTERNAL test variant
	// recompiles the package under test into this assembly and emits into the SAME package class,
	// so its test files must REFERENCE the fields the production `.cs` on disk already declares —
	// never re-declare them (a `_test.go` can sort BEFORE its production owner, so this, not name
	// luck, is what prevents CS0102). The production map is recomputed from the production files
	// exactly as processConversion computed it (same collector, same order, same manual-conversion
	// flags), then handed to the real pass as a seed; only `_test.go` files may claim a NEW field.
	// The EXTERNAL variant carries no production files, so its seed is empty and its own class
	// (`<pkg>_test_package`) claims freely — which is required, since a production field is
	// `private` to a different class.

	// The seed run SIMULATES processConversion, which does relocate an out-of-order initializer
	// (initOrderRelocated=true), so it reproduces the production `.cs` on disk exactly. The real
	// run STILL passes false even though relocation now runs here (collectMovedInitVars below):
	// suppressing test-file hoists in initializer-reachable functions is a pure (and tiny)
	// allocation pessimization, never a correctness risk, and flipping the flag would drift the
	// hoist claims of every banked *_test.cs for no behavioral gain.
	collectHoistedLiterals(prodEntries, pkg.Types, pkg.TypesInfo, goosOfTarget(options.targetPlatform), nil, true)
	productionHoistSeed := packageHoistNames
	collectHoistedLiterals(allEntries, pkg.Types, pkg.TypesInfo, goosOfTarget(options.targetPlatform), productionHoistSeed, false)

	// Find test-file package-level var initializers whose Go dependency order C#'s
	// static-field-initializer order cannot reproduce — the same pass processConversion runs
	// (three-drivers rule), over the whole variant package so an internal-variant test var that
	// reads a production var cross-file (gob's basicTypes over type.go's tBool…) relocates too.
	// Production vars it flags are never re-emitted here (only test files convert below), so
	// only test-file relocations reach packageMovedInitMethods; the ordered assignments are
	// emitted by writeTestVariantInitFile after the convert loop. First demonstrated consumer:
	// internal/fmtsort's sort_test.go (compareTests reads chans/ints declared later in the file
	// — every test died in the class cctor on the default slice).
	collectMovedInitVars(pkg.Fset, pkg.Types, pkg.TypesInfo, pkg.Syntax)

	var compileNames []string // emitted test .cs basenames — the csproj's compile items
	var resolveNames []string // every emission (incl. .cs.auto review siblings) for marker resolution

	convert := func(entry FileEntry) (err error) {
		if !options.debugMode {
			defer func() {
				if r := recover(); r != nil {
					err = fmt.Errorf("convert test file %q: %v", entry.filePath, r)
				}
			}()
		}

		visitor := newFileVisitor(pkg.Fset, pkg.Types, pkg.TypesInfo, options, globalIdentNames, globalScope, needsNoInlining, entry)
		visitor.visitFile(entry.file)

		baseName := strings.TrimSuffix(filepath.Base(entry.filePath), ".go")

		if entry.manualConversion {
			// Hand-owned destination: the visit above already fed this file's package-wide state;
			// emit the auto conversion to the `.cs.auto` review sibling, leaving the marked `.cs`
			// untouched. The HAND-OWNED `.cs` is the compile item; the `.cs.auto` sibling never is.
			outputName := filepath.Join(outputPath, baseName+".cs.auto")
			visitor.finalizePositionMap(outputName)

			if writeErr := writeAutoConversionSibling(outputName, baseName, visitor.outputBuilder.String()); writeErr != nil {
				showWarning("%s", writeErr)
			}

			projectImports.UnionWithSet(visitor.importQueue)
			compileNames = append(compileNames, baseName+".cs")
			resolveNames = append(resolveNames, outputName)
			return nil
		}

		outputName := filepath.Join(outputPath, baseName+".cs")
		if writeErr := visitor.writeOutputFile(outputName); writeErr != nil {
			return writeErr
		}

		projectImports.UnionWithSet(visitor.importQueue)
		compileNames = append(compileNames, filepath.Base(outputName))
		resolveNames = append(resolveNames, outputName)
		return nil
	}

	for _, entry := range selected {
		if err := convert(entry); err != nil {
			return nil, nil, err
		}
	}

	// Emit the variant's ordered relocated-initializer file (no-op unless the convert loop
	// recorded any relocation). The internal variant shares the production `<pkg>_package`
	// class: when the production package_init.cs exists it owns the single static-ctor slot,
	// so the test side implements its erasable partial hook instead of a second ctor. The
	// `_test.cs` suffix keeps the file out of the production csproj's compile set (the IP-4
	// test-artifact exclusion) — it compiles only into the test assembly.
	if len(packageMovedInitMethods) > 0 {
		isExternalVariant := strings.HasSuffix(pkg.Name, "_test")
		variantKind := "internal"

		if isExternalVariant {
			variantKind = "external"
		}

		initFileName := fmt.Sprintf("package_init_%s_test.cs", variantKind)
		variantClassName := getSanitizedImport(pkg.Name + PackageSuffix)
		implementHook := false

		if options.testClassNameOverride != "" {
			variantClassName = options.testClassNameOverride
		} else if !isExternalVariant {
			// Layout L3 puts the production package_init.cs in the package's per-GOOS folder (Go's
			// InitOrder differs when the file set does — conversionDriver.go), so this must ask
			// where the tree actually keeps it. A flat-only probe answered "no production ctor" for
			// every L3 package and emitted a SECOND `static <pkg>_package()` beside the real one:
			// CS0111 on the constructor itself, once crypto/x509's platform folder started
			// compiling into its test assembly at all.
			_, statErr := os.Stat(platformLayoutPath(outputPath, goosOfTarget(options.targetPlatform), PackageInitFileName))
			implementHook = statErr == nil
		}

		if err := writeTestVariantInitFile(outputPath, initFileName, packageNamespace, variantClassName, implementHook); err != nil {
			return nil, nil, err
		}

		compileNames = append(compileNames, initFileName)
	}

	resolveDynamicTypeMarkers(resolveNames)

	// Adapter names cannot resolve here: this variant's GoImplement records are not merged into
	// package_test_info.cs until the caller writes it, and a name depends on the FINAL set. Hand
	// the emission PATHS up (compileNames are csproj-relative) for the caller's single pass.
	testAdapterResolveNames = append(testAdapterResolveNames, resolveNames...)

	return compileNames, NewHashSet(projectImports.Keys()), nil
}

// appendExternalTestPackageClass appends the external test package's [GoPackage] partial class
// declaration to package_test_info.cs — converted external-test files declare partial pieces of
// <name>_test_package, and this block is the attribute-bearing anchor the production
// package_info.cs provides for the production class. It also widens the file's `using static`
// scope to that class (B3): metadata attributes merged from the test variants (GoImplement /
// GoImplicitConv) can reference types DECLARED in the external test package (e.g. an errWriter
// helper cast to io.Writer), which the seeded production-only `using static <ns>.<pkg>_package;`
// cannot resolve — CS0246 on every such attribute argument.
func appendExternalTestPackageClass(testInfoPath, packageNamespace, productionPackageName string, external *packages.Package) error {
	if external == nil {
		return nil
	}

	data, err := os.ReadFile(testInfoPath)
	if err != nil {
		return fmt.Errorf("read test package metadata: %w", err)
	}

	// EOL-agnostic: package_test_info.cs is READ BACK off disk, and for a validated package it is a
	// COMMITTED file, so its line endings are the checkout's rather than the converter's. Every test
	// below is CRLF-shaped — the `block` literal and the using-directive insert — so an LF copy makes
	// `strings.Contains(contents, block)` miss and the [GoPackage] class is appended AGAIN on every
	// run, accumulating duplicate declarations. writePackageInfoFile emits this file uniformly CRLF
	// (one "\r\n" per line, no exceptions), which is why normalizing to CRLF is the faithful
	// reconstruction rather than a guess, and why it is inert on a Windows checkout: the content is
	// already CRLF, so `contents == string(data)` still short-circuits the write below.
	// F3 in docs/PLAN-linux-operation.md.
	contents := normalizeToCRLF(string(data))
	className := getSanitizedImport(external.Name + PackageSuffix)

	productionUsing := fmt.Sprintf("using static %s.%s;", packageNamespace, getSanitizedImport(productionPackageName+PackageSuffix))
	testUsing := fmt.Sprintf("using static %s.%s;", packageNamespace, className)

	if !strings.Contains(contents, testUsing) {
		if !strings.Contains(contents, productionUsing) {
			return fmt.Errorf("seeded test package metadata %q is missing the production using directive %q", testInfoPath, productionUsing)
		}
		contents = strings.Replace(contents, productionUsing, productionUsing+"\r\n"+testUsing, 1)
	}

	block := fmt.Sprintf("\r\n[GoPackage(\"%s\")]\r\npublic static partial class %s\r\n{\r\n}\r\n", external.Name, className)

	if !strings.Contains(contents, block) {
		contents += block
	}

	if contents == string(data) {
		return nil
	}

	return os.WriteFile(testInfoPath, []byte(contents), 0644)
}

// conversionRecordSet snapshots the package-scoped GoImplement/GoImplicitConv record globals so
// the external test variant's records can be written through the shared writePackageInfoFile in
// TWO passes with different anchors (B4/B5) — the writer reads the live globals, so each pass
// installs its partition.
type conversionRecordSet struct {
	interfaceImplements map[string]HashSet[string]
	promotedImplements  map[string]HashSet[string]
	proxies             map[string][2]string
	implicitConvs       map[string]HashSet[string]
	invertedConvs       map[string]HashSet[string]
	indirectConvs       map[string]HashSet[string]
	numericConvs        map[string]map[string]string
	indirectNumerics    map[string]map[string]string
}

func newConversionRecordSet() conversionRecordSet {
	return conversionRecordSet{
		interfaceImplements: make(map[string]HashSet[string]),
		promotedImplements:  make(map[string]HashSet[string]),
		proxies:             make(map[string][2]string),
		implicitConvs:       make(map[string]HashSet[string]),
		invertedConvs:       make(map[string]HashSet[string]),
		indirectConvs:       make(map[string]HashSet[string]),
		numericConvs:        make(map[string]map[string]string),
		indirectNumerics:    make(map[string]map[string]string),
	}
}

func (r conversionRecordSet) install() {
	interfaceImplementations = r.interfaceImplements
	promotedInterfaceImplementations = r.promotedImplements
	constraintProxies = r.proxies
	implicitConversions = r.implicitConvs
	invertedImplicitConversions = r.invertedConvs
	indirectImplicitConversions = r.indirectConvs
	numericConversions = r.numericConvs
	indirectNumericConversions = r.indirectNumerics
}

func (r conversionRecordSet) isEmpty() bool {
	return len(r.interfaceImplements) == 0 && len(r.promotedImplements) == 0 &&
		len(r.proxies) == 0 && len(r.implicitConvs) == 0 && len(r.invertedConvs) == 0 &&
		len(r.indirectConvs) == 0 && len(r.numericConvs) == 0 && len(r.indirectNumerics) == 0
}

// isTestAnchoredImplementRecord decides which -tests metadata anchor hosts an EXTERNAL variant
// GoImplement record (B4/B5). The go2cs-gen generators host generated code in the FIRST class
// of the attribute-bearing file, so anchoring is dictated by where each record's generated form
// must land:
//   - an adapter-CLASS record (interface-sourced or foreign-value ᴠ adapters, per
//     adapterClassImplementations; and every ж pointer adapter for a non-production type) is
//     referenced BARE from test-file cast sites, which are partial pieces of the test package
//     class — the adapter must be its member;
//   - a BARE impl name is a type declared in the external test package itself — its generated
//     partial struct must merge with that declaration in the test package class;
//   - a PRODUCTION-qualified record (`sort_package.IntSlice`, its rooted form, or its
//     namespace-relative form `math.rand_package.Rand`) generates a partial/adapter on the
//     production class — it stays with the production-anchored package_test_info.cs, whose first
//     class is the production class.
func isTestAnchoredImplementRecord(ifaceName, implName, productionClassName string, handOwnHost bool) bool {
	if adapterClassImplementations.Contains(ifaceName + "|" + implName) {
		return true
	}

	inner := implName
	pointerForm := false

	if trimmed, ok := strings.CutPrefix(inner, PointerPrefix+"<"); ok {
		inner = strings.TrimSuffix(trimmed, ">")
		pointerForm = true
	}

	if !strings.Contains(inner, ".") {
		return true
	}

	if pointerForm {
		// HAND-OWNED HOST (option B, owner-ruled 2026-09-04): a pointer-form implement record is
		// RELOCATABLE. The production class here is the hand-written host itself, so the last
		// bullet above -- "generates a partial/adapter on the production class" -- would put
		// generated code inside a hand-owned type in a SEPARATE assembly, which is not something
		// any model can do; and it is not what the generator does under a reference model anyway.
		// recordsRequireProductionMutation, the white-box predicate below, states the mechanism
		// for exactly this record class: "qualified production structs are foreign to the test
		// compilation, so go2cs-gen emits value or pointer adapter classes in the test anchor
		// instead of partial production structs."
		//
		// MEASURED, one record, by a convert-only probe of `testing` (2026-09-04):
		// GoImplement<ж<testing_package.T>, testing_package.TB> -- *testing.T implements
		// testing.TB, reached through helperfuncs_test.go's `func testHelper(t testing.TB)`.
		// Every OTHER row in the corpus carries the same record with `testing_package.` foreign to
		// its own production class, so it already anchors test-side; `testing` is the first
		// package where the package under test SUPPLIES the interface, and therefore the first to
		// reach the production-qualified branch below at all.
		//
		// DELIBERATELY NARROW. Whether this is the right answer for EVERY reference-model row --
		// i.e. whether the white-box predicate's reasoning should govern here generally -- is a
		// real question with a blast radius this gate cannot see: rows that silently took the
		// recompile fallback for this reason would stop taking it, changing their emission and
		// their assembly identity. That is sized separately (Q25) with a two-seeded reference-model
		// census as its precondition, and must NOT be widened here on the strength of one row.
		if handOwnHost {
			return true
		}

		inner = strings.TrimPrefix(inner, "global::")

		if strings.HasPrefix(inner, productionClassName+".") ||
			strings.HasPrefix(inner, packageNamespace+"."+productionClassName+".") {
			return false
		}

		// The live records qualify the implementer NAMESPACE-RELATIVE — without the `go.` root —
		// so a NESTED package's production type arrives as `math.rand_package.Rand`, matching
		// neither form above and landing in the wrong anchor. (A TOP-LEVEL package worked by
		// accident: its relative qualifier IS the bare `sort_package.` form.) Recognize the
		// relative qualifier so nested packages keep production types production-anchored.
		if relative, ok := strings.CutPrefix(packageNamespace, RootNamespace+"."); ok {
			return !strings.HasPrefix(inner, relative+"."+productionClassName+".")
		}

		return true
	}

	return false
}

// isTestAnchoredConversionRecord decides the anchor for an EXTERNAL variant GoImplicitConv
// record: the generated conversion operators live inside a partial declaration of one of the
// two types, so a pair involving ANY test-package-local (bare) type must anchor to the test
// package class; a pair between qualified (production/foreign) types keeps the production
// anchor, matching the pre-split emission.
func isTestAnchoredConversionRecord(sourceType, targetType string) bool {
	isBare := func(name string) bool {
		if trimmed, ok := strings.CutPrefix(name, PointerPrefix+"<"); ok {
			name = strings.TrimSuffix(trimmed, ">")
		}

		return !strings.Contains(name, ".")
	}

	return isBare(sourceType) || isBare(targetType)
}

// splitExternalVariantRecords partitions the LIVE record globals (the external variant's
// collected records) into the test-anchored and production-anchored sets (B4/B5).
func splitExternalVariantRecords(productionClassName string, handOwnHost bool) (testAnchored, productionAnchored conversionRecordSet) {
	testAnchored = newConversionRecordSet()
	productionAnchored = newConversionRecordSet()

	splitImplements := func(source map[string]HashSet[string], test, production map[string]HashSet[string]) {
		for ifaceName, implementations := range source {
			for implementation := range implementations {
				target := production

				if isTestAnchoredImplementRecord(ifaceName, implementation, productionClassName, handOwnHost) {
					target = test
				}

				if existing, ok := target[ifaceName]; ok {
					existing.Add(implementation)
				} else {
					target[ifaceName] = NewHashSet([]string{implementation})
				}
			}
		}
	}

	splitImplements(interfaceImplementations, testAnchored.interfaceImplements, productionAnchored.interfaceImplements)
	splitImplements(promotedInterfaceImplementations, testAnchored.promotedImplements, productionAnchored.promotedImplements)

	for key, proxy := range constraintProxies {
		if isTestAnchoredConversionRecord(proxy[0], proxy[1]) {
			testAnchored.proxies[key] = proxy
		} else {
			productionAnchored.proxies[key] = proxy
		}
	}

	splitConversions := func(source map[string]HashSet[string], test, production map[string]HashSet[string]) {
		for sourceType, targetTypes := range source {
			for targetType := range targetTypes {
				target := production

				if isTestAnchoredConversionRecord(sourceType, targetType) {
					target = test
				}

				if existing, ok := target[sourceType]; ok {
					existing.Add(targetType)
				} else {
					target[sourceType] = NewHashSet([]string{targetType})
				}
			}
		}
	}

	splitConversions(implicitConversions, testAnchored.implicitConvs, productionAnchored.implicitConvs)
	splitConversions(invertedImplicitConversions, testAnchored.invertedConvs, productionAnchored.invertedConvs)
	splitConversions(indirectImplicitConversions, testAnchored.indirectConvs, productionAnchored.indirectConvs)

	splitNumerics := func(source map[string]map[string]string, test, production map[string]map[string]string) {
		for sourceType, targetTypes := range source {
			for targetType, valueType := range targetTypes {
				target := production

				if isTestAnchoredConversionRecord(sourceType, targetType) {
					target = test
				}

				if existing, ok := target[sourceType]; ok {
					existing[targetType] = valueType
				} else {
					target[sourceType] = map[string]string{targetType: valueType}
				}
			}
		}
	}

	splitNumerics(numericConversions, testAnchored.numericConvs, productionAnchored.numericConvs)
	splitNumerics(indirectNumericConversions, testAnchored.indirectNumerics, productionAnchored.indirectNumerics)

	return testAnchored, productionAnchored
}

// externalTestPackageInfoSeed composes the initial contents of package_info_external_test.cs. The
// structure mirrors package_info-template.txt (the shared writer requires all four marker
// sections); the FIRST — and only — class declaration is the external test package class, which
// is what the go2cs-gen generators anchor generated adapters and partials to
// (GetFirstClassName). The class is declared WITHOUT [GoPackage]: the attribute-bearing partial
// lives in package_test_info.cs (appendExternalTestPackageClass), and duplicating the attribute
// on a second partial declaration is CS0579. Both `using static` scopes are included so
// attribute arguments resolve exactly as they do in package_test_info.cs.
// internalTestPackageInfoSeed composes the initial contents of package_info_internal_test.cs —
// the WHITE-BOX bridge's metadata anchor. A mixed suite's test compilation carries TWO classes
// that generated code must merge into: the external test class (package_test_info.cs) and the
// internal bridge. The generators host output in the FIRST class of the attribute-bearing file,
// so a record whose generated partial must merge with a bridge-declared type needs a file whose
// first class IS the bridge — anchoring it in the external class would declare a phantom empty
// type there instead (the same B4/B5 reasoning that gives the recompile model its two files).
// This is also the bridge's ONE `static` declaration (its .cs parts are bare `partial class`),
// and its ONE `[GoPackage]` carrier — no other partial declares the attribute, so no CS0579.
func internalTestPackageInfoSeed(projectNamespace, productionClassName, bridgeClassName, goPackageName string) string {
	var b strings.Builder

	b.WriteString("// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /\r\n")
	b.WriteString("// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type\r\n")
	b.WriteString("// anchor here — the source generators host output in the first class of the\r\n")
	b.WriteString("// attribute-bearing file, and only this file's first class is the bridge. Records for\r\n")
	b.WriteString("// production and external-test types stay in package_test_info.cs.\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <ImportedTypeAliases>\r\n")
	b.WriteString("// </ImportedTypeAliases>\r\n")
	b.WriteString("\r\n")
	b.WriteString("using go;\r\n")
	b.WriteString(fmt.Sprintf("using static %s.%s;\r\n", projectNamespace, productionClassName))
	b.WriteString(fmt.Sprintf("using static %s.%s;\r\n", projectNamespace, bridgeClassName))
	b.WriteString("\r\n")
	b.WriteString("// <ExportedTypeAliases>\r\n")
	b.WriteString("// </ExportedTypeAliases>\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <InterfaceImplementations>\r\n")
	b.WriteString("// </InterfaceImplementations>\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <ImplicitConversions>\r\n")
	b.WriteString("// </ImplicitConversions>\r\n")
	b.WriteString("\r\n")
	b.WriteString(fmt.Sprintf("namespace %s;\r\n", projectNamespace))
	b.WriteString("\r\n")
	b.WriteString(fmt.Sprintf("[GoPackage(\"%s\")]\r\n", goPackageName))
	b.WriteString(fmt.Sprintf("public static partial class %s\r\n{\r\n}\r\n", bridgeClassName))

	return b.String()
}

// splitWhiteboxVariantRecords partitions the LIVE record globals between the bridge anchor and
// the test anchor. The discriminator is the record participant's spelling plus the bridge's
// declared-name set: a BARE name declared by an internal _test.go file is a bridge type, whose
// generated partial must merge inside the bridge class; every other record — production-qualified,
// foreign, or bare-but-external-declared — anchors to the test class as before.
//
// ⚠ A BARE name resolves in the scope of the variant that RECORDED it, so the declared-name set
// may only be consulted while splitting the BRIDGE variant's own records (bridgeVariant). Each
// variant's records are split as they are collected, and every cross-variant reference is routed
// by go/types.Object identity to a CLASS-QUALIFIED spelling (whiteboxBridgeNamedType renders an
// internal-test type the external suite names as `global::<ns>.<pkg>_internal_test_package.T`;
// whiteboxProductionObject does the mirror while the bridge converts) — so a bare name recorded by
// the external suite is external-declared by construction, whatever the bridge happens to declare
// under the same simple name. Matching it against the bridge's set anchors the record at the
// bridge, where the OTHER participant is out of scope: encoding/gob declares `Point` in both
// variants (codec_test.go and example_interface_test.go), and the external pair
// `Point → Pythagoras` landed in package_info_internal_test.cs with `Pythagoras` — external-only —
// unqualified, CS0246 with no test host and all 106 verdicts empty. (Write-time qualification
// cannot repair it: qualifyAmbiguousTestTypeRefs roots an ambiguous bare name at the file it is
// ALREADY being written into, so a mis-anchored record is merely qualified to the wrong variant.)
func splitWhiteboxVariantRecords(bridgeTypeNames HashSet[string], bridgeVariant bool) (bridgeAnchored, testAnchored conversionRecordSet) {
	bridgeAnchored = newConversionRecordSet()
	testAnchored = newConversionRecordSet()

	isBridgeName := func(name string) bool {
		if !bridgeVariant {
			return false
		}

		if trimmed, ok := strings.CutPrefix(name, PointerPrefix+"<"); ok {
			name = strings.TrimSuffix(trimmed, ">")
		}

		return !strings.Contains(name, ".") && bridgeTypeNames.Contains(strings.TrimPrefix(name, ShadowVarMarker))
	}

	splitImplements := func(source map[string]HashSet[string], bridge, test map[string]HashSet[string]) {
		for ifaceName, implementations := range source {
			for implementation := range implementations {
				target := test

				// EITHER side being bridge-declared anchors the record at the bridge: a bridge
				// implementer needs its partial-struct there, and a bridge INTERFACE with a
				// foreign implementer needs its conversion operator on the interface partial —
				// encoding/binary's `TestByteOrder_byteOrder` ← `binary_package.bigEndian`.
				if isBridgeName(implementation) || isBridgeName(ifaceName) {
					target = bridge
				}

				if existing, ok := target[ifaceName]; ok {
					existing.Add(implementation)
				} else {
					target[ifaceName] = NewHashSet([]string{implementation})
				}
			}
		}
	}

	splitImplements(interfaceImplementations, bridgeAnchored.interfaceImplements, testAnchored.interfaceImplements)
	splitImplements(promotedInterfaceImplementations, bridgeAnchored.promotedImplements, testAnchored.promotedImplements)

	for key, proxy := range constraintProxies {
		if isBridgeName(proxy[0]) || isBridgeName(proxy[1]) {
			bridgeAnchored.proxies[key] = proxy
		} else {
			testAnchored.proxies[key] = proxy
		}
	}

	splitConversions := func(source map[string]HashSet[string], bridge, test map[string]HashSet[string]) {
		for sourceType, targetTypes := range source {
			for targetType := range targetTypes {
				target := test

				if isBridgeName(sourceType) || isBridgeName(targetType) {
					target = bridge
				}

				if existing, ok := target[sourceType]; ok {
					existing.Add(targetType)
				} else {
					target[sourceType] = NewHashSet([]string{targetType})
				}
			}
		}
	}

	splitConversions(implicitConversions, bridgeAnchored.implicitConvs, testAnchored.implicitConvs)
	splitConversions(invertedImplicitConversions, bridgeAnchored.invertedConvs, testAnchored.invertedConvs)
	splitConversions(indirectImplicitConversions, bridgeAnchored.indirectConvs, testAnchored.indirectConvs)

	splitNumerics := func(source map[string]map[string]string, bridge, test map[string]map[string]string) {
		for sourceType, targetTypes := range source {
			for targetType, valueType := range targetTypes {
				target := test

				if isBridgeName(sourceType) || isBridgeName(targetType) {
					target = bridge
				}

				if existing, ok := target[sourceType]; ok {
					existing[targetType] = valueType
				} else {
					target[sourceType] = map[string]string{targetType: valueType}
				}
			}
		}
	}

	splitNumerics(numericConversions, bridgeAnchored.numericConvs, testAnchored.numericConvs)
	splitNumerics(indirectNumericConversions, bridgeAnchored.indirectNumerics, testAnchored.indirectNumerics)

	return bridgeAnchored, testAnchored
}

// writeWhiteboxVariantMetadata merges a WHITE-BOX variant's live metadata globals into the two
// -tests anchor files: bridge-anchored records into package_info_internal_test.cs (first class:
// the bridge), everything else into package_test_info.cs (first class: the external test class).
// Alias globals are stashed around the bridge-unit write for the same CS1537 reason
// writeExternalVariantMetadata stashes them, and the accessibility section never reaches the
// bridge unit — bridge-declared types carry their accessibility inline (testInlineTypeAccess).
// Returns the unit's file name when it was written, or "" when this variant contributed no
// bridge-anchored records. bridgeVariant states which variant collected the live records — the
// bridge's declared-name set only resolves ITS own bare spellings (splitWhiteboxVariantRecords).
func writeWhiteboxVariantMetadata(testInfoPath, outputPath, productionClassName, bridgeClassName, goPackageName, internalAnchor, testAnchor string, bridgeTypeNames HashSet[string], bridgeVariant bool) (string, error) {
	bridgeAnchored, testAnchored := splitWhiteboxVariantRecords(bridgeTypeNames, bridgeVariant)

	// Both anchored writes below are reference-model files: their anchor class IS the local
	// type scope, and the production class is a referenced assembly.
	metadataAnchorLocalTypes = true

	unitName := ""

	// The bridge unit is written whether or not this variant contributed records, because the
	// file is not only a metadata anchor — it is the ONLY place `<pkg>_internal_test_package` is
	// declared `public static partial`. Every converted SOURCE file opens its package class bare
	// (`partial class X {`) by design; the modifier lives in the metadata file, exactly as it does
	// for the production and external-test classes. A record-less bridge therefore had no static
	// declaration at all, and an internal test file declaring an extension method is then CS1106 —
	// `internal/syscall/windows/registry`'s `export_test.go`, whose whole 6-verdict suite sat
	// behind `func (k Key) SetValue(…)`. Banked mixed suites only appear to escape it: `sort`,
	// `bytes` and `strings` each happen to have a go2cs-gen RecvGenerator file that re-declares
	// the class `public static partial`, i.e. a GENERATOR supplying a modifier the emitter owes.
	// A bridge with no records writes an anchor whose sections are all empty, which is what the
	// production and external-test seeds already do in the same situation.
	{
		unitPath := filepath.Join(outputPath, internalTestPackageInfoFileName)

		if _, err := os.Stat(unitPath); os.IsNotExist(err) {
			seed := internalTestPackageInfoSeed(packageNamespace, productionClassName, bridgeClassName, goPackageName)

			if err := os.WriteFile(unitPath, []byte(seed), 0644); err != nil {
				return "", fmt.Errorf("seed internal test package metadata: %w", err)
			}
		}

		savedImported, savedExported := importedTypeAliases, exportedTypeAliases
		savedAccess := packageEmittedTypeAccess
		importedTypeAliases = map[string]string{}
		exportedTypeAliases = map[string]string{}
		packageEmittedTypeAccess = HashSet[string]{}

		bridgeAnchored.install()
		metadataAnchorClassPrefix = internalAnchor
		writePackageInfoFile(unitPath, true)

		importedTypeAliases, exportedTypeAliases = savedImported, savedExported
		packageEmittedTypeAccess = savedAccess
		unitName = internalTestPackageInfoFileName
	}

	testAnchored.install()
	metadataAnchorClassPrefix = testAnchor
	writePackageInfoFile(testInfoPath, true)

	return unitName, nil
}

func externalTestPackageInfoSeed(projectNamespace, productionClassName, testClassName string) string {
	var b strings.Builder

	b.WriteString("// go2cs metadata anchor for the EXTERNAL test package (<name>_test): GoImplement /\r\n")
	b.WriteString("// GoImplicitConv attributes recorded by its converted _test files whose GENERATED code\r\n")
	b.WriteString("// (adapter classes, partial-struct implementations, conversion operators) must anchor to\r\n")
	b.WriteString("// the test package class — the source generators host output in the first class of the\r\n")
	b.WriteString("// attribute-bearing file, and test-file cast sites reference the adapters as members of\r\n")
	b.WriteString("// the test package class. Production-anchored records stay in package_test_info.cs.\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <ImportedTypeAliases>\r\n")
	b.WriteString("// </ImportedTypeAliases>\r\n")
	b.WriteString("\r\n")
	b.WriteString("using go;\r\n")
	b.WriteString(fmt.Sprintf("using static %s.%s;\r\n", projectNamespace, productionClassName))
	b.WriteString(fmt.Sprintf("using static %s.%s;\r\n", projectNamespace, testClassName))
	b.WriteString("\r\n")
	b.WriteString("// <ExportedTypeAliases>\r\n")
	b.WriteString("// </ExportedTypeAliases>\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <InterfaceImplementations>\r\n")
	b.WriteString("// </InterfaceImplementations>\r\n")
	b.WriteString("\r\n")
	b.WriteString("// <ImplicitConversions>\r\n")
	b.WriteString("// </ImplicitConversions>\r\n")
	b.WriteString("\r\n")
	b.WriteString(fmt.Sprintf("namespace %s;\r\n", projectNamespace))
	b.WriteString("\r\n")
	b.WriteString(fmt.Sprintf("public static partial class %s\r\n{\r\n}\r\n", testClassName))

	return b.String()
}

// writeExternalVariantMetadata merges the EXTERNAL test variant's live metadata globals into
// the -tests anchor files (B4/B5). Test-anchored records are written into
// package_info_external_test.cs — a separate compilation unit whose first class is the test package
// class — through the SAME shared writer (with the alias globals stashed: `global using`
// aliases must live in exactly one file, CS1537, and GoTypeAlias attributes stay with the
// production-anchored metadata). Production-anchored records and every alias then merge into
// package_test_info.cs as before. Returns the unit's file name when it was written (the caller
// adds it to the test project's compile items), or "" when the variant introduced no
// test-anchored records — utf8-class packages keep their single-file shape byte-identical.
func writeExternalVariantMetadata(testInfoPath, outputPath, productionPackageName, productionAnchor, testAnchor string, handOwnHost bool) (string, error) {
	productionClassName := getSanitizedImport(productionPackageName + PackageSuffix)
	testAnchored, productionAnchored := splitExternalVariantRecords(productionClassName, handOwnHost)

	// RECOMPILE-model anchored writes: the production class is compiled into this assembly, so
	// the historical production-local type qualification stays in force (see writePackageInfoFile).
	metadataAnchorLocalTypes = false

	unitName := ""

	// The external variant's `[GoType]` types are declared in the TEST package class, so their
	// accessibility section belongs to the test-anchored unit — never to package_test_info.cs,
	// whose section sits inside the PRODUCTION package class (a stray entry there would declare a
	// second, phantom type of the same simple name in the wrong class). A variant that declares
	// types therefore needs the unit even when it recorded no test-anchored GoImplement /
	// GoImplicitConv attributes.
	if !testAnchored.isEmpty() || len(packageEmittedTypeAccess) > 0 {
		unitPath := filepath.Join(outputPath, externalTestPackageInfoFileName)

		if _, err := os.Stat(unitPath); os.IsNotExist(err) {
			seed := externalTestPackageInfoSeed(packageNamespace, productionClassName, getSanitizedImport(packageName+PackageSuffix))

			if err := os.WriteFile(unitPath, []byte(seed), 0644); err != nil {
				return "", fmt.Errorf("seed external test package metadata: %w", err)
			}
		}

		savedImported, savedExported := importedTypeAliases, exportedTypeAliases
		importedTypeAliases = map[string]string{}
		exportedTypeAliases = map[string]string{}

		testAnchored.install()
		metadataAnchorClassPrefix = testAnchor
		writePackageInfoFile(unitPath, true)

		importedTypeAliases, exportedTypeAliases = savedImported, savedExported
		unitName = externalTestPackageInfoFileName
	}

	// The production-anchored partition (plus the full alias globals) merges into
	// package_test_info.cs; the split partitions stay installed afterward — the external
	// variant is the last one converted, and nothing downstream reads these globals. The
	// accessibility entries were written to the test-anchored unit above and must NOT reach this
	// file's production-class section; clearing the set leaves the merge to preserve exactly the
	// production + internal-variant entries already there.
	packageEmittedTypeAccess = HashSet[string]{}

	productionAnchored.install()

	// package_test_info.cs anchors to the PRODUCTION class even though the EXTERNAL variant is the
	// one merging into it here — the anchor is a property of the file (its first class), not of the
	// variant writing it.
	metadataAnchorClassPrefix = productionAnchor
	writePackageInfoFile(testInfoPath, true)

	return unitName, nil
}

// discoverTestDeclarations finds every go-test-shaped top-level declaration in the variant's
// selected test files and classifies it. Disclosure is total by design (req §2.7): every
// discovered Test*/Benchmark*/Fuzz*/Example*/TestMain declaration lands in the manifest with an
// explicit status — nothing is silently absent. Capability gating is PER TEST (F4): a test whose
// transitive call closure requires capabilities outside the supported list blocks itself
// (status "unsupported" + reason), not its package.
func discoverTestDeclarations(pkg *packages.Package, entries []FileEntry, inputPath string, capabilities testCapabilityAnalysis, supported HashSet[string]) ([]testDeclaration, *testDeclaration) {
	selected := make(map[*ast.File]string, len(entries))
	for _, entry := range entries {
		selected[entry.file] = entry.filePath
	}

	result := make([]testDeclaration, 0)
	var testMain *testDeclaration

	for _, file := range pkg.Syntax {
		path, ok := selected[file]
		if !ok {
			continue
		}

		relPath, _ := filepath.Rel(inputPath, path)
		for _, decl := range file.Decls {
			fn, ok := decl.(*ast.FuncDecl)
			if !ok || fn.Recv != nil || fn.Name == nil {
				continue
			}

			obj, ok := pkg.TypesInfo.Defs[fn.Name].(*types.Func)
			if !ok {
				continue
			}

			sig, ok := obj.Type().(*types.Signature)
			if !ok || sig.TypeParams().Len() != 0 || sig.Results().Len() != 0 {
				continue
			}

			name := fn.Name.Name
			position := pkg.Fset.Position(fn.Pos())
			entry := testDeclaration{Name: name, PackageName: file.Name.Name, Source: filepath.ToSlash(relPath), Line: position.Line}

			requirements := capabilities.requiredFor(obj)
			required := requirements.Keys()
			sort.Strings(required)
			entry.RequiredCapabilities = required

			// Example functions take no parameters (F2): `go test` runs them with Output:
			// comparison, so they MUST appear in the manifest — disclosed-unsupported until
			// Phase 4D — or the differential oracle would silently under-compare.
			if sig.Params().Len() == 0 {
				if isGoTestName(name, "Example") {
					entry.Kind, entry.Status, entry.Reason = "example", "unsupported", "example execution is deferred to Phase 4D"
					result = append(result, entry)
				}
				continue
			}

			if sig.Params().Len() != 1 {
				continue
			}

			switch {
			case name == "TestMain" && isTestingPointer(sig.Params().At(0).Type(), "M"):
				entry.Kind = "test-main"
				entry.Status = "included"
				applyCapabilityGate(&entry, requirements, supported)
				declarationCopy := entry
				testMain = &declarationCopy
			case isGoTestName(name, "Test") && isTestingPointer(sig.Params().At(0).Type(), "T"):
				entry.Kind = "test"
				entry.Status = "included"
				applyCapabilityGate(&entry, requirements, supported)
				result = append(result, entry)
			case isGoTestName(name, "Benchmark") && isTestingPointer(sig.Params().At(0).Type(), "B"):
				entry.Kind, entry.Status, entry.Reason = "benchmark", "unsupported", "benchmark execution is deferred to Phase 4D"
				result = append(result, entry)
			case isGoTestName(name, "Fuzz") && isTestingPointer(sig.Params().At(0).Type(), "F"):
				entry.Kind, entry.Status, entry.Reason = "fuzz", "unsupported", "fuzz execution is deferred to Phase 4D"
				result = append(result, entry)
			}
		}
	}

	return result, testMain
}

// applyCapabilityGate downgrades an included declaration to disclosed-unsupported when its
// transitive capability requirements exceed the supported list.
func applyCapabilityGate(entry *testDeclaration, requirements HashSet[string], supported HashSet[string]) {
	unsupported := NewHashSet(requirements.Keys())
	unsupported.ExceptWith(supported.Keys())

	if unsupported.IsEmpty() {
		return
	}

	blocked := unsupported.Keys()
	sort.Strings(blocked)
	entry.Status = "unsupported"
	entry.Reason = unsupportedCapabilityReasonPrefix + strings.Join(blocked, ", ")
}

func isTestingPointer(t types.Type, typeName string) bool {
	pointer, ok := t.(*types.Pointer)
	if !ok {
		return false
	}
	named, ok := pointer.Elem().(*types.Named)
	if !ok || named.Obj() == nil || named.Obj().Pkg() == nil {
		return false
	}
	return named.Obj().Pkg().Path() == "testing" && named.Obj().Name() == typeName
}

func isGoTestName(name, prefix string) bool {
	if !strings.HasPrefix(name, prefix) {
		return false
	}
	if len(name) == len(prefix) {
		return true
	}
	r, _ := utf8.DecodeRuneInString(name[len(prefix):])
	return !unicode.IsLower(r)
}

func supportedTestCapabilities() []string {
	capabilities := []string{
		"T.Cleanup", "T.Error", "T.Errorf", "T.Fail", "T.FailNow", "T.Failed",
		"T.Fatal", "T.Fatalf", "T.Helper", "T.Log", "T.Logf", "T.Name", "T.Parallel",
		"T.Run", "T.Setenv", "T.Skip", "T.SkipNow", "T.Skipf", "T.Skipped", "T.TempDir", "M.Run",
		// T.Deadline reports when the package deadline (-timeout) expires. It was the LAST
		// unsupported member of Go 1.23's *testing.T surface, blocked only because a shim that
		// could not name a converted `time.Time` had nothing to return; the one-tree consolidation
		// (2026-08-01) gave core/testing a real `time` reference and the member landed with it
		// (core/testing/testing.cs Deadline + TestHost.PackageDeadlineUtc) — the capability list
		// simply was not widened to match. Six of context's cancellation tests were excluded for
		// want of it, including the whole tree-cancellation family. Roster impact measured before
		// widening (charter §9): the only validated package whose _test.go calls it is os/signal,
		// and both call sites are in `//go:build unix` files this platform never builds.
		"T.Deadline",
		// The testing.TB surface. A capability name is keyed on the RECEIVER's named type
		// (analyzeTestingCapabilities), so a helper declared `func h(t testing.TB)` records TB.Fatal
		// where the identical call on a *testing.T records T.Fatal — two rosters over ONE
		// implementation. Listing only the T spelling therefore excluded every test that funnels
		// through a same-package TB-typed helper, whole: os/exec's tests all reach
		// `exePath(t testing.TB)`, and 26 of them — every process-spawn shape the package has —
		// were gated out and had never run.
		//
		// What makes these honest is that nothing here is new behavior. "Supported" means three
		// things hold, and for TB all three already did: core/testing's TB interface declares the
		// member (Go 1.23's full 18, minus the unexported private()); go2cs-gen's ImplementGenerator
		// mints the `testing_TжTB` adapter the converter's `[assembly: GoImplement<T, TB>(Pointer =
		// true)]` record asks for, forwarding EVERY member to the package-scope T implementation
		// (`TB.Fatal(Span<object>) => testing_package.Fatal(m_box, args)` — verified against the
		// generated file, not assumed); and that implementation is the same TestExecution-backed one
		// T.Fatal has always answered, so a TB.FailNow throws the same TestAbortException and aborts
		// the same way. No member of Go's TB is absent: TB has no Run, Parallel or Deadline to want.
		//
		// The one declared limit, and it is a property of B rather than of TB: an adapter built from
		// a *testing.B forwards to B's compile-only no-ops. Benchmarks are never registered or run,
		// so the only path that puts a live B behind a TB parameter is a Test that calls
		// testing.Benchmark itself and passes the b onward — no suite does, and if one appears its
		// failure reports would be silently swallowed. That is a benchmark-execution question
		// (Phase 4D), not a reason to withhold the T-backed surface from every test that has one.
		"TB.Cleanup", "TB.Error", "TB.Errorf", "TB.Fail", "TB.FailNow", "TB.Failed",
		"TB.Fatal", "TB.Fatalf", "TB.Helper", "TB.Log", "TB.Logf", "TB.Name", "TB.Setenv",
		"TB.Skip", "TB.SkipNow", "TB.Skipf", "TB.Skipped", "TB.TempDir",
		"testing.AllocsPerRun", "testing.CoverMode", "testing.Short", "testing.Verbose",
		// testing.Testing reports whether the binary is a test binary. The host has implemented it
		// since the one-tree consolidation (core/testing/testing.cs) and the capability list simply
		// never named it -- an omission train 18 recorded in the same breath as the admission census
		// ("the host implements testing.Testing at testing.cs:672, but the capability list omits the
		// name"). Roster impact measured before widening, per the charter: a GOROOT-wide scan of
		// every *_test.go for `testing.Testing()` returns exactly ONE file, testing's own
		// testing_test.go, so no validated package gains or loses a test by this.
		"testing.Testing",
		// In-process benchmarking driven from a Test function: testing.Benchmark runs a
		// func(*B) closure and returns a BenchmarkResult, setting B.N and exposing NsPerOp
		// (unicode's TestCalibrate uses this to pick a linear-vs-binary search cutoff). The
		// host implements these (core/testing/testing.cs: Benchmark, B.N, BenchmarkResult).
		// Top-level BenchmarkXxx DECLARATIONS remain unsupported by their kind (they are never
		// registered — see the "benchmark" case in discoverTestDeclarations), so supporting
		// these members only unblocks Test functions that call testing.Benchmark themselves.
		"testing.Benchmark", "B.N", "BenchmarkResult.NsPerOp",
	}
	sort.Strings(capabilities)
	return capabilities
}

// unsupportedRuntimeCapabilities maps a SYMBOL — "<import path>.<func>" — to the NAME of the
// capability that symbol requires and that the managed runtime provably cannot provide. A test whose
// transitive closure reaches a listed symbol, or which IS one, is gated to `unsupported` by the SAME
// mechanism that gates an unsupported testing.* member: the capability name becomes a REQUIREMENT
// that supportedTestCapabilities deliberately does not list.
//
// The key is the symbol and the value is the capability because the two are not the same thing and
// the report needs the second. Several symbols can want one capability, and a capability that is a
// property of the HOST rather than of anything the test calls has no symbol to name at all — so a key
// may also name the TEST DECLARATION itself, which requiredFor honors by gating a listed function on
// its own account and not only on its callers'. What the manifest, the comparison and the proof page
// then show is the capability ("relocatable single-file test executable"), never the bare symbol.
//
// Why this exists as a gate rather than a runtime failure: an unimplemented assembly primitive
// throws a .NET NotImplementedException, and when the reaching path runs on a goroutine (a managed
// thread) that exception is unhandled and TERMINATES THE HOST — every test after it reports no
// result and the whole package reads as a mass infrastructure wall. sync's TestOnceFuncGoexit did
// exactly that: runtime.Goexit → getcallerpc, taking 28 of sync's 51 tests down with it. Declaring
// the capability unsupported is both more honest and more useful — the one test is excluded and
// disclosed by name, and the rest of the package is measurable.
//
// runtime.Goexit, the map's first and for a while only entry, graduated when the managed shape landed
// — an unwinding golib GoexitException that the defer machinery runs defers for, recover() cannot
// see, and the goroutine root swallows (docs/phase4/DESIGN-goexit.md, §2 + option C). Goexit from the
// MAIN goroutine is still unimplemented, but that case cannot be distinguished statically — a
// function's call graph says nothing about which goroutine will run it — so it is gated where the
// distinction actually exists, at runtime, by runtime/managed_impl.cs (a loud NotSupportedException,
// never a silent no-op).
//
// Add an entry ONLY for something provably unavailable, never for something merely unimplemented;
// and before adding one, scan every VALIDATED package for the symbol, since gating it removes those
// tests from the run set (the mirror of the widening trap in the charter's §9). Guarded by
// TestUnsupportedRuntimeCapabilityGate.
var unsupportedRuntimeCapabilities = map[string]string{
	// The block returned by CommandLineToArgv is OS-allocated and CALLER-FREED — every Go caller ends
	// `defer syscall.LocalFree(syscall.Handle(uintptr(unsafe.Pointer(argv))))`. Walking it needs a
	// managed materialization of the pointer array; taking the address of one hands back the GC-heap
	// data address (ж's pinnedArrayData path), so the deferred LocalFree would be asked to free GC
	// memory — a STATUS_HEAP_CORRUPTION process kill in place of a contained failure. Reading the
	// native block WITHOUT materializing it is the snapshot-pointer flavor golib does not have, and
	// which the 2026-08-02 ruling deferred to net's DNS work rather than mint for one test.
	"syscall.CommandLineToArgv": "native output block with caller-side LocalFree",

	// createMountPoint lays a *windows.MountPointReparseBuffer over a managed []byte and writes four
	// uint16 fields through it, then indexes &buf.PathBuffer[0] — a Go `[1]uint16` inline tail standing
	// in for however many kernel bytes follow. The conversion of that tail is an 8-byte MANAGED
	// REFERENCE, so no managed array reference can be laid over inline OS bytes: this is the raw-metal
	// arm of the S1 fork, whose remedy everywhere else is to hand-own the file — unavailable here
	// because the code is a TEST helper, which the converter regenerates by definition.
	"os_test.createMountPoint": "raw-metal struct overlay on managed bytes",

	// createSymbolicLink is createMountPoint's EXACT sibling, twenty lines down the same file and the
	// same shape: `(*windows.SymbolicLinkReparseBuffer)(unsafe.Pointer(&byteblob[0]))`, four uint16
	// fields written through the overlay, then `&buf.PathBuffer[0]` walked as `[2048]uint16` over a
	// `[1]uint16` inline tail. Identical class, identical impossibility, identical remedy-shaped hole
	// (regenerated TEST code, so no hand-own can exist), so listing it is CONSISTENCY with the entry
	// above rather than a new precedent — TestDirectoryJunction is already gated for exactly this.
	//
	// It surfaced only on 2026-08-29, and the reason is worth keeping: TestDirectorySymbolicLink never
	// REACHED it. The test's privilege preamble was failing first, because adjustTokenPrivileges handed
	// advapi32 a managed TOKEN_PRIVILEGES whose privilege slot is an `array<>` T[] reference, so the
	// kernel read a GC-heap address as the LUID and answered ERROR_NOT_ALL_ASSIGNED and the test SKIPPED
	// blaming the host. Repairing that (internal/syscall/windows/windows/zsyscall_windows_privilege_impl.cs)
	// let the test run on into this wall. One defect standing in front of another is why a skip whose
	// message names a host capability is worth measuring rather than believing.
	//
	// Registry doctrine's cross-package scan, run before adding this: `createSymbolicLink` exists at
	// exactly ONE site in all of GOROOT (os/os_windows_test.go:346) and is called only from lines 418
	// and 427, both inside TestDirectorySymbolicLink; the reparse-overlay shape itself appears in
	// exactly TWO GOROOT test sites, this one and the createMountPoint above. So this entry withdraws
	// exactly ONE row and reaches no other package. The two PRODUCTION consumers of
	// SymbolicLinkReparseBuffer (internal/syscall/windows/reparse_windows.go, os/file_windows.go) are
	// not test code and a test-declaration key cannot reach them.
	//
	// ⚠ NOT the durable answer. The real remedy is the byte-buffer-reinterpret fork at the CONVERTER
	// level — the same arc that owes NetShareAdd its repair — and this gate must retire when that
	// lands. The board entry stays OPEN.
	"os_test.createSymbolicLink": "raw-metal struct overlay on managed bytes",

	// The one entry that names a TEST rather than a symbol, because the impossibility is a property of
	// the host: the test copies os.Executable() — ONE file — into a temp directory 100 times and runs
	// each copy. os.Executable() is correct (it returns the apphost, os.tests.exe), but an apphost is a
	// stub bound at build time to a managed assembly of the same base name that must sit beside it, so
	// a single-file copy can never run: hostfxr answers 0x8000809a LibHostAppRootFindFailure, which is
	// byte-for-byte the code the test reports. Go's test binary is statically linked, which is the only
	// reason its premise holds there. Satisfying it means publishing every converted test host
	// self-contained single-file — ~70 MB and a publish rather than a build, per package.
	"os_test.TestRemoveAllWithExecutedProcess": "relocatable single-file test executable",

	// os/exec's TestCommand and TestLookPathWindows want this SAME capability from the other
	// direction — installExe (lp_windows_test.go) copies the running test executable into a
	// t.TempDir() tree and runs the copy — and they are NOT listed here. They are DISCLOSED
	// instead, under the host-limit class ruled 2026-08-15: src/core/os/exec's committed
	// go2cs_test_disclosures.json pins 25 leaf rows on `exit status 0x8000809a`, their 2 parents
	// ride the disclosed-parent aggregation, and os/exec banks at 74 matched + 27 disclosed. The
	// class and the bar an entry must clear are in docs/ConversionStrategies-Reference.md,
	// "host-limit — the third disclosed-divergence class". Gating them was measured FIRST and is
	// worse on three counts — the two below, plus that a gate hides the very rows whose future
	// passing is the only signal the limit has lifted
	// (docs/phase4/BOARD-next-validation-candidates.md, lane claude/os-exec-gate-bank):
	//
	//  1. A gate is DECLARATION-keyed and eligibleTerminalTestResults cuts a verdict row at its
	//     first "/", so 2 entries withdraw 40 rows rather than the 27 that were failing. The other
	//     13 are agreeing passes, and os/exec drops from 74 agreeing rows to 61.
	//  2. Gating the failures is what BREAKS it. os/exec's TestMain runs a helper-registry census
	//     guarded by `code == 0`, and the only callers of maySkipHelperCommand("printpath") are the
	//     two tests a gate removes — their file's init() still registers the helper. Green the
	//     suite and the census fires: `helper command unused: "printpath"`, exit 1, and the package
	//     validates at no count at all.
	//
	// os_test.TestRemoveAllWithExecutedProcess never showed (2) only because os's TestMain is a bare
	// Exit(m.Run()), and os is not yet on the roster — its disposition is decided when it banks.
	// The underlying hazard is unfixed and QUEUED rather than closed: a gate is invisible to the
	// running host, since nothing publishes the fact that a SUBSET ran where Go's own vocabulary for
	// it is a non-empty test.run. So any suite asserting that the whole suite ran will mis-answer
	// while a gate is active. Nothing is broken today (the only gated declarations live in os, whose
	// TestMain asserts nothing), but CHECK FOR SUCH A TestMain before adding a declaration-keyed
	// entry — and prefer a disclosure whenever the tests can still run.
	//
	// Checked for net/http: main_test.go's TestMain only runs goroutineLeaked() after m.Run() exits
	// 0 — a post-hoc stack census with no dependency on which tests ran, unlike os/exec's helper
	// registry. Gating testTransportGCRequest below removes it cleanly.

	// codegen-liveness: managed GC provably retains the object (wrapper-field round-trip trigger,
	// 10-variant isolation 2026-08-29). testTransportGCRequest registers a runtime.SetFinalizer on a
	// freshly built *Request, round-trips it through Transport.Do, then busy-polls runtime.GC() until
	// the finalizer closes a channel — Go's own long-standing, non-flaky liveness test. The converted
	// host never observes the finalizer: the request is genuinely retained, not merely slow to
	// collect, so no retry/patience window fixes it (this is `codegen-liveness`'s existing structural
	// bar — DESIGN-object-lifetime-disclosure.md §2's "genuinely unreachable" clause fails — not the
	// newer `object-lifetime` timing class, whose admission test this row cannot pass).
	//
	// A minimal, isolated repro (golib's actual SetFinalizer/GC() bridge bodies, copied verbatim; 10
	// variants; both Debug and Release; both configs identical) settled the mechanism and REFUTES the
	// dispatch's own working hypothesis: the IIFE-plus-polling-loop calling shape that
	// testTransportGCRequest itself uses collects cleanly in every structural variant tried (inline
	// poll, poll in a separate frame, no lambda at all — all ~15ms). What DOES reproduce, in six
	// independent variants (a parked background thread, a background thread that exits immediately,
	// BlockingCollection<T>, a plain Queue<T>, a bare static-field handoff with no collection type,
	// and — the minimal case — a same-thread, no-concurrency wrap-then-read) is: store the finalized
	// object into a wrapper object's field, then read that field back (`wrapper.Field.SomeProperty`).
	// That is exactly what the real persistConn.readLoop does with the request — repeatedly, through
	// the requestAndChan/transportRequest wrapper (Request.Method, Request.Close, .trace, .cancel(...))
	// — never a bare pass-by-argument. This is `codegen-liveness`'s FOURTH documented trigger shape
	// (the other three: a by-value slice header's address-exposed caller temp, a two-result call's
	// address-exposed temp, a frame-rooted large buffer) — new in KIND, same CLASS.
	//
	// Why a gate and not a disclosure, per the same fork os_test.TestRemoveAllWithExecutedProcess
	// took: the disclosure manifest pins a FAILURE's captured signature, and this test doesn't fail —
	// it HANGS, forever, with no output to pin. DESIGN-object-lifetime-disclosure.md §3c named this
	// exact gap against internal/weak's TestPointerFinalizer (structurally identical: a still-rooted
	// object whose finalizer a test blocks on forever) and left it to ⟨OQ-L3⟩, unruled until this row
	// forced it: candidate remedies were (a) a timeout-matched disclosure shape, or (b) a test-host
	// watchdog turning a per-test deadline into synthesizable failure text. Neither is built yet, so
	// this row gates on the existing declaration-keyed mechanism instead — the SAME fork
	// unsupportedRuntimeCapabilities has always drawn between "the tests can still run, disclose the
	// divergence" and "nothing to run, name the capability."
	//
	// ⚠ RETIREMENT: this entry retires the day either (⟨OQ-L3⟩'s remedy) a hang gets a disclosable
	// shape AND this row is re-pointed at that shape instead, or (unlikely, tracked for completeness)
	// a future CLR ships address-exposed-slot liveness precise enough to release a wrapper-stored
	// reference at last use rather than end of frame — the same condition DESIGN-object-lifetime-
	// disclosure.md ⟨OQ-L5⟩ already names for TestFreeOSMemory's sibling pin. Until either lands, do
	// not widen this entry to cover other codegen-liveness-shaped hangs by pattern-matching the
	// reason string; each gets its own entry with its own evidence, per the map's own discipline.
	"net/http_test.testTransportGCRequest": "codegen-liveness: managed GC provably retains the object (wrapper-field round-trip trigger, 10-variant isolation 2026-08-29)",

	// runtime-capability, and the second entry that names a TEST rather than a symbol — this time an
	// INTERNAL test (proto_test.go is `package pprof`), so the key is the bare import path: the
	// types.Package path of the internal test variant is runtime/pprof, not runtime/pprof_test, and
	// TestDeclarationKeyedCapabilityEntries pins the spelling per declared package kind.
	//
	// The gate exists because this test PASSES VACUOUSLY on the converted runtime, and a vacuous pass
	// is never a match (the bar internal/abi's TestFuncPC set when its banked `0 == 0` was retired to
	// `1 + 1 disclosed`). Read from Go's own source, not inferred: TestFakeMapping writes the heap
	// profile through Lookup("heap").WriteTo and asserts on the result. The converted
	// pprof_memProfileInternal is an honest zero-record reader — this runtime keeps no memory-profile
	// records, so the profile carries zero samples. The test's FIRST assertion, "want profile with at
	// least one mapping entry, got 0 mapping", can never fire on ANY platform: proto_other.go's
	// readMapping reads /proc/self/maps (or emits the fake entry as the empty fallback), so mappings
	// never come from samples and the profile always has at least one. Every REMAINING assertion
	// iterates prof.Location, which is EMPTY with zero samples — so the test measures nothing about
	// the reader and passes. Measured 2026-09-03 (gated before/after at 6fa031d08 → 3aa69f6e8,
	// Release+TC0, oracle go1.23.12): infrastructure-error → PASS with 23 real host mappings and zero
	// locations, while TestMemoryProfiler beside it honestly FAILS on `heap profile: 0: 0 [0: 1]`.
	//
	// Why a gate and not a disclosure, the same fork testTransportGCRequest took from the other side:
	// the disclosure manifest pins a FAILURE's signature, and this test does not fail — it cannot be
	// made to fail without editing it. So it goes through the capability gate, neither matched nor
	// disclosed-failing, listed with its capability (ruled (b), 2026-09-03; the two content failures
	// TestMemoryProfiler/debug=1 and /proto stay as honest Option-B disclosed rows, because they ARE
	// the deliverable: a stated, measurable, wrong answer where there was a host classification).
	//
	// Registry doctrine's checks, run before adding this: TestFakeMapping is declared at exactly ONE
	// GOROOT site (runtime/pprof/proto_test.go:426) and referenced nowhere else, and it has no
	// subtests — so this entry withdraws exactly ONE verdict row and reaches no other package.
	// runtime/pprof declares NO TestMain (the os/exec helper-registry hazard cannot fire).
	//
	// ⚠ RETIREMENT: this entry retires the day pprof_memProfileInternal returns real memory-profile
	// records — the same test then regains its teeth without anyone remembering, and the entry is
	// deleted in the same commit as that increment.
	"runtime/pprof.TestFakeMapping": "runtime-capability: the memory profiler records no samples on the converted runtime, so the test's mapping/symbolization loop runs over an empty location set (vacuous pass); lifts when an increment returns real memory-profile records",
	// ─── `testing`'s own suite ───────────────────────────────────────────────────────────────────
	//
	// Go's `testing` package is measured against the HAND-OWNED Phase-4 host (Option 1, owner-ruled
	// 2026-08-30; docs/phase4/CENSUS-testing-osuser-rows.md §2.4). Its external suite re-executes the
	// test binary and reads the child's TERMINAL TEXT back, and two families of that shape can produce
	// nothing a differential comparison should count. Both are gated rather than disclosed for the
	// reason this map's own fork draws: a disclosure PINS A FAILURE'S captured signature, and neither
	// family produces a failure to pin.
	//
	// FAMILY 1 — the race tests (ten). Each spawns a child and asserts `count(<literal>) == 0` where
	// the literal is Go's own race-report or verbose-run vocabulary ("race detected", "--- FAIL:",
	// "=== NAME"). With `race.Enabled == false` Go's own run wants zero too, so the shape LOOKS like
	// an agreeing pass — but the host's reporter (core/testing/TestReporter.cs, Report) writes
	// `"{ACTION,-20} {Test} — {Output}"` and emits none of those literals anywhere, so the count is
	// zero however the child behaves, INCLUDING when it never starts. A second mechanism guarantees
	// it independently: runTest passes `-test.bench` ahead of `-test.v`, TestOptions.Parse stops at
	// the first name it does not own and never writes back, so the child is non-verbose regardless.
	//
	// The distinction that puts these here while TestPanicHelper, TestCallRunInCleanupHelper and
	// TestGoexitInCleanupAfterPanicHelper stay ADMITTED (owner ruling, 2026-09-04): those three open
	// with `if os.Getenv("GO_WANT_HELPER_PROCESS") != "1" { return }` and do the same nothing on both
	// sides by design — an agreeing no-op is Go's row, not a false green, and their real assertions
	// run inside TestPanic/TestMorePanic's child where they are disclosed. The anti-laundering
	// clause reaches a pass THE HOST CANNOT FAIL — a real check on Go's side, an unwritable literal
	// on ours — not a pass neither side was meant to make.
	//
	// The unavailable capability is the one `runtime/race` is already ruled E1 for: there is no
	// race-instrumented build of the converted corpus, so the instrument these ten read is absent
	// rather than unimplemented.
	//
	// FAMILY 2 — the two running-tests tests. `parseRunningTests` scrapes Go's -test.timeout dump
	// ("running tests:\n\tTestX (Nms)") out of the child; on no match the parent DOUBLES the timeout
	// and loops, with no failure path anywhere in it. The host emits no such dump, so the loop never
	// terminates — the same "it doesn't fail, it HANGS, with no output to pin" fork the net/http
	// entry above draws, and a package deadline swallowing the rest of the row is what a gate here
	// prevents.
	//
	// CHECKED, as the note above requires before any declaration-keyed entry: `testing`'s own
	// TestMain (testing_test.go:27) is `if os.Getenv("GO_WANT_RACE_BEFORE_TESTS") == "1" { doRace() }`
	// then a bare `m.Run()` — it censuses nothing about which tests ran and does not even propagate
	// an exit code, so a gate cannot make it mis-answer the way os/exec's helper census does.
	"testing_test.TestRaceReports":                        "race-detector-instrumented build: asserts a count of \"race detected\" in a re-exec'd child, a literal the host's reporter never writes",
	"testing_test.TestRaceName":                           "race-detector-instrumented build: asserts the verbose-run marker \"=== NAME\" is absent from a re-exec'd child, a literal the host's reporter never writes",
	"testing_test.TestRaceSubReports":                     "race-detector-instrumented build: asserts counts of \"race detected during execution of test\" and \"--- FAIL:\" in a re-exec'd child, literals the host's reporter never writes",
	"testing_test.TestRaceInCleanup":                      "race-detector-instrumented build: asserts counts of \"race detected during execution of test\" and \"--- FAIL:\" in a re-exec'd child, literals the host's reporter never writes",
	"testing_test.TestDeepSubtestRace":                    "race-detector-instrumented build: asserts a count of \"race detected\" in a re-exec'd child, a literal the host's reporter never writes",
	"testing_test.TestRaceDuringParallelFailsAllSubtests": "race-detector-instrumented build: asserts a count of \"race detected\" in a re-exec'd child, a literal the host's reporter never writes",
	"testing_test.TestRaceBeforeParallel":                 "race-detector-instrumented build: asserts a count of \"race detected\" in a re-exec'd child, a literal the host's reporter never writes",
	"testing_test.TestRaceBeforeTests":                    "race-detector-instrumented build: asserts a count of \"race detected\" in a child run with GO_WANT_RACE_BEFORE_TESTS=1, a literal the host's reporter never writes",
	"testing_test.TestBenchmarkRace":                      "race-detector-instrumented build: asserts a count of \"race detected\" in a re-exec'd child running a benchmark, a literal the host's reporter never writes",
	"testing_test.TestBenchmarkSubRace":                   "race-detector-instrumented build: asserts a count of \"race detected\" in a re-exec'd child running a benchmark, a literal the host's reporter never writes",
	"testing_test.TestRunningTests":                       "Go's -test.timeout running-tests dump: the parent retries with a doubled timeout until the child prints it and has no failure path, so a host that does not emit the dump makes the test loop forever rather than fail",
	"testing_test.TestRunningTestsInCleanup":              "Go's -test.timeout running-tests dump: the parent retries with a doubled timeout until the child prints it and has no failure path, so a host that does not emit the dump makes the test loop forever rather than fail",
}

// unsupportedRuntimeCapability reports whether fn requires a listed unsupported runtime capability,
// returning the capability name used in the requirement set.
func unsupportedRuntimeCapability(fn *types.Func) (string, bool) {
	if fn == nil || fn.Pkg() == nil || fn.Type() == nil {
		return "", false
	}

	// Package-scope functions only — a method named Goexit on some type is not runtime.Goexit.
	if sig, ok := fn.Type().(*types.Signature); ok && sig.Recv() != nil {
		return "", false
	}

	capability, blocked := unsupportedRuntimeCapabilities[fn.Pkg().Path()+"."+fn.Name()]

	return capability, blocked
}

// testCapabilityAnalysis is the per-variant capability attribution input (F4): the testing.*
// members each function uses DIRECTLY, and the static same-package reference graph used to close
// over helpers. References are collected conservatively (any use of a same-package function, not
// just direct calls), so a capability reached through a stored function value still gates the
// test that stores it; cross-package helpers (e.g. internal/testenv) are outside the graph and
// gate through their own package's conversion instead.
type testCapabilityAnalysis struct {
	direct   map[*types.Func]HashSet[string]
	referees map[*types.Func]map[*types.Func]bool
}

// analyzeTestingCapabilities walks every function declaration in the variant (production and
// test files alike — helpers can live in either) recording direct testing.* usage and the
// same-package reference graph. The receiver filter is deliberately absent (F5): a helper taking
// *testing.B contributes B.* requirements, so a supported-kind test calling it is gated instead
// of sailing through.
func analyzeTestingCapabilities(pkg *packages.Package) testCapabilityAnalysis {
	analysis := testCapabilityAnalysis{
		direct:   make(map[*types.Func]HashSet[string]),
		referees: make(map[*types.Func]map[*types.Func]bool),
	}

	for _, file := range pkg.Syntax {
		for _, decl := range file.Decls {
			fn, ok := decl.(*ast.FuncDecl)
			if !ok || fn.Name == nil {
				continue
			}

			obj, ok := pkg.TypesInfo.Defs[fn.Name].(*types.Func)
			if !ok {
				continue
			}

			direct := HashSet[string]{}
			referees := make(map[*types.Func]bool)

			if fn.Body != nil {
				ast.Inspect(fn.Body, func(node ast.Node) bool {
					switch expr := node.(type) {
					case *ast.SelectorExpr:
						if selection := pkg.TypesInfo.Selections[expr]; selection != nil {
							member := selection.Obj()
							if member == nil || member.Pkg() == nil || member.Pkg().Path() != "testing" {
								return true
							}

							receiver := selection.Recv()
							if pointer, ok := receiver.(*types.Pointer); ok {
								receiver = pointer.Elem()
							}
							if named, ok := receiver.(*types.Named); ok && named.Obj().Pkg() != nil && named.Obj().Pkg().Path() == "testing" {
								direct.Add(named.Obj().Name() + "." + member.Name())
							}
						} else if member := pkg.TypesInfo.Uses[expr.Sel]; member != nil && member.Pkg() != nil && member.Pkg().Path() == "testing" {
							if _, ok := member.(*types.Func); ok {
								direct.Add("testing." + member.Name())
							}
						}
					case *ast.Ident:
						used, ok := pkg.TypesInfo.Uses[expr].(*types.Func)

						if !ok {
							return true
						}

						if used.Pkg() == pkg.Types {
							referees[used] = true
						}

						// A RUNTIME capability the managed model cannot provide is recorded the
						// same way a testing.* member is, and gates the same way (it is absent
						// from supportedTestCapabilities). This is keyed on the resolved OBJECT,
						// so it catches the call however it is spelled or aliased.
						if capability, blocked := unsupportedRuntimeCapability(used); blocked {
							direct.Add(capability)
						}
					}
					return true
				})
			}

			analysis.direct[obj] = direct
			analysis.referees[obj] = referees
		}
	}

	return analysis
}

// requiredFor returns the transitive testing.* capability requirements of fn — its own direct
// usage plus that of every same-package function reachable through the reference graph.
func (a testCapabilityAnalysis) requiredFor(fn *types.Func) HashSet[string] {
	required := HashSet[string]{}
	visited := make(map[*types.Func]bool)

	var walk func(current *types.Func)
	walk = func(current *types.Func) {
		if visited[current] {
			return
		}
		visited[current] = true

		// A listed unsupported capability gates the function that REQUIRES it on its own account,
		// not only its callers'. The caller-side arm (analyzeTestingCapabilities, which records the
		// requirement at every ident that names a listed symbol) cannot reach the case where the
		// requirement belongs to the test itself: nothing names a test, so nothing records it. That
		// is the shape a HOST capability takes — the test calls no impossible function, it merely
		// assumes something of the binary it runs in.
		if capability, blocked := unsupportedRuntimeCapability(current); blocked {
			required.Add(capability)
		}

		if direct, ok := a.direct[current]; ok {
			required.UnionWithSet(direct)
		}
		for referee := range a.referees[current] {
			walk(referee)
		}
	}

	walk(fn)
	return required
}

func writeTestHost(outputPath, namespace, importPath string, declarations []testDeclaration, testMain *testDeclaration, fixtures, fixtureDirectories, fixtureLinks []string) error {
	var b strings.Builder
	b.WriteString("// Code generated by go2cs test conversion. DO NOT EDIT.\r\n")
	b.WriteString(fmt.Sprintf("namespace %s;\r\n\r\n", namespace))
	// Emitted INSIDE `namespace go.<pkg>;`, so the leading `go` re-binds to a `go.go` namespace
	// whenever the test closure pulls in a go/* package (math/rand/v2's regress_test.go imports
	// go/format) — CS0234. packageChildNamespaces still holds the last converted test variant's
	// closure here, which is the closure of the assembly this host compiles into.
	b.WriteString(fmt.Sprintf("using %s;\r\n\r\n", globalQualifyRooted(RootNamespace+".testing_runtime")))
	b.WriteString("internal static class Go2CsTestHost\r\n{\r\n")
	b.WriteString("    public static int Main(string[] args)\r\n    {\r\n")
	b.WriteString(fmt.Sprintf("        TestRegistry registry = new(\"%s\", new string[]\r\n        {\r\n", escapeCSharp(importPath)))
	for _, fixture := range fixtures {
		b.WriteString(fmt.Sprintf("            \"%s\",\r\n", escapeCSharp(filepath.ToSlash(fixture))))
	}
	// Each trailing list is OMITTED when it is empty AND every list after it is empty too — which is
	// most packages — so their host stays byte-identical to what a converter without these
	// capabilities emitted (TestRegistry defaults both parameters). An emitted-but-empty array would
	// churn every banked host for a run environment that is unchanged. A package with links but no
	// subdirectories cannot occur (a link IS a subdirectory of the package or of one), but the empty
	// initializer is emitted rather than assumed away, because a positional parameter list cannot
	// skip one.
	if len(fixtureDirectories) > 0 || len(fixtureLinks) > 0 {
		b.WriteString("        }, new string[]\r\n        {\r\n")
		for _, directory := range fixtureDirectories {
			b.WriteString(fmt.Sprintf("            \"%s\",\r\n", escapeCSharp(filepath.ToSlash(directory))))
		}
	}

	if len(fixtureLinks) > 0 {
		b.WriteString("        }, new string[]\r\n        {\r\n")
		for _, link := range fixtureLinks {
			b.WriteString(fmt.Sprintf("            \"%s\",\r\n", escapeCSharp(filepath.ToSlash(link))))
		}
	}

	b.WriteString("        });\r\n")

	for _, test := range declarations {
		if test.Kind != "test" || test.Status != "included" {
			continue
		}
		className := test.CSharpClassName
		if className == "" {
			className = getSanitizedImport(test.PackageName + PackageSuffix)
		}
		methodName := getSanitizedFunctionName(test.Name)
		b.WriteString(fmt.Sprintf("        registry.Add(\"%s\", %s.%s, \"%s\", %d);\r\n", escapeCSharp(test.Name), className, methodName, escapeCSharp(test.Source), test.Line))
	}

	if testMain != nil && testMain.Status == "included" {
		className := testMain.CSharpClassName
		if className == "" {
			className = getSanitizedImport(testMain.PackageName + PackageSuffix)
		}
		b.WriteString(fmt.Sprintf("        registry.SetTestMain(%s.%s);\r\n", className, getSanitizedFunctionName(testMain.Name)))
	}

	b.WriteString("        return TestHost.Run(registry, args);\r\n")
	b.WriteString("    }\r\n}\r\n")

	contents := []byte(b.String())
	fileName := filepath.Join(outputPath, testHostFileName)
	if needToWriteFile(fileName, contents) {
		return os.WriteFile(fileName, contents, 0644)
	}
	return nil
}

// writeTestProject emits the test project from the embedded test-csproj-template.xml (following
// the csproj-template.xml precedent). The template carries the static machinery (explicit
// compile items via EnableDefaultCompileItems=false, generated-files exposure, the go2csPath
// fallback chain with the $(HOME) non-Windows fallback, the Go type-alias usings); the markers
// carry the per-project values.
// testProjectFixedReferences are the references EVERY converted test project carries regardless of
// what the package under test imports: the shared runtime, and the hand-owned `testing` package
// that hosts the run. Both are rooted in the one converted-standard-library tree at
// $(go2csPath)core — the same root every resolved dependency reference uses.
var testProjectFixedReferences = []string{
	`$(go2csPath)core/golib/golib.csproj`,
	`$(go2csPath)core/testing/testing.csproj`,
}

func writeTestProject(projectFile, projectName, namespace string, model testProjectModel, productionFiles, testFiles, fixtures, dependencies []string, options Options) error {
	references := HashSet[string]{}

	for _, fixed := range testProjectFixedReferences {
		// A fixed reference that IS the package under test duplicates the production reference
		// below by a SECOND SPELLING, and a HashSet of strings cannot see that
		// `$(go2csPath)core/testing/testing.csproj` and the colocated `testing.csproj` name one
		// file. Only a hand-owned HOST row can reach this — the fixed set is golib and testing —
		// and `testing` does: measured by a convert-only probe (2026-09-04), which emitted BOTH
		// spellings into testing.tests.csproj.
		//
		// The colocated form is the one kept, deliberately: the -tests contract colocates the test
		// project with the production csproj, so that spelling is layout-independent and involves
		// no $(go2csPath) tree mapping — which is the reason the reference model emits it at all.
		//
		// isSelfProjectReference is the predicate the DEPENDENCY loop below already applies for
		// exactly this reason. The fixed set never consulted it because, until a hand-owned host
		// became a test TARGET, no fixed reference could name the package under test.
		if isSelfProjectReference(fixed, projectName) {
			continue
		}

		references.Add(fixed)
	}

	// REFERENCE model: the production package compiles ONLY in its own project; reference it so
	// its assembly stays the single identity for the production types. Colocated-relative — the
	// -tests contract colocates the test project with the production csproj — so the reference
	// is layout-independent (no $(go2csPath) tree mapping involved).
	if model.referencesProduction() {
		references.Add(projectFileBaseName(projectName) + ".csproj")
	}

	for _, dependency := range dependencies {
		for _, info := range getImportPackageInfo([]string{dependency}, options) {
			// A dependency that fails to resolve must fail the conversion NAMING the dependency
			// (F14b) — silently dropping the reference would surface later as an uncaused CS0246.
			if info.Err != nil {
				return fmt.Errorf("resolve test project dependency %q: %w", dependency, info.Err)
			}

			reference := info.ProjectReference
			if reference != "" && !isSelfProjectReference(reference, projectName) {
				references.Add(reference)
			}
		}
	}

	// The template's last-resort go2csPath fallback must be a COMPLETE property value: an
	// $(MSBuildThisFileDirectory)-anchored relative walk-up when one exists, else the absolute
	// path on its own. filepath.Rel fails across Windows drive letters (an H:\ checkout with the
	// default C:\Users\...\go2cs), and concatenating the absolute after the MSBuild prefix
	// produced an unresolvable garbage path — the bare-clone CS0246 golib failure.
	relativeGo2CSPath, relErr := filepath.Rel(filepath.Dir(projectFile), options.go2csPath)
	if relErr == nil {
		relativeGo2CSPath = "$(MSBuildThisFileDirectory)" + strings.TrimRight(filepath.ToSlash(relativeGo2CSPath), "/") + "/"
	} else {
		relativeGo2CSPath = strings.TrimRight(filepath.ToSlash(options.go2csPath), "/") + "/"
	}

	var compileItems strings.Builder
	compileFiles := append([]string{}, testFiles...)

	// The production sources are compile items only under the RECOMPILE model; the reference
	// model binds them through the production project reference above instead.
	if !model.referencesProduction() {
		compileFiles = append(compileFiles, productionFiles...)
	}

	compileFiles = append(compileFiles, testPackageInfoFileName, testHostFileName)
	sort.Strings(compileFiles)
	for _, file := range compileFiles {
		compileItems.WriteString(fmt.Sprintf("\r\n    <Compile Include=\"%s\" />", escapeXMLAttributeValue(filepath.ToSlash(file))))
	}

	var fixtureItems strings.Builder
	for _, fixture := range fixtures {
		slashed := filepath.ToSlash(fixture)

		// A fixture ABOVE the package ("../testdata/e.txt") needs an explicit <Link>: MSBuild's
		// default link for a `..`-relative item is its BARE FILE NAME, which both flattens the two
		// `testdata` trees into one and drops the relative shape the test's own open() needs. Link
		// it into a staging root under the output directory, keyed by how far up it reaches, and
		// TestHost.CopyFixtures maps it back to the true relative path inside the run sandbox
		// (SharedFixtureStagingRoot there — keep the two in sync).
		// ExcludeFromSingleFile keeps a fixture LOOSE beside the published executable, and it is
		// what makes a fixture survive being published TWICE into one directory. Measured
		// 2026-08-29, three publishes into the same output, counting `testdata/`:
		//
		//     single-file, as emitted before this            4 -> 0 -> 0   (exit 0 every time)
		//     -p:PublishSingleFile=false                     4 -> 4 -> 4
		//     single-file + ExcludeFromSingleFile="true"     4 -> 4 -> 4
		//
		// So the deleter is the single-file BUNDLER, not the copy: on a republish it reclaims the
		// output directory for the files it owns and takes the loose content with it. The first
		// publish into a fresh directory is always correct, which is exactly why this hid — every
		// bank re-converts fresh, and only a re-measurement over an existing publish tree can see
		// it (R's isolation, both platforms, 2026-08-29). CopyToPublishDirectory does NOT fix it;
		// measured too, and left out for that reason — the item was already reaching publish, the
		// bundler was removing it afterwards.
		//
		// A fixture must be loose because a test opens it by relative path (`os.Open("testdata/x")`)
		// — bundling it into the executable would take it out of the filesystem the test reads.
		if up, tail, isShared := sharedFixtureStagingParts(slashed); isShared {
			fixtureItems.WriteString(fmt.Sprintf("\r\n    <None Include=\"%s\" Link=\"%s/up%d/%s\" CopyToOutputDirectory=\"PreserveNewest\" ExcludeFromSingleFile=\"true\" />",
				escapeXMLAttributeValue(slashed), SharedFixtureStagingRoot, up, escapeXMLAttributeValue(tail)))
			continue
		}

		fixtureItems.WriteString(fmt.Sprintf("\r\n    <None Include=\"%s\" CopyToOutputDirectory=\"PreserveNewest\" ExcludeFromSingleFile=\"true\" />", escapeXMLAttributeValue(slashed)))
	}

	var referenceItems strings.Builder
	refs := references.Keys()
	sort.Strings(refs)
	for _, reference := range refs {
		// Forward slashes on every host, matching the production writer (see F5): a resolved
		// dependency arrives already slashed from emittedProjectReference, but an ABSOLUTE
		// reference (a local module) is OS-native.
		referenceItems.WriteString(fmt.Sprintf("\r\n    <ProjectReference Include=\"%s\" />", escapeXMLAttributeValue(filepath.ToSlash(reference))))
	}

	contents := []byte(strings.NewReplacer(
		TestRootNamespaceMarker, namespace,
		TestAssemblyNameMarker, projectName+".tests",
		TestGo2CSRelativePathMarker, escapeXMLAttributeValue(relativeGo2CSPath),
		TestCompileItemsMarker, compileItems.String(),
		TestFixtureItemsMarker, fixtureItems.String(),
		TestProjectReferencesMarker, referenceItems.String(),
	).Replace(string(testCsprojTemplate)))

	if needToWriteFile(projectFile, contents) {
		return os.WriteFile(projectFile, contents, 0644)
	}
	return nil
}

// aliasReferenceImports returns the import paths of converted packages that `using` ALIASES in
// the scanned files target but that the test project does not directly reference (B2c). Both the
// test metadata files AND the converted test sources are scanned: a seeded global alias, or a
// file-local package-qualifier using emitted into a *_test.cs, can target an assembly the package
// reaches only transitively — including one no import list mentions, when a test-only helper
// RETURNS a type from it — and DisableTransitiveProjectReferences (B2b) hides such assemblies
// from the test compile view, so the alias line itself fails (CS0234). Candidates
// come from the module-aware TRANSITIVE import closure captured at load time
// (importPackageDirs), whose namespace tokens are rendered by the same machinery that emitted
// the aliases — including the /vN major-version collapse — so matching is exact. When several
// closure paths render the same token (math/rand beside math/rand/v2), the lexically first is
// taken, deterministically.
// testProjectAliasScanFiles returns the files whose emitted `using` aliases and conversion records
// the B2c project-reference scan must read: EVERY C# source the test project compiles, plus the two
// metadata files.
//
// B2c: a seeded/merged `using` ALIAS in the test metadata — or a package-qualifier `using` emitted
// into a converted SOURCE — can target an assembly the package reaches only TRANSITIVELY (sort's
// `global using reflectliteꓸKind = go.@internal.abi_package.ΔKind;` targets internal/abi via
// sort → reflectlite → abi; math/rand's default_test.cs needs os/exec purely because
// testenv.Command RETURNS *exec.Cmd, so "os/exec" appears in no import list), which
// DisableTransitiveProjectReferences (B2b) hides from the test compile view. The manifest's
// dependency list stays import-derived — alias targets are purely a project-reference concern.
//
// The PRODUCTION sources belong in that scan under the RECOMPILE model for exactly the same reason
// the test sources do, and for no other: there they are compile items of the test assembly (see
// writeTestProject), so an alias one of them emits is a reference the TEST project owns. That
// production sources were omitted was a scan-set gap, not a different rule — and the omission is
// invisible in the ordinary case because a production file's aliases are usually its own package's
// direct imports, which `dependencies` already carries. It bites where the alias names a package
// the production half reaches only transitively: `crypto/x509`'s x509.cs and pem_decrypt.cs emit
// `using hash = hash_package;` (crypto.Hash.New() RETURNS hash.Hash — `hash` is in no import list
// of x509 and in no reference of its own production csproj, which compiles only because it does NOT
// disable transitive references), and the test build failed CS0246 inside the PRODUCTION files.
//
// Under the REFERENCE model the production sources compile in their own project and are bound
// through its assembly reference, so their aliases are that project's concern; scanning them here
// would add references the test project does not need.
func testProjectAliasScanFiles(model testProjectModel, outputPath, testInfoPath string, testFiles, productionFiles []string) []string {
	scanFiles := []string{testInfoPath, filepath.Join(outputPath, externalTestPackageInfoFileName)}

	for _, testFile := range testFiles {
		scanFiles = append(scanFiles, filepath.Join(outputPath, testFile))
	}

	if !model.referencesProduction() {
		for _, productionFile := range productionFiles {
			scanFiles = append(scanFiles, filepath.Join(outputPath, productionFile))
		}
	}

	return scanFiles
}

func aliasReferenceImports(infoFiles []string, productionPkgPath string, directDependencies []string) []string {
	direct := NewHashSet(directDependencies)
	tokens := make(map[string][]string)
	bareTokens := make(map[string][]string)

	for importPath := range importPackageDirs {
		if importPath == productionPkgPath || importPath == "testing" || direct.Contains(importPath) {
			continue
		}

		namespace := convertImportPathToNamespace(importPath, PackageSuffix)

		token := RootNamespace + "." + namespace
		tokens[token] = append(tokens[token], importPath)

		// A SINGLE-SEGMENT package emits its alias UNROOTED — `using hash = hash_package;` inside
		// `namespace go.math.rand`, where C#'s outward lookup finds the class in the enclosing root
		// namespace without a qualifier. The rooted token above never matches such a line, so the
		// reference went missing and DisableTransitiveProjectReferences turned it into CS0246
		// (math/rand/v2's chacha8_test.cs: `sha256.New()` RETURNS hash.Hash, so `hash` appears in no
		// import list and only this alias scan can find it). Multi-segment namespaces always emit
		// with at least their leading package segment, so the rooted token still covers them.
		// Matched on a SEGMENT boundary (see bareTokens below) — a substring test would let
		// `hash_package` match `go.hash.maphash_package` and pull in a package nothing references.
		if !strings.Contains(namespace, ".") {
			bareTokens[namespace] = append(bareTokens[namespace], importPath)
		}
	}

	found := HashSet[string]{}

	for _, infoFile := range infoFiles {
		data, err := os.ReadFile(infoFile)
		if err != nil {
			continue
		}

		for _, line := range strings.Split(string(data), "\n") {
			trimmed := strings.TrimSpace(strings.TrimSuffix(line, "\r"))

			for _, target := range referenceScanTargets(trimmed) {
				for token, paths := range tokens {
					// Three match shapes for a multi-segment package's alias target:
					//   Contains(target, token+".")  — token is a leading/middle namespace segment.
					//   HasSuffix(target, token)      — target ends with the fully-ROOTED token,
					//                                    e.g. `go.os.exec_package` or `global::go.os.exec_package`
					//                                    (math/rand's default_test.cs, emitted from namespace go.math).
					//   HasSuffix(token, "."+target)  — target is the UNROOTED tail of the rooted token,
					//                                    e.g. `os.exec_package` matching token `go.os.exec_package`.
					//                                    A test emitted inside a namespace that SHADOWS the root
					//                                    `go` (go/doc/comment's std_test.cs in namespace go.go.doc,
					//                                    internal/abi's abi_test.cs in go.@internal) emits the alias
					//                                    unrooted and relies on C# outward lookup; the single-segment
					//                                    bareTokens path below never covers a multi-segment tail.
					//                                    Anchored on the leading "." so `os.exec_package` cannot
					//                                    match an unrelated `go.xos.exec_package`.
					if rootedQualifierMatch(target, token) {
						sort.Strings(paths)
						found.Add(paths[0])
					}
				}

				for token, paths := range bareTokens {
					if bareQualifierMatch(target, token) {
						sort.Strings(paths)
						found.Add(paths[0])
					}
				}
			}
		}
	}

	result := found.Keys()
	sort.Strings(result)

	return result
}

// rootedQualifierMatch reports whether a rendered qualifier TARGET names the package class spelled
// by a fully-ROOTED token (`go.os.exec_package`). Three shapes, all of them observed in emitted
// output — see the call site in aliasReferenceImports for which emission produces each.
func rootedQualifierMatch(target, token string) bool {
	return strings.Contains(target, token+".") || strings.HasSuffix(target, token) || strings.HasSuffix(token, "."+target)
}

// bareQualifierMatch is the same test for a SINGLE-SEGMENT package, whose alias is emitted
// UNROOTED (`using hash = hash_package;`) and found by C#'s outward lookup. Matched on a segment
// boundary so `hash_package` cannot match `go.hash.maphash_package`.
func bareQualifierMatch(target, token string) bool {
	return target == token || strings.HasPrefix(target, token+".")
}

// qualifierTargetNamesPackage reports whether a rendered qualifier TARGET names importPath's
// package class, in either spelling. The inverse direction of aliasReferenceImports' index: there
// the target is matched against every known import path, here against ONE already in hand (the
// closure walk knows its candidate interface's package and only asks whether a record named it).
func qualifierTargetNamesPackage(target, importPath string) bool {
	namespace := convertImportPathToNamespace(importPath, PackageSuffix)

	if rootedQualifierMatch(target, RootNamespace+"."+namespace) {
		return true
	}

	return !strings.Contains(namespace, ".") && bareQualifierMatch(target, namespace)
}

// packageImplementBases returns, per declared TYPE, the package-class qualifiers of the interfaces
// that package's VALUE-form `[assembly: GoImplement<T, I>]` records name — the base list go2cs-gen
// realizes on the type inside its own assembly, and therefore what binding a member on it must
// resolve (see declarationClosureImports' implementEdge).
//
// POINTER-form records are excluded by the parser: they generate an adapter CLASS (`FileжWriter`)
// rather than a base on the type, so they place no demand on a member binding — os records `File`
// as io/fs.File and io.Writer that way, and thirteen banked projects bind `os.File` members with
// neither reference.
//
// A record naming an interface the recording package DECLARES ITSELF (`error`, `FileInfo`) carries
// no `<pkg>_package` qualifier, so it contributes nothing and needs nothing: a same-package base
// is in the assembly already being referenced.
func packageImplementBases(packageInfoFile string) map[string][]string {
	lines, err := readPackageInfoLines(packageInfoFile)

	if err != nil {
		return nil
	}

	bases := map[string][]string{}

	for _, pair := range parseExportedValueImplementLines(lines) {
		qualifier := packageQualifierPattern.FindStringSubmatch(pair[1])

		if qualifier == nil {
			continue
		}

		bases[recordTypeGoName(pair[0])] = append(bases[recordTypeGoName(pair[0])], qualifier[1])
	}

	return bases
}

// foreignImplementBasesResolver returns the per-package record lookup declarationClosureImports'
// implementEdge uses for a type declared OUTSIDE the roots: the same packageImplementBases parse,
// pointed at the DECLARING package's own emitted package_info.cs in the runtime root rather than at
// the package under test's. Resolution reuses getImportPackageInfo, so it follows exactly the route
// the emitted `<ImportedTypeAliases>` block already reads a dependency's metadata by — including
// layout L3's per-GOOS placement (platformPackageInfoPath).
//
// Memoized per import path because the walk asks once per NAMED TYPE and a suite mentions the same
// few dependencies repeatedly. A package with no readable package_info.cs — never converted, or a
// runtime root that does not hold it — caches an empty map and simply contributes no edge, which is
// the same silent-nothing the root lookup produces from an unreadable file and is why the resolved
// root gets the loud once-per-run warning documented in CLAUDE.md.
func foreignImplementBasesResolver(options Options) func(string) map[string][]string {
	cache := map[string]map[string][]string{}

	return func(importPath string) map[string][]string {
		if bases, ok := cache[importPath]; ok {
			return bases
		}

		bases := map[string][]string{}

		for _, info := range getImportPackageInfo([]string{importPath}, options) {
			if info.Err != nil || len(info.TargetDir) == 0 {
				continue
			}

			if parsed := packageImplementBases(platformPackageInfoPath(info.TargetDir, goosOfTarget(options.targetPlatform))); parsed != nil {
				bases = parsed
			}
		}

		cache[importPath] = bases

		return bases
	}
}

// recordTypeGoName recovers the Go type name a record's IMPLEMENTATION side was emitted from, by
// undoing the spellings identifierNaming adds: the `@` keyword escape, the `Δ` reserved/collision
// prefix and its `ᴛ` type marker, and a generic instantiation's argument list
// (`nistCurve<Point>` → `nistCurve`). Two Go types in one package cannot normalize to one name —
// the markers exist to separate a type from a METHOD, never from another type — so the recovery is
// unambiguous where it is used, and an unrecovered name can only cost the edge a reference the
// build then demands loudly.
func recordTypeGoName(recorded string) string {
	if open := strings.Index(recorded, "<"); open >= 0 {
		recorded = recorded[:open]
	}

	recorded = removeLeadingSanitizationMarker(recorded)
	recorded = strings.TrimSuffix(strings.TrimPrefix(recorded, ShadowVarMarker), TempVarMarker)

	return recorded
}

// conversionRecordPrefixes are the emitted assembly-attribute line prefixes whose GENERIC ARGUMENT
// LIST names converted types that the test compilation must be able to BIND — go2cs-gen realizes
// each record into a generated adapter/partial/operator, so an unreferenced assembly on either side
// is CS0246 at the attribute itself.
var conversionRecordPrefixes = []string{"[assembly: GoImplement<", "[assembly: GoImplicitConv<"}

// packageQualifierPattern captures the PACKAGE-CLASS qualifier of a rendered type reference —
// everything up to and including the first segment that ends in PackageSuffix (`io_package`,
// `go.io.fs_package`, `go.@internal.abi_package`). Deliberately stops at the package class rather
// than taking the whole type reference, so the captured text has exactly the shape a `using` alias
// TARGET has and the same token matcher decides both.
var packageQualifierPattern = regexp.MustCompile(`(?:global::)?((?:@?[\p{L}_][\p{L}\p{N}_]*\.)*@?[\p{L}_][\p{L}\p{N}_]*` + PackageSuffix + `)`)

// referenceScanTargets returns the reference TARGETS a scanned metadata/source line contributes to
// the B2c project-reference augmentation.
//
// Two line shapes carry a cross-assembly type reference that no import list mentions:
//
//   - a `using` ALIAS (`global using reflectliteꓸKind = go.@internal.abi_package.ΔKind;`) — the
//     alias target itself, handled since B2c.
//
//   - an emitted CONVERSION RECORD (`[assembly: GoImplement<strings_package.Builder,
//     io_package.Writer>(Pointer = true)]`). The converter records an interface pair from a
//     type's USE — os/signal's test reaches `cmd.Stdout = &buf`, whose os/exec field type names
//     io.Writer — so the interface side can belong to a package that appears in NO import list of
//     either the production package or its tests, and DisableTransitiveProjectReferences (B2b)
//     then hides it (CS0246 on package_test_info.cs itself, plus a cascading go2cs-gen
//     CS8785 "second generic type argument must be an interface" once the interface fails to bind).
//     Only the generic argument list is scanned — an attribute's `(Pointer = true)` /
//     `(ValueType = "…")` payload is metadata, not a bindable reference.
func referenceScanTargets(line string) []string {
	if strings.HasPrefix(line, "global using ") || strings.HasPrefix(line, "using ") {
		if strings.HasPrefix(line, "using static ") || !strings.Contains(line, "=") {
			return nil
		}

		_, target, _ := strings.Cut(line, "=")

		return []string{strings.TrimSuffix(strings.TrimSpace(target), ";")}
	}

	isRecord := false

	for _, prefix := range conversionRecordPrefixes {
		if strings.HasPrefix(line, prefix) {
			isRecord = true
			break
		}
	}

	if !isRecord {
		return nil
	}

	// Span the record's generic argument list — first '<' to LAST '>' — so a nested generic
	// (`GoImplicitConv<Δindirect<K, V>, ж<Δindirect<K, V>>>`) is covered whole.
	open := strings.Index(line, "<")
	end := strings.LastIndex(line, ">")

	if open < 0 || end < open {
		return nil
	}

	var targets []string

	for _, match := range packageQualifierPattern.FindAllStringSubmatch(line[open+1:end], -1) {
		targets = append(targets, match[1])
	}

	return targets
}

// declarationClosureImports returns the import paths a test project must reference IN ADDITION to
// its computed direct set so that set is CLOSED under the type-reference edges of the C#
// DECLARATIONS of the types the compilation NAMES — a class of dependency neither the import lists
// nor the B2c alias scan can see.
//
// One rule: binding something in C# requires the assemblies of the types ITS OWN declaration names,
// and those names belong to the DECLARING package's import graph, so they appear in NO test-file
// import and NO alias `using`. The import-derived + alias-scan set (B2c) misses them,
// `DisableTransitiveProjectReferences` (B2b) hides the declaring package's own reference, and the test
// compile fails CS0012. Four edges carry it — two on a TYPE's declaration, two on an ACCESS (the
// second of which reads the declaration of the type the access lands on):
//
//   - INTERFACE BASES. Go interfaces satisfy structurally and compose by embedding; C# interfaces
//     are nominal, so the converter carries both shapes as C# inheritance at the declaration site
//     (getStructuralInterfaceBases): hash's `Hash` embeds io.Writer, io/fs's `File` merely lists
//     Read/Close, and both emit a declaration that NAMES an io base. The failure shows up at the
//     emitted conversion record (`[assembly: GoImplement<Hash, hash_package.Hash64>]` —
//     'io_package.Writer' is defined in an unreferenced assembly: hash/maphash, crypto/hmac, whose
//     closures reach `hash` but never `io`), at the go2cs-gen adapter realizing it, and at every
//     converted source naming the interface in a signature.
//
//   - STRUCT FIELDS AT AN ELEMENT-BEARING COMPOSITE LITERAL. The converter renders such a literal
//     as `new T(Field: …)` — a call to the FIELDWISE CONSTRUCTOR go2cs-gen generates for a
//     `[GoType]` struct, whose parameter list spells out EVERY field's type, supplied or not.
//     Binding that call therefore needs every field type's assembly. testing/quick's `Config` holds
//     a `Rand *rand.Rand`, so image/draw's `quick.CheckEqual(…, &quick.Config{MaxCountScale: 10})`
//     fails `CS0012 … 'rand_package.Rand' … assembly that is not referenced` at the
//     `new quick.Config(MaxCountScale: 10D)` expression, with math/rand in no import list on either
//     side. No interface closure can reach it — `Rand` is a STRUCT. A ZERO-VALUE DECLARATION is the
//     same constructor call by another route: `var l Logger` names no literal at all, yet renders as
//     `heap(new Logger(), out var Ꮡl)`, and log's white-box TestNonNewLogger failed
//     `CS0012 … 'atomic_package.Pointer<>'` resolving against the accessible fieldwise overload.
//
//   - THE RECEIVER OF A MEMBER ACCESS. Resolving `x.M` requires binding x's TYPE, and when x is
//     declared in another package that type is spelled nowhere here. `unique`'s white-box suite calls
//     `cleanupMu.Lock()` on the production package's `var cleanupMu sync.Mutex`; the test project
//     referenced no `sync` and the compile died `CS0012 … 'sync_package.Mutex'` twice, so the package
//     never linked a host and had never been measured. See the seed's own minimality note below.
//
//   - THE INTERFACES A NAMED TYPE'S DECLARATION IMPLEMENTS — what binding a member costs ON TOP
//     of binding the type. A converted CONCRETE type names no interface in its own emitted
//     declaration (`[GoType("[]ж<ΔError>")] partial struct ErrorList;`): its bases arrive as the
//     VALUE-form `[assembly: GoImplement<T, I>]` records its package emits, which go2cs-gen realizes
//     as `partial struct ErrorList : sort_package.Interface` INSIDE THE DECLARING ASSEMBLY. The
//     metadata type therefore declares that base, and binding any member on it resolves the list:
//     go/scanner's `list.Sort()`, `len(list)` and the generated `error` adapter's own
//     `m_value.Equals(…)` all failed `CS0012 … 'sort_package.Interface'`, ×13.
//
//     The records are read PER DECLARING PACKAGE, so the edge reaches a FOREIGN type's base list
//     too: go/types' check_test.go asserts `err.(scanner.ErrorList)` and calls `len(list)` on the
//     result, and `sort` — which go/types' PRODUCTION project references and no test file imports —
//     is surfaced only from go/scanner's own record set. Reading the declaring package's
//     package_info.cs is what makes that one lookup answer both shapes.
//
// Neither scan covers these because the named type itself DOES bind: its package IS referenced.
// What is missing is a package named inside that type's own C# declaration — or, for the third edge,
// the type of a value the compilation only ever reaches THROUGH a declaration in another package.
//
// MINIMALITY — three gates, because over-including is its own defect (every extra reference is
// churn across the banked corpus and a chance at a duplicate-type conflict):
//
//   - Seeds come from the files the test assembly actually COMPILES. A Phase-4D-excluded
//     Example/Benchmark-only file (selectCompileExcludedTestFiles) is analyzed but never emitted, so
//     it names nothing in the compilation: seeding from it handed compress/gzip the context,
//     crypto/tls, mime/multipart, net/http and net/url references reached through
//     `http.Request`'s fields — from an example_test.go that is not compiled at all.
//   - The interface walk starts from the types those files NAME, never from whole packages. C#
//     needs a base interface's assembly only when the derived interface is BOUND, and walking every
//     exported interface of every referenced package would hand io to almost the whole corpus
//     through `fmt.State`'s structural io.Writer base — a reference no project that never names
//     `fmt.State` requires.
//   - The struct-field edge fires where a composite literal constructs the struct, and an EMPTY
//     literal only when the struct is declared in a ROOT package. Measured against the corpus in
//     three steps: eleven banked packages hold `sync.Once`/`sync.Map`/`reflect.Value` VALUES
//     (strconv's package-level `atofOnce`, encoding/binary's `reflect.ValueOf`) and compile clean
//     today with no sync/atomic or internal/abi reference, so mere value use demands nothing;
//     three more (encoding/binary, mime, testing/quick) construct those same FOREIGN types with an
//     EMPTY literal — `once = sync.Once{}`, `return reflect.Value{}, false` — and also compile
//     clean, because the fieldwise constructor is `internal` for any struct with an unexported
//     field and so is not even a resolution candidate outside its assembly and friends; but a ROOT
//     package's struct IS visible that way (recompiled into the test assembly, or reached through
//     the white-box `InternalsVisibleTo` grant), so its empty literal must carry the edge —
//     math/rand/v2's `*p = ChaCha8{}` renders `new ChaCha8(nil)` and still fails
//     `CS0012 … 'chacha8rand_package.State'` while resolving against the internal fieldwise
//     overload. A one-level edge suffices throughout: the fieldwise constructor's parameters are
//     `default` unless supplied, and a NESTED literal is itself a seed.
//
// `testing` is skipped as a walk SOURCE (closureWalkable): it binds to the hand-owned core/testing
// shim per F15b, whose C# declarations are authored by hand and share only NAMES with Go's — Go's
// `testing.T` embeds a `common` holding io.Writer, time.Time, sync.RWMutex and a dozen more, none
// of which the shim's two-field `T` names, so inferring C# edges from the Go declaration there is
// simply invalid. Nothing is lost: the shim's reference is fixed in the project template.
//
// The per-interface match reproduces the CANDIDATE gates the converter runs at each declaration
// site (the same Exported / non-alias / non-generic / method-set / strictly-fewer-methods /
// types.Implements tests as getStructuralInterfaceBases). It is deliberately taken before that
// function's covered-by-embed skip and minimal-covering-set prune, so the result is a superset of
// the emitted base list — the guarantee that matters is that no emitted base's assembly is missing.
// Only the declaring package's own IMPORTS are scanned, so a same-package base contributes nothing
// new and needs no separate visit: an interface implements its base's bases too, so those candidates
// are found directly. Output is a sorted set, so the map-ordered walk stays deterministic.
func declarationClosureImports(roots []*packages.Package, compileExcluded map[string]bool, referenced []string, recordedBases map[string][]string, foreignBases func(importPath string) map[string][]string) []string {
	found := HashSet[string]{}
	seen := NewHashSet(referenced)
	visited := map[*types.Named]bool{}

	var queue []*types.Named

	enqueue := func(named *types.Named) {
		if named == nil || visited[named] || !closureWalkable(named) {
			return
		}

		if _, isInterface := named.Underlying().(*types.Interface); !isInterface {
			return
		}

		visited[named] = true
		queue = append(queue, named)
	}

	// reach records the assembly a named type the compilation must BIND lives in. TYPE seeds never
	// go through it — their packages are already referenced by construction; only a package named by
	// a walked DECLARATION is an addition (plus the member-access edge below, whose RECEIVER types
	// are likewise spelled nowhere in the compilation).
	reach := func(named *types.Named) {
		object := named.Obj()

		if object == nil || object.Pkg() == nil {
			return
		}

		// `testing` is never an ADDITION for the same reason it is never a walk SOURCE
		// (closureWalkable): it binds to the hand-owned core/testing shim, and that reference is
		// fixed in the project template — which is why the caller strips "testing" from the
		// import-derived set rather than passing it through as already-referenced. Every -tests
		// compilation calls a method ON a `*testing.T`, so without this the member-access edge
		// would hand a second, closure-derived `testing` reference to every test project.
		if !closureWalkable(named) {
			return
		}

		path := object.Pkg().Path()

		if seen.Contains(path) {
			return
		}

		seen.Add(path)
		found.Add(path)
	}

	// A ROOT's own types are compiled into the test assembly (or, for the production package under
	// the reference model, bound through the colocated project reference the template already
	// carries), so an edge landing back on one is never a project reference. The EXTERNAL variant
	// makes this load-bearing rather than theoretical: its go/packages PkgPath is the synthetic
	// `<pkg>_test`, which resolves to no importable package at all — a `bytes_test` struct literal
	// whose field type is declared beside it would fail the conversion outright ("package
	// bytes_test is not in std"), by design (F14b: a dependency that cannot resolve is loud).
	rootPaths := HashSet[string]{}

	for _, root := range roots {
		if root != nil {
			seen.Add(root.PkgPath)
			rootPaths.Add(root.PkgPath)
		}
	}

	// The fieldwise-constructor edge. One level, and never recursive on its own account: an
	// interface field still joins the base walk, and a nested literal is its own seed.
	fieldEdge := func(named *types.Named) {
		structType, isStruct := named.Underlying().(*types.Struct)

		if !isStruct || !closureWalkable(named) {
			return
		}

		for i := range structType.NumFields() {
			for _, mentioned := range namedTypesIn(structType.Field(i).Type()) {
				reach(mentioned)
				enqueue(mentioned)
			}
		}
	}

	// The IMPLEMENTED-INTERFACE edge, the CONCRETE counterpart of the interface-base walk. A
	// converted struct or named type carries its interfaces NOT in its own emitted declaration
	// (`[GoType("[]ж<ΔError>")] partial struct ErrorList;` names none) but in the VALUE-form
	// `[assembly: GoImplement<T, I>]` records its package emits, which go2cs-gen realizes as
	// `partial struct ErrorList : global::go.sort_package.Interface` INSIDE THE DECLARING
	// ASSEMBLY. So the type's metadata declares that base, and binding ANY member on it — Go's
	// `len(list)`, `list.Sort()`, and the generated value adapter's own `m_value.Equals(…)` —
	// makes the compiler resolve the base list. go/scanner's white-box suite failed
	// `CS0012 … 'sort_package.Interface'` ×13 that way, `sort` being in no test import and no
	// alias `using`. Interfaces are excluded here because the base walk already covers them.
	//
	// The RECORDS, not go/types satisfaction, are the gate, and that is measured rather than
	// argued: a record exists only where the converter converted a CAST, so Go satisfaction wildly
	// over-approximates the emitted base list. Gating on satisfaction alone drifts 16 of the 96
	// banked projects — `os.File` satisfies `syscall.Conn` and hands syscall to thirteen (os
	// records `File` only as io/fs.File and io.Writer, and both POINTER-form, which generate an
	// adapter CLASS rather than a base); `bytes.Buffer` satisfies most of io and hands io to sort
	// and unicode/utf8 though bytes records nothing at all; `internal/buildcfg`'s Stringer hands
	// it fmt from an empty record set. All 96 compile clean today with none of it. Pointer-form
	// records are therefore excluded here too — only the value form lands a base on the type.
	implementEdge := func(named *types.Named) {
		object := named.Obj()

		if _, isInterface := named.Underlying().(*types.Interface); isInterface || !closureWalkable(named) {
			return
		}

		if object == nil || object.Pkg() == nil {
			return
		}

		// The records are read PER DECLARING PACKAGE. A ROOT type's list is the record set the
		// production half of this same run just emitted (recordedBases); a FOREIGN type's is its own
		// package's, in its own package_info.cs — the widening this lookup was always shaped for
		// ("no measured case has ever demanded one" held until go/types), and it is the same gate
		// pointed at a different file, not a looser one. The per-package keying is what keeps a
		// same-named production type from answering for a foreign one.
		//
		// go/types' check_test.go is the measured case: `if list, _ := err.(scanner.ErrorList);
		// len(list) > 0`. `ErrorList` is declared in go/scanner — REFERENCED, so it binds — but its
		// realized base list comes from go/scanner's own
		// `[assembly: GoImplement<ErrorList, sort_package.Interface>]`, and resolving the `len(list)`
		// overload against it failed `CS0012 … 'sort_package.Interface'`. `sort` is in the PRODUCTION
		// project's references (go/types imports it) and in no test import, so only this edge can
		// surface it.
		targets := recordedBases[object.Name()]

		if !rootPaths.Contains(object.Pkg().Path()) {
			if foreignBases == nil {
				return
			}

			targets = foreignBases(object.Pkg().Path())[object.Name()]
		}

		if len(targets) == 0 {
			return
		}

		for _, candidate := range implementedInterfaceCandidates(named) {
			candidateObject := candidate.Obj()

			if candidateObject == nil || candidateObject.Pkg() == nil {
				continue
			}

			for _, target := range targets {
				if qualifierTargetNamesPackage(target, candidateObject.Pkg().Path()) {
					reach(candidate)
					enqueue(candidate)

					break
				}
			}
		}
	}

	for _, root := range roots {
		seeds := referencedTypeSeeds(root, compileExcluded)

		for _, named := range seeds.named {
			enqueue(named)
		}

		for _, named := range seeds.constructed {
			fieldEdge(named)
		}

		// The MEMBER-ACCESS edge. `cleanupMu` is `var cleanupMu sync.Mutex` in unique's PRODUCTION
		// source; the white-box suite calls `cleanupMu.Lock()`, and binding that member needs sync —
		// a package no test file imports and no alias `using` names, whose reference the reference
		// model deliberately does not inherit from the production assembly (that model adds only what
		// the test files import, precisely so a package's whole import graph is not re-declared).
		// CS0012 ×2, and `unique` never linked a host. TYPE seeds still never go through reach() —
		// the type a test file SPELLS comes from a package it imports — but a RECEIVER's type is
		// spelled nowhere in the compilation, which is exactly the class this function exists for.
		//
		// The receiver is the minimal form of the edge, and BOTH halves of that were measured against
		// the banked roster rather than argued. Widening it to the type of every var/const/func the
		// compilation NAMES is equally true of C#'s binding rules in the abstract and drifts **23 of
		// 73** banked projects (bufio into compress/bzip2, internal/abi + internal/reflectlite into
		// errors, three into hash/crc32 …), all of which compile clean today with none of it: naming
		// a declaration does not force its signature to be materialized, ACCESSING A MEMBER of it
		// forces the receiver's. And the seed is `_test.go`-scoped (referencedTypeSeeds), because
		// under the reference model the production sources are not in this compilation at all;
		// seeding from them too still drifts **13** (`castagnoliOnce.Do` in crc32.go, `cpu.X86` in
		// math's arith, …). Both restrictions together are ZERO-drift across the banked roster:
		// unique's own `sync` reference is the only line that changes.
		for _, named := range seeds.memberBases {
			reach(named)
			enqueue(named)
			implementEdge(named)
		}

		// The implemented-interface edge ALSO fires where a member is bound on a value WITHOUT a
		// selector: `len(list)`, `range list`, `list[i]`. Each lowers to a member on the value's
		// type — golib's generic `len`, the emitted enumeration, the indexer — so resolving it makes
		// the compiler read that type's realized base list exactly as `x.M` does. go/types'
		// check_test.go binds `scanner.ErrorList` only that way (`len(list)` and `range list`; there
		// is no `list.Sort()` anywhere in the suite), and failed `CS0012 … 'sort_package.Interface'`.
		//
		// Deliberately NOT routed through reach()/enqueue() the way memberBases is, and deliberately
		// NOT seeded from every NAMED type. Both restrictions keep the pinned negatives true: a type
		// a test file merely spells or PASSES ALONG (`var r Rows; Order(r)`) binds no member and
		// needs no base assembly — measured, and the boundary this edge is not allowed to cross —
		// while the type's OWN package is referenced by construction, since the value is spelled by
		// a package the suite imports.
		for _, named := range seeds.memberBound {
			implementEdge(named)
		}

		// The EMPTY-literal form of the same edge, scoped to the ROOT packages. `T{}` converts to
		// `new T(nil)` — go2cs-gen's dedicated nil constructor, which names no field — but the
		// FIELDWISE overload remains a resolution candidate whenever it is ACCESSIBLE, and
		// binding a candidate's signature is what demands its parameter assemblies. That
		// constructor is `internal` for any struct with an unexported field, so it is a candidate
		// exactly in the declaring assembly and its FRIENDS: a root package's types either
		// recompile into the test assembly or are reached through the white-box
		// `InternalsVisibleTo` grant, so both make it visible. math/rand/v2's `*p = ChaCha8{}`
		// failed `CS0012 … 'chacha8rand_package.State' … assembly that is not referenced` at the
		// `new ChaCha8(nil)` expression for exactly that reason, with internal/chacha8rand in no
		// import list on either side. A FOREIGN struct's internal constructor is invisible here,
		// which is the measured negative that keeps this edge root-scoped: mime's
		// `once = sync.Once{}` and testing/quick's `return reflect.Value{}, false` compile clean
		// today with no sync/atomic or internal/abi reference, and must stay that way.
		for _, named := range seeds.constructedEmpty {
			object := named.Obj()

			if object == nil || object.Pkg() == nil || !rootPaths.Contains(object.Pkg().Path()) {
				continue
			}

			fieldEdge(named)
		}
	}

	for len(queue) > 0 {
		named := queue[0]
		queue = queue[1:]

		for _, base := range interfaceBaseCandidates(named) {
			reach(base)
			enqueue(base)
		}
	}

	result := found.Keys()
	sort.Strings(result)

	return result
}

// closureWalkable reports whether declarationClosureImports may read named's Go DECLARATION for
// reference edges. `testing` is excluded: it binds to the hand-owned core/testing shim, whose C#
// declarations are authored by hand rather than converted from the Go declaration this walk would
// read (see declarationClosureImports).
func closureWalkable(named *types.Named) bool {
	object := named.Obj()

	return object != nil && object.Pkg() != nil && object.Pkg().Path() != "testing"
}

// namedTypesIn returns every NAMED type a type expression mentions — what the C# rendering of that
// expression spells out, and therefore what must bind. It descends through the composite forms the
// converter renders as generic instantiations or delegates (ж<T>, slice<T>, array<T>, map<K,V>,
// channel<T>, Func/Action<…>) and through a generic type's ARGUMENTS, but deliberately NOT through
// a named type's own underlying: whether that declaration is walked in turn is the caller's
// recursion decision (see declarationClosureImports' minimality note).
func namedTypesIn(typ types.Type) []*types.Named {
	var result []*types.Named

	visited := map[types.Type]bool{}

	var walk func(types.Type)

	walk = func(current types.Type) {
		if current == nil || visited[current] {
			return
		}

		visited[current] = true

		switch typed := types.Unalias(current).(type) {
		case *types.Named:
			result = append(result, typed)

			for i := range typed.TypeArgs().Len() {
				walk(typed.TypeArgs().At(i))
			}
		case *types.Pointer:
			walk(typed.Elem())
		case *types.Slice:
			walk(typed.Elem())
		case *types.Array:
			walk(typed.Elem())
		case *types.Chan:
			walk(typed.Elem())
		case *types.Map:
			walk(typed.Key())
			walk(typed.Elem())
		case *types.Signature:
			for i := range typed.Params().Len() {
				walk(typed.Params().At(i).Type())
			}

			for i := range typed.Results().Len() {
				walk(typed.Results().At(i).Type())
			}
		case *types.Struct: // an ANONYMOUS struct field type renders its own field types inline
			for i := range typed.NumFields() {
				walk(typed.Field(i).Type())
			}
		case *types.Interface: // likewise an anonymous interface: its method signatures
			for i := range typed.NumMethods() {
				walk(typed.Method(i).Type())
			}
		}
	}

	walk(typ)

	return result
}

// typeSeeds carries the seed sets declarationClosureImports takes from one compilation unit:
// every named type its compiled files MENTION (what an interface base edge starts from) and the
// named types a composite literal CONSTRUCTS (what the fieldwise-constructor edge starts from).
// The constructed set is split by whether the literal bears elements, because only the EMPTY form
// depends on the fieldwise constructor's ACCESSIBILITY (see declarationClosureImports).
type typeSeeds struct {
	named            []*types.Named
	constructed      []*types.Named
	constructedEmpty []*types.Named
	memberBases      []*types.Named
	// memberBound carries the types a compiled test source binds a member on through a form that
	// spells no selector — a builtin call, a range, an index/slice. It feeds ONLY the
	// implemented-interface edge (see declarationClosureImports): the demand is on the type's
	// realized base list, never on its own package, which the spelling already references.
	memberBound []*types.Named
}

// referencedTypeSeeds collects those seeds from the files the test assembly actually COMPILES.
// Phase-4D compile-excluded files (selectCompileExcludedTestFiles) are skipped: they are analyzed
// so their declarations still reach the manifest, but no C# is emitted for them, so they name
// nothing the compilation must bind — seeding from one handed compress/gzip five references its
// example_test.go reached through `http.Request` (see declarationClosureImports' minimality note).
// Walking the syntax tree rather than iterating the TypesInfo maps is what makes the file scoping
// possible at all, and it makes the seed ORDER deterministic as a side effect.
func referencedTypeSeeds(pkg *packages.Package, compileExcluded map[string]bool) typeSeeds {
	var seeds typeSeeds

	if pkg == nil || pkg.TypesInfo == nil {
		return seeds
	}

	add := func(typ types.Type) {
		if typ == nil {
			return
		}

		if named, ok := types.Unalias(typ).(*types.Named); ok {
			seeds.named = append(seeds.named, named)
		}
	}

	for i, file := range pkg.Syntax {
		if i < len(pkg.CompiledGoFiles) && compileExcluded[filepath.Clean(pkg.CompiledGoFiles[i])] {
			continue
		}

		// The member-access edge is scoped to `_test.go` sources: under the REFERENCE model the
		// production files are not in this compilation at all (the internal variant loads them
		// alongside its own, so the scoping has to be per-FILE, not per-package), and under the
		// recompile model they are, but that model already references every production import
		// wholesale — so a production receiver can never be an addition either way. See
		// declarationClosureImports.
		isTestFile := i < len(pkg.CompiledGoFiles) && strings.HasSuffix(pkg.CompiledGoFiles[i], "_test.go")

		ast.Inspect(file, func(node ast.Node) bool {
			switch typed := node.(type) {
			case *ast.Ident:
				if object := pkg.TypesInfo.Uses[typed]; object != nil {
					add(object.Type())
				}

				if object := pkg.TypesInfo.Defs[typed]; object != nil {
					add(object.Type())
				}
			case *ast.SelectorExpr:
				// The MEMBER-ACCESS edge. Resolving `x.M` in C# requires BINDING x's type, and when
				// x is declared in another package that type is spelled nowhere in this compilation
				// — not in an import, not in an alias `using`. `unique`'s white-box suite calls
				// `cleanupMu.Lock()` on the production package's `var cleanupMu sync.Mutex`, the
				// test project referenced no `sync`, and the compile died CS0012 ×2 with no host
				// ever linking. A package-QUALIFIED selector (`sync.Mutex`, `lib.F`) is not this
				// shape: its base is a PkgName, which has no type, so it contributes nothing — and
				// the import that spells it already carries the reference.
				if isTestFile {
					seeds.memberBases = append(seeds.memberBases, namedTypesIn(pkg.TypesInfo.Types[typed.X].Type)...)
				}
			case *ast.RangeStmt:
				// `range list` enumerates the value, which binds a member on its type.
				if isTestFile {
					seeds.memberBound = append(seeds.memberBound, namedTypesIn(pkg.TypesInfo.Types[typed.X].Type)...)
				}
			case *ast.IndexExpr:
				// `list[i]` binds the indexer. A generic INSTANTIATION wears the same node shape;
				// its base is a func/type, not a value, so namedTypesIn over the operand's type
				// yields the instantiated named type — harmless, and still record-gated.
				if isTestFile {
					seeds.memberBound = append(seeds.memberBound, namedTypesIn(pkg.TypesInfo.Types[typed.X].Type)...)
				}
			case *ast.SliceExpr:
				// `list[1:2]` binds the slice member for the same reason.
				if isTestFile {
					seeds.memberBound = append(seeds.memberBound, namedTypesIn(pkg.TypesInfo.Types[typed.X].Type)...)
				}
			case *ast.CallExpr:
				// A BUILTIN call (`len`, `cap`, `append`, `copy`, `clear`, `delete`) lowers to a
				// golib member resolved against the ARGUMENT's type — the shape go/types' failing
				// `len(list)` takes. An ordinary call is NOT this: passing a value to a function
				// with an exact parameter type binds nothing on the value's own type, which is the
				// measured negative (`Order(r)`) this must not cross.
				if isTestFile {
					if identifier, isIdent := typed.Fun.(*ast.Ident); isIdent {
						if _, isBuiltin := pkg.TypesInfo.Uses[identifier].(*types.Builtin); isBuiltin {
							for _, argument := range typed.Args {
								seeds.memberBound = append(seeds.memberBound, namedTypesIn(pkg.TypesInfo.Types[argument].Type)...)
							}
						}
					}
				}
			case *ast.CompositeLit:
				// An IMPLICIT element literal ([]T{{…}}) carries no Type expression of its own;
				// go/types still records the composite's type, which is the one being constructed.
				literalType := pkg.TypesInfo.Types[typed].Type

				add(literalType)

				// An ELEMENT-BEARING literal calls the fieldwise constructor outright. The EMPTY
				// literal — Go's zero value — converts to `new Δsync.Once(nil)`, go2cs-gen's
				// dedicated nil constructor, but the fieldwise overload is still a RESOLUTION
				// CANDIDATE wherever it is accessible; that distinction is the caller's
				// (declarationClosureImports' root-scoped empty-literal edge).
				if named, ok := types.Unalias(literalType).(*types.Named); ok {
					if len(typed.Elts) == 0 {
						seeds.constructedEmpty = append(seeds.constructedEmpty, named)
					} else {
						seeds.constructed = append(seeds.constructed, named)
					}
				}
			case *ast.ValueSpec:
				// The ZERO-VALUE DECLARATION form of the same construction. `var l Logger` names no
				// composite literal anywhere, yet the converter renders Go's zero value as a
				// CONSTRUCTOR CALL — `ref var l = ref heap(new Logger(), out var Ꮡl)` for the
				// address-taken shape, `new Logger()` otherwise — so it makes exactly the demand
				// `Logger{}` does and reaches it by a route no CompositeLit walk can see. `log`'s
				// white-box suite declares `var l Logger` in TestNonNewLogger and the compile died
				// `CS0012 … 'atomic_package.Pointer<>'`. Scoped to `_test.go` for the member-access
				// edge's reason: under the reference model the production files are not in this
				// compilation, and under the recompile model that model already references every
				// production import wholesale. The ROOT/accessibility gate is the caller's, shared
				// with the empty-literal form.
				if isTestFile && len(typed.Values) == 0 {
					for _, name := range typed.Names {
						variable, isVar := pkg.TypesInfo.Defs[name].(*types.Var)

						if !isVar {
							continue
						}

						if named, ok := types.Unalias(variable.Type()).(*types.Named); ok {
							seeds.constructedEmpty = append(seeds.constructedEmpty, named)
						}
					}
				}
			}

			if expression, ok := node.(ast.Expr); ok {
				add(pkg.TypesInfo.Types[expression].Type)
			}

			return true
		})
	}

	return seeds
}

// interfaceBaseCandidates returns the exported interfaces from the DECLARING package's imports that
// the converter can name as C# bases of named — the same candidate match getStructuralInterfaceBases
// makes at the declaration site. See interfaceBaseClosureImports.
func interfaceBaseCandidates(named *types.Named) []*types.Named {
	pkg := named.Obj().Pkg()

	if pkg == nil {
		return nil
	}

	iface, ok := named.Underlying().(*types.Interface)

	if !ok || iface.NumMethods() == 0 {
		return nil
	}

	var result []*types.Named

	for _, imported := range pkg.Imports() {
		scope := imported.Scope()

		for _, name := range scope.Names() {
			typeName, ok := scope.Lookup(name).(*types.TypeName)

			if !ok || !typeName.Exported() || typeName.IsAlias() {
				continue
			}

			candidate, ok := typeName.Type().(*types.Named)

			if !ok || candidate.TypeParams().Len() > 0 {
				continue
			}

			candidateInterface, ok := candidate.Underlying().(*types.Interface)

			if !ok || candidateInterface.NumMethods() == 0 || candidateInterface.NumMethods() >= iface.NumMethods() || !candidateInterface.IsMethodSet() {
				continue
			}

			if types.Implements(named, candidateInterface) {
				result = append(result, candidate)
			}
		}
	}

	return result
}

// implementedInterfaceCandidates returns the exported interfaces from the DECLARING package's
// imports that a CONCRETE named type can carry as a C# base — the counterpart of
// interfaceBaseCandidates for a type whose interfaces reach its declaration through the package's
// emitted `[assembly: GoImplement<T, I>]` records rather than through the Go declaration itself.
// Same candidate gates (exported, non-alias, non-generic, real method set), and both receiver
// forms are tested: a record is written for whichever of T or *T satisfies the interface, and a
// pointer-form record realizes a base on the value type all the same.
//
// This supplies only the CANDIDATE UNIVERSE — the interfaces a record for this type could possibly
// name, resolved to go/types objects the walk can carry on with. It is a wild over-approximation of
// the emitted base list on its own (a record exists only where a CAST was converted, so `os.File`
// satisfies syscall.Conn while os records no such base), which is why the caller gates every
// candidate on the declaring package's actual records rather than on satisfaction — see
// declarationClosureImports' implementEdge for that measurement.
func implementedInterfaceCandidates(named *types.Named) []*types.Named {
	pkg := named.Obj().Pkg()

	if pkg == nil {
		return nil
	}

	pointer := types.NewPointer(named)

	var result []*types.Named

	for _, imported := range pkg.Imports() {
		scope := imported.Scope()

		for _, name := range scope.Names() {
			typeName, ok := scope.Lookup(name).(*types.TypeName)

			if !ok || !typeName.Exported() || typeName.IsAlias() {
				continue
			}

			candidate, ok := typeName.Type().(*types.Named)

			if !ok || candidate.TypeParams().Len() > 0 {
				continue
			}

			candidateInterface, ok := candidate.Underlying().(*types.Interface)

			if !ok || candidateInterface.NumMethods() == 0 || !candidateInterface.IsMethodSet() {
				continue
			}

			if types.Implements(named, candidateInterface) || types.Implements(pointer, candidateInterface) {
				result = append(result, candidate)
			}
		}
	}

	return result
}

// The F15 mixed-tree remap that used to live here is GONE (2026-08-01): the converted standard
// library now lives at src/core, which is exactly where every resolver already emits its
// `$(go2csPath)core\<pkg>` reference, so a test project's stdlib dependencies need no mapping at
// all. F15b's "ONE testing package, period" is now enforced structurally instead: `testing` is
// hand-owned like `unsafe` (the converter never queues it — see stdLibConverter.go), so
// core/testing IS the only testing package and there is nothing left to collide with.
//
// isSelfProjectReference reports whether reference points at the package-under-test's own
// production csproj. The comparison must be on the path's BASE NAME: a raw suffix test drops
// any dependency whose project file name merely ENDS with the target's ("runtime.csproj" ends
// with "time.csproj", so converting time silently lost its runtime reference — 5x CS0234).
//
// The reference is normalized first, and with path.Base rather than filepath.Base, so the base name
// is taken the same way on every host: filepath.Base off Windows does not split on a backslash, so a
// `\`-spelled reference (a pre-F5 corpus, a deployed tree, a hand-authored project) came back whole
// and matched nothing.
func isSelfProjectReference(reference, projectName string) bool {
	return strings.EqualFold(path.Base(normalizeEmittedPath(reference)), projectFileBaseName(projectName)+".csproj")
}

// productionCSFiles enumerates the package's converted PRODUCTION sources — the compile items a
// recompile-model test project adds to its own (see writeTestProject), and the files the B2c alias
// scan must read for it (testProjectAliasScanFiles).
//
// Layout L3: a package whose emitted C# varies by GOOS keeps the varying files in per-GOOS
// subfolders and its production csproj compiles exactly one of them via `$(GoTargetOS)/*.cs`
// (docs/phase4/DESIGN-multiplatform-corpus.md). The test project lists its compile items
// EXPLICITLY, so the same selection has to be made here or the recompiled half is simply missing
// those files. `crypto/x509` is the corpus's only L3 package on the recompile model — every other
// L3 suite takes the reference model, where the production ASSEMBLY carries its per-GOOS half — so
// the omission had never been exercised. What it costs is not subtle: x509's whole Windows verifier
// (windows/verify.cs, windows/root_windows.cs) fell out of the test compilation, and with it
// `Verify`, `VerifyOptions`' fields, `loadSystemRoots`, `domainToReverseLabels` and every error
// type's `Error()` method — 187 errors that name the TEST files, not the missing folder.
//
// The per-GOOS `package_init.cs` belongs in the set for the same reason: a `-tests` run rewrites it
// to declare the `initᴛᴛtests()` partial hook the internal variant's package_init_internal_test.cs
// implements, and a declaration in one compilation with its implementation in another is no hook at
// all. Only the TARGET platform's folder is taken; the others are a different build.
func productionCSFiles(outputPath string, goos string) ([]string, error) {
	result, err := productionCSFilesIn(outputPath, "")

	if err != nil {
		return nil, err
	}

	if len(goos) > 0 && isPlatformSourceFolder(outputPath, goos) {
		platformFiles, err := productionCSFilesIn(filepath.Join(outputPath, goos), goos)

		if err != nil {
			return nil, err
		}

		result = append(result, platformFiles...)
	}

	sort.Strings(result)
	return result, nil
}

// productionCSFilesIn returns the converted production sources directly inside one directory, named
// relative to the package root (so a per-GOOS folder's files carry their `<goos>/` prefix, which is
// exactly the compile-item and scan spelling both callers need). Subdirectories are never
// descended: below a per-GOOS folder there is nothing, and below the package root there are only
// NESTED PACKAGES, which are separate assemblies.
func productionCSFilesIn(directory string, relativeTo string) ([]string, error) {
	entries, err := os.ReadDir(directory)
	if err != nil {
		return nil, err
	}
	result := make([]string, 0)
	for _, entry := range entries {
		name := entry.Name()
		lower := strings.ToLower(name)
		if entry.IsDir() || !strings.HasSuffix(lower, ".cs") || strings.HasSuffix(lower, "_test.cs") ||
			lower == strings.ToLower(PackageInfoFileName) || lower == testPackageInfoFileName || lower == testHostFileName || strings.HasSuffix(lower, ".g.cs") {
			continue
		}
		if relativeTo != "" {
			name = relativeTo + "/" + name
		}
		result = append(result, name)
	}
	return result, nil
}

// testFixturePaths enumerates the package's test fixture inputs — every top-level *.go source, the
// full testdata/ tree, the testdata trees of the package's NESTED sub-directories, and the fixtures
// its tests read from ABOVE it — as sorted slash-relative paths. Shared by copyTestFixtures and
// testInputDigest so staleness detection always sees the CURRENT fixture set (a newly added
// testdata file changes the digest; the manifest's recorded list plays no part — F7).
func testFixturePaths(inputPath string) ([]string, error) {
	paths := make([]string, 0)

	goSources, err := filepath.Glob(filepath.Join(inputPath, "*.go"))
	if err != nil {
		return nil, err
	}
	for _, sourceFile := range goSources {
		paths = append(paths, filepath.Base(sourceFile))
	}

	testdata := filepath.Join(inputPath, "testdata")
	if info, err := os.Stat(testdata); err == nil && info.IsDir() {
		err = filepath.WalkDir(testdata, func(path string, entry fs.DirEntry, walkErr error) error {
			if walkErr != nil {
				return walkErr
			}
			if entry.IsDir() {
				return nil
			}
			rel, err := filepath.Rel(inputPath, path)
			if err != nil {
				return err
			}
			paths = append(paths, filepath.ToSlash(rel))
			return nil
		})
		if err != nil {
			return nil, err
		}
	}

	nested, err := nestedFixturePaths(inputPath)
	if err != nil {
		return nil, err
	}
	paths = append(paths, nested...)

	shared, err := parentRelativeFixturePaths(inputPath)
	if err != nil {
		return nil, err
	}
	paths = append(paths, shared...)

	sort.Strings(paths)
	return paths, nil
}

// nestedFixturePaths enumerates the testdata trees held by the package's NESTED sub-directories —
// the sibling packages that live under it on disk — as slash-relative paths ("internal/oldtrace/
// testdata/user_task_region_1_21_good"). They keep that shape all the way through: the csproj
// copies each to the matching relative location in the build output, and TestHost.CopyFixtures
// re-creates the same relative path inside the run sandbox, so a test's own relative read resolves.
//
// `go test` runs a package in its real source directory, where the whole subtree below it is
// present, and a test may read into it: internal/trace's TestOldtrace globs
// "./internal/oldtrace/testdata/*_good" for the twelve traces it drives its twelve subtests from.
// Staging only the package's OWN testdata/ left the run directory holding an EMPTY `internal/`
// (the sibling-shape pass creates immediate subdirectory NAMES), the glob matched nothing, and the
// parent failed on "didn't see expected test case user_task_region_1_21_good" — 13 verdicts, the
// parent plus twelve subtests that were never created.
//
// The rule is the SIMPLE one, deliberately: every `testdata` directory below the package, staged
// whole, with no reference analysis. It is the same rule the package's own testdata already gets,
// just applied to the rest of the tree the package occupies on disk, so there is one thing to know
// rather than two — and no test can read a fixture the staging failed to predict. The blast radius
// is bounded by Go's own convention of keeping fixtures in `testdata`: measured over all of
// $GOROOT/src (2026-08-28, Go 1.23.12), sixteen packages gain anything at all, 401 files and 3.6 MB
// in total, the largest single package being internal/trace itself at 1.13 MB.
//
// Only `testdata` is staged, never a nested package's SOURCES: a sibling package is compiled and
// referenced as its own assembly, and its .go files are staged by its own conversion run. A
// directory whose name begins with "." or "_" is not descended into, matching go/build's own
// ignored-directory convention — neither can hold a package, and skipping them keeps a VCS
// directory out of the fixture set when -tests converts a package inside a user's module.
func nestedFixturePaths(inputPath string) ([]string, error) {
	paths := make([]string, 0)

	err := filepath.WalkDir(inputPath, func(path string, entry fs.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}
		if !entry.IsDir() {
			return nil
		}

		relative, err := filepath.Rel(inputPath, path)
		if err != nil {
			return err
		}
		if relative == "." {
			return nil
		}
		relative = filepath.ToSlash(relative)

		name := entry.Name()
		if strings.HasPrefix(name, ".") || strings.HasPrefix(name, "_") {
			return fs.SkipDir
		}
		if name != "testdata" {
			return nil
		}

		// The package's OWN testdata/ is a single path segment and is staged by testFixturePaths
		// itself; descending here would list every one of its files twice.
		if !strings.Contains(relative, "/") {
			return fs.SkipDir
		}

		if err := filepath.WalkDir(path, func(fixture string, fixtureEntry fs.DirEntry, fixtureErr error) error {
			if fixtureErr != nil {
				return fixtureErr
			}
			if fixtureEntry.IsDir() {
				return nil
			}
			fixtureRelative, err := filepath.Rel(inputPath, fixture)
			if err != nil {
				return err
			}
			paths = append(paths, filepath.ToSlash(fixtureRelative))
			return nil
		}); err != nil {
			return err
		}

		return fs.SkipDir
	})

	if err != nil {
		return nil, err
	}

	return paths, nil
}

// linkStagedFixtureDirs selects the fixture directories that must be staged as a LINK into the real
// GOROOT directory rather than as file copies — the RUNNABLE-PROGRAM fixture trees, whose files a
// test hands to the real Go toolchain to compile.
//
// THE REFUSAL THIS EXISTS FOR. `cmd/go/internal/load.disallowInternal` decides an `internal/…`
// import for a standard-library package with ONE directory comparison: the directory holding the
// file being compiled must sit under `$GOROOT/src` (`pkg.go:1425`, and after the plain compare it
// retries through `expandPath`, i.e. filepath.EvalSymlinks). Under `go test` that directory IS
// `$GOROOT/src/<pkg>/testdata/…`; under the converted host it is the sandbox, so
// `go run ./testdata/testprog/cpu-profile.go` is refused its `internal/profile` import —
//
//	testdata\testprog\cpu-profile.go:15:2: use of internal package internal/profile not allowed
//
// — while the same file compiles under `go test`. That is an environment-fidelity gap, the same
// category the PackageAncestry view closed for working directories, and there is no escape flag:
// disallowInternal's only allowances are gccgo, a `bootstrap/` importer, testing/internal from
// testmain, an empty importer path, and the two directory tests.
//
// THE REMEDY, AND WHY THIS SHAPE. Staging the directory as a LINK to the real GOROOT directory
// makes the toolchain see a GOROOT path and permit the import. Measured on Go 1.23.12 / Windows,
// `go build ./testdata/testprog/cpu-profile.go` from a sandbox-shaped tree: a plain COPY is
// REFUSED; a directory SYMLINK is ALLOWED; a JUNCTION is ALLOWED. The symlink is the attributable
// form — `filepath.EvalSymlinks` resolves it to the GOROOT path, which is exactly what
// `expandPath` does inside disallowInternal — so it is the host's primary form, with the junction
// as the unprivileged fallback; the host probes the toolchain before trusting either
// (PackageAncestry.StageFixtureLinks).
//
// THE PREDICATE, measured over $GOROOT/src rather than invented: a directory at or below a
// `testdata` path segment that holds AT LEAST ONE `.go` file and in which EVERY `.go` file declares
// `package main`. That is precisely the shape of a fixture tree meant to be COMPILED, and it
// excludes the parse/type-check fixture trees that merely look similar. Census (Go 1.23.12,
// 2026-08-29): 273 directories under a `testdata` tree hold `.go` files; 125 are selected. The
// members that matter here —
//
//	internal/trace/testdata/testprog       13 files / 13 .go / 13 main   selected
//	internal/trace/testdata/generators     17 / 17 / 17                  selected
//	internal/coverage/cfile/testdata        1 /  1 /  1                  selected
//	runtime/testdata/testprog              33 / 31 / 31                  selected (future member)
//	go/doc/testdata                        81 / 23 /  0                  NOT — parse fixtures
//	internal/types/testdata/check          67 / 67 /  3                  NOT — type-check fixtures
//
// A file that will not parse counts as NOT `package main`, so an unparseable fixture keeps its
// directory on the copy path — the conservative answer, since a link is the sharper instrument.
//
// OUTERMOST WINS. A selected directory is staged whole, so its subdirectories arrive through the
// link and are never selected separately (`internal/coverage/cfile/testdata` selects, and its
// `issue56006`/`issue59563` children come with it). Descending would try to plant a link inside a
// link, which is a write into the real Go installation.
//
// A DIRECTORY THAT IS LINK-STAGED IS NEVER WRITTEN TO. Its files are dropped from the csproj's
// `<None>` items and from the host's fixture list — the LINK is created by the host at sandbox
// construction, not by MSBuild — and PackageAncestry refuses a write into one loudly rather than
// absorbing it (its EnsureWritable would otherwise replace the link with an EMPTY real directory
// and silently lose the whole tree). The F7 input digest is deliberately unaffected: it hashes
// testFixturePaths, which walks the REAL directory at conversion time, so a fixture edit still
// invalidates a prior comparison exactly as it did when the tree was copied.
func linkStagedFixtureDirs(inputPath string) ([]string, error) {
	paths := make([]string, 0)

	err := filepath.WalkDir(inputPath, func(path string, entry fs.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}
		if !entry.IsDir() {
			return nil
		}

		relative, err := filepath.Rel(inputPath, path)
		if err != nil {
			return err
		}
		if relative == "." {
			return nil
		}
		relative = filepath.ToSlash(relative)

		// go/build's own ignored-directory convention, matching nestedFixturePaths.
		name := entry.Name()
		if strings.HasPrefix(name, ".") || strings.HasPrefix(name, "_") {
			return fs.SkipDir
		}

		// Only inside a `testdata` tree — the universal convention for fixtures, and the boundary
		// that keeps this from ever selecting a real package directory.
		if !hasPathSegment(relative, "testdata") {
			return nil
		}

		selected, err := holdsOnlyMainPrograms(path)
		if err != nil {
			return err
		}
		if !selected {
			return nil
		}

		paths = append(paths, relative)

		// Staged WHOLE: everything below arrives through the link.
		return fs.SkipDir
	})

	if err != nil {
		return nil, err
	}

	sort.Strings(paths)
	return paths, nil
}

// holdsOnlyMainPrograms reports whether a directory holds at least one `.go` file and every `.go`
// file it holds DIRECTLY declares `package main`. Subdirectories play no part — this is a statement
// about one compilable unit. The package clause is read with go/parser rather than matched by
// regexp so a clause inside a comment or a string cannot answer for the file; a file that fails to
// parse answers "not main", which keeps its directory on the copy path.
func holdsOnlyMainPrograms(directory string) (bool, error) {
	entries, err := os.ReadDir(directory)
	if err != nil {
		return false, err
	}

	goFiles := 0

	for _, entry := range entries {
		if entry.IsDir() || !strings.HasSuffix(entry.Name(), ".go") {
			continue
		}

		goFiles++

		file, err := parser.ParseFile(token.NewFileSet(), filepath.Join(directory, entry.Name()), nil, parser.PackageClauseOnly)
		if err != nil || file.Name == nil || file.Name.Name != "main" {
			return false, nil
		}
	}

	return goFiles > 0, nil
}

// isUnderLinkStagedDir reports whether a slash-relative fixture path lies at or below one of the
// link-staged directories — i.e. whether the link already carries it and it must NOT also be
// emitted as a csproj `<None>` item or copied into the sandbox.
func isUnderLinkStagedDir(fixture string, linkStaged []string) bool {
	for _, directory := range linkStaged {
		if fixture == directory || strings.HasPrefix(fixture, directory+"/") {
			return true
		}
	}

	return false
}

// testFixtureDirectories enumerates the package directory's IMMEDIATE subdirectory names, sorted.
// The host creates each one (empty) in its isolated run directory, so a test that asks what its own
// working directory contains sees the same SHAPE `go test` shows it.
//
// `go test` runs a package in its real source directory, where the sibling packages nested under it
// are present as subdirectories; the isolated run directory holds only the staged *.go sources and
// testdata, so os's TestReadDir found `read_test.go` but not the `exec` SUBDIRECTORY and failed on
// "exec directory not found". That is environment fidelity, not conversion — os.ReadDir itself was
// probed against `go run` over the same 201-entry directory and is byte-identical, IsDir() included.
//
// NAMES ONLY, one level deep, and empty: the name is what such a test observes, and mirroring a
// sibling package's SOURCES would stage a second copy of a tree no test reads — each sibling is
// compiled and referenced as its own assembly, and its own conversion run stages its files.
//
// A test that reads INTO a sibling directory needs that directory's CONTENT, and one does:
// internal/trace's TestOldtrace globs "./internal/oldtrace/testdata/*_good" (measured 2026-08-28 —
// this comment used to say none did, and its "such a read would be a fixture reference, which the
// fixture pass already covers" was wrong twice over, because the fixture pass then staged only the
// package's own testdata/ and an empty `internal/` created here matched the glob to nothing). It is
// covered now, and by the fixture pass as that sentence expected: nestedFixturePaths stages every
// testdata tree below the package, and the fixture copy creates the intermediate directories on its
// way. This pass keeps its narrow job — the SHAPE of the package directory, one level, empty.
func testFixtureDirectories(inputPath string) ([]string, error) {
	entries, err := os.ReadDir(inputPath)
	if err != nil {
		return nil, err
	}

	directories := make([]string, 0)

	for _, entry := range entries {
		if entry.IsDir() {
			directories = append(directories, entry.Name())
		}
	}

	sort.Strings(directories)
	return directories, nil
}

// sharedFixtureRef matches a Go double-quoted literal naming a fixture ABOVE the package —
// "../testdata/e.txt", "../../testdata/Isaac.Newton-Opticks.txt". Go's stdlib spells these as
// plain literals everywhere they occur, so a source scan is exact; nothing builds them with
// filepath.Join.
var sharedFixtureRef = regexp.MustCompile(`"((?:\.\./)+[^"]*)"`)

// parentRelativeFixturePaths finds the fixtures a package's tests read from ABOVE their own
// directory and returns them as "../"-prefixed slash paths — the same shape testFixturePaths
// returns for the package's own testdata, so copyTestFixtures stages them and testInputDigest
// covers them for staleness with no further work (filepath.Join cleans the "../" on both the read
// and the write, landing each file at the mirrored ancestor location under the output root, which
// is what makes the test's own relative open() resolve).
//
// Go shares large fixtures between sibling packages rather than duplicating them: compress/flate,
// compress/zlib and compress/lzw all read ../testdata/{e,pi,gettysburg}.txt, and flate also reads
// ../../testdata/Isaac.Newton-Opticks.txt. Staging only the package's OWN testdata/ left those
// opens failing, which is what kept compress/flate at 61 of 64 tests (and gates image/{draw,gif,
// jpeg,png}, index/suffixarray, internal/zstd and net the same way).
//
// Two constraints keep this bounded. The path must have a "testdata" segment — the universal
// convention for every occurrence in the stdlib — and the resolved source must exist. Together
// they stop an unrelated "../" literal (a URL, a comment fragment, a relative import in a string)
// from being treated as a fixture and reaching outside the tree.
func parentRelativeFixturePaths(inputPath string) ([]string, error) {
	testSources, err := filepath.Glob(filepath.Join(inputPath, "*_test.go"))
	if err != nil {
		return nil, err
	}

	seen := HashSet[string]{}
	paths := make([]string, 0)

	for _, testSource := range testSources {
		contents, err := os.ReadFile(testSource)
		if err != nil {
			return nil, err
		}

		for _, match := range sharedFixtureRef.FindAllStringSubmatch(string(contents), -1) {
			reference := filepath.ToSlash(filepath.Clean(match[1]))

			if !hasPathSegment(reference, "testdata") || seen.Contains(reference) {
				continue
			}

			resolved := filepath.Join(inputPath, filepath.FromSlash(reference))
			info, err := os.Stat(resolved)

			if err != nil {
				// A referenced-but-absent fixture is not this pass's business: the test that reads
				// it fails identically under `go test`, so the differential comparison still agrees.
				continue
			}

			seen.Add(reference)

			if !info.IsDir() {
				paths = append(paths, reference)
				continue
			}

			err = filepath.WalkDir(resolved, func(path string, entry fs.DirEntry, walkErr error) error {
				if walkErr != nil {
					return walkErr
				}
				if entry.IsDir() {
					return nil
				}
				rel, err := filepath.Rel(inputPath, path)
				if err != nil {
					return err
				}
				paths = append(paths, filepath.ToSlash(rel))
				return nil
			})

			if err != nil {
				return nil, err
			}
		}
	}

	return paths, nil
}

// SharedFixtureStagingRoot is the output-directory folder that holds fixtures reaching ABOVE the
// package. They cannot keep their `../` shape under the build output, so each is staged at
// "<root>/up<N>/<tail>" and the test host restores the true relative path inside its run sandbox.
// MUST match TestHost.SharedFixtureStagingRoot.
const SharedFixtureStagingRoot = "go2cs_shared_fixtures"

// sharedFixtureStagingParts splits a fixture path that reaches above the package into the number of
// levels it ascends and the remainder, so it can be staged at a flat, collision-free location:
// "../testdata/e.txt" -> (1, "testdata/e.txt"). The level count is part of the key because two
// different ancestors can hold a same-named file ("../testdata/e.txt" vs "../../testdata/e.txt").
// Reports false for a fixture at or below the package, which needs no staging.
func sharedFixtureStagingParts(fixture string) (int, string, bool) {
	up := 0
	tail := fixture

	for strings.HasPrefix(tail, "../") {
		up++
		tail = tail[len("../"):]
	}

	return up, tail, up > 0 && tail != ""
}

// hasPathSegment reports whether a slash path contains the given segment whole — "testdata/e.txt"
// and "../testdata" match "testdata", "mytestdata.txt" does not.
func hasPathSegment(path, segment string) bool {
	for _, part := range strings.Split(path, "/") {
		if part == segment {
			return true
		}
	}

	return false
}

// copyTestFixtures stages the package's fixtures into the output directory and reports what it
// staged, split two ways: the COPIED fixtures (the csproj's `<None>` items and the host's fixture
// list) and the LINK-STAGED directories (see linkStagedFixtureDirs), whose files are deliberately
// neither copied nor listed — the host creates one link per directory at sandbox construction, and
// a copy beside it would be a second, divergent truth.
func copyTestFixtures(inputPath, outputPath string) (copied []string, linkStaged []string, err error) {
	fixtures, err := testFixturePaths(inputPath)
	if err != nil {
		return nil, nil, err
	}

	linkStaged, err = linkStagedFixtureDirs(inputPath)
	if err != nil {
		return nil, nil, err
	}

	copied = make([]string, 0, len(fixtures))
	for _, fixture := range fixtures {
		if !isUnderLinkStagedDir(fixture, linkStaged) {
			copied = append(copied, fixture)
		}
	}

	if samePath(inputPath, outputPath) {
		return copied, linkStaged, nil
	}

	for _, fixture := range copied {
		data, err := os.ReadFile(filepath.Join(inputPath, filepath.FromSlash(fixture)))
		if err != nil {
			return nil, nil, err
		}

		target := filepath.Join(outputPath, filepath.FromSlash(fixture))
		if err := os.MkdirAll(filepath.Dir(target), 0755); err != nil {
			return nil, nil, err
		}
		if needToWriteFile(target, data) {
			if err := os.WriteFile(target, data, 0644); err != nil {
				return nil, nil, err
			}
		}
	}

	return copied, linkStaged, nil
}

func classifyTestSources(inputPath string, included HashSet[string], compileExcluded, handOwnHostExcluded map[string]bool, external *packages.Package) ([]testSource, error) {
	matches, err := filepath.Glob(filepath.Join(inputPath, "*_test.go"))
	if err != nil {
		return nil, err
	}
	result := make([]testSource, 0, len(matches))
	for _, path := range matches {
		kind := "internal-test"
		if external != nil {
			for _, file := range external.CompiledGoFiles {
				if samePath(file, path) {
					kind = "external-test"
					break
				}
			}
		}
		// compile-excluded is checked BEFORE included: a Phase-4D Example/Benchmark-only file was
		// platform-SELECTED (so it is not platform-excluded) yet is deliberately not compiled, and
		// its distinct status keeps the manifest truthful about why.
		status, reason := "included", ""
		switch {
		case handOwnHostExcluded[filepath.Clean(path)]:
			// Checked ahead of the Phase-4D status: a host row's internal file may satisfy both
			// predicates, and "the host replaces this representation" is the accurate reason where
			// "deferred to Phase 4D" would promise a later run that is never coming.
			status, reason = handOwnHostExcludedSourceStatus, handOwnHostExcludedSourceReason
		case compileExcluded[filepath.Clean(path)]:
			status, reason = compileExcludedSourceStatus, compileExcludedSourceReason
		case !included.Contains(filepath.Clean(path)):
			status, reason = "platform-excluded", "not selected by go/packages for the requested GOOS/GOARCH and build constraints"
		}
		result = append(result, testSource{Path: filepath.ToSlash(filepath.Base(path)), Kind: kind, Status: status, Reason: reason})
	}
	sort.Slice(result, func(i, j int) bool { return result[i].Path < result[j].Path })
	return result, nil
}

// conversionOptionsDigest canonicalizes the OUTPUT-AFFECTING conversion options for the input
// digest (F7): any option that changes emitted C# invalidates the manifest. Machine-specific
// paths (goRoot/goPath/go2csPath) are deliberately excluded so digests stay machine-portable.
func conversionOptionsDigest(options Options) string {
	return fmt.Sprintf("uco=%t;var=%t;indent=%d;comments=%t;cgo=%t",
		options.useChannelOperators, options.preferVarDecl, options.indentSpaces,
		options.includeComments, options.parseCgoTargets)
}

// runtimeSourcesDigest hashes the hand-owned runtime the converted tests build against (golib +
// the go.testing shim) as staged under the converter's go2csPath output root (F7): a runtime
// behavior change invalidates prior comparisons. KNOWN ITEM (review #5, accepted): best-effort
// by design — in a dev tree the runtime is resolved by MSBuild from $(SolutionDir), not the
// converter's output root, so the sources may not be present and a runtime edit then does NOT
// invalidate the manifest ("runtime-unavailable" keeps the digest deterministic either way);
// deployed (deploy-core) and -go2cspath-staged layouts get full invalidation.
func runtimeSourcesDigest(options Options) string {
	var files []string

	for _, dir := range []string{
		filepath.Join(options.go2csPath, "core", "golib"),
		filepath.Join(options.go2csPath, "core", "testing"),
	} {
		if matches, err := filepath.Glob(filepath.Join(dir, "*.cs")); err == nil {
			files = append(files, matches...)
		}
	}

	if len(files) == 0 {
		return "runtime-unavailable"
	}

	sort.Strings(files)
	hash := sha256.New()

	for _, fileName := range files {
		data, err := os.ReadFile(fileName)
		if err != nil {
			return "runtime-unavailable"
		}
		fmt.Fprintf(hash, "%s\x00%d\x00", filepath.Base(fileName), len(data))
		hash.Write(data)
	}

	return "runtime-" + hex.EncodeToString(hash.Sum(nil)[:8])
}

// testInputDigest fingerprints everything that determines a test conversion's outputs: the
// package's Go sources and testdata (globbed FRESH — never from a recorded list, F7), hand-owned
// *_impl.cs companions in the output, the output-affecting conversion options, the staged
// runtime sources, the target platform, the Go toolchain, and the converter revision.
func testInputDigest(inputPath, outputPath string, options Options, revision string) (string, error) {
	hash := sha256.New()

	fixtures, err := testFixturePaths(inputPath)
	if err != nil {
		return "", err
	}

	inputs := make([]string, 0, len(fixtures)+8)
	for _, fixture := range fixtures {
		inputs = append(inputs, "source:"+fixture)
	}

	companions, err := filepath.Glob(filepath.Join(outputPath, "*_impl.cs"))
	if err != nil {
		return "", err
	}
	for _, path := range companions {
		inputs = append(inputs, "output:"+filepath.Base(path))
	}

	// TEST-file companions (`*_impl_test.cs`) are conversion inputs exactly as the production
	// `*_impl.cs` companions above are: editing one must invalidate a prior comparison.
	testCompanions, err := filepath.Glob(filepath.Join(outputPath, "*_impl_test.cs"))
	if err != nil {
		return "", err
	}
	for _, path := range testCompanions {
		inputs = append(inputs, "output:"+filepath.Base(path))
	}

	sort.Strings(inputs)
	for _, taggedPath := range inputs {
		tag, rel, _ := strings.Cut(taggedPath, ":")
		root := inputPath
		if tag == "output" {
			root = outputPath
		}
		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))
		if err != nil {
			return "", err
		}
		fmt.Fprintf(hash, "%s\x00%d\x00", taggedPath, len(data))
		hash.Write(data)
	}

	// The package's immediate subdirectory NAMES are part of the run environment the host builds
	// (testFixtureDirectories), so a subdirectory appearing or disappearing invalidates a prior
	// comparison exactly as a new testdata file does. Names only — their contents are never staged.
	directories, err := testFixtureDirectories(inputPath)
	if err != nil {
		return "", err
	}
	for _, directory := range directories {
		fmt.Fprintf(hash, "dir:%s\x00", directory)
	}

	// LINK-STAGED fixture trees are deliberately NOT a separate digest input, and the reason is worth
	// stating so the omission is not read as one. The fixture walk above enumerates the package's own
	// `testdata` tree WHOLE and hashes every file's bytes — it reads the real directory at conversion
	// time, so a link changes nothing about what it sees. That covers both halves at once: an edit to
	// a fixture inside a link-staged tree invalidates a prior comparison exactly as it did when the
	// tree was copied (F7), and the SELECTION itself is a pure function of the same bytes (which
	// files are `.go`, and which declare `package main`), so a tree crossing the predicate cannot do
	// so without moving this digest already. Hashing the selected paths as well would be a second
	// spelling of one fact, and a digest input that cannot independently change is one no guard can
	// ever prove.

	fmt.Fprintf(hash, "\x00%s\x00%s\x00%s\x00%s\x00%s",
		options.targetPlatform, conversionOptionsDigest(options), runtimeSourcesDigest(options),
		runtime.Version(), revision)
	return hex.EncodeToString(hash.Sum(nil)), nil
}

func writeNoTestsManifest(production *packages.Package, inputPath, outputPath string, target []string, options Options) error {
	projectName, _ := getProjectName(inputPath, options)
	manifest := testManifest{
		SchemaVersion: 1, CapabilitiesVersion: 1, PackageImportPath: production.PkgPath,
		ProjectName: projectName, TestProject: projectFileBaseName(projectName) + ".tests.csproj", GoVersion: runtime.Version(),
		TargetGOOS: target[0], TargetGOARCH: target[1], SourceRevision: gitRevision(inputPath),
		ConverterRevision: converterRevision(), ProductionFiles: []string{}, TestSources: []testSource{},
		Fixtures: []string{}, FixtureDirectories: []string{}, Tests: []testDeclaration{}, Dependencies: []string{}, Capabilities: supportedTestCapabilities(),
		RequiredCapabilities: []string{}, UnsupportedCapabilities: []string{},
	}
	digest, err := testInputDigest(inputPath, outputPath, options, manifest.ConverterRevision)
	if err != nil {
		return fmt.Errorf("compute no-tests manifest input digest: %w", err)
	}
	manifest.InputDigest = digest

	return writeJSONFile(filepath.Join(outputPath, testManifestFileName), manifest)
}

// writeComparisonRecord is the ONE write path for go2cs_test_comparison.json, and it exists so the
// gated stamp cannot be forgotten at a call site. A `-test-filter` run rewrites the SAME record a
// full run writes, and nothing in the file says which it is -- worse, the fleet's restore step
// cannot clear it, because the record is gitignored and `git clean -fd` skips ignored paths. So a
// diagnostic record survives into the next run looking exactly like that run's own output; a gated
// census once read its own filter's survivor set back as a package's verdicts and called the row
// bankable, with arithmetic the only tell.
//
// Routing every writer through here rather than stamping at each of the three sites is what makes
// the guard able to fail: a unit test calls this function and reads the file back, so deleting the
// stamp reddens it. Three scattered assignments would each have been invisible to any test that
// does not run the whole pipeline -- a guard green for the wrong reason.
func writeComparisonRecord(outputPath string, result any, testFilter string) error {
	if testFilter != "" {
		switch record := result.(type) {
		case map[string]any:
			record["testFilter"] = testFilter
		case *testComparison:
			record.TestFilter = testFilter
		default:
			// Refusing is the point. A value (rather than pointer) testComparison, or any shape
			// added later, would take the stamp silently nowhere and publish a gated record that
			// reads as a full one -- the exact failure this function exists to prevent, arriving
			// through the function meant to prevent it. Fail loudly instead of writing a lie.
			return fmt.Errorf("cannot stamp the -test-filter expression onto a %T comparison record: "+
				"pass *testComparison or map[string]any, or a gated record would publish as a full run", result)
		}
	}
	return writeJSONFile(filepath.Join(outputPath, "go2cs_test_comparison.json"), result)
}

func writeJSONFile(fileName string, value any) error {
	data, err := json.MarshalIndent(value, "", "  ")
	if err != nil {
		return err
	}
	data = append(data, '\n')
	if needToWriteFile(fileName, data) {
		return os.WriteFile(fileName, data, 0644)
	}
	return nil
}

// converterRevision identifies the converter BINARY that produced a manifest. The executable
// hash comes first (F7): hashing the on-disk source directory would report a fresh revision for
// a STALE go2cs.exe — precisely the stale-binary false-green failure mode this project has been
// burned by. VCS build info (when unmodified) and the source-directory digest are fallbacks.
func converterRevision() string {
	if executable, err := os.Executable(); err == nil {
		if data, readErr := os.ReadFile(executable); readErr == nil {
			digest := sha256.Sum256(data)
			return "exe-" + hex.EncodeToString(digest[:8])
		}
	}

	revision := "development"
	modified := false
	if info, ok := debug.ReadBuildInfo(); ok {
		for _, setting := range info.Settings {
			switch setting.Key {
			case "vcs.revision":
				if setting.Value != "" {
					revision = setting.Value
				}
			case "vcs.modified":
				modified = setting.Value == "true"
			}
		}
	}
	if !modified && revision != "development" {
		return revision
	}

	if _, sourceFile, _, ok := runtime.Caller(0); ok {
		sourceFiles, globErr := filepath.Glob(filepath.Join(filepath.Dir(sourceFile), "*.go"))
		if globErr == nil && len(sourceFiles) > 0 {
			sort.Strings(sourceFiles)
			hash := sha256.New()
			complete := true
			for _, fileName := range sourceFiles {
				data, readErr := os.ReadFile(fileName)
				if readErr != nil {
					complete = false
					break
				}
				fmt.Fprintf(hash, "%s\x00%d\x00", filepath.Base(fileName), len(data))
				hash.Write(data)
			}
			if complete {
				return "source-" + hex.EncodeToString(hash.Sum(nil)[:8])
			}
		}
	}

	return revision + "+modified"
}

func gitRevision(path string) string {
	cmd := exec.Command("git", "-C", path, "rev-parse", "HEAD")
	output, err := cmd.Output()
	if err != nil {
		return ""
	}
	return strings.TrimSpace(string(output))
}

func executeTestAction(inputPath, outputPath string, options Options) error {
	projectName, _ := getProjectName(inputPath, options)
	testProject := filepath.Join(outputPath, projectFileBaseName(projectName)+".tests.csproj")
	if err := validateTestManifest(inputPath, outputPath, options); err != nil {
		return err
	}
	manifest, err := readTestManifest(outputPath)
	if err != nil {
		return err
	}

	// Package-level infrastructure blocking applies only when NO converted test can run (a
	// capability-blocked TestMain gates everything; or every declared test blocked itself).
	// Individually blocked tests among runnable siblings are excluded-disclosed instead (F4).
	if blocked := manifestCapabilityBlock(manifest); len(blocked) > 0 {
		result := map[string]any{
			"package": filepath.Base(inputPath), "status": "infrastructure-blocked", "matched": false,
			"errors": []string{"unsupported testing capabilities: " + strings.Join(blocked, ", ")},
		}
		if err := writeComparisonRecord(outputPath, result, options.testFilter); err != nil {
			return err
		}
		return fmt.Errorf("converted tests are infrastructure-blocked: %s", strings.Join(blocked, ", "))
	}

	if !manifestHasEligibleTests(manifest) {
		if options.testAction == "all" || options.testAction == "compare" {
			result := map[string]any{"package": filepath.Base(inputPath), "status": "not-applicable", "matched": true, "errors": []string{}}
			if err := writeComparisonRecord(outputPath, result, options.testFilter); err != nil {
				return err
			}
		}
		fmt.Println("No eligible Go tests for the requested target.")
		return nil
	}

	if _, err := os.Stat(testProject); err != nil {
		return fmt.Errorf("test project is missing or stale; run -tests -test-action convert first: %w", err)
	}

	switch options.testAction {
	case "build":
		return publishTestHost(outputPath, testProject, options)
	case "run":
		if err := publishTestHost(outputPath, testProject, options); err != nil {
			return err
		}
		output, err := runCommandWithTimeoutEnv(testChildTimeout(options), outputPath, options, testHostRunEnv(options), publishedTestHostPath(outputPath, testProject),
			"--json", "-timeout", options.testTimeout.String())
		fmt.Print(output)
		return err
	case "compare", "all":
		return compareGoAndConvertedTests(inputPath, outputPath, testProject, options)
	default:
		return nil
	}
}

// publishTestHost builds the converted test host as the RELOCATABLE SINGLE-FILE executable the
// run/compare actions execute — the host-limit retirement (the -tests csproj template's
// publish-gated SelfContained+PublishSingleFile block; the deployment property Go's statically
// linked test binary has, which os/exec-style tests copy to a temp directory and re-exec).
// `-c Debug` is LOAD-BEARING twice over: `dotnet publish` defaults to Release since SDK 8, and the
// template resolves `$(go2csPath)` per configuration — a Release publish silently re-points every
// stdlib reference at the deployed `~/go2cs` root instead of this tree. Publish is incremental
// (MSBuild's up-to-date checks carry; only the bundling step re-runs warm), so build/run/compare
// can each call this without re-paying the build.
//
// options.testConfig == "Release" takes the OTHER honest seam instead: the template's
// Debug-conditional default is inside a `Condition="'$(go2csPath)'==''"` guard
// (test-csproj-template.xml), so passing `-p:go2csPath` explicitly on the command line skips that
// guard entirely regardless of configuration — the same escape CLAUDE.md documents for a
// hand-invoked Release build elsewhere. Forward slashes and a trailing slash, matching that
// documented form exactly; a trailing backslash escapes the closing quote and mangles the path into
// phantom golib-not-found errors.
func publishTestHost(outputPath, testProject string, options Options) error {
	if options.testConfig == "Release" {
		go2csPathArg := strings.TrimRight(filepath.ToSlash(options.go2csPath), "/") + "/"
		_, err := runCommandWithTimeout(options.testTimeout, outputPath, options, "dotnet", "publish", testProject,
			"-c", "Release", "-p:go2csPath="+go2csPathArg, "-o", filepath.Join(outputPath, "bin", "tests", "publish"))
		return err
	}
	_, err := runCommandWithTimeout(options.testTimeout, outputPath, options, "dotnet", "publish", testProject,
		"-c", "Debug", "-o", filepath.Join(outputPath, "bin", "tests", "publish"))
	return err
}

// testHostRunEnv is the extra environment a Release configuration asks the RUN half of the pair
// for. Release publish alone does not force tier-0 to disappear, since a program can still start at
// tier-0 and simply never run long enough for the runtime to promote a method; DOTNET_TieredCompilation=0
// is the documented .NET knob that disables the tiering system outright, and it is Release's DEFAULT
// here — ruled 2026-09-02 from measurement, not assumption: the same published binary run six times
// gave deterministic sub-millisecond verdicts with tiering off and flipped verdicts run-to-run with
// it on (net/http's h2 write-deadline row), because a timing-bounded row's pass/fail can depend on
// which run happens to hit the tier-0→tier-1 promotion boundary. -test-tiered opts back IN to the
// CLR's default tiering when that A/B measurement itself is what's wanted. Meaningless under Debug,
// which this function never touches.
func testHostRunEnv(options Options) []string {
	if options.testConfig == "Release" && !options.testTiered {
		return []string{"DOTNET_TieredCompilation=0"}
	}
	return nil
}

// testEnvironmentRecord is the configuration-provenance twin of TestHost.cs's own `environment`
// block in go2cs_test_results.json (dotnetRuntime/culture/timezone/shuffleSeed) — recorded here too,
// in the COMPARISON record and from there the proof page, so a verdict carries the level it was
// measured at without a reader needing to cross-reference the results file. Tiered reflects the
// EFFECTIVE state (true unless Release AND not overridden by -test-tiered), matching what
// testHostRunEnv above actually decided, not just the raw flag.
type testEnvironmentRecord struct {
	Configuration string `json:"configuration"`
	Tiered        bool   `json:"tiered"`

	// OracleGoVersion is the bare `go version` output of the toolchain that actually ran the
	// oracle `go test -json` child for THIS comparison — see oracleGoVersion below. Distinct from
	// the test manifest's GoVersion (the converter's own runtime.Version(), i.e. what built go2cs
	// itself): this field answers "what ran the Go side of the comparison", which is unanswerable
	// from evidence after the fact without it (coordinator ruling, 2026-09-02, from a container
	// whose bare `go` silently resolved a different release than the pinned GOROOT). Omitted, not
	// empty-stringed, when the probe itself could not run — a comparison that already completed
	// must not be invalidated by a version probe failing after the fact.
	OracleGoVersion string `json:"oracleGoVersion,omitempty"`

	// Terminal records whether the comparison's DRIVER had a controlling terminal when it ran the
	// two sides — "tty" or "none" — observed by the same primitive Go's own tests decide with
	// (os.OpenFile("/dev/tty", O_RDWR), see driverTerminal below). A driver-context axis every
	// detached sweep is blind to (Q15, 2026-09-04): syscall's TestForeground/TestForegroundSignal
	// skip on BOTH sides without one and RUN on both with one, so a row's count can depend on the
	// terminal the driver happened to have, and a proof page that does not say which context it
	// was taken in cannot be reproduced. Both children inherit the driver's session (nothing in
	// the pipeline calls setsid), so the driver's answer is theirs. Omitted on Windows, where the
	// probe can only ever fail and would say nothing about the row.
	Terminal string `json:"terminal,omitempty"`
}

// testEnvironmentFromOptions derives the record from the same two options testHostRunEnv reads, so
// the two can never disagree about what "tiered" means for a given configuration. Pure and
// side-effect-free by design (see TestTestEnvironmentRecordRoundTrips) — OracleGoVersion is filled
// in separately by the caller, since answering it means actually running a child process.
func testEnvironmentFromOptions(options Options) testEnvironmentRecord {
	return testEnvironmentRecord{
		Configuration: options.testConfig,
		Tiered:        !(options.testConfig == "Release" && !options.testTiered),
	}
}

// oracleGoVersion runs `go version` through the EXACT SAME child-invocation mechanism the oracle
// `go test` command itself uses (runCommandWithTimeout, in the same working directory, under the
// same options-derived environment) — so it resolves the SAME `go` on the SAME PATH the real oracle
// run just did, not whatever `go env GOROOT` claims. GOROOT is a claim about where the toolchain
// SHOULD be; this is a direct observation of the toolchain that ACTUALLY ran, closing the gap a
// container class exposed: bare `go` there resolved 1.24.7 against the corpus's pinned 1.23.12 with
// GOROOT reading correctly the whole time. Best-effort: a comparison the real oracle run already
// completed must not be invalidated by this probe failing after the fact, so an error answers "".
func oracleGoVersion(inputPath string, options Options) string {
	output, err := runCommandWithTimeout(options.testTimeout, inputPath, options, "go", "version")

	if err != nil {
		return ""
	}

	return strings.TrimSpace(output)
}

// Terminal vocabulary for testEnvironmentRecord.Terminal — the two observed states, and the empty
// string for "not probed" (Windows), which omitempty drops from the record.
const (
	driverTerminalPresent = "tty"
	driverTerminalAbsent  = "none"
)

// driverTerminal observes whether THIS process — the pipeline driver, whose session both the
// oracle `go test` child and the converted host inherit — has a controlling terminal, using the
// exact predicate Go's terminal-gated tests use: os.OpenFile("/dev/tty", O_RDWR). Success means a
// controlling terminal exists (the kernel resolves /dev/tty to it, or fails with ENXIO without
// one); the descriptor is closed at once, nothing about the terminal is changed. Windows has no
// /dev/tty and no job control, so the probe is not run there and the field is omitted rather than
// recorded as a meaningless "none".
func driverTerminal() string {
	if runtime.GOOS == "windows" {
		return ""
	}

	tty, err := os.OpenFile("/dev/tty", os.O_RDWR, 0)

	if err != nil {
		return driverTerminalAbsent
	}

	tty.Close()

	return driverTerminalPresent
}

// publishedTestHostPath is the single-file executable publishTestHost produces: the test project's
// own base name (which the template also uses as the AssemblyName), under the deterministic
// publish directory, with the HOST platform's executable suffix — the run always happens on the
// machine that published.
func publishedTestHostPath(outputPath, testProject string) string {
	name := strings.TrimSuffix(filepath.Base(testProject), filepath.Ext(testProject))

	if runtime.GOOS == "windows" {
		name += ".exe"
	}

	return filepath.Join(outputPath, "bin", "tests", "publish", name)
}

func readTestManifest(outputPath string) (testManifest, error) {
	var manifest testManifest
	data, err := os.ReadFile(filepath.Join(outputPath, testManifestFileName))
	if err != nil {
		return manifest, fmt.Errorf("test manifest is missing: %w", err)
	}
	if err := json.Unmarshal(data, &manifest); err != nil {
		return manifest, fmt.Errorf("test manifest is invalid: %w", err)
	}
	return manifest, nil
}

func manifestHasEligibleTests(manifest testManifest) bool {
	for _, test := range manifest.Tests {
		if test.Kind == "test" && test.Status == "included" {
			return true
		}
	}
	return false
}

// manifestCapabilityBlock returns the capability names that leave the package with NO runnable
// converted tests: a capability-blocked TestMain gates every test (Go routes all tests through
// it), and a package whose every declared test blocked itself has nothing to run. A blocked test
// among runnable siblings does NOT block the package (F4) — it is excluded-disclosed.
func manifestCapabilityBlock(manifest testManifest) []string {
	capabilityReason := func(declaration testDeclaration) []string {
		if declaration.Status != "unsupported" || !strings.HasPrefix(declaration.Reason, unsupportedCapabilityReasonPrefix) {
			return nil
		}
		return strings.Split(strings.TrimPrefix(declaration.Reason, unsupportedCapabilityReasonPrefix), ", ")
	}

	if manifest.TestMain != nil {
		if blocked := capabilityReason(*manifest.TestMain); len(blocked) > 0 {
			return blocked
		}
	}

	blockedCapabilities := HashSet[string]{}
	hasIncludedTest := false
	hasBlockedTest := false

	for _, test := range manifest.Tests {
		if test.Kind != "test" {
			continue
		}
		if test.Status == "included" {
			hasIncludedTest = true
			continue
		}
		if blocked := capabilityReason(test); len(blocked) > 0 {
			hasBlockedTest = true
			blockedCapabilities.UnionWith(blocked)
		}
	}

	if hasIncludedTest || !hasBlockedTest {
		return nil
	}

	blocked := blockedCapabilities.Keys()
	sort.Strings(blocked)
	return blocked
}

func validateTestManifest(inputPath, outputPath string, options Options) error {
	manifest, err := readTestManifest(outputPath)
	if err != nil {
		return err
	}
	target := strings.Split(options.targetPlatform, "/")
	if len(target) != 2 || manifest.TargetGOOS != target[0] || manifest.TargetGOARCH != target[1] {
		return fmt.Errorf("test manifest is stale: target is %s/%s, requested %s", manifest.TargetGOOS, manifest.TargetGOARCH, options.targetPlatform)
	}
	// The digest recomputes over the CURRENT inputs (fresh fixture glob — a newly added testdata
	// file is a staleness signal the manifest's recorded list could never carry, F7).
	digest, err := testInputDigest(inputPath, outputPath, options, converterRevision())
	if err != nil {
		return fmt.Errorf("validate test manifest inputs: %w", err)
	}
	if digest != manifest.InputDigest {
		return fmt.Errorf("test manifest is stale: input digest changed (run -tests -test-action convert)")
	}
	return nil
}

type normalizedTestEvent struct {
	Test    string  `json:"test"`
	Action  string  `json:"action"`
	Output  string  `json:"output,omitempty"`
	Elapsed float64 `json:"elapsed,omitempty"`
}

type testComparison struct {
	Package   string            `json:"package"`
	Status    string            `json:"status"`
	Go        map[string]string `json:"go"`
	CSharp    map[string]string `json:"csharp"`
	Matched   bool              `json:"matched"`
	Skipped   []string          `json:"skipped"`
	Disclosed []string          `json:"disclosed"`
	Excluded  []string          `json:"excluded"`
	Errors    []string          `json:"errors"`

	// Gated is the row-level detail behind the capability-gated members of Excluded, and it exists
	// because a DECLARATION-keyed gate is not a declaration-sized omission: eligibleTerminalTestResults
	// cuts a verdict row at its first "/", so gating one table-driven test can withdraw dozens of rows
	// from both sides at once. A matched count that silently absorbed that would be the quiet kind of
	// dishonest, so the rows are enumerated from the UNFILTERED `go test` results and published on the
	// proof page. Empty for every package with no gated declaration, which is nearly all of them.
	Gated []capabilityGatedDeclaration `json:"gated,omitempty"`

	// Withdrawn is the same honesty for the DOWNWARD disclosure dual (matchTerminalStatuses'
	// withdrawal rule): the Go-side verdict rows underneath a signature-matched disclosed root,
	// which the converted host never reached because the disclosed failure precedes its case
	// fan-out. Removed from the Go map so every count self-corrects, published here and on the
	// proof page so the omission is stated rather than absorbed. Empty for every package whose
	// disclosed tests have no subtests, which is all of them before crypto/tls's TestBogoSuite.
	Withdrawn []string `json:"withdrawn,omitempty"`

	// TestFilter records the -test-filter expression a GATED run was produced under, and it exists
	// because the record does not otherwise know how it was made. A filtered run rewrites the SAME
	// go2cs_test_comparison.json a full run writes, with nothing distinguishing the two -- and the
	// fleet's restore step cannot clear it either, since the file is gitignored and `git clean -fd`
	// skips ignored paths. So a diagnostic record survives into the next run looking exactly like
	// that run's own output, which is how a gated census once reported a row "bankable" off its own
	// filter's survivor set. Carrying the EXPRESSION rather than a bare true is deliberate: it says
	// WHICH names could have been withheld, so a reader can tell whether an absence is the filter or
	// the conversion. Empty and omitted for every ungated run, so an unfiltered record is unchanged.
	//
	// NOTE the key is testFilter and not `gated`: `gated` is already taken by the Gated array above,
	// which is live data (net/http carries one entry, TestTransportGCRequest), so a boolean of that
	// name would collide with an array on the very rows most worth reading carefully.
	TestFilter string `json:"testFilter,omitempty"`

	// Environment carries the publish/run configuration this comparison was measured at — see
	// testEnvironmentRecord. Present on every comparison record (not omitempty): a reader must never
	// have to assume Debug by absence, since the whole point is that a verdict states its own level.
	Environment testEnvironmentRecord `json:"environment"`
}

// capabilityGatedDeclaration records one test declaration the converted host provably cannot run,
// the capability it would need, and every verdict row `go test` reports underneath it.
type capabilityGatedDeclaration struct {
	Name         string   `json:"name"`
	Capabilities string   `json:"capabilities"`
	Rows         []string `json:"rows"`
}

// platformSkipClass is the ONE disclosure class that is load-bearing in the compare oracle
// rather than descriptive. Every other class labels a Go=pass/C#=fail divergence and changes
// nothing about how the pair is matched; this one is the sole key that admits the
// Go=pass/C#=SKIP shape (matchTerminalStatuses), so the ruling's anti-laundering clause is
// enforced structurally: a manifest cannot turn some other class's failure into a skip, and a
// skip cannot be disclosed under some other class. Minted 2026-08-25 with crypto/cipher's
// TestGCMAsm (board: "TestGCMAsm closes as a source-defined platform skip").
const platformSkipClass = "platform-skip"

// cgoConfigurationClass names a verdict whose divergence is the residue of the corpus and the
// oracle being pinned to DIFFERENT cgo configurations for one seam: the converted side is
// behaviourally a cgo-LINKED build there (runtime_doAllThreadsSyscall answers ENOTSUP, which is
// precisely the answer a cgo-linked Go build gives), the oracle is pinned cgo-OFF -- the state of
// record on every platform since 2026-09-03 -- and Go's OWN test source turns that difference
// into a skip whose message the entry pins. Its shape is therefore Go=pass / C#=skip, exactly
// platformSkipClass's, and it is admitted for that shape and NO other (see classAdmitsSkipShape).
//
// It was minted 2026-08-27 into three syscall entries for exactly this shape, and could never
// fire: the skip shape was unlocked for platformSkipClass ALONE, so the class fell to the generic
// arm below, which requires C#=fail. A guard that cannot go green, one layer down -- and it
// stayed invisible for as long as the Linux bank ran cgo ON, where Go skips those tests too (the
// ENOTSUP coincidence) and they matched skip/skip. The cgo-OFF ruling made the class live, and
// the 2026-09-03 Linux leveling re-sweep is what read it: four unabsorbed verdicts on a banked
// row. Admitting the class is the remedy rather than re-labelling the entries to platform-skip,
// because the class name carries WHY -- the cgo axis -- and re-labelling would throw that away
// (coordinator ruling, mailbox 82ec6654c).
const cgoConfigurationClass = "cgo-configuration"


// deferredClass and structuralClass are the TWO labels every allocation-count disclosure resolves
// into (coordinator ruling 2026-09-05, owner-ratified the same day, mailbox bd08f67c6 / 7c2d7ee44 /
// 6087c58c7). They exist because `alloc-profile` had come to carry two different claims under one
// word, and the amendment to ruling #1 forced them apart: an AllocsPerRun-style assertion measures
// Go's escape analysis -- a compiler optimization the CLR JIT does not perform -- rather than a
// behavioural property, so such an assertion is never disclosed as CLR-STRUCTURAL merely because it
// fails today, but it MAY be deferred against a named plan to reach it.
//
//   deferred   -- the CLR CAN meet the assertion in principle; the entry names the retirement plan
//                 that will. One plan may cover MANY entries by MECHANISM FAMILY (a box minted per
//                 address-take, an element take, an out-parameter, an intermediate buffer in a
//                 string conversion), so the requirement is met by family rather than by a bespoke
//                 design per entry. It is a COMMITMENT, not an excuse: the owner's strengthening
//                 makes the plan a hard requirement, and a plan whose design record is retired
//                 without a replacement fails the row at its next sweep exactly as a regression
//                 would.
//   structural -- the entry carries a PROOF, stated in its reason, that no managed implementation
//                 can meet the assertion, with the object Go keeps off the heap NAMED. Expected to
//                 be rare, and each one is a claim the next reader may falsify.
//
// The bare `alloc-profile` label is LEGACY and retires per row as rows re-sweep; it is still
// accepted here so a row that has not re-swept keeps comparing, and its entries re-classify at
// that row's next rebank rather than wholesale.
const (
	deferredClass   = "deferred"
	structuralClass = "structural"
)

// classAdmitsSkipShape reports whether a disclosure class may absorb a Go=pass / C#=skip pair.
// ONE predicate, read by BOTH arms below -- the skip arm's admission and the generic arm's
// exclusion -- so the two can never drift apart again. Drift is exactly what let cgo-configuration
// be admitted by neither: the shape was named in one arm by a bare class comparison and excluded
// in the other by a second, and a class that matched neither list simply fell through.
func classAdmitsSkipShape(class string) bool {
	return class == platformSkipClass || class == cgoConfigurationClass
}

// hostFatalClass names a test the converted host cannot RUN AT ALL -- not one whose verdict
// diverges, but one whose execution takes the whole process down, so every test after it in its
// phase is lost too. runtime/debug's TestPanicOnFault is the first member: it mmaps a PROT_READ
// page and writes to it, expecting SetPanicOnFault to convert the hardware fault into a
// recoverable panic; Go installs a SIGSEGV handler, the CLR on Linux has no SEH equivalent, and
// the process dies -- costing that row NINE verdicts rather than one.
//
// WIDENED (coordinator ruling, 2026-09-05): the class admits a CRASH or a deadline-consuming HANG,
// because the charter is "every test after it in its phase is lost too" and a test that never
// returns costs exactly that -- runtime/pprof's TestGoroutineProfileLabelRace is the first hang
// member, spinning forever in a /reset subtest whose exit condition is a pprof label the converted
// goroutine profile withholds (measured alone: zero verdicts, the whole package deadline consumed).
//
// It is the one class that CHANGES WHAT RUNS rather than labelling what a run produced. The
// named test is withdrawn from BOTH sides by name (`go test -skip` and the host's own `--skip`,
// one string handed to each) so the comparison stays symmetric, and it is counted in DISCLOSED
// -- never hidden. A bankable exclusion differs from the gated census the doctrine forbids in
// exactly this: it is named in the committed manifest with a class and a reason, auditable in
// the same place as every other disclosure, and it shows up in the row's published counts.
//
// It is EXCLUDED from matchTerminalStatuses on purpose (see the arm there): a host-fatal test
// produces no verdict on either side, so there is no status pair for it to absorb, and admitting
// it as one would make it a second way to disclose a failure -- the laundering the ruling forbids.
const hostFatalClass = "host-fatal"

// hostFatalSkipExpression builds the ONE regexp handed verbatim to both sides. Anchored per name
// so `TestFoo` cannot withdraw `TestFooBar`, and sorted so the string is stable run to run (a
// reader comparing two logs should see the same expression, not a map-iteration reshuffle).
// Returns "" when the manifest names no host-fatal test, which is every package but one.
func hostFatalSkipExpression(disclosures map[string]testDisclosure) string {
	var names []string
	for name, d := range disclosures {
		if d.Class == hostFatalClass {
			names = append(names, regexp.QuoteMeta(name))
		}
	}
	if len(names) == 0 {
		return ""
	}
	sort.Strings(names)
	return "^(?:" + strings.Join(names, "|") + ")$"
}

// hostFatalMintViolations enforces the class's mint rule (coordinator ruling, 2026-09-02): a
// host-fatal entry must NOT name a test that any committed proof page records as a MATCHING
// verdict. The rule exists because this class is the only one that changes what RUNS, and the
// manifest is shared across platforms -- so an entry naming a test that exists and agrees on
// another flavour would apply the skip there too and convert a working row into a disclosed one,
// with both sides agreeing because both were told to skip. Silent shrinkage of a banked row.
//
// Checked from COMMITTED DATA rather than a cross-platform run: docs/validation/current/*.md is
// the corpus's own record of what agreed where, it is in the repository already, and one pass over
// it costs nothing. runtime/debug's TestPanicOnFault passes the rule only because panic_test.go is
// //go:build unix -- the Windows page lists nine tests and never mentions it -- which is a fact
// about that test rather than a property of the class, and precisely why the rule is mechanical.
//
// A `goos` qualifier on the entry would be more expressive; it is deliberately NOT built until a
// real entry needs one. Refusing is enough while the answer is always "no such row".
func hostFatalMintViolations(outputPath string, disclosures map[string]testDisclosure) []string {
	fatal := map[string]bool{}
	for name, d := range disclosures {
		if d.Class == hostFatalClass {
			fatal[name] = true
		}
	}
	if len(fatal) == 0 {
		return nil
	}
	root := findGo2CSRootAbove(outputPath)
	if root == "" {
		return nil
	}
	dir := filepath.Join(filepath.Dir(root), "docs", validationDocsDirName, validationCurrentDirName)
	pages, err := filepath.Glob(filepath.Join(dir, "*.md"))
	if err != nil || len(pages) == 0 {
		// No committed pages to check against is not a violation -- a fresh clone or a staging
		// root legitimately has none. The rule can only refuse on POSITIVE evidence of agreement.
		return nil
	}
	row := regexp.MustCompile("^\\|\\s*`([^`]+)`\\s*\\|([^|]*)\\|([^|]*)\\|")
	var violations []string
	for _, page := range pages {
		data, err := os.ReadFile(page)
		if err != nil {
			continue
		}
		for _, line := range strings.Split(string(data), "\n") {
			m := row.FindStringSubmatch(strings.TrimSpace(line))
			if m == nil || !fatal[m[1]] {
				continue
			}
			if strings.TrimSpace(m[2]) == strings.TrimSpace(m[3]) {
				violations = append(violations, fmt.Sprintf(
					"%s is disclosed %s, but %s records it as a MATCHING verdict (%s/%s): excluding it "+
						"would withdraw a row that platform runs successfully",
					m[1], hostFatalClass, filepath.Base(page), strings.TrimSpace(m[2]), strings.TrimSpace(m[3])))
			}
		}
	}
	sort.Strings(violations)
	return violations
}

// hostFatalNames lists the excluded tests for the DISCLOSED column, sorted. They appear in
// neither side's results by construction, so without this the counts would silently shrink --
// which is the hidden-test outcome the ruling forbids.
func hostFatalNames(disclosures map[string]testDisclosure) []string {
	var out []string
	for name, d := range disclosures {
		if d.Class == hostFatalClass {
			out = append(out, name+" ("+hostFatalClass+"): "+d.Reason)
		}
	}
	sort.Strings(out)
	return out
}

// testDisclosure pins one test-level disclosed divergence — extending the declaration-level
// "disclosed-unsupported" vocabulary (req §2.7) to individual test outcomes. A hand-owned,
// repo-committed manifest beside the converted package lists tests whose Go=pass/C#=fail
// divergence is provably unsatisfiable in the managed runtime (e.g. the AllocsPerRun
// allocation-count/-profile classes: the CLR allocates where Go's compiler stack-allocates, so
// a malloc-counting shim would fail the same asserts). The signature pin is the integrity
// guard: the oracle reclassifies ONLY a failure whose captured C# output contains the pinned
// substring — a disclosed test failing any OTHER way (a regression beyond the documented
// divergence) is still a mismatch, and a package without a manifest compares strictly.
//
// The single exception to "Go=pass/C#=fail" is platformSkipClass, where the C# side SKIPS via
// the upstream test's own skip statement because the managed corpus genuinely is the platform
// that skip describes; see the arm in matchTerminalStatuses for the admission rule.
type testDisclosure struct {
	Name      string `json:"name"`
	Class     string `json:"class"`
	Signature string `json:"signature"`
	Reason    string `json:"reason"`

	// HostConditional annotates a pinned row whose GO side is not deterministic, and its
	// non-empty value IS the marker — one sentence naming the environmental dependency, so a
	// row can never be marked without saying what it depends on (coordinator ruling,
	// 2026-08-20). A disclosure asserts *Go passes, C# provably cannot*, which is only stable
	// while the Go side is stable; where the baseline itself is load- or network-dependent the
	// pin goes red in BOTH directions — when Go starts failing, and again when it goes back to
	// passing on a quieter host. An annotated row is therefore accepted in EXACTLY two shapes,
	// and accounts as DISCLOSED — never as matching — in both: Go pass / C# fail (the pinned
	// divergence) and Go fail / C# fail (agreement, on a host where the Go premise fails). The
	// signature pin still governs both, so any movement on the C# side fails the comparison;
	// the tolerance is confined to the half that was never deterministic. Ruled IN only for a
	// row with coordinator-accepted rooting evidence — crypto/tls's TestBogoSuite, whose Go
	// side is decided by network reachability of the boringssl module and the runner's own
	// 10-minute child deadline, is the first and only member.
	HostConditional string `json:"hostConditional,omitempty"`

	// HostConditionalSignature is the SECOND accepted failure text, and it exists because an
	// environmentally-conditional test does not fail the same way for both of its environmental
	// reasons. crypto/tls's TestBogoSuite is pinned on `bogo failed: exit status 1` — the arm
	// taken when the BoGo runner RAN and outlived its own deadline (bogo_shim_test.go:414). A
	// host that simply does not have the runner never reaches that arm: it dies four lines into
	// the test at bogo_shim_test.go:364, `failed to download boringssl`, and so does Go —
	// identically, at the same line, for the same reason. Measured 2026-08-28 by pointing
	// GOMODCACHE at an empty directory with GOPROXY=off: both runtimes report
	// `--- FAIL: TestBogoSuite`, the comparison calls the C# side MOVED, and the package cannot
	// validate at all on a host that never had the capability.
	//
	// Admission is deliberately confined to the HostConditional fail/fail shape, and the
	// confinement is the whole safety argument: there, GO ITSELF failed the same way, so the
	// converted side is provably not the half that moved. In the Go pass / C# fail shape the
	// primary Signature still governs alone — a run whose Go side downloaded the module fine
	// while the converted side could not is a real divergence, and pinning it is exactly what
	// this field must never launder. Empty for every disclosure that is not host-conditional.
	HostConditionalSignature string `json:"hostConditionalSignature,omitempty"`

	// Want, Reading and Plan carry the DEFERRED class's contract (see deferredClass below). They
	// are what makes a deferred entry a commitment rather than an excuse, and the loader refuses a
	// deferred entry missing any of them:
	//
	//   Want    -- the assertion's own bound, verbatim enough to read without opening the test
	//              ("0 allocations"; "1 allocation per leg").
	//   Reading -- the MEASURED current value with the CONFIGURATION named, because a reading taken
	//              at another configuration is not comparable (Release + tiering off is the
	//              measurement of record) and a reading with no tree named cannot be re-checked.
	//   Plan    -- the retirement plan: the design record and the increment that closes it. One
	//              plan may serve MANY entries by mechanism family, so this names the family's
	//              record rather than demanding a bespoke design per entry.
	//
	// Empty on every entry that is not deferred. A structural entry must NOT carry a Plan: its
	// claim is that no managed implementation can meet the assertion, and naming a plan to meet it
	// contradicts that in the same breath -- the loader refuses that combination too, which is the
	// cheap mechanical guard against an entry mislabelled by copy-paste.
	Want    string `json:"want,omitempty"`
	Reading string `json:"reading,omitempty"`
	Plan    string `json:"plan,omitempty"`

	// Floor and Proof carry the ruling of 2026-09-05 (mailbox 310d2f6f9), which the reflect census
	// forced: an entry can have BOTH a structural floor and a deferrable excess above it, and
	// neither label alone is honest about that shape. `DeepEqual(any, any)` boxes a value type into
	// `object` -- an allocation the CLR's object model requires, a floor of two per run with a proof
	// -- while the measured readings run to 53 objects, an excess that is ordinary reducible bridge
	// work. Labelling it structural would bury 51 reducible objects; labelling it deferred would
	// name a want (0) that no plan can reach.
	//
	// So a DEFERRED entry may carry a floor: an object count GREATER than its want, with its own
	// proof sketch beside it. Its retirement condition becomes "the host's reading equals the
	// floor" rather than "equals the want", and at that point the entry RE-LABELS structural with
	// the proof already attached and its plan discharged. The plan requirement is unchanged -- the
	// excess is what the plan retires. A floor is a CLAIM the census can falsify: a segment reading
	// zero where a floor was predicted retires the floor, not the entry.
	//
	// Zero means ABSENT, which is sound because a floor must exceed a want and a want is never
	// negative, so a legal floor is always at least 1. A floor on a STRUCTURAL entry is refused:
	// that label's whole claim is that the reading cannot be reduced, so a floor beneath an excess
	// contradicts it exactly as a plan does.
	Floor int    `json:"floor,omitempty"`
	Proof string `json:"proof,omitempty"`
}


// leadingInteger reads the integer a disclosure's `want` leads with ("0 allocations" -> 0), which is
// what a floor is compared against. Deliberately strict: it does not hunt for a number anywhere in
// the string, because a want like "at most 1 per leg, 2 on darwin" has no single number a floor can
// exceed, and guessing which one was meant is worse than refusing the pairing.
func leadingInteger(text string) (int, bool) {
	trimmed := strings.TrimSpace(text)
	end := 0
	for end < len(trimmed) && trimmed[end] >= '0' && trimmed[end] <= '9' {
		end++
	}
	if end == 0 {
		return 0, false
	}

	value, err := strconv.Atoi(trimmed[:end])
	if err != nil {
		return 0, false
	}

	return value, true
}

// hostConditionalFailureMatches reports whether a C# failure text is one of the environmental
// failure arms an annotated disclosure accepts in its fail/fail shape. Only reachable from that
// shape; the primary signature governs everywhere else.
func hostConditionalFailureMatches(disclosure testDisclosure, csOutput string) bool {
	if strings.Contains(csOutput, disclosure.Signature) {
		return true
	}

	return disclosure.HostConditionalSignature != "" && strings.Contains(csOutput, disclosure.HostConditionalSignature)
}

type testDisclosureManifest struct {
	SchemaVersion int              `json:"schemaVersion"`
	Disclosures   []testDisclosure `json:"disclosures"`

	// Notes are optional package-level caveats the generated proof page must carry — facts about
	// the comparison's MEANING that no verdict row can express, rendered verbatim above the
	// verdicts. First consumer: crypto/tls's expired-fixture ceiling, where four tests fail
	// AGREEING on both runtimes because the suite's test certificates expired 2025-01-01 — the
	// agreement is honest, but a reader must know the ceiling moves with the calendar. Hand-owned
	// here rather than hand-edited into the page, because the page is regenerated on every
	// re-validation and a hand edit would not survive one.
	Notes []string `json:"notes,omitempty"`
}

// loadTestDisclosures reads the package's hand-owned disclosure manifest. A missing file is the
// normal case (no disclosures — strict comparison); a malformed or incomplete manifest is an
// error, never a silent no-op, because a broken disclosure must not widen the oracle. Every
// field is required: an empty signature would substring-match ANY failure, defeating the pin.
func loadTestDisclosures(outputPath string) (map[string]testDisclosure, []string, error) {
	data, err := os.ReadFile(filepath.Join(outputPath, testDisclosureFileName))
	if os.IsNotExist(err) {
		return nil, nil, nil
	}
	if err != nil {
		return nil, nil, err
	}

	var manifest testDisclosureManifest
	if err := json.Unmarshal(data, &manifest); err != nil {
		return nil, nil, err
	}

	disclosures := make(map[string]testDisclosure, len(manifest.Disclosures))
	for _, disclosure := range manifest.Disclosures {
		// The signature requirement is CLASS-AWARE, and hostFatalClass is the one exemption: its
		// test is withdrawn from both command lines and produces no verdict on either side, so
		// there is no captured output for a signature to pin. Requiring one would mean inventing a
		// string nothing can ever match -- worse than useless, because it would read as a pin.
		// Every other class keeps the requirement: the signature IS the integrity guard that stops
		// a disclosure absorbing a regression beyond the documented divergence.
		if disclosure.Name == "" || disclosure.Class == "" || disclosure.Reason == "" {
			return nil, nil, fmt.Errorf("disclosure entries require name, class, and reason: %+v", disclosure)
		}
		if disclosure.Signature == "" && disclosure.Class != hostFatalClass {
			return nil, nil, fmt.Errorf("disclosure entries require a signature except for %s: %+v", hostFatalClass, disclosure)
		}

		// The DEFERRED class's contract, enforced where every other required field is (coordinator
		// ruling 2026-09-05, owner-ratified): a deferred entry is a COMMITMENT to reach the
		// assertion, so an entry that names no plan is refused outright rather than accepted as a
		// quieter disclosure. Want and Reading are required with it because a plan with no bound
		// and no measured starting point cannot be scored at the next sweep -- the sweep prints the
		// reading beside the want, and a reading moving AWAY from the want fails the row exactly as
		// a matched verdict flipping would.
		if disclosure.Class == deferredClass {
			missing := []string{}
			if strings.TrimSpace(disclosure.Want) == "" {
				missing = append(missing, "want")
			}
			if strings.TrimSpace(disclosure.Reading) == "" {
				missing = append(missing, "reading")
			}
			if strings.TrimSpace(disclosure.Plan) == "" {
				missing = append(missing, "plan")
			}
			if len(missing) > 0 {
				return nil, nil, fmt.Errorf("deferred disclosure %s requires %s: a deferred entry is a commitment to reach the assertion, and one that names no plan is refused", disclosure.Name, strings.Join(missing, ", "))
			}
		}


		// The FLOOR's own contract (ruling 2026-09-05). A floor is only meaningful where the want is
		// a number the reading can be compared against, so the want must LEAD with its integer for
		// the relation to be checkable at all -- refusing an uncheckable pairing is the difference
		// between a guard and a decoration.
		if disclosure.Floor != 0 {
			if disclosure.Class == structuralClass {
				return nil, nil, fmt.Errorf("structural disclosure %s must not name a floor: that label claims the reading cannot be reduced, so an excess above a floor belongs to the deferred class", disclosure.Name)
			}
			if disclosure.Floor < 0 {
				return nil, nil, fmt.Errorf("disclosure %s has a negative floor (%d): a floor is an object count", disclosure.Name, disclosure.Floor)
			}
			if strings.TrimSpace(disclosure.Proof) == "" {
				return nil, nil, fmt.Errorf("disclosure %s names a floor but no proof: a floor is a CLAIM the census can falsify, and one with no sketch cannot be", disclosure.Name)
			}

			wantValue, ok := leadingInteger(disclosure.Want)
			if !ok {
				return nil, nil, fmt.Errorf("disclosure %s names a floor, so its want must LEAD with the number the floor is compared against (got %q)", disclosure.Name, disclosure.Want)
			}
			if disclosure.Floor <= wantValue {
				return nil, nil, fmt.Errorf("disclosure %s has a floor (%d) that does not exceed its want (%d): a floor names the part of the reading no plan can remove, so an entry whose floor equals its want has nothing deferred and is simply structural", disclosure.Name, disclosure.Floor, wantValue)
			}
		}
		// A structural entry claims no managed implementation can meet the assertion; naming a plan
		// to meet it contradicts that claim in the same entry, and the pairing is the shape a
		// mislabelled copy-paste takes. Refused so the two labels cannot blur back together.
		if disclosure.Class == structuralClass && strings.TrimSpace(disclosure.Plan) != "" {
			return nil, nil, fmt.Errorf("structural disclosure %s must not name a retirement plan: its claim is that the assertion cannot be met, so a plan to meet it belongs to the deferred class", disclosure.Name)
		}
		if _, exists := disclosures[disclosure.Name]; exists {
			return nil, nil, fmt.Errorf("duplicate disclosure for %s", disclosure.Name)
		}

		// The host-conditional marker IS its sentence, so a blank one would widen the oracle
		// (a second accepted status pair) while naming nothing — the one thing the ruling
		// requires of an annotated row. Same reasoning as the required fields above: a broken
		// disclosure must fail loudly rather than quietly tolerate more.
		if disclosure.HostConditional != "" && strings.TrimSpace(disclosure.HostConditional) == "" {
			return nil, nil, fmt.Errorf("host-conditional disclosure %s must name its environmental dependency", disclosure.Name)
		}

		// The second failure arm is admitted ONLY inside the host-conditional fail/fail shape, so
		// on an unannotated entry it would be a pin that never governs anything — a widening its
		// author would read as recorded. Refused rather than ignored, and blank-checked for the
		// same reason the signature is: an empty substring matches every failure.
		if disclosure.HostConditionalSignature != "" {
			if disclosure.HostConditional == "" {
				return nil, nil, fmt.Errorf("disclosure %s carries a hostConditionalSignature without being host-conditional; the second arm is only ever admitted in the fail/fail shape", disclosure.Name)
			}
			if strings.TrimSpace(disclosure.HostConditionalSignature) == "" {
				return nil, nil, fmt.Errorf("host-conditional disclosure %s has a blank hostConditionalSignature, which would match every failure", disclosure.Name)
			}
		}
		disclosures[disclosure.Name] = disclosure
	}

	return disclosures, manifest.Notes, nil
}

// matchTerminalStatuses compares the two sides' terminal statuses per test. A test matches when
// both sides report the SAME terminal status (F1) — skip==skip is agreement, disclosed via the
// returned skipped list rather than flagged as failure (real stdlib suites skip routinely). A
// Go=pass/C#=fail divergence pinned by the package's hand-owned disclosure manifest — exact
// test name AND the pinned signature present in the captured C# failure output — is returned
// as disclosed-divergent instead of a mismatch; any other failure shape of a disclosed test
// (different signature, different status pair, a subtest) remains a strict mismatch. The one
// exception is a HOST-CONDITIONAL disclosure (see testDisclosure.HostConditional), which adds a
// SECOND accepted status pair — Go fail / C# fail, still signature-pinned — and accounts as
// disclosed there too rather than as an agreed failure.
// addressTokenPattern matches a 0x-hex token in a subtest name — a pointer ADDRESS embedded via
// %v/%p, run-varying on BOTH sides by construction (Go's own reruns disagree with themselves).
var addressTokenPattern = regexp.MustCompile(`0x[0-9a-fA-F]+`)

// pairAddressVariantNames re-keys the ONE-SIDED rows of the two result maps whose names differ
// only by embedded 0x-hex address tokens onto a shared normalized key, so the status match
// compares them as one row (errors' TestAsValidation/*string(0xc…) names). This is the SECOND
// phase of matching — exact names already paired stay untouched, so a deterministic hex literal
// used as a subtest name is never collapsed. Only UNAMBIGUOUS 1:1 pairs are re-keyed: a
// normalized key claimed by multiple names on either side, or colliding with an existing exact
// name, keeps all originals — the rows stay one-sided and the comparison fails loud, never
// masking. csOutputs follows the C# rename so disclosure-signature matching keeps its text.
func pairAddressVariantNames(goResults, csResults, csOutputs map[string]string) {
	goOnly := make(map[string][]string)
	csOnly := make(map[string][]string)

	for name := range goResults {
		if _, matched := csResults[name]; !matched {
			if key := addressTokenPattern.ReplaceAllString(name, "0x?"); key != name {
				goOnly[key] = append(goOnly[key], name)
			}
		}
	}

	for name := range csResults {
		if _, matched := goResults[name]; !matched {
			if key := addressTokenPattern.ReplaceAllString(name, "0x?"); key != name {
				csOnly[key] = append(csOnly[key], name)
			}
		}
	}

	for key, goNames := range goOnly {
		csNames := csOnly[key]

		if len(goNames) != 1 || len(csNames) != 1 {
			continue
		}

		if _, exists := goResults[key]; exists {
			continue
		}

		if _, exists := csResults[key]; exists {
			continue
		}

		goResults[key] = goResults[goNames[0]]
		delete(goResults, goNames[0])
		csResults[key] = csResults[csNames[0]]
		delete(csResults, csNames[0])

		if output, ok := csOutputs[csNames[0]]; ok {
			csOutputs[key] = output
			delete(csOutputs, csNames[0])
		}
	}
}

func matchTerminalStatuses(names []string, goResults, csResults map[string]string, disclosures map[string]testDisclosure, csOutputs map[string]string) (mismatches, skipped, disclosed, withdrawn []string) {
	// Deepest names classify FIRST: a subtest failure rolls up to its ancestors in BOTH
	// runtimes, so an ancestor whose Go=pass/C#=fail divergence is PURELY the aggregation of
	// disclosed descendants — no failure output of its own, no own disclosure entry, at least
	// one disclosed descendant, and NO mismatched descendant — is itself disclosed-divergent
	// (encoding/binary's TestSizeAllocs: every failing child is a pinned alloc-profile
	// disclosure; the t.Run parent carries no text). Any other ancestor failure stays a strict
	// mismatch — the aggregation rule can never mask an undisclosed child.
	ordered := make([]string, len(names))
	copy(ordered, names)
	sort.SliceStable(ordered, func(i, j int) bool {
		return strings.Count(ordered[i], "/") > strings.Count(ordered[j], "/")
	})

	// The DOWNWARD dual of that ancestor aggregation, resolvable UP FRONT because it depends
	// only on the manifest and the two top-level rows: a disclosure ROOT is a pinned test whose
	// own Go=pass/C#=fail divergence matches its signature, and a Go-side row UNDERNEATH one —
	// a subtest `go test` ran that the converted host never reached, because the disclosed test
	// failed at its root before its case fan-out — is the disclosed failure's mechanical
	// consequence, not an independent divergence. Those rows are WITHDRAWN: returned by name so
	// the caller reports and subtracts them, never silently dropped (req §2.7), and never
	// widened — a C#-side row that EXISTS under a disclosed root still compares strictly, and a
	// Go-only row under anything that is not a signature-matched disclosure root stays a
	// mismatch. First consumer: crypto/tls's TestBogoSuite, whose Go run fans out 3,242 BoGo
	// case rows the disclosed (host-limit) root failure precedes.
	//
	// A HOST-CONDITIONAL row roots on its second accepted shape too (Go fail / C# fail): there
	// the Go side is the half that broke, and its fan-out is the annotated row's own baseline
	// flapping — TestBogoSuite's 3,243 BoGo case rows appear on a host whose `go test` actually
	// reached the matrix and failed it. Those children are the same mechanical consequence and
	// withdraw the same way; counting them as one-sided rows would flood the comparison with
	// 3,243 mismatches that say nothing about the converted code. The C# side is pinned by
	// signature in BOTH shapes, so the root admission never widens on the half that can be
	// strict.
	disclosureRoots := HashSet[string]{}
	for name, disclosure := range disclosures {
		if csResults[name] != "fail" {
			continue
		}

		if goResults[name] == "pass" && strings.Contains(csOutputs[name], disclosure.Signature) {
			disclosureRoots.Add(name)
			continue
		}

		// The fail/fail shape admits the annotated row's SECOND environmental arm as well, so a
		// root that withdrew its fan-out under one arm withdraws it under the other too.
		if goResults[name] == "fail" && disclosure.HostConditional != "" && hostConditionalFailureMatches(disclosure, csOutputs[name]) {
			disclosureRoots.Add(name)
		}
	}

	mismatchNames := HashSet[string]{}
	disclosedNames := HashSet[string]{}

	for _, name := range ordered {
		goStatus, goOK := goResults[name]
		csStatus, csOK := csResults[name]

		// The SECOND accepted shape of a host-conditional row, intercepted before the
		// equal-status arm below would silently count it as an agreed failure. Go fail /
		// C# fail is agreement, but agreement reached because the annotated row's own Go
		// premise failed for an environmental reason — so the row accounts as DISCLOSED, never
		// as matching, and the roster arithmetic stays host-stable (crypto/tls reads 400 + 2 on
		// every machine rather than 401 + 1 on a host whose BoGo baseline broke). The signature
		// still governs: a C# side that fails some OTHER way has MOVED, and moving is exactly
		// what this pin exists to catch, so it is a strict mismatch under the same wording the
		// first shape uses.
		if disclosure, ok := disclosures[name]; ok && disclosure.HostConditional != "" &&
			goOK && csOK && goStatus == "fail" && csStatus == "fail" {
			if hostConditionalFailureMatches(disclosure, csOutputs[name]) {
				disclosed = append(disclosed, name)
				disclosedNames.Add(name)
				continue
			}

			mismatches = append(mismatches, fmt.Sprintf("%s: Go=%q C#=%q (failure does not match the disclosed %s signature %q)",
				name, goStatus, csStatus, disclosure.Class, disclosure.Signature))
			mismatchNames.Add(name)
			continue
		}

		if !goOK || !csOK || goStatus != csStatus {
			if goOK && !csOK && underDisclosureRoot(name, disclosureRoots) {
				withdrawn = append(withdrawn, name)
				continue
			}

			// The THIRD accepted shape: a SOURCE-DEFINED PLATFORM SKIP. Go's own test source
			// writes a skip branch for a platform that lacks some property, and the managed
			// corpus IS such a platform — by design and permanently (crypto/cipher's TestGCMAsm
			// skips when the assembly GCM type equals the generic one; the converted corpus has
			// no .s codepaths at all). So the divergence is between two PLATFORMS' verdicts on
			// one test, not between Go and the conversion on one platform, and the honest record
			// is the skip Go's source defines rather than a manufactured second implementation
			// whose only consumer would be a differential test.
			//
			// Admission is deliberately narrow and structural: the class must be
			// platformSkipClass (no other class unlocks this shape), and the signature must
			// appear in the C# side's own skip output — which is the UPSTREAM skip message, so a
			// harness-injected or conversion-added skip cannot be laundered through here. A
			// platform-skip row whose C# side skips for some OTHER reason has MOVED, and moving
			// is exactly what the pin exists to catch.
			// The classes that unlock this shape are exactly the ones classAdmitsSkipShape names
			// (platform-skip and cgo-configuration); the signature requirement below is identical
			// for both, so admitting a second class widens WHICH manifests may absorb a skip and
			// nothing about WHAT is absorbed.
			if disclosure, ok := disclosures[name]; ok && classAdmitsSkipShape(disclosure.Class) &&
				goOK && csOK && goStatus == "pass" && csStatus == "skip" {
				if strings.Contains(csOutputs[name], disclosure.Signature) {
					disclosed = append(disclosed, name)
					disclosedNames.Add(name)
					continue
				}

				mismatches = append(mismatches, fmt.Sprintf("%s: Go=%q C#=%q (skip does not match the disclosed %s signature %q)",
					name, goStatus, csStatus, disclosure.Class, disclosure.Signature))
				mismatchNames.Add(name)
				continue
			}

			// The SKIP-SHAPE classes (classAdmitsSkipShape: platform-skip and cgo-configuration)
			// are EXCLUDED here on purpose: each admits exactly one shape, the skip arm above, so
			// a row of theirs whose C# side FAILS has moved and must read as a mismatch even if
			// the failure text happens to contain the pinned skip message. Without this either
			// class would be a second way to disclose a failure, which is the laundering the
			// ruling forbids -- and reading the SAME predicate both arms read is what keeps the
			// admission and the exclusion in step whenever the set gains a member.
			//
			// hostFatalClass is excluded for a DIFFERENT reason and it is worth stating separately:
			// that class withdraws its test from both command lines, so it produces no verdict on
			// either side and there is no status pair here for it to absorb. If one ever reaches
			// this arm the exclusion did not take -- the test RAN -- and reading it as disclosed
			// would hide exactly that. It must fall through to a mismatch and be seen.
			if disclosure, ok := disclosures[name]; ok &&
				!classAdmitsSkipShape(disclosure.Class) && disclosure.Class != hostFatalClass &&
				goStatus == "pass" && csStatus == "fail" {
				if strings.Contains(csOutputs[name], disclosure.Signature) {
					disclosed = append(disclosed, name)
					disclosedNames.Add(name)
					continue
				}
				mismatches = append(mismatches, fmt.Sprintf("%s: Go=%q C#=%q (failure does not match the disclosed %s signature %q)",
					name, goStatus, csStatus, disclosure.Class, disclosure.Signature))
				mismatchNames.Add(name)
				continue
			}

			if goStatus == "pass" && csStatus == "fail" && strings.TrimSpace(csOutputs[name]) == "" {
				prefix := name + "/"
				hasDisclosedDescendant := false
				hasMismatchedDescendant := false

				for descendant := range disclosedNames {
					if strings.HasPrefix(descendant, prefix) {
						hasDisclosedDescendant = true
						break
					}
				}

				for descendant := range mismatchNames {
					if strings.HasPrefix(descendant, prefix) {
						hasMismatchedDescendant = true
						break
					}
				}

				if hasDisclosedDescendant && !hasMismatchedDescendant {
					disclosed = append(disclosed, name)
					disclosedNames.Add(name)
					continue
				}
			}

			mismatches = append(mismatches, fmt.Sprintf("%s: Go=%q C#=%q", name, goStatus, csStatus))
			mismatchNames.Add(name)
			continue
		}

		if goStatus == "skip" {
			skipped = append(skipped, name)
		}
	}

	sort.Strings(withdrawn)

	return mismatches, skipped, disclosed, withdrawn
}

// underDisclosureRoot reports whether name is a strict descendant of a signature-matched
// disclosure root (see the withdrawal rule in matchTerminalStatuses).
func underDisclosureRoot(name string, roots HashSet[string]) bool {
	for {
		idx := strings.LastIndex(name, "/")

		if idx < 0 {
			return false
		}

		name = name[:idx]

		if roots.Contains(name) {
			return true
		}
	}
}

// flavorExcludedTestDeclarations returns manifest declarations for the Test/Benchmark/Fuzz/Example
// functions in `_test.go` files that the CONVERSION's build tags exclude but a plain `go test` on
// the same platform includes — the flavor gap the F6 census gate below would otherwise report as
// undeclared tests. The corpus reproduces Go's pure-Go flavor (`-tags purego`, the -stdlib/-tests
// default), while the differential baseline runs the platform's NATIVE flavor, so a test file gated
// `!purego && (amd64 || …)` (crypto/internal/nistec's p256_asm_table_test.go and
// p256_ordinv_test.go, the first measured instances) is real on exactly one side. Declaring its
// tests disclosed-unsupported keeps the census honest — every name `go test` runs is accounted for,
// filtered from BOTH sides of the oracle like any excluded declaration — and the proof page names
// the flavor each one needs. Files are parsed STANDALONE (no type-check): they are outside the
// loaded package by construction, and the census needs names, kinds and positions, nothing more, so
// the *testing.T/B/F parameter shape is matched on the AST. A file excluded under BOTH tag sets
// (another OS/arch) contributes nothing — `go test` never runs it either.
func flavorExcludedTestDeclarations(inputPath string, options Options, declared []testDeclaration) []testDeclaration {
	// The gap can only exist when the conversion applied tags a bare `go test` does not.
	if len(options.buildTags) == 0 {
		return nil
	}

	paths, err := filepath.Glob(filepath.Join(inputPath, "*_test.go"))
	if err != nil {
		return nil
	}

	seen := HashSet[string]{}
	for _, decl := range declared {
		seen.Add(decl.Name)
	}

	result := make([]testDeclaration, 0)

	for _, filePath := range paths {
		converted, err := CheckBuildConstraints(filePath, options.targetPlatform, options.buildTags, inputPath)
		if err != nil || converted {
			continue // in the conversion's own set (or unreadable, which the loader reports)
		}

		native, err := CheckBuildConstraints(filePath, options.targetPlatform, nil, inputPath)
		if err != nil || !native {
			continue // excluded on both sides — go test never runs it either
		}

		fset := token.NewFileSet()
		file, err := parser.ParseFile(fset, filePath, nil, parser.SkipObjectResolution)
		if err != nil || file.Name == nil {
			continue
		}

		relPath, _ := filepath.Rel(inputPath, filePath)
		reason := fmt.Sprintf("requires the native implementation flavor: %s is excluded by the conversion's build tags (%s), and the corpus reproduces the pure-Go flavor",
			filepath.Base(filePath), strings.Join(options.buildTags, ","))

		for _, decl := range file.Decls {
			fn, ok := decl.(*ast.FuncDecl)
			if !ok || fn.Recv != nil || fn.Name == nil || fn.Name.Name == "TestMain" || seen.Contains(fn.Name.Name) {
				continue
			}

			if fn.Type.Results.NumFields() != 0 {
				continue
			}

			name := fn.Name.Name
			kind := ""

			switch {
			case isGoTestName(name, "Example") && fn.Type.Params.NumFields() == 0:
				kind = "example"
			case isGoTestName(name, "Test") && hasSingleTestingParam(fn, "T"):
				kind = "test"
			case isGoTestName(name, "Benchmark") && hasSingleTestingParam(fn, "B"):
				kind = "benchmark"
			case isGoTestName(name, "Fuzz") && hasSingleTestingParam(fn, "F"):
				kind = "fuzz"
			}

			if kind == "" {
				continue
			}

			seen.Add(name)
			result = append(result, testDeclaration{
				Name:        name,
				PackageName: file.Name.Name,
				Source:      filepath.ToSlash(relPath),
				Line:        fset.Position(fn.Pos()).Line,
				Kind:        kind,
				Status:      "unsupported",
				Reason:      reason,
			})
		}
	}

	return result
}

// hasSingleTestingParam reports whether fn takes exactly one parameter spelled `*testing.<typeName>`
// — the AST-textual form of discoverTestDeclarations' typed check, for files parsed standalone.
func hasSingleTestingParam(fn *ast.FuncDecl, typeName string) bool {
	params := fn.Type.Params

	if params.NumFields() != 1 || len(params.List[0].Names) > 1 {
		return false
	}

	star, ok := params.List[0].Type.(*ast.StarExpr)
	if !ok {
		return false
	}

	sel, ok := star.X.(*ast.SelectorExpr)
	if !ok || sel.Sel == nil || sel.Sel.Name != typeName {
		return false
	}

	ident, ok := sel.X.(*ast.Ident)
	return ok && ident.Name == "testing"
}

// manifestCensusGaps returns the top-level test names present in the RAW `go test -json` results
// that the manifest cannot account for (F6 census gate). Discovery and comparison otherwise share
// a single point of failure: eligibleTerminalTestResults filters BOTH sides by the manifest, so a
// discovery bug self-censors — a test the converter never discovered is silently removed from the
// comparison and the package can be declared "validated" without it. The census runs against the
// UNFILTERED Go results: every name go test actually ran must be declared in the manifest under
// SOME status (included, capability-blocked, or disclosed-unsupported — examples and fuzz
// seed-corpus runs land here too); subtest names roll up to their top-level parent. Any gap fails
// the comparison — a package cannot validate past a test the manifest never accounted for.
func manifestCensusGaps(goResults map[string]string, manifest testManifest) []string {
	declared := HashSet[string]{}
	for _, test := range manifest.Tests {
		declared.Add(test.Name)
	}
	if manifest.TestMain != nil {
		declared.Add(manifest.TestMain.Name)
	}

	gaps := HashSet[string]{}
	for name := range goResults {
		topLevelName, _, _ := strings.Cut(name, "/")
		if !declared.Contains(topLevelName) {
			gaps.Add(topLevelName)
		}
	}

	result := gaps.Keys()
	sort.Strings(result)
	return result
}

// excludedDeclarations lists every disclosed-unsupported declaration the comparison excludes
// (F2/F3): benchmarks, fuzz targets, Examples, and capability-blocked tests are filtered from
// BOTH sides of the oracle, so the comparison record must say what was excluded and why —
// silent filtering is the exact silent-pass channel req §2.7 forbids.
func excludedDeclarations(manifest testManifest) []string {
	excluded := make([]string, 0)

	for _, test := range manifest.Tests {
		if test.Status != "included" {
			excluded = append(excluded, fmt.Sprintf("%s (%s): %s", test.Name, test.Kind, test.Reason))
		}
	}

	if manifest.TestMain != nil && manifest.TestMain.Status != "included" {
		excluded = append(excluded, fmt.Sprintf("%s (%s): %s", manifest.TestMain.Name, manifest.TestMain.Kind, manifest.TestMain.Reason))
	}

	return excluded
}

// testChildTimeoutGrace is how much longer a test child PROCESS is allowed to live than the package
// deadline it was given. The deadline is enforced IN-process by `go test` and by the converted host,
// both of which write their results on expiry; the outer kill is only a safety net for a child that
// ignores it, so it must fire strictly later or it destroys the very results the deadline produced.
const testChildTimeoutGrace = time.Minute

// testChildTimeout is the outer kill for a test child process — the package deadline plus the grace
// margin above.
func testChildTimeout(options Options) time.Duration {
	return options.testTimeout + testChildTimeoutGrace
}

// filterMatchedNothing reports the error a `-test-filter` run owes when the filter matched NOTHING,
// and whether it fired at all. Extracted from compareGoAndConvertedTests so the decision is unit
// testable on its own terms rather than only through a full differential run.
//
// The rule is narrow BY CONSTRUCTION, and each conjunct earns its place:
//
//   - testFilter != "" — an UNFILTERED package with no verdicts is a different (and already
//     handled) situation; this guard is about the filter specifically.
//   - matched && status == "validated" — fire only on a run that would OTHERWISE have reported a
//     validated pass. That keeps it off `not-applicable` packages, whose zero verdicts are a
//     property of the package rather than of the filter, and stops it from overwriting a real
//     failure that has already cleared Matched.
//   - goCount == 0 && csCount == 0 — BOTH sides empty. A filter that matched on one side only
//     already surfaces as one-sided rows, i.e. mismatches, and stays fatal through the existing
//     path; demanding both empty keeps this guard from claiming those.
//
// excludedCount does not gate the decision, only the MESSAGE. A filter can legitimately match
// nothing because its target is a deliberately EXCLUDED declaration, which at exit-code level is
// indistinguishable from a filter that silently matched nothing. Both measured nothing, so both
// stay non-zero exits — the operator is told which case they are in instead of guessing.
func filterMatchedNothing(testFilter, status string, matched bool, goCount, csCount, excludedCount int) (string, bool) {
	if testFilter == "" || !matched || status != "validated" || goCount != 0 || csCount != 0 {
		return "", false
	}

	excludedNote := "the package excludes no declarations, so the regex itself matched nothing"
	if excludedCount > 0 {
		excludedNote = fmt.Sprintf("the package excludes %d declaration(s) — the filter may be naming one of them", excludedCount)
	}

	return fmt.Sprintf(
		"-test-filter %q matched NO tests on either side: zero verdicts were compared, so this run "+
			"measured nothing and must not read as a pass (%s)", testFilter, excludedNote), true
}

func compareGoAndConvertedTests(inputPath, outputPath, testProject string, options Options) error {
	// -test-timeout is the PACKAGE deadline, handed to BOTH sides so they agree: `go test -timeout`
	// and the converted host's own `--timeout`. Without it each side silently used its OWN 10-minute
	// default — `go test`'s and TestHost's — so no value of the flag could let a slow suite finish:
	// hash/maphash's C# run self-terminated at exactly 600 s under `-test-timeout 40m`, reporting its
	// still-running TestSmhasherAvalanche as an empty verdict that reads exactly like a real failure
	// (the C# suite needs ~15 min where Go's needs 7.6 s — a performance gap, not a correctness one).
	// -test-filter is the block-gated census mechanism (coordinator ruling, 2026-08-29): ONE regex
	// handed VERBATIM to both sides so the two runs filter identically. It is deliberately NOT
	// composed here - the two command lines carry the SAME string, verifiable by eye in the log,
	// rather than two expressions someone would have to prove equivalent. A gated census is
	// DIAGNOSTIC ONLY: the row banks from an ungated run, never from this one.
	// The disclosure manifest is read BEFORE either child runs, because a host-fatal entry has to
	// withdraw its test from BOTH command lines rather than be reclassified after the fact. Every
	// other class labels a verdict a run produced; this one decides what the run contains.
	disclosures, disclosureNotes, disclosureErr := loadTestDisclosures(outputPath)
	if violations := hostFatalMintViolations(outputPath, disclosures); len(violations) > 0 {
		// Refused BEFORE either child runs, so a bad entry cannot quietly withdraw a row that some
		// platform passes. Fatal rather than a warning: the whole point of the class is that it
		// changes what runs, and a warning would let the withdrawal happen anyway.
		return fmt.Errorf("host-fatal disclosure refused at mint:\n  %s", strings.Join(violations, "\n  "))
	}
	hostFatalSkip := hostFatalSkipExpression(disclosures)

	goArgs := []string{"test", "-json", "-count=1", "-timeout", options.testTimeout.String()}
	if options.testFilter != "" {
		goArgs = append(goArgs, "-run", options.testFilter)
	}
	// Handed VERBATIM to both sides, exactly as -test-filter is: the SAME string on the two command
	// lines, verifiable by eye in the log, rather than two expressions someone would have to prove
	// equivalent. Go's -skip and the host's --skip compile it identically (same `/` split, same
	// per-segment regexes), which is what keeps the two runs answering the same question.
	if hostFatalSkip != "" {
		goArgs = append(goArgs, "-skip", hostFatalSkip)
	}
	goArgs = append(goArgs, ".")
	goOutput, goErr := runCommandWithTimeout(testChildTimeout(options), inputPath, options, "go", goArgs...)

	// Captured immediately after the real oracle run, same directory, same options — see
	// oracleGoVersion's own doc comment for why this is not `go env GOROOT`.
	oracleVersion := oracleGoVersion(inputPath, options)

	// The driver's terminal context, observed beside the oracle run it applies to — see
	// testEnvironmentRecord.Terminal for why a count without it cannot be reproduced.
	terminal := driverTerminal()

	// The converted side runs the PUBLISHED single-file host (the host-limit retirement) — the
	// same relocatable artifact an os/exec-style test copies and re-execs, so what the comparison
	// measures is the deployment shape the verdicts claim.
	if err := publishTestHost(outputPath, testProject, options); err != nil {
		return err
	}

	csArgs := []string{"--json", "-timeout", options.testTimeout.String()}
	if options.testFilter != "" {
		csArgs = append(csArgs, "--run", options.testFilter)
	}
	if hostFatalSkip != "" {
		csArgs = append(csArgs, "--skip", hostFatalSkip)
	}
	csArgs = append(csArgs,
		"--result", filepath.Join(outputPath, "go2cs_test_results.json"), "--junit", filepath.Join(outputPath, "go2cs_test_results.xml"))
	csOutput, csErr := runCommandWithTimeoutEnv(testChildTimeout(options), outputPath, options, testHostRunEnv(options), publishedTestHostPath(outputPath, testProject), csArgs...)

	goResults := terminalTestResults(goOutput)
	csResults := terminalTestResults(csOutput)
	csOutputs := terminalTestOutputs(csOutput)
	var manifest testManifest
	var censusGaps []string
	var gated []capabilityGatedDeclaration
	manifestData, manifestErr := os.ReadFile(filepath.Join(outputPath, testManifestFileName))
	if manifestErr == nil {
		manifestErr = json.Unmarshal(manifestData, &manifest)
		if manifestErr == nil {
			// F6 census gate: computed over the RAW Go results BEFORE the manifest-driven
			// filtering below — the filter shares the manifest with discovery, so only the
			// unfiltered stream can expose a declaration discovery missed.
			censusGaps = manifestCensusGaps(goResults, manifest)
			// Same window, same reason: the rows a capability gate withdraws exist only in the
			// unfiltered stream, and the proof page publishes them so the matched count below
			// never absorbs a subtest silently.
			gated = capabilityGatedDeclarations(goResults, manifest)
			goResults = eligibleTerminalTestResults(goResults, manifest)
			csResults = eligibleTerminalTestResults(csResults, manifest)
		}
	}
	pairAddressVariantNames(goResults, csResults, csOutputs)

	names := make([]string, 0, len(goResults)+len(csResults))
	seen := HashSet[string]{}
	for name := range goResults {
		if seen.Add(name) {
			names = append(names, name)
		}
	}
	for name := range csResults {
		if seen.Add(name) {
			names = append(names, name)
		}
	}
	sort.Strings(names)

	status := "validated"
	if !manifestHasEligibleTests(manifest) {
		status = "not-applicable"
	}
	environment := testEnvironmentFromOptions(options)
	environment.OracleGoVersion = oracleVersion
	environment.Terminal = terminal
	result := testComparison{
		Package: filepath.Base(inputPath), Status: status, Go: goResults, CSharp: csResults,
		Matched: true, Skipped: []string{}, Disclosed: []string{}, Excluded: excludedDeclarations(manifest), Errors: []string{},
		Gated: gated, Withdrawn: []string{}, Environment: environment,
	}
	if disclosureErr != nil {
		result.Matched = false
		result.Errors = append(result.Errors, "test disclosures: "+disclosureErr.Error())
		disclosures = nil
	}
	if manifestErr != nil {
		result.Matched = false
		result.Status = "conversion-blocked"
		result.Errors = append(result.Errors, "test manifest: "+manifestErr.Error())
	} else if blocked := manifestCapabilityBlock(manifest); len(blocked) > 0 {
		result.Matched = false
		result.Status = "infrastructure-blocked"
		result.Errors = append(result.Errors, "unsupported testing capabilities: "+strings.Join(blocked, ", "))
	}

	if len(censusGaps) > 0 {
		// F6: go test ran declarations the manifest never accounted for — a DISCOVERY defect,
		// not a test failure. The package must not validate past it.
		result.Matched = false
		result.Errors = append(result.Errors, "census: go test reported tests the manifest does not declare: "+strings.Join(censusGaps, ", "))
	}

	mismatches, skipped, disclosed, withdrawn := matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)
	if len(mismatches) > 0 {
		result.Matched = false
		result.Errors = append(result.Errors, mismatches...)
	}
	result.Skipped = append(result.Skipped, skipped...)
	for _, name := range disclosed {
		disclosure := disclosures[name]
		result.Disclosed = append(result.Disclosed, fmt.Sprintf("%s (%s): %s", name, disclosure.Class, disclosure.Reason))
	}

	// Withdrawn rows leave the comparison record the way capability-gated rows do: removed from
	// the Go map (they are Go-only by construction, so nothing exists on the C# side) and
	// PUBLISHED in their own field, so the matched count below and every proof-page derivation
	// self-correct while the omission stays visible — silent absorption is the channel §2.7
	// forbids.
	for _, name := range withdrawn {
		delete(goResults, name)
	}
	result.Withdrawn = append(result.Withdrawn, withdrawn...)

	// Whether at least one failure is AGREED — both runtimes reporting "fail" for the same row.
	// An agreed failure is a matched verdict, and it is the one legitimate reason a side's exit
	// code goes nonzero without any divergence existing.
	agreedFailure := false
	for name, goStatus := range goResults {
		if goStatus == "fail" && csResults[name] == "fail" {
			agreedFailure = true
			break
		}
	}

	if goErr != nil && csErr != nil && agreedFailure && len(mismatches) == 0 && len(goResults) > 0 && len(csResults) > 0 {
		// The MIRROR of the C# forgiveness below: go test's nonzero exit is the agreed outcome
		// of failures BOTH runtimes report identically, so the exit codes carry no information
		// the per-test rows have not already matched. First consumer: crypto/tls, whose test
		// fixtures expired 2025-01-01 and fail four resumption/verification tests with the same
		// `x509: certificate has expired` text on either runtime — the most that suite can
		// score in either language, worsening with the calendar. Narrow on the same terms as
		// the arm below: zero mismatches (a truncated or divergent run stays fatal), both runs
		// produced results, at least one agree-fail row exists to attribute the exits to, and
		// BOTH sides exited nonzero — a red Go baseline beside a green converted run is a
		// divergence, never a forgiveness.
		goErr = nil
	}

	if csErr != nil && goErr == nil && (len(disclosed) > 0 || agreedFailure) && len(mismatches) == 0 && len(csResults) > 0 {
		// The converted host exits nonzero BECAUSE the disclosed-divergent tests fail — that
		// exit code is part of the disclosed outcome, not an additional failure signal.
		// Forgiveness is deliberately narrow: go test itself was clean (or its own exit was
		// just forgiven on agreed failures, which carry a C# exit exactly as disclosed rows
		// do), the host produced results, and every divergence matched its pinned signature
		// (zero mismatches — a truncated run surfaces as one-sided rows, which are mismatches,
		// and stays fatal).
		csErr = nil
	}

	if goErr != nil {
		result.Matched = false
		result.Status = "failing"
		result.Errors = append(result.Errors, "go test: "+goErr.Error())
	}
	if csErr != nil {
		result.Matched = false
		if result.Status != "infrastructure-blocked" {
			// Parsed events prove the converted host RAN: a nonzero exit with results is a
			// genuine test failure (`failing`). `conversion-blocked` is reserved for actual
			// conversion/build/run infrastructure causes — the host produced no events at all.
			if len(csResults) > 0 {
				result.Status = "failing"
			} else {
				result.Status = "conversion-blocked"
			}
		}
		result.Errors = append(result.Errors, "converted tests: "+csErr.Error())
	}

	// ZERO-MATCH GUARD. A `-test-filter` run that matches NOTHING produced no verdicts at all, and
	// without this it reported SUCCESS: neither side ran a test, so both exit 0, every count is
	// zero, no mismatch exists to fail on, and the run reads exactly like a clean validation. That
	// is the census equivalent of a vacuous proof — a gated census exists to MEASURE something, and
	// a filter typo, a renamed test, or a regex whose anchoring is subtly wrong all land here
	// silently. The failure mode is the same shape as the false-green routes catalogued in
	// CLAUDE.md: not a wrong answer, an answer about nothing that is indistinguishable from a right
	// one.
	//
	// Deliberately narrow. It fires only on a run that would OTHERWISE have reported a validated
	// pass, so it cannot touch a package with no eligible tests (`not-applicable`, whose zero
	// verdicts are a property of the package and not of the filter), and it cannot mask a real
	// failure that has already cleared Matched. It also requires BOTH sides empty: a filter that
	// matched on one side only already surfaces as one-sided rows — i.e. mismatches — and stays
	// fatal through the existing path.
	//
	// No new Status string. Clearing Matched routes through the existing `validated` -> `failing`
	// mapping immediately below, so every downstream consumer (proof-page gating, roster tooling,
	// the sweep script's row parser) keeps reading the vocabulary it already knows; the MESSAGE
	// carries the meaning rather than a new token nothing has been taught.
	//
	// The excluded-declaration note answers a distinction R raised while this was being cut: a
	// filter can legitimately match nothing because its target is a DELIBERATELY EXCLUDED
	// declaration, which at exit-code level is indistinguishable from a filter that silently matched
	// nothing. Both still measured nothing, so both are still a non-zero exit — the guard does not
	// exempt the excluded case, it TELLS the operator which case they are in instead of leaving them
	// to guess.
	if violation, fired := filterMatchedNothing(
		options.testFilter, result.Status, result.Matched,
		len(goResults), len(csResults), len(result.Excluded)); fired {
		result.Matched = false
		result.Errors = append(result.Errors, violation)
	}

	// The host-fatal names appear in NEITHER side's results, by construction -- they were withdrawn
	// from both command lines. Without adding them here the counts would silently shrink, which is
	// the hidden-test outcome the ruling forbids: runtime/debug banks 4 + 6, never 4 + 5 with the
	// test quietly gone. Appended after the comparison so they cannot influence matching.
	result.Disclosed = append(result.Disclosed, hostFatalNames(disclosures)...)
	sort.Strings(result.Disclosed)

	if !result.Matched && result.Status == "validated" {
		result.Status = "failing"
	}
	if err := writeComparisonRecord(outputPath, &result, options.testFilter); err != nil {
		return err
	}
	if !result.Matched {
		return fmt.Errorf("Go/C# test comparison failed: %s", strings.Join(result.Errors, "; "))
	}
	// The differential that just proved the package is the proof: publish it as a committed page
	// under docs/validation (no-op outside a repository checkout). See validationProofPages.go.
	if publishesRosterArtifacts(result.Status, options.testFilter) {
		if err := emitValidationProofPage(outputPath, result, manifest, disclosures, disclosureNotes, options); err != nil {
			return fmt.Errorf("write validation proof page: %w", err)
		}

		// The page the package's README badge reads has just changed, and the README was composed
		// BEFORE it — so level it here, in the same run, rather than one conversion later. Reaches a
		// README only on a run that CONVERTED this package: `compare` alone left no record, so it
		// cannot write one (see packageReadmeEmission).
		if err := refreshPackageReadmeAfterProof(outputPath, options); err != nil {
			return fmt.Errorf("refresh package README: %w", err)
		}
	} else if result.Status == "validated" {
		// Suppressed, and said OUT LOUD: a silent skip on exactly the run that must not write these
		// would read as "the artifacts were already current".
		fmt.Fprintf(os.Stderr, "WARNING: -test-filter %q was active, so NO validation artifacts were published for %s "+
			"(proof page, docs/validation index row, README Tests badge). A gated census is DIAGNOSTIC ONLY; "+
			"re-run WITHOUT -test-filter to bank a row.\n", options.testFilter, manifest.PackageImportPath)
	}
	if len(disclosed) > 0 {
		classes := HashSet[string]{}
		for _, name := range disclosed {
			classes.Add(disclosures[name].Class)
		}
		classList := classes.Keys()
		sort.Strings(classList)
		fmt.Printf("Validated %d tests against go test (%d skipped identically on both sides, %d disclosed-divergent (%s), %d disclosed-unsupported declarations excluded).\n",
			len(goResults)-len(disclosed), len(result.Skipped), len(disclosed), strings.Join(classList, ", "), len(result.Excluded))
	} else {
		fmt.Printf("Validated %d tests against go test (%d skipped identically on both sides, %d disclosed-unsupported declarations excluded).\n",
			len(goResults), len(result.Skipped), len(result.Excluded))
	}
	return nil
}

func terminalTestResults(output string) map[string]string {
	result := make(map[string]string)
	for _, line := range strings.Split(output, "\n") {
		var event normalizedTestEvent
		if json.Unmarshal([]byte(line), &event) != nil || event.Test == "" {
			continue
		}
		switch event.Action {
		case "pass", "fail", "skip", "timeout", "infrastructure-error":
			result[event.Test] = event.Action
		}
	}
	return result
}

// terminalTestOutputs captures each test's accumulated log output from its terminal event —
// the converted host attaches the joined t.Log/t.Error text to the terminal record — keyed by
// test name, for disclosure signature matching against the C# side's failure messages.
func terminalTestOutputs(output string) map[string]string {
	result := make(map[string]string)
	for _, line := range strings.Split(output, "\n") {
		var event normalizedTestEvent
		if json.Unmarshal([]byte(line), &event) != nil || event.Test == "" {
			continue
		}
		switch event.Action {
		case "pass", "fail", "skip", "timeout", "infrastructure-error":
			result[event.Test] = event.Output
		}
	}
	return result
}

// capabilityGatedDeclarations enumerates, per capability-gated declaration, the verdict rows the
// UNFILTERED `go test` run produced for it — the declaration itself plus every subtest underneath.
// It must be called before eligibleTerminalTestResults, which is precisely what removes those rows;
// after the filter the information no longer exists anywhere.
//
// Ordering is fully determined here — declarations and rows are both sorted — so the proof page this
// feeds is byte-stable for a given row SET. The set itself comes from a live `go test`, which is the
// one thing this cannot pin: a gated test whose subtests vary run to run would churn its page on
// every sweep, where a non-gated one would fail the verdict count instead. No such test is gated
// today (both os/exec candidates are fixed tables), and the first that is should be checked for it.
func capabilityGatedDeclarations(goResults map[string]string, manifest testManifest) []capabilityGatedDeclaration {
	gated := make(map[string]string)

	for _, test := range manifest.Tests {
		if test.Status == "unsupported" && strings.HasPrefix(test.Reason, unsupportedCapabilityReasonPrefix) {
			gated[test.Name] = strings.TrimPrefix(test.Reason, unsupportedCapabilityReasonPrefix)
		}
	}

	if len(gated) == 0 {
		return nil
	}

	rows := make(map[string][]string)

	for name := range goResults {
		topLevelName, _, _ := strings.Cut(name, "/")

		if _, blocked := gated[topLevelName]; blocked {
			rows[topLevelName] = append(rows[topLevelName], name)
		}
	}

	names := make([]string, 0, len(gated))

	for name := range gated {
		names = append(names, name)
	}

	sort.Strings(names)

	declarations := make([]capabilityGatedDeclaration, 0, len(names))

	for _, name := range names {
		declarationRows := rows[name]
		sort.Strings(declarationRows)
		declarations = append(declarations, capabilityGatedDeclaration{
			Name: name, Capabilities: gated[name], Rows: declarationRows,
		})
	}

	return declarations
}

func eligibleTerminalTestResults(results map[string]string, manifest testManifest) map[string]string {
	eligible := HashSet[string]{}
	for _, test := range manifest.Tests {
		if test.Kind == "test" && test.Status == "included" {
			eligible.Add(test.Name)
		}
	}

	filtered := make(map[string]string)
	for name, status := range results {
		topLevelName, _, _ := strings.Cut(name, "/")
		if eligible.Contains(topLevelName) {
			filtered[name] = status
		}
	}
	return filtered
}

func runCommandWithTimeout(timeout time.Duration, workingDir string, options Options, name string, args ...string) (string, error) {
	return runCommandWithTimeoutEnv(timeout, workingDir, options, nil, name, args...)
}

// runCommandWithTimeoutEnv is runCommandWithTimeout plus caller-supplied extra environment entries,
// appended after the standard child environment so they win on any key collision (os/exec takes the
// LAST value for a duplicate key). Introduced for options.testReleaseTC0's DOTNET_TieredCompilation=0
// — a property of ONE specific child invocation (the converted host's own run), never the whole
// pipeline's environment, so it does not belong in childEnvWithGo2CSPath's shared construction.
func runCommandWithTimeoutEnv(timeout time.Duration, workingDir string, options Options, extraEnv []string, name string, args ...string) (string, error) {
	ctx, cancel := context.WithTimeout(context.Background(), timeout)
	defer cancel()
	cmd := exec.CommandContext(ctx, name, args...)
	cmd.Dir = workingDir
	target := strings.Split(options.targetPlatform, "/")
	cmd.Env = childEnvWithGo2CSPath(os.Environ(), options.go2csPath)
	cmd.Env = append(cmd.Env, extraEnv...)
	if len(target) == 2 {
		cmd.Env = append(cmd.Env, "GOOS="+target[0], "GOARCH="+target[1])
	}
	if len(options.goRoot) > 0 {
		// Hand both sides the same GOROOT explicitly. `go test` resolves it on its own, but the
		// converted C# host has no linker-baked defaultGOROOT to fall back on — runtime.GOROOT()
		// reads the environment — so testenv.GOROOT consumers agree with Go only when the pipeline
		// exports the root it converted from. Duplicate keys are fine: os/exec takes the last value.
		cmd.Env = append(cmd.Env, "GOROOT="+options.goRoot)

		// `go test` PREPENDS $GOROOT/bin to the test binary's PATH, so a test that shells out to
		// `go` gets the toolchain matching the GOROOT it was built against. Measured against Go
		// 1.23.1: inside a test, PATH[0] is $GOROOT/bin and exec.LookPath("go") resolves there.
		// Without the same treatment the converted host resolves `go` from the ambient PATH, which
		// on a machine with more than one installation is a DIFFERENT go of the same version —
		// internal/testenv's TestGoToolLocation compares ../../../bin/go against
		// exec.LookPath("go") with os.SameFile and fails on exactly that difference, and
		// internal/godebugs shells out to `go list std cmd`. Reproducing go test's environment is
		// the harness's job, and PATH is part of that environment just as GOROOT and the working
		// directory are.
		cmd.Env = append(cmd.Env, "PATH="+filepath.Join(options.goRoot, "bin")+string(os.PathListSeparator)+os.Getenv("PATH"))
	}

	// TZ is part of go test's environment for the same reason GOROOT and PATH above are, and it
	// has to be set HERE rather than inside the host because the converted snapshot is taken
	// before any host code runs: runtime.envs is filled by a [ModuleInitializer]
	// (runtime/goenvs_impl.cs), which .NET runs at assembly load, and syscall.envs is a static
	// field initializer over that. TestHost.Run's own Environment.SetEnvironmentVariable pin
	// therefore reaches the CLR (TimeZoneInfo.Local) but NEVER syscall.Getenv on unix, so a
	// converted time-sensitive test ran under the HOST's zone while its Windows counterpart ran
	// under the pin -- measured 2026-09-02 on a fleet box sitting at America/Chicago, five hours
	// off. Making the snapshot live instead was refused deliberately: Go documents it as "set at
	// process start" (GOROOT's own doc) and setenv_c mirrors into the C environment only under
	// cgo, so a later os.Setenv does not and should not appear there. Setting it at LAUNCH keeps
	// that semantic exactly and needs no corpus or golib change.
	//
	// It goes in the SHARED path, not in testHostRunEnv: `go test` (the oracle) is launched
	// through runCommandWithTimeout with no extra environment, so a host-only pin would leave
	// the two sides of one comparison in DIFFERENT zones -- trading a cross-platform divergence
	// for a cross-side one. A test that changes TZ within its own process is unaffected; only
	// the starting value is pinned, which is what TestHost.Run was already trying to do.
	cmd.Env = append(cmd.Env, "TZ=UTC")

	// The child is spawned into its OWN killable unit — a process group on unix, a job object on
	// Windows — so the deadline kill below can name that unit instead of the single process. What
	// it fixes: the converted host RE-EXECS ITSELF for the subprocess-style rows Go's suites carry,
	// and when a test goroutine is blocked before its deferred cmd.Process.Kill() the package
	// deadline kills the host from outside and the re-exec'd child is ORPHANED — measured at six
	// alive at once on one row. The child answers signals normally; there is simply nobody left to
	// send one, which is why the remedy is at the launcher. Both mechanisms and their residuals are
	// documented in processGroupUnix.go / processGroupWindows.go.
	//
	// Why it goes HERE, in the ONE shared helper, rather than at the converted host's two call
	// sites: `go test` orphans its own test binary under an external kill exactly as the host
	// orphans its re-exec, so the defect belongs to the launcher on BOTH sides of a comparison —
	// and treating one side only would trade an orphan for a cross-SIDE difference in how the two
	// runs are torn down, the worse trade. Same reasoning that put the TZ pin in this shared path
	// instead of in the host's own environment.
	group, groupErr := newProcessGroup(cmd)
	defer group.close()
	if groupErr != nil {
		warnProcessGroupDegradedOnce(groupErr)
	}

	// cmd.Cancel is what os/exec runs when ctx expires; CommandContext defaults it to
	// Process.Kill — the host ALONE, which is the defect. The group kill replaces that default and
	// keeps the single-process kill behind it as its own fallback, so the safety net is never
	// removed, only preceded.
	cmd.Cancel = func() error { return group.kill(cmd) }

	// Start/attach/Wait rather than CombinedOutput: the Windows half needs a hook BETWEEN the spawn
	// and the wait to put the started process in the job, and CombinedOutput offers none. The bytes
	// captured and the error returned are the ones CombinedOutput would have produced — one buffer
	// shared by stdout and stderr, and a Start failure surfaced with empty output.
	var combined bytes.Buffer
	cmd.Stdout = &combined
	cmd.Stderr = &combined

	err := cmd.Start()
	if err == nil {
		if attachErr := group.attach(cmd); attachErr != nil {
			warnProcessGroupDegradedOnce(attachErr)
		}
		err = cmd.Wait()
	}
	output := combined.Bytes()

	if ctx.Err() == context.DeadlineExceeded {
		return string(output), fmt.Errorf("%s timed out after %s", name, timeout)
	}
	if err != nil {
		return string(output), fmt.Errorf("%s %s failed: %w\n%s", name, strings.Join(args, " "), err, strings.TrimSpace(string(output)))
	}
	return string(output), nil
}

// processGroupWarnOnce keeps the degrade notice below to one line per run.
var processGroupWarnOnce sync.Once

// warnProcessGroupDegradedOnce says, exactly once per run, that a pipeline child could not be
// placed in its own killable unit — so a deadline kill for that child is the single-process form
// and a descendant it spawns can be orphaned, which is precisely the condition the group exists to
// remove. ONCE, because the cause is a property of the HOST (a job object refused, a kernel that
// would not place the child) rather than of any one child, so repeating it per spawn would bury the
// pipeline's own output under a fact that does not change. Advisory rather than fatal: an orphan is
// a hygiene defect, and refusing to run the pipeline over one would be worse than the defect.
func warnProcessGroupDegradedOnce(err error) {
	processGroupWarnOnce.Do(func() {
		fmt.Fprintf(os.Stderr, "WARNING: test child could not be placed in its own process group (%v); "+
			"a deadline kill will reach the child but not its descendants\n", err)
	})
}

// childEnvWithGo2CSPath builds a pipeline child's environment carrying EXACTLY ONE spelling of the
// go2csPath variable: every case-insensitive variant inherited from the parent is dropped, then the
// canonical lowercase entry the emitted .csproj files reference is appended with the resolved root.
//
// Why the scrub rather than a plain append. MSBuild materializes environment variables as properties
// and resolves property names CASE-INSENSITIVELY, while a POSIX environment block is case-SENSITIVE.
// So `GO2CSPATH=/root/go2cs` and `go2csPath=/root/go2cs/src/` are two distinct entries to the OS and
// ONE property to MSBuild, and which value wins is decided by enumeration order inside the .NET
// env-table plumbing — a per-process coin flip. Losing the draw concatenates `$(go2csPath)gen/...`
// into `/root/go2csgen/...`; the analyzer and every stdlib ProjectReference dangle, and the build dies
// in a CS0246 storm on every golib type. That is the intermittent, package-shuffling Linux pipeline
// failure root-caused on 2026-08-21 (binlog-proven: "Property 'go2csPath' with value '/root/go2cs'
// expanded from the environment"). Windows never saw it in five weeks of sweeps because its
// environment block is case-insensitive at the OS level — the two names are ONE slot there, so the
// appended value always won.
//
// The converter no longer manufactures the colliding entry itself (see resolveGo2CSPathDefault), but
// a USER may still set GO2CSPATH — the documented way to choose the runtime root, and what the Linux
// harness pin does. That value is honored as the -go2cspath DEFAULT and nothing more: by the time a
// child runs, options.go2csPath is the ONE resolved answer, so any ambient spelling is a second
// opinion the child must not receive. Scrubbing here is what makes the guarantee hold regardless of
// the invoking shell — including the nastier variant where an ambient GO2CSPATH names a DIFFERENT
// real tree and the child would otherwise bind the wrong stdlib nondeterministically.
func childEnvWithGo2CSPath(parentEnv []string, go2csPath string) []string {
	const go2csPathVar = "go2csPath"

	env := make([]string, 0, len(parentEnv)+1)

	for _, entry := range parentEnv {
		if name, _, found := strings.Cut(entry, "="); found && strings.EqualFold(name, go2csPathVar) {
			continue
		}

		env = append(env, entry)
	}

	return append(env, go2csPathVar+"="+ensureTrailingSeparator(go2csPath))
}

func ensureTrailingSeparator(path string) string {
	return strings.TrimRight(path, `/\`) + string(filepath.Separator)
}

func removeString(values []string, target string) []string {
	result := values[:0]
	for _, value := range values {
		if value != target {
			result = append(result, value)
		}
	}
	return result
}

func samePath(left, right string) bool {
	leftAbs, _ := filepath.Abs(left)
	rightAbs, _ := filepath.Abs(right)
	return strings.EqualFold(filepath.Clean(leftAbs), filepath.Clean(rightAbs))
}

// escapeCSharp escapes a value for a C# regular (non-verbatim) string literal, so a Windows path
// emitted into generated test-host source survives as written rather than turning its separators
// into escape sequences.
//
// XML attribute values use escapeXMLAttributeValue (solutionGenerator.go) instead.
func escapeCSharp(value string) string {
	return strings.ReplaceAll(strings.ReplaceAll(value, `\`, `\\`), `"`, `\"`)
}
