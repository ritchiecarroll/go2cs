// census: the METHOD-SET population a cross-package promoted-method forwarder change in go2cs-gen would add.
//
// go2cs-gen mints a promoted method's forwarder only through SAME-Go-package embeds (GetMetadataPromotedMethods returns
// nothing for a foreign embed), and golib reconstructs a Go method set at run time from the EMITTED extension methods
// (GetGoMethodSetCandidates). So a concrete method promoted into T's (or *T's) method set through an embed path with a
// hop that crosses packages is ABSENT at run time today: every dynamic assert / type switch / fmt verb that consults it
// answers MISS where Go answers HIT.
//
// For every named type declared in each loaded package (package-level and local, production and test variants), this
// lists each method of types.NewMethodSet(T) and NewMethodSet(*T) that is promoted (index path >= 2), concrete (not an
// interface's method), and reached through a package-crossing hop. go/types applies Go's shadowing and same-depth
// ambiguity rules, so a name Go drops never appears. Each row also carries:
//   - set: "T" (in the value set, hence also *T's) or "*T" (pointer set only);
//   - the method's receiver kind, the embed path, and the kind (value / pointer) of every hop;
//   - exported or not (an unexported promoted method is only matchable by an interface in the METHOD's package);
//   - shim: a pointer-receiver method reached through a VALUE embed deeper than depth 1 (the generator's box shim is
//     direct-embed only: a residual, same as for same-package embeds);
//   - clash: the enclosing package declares a package-level FUNCTION of the same name (the forwarder lands in the same
//     static package class, so it joins that overload set).
//
// Then, per type, the well-known interfaces whose satisfaction by T or *T DEPENDS on at least one such method.
//
// Usage: census [-root dir] <patterns...>
package main

