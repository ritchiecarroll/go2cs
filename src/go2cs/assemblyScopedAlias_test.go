// assemblyScopedAlias_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the six emission rules that stood between `encoding/xml` and any verdict at all, and
// between `net/netip` and its last two. The board handed these on as ONE shared root — the
// assembly-scoped-alias CS0426, "measured in both and identical in shape" — and the diagnostic is
// identical while the root is not: rules 1 and 2 below are two different aliases minted by two
// different mechanisms, and neither fix reaches the other package.
//
//  1. A package-level type the PRODUCTION conversion emits as a compilation-scoped `global using`
//     alias is not a member of `<pkg>_package`, so a `-tests` conversion under a reference model
//     may not qualify it through that class (CS0426 ×36, `encoding/xml`).
//
//  2. The same for an alias a `_test.go` file declares — reached from the OTHER variant class, which
//     Go spells through the package (CS0426, `net/netip`).
//
//  3. Structurally identical anonymous structs are ONE Go type. A function-local lift of one the
//     package has already lifted must adopt the package-level name rather than mint a second C#
//     type, or Go's own assignment between them is CS1503 (×6).
//
//  4. An EXPORTED test declaration over an UNEXPORTED production type must be downgraded to
//     `internal` — through a generic type argument and an alias and a signature, not only through a
//     pointer (CS0050/CS0051/CS0052 ×7).
//
//  5. Go's built-in `comparable`, EMBEDDED in an interface, is no more expressible than a bare one:
//     it is not a C# base and it does not make an otherwise-method-set interface generic
//     (CS0305 + CS0308).
//
//  6. A concrete value in a TYPE-PARAMETER slot records against the parameter's CONSTRAINT and emits
//     unchanged — the record and the emission move in opposite directions (CS0246 ×3 + CS8785, then
//     CS1503 ×5).

package main

