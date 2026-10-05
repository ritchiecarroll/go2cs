// Census for a conversion whose callee is a FUNCTION-LOCAL named type: `sPtr(p)` with `type sPtr *s`
// declared inside a function. go2cs names the constructor callee from the raw ident (`new sPtr(ps)`)
// while the type is lifted as `main_sPtr` (CS0246). Reports the pointer-underlying sites (the shape
// measured) and, separately, every local-named-type conversion callee for context.
// Usage: localnp <dir> <tests:0|1> <pattern>...
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
	ptrSites := map[string]bool{}
	allSites := map[string]string{}
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
				id, ok := ast.Unparen(ce.Fun).(*ast.Ident)
				if !ok {
					return true
				}
				tn, ok := p.TypesInfo.ObjectOf(id).(*types.TypeName)
				if !ok || tn.Pkg() == nil || tn.Parent() == tn.Pkg().Scope() || tn.Parent() == types.Universe {
					return true
				}
				pos := p.Fset.Position(ce.Pos()).String()
				u := tn.Type().Underlying()
				allSites[pos] = fmt.Sprintf("%T", u)
				if _, isPtr := u.(*types.Pointer); isPtr {
					ptrSites[pos] = true
				}
				return true
			})
		}
	}
	keys := []string{}
	for k := range ptrSites {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	for _, k := range keys {
		fmt.Println("PTR", k)
	}
	kinds := map[string]int{}
	for _, k := range allSites {
		kinds[k]++
	}
	fmt.Printf("TOTAL-PTR %d  ALL-LOCAL-CONVERSIONS %d %v\n", len(keys), len(allSites), kinds)
}
