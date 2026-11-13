// inlineArrayFaces_test.go - Gbtc
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
	"go/token"
	"go/types"
	"os"
	"sort"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// facesFixture converts seven structs whole; one uses its array fields only through spelled faces, five each through
// one unspelled face, and one through a face only its _test.go file uses.
const facesFixture = `package lib

import (
	"fmt"
	"unsafe"
)

type Spelled struct {
	A [4]int32
	B [2]byte
}

type Sliced struct{ A [4]int32 }

type Addressed struct{ A [4]int32 }

type ElemAddr struct{ A [4]int32 }

type Passed struct{ A [4]int32 }

type Stored struct{ A [4]int32 }

type TestOnly struct{ A [4]int32 }

func take(a [4]int32) int32 { return a[0] }

func use(p unsafe.Pointer) {
	s := (*Spelled)(p)
	s.A[1] = 2
	s.A[0] += 1
	_ = len(s.A) + cap(s.A)
	for i, v := range s.A {
		_, _ = i, v
	}
	fmt.Println(s.B)
	_ = unsafe.Offsetof(Spelled{}.B) + unsafe.Sizeof(s.A) + unsafe.Alignof(s.A)

	sl := (*Sliced)(p)
	_ = sl.A[1:]

	ad := (*Addressed)(p)
	_ = &ad.A

	ea := (*ElemAddr)(p)
	_ = &ea.A[2]

	pa := (*Passed)(p)
	_ = take(pa.A)

	st := (*Stored)(p)
	v := st.A
	_ = v

	_ = (*TestOnly)(p)
}
`

// The automatic rule keeps out exactly the types whose array fields the package's non-test files use through an
// unspelled face, each with a GO2CS2009 warning naming the face and the site; forced mode does not consult faces.
func TestInlineArrayAdmissionKeepsOutAnUnspelledFace(t *testing.T) {
	testFile := [2]string{"lib_test.go", "package lib\n\nfunc probe(x *TestOnly) []int32 { return x.A[:] }\n"}
	a, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", facesFixture}, testFile)

	if got := admittedKeys(a); got != "example.com/lib.Spelled example.com/lib.TestOnly" {
		t.Fatalf("admitted %q, want Spelled and TestOnly", got)
	}

	want := map[string]string{
		"Sliced":    "field A is sliced (x.f[:]) at lib.go:",
		"Addressed": "field A is its address (&x.f) at lib.go:",
		"ElemAddr":  "field A is an element's address (&x.f[i]) at lib.go:",
		"Passed":    "field A is a whole value passed as an argument at lib.go:",
		"Stored":    "field A is a whole value (copied, stored, compared or returned) at lib.go:",
	}

	if len(a.warnings) != len(want) {
		t.Fatalf("%d warnings, want %d: %v", len(a.warnings), len(want), a.warnings)
	}

	for _, warning := range a.warnings {
		name := strings.Fields(warning.text)[0]

		if expected, ok := want[name]; !ok || !strings.Contains(warning.text, "keeps array<T> storage: "+expected) {
			t.Errorf("warning for %s reads %q, want it to name %q", name, warning.text, expected)
		}
	}

	forced, _ := admitFixture(t, "", Options{inlineArrays: inlineArrayOverride{force: []string{"example.com/lib"}}}, [2]string{"lib.go", facesFixture}, testFile)

	if len(forced.keyOrder) != 7 || len(forced.warnings) != 0 {
		t.Fatalf("forced mode consulted faces: admitted %q, warnings %v", admittedKeys(forced), forced.warnings)
	}
}

