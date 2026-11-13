// i0e: in DECL's non-test files, uses that reach BELOW an element of an array field of a struct (x.f[i] followed by a
// selector or index chain), classified by what the chain's top is used for: its address taken (&x.f[i].g), a
// pointer-receiver method called on it (x.f[i].g.M()), sliced, or boxed into an interface (argument of interface type).
// Also counts, for the WHOLE field x.f, an interface boxing (an argument whose parameter is an interface).
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

func isArrayField(e ast.Expr, info *types.Info) bool {
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
	for _, p := range pkgs {
		info := p.TypesInfo
		for _, f := range p.Syntax {
			if strings.HasSuffix(p.Fset.Position(f.Pos()).Filename, "_test.go") {
				continue
			}
			var stack []ast.Node
			ast.Inspect(f, func(n ast.Node) bool {
				if n == nil {
					stack = stack[:len(stack)-1]
					return true
				}
				stack = append(stack, n)
				pos := p.Fset.Position(n.Pos()).String()
				parent := func(k int) ast.Node {
					for j := len(stack) - 1 - k; j >= 0; j-- {
						if _, ok := stack[j].(*ast.ParenExpr); !ok {
							return stack[j]
						}
						k++
					}
					return nil
				}
				_ = parent
				// whole field boxed into an interface
				if call, ok := n.(*ast.CallExpr); ok {
					if sig, ok := info.TypeOf(call.Fun).(*types.Signature); ok {
						for i, a := range call.Args {
							if !isArrayField(a, info) {
								continue
							}
							var pt types.Type
							if sig.Variadic() && i >= sig.Params().Len()-1 {
								if sl, ok := sig.Params().At(sig.Params().Len() - 1).Type().(*types.Slice); ok {
									pt = sl.Elem()
								}
							} else if i < sig.Params().Len() {
								pt = sig.Params().At(i).Type()
							}
							if pt != nil && types.IsInterface(pt) {
								add("whole array field boxed into an interface argument", p.Fset.Position(a.Pos()).String())
							}
						}
					}
				}
				ix, ok := n.(*ast.IndexExpr)
				if !ok || !isArrayField(ix.X, info) {
					return true
				}
				// climb the chain above x.f[i]
				top := ast.Expr(ix)
				depth := 0
				j := len(stack) - 2
				for ; j >= 0; j-- {
					switch q := stack[j].(type) {
					case *ast.ParenExpr:
						top = q
						continue
					case *ast.SelectorExpr:
						if s := info.Selections[q]; s != nil && s.Kind() == types.FieldVal && ast.Unparen(q.X) == ast.Unparen(top) {
							top = q
							depth++
							continue
						}
					case *ast.IndexExpr:
						if ast.Unparen(q.X) == ast.Unparen(top) {
							if _, isArr := info.TypeOf(q.X).Underlying().(*types.Array); isArr {
								top = q
								depth++
								continue
							}
						}
					}
					break
				}
				if depth == 0 || j < 0 {
					return true
				}
				switch q := stack[j].(type) {
				case *ast.UnaryExpr:
					if q.Op == token.AND {
						add("address BELOW an element: &x.f[i].g...", pos)
					}
				case *ast.SliceExpr:
					add("slice BELOW an element: x.f[i].g...[:]", pos)
				case *ast.SelectorExpr:
					if s := info.Selections[q]; s != nil && s.Kind() == types.MethodVal {
						if sig, ok := s.Obj().Type().(*types.Signature); ok && sig.Recv() != nil {
							if _, pr := sig.Recv().Type().Underlying().(*types.Pointer); pr {
								if _, isPtr := info.TypeOf(top).Underlying().(*types.Pointer); !isPtr {
									add("pointer-receiver method BELOW an element: x.f[i].g.M()", pos)
								}
							}
						}
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
