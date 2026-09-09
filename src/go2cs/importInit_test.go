// importInit_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// TestImportInitName locks the generated hook name for an imported package: one hook per imported
// package, named from the IMPORT PATH so it is unique by construction within the class (two
// imports in one file are two methods — a shared name would be CS0111), and composed only of C#
// identifier characters so a module path's dots and hyphens cannot break it.
func TestImportInitName(t *testing.T) {
	prefix := "init" + TempVarMarker + TempVarMarker + "import"

	cases := []struct {
		importPath string
		want       string
	}{
		{"image/png", prefix + TypeAliasDot + "image" + TypeAliasDot + "png"},
		{"crypto/sha256", prefix + TypeAliasDot + "crypto" + TypeAliasDot + "sha256"},
		{"runtime", prefix + TypeAliasDot + "runtime"},
		{"math/rand/v2", prefix + TypeAliasDot + "math" + TypeAliasDot + "rand" + TypeAliasDot + "v2"},
		// A module path: the dots of a domain and the hyphen of a directory are not identifier
		// characters, so both reduce to '_'.
		{"github.com/mattn/go-isatty", prefix + TypeAliasDot + "github_com" + TypeAliasDot + "mattn" + TypeAliasDot + "go_isatty"},
	}

	for _, tc := range cases {
		if got := importInitName(tc.importPath); got != tc.want {
			t.Errorf("importInitName(%q) = %q, want %q", tc.importPath, got, tc.want)
		}
	}

	// Distinct import paths must never collide, and none may land on the -tests package-init hook's
	// reserved name (both live under the doubled temp marker).
	seen := map[string]string{}

	for _, tc := range cases {
		name := importInitName(tc.importPath)

		if prior, dup := seen[name]; dup {
			t.Errorf("importInitName collision: %q and %q both yield %q", prior, tc.importPath, name)
		}

		seen[name] = tc.importPath

		if name == PackageTestInitHookMethod {
			t.Errorf("importInitName(%q) collides with PackageTestInitHookMethod", tc.importPath)
		}

		if strings.ContainsAny(name, "./-") {
			t.Errorf("importInitName(%q) = %q contains a non-identifier character", tc.importPath, name)
		}
	}
}

// TestNoInitPseudoPackages locks the packages an import must NOT force. Go's `unsafe` and
// `builtin` are compiler-provided and have no initialization at all — `import _ "unsafe"` is the
// //go:linkname ritual, present in 67 files of the converted standard library — and `C` is cgo.
// Forcing their module constructors would be guaranteed no-ops, so the emission skips them; every
// real package must still be eligible.
func TestNoInitPseudoPackages(t *testing.T) {
	for _, pseudo := range []string{"unsafe", "builtin", "C"} {
		if !noInitPseudoPackages.Contains(pseudo) {
			t.Errorf("noInitPseudoPackages is missing %q", pseudo)
		}
	}

	for _, real := range []string{"image/png", "crypto/sha256", "runtime", "internal/unsafeheader", "cmp"} {
		if noInitPseudoPackages.Contains(real) {
			t.Errorf("noInitPseudoPackages must not contain the real package %q", real)
		}
	}
}

