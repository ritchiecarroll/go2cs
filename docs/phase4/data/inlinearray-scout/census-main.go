// iacensus: Go struct types holding an array field, and how those array fields are used, over a package pattern.
//   structs      named types whose underlying type is a struct; with an array field (direct, any length)
//   fields       array-typed fields; zero-length; element unmanaged (numbers, bool, unsafe-free fixed structs) or managed
//   uses of a field f, at its selector x.f:  sliced (x.f[a:b]), whole address (&x.f), element address (&x.f[i]),
//                unsafe (an unsafe.Pointer conversion of &x.f or &x.f[i])
//   struct-level unsafe: (*S)(p) with p an unsafe.Pointer, or unsafe.Pointer(e) with e a *S, for S holding an array field
package main

import (
	"flag"
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

func unmanaged(t types.Type, depth int) bool {
	if depth > 8 {
		return false
	}
	switch u := t.Underlying().(type) {
	case *types.Basic:
		return u.Kind() != types.String && u.Kind() != types.UntypedNil && u.Kind() != types.UnsafePointer
	case *types.Array:
		return unmanaged(u.Elem(), depth+1)
	case *types.Struct:
		for i := 0; i < u.NumFields(); i++ {
			if !unmanaged(u.Field(i).Type(), depth+1) {
				return false
			}
		}
		return true
	}
	return false
}

type fieldInfo struct {
	owner, name                            string
	length                                 int64
	unmanaged, test                        bool
	sliced, whole, elem, unsafeP, otherRef int
}

func main() {
	tests := flag.Bool("tests", true, "include test files")
	verbose := flag.Bool("v", false, "print fields with uses")
	flag.Parse()
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo, Tests: *tests}
	pkgs, err := packages.Load(cfg, flag.Args()...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
	// Keyed by qualified NAME, not by object: a package and its test variant type-check the same declaration twice.
	fields := map[string]*fieldInfo{}
	structsWith := map[string]bool{}
	structsAll := map[string]bool{}
	structUnsafe := map[string]int{}
	anonFields := 0
	tkey := func(o *types.TypeName) string { return o.Pkg().Path() + "." + o.Name() }
	seenFile := map[string]bool{}
	// pass 1: declarations
	for _, p := range pkgs {
		if p.TypesInfo == nil {
			continue
		}
		for _, f := range p.Syntax {
			fname := p.Fset.Position(f.Pos()).Filename
			isTest := strings.HasSuffix(fname, "_test.go")
			ast.Inspect(f, func(n ast.Node) bool {
				// Every struct type EXPRESSION, named or not: its array fields, keyed by position.
				if se, ok := n.(*ast.StructType); ok {
					if st, ok := p.TypesInfo.TypeOf(se).(*types.Struct); ok {
						for i := 0; i < st.NumFields(); i++ {
							fv := st.Field(i)
							if a, ok := fv.Type().Underlying().(*types.Array); ok {
								k := p.Fset.Position(fv.Pos()).String()
								if _, dup := fields[k]; !dup {
									fields[k] = &fieldInfo{owner: "struct@" + p.Fset.Position(se.Pos()).String(), name: fv.Name(), length: a.Len(), unmanaged: unmanaged(a.Elem(), 0), test: isTest}
									anonFields++
								}
							}
						}
					}
					return true
				}
				ts, ok := n.(*ast.TypeSpec)
				if !ok {
					return true
				}
				obj, _ := p.TypesInfo.Defs[ts.Name].(*types.TypeName)
				if obj == nil {
					return true
				}
				st, ok := obj.Type().Underlying().(*types.Struct)
				if !ok {
					return true
				}
				structsAll[tkey(obj)] = true
				for i := 0; i < st.NumFields(); i++ {
					fv := st.Field(i)
					if a, ok := fv.Type().Underlying().(*types.Array); ok {
						structsWith[tkey(obj)] = true
						k := p.Fset.Position(fv.Pos()).String()
						if _, dup := fields[k]; !dup {
							fields[k] = &fieldInfo{owner: obj.Pkg().Path() + "." + obj.Name(), name: fv.Name(), length: a.Len(), unmanaged: unmanaged(a.Elem(), 0), test: isTest}
						}
					}
				}
				return true
			})
		}
	}
	isUnsafePtr := func(t types.Type) bool {
		b, ok := t.(*types.Basic)
		return ok && b.Kind() == types.UnsafePointer
	}
	hasArrayField := func(t types.Type) bool {
		st, ok := t.Underlying().(*types.Struct)
		if !ok {
			return false
		}
		for i := 0; i < st.NumFields(); i++ {
			if _, ok := st.Field(i).Type().Underlying().(*types.Array); ok {
				return true
			}
		}
		return false
	}
	unsafeSites := 0
	ownerOfPtr := func(t types.Type) string {
		if pt, ok := t.Underlying().(*types.Pointer); ok && hasArrayField(pt.Elem()) {
			unsafeSites++
			return types.TypeString(types.Unalias(pt.Elem()), nil)
		}
		return ""
	}
	// pass 2: uses
	for _, p := range pkgs {
		if p.TypesInfo == nil {
			continue
		}
		info := p.TypesInfo
		for _, f := range p.Syntax {
			fname := p.Fset.Position(f.Pos()).Filename
			if seenFile[fname] {
				continue
			}
			seenFile[fname] = true
			fieldOf := func(e ast.Expr) *fieldInfo {
				e = ast.Unparen(e)
				if s, ok := e.(*ast.StarExpr); ok {
					e = ast.Unparen(s.X)
				}
				sel, ok := e.(*ast.SelectorExpr)
				if !ok {
					return nil
				}
				if v, ok := info.Uses[sel.Sel].(*types.Var); ok && v.IsField() {
					return fields[p.Fset.Position(v.Origin().Pos()).String()]
				}
				return nil
			}
			claimed := map[ast.Node]bool{}
			ast.Inspect(f, func(n ast.Node) bool {
				switch x := n.(type) {
				case *ast.SliceExpr:
					if fi := fieldOf(x.X); fi != nil {
						fi.sliced++
						claimed[ast.Unparen(x.X)] = true
					}
				case *ast.UnaryExpr:
					if x.Op.String() != "&" {
						break
					}
					if fi := fieldOf(x.X); fi != nil {
						fi.whole++
					} else if ix, ok := ast.Unparen(x.X).(*ast.IndexExpr); ok {
						if fi := fieldOf(ix.X); fi != nil {
							fi.elem++
						}
					}
				case *ast.CallExpr:
					if len(x.Args) != 1 {
						break
					}
					tv, ok := info.Types[x.Fun]
					if !ok || !tv.IsType() {
						break
					}
					arg := ast.Unparen(x.Args[0])
					argT := info.TypeOf(arg)
					if isUnsafePtr(tv.Type) {
						if u, ok := arg.(*ast.UnaryExpr); ok && u.Op.String() == "&" {
							if fi := fieldOf(u.X); fi != nil {
								fi.unsafeP++
							} else if ix, ok := ast.Unparen(u.X).(*ast.IndexExpr); ok {
								if fi := fieldOf(ix.X); fi != nil {
									fi.unsafeP++
								}
							}
						}
						if argT != nil {
							if o := ownerOfPtr(argT); o != "" {
								structUnsafe[o]++
							}
						}
					} else if argT != nil && isUnsafePtr(argT) {
						if o := ownerOfPtr(tv.Type); o != "" {
							structUnsafe[o]++
						}
					}
				}
				return true
			})
		}
	}
	nf, zero, unm, man, sliced, whole, elem, unsafeP, anyRefusal := 0, 0, 0, 0, 0, 0, 0, 0, 0
	var rows []string
	for _, fi := range fields {
		nf++
		if fi.length == 0 {
			zero++
		}
		if fi.unmanaged {
			unm++
		} else {
			man++
		}
		if fi.sliced > 0 {
			sliced++
		}
		if fi.whole > 0 {
			whole++
		}
		if fi.elem > 0 {
			elem++
		}
		if fi.unsafeP > 0 {
			unsafeP++
		}
		if fi.sliced+fi.whole+fi.unsafeP > 0 {
			anyRefusal++
		}
		if *verbose && fi.sliced+fi.whole+fi.elem+fi.unsafeP > 0 {
			rows = append(rows, fmt.Sprintf("%s.%s [%d] unmanaged=%v sliced=%d whole=%d elem=%d unsafe=%d", fi.owner, fi.name, fi.length, fi.unmanaged, fi.sliced, fi.whole, fi.elem, fi.unsafeP))
		}
	}
	nUnsafeStructs := 0
	for range structUnsafe {
		nUnsafeStructs++
	}
	fmt.Printf("packages %d; struct types %d; with an array field %d\n", len(pkgs), len(structsAll), len(structsWith))
	fmt.Printf("array fields %d (%d first met in a struct type expression rather than a named declaration): zero-length %d; unmanaged element %d; managed element %d\n", nf, anonFields, zero, unm, man)
	fmt.Printf("fields ever sliced %d; whole-address-taken %d; element-address-taken %d; through unsafe.Pointer %d; any of REC-A's refusals (sliced, &whole, unsafe) %d\n", sliced, whole, elem, unsafeP, anyRefusal)
	fmt.Printf("struct types (named, alias or anonymous) holding an array field, converted to/from unsafe.Pointer as a whole: %d distinct, %d sites\n", nUnsafeStructs, unsafeSites)
	// Distinct lengths: of every array field, and of the fields a first increment admits (unmanaged, nonzero, none of
	// REC-A's refusals and no element address).
	allN, admitN := map[int64]int{}, map[int64]int{}
	admitted := 0
	for _, fi := range fields {
		allN[fi.length]++
		if fi.unmanaged && fi.length > 0 && fi.sliced+fi.whole+fi.unsafeP+fi.elem == 0 {
			admitN[fi.length]++
			admitted++
		}
	}
	keys := func(m map[int64]int) []int64 {
		var k []int64
		for n := range m {
			k = append(k, n)
		}
		sort.Slice(k, func(i, j int) bool { return k[i] < k[j] })
		return k
	}
	ak := keys(admitN)
	fmt.Printf("distinct lengths: %d over all array fields; %d over the %d admitted fields: %v\n", len(allN), len(ak), admitted, ak)
	sort.Strings(rows)
	for _, r := range rows {
		fmt.Println(r)
	}
}

func derefT(t types.Type) types.Type {
	if p, ok := t.Underlying().(*types.Pointer); ok {
		return p.Elem()
	}
	return t
}
