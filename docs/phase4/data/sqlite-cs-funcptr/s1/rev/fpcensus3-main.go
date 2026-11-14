// fpcensus2: every shape that carries a Go func value as a machine word (sqlite-cs S1, revised per the Fable review).
// Classified structurally, never by name, per package, for one GOOS/GOARCH.
//   MINT  m1  __ccgo_fp-style: a function whose body reads an interface parameter's data word ((*[2]uintptr)(…)[1]), and its call sites
//         m2  a func value read as a word: *(*uintptr)(unsafe.Pointer(&X)) or *(*unsafe.Pointer)(…), X a func, or a struct whose ONLY field is a func or a type parameter
//   RECOVER r1  *(*F)(unsafe.Pointer(&S{x})), F a func type, S a struct whose only field is uintptr (named field, or EMBEDDED)
//           r2  (*struct{ f F })(unsafe.Pointer(&S{x})).f  -- the struct-with-func target
//           r3  *(*F)(unsafe.Pointer(&v)), v a uintptr variable or field
//         each: CALLED at once, or STORED (anything else)
//   WRITE  w1  *(*uintptr)(unsafe.Pointer(&f)) = x, f func-typed: a word written THROUGH a func variable (a recovery)
//          w2  *(*F)(unsafe.Pointer(&slot or p)) = fn: a func written INTO a word slot (a mint; v3, 2026-11-13)
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

var counts = map[string]int{}
var examples = map[string]string{}

func add(k string, pos string) {
	counts[k]++
	if _, ok := examples[k]; !ok {
		examples[k] = pos
	}
}

func isUnsafePtr(t types.Type) bool {
	b, ok := t.(*types.Basic)
	return ok && b.Kind() == types.UnsafePointer
}

func isUintptr(t types.Type) bool {
	if t == nil {
		return false
	}
	b, ok := t.Underlying().(*types.Basic)
	return ok && b.Kind() == types.Uintptr
}

func isFunc(t types.Type) bool {
	if t == nil {
		return false
	}
	_, ok := t.Underlying().(*types.Signature)
	return ok
}

// oneField: the single field of a struct type, or nil.
func oneField(t types.Type) *types.Var {
	if t == nil {
		return nil
	}
	st, ok := t.Underlying().(*types.Struct)
	if !ok || st.NumFields() != 1 {
		return nil
	}
	return st.Field(0)
}