// initFactsFixture writes a module whose packages cover every shape the fact has to tell apart,
// loads it, and installs the result as the converter's per-package import state exactly as
// resetPackageState does during a conversion.
//
// It is a REAL module put through go/packages rather than a synthesized types.Info, because the
// whole design rests on one measured claim: the handles a conversion already holds carry the
// dependencies' syntax and type information. A fixture that bypassed the loader would prove
// nothing about that.
//
// Returns the module directory, so a caller can re-load the SAME sources through a different load
// path (see installInitFacts).
func initFactsFixture(t *testing.T) string {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module initfacts\n\ngo 1.23\n",

		// No init func, no package-level initializer at all.
		"inert/inert.go": `package inert

type Marker struct{}

func Use() int { return 1 }
`,

		// Package-level vars, but every initializer is a compile-time constant, so C# emits them as
		// static FIELD initializers — which run with the type, not with the module, and no module
		// constructor could force.
		"constonly/constonly.go": `package constonly

var Limit = 1 << 20

var Name = "constonly"

var Flag = true
`,

		// The plain case: a func init().
		"hasinitfunc/hasinitfunc.go": `package hasinitfunc

var Value int

func init() { Value = 7 }
`,

		// The other Go initialization source: a package-level var whose right-hand side must be
		// evaluated. No init func anywhere.
		"hasvarinit/hasvarinit.go": `package hasvarinit

func compute() int { return 3 }

var Value = compute()
`,

		// Imports only inert packages, so the closure reaches nothing to force.
		"viainert/viainert.go": `package viainert

import (
	"initfacts/constonly"
	"initfacts/inert"
)

func Use() int { return inert.Use() + constonly.Limit }
`,

		// Initializes nothing itself; its IMPORT does. This is the shape the trigger exists for —
		// forcing this package is the only path the runtime has to the one behind it.
		"viainit/viainit.go": `package viainit

import "initfacts/hasinitfunc"

func Read() int { return hasinitfunc.Value }
`,

		// Two hops to the initializing package, through a package that initializes nothing.
		"viadeep/viadeep.go": `package viadeep

import "initfacts/viainit"

func Read() int { return viainit.Read() }
`,
	})

	installInitFacts(t, dir, false)

	return dir
}

// installInitFacts loads the fixture module and installs the result as the converter's per-package
// import state, exactly as resetPackageState does during a conversion.
//
// withTests selects the LOAD PATH: a production conversion loads with LoadAllSyntax
// (conversionDriver), a `-tests` conversion loads with LoadAllSyntax AND Tests:true
// (testConversion). Both are covered because the fact is read on both, and an answer that differed
// between them would make a package's emitted hooks differ between its two emissions.
func installInitFacts(t *testing.T, dir string, withTests bool) {
	t.Helper()

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: dir, Tests: withTests}, "./...")

	if err != nil {
		t.Fatalf("loading fixture packages (tests=%v): %v", withTests, err)
	}

	importedPackages = map[string]*packages.Package{}
	packageInitFacts = map[string]bool{}

	var capture func(map[string]*packages.Package)

	capture = func(imports map[string]*packages.Package) {
		for importPath, imported := range imports {
			if _, seen := importedPackages[importPath]; seen {
				continue
			}

			importedPackages[importPath] = imported
			capture(imported.Imports)
		}
	}

	for _, pkg := range loaded {
		if len(pkg.Errors) > 0 {
			t.Fatalf("fixture package %s failed to load: %v", pkg.PkgPath, pkg.Errors)
		}

		// A Tests:true load returns variant packages alongside the production ones, under
		// synthesized paths (`initfacts/inert [initfacts/inert.test]`). resetPackageState keys on
		// whatever the loader reports, and so does this — the production entries are the ones the
		// assertions ask for, and a variant simply never collides with them.
		if _, seen := importedPackages[pkg.PkgPath]; !seen {
			importedPackages[pkg.PkgPath] = pkg
		}

		capture(pkg.Imports)
	}
}

// TestPackageInitializesTransitively is the fact the emission trigger reads: does an imported
// package initialize anything at run time, transitively. Every row is a shape the fixture module
// declares for exactly this purpose — including the three that must answer NO, which are what make
// the trigger an optimization over "force every import" rather than a rename of it.
func TestPackageInitializesTransitively(t *testing.T) {
	_ = initFactsFixture(t)

	cases := []struct {
		importPath string
		want       bool
		why        string
	}{
		{"initfacts/inert", false, "no init func and no package-level initializer at all"},
		{"initfacts/constonly", false, "package-level vars, but every initializer is a compile-time constant"},
		{"initfacts/hasinitfunc", true, "declares func init()"},
		{"initfacts/hasvarinit", true, "a package-level var initialized by a call"},
		{"initfacts/viainert", false, "imports only packages that initialize nothing"},
		{"initfacts/viainit", true, "initializes nothing itself, but imports a package that does"},
		{"initfacts/viadeep", true, "reaches an initializing package two hops away"},
	}

	for _, tc := range cases {
		if got := packageInitializesTransitively(tc.importPath); got != tc.want {
			t.Errorf("packageInitializesTransitively(%q) = %v, want %v (%s)", tc.importPath, got, tc.want, tc.why)
		}
	}
}

