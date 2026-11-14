// i0f: two faces I0's D3 emission leaves to a C# compile error, counted over non-test files (upper bound: every struct
// holding an array, not the admission's set):
//   K  a struct literal whose value for an array field is a KEYED array literal ([N]T{2: x});
//   C  len/cap of, or range over, an array field whose operand contains a call or a receive.
package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

func main() {
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo}
	pkgs, err := packages.Load(cfg, os.Args[1:]...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
	counts := map[string]int{}
	ex := map[string]string{}
	add := func(k, pos string) {
		counts[k]++
		if ex[k] == "" {
			ex[k] = pos
		}
	}
	isArrField := func(e ast.Expr, info *types.Info) bool {
		sel, ok := ast.Unparen(e).(*ast.SelectorExpr)
		if !ok {
			return false
		}
		s := info.Selections[sel]
		if s == nil || s.Kind() != types.FieldVal {
			return false
		}
		_, ok = s.Obj().Type().Underlying().(*types.Array)
		return ok
	}
	hasCall := func(e ast.Expr) bool {
		found := false
		ast.Inspect(e, func(n ast.Node) bool {
			switch x := n.(type) {
			case *ast.CallExpr:
				found = true
			case *ast.UnaryExpr:
				if x.Op == token.ARROW {
					found = true
				}
			}
			return !found
		})
		return found
	}
	for _, p := range pkgs {
		info := p.TypesInfo
		for _, f := range p.Syntax {
			if strings.HasSuffix(p.Fset.Position(f.Pos()).Filename, "_test.go") {
				continue
			}
			ast.Inspect(f, func(n ast.Node) bool {
				switch x := n.(type) {
				case *ast.CompositeLit:
					st, ok := types.Unalias(info.TypeOf(x)).Underlying().(*types.Struct)
					if !ok {
						return true
					}
					for i, elt := range x.Elts {
						val, fi := elt, i
						if kv, ok := elt.(*ast.KeyValueExpr); ok {
							val, fi = kv.Value, -1
							if id, ok := kv.Key.(*ast.Ident); ok {
								for j := 0; j < st.NumFields(); j++ {
									if st.Field(j).Name() == id.Name {
										fi = j
									}
								}
							}
						}
						if fi < 0 || fi >= st.NumFields() {
							continue
						}
						if _, isArr := st.Field(fi).Type().Underlying().(*types.Array); !isArr {
							continue
						}
						if lit, ok := ast.Unparen(val).(*ast.CompositeLit); ok {
							for _, e := range lit.Elts {
								if _, keyed := e.(*ast.KeyValueExpr); keyed {
									add(p.PkgPath+"|K keyed array literal for an array field", p.Fset.Position(lit.Pos()).String())
									break
								}
							}
						}
					}
				case *ast.CallExpr:
					if id, ok := ast.Unparen(x.Fun).(*ast.Ident); ok && (id.Name == "len" || id.Name == "cap") && len(x.Args) == 1 {
						if _, b := info.Uses[id].(*types.Builtin); b && isArrField(x.Args[0], info) && hasCall(x.Args[0]) {
							add(p.PkgPath+"|C len/cap of an array field whose operand calls", p.Fset.Position(x.Pos()).String())
						}
					}
				case *ast.RangeStmt:
					if isArrField(x.X, info) && hasCall(x.X) {
						add(p.PkgPath+"|C range over an array field whose operand calls", p.Fset.Position(x.Pos()).String())
					}
				}
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
