// commandLineOptions.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the converter's COMMAND LINE: the Options struct every later stage reads, the
// custom flag types the standard flag package cannot express on its own, and the build-tag
// resolution that decides which Go source files a conversion is even allowed to see.
//
// It sits apart from main.go because "what did the user ask for" is a stable, self-contained
// question, while main() is the sequence of things done about the answer. Option PLUMBING lives
// here; the conversion those options drive lives in conversionDriver.go.

package main

import (
	"flag"
	"fmt"
	"path/filepath"
	"strings"
	"time"
	"unicode"
)

type Options struct {
	licenseExpression   string // -license: explicit NuGet SPDX expression
	provenance          bool   // -provenance: deterministic source-file comment; default off
	goRoot              string
	goPath              string
	go2csPath           string
	convertStdLib       bool
	convertTimeout      time.Duration // -convert-timeout: per-package cap the -stdlib driver applies to one package's conversion; zero means an Options built in code rather than from the command line, which takes defaultConvertTimeout
	recurse             bool
	recurseOutputRoot   string   // -recurse: writable root for generated src\ + pkg\ trees; defaults to go2csPath
	mainModulePath      string   // -recurse: import path of the app (main) module; routes its packages to src\, deps to pkg\
	mainModuleDir       string   // -recurse: DIRECTORY of the app (main) module; the context a module-cache package is loaded from (see processConversion)
	packageImportPath   string   // -recurse: import path of the ONE package being converted right now; set per package by convertAll, not from the command line
	moduleOnly          bool     // -recurse=module: convert the input module's OWN packages only; the third-party closure is referenced (into pkg\) but not converted
	nugetRefs           bool     // -recurse=nuget: reference the published go2cs NuGet packages (go.<pkg>/go.lib/go.gen) instead of local $(go2csPath) project references
	targetPlatform      string   // the ONE os/arch a conversion emits for; every pass reads this
	targetPlatforms     []string // -platforms: the full requested list (len 1 for every ordinary run); targetPlatform is its first entry
	platformCensusDir   string   // -platform-census: staging + manifest directory for a multi-target emission census (no corpus output)
	platformStageDir    string   // -platform-stage: where a multi-target EMISSION stages its per-target conversions; a temporary directory this run owns and removes when empty
	refCensusPath       string   // -ref-census: JSON output path for the ж-box A1 ref-lowering census (analysis only, no corpus output)
	dualRecv            bool     // -dual-recv: B′ S0 — eligible pointer-receiver methods emit the `[GoRecv] this ref T` PRIMARY (R3 arms; the ж twin is minted by RecvGenerator). Flag-gated and corpus-inert: default off, scratch-root regens only until S2's own rebank ride
	dualRecvParams      bool     // -dual-recv-params: B′ S1 (requires -dual-recv) — the PARAMETER half: primaries lower their pointer params (twin ref-forwards), the §4.2 call-site table lands corpus-wide, and the §4.3 X3 method-call arm relaxes for directly-selectable methods (feMul/feSquare unstrip). Kept a SEPARATE flag so `-dual-recv` alone still emits the S0 floor after S1 lands — the measurability condition (coordinator, 2026-09-03)
	buildTags           []string // -tags: build tags applied to package loading AND constraint evaluation
	tagsExplicit        bool     // whether -tags was passed on the command line (vs. the -stdlib purego default)
	indentSpaces        int
	preferVarDecl       bool
	useChannelOperators bool
	includeComments     bool
	parseCgoTargets     bool
	showParseTree       bool
	debugMode           bool

	// -tests conversion options (dispatch is wired in a later stage; until then these are set only
	// by the test-conversion entry points and unit tests — the flag surface stays default-off)
	convertTests           bool          // convert the package's _test.go variants into a runnable test project
	testAction             string        // convert | build | run | compare | all
	testTimeout            time.Duration // per-child-command timeout for test build/run/compare actions
	testFilter             string        // -run regex handed VERBATIM to BOTH compare sides, and to the host under -test-action run (block-gated census)
	testPackagePath        string        // import path of the package under test (self-import binding, IP-3)
	testPackageName        string        // package name of the package under test (self-import binding, IP-3)
	testWhiteboxReference  bool          // internal _test.go files emit into a bridge while production is referenced
	testInternalBridgeName string        // C# class that owns internal-test declarations under whitebox reference
	testClassNameOverride  string        // per-variant emitted package class override for internal test files
	testMetadataAnchorName string        // C# class that owns test-generated adapters under reference models
	testConfig             string        // -test-config Debug|Release: publish/run configuration for the converted test host (default Debug -- unchanged pipeline behavior); Release also publishes with an explicit -p:go2csPath, replacing the Debug-conditional csproj-template default. See docs/phase4 tiering census
	testTiered             bool          // -test-tiered: explicit opt-back-IN to the CLR's default tiered JIT when testConfig is Release (Release's own default is DOTNET_TieredCompilation=0, for deterministic timing-bounded verdicts -- see the tiering census); meaningless when testConfig is Debug
	testAllowHandOwn       bool          // -test-allow-handown: deliberately convert a package the -stdlib queue skips (testing, unsafe, ...) into a SCRATCH root; refused by default because the default output path IS the hand-owned tree -- see requireConvertibleTestTarget
	testHandOwnHost        bool          // DERIVED, never a flag: the package under test is a hand-owned host already present at the output path, so this run converts its TESTS ONLY -- no production emission, external variant only. Set from testTargetKind; see handOwnHostTestTarget
	testProductionPath     string        // original package path retained after reference-mode self-binding is cleared
	testProductionName     string        // original package name retained for white-box object routing
	testExternalVariant    bool          // current variant is the external <name>_test package
	// testProductionInternalsVisible reports whether the COMPILATION this variant emits into may
	// name production's INTERNAL declarations. It is a property of the test ASSEMBLY, not of the Go
	// variant, and testVariantOptions — the one place that knows the model AND the variant — is what
	// sets it. Read by productionLiftReuseReachable; see its comment for the measurement.
	testProductionInternalsVisible bool
	// positionMapTarget is the package-info file this conversion's GoSourcePositionMaps
	// records land in. It must be the info file of the COMPILATION that compiles the mapped
	// source, because the records are assembly-scoped: production sources answer through
	// package_info.cs, and each test variant through its own test-info anchor.
	positionMapTarget    string
	testInlineTypeAccess bool // internal bridge types carry accessibility on their source declaration
	testFriendAssembly   bool // production internals may be consumed by the separate test assembly
}

