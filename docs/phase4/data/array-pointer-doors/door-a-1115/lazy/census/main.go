// nacensus lists, for the package closure of its patterns, the defined named array types and every conversion
// to unsafe.Pointer whose operand is a pointer to one (the address of a named array handed out as an address).
//
// Usage: nacensus <tag> <pattern>...     (run inside a module that can load the patterns; GOOS/GOARCH select)
// stdout, tab separated: tag, kind (type|site), package, file:line, type name, length, element, operand shape,
// the enclosing function. stderr: totals.
package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

func namedArray(t types.Type) (*types.Named, *types.Array) {
	n, ok := types.Unalias(t).(*types.Named)
	if !ok {
		return nil, nil
	}
	a, ok := n.Underlying().(*types.Array)
	if !ok {
		return nil, nil
	}
	return n, a
}

func shape(info *types.Info, e ast.Expr) string {
	e = ast.Unparen(e)
	switch x := e.(type) {
	case *ast.UnaryExpr:
		if x.Op == token.AND {
			switch y := ast.Unparen(x.X).(type) {
			case *ast.Ident:
				if v, ok := info.Uses[y].(*types.Var); ok {
					if v.Parent() == v.Pkg().Scope() {
						return "addr-of-package-var"
					}
					return "addr-of-local"
				}
				return "addr-of-ident"
			case *ast.SelectorExpr:
				return "addr-of-field"
			case *ast.IndexExpr:
				return "addr-of-element"
			case *ast.CompositeLit:
				return "addr-of-literal"
			}
			return "addr-of-other"
		}
	case *ast.Ident:
		return "pointer-var"
	case *ast.SelectorExpr:
		return "pointer-field"
	case *ast.CallExpr:
		if id, ok := ast.Unparen(x.Fun).(*ast.Ident); ok && id.Name == "new" {
			return "new"
		}
		return "call"
	}
	return "other"
}

func main() {
	tag := os.Args[1]
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes |
		packages.NeedTypesInfo | packages.NeedImports | packages.NeedDeps | packages.NeedModule}
	pkgs, err := packages.Load(cfg, os.Args[2:]...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}
	seen := map[string]*packages.Package{}
	bad := 0
	packages.Visit(pkgs, nil, func(p *packages.Package) {
		seen[p.PkgPath] = p
		bad += len(p.Errors)
	})
	var paths []string
	for k := range seen {
		paths = append(paths, k)
	}
	sort.Strings(paths)
	nTypes, nSites, nStdTypes, nStdSites := 0, 0, 0, 0
	for _, path := range paths {
		p := seen[path]
		std := p.Module == nil
		origin := "module"
		if std {
			origin = "std"
		}
		if p.Types == nil {
			continue
		}
		scope := p.Types.Scope()
		for _, name := range scope.Names() {
			tn, ok := scope.Lookup(name).(*types.TypeName)
			if !ok || tn.IsAlias() {
				continue
			}
			if n, a := namedArray(tn.Type()); n != nil {
				pos := p.Fset.Position(tn.Pos())
				fmt.Printf("%s\ttype\t%s\t%s\t%s:%d\t%s\t%d\t%s\t-\t-\n", tag, origin, path, filepath.Base(pos.Filename), pos.Line, name, a.Len(), a.Elem())
				if std {
					nStdTypes++
				} else {
					nTypes++
				}
			}
		}
		for _, f := range p.Syntax {
			var stack []ast.Node
			ast.Inspect(f, func(n ast.Node) bool {
				if n == nil {
					stack = stack[:len(stack)-1]
					return true
				}
				stack = append(stack, n)
				call, ok := n.(*ast.CallExpr)
				if !ok || len(call.Args) != 1 {
					return true
				}
				tv, ok := p.TypesInfo.Types[call.Fun]
				if !ok || !tv.IsType() {
					return true
				}
				if b, ok := tv.Type.Underlying().(*types.Basic); !ok || b.Kind() != types.UnsafePointer {
					return true
				}
				at := p.TypesInfo.TypeOf(call.Args[0])
				if at == nil {
					return true
				}
				ptr, ok := at.Underlying().(*types.Pointer)
				if !ok {
					return true
				}
				nm, a := namedArray(ptr.Elem())
				if nm == nil {
					return true
				}
				fn := "-"
				for i := len(stack) - 1; i >= 0; i-- {
					if fd, ok := stack[i].(*ast.FuncDecl); ok {
						fn = fd.Name.Name
						break
					}
				}
				pos := p.Fset.Position(call.Pos())
				fmt.Printf("%s\tsite\t%s\t%s\t%s:%d\t%s\t%d\t%s\t%s\t%s\n", tag, origin, path, filepath.Base(pos.Filename), pos.Line,
					strings.TrimPrefix(nm.Obj().Pkg().Path()+".", path+".")+nm.Obj().Name(), a.Len(), a.Elem(), shape(p.TypesInfo, call.Args[0]), fn)
				if std {
					nStdSites++
				} else {
					nSites++
				}
				return true
			})
		}
	}
	fmt.Fprintf(os.Stderr, "%s: %d packages (load errors %d); module packages: %d defined named arrays, %d sites; std packages: %d defined named arrays, %d sites\n",
		tag, len(paths), bad, nTypes, nSites, nStdTypes, nStdSites)
}
