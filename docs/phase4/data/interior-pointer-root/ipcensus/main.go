// ipcensus: every conversion (*T)(e) whose operand e is an unsafe.Pointer derived by ARITHMETIC (an interior
// address), classified by where the base came from, how the result is used, and whether T holds pointers.
//
//   ipcensus <tag> <pattern>...       (run in a module that resolves the patterns; GOOS/GOARCH from the environment)
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

type site struct {
	pkg, pos, target, origin, use, via string
	refBearing                         bool
	test                               bool
}

var (
	adders = map[*types.Func]string{} // func -> origin of its base ("param", ...)
	fset   *token.FileSet
)

func isUnsafePointer(t types.Type) bool {
	b, ok := types.Unalias(t).Underlying().(*types.Basic)
	return ok && b.Kind() == types.UnsafePointer
}

func isUintptr(t types.Type) bool {
	b, ok := types.Unalias(t).Underlying().(*types.Basic)
	return ok && b.Kind() == types.Uintptr
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
	case *types.Pointer, *types.Slice, *types.Map, *types.Chan, *types.Signature, *types.Interface:
		return true
	case *types.Array:
		return hasPointers(u.Elem(), seen)
	case *types.Struct:
		for i := 0; i < u.NumFields(); i++ {
			if hasPointers(u.Field(i).Type(), seen) {
				return true
			}
		}
		return false
	case *types.TypeParam:
		return true
	}
	return true
}

type ctx struct {
	info  *types.Info
	fn    ast.Node              // enclosing func body owner
	local map[types.Object]string // memo for local vars: "" not interior, else via
}

func calleeFunc(info *types.Info, call *ast.CallExpr) *types.Func {
	var id *ast.Ident
	switch f := ast.Unparen(call.Fun).(type) {
	case *ast.Ident:
		id = f
	case *ast.SelectorExpr:
		id = f.Sel
	case *ast.IndexExpr:
		switch g := ast.Unparen(f.X).(type) {
		case *ast.Ident:
			id = g
		case *ast.SelectorExpr:
			id = g.Sel
		}
	}
	if id == nil {
		return nil
	}
	fn, _ := info.Uses[id].(*types.Func)
	if fn != nil {
		fn = fn.Origin()
	}
	return fn
}

func isUnsafeBuiltin(info *types.Info, call *ast.CallExpr, name string) bool {
	sel, ok := ast.Unparen(call.Fun).(*ast.SelectorExpr)
	if !ok || sel.Sel.Name != name {
		return false
	}
	b, ok := info.Uses[sel.Sel].(*types.Builtin)
	return ok && b.Name() == name
}

func isConversion(info *types.Info, call *ast.CallExpr) (types.Type, bool) {
	if len(call.Args) != 1 {
		return nil, false
	}
	tv, ok := info.Types[call.Fun]
	if !ok || !tv.IsType() {
		return nil, false
	}
	return tv.Type, true
}

func hasArith(e ast.Expr) bool {
	found := false
	ast.Inspect(e, func(n ast.Node) bool {
		if b, ok := n.(*ast.BinaryExpr); ok && (b.Op == token.ADD || b.Op == token.SUB) {
			found = true
		}
		return !found
	})
	return found
}

// originOfNumber classifies where a uintptr expression's address came from.
func (c *ctx) originOfNumber(e ast.Expr) string {
	origin := ""
	ast.Inspect(e, func(n ast.Node) bool {
		call, ok := n.(*ast.CallExpr)
		if !ok {
			return true
		}
		if t, ok := isConversion(c.info, call); ok && isUintptr(t) {
			at := c.info.TypeOf(call.Args[0])
			if at != nil && isUnsafePointer(at) {
				o := c.originOfPointer(call.Args[0])
				if origin == "" || o == "managed" {
					origin = o
				}
				return false
			}
		}
		return true
	})
	if origin == "" {
		return "number"
	}
	return origin
}

