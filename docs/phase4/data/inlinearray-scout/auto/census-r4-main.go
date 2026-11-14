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
	sliced, whole, elem, unsafeP           int
	implicit, nested, value                int // review item 1: pointer-method receivers, faces below x.f, whole-value flows
}

func main() {
	tests := flag.Bool("tests", true, "include test files")
	verbose := flag.Bool("v", false, "print fields with uses")
	flag.Int64Var(&structCap, "structcap", 65536, "per-struct size cap in bytes, every level")
	auto := flag.Bool("auto", false, "run the auto-admission census (auto.go) instead")
	flag.Parse()
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo, Tests: *tests}
	pkgs, err := packages.Load(cfg, flag.Args()...)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
	if *auto {
		runAuto(pkgs, structCap)
		return
	}
	// Keyed by qualified NAME, not by object: a package and its test variant type-check the same declaration twice.
	fields := map[string]*fieldInfo{}
	structsWith := map[string]bool{}
	structsAll := map[string]bool{}
	structUnsafe := map[string]int{}
	anonFields := 0
	// Every struct type met, named or a type expression, for the declaration-local re-derivation (review items 2, 3, 8).
	structTypes := map[string]*types.Struct{}
	unsafeTypes := map[string]types.Type{}
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
						structTypes["expr@"+p.Fset.Position(se.Pos()).String()] = st
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
			k := types.TypeString(types.Unalias(pt.Elem()), nil)
			unsafeTypes[k] = pt.Elem()
			return k
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
			// chain: the array fields whose storage holds e, walking index-into-array and direct field selections.
			// The top field (e itself is x.f) is reported separately from the fields below it.
			chain := func(e ast.Expr) (top *fieldInfo, below []*fieldInfo) {
				first := true
				for {
					e = ast.Unparen(e)
					switch x := e.(type) {
					case *ast.IndexExpr:
						if _, ok := info.TypeOf(x.X).Underlying().(*types.Array); !ok {
							return
						}
						e, first = x.X, false
						continue
					case *ast.SelectorExpr:
						sel := info.Selections[x]
						if sel == nil || sel.Kind() != types.FieldVal {
							return
						}
						if fi := fieldOf(x); fi != nil {
							if first {
								top = fi
							} else {
								below = append(below, fi)
							}
						}
						if sel.Indirect() {
							return
						}
						if _, ok := info.TypeOf(x.X).Underlying().(*types.Pointer); ok {
							return
						}
						e, first = x.X, false
						continue
					}
					return
				}
			}
			var stack []ast.Node
			ast.Inspect(f, func(n ast.Node) bool {
				if n == nil {
					stack = stack[:len(stack)-1]
					return true
				}
				var parent ast.Node
				for i := len(stack) - 1; i >= 0; i-- {
					if _, ok := stack[i].(*ast.ParenExpr); !ok {
						parent = stack[i]
						break
					}
				}
				stack = append(stack, n)
				switch x := n.(type) {
				case *ast.SliceExpr:
					_, below := chain(x.X)
					for _, fi := range below {
						fi.nested++
					}
				case *ast.UnaryExpr:
					if x.Op.String() == "&" {
						_, below := chain(x.X)
						for _, fi := range below {
							fi.nested++
						}
					}
				case *ast.CallExpr:
					// A pointer-receiver method called on an addressable value takes its address implicitly:
					// h.counts[i].Add(1) is (&h.counts[i]).Add(1).
					if sx, ok := ast.Unparen(x.Fun).(*ast.SelectorExpr); ok {
						if sel := info.Selections[sx]; sel != nil && sel.Kind() == types.MethodVal {
							if sig, ok := sel.Obj().Type().(*types.Signature); ok && sig.Recv() != nil {
								if _, ptrRecv := sig.Recv().Type().(*types.Pointer); ptrRecv && !sel.Indirect() {
									if _, recvPtr := info.TypeOf(sx.X).Underlying().(*types.Pointer); !recvPtr {
										top, below := chain(sx.X)
										if top != nil {
											top.implicit++
										}
										for _, fi := range below {
											fi.implicit++
										}
									}
								}
							}
						}
					}
				case *ast.SelectorExpr:
					// A whole-value use of x.f: anything but an index, a slice, an address, len/cap, or a selector base.
					fi := fieldOf(x)
					if fi == nil {
						break
					}
					if s, ok := info.Selections[x]; !ok || s.Kind() != types.FieldVal {
						break
					}
					switch pn := parent.(type) {
					case *ast.IndexExpr:
						if ast.Unparen(pn.X) == x {
							return true
						}
					case *ast.SliceExpr:
						if ast.Unparen(pn.X) == x {
							return true
						}
					case *ast.UnaryExpr:
						if pn.Op.String() == "&" {
							return true
						}
					case *ast.SelectorExpr:
						if ast.Unparen(pn.X) == x {
							return true
						}
					case *ast.CallExpr:
						if id, ok := ast.Unparen(pn.Fun).(*ast.Ident); ok {
							if b, ok := info.Uses[id].(*types.Builtin); ok && (b.Name() == "len" || b.Name() == "cap") {
								return true
							}
						}
					case *ast.RangeStmt:
						// for i := range x.f reads no element value; for i, v := range x.f copies (a value use).
						if pn.X == x && pn.Value == nil {
							return true
						}
					}
					fi.value++
				}
				return true
			})
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
		if *verbose && fi.sliced+fi.whole+fi.elem+fi.unsafeP+fi.implicit+fi.nested > 0 {
			rows = append(rows, fmt.Sprintf("%s.%s [%d] unmanaged=%v sliced=%d whole=%d elem=%d unsafe=%d implicit=%d nested=%d value=%d", fi.owner, fi.name, fi.length, fi.unmanaged, fi.sliced, fi.whole, fi.elem, fi.unsafeP, fi.implicit, fi.nested, fi.value))
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
	// Review item 1: the faces the first predicate missed.
	impl, nest, val, valSites, implNew, nestNew, still := 0, 0, 0, 0, 0, 0, 0
	for _, fi := range fields {
		old := fi.unmanaged && fi.length > 0 && fi.sliced+fi.whole+fi.unsafeP+fi.elem == 0
		if fi.implicit > 0 {
			impl++
			if old {
				implNew++
			}
		}
		if fi.nested > 0 {
			nest++
			if old && fi.implicit == 0 {
				nestNew++
			}
		}
		if fi.value > 0 {
			val++
			valSites += fi.value
		}
		if old && fi.implicit+fi.nested == 0 {
			still++
		}
	}
	fmt.Printf("faces the first predicate missed: implicit address (pointer-method receiver) %d fields; address or slice below the field (x.f[i][:], &x.f[i].g) %d fields; of the %d admitted, %d have an implicit address and %d more a face below; %d remain\n", impl, nest, admitted, implNew, nestNew, still)
	fmt.Printf("whole-value uses of a field (copy, argument, conversion, compare, store, range with a value): %d fields, %d sites\n", val, valSites)
	sizes := types.SizesFor("gc", "amd64")
	describe := func(label string, set map[string]types.Type) {
		n, unm, capped, zeroAlign, seqWrong, nestWrong, withBool, anyArr, overKiB := 0, 0, 0, 0, 0, 0, 0, 0, 0
		for _, t := range set {
			st, ok := t.Underlying().(*types.Struct)
			if !ok {
				continue
			}
			n++
			if !holdsArray(st, 0) {
				continue
			}
			anyArr++
			if !unmanaged(st, 0) {
				continue
			}
			unm++
			if !underCaps(sizes, st, 0) {
				continue
			}
			capped++
			if sizes.Sizeof(st) > 1024 {
				overKiB++
			}
			if hasAlignZero(sizes, st, 0) {
				zeroAlign++
			}
			ss, sa := seqLayout(sizes, st, 0)
			if ss != sizes.Sizeof(st) || sa != sizes.Alignof(st) {
				seqWrong++
				if *verbose {
					fmt.Printf("  %s: sequential without zero-size fields reads size %d align %d; Go %d align %d: %s\n", label[:strings.Index(label, " ")], ss, sa, sizes.Sizeof(st), sizes.Alignof(st), types.TypeString(t, nil))
				}
			}
			if sa != sizes.Alignof(st) {
				nestWrong++
			}
			if hasBool(st, 0) {
				withBool++
			}
		}
		fmt.Printf("%s: %d struct types; holding an array at any depth %d; all-unmanaged once inline %d; within the caps (4 KiB per array, %d bytes per struct, every level) %d; of those: carry an alignment-forcing [0]T %d, sequential layout without [0]T differs from Go in size or alignment %d (in alignment, so wrong when nested %d), hold a bool at some depth %d; larger than 1 KiB (heap-boxed when held by value in a local or parameter) %d\n", label, n, anyArr, unm, structCap, capped, zeroAlign, seqWrong, nestWrong, withBool, overKiB)
	}
	describe("whole-converted through unsafe.Pointer (review item 8)", unsafeTypes)
	all := map[string]types.Type{}
	for k, st := range structTypes {
		all[k] = st
	}
	describe("every struct type expression (declaration-local admission)", all)
	sort.Strings(rows)
	for _, r := range rows {
		fmt.Println(r)
	}
}

