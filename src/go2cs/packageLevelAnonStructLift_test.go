// packageLevelAnonStructLift_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the two converter rules internal/reflectlite's test suite was the first thing in the
// corpus to reach, behind the local-value interface-cast fix:
//
//  1. Structurally IDENTICAL anonymous struct types are ONE Go type at PACKAGE level exactly as
//     inside a function. Splitting them per declaration makes the C# types un-unifiable where Go
//     unifies freely — `append(assignableTests, implementsTests...)` could not type (CS9244 +
//     CS8130 ×2 on the range deconstruction).
//
//  2. On the whitebox `-tests` variant, a PRODUCTION-declared type is not a LOCAL operand for a
//     GoImplicitConv record: its C# lives in the closed referenced production assembly, which the
//     generator cannot extend with an operator, so recording the pair declares a PHANTOM partial
//     in the test class whose members do not exist (CS1061).

package main

import (
	"go/build"
	"go/types"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestPackageLevelAnonStructDedup pins that two package-level vars written over one anonymous
// struct type lift to a SINGLE C# type, that a structurally DIFFERENT anonymous struct keeps its
// own lift, and that a function-local identical struct stays in its own scope.
func TestPackageLevelAnonStructDedup(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/anondedup\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

// The reflectlite shape: two package vars over ONE written anonymous struct type. Go's structural
// identity makes them one type, so the append below is legal Go and the emission must unify.
var implementsTests = []struct {
	x any
	b bool
}{
	{1, true},
}

var assignableTests = []struct {
	x any
	b bool
}{
	{2, false},
}

// A structurally DIFFERENT anonymous struct must keep its own lifted type.
var otherTests = []struct {
	y int
}{
	{3},
}

func main() {
	// The un-unifiable emission: both operands must be one C# type or neither T nor the
	// deconstruction infers.
	for i, tt := range append(assignableTests, implementsTests...) {
		fmt.Println(i, tt.x, tt.b)
	}

	// Scope separation: a function-local identical struct does NOT unify with the package-level
	// lift (the function-scoped dedup rule keys per function, unchanged).
	local := []struct {
		x any
		b bool
	}{
		{9, true},
	}
	fmt.Println(local[0].x, otherTests[0].y)
}
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "anondedup", "main.cs"))

	// ONE lifted declaration for the shared signature: the first declaration's name wins and the
	// second declaration re-uses it (declared once, referenced twice).
	if got := strings.Count(mainCs, "partial struct implementsTests"+TempVarMarker+"1"); got != 1 {
		t.Errorf("the shared anonymous struct must be DECLARED exactly once, got %d:\n%s", got, mainCs)
	}

	if strings.Contains(mainCs, "partial struct assignableTests"+TempVarMarker+"1") {
		t.Errorf("the second var must not lift its own type — its struct is the FIRST var's type:\n%s", mainCs)
	}

	// Both vars type as slices of the ONE lifted type.
	if got := strings.Count(mainCs, "slice<implementsTests"+TempVarMarker+"1>"); got < 2 {
		t.Errorf("both package vars must type by the one lifted struct, found %d references:\n%s", got, mainCs)
	}

	// The structurally different struct keeps its own lift.
	if got := strings.Count(mainCs, "partial struct otherTests"+TempVarMarker+"1"); got != 1 {
		t.Errorf("a structurally DIFFERENT anonymous struct must keep its own lifted type, got %d:\n%s", got, mainCs)
	}

	// The function-local identical struct ADOPTS the package-level lift rather than minting a
	// function-scoped one: Go's struct identity is structural (tags included), so the local
	// composite literal IS the package-level type, and the adoption rule (see the xml `Child_G`
	// entry in ConversionStrategies-Reference) makes the emitted C# unify exactly as Go does.
	// This assertion originally pinned the opposite (a function-scoped lift) — that pin was about
	// the dedup REGISTRY's scope keying, not observable semantics, and the union of the two
	// 2026-08-18 lanes resolved it in favor of Go's structural identity: coordinator ruling at
	// the merge of claude/local-iface-cast x claude/escape-box-copy.
	if strings.Contains(mainCs, "partial struct main_local") {
		t.Errorf("a function-local identical struct must ADOPT the package-level lift, not mint its own:\n%s", mainCs)
	}
	if got := strings.Count(mainCs, "partial struct implementsTests"+TempVarMarker+"1"); got != 1 {
		t.Errorf("adoption must not duplicate the package-level declaration, got %d:\n%s", got, mainCs)
	}

	// ...and the local composite literal builds the adopted package-level struct (the two
	// package vars plus the local = three constructions; the local is `var`-typed so the
	// slice<T> spelling itself appears only on the package vars).
	if got := strings.Count(mainCs, "new implementsTests"+TempVarMarker+"1[]"); got != 3 {
		t.Errorf("the function-local composite must construct the adopted package-level struct, found %d constructions:\n%s", got, mainCs)
	}
}