// originOfPointer classifies an unsafe.Pointer-typed base expression.
func (c *ctx) originOfPointer(e ast.Expr) string {
	e = ast.Unparen(e)
	switch x := e.(type) {
	case *ast.CallExpr:
		if isUnsafeBuiltin(c.info, x, "Add") {
			return c.originOfPointer(x.Args[0])
		}
		if isUnsafeBuiltin(c.info, x, "SliceData") || isUnsafeBuiltin(c.info, x, "StringData") {
			return "managed"
		}
		if t, ok := isConversion(c.info, x); ok && isUnsafePointer(t) {
			at := c.info.TypeOf(x.Args[0])
			if at == nil {
				return "unknown"
			}
			if isUintptr(at) {
				return c.originOfNumber(x.Args[0])
			}
			if isUnsafePointer(at) {
				return c.originOfPointer(x.Args[0])
			}
			return "managed" // a typed Go pointer, &x, &s[i]
		}
		if fn := calleeFunc(c.info, x); fn != nil {
			if _, ok := adders[fn]; ok && len(x.Args) > 0 {
				if at := c.info.TypeOf(x.Args[0]); at != nil && isUnsafePointer(at) {
					return c.originOfPointer(x.Args[0])
				}
			}
		}
		return "pointer-call"
	case *ast.Ident, *ast.SelectorExpr, *ast.IndexExpr, *ast.StarExpr:
		return "pointer-var"
	}
	return "unknown"
}

// interior reports whether e (unsafe.Pointer-typed) is derived by arithmetic; via names how.
func (c *ctx) interior(e ast.Expr, depth int) (via string, origin string, ok bool) {
	e = ast.Unparen(e)
	switch x := e.(type) {
	case *ast.CallExpr:
		if isUnsafeBuiltin(c.info, x, "Add") {
			return "unsafe.Add", c.originOfPointer(x.Args[0]), true
		}
		if t, isConv := isConversion(c.info, x); isConv && isUnsafePointer(t) {
			at := c.info.TypeOf(x.Args[0])
			if at != nil && isUintptr(at) && hasArith(x.Args[0]) {
				return "uintptr+", c.originOfNumber(x.Args[0]), true
			}
			if at != nil && isUnsafePointer(at) {
				return c.interior(x.Args[0], depth)
			}
			if at != nil && isUintptr(at) {
				// unsafe.Pointer(u) where u is a local uintptr computed by arithmetic
				if id, isId := ast.Unparen(x.Args[0]).(*ast.Ident); isId && depth < 3 {
					if v, o, ok2 := c.localNumber(id, depth+1); ok2 {
						return v, o, true
					}
				}
			}
			return "", "", false
		}
		if fn := calleeFunc(c.info, x); fn != nil {
			if _, isAdder := adders[fn]; isAdder {
				origin := "pointer-var"
				if len(x.Args) > 0 {
					if at := c.info.TypeOf(x.Args[0]); at != nil && isUnsafePointer(at) {
						origin = c.originOfPointer(x.Args[0])
					} else if at != nil && isUintptr(at) {
						origin = c.originOfNumber(x.Args[0])
					} else if sel, isSel := ast.Unparen(x.Fun).(*ast.SelectorExpr); isSel {
						_ = sel
						origin = "managed" // a method on a typed pointer or value
					}
				} else {
					origin = "managed"
				}
				return "helper:" + fn.Name(), origin, true
			}
		}
	case *ast.Ident:
		if depth < 3 {
			return c.localPointer(x, depth+1)
		}
	}
	return "", "", false
}

func (c *ctx) assignmentsOf(obj types.Object) []ast.Expr {
	var rhs []ast.Expr
	if c.fn == nil {
		return nil
	}
	ast.Inspect(c.fn, func(n ast.Node) bool {
		switch s := n.(type) {
		case *ast.AssignStmt:
			if len(s.Lhs) == len(s.Rhs) {
				for i, l := range s.Lhs {
					if id, ok := l.(*ast.Ident); ok && (c.info.Defs[id] == obj || c.info.Uses[id] == obj) {
						rhs = append(rhs, s.Rhs[i])
					}
				}
			}
		case *ast.ValueSpec:
			if len(s.Names) == len(s.Values) {
				for i, id := range s.Names {
					if c.info.Defs[id] == obj {
						rhs = append(rhs, s.Values[i])
					}
				}
			}
		}
		return true
	})
	return rhs
}