import (
	"go/build"
	"go/types"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// TestSeedProductionInterfaceAliasesFollowsPublishedChain pins the seeding of a DEFINED-OVER-
// INTERFACE production type — the second kind of package-level declaration that has a `global using`
// and no class member.
//
// `type Token any` is a defined type with its own identity, but it has exactly the empty interface's
// method set and can carry no methods of its own, so visitTypeSpec emits `global using ΔToken =
// object;`. A reference-model test project is a second compilation that declares no such alias, and
// its renderers reached the type as an ordinary production named type —
// `global::go.encoding.xml_package.ΔToken`, which qualifies an assembly-scoped alias as a type
// member. Both halves must be seeded together: the TYPE so every renderer spells the alias, and the
// alias TARGET so the name resolves where it lands.
func TestSeedProductionInterfaceAliasesFollowsPublishedChain(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads a module fixture through go/packages")
	}

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/ifacealias\n\ngo 1.23\n",
		// Token's shape, collision and all: the TYPE `Token` shares its name with a METHOD, so the
		// type is Δ-renamed and the alias the production compilation declares is `ΔToken` — two
		// published records, not one. Stream is the uncollided one-hop form over a NAMED interface.
		// Reader is the negative control: an INLINE interface definition is a real member of the
		// package class, and only the right-hand SYNTAX separates it from Token.
		"value.go": "package ifacealias\n\n" +
			"type Token any\n\n" +
			"type Reader interface{ Read() int }\n\n" +
			"type Stream Reader\n\n" +
			"type Decoder struct{ n int }\n\n" +
			"func (d *Decoder) Token() (Token, error) { return nil, nil }\n",
		// A test file's own defined-over-interface type emits its `global using` into THIS
		// compilation and must not be seeded from production metadata.
		"value_test.go": "package ifacealias\n\n" +
			"type localToken any\n\n" +
			"func helper(v localToken) bool { return v == nil }\n",
	})

	infoPath := filepath.Join(dir, "package_info.cs")
	info := "// <ExportedTypeAliases>\r\n" +
		"[assembly: GoTypeAlias(\"Token\", \"ΔToken\")]\r\n" +
		"[assembly: GoTypeAlias(\"ΔToken\", \"object\")]\r\n" +
		"[assembly: GoTypeAlias(\"Stream\", \"go.example.ifacealias_package.Reader\")]\r\n" +
		// Deliberately published for the inline-interface type too, so the negative control tests
		// the AST predicate rather than the publication.
		"[assembly: GoTypeAlias(\"Reader\", \"go.example.ifacealias_package.Reader\")]\r\n" +
		"[assembly: GoTypeAlias(\"localToken\", \"object\")]\r\n" +
		"// </ExportedTypeAliases>\r\n"

	if err := os.WriteFile(infoPath, []byte(info), 0644); err != nil {
		t.Fatal(err)
	}

	internal, _ := loadTestVariantsForDir(t, dir)

	if internal == nil {
		t.Fatal("fixture must load the internal test variant")
	}

	scope := internal.Types.Scope()

	// The AST predicate itself, before the seeding that consumes it: exactly the two named-RHS
	// declarations, and never the inline interface definition beside them.
	names := definedOverInterfaceTypeNames(internal)

	if got, want := strings.Join(names, ","), "Token,Stream"; got != want {
		t.Fatalf("definedOverInterfaceTypeNames must select the named-RHS declarations only\n got: %s\nwant: %s", got, want)
	}

	resetPackageState(&packages.Package{})
	packageNamespace = "go"

	// The RECOMPILE model compiles the production `.cs` into the test assembly, so the alias is
	// already declared there. Seeding is gated on the models that REFERENCE production, and an
	// empty testProductionPath is what says "recompile" — assert the gate before the behavior.
	seedProductionInterfaceAliases(internal, infoPath, Options{})

	if len(productionAliasLiftedTypes) != 0 || len(importedTypeAliases) != 0 {
		t.Fatalf("the recompile model must seed nothing: %v / %v", productionAliasLiftedTypes, importedTypeAliases)
	}

	seedProductionInterfaceAliases(internal, infoPath, Options{testProductionPath: "example/ifacealias"})

	// The chain, followed to its end: the name to spell is the alias the production compilation
	// DECLARES (`ΔToken`), never the Go name that renames into it.
	if got := importedTypeAliases["ΔToken"]; got != "object" {
		t.Errorf("the collision-renamed alias must be re-emitted into the test compilation, got %q", got)
	}

	if _, minted := importedTypeAliases["Token"]; minted {
		t.Errorf("the RENAME record is not an alias declaration and must not be emitted as one")
	}

	tokenObj := scope.Lookup("Token")

	if tokenObj == nil {
		t.Fatal("fixture is inert: Token must be in package scope")
	}

	if got := productionAliasLiftedTypes[tokenObj.Type()]; got != "ΔToken" {
		t.Errorf("the defined-over-interface type must resolve to its alias name, got %q", got)
	}

	// The uncollided one-hop form ends at its first record.
	if got := importedTypeAliases["Stream"]; got != "go.example.ifacealias_package.Reader" {
		t.Errorf("a one-hop published alias must seed its own target, got %q", got)
	}

	streamObj := scope.Lookup("Stream")

	if streamObj == nil {
		t.Fatal("fixture is inert: Stream must be in package scope")
	}

	if got := productionAliasLiftedTypes[streamObj.Type()]; got != "Stream" {
		t.Errorf("a defined type over a NAMED interface must resolve to its alias name, got %q", got)
	}

	// An INLINE interface definition IS a member of the package class and resolves through it. It
	// publishes here only to prove the predicate reads the declaration's syntax, not the metadata.
	readerObj := scope.Lookup("Reader")

	if readerObj == nil {
		t.Fatal("fixture is inert: Reader must be in package scope")
	}

	if got, seeded := productionAliasLiftedTypes[readerObj.Type()]; seeded {
		t.Errorf("an inline interface definition is a class member and must not be seeded, got %q", got)
	}

	// A `_test.go` declaration emits its own `global using` into this compilation.
	if _, seeded := importedTypeAliases["localToken"]; seeded {
		t.Errorf("a test-declared type declares its own alias here and must not be seeded from production")
	}

	resetPackageState(&packages.Package{})
}

