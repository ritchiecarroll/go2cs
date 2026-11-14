// fpaggr: which func-word signatures pass a STRUCT or ARRAY by value (sqlite-cs S1 revision, Fable review item 4).
// A C# delegate rebind (Delegate.CreateDelegate) binds only when every parameter type is the same C# type, so a by-value
// aggregate that two packages lift separately (lib's TLARGE_INTEGER, libc's largeInteger: aliases of one anonymous
// struct) cannot rebind. Counted:
//   R  every recovery-target func type: (*F)(unsafe.Pointer(…)) with F's underlying a func, or a struct whose only field is a func
//   M  every function minted through a call to __ccgo_fp (sub-count keyed on the name; the structural m1 census is fpcensus2)
// For each, whether a parameter or result is (after Unalias) a struct or array by value, and whether that struct is UNNAMED
// (an anonymous struct or an alias of one: the case two packages lift separately).
package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"sort"

	"golang.org/x/tools/go/packages"
)

func aggr(sig *types.Signature) (has bool, anon bool, what string) {
	check := func(t types.Type) {
		u := types.Unalias(t)
		switch x := u.Underlying().(type) {
		case *types.Struct, *types.Array:
			has = true
			if _, named := u.(*types.Named); !named {
				anon = true
			}
			_ = x
			if what == "" {
				what = types.TypeString(t, func(p *types.Package) string { return p.Name() })
			}
		}
	}
	for i := 0; i < sig.Params().Len(); i++ {
		check(sig.Params().At(i).Type())
	}
	for i := 0; i < sig.Results().Len(); i++ {
		check(sig.Results().At(i).Type())
	}
	return
}

func funcOf(t types.Type) *types.Signature {
	switch u := t.Underlying().(type) {
	case *types.Signature:
		return u
	case *types.Struct:
		if u.NumFields() == 1 {
			if s, ok := u.Field(0).Type().Underlying().(*types.Signature); ok {
				return s
			}
		}
	}
	return nil
}

func main() {
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo}
	pkgs, err := packages.Load(cfg, os.Args[1:]...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}
	type row struct{ sites, anonSites int; sigs, anonSigs map[string]bool; ex string }
	rows := map[string]*row{}
	get := func(k string) *row {
		if rows[k] == nil {
			rows[k] = &row{sigs: map[string]bool{}, anonSigs: map[string]bool{}}
		}
		return rows[k]
	}
	minted := map[string]map[*types.Func]bool{}
	for _, p := range pkgs {
		for _, f := range p.Syntax {
			ast.Inspect(f, func(n ast.Node) bool {
				call, ok := n.(*ast.CallExpr)
				if !ok || len(call.Args) != 1 {
					return true
				}
				// R: a conversion (*F)(unsafe.Pointer-typed arg)
				if tv, ok := p.TypesInfo.Types[call.Fun]; ok && tv.IsType() {
					if ptr, ok := tv.Type.Underlying().(*types.Pointer); ok {
						if b, ok := p.TypesInfo.TypeOf(call.Args[0]).(*types.Basic); ok && b.Kind() == types.UnsafePointer {
							if sig := funcOf(ptr.Elem()); sig != nil {
								has, anon, what := aggr(sig)
								r := get(p.PkgPath + "|R recovery target")
								r.sites++
								key := types.TypeString(sig, nil)
								r.sigs[key] = true
								if has {
									r2 := get(p.PkgPath + "|R ... passing a struct or array by value")
									r2.sites++
									r2.sigs[key] = true
									if r2.ex == "" {
										r2.ex = p.Fset.Position(call.Pos()).String() + "  " + what
									}
									if anon {
										r3 := get(p.PkgPath + "|R ... an UNNAMED one (anonymous, or an alias of one)")
										r3.sites++
										r3.sigs[key] = true
										if r3.ex == "" {
											r3.ex = p.Fset.Position(call.Pos()).String() + "  " + what
										}
									}
								}
							}
						}
					}
					return true
				}
				// M: __ccgo_fp(F)
				if id, ok := call.Fun.(*ast.Ident); ok && id.Name == "__ccgo_fp" {
					var obj types.Object
					switch a := ast.Unparen(call.Args[0]).(type) {
					case *ast.Ident:
						obj = p.TypesInfo.Uses[a]
					case *ast.SelectorExpr:
						obj = p.TypesInfo.Uses[a.Sel]
					}
					if fn, ok := obj.(*types.Func); ok {
						if minted[p.PkgPath] == nil {
							minted[p.PkgPath] = map[*types.Func]bool{}
						}
						if !minted[p.PkgPath][fn] {
							minted[p.PkgPath][fn] = true
							has, anon, what := aggr(fn.Type().(*types.Signature))
							if has {
								r := get(p.PkgPath + "|M minted function passing a struct or array by value")
								r.sites++
								if r.ex == "" {
									r.ex = fn.FullName() + "  " + what
								}
								if anon {
									r3 := get(p.PkgPath + "|M ... an UNNAMED one")
									r3.sites++
									if r3.ex == "" {
										r3.ex = fn.FullName() + "  " + what
									}
								}
							}
						}
					}
				}
				return true
			})
		}
	}
	keys := []string{}
	for k := range rows {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	for _, k := range keys {
		r := rows[k]
		fmt.Printf("%6d sites  %4d signatures  %s   e.g. %s\n", r.sites, len(r.sigs), k, r.ex)
	}
}
