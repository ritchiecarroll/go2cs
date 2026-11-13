// fpcensus: how ccgo code carries C function pointers as uintptr (sqlite-cs S1). Read-only.
//   MINT      a call of a package's __ccgo_fp(f interface{}) uintptr, by argument kind
//   RECOVER   *(*F)(unsafe.Pointer(&struct{ f uintptr }{x})) and kin: a struct holding one uintptr, reinterpreted as a
//             func type F; by shape and by F
//   GO-SIDE   the reverse, a struct holding one func value read as uintptr (sqlite.go's cFuncPointer)
//   TABLES    mint calls inside composite literals (static callback tables), and package-level ones
//   CASTS     recovery signatures that no mint in the corpus has (a C cast through a different fp type)
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

func add(k string, n int) { counts[k] += n }

// sigKey: a signature's parameter and result TYPES, without names, so a declared func and an unnamed func type compare.
func sigKey(sig *types.Signature) string {
	var b strings.Builder
	b.WriteString("func(")
	for i := 0; i < sig.Params().Len(); i++ {
		if i > 0 {
			b.WriteString(", ")
		}
		b.WriteString(types.TypeString(types.Unalias(sig.Params().At(i).Type()), nil))
	}
	b.WriteString(")")
	if sig.Results().Len() > 0 {
		b.WriteString(" (")
		for i := 0; i < sig.Results().Len(); i++ {
			if i > 0 {
				b.WriteString(", ")
			}
			b.WriteString(types.TypeString(types.Unalias(sig.Results().At(i).Type()), nil))
		}
		b.WriteString(")")
	}
	if sig.Variadic() {
		b.WriteString(" variadic")
	}
	return b.String()
}

func isUnsafePtr(t types.Type) bool {
	b, ok := t.(*types.Basic)
	return ok && b.Kind() == types.UnsafePointer
}

func isUintptr(t types.Type) bool {
	b, ok := t.Underlying().(*types.Basic)
	return ok && b.Kind() == types.Uintptr
}