// packageUnderTestPath returns the import path of the package under test, whichever field the
// current test-project model parks it in. The two reference models clear the self-import binding —
// production binds as an ordinary imported package — and retain the path in testProductionPath; a
// RECOMPILE conversion keeps the binding, so the path stays in testPackagePath and
// testProductionPath is empty. Anything asking "is this declaration the package under test's?"
// wants the answer under every model and must come through here.
func (o Options) packageUnderTestPath() string {
	if o.testProductionPath != "" {
		return o.testProductionPath
	}

	return o.testPackagePath
}

// recurseMode backs the -recurse flag. It is an optional-value boolean-ish flag: it implements
// IsBoolFlag so a bare `-recurse` (or `-recurse .`) sets the mode without consuming the next argument,
// while an explicit `-recurse=<value>` selects the reference style and the conversion SCOPE. Values
// are comma-separated and compose (`-recurse=module,nuget`):
//
//	(bare) / true  convert the module AND its third-party dependency closure, against LOCAL project
//	               references ($(go2csPath)core\... staged by deploy-core)
//	nuget          reference style: the stdlib, runtime and analyzer come from NuGet
//	               (go.<pkg> + go.lib + go.gen) instead, so nothing is staged locally
//	module         scope: convert the input module's OWN packages only — every third-party package is
//	               referenced (into the pkg\ tree) but never converted, so an unconvertible dependency
//	               closure cannot hold up the app's own code
//	false          disable recursion (single-package conversion)
type recurseMode struct {
	enabled    bool
	nuget      bool
	moduleOnly bool
}

// IsBoolFlag lets the flag package treat a bare `-recurse` as a boolean (Set("true")) rather than
// consuming the following token as its value — so `go2cs -recurse .` keeps "." as a positional.
func (r *recurseMode) IsBoolFlag() bool { return true }

func (r *recurseMode) String() string {
	if r == nil || !r.enabled {
		return "false"
	}

	var modes []string

	if r.moduleOnly {
		modes = append(modes, "module")
	}

	if r.nuget {
		modes = append(modes, "nuget")
	}

	if len(modes) == 0 {
		return "true"
	}

	return strings.Join(modes, ",")
}

