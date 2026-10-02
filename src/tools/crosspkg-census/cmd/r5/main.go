// r5: the VALUE-method-set over-claim that go2cs-gen's forwarder shape causes (COORD's R5).
//
// A promoted POINTER-receiver method is emitted by the converter as `[GoRecv] static M(this ref E ...)` (the common
// form; a direct-ж box primary is the exception). The struct template forwards it as `static M(this ref T target ...)`
// with NO [GoRecv], and golib's GetGoMethodSetCandidates then admits it to T's VALUE method set. Go admits a
// pointer-receiver method to T's value set only when the embed path holds a POINTER hop. So every promoted
// pointer-receiver method whose path is all VALUE hops is a value-set over-claim, today for same-package paths
// (a named residual) and, under the cut, for cross-package ones too (what R5 fixes).
//
// Rows: (declaration position of T, method). Population per class, and per well-known interface the VALUE T then
// falsely satisfies in golib's view (every interface method is in Go's value set or in the over-claimed names), while
// Go says T does not implement it.
// Usage: r5 [-root dir] <patterns...>
package main

import (
	"flag"
	"fmt"
	"go/types"
	"os"
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

// path reports whether the selection's embed path crosses packages and whether it holds a pointer hop.
func path(sel *types.Selection) (crosses, hasPtr, ok bool) {
	cur := sel.Recv()
	if _, p := deref(cur); p {
		cur, _ = deref(cur) // the *T receiver itself is not an embed hop
	}
	for _, idx := range sel.Index()[:len(sel.Index())-1] {
		owner := named(cur)
		base, _ := deref(cur)
		st, isStruct := types.Unalias(base).Underlying().(*types.Struct)
		if !isStruct || owner == nil {
			return false, false, false
		}
		f := st.Field(idx)
		next := named(f.Type())
		if next == nil {
			return false, false, false
		}
		if owner.Obj().Pkg() != next.Obj().Pkg() {
			crosses = true
		}
		if _, p := deref(f.Type()); p {
			hasPtr = true
		}
		cur = f.Type()
	}
	return crosses, hasPtr, true
}

func main() {
	root := flag.String("root", "", "directory to load from")
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
	want := map[string][]string{
		"fmt": {"Stringer", "GoStringer", "Formatter"}, "encoding/json": {"Marshaler", "Unmarshaler"},
		"encoding": {"TextMarshaler", "TextUnmarshaler", "BinaryMarshaler", "BinaryUnmarshaler"},
		"io":       {"Reader", "Writer", "Closer", "ReaderFrom", "WriterTo", "StringWriter", "ByteReader"},
		"sort":     {"Interface"}, "sync": {"Locker"}, "flag": {"Value"},
	}
	type iface struct {
		name string
		t    *types.Interface
	}
	var ifaces []iface
	got := map[string]bool{}
	for _, p := range all {
		names, ok := want[p.PkgPath]
		if !ok || p.ID != p.PkgPath || p.Types == nil {
			continue
		}
		for _, n := range names {
			if k := p.PkgPath + "." + n; !got[k] {
				if obj := p.Types.Scope().Lookup(n); obj != nil {
					if it, ok := obj.Type().Underlying().(*types.Interface); ok {
						ifaces = append(ifaces, iface{k, it})
						got[k] = true
					}
				}
			}
		}
	}
	seen := map[string]bool{}
	var rows, ifrows []string
	counts := map[string]int{}
	types_ := map[string]map[string]bool{"cross": {}, "same": {}}
	for _, p := range pkgs {
		if p.TypesInfo == nil {
			continue
		}
		for id, obj := range p.TypesInfo.Defs {
			tn, ok := obj.(*types.TypeName)
			if !ok || tn.IsAlias() {
				continue
			}
			nt, ok := tn.Type().(*types.Named)
			if !ok || nt.TypeParams().Len() > 0 {
				continue
			}
			if _, ok := nt.Underlying().(*types.Struct); !ok {
				continue
			}
			pos := p.Fset.Position(id.Pos())
			key := fmt.Sprintf("%s:%d:%d", pos.Filename, pos.Line, pos.Column)
			if seen[key] {
				continue
			}
			seen[key] = true
			kind := "prod"
			if strings.HasSuffix(pos.Filename, "_test.go") {
				kind = "test"
			}
			vs := types.NewMethodSet(nt)
			inValue := map[string]bool{}
			for i := 0; i < vs.Len(); i++ {
				inValue[vs.At(i).Obj().Name()] = true
			}
			ps := types.NewMethodSet(types.NewPointer(nt))
			over := map[string]bool{}
			for i := 0; i < ps.Len(); i++ {
				sel := ps.At(i)
				fn, ok := sel.Obj().(*types.Func)
				if !ok || len(sel.Index()) < 2 || inValue[fn.Name()] {
					continue
				}
				recv := fn.Type().(*types.Signature).Recv()
				if recv == nil || types.IsInterface(recv.Type()) {
					continue
				}
				if _, rp := deref(recv.Type()); !rp {
					continue
				}
				crosses, hasPtr, ok := path(sel)
				if !ok || hasPtr {
					continue // a pointer hop puts it in Go's value set: not this class
				}
				class := "same"
				if crosses {
					class = "cross"
					if !fn.Exported() {
						continue // R1 harvests exported names only
					}
				}
				over[fn.Name()] = true
				counts[class]++
				types_[class][key] = true
				rows = append(rows, fmt.Sprintf("%s\t%s\t%s\t%s\t%s.%s\tdepth=%d", class, tn.Pkg().Path(), kind, tn.Name(), fn.Pkg().Path(), fn.Name(), len(sel.Index())-1))
			}
			// B1's class: a POINTER-receiver method reached through a POINTER hop sits in Go's VALUE set, but the
			// generator's forwarder is `this ref T` (by-ref): golib's binder skips it and reflect method values throw.
			// same-ptrhop = today's same-package rows (a crash residual unless reachable); cross-ptrhop = what the cut
			// emits BY VALUE instead.
			for i := 0; i < vs.Len(); i++ {
				sel := vs.At(i)
				fn, ok := sel.Obj().(*types.Func)
				if !ok || len(sel.Index()) < 2 {
					continue
				}
				recv := fn.Type().(*types.Signature).Recv()
				if recv == nil || types.IsInterface(recv.Type()) {
					continue
				}
				if _, rp := deref(recv.Type()); !rp {
					continue
				}
				crosses, hasPtr, ok := path(sel)
				if !ok || !hasPtr {
					continue
				}
				class := "same-ptrhop"
				if crosses {
					class = "cross-ptrhop"
					if !fn.Exported() {
						continue
					}
				}
				counts[class]++
				if types_[class] == nil {
					types_[class] = map[string]bool{}
				}
				types_[class][key] = true
				rows = append(rows, fmt.Sprintf("%s\t%s\t%s\t%s\t%s.%s\tdepth=%d", class, tn.Pkg().Path(), kind, tn.Name(), fn.Pkg().Path(), fn.Name(), len(sel.Index())-1))
			}
			if len(over) == 0 {
				continue
			}
			for _, it := range ifaces {
				if types.Implements(nt, it.t) {
					continue
				}
				covered, uses := true, false
				for i := 0; i < it.t.NumMethods(); i++ {
					n := it.t.Method(i).Name()
					if over[n] {
						uses = true
					} else if !inValue[n] {
						covered = false
					}
				}
				if covered && uses {
					ifrows = append(ifrows, fmt.Sprintf("%s\t%s\t%s\tVALUE falsely satisfies %s", tn.Pkg().Path(), kind, tn.Name(), it.name))
				}
			}
		}
	}
	sort.Strings(rows)
	sort.Strings(ifrows)
	fmt.Println("## rows (class pkg kind type method depth)")
	for _, r := range rows {
		fmt.Println(r)
	}
	fmt.Println("## false interface satisfactions (value T, golib view)")
	for _, r := range ifrows {
		fmt.Println(r)
	}
	fmt.Printf("\npackages loaded %d (load errors %d); interfaces %d; cross rows %d in %d types; same rows %d in %d types; false-iface rows %d; same-ptrhop rows %d in %d types; cross-ptrhop rows %d in %d types\n",
		len(pkgs), loadErrors, len(ifaces), counts["cross"], len(types_["cross"]), counts["same"], len(types_["same"]), len(ifrows),
		counts["same-ptrhop"], len(types_["same-ptrhop"]), counts["cross-ptrhop"], len(types_["cross-ptrhop"]))
}