// TestFunctionLocalAnonStructAdoptsPackageLift pins that a function-local lift of an anonymous
// struct the PACKAGE has already lifted reuses that name instead of minting a second C# type.
//
// encoding/xml's read_test.go declares `type Child struct{ G struct{ I int } }` — package-level,
// lifted `Child_G` — and then writes the very same anonymous type as a composite literal inside a
// function, which minted `TestUnmarshalEmptyValues_type`. Go says those are one type and assigns one
// to the other; C# saw two structs and refused with CS1503, the package's last build error.
//
// The dedup was function-scoped, and its own comment recorded the wider split as a residual. The
// package-level registry is the authority for closing it: package-scoped, and keyed by the full
// types.String() including field TAGS, which is what Go's struct identity compares.
func TestFunctionLocalAnonStructAdoptsPackageLift(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/anonlift\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

// The package-level occurrence: an anonymous struct as a FIELD type, lifted and registered.
type Child struct {
	G struct {
		I int
	}
}

// A DIFFERENT shape sharing no signature — the control that must keep its own lift.
type Other struct {
	H struct {
		S string
	}
}

func main() {
	// The same Go type spelled again, inside a function. Go assigns one to the other because
	// there is only one type; a second lift makes two C# structs and the assignment is CS1503.
	c := Child{G: struct{ I int }{I: 7}}
	o := Other{H: struct{ S string }{S: "x"}}

	fmt.Println(c.G.I, o.H.S)
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "anonlift", "main.cs"))

	// The literal must construct the type the FIELD is declared with. Before the fix it constructed
	// a freshly minted `main_type`, which no assignment to `Child.G` accepts.
	if !strings.Contains(mainCs, "new Child_G(I: 7)") {
		t.Errorf("the function-local literal must adopt the package-level lift `Child_G`:\n%s", mainCs)
	}

	// …and no second declaration may exist for it.
	if strings.Count(mainCs, "partial struct Child_G") != 1 {
		t.Errorf("the package-level lift must be declared exactly once:\n%s", mainCs)
	}

	if strings.Contains(mainCs, "main_type") {
		t.Errorf("a second lift was minted for an already-lifted anonymous struct:\n%s", mainCs)
	}

	// The negative control: a DIFFERENT signature keeps its own lift, so the reuse is identity-
	// driven and not a blanket collapse of every anonymous struct onto the first one seen.
	if !strings.Contains(mainCs, "new Other_H(S:") {
		t.Errorf("an anonymous struct with its own signature must keep its own lift:\n%s", mainCs)
	}
}

// TestPublishedAliasChainTerminates pins followPublishedAliasChain's two ends: the two-hop rename
// chain resolves to the alias the production compilation declares, a one-hop record resolves to
// itself, an unpublished name resolves to nothing, and a self-referential record — read from a file
// this run did not necessarily write — terminates rather than spinning.
func TestPublishedAliasChainTerminates(t *testing.T) {
	targets := map[string]string{
		"Token":  "ΔToken",
		"ΔToken": "object",
		"Stream": "go.example_package.Reader",
		"Loop":   "Knot",
		"Knot":   "Loop",
	}

	for _, tc := range []struct {
		name       string
		wantAlias  string
		wantTarget string
		wantOK     bool
	}{
		{name: "Token", wantAlias: "ΔToken", wantTarget: "object", wantOK: true},
		{name: "Stream", wantAlias: "Stream", wantTarget: "go.example_package.Reader", wantOK: true},
		{name: "Absent", wantOK: false},
		{name: "Loop", wantAlias: "Knot", wantTarget: "Loop", wantOK: true},
	} {
		alias, target, ok := followPublishedAliasChain(targets, tc.name)

		if ok != tc.wantOK {
			t.Errorf("%s: resolved = %v, want %v", tc.name, ok, tc.wantOK)
			continue
		}

		if ok && (alias != tc.wantAlias || target != tc.wantTarget) {
			t.Errorf("%s: got (%q, %q), want (%q, %q)", tc.name, alias, target, tc.wantAlias, tc.wantTarget)
		}
	}
}