// TestWhiteboxProductionNumericConvNotRecorded pins that a numeric GoImplicitConv pair whose
// operands are PRODUCTION-declared records nothing on the whitebox internal variant — and that a
// TEST-declared operand still records (the relocatable case recordsRequireProductionMutation's
// rule text names).
func TestWhiteboxProductionNumericConvNotRecorded(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads and converts a test-variant fixture")
	}

	dir := t.TempDir()
	files := map[string]string{
		"go.mod": "module example/wbnum\n\ngo 1.23\n",
		"value.go": "package wbnum\n\ntype flag uintptr\n\ntype Kind uint8\n\n" +
			"func Use(f flag, k Kind) bool { return uintptr(f) == uintptr(k) }\n",
		"export_test.go": "package wbnum\n\ntype testCounter uint32\n\n" +
			"func FlagOf(k Kind) flag { return flag(k) }\n\n" +
			"func CounterOf(k Kind) testCounter { return testCounter(k) }\n",
	}

	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}

	internal, _ := loadBothTestVariantsForDir(t, dir)

	if internal == nil {
		t.Fatal("the internal test variant was not loaded")
	}

	outputPath := t.TempDir()
	options := Options{indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}

	// What convertTestVariants sets for the internal variant under the white-box REFERENCE model —
	// the arm where production C# is a closed referenced assembly.
	options.testClassNameOverride = getSanitizedImport("wbnum_internal_test" + PackageSuffix)
	options.testWhiteboxReference = true
	options.testInlineTypeAccess = true
	options.testProductionPath = "example/wbnum"

	testMethodRenames = make(map[types.Object]bool)
	t.Cleanup(func() { testMethodRenames = nil })

	if _, _, err := convertTestVariant(internal, testFileEntries(internal), outputPath, "go", productionSeed{}, options); err != nil {
		t.Fatal(err)
	}

	// The production×production pair (flag ← Kind) must NOT be recorded: the generator cannot host
	// an operator on either closed type, and the cast site's explicit chain needs none.
	for source, targets := range numericConversions {
		for target := range targets {
			if strings.Contains(source+"|"+target, "flag") {
				t.Errorf("a whitebox-production numeric pair must not record (phantom partial, CS1061): %s -> %s", source, target)
			}
		}
	}

	// The TEST-declared operand still records — the relocatable case: the operator hosts on the
	// test-local type.
	found := false

	for source, targets := range numericConversions {
		for target := range targets {
			if strings.Contains(source, "testCounter") || strings.Contains(target, "testCounter") {
				found = true
			}
		}
	}

	if !found {
		t.Errorf("a numeric pair with a TEST-LOCAL operand must still record (it relocates there); got %v", numericConversions)
	}
}
// TestPointerBoxConversionRecordShape pins the ONE definition of the pointer-boxing route both
// conversionRecordHasLocalOperand and recordsRequireProductionMutation read. The `global::` trim is
// load-bearing: a whitebox-production operand renders class-qualified while the box wraps that same
// qualified spelling, and an ordinary local operand renders bare.
func TestPointerBoxConversionRecordShape(t *testing.T) {
	box := func(inner string) string { return PointerPrefix + "<" + inner + ">" }

	for _, pair := range []struct {
		source, target string
		want           bool
	}{
		{"Cipher", box("Cipher"), true},
		{"global::go.crypto.rc4_package.Cipher", box("global::go.crypto.rc4_package.Cipher"), true},
		{"Cipher", box("global::go.crypto.rc4_package.Cipher"), false},
		{"global::go.crypto.rc4_package.Cipher", box("go.crypto.rc4_package.Cipher"), true},
		{"Cipher", box("Other"), false},
		{"Cipher", "Cipher", false},
		{"Cipher", box("Cipher") + "extra", false},
	} {
		if got := pointerBoxConversionRecord(pair.source, pair.target); got != pair.want {
			t.Errorf("pointerBoxConversionRecord(%q, %q) = %v, want %v", pair.source, pair.target, got, pair.want)
		}
	}
}