import (
	"flag"
	"fmt"
	"go/types"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

func deref(t types.Type) (types.Type, bool) {
	if p, ok := types.Unalias(t).(*types.Pointer); ok {
		return p.Elem(), true
	}
	return t, false
}

func named(t types.Type) *types.Named {
	t, _ = deref(t)
	n, _ := types.Unalias(t).(*types.Named)
	return n
}

// walk describes a promoted selection's embed path: the field names, the kind of each hop, whether any hop crosses
// packages, and the depth of the first crossing.
func walk(sel *types.Selection) (crosses bool, path string, kinds string, valueDepth2 bool) {
	crosses, path, kinds, valueDepth2, _ = walkAt(sel)
	return
}

// walkAt is walk plus the index of the first package-crossing hop (-1 when none).
func walkAt(sel *types.Selection) (crosses bool, path string, kinds string, valueDepth2 bool, firstCross int) {
	firstCross = -1
	cur := sel.Recv()
	var hops, ks []string
	for i, idx := range sel.Index()[:len(sel.Index())-1] {
		owner := named(cur)
		base, _ := deref(cur)
		st, ok := types.Unalias(base).Underlying().(*types.Struct)
		if !ok || owner == nil {
			return false, "", "", false, -1
		}
		f := st.Field(idx)
		next := named(f.Type())
		if next == nil {
			return false, "", "", false, -1
		}
		if owner.Obj().Pkg() != next.Obj().Pkg() {
			crosses = true
			if firstCross < 0 {
				firstCross = i
			}
		}
		_, isPtr := deref(f.Type())
		k := "v"
		if isPtr {
			k = "p"
		}
		if i >= 1 && !isPtr {
			valueDepth2 = true
		}
		hops = append(hops, f.Name())
		ks = append(ks, k)
		cur = f.Type()
	}
	return crosses, strings.Join(hops, "."), strings.Join(ks, ""), valueDepth2, firstCross
}

type iface struct {
	name string
	t    *types.Interface
}

func main() {
	root := flag.String("root", "", "directory to load from (default: current)")
	flag.Parse()
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo | packages.NeedDeps | packages.NeedImports, Tests: true, Dir: *root}
	pkgs, err := packages.Load(cfg, flag.Args()...)
	if err != nil {
		fmt.Println("load:", err)
		os.Exit(2)
	}
	loadErrors := 0
	var all []*packages.Package
	packages.Visit(pkgs, nil, func(p *packages.Package) { loadErrors += len(p.Errors); all = append(all, p) })

	// The well-known interfaces, from the canonical (non-test-variant) packages in the import graph.
	want := map[string][]string{
		"fmt":           {"Stringer", "GoStringer", "Formatter"},
		"encoding/json": {"Marshaler", "Unmarshaler"},
		"encoding":      {"TextMarshaler", "TextUnmarshaler", "BinaryMarshaler", "BinaryUnmarshaler"},
		"io":            {"Reader", "Writer", "Closer", "ReaderFrom", "WriterTo", "StringWriter", "ByteReader"},
		"sort":          {"Interface"},
	}
	var ifaces []iface
	ifaces = append(ifaces, iface{"error", types.Universe.Lookup("error").Type().Underlying().(*types.Interface)})
	gotIface := map[string]bool{}
	for _, p := range all {
		names, ok := want[p.PkgPath]
		if !ok || p.ID != p.PkgPath || p.Types == nil {
			continue
		}
		for _, n := range names {
			key := p.PkgPath + "." + n
			if gotIface[key] {
				continue
			}
			if obj := p.Types.Scope().Lookup(n); obj != nil {
				if it, ok := obj.Type().Underlying().(*types.Interface); ok {
					ifaces = append(ifaces, iface{key, it})
					gotIface[key] = true
				}
			}
		}
	}

	seen := map[string]bool{} // a type declared in production files appears in the package AND its test variant
	var rows, ifrows, droprows []string
	types_, methods, exported, shims, clashes, generic, sigunexp, transit, internalfwd, multicross, collide := 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
	byIface := map[string]int{}
	for _, p := range pkgs {
		if p.TypesInfo == nil {
			continue
		}
		var tns []*types.TypeName
		for _, obj := range p.TypesInfo.Defs {
			if tn, ok := obj.(*types.TypeName); ok && !tn.IsAlias() {
				tns = append(tns, tn)
			}
		}
		sort.Slice(tns, func(i, j int) bool { return tns[i].Pos() < tns[j].Pos() })
		for _, tn := range tns {
			nt, ok := tn.Type().(*types.Named)
			if !ok {
				continue
			}
			if _, isStruct := nt.Underlying().(*types.Struct); !isStruct {
				continue
			}
			pos := p.Fset.Position(tn.Pos())
			key := fmt.Sprintf("%s:%d:%d", pos.Filename, pos.Line, pos.Column)
			if seen[key] {
				continue
			}
			seen[key] = true
			rel := pos.Filename
			if r, err := filepath.Rel(filepath.Join(os.Getenv("GOROOT"), "src"), pos.Filename); err == nil && !strings.HasPrefix(r, "..") {
				rel = filepath.ToSlash(r)
			}
			kind := "prod"
			if strings.HasSuffix(pos.Filename, "_test.go") {
				kind = "test"
			}
			scope := "pkg"
			if tn.Parent() != tn.Pkg().Scope() {
				scope = "local"
			}
			isGeneric := nt.TypeParams().Len() > 0

			valueSet := types.NewMethodSet(nt)
			ptrSet := types.NewMethodSet(types.NewPointer(nt))
			inValue := map[string]bool{}
			for i := 0; i < valueSet.Len(); i++ {
				inValue[valueSet.At(i).Obj().Name()] = true
			}
			crossing := map[string]bool{} // method names in *T's set reached through a crossing path
			var mine []string
			for i := 0; i < ptrSet.Len(); i++ {
				sel := ptrSet.At(i)
				if len(sel.Index()) < 2 {
					continue
				}
				fn, ok := sel.Obj().(*types.Func)
				if !ok {
					continue
				}
				recv := fn.Type().(*types.Signature).Recv()
				if recv == nil || types.IsInterface(recv.Type()) {
					continue
				}
				cr, path, hk, vd2 := walk(sel)
				if !cr {
					continue
				}
				crossing[fn.Name()] = true
				set := "*T"
				if inValue[fn.Name()] {
					set = "T"
				}
				_, recvPtr := deref(recv.Type())
				rk := "value"
				if recvPtr {
					rk = "pointer"
				}
				flags := []string{}
				if fn.Exported() {
					exported++
				} else {
					flags = append(flags, "unexported")
				}
				if recvPtr && vd2 {
					flags = append(flags, "shim")
					shims++
				}
				if obj := tn.Pkg().Scope().Lookup(fn.Name()); obj != nil {
					if _, isFunc := obj.(*types.Func); isFunc {
						flags = append(flags, "clash")
						clashes++
					}
				}
				sig := fn.Type().(*types.Signature)
				// C3: the signature names an unexported type of another package, so a forwarder in T's assembly cannot
				// spell it (CS0122/CS0050/CS0051) even though the method itself is exported.
				if fn.Exported() && sigNamesForeignUnexported(sig, tn.Pkg()) {
					flags = append(flags, "sigunexp")
					sigunexp++
				}
				// C4: reached THROUGH a foreign type's own forwarder (hops remain after the first crossing hop), and
				// that forwarder's return shape reads non-public to the generator's name heuristic (a tuple, a builtin,
				// an unnamed or unexported type), so it is emitted internal and a foreign harvest cannot see it.
				if _, _, _, _, fc := walkAt(sel); fc >= 0 && fc < len(sel.Index())-2 {
					flags = append(flags, "transit")
					transit++
					if !publicReturnShape(sig) {
						flags = append(flags, "internalfwd")
						internalfwd++
						// multicross: the path crosses packages at 2+ hops, so the foreign type's hiding forwarder is
						// itself a NEW cross-package forwarder the cut mints (ProtoC's Outer shape); otherwise it is
						// a PRE-EXISTING same-package forwarder (reflect <- internal/abi).
						if crossings(sel) >= 2 {
							flags = append(flags, "multicross")
							multicross++
						}
					}
				}
				if isGeneric {
					flags = append(flags, "genencl")
				}
				// G's uniqueness rule (i') at the Go level: how often the name occurs across T's WHOLE tree (methods
				// declared on any embedded type at any depth, interface-embed providers, fields at any depth). A row
				// whose name is not unique is refused by the cut and keeps master's MISS (k; "Go picks metadata").
				if treeOccurrences(nt, fn.Name()) != 1 {
					flags = append(flags, "collide")
					if fn.Exported() {
						collide++
					}
				}
				methods++
				mine = append(mine, fmt.Sprintf("%s\t%s\t%s\t%s\t%s\t%s.%s\t%s-recv\tset=%s\tpath=%s[%s]\t%s", tn.Pkg().Path(), kind, scope, tn.Name(), rel,
					fn.Pkg().Path(), fn.Name(), rk, set, path, hk, strings.Join(flags, ",")))
			}
			// DROP rows (question 3): an exported method of a crossing embed's own method set that Go does NOT promote into
			// *T. A harvest of the embed's emitted extension methods would see it anyway, so the generator must drop it
			// too. Reasons: a field of T (at a shallower depth) shadows it, a same-depth ambiguity removes it, or a
			// shallower method wins. A method T declares itself is excluded: the generator already skips those by name.
			var drops func(t types.Type, depth int, path string, crossed bool, visiting map[*types.Named]bool)
			drops = func(t types.Type, depth int, path string, crossed bool, visiting map[*types.Named]bool) {
				owner := named(t)
				base, _ := deref(t)
				st, ok := types.Unalias(base).Underlying().(*types.Struct)
				if !ok || owner == nil || visiting[owner] {
					return
				}
				visiting[owner] = true
				defer delete(visiting, owner)
				for i := 0; i < st.NumFields(); i++ {
					f := st.Field(i)
					if !f.Embedded() {
						continue
					}
					e := named(f.Type())
					if e == nil || types.IsInterface(e) {
						continue
					}
					c := crossed || owner.Obj().Pkg() != e.Obj().Pkg()
					p := strings.TrimPrefix(path+"."+f.Name(), ".")
					if c {
						es := types.NewMethodSet(types.NewPointer(e))
						for j := 0; j < es.Len(); j++ {
							m := es.At(j).Obj()
							if !m.Exported() || ptrSet.Lookup(m.Pkg(), m.Name()) != nil {
								continue
							}
							declared := false
							for k := 0; k < nt.NumMethods(); k++ {
								if nt.Method(k).Name() == m.Name() {
									declared = true
								}
							}
							if declared {
								continue
							}
							obj, _, _ := types.LookupFieldOrMethod(types.NewPointer(nt), true, m.Pkg(), m.Name())
							reason := "ambiguous"
							switch o := obj.(type) {
							case *types.Var:
								reason = "field-shadow(" + o.Name() + ")"
							case *types.Func:
								reason = "method-shadow"
							}
							key := tn.Name() + "\x00" + p + "\x00" + m.Name()
							if seen[key] {
								continue
							}
							seen[key] = true
							droprows = append(droprows, fmt.Sprintf("%s\t%s\t%s\t%s\tdepth=%d path=%s\t%s\t%s", tn.Pkg().Path(), kind, tn.Name(), rel, depth, p, m.Name(), reason))
						}
					}
					drops(f.Type(), depth+1, p, c, visiting)
				}
			}
			drops(nt, 1, "", false, map[*types.Named]bool{})
			if len(mine) == 0 {
				continue
			}
			types_++
			if isGeneric {
				generic++
			}
			rows = append(rows, mine...)
			// Interfaces whose satisfaction depends on a crossing method.
			for _, it := range ifaces {
				for _, form := range []struct {
					label string
					t     types.Type
				}{{"T", nt}, {"*T", types.NewPointer(nt)}} {
					if isGeneric || !types.Implements(form.t, it.t) {
						continue
					}
					var deps []string
					for i := 0; i < it.t.NumMethods(); i++ {
						if crossing[it.t.Method(i).Name()] {
							deps = append(deps, it.t.Method(i).Name())
						}
					}
					if len(deps) == 0 {
						continue
					}
					ifrows = append(ifrows, fmt.Sprintf("%s\t%s\t%s\t%s\t%s satisfies %s via %s", tn.Pkg().Path(), kind, tn.Name(), rel, form.label, it.name, strings.Join(deps, ",")))
					byIface[it.name]++
					break // the value form implies the pointer form; report the narrowest
				}
			}
		}
	}
	sort.Strings(rows)
	sort.Strings(ifrows)
	fmt.Println("## methods")
	for _, r := range rows {
		fmt.Println(r)
	}
	fmt.Println("## drops")
	sort.Strings(droprows)
	for _, r := range droprows {
		fmt.Println(r)
	}
	fmt.Println("## interfaces")
	for _, r := range ifrows {
		fmt.Println(r)
	}
	var ik []string
	for k, v := range byIface {
		ik = append(ik, fmt.Sprintf("%s=%d", k, v))
	}
	sort.Strings(ik)
	fmt.Printf("\npackages loaded %d (load errors %d); interfaces resolved %d; types %d (generic %d); methods %d (exported %d, shim %d, clash %d, sigunexp %d, transit %d, internalfwd %d, multicross %d, collide %d); drop rows %d; interface rows %d: %s\n",
		len(pkgs), loadErrors, len(ifaces), types_, generic, methods, exported, shims, clashes, sigunexp, transit, internalfwd, multicross, collide, len(droprows), len(ifrows), strings.Join(ik, " "))
}

