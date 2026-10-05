// Census for the named-pointer equality shape: `x == y` / `x != y` where an operand's type is a NAMED type
// whose underlying is a pointer (`type itemPtr *item`) and neither operand is nil. go2cs emitted
// `Ꮡx == new itemPtr(...)`, the address of a box that does not exist (CS0103).
// Usage: namedptreq <dir> <tests:0|1> <pattern>...
package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"sort"

	"golang.org/x/tools/go/packages"
)

func namedPtr(t types.Type) bool {
	n, ok := types.Unalias(t).(*types.Named)
	if !ok {
		return false
	}
	_, ptr := n.Underlying().(*types.Pointer)
	return ptr
}

func isNil(info *types.Info, e ast.Expr) bool {
	tv := info.Types[e]
	return tv.IsNil()
}

func main() {
	cfg := &packages.Config{Mode: packages.LoadAllSyntax, Dir: os.Args[1], Tests: os.Args[2] == "1"}
	pkgs, err := packages.Load(cfg, os.Args[3:]...)
	if err != nil {
		fmt.Println("LOAD ERROR", err)
		os.Exit(2)
	}
	sites := map[string]bool{}
	for _, p := range pkgs {
		if p.TypesInfo == nil {
			continue
		}
		for _, f := range p.Syntax {
			ast.Inspect(f, func(n ast.Node) bool {
				be, ok := n.(*ast.BinaryExpr)
				if !ok || (be.Op != token.EQL && be.Op != token.NEQ) {
					return true
				}
				if isNil(p.TypesInfo, be.X) || isNil(p.TypesInfo, be.Y) {
					return true
				}
				if namedPtr(p.TypesInfo.TypeOf(be.X)) || namedPtr(p.TypesInfo.TypeOf(be.Y)) {
					sites[fmt.Sprintf("%s", p.Fset.Position(be.Pos()))] = true
				}
				return true
			})
		}
	}
	keys := []string{}
	for k := range sites {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	for _, k := range keys {
		fmt.Println(k)
	}
	fmt.Printf("TOTAL %d\n", len(keys))
}
