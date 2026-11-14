// inlineArrayStamp_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"slices"
	"strings"
	"testing"
)

// stampFixture is a package with one struct converted whole (Holder, holding Plain by value), one that holds an array
// and is never converted (Loose), and an anonymous struct converted whole.
const stampFixture = `package lib

import "unsafe"

type Plain struct{ N int32 }

type Holder struct {
	P   Plain
	Buf [8]byte
}

type Loose struct{ Tab [4]int64 }

func read(p unsafe.Pointer) *Holder { return (*Holder)(p) }

func anon(p unsafe.Pointer) *struct{ X [2]int32 } { return (*struct{ X [2]int32 })(p) }
`

// The records name each admitted NAMED type and, only when it is not the default, the rule. The anonymous struct is
// admitted (it is in keyOrder) and not recorded.
func TestInlineArrayStampRecords(t *testing.T) {
	typeRecord := func(name string) string { return `[assembly: go.golib.GoInlineArrayType("` + name + `")]` }
	keysRecord := func(a *inlineArrayAdmission) string {
		return fmt.Sprintf(`[assembly: go.golib.GoInlineArrayKeys(%d, "%s")]`, len(a.keyOrder), inlineArrayKeyDigest(a.keyOrder))
	}

	auto, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", stampFixture})

	if !slices.ContainsFunc(auto.keyOrder, func(key string) bool { return strings.HasPrefix(key, "anon:") }) {
		t.Fatalf("the fixture's anonymous struct is not admitted (%q); the test would not prove it goes unrecorded", admittedKeys(auto))
	}

	if got, want := inlineArrayStampRecords(auto, inlineArraysModeAuto), []string{keysRecord(auto), typeRecord("Holder"), typeRecord("Plain")}; !slices.Equal(got, want) {
		t.Fatalf("auto records:\n got %q\nwant %q", got, want)
	}

	forced, _ := admitFixture(t, "", Options{inlineArrays: inlineArrayOverride{force: []string{"example.com/lib"}}}, [2]string{"lib.go", stampFixture})
	want := []string{`[assembly: go.golib.GoInlineArrays("forced")]`, keysRecord(forced), typeRecord("Holder"), typeRecord("Loose"), typeRecord("Plain")}

	if got := inlineArrayStampRecords(forced, inlineArraysModeForced); !slices.Equal(got, want) {
		t.Fatalf("forced records:\n got %q\nwant %q", got, want)
	}

	if got := inlineArrayStampRecords(nil, inlineArraysModeOff); !slices.Equal(got, []string{`[assembly: go.golib.GoInlineArrays("off")]`}) {
		t.Fatalf("off records: %q", got)
	}

	if got := inlineArrayStampRecords(nil, ""); len(got) != 0 {
		t.Fatalf("an excluded package records %q", got)
	}

	// The records read back as the stamp they encode, whichever spelling rootAttributeName gave them.
	lines := append([]string{"// unrelated", `[assembly: GoTypeAlias("A", "B")]`}, want...)
	lines = append(lines, `[assembly: global::go.golib.GoInlineArrayType("Zed")]`)
	stamp := parseInlineArrayStamp(lines)

	if stamp.mode != inlineArraysModeForced || !slices.Equal(stamp.types, []string{"Holder", "Loose", "Plain", "Zed"}) ||
		stamp.keyCount != len(forced.keyOrder) || stamp.keyDigest != inlineArrayKeyDigest(forced.keyOrder) {
		t.Fatalf("parsed %+v", stamp)
	}

	if stamp := parseInlineArrayStamp([]string{"// nothing here"}); stamp.mode != inlineArraysModeAuto || len(stamp.types) != 0 {
		t.Fatalf("a file without the section parsed as %+v", stamp)
	}
}