// TestComparableConstraintInterfaceEmitsMethodSetForm pins both sides of an interface that EMBEDS
// Go's built-in `comparable`, which must agree with each other and with the doctrine the bare
// constraint already follows.
//
// `comparable` admits every ==-able Go type, a set no C# constraint expresses — golib's
// `comparable<T>` CRTP is implemented by nothing, which is why a BARE `comparable` constraint has
// long emitted no C# constraint beyond `new()`. An interface EMBEDDING it inherits that fact, and
// the two sides disagreed: the DECLARATION appended a bare `comparable` to the C# base list (CS0305
// — that unimplementable generic named with no type argument) while the CONSTRAINT decided the
// interface was not a method set and took the generic CRTP form `LabeledCmp<P>` against a
// declaration emitted arity-0 (CS0308). net/netip's fuzz_test.go is the corpus's first instance.
//
// The same fixture pins the TYPE-PARAMETER SLOT rule, because netip reached both through one call:
// a concrete value passed to a `P`-typed parameter is RECORDED against P's constraint (the only one
// of the two with a C# spelling) and EMITTED unchanged (C# infers P from the argument and checks the
// constraint nominally, so an adapter wrap there is CS1503).
func TestComparableConstraintInterfaceEmitsMethodSetForm(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/cmpcon\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "package main\n\n"+
		"import \"fmt\"\n\n"+
		"// netip's shape: a method-set interface, and a constraint interface that embeds it\n"+
		"// alongside Go's built-in comparable.\n"+
		"type Labeled interface {\n\tLabel() string\n}\n\n"+
		"type LabeledCmp interface {\n\tcomparable\n\tLabeled\n}\n\n"+
		"type Tag struct{ name string }\n\n"+
		"func (t Tag) Label() string { return t.name }\n\n"+
		"func describe[P LabeledCmp](x P) string {\n\treturn x.Label()\n}\n\n"+
		"func main() {\n\tfmt.Println(describe(Tag{name: \"a\"}))\n}\n")

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

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "cmpcon")
	mainCs := readGenerated(t, filepath.Join(outDir, "main.cs"))
	infoCs := readGenerated(t, filepath.Join(outDir, "package_info.cs"))

	// The DECLARATION: `comparable` is not a C# base. The interface it embeds still is.
	if regexp.MustCompile(`(?m)^\s*comparable,?\s*$`).MatchString(mainCs) {
		t.Errorf("`comparable` must not be emitted as a C# base interface:\n%s", mainCs)
	}

	if !strings.Contains(mainCs, "partial interface LabeledCmp :") || !strings.Contains(mainCs, "Labeled") {
		t.Errorf("the constraint interface must keep its real method-set base:\n%s", mainCs)
	}

	// The CONSTRAINT: the arity-0 form the declaration actually emits, not the CRTP form.
	if !strings.Contains(mainCs, "where P : LabeledCmp") {
		t.Errorf("the constraint must name the interface itself:\n%s", mainCs)
	}

	if strings.Contains(mainCs, "LabeledCmp<P>") {
		t.Errorf("an interface that is a method set beyond `comparable` must not take the CRTP form:\n%s", mainCs)
	}

	// The type-parameter SLOT: the record names the CONSTRAINT, never the parameter.
	if !strings.Contains(infoCs, "GoImplement<Tag, LabeledCmp>") {
		t.Errorf("the implement record must name P's constraint:\n%s", infoCs)
	}

	if strings.Contains(infoCs, ", P>") {
		t.Errorf("a type PARAMETER is not an interface and must never reach an assembly attribute:\n%s", infoCs)
	}

	// …and the argument arrives as its own type, because C# infers P from it.
	if !strings.Contains(mainCs, "describe(new Tag(") {
		t.Errorf("a value passed to a type-parameter slot must be emitted unwrapped:\n%s", mainCs)
	}

	if strings.Contains(mainCs, "ᴠLabeledCmp") {
		t.Errorf("an adapter wrap in a type-parameter slot is CS1503 — the value must arrive as its own type:\n%s", mainCs)
	}
}