// Set accepts the comma-separated (or space-separated) mode list documented on recurseMode. The
// values are independent — a reference style and a scope compose — so each is applied in turn
// rather than selected exclusively; `false` anywhere in the list turns recursion off entirely.
func (r *recurseMode) Set(value string) error {
	values := parseBuildTags(value) // same comma/whitespace splitting, no empty fields

	if len(values) == 0 {
		// A bare `-recurse` (flag.Value sees "true") or an empty `-recurse=`: the established
		// default — module plus its third-party closure, local project references.
		r.enabled, r.nuget, r.moduleOnly = true, false, false
		return nil
	}

	for _, mode := range values {
		switch strings.ToLower(mode) {
		case "true", "1", "on":
			r.enabled = true
		case "false", "0", "off":
			r.enabled, r.nuget, r.moduleOnly = false, false, false
			return nil
		case "nuget":
			r.enabled, r.nuget = true, true
		case "module":
			r.enabled, r.moduleOnly = true, true
		default:
			return fmt.Errorf("invalid -recurse value %q (want: (bare) | module | nuget | false, comma-separated to combine)", mode)
		}
	}

	return nil
}

// parseArgsInterspersed parses fs while allowing flags to appear AFTER positional arguments. Go's
// flag package stops at the first non-flag token, so an invocation like
// `go2cs -recurse . -go2cspath dir` would silently drop -go2cspath (leaving the default output
// root). This peels off one positional at a time and re-parses the remainder, so flags interspersed
// with or following positionals are still applied. Returns the positionals in order and the first
// parse error, if any. fs must use flag.ContinueOnError so a parse error is returned, not fatal.
func parseArgsInterspersed(fs *flag.FlagSet, args []string) ([]string, error) {
	var positionals []string

	for {
		if err := fs.Parse(args); err != nil {
			return positionals, err
		}

		rest := fs.Args()

		if len(rest) == 0 {
			return positionals, nil
		}

		positionals = append(positionals, rest[0])
		args = rest[1:]
	}
}

// defaultStdLibBuildTags are the build tags a bare `-stdlib` conversion applies when the caller did
// not pass an explicit `-tags`. The converted standard library is defined to reproduce Go built with
// `-tags purego`: a managed C# runtime can never execute the hand-written `.s` assembly that the
// default (amd64/arm64/…) build binds hot crypto/hash functions to, so those declarations would
// convert to throwing stubs that COMPILE but cannot RUN. `purego` selects the portable pure-Go
// variants (real bodies the transpiler can convert), making "the corpus reproduces Go -tags purego"
// a claim go2cs can actually honor. This default applies to `-stdlib` (the whole-library corpus) AND
// to `-tests` (see resolveBuildTags); `-recurse` end-user conversions and single-file/dir conversions
// stay tag-neutral so the user's own build tags govern, and an explicit `-tags` overrides it verbatim.
//
// `math_big_pure_go` is the SAME decision under a different spelling. `purego` is the tag the crypto
// and hash packages happen to use; math/big predates it and gates its own portable fallbacks on
// `math_big_pure_go` instead (arith_decl_pure.go vs arith_decl.go + arith_amd64.go). Without it the
// converter selected arith_decl.go — eight bodyless `//go:linkname`/`//go:noescape` declarations whose
// bodies live in arith_$GOARCH.s — so `addVV`, `subVV`, `addVW`, `subVW`, `shlVU`, `shrVU`,
// `mulAddVWW` and `addMulVVW` all became throwing partial stubs and EVERY big.Int/big.Float/big.Rat
// arithmetic path panicked at run time while compiling clean. arith_decl_pure.go forwards each to the
// `_g` pure-Go implementation already in arith.go, which converts and runs. Reached as a `time`
// failure (TestTruncateRound → big.Int.Mul → mulAddVWW), but the scope is all of math/big.
var defaultStdLibBuildTags = []string{"purego", "math_big_pure_go"}

// resolveBuildTags picks the effective build tags for a conversion run. A bare `-stdlib` run and a
// `-tests` run both apply the purego default (unless `-tags` was passed explicitly, even `-tags=` to
// clear it — a deliberate override honored verbatim). `-tests` needs the SAME default as `-stdlib`
// because it reconverts the package's PRODUCTION sources and recompiles them into the test assembly:
// it must select the exact same source files the committed converted stdlib tree was built from (that
// tree is Go built with `-tags purego`). Without this, a package whose asm and pure-Go variants are
// gated `!purego`/`purego` (crypto/subtle's xor_amd64.go vs xor_generic.go, both declaring xorBytes)
// gets BOTH files converted and collides (CS0111), and the regenerated production .cs diverges from
// the committed purego emission. All other conversions stay tag-neutral (explicit tags only).
func resolveBuildTags(convertStdLib, convertTests, tagsExplicit bool, explicit []string) []string {
	if (convertStdLib || convertTests) && !tagsExplicit {
		return defaultStdLibBuildTags
	}

	return explicit
}

