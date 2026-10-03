// stmtlines <file.go>: the start line of every Go statement (blocks excluded), the lines a debugger should stop on.
package main

import (
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"sort"
)

func main() {
	fset := token.NewFileSet()
	f, err := parser.ParseFile(fset, os.Args[1], nil, 0)
	if err != nil {
		panic(err)
	}
	lines := map[int]bool{}
	ast.Inspect(f, func(n ast.Node) bool {
		if s, ok := n.(ast.Stmt); ok {
			if _, block := s.(*ast.BlockStmt); !block {
				lines[fset.Position(s.Pos()).Line] = true
			}
		}
		return true
	})
	var out []int
	for l := range lines {
		out = append(out, l)
	}
	sort.Ints(out)
	fmt.Println(out)
}
