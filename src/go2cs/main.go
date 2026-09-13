// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// go2cs converts Go source code into C# that a Go developer can read and follow.
//
// This file is the ENTRY POINT and nothing else: it resolves the Go toolchain paths, parses the
// command line, and dispatches to exactly one of three modes —
//
//	-stdlib    convert the Go standard library      (stdLibConverter.go)
//	-recurse   convert an end-user module + its deps (moduleConverter.go)
//	 default   convert one file or one package       (conversionDriver.go)
//
// Everything main() needs lives beside it under a name that says what it owns; start with these:
//
//	commandLineOptions.go   the Options struct, the custom flag types, build-tag resolution
//	conversionDriver.go     processConversion — the order the passes run in for ONE package
//	visitorState.go         the Visitor: the per-file walker every conv*/visit* file extends
//	packageGlobalState.go   the registries concurrently-visited files publish to each other
//
// The conversion itself is spread across visit*.go (AST node -> C# declaration or statement) and
// conv*.go (expression or type -> C# text), with the analysis passes those depend on in
// *Operations.go. docs/Architecture.md has the full taxonomy.
package main

import (
	"errors"
	"flag"
	"fmt"
	"go/build"
	"io"
	"log"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"time"
)

// resolveGo2CSPathDefault answers the DEFAULT for the -go2cspath flag: a user-set GO2CSPATH when the
// environment carries one, otherwise ~/go2cs (falling back to the GOPATH parent when the home
// directory cannot be resolved). An explicit -go2cspath still overrides it, as flags override
// everything here.
//
// The semantic invariant, stated once because two halves of the converter depend on it:
//
//   - A user-set GO2CSPATH IS honored — it is the documented way to choose the runtime/stdlib root
//     without passing a flag, and the Linux harness pin relies on it.
//   - The converter NEVER exports its own derived value. GOROOT and GOPATH above are os.Setenv'd
//     deliberately, because the `go` toolchain children the converter spawns read them; GO2CSPATH has
//     no such consumer — this call site is its only reader, and the value is consumed immediately as
//     the flag default.
//   - Consequently a child environment carries exactly ONE spelling of the root (see
//     childEnvWithGo2CSPath), never a fabricated second opinion.
//
// The export this function replaced was the root of the Linux Phase-4 pipeline race (2026-08-21): it
// put an un-slashed `GO2CSPATH=<home>/go2cs` into the converter's own environment, every pipeline
// child inherited it BESIDE the injected canonical `go2csPath=<root>/`, and MSBuild — which resolves
// environment-derived properties case-insensitively — saw two entries for ONE property and picked a
// winner by enumeration order. Windows was structurally immune (one case-insensitive OS slot), so the
// defect was invisible for five weeks of Windows sweeps.
func resolveGo2CSPathDefault(goPath string) string {
	if fromEnv := os.Getenv("GO2CSPATH"); len(fromEnv) > 0 {
		return fromEnv
	}

	homeDir, err := os.UserHomeDir()

	if err != nil {
		homeDir = strings.TrimSuffix(strings.TrimSuffix(goPath, "go"), string(os.PathSeparator))
	}

	return filepath.Join(homeDir, "go2cs")
}

// checkGoRootSpelling reports a resolved GOROOT that this host cannot resolve to a Go source tree.
//
// GOROOT is the axis EVERY standard-library decision keys off: which packages are stdlib, where their
// sources are, what each one's import path is, and therefore what namespace the emission lands in. A
// value that is merely MISSPELLED — naming the right toolchain in a form the host's filesystem does
// not answer to — is the worst of the three states, because it is neither absent (the converter would
// derive one) nor usable. It reaches getProjectName, fails the under-GOROOT test there, sends every
// standard-library package into the `module std` walk-up, and produces a complete `namespace go.std.*`
// emission at exit code 0.
//
// The check is deliberately about RESOLVABILITY, not agreement. A GOROOT that names a genuinely
// different toolchain than the one on PATH is legitimate and supported — that is what -goroot and the
// resolveLoaderGoRoot toolchain switch below are for — so this must never turn a deliberate choice
// into an error. What it rejects is the value that answers no directory at all: the MSYS/Cygwin
// `/c/Users/<user>/sdk/go1.23.12` spelling of a Windows path, a stale root whose directory has moved,
// a typo. Requiring `<goRoot>/src` rather than merely `<goRoot>` is what makes it specific: every Go
// distribution ships its sources there, the converter cannot convert a line without them, and a
// directory that exists but holds no `src` is not a toolchain root whatever its name suggests.
//
// The error names the toolchain's OWN answer when it can get one, because the fix is almost always
// "use that spelling" and the two strings side by side are the whole diagnosis.
func checkGoRootSpelling(goRoot string) error {
	if info, err := os.Stat(filepath.Join(goRoot, "src")); err == nil && info.IsDir() {
		return nil
	}

	// %s, not %q: on Windows %q escapes every separator in the paths it is asking the reader to compare.
	message := fmt.Sprintf("GOROOT is set to \"%s\", which is not a Go toolchain root on this host — no src directory under it.\n"+
		"       Every standard-library decision keys off GOROOT, so a spelling this host cannot resolve would\n"+
		"       silently emit the whole standard library into namespace go.std.* and exit 0.", goRoot)

	if reported, err := getGoEnv("GOROOT"); err == nil {
		if reported = strings.TrimSpace(reported); reported != "" {
			message += fmt.Sprintf("\n       The go command on PATH reports its GOROOT as \"%s\" — set GOROOT to exactly that spelling,\n"+
				"       or pass -goroot with it. An MSYS/Cygwin-style \"/c/...\" path is the usual cause on Windows.", reported)
		}
	}

	return errors.New(message)
}