// sigNamesForeignUnexported reports whether any parameter or result type mentions a named type that is unexported
// and declared outside pkg.
func sigNamesForeignUnexported(sig *types.Signature, pkg *types.Package) bool {
	seen := map[types.Type]bool{}
	var bad func(t types.Type) bool
	bad = func(t types.Type) bool {
		if t == nil || seen[t] {
			return false
		}
		seen[t] = true
		switch u := types.Unalias(t).(type) {
		case *types.Named:
			if o := u.Obj(); o.Pkg() != nil && o.Pkg() != pkg && !o.Exported() {
				return true
			}
			if ta := u.TypeArgs(); ta != nil {
				for i := 0; i < ta.Len(); i++ {
					if bad(ta.At(i)) {
						return true
					}
				}
			}
			return false
		case *types.Pointer:
			return bad(u.Elem())
		case *types.Slice:
			return bad(u.Elem())
		case *types.Array:
			return bad(u.Elem())
		case *types.Map:
			return bad(u.Key()) || bad(u.Elem())
		case *types.Chan:
			return bad(u.Elem())
		case *types.Signature:
			return tupleBad(u.Params(), bad) || tupleBad(u.Results(), bad)
		case *types.Struct:
			for i := 0; i < u.NumFields(); i++ {
				if bad(u.Field(i).Type()) {
					return true
				}
			}
		}
		return false
	}
	return tupleBad(sig.Params(), bad) || tupleBad(sig.Results(), bad)
}

