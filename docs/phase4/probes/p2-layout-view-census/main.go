// vbcensus2: every (*T)(unsafe.Pointer(X)) and every uintptr(unsafe.Pointer(P)) whose pointee T is a struct
// whose go2cs C# layout can differ from Go's: a reference field (array, pointer, slice, string, map, chan,
// func, interface) or an embedded field (go2cs-gen declares embed storage after the converter's fields).
// Kinds: BYTES (X addresses bytes), UINTPTR (X is a uintptr: a native address), UPTR (X is an
// unsafe.Pointer-typed expression), RETYPE (X is a *U of another type), CALLEE (uintptr(unsafe.Pointer(&v))
// handed to a call: a native callee argument).
package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

func reasons(t types.Type, seen map[types.Type]bool, prefix string, out map[string]bool, fields *[]string) {
	st, ok := t.Underlying().(*types.Struct)
	if !ok || seen[t] {
		return
	}
	seen[t] = true
	for i := 0; i < st.NumFields(); i++ {
		f := st.Field(i)
		name := prefix + f.Name()
		if f.Embedded() {
			out["embed"] = true
			*fields = append(*fields, name+"(embed)")
		}
		switch u := f.Type().Underlying().(type) {
		case *types.Array:
			out["array"] = true
			*fields = append(*fields, name+"(array)")
		case *types.Pointer:
			out["pointer"] = true
			*fields = append(*fields, name+"(pointer)")
		case *types.Slice, *types.Map, *types.Chan, *types.Signature, *types.Interface:
			out["otherref"] = true
			*fields = append(*fields, name+"("+strings.ToLower(fmt.Sprintf("%T", u)[7:])+")")
		case *types.Basic:
			if u.Kind() == types.String {
				out["otherref"] = true
				*fields = append(*fields, name+"(string)")
			} else if u.Kind() == types.UnsafePointer {
				out["pointer"] = true
				*fields = append(*fields, name+"(unsafe.Pointer)")
			}
		case *types.Struct:
			reasons(f.Type(), seen, name+".", out, fields)
		}
	}
}

func isByteish(t types.Type) bool {
	p, ok := t.Underlying().(*types.Pointer)
	if !ok {
		return false
	}
	switch e := p.Elem().Underlying().(type) {
	case *types.Basic:
		return e.Kind() == types.Byte || e.Kind() == types.Uint8 || e.Kind() == types.Int8
	case *types.Array:
		b, ok := e.Elem().Underlying().(*types.Basic)
		return ok && (b.Kind() == types.Byte || b.Kind() == types.Uint8 || b.Kind() == types.Int8)
	}
	return false
}

func isUnsafePointerConv(info *types.Info, e ast.Expr) (ast.Expr, bool) {
	c, ok := ast.Unparen(e).(*ast.CallExpr)
	if !ok || len(c.Args) != 1 {
		return nil, false
	}
	tv, ok := info.Types[c.Fun]
	if !ok || !tv.IsType() {
		return nil, false
	}
	if b, ok := tv.Type.(*types.Basic); !ok || b.Kind() != types.UnsafePointer {
		return nil, false
	}
	return c.Args[0], true
}

func main() {
	goos := os.Args[1]
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedTypes | packages.NeedSyntax | packages.NeedTypesInfo | packages.NeedFiles,
		Tests: true, Env: append(os.Environ(), "GOOS="+goos, "CGO_ENABLED=0")}
	pkgs, err := packages.Load(cfg, "std")
	if err != nil {
		panic(err)
	}
	seen := map[string]bool{}
	var rows []string
	for _, p := range pkgs {
		for _, f := range p.Syntax {
			var encl string
			ast.Inspect(f, func(n ast.Node) bool {
				if fd, ok := n.(*ast.FuncDecl); ok {
					encl = fd.Name.Name
				}
				c, ok := n.(*ast.CallExpr)
				if !ok || len(c.Args) != 1 {
					return true
				}
				tv, ok := p.TypesInfo.Types[c.Fun]
				if !ok || !tv.IsType() {
					return true
				}
				x, ok := isUnsafePointerConv(p.TypesInfo, c.Args[0])
				if !ok {
					// (*T)(p) with p already an unsafe.Pointer value (unsafe.Add, sysAlloc, a field).
					at := p.TypesInfo.TypeOf(c.Args[0])
					if at == nil || !types.Identical(at, types.Typ[types.UnsafePointer]) {
						return true
					}
					x = c.Args[0]
				}
				xt := p.TypesInfo.TypeOf(x)
				if xt == nil {
					return true
				}
				var kind string
				var dst types.Type
				switch d := tv.Type.Underlying().(type) {
				case *types.Pointer:
					dst = d.Elem()
					switch {
					case isByteish(xt):
						kind = "BYTES"
					case types.Identical(xt.Underlying(), types.Typ[types.Uintptr]):
						kind = "UINTPTR"
					case types.Identical(xt.Underlying(), types.Typ[types.UnsafePointer]):
						kind = "UPTR"
					default:
						xp, ok := xt.Underlying().(*types.Pointer)
						if !ok || types.Identical(xp.Elem(), dst) {
							return true
						}
						kind = "RETYPE"
					}
				case *types.Basic:
					if d.Kind() != types.Uintptr {
						return true
					}
					xp, ok := xt.Underlying().(*types.Pointer)
					if !ok {
						return true
					}
					dst = xp.Elem()
					kind = "CALLEE"
				default:
					return true
				}
				why := map[string]bool{}
				var fields []string
				reasons(dst, map[types.Type]bool{}, "", why, &fields)
				if len(why) == 0 {
					return true
				}
				pos := p.Fset.Position(c.Pos())
				rel := pos.Filename
				if k := strings.Index(rel, "/src/"); k >= 0 {
					rel = rel[k+5:]
				}
				key := fmt.Sprintf("%s:%d:%d", rel, pos.Line, pos.Column)
				if seen[key] {
					return true
				}
				seen[key] = true
				var ws []string
				for w := range why {
					ws = append(ws, w)
				}
				sort.Strings(ws)
				tag := "std"
				if strings.HasSuffix(rel, "_test.go") {
					tag = "test"
				}
				if len(fields) > 6 {
					fields = append(fields[:6], "...")
				}
				rows = append(rows, fmt.Sprintf("%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s", kind, tag, strings.Join(ws, "+"), key, p.Types.Path(), encl,
					types.TypeString(dst, types.RelativeTo(p.Types)), strings.Join(fields, ",")))
				return true
			})
		}
	}
	sort.Strings(rows)
	for _, r := range rows {
		fmt.Println(r)
	}
}
