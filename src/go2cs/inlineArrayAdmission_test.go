// inlineArrayAdmission_test.go - Gbtc
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
	"os"
	"path/filepath"
	"sort"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// admitFixture type-checks one package from (file name, source) pairs and runs the admission over it.
func admitFixture(t *testing.T, pkgDir string, options Options, sources ...[2]string) (*inlineArrayAdmission, *types.Package) {
	t.Helper()

	fset := token.NewFileSet()
	var files []*ast.File

	for _, source := range sources {
		file, err := parser.ParseFile(fset, source[0], source[1], parser.ParseComments)

		if err != nil {
			t.Fatalf("parse %s: %v", source[0], err)
		}

		files = append(files, file)
	}

	info := &types.Info{Types: map[ast.Expr]types.TypeAndValue{}, Defs: map[*ast.Ident]types.Object{}, Uses: map[*ast.Ident]types.Object{}, Selections: map[*ast.SelectorExpr]*types.Selection{}}
	config := types.Config{Importer: importer.Default()}
	pkg, err := config.Check("example.com/lib", fset, files, info)

	if err != nil {
		t.Fatalf("type-check: %v", err)
	}

	return computeInlineArrayAdmission(files, fset, pkg, info, pkgDir, options), pkg
}

func admittedKeys(a *inlineArrayAdmission) string {
	if a == nil {
		return "<nil>"
	}

	return strings.Join(a.keyOrder, " ")
}

// sqlite's S22 shape, converted whole; a holder nests it by value beside a PLAIN struct, which joins the closure (N4).
const admitS22 = `package lib

import "unsafe"

type Plain struct{ X, Y int32 }

type S22 struct {
	FforceAlignment [0]int16
	Fa              [201]int8
	F__ccgo_pad2    [1]byte
}

type Holder struct {
	B byte
	S S22
	P Plain
}

type NotConverted struct{ A [4]int32 }

func read(p unsafe.Pointer) *Holder { return (*Holder)(p) }
`

func TestInlineArrayAdmissionTakesTheClosure(t *testing.T) {
	a, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", admitS22})

	if got, want := admittedKeys(a), "example.com/lib.Holder example.com/lib.Plain example.com/lib.S22"; got != want {
		t.Fatalf("admitted %q, want %q (NotConverted is never converted whole, so auto leaves it)", got, want)
	}

	if a.holding != 2 {
		t.Errorf("holding %d, want 2 (Holder and S22; Plain holds no array)", a.holding)
	}

	if a.admits(types.Typ[types.Int]) {
		t.Errorf("a basic type is never admitted")
	}
}

func TestInlineArrayAdmissionOverrides(t *testing.T) {
	forced, _ := admitFixture(t, "", Options{inlineArrays: inlineArrayOverride{force: []string{"example.com/..."}}}, [2]string{"lib.go", admitS22})

	if got, want := admittedKeys(forced), "example.com/lib.Holder example.com/lib.NotConverted example.com/lib.Plain example.com/lib.S22"; got != want {
		t.Errorf("forced: admitted %q, want %q (the declaration-local population joins)", got, want)
	}

	off, _ := admitFixture(t, "", Options{inlineArrays: inlineArrayOverride{off: []string{"example.com/lib"}}}, [2]string{"lib.go", admitS22})

	if off != nil {
		t.Errorf("-example.com/lib: admitted %q, want nothing", admittedKeys(off))
	}

	goRoot := t.TempDir()
	std, _ := admitFixture(t, filepath.Join(goRoot, "src", "lib"), Options{goRoot: goRoot, inlineArrays: inlineArrayOverride{force: []string{"example.com/lib"}}}, [2]string{"lib.go", admitS22})

	if std != nil {
		t.Errorf("a package under GOROOT/src: admitted %q, want nothing, even forced", admittedKeys(std))
	}
}

