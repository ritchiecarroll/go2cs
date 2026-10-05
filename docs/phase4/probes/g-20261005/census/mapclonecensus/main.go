// Census for the CloneMap seat: calls of maps.Clone whose argument's type is a NAMED map type
// (`type Fields map[string]any`). golib's default CloneMap returned a plain map, so maps.Clone's
// `clone(m).(M)` assertion panicked for exactly these. Also counts direct maps.clone/linkname
// users through a named map, and the total maps.Clone calls for context.
// Usage: mapclonecensus <dir> <tests:0|1> <pattern>...
package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"sort"

	"golang.org/x/tools/go/packages"
)

func main() {
	cfg := &packages.Config{Mode: packages.LoadAllSyntax, Dir: os.Args[1], Tests: os.Args[2] == "1"}
	pkgs, err := packages.Load(cfg, os.Args[3:]...)
	if err != nil {
		fmt.Println("LOAD ERROR", err)
		os.Exit(2)
	}
	named := map[string]string{}
	all := map[string]bool{}
	for _, p := range pkgs {
		if p.TypesInfo == nil {
			continue
		}
		for _, f := range p.Syntax {
			ast.Inspect(f, func(n ast.Node) bool {
				ce, ok := n.(*ast.CallExpr)
				if !ok || len(ce.Args) != 1 {
					return true
				}
				fun := ast.Unparen(ce.Fun)
				if ix, isIndex := fun.(*ast.IndexListExpr); isIndex {
					fun = ix.X
				} else if ix, isIndex := fun.(*ast.IndexExpr); isIndex {
					fun = ix.X
				}
				var obj types.Object
				switch fe := fun.(type) {
				case *ast.SelectorExpr:
					obj = p.TypesInfo.ObjectOf(fe.Sel)
				case *ast.Ident:
					obj = p.TypesInfo.ObjectOf(fe)
				}
				fn, ok := obj.(*types.Func)
				if !ok || fn.Pkg() == nil || fn.Pkg().Path() != "maps" || fn.Name() != "Clone" {
					return true
				}
				pos := p.Fset.Position(ce.Pos()).String()
				all[pos] = true
				t := p.TypesInfo.TypeOf(ce.Args[0])
				if nt, isNamed := types.Unalias(t).(*types.Named); isNamed {
					if _, isMap := nt.Underlying().(*types.Map); isMap {
						named[pos] = nt.String()
					}
				}
				return true
			})
		}
	}
	keys := []string{}
	for k := range named {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	for _, k := range keys {
		fmt.Println("NAMED", k, named[k])
	}
	fmt.Printf("TOTAL-NAMED %d  ALL-maps.Clone %d\n", len(named), len(all))
}
