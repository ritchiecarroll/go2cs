package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"runtime"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

// importerCensus counts, over std and its tests, every site that names ANOTHER package's exported package-level var
// whose type is not a pointer, by what the site does with it:
//
//	addr         &pkg.V, or &pkg.V.f / &pkg.V[i] (the address of the var or of storage inside it)
//	ptrcall      pkg.V.M(...) with M a pointer-receiver method (the call takes &pkg.V)
//	methodvalue  pkg.V.M not called, M a pointer-receiver method (binds &pkg.V)
//	read         anything else (emission unchanged by the box: the importer reads pkg.V either way)
//
// The first three are the sites whose emission moves when the owner boxes V (the importer binds pkg.ᏑV).
func importerCensus(goos string) {
	cfg := &packages.Config{
		Mode:       packages.NeedName | packages.NeedFiles | packages.NeedTypes | packages.NeedSyntax | packages.NeedTypesInfo,
		Env:        append(os.Environ(), "GOOS="+goos, "GOARCH=amd64", "CGO_ENABLED=0"),
		BuildFlags: []string{"-tags=purego"},
		Tests:      true,
	}

	pkgs, err := packages.Load(cfg, "std")

	if err != nil {
		fmt.Fprintln(os.Stderr, "load:", err)
		os.Exit(2)
	}

	count := map[string]int{}
	seen := map[string]bool{}

	fmt.Println("goos\tsite\tuse\tvar\tkind\ttest")

	for _, p := range pkgs {
		if strings.HasPrefix(p.PkgPath, "cmd/") || strings.HasPrefix(p.PkgPath, "vendor/") || p.TypesInfo == nil {
			continue
		}

		for _, file := range p.Syntax {
			fileName := p.Fset.Position(file.Pos()).Filename
			isTest := strings.HasSuffix(fileName, "_test.go")

			// Generated files (a test variant's testmain lives in the build cache) are not GOROOT sources and are
			// never converted; skipping them also keeps every recorded path GOROOT-relative.
			if _, inGoroot := trimRoot(fileName); !inGoroot {
				continue
			}

			var stack []ast.Node

			ast.Inspect(file, func(n ast.Node) bool {
				if n == nil {
					stack = stack[:len(stack)-1]
					return true
				}

				stack = append(stack, n)
				sel, ok := n.(*ast.SelectorExpr)

				if !ok {
					return true
				}

				if _, isPkg := p.TypesInfo.Uses[identOf(sel.X)].(*types.PkgName); !isPkg {
					return true
				}

				v, ok := p.TypesInfo.Uses[sel.Sel].(*types.Var)

				if !ok || v.Pkg() == nil || v.Parent() != v.Pkg().Scope() || !v.Exported() {
					return true
				}

				kind := "value"

				switch types.Unalias(v.Type()).Underlying().(type) {
				case *types.Pointer:
					return true
				case *types.Interface:
					kind = "interface"
				}

				use := classify(stack, p.TypesInfo)
				pos := p.Fset.Position(sel.Pos())
				rel, _ := trimRoot(pos.Filename)
				site := fmt.Sprintf("%s:%d:%d", rel, pos.Line, pos.Column)
				key := site + "|" + use

				// a file compiled into both the package and its test variant is seen twice
				if seen[key] {
					return true
				}

				seen[key] = true
				fmt.Printf("%s\t%s\t%s\t%s.%s\t%s\t%v\n", goos, site, use, v.Pkg().Path(), v.Name(), kind, isTest)
				count[fmt.Sprintf("use=%-12s kind=%-9s test=%v", use, kind, isTest)]++

				return true
			})
		}
	}

	keys := make([]string, 0, len(count))

	for k := range count {
		keys = append(keys, k)
	}

	sort.Strings(keys)

	for _, k := range keys {
		fmt.Fprintf(os.Stderr, "%s importers %s %d\n", goos, k, count[k])
	}
}

func identOf(e ast.Expr) *ast.Ident {
	id, _ := e.(*ast.Ident)
	return id
}

// classify reads what the enclosing expressions do with pkg.V; stack ends at the pkg.V selector.
func classify(stack []ast.Node, info *types.Info) string {
	i := len(stack) - 1
	cur := stack[i].(ast.Expr)

	// climb through storage INSIDE the var: pkg.V.f, pkg.V[i], (pkg.V)
	for i > 0 {
		parent := stack[i-1]

		switch p := parent.(type) {
		case *ast.ParenExpr:
			cur, i = p, i-1
			continue
		case *ast.IndexExpr:
			if p.X == cur {
				if _, isArray := types.Unalias(info.TypeOf(p.X)).Underlying().(*types.Array); isArray {
					cur, i = p, i-1
					continue
				}
			}
		case *ast.SelectorExpr:
			if p.X == cur {
				if s, ok := info.Selections[p]; ok && s.Kind() == types.FieldVal && !s.Indirect() {
					cur, i = p, i-1
					continue
				}

				if s, ok := info.Selections[p]; ok && s.Kind() == types.MethodVal && !s.Indirect() {
					if sig, ok := s.Obj().Type().(*types.Signature); ok && sig.Recv() != nil {
						if _, ptr := sig.Recv().Type().(*types.Pointer); ptr {
							if i >= 2 {
								if call, ok := stack[i-2].(*ast.CallExpr); ok && call.Fun == p {
									return "ptrcall"
								}
							}

							return "methodvalue"
						}
					}
				}
			}
		case *ast.UnaryExpr:
			if p.Op.String() == "&" {
				return "addr"
			}
		}

		break
	}

	return "read"
}

// trimRoot returns path relative to GOROOT/src, and whether it is under it at all.
func trimRoot(path string) (string, bool) {
	path = strings.ReplaceAll(path, "\\", "/")
	src := strings.ReplaceAll(runtime.GOROOT(), "\\", "/") + "/src/"

	if strings.HasPrefix(strings.ToLower(path), strings.ToLower(src)) {
		return path[len(src):], true
	}

	return "", false
}