func holdsArray(st *types.Struct, depth int) bool {
	if depth > 8 {
		return false
	}
	for i := 0; i < st.NumFields(); i++ {
		switch u := st.Field(i).Type().Underlying().(type) {
		case *types.Array:
			return true
		case *types.Struct:
			if holdsArray(u, depth+1) {
				return true
			}
		}
	}
	return false
}

func hasBool(t types.Type, depth int) bool {
	if depth > 8 {
		return false
	}
	switch u := t.Underlying().(type) {
	case *types.Basic:
		return u.Kind() == types.Bool
	case *types.Array:
		return hasBool(u.Elem(), depth+1)
	case *types.Struct:
		for i := 0; i < u.NumFields(); i++ {
			if hasBool(u.Field(i).Type(), depth+1) {
				return true
			}
		}
	}
	return false
}

var structCap int64 = 65536

// underCaps: every array no larger than 4 KiB and every struct no larger than structCap, at every level.
func underCaps(sz types.Sizes, t types.Type, depth int) bool {
	if depth > 8 {
		return false
	}
	switch u := t.Underlying().(type) {
	case *types.Array:
		return sz.Sizeof(u) <= 4096 && underCaps(sz, u.Elem(), depth+1)
	case *types.Struct:
		if sz.Sizeof(u) > structCap {
			return false
		}
		for i := 0; i < u.NumFields(); i++ {
			if !underCaps(sz, u.Field(i).Type(), depth+1) {
				return false
			}
		}
	}
	return true
}