// TestUnexportedProductionTypeReachedThroughWrappers pins the three positions the test-file
// accessibility downgrade had to learn to look through.
//
// A production package emits an unexported type `internal`, so an EXPORTED test-file declaration
// over one is a public member whose type is less accessible (CS0050/CS0051/CS0052). The downgrade
// that fixes it looked through pointer/slice/array/map/channel and stopped there — net/netip's
// export_test.go reaches its unexported `addrDetail` through a generic type ARGUMENT
// (`unique.Handle[addrDetail]`), through an ALIAS (`type AddrDetail = addrDetail`), and a
// func-typed declaration reaches one through a SIGNATURE. All three are positions C#
// accessibility-consistency looks through exactly as it looks through a pointer.
//
// The negative controls matter as much: the downgrade must not fire for an exported production type
// or for an unexported type the TEST files declare — that one is publicized within this same pass.
func TestUnexportedProductionTypeReachedThroughWrappers(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads a module fixture through go/packages")
	}

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/accwrap\n\ngo 1.23\n",
		"value.go": "package accwrap\n\n" +
			"type addrDetail struct{ v bool }\n\n" +
			"type Exported struct{ n int }\n\n" +
			"type Handle[T any] struct{ x T }\n\n" +
			"func use(d addrDetail) bool { return d.v }\n",
		"export_test.go": "package accwrap\n\n" +
			"type AddrDetail = addrDetail\n\n" +
			"type testOnly struct{ q int }\n\n" +
			"var TestLocal testOnly\n",
	})

	internal, _ := loadTestVariantsForDir(t, dir)

	if internal == nil {
		t.Fatal("fixture must load the internal test variant")
	}

	pkg := internal.Types
	scope := pkg.Scope()
	v := &Visitor{fset: internal.Fset}

	lookup := func(name string) types.Type {
		obj := scope.Lookup(name)

		if obj == nil {
			t.Fatalf("fixture is inert: %s must be in package scope", name)
		}

		return obj.Type()
	}

	addrDetail := lookup("addrDetail")
	handle, ok := lookup("Handle").(*types.Named)

	if !ok {
		t.Fatal("fixture is inert: Handle must be a named generic type")
	}

	instantiated, err := types.Instantiate(nil, handle, []types.Type{addrDetail}, false)

	if err != nil {
		t.Fatalf("Instantiate: %v", err)
	}

	for _, tc := range []struct {
		name string
		typ  types.Type
		want bool
	}{
		{name: "the bare unexported production type", typ: addrDetail, want: true},
		{name: "through a pointer (already covered)", typ: types.NewPointer(addrDetail), want: true},
		{name: "through a generic type ARGUMENT", typ: instantiated, want: true},
		{name: "through an ALIAS", typ: lookup("AddrDetail"), want: true},
		{name: "through a SIGNATURE result", typ: types.NewSignatureType(nil, nil, nil, nil,
			types.NewTuple(types.NewVar(0, pkg, "r", addrDetail)), false), want: true},
		{name: "an EXPORTED production type", typ: lookup("Exported"), want: false},
		{name: "an unexported type the TEST files declare", typ: lookup("testOnly"), want: false},
	} {
		if got := v.typeReferencesUnexportedProductionNamed(tc.typ, pkg); got != tc.want {
			t.Errorf("%s: got %v, want %v", tc.name, got, tc.want)
		}
	}
}

