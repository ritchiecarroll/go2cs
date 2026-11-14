// auto.go: the -auto mode. Can I0's admission be auto-detected? (COORD's owner ask, 2026-11-13.)
//
// THE CANDIDATE RULE: a struct type is admitted when its OWN declaring package, in its NON-TEST files, converts a value
// of it WHOLE through unsafe.Pointer, and it passes 7.7's declaration-local rules and caps.
//
// For every conversion site (*X)(p) with p an unsafe.Pointer, or unsafe.Pointer(e) with e a *X, the types that must be
// inline for X to be layout-equal are NEEDS(X): every struct type reachable from X BY VALUE (X itself, array elements,
// struct fields, at any depth) that holds an array at any depth. Each such type is recorded against the CONVERTING
// package and file kind. A named type's declaring package is its object's package. An anonymous struct type has no
// declaring package; go2cs emits it in the package that spells it, so it is counted as declared where it is converted.
//
// Blind spots counted, not resolved: a conversion whose target holds a TYPE PARAMETER (the converted type is chosen
// by the instantiation, out of the census's sight), and reflect.NewAt (a reinterpretation by reflection).
package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

type autoType struct {
	key, declPkg string
	anon, top    bool // top: a conversion's own target, not only reached by nesting
	st           *types.Struct
	convNonTest  map[string]int // converting package -> sites, non-test files
	convTest     map[string]int // converting package -> sites, _test.go files
}

func holdsTypeParam(t types.Type, depth int) bool {
	if depth > 8 {
		return false
	}
	switch u := t.(type) {
	case *types.TypeParam:
		return true
	case *types.Pointer:
		return holdsTypeParam(u.Elem(), depth+1)
	case *types.Array:
		return holdsTypeParam(u.Elem(), depth+1)
	case *types.Named:
		for i := 0; i < u.TypeArgs().Len(); i++ {
			if holdsTypeParam(u.TypeArgs().At(i), depth+1) {
				return true
			}
		}
		return holdsTypeParam(u.Underlying(), depth+1)
	case *types.Struct:
		for i := 0; i < u.NumFields(); i++ {
			if holdsTypeParam(u.Field(i).Type(), depth+1) {
				return true
			}
		}
	}
	return false
}

// needs walks X by value and calls f for each struct type holding an array at any depth.
func needs(t types.Type, depth int, f func(types.Type, *types.Struct)) {
	if depth > 10 {
		return
	}
	switch u := t.Underlying().(type) {
	case *types.Array:
		needs(u.Elem(), depth+1, f)
	case *types.Struct:
		if holdsArray(u, 0) {
			f(t, u)
		}
		for i := 0; i < u.NumFields(); i++ {
			needs(u.Field(i).Type(), depth+1, f)
		}
	}
}