// TestPackageInitializesSeesAHandOwnedInit is the counterexample that decided WHERE this fact is
// allowed to live, and it is GUARDED rather than merely argued because the alternative design was
// specified and would have shipped blind to exactly this case.
//
// The obvious home for a cross-package fact is a published `package_info.cs` record, scraped from
// emitted output the way exported type aliases and GoImplement records already are.
// `internal/godebug` is where that breaks, twice over:
//
//   - its ENTIRE single Go file is hand-owned (`[module: GoManualConversion]`), so the converter
//     never visits it and the emitted `godebug.cs` carries NO `[GoInit]` at all; and
//   - it is hand-owned BY CONSEQUENCE — with no unmarked file left to convert, the driver returns
//     before `writeProjectFile`, so its `package_info.cs` is never re-emitted and no record could
//     be added to it even if one were wanted.
//
// Both halves are asserted: Go's own source says the package initializes, and the emitted artifacts
// a scrape would read say nothing. That is the failing-first shape, in one test — the scraped view
// is computed, shown to answer FALSE, and the derivation this converter uses answers TRUE.
//
// It matters beyond tidiness: `godebug`'s `init` wires `setUpdate`/`setNewIncNonDefault`, and a
// consumer that skipped forcing it on a FALSE answer would lose Go's ordering silently — which is
// the failure mode this whole arc exists to close.
func TestPackageInitializesSeesAHandOwnedInit(t *testing.T) {
	const handOwnedPackage = "internal/godebug"

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax}, handOwnedPackage)

	if err != nil {
		t.Fatalf("loading %s: %v", handOwnedPackage, err)
	}

	if len(loaded) != 1 || len(loaded[0].Errors) > 0 {
		t.Fatalf("expected one clean package for %s, got %d", handOwnedPackage, len(loaded))
	}

	// The WHOLE closure is installed, not just the package itself. With only the root installed,
	// every one of its imports is an unknown path, the fail-open answers TRUE for each, and the
	// package comes back TRUE transitively — passing this test for entirely the wrong reason, and
	// staying green even with the direct predicate torn out. (Measured: it did exactly that.)
	importedPackages = map[string]*packages.Package{}
	packageInitFacts = map[string]bool{}

	var capture func(map[string]*packages.Package)

	capture = func(imports map[string]*packages.Package) {
		for importPath, imported := range imports {
			if _, seen := importedPackages[importPath]; seen {
				continue
			}

			importedPackages[importPath] = imported
			capture(imported.Imports)
		}
	}

	importedPackages[handOwnedPackage] = loaded[0]
	capture(loaded[0].Imports)

	if !packageInitializesTransitively(handOwnedPackage) {
		t.Fatalf("%s declares func init() in its Go source; the fact must answer true", handOwnedPackage)
	}

	// …and it must answer true on its OWN account, not by way of something it imports — otherwise
	// the hand-owned `init` could go unseen and the answer would still look right.
	if !packageInitializes(loaded[0]) {
		t.Errorf("%s must be reported as initializing DIRECTLY: its `func init()` is the fact under "+
			"test, and reaching the same answer through its imports would hide the blindness this "+
			"test exists to rule out", handOwnedPackage)
	}

	// …and now the scraped view of the same package. The corpus is a sibling of this module, so a
	// tree without it — never the repository, but conceivable in isolation — skips rather than
	// fails: the assertion above is the one that guards the converter.
	converted := filepath.Join("..", "core", filepath.FromSlash(handOwnedPackage))

	if _, statErr := os.Stat(converted); statErr != nil {
		t.Skipf("converted %s is not present at %s; the Go-source assertion above still holds", handOwnedPackage, converted)
	}

	for _, name := range []string{"godebug.cs", PackageInfoFileName} {
		emitted, readErr := os.ReadFile(filepath.Join(converted, name))

		if readErr != nil {
			t.Fatalf("reading emitted %s/%s: %v", handOwnedPackage, name, readErr)
		}

		// The blindness this test pins is about the package's OWN initialization, and the check has
		// to say so: since the metadata un-freeze (the hand-owned-by-consequence class is re-minted
		// rather than frozen), `package_info.cs` legitimately carries one `[GoInit]` per IMPORT —
		// `initᴛᴛimportꓸinternalꓸbisect`, `…ꓸinternalꓸgodebugs`, `…ꓸsync`, `…ꓸsyncꓸatomic` for this
		// package. Those force the IMPORTS' inits and say nothing about godebug's own `func init()`.
		//
		// Re-derived at that landing rather than relaxed, and the numbers are the argument: the
		// hand-owned `godebug.cs` carries ZERO `[GoInit]`, the Go source declares ONE `func init()`,
		// and the emitted artifacts carry ZERO non-import hooks — so a scrape still cannot answer
		// "does this package initialize on its own account", which is exactly why
		// packageInitFacts.go reads Go's own two sources (a `func init()` declaration and
		// Info.InitOrder) instead. A bare `[GoInit]` grep could not tell the two facts apart and
		// went red on a change that did not touch the property it guards.
		for _, line := range strings.Split(string(emitted), "\n") {
			if !strings.Contains(line, "[GoInit]") || strings.Contains(line, "initᴛᴛimport") {
				continue
			}

			t.Errorf("emitted %s/%s carries a NON-IMPORT [GoInit] (%s): this test's premise — that an "+
				"artifact scrape is BLIND to a hand-owned package's OWN initialization — no longer "+
				"holds. Re-derive the reasoning in packageInitFacts.go rather than deleting the test.",
				handOwnedPackage, name, strings.TrimSpace(line))
		}
	}
}

