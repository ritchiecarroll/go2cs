// cgoDeclarations_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Phase 1 of docs/PLAN-cgo-interop.md: the recognizer and the source mapping, over the declarations cmd/cgo writes.
// The first test type-checks cgo-shaped declarations written here. The fixture tests read cmd/cgo's real output for
// lane P1's five-kind fixture, checked in under testdata/cgo/phase1 (its README says how it was generated): cgo's
// rewriting needs a C compiler, but what it writes is plain Go that loads without one. Every test runs on any host.

package main

import (
	"go/ast"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"slices"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// cgoShapedSource carries one declaration of each recognized kind, cgo's plumbing as P1 listed it, and an ordinary
// Go declaration, under the names cmd/cgo gives them.
const cgoShapedSource = `package cgoshaped

import "unsafe"

type _Ctype_int int32
type _Ctype_char int8
type _Ctype_double float64
type _Ctype_void [0]byte
type _Ctype_struct_x struct {
	a    _Ctype_int
	next *_Ctype_struct_x
}
type _Ctype_union_u [8]byte
type _Ctype_enum_color uint32
type _Ctype_xptr = *_Ctype_struct_x

const _Ciconst_CONST = 42
const _Ciconst_GREEN = 5
const _Cfconst_FCONST = 2.5
const _Csconst_SCONST = "hi"

var _Cvar_counter *_Ctype_int

func _Cfunc_add(p0 _Ctype_int, p1 _Ctype_int) (r1 _Ctype_int) { return p0 + p1 }
func _Cfunc_free(p0 unsafe.Pointer) (r1 _Ctype_void)         { return }
func _Cfunc_CString(s string) *_Ctype_char                    { return nil }
func _Cfunc__CMalloc(n uintptr) unsafe.Pointer                { return nil }

var _cgo_0123456789ab_Cfunc_add byte
var __cgofn__cgo_0123456789ab_Cfunc_add byte
var _Cgo_always_false bool

func _Cgo_use(interface{})                {}
func _Cgo_ptr(p unsafe.Pointer) unsafe.Pointer { return p }
func _cgoCheckPointer(interface{}, interface{}) {}

func Add(a, b int) int { return int(_Cfunc_add(_Ctype_int(a), _Ctype_int(b))) }
`

func TestCgoDeclarationsClassifiesEachKindAndFiltersThePlumbing(t *testing.T) {
	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "cgoshaped.go", cgoShapedSource, 0)

	if err != nil {
		t.Fatal(err)
	}

	config := types.Config{Importer: importer.Default()}
	pkg, err := config.Check("cgoshaped", fset, []*ast.File{file}, nil)

	if err != nil {
		t.Fatal(err)
	}

	want := map[string]cgoDeclKind{
		"CONST":      cgoIntConst,
		"CString":    cgoFunc,
		"FCONST":     cgoFloatConst,
		"GREEN":      cgoIntConst,
		"SCONST":     cgoStringConst,
		"add":        cgoFunc,
		"char":       cgoType,
		"counter":    cgoVar,
		"double":     cgoType,
		"enum_color": cgoEnum,
		"free":       cgoFunc,
		"int":        cgoType,
		"struct_x":   cgoStruct,
		"union_u":    cgoUnion,
		"void":       cgoType,
		"xptr":       cgoType,
	}

	got := cgoDeclarations(pkg)

	if len(got) != len(want) {
		t.Errorf("recognized %d cgo declarations, want %d: %v", len(got), len(want), cgoDeclarationNames(got))
	}

	for _, declaration := range got {
		kind, known := want[declaration.cName]

		if !known {
			t.Errorf("C.%s was recognized, but it is plumbing or not a cgo declaration", declaration.cName)
			continue
		}

		if declaration.kind != kind {
			t.Errorf("C.%s classified as %d, want %d", declaration.cName, declaration.kind, kind)
		}

		if declaration.object == nil {
			t.Errorf("C.%s carries no typed object", declaration.cName)
		}
	}
}

