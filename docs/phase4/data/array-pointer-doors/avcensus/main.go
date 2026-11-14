// avcensus: every conversion (*A)(p), p an unsafe.Pointer and A an array type (named or not), classified by the
// form that consumes it: a slice (three-index, two-index with a high bound, or open), an index, a dereference, or
// the pointer itself bound, passed or returned.
//
//	avcensus <tag> <pattern>...   (run in a module that resolves the patterns; GOOS/GOARCH from the environment)
//
// Output: one TSV row per site on stdout, a summary on stderr.
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

// golib's longest span: Array.MaxLength.
const spanLimit = 0x7FFFFFC7

func isUnsafePointer(t types.Type) bool {
	b, ok := types.Unalias(t).Underlying().(*types.Basic)
	return ok && b.Kind() == types.UnsafePointer
}

func hasPointers(t types.Type, seen map[types.Type]bool) bool {
	t = types.Unalias(t)
	if seen[t] {
		return false
	}
	seen[t] = true
	switch u := t.Underlying().(type) {
	case *types.Basic:
		return u.Kind() == types.String || u.Kind() == types.UnsafePointer
	case *types.Array:
		return hasPointers(u.Elem(), seen)
	case *types.Struct:
		for i := 0; i < u.NumFields(); i++ {
			if hasPointers(u.Field(i).Type(), seen) {
				return true
			}
		}
		return false
	}
	return true
}

func operandKind(info *types.Info, e ast.Expr) string {
	e = ast.Unparen(e)
	switch x := e.(type) {
	case *ast.Ident:
		return "pointer-var"
	case *ast.SelectorExpr:
		return "pointer-field"
	case *ast.CallExpr:
		if tv, ok := info.Types[x.Fun]; ok && tv.IsType() && len(x.Args) == 1 { // unsafe.Pointer(arg)
			arg := ast.Unparen(x.Args[0])
			at := info.TypeOf(arg)
			if at != nil {
				if b, ok := types.Unalias(at).Underlying().(*types.Basic); ok && b.Kind() == types.Uintptr {
					if _, isBin := arg.(*ast.BinaryExpr); isBin {
						return "number-arith"
					}
					return "number"
				}
			}
			if u, ok := arg.(*ast.UnaryExpr); ok && u.Op == token.AND {
				switch ast.Unparen(u.X).(type) {
				case *ast.IndexExpr:
					return "addr-of-element"
				case *ast.SelectorExpr:
					return "addr-of-field"
				}
				return "addr-of-var"
			}
			return "typed-pointer"
		}
		if sel, ok := ast.Unparen(x.Fun).(*ast.SelectorExpr); ok {
			if id, ok := sel.X.(*ast.Ident); ok && id.Name == "unsafe" && sel.Sel.Name == "Add" {
				return "unsafe.Add"
			}
			return "call:" + sel.Sel.Name
		}
		if id, ok := ast.Unparen(x.Fun).(*ast.Ident); ok {
			return "call:" + id.Name
		}
		return "call"
	}
	return fmt.Sprintf("%T", e)
}