func runAuto(pkgs []*packages.Package, structCap int64) {
	sizes := types.SizesFor("gc", "amd64")
	all := map[string]*autoType{}
	genericSites, genericNonTest, newAtSites := 0, 0, 0
	type pair struct{ dst, src types.Type; where string }
	var pairs []pair
	var genericExamples []string
	seenFile := map[string]bool{}
	isUnsafePtr := func(t types.Type) bool {
		b, ok := t.(*types.Basic)
		return ok && b.Kind() == types.UnsafePointer
	}
	for _, p := range pkgs {
		if p.TypesInfo == nil {
			continue
		}
		info := p.TypesInfo
		convPkg := strings.TrimSuffix(p.PkgPath, "_test")
		for _, f := range p.Syntax {
			fname := p.Fset.Position(f.Pos()).Filename
			if seenFile[fname] {
				continue
			}
			seenFile[fname] = true
			isTest := strings.HasSuffix(fname, "_test.go")
			record := func(elem types.Type, pos ast.Node) {
				if holdsTypeParam(elem, 0) {
					genericSites++
					if !isTest {
						genericNonTest++
						if len(genericExamples) < 6 {
							genericExamples = append(genericExamples, p.Fset.Position(pos.Pos()).String()+" "+types.TypeString(elem, nil))
						}
					}
					return
				}
				target := types.Unalias(elem)
				needs(elem, 0, func(t types.Type, st *types.Struct) {
					t = types.Unalias(t)
					var key, decl string
					anon := false
					if n, ok := t.(*types.Named); ok && n.Obj().Pkg() != nil {
						decl = n.Obj().Pkg().Path()
						key = decl + "." + n.Obj().Name()
						if n.TypeArgs().Len() > 0 {
							key = types.TypeString(t, nil)
						}
					} else {
						anon = true
						decl = convPkg
						key = "anon:" + types.TypeString(t, nil)
					}
					a := all[key]
					if a == nil {
						a = &autoType{key: key, declPkg: decl, anon: anon, st: st, convNonTest: map[string]int{}, convTest: map[string]int{}}
						all[key] = a
					}
					if types.Identical(t, target) {
						a.top = true
					}
					if isTest {
						a.convTest[convPkg]++
					} else {
						a.convNonTest[convPkg]++
					}
				})
			}
			ast.Inspect(f, func(n ast.Node) bool {
				call, ok := n.(*ast.CallExpr)
				if !ok {
					return true
				}
				if sel, ok := ast.Unparen(call.Fun).(*ast.SelectorExpr); ok && sel.Sel.Name == "NewAt" {
					if fn, ok := info.Uses[sel.Sel].(*types.Func); ok && fn.Pkg() != nil && fn.Pkg().Path() == "reflect" {
						newAtSites++
					}
				}
				if len(call.Args) != 1 {
					return true
				}
				tv, ok := info.Types[call.Fun]
				if !ok || !tv.IsType() {
					return true
				}
				argT := info.TypeOf(ast.Unparen(call.Args[0]))
				if argT == nil {
					return true
				}
				if isUnsafePtr(tv.Type) {
					if pt, ok := argT.Underlying().(*types.Pointer); ok {
						record(pt.Elem(), call)
					}
				} else if isUnsafePtr(argT) {
					if pt, ok := tv.Type.Underlying().(*types.Pointer); ok {
						record(pt.Elem(), call)
						// The reinterpret chain (*T)(unsafe.Pointer(e)), e a *X: T and X must agree on layout.
						if inner, ok := ast.Unparen(call.Args[0]).(*ast.CallExpr); ok && len(inner.Args) == 1 && !isTest {
							if itv, ok := info.Types[inner.Fun]; ok && itv.IsType() && isUnsafePtr(itv.Type) {
								if et := info.TypeOf(ast.Unparen(inner.Args[0])); et != nil {
									if ep, ok := et.Underlying().(*types.Pointer); ok {
										pairs = append(pairs, pair{pt.Elem(), ep.Elem(), p.Fset.Position(call.Pos()).String()})
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
	loaded := map[string]bool{}
	for _, p := range pkgs {
		loaded[strings.TrimSuffix(p.PkgPath, "_test")] = true
	}
	// The opt-in's population, keyed as above: every struct type declared or spelled in a loaded package's non-test
	// files that holds an array at any depth and passes 7.7's declaration-local rules (unmanaged, caps).
	optIn := map[string]string{} // key -> declaring package (for an anonymous type, the first package that spells it)
	spelledIn := map[string]map[string]bool{}
	spell := func(key, pkg string) {
		if spelledIn[key] == nil {
			spelledIn[key] = map[string]bool{}
		}
		spelledIn[key][pkg] = true
	}
	seenOpt := map[string]bool{}
	for _, p := range pkgs {
		if p.TypesInfo == nil {
			continue
		}
		convPkg := strings.TrimSuffix(p.PkgPath, "_test")
		for _, f := range p.Syntax {
			fname := p.Fset.Position(f.Pos()).Filename
			if seenOpt[fname] || strings.HasSuffix(fname, "_test.go") {
				continue
			}
			seenOpt[fname] = true
			named := map[ast.Node]bool{}
			ast.Inspect(f, func(n ast.Node) bool {
				switch x := n.(type) {
				case *ast.TypeSpec:
					obj, _ := p.TypesInfo.Defs[x.Name].(*types.TypeName)
					if obj == nil || x.TypeParams != nil {
						return true
					}
					if st, ok := obj.Type().Underlying().(*types.Struct); ok && !obj.IsAlias() {
						named[x.Type] = true
						if holdsArray(st, 0) && unmanaged(st, 0) && underCaps(sizes, st, 0) {
							optIn[obj.Pkg().Path()+"."+obj.Name()] = obj.Pkg().Path()
						}
					}
				case *ast.StructType:
					if named[x] {
						return true
					}
					if st, ok := p.TypesInfo.TypeOf(x).(*types.Struct); ok && !holdsTypeParam(st, 0) {
						k := "anon:" + types.TypeString(st, nil)
						spell(k, convPkg)
						if holdsArray(st, 0) && unmanaged(st, 0) && underCaps(sizes, st, 0) {
							if _, seen := optIn[k]; !seen {
								optIn[k] = convPkg
							}
						}
					}
				}
				return true
			})
		}
	}
	// An anonymous type is IN-PACKAGE when a package that spells it (a struct expression or an alias) converts it.
	own := func(a *autoType) bool {
		if !a.anon {
			return a.convNonTest[a.declPkg] > 0
		}
		for k := range a.convNonTest {
			if spelledIn[a.key][k] {
				return true
			}
		}
		return false
	}
	for _, a := range all {
		if !a.anon {
			continue
		}
		best := ""
		for k := range a.convNonTest {
			if spelledIn[a.key][k] && (best == "" || k < best) {
				best = k
			}
		}
		if best == "" {
			for k := range spelledIn[a.key] {
				if best == "" || k < best {
					best = k
				}
			}
		}
		if best != "" {
			a.declPkg = best
		}
	}
	// Per declaring package (loaded ones only), the rule's outcome.
	type tally struct{ needed, inPkg, otherOnly, testOnly, admit, failCaps, managed int }
	byDecl := map[string]*tally{}
	var otherOnlyRows, failRows []string
	for _, a := range all {
		if !loaded[a.declPkg] {
			continue
		}
		t := byDecl[a.declPkg]
		if t == nil {
			t = &tally{}
			byDecl[a.declPkg] = t
		}
		t.needed++
		switch {
		case own(a):
			t.inPkg++
			if !unmanaged(a.st, 0) {
				t.managed++
				failRows = append(failRows, "managed  "+a.key)
			} else if !underCaps(sizes, a.st, 0) {
				t.failCaps++
				failRows = append(failRows, "caps     "+a.key)
			} else {
				t.admit++
			}
		case len(a.convNonTest) > 0:
			t.otherOnly++
			var by []string
			for k := range a.convNonTest {
				by = append(by, k)
			}
			sort.Strings(by)
			otherOnlyRows = append(otherOnlyRows, a.key+" <- "+strings.Join(by, ","))
		default:
			t.testOnly++
		}
	}
	optByPkg := map[string][3]int{} // opt-in admits, auto admits too, auto admits outside opt-in
	for k, d := range optIn {
		c := optByPkg[d]
		c[0]++
		if a := all[k]; a != nil && own(a) {
			c[1]++
		}
		optByPkg[d] = c
	}
	for k, a := range all {
		if !loaded[a.declPkg] || !own(a) || !unmanaged(a.st, 0) || !underCaps(sizes, a.st, 0) {
			continue
		}
		if _, ok := optIn[k]; !ok {
			c := optByPkg[a.declPkg]
			c[2]++
			optByPkg[a.declPkg] = c
			fmt.Println("  auto admits outside the opt-in population: " + k)
		}
	}
	var optPkgs []string
	for d := range optByPkg {
		optPkgs = append(optPkgs, d)
	}
	sort.Strings(optPkgs)
	for _, d := range optPkgs {
		c := optByPkg[d]
		fmt.Printf("OPTIN %s: the opt-in admits %d; auto admits %d of them; auto admits %d the opt-in does not\n", d, c[0], c[1], c[2])
	}
	// Pairs: a chain whose two sides both need inline storage, but exactly one side is admitted by the rule.
	admittedSet := func(t types.Type) (needsAny, allAdmitted bool) {
		allAdmitted = true
		needs(t, 0, func(nt types.Type, st *types.Struct) {
			needsAny = true
			nt = types.Unalias(nt)
			key := "anon:" + types.TypeString(nt, nil)
			if n, ok := nt.(*types.Named); ok && n.Obj().Pkg() != nil {
				key = n.Obj().Pkg().Path() + "." + n.Obj().Name()
				if n.TypeArgs().Len() > 0 {
					key = types.TypeString(nt, nil)
				}
			}
			a := all[key]
			if a == nil || !own(a) || !unmanaged(a.st, 0) || !underCaps(sizes, a.st, 0) {
				allAdmitted = false
			}
		})
		return
	}
	mixed, bothIn, bothOut, oneSide, oneSideIn := 0, 0, 0, 0, 0
	for _, pr := range pairs {
		dn, da := admittedSet(pr.dst)
		sn, sa := admittedSet(pr.src)
		switch {
		case !dn && !sn:
			continue
		case dn != sn:
			oneSide++ // only one side holds an array: the other is bytes or scalars, e.g. *(*uint64)(unsafe.Pointer(&s))
			if (dn && da) || (sn && sa) {
				oneSideIn++
			}
		case da && sa:
			bothIn++
		case !da && !sa:
			bothOut++
		default:
			mixed++
			fmt.Printf("  MIXED pair (one side admitted, the other not): %s %s <- %s\n", pr.where, types.TypeString(pr.dst, nil), types.TypeString(pr.src, nil))
		}
	}
	fmt.Printf("PAIRS (*T)(unsafe.Pointer(&x)) chains in non-test files: %d; both sides need inline storage and both admitted %d, both not admitted %d, MIXED %d; only one side holds an array %d (that side admitted in %d)\n", len(pairs), bothIn, bothOut, mixed, oneSide, oneSideIn)
	var rowsOut []string
	for _, a := range all {
		if !loaded[a.declPkg] {
			continue
		}
		direct := false
		for i := 0; i < a.st.NumFields(); i++ {
			if _, ok := a.st.Field(i).Type().Underlying().(*types.Array); ok {
				direct = true
			}
		}
		var by []string
		for k := range a.convNonTest {
			by = append(by, k)
		}
		sort.Strings(by)
		rowsOut = append(rowsOut, fmt.Sprintf("  row: %s\tdecl=%s\tanon=%v\tdirect-array=%v\ttop=%v\tsize=%d\tconverted-in=%s", a.key, a.declPkg, a.anon, direct, a.top, sizes.Sizeof(a.st), strings.Join(by, ",")))
	}
	sort.Strings(rowsOut)
	defer func() {
		for _, r := range rowsOut {
			fmt.Println(r)
		}
	}()
	var decls []string
	for d := range byDecl {
		decls = append(decls, d)
	}
	sort.Strings(decls)
	sum := tally{}
	for _, d := range decls {
		t := byDecl[d]
		fmt.Printf("AUTO %s: needs-inline %d; converted whole in its own non-test files %d (admitted %d, managed %d, over the caps %d); converted only in another package's non-test files %d; only in test files %d\n", d, t.needed, t.inPkg, t.admit, t.managed, t.failCaps, t.otherOnly, t.testOnly)
		sum.needed += t.needed
		sum.inPkg += t.inPkg
		sum.admit += t.admit
		sum.managed += t.managed
		sum.failCaps += t.failCaps
		sum.otherOnly += t.otherOnly
		sum.testOnly += t.testOnly
	}
	fmt.Printf("AUTO TOTAL over %d declaring packages: needs-inline %d; own-package %d (admitted %d, managed %d, over the caps %d); other-package only %d; test only %d\n", len(decls), sum.needed, sum.inPkg, sum.admit, sum.managed, sum.failCaps, sum.otherOnly, sum.testOnly)
	fmt.Printf("AUTO blind spots: conversions whose target holds a type parameter %d (%d in non-test files); reflect.NewAt calls %d\n", genericSites, genericNonTest, newAtSites)
	for _, e := range genericExamples {
		fmt.Println("  generic: " + e)
	}
	sort.Strings(otherOnlyRows)
	for _, r := range otherOnlyRows {
		fmt.Println("  other-package only: " + r)
	}
	sort.Strings(failRows)
	for _, r := range failRows {
		fmt.Println("  own-package but not admissible: " + r)
	}
	_ = structCap
}