func cgoDeclarationNames(declarations []cgoDeclaration) []string {
	names := make([]string, 0, len(declarations))

	for _, declaration := range declarations {
		names = append(names, declaration.cName)
	}

	return names
}

// cgoFixtureDir holds fixture.go and the three files cmd/cgo generated from it.
var cgoFixtureDir = filepath.Join("testdata", "cgo", "phase1")

// cgoFixtureWant is every declaration the fixture's generated files carry, by C name: the fourteen fixture.go writes
// (TestTheCgoFixtureWritesFourteenCNames derives them from its source) and the seven C types cmd/cgo declares on the
// way to them (scale returns a double that Go reads through float64, so C.double is never written). cgo's own _GoString_ is not here: its C name is reserved.
var cgoFixtureWant = map[string]cgoDeclKind{
	"BLUE":       cgoIntConst,
	"CONST":      cgoIntConst,
	"CString":    cgoFunc,
	"FCONST":     cgoFloatConst,
	"GREEN":      cgoIntConst,
	"GoString":   cgoFunc,
	"SCONST":     cgoStringConst,
	"add":        cgoFunc,
	"char":       cgoType,
	"double":     cgoType,
	"enum_color": cgoEnum,
	"free":       cgoFunc,
	"int":        cgoType,
	"intgo":      cgoType,
	"long":       cgoType,
	"ptrdiff_t":  cgoType,
	"scale":      cgoFunc,
	"struct_x":   cgoStruct,
	"uchar":      cgoType,
	"void":       cgoType,
	"xptr":       cgoType,
}

// cgoIncompleteOverlay declares runtime/cgo's Incomplete exactly as cgo.go does.
const cgoIncompleteOverlay = `package cgo

import "internal/runtime/sys"

type Incomplete struct {
	_ sys.NotInHeap
}
`

// loadCgoFixture loads the fixture directory at CGO_ENABLED=0, where fixture.go drops out (it imports "C") and the
// three generated files are the package. withIncomplete adds an Overlay giving runtime/cgo the one declaration it
// loses at CGO_ENABLED=0: Incomplete is declared in cgo.go, which imports "C" itself, and every _cgo_gotypes.go names
// it in `type _ _cgopackage.Incomplete`. The fixture's own files are never touched.
func loadCgoFixture(t *testing.T, withIncomplete bool) *packages.Package {
	t.Helper()

	dir, err := filepath.Abs(cgoFixtureDir)

	if err != nil {
		t.Fatal(err)
	}

	config := &packages.Config{
		Mode: packages.LoadAllSyntax,
		Dir:  dir,
		Env:  append(os.Environ(), "CGO_ENABLED=0"),
	}

	if withIncomplete {
		goRoot, err := getGoEnvFrom(dir, "GOROOT")

		if err != nil {
			t.Fatal(err)
		}

		config.Overlay = map[string][]byte{
			filepath.Join(goRoot, "src", "runtime", "cgo", "go2cs_incomplete.go"): []byte(cgoIncompleteOverlay),
		}
	}

	pkgs, err := packages.Load(config, ".")

	if err != nil {
		t.Fatal(err)
	}

	if len(pkgs) != 1 || pkgs[0].Name != "cgofixture" {
		t.Fatalf("loaded %d packages from the fixture, want one cgofixture", len(pkgs))
	}

	return pkgs[0]
}

// The premise of every fixture test, and the reason for the Overlay: at CGO_ENABLED=0 the generated files are the
// package, and runtime/cgo's Incomplete is the ONE thing they cannot resolve. A toolchain that moves Incomplete, or
// a fixture regenerated with a new dependency, reds here by name.
func TestTheCgoFixtureLoadsAtCgoDisabledMissingOnlyRuntimeCgosIncomplete(t *testing.T) {
	bare := loadCgoFixture(t, false)

	var names []string

	for _, file := range bare.CompiledGoFiles {
		names = append(names, filepath.Base(file))
	}

	slices.Sort(names)

	if want := []string{"cgo_gotypes.go", "cgo_import.go", "fixture.cgo1.go"}; !slices.Equal(names, want) {
		t.Errorf("the fixture's package compiles %v, want %v", names, want)
	}

	if len(bare.Errors) != 1 || !strings.Contains(bare.Errors[0].Msg, "undefined: _cgopackage.Incomplete") {
		t.Errorf("without the Overlay the fixture reports %v, want exactly runtime/cgo's Incomplete undefined", bare.Errors)
	}

	if overlaid := loadCgoFixture(t, true); len(overlaid.Errors) != 0 {
		t.Errorf("with the Overlay the fixture reports %v, want none", overlaid.Errors)
	}
}

