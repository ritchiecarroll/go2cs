// i0d: two checks for the review of C2's I0 part D (read-only census).
//  (a) locals and parameters, in lib's own non-test files, whose type is a struct declared in lib that holds an array
//      at any depth and whose Go size is over 1 KiB (the 7.7.4 by-value cap's population, approximated: not the
//      admission's closure, so an upper bound).
//  (b) uses, in packages OTHER than lib, of an array field whose owning struct was declared in lib, by face.
package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"sort"

	"golang.org/x/tools/go/packages"
)

func holdsArray(t types.Type, d int) bool {
	if d > 16 {
		return false
	}
	switch u := t.Underlying().(type) {
	case *types.Array:
		return true
	case *types.Struct:
		for i := 0; i < u.NumFields(); i++ {
			if holdsArray(u.Field(i).Type(), d+1) {
				return true
			}
		}
	}
	return false
}

func main() {
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo | packages.NeedDeps | packages.NeedImports}
	cfg.Tests = true
	pkgs, err := packages.Load(cfg, os.Args[1:]...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
	lib := os.Getenv("DECL")
	sizes := types.SizesFor("gc", os.Getenv("GOARCH"))
	seen := map[string]bool{}
	counts := map[string]int{}
	ex := map[string]string{}
	add := func(k, pos string) {
		if seen[k+pos] {
			return
		}
		seen[k+pos] = true
		counts[k]++
		if os.Getenv("ALL") == "1" { fmt.Println("SITE", k, pos) }
		if ex[k] == "" {
			ex[k] = pos
		}
	}
	for _, p := range pkgs {
		for _, f := range p.Syntax {
			fname := p.Fset.Position(f.Pos()).Filename
			test := len(fname) > 8 && fname[len(fname)-8:] == "_test.go"
			if p.PkgPath == lib {
				if test {
					add("lib _test.go file", fname)
				}
				ast.Inspect(f, func(n ast.Node) bool {
					id, ok := n.(*ast.Ident)
					if !ok {
						return true
					}
					v, ok := p.TypesInfo.Defs[id].(*types.Var)
					if !ok || v.IsField() || v.Parent() == p.Types.Scope() {
						return true
					}
					if _, isStruct := v.Type().Underlying().(*types.Struct); isStruct && holdsArray(v.Type(), 0) {
						if sz := sizes.Sizeof(v.Type()); sz > 1024 && sz <= 4096 {
							add("(a) lib local/param, array-holding struct, 1 KiB < size <= 4 KiB", p.Fset.Position(id.Pos()).String())
						} else if sz > 4096 {
							add("(a) ... over 4 KiB (beyond the struct cap)", p.Fset.Position(id.Pos()).String())
						}
					}
					return true
				})
				if !test {
					continue
				}
			}
			var stack []ast.Node
			ast.Inspect(f, func(n ast.Node) bool {
				if n == nil {
					stack = stack[:len(stack)-1]
					return true
				}
				stack = append(stack, n)
				sel, ok := n.(*ast.SelectorExpr)
				if !ok {
					return true
				}
				s := p.TypesInfo.Selections[sel]
				if s == nil || s.Kind() != types.FieldVal {
					return true
				}
				fv := s.Obj().(*types.Var)
				if fv.Pkg() == nil || fv.Pkg().Path() != lib {
					return true
				}
				if _, isArr := fv.Type().Underlying().(*types.Array); !isArr {
					return true
				}
				face := "other"
				if len(stack) >= 2 {
					switch par := stack[len(stack)-2].(type) {
					case *ast.IndexExpr:
						face = "index"
					case *ast.CallExpr:
						if id, ok := par.Fun.(*ast.Ident); ok && (id.Name == "len" || id.Name == "cap") {
							face = "len/cap"
						} else {
							face = "whole value as an argument"
						}
					case *ast.RangeStmt:
						face = "range"
					case *ast.SliceExpr:
						face = "slicing"
					case *ast.UnaryExpr:
						face = "address or other unary"
					case *ast.AssignStmt:
						face = "whole-value assign"
					}
				}
				where := p.PkgPath
				if test {
					where += " (_test.go)"
				}
				add(fmt.Sprintf("(b) %s: array field of a DECL struct used, %s", where, face), p.Fset.Position(sel.Pos()).String())
				return true
			})
		}
	}
	var ks []string
	for k := range counts {
		ks = append(ks, k)
	}
	sort.Strings(ks)
	for _, k := range ks {
		fmt.Printf("%6d  %s   e.g. %s\n", counts[k], k, ex[k])
	}
}
