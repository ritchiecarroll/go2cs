// asmcensus: bodyless (assembly or linkname) functions in the NON-std packages of a dependency closure (sqlite-cs S21).
//   asmcensus [-tags t1,t2] <patterns>      run from a module that requires the patterns' modules
package main

import (
	"flag"
	"fmt"
	"go/ast"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

func main() {
	tags := flag.String("tags", "", "build tags")
	flag.Parse()
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedDeps | packages.NeedImports | packages.NeedModule}
	if *tags != "" {
		cfg.BuildFlags = []string{"-tags=" + *tags}
	}
	roots, err := packages.Load(cfg, flag.Args()...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
	var rows []string
	modules := map[string]bool{}
	seen := map[string]bool{}
	packages.Visit(roots, nil, func(p *packages.Package) {
		if seen[p.PkgPath] || p.Module == nil || p.Module.Main && false {
			return
		}
		seen[p.PkgPath] = true
		if !strings.Contains(strings.SplitN(p.PkgPath, "/", 2)[0], ".") {
			return // std
		}
		hasAsm := false
		for _, o := range p.OtherFiles {
			if strings.HasSuffix(o, ".s") {
				hasAsm = true
			}
		}
		for _, f := range p.Syntax {
			linknames := map[string]bool{}
			for _, cg := range f.Comments {
				for _, c := range cg.List {
					if strings.HasPrefix(c.Text, "//go:linkname ") {
						if fs := strings.Fields(c.Text); len(fs) >= 2 {
							linknames[fs[1]] = true
						}
					}
				}
			}
			for _, d := range f.Decls {
				fd, ok := d.(*ast.FuncDecl)
				if !ok || fd.Body != nil {
					continue
				}
				kind := "assembly"
				if linknames[fd.Name.Name] {
					kind = "linkname"
				} else if !hasAsm {
					kind = "bodyless, no .s file"
				}
				name := fd.Name.Name
				if fd.Recv != nil {
					name = "(method) " + name
				}
				mod := p.Module.Path
				modules[mod] = true
				rows = append(rows, fmt.Sprintf("%s\t%s\t%s\t%s\t%s", mod, p.PkgPath, kind, name, filepath.Base(p.Fset.Position(fd.Pos()).Filename)))
			}
		}
	})
	sort.Strings(rows)
	byKind := map[string]int{}
	byPkg := map[string]int{}
	for _, r := range rows {
		c := strings.Split(r, "\t")
		byKind[c[2]]++
		if c[2] == "assembly" {
			byPkg[c[1]]++
		}
	}
	fmt.Printf("tags %q: bodyless functions %d in %d modules; by kind %v\n", *tags, len(rows), len(modules), byKind)
	var ps []string
	for p, n := range byPkg {
		ps = append(ps, fmt.Sprintf("%4d  %s", n, p))
	}
	sort.Strings(ps)
	for _, p := range ps {
		fmt.Println("  assembly functions in " + p)
	}
	for _, r := range rows {
		fmt.Println("ROW\t" + r)
	}
}