func TestInlineArrayAdmissionIgnoresTestFiles(t *testing.T) {
	a, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", strings.Replace(admitS22, "func read(p unsafe.Pointer) *Holder { return (*Holder)(p) }", "var _ unsafe.Pointer", 1)},
		[2]string{"lib_test.go", "package lib\n\nimport \"unsafe\"\n\nfunc readT(p unsafe.Pointer) *S22 { return (*S22)(p) }\n"})

	if a == nil || len(a.keyOrder) != 0 {
		t.Errorf("a conversion only in a _test.go file admitted %q; the decision never comes from tests", admittedKeys(a))
	}
}

// GO2CS2009: a selected type that a cap keeps out stays as today, with a warning naming the field and the cap.
func TestInlineArrayAdmissionWarnsOnACap(t *testing.T) {
	a, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", `package lib

import "unsafe"

type Big struct {
	N   int32
	Tab [2048]int32
}

func read(p unsafe.Pointer) *Big { return (*Big)(p) }
`})

	if len(a.keyOrder) != 0 {
		t.Fatalf("admitted %q over the array cap", admittedKeys(a))
	}

	if len(a.warnings) != 1 || !strings.Contains(a.warnings[0].text, "field Tab: [2048]int32 is 8192 bytes, over the 4096-byte array cap") {
		t.Fatalf("warnings %v, want one naming field Tab and the array cap", a.warnings)
	}
}

// C1's review, m2: go2cs declares ONE C# struct for a generic Go struct, shared by every instantiation, and that
// declaration stays non-inline (visitStructType keys the origin). Admitting the key G[int] would drop the copy's ΔClone
// while its storage is still array<T>: silent aliasing. So an instantiated generic seed stays out, by name, as forced
// mode already keeps generic declarations out.
func TestInlineArrayAdmissionKeepsOutAnInstantiatedGeneric(t *testing.T) {
	a, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", `package lib

import "unsafe"

type G[T any] struct {
	V   T
	Tab [4]int32
}

func read(p unsafe.Pointer) *G[int] { return (*G[int])(p) }
`})

	if len(a.keyOrder) != 0 {
		t.Fatalf("admitted %q, an instantiation of a generic struct", admittedKeys(a))
	}

	if len(a.warnings) != 1 || !strings.Contains(a.warnings[0].text, "G[int] is an instantiation of a generic struct") {
		t.Fatalf("warnings %v, want one naming the instantiation", a.warnings)
	}
}

func TestInlineArrayAdmissionWarnsOnAForeignClosure(t *testing.T) {
	a, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", `package lib

import (
	"sync"
	"unsafe"
)

type Holds struct {
	Buf [8]byte
	Mu  sync.Mutex
}

func read(p unsafe.Pointer) *Holds { return (*Holds)(p) }
`})

	if len(a.keyOrder) != 0 {
		t.Fatalf("admitted %q with a foreign struct in its closure", admittedKeys(a))
	}

	if len(a.warnings) != 1 || !strings.Contains(a.warnings[0].text, "sync.Mutex, declared in another package") {
		t.Fatalf("warnings %v, want one naming sync.Mutex", a.warnings)
	}
}

// S7's control: S22's shape on one side of a reinterpret chain, admitted, and a capped struct on the other.
func TestInlineArrayAdmissionRefusesAMixedPair(t *testing.T) {
	a, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", `package lib

import "unsafe"

type S22 struct {
	FforceAlignment [0]int16
	Fa              [201]int8
	F__ccgo_pad2    [1]byte
}

type Big struct {
	Fa  [201]int8
	Tab [8192]uint8
}

func both(b *Big) *S22 { return (*S22)(unsafe.Pointer(b)) }
`})

	if got := admittedKeys(a); got != "example.com/lib.S22" {
		t.Fatalf("admitted %q, want S22 alone (Big is capped)", got)
	}

	if len(a.refusals) != 1 || !strings.Contains(a.refusals[0].text, "reinterprets S22, whose arrays are held inline, as Big, whose arrays are not") {
		t.Fatalf("refusals %v, want one naming S22 and Big", a.refusals)
	}

	if err := reportInlineArrayAdmission(a); err == nil || !strings.Contains(err.Error(), diagnosticInlineArrayMixedPair) {
		t.Fatalf("report error %v, want the package refused with %s", err, diagnosticInlineArrayMixedPair)
	}

	// Both sides admitted, or only one side holding arrays (a struct read as bytes), is not mixed.
	clean, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", `package lib

import "unsafe"

type A struct{ X [4]int32 }

type B struct{ Y [4]int32 }

func same(b *B) *A { return (*A)(unsafe.Pointer(b)) }

func asBytes(a *A) *[16]byte { return (*[16]byte)(unsafe.Pointer(a)) }
`})

	if len(clean.refusals) != 0 || admittedKeys(clean) != "example.com/lib.A example.com/lib.B" {
		t.Fatalf("clean pairs: refusals %v, admitted %q", clean.refusals, admittedKeys(clean))
	}
}

func TestParseInlineArrayOverride(t *testing.T) {
	o, err := parseInlineArrayOverride(" modernc.org/sqlite/lib , -example.com/x/..., ")

	if err != nil || fmt.Sprint(o.force, o.off) != "[modernc.org/sqlite/lib] [example.com/x/...]" {
		t.Fatalf("parse: %v %v %v", o.force, o.off, err)
	}

	if !o.matches(o.off, "example.com/x/y") || !o.matches(o.off, "example.com/x") || o.matches(o.off, "example.com/xy") {
		t.Errorf("a /... pattern matches the path and below it only")
	}

	if _, err := parseInlineArrayOverride("-"); err == nil {
		t.Errorf("a bare - is an error")
	}
}

// The census arm: GO2CS_INLINE_CENSUS=<dir>:<pattern>[,<pattern>...] loads real packages (GO2CS_INLINE_CENSUS_ENV
// passes the loaded module's GOMODCACHE and GOFLAGS to go list) and prints each package's decision, the reinterpret pairs, and the fmt and reflect uses that reach an
// admitted type (COORD, 2026-11-13 05:18Z). Skipped unless set; P1 is read from it.
func TestInlineArrayAdmissionCensus(t *testing.T) {
	spec := os.Getenv("GO2CS_INLINE_CENSUS")

	if spec == "" {
		t.Skip("GO2CS_INLINE_CENSUS=<dir>:<patterns> runs the admission census over real packages")
	}

	dir, patterns, _ := strings.Cut(spec, ":")
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo, Dir: dir}

	// The loaded module's own cache and flags, kept off the test binary's build (GO2CS_INLINE_CENSUS_ENV=K=V;K=V).
	if extra := os.Getenv("GO2CS_INLINE_CENSUS_ENV"); extra != "" {
		cfg.Env = append(os.Environ(), strings.Split(extra, ";")...)
	}
	pkgs, err := packages.Load(cfg, strings.Split(patterns, ",")...)

	if err != nil {
		t.Fatal(err)
	}

	options := Options{targetPlatform: "linux/amd64", inlineArrays: inlineArrayOverride{}}

	if force := os.Getenv("GO2CS_INLINE_CENSUS_FORCE"); force != "" {
		options.inlineArrays.force = strings.Split(force, ",")
	}

	sort.Slice(pkgs, func(i, j int) bool { return pkgs[i].PkgPath < pkgs[j].PkgPath })

	for _, pkg := range pkgs {
		if pkg.TypesInfo == nil {
			continue
		}

		a := computeInlineArrayAdmission(pkg.Syntax, pkg.Fset, pkg.Types, pkg.TypesInfo, pkg.Dir, options)

		if a == nil {
			fmt.Printf("CENSUS %s: excluded\n", pkg.PkgPath)
			continue
		}

		fmtUses, reflectUses := 0, 0

		for _, file := range pkg.Syntax {
			ast.Inspect(file, func(n ast.Node) bool {
				call, ok := n.(*ast.CallExpr)

				if !ok {
					return true
				}

				sel, ok := ast.Unparen(call.Fun).(*ast.SelectorExpr)

				if !ok {
					return true
				}

				fn, ok := pkg.TypesInfo.Uses[sel.Sel].(*types.Func)

				if !ok || fn.Pkg() == nil || (fn.Pkg().Path() != "fmt" && fn.Pkg().Path() != "reflect") {
					return true
				}

				for _, arg := range call.Args {
					argType := pkg.TypesInfo.TypeOf(arg)

					if argType == nil {
						continue
					}

					if pointer, ok := argType.Underlying().(*types.Pointer); ok {
						argType = pointer.Elem()
					}

					if a.admits(argType) {
						if fn.Pkg().Path() == "fmt" {
							fmtUses++
						} else {
							reflectUses++
						}

						fmt.Printf("  REFLECTED %s %s.%s(%s)\n", pkg.Fset.Position(call.Pos()), fn.Pkg().Path(), fn.Name(), types.TypeString(argType, nil))
					}
				}

				return true
			})
		}

		fmt.Printf("CENSUS %s: admitted %d (holding arrays %d), GO2CS2009 %d, GO2CS2010 %d, foreign pairs %d, fmt uses %d, reflect uses %d\n",
			pkg.PkgPath, len(a.keyOrder), a.holding, len(a.warnings), len(a.refusals), a.foreign, fmtUses, reflectUses)

		for _, w := range a.warnings {
			fmt.Printf("  GO2CS2009 %s %s\n", w.position, w.text)
		}

		for _, r := range a.refusals {
			fmt.Printf("  GO2CS2010 %s %s\n", r.position, r.text)
		}

		if os.Getenv("GO2CS_INLINE_CENSUS_LIST") != "" {
			for _, key := range a.keyOrder {
				fmt.Printf("  ADMITTED %s holds=%t\n", key, a.holds[key])
			}

			for seed, why := range a.blocked {
				fmt.Printf("  BLOCKED %s :: %s :: %s\n", seed, why, strings.Join(a.members[seed], " | "))
			}
		}
	}
}

// A seed may be an array of structs: lib's unsafe.Pointer(&_aMutex), a [12]struct{...}, admits the element struct (the
// census found the first pass dropped such seeds).
func TestInlineArrayAdmissionTakesAnArrayOfStructsSeed(t *testing.T) {
	a, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", `package lib

import "unsafe"

var table = [12]struct {
	Spacer [0][128]int8
	M      int64
	Pad    [88]byte
}{}

func base() unsafe.Pointer { return unsafe.Pointer(&table) }
`})

	if got, want := admittedKeys(a), "anon:struct{Spacer [0][128]int8; M int64; Pad [88]byte}"; got != want {
		t.Fatalf("admitted %q, want %q", got, want)
	}
}

// An anonymous struct reached through an ALIAS another package declares (ccgo's `type T = struct{...}`) is that
// package's to lay out: converting it here admits nothing here (the census's sqlite-package case).
func TestInlineArrayAdmissionLeavesAForeignAliasToItsPackage(t *testing.T) {
	fset := token.NewFileSet()
	parse := func(name, source string) *ast.File {
		file, err := parser.ParseFile(fset, name, source, 0)

		if err != nil {
			t.Fatal(err)
		}

		return file
	}

	libFile := parse("lib.go", "package lib\n\ntype T = struct {\n\tA [4]int32\n\tB int64\n}\n")
	libPkg, err := (&types.Config{Importer: importer.Default()}).Check("example.com/lib", fset, []*ast.File{libFile}, nil)

	if err != nil {
		t.Fatal(err)
	}

	useFile := parse("use.go", "package use\n\nimport (\n\t\"unsafe\"\n\n\t\"example.com/lib\"\n)\n\nfunc read(p unsafe.Pointer) *lib.T { return (*lib.T)(p) }\n")
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

	a := computeInlineArrayAdmission([]*ast.File{useFile}, fset, usePkg, info, "", Options{})

	if len(a.keyOrder) != 0 || len(a.warnings) != 0 {
		t.Fatalf("use admitted %q (warnings %v); lib.T is lib's to lay out", admittedKeys(a), a.warnings)
	}
}