func tupleBad(t *types.Tuple, bad func(types.Type) bool) bool {
	for i := 0; i < t.Len(); i++ {
		if bad(t.At(i).Type()) {
			return true
		}
	}
	return false
}

// publicReturnShape approximates the generator's return-type scope heuristic (GetScope(GetSimpleName(ReturnType))):
// void, or ONE result that is an exported named type (or a pointer to one), reads public; a tuple, a builtin
// (lowercase: @string, error, bool, nint...), or an unnamed type reads internal.
func publicReturnShape(sig *types.Signature) bool {
	r := sig.Results()
	if r.Len() == 0 {
		return true
	}
	if r.Len() > 1 {
		return false
	}
	t := types.Unalias(r.At(0).Type())
	if p, ok := t.(*types.Pointer); ok {
		t = types.Unalias(p.Elem())
	}
	n, ok := t.(*types.Named)
	return ok && n.Obj().Pkg() != nil && n.Obj().Exported()
}

// crossings counts the hops of the selection's embed path whose embedding and embedded types are declared in
// different packages.
func crossings(sel *types.Selection) int {
	n := 0
	cur := sel.Recv()
	for _, idx := range sel.Index()[:len(sel.Index())-1] {
		owner := named(cur)
		base, _ := deref(cur)
		st, ok := types.Unalias(base).Underlying().(*types.Struct)
		if !ok || owner == nil {
			return n
		}
		f := st.Field(idx)
		next := named(f.Type())
		if next == nil {
			return n
		}
		if owner.Obj().Pkg() != next.Obj().Pkg() {
			n++
		}
		cur = f.Type()
	}
	return n
}

