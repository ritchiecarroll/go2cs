// addrderef: every `&*p` in Go source, read off the syntax: a unary & whose operand, parentheses peeled, is a
// dereference. Classes: bare `&*p`, paren `&(*p)` (any parentheses between the two operators), conv when the
// dereferenced operand is itself a call `&*(*T)(x)` (a conversion or a call, not told apart without types).
// Usage: addrderef <dir>... ; skips directories named testdata and, with SKIPCMD=1, a top-level cmd.
package main

import (
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"io/fs"
	"os"
	"path/filepath"
	"sort"
	"strings"
)

func main() {
	counts := map[string]int{}
	files, bad := 0, 0
	var out []string
	for _, root := range os.Args[1:] {
		filepath.WalkDir(root, func(path string, d fs.DirEntry, err error) error {
			if err != nil {
				return nil
			}
			if d.IsDir() {
				if d.Name() == "testdata" || (os.Getenv("SKIPCMD") != "" && path == filepath.Join(root, "cmd")) {
					return filepath.SkipDir
				}
				return nil
			}
			if !strings.HasSuffix(path, ".go") {
				return nil
			}
			fset := token.NewFileSet()
			f, err := parser.ParseFile(fset, path, nil, parser.SkipObjectResolution)
			if err != nil {
				bad++
				return nil
			}
			files++
			ast.Inspect(f, func(n ast.Node) bool {
				u, ok := n.(*ast.UnaryExpr)
				if !ok || u.Op != token.AND {
					return true
				}
				x, parens := u.X, false
				for {
					p, ok := x.(*ast.ParenExpr)
					if !ok {
						break
					}
					x, parens = p.X, true
				}
				star, ok := x.(*ast.StarExpr)
				if !ok {
					return true
				}
				class := "bare"
				if parens {
					class = "paren"
				}
				inner := star.X
				for {
					p, ok := inner.(*ast.ParenExpr)
					if !ok {
						break
					}
					inner = p.X
				}
				if _, ok := inner.(*ast.CallExpr); ok {
					class += "-conv"
				}
				if strings.HasSuffix(path, "_test.go") {
					class += "-test"
				}
				counts[class]++
				rel, _ := filepath.Rel(root, path)
				out = append(out, fmt.Sprintf("%s\t%s:%d", class, rel, fset.Position(u.Pos()).Line))
				return true
			})
			return nil
		})
	}
	sort.Strings(out)
	for _, l := range out {
		fmt.Println(l)
	}
	var keys []string
	for k := range counts {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	var parts []string
	for _, k := range keys {
		parts = append(parts, fmt.Sprintf("%s=%d", k, counts[k]))
	}
	fmt.Fprintf(os.Stderr, "files=%d unparsed=%d sites=%d %s\n", files, bad, len(out), strings.Join(parts, " "))
}