func main() {
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo}
	pkgs, err := packages.Load(cfg, os.Args[1:]...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
	mintSigs := map[string]int{}
	recSigs := map[string]int{}
	var mintTypes, recTypes []types.Type
	funcsMinted := map[types.Object]bool{}
	for _, p := range pkgs {
		info := p.TypesInfo
		pp := p.PkgPath
		for _, f := range p.Syntax {
			// stack for "inside a composite literal" and "package level"
			var stack []ast.Node
			ast.Inspect(f, func(n ast.Node) bool {
				if n == nil {
					stack = stack[:len(stack)-1]
					return true
				}
				stack = append(stack, n)
				if be, ok := n.(*ast.BinaryExpr); ok && (be.Op == token.EQL || be.Op == token.NEQ) {
					isMint := func(e ast.Expr) bool {
						c, ok := ast.Unparen(e).(*ast.CallExpr)
						if !ok {
							return false
						}
						id, ok := ast.Unparen(c.Fun).(*ast.Ident)
						return ok && (id.Name == "__ccgo_fp" || id.Name == "cFuncPointer")
					}
					if isMint(be.X) || isMint(be.Y) {
						add(pp+"|COMPARE a number against a mint (== or !=)", 1)
					}
				}
				call, ok := n.(*ast.CallExpr)
				if !ok {
					return true
				}
				if id, ok := ast.Unparen(call.Fun).(*ast.Ident); ok && id.Name == "cFuncPointer" {
					add(pp+"|MINT (Go side, cFuncPointer) sites", 1)
				}
				// MINT
				if id, ok := ast.Unparen(call.Fun).(*ast.Ident); ok && id.Name == "__ccgo_fp" && len(call.Args) == 1 {
					add(pp+"|MINT sites", 1)
					arg := ast.Unparen(call.Args[0])
					kind := "other"
					if aid, ok := arg.(*ast.Ident); ok {
						if fo, ok := info.Uses[aid].(*types.Func); ok {
							kind = "a package-level func"
							funcsMinted[fo] = true
						} else if _, ok := info.Uses[aid].(*types.Var); ok {
							kind = "a func VARIABLE"
						}
					} else if sel, ok := arg.(*ast.SelectorExpr); ok {
						if fo, ok := info.Uses[sel.Sel].(*types.Func); ok {
							if s := info.Selections[sel]; s != nil {
								kind = "a method value"
							} else {
								kind = "another package's func"
								funcsMinted[fo] = true
							}
						}
					} else if _, ok := arg.(*ast.FuncLit); ok {
						kind = "a func literal"
					}
					add(pp+"|MINT arg: "+kind, 1)
					if t := info.TypeOf(arg); t != nil {
						if sig, ok := t.Underlying().(*types.Signature); ok {
							mintSigs[sigKey(sig)]++
							mintTypes = append(mintTypes, sig)
						}
					}
					inLit, pkgLevel := false, true
					for _, a := range stack[:len(stack)-1] {
						switch a.(type) {
						case *ast.CompositeLit:
							inLit = true
						case *ast.FuncDecl, *ast.FuncLit:
							pkgLevel = false
						}
					}
					if inLit {
						add(pp+"|MINT inside a composite literal (a callback table or struct)", 1)
					}
					if pkgLevel {
						add(pp+"|MINT in a package-level initializer", 1)
					}
					return true
				}
				// RECOVER: (*F)(unsafe.Pointer(X)) where F is a func type and X is &struct{one uintptr}{..} or similar
				if len(call.Args) != 1 {
					return true
				}
				tv, ok := info.Types[call.Fun]
				if !ok || !tv.IsType() {
					return true
				}
				arg := ast.Unparen(call.Args[0])
				if pt, ok := tv.Type.Underlying().(*types.Pointer); ok && isUnsafePtr(info.TypeOf(arg)) {
					if sig, ok := pt.Elem().Underlying().(*types.Signature); ok {
						shape := "from another unsafe.Pointer"
						if inner, ok := arg.(*ast.CallExpr); ok && len(inner.Args) == 1 {
							src := ast.Unparen(inner.Args[0])
							if u, ok := src.(*ast.UnaryExpr); ok && u.Op == token.AND {
								if cl, ok := ast.Unparen(u.X).(*ast.CompositeLit); ok {
									if st, ok := info.TypeOf(cl).Underlying().(*types.Struct); ok && st.NumFields() == 1 && isUintptr(st.Field(0).Type()) {
										shape = "&struct{ f uintptr }{x} (ccgo's form)"
									} else {
										shape = "&<other composite>"
									}
								} else {
									shape = "&<variable or field>"
								}
							}
						}
						add(pp+"|RECOVER sites: "+shape, 1)
						named := "unnamed func type"
						if _, ok := types.Unalias(pt.Elem()).(*types.Named); ok {
							named = "NAMED func type"
						}
						add(pp+"|RECOVER target: "+named, 1)
						if sig.Variadic() {
							add(pp+"|RECOVER target: Go-variadic", 1)
						}
						recSigs[sigKey(sig)]++
						recTypes = append(recTypes, sig)
						// the call through it, directly: (*(*F)(...))(args)
						if len(stack) >= 3 {
							if star, ok := stack[len(stack)-2].(*ast.StarExpr); ok {
								_ = star
								add(pp+"|RECOVER dereferenced at once", 1)
							}
						}
					}
				}
				// GO-SIDE: (*uintptr)(unsafe.Pointer(&struct{ f F }{fn})) -- a func value read as a number
				if pt, ok := tv.Type.Underlying().(*types.Pointer); ok && isUintptr(pt.Elem()) && isUnsafePtr(info.TypeOf(arg)) {
					if inner, ok := arg.(*ast.CallExpr); ok && len(inner.Args) == 1 {
						if u, ok := ast.Unparen(inner.Args[0]).(*ast.UnaryExpr); ok && u.Op == token.AND {
							if cl, ok := ast.Unparen(u.X).(*ast.CompositeLit); ok {
								if st, ok := info.TypeOf(cl).Underlying().(*types.Struct); ok && st.NumFields() == 1 {
									if _, isFunc := st.Field(0).Type().Underlying().(*types.Signature); isFunc {
										add(pp+"|GO-SIDE: a struct{ f func } read as uintptr", 1)
									} else if _, isTP := st.Field(0).Type().(*types.TypeParam); isTP {
										add(pp+"|GO-SIDE: a struct{ f T } read as uintptr, T a type parameter", 1)
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
	add("ALL|distinct funcs minted", len(funcsMinted))
	add("ALL|distinct mint signatures", len(mintSigs))
	add("ALL|distinct recovery signatures", len(recSigs))
	// casts: a recovery signature identical to no mint signature
	casts := 0
	var castList []string
	for s := range recSigs {
		if _, ok := mintSigs[s]; !ok {
			casts++
			castList = append(castList, s)
		}
	}
	add("ALL|recovery signatures no mint has (a cast in C)", casts)
	// unused minted signature
	uncalled := 0
	for s := range mintSigs {
		if _, ok := recSigs[s]; !ok {
			uncalled++
		}
	}
	add("ALL|mint signatures never recovered in Go (passed to C only)", uncalled)
	var ks []string
	for k := range counts {
		ks = append(ks, k)
	}
	sort.Strings(ks)
	for _, k := range ks {
		fmt.Printf("%7d  %s\n", counts[k], k)
	}
	sort.Strings(castList)
	for i, c := range castList {
		if i >= 12 {
			fmt.Printf("  ... %d more\n", len(castList)-12)
			break
		}
		fmt.Println("  cast-only signature: " + c)
	}
	// top signatures
	type kv struct {
		k string
		v int
	}
	var top []kv
	for k, v := range recSigs {
		top = append(top, kv{k, v})
	}
	sort.Slice(top, func(i, j int) bool { return top[i].v > top[j].v })
	for i, t := range top {
		if i >= 6 {
			break
		}
		fmt.Printf("  recovery signature x%d: %s\n", t.v, strings.ReplaceAll(t.k, "modernc.org/libc.", "libc."))
	}
}