func (c *ctx) localPointer(id *ast.Ident, depth int) (string, string, bool) {
	obj := c.info.Uses[id]
	v, isVar := obj.(*types.Var)
	if !isVar || v.IsField() || v.Parent() == nil || v.Parent() == v.Pkg().Scope() {
		return "", "", false
	}
	for _, r := range c.assignmentsOf(obj) {
		if via, origin, ok := c.interior(r, depth); ok {
			return "local<-" + via, origin, true
		}
	}
	return "", "", false
}

func (c *ctx) localNumber(id *ast.Ident, depth int) (string, string, bool) {
	obj := c.info.Uses[id]
	v, isVar := obj.(*types.Var)
	if !isVar || v.IsField() || v.Parent() == nil || v.Parent() == v.Pkg().Scope() {
		return "", "", false
	}
	for _, r := range c.assignmentsOf(obj) {
		if hasArith(r) {
			o := c.originOfNumber(r)
			if o != "number" {
				return "local-uintptr+", o, true
			}
		}
	}
	return "", "", false
}

// useOf classifies how the conversion's result is used, from its ancestors (innermost last).
func useOf(stack []ast.Node, conv ast.Expr) string {
	cur := ast.Node(conv)
	deref := false
	for i := len(stack) - 1; i >= 0; i-- {
		p := stack[i]
		switch x := p.(type) {
		case *ast.ParenExpr:
			cur = p
			continue
		case *ast.StarExpr:
			deref = true
			cur = p
			continue
		case *ast.SelectorExpr:
			if x.X == cur {
				deref = true
				cur = p
				continue
			}
		case *ast.IndexExpr:
			if x.X == cur {
				deref = true
				cur = p
				continue
			}
		case *ast.SliceExpr:
			if x.X == cur {
				return "held:slice-of"
			}
		case *ast.UnaryExpr:
			if x.Op == token.AND && deref {
				return "held:address-of"
			}
		case *ast.AssignStmt:
			for _, l := range x.Lhs {
				if l == cur {
					if deref {
						return "write"
					}
					return "held:assign-target"
				}
			}
			if deref {
				return "read"
			}
			return "held:stored"
		case *ast.IncDecStmt:
			if deref {
				return "write"
			}
		case *ast.CallExpr:
			if !deref {
				for _, a := range x.Args {
					if a == cur {
						return "held:argument"
					}
				}
			} else if x.Fun == cur {
				return "read:call"
			} else {
				// a method call whose receiver is the pointer or its field
				if sel, ok := ast.Unparen(x.Fun).(*ast.SelectorExpr); ok && sel == cur {
					return "held:receiver"
				}
			}
		case *ast.ReturnStmt:
			if !deref {
				return "held:returned"
			}
		case *ast.BinaryExpr:
			if !deref && (x.Op == token.EQL || x.Op == token.NEQ) {
				return "compare"
			}
		case *ast.CompositeLit, *ast.KeyValueExpr:
			if !deref {
				return "held:stored"
			}
		case *ast.ValueSpec:
			if !deref {
				return "held:stored"
			}
		}
		if deref {
			return "read"
		}
		return "held:other"
	}
	if deref {
		return "read"
	}
	return "held:other"
}