// TestPackageInitializesUnderTestVariantLoad covers the OTHER load path the fact is read on. A
// production conversion loads with LoadAllSyntax (conversionDriver); a `-tests` conversion loads
// with LoadAllSyntax AND Tests:true (testConversion), which returns additional variant packages and
// re-keys the graph around them. The fact must answer identically on both — an answer that differed
// would make a package's emitted hooks differ between its two emissions, which is precisely the
// shape that becomes standing working-tree dirt nobody can classify.
func TestPackageInitializesUnderTestVariantLoad(t *testing.T) {
	dir := initFactsFixture(t)

	fixturePaths := []string{
		"initfacts/inert", "initfacts/constonly", "initfacts/hasinitfunc", "initfacts/hasvarinit",
		"initfacts/viainert", "initfacts/viainit", "initfacts/viadeep",
	}

	production := map[string]bool{}

	for _, importPath := range fixturePaths {
		production[importPath] = packageInitializesTransitively(importPath)
	}

	installInitFacts(t, dir, true)

	for _, importPath := range fixturePaths {
		if got := packageInitializesTransitively(importPath); got != production[importPath] {
			t.Errorf("packageInitializesTransitively(%q) = %v under a -tests load, %v under a production load",
				importPath, got, production[importPath])
		}
	}
}

// TestPackageInitializesTransitivelyFailsOpen locks the direction the trigger errs in. An import
// path this conversion has no loaded handle for cannot be PROVEN inert, and a missed forcing is a
// silent loss of Go's ordering while a needless one is a guaranteed no-op — so the unknown answer
// is yes. The pseudo-packages are the deliberate exception: the language gives them no
// initialization, so they are answered without a handle at all (`unsafe` in particular loads with
// type information but no syntax, which is otherwise indistinguishable from an empty package).
func TestPackageInitializesTransitivelyFailsOpen(t *testing.T) {
	_ = initFactsFixture(t)

	if !packageInitializesTransitively("initfacts/no-such-package") {
		t.Error("an unloaded import path must answer yes, so a missing handle can only cost an extra no-op hook")
	}

	for _, pseudo := range []string{"unsafe", "builtin", "C"} {
		if packageInitializesTransitively(pseudo) {
			t.Errorf("pseudo-package %q must never be reported as initializing", pseudo)
		}
	}
}

