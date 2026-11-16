// Header-store census (W8 BOARD row 3): which code stores a string header's Data and then its Len through a POINTER
// to the header, the shape go2cs's StringHeaderBox commits on every field store. Type-based (go/packages), per target.
//
// Usage: hdrcensus [-goos G -goarch A] [-tests] <pattern>...
//
// A HEADER STORE is an assignment (= or op=, ++/--) whose left side is a field selector x.F where x's type is a POINTER
// to one of the header types below, read through go/types, never by name. F is classified by its declaration index:
// 0 data, 1 len, 2 cap. A header held as a VALUE (var h reflect.StringHeader; h.Data = ...) is reported apart: it is a
// plain struct, and go2cs commits nothing on its stores.
//
// Per function body (a FuncDecl or FuncLit, nested literals apart), and per header EXPRESSION (types.ExprString of x),
// each data store is classified by the stores of the same expression around it, in source order:
//   data-then-len  a len store follows it     (the row-3 hazard when the header's old len exceeds what Data names)
//   len-then-data  a len store precedes it and none follows
//   data-only      no len store of that expression in the body
// Data's right side is classified go-derived (unsafe.Pointer(&v), unsafe.StringData/SliceData, a conversion of either,
// or an expression of a Go pointer type) or other (a number, a parameter, a call result: what can name native memory).
package main

