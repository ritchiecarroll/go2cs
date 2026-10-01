// sim: models go2cs-gen's promoted-method emission (the forwarder NAMES a struct gets) TODAY and under the proposed
// CUT, and compares both against Go's own method set (go/types). It answers two design questions with populations:
//   - item 7: which same-package forwarders the cut REMOVES (a crossing provider newly makes a name ambiguous);
//   - item 5: where the cut's view (a metadata embed contributes its WHOLE emitted surface -- declared methods AND its
//     own generated forwarders -- flattened to the embed's depth) disagrees with Go's depth rule.
//
// Model of the generator (StructTypeTemplate.PromotedStructReceivers), per enclosing named struct T in package P:
//   - providers are walked from T's embeds. A SOURCE embed (declared in P, same production/test kind as T: one
//     compilation) contributes its DECLARED methods at its depth (pointer-receiver ones only through a pointer embed or
//     a direct value embed: the box shim) and recurses at depth+1.
//   - a METADATA embed contributes, at its own depth and without recursion, every extension method with its receiver:
//     its declared methods plus the forwarders ITS OWN generator run emitted (computed recursively with the same model).
//     TODAY only a same-Go-package metadata embed contributes; under the CUT a cross-package one does too (exported
//     names only), and embedded INTERFACES contribute their methods (the converter marker the cut adds).
//   - a name is emitted iff it occurs exactly once at its minimum depth, and T does not declare it. Field shadowing is
//     applied under the CUT (the new rule), never TODAY.
//
// Go's truth is types.NewMethodSet(*T): promoted (index path >= 2) names, exported or declared in P.
package main

