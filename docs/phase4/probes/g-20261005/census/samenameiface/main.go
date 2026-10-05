// Census for the same-name interface conversion: a concrete NAMED type T (or *T) converted to a NAMED
// interface I where T and I are DIFFERENT types with the SAME simple name (logrus' *lslog.Handler passed
// to slog.New(h slog.Handler)). go2cs' recordable gate compares the two rendered names as strings, so
// the pair is taken for one type: no GoImplement record, no adapter, and a pointer is dereferenced.
// Counts the three positions where an interface target type is known from the AST: call arguments,
// return values, and assignments/var specs with a declared interface type.
// Usage: samenameiface <dir> <tests:0|1> <pattern>...
package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"sort"

	"golang.org/x/tools/go/packages"
)

func namedOf(t types.Type) *types.Named {
	t = types.Unalias(t)
	if p, ok := t.(*types.Pointer); ok {
		t = types.Unalias(p.Elem())
	}
	n, _ := t.(*types.Named)
	return n
}

func check(sites map[string]string, fset func(ast.Node) string, src, dst types.Type, at ast.Node) {
	if src == nil || dst == nil {
		return
	}
	in, ok := types.Unalias(dst).(*types.Named)
	if !ok {
		return
	}
	if _, isIface := in.Underlying().(*types.Interface); !isIface {
		return
	}
	sn := namedOf(src)
	if sn == nil {
		return
	}
	if _, srcIface := sn.Underlying().(*types.Interface); srcIface {
		return
	}
	if sn.Obj().Name() != in.Obj().Name() || types.Identical(sn, in) {
		return
	}
	// The trigger is the QUALIFIED name by package NAME: `slog.Handler` (logrus' hooks/slog is itself
	// `package slog`) equals log/slog's `slog.Handler`. A same simple name across differently NAMED
	// packages (`tabwriter.Writer` -> `io.Writer`) converts correctly and is not counted.
	if sn.Obj().Pkg() == nil || in.Obj().Pkg() == nil || sn.Obj().Pkg().Name() != in.Obj().Pkg().Name() || sn.Obj().Pkg().Path() == in.Obj().Pkg().Path() {
		return
	}
	sites[fset(at)] = fmt.Sprintf("%s -> %s", types.TypeString(src, nil), types.TypeString(dst, nil))
}

func main() {
	cfg := &packages.Config{Mode: packages.LoadAllSyntax, Dir: os.Args[1], Tests: os.Args[2] == "1"}
	pkgs, err := packages.Load(cfg, os.Args[3:]...)
	if err != nil {
		fmt.Println("LOAD ERROR", err)
		os.Exit(2)
	}
	sites := map[string]string{}
	for _, p := range pkgs {
		if p.TypesInfo == nil {
			continue
		}
		info := p.TypesInfo
		pos := func(n ast.Node) string { return p.Fset.Position(n.Pos()).String() }
		for _, f := range p.Syntax {
			var sigStack []*types.Signature
			ast.Inspect(f, func(n ast.Node) bool {
				switch x := n.(type) {
				case *ast.FuncDecl:
					if obj, ok := info.Defs[x.Name].(*types.Func); ok {
						sigStack = append(sigStack[:0], obj.Type().(*types.Signature))
					}
				case *ast.FuncLit:
					if sig, ok := info.TypeOf(x).(*types.Signature); ok {
						sigStack = append(sigStack, sig)
					}
				case *ast.CallExpr:
					sig, ok := types.Unalias(info.TypeOf(x.Fun)).(*types.Signature)
					if !ok {
						return true
					}
					params := sig.Params()
					for i, a := range x.Args {
						var pt types.Type
						switch {
						case sig.Variadic() && i >= params.Len()-1:
							if s, isSlice := params.At(params.Len() - 1).Type().(*types.Slice); isSlice && !x.Ellipsis.IsValid() {
								pt = s.Elem()
							}
						case i < params.Len():
							pt = params.At(i).Type()
						}
						check(sites, pos, info.TypeOf(a), pt, a)
					}
				case *ast.ReturnStmt:
					if len(sigStack) > 0 {
						res := sigStack[len(sigStack)-1].Results()
						if res.Len() == len(x.Results) {
							for i, r := range x.Results {
								check(sites, pos, info.TypeOf(r), res.At(i).Type(), r)
							}
						}
					}
				case *ast.AssignStmt:
					if len(x.Lhs) == len(x.Rhs) {
						for i := range x.Lhs {
							check(sites, pos, info.TypeOf(x.Rhs[i]), info.TypeOf(x.Lhs[i]), x.Rhs[i])
						}
					}
				case *ast.ValueSpec:
					if x.Type != nil && len(x.Values) == len(x.Names) {
						for _, v := range x.Values {
							check(sites, pos, info.TypeOf(v), info.TypeOf(x.Type), v)
						}
					}
				}
				return true
			})
		}
	}
	keys := []string{}
	for k := range sites {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	for _, k := range keys {
		fmt.Println("SITE", k, sites[k])
	}
	fmt.Printf("TOTAL %d\n", len(keys))
}