// canonicalTestConfig normalizes -test-config's raw flag value the same way testAction canonicalizes
// its own (case-insensitive input, one fixed spelling out) — but titlecased rather than lowercased,
// since the result is passed verbatim to `dotnet publish -c <value>` and recorded on proof pages,
// where "Debug"/"Release" is the spelling every other MSBuild-facing surface in this repo already
// uses. An input that matches neither is returned UNCHANGED (trimmed only) so the caller's validation
// switch reports the exact string the user typed, not a silently-substituted default.
func canonicalTestConfig(raw string) string {
	trimmed := strings.TrimSpace(raw)

	switch strings.ToLower(trimmed) {
	case "debug":
		return "Debug"
	case "release":
		return "Release"
	default:
		return trimmed
	}
}

func main() {
	// No-op unless GO2CS_PPROF is set; see diagnosticProfiling.go. First thing in main so a run that
	// stalls during option resolution or package loading is still reachable by a profiler.
	startDiagnosticProfiling()

	var goRoot, goPath, go2csPath string
	var err error

	// goRootPinned records that the OPERATOR chose GOROOT rather than the converter deriving it. A
	// pinned value is honored verbatim; a DERIVED one is re-resolved below against the toolchain the
	// loader will actually run, which is not necessarily the one on PATH.
	goRootPinned := false

	// Resolve GOROOT and GOPATH variables, any defined environment
	// variables will take precedence over derived values and command
	// line flags will override all
	if goRoot = os.Getenv("GOROOT"); len(goRoot) == 0 {
		if goRoot, err = getGoEnv("GOROOT"); err != nil {
			goRoot = runtime.GOROOT()
		}

		if len(goRoot) == 0 {
			log.Fatalln("Failed to resolve GOROOT path")
		}

		os.Setenv("GOROOT", goRoot)
	} else {
		goRootPinned = true
	}

	if goPath = os.Getenv("GOPATH"); len(goPath) == 0 {
		if goPath, err = getGoEnv("GOPATH"); err != nil {
			goPath = build.Default.GOPATH
		}

		if len(goPath) == 0 {
			log.Fatalln("Failed to resolve GOPATH path")
		}

		os.Setenv("GOPATH", goPath)
	}

	// Resolve the -go2cspath flag's default. Unlike GOROOT/GOPATH above this is NOT exported back
	// into the converter's environment — see resolveGo2CSPathDefault for why that mattered.
	go2csPath = resolveGo2CSPathDefault(goPath)

	// Define command line flags for options
	commandLine := flag.NewFlagSet(os.Args[0], flag.ContinueOnError)
	commandLine.SetOutput(io.Discard)

	goRootCmd := commandLine.String("goroot", goRoot, "Path to Go root directory")
	goPathCmd := commandLine.String("gopath", goPath, "Path to Go path directory")
	go2csPathCmd := commandLine.String("go2cspath", go2csPath, "Path to C# converted code")
	convertStdLibCmd := commandLine.Bool("stdlib", false, "Convert Go standard library (implies -tags purego by default; pass an explicit -tags to override)")
	convertTimeoutCmd := commandLine.Duration("convert-timeout", defaultConvertTimeout, "Per-package cap the -stdlib driver applies to ONE package's conversion; Go duration syntax, must be greater than zero. It is a safety net against a hung conversion, never a performance assumption -- a slow host under concurrent load can legitimately need far more than the default, and a package killed early is reported as a failed package")
	convertTestsCmd := commandLine.Bool("tests", false, "Convert eligible Go package tests and emit a runnable test host project")
	testActionCmd := commandLine.String("test-action", "convert", "Converted-test action: convert, build, run, compare, or all")
	testTimeoutCmd := commandLine.Duration("test-timeout", 2*time.Minute, "Timeout for each converted-test child process (build/run/compare)")
	testFilterCmd := commandLine.String("test-filter", "", "Regex handed VERBATIM to BOTH sides of a -test-action compare (go test -run and the converted host --run), so the two runs filter identically. Intended for the block-gated census: exclude a test that BLOCKS the suite by passing an anchored alternation of the parents to keep. A gated census is DIAGNOSTIC ONLY and must never bank a row -- the row banks from an ungated run, after the block is rooted or the divergence disclosed")
	testConfigCmd := commandLine.String("test-config", "Release", "Publish/run configuration for the converted test host: Release (default since 2026-09-02 -- the validation configuration of RECORD, with an explicit -p:go2csPath replacing the Debug-conditional csproj-template default, and the CLR's tiered JIT disabled by default, see -test-tiered) or Debug (the pre-2026-09-02 default, still fully supported by flag). Recorded on every proof page and in the comparison record so a verdict carries the level it was measured at. The default moved on the deployment owner's ruling, gated on the Release census (docs/phase4/CENSUS-release-tc0-delta.md), not on this flag's own authority")
	testTieredCmd := commandLine.Bool("test-tiered", false, "With -test-config Release, opt back IN to the CLR's default tiered JIT (Release's own default is DOTNET_TieredCompilation=0, since a verdict that depends on JIT promotion timing is not reproducible run to run). Meaningless with -test-config Debug. It changes what the C# host's JIT does, not what the converter emits")
	testAllowHandOwnCmd := commandLine.Bool("test-allow-handown", false, "Convert a package the -stdlib queue deliberately skips (testing, unsafe, builtin, cmd/...) as a -tests target anyway. Refused by default: `testing` IS the hand-owned Phase-4 test host, and the pipeline's natural output path is the very directory the host lives in, so a mistyped command overwrites it (measured 2026-09-03 -- the run replaced src/core/testing/testing.cs with Go's converted testing.go). Pass this ONLY with a SCRATCH output root, for a deliberate measurement whose emission is thrown away; it is not a route to banking such a package, which would still hit the F15b 'ONE testing package, period' collision")
	var recurseVal recurseMode
	commandLine.Var(&recurseVal, "recurse", "Recursively convert an end-user module and its third-party dependencies (references the pre-converted standard library); use -recurse=module to convert only the module's own packages, leaving the third-party closure referenced but unconverted, and -recurse=nuget to reference the published go2cs NuGet packages (go.<pkg>/go.lib/go.gen) instead of local project references (values combine: -recurse=module,nuget)")
	targetPlatformCmd := commandLine.String("platforms", fmt.Sprintf("%s/%s", runtime.GOOS, runtime.GOARCH), "Target platform(s) for conversion, format: os/arch; comma-separated for a list (windows/amd64,linux/amd64,darwin/amd64), which with -stdlib emits the multi-platform (layout L3) corpus — one GOOS per target")
	platformCensusCmd := commandLine.String("platform-census", "", "With -stdlib and two or more -platforms targets: convert once per target into an isolated seeded staging root under this directory, classify the emissions (shared/variant/partial/exclusive) and write platform-manifest.json there. Emits NO corpus output")
	refCensusCmd := commandLine.String("ref-census", "", "With -stdlib: run the ж-box A1 ref-lowering classification census (analysis only, never emits) over the standard library — once per -platforms target — and write the JSON report to this path. See docs/phase4/DESIGN-zh-box-reduction.md §9 stage A1")
	platformStageCmd := commandLine.String("platform-stage", "", "Directory a multi-platform -stdlib EMISSION stages its per-target conversions in (kept afterwards for inspection); a temporary directory is created and removed when omitted")
	buildTagsCmd := commandLine.String("tags", "", "Comma-separated build tags applied when loading packages, e.g. -tags purego to select the portable Go implementations over assembly ones (with -stdlib, purego is applied by default and any explicit -tags value replaces it)")
	indentSpacesCmd := commandLine.Int("indent", 4, "Number of spaces for indentation")
	preferVarDeclCmd := commandLine.Bool("var", true, "Prefer \"var\" declarations")
	useChannelOperatorsCmd := commandLine.Bool("uco", true, fmt.Sprintf("Use channel operators: %s / %s", ChannelLeftOp, ChannelRightOp))
	licenseCmd := commandLine.String("license", "", "Override NuGet license metadata with an SPDX expression (for example MIT or BSD-3-Clause); does not relicense input code")
	provenanceCmd := commandLine.Bool("provenance", false, "Include a deterministic source-file provenance comment (no conversion timestamp)")
	includeCommentsCmd := commandLine.Bool("comments", false, "Include comments in output")
	parseCgoTargetsCmd := commandLine.Bool("cgo", false, "Parse cgo targets")
	showParseTreeCmd := commandLine.Bool("tree", false, "Show parse tree")
	csprojFileCmd := commandLine.String("csproj", "", "Path to custom .csproj template file")
	debugModeCmd := commandLine.Bool("debug", false, "Enable debug mode")
	allowStaleConverterCmd := commandLine.Bool("allow-stale-converter", false, "Proceed with a -stdlib or -tests run even when this binary is OLDER than the converter sources beside it. Those two drivers otherwise refuse, because their output is banked or measured and a stale run emits the previous converter's C# while exiting 0. Pass this for the one deliberate case: an A/B against a PRESERVED binary, where the pinned binary IS the measurement (see converterStaleness.go)")
	dualRecvCmd := commandLine.Bool("dual-recv", false, "B′ S0: eligible pointer-receiver methods emit the ref-receiver PRIMARY beside the ж twin (flag-gated; scratch-root regens only until S2's rebank ride)")
	dualRecvParamsCmd := commandLine.Bool("dual-recv-params", false, "B′ S1 (requires -dual-recv): primaries lower their pointer params, the call-site selection table lands, and the X3 method-call veto relaxes for directly-selectable methods (kept separate so -dual-recv alone still emits the S0 floor)")

	var positionals []string
	positionals, err = parseArgsInterspersed(commandLine, os.Args[1:])

	// Pin go/build's resolver to the converter's robustly-resolved GOROOT/GOPATH. build.Default is
	// initialized at package-init from the start-up environment, which can be empty or stale in a
	// child process (e.g. the behavioral runner Execs go2cs.exe with a sparse env) — leaving
	// build.Import unable to find even stdlib packages like "fmt". Without this, getImportPackageInfo
	// falls through to the local-module path and emits a machine-specific absolute GOROOT reference.
	build.Default.GOROOT = *goRootCmd
	build.Default.GOPATH = *goPathCmd

	var inputFilePath string
	var convertStdLib bool

	if err == nil {
		convertStdLib = *convertStdLibCmd
	}

	if !convertStdLib && len(positionals) > 0 {
		inputFilePath = strings.TrimSpace(positionals[0])
	}

	// A bare `-stdlib` conversion (the whole-library corpus) applies `-tags purego` by default; the
	// converted standard library is defined to reproduce Go built with that tag (see
	// defaultStdLibBuildTags). Detect whether the caller passed `-tags` at all: if they did — even
	// `-tags=` to clear it — that is a deliberate override and we honor it verbatim.
	//
	// `-tests` gets the SAME purego default: a `-tests` run reconverts the package's PRODUCTION
	// sources and recompiles them into the test assembly, so it must select the exact same source
	// files the committed converted stdlib tree was built from (that tree is defined as Go built with
	// -tags purego). Without this, a stdlib package whose asm variant and pure-Go variant are gated
	// `!purego`/`purego` (crypto/subtle's xor_amd64.go declares a bodyless `func xorBytes` the .s
	// provides, while xor_generic.go declares the same func WITH a body) has BOTH files converted and
	// collides — CS0111 duplicate member — and the regenerated production .cs diverges from the
	// committed purego emission. A managed C# runtime can never execute the hand-written .s assembly,
	// so purego (the portable pure-Go implementations) is the correct default for a `-tests` run too.
	// `-recurse`/single-file conversions stay tag-neutral (their build tags govern).
	tagsExplicit := false
	commandLine.Visit(func(f *flag.Flag) {
		if f.Name == "tags" {
			tagsExplicit = true
		}
	})

	buildTags := resolveBuildTags(convertStdLib, *convertTestsCmd, tagsExplicit, parseBuildTags(*buildTagsCmd))

	if err != nil || (!convertStdLib && len(inputFilePath) == 0) {
		if err != nil {
			fmt.Fprintf(os.Stderr, "Error: %s\n", err)
		}

		fmt.Fprintln(os.Stderr, `
 Usage: go2cs [options] <input_dir> [output_dir]
 
 Options:`)

		commandLine.SetOutput(nil)
		commandLine.PrintDefaults()

		fmt.Fprintln(os.Stderr, `
Examples:
  go2cs -indent 2 -var=false example.go conv/example.cs
  go2cs example.go
  go2cs -cgo=true input_dir output_dir
  go2cs package_dir
  go2cs -tests package_dir                  # Convert production sources and package tests
  go2cs -tests -test-action all package_dir # Convert, build, run, and compare with go test
  go2cs -stdlib                           # Convert the entire Go standard library (applies -tags purego by default)
  go2cs -stdlib fmt io/ioutil strings     # Convert specific standard library packages
  go2cs -recurse module_dir               # Convert a module + its third-party deps (references stdlib)
  go2cs -recurse module_dir output_root   # Same, with generated src/pkg trees isolated under output_root
  go2cs -recurse=module module_dir        # Convert only the module's own packages (deps referenced, not converted)
  go2cs -recurse=nuget module_dir         # Same, but reference the go2cs stdlib from NuGet (go.*, no deploy-core)
  go2cs -recurse=module,nuget module_dir  # Values combine: module-only scope with NuGet references
  go2cs -stdlib -allow-stale-converter    # Proceed on a DELIBERATELY pinned binary (an A/B against a preserved go2cs);
                                          # without it, -stdlib and -tests refuse to run a binary older than their sources
  go2cs -stdlib -comments -tags purego    # Explicit form of the default: the portable Go crypto over the assembly ones
  go2cs -stdlib -tags=                    # Opt OUT of the purego default (reproduce the asm-backed default build)
  go2cs -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 -platform-census out
                                          # Emission census: convert once per target into out\<goos>-<goarch>,
                                          # write out\platform-manifest.json; no corpus output
  go2cs -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64
                                          # Multi-platform corpus (layout L3): files shared by every target
                                          # stay flat, platform-varying ones land in <pkg>\<goos>, and the
                                          # .csproj selects one with $(GoTargetOS)
 `)
		os.Exit(1)
	}

	// GOROOT must describe the toolchain that will actually LOAD the packages, not whichever one is
	// on PATH. Since Go 1.21 a module can require a newer release and the go command silently
	// re-execs a downloaded toolchain to satisfy it; go/packages follows that choice, the converter
	// did not, and every stdlib-versus-third-party decision keys off GOROOT — so the mismatch
	// reclassified the ENTIRE standard library and emitted references to projects nothing generates,
	// with a 0 exit code. Re-resolve from the input's own module context unless the operator pinned
	// GOROOT, in which case their value stands untouched.
	// See docs/phase4/FINDING-toolchain-goroot-divergence.md.
	commandLine.Visit(func(f *flag.Flag) {
		if f.Name == "goroot" {
			goRootPinned = true
		}
	})

	// Is this binary itself current? Every harness that runs the converter already asks, but the
	// paths that consult NOTHING are the hand-invoked ones — and their caller is a person at a
	// shell, so the only place the question can be asked for them is here. It ENUMERATES what is
	// newer rather than naming one file (a lane reasoned from a single named entry to a wrong
	// conclusion on 2026-09-03), and it is FATAL for the two drivers whose output is banked or
	// measured — -stdlib and -tests — escapable only by the named -allow-stale-converter. Every
	// other shape is the scratch probe, and keeps the advisory. Silent when no source tree sits
	// beside the executable. See converterStaleness.go.
	checkConverterStaleness(convertStdLib || *convertTestsCmd, *allowStaleConverterCmd)

	// Normalize the RESOLVED GOROOT once, here, rather than at each of the dozen sites that compare a
	// path against it. filepath.Clean folds the spelling variants of one directory that this host can
	// still resolve — forward slashes on Windows, a trailing separator, a doubled one, an interior
	// `.` — so no downstream comparison has to know which spelling arrived. Cheap, and it makes the
	// value that every stdlib decision keys off single-valued by construction.
	*goRootCmd = filepath.Clean(*goRootCmd)

	// Clean is normalization, not validation, and it cannot rescue a spelling that names no directory
	// on this host — an MSYS/Cygwin `/c/Users/...` GOROOT Cleans to `\c\Users\...`, which is still not
	// a path Windows resolves. Reject that here. The doctrine this enforces is the one the
	// forward-slash-GOROOT finding paid for: A PATH THE CONVERTER HALF-RECOGNIZES IS WORSE THAN ONE
	// IT REJECTS. A run that proceeds on an unresolvable GOROOT does not fail — it silently reclassifies
	// the entire standard library (see getProjectName) and exits 0 over a poisoned emission.
	if err := checkGoRootSpelling(*goRootCmd); err != nil {
		log.Fatalf("%v\n", err)
	}

	if !goRootPinned {
		if resolved, loaderVersion, switched := resolveLoaderGoRoot(inputFilePath, *goRootCmd); switched {
			showWarning("the Go module being converted selects a toolchain other than the one on PATH; "+
				"resolving GOROOT as %s (PATH toolchain: %s). Conversion follows the module's toolchain", resolved, *goRootCmd)

			*goRootCmd = resolved
			build.Default.GOROOT = resolved

			// Must precede the first goVersion(): it fixes the release reported everywhere,
			// including the $(GoStdLibVersion) a -recurse=nuget project restores its go.<pkg>
			// references at.
			pinGoVersion(loaderVersion)
		}
	}

	// -recurse=nuget references a PUBLISHED corpus, which exists for exactly one Go release. Converting
	// a different release's standard library against it yields a project that cannot restore, and the
	// user meets that as NU1101s naming packages they never imported. Refuse while the diagnosis is
	// still legible. See docs/phase4/FINDING-toolchain-goroot-divergence.md.
	if recurseVal.nuget && !convertStdLib {
		if err := checkNuGetStdLibCompatibility(goVersion(), publishedStdLibRelease()); err != nil {
			log.Fatalf("%v\n", err)
		}
	}

	// -platforms is a LIST as of the multiplatform-corpus design's increment 1. A single conversion
	// PASS still emits for exactly one target — targetPlatform stays the first entry and
	// single-platform behavior is unchanged — but a `-stdlib` run given several targets now converts
	// once per target and MERGES the emissions into one layout-L3 corpus (increment 2,
	// platformEmit.go). -platform-census remains the read-only instrument: same staging, same
	// classification, a manifest instead of a corpus.
	targetPlatforms, err := parsePlatformList(*targetPlatformCmd)

	if err != nil {
		log.Fatalf("%v\n", err)
	}

	platformCensusDir := strings.TrimSpace(*platformCensusCmd)
	refCensusPath := strings.TrimSpace(*refCensusCmd)

	if refCensusPath != "" && !convertStdLib {
		log.Fatalln("-ref-census requires -stdlib: the census classifies the standard library (analysis only)")
	}

	if platformCensusDir != "" {
		if !convertStdLib {
			log.Fatalln("-platform-census requires -stdlib: the census converts the standard library once per target")
		}

		if len(targetPlatforms) < 2 {
			log.Fatalf("-platform-census needs at least two -platforms targets to compare (got %d: %s)\n",
				len(targetPlatforms), strings.Join(targetPlatforms, ", "))
		}
	} else if len(targetPlatforms) > 1 && !convertStdLib {
		// Only the standard-library driver has the seeded-staging + classification machinery a
		// multi-platform emission is built on; a single-package or -recurse conversion emits for one
		// target, so a list there is a mistake worth naming rather than silently truncating.
		log.Fatalf("-platforms lists %d targets (%s) but multi-platform emission requires -stdlib; name a single target\n",
			len(targetPlatforms), strings.Join(targetPlatforms, ", "))
	}

	options := Options{
		goRoot:              *goRootCmd,
		goPath:              *goPathCmd,
		go2csPath:           *go2csPathCmd,
		convertStdLib:       convertStdLib,
		convertTimeout:      *convertTimeoutCmd,
		convertTests:        *convertTestsCmd,
		testAction:          strings.ToLower(strings.TrimSpace(*testActionCmd)),
		testTimeout:         *testTimeoutCmd,
		testFilter:          strings.TrimSpace(*testFilterCmd),
		testConfig:          canonicalTestConfig(*testConfigCmd),
		testTiered:          *testTieredCmd,
		testAllowHandOwn:    *testAllowHandOwnCmd,
		recurse:             recurseVal.enabled,
		moduleOnly:          recurseVal.moduleOnly,
		nugetRefs:           recurseVal.nuget,
		targetPlatform:      targetPlatforms[0],
		targetPlatforms:     targetPlatforms,
		platformCensusDir:   platformCensusDir,
		platformStageDir:    strings.TrimSpace(*platformStageCmd),
		refCensusPath:       refCensusPath,
		buildTags:           buildTags,
		tagsExplicit:        tagsExplicit,
		indentSpaces:        *indentSpacesCmd,
		preferVarDecl:       *preferVarDeclCmd,
		useChannelOperators: *useChannelOperatorsCmd,
		includeComments:     *includeCommentsCmd,
		provenance:          *provenanceCmd,
		licenseExpression:   strings.TrimSpace(*licenseCmd),
		parseCgoTargets:     *parseCgoTargetsCmd,
		showParseTree:       *showParseTreeCmd,
		debugMode:           *debugModeCmd,
		dualRecv:            *dualRecvCmd,
		dualRecvParams:      *dualRecvParamsCmd,
	}

	// -dual-recv-params (S1) is a refinement of -dual-recv (S0), never standalone: the parameter
	// half emits into the ref-return primaries S0 declares, and the X3 relaxation only pays off
	// once those primaries exist. Rejecting the lone form keeps the S0-floor state (`-dual-recv`
	// alone) unambiguous — the measurability condition the flag split exists for.
	if options.dualRecvParams && !options.dualRecv {
		log.Fatalln("-dual-recv-params (B′ S1) requires -dual-recv (B′ S0): the parameter half emits into S0's primaries")
	}

	// The capture-mode pass runs across four drivers with no options in reach; the flag mirrors
	// into its package global once, here (see selectRefReturnPrimaries).
	dualRecvEnabled = options.dualRecv
	dualRecvParamsEnabled = options.dualRecvParams

	// Validated unconditionally rather than under -stdlib (the only mode that reads it today):
	// a non-positive cap is never meaningful in any mode, and the value the user typed is worth
	// rejecting where they typed it. Same fail-fast posture as -test-timeout below.
	if err := validateConvertTimeout(options.convertTimeout); err != nil {
		log.Fatalln(err)
	}

	if options.convertTests {
		// -tests and -recurse compose badly today (the recursive module walk has its own
		// conversion driver and output routing); convert the module first, then its packages'
		// tests individually. Revisit when a recursive test-conversion mode is designed.
		if options.recurse {
			log.Fatalln("-tests cannot be combined with -recurse: convert the module first, then convert its package tests individually")
		}

		switch options.testAction {
		case "convert", "build", "run", "compare", "all":
		default:
			log.Fatalf("Invalid -test-action %q: expected convert, build, run, compare, or all\n", options.testAction)
		}

		switch options.testConfig {
		case "Debug", "Release":
		default:
			log.Fatalf("Invalid -test-config %q: expected Debug or Release\n", options.testConfig)
		}

		if options.testTimeout <= 0 {
			log.Fatalln("-test-timeout must be greater than zero")
		}

		// Same fail-fast posture as -test-timeout above: a filter that cannot compile must die
		// HERE, naming itself, rather than reaching two child processes that each reject it in
		// their own dialect. Go's RE2 is the stricter of the two engines the string is handed to
		// (the converted host parses it with .NET Regex), so compiling it here also rejects a
		// pattern that only .NET would accept -- which is what keeps the two sides identical.
		if options.testFilter != "" {
			for _, element := range strings.Split(options.testFilter, "/") {
				if _, err := regexp.Compile(element); err != nil {
					log.Fatalf("Invalid -test-filter %q: %v\n", options.testFilter, err)
				}
			}
		}
	}

	// Load custom .csproj template if specified
	if *csprojFileCmd != "" {
		var err error
		csprojTemplate, err = os.ReadFile(*csprojFileCmd)

		if err != nil {
			log.Fatalf("Failed to read custom .csproj template file \"%s\": %s\n", *csprojFileCmd, err)
		}
	}

	if convertStdLib {
		// No go2csPath resolution here, deliberately: under -stdlib the root is the OUTPUT root this
		// run populates (core\<pkg> is written into it and each package reads its already-converted
		// dependencies back from the same place), so an absent core\golib is the normal state of a
		// first conversion into a fresh root — neither a thing to relocate nor a thing to warn about.

		// The toolchain pin, checked before any package is loaded. Placed at the top of the -stdlib
		// block so it covers every mode reached through it — the conversion itself, the multi-platform
		// emission, and both censuses — since all of them read GOROOT's sources as their input and a
		// census taken on the wrong release measures the wrong sources just as surely as a conversion
		// emits them.
		if err := checkCorpusToolchainPin("-stdlib", convertingRelease(options.goRoot), corpusPinnedRelease(options.go2csPath)); err != nil {
			log.Fatalf("%v\n", err)
		}

		// Check if specific packages are specified
		var packageFilter []string

		if len(positionals) > 0 {
			packageFilter = make([]string, len(positionals))

			for i := range positionals {
				packageFilter[i] = strings.TrimSpace(positionals[i])
			}

			fmt.Printf("Only converting specified packages: %s\n", strings.Join(packageFilter, ", "))
		}

		if options.refCensusPath != "" {
			// ж-box A1 ref-lowering census: pure analysis — the standard library is LOADED once
			// per target and classified, nothing is emitted anywhere. -go2cspath is read only to
			// locate src/core for the hand-own cross-reference (see refLoweringCensus.go).
			if err := runRefLoweringCensus(options, options.refCensusPath, packageFilter); err != nil {
				log.Fatalf("Ref-lowering census failed: %v", err)
			}

			return
		}

		if options.platformCensusDir != "" {
			// Multi-platform EMISSION CENSUS: the -go2cspath root is read as the SEED every staging
			// root is copied from and is never written to; each target converts into its own root
			// under the census directory, and the only artifact produced is the manifest.
			if err := runPlatformCensus(options, options.platformCensusDir, packageFilter); err != nil {
				log.Fatalf("Multi-platform census failed: %v", err)
			}

			return
		}

		if len(options.targetPlatforms) > 1 {
			// Multi-platform EMISSION (layout L3): convert once per target into a seeded staging
			// root, classify the emissions, and merge them into the -go2cspath corpus — shared
			// files flat, platform-varying ones in per-GOOS folders.
			if err := runPlatformEmission(options, options.platformStageDir, packageFilter); err != nil {
				log.Fatalf("Multi-platform emission failed: %v", err)
			}

			return
		}

		// Initialize standard library converter
		converter := NewStdLibConverter(options)

		// Run the conversion process
		if err := converter.ScanAndConvertFiltered(packageFilter); err != nil {
			log.Fatalf("Standard library conversion failed: %v", err)
		}
	} else {
		// Check if the input is a file or a directory
		fileInfo, err := os.Stat(inputFilePath)

		if err != nil {
			log.Fatalf("Failed to access input file path \"%s\": %s\n", inputFilePath, err)
		}

		isDir := fileInfo.IsDir()

		if !isDir {
			inputFilePath = filepath.Dir(inputFilePath)
		}

		var outputFilePath string

		// If the user has provided a second argument, we will use it as the output directory or file
		if len(positionals) > 1 {
			outputFilePath = strings.TrimSpace(positionals[1])
		} else {
			outputFilePath = inputFilePath
		}

		inputFilePath, err = filepath.Abs(inputFilePath)

		if err != nil {
			log.Fatalf("Failed to get absolute file path \"%s\": %s\n", inputFilePath, err)
			return
		}

		if options.recurse {
			// Recursive end-user conversion: convert the input module AND every third-party
			// dependency package in its transitive closure, in dependency order, referencing the
			// pre-converted standard library. The stdlib is not converted. A supplied second
			// positional is the writable recurse output root; without one, preserve the established
			// layout under -go2cspath.
			options.recurseOutputRoot = options.go2csPath

			if len(positionals) > 1 {
				options.recurseOutputRoot, err = filepath.Abs(outputFilePath)

				if err != nil {
					log.Fatalf("Failed to get absolute recurse output path %q: %v\n", outputFilePath, err)
				}
			}

			// Warn on an unusable runtime root, but do NOT self-locate one: without a second
			// positional the runtime root IS the recurse output root assigned just above, so
			// relocating it here would move the whole generated tree out from under the caller.
			resolveGo2CSPath(&options, "", false)

			converter := NewModuleConverter(options)

			if err := converter.ConvertModule(inputFilePath); err != nil {
				log.Fatalf("Recursive module conversion failed: %s\n", err)
			}
		} else {
			// -tests always preserves comments: test conversions are derivative works of the
			// package's Go sources, and the per-file Go copyright/license header MUST survive
			// into the emitted *_test.cs (the same license requirement as -stdlib -comments) —
			// plus the doc comments are what keep the converted suite reviewable.
			if options.convertTests {
				options.includeComments = true

				// Resolve the output to an ABSOLUTE path up front so every downstream path is
				// absolute: the test .tests.csproj passed to `dotnet build`/`run` (executeTestAction
				// joins it onto this outputPath and runs from it), and the $(go2csPath) root walk
				// below. A relative output argument — the README's `src/core/...` — left
				// `dotnet run --project <relative>` resolving against the wrong working directory
				// (MSB1009), and left the located root relative so the emitted csproj's relative
				// go2csPath (via filepath.Rel, which needs matching absoluteness) came out broken and
				// non-reproducible. Absolute here makes the whole -tests flow invocation-independent.
				if abs, absErr := filepath.Abs(outputFilePath); absErr == nil {
					outputFilePath = abs
				}
			}

			// Self-locate the project-reference root when the configured go2csPath is not itself a
			// valid root (no core\golib — e.g. the ~/go2cs default on a bare clone with no
			// deploy-core staging) and the OUTPUT lands inside a go2cs source tree; warn once when
			// none is found. Applies to EVERY single-package conversion, not just -tests: the
			// imported-type-alias metadata a bare `go2cs <pkg-dir>` reads comes from this root, so
			// leaving it ambient made the emitted package_info.cs vary with the shell's GO2CSPATH.
			// Mutated HERE, before conversion AND the -tests actions, so the manifest's digest
			// (which folds the options) is written and validated with the same root — the documented
			// two-argument validation command then works from a clone with no flags or environment
			// setup. An explicitly configured WORKING root wins.
			resolveGo2CSPath(&options, outputFilePath, true)

			// The toolchain pin, checked once the root above is final — it is the tree version.props is
			// read from. Every -test-action is covered, not just the converting ones: build/run/compare
			// act on artifacts whose Go counterpart is re-run from THIS toolchain's sources, so a
			// mismatch invalidates the comparison as thoroughly as it invalidates a conversion. A plain
			// single-package conversion stays unguarded — converting arbitrary Go with any toolchain is
			// legitimate, and only the corpus-defining modes carry the pin.
			if options.convertTests {
				if err := checkCorpusToolchainPin("-tests", convertingRelease(options.goRoot), corpusPinnedRelease(options.go2csPath)); err != nil {
					log.Fatalf("%v\n", err)
				}

				// The hand-own guard, checked BEFORE processConversion writes anything: the
				// -stdlib queue's skip list has no -tests counterpart, so until this existed a
				// -tests run pointed at `testing` converted Go's production sources straight
				// over the hand-owned host they would then have to compile against.
				kind, err := requireConvertibleTestTarget(inputFilePath, outputFilePath, options)
				if err != nil {
					log.Fatalf("%v\n", err)
				}

				// A hand-owned HOST is admitted for its TESTS ONLY: its C# counterpart already
				// stands at the output path, so there is nothing to convert on the production
				// side and everything to lose by trying. The flag routes past processConversion
				// entirely below — the production emission is not suppressed file-by-file, it
				// never runs.
				options.testHandOwnHost = kind == testTargetHandOwnHost
			}

			// -tests: convert-and-hook runs for the convert/all actions (processConversion ends
			// by converting the package's tests); build/run/compare act on EXISTING artifacts
			// (manifest-validated) without reconverting.
			if !options.convertTests || options.testAction == "convert" || options.testAction == "all" {
				// The production sources about to be converted are RECOMPILED into the test
				// assembly, so their usings must be qualified against the UNION of the production
				// and _test.go reference closures — collect the test half first.
				if options.convertTests {
					collectSiblingTestClosure(inputFilePath, options)
				}

				switch {
				case options.testHandOwnHost:
					// TESTS ONLY. processConversion is what emits the production .cs, the
					// package_init.cs hook and the production csproj; for a hand-owned host all
					// three would overwrite hand-written files, so the whole call is skipped and
					// the test conversion — which processConversion would otherwise have ended by
					// calling — is entered directly. This is the ONE structural difference between
					// a host row and every other row, and it is why the F15b collision cannot be
					// reached from the default command line at all.
					if err := processTestConversion(inputFilePath, outputFilePath, options); err != nil {
						log.Fatalf("Failed to convert package tests in %q: %v\n", inputFilePath, err)
					}

				default:
					// A single-package conversion has nothing to continue with, so the load failure
					// processConversion returns — for a batch driver to record and skip — is fatal
					// here, the behavior this call site always had.
					if err := processConversion(inputFilePath, isDir, outputFilePath, options); err != nil {
						log.Fatalf("Conversion failed: %v\n", err)
					}
				}
			}

			if options.convertTests && options.testAction != "convert" {
				if err := executeTestAction(inputFilePath, outputFilePath, options); err != nil {
					log.Fatalf("Converted test action failed: %v\n", err)
				}
			}
		}
	}
}
