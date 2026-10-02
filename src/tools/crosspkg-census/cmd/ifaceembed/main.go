// ifaceembed: the converter-side footprint of stamping [GoEmbedded] on INTERFACE embeds (the TRAIN M marker).
// Counts every embedded field (any ast.StructType, named or anonymous struct types alike) whose type is an interface,
// deduplicated by position, split production / test by file name. Each such field's emitted line would change.
// Usage: ifaceembed [-root dir] <patterns...>
package main

import (
	"flag"
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

func main() {
	root := flag.String("root", "", "directory to load from")
	flag.Parse()
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo, Tests: true, Dir: *root}
	pkgs, err := packages.Load(cfg, flag.Args()...)
	if err != nil {
		fmt.Println("load:", err)
		os.Exit(2)
	}
	loadErrors := 0
	packages.Visit(pkgs, nil, func(p *packages.Package) { loadErrors += len(p.Errors) })
	seen := map[string]bool{}
	var rows []string
	prod, test, composite := 0, 0, 0
	prodFiles, testFiles := map[string]bool{}, map[string]bool{}
	for _, p := range pkgs {
		if p.TypesInfo == nil {
			continue
		}
		for _, f := range p.Syntax {
			ast.Inspect(f, func(n ast.Node) bool {
				st, ok := n.(*ast.StructType)
				if !ok {
					return true
				}
				for _, fld := range st.Fields.List {
					if len(fld.Names) != 0 {
						continue
					}
					t := p.TypesInfo.TypeOf(fld.Type)
					if t == nil || !types.IsInterface(t) {
						continue
					}
					pos := p.Fset.Position(fld.Pos())
					k := fmt.Sprintf("%s:%d:%d", pos.Filename, pos.Line, pos.Column)
					if seen[k] {
						continue
					}
					seen[k] = true
					rel := pos.Filename
					if r, err := filepath.Rel(filepath.Join(os.Getenv("GOROOT"), "src"), pos.Filename); err == nil && !strings.HasPrefix(r, "..") {
						rel = filepath.ToSlash(r)
					}
					if strings.HasSuffix(pos.Filename, "_test.go") {
						test++
						testFiles[pos.Filename] = true
					} else {
						prod++
						prodFiles[pos.Filename] = true
					}
					// R3: a COMPOSITE interface (built from embedded interfaces) is emitted as C# interface
					// inheritance with an empty body, so its methods are only visible through AllInterfaces.
					comp := ""
					if it, ok := t.Underlying().(*types.Interface); ok && it.NumEmbeddeds() > 0 {
						comp = "\tcomposite"
						composite++
					}
					rows = append(rows, fmt.Sprintf("%s:%d\t%s%s", rel, pos.Line, types.TypeString(t, nil), comp))
				}
				return true
			})
		}
	}
	sort.Strings(rows)
	for _, r := range rows {
		fmt.Println(r)
	}
	fmt.Printf("\npackages loaded %d (load errors %d); interface embeds %d (composite %d): production %d in %d files, test %d in %d files\n",
		len(pkgs), loadErrors, len(rows), composite, prod, len(prodFiles), test, len(testFiles))
}
