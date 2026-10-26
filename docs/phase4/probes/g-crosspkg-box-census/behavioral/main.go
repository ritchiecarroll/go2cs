// beh: exported package-level vars declared in NON-main, non-test Go files under a root (syntax only).
package main

import (
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"io/fs"
	"os"
	"path/filepath"
	"strings"
)

func main() {
	root := os.Args[1]
	fset := token.NewFileSet()
	total, mainVars := 0, 0
	projects := map[string]int{}

	filepath.WalkDir(root, func(path string, d fs.DirEntry, err error) error {
		if err != nil || d.IsDir() || !strings.HasSuffix(path, ".go") || strings.HasSuffix(path, "_test.go") {
			return nil
		}

		f, perr := parser.ParseFile(fset, path, nil, parser.SkipObjectResolution)

		if perr != nil {
			return nil
		}

		for _, decl := range f.Decls {
			gen, ok := decl.(*ast.GenDecl)

			if !ok || gen.Tok != token.VAR {
				continue
			}

			for _, spec := range gen.Specs {
				for _, id := range spec.(*ast.ValueSpec).Names {
					if !id.IsExported() {
						continue
					}

					if f.Name.Name == "main" {
						mainVars++
						continue
					}

					total++
					rel, _ := filepath.Rel(root, path)
					project := strings.SplitN(filepath.ToSlash(rel), "/", 2)[0]
					projects[project]++
					fmt.Printf("%s\t%s\t%s\n", filepath.ToSlash(rel), f.Name.Name, id.Name)
				}
			}
		}

		return nil
	})

	fmt.Fprintf(os.Stderr, "non-main exported vars %d in %d projects; package-main exported vars %d (no box needed)\n", total, len(projects), mainVars)
}