// The section is inserted only while there is a record, and a rewrite replaces it whole.
func TestInlineArrayStampSection(t *testing.T) {
	base := []string{"// <ImplicitConversions>", "// </ImplicitConversions>", "", "namespace go;"}

	if got := applyRecordSection(slices.Clone(base), nil, false, inlineArraysSection); !slices.Equal(got, base) {
		t.Fatalf("an empty record set changed the file:\n%s", strings.Join(got, "\n"))
	}

	first := applyRecordSection(slices.Clone(base), []string{`[assembly: go.golib.GoInlineArrayType("A")]`}, false, inlineArraysSection)
	text := strings.Join(first, "\n")

	if !strings.Contains(text, inlineArraysSectionStart+"\n"+`[assembly: go.golib.GoInlineArrayType("A")]`+"\n"+inlineArraysSectionEnd) {
		t.Fatalf("section not written:\n%s", text)
	}

	second := applyRecordSection(first, []string{`[assembly: go.golib.GoInlineArrayType("B")]`}, false, inlineArraysSection)

	if stamp := parseInlineArrayStamp(second); !slices.Equal(stamp.types, []string{"B"}) {
		t.Fatalf("a whole-package rewrite kept a stale record: %+v", stamp)
	}

	if got := applyRecordSection(second, nil, false, inlineArraysSection); !slices.Equal(got, base) {
		t.Fatalf("removing the last record left a trace:\n%s", strings.Join(got, "\n"))
	}
}

// A -tests conversion applies the RECORDED rule, not its own options, and refuses a record its production files do not
// reproduce. Its _test.go files never enter the decision.
func TestInlineArrayAdmitFromStamp(t *testing.T) {
	fset := token.NewFileSet()
	var files []*ast.File

	for _, source := range [][2]string{
		{"lib.go", stampFixture},
		{"lib_test.go", "package lib\n\nimport \"unsafe\"\n\ntype Probe struct{ B [3]byte }\n\nfunc probe(p unsafe.Pointer) *Probe { return (*Probe)(p) }\n"},
	} {
		file, err := parser.ParseFile(fset, source[0], source[1], 0)

		if err != nil {
			t.Fatal(err)
		}

		files = append(files, file)
	}

	info := &types.Info{Types: map[ast.Expr]types.TypeAndValue{}, Defs: map[*ast.Ident]types.Object{}, Uses: map[*ast.Ident]types.Object{}, Selections: map[*ast.SelectorExpr]*types.Selection{}}
	pkg, err := (&types.Config{Importer: importer.Default()}).Check("example.com/lib", fset, files, info)

	if err != nil {
		t.Fatal(err)
	}

	// Production ran forced; the -tests run carries no -inline-arrays value at all. Its record holds the forced rule's keys.
	production := computeInlineArrayAdmission(files[:1], fset, pkg, info, "", Options{inlineArrays: inlineArrayOverride{force: []string{"example.com/lib"}}})
	forcedStamp := inlineArrayStamp{mode: inlineArraysModeForced, types: []string{"Holder", "Loose", "Plain"}, keyCount: len(production.keyOrder), keyDigest: inlineArrayKeyDigest(production.keyOrder)}
	a, err := admitFromStamp(files, fset, pkg, info, "", forcedStamp, Options{})

	if err != nil {
		t.Fatalf("a matching forced record refused: %v", err)
	}

	if got := a.namedTypes(); !slices.Equal(got, []string{"Holder", "Loose", "Plain"}) {
		t.Fatalf("admitted %q", got)
	}

	if _, err := admitFromStamp(files, fset, pkg, info, "", inlineArrayStamp{mode: inlineArraysModeAuto, types: []string{"Holder", "Loose", "Plain"}}, Options{}); err == nil || !strings.Contains(err.Error(), "admit Holder, Plain under the same rule") {
		t.Fatalf("a record the production files do not reproduce was accepted, or refused unclearly: %v", err)
	}

	if a, err := admitFromStamp(files, fset, pkg, info, "", inlineArrayStamp{mode: inlineArraysModeOff}, Options{inlineArrays: inlineArrayOverride{force: []string{"example.com/lib"}}}); err != nil || a != nil {
		t.Fatalf("a recorded off rule did not override the run's own forcing: %v, %q", err, admittedKeys(a))
	}

	if _, err := admitFromStamp(files, fset, pkg, info, "", inlineArrayStamp{mode: "sideways"}, Options{}); err == nil {
		t.Fatal("an unknown recorded rule was accepted")
	}
}