// TestTestDeclaredAliasSpelledBare pins the SAME-PACKAGE white-box instance of the
// assembly-scoped-alias rule — net/netip's last one-line wall, and the mirror of the production-side
// arm beside it.
//
// An alias a `_test.go` file DECLARES emits its own `global using AddrDetail = …` into the test
// assembly, so the internal half needs no seeding. But the assembly has more than one variant class,
// and the EXTERNAL one reaches the alias by PACKAGE QUALIFICATION, because Go says
// `netip.AddrDetail` — `export_test.go` is part of package netip during a test build. A `global
// using` is a member of no class, so that spelling is CS0426.
//
// The rule is MODEL-INDEPENDENT and this guard pins BOTH arms. Its first version required the
// white-box reference model and listed recompile as a case that must NOT fire; netip disproved that
// by taking recompile to satisfy a nominal constraint and landing on the identical CS0426.
//
// The predicate is guarded rather than the emitted text because the qualified spelling depends on
// which file-local package aliases a real conversion happens to register, and no minimal fixture
// reproduced netip's (several were tried — see the board entry). Every clause below is a distinct
// reason the rule must NOT fire, and each is exercised.
func TestTestDeclaredAliasSpelledBare(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads a module fixture through go/packages")
	}

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/extalias\n\ngo 1.23\n",
		"value.go": "package extalias\n\n" +
			"type addrDetail struct{ v bool }\n\n" +
			// A PRODUCTION-declared alias: the sibling arm's business, never this one's.
			"type Detail = addrDetail\n\n" +
			"type Plain struct{ n int }\n\n" +
			"func detail(v bool) addrDetail { return addrDetail{v: v} }\n",
		// netip's export_test.go shape: an EXPORTED alias to an unexported production type.
		"export_test.go": "package extalias\n\n" +
			"type AddrDetail = addrDetail\n\n" +
			"func Mk(v bool) AddrDetail { return detail(v) }\n",
	})

	internal, _ := loadTestVariantsForDir(t, dir)

	if internal == nil {
		t.Fatal("fixture must load the internal test variant")
	}

	scope := internal.Types.Scope()

	lookup := func(name string) types.Type {
		obj := scope.Lookup(name)

		if obj == nil {
			t.Fatalf("fixture is inert: %s must be in package scope", name)
		}

		return obj.Type()
	}

	external := Options{
		testWhiteboxReference: true,
		testExternalVariant:   true,
		testProductionPath:    "example/extalias",
	}

	v := &Visitor{fset: internal.Fset, options: external}

	// The rule itself: the test-declared alias is spelled bare — under BOTH models that reach this
	// arm. The white-box reference model was the only one the rule's first version served; netip
	// measured the recompile arm carrying the identical CS0426, because what makes the qualified
	// spelling invalid is that a `global using` is a member of no class, not where production lives.
	for _, tc := range []struct {
		name    string
		options Options
	}{
		{
			name: "white-box reference: production is a referenced assembly",
			options: Options{
				testWhiteboxReference: true,
				testExternalVariant:   true,
				testProductionPath:    "example/extalias",
			},
		},
		{
			// A recompile conversion keeps the self-import binding, so the package under test
			// lives in testPackagePath and testProductionPath is EMPTY — modelling it with the
			// reference models' field would pass for the wrong reason and pin nothing.
			name: "recompile: production, internal and external are ONE compilation",
			options: Options{
				testExternalVariant: true,
				testPackagePath:     "example/extalias",
			},
		},
	} {
		vm := &Visitor{fset: internal.Fset, options: tc.options}

		if name, bare := vm.testDeclaredAliasSpelledBare(lookup("AddrDetail")); !bare || name != "AddrDetail" {
			t.Errorf("%s: a test-declared alias must be spelled bare from the external variant, got (%q, %v)", tc.name, name, bare)
		}
	}

	// …and every clause that must hold it back.
	for _, tc := range []struct {
		name    string
		visitor *Visitor
		typ     types.Type
	}{
		{
			name:    "a PRODUCTION-declared alias (the sibling arm renders its target)",
			visitor: v,
			typ:     lookup("Detail"),
		},
		{
			name:    "a defined type that is not an alias at all",
			visitor: v,
			typ:     lookup("Plain"),
		},
		{
			name:    "the INTERNAL variant, whose files already use the bare name",
			visitor: &Visitor{fset: internal.Fset, options: Options{testWhiteboxReference: true, testProductionPath: "example/extalias"}},
			typ:     lookup("AddrDetail"),
		},
		{
			name:    "a PRODUCTION conversion, which is no test variant at all",
			visitor: &Visitor{fset: internal.Fset, options: Options{testExternalVariant: true}},
			typ:     lookup("AddrDetail"),
		},
		{
			name:    "an alias of some OTHER package, a real member of a real referenced assembly",
			visitor: &Visitor{fset: internal.Fset, options: Options{testWhiteboxReference: true, testExternalVariant: true, testProductionPath: "example/elsewhere"}},
			typ:     lookup("AddrDetail"),
		},
	} {
		if name, bare := tc.visitor.testDeclaredAliasSpelledBare(tc.typ); bare {
			t.Errorf("%s: must not be spelled bare, got %q", tc.name, name)
		}
	}
}