// fixture.go writes fourteen C names, and the want table carries every one of them.
func TestTheCgoFixtureWritesFourteenCNames(t *testing.T) {
	file, err := parser.ParseFile(token.NewFileSet(), filepath.Join(cgoFixtureDir, "fixture.go"), nil, 0)

	if err != nil {
		t.Fatal(err)
	}

	written := map[string]bool{}

	ast.Inspect(file, func(node ast.Node) bool {
		if selector, ok := node.(*ast.SelectorExpr); ok {
			if ident, ok := selector.X.(*ast.Ident); ok && ident.Name == "C" {
				written[selector.Sel.Name] = true
			}
		}

		return true
	})

	if len(written) != 14 {
		t.Errorf("fixture.go writes %d C names, want 14: %v", len(written), written)
	}

	for name := range written {
		if _, ok := cgoFixtureWant[name]; !ok {
			t.Errorf("fixture.go writes C.%s, which the want table does not carry", name)
		}
	}
}

func TestCgoDeclarationsResolvesEveryCNameInTheFixture(t *testing.T) {
	pkg := loadCgoFixture(t, true)
	got := cgoDeclarations(pkg.Types)

	if len(got) != len(cgoFixtureWant) {
		t.Errorf("recognized %d cgo declarations in the fixture, want %d: %v", len(got), len(cgoFixtureWant), cgoDeclarationNames(got))
	}

	seen := map[string]bool{}

	for _, declaration := range got {
		seen[declaration.cName] = true
		kind, known := cgoFixtureWant[declaration.cName]

		if !known {
			t.Errorf("C.%s was recognized, but it is plumbing or not a cgo declaration", declaration.cName)
			continue
		}

		if declaration.kind != kind {
			t.Errorf("C.%s classified as %d, want %d", declaration.cName, declaration.kind, kind)
		}

		if declaration.object == nil || declaration.object.Pkg() != pkg.Types {
			t.Errorf("C.%s carries no typed object of the fixture's package", declaration.cName)
		}
	}

	for name := range cgoFixtureWant {
		if !seen[name] {
			t.Errorf("C.%s is declared by the fixture but was not recognized", name)
		}
	}
}

// fixture.cgo1.go maps to fixture.go through its //line directive; cmd/cgo's own two files map to no source.
func TestCgoSourceFilesMapsTheFixturesTreesToTheirSource(t *testing.T) {
	pkg := loadCgoFixture(t, true)
	mapped := cgoSourceFiles(pkg.Fset, pkg.Syntax)

	dir, err := filepath.Abs(cgoFixtureDir)

	if err != nil {
		t.Fatal(err)
	}

	want := map[string]string{
		"cgo_gotypes.go":  "",
		"cgo_import.go":   "",
		"fixture.cgo1.go": filepath.Join(dir, "fixture.go"),
	}

	if len(mapped) != len(want) {
		t.Fatalf("mapped %d trees, want %d", len(mapped), len(want))
	}

	for _, entry := range mapped {
		source, known := want[filepath.Base(entry.generated)]

		if !known {
			t.Errorf("mapped an unexpected tree %s", filepath.Base(entry.generated))
			continue
		}

		if entry.source != source {
			t.Errorf("%s maps to source %q, want %q", filepath.Base(entry.generated), entry.source, source)
		}

		if entry.file == nil {
			t.Errorf("%s carries no syntax tree", filepath.Base(entry.generated))
		}
	}
}
