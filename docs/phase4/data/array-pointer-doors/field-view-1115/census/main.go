// F1 census: every `(*[N]T)(unsafe.Pointer(&x.f))` in the packages loaded from a module directory, classified
// by the converter's own field-view predicate (nativeFieldArrayView.go) and by the form that uses it.
package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

func main() {
	dir := os.Args[1]
	cfg := &packages.Config{Mode: packages.LoadAllSyntax | packages.NeedModule, Dir: dir, Env: append(os.Environ(), "GOOS=linux", "GOARCH=amd64", "CGO_ENABLED=0", "GOFLAGS=-mod=mod")}
	pkgs, err := packages.Load(cfg, os.Args[2:]...)

	if err != nil {
		fmt.Println("load:", err)
		os.Exit(1)
	}

	fmt.Printf("roots: %d, errors: %d\n", len(pkgs), packages.PrintErrors(pkgs))
	counts := map[string]int{}
	var lines []string
	files, loaded := 0, 0

	packages.Visit(pkgs, nil, func(p *packages.Package) {
		if p.Module == nil || p.TypesInfo == nil {
			return
		}

		loaded++

		for _, f := range p.Syntax {
			files++
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

				ptr, ok := tv.Type.Underlying().(*types.Pointer)

				if !ok {
					return true
				}

				arr, ok := types.Unalias(ptr.Elem()).Underlying().(*types.Array)

				if !ok {
					return true
				}

				inner, ok := ast.Unparen(call.Args[0]).(*ast.CallExpr)

				if !ok || len(inner.Args) != 1 {
					return true
				}

				if itv, ok := p.TypesInfo.Types[inner.Fun]; !ok || !itv.IsType() || itv.Type.String() != "unsafe.Pointer" {
					return true
				}

				unary, ok := ast.Unparen(inner.Args[0]).(*ast.UnaryExpr)

				if !ok || unary.Op != token.AND {
					return true
				}

				sel, ok := ast.Unparen(unary.X).(*ast.SelectorExpr)

				if !ok {
					return true
				}

				selection := p.TypesInfo.Selections[sel]

				if selection == nil || selection.Kind() != types.FieldVal {
					return true
				}

				fieldType := selection.Type()
				class := "other-elem"

				switch {
				case types.Identical(fieldType, arr.Elem()):
					class = "same-elem"
				default:
					if fa, ok := fieldType.Underlying().(*types.Array); ok && types.Identical(fa.Elem(), arr.Elem()) {
						class = "field-is-array-of-elem"
					}
				}

				numeric := false

				if basic, ok := arr.Elem().Underlying().(*types.Basic); ok && basic.Info()&types.IsNumeric != 0 {
					numeric = true
				}

				root := "root-other"

				if ident, ok := ast.Unparen(sel.X).(*ast.Ident); ok {
					if _, ok := p.TypesInfo.TypeOf(ident).Underlying().(*types.Pointer); ok {
						root = "root-pointer-ident"
					} else {
						root = "root-value-ident"
					}
				}

				hook := class != "same-elem" && numeric && len(selection.Index()) == 1 && root == "root-pointer-ident"
				use := "other"

				// the nearest ancestor that is not a paren
				for i := len(stack) - 2; i >= 0; i-- {
					if _, ok := stack[i].(*ast.ParenExpr); ok {
						continue
					}

					switch parent := stack[i].(type) {
					case *ast.IndexExpr:
						if ast.Unparen(parent.X) == ast.Expr(call) {
							use = "direct-index"
						}
					case *ast.SliceExpr:
						use = "direct-slice"
					case *ast.StarExpr:
						use = "direct-deref"
					case *ast.AssignStmt:
						use = "bound-local"
					case *ast.ValueSpec:
						use = "bound-local"
					case *ast.CallExpr:
						use = "call-argument"
					}

					break
				}

				refBearing := "root-struct-unknown"

				if rt := p.TypesInfo.TypeOf(sel.X); rt != nil {
					if pt, ok := rt.Underlying().(*types.Pointer); ok {
						rt = pt.Elem()
					}

					if holdsReference(rt, 0) {
						refBearing = "root-struct-holds-reference"
					} else {
						refBearing = "root-struct-reference-free"
					}
				}

				key := fmt.Sprintf("hook=%v %s %s %s %s", hook, class, root, use, refBearing)
				counts[key]++
				pos := p.Fset.Position(call.Pos())
				lines = append(lines, fmt.Sprintf("%s\t%s\t%s:%d", key, p.PkgPath, shortFile(pos.Filename), pos.Line))
				return true
			})
		}
	})

	fmt.Printf("packages with a module: %d, files: %d\n", loaded, files)
	keys := make([]string, 0, len(counts))

	for k := range counts {
		keys = append(keys, k)
	}

	sort.Strings(keys)
	total := 0

	for _, k := range keys {
		fmt.Printf("%6d  %s\n", counts[k], k)
		total += counts[k]
	}

	fmt.Printf("%6d  total field-address array views\n", total)
	sort.Strings(lines)
	os.WriteFile("sites.txt", []byte(strings.Join(lines, "\n")+"\n"), 0o644)
}

func shortFile(name string) string {
	if i := strings.LastIndex(name, "/"); i >= 0 {
		return name[i+1:]
	}

	return name
}

func holdsReference(t types.Type, depth int) bool {
	if depth > 8 {
		return true
	}

	switch u := t.Underlying().(type) {
	case *types.Basic:
		return u.Kind() == types.String || u.Kind() == types.UnsafePointer
	case *types.Array:
		// a fixed array field is a managed array<T> in the converted struct
		return depth > 0 || holdsReference(u.Elem(), depth+1)
	case *types.Struct:
		for i := 0; i < u.NumFields(); i++ {
			if holdsReference(u.Field(i).Type(), depth+1) {
				return true
			}
		}

		return false
	default:
		return true
	}
}