// TestWhiteboxProductionPointerBoxConvStillRecorded is the numeric guard's counterpart, and the two
// together are the whole rule: a whitebox-production operand is declined when an operator WOULD be
// hosted on it, and admitted when none can be.
//
// The pointer-boxing record `T` → `ж<T>` hosts nothing: `ж<T>` is golib's generic box and no
// converted package declares it, so ImplicitConvGenerator finds no struct declaration and skips
// the record before choosing a host — there is no phantom to mint and no closed production assembly
// to extend. Excluding it anyway (2026-08-18, e61758549, whose rationale was the NUMERIC phantom)
// silently shrank every whitebox package's committed package_test_info.cs on regen: crypto/rc4 lost
// its Cipher record AND the `testing` qualifier alias that rides the same site, and go/types lost
// three. Neither is caught by CNR, which never runs `-tests`.
//
// The BOTH-FOREIGN arm is the boundary this must not cross: those pairs were declined before the
// whitebox exclusion existed (os's syscall.Handle precedent) and stay declined, so the fix restores
// exactly what e61758549 removed and adds nothing.
func TestWhiteboxProductionPointerBoxConvStillRecorded(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads and converts a test-variant fixture")
	}

	dir := t.TempDir()
	files := map[string]string{
		"go.mod": "module example/wbbox\n\ngo 1.23\n",
		"value.go": "package wbbox\n\ntype Cipher struct{ s uint32 }\n\n" +
			"func New() *Cipher { return &Cipher{} }\n",
		"wbbox_test.go": "package wbbox\n\nimport (\n\t\"bytes\"\n\t\"testing\"\n)\n\n" +
			"func check(t *testing.T, c *Cipher) {\n\tif c == nil {\n\t\tt.Fatal(\"nil\")\n\t}\n}\n\n" +
			"func pair(t *testing.T, b *bytes.Buffer) {\n\tif b == nil {\n\t\tt.Fatal(\"nil\")\n\t}\n}\n\n" +
			"func TestNew(t *testing.T) {\n\tcheck(t, New())\n\tpair(t, new(bytes.Buffer))\n}\n",
	}

	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}

	internal, _ := loadBothTestVariantsForDir(t, dir)

	if internal == nil {
		t.Fatal("the internal test variant was not loaded")
	}

	outputPath := t.TempDir()
	options := Options{indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}

	// What convertTestVariants sets for the internal variant under the white-box REFERENCE model —
	// the arm where production C# is a closed referenced assembly.
	options.testClassNameOverride = getSanitizedImport("wbbox_internal_test" + PackageSuffix)
	options.testWhiteboxReference = true
	options.testInlineTypeAccess = true
	options.testProductionPath = "example/wbbox"

	testMethodRenames = make(map[types.Object]bool)
	t.Cleanup(func() { testMethodRenames = nil })

	if _, _, err := convertTestVariant(internal, testFileEntries(internal), outputPath, "go", productionSeed{}, options); err != nil {
		t.Fatal(err)
	}

	boxedRecord := func(simpleName string) bool {
		for source, targets := range indirectImplicitConversions {
			for target := range targets {
				if pointerBoxConversionRecord(source, target) && strings.HasSuffix(source, simpleName) {
					return true
				}
			}
		}

		return false
	}

	// The whitebox-production operand: recorded, exactly as a plain (non--tests) conversion of the
	// same package records it.
	if !boxedRecord("Cipher") {
		t.Errorf("the whitebox-production pointer-box record was dropped; got %v", indirectImplicitConversions)
	}

	// ...and so is the import qualifier the same site registers, which is what carries the
	// `using testing = …` line into package_test_info.cs.
	if _, ok := conversionPackageUsings["testing"]; !ok {
		t.Errorf("the record site's package qualifier was not registered; got %v", conversionPackageUsings)
	}

	// The boundary: NEITHER operand local (both live in other assemblies) stays declined.
	if boxedRecord("Buffer") {
		t.Errorf("a both-foreign pointer-box pair must stay declined; got %v", indirectImplicitConversions)
	}
}