// The pair rule across two packages reads the other package's record: a chain from a type the other package admitted
// to one this package did not (or the reverse) disagrees, and a package whose record cannot be read is counted. Under
// the automatic rule this package's side is kept out with the other's (COORD 2026-11-14 13:56Z); forced, the package
// is refused.
func TestInlineArrayAdmissionJudgesAForeignPairByItsStamp(t *testing.T) {
	fset := token.NewFileSet()
	parse := func(name, source string) *ast.File {
		file, err := parser.ParseFile(fset, name, source, 0)

		if err != nil {
			t.Fatal(err)
		}

		return file
	}

	libFile := parse("lib.go", "package lib\n\ntype F struct {\n\tA [4]int32\n\tN int64\n}\n")
	libPkg, err := (&types.Config{Importer: importer.Default()}).Check("example.com/lib", fset, []*ast.File{libFile}, nil)

	if err != nil {
		t.Fatal(err)
	}

	useFile := parse("use.go", `package use

import (
	"unsafe"

	"example.com/lib"
)

type L struct {
	A [4]int32
	N int64
}

func view(f *lib.F) *L { return (*L)(unsafe.Pointer(f)) }
`)
	info := &types.Info{Types: map[ast.Expr]types.TypeAndValue{}, Defs: map[*ast.Ident]types.Object{}, Uses: map[*ast.Ident]types.Object{}, Selections: map[*ast.SelectorExpr]*types.Selection{}}
	imports := importerFunc(func(path string) (*types.Package, error) {
		if path == "example.com/lib" {
			return libPkg, nil
		}

		return importer.Default().Import(path)
	})
	usePkg, err := (&types.Config{Importer: imports}).Check("example.com/use", fset, []*ast.File{useFile}, info)

	if err != nil {
		t.Fatal(err)
	}

	saved := lookupInlineArrayStamp
	defer func() { lookupInlineArrayStamp = saved }()

	run := func(stamp *inlineArrayStamp, options ...Options) *inlineArrayAdmission {
		lookupInlineArrayStamp = func(importPath string, _ Options) (inlineArrayStamp, bool) {
			if importPath != "example.com/lib" || stamp == nil {
				return inlineArrayStamp{}, false
			}

			return *stamp, true
		}

		chosen := Options{}

		if len(options) > 0 {
			chosen = options[0]
		}

		return computeInlineArrayAdmission([]*ast.File{useFile}, fset, usePkg, info, "", chosen)
	}

	// use.L is converted whole, so it is admitted; lib.F is lib's.
	if a := run(&inlineArrayStamp{mode: inlineArraysModeAuto, types: []string{"F"}}); len(a.refusals) != 0 || a.foreign != 0 {
		t.Fatalf("both sides admitted, yet refused %v (foreign %d)", a.refusals, a.foreign)
	}

	a := run(&inlineArrayStamp{mode: inlineArraysModeAuto}, Options{inlineArrays: inlineArrayOverride{force: []string{"example.com/use"}}})

	if len(a.refusals) != 1 || !strings.Contains(a.refusals[0].text, "reinterprets L, whose arrays are held inline, as lib.F, whose arrays are not") {
		t.Fatalf("forced: a foreign side left as array<T> was not refused by name: %v", a.refusals)
	}

	auto := run(&inlineArrayStamp{mode: inlineArraysModeAuto})

	if len(auto.refusals) != 0 || len(auto.keyOrder) != 0 || !hasWarning(auto, "L is converted whole through unsafe.Pointer but keeps array<T> storage: lib.F is reinterpreted as it") {
		t.Fatalf("auto: L was not kept out with lib.F: admitted %v, refusals %v, warnings %v", auto.keyOrder, auto.refusals, auto.warnings)
	}

	if a := run(nil); len(a.refusals) != 0 || a.foreign != 1 {
		t.Fatalf("an unreadable foreign record: refusals %v, foreign %d (want 0 and 1)", a.refusals, a.foreign)
	}
}

// checkStampFixture type-checks one production file for the -tests side of the record.
func checkStampFixture(t *testing.T, source string) ([]*ast.File, *token.FileSet, *types.Package, *types.Info) {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "lib.go", source, 0)

	if err != nil {
		t.Fatal(err)
	}

	info := &types.Info{Types: map[ast.Expr]types.TypeAndValue{}, Defs: map[*ast.Ident]types.Object{}, Uses: map[*ast.Ident]types.Object{}, Selections: map[*ast.SelectorExpr]*types.Selection{}}
	pkg, err := (&types.Config{Importer: importer.Default()}).Check("example.com/lib", fset, []*ast.File{file}, info)

	if err != nil {
		t.Fatal(err)
	}

	return []*ast.File{file}, fset, pkg, info
}