import (
	"flag"
	"fmt"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

type mode int

const (
	today mode = iota
	cut
)

type occ struct {
	depth int
	via   string
}

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

var isTestDecl = map[*types.TypeName]bool{}

// declared returns the names of the methods declared on n: all, and the pointer-receiver subset.
func declared(n *types.Named) (all, ptr map[string]bool) {
	all, ptr = map[string]bool{}, map[string]bool{}
	for i := 0; i < n.NumMethods(); i++ {
		m := n.Method(i)
		all[m.Name()] = true
		if _, p := deref(m.Type().(*types.Signature).Recv().Type()); p {
			ptr[m.Name()] = true
		}
	}
	return
}

type key struct {
	n *types.Named
	m mode
}

var memo = map[key]map[string]bool{}

// crossMinted[k] holds the names minted through a cross-package path (the cut's ᴛxpkg forwarders, semantic scope).
var crossMinted = map[key]map[string]bool{}

// emitted models the forwarder names the generator mints for struct n in its own compilation.
func emitted(n *types.Named, m mode) map[string]bool {
	k := key{n, m}
	if r, ok := memo[k]; ok {
		return r
	}
	memo[k] = map[string]bool{} // cycle guard
	occs := map[string][]occ{}
	pkg := n.Obj().Pkg()
	testKind := isTestDecl[n.Obj()]
	var walk func(s *types.Named, depth int, visiting map[*types.Named]bool)
	walk = func(s *types.Named, depth int, visiting map[*types.Named]bool) {
		st, ok := s.Underlying().(*types.Struct)
		if !ok || visiting[s] {
			return
		}
		visiting[s] = true
		defer delete(visiting, s)
		for i := 0; i < st.NumFields(); i++ {
			f := st.Field(i)
			if !f.Embedded() {
				continue
			}
			e := named(f.Type())
			if e == nil {
				continue
			}
			_, isPtr := deref(f.Type())
			if it, ok := e.Underlying().(*types.Interface); ok {
				if m == cut {
					for j := 0; j < it.NumMethods(); j++ {
						occs[it.Method(j).Name()] = append(occs[it.Method(j).Name()], occ{depth, "iface " + f.Name()})
					}
				}
				continue
			}
			if _, ok := e.Underlying().(*types.Struct); !ok {
				continue
			}
			samePkg := e.Obj().Pkg() == pkg
			source := samePkg && isTestDecl[e.Obj()] == testKind
			all, _ := declared(e)
			_ = isPtr
			if source {
				// A pointer-receiver method is assumed in the common `[GoRecv] this ref T` form, which promotes at
				// ANY depth (only a box primary needs the depth-1 shim).
				for name := range all {
					occs[name] = append(occs[name], occ{depth, "src " + f.Name()})
				}
				walk(e, depth+1, visiting)
				continue
			}
			if !samePkg && m == today {
				continue // the metadata gate: a foreign Go package contributes nothing today
			}
			surface := map[string]bool{}
			for name := range all {
				surface[name] = true
			}
			for name := range emitted(e, m) {
				// C4: a foreign embed's SAME-package forwarder takes the name heuristic's scope, so one with a
				// non-public return shape (builtin, tuple, unnamed) is internal and invisible here. Its cross-package
				// (ᴛxpkg) forwarders take the semantic scope and stay visible.
				if !samePkg && m == cut && !crossMinted[key{e, m}][name] && !publicReturnShapeOf(e, name) {
					continue
				}
				surface[name] = true
			}
			for name := range surface {
				if !samePkg && !token.IsExported(name) {
					continue // the cut's exported-only filter (item 4)
				}
				occs[name] = append(occs[name], occ{depth, map[bool]string{true: "meta ", false: "xmeta "}[samePkg] + f.Name()})
			}
		}
	}
	walk(n, 1, map[*types.Named]bool{})
	own, _ := declared(n)
	out := map[string]bool{}
	crossSeen := map[string]bool{}
	for name, all := range occs {
		if own[name] {
			continue
		}
		// MASTER's counter: same-package occurrences only. Cross-package names neither mint nor COUNT here (G's
		// uniqueness rule (i')), and interface providers were never visible to it.
		var os []occ
		for _, o := range all {
			switch {
			case strings.HasPrefix(o.via, "xmeta "):
				crossSeen[name] = true
			case strings.HasPrefix(o.via, "iface "):
			default:
				os = append(os, o)
			}
		}
		if len(os) == 0 {
			continue
		}
		min := 1 << 30
		for _, o := range os {
			if o.depth < min {
				min = o.depth
			}
		}
		c := 0
		for _, o := range os {
			if o.depth == min {
				c++
			}
		}
		if c != 1 {
			continue
		}
		out[name] = true
	}
	if m == cut {
		// (i'): a name reached through a cross-package path mints ONLY when it is unique across the WHOLE tree
		// (methods at any depth, interface providers, fields at any depth) -- R3 and R4 fold into this.
		crossMinted[k] = map[string]bool{}
		for name := range crossSeen {
			if own[name] || out[name] || treeOccurrences(n, name) != 1 {
				continue
			}
			out[name] = true
			crossMinted[k][name] = true
		}
	}
	memo[k] = out
	return out
}

func main() {
	root := flag.String("root", "", "directory to load from (default: current)")
	builtinFile := flag.String("builtin", "", "file of go.builtin member names (the global using static), one per line")
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
	for _, p := range all {
		if p.TypesInfo == nil {
			continue
		}
		for id, obj := range p.TypesInfo.Defs {
			if tn, ok := obj.(*types.TypeName); ok {
				if strings.HasSuffix(p.Fset.Position(id.Pos()).Filename, "_test.go") {
					isTestDecl[tn] = true
				}
			}
		}
	}

	seen := map[string]bool{}
	// Name clashes for a minted forwarder: the ENCLOSING package's own functions (same static class) and every
	// function of a DOT-imported package (`using static`), which a same-named class member shadows for bare calls.
	// Unioned per Go package path across variants (conservative).
	ownFuncs, dotFuncs := map[string]map[string]bool{}, map[string]map[string]string{}
	for _, p := range all {
		if p.Types == nil {
			continue
		}
		if ownFuncs[p.PkgPath] == nil {
			ownFuncs[p.PkgPath], dotFuncs[p.PkgPath] = map[string]bool{}, map[string]string{}
		}
		// COORD's R2 set: EVERY member name of the enclosing package class (types, vars, consts, funcs), not functions
		// only; a type/var/const clash is CS0102. The union over variants covers the internal-test bridge class, which
		// `using static`s the production class (export_test.cs:6).
		for _, nm := range p.Types.Scope().Names() {
			ownFuncs[p.PkgPath][nm] = true
		}
		for _, f := range p.Syntax {
			for _, imp := range f.Imports {
				if imp.Name == nil || imp.Name.Name != "." {
					continue
				}
				path := strings.Trim(imp.Path.Value, `"`)
				if ip := p.Imports[path]; ip != nil && ip.Types != nil {
					for _, nm := range ip.Types.Scope().Names() {
						if token.IsExported(nm) {
							dotFuncs[p.PkgPath][nm] = path
						}
					}
				}
			}
		}
	}
	clashes := 0
	builtinNames := map[string]bool{}
	if *builtinFile != "" {
		if b, err := os.ReadFile(*builtinFile); err == nil {
			for _, ln := range strings.Fields(string(b)) {
				if ln = strings.TrimSpace(ln); ln != "" {
					builtinNames[ln] = true
				}
			}
		}
	}
	var removed, added, overclaim, underclaim []string
	structs := 0
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
			k := fmt.Sprintf("%s:%d", pos.Filename, pos.Line)
			if seen[k] {
				continue
			}
			seen[k] = true
			structs++
			kind := "prod"
			if isTestDecl[tn] {
				kind = "test"
			}
			label := fmt.Sprintf("%s\t%s\t%s\t%s:%d", tn.Pkg().Path(), kind, tn.Name(), filepath.Base(pos.Filename), pos.Line)
			t0, t1 := emitted(nt, today), emitted(nt, cut)
			truth := map[string]bool{}
			ms := types.NewMethodSet(types.NewPointer(nt))
			for i := 0; i < ms.Len(); i++ {
				sel := ms.At(i)
				fn, ok := sel.Obj().(*types.Func)
				if !ok || len(sel.Index()) < 2 || types.IsInterface(fn.Type().(*types.Signature).Recv().Type()) {
					continue
				}
				if fn.Exported() || fn.Pkg() == tn.Pkg() {
					truth[fn.Name()] = true
				}
			}
			for name := range t0 {
				if !t1[name] {
					tag := "Go-correct"
					if truth[name] {
						tag = "REGRESSION (Go promotes it)"
					}
					removed = append(removed, fmt.Sprintf("%s\t%s\t%s", label, name, tag))
				}
			}
			for name := range t1 {
				if !t0[name] {
					flag := ""
					if ownFuncs[tn.Pkg().Path()][name] {
						flag += "\townclash"
					}
					if from, ok := dotFuncs[tn.Pkg().Path()][name]; ok {
						flag += "\tdotclash(" + from + ")"
					}
					if builtinNames[name] {
						flag += "\tbuiltinclash"
					}
					// An external test package X_test `using static`s X's internal-test class (io_test.cs:16); the
					// union of X's variant scopes stands in for it (conservative: it also holds X's production names).
					if base, isExt := strings.CutSuffix(tn.Pkg().Path(), "_test"); isExt && ownFuncs[base][name] {
						flag += "\tbridgeclash(" + base + ")"
					}
					if flag != "" {
						clashes++
					}
					added = append(added, fmt.Sprintf("%s\t%s%s", label, name, flag))
				}
				if !truth[name] {
					overclaim = append(overclaim, fmt.Sprintf("%s\t%s", label, name))
				}
			}
			for name := range truth {
				if !t1[name] {
					underclaim = append(underclaim, fmt.Sprintf("%s\t%s", label, name))
				}
			}
		}
	}
	for _, sec := range []struct {
		title string
		rows  []string
	}{{"removed (today emits, cut does not) -- item 7", removed}, {"added by the cut", added},
		{"cut OVER-claims (emitted, Go does not promote) -- item 5 and residuals", overclaim},
		{"cut UNDER-claims (Go promotes, not emitted) -- residuals", underclaim}} {
		sort.Strings(sec.rows)
		fmt.Printf("## %s: %d\n", sec.title, len(sec.rows))
		for _, r := range sec.rows {
			fmt.Println(r)
		}
	}
	fmt.Printf("\npackages loaded %d (load errors %d); structs %d; removed %d; added %d (clash %d); overclaim %d; underclaim %d\n",
		len(pkgs), loadErrors, structs, len(removed), len(added), clashes, len(overclaim), len(underclaim))
}

// publicReturnShapeOf approximates the generator's name-heuristic scope for a SAME-package forwarder of e's promoted
// method name: void, or one result that is an exported named type (or a pointer to one), reads public.
func publicReturnShapeOf(e *types.Named, name string) bool {
	obj, _, _ := types.LookupFieldOrMethod(types.NewPointer(e), true, e.Obj().Pkg(), name)
	fn, ok := obj.(*types.Func)
	if !ok {
		return true
	}
	r := fn.Type().(*types.Signature).Results()
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
	nt, ok := t.(*types.Named)
	return ok && nt.Obj().Pkg() != nil && nt.Obj().Exported()
}

// treeOccurrences counts how often name occurs across the struct t's whole embedding tree: the fields of every struct
// on the way (t's own included), the methods DECLARED on every embedded named type, and the methods of every embedded
// interface. t's own declared methods are not counted. (The same predicate as cmd/census's `collide`.)
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