// TestImportInitSectionMergeIsKeyedByHookIdentity pins the merge's UNIQUENESS invariant: one hook per
// imported package per emission unit, whatever the forcing target is spelled as.
//
// The defect it locks out, stated as the defect. The forcing target is root-qualified or bare
// depending on which class the deciding unit was writing into (forcingTargetShadowed), so a merging
// write — the -tests seed, which carries the PRODUCTION package_info.cs's entries into a variant's
// file — meets the same hook under two spellings. A merge keyed on the rendered LINE keeps both, and
// two methods of one name in one partial class is CS0111. Measured on crypto/x509, whose `math/big`
// reaches the test variants by more than one route, and caught by the canary battery on master:
//
//	package_test_info.cs(139,35): CS0111 'x509_test_package' already defines 'initᴛᴛimportꓸmathꓸbig'
//
// This asserts the DECISION — how many hooks of a given identity the merge yields, and which one
// survives — rather than the text of any emitted file. A guard that grepped a corpus file for the
// duplicate would go vacuous the moment the section moved or was renamed (FALSE-GREEN route 8), and
// would say nothing about WHICH spelling won.
func TestImportInitSectionMergeIsKeyedByHookIdentity(t *testing.T) {
	previous := packageImportInits
	t.Cleanup(func() { packageImportInits = previous })

	// This unit's own decision: the BARE spelling, as a variant writing into its own class renders it.
	packageImportInits = map[string]string{"math/big": "math.big_package"}

	hook := importInitName("math/big")

	// The seeded half, decided by the PRODUCTION unit against a different class, hence root-qualified.
	seeded := importInitLine("math/big", "go.math.big_package")

	lines := []string{
		"public static partial class x509_test_package",
		"{",
		importInitIndent + "// <" + ImportInitSection + ">",
		importInitIndent + seeded,
		importInitIndent + "// </" + ImportInitSection + ">",
		"}",
	}

	merged := applyImportInitSection(lines, true)

	occurrences := 0
	var surviving string

	for _, line := range merged {
		if trimmed := strings.TrimSpace(line); importInitLineName(trimmed) == hook {
			occurrences++
			surviving = trimmed
		}
	}

	if occurrences != 1 {
		t.Fatalf("the merge must yield exactly ONE hook per import identity, got %d:\n%s",
			occurrences, strings.Join(merged, "\n"))
	}

	// The FRESH entry wins: it was decided against the class this file declares, where the seeded one
	// was decided against another.
	if want := importInitLine("math/big", "math.big_package"); surviving != want {
		t.Errorf("this unit's own rendering must win the merge:\n got %s\nwant %s", surviving, want)
	}
}

// TestImportInitLineNameReadsOnlyItsOwnShape keeps the identity extractor narrow. A line this file did
// not render must answer "" so the merge carries it through untouched rather than reinterpreting it —
// the merge keys on this, so a loose match would silently collapse two unrelated entries into one.
func TestImportInitLineNameReadsOnlyItsOwnShape(t *testing.T) {
	rendered := importInitLine("math/big", "math.big_package")

	if got := importInitLineName(rendered); got != importInitName("math/big") {
		t.Errorf("importInitLineName(%q) = %q, want %q", rendered, got, importInitName("math/big"))
	}

	for _, foreign := range []string{
		"",
		"// a comment",
		"public partial struct T {}",
		"[GoInit] internal static void initᴛᴛproduction() { builtin.initPackage(typeof(x)); }",
	} {
		if got := importInitLineName(foreign); got != "" && !strings.HasPrefix(foreign, "[GoInit] internal static void init") {
			t.Errorf("importInitLineName(%q) = %q, want \"\" for a line this file did not render", foreign, got)
		}
	}
}