// treeOccurrences counts how often name occurs across the struct t's whole embedding tree: the fields of every
// struct on the way (t's own included), the methods DECLARED on every embedded named type (each type once per path),
// and the methods of every embedded interface. t's own declared methods are not counted (a declared method is not a
// promotion; the generator skips those by name).
func treeOccurrences(t *types.Named, name string) int {
	n := 0
	var walk func(s types.Type, visiting map[*types.Named]bool)
	walk = func(s types.Type, visiting map[*types.Named]bool) {
		owner := named(s)
		base, _ := deref(s)
		st, ok := types.Unalias(base).Underlying().(*types.Struct)
		if !ok || owner == nil || visiting[owner] {
			return
		}
		visiting[owner] = true
		defer delete(visiting, owner)
		for i := 0; i < st.NumFields(); i++ {
			f := st.Field(i)
			if f.Name() == name {
				n++
			}
			if !f.Embedded() {
				continue
			}
			e := named(f.Type())
			if e == nil {
				continue
			}
			if it, ok := e.Underlying().(*types.Interface); ok {
				for j := 0; j < it.NumMethods(); j++ {
					if it.Method(j).Name() == name {
						n++
					}
				}
				continue
			}
			for j := 0; j < e.NumMethods(); j++ {
				if e.Method(j).Name() == name {
					n++
				}
			}
			walk(f.Type(), visiting)
		}
	}
	walk(t, map[*types.Named]bool{})
	return n
}