func main() {
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo}
	pkgs, err := packages.Load(cfg, os.Args[1:]...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
	for _, p := range pkgs {
		info := p.TypesInfo
		if info == nil {
			continue
		}
		pp := p.PkgPath
		// m1 declarations: a function whose body reads an interface parameter's data word
		m1Funcs := map[*types.Func]bool{}
		for _, f := range p.Syntax {
			for _, d := range f.Decls {
				fd, ok := d.(*ast.FuncDecl)
				if !ok || fd.Body == nil || fd.Type.Params == nil || len(fd.Type.Params.List) != 1 {
					continue
				}
				pt := info.TypeOf(fd.Type.Params.List[0].Type)
				if pt == nil || !types.IsInterface(pt) {
					continue
				}
				reads := false
				ast.Inspect(fd.Body, func(n ast.Node) bool {
					if ix, ok := n.(*ast.IndexExpr); ok {
						if tv := info.Types[ix.Index]; tv.Value != nil && tv.Value.String() == "1" {
							if c, ok := ast.Unparen(ix.X).(*ast.CallExpr); ok && len(c.Args) == 1 && isUnsafePtr(info.TypeOf(c.Args[0])) {
								reads = true
							}
						}
					}
					return !reads
				})
				if reads {
					if fo, ok := info.Defs[fd.Name].(*types.Func); ok {
						m1Funcs[fo] = true
						add(pp+"|m1 declaration (an interface's data word read)", p.Fset.Position(fd.Pos()).String())
					}
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
				pos := p.Fset.Position(n.Pos()).String()
				parent := func(k int) ast.Node {
					if len(stack) > k {
						return stack[len(stack)-1-k]
					}
					return nil
				}
				pkgLevel := true
				for _, a := range stack {
					if _, ok := a.(*ast.FuncDecl); ok {
						pkgLevel = false
					}
					if _, ok := a.(*ast.FuncLit); ok {
						pkgLevel = false
					}
				}
				// w1: *(*uintptr)(unsafe.Pointer(&f)) = x
				if as, ok := n.(*ast.AssignStmt); ok {
					for _, l := range as.Lhs {
						if star, ok := ast.Unparen(l).(*ast.StarExpr); ok {
							if c, ok := ast.Unparen(star.X).(*ast.CallExpr); ok && len(c.Args) == 1 {
								if tv, ok := info.Types[c.Fun]; ok && tv.IsType() {
									if pt, ok := tv.Type.Underlying().(*types.Pointer); ok && (isUintptr(pt.Elem()) || isUnsafePtr(pt.Elem())) {
										if inner, ok := ast.Unparen(c.Args[0]).(*ast.CallExpr); ok && len(inner.Args) == 1 {
											if u, ok := ast.Unparen(inner.Args[0]).(*ast.UnaryExpr); ok && u.Op == token.AND && isFunc(info.TypeOf(u.X)) {
												add(pp+"|w1 write THROUGH a func variable", pos)
											}
										}
									}
								}
							}
						}
					}
				}
				call, ok := n.(*ast.CallExpr)
				if !ok {
					return true
				}
				// m1 call sites
				if id, ok := ast.Unparen(call.Fun).(*ast.Ident); ok {
					if fo, ok := info.Uses[id].(*types.Func); ok && m1Funcs[fo] {
						add(pp+"|m1 call site", pos)
						if pkgLevel {
							add(pp+"|m1 call site in a package-level initializer", pos)
						}
					}
				} else if sel, ok := ast.Unparen(call.Fun).(*ast.SelectorExpr); ok {
					if fo, ok := info.Uses[sel.Sel].(*types.Func); ok && fo.Name() == "__ccgo_fp" {
						add(pp+"|m1 call site (another package's)", pos)
					}
				}
				if len(call.Args) != 1 {
					return true
				}
				tv, ok := info.Types[call.Fun]
				if !ok || !tv.IsType() {
					return true
				}
				pt, ok := tv.Type.Underlying().(*types.Pointer)
				if !ok || !isUnsafePtr(info.TypeOf(call.Args[0])) {
					return true
				}
				inner, _ := ast.Unparen(call.Args[0]).(*ast.CallExpr)
				var src ast.Expr
				if inner != nil && len(inner.Args) == 1 {
					src = ast.Unparen(inner.Args[0])
				}
				var srcElem types.Type
				if u, ok := src.(*ast.UnaryExpr); ok && u.Op == token.AND {
					srcElem = info.TypeOf(u.X)
				}
				// m2: a func (or a struct holding only a func) read as a word
				if isUintptr(pt.Elem()) || isUnsafePtr(pt.Elem()) {
					if isFunc(srcElem) {
						add(pp+"|m2 a func read as a word (*(*uintptr)(unsafe.Pointer(&f)))", pos)
					} else if fv := oneField(srcElem); fv != nil {
						if isFunc(fv.Type()) {
							add(pp+"|m2 a struct{ f func } read as a word", pos)
							if pkgLevel {
								add(pp+"|m2 ... in a package-level initializer", pos)
							}
						} else if _, isTP := fv.Type().(*types.TypeParam); isTP {
							add(pp+"|m2 a struct{ f T } read as a word, T a type parameter", pos)
						}
					}
					return true
				}
				// recoveries
				tgt := pt.Elem()
				target := ""
				if isFunc(tgt) {
					target = "func"
				} else if fv := oneField(tgt); fv != nil && isFunc(fv.Type()) {
					target = "struct{ f func }"
				}
				if target == "" {
					return true
				}
				srcShape := ""
				if fv := oneField(srcElem); fv != nil && isUintptr(fv.Type()) {
					if fv.Embedded() {
						srcShape = "&struct{ uintptr }{x} (EMBEDDED)"
					} else {
						srcShape = "&struct{ f uintptr }{x}"
					}
				} else if isUintptr(srcElem) {
					srcShape = "&v, v a uintptr variable or field"
				} else if srcElem != nil {
					srcShape = "&<other>"
				} else {
					srcShape = "MEMORY at an address (unsafe.Pointer(p))"
				}
				kind := "r1"
				if target == "struct{ f func }" {
					kind = "r2"
				} else if strings.HasPrefix(srcShape, "&v") {
					kind = "r3"
				} else if strings.HasPrefix(srcShape, "MEMORY") {
					kind = "r4"
				}
				// called at once, or stored
				use := "STORED"
				// isCallee: whether node k levels up is (through any parens) the Fun of the call above it
				isCallee := func(k int) bool {
					for j := k + 1; j < len(stack); j++ {
						switch a := parent(j).(type) {
						case *ast.ParenExpr:
							continue
						case *ast.CallExpr:
							return ast.Unparen(a.Fun) == parent(k)
						}
						return false
					}
					return false
				}
				if target == "func" {
					if _, ok := parent(1).(*ast.StarExpr); ok && isCallee(1) {
						use = "called at once"
					}
				} else {
					// (*struct{f F})(…).f then a call
					if se, ok := parent(1).(*ast.SelectorExpr); ok {
						if pc, ok := parent(2).(*ast.CallExpr); ok && ast.Unparen(pc.Fun) == ast.Expr(se) {
							use = "called at once"
						}
					} else if pe, ok := parent(1).(*ast.ParenExpr); ok {
						if se, ok := parent(2).(*ast.SelectorExpr); ok && se.X == ast.Expr(pe) {
							if pc, ok := parent(3).(*ast.CallExpr); ok && ast.Unparen(pc.Fun) == ast.Expr(se) {
								use = "called at once"
							}
						}
					}
				}
				// v3 (2026-11-13): an assignment TARGET is not a recovery. `*(*F)(unsafe.Pointer(&slot)) = fn` stores a func INTO a
				// word slot, which is a MINT (w2); v2 counted it as a STORED recovery.
				if target == "func" {
					if star, ok := parent(1).(*ast.StarExpr); ok {
						for j := 2; j < len(stack); j++ {
							if _, ok := parent(j).(*ast.ParenExpr); ok {
								continue
							}
							if as, ok := parent(j).(*ast.AssignStmt); ok {
								for _, l := range as.Lhs {
									if ast.Unparen(l) == ast.Expr(star) {
										add(fmt.Sprintf("%s|w2 a func written INTO a word slot (a mint) at %s", pp, srcShape), pos)
										return true
									}
								}
							}
							break
						}
					}
				}
				add(fmt.Sprintf("%s|%s %s from %s, %s", pp, kind, target, srcShape, use), pos)
				return true
			})
		}
	}
	var ks []string
	for k := range counts {
		ks = append(ks, k)
	}
	sort.Strings(ks)
	for _, k := range ks {
		fmt.Printf("%6d  %s   e.g. %s\n", counts[k], k, examples[k])
	}
}