// hasAlignZero: a zero-length array field whose element alignment exceeds 1, at any depth.
func hasAlignZero(sz types.Sizes, t types.Type, depth int) bool {
	if depth > 8 {
		return false
	}
	switch u := t.Underlying().(type) {
	case *types.Array:
		if u.Len() == 0 {
			return sz.Alignof(u.Elem()) > 1
		}
		return hasAlignZero(sz, u.Elem(), depth+1)
	case *types.Struct:
		for i := 0; i < u.NumFields(); i++ {
			if hasAlignZero(sz, u.Field(i).Type(), depth+1) {
				return true
			}
		}
	}
	return false
}

// seqLayout: the CLR sequential layout of the inline form when a zero-size field contributes neither bytes nor
// alignment (what an emission that drops [0]T, or gives it no alignment, produces). Go's trailing zero-size padding
// is dropped with it.
func seqLayout(sz types.Sizes, t types.Type, depth int) (size, align int64) {
	if depth > 8 {
		return sz.Sizeof(t), sz.Alignof(t)
	}
	switch u := t.Underlying().(type) {
	case *types.Array:
		if u.Len() == 0 {
			return 0, 1
		}
		es, ea := seqLayout(sz, u.Elem(), depth+1)
		return es * u.Len(), ea
	case *types.Struct:
		var off int64
		align := int64(1)
		for i := 0; i < u.NumFields(); i++ {
			fs, fa := seqLayout(sz, u.Field(i).Type(), depth+1)
			if fs == 0 {
				continue
			}
			off = (off + fa - 1) / fa * fa
			off += fs
			if fa > align {
				align = fa
			}
		}
		return (off + align - 1) / align * align, align
	}
	return sz.Sizeof(t), sz.Alignof(t)
}

func derefT(t types.Type) types.Type {
	if p, ok := t.Underlying().(*types.Pointer); ok {
		return p.Elem()
	}
	return t
}