// TestInlineArrayFaceCensus counts, over real packages, how the code uses the array fields of the structs the admission
// admits: each face is one the emission must spell for an inline array (index, len, range, element address, slicing,
// field address, whole-value read or store). Env-gated like TestInlineArrayAdmissionCensus, whose variables it takes.
func TestInlineArrayFaceCensus(t *testing.T) {
	spec := os.Getenv("GO2CS_INLINE_CENSUS")

	if spec == "" {
		t.Skip("GO2CS_INLINE_CENSUS=<dir>:<patterns> runs the face census over real packages")
	}

	dir, patterns, _ := strings.Cut(spec, ":")
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo, Dir: dir}

	if extra := os.Getenv("GO2CS_INLINE_CENSUS_ENV"); extra != "" {
		cfg.Env = append(os.Environ(), strings.Split(extra, ";")...)
	}

	pkgs, err := packages.Load(cfg, strings.Split(patterns, ",")...)

	if err != nil {
		t.Fatal(err)
	}

	options := Options{targetPlatform: "linux/amd64"}

	if force := os.Getenv("GO2CS_INLINE_CENSUS_FORCE"); force != "" {
		options.inlineArrays.force = strings.Split(force, ",")
	}

	sort.Slice(pkgs, func(i, j int) bool { return pkgs[i].PkgPath < pkgs[j].PkgPath })
	sizes := types.SizesFor("gc", "amd64")

	for _, pkg := range pkgs {
		if pkg.TypesInfo == nil {
			continue
		}

		a := computeInlineArrayAdmission(pkg.Syntax, pkg.Fset, pkg.Types, pkg.TypesInfo, pkg.Dir, options)

		if a == nil || len(a.keyOrder) == 0 {
			continue
		}

		faces := map[string]int{}
		examples := map[string][]string{}
		note := func(face string, pos token.Pos) {
			faces[face]++

			if len(examples[face]) < 3 {
				examples[face] = append(examples[face], pkg.Fset.Position(pos).String())
			}
		}

		// The admitted struct a selector's field belongs to, and whether the field is an array (and of length 0).
		admittedArrayField := func(sel *ast.SelectorExpr) (bool, bool) {
			selection := pkg.TypesInfo.Selections[sel]

			if selection == nil || selection.Kind() != types.FieldVal {
				return false, false
			}

			array, ok := selection.Obj().Type().Underlying().(*types.Array)

			if !ok {
				return false, false
			}

			recv := selection.Recv()

			if pointer, ok := recv.Underlying().(*types.Pointer); ok {
				recv = pointer.Elem()
			}

			// The field's own struct is the last one on the path (an embedded promotion walks through others).
			owner := recv

			for _, index := range selection.Index()[:len(selection.Index())-1] {
				st, ok := owner.Underlying().(*types.Struct)

				if !ok {
					return false, false
				}

				owner = st.Field(index).Type()

				if pointer, ok := owner.Underlying().(*types.Pointer); ok {
					owner = pointer.Elem()
				}
			}

			return a.admits(owner), array.Len() == 0
		}

		for _, file := range pkg.Syntax {
			var stack []ast.Node

			ast.Inspect(file, func(n ast.Node) bool {
				if n == nil {
					stack = stack[:len(stack)-1]
					return true
				}

				stack = append(stack, n)

				sel, ok := n.(*ast.SelectorExpr)

				if !ok {
					return true
				}

				admitted, zero := admittedArrayField(sel)

				if !admitted {
					return true
				}

				prefix := ""

				if zero {
					prefix = "zero-length "
				}

				// The nearest enclosing node that is not a parenthesis, and the one above it.
				up := func(from int) (ast.Node, int) {
					for i := from; i >= 0; i-- {
						if _, ok := stack[i].(*ast.ParenExpr); !ok {
							return stack[i], i
						}
					}

					return nil, -1
				}

				parent, at := up(len(stack) - 2)
				grand, _ := up(at - 1)

				switch p := parent.(type) {
				case *ast.IndexExpr:
					if ast.Unparen(p.X) != sel {
						note(prefix+"used as an index", sel.Pos())
						break
					}

					if unary, ok := grand.(*ast.UnaryExpr); ok && unary.Op == token.AND {
						note(prefix+"element address &x.f[i]", sel.Pos())
					} else if assign, ok := grand.(*ast.AssignStmt); ok && assign.Tok != token.ASSIGN && assign.Tok != token.DEFINE && containsLHS(assign, p) {
						note(prefix+"element compound assignment", sel.Pos())

						if containsReceive(assign.Rhs) {
							note(prefix+"element compound assignment with a receive", sel.Pos())
						}
					} else if inner, ok := grand.(*ast.SelectorExpr); ok && ast.Unparen(inner.X) == p {
						note(prefix+"element selector x.f[i].g", sel.Pos())
					} else if _, ok := grand.(*ast.IndexExpr); ok {
						note(prefix+"element index x.f[i][j]", sel.Pos())
					} else if slice, ok := grand.(*ast.SliceExpr); ok && ast.Unparen(slice.X) == p {
						note(prefix+"element slicing x.f[i][:]", sel.Pos())
					} else {
						note(prefix+"index x.f[i]", sel.Pos())
					}
				case *ast.CallExpr:
					if id, ok := ast.Unparen(p.Fun).(*ast.Ident); ok && (id.Name == "len" || id.Name == "cap") {
						if _, isBuiltin := pkg.TypesInfo.Uses[id].(*types.Builtin); isBuiltin {
							note(prefix+"len/cap", sel.Pos())
							break
						}
					}

					if ast.Unparen(p.Fun) == sel {
						break
					}

					note(prefix+"whole value as an argument", sel.Pos())
				case *ast.RangeStmt:
					if ast.Unparen(p.X) == sel {
						note(prefix+"range", sel.Pos())
					}
				case *ast.SliceExpr:
					if ast.Unparen(p.X) == sel {
						note(prefix+"slicing x.f[:]", sel.Pos())
					}
				case *ast.UnaryExpr:
					if p.Op == token.AND {
						note(prefix+"field address &x.f", sel.Pos())
					} else {
						note(prefix+"whole value, other", sel.Pos())
					}
				case *ast.AssignStmt:
					if containsLHS(p, sel) {
						note(prefix+"whole-value store x.f = v", sel.Pos())
					} else {
						note(prefix+"whole-value read v = x.f", sel.Pos())
					}
				case *ast.SelectorExpr:
					// x.f.g is not an array face; x.f is the X of another selector only through a method value.
					note(prefix+"method value on the array", sel.Pos())
				case *ast.BinaryExpr:
					note(prefix+"whole-value comparison", sel.Pos())
				case *ast.ReturnStmt:
					note(prefix+"whole value returned", sel.Pos())
				case *ast.CompositeLit, *ast.KeyValueExpr:
					note(prefix+"whole value in a composite literal", sel.Pos())
				case *ast.ValueSpec:
					note(prefix+"whole-value read var v = x.f", sel.Pos())
				default:
					note(fmt.Sprintf("%swhole value under %T", prefix, parent), sel.Pos())
				}

				return true
			})
		}

		// By-value locals and parameters of an admitted type over 1 KiB (7.7.4's by-value cap).
		bigLocals := 0

		for ident, obj := range pkg.TypesInfo.Defs {
			v, ok := obj.(*types.Var)

			if !ok || v.IsField() || v.Parent() == pkg.Types.Scope() || v.Parent() == nil {
				continue
			}

			if a.admits(v.Type()) && sizes.Sizeof(v.Type()) > 1024 {
				bigLocals++

				if len(examples["by-value local over 1 KiB"]) < 3 {
					examples["by-value local over 1 KiB"] = append(examples["by-value local over 1 KiB"], fmt.Sprintf("%s %s", pkg.Fset.Position(ident.Pos()), types.TypeString(v.Type(), packageQualifier(pkg.Types))))
				}
			}
		}

		// The emission's own population: the admitted structs' array fields (nonzero, their lengths, and [0]T carriers),
		// and the composite literals of an admitted struct that set an array field (keyed or positional).
		arrayFields, carriers, literalSets := 0, 0, 0
		lengths := map[int64]bool{}

		for _, key := range a.keyOrder {
			st, ok := a.typeOf(key)

			if !ok {
				continue
			}

			for i := 0; i < st.NumFields(); i++ {
				if array, ok := st.Field(i).Type().Underlying().(*types.Array); ok {
					if array.Len() == 0 {
						carriers++
					} else {
						arrayFields++
						lengths[array.Len()] = true
					}
				}
			}
		}

		for _, file := range pkg.Syntax {
			ast.Inspect(file, func(n ast.Node) bool {
				lit, ok := n.(*ast.CompositeLit)

				if !ok || !a.admits(pkg.TypesInfo.TypeOf(lit)) {
					return true
				}

				st, _ := pkg.TypesInfo.TypeOf(lit).Underlying().(*types.Struct)

				for i, elt := range lit.Elts {
					field := -1

					if kv, ok := elt.(*ast.KeyValueExpr); ok {
						if id, ok := kv.Key.(*ast.Ident); ok {
							for j := 0; st != nil && j < st.NumFields(); j++ {
								if st.Field(j).Name() == id.Name {
									field = j
								}
							}
						}
					} else {
						field = i
					}

					if st != nil && field >= 0 && field < st.NumFields() {
						if array, ok := st.Field(field).Type().Underlying().(*types.Array); ok && array.Len() > 0 {
							literalSets++
							note("composite literal sets an array field", elt.Pos())
						}
					}
				}

				return true
			})
		}

		fmt.Printf("  admitted array fields %d over %d lengths, [0]T carriers %d, literal sets %d\n", arrayFields, len(lengths), carriers, literalSets)

		var names []string

		for face := range faces {
			names = append(names, face)
		}

		sort.Strings(names)
		fmt.Printf("FACES %s (admitted %d):\n", pkg.PkgPath, len(a.keyOrder))

		for _, face := range names {
			fmt.Printf("  %-52s %6d   e.g. %s\n", face, faces[face], strings.Join(examples[face], ", "))
		}

		fmt.Printf("  %-52s %6d   e.g. %s\n", "by-value local over 1 KiB", bigLocals, strings.Join(examples["by-value local over 1 KiB"], ", "))
	}
}

func containsLHS(assign *ast.AssignStmt, target ast.Expr) bool {
	for _, lhs := range assign.Lhs {
		if ast.Unparen(lhs) == target {
			return true
		}
	}

	return false
}

func containsReceive(exprs []ast.Expr) bool {
	found := false

	for _, expr := range exprs {
		ast.Inspect(expr, func(n ast.Node) bool {
			if unary, ok := n.(*ast.UnaryExpr); ok && unary.Op == token.ARROW {
				found = true
			}

			return !found
		})
	}

	return found
}