// TestPointerBoxRecordDoesNotForceRecompile guards the MODEL-SELECTION half of the pointer-boxing
// rule, which is a different question from the RECORDING half its two neighbours above pin. A
// record can be perfectly legal to keep (the generator hosts nothing, so nothing is mutated) and
// still, if recordsRequireProductionMutation counts it as production-anchored, push the whole suite
// off the white-box reference model onto the recompile fallback. That fallback compiles a SECOND
// copy of every production type into the test assembly, so any referenced assembly that returns the
// production copy no longer type-matches: an identity split, which no compile-set or reference
// adjustment can repair.
//
// `crypto/x509` is the corpus's measured case. Its ONE production-typed record is
// `Certificate -> ж<Certificate>` in implicitConversions — the exact shape the indirect map had
// exempted since e61758549 — and the recompile fallback it forced made `hybrid_pool_test.go`'s
// `c.ConnectionState().PeerCertificates` (a crypto/tls return, i.e. the PRODUCTION Certificate)
// unassignable to the recompiled one: CS0012 x4 + CS1929 x2.
//
// Both direct maps are pinned, in both orientations, because the two disagree about which side
// holds the box: a conversion site records argument-first, while invertedImplicitConversions is
// keyed by the interface parameter and emits its attribute with the arguments swapped.
//
// The control case is the point of the test: a structural pair between two PRODUCTION types must
// still force the fallback, or the guard would pass just as well with the predicate stubbed to
// `return false`.
func TestPointerBoxRecordDoesNotForceRecompile(t *testing.T) {
	const productionClass = "x509_package"
	const productionPackage = "x509"

	cert := "global::go.crypto." + productionClass + ".Certificate"
	certBox := PointerPrefix + "<" + cert + ">"
	opts := "global::go.crypto." + productionClass + ".VerifyOptions"

	saveImplicit, saveInverted := implicitConversions, invertedImplicitConversions
	saveIndirect := indirectImplicitConversions
	saveNumeric, saveIndirectNumeric := numericConversions, indirectNumericConversions

	t.Cleanup(func() {
		implicitConversions, invertedImplicitConversions = saveImplicit, saveInverted
		indirectImplicitConversions = saveIndirect
		numericConversions, indirectNumericConversions = saveNumeric, saveIndirectNumeric
	})

	for _, testCase := range []struct {
		name           string
		source, target string
		into           func(map[string]HashSet[string])
		want           bool
	}{
		{"implicit box on target", cert, certBox, func(m map[string]HashSet[string]) { implicitConversions = m }, false},
		{"implicit box on source", certBox, cert, func(m map[string]HashSet[string]) { implicitConversions = m }, false},
		{"inverted box on target", cert, certBox, func(m map[string]HashSet[string]) { invertedImplicitConversions = m }, false},
		{"inverted box on source", certBox, cert, func(m map[string]HashSet[string]) { invertedImplicitConversions = m }, false},
		{"indirect box on target", cert, certBox, func(m map[string]HashSet[string]) { indirectImplicitConversions = m }, false},

		// The control: a production x production STRUCTURAL pair is not the boxing route, the
		// generator really would host an operator on a closed type, and the fallback must fire.
		{"structural production pair still falls back", cert, opts, func(m map[string]HashSet[string]) { implicitConversions = m }, true},
	} {
		t.Run(testCase.name, func(t *testing.T) {
			implicitConversions = make(map[string]HashSet[string])
			invertedImplicitConversions = make(map[string]HashSet[string])
			indirectImplicitConversions = make(map[string]HashSet[string])
			numericConversions = make(map[string]map[string]string)
			indirectNumericConversions = make(map[string]map[string]string)

			seeded := map[string]HashSet[string]{testCase.source: NewHashSet([]string{testCase.target})}
			testCase.into(seeded)

			if got := recordsRequireProductionMutation(productionClass, productionPackage); got != testCase.want {
				t.Errorf("recordsRequireProductionMutation with %s -> %s = %v, want %v",
					testCase.source, testCase.target, got, testCase.want)
			}
		})
	}
}