func main() {
	tag := os.Args[1]
	cfg := &packages.Config{
		Mode:  packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo | packages.NeedCompiledGoFiles,
		Tests: os.Getenv("IP_TESTS") == "1",
	}
	pkgs, err := packages.Load(cfg, os.Args[2:]...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
	loadErrs := 0
	for _, p := range pkgs {
		loadErrs += len(p.Errors)
	}
	if len(pkgs) > 0 {
		fset = pkgs[0].Fset
	}

	// Pass 1: helper functions that return a parameter (or receiver) moved by arithmetic, to a fixpoint.
	for changed, round := true, 0; changed && round < 6; round++ {
		changed = false
		for _, p := range pkgs {
			for _, f := range p.Syntax {
				for _, d := range f.Decls {
					fd, ok := d.(*ast.FuncDecl)
					if !ok || fd.Body == nil || fd.Type.Results == nil || len(fd.Type.Results.List) != 1 {
						continue
					}
					obj, _ := p.TypesInfo.Defs[fd.Name].(*types.Func)
					if obj == nil {
						continue
					}
					if _, done := adders[obj]; done {
						continue
					}
					rt := p.TypesInfo.TypeOf(fd.Type.Results.List[0].Type)
					if rt == nil || !isUnsafePointer(rt) {
						continue
					}
					c := &ctx{info: p.TypesInfo, fn: fd}
					found := false
					ast.Inspect(fd.Body, func(n ast.Node) bool {
						if _, isLit := n.(*ast.FuncLit); isLit {
							return false
						}
						if r, ok := n.(*ast.ReturnStmt); ok && len(r.Results) == 1 {
							if _, _, ok := c.interior(r.Results[0], 0); ok {
								found = true
							}
						}
						return !found
					})
					if found {
						adders[obj] = p.PkgPath
						changed = true
					}
				}
			}
		}
	}

	// Pass 2: the sites.
	seen := map[string]bool{}
	var sites []site
	for _, p := range pkgs {
		for _, f := range p.Syntax {
			fname := fset.Position(f.Pos()).Filename
			isTest := strings.HasSuffix(fname, "_test.go")
			var stack []ast.Node
			var fnStack []ast.Node
			ast.Inspect(f, func(n ast.Node) bool {
				if n == nil {
					top := stack[len(stack)-1]
					stack = stack[:len(stack)-1]
					if len(fnStack) > 0 && fnStack[len(fnStack)-1] == top {
						fnStack = fnStack[:len(fnStack)-1]
					}
					return true
				}
				if fd, ok := n.(*ast.FuncDecl); ok {
					fnStack = append(fnStack, fd)
				}
				if call, ok := n.(*ast.CallExpr); ok {
					if t, isConv := isConversion(p.TypesInfo, call); isConv {
						if pt, isPtr := types.Unalias(t).Underlying().(*types.Pointer); isPtr {
							if at := p.TypesInfo.TypeOf(call.Args[0]); at != nil && isUnsafePointer(at) {
								var fn ast.Node
								if len(fnStack) > 0 {
									fn = fnStack[len(fnStack)-1]
								}
								c := &ctx{info: p.TypesInfo, fn: fn}
								if via, origin, ok := c.interior(call.Args[0], 0); ok {
									pos := fset.Position(call.Pos())
									key := fmt.Sprintf("%s:%d:%d", pos.Filename, pos.Line, pos.Column)
									if !seen[key] {
										seen[key] = true
										short := pos.Filename
										if i := strings.LastIndex(short, "/"); i >= 0 {
											short = short[i+1:]
										}
										pkgPath := strings.TrimSuffix(strings.TrimSuffix(p.PkgPath, "_test"), ".test")
										sites = append(sites, site{
											pkg: pkgPath, pos: fmt.Sprintf("%s:%d", short, pos.Line),
											target:     types.TypeString(pt.Elem(), func(q *types.Package) string { return q.Name() }),
											origin:     origin, use: useOf(stack, call), via: via,
											refBearing: hasPointers(pt.Elem(), map[types.Type]bool{}), test: isTest,
										})
									}
								}
							}
						}
					}
				}
				stack = append(stack, n)
				return true
			})
		}
	}

	sort.Slice(sites, func(i, j int) bool {
		if sites[i].pkg != sites[j].pkg {
			return sites[i].pkg < sites[j].pkg
		}
		return sites[i].pos < sites[j].pos
	})
	for _, s := range sites {
		fmt.Printf("%s\t%s\t%s\t%s\t%s\t%s\t%v\t%v\t%s\n", tag, s.pkg, s.pos, s.via, s.origin, s.use, s.refBearing, s.test, s.target)
	}
	var names []string
	for fn, pkg := range adders {
		names = append(names, pkg+"."+fn.FullName())
	}
	sort.Strings(names)
	fmt.Fprintf(os.Stderr, "%s: packages %d, load errors %d, helpers %d, sites %d\n", tag, len(pkgs), loadErrs, len(adders), len(sites))
	for _, n := range names {
		fmt.Fprintln(os.Stderr, "  helper", n)
	}
}