// parseBuildTags splits a -tags value into individual build tags. Commas and whitespace are both
// accepted as separators (the go command has used each form over time), and empty fields are dropped
// so "-tags=" is indistinguishable from omitting the flag.
func parseBuildTags(value string) []string {
	tags := strings.FieldsFunc(value, func(r rune) bool {
		return r == ',' || unicode.IsSpace(r)
	})

	if len(tags) == 0 {
		return nil
	}

	return tags
}

// go2csRootWarned pins the unusable-root warning to ONCE per run (a conversion resolves dozens of
// imports and every one of them would otherwise repeat the same advice). Package-level and
// test-pinnable in the goModCache manner: a test saves it, resets it, and restores it on cleanup.
var go2csRootWarned bool

// resolveGo2CSPath settles the runtime/stdlib root a conversion resolves its references AND its
// imported-type-alias metadata against — the converter's -go2cspath (env GO2CSPATH, default
// ~/go2cs), which is NOT the MSBuild $(go2csPath) property of the same name.
//
// Why this exists. getImportPackageInfo maps a stdlib import to $(go2csPath)core\<pkg> and
// substitutes THIS path to find the imported package's package_info.cs, from which the emitted
// <ImportedTypeAliases> block is minted. When the root is missing, stale or partial the aliases are
// simply omitted — no warning, no error, exit 0 — so the conversion's OUTPUT silently varies with an
// ambient environment variable. That is how the behavioral corpus ended up split across two roots
// (BOARD-next-validation-candidates.md, 2026-08-06): 12 package_info.cs matched a stale ~/go2cs
// stub deploy while the rest matched the repository's own src\ tree.
//
// Two recoveries, in order:
//
//	Self-location — when the configured root is not a go2cs root (no core\golib\golib.csproj) the
//	ancestor chain of the conversion's OUTPUT is walked for one. The output, not the input, is the
//	anchor: the emitted package_info.cs/.csproj and their $(go2csPath)core references live THERE, so
//	the tree that must satisfy them is the tree the output is written into. For a bare
//	`go2cs <pkg-dir>` the two are the same directory, which is what makes an unconfigured run inside
//	a go2cs clone resolve against that clone. An explicitly configured WORKING root always wins.
//
//	A loud failure — when no root is found the run still proceeds (converting standalone code with
//	no deployed runtime is legitimate) but says so once, naming the resolved path and the
//	consequence, instead of emitting quietly alias-free metadata.
//
// Callers pass selfLocate=false where the root cannot be relocated safely: under -recurse the
// runtime root doubles as the output root when no second positional was given, so redirecting it
// would move the generated tree. -stdlib calls neither side — there the root IS the output root the
// run itself populates (core\<pkg> is written into it), so having no golib under it is the normal
// state of a first conversion, not a fault.
func resolveGo2CSPath(options *Options, outputAnchor string, selfLocate bool) {
	if isGo2CSRoot(options.go2csPath) {
		return
	}

	if selfLocate && outputAnchor != "" {
		anchor := outputAnchor

		if abs, err := filepath.Abs(anchor); err == nil {
			anchor = abs
		}

		if root := findGo2CSRootAbove(anchor); root != "" {
			options.go2csPath = root
			return
		}
	}

	// -recurse=nuget references the PUBLISHED go2cs packages (go.<pkg>/go.lib/go.gen) instead of
	// local project references, and reads its stdlib metadata from the converter's own embedded
	// records — so it needs no local root at all and must not be nagged about one.
	if options.nugetRefs || go2csRootWarned {
		return
	}

	go2csRootWarned = true

	// %s, not %q: on Windows %q escapes every separator in the path it is telling the reader to fix.
	showWarning("go2cs runtime root \"%s\" is not a converted go2cs tree — no %s under it.\n"+
		"         Imported-type-alias metadata cannot be read from it, so emitted package_info.cs\n"+
		"         <ImportedTypeAliases> blocks will be empty and $(go2csPath)core project references\n"+
		"         may not resolve. Pass -go2cspath <root>, set GO2CSPATH, or stage a root with deploy-core.",
		options.go2csPath, filepath.Join("core", "golib", "golib.csproj"))
}

// loaderBuildFlags renders the go/packages BuildFlags for the configured build tags. It returns nil
// when no tags are set so the default load path stays byte-for-byte what it was before -tags existed.
func (o Options) loaderBuildFlags() []string {
	if len(o.buildTags) == 0 {
		return nil
	}

	return []string{"-tags=" + strings.Join(o.buildTags, ",")}
}
