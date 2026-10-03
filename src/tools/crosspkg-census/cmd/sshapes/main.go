// sshapes: the populations of the (i') residual's three shapes (the TRAIN M cut's gate 2), measured at the Go level.
//
// For every named struct T and every method name reached through T's embedding tree, every PROVIDER is enumerated: a
// method declared on an embedded type reached at embed depth d (depth = path length), or a field at depth d. Each
// method provider is "same" when every hop of its path stays in T's package (master's generator sees it) and "cross"
// otherwise (master's generator never does).
//
// MASTER emits a forwarder for a name iff its SAME-package METHOD providers are unique at their own minimum depth and T
// does not declare it (the fields-blind counter). The cut keeps master's answer whenever the name collides under (i').
//   - S1: Go finds the name AMBIGUOUS among METHOD providers at the overall minimum depth 1, at least one cross; master
//     emits it, so C# over-claims.
//   - S4: the same with minimum depth >= 2.
//   - S3: Go's UNIQUE winner at the minimum depth is a CROSS method, while master forwards a SAME-package provider
//     that sits deeper. C# dispatches to the wrong method: a silent wrong call.
//
// Names whose minimum-depth provider set holds a FIELD are excluded (the field-shadow residual, counted elsewhere).
//
// Usage: sshapes [-root dir] <patterns...>
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

type provider struct {
	depth   int
	isField bool
	cross   bool
	where   string
}

func deref(t types.Type) types.Type {
	if p, ok := types.Unalias(t).(*types.Pointer); ok {
		return p.Elem()
	}
	return t
}

func named(t types.Type) *types.Named {
	n, _ := types.Unalias(deref(t)).(*types.Named)
	return n
}

// providers enumerates every method/field provider of every name in t's embedding tree.
func providers(t *types.Named) map[string][]provider {
	out := map[string][]provider{}
	var walk func(s *types.Named, depth int, cross bool, path string, visiting map[*types.Named]bool)
	walk = func(s *types.Named, depth int, cross bool, path string, visiting map[*types.Named]bool) {
		st, ok := s.Underlying().(*types.Struct)
		if !ok || visiting[s] {
			return
		}
		visiting[s] = true
		defer delete(visiting, s)
		for i := 0; i < st.NumFields(); i++ {
			f := st.Field(i)
			if depth > 0 || !f.Embedded() {
				// a field of an embedded struct (depth >= 1), or T's own named field (depth 0)
				out[f.Name()] = append(out[f.Name()], provider{depth, true, cross, path + "." + f.Name()})
			}
			if !f.Embedded() {
				continue
			}
			e := named(f.Type())
			if e == nil {
				continue
			}
			c := cross || e.Obj().Pkg() != t.Obj().Pkg()
			p := strings.TrimPrefix(path+"."+f.Name(), ".")
			if depth > 0 {
				// the embedded field's own name is a field at this depth too (counted above for depth > 0)
			} else {
				out[f.Name()] = append(out[f.Name()], provider{0, true, false, p})
			}
			if it, ok := e.Underlying().(*types.Interface); ok {
				for j := 0; j < it.NumMethods(); j++ {
					m := it.Method(j)
					out[m.Name()] = append(out[m.Name()], provider{depth + 1, false, c, p + "(iface)"})
				}
				continue
			}
			for j := 0; j < e.NumMethods(); j++ {
				m := e.Method(j)
				out[m.Name()] = append(out[m.Name()], provider{depth + 1, false, c, p})
			}
			walk(e, depth+1, c, p, visiting)
		}
	}
	walk(t, 0, false, "", map[*types.Named]bool{})
	return out
}

func main() {
	root := flag.String("root", "", "directory to load from")
	flag.Parse()
	cfg := &packages.Config{Mode: packages.NeedName | packages.NeedFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedTypesInfo, Tests: true, Dir: *root}
	pkgs, err := packages.Load(cfg, flag.Args()...)
	if err != nil {
		fmt.Println("load:", err)
		os.Exit(2)
	}
	loadErrors := 0
	packages.Visit(pkgs, nil, func(p *packages.Package) { loadErrors += len(p.Errors) })
	seen := map[string]bool{}
	var rows []string
	counts := map[string]int{}
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
			key := fmt.Sprintf("%s:%d", pos.Filename, pos.Line)
			if seen[key] {
				continue
			}
			seen[key] = true
			structs++
			own := map[string]bool{}
			for i := 0; i < nt.NumMethods(); i++ {
				own[nt.Method(i).Name()] = true
			}
			for name, ps := range providers(nt) {
				if own[name] {
					continue
				}
				min := 1 << 30
				for _, pr := range ps {
					if pr.depth < min {
						min = pr.depth
					}
				}
				var atMin []provider
				fieldAtMin := false
				for _, pr := range ps {
					if pr.depth == min {
						atMin = append(atMin, pr)
						if pr.isField {
							fieldAtMin = true
						}
					}
				}
				if fieldAtMin || min == 0 {
					continue
				}
				// master: same-package METHOD providers, unique at their own minimum depth
				sameMin := 1 << 30
				for _, pr := range ps {
					if !pr.isField && !pr.cross && pr.depth < sameMin {
						sameMin = pr.depth
					}
				}
				sameAt := 0
				for _, pr := range ps {
					if !pr.isField && !pr.cross && pr.depth == sameMin {
						sameAt++
					}
				}
				masterEmits := sameMin < 1<<30 && sameAt == 1
				if !masterEmits {
					continue
				}
				crossAtMin := 0
				for _, pr := range atMin {
					if pr.cross {
						crossAtMin++
					}
				}
				class := ""
				switch {
				case len(atMin) >= 2 && crossAtMin >= 1 && min == 1:
					class = "S1"
				case len(atMin) >= 2 && crossAtMin >= 1 && min >= 2:
					class = "S4"
				case len(atMin) == 1 && crossAtMin == 1 && sameMin > min:
					class = "S3"
				}
				if class == "" {
					continue
				}
				counts[class]++
				var where []string
				for _, pr := range ps {
					if !pr.isField {
						where = append(where, fmt.Sprintf("%s@%d%s", pr.where, pr.depth, map[bool]string{true: "x", false: ""}[pr.cross]))
					}
				}
				sort.Strings(where)
				rows = append(rows, fmt.Sprintf("%s\t%s\t%s\t%s:%d\t%s\t%s", class, tn.Pkg().Path(), tn.Name(), pos.Filename[strings.LastIndexAny(pos.Filename, `/\`)+1:], pos.Line, name, strings.Join(where, " ")))
			}
		}
	}
	sort.Strings(rows)
	for _, r := range rows {
		fmt.Println(r)
	}
	fmt.Printf("\npackages loaded %d (load errors %d); structs %d; S1 %d; S3 %d; S4 %d\n", len(pkgs), loadErrors, structs, counts["S1"], counts["S3"], counts["S4"])
}