// C1's review, M2: lib's admitted structs are all aliases of ANONYMOUS structs, which the named-type records cannot
// name, so a -tests run compared an empty list with an empty list and could never disagree. The record now carries the
// count and a digest of EVERY admitted key, so a production set that admits differently is refused, lib's shape included.
func TestInlineArrayRecordCatchesAnAnonymousDisagreement(t *testing.T) {
	const production = `package lib

import "unsafe"

type Tq = struct {
	A [4]int32
	N int64
}

func read(p unsafe.Pointer) *Tq { return (*Tq)(p) }
`
	// The same package after an edit that no longer converts Tq whole: it admits nothing.
	const edited = `package lib

type Tq = struct {
	A [4]int32
	N int64
}
`

	files, fset, pkg, info := checkStampFixture(t, production)
	a := computeInlineArrayAdmission(files, fset, pkg, info, "", Options{})

	if len(a.keyOrder) != 1 || len(a.namedTypes()) != 0 {
		t.Fatalf("the fixture must admit exactly one ANONYMOUS struct, admitted %q", admittedKeys(a))
	}

	stamp := parseInlineArrayStamp(inlineArrayStampRecords(a, inlineArraysModeAuto))

	if _, err := admitFromStamp(files, fset, pkg, info, "", stamp, Options{}); err != nil {
		t.Fatalf("the production files' own record refused: %v", err)
	}

	files, fset, pkg, info = checkStampFixture(t, edited)

	if _, err := admitFromStamp(files, fset, pkg, info, "", stamp, Options{}); err == nil || !strings.Contains(err.Error(), "1 admitted struct type") {
		t.Fatalf("a production set that admits nothing was accepted against a record of one anonymous type, or refused unclearly: %v", err)
	}
}

// C1's review, M2: with no record to read, a -tests run assumed the automatic rule even when its own -inline-arrays
// value names the package, which production may have forced or turned off. It now refuses by name.
func TestInlineArrayTestsRefuseAMissingRecordTheOptionNames(t *testing.T) {
	if stamp, err := inlineArrayTestsStamp(inlineArrayStamp{}, false, "example.com/lib", Options{}); err != nil || stamp.mode != inlineArraysModeAuto {
		t.Fatalf("no record and no option: want the automatic rule, got %+v, %v", stamp, err)
	}

	for _, options := range []Options{
		{inlineArrays: inlineArrayOverride{force: []string{"example.com/lib"}}},
		{inlineArrays: inlineArrayOverride{off: []string{"example.com/lib"}}},
	} {
		if _, err := inlineArrayTestsStamp(inlineArrayStamp{}, false, "example.com/lib", options); err == nil || !strings.Contains(err.Error(), "example.com/lib") {
			t.Fatalf("no record while -inline-arrays names the package (%+v): want a refusal naming it, got %v", options.inlineArrays, err)
		}
	}

	if stamp, err := inlineArrayTestsStamp(inlineArrayStamp{mode: inlineArraysModeForced}, true, "example.com/lib", Options{}); err != nil || stamp.mode != inlineArraysModeForced {
		t.Fatalf("a readable record decides: got %+v, %v", stamp, err)
	}
}

// A record section's prose CHANGES between converter versions (M2 rewrote the <InlineArrays> prose), and a re-conversion
// over the old package_info.cs must replace it, not leave the old prose above the new: the remover matched only the
// CURRENT prose, so the four behavioral projects' regenerated package_info.cs carried both. The prose block is the run
// of comment lines directly above the start marker, up to a blank line or another section's marker.
func TestARecordSectionReplacesProseItNoLongerWrites(t *testing.T) {
	old := []string{
		"// <InterfaceImplementations>",
		"// </InterfaceImplementations>",
		"",
		"// An older wording of the prose,",
		"// two lines long.",
		inlineArraysSectionStart,
		`[assembly: go.golib.GoInlineArrayType("reparse")]`,
		inlineArraysSectionEnd,
		"",
		"// <ImplicitConversions>",
		"// </ImplicitConversions>",
		"",
		"namespace go;",
	}

	got := strings.Join(applyRecordSection(old, []string{`[assembly: go.golib.GoInlineArrayType("reparse")]`}, false, inlineArraysSection), "\n")

	if strings.Contains(got, "An older wording") || strings.Contains(got, "two lines long") {
		t.Errorf("the old prose survived:\n%s", got)
	}

	if strings.Count(got, inlineArraysProseLines()[0]) != 1 || !strings.Contains(got, "// </InterfaceImplementations>\n\n"+inlineArraysProseLines()[0]) {
		t.Errorf("want the current prose once, after the blank line that ends the previous section:\n%s", got)
	}
}