func main() {
	tag := os.Args[1]
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo | packages.NeedImports | packages.NeedDeps, Tests: true}
	pkgs, err := packages.Load(cfg, os.Args[2:]...)
	if err != nil {
		fmt.Fprintln(os.Stderr, "load:", err)
		os.Exit(2)
	}
	seenFile := map[string]bool{}
	var rows []string
	loadErrs := 0
	counts := map[string]int{}
	for _, pkg := range pkgs {
		loadErrs += len(pkg.Errors)
		info := pkg.TypesInfo
		for _, file := range pkg.Syntax {
			name := pkg.Fset.Position(file.Pos()).Filename
			if seenFile[name] {
				continue
			}
			seenFile[name] = true
			var stack []ast.Node
			ast.Inspect(file, func(n ast.Node) bool {
				if n == nil {
					stack = stack[:len(stack)-1]
					return true
				}
				stack = append(stack, n)
				call, ok := n.(*ast.CallExpr)
				if !ok || len(call.Args) != 1 {
					return true
				}
				tv, ok := info.Types[call.Fun]
				if !ok || !tv.IsType() {
					return true
				}
				ptr, ok := types.Unalias(tv.Type).Underlying().(*types.Pointer)
				if !ok {
					return true
				}
				arr, ok := types.Unalias(ptr.Elem()).Underlying().(*types.Array)
				if !ok || !isUnsafePointer(info.TypeOf(call.Args[0])) {
					return true
				}
				named := "-"
				if nt, ok := types.Unalias(ptr.Elem()).(*types.Named); ok {
					named = nt.Obj().Name()
				}
				// Walk up through parentheses and an explicit dereference.
				i := len(stack) - 2
				var child ast.Node = call
				deref := false
				for i >= 0 {
					switch p := stack[i].(type) {
					case *ast.ParenExpr:
						child = p
						i--
						continue
					case *ast.StarExpr:
						deref = true
						child = p
						i--
						continue
					}
					break
				}
				form, context := "", "-"
				var parent ast.Node
				if i >= 0 {
					parent = stack[i]
				}
				up := func(k int) ast.Node {
					j := i - k
					for j >= 0 {
						if _, ok := stack[j].(*ast.ParenExpr); ok {
							j--
							continue
						}
						return stack[j]
					}
					return nil
				}
				usage := func(self ast.Node, par ast.Node) string {
					switch g := par.(type) {
					case *ast.CallExpr:
						if id, ok := ast.Unparen(g.Fun).(*ast.Ident); ok && id.Name == "copy" && info.Uses[id] == types.Universe.Lookup("copy") {
							if ast.Unparen(g.Args[0]) == self || g.Args[0] == self {
								return "copy-dst"
							}
							return "copy-src"
						}
						return "arg"
					case *ast.AssignStmt:
						for _, l := range g.Lhs {
							if ast.Unparen(l) == self || l == self {
								return "store"
							}
						}
						return "bound"
					case *ast.ValueSpec:
						return "bound"
					case *ast.UnaryExpr:
						if g.Op == token.AND {
							return "addr"
						}
						return "read"
					case *ast.ReturnStmt:
						return "return"
					case *ast.RangeStmt:
						return "range"
					case *ast.IncDecStmt:
						return "store"
					}
					return "read"
				}
				switch p := parent.(type) {
				case *ast.SliceExpr:
					if p.X == child {
						switch {
						case p.Slice3:
							form = "slice3"
						case p.High != nil:
							form = "slice2"
						default:
							form = "slice-open"
						}
						context = usage(p, up(1))
					}
				case *ast.IndexExpr:
					if p.X == child {
						form = "index"
						context = usage(p, up(1))
					}
				}
				if form == "" {
					if deref {
						form = "deref"
					} else {
						form = "pointer"
					}
					context = usage(child, parent)
				}
				n64, exact := arr.Len(), true
				size := "fits"
				if n64 > spanLimit {
					size = "past-span"
				}
				_ = exact
				pos := pkg.Fset.Position(call.Pos())
				short := pos.Filename
				if k := strings.LastIndex(short, "/"); k >= 0 {
					short = short[k+1:]
				}
				test := strings.HasSuffix(short, "_test.go")
				rows = append(rows, fmt.Sprintf("%s\t%s\t%s:%d\t%s\t%d\t%s\t%s\t%v\t%s\t%s\t%s\t%v",
					tag, pkg.PkgPath, short, pos.Line, named, n64, size, types.TypeString(arr.Elem(), types.RelativeTo(pkg.Types)),
					hasPointers(arr.Elem(), map[types.Type]bool{}), form, context, operandKind(info, call.Args[0]), test))
				key := form + "/" + context
				if test {
					key += " (test)"
				}
				counts[key]++
				return true
			})
		}
	}
	sort.Strings(rows)
	rows = dedupe(rows)
	for _, r := range rows {
		fmt.Println(r)
	}
	fmt.Fprintf(os.Stderr, "%s: %d packages, %d load errors, %d sites\n", tag, len(pkgs), loadErrs, len(rows))
	var keys []string
	for k := range counts {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	for _, k := range keys {
		fmt.Fprintf(os.Stderr, "  %-28s %d\n", k, counts[k])
	}
}

func dedupe(rows []string) []string {
	var out []string
	for i, r := range rows {
		if i == 0 || r != rows[i-1] {
			out = append(out, r)
		}
	}
	return out
}