import (
	"flag"
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

var stringHeaders = map[string]bool{
	"reflect.StringHeader": true, "internal/unsafeheader.String": true,
	"runtime.stringStruct": true, "internal/runtime/maps.stringStruct": true,
}

var sliceHeaders = map[string]bool{
	"reflect.SliceHeader": true, "internal/unsafeheader.Slice": true, "runtime.slice": true, "runtime.notInHeapSlice": true,
}

type store struct {
	key   string
	field int
	pos   token.Pos
	rhs   ast.Expr
	kind  string // "string" or "slice"
	value bool   // the header is a value, not a pointer
}

func headerKind(t types.Type) (kind string, pointer bool) {
	if p, ok := t.Underlying().(*types.Pointer); ok {
		t = p.Elem()
		pointer = true
	}
	n, ok := types.Unalias(t).(*types.Named)
	if !ok || n.Obj().Pkg() == nil {
		return "", false
	}
	q := n.Obj().Pkg().Path() + "." + n.Obj().Name()
	switch {
	case stringHeaders[q]:
		return "string", pointer
	case sliceHeaders[q]:
		return "slice", pointer
	}
	return "", false
}

func fieldIndex(sel *ast.SelectorExpr, info *types.Info) int {
	s, ok := info.Selections[sel]
	if !ok || s.Kind() != types.FieldVal || len(s.Index()) != 1 {
		return -1
	}
	return s.Index()[0]
}

func goDerived(e ast.Expr, info *types.Info) bool {
	for {
		switch x := e.(type) {
		case *ast.ParenExpr:
			e = x.X
			continue
		case *ast.UnaryExpr:
			return x.Op == token.AND
		case *ast.CallExpr:
			if fn, ok := x.Fun.(*ast.SelectorExpr); ok {
				if id, ok := fn.X.(*ast.Ident); ok && id.Name == "unsafe" && (fn.Sel.Name == "StringData" || fn.Sel.Name == "SliceData") {
					return true
				}
			}
			if tv, ok := info.Types[x.Fun]; ok && tv.IsType() && len(x.Args) == 1 {
				e = x.Args[0] // a conversion: look through it
				continue
			}
			return false
		}
		if t := info.TypeOf(e); t != nil {
			if _, ok := t.Underlying().(*types.Pointer); ok {
				return true
			}
		}
		return false
	}
}

func main() {
	goos := flag.String("goos", "", "GOOS to load for")
	goarch := flag.String("goarch", "", "GOARCH to load for")
	tests := flag.Bool("tests", false, "include _test.go files")
	flag.Parse()

	env := append(os.Environ(), "CGO_ENABLED=0")
	if *goos != "" {
		env = append(env, "GOOS="+*goos)
	}
	if *goarch != "" {
		env = append(env, "GOARCH="+*goarch)
	}
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo, Env: env, Tests: *tests}
	pkgs, err := packages.Load(cfg, flag.Args()...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}

	counts := map[string]int{}
	sites := map[string][]string{}
	seen := map[string]bool{}
	files := 0

	for _, p := range pkgs {
		for _, f := range p.Syntax {
			name := p.Fset.Position(f.Pos()).Filename
			if seen[name] {
				continue
			}
			seen[name] = true
			files++

			var bodies []*ast.BlockStmt
			ast.Inspect(f, func(n ast.Node) bool {
				switch fn := n.(type) {
				case *ast.FuncDecl:
					if fn.Body != nil {
						bodies = append(bodies, fn.Body)
					}
				case *ast.FuncLit:
					bodies = append(bodies, fn.Body)
				}
				return true
			})

			for _, body := range bodies {
				var stores []store
				ast.Inspect(body, func(n ast.Node) bool {
					if _, ok := n.(*ast.FuncLit); ok && n != ast.Node(body) {
						return false // a nested literal is its own body
					}
					var lhs []ast.Expr
					var rhs []ast.Expr
					switch s := n.(type) {
					case *ast.AssignStmt:
						lhs, rhs = s.Lhs, s.Rhs
					case *ast.IncDecStmt:
						lhs = []ast.Expr{s.X}
					default:
						return true
					}
					for i, l := range lhs {
						sel, ok := l.(*ast.SelectorExpr)
						if !ok {
							continue
						}
						t := p.TypesInfo.TypeOf(sel.X)
						if t == nil {
							continue
						}
						kind, pointer := headerKind(t)
						if kind == "" {
							continue
						}
						idx := fieldIndex(sel, p.TypesInfo)
						if idx < 0 {
							continue
						}
						var r ast.Expr
						if len(rhs) == len(lhs) {
							r = rhs[i]
						}
						stores = append(stores, store{key: types.ExprString(sel.X), field: idx, pos: sel.Pos(), rhs: r, kind: kind, value: !pointer})
					}
					return true
				})

				for i, s := range stores {
					if s.field != 0 {
						continue
					}
					var class string
					switch {
					case s.value:
						class = s.kind + "-value-header"
					default:
						before, after := false, false
						for j, o := range stores {
							if o.key == s.key && o.field == 1 {
								if j < i {
									before = true
								} else if j > i {
									after = true
								}
							}
						}
						switch {
						case after && s.kind == "string":
							class = s.kind + "-data-then-len-" + freshness(body, s, p.TypesInfo)
						case after:
							class = s.kind + "-data-then-len"
						case before:
							class = s.kind + "-len-then-data"
						default:
							class = s.kind + "-data-only"
						}
					}
					src := "other"
					if s.rhs != nil && goDerived(s.rhs, p.TypesInfo) {
						src = "go-derived"
					}
					class += "/" + src
					counts[class]++
					pos := p.Fset.Position(s.pos)
					sites[class] = append(sites[class], fmt.Sprintf("%s:%d:%d  %s.Data-store key=%s", trim(pos.Filename), pos.Line, pos.Column, s.kind, s.key))
				}
			}
		}
	}

	fmt.Printf("target %s/%s tests=%v; files %d\n", *goos, *goarch, *tests, files)
	var keys []string
	for k := range counts {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	if len(keys) == 0 {
		fmt.Println("   none")
	}
	for _, k := range keys {
		fmt.Printf("   %-40s %d\n", k, counts[k])
	}
	for _, k := range keys {
		fmt.Printf("\n== %s (%d)\n", k, counts[k])
		sort.Strings(sites[k])
		for _, s := range sites[k] {
			fmt.Println("   " + s)
		}
	}
}

// freshness answers "fresh-empty" when the header store cannot see an old Len above 0: the header expression is a
// local identifier defined in this body as (*H)(unsafe.Pointer(&x)), x is a local `var x string` with no value, nothing
// assigns x between its declaration and the store, and no loop encloses the store without also enclosing x's
// declaration (a loop would carry the previous iteration's Len). Anything else is "maybe-nonempty".
func freshness(body *ast.BlockStmt, s store, info *types.Info) string {
	const unknown = "maybe-nonempty"
	if strings.ContainsAny(s.key, ".()[]*") {
		return unknown
	}
	var target *ast.Ident
	ast.Inspect(body, func(n ast.Node) bool {
		as, ok := n.(*ast.AssignStmt)
		if !ok || target != nil || len(as.Lhs) != 1 || len(as.Rhs) != 1 {
			return true
		}
		if id, ok := as.Lhs[0].(*ast.Ident); !ok || id.Name != s.key || as.Pos() > s.pos {
			return true
		}
		e := as.Rhs[0]
		if c, ok := e.(*ast.CallExpr); ok && len(c.Args) == 1 {
			if c2, ok := c.Args[0].(*ast.CallExpr); ok && len(c2.Args) == 1 {
				if u, ok := c2.Args[0].(*ast.UnaryExpr); ok && u.Op == token.AND {
					if x, ok := u.X.(*ast.Ident); ok {
						target = x
					}
				}
			}
		}
		return true
	})
	if target == nil {
		return unknown
	}
	obj := info.ObjectOf(target)
	v, ok := obj.(*types.Var)
	if !ok || v.Parent() == nil || v.Parent() == v.Pkg().Scope() {
		return unknown
	}
	if b, ok := v.Type().Underlying().(*types.Basic); !ok || b.Kind() != types.String {
		return unknown
	}
	declPos := v.Pos()
	fresh := false
	ast.Inspect(body, func(n ast.Node) bool {
		if vs, ok := n.(*ast.ValueSpec); ok && len(vs.Values) == 0 {
			for _, nm := range vs.Names {
				if nm.Pos() == declPos {
					fresh = true
				}
			}
		}
		return true
	})
	if !fresh {
		return unknown
	}
	assigned, looped := false, false
	ast.Inspect(body, func(n ast.Node) bool {
		switch x := n.(type) {
		case *ast.AssignStmt:
			for _, l := range x.Lhs {
				if id, ok := l.(*ast.Ident); ok && info.ObjectOf(id) == obj && x.Pos() > declPos && x.Pos() < s.pos {
					assigned = true
				}
			}
		case *ast.ForStmt, *ast.RangeStmt:
			if n.Pos() <= s.pos && s.pos < n.End() && !(n.Pos() <= declPos && declPos < n.End()) {
				looped = true
			}
		}
		return true
	})
	if assigned || looped {
		return unknown
	}
	return "fresh-empty"
}

func trim(path string) string {
	path = strings.ReplaceAll(path, "\\", "/")
	if i := strings.Index(path, "/src/"); i >= 0 {
		return path[i+5:]
	}
	return path
}
