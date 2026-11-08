// cgoprobe loads one cgo package with golang.org/x/tools/go/packages and reports what go/types sees.
// usage: cgoprobe <dir> <CGO_ENABLED value> <mode: coord|go2cs>
package main

import (
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
	dir, cgo, modeName := os.Args[1], os.Args[2], os.Args[3]
	mode := packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo | packages.NeedCompiledGoFiles
	if modeName == "go2cs" {
		mode = packages.LoadAllSyntax
	}
	cfg := &packages.Config{Mode: mode, Dir: dir, Env: append(os.Environ(), "CGO_ENABLED="+cgo, "GOFLAGS=")}
	pkgs, err := packages.Load(cfg, ".")
	fmt.Printf("== CGO_ENABLED=%s mode=%s load err=%v packages=%d\n", cgo, modeName, err, len(pkgs))
	for _, p := range pkgs {
		fmt.Printf("pkg %s\n", p.PkgPath)
		for _, e := range p.Errors {
			fmt.Printf("  error: %s\n", e)
		}
		fmt.Printf("  GoFiles: %v\n", base(p.GoFiles))
		fmt.Printf("  CompiledGoFiles: %v\n", base(p.CompiledGoFiles))
		fmt.Printf("  OtherFiles: %v\n", base(p.OtherFiles))
		fmt.Printf("  Syntax files: %d\n", len(p.Syntax))
		for _, f := range p.Syntax {
			name := p.Fset.Position(f.Package).Filename
			hasC := false
			for _, imp := range f.Imports {
				if imp.Path.Value == `"C"` {
					hasC = true
				}
			}
			fmt.Printf("    %s  decls=%d importC=%v\n", filepath.Base(name), len(f.Decls), hasC)
		}
		if p.Types == nil {
			continue
		}
		scope := p.Types.Scope()
		fmt.Printf("  scope names: %d\n", len(scope.Names()))
		for _, n := range scope.Names() {
			obj := scope.Lookup(n)
			if !strings.HasPrefix(n, "_C") {
				fmt.Printf("    [go] %s: %s\n", n, obj.Type())
				continue
			}
			desc := obj.Type().String()
			if c, ok := obj.(*types.Const); ok {
				desc += " = " + c.Val().ExactString()
			}
			if tn, ok := obj.(*types.TypeName); ok {
				desc = "type -> " + tn.Type().Underlying().String()
			}
			fmt.Printf("    [%T] %s: %s\n", obj, n, desc)
		}
		// what the Go code's references resolve to, with the position they report
		if p.TypesInfo == nil {
			continue
		}
		type use struct{ pos, line string }
		seen := map[string]use{}
		for id, obj := range p.TypesInfo.Uses {
			if strings.HasPrefix(id.Name, "_C") {
				pos := p.Fset.Position(id.Pos())
				t := ""
				if obj != nil {
					t = obj.Type().String()
					if c, ok := obj.(*types.Const); ok {
						t += " = " + c.Val().ExactString()
					}
				}
				seen[id.Name] = use{fmt.Sprintf("%s:%d", filepath.Base(pos.Filename), pos.Line), t}
			}
		}
		keys := make([]string, 0, len(seen))
		for k := range seen {
			keys = append(keys, k)
		}
		sort.Strings(keys)
		fmt.Printf("  uses of _C* identifiers in the package's syntax: %d\n", len(keys))
		for _, k := range keys {
			fmt.Printf("    %s @ %s : %s\n", k, seen[k].pos, seen[k].line)
		}
		// the user function Use: does its body reference C.* through a selector still?
		for _, f := range p.Syntax {
			ast.Inspect(f, func(n ast.Node) bool {
				if s, ok := n.(*ast.SelectorExpr); ok {
					if x, ok := s.X.(*ast.Ident); ok && x.Name == "C" {
						fmt.Printf("  selector C.%s still in syntax at %s\n", s.Sel.Name, p.Fset.Position(s.Pos()))
					}
				}
				return true
			})
		}
	}
}

func base(paths []string) []string {
	out := make([]string, len(paths))
	for i, p := range paths {
		out[i] = filepath.Base(p)
		if strings.Contains(p, "go-build") {
			out[i] += " (build cache)"
		}
	}
	return out
}
