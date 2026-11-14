// creation.go: N3's creation census (the Fable review's owed reading, 2026-11-13). For every func-value call site
// that can park (a dynamic call, `defer` included, `go` excluded, with a VTA callee that can park), it finds where each
// parking callee is CREATED as a value -- a MakeClosure for a func literal or a bound method value, an operand use
// for a named function or a method expression -- and asks whether that creation site sits in a TWIN body, which is
// where design D3 makes its pairs.
//
// A twin body is modelled as the generator decides it: a named function is twinned when it can park through its own
// body (parks[], under the flags' reach). A closure is classified by its outermost named parent: D3 pairs a literal
// only in a twinned function, and a literal inside a literal sits in its outer function's twin.
package main

import (
	"fmt"
	"go/types"
	"os"
	"sort"
	"strings"

	"golang.org/x/tools/go/callgraph"
	"golang.org/x/tools/go/ssa"
)

func outermost(fn *ssa.Function) *ssa.Function {
	for fn.Parent() != nil {
		fn = fn.Parent()
	}

	return fn
}

// creationKind classifies the function a value is created in.
func creationKind(g *ssa.Function, parks map[*ssa.Function]bool) string {
	outer := outermost(g)
	nested := outer != g

	switch {
	case outer.Name() == "init" || strings.HasPrefix(outer.Name(), "init#"):
		return "package-init"
	case parks[outer] && !nested:
		return "twin"
	case parks[outer]:
		return "twin(closure)"
	case nested:
		return "plain(closure)"
	default:
		return "plain"
	}
}

func creationCensus(prog *ssa.Program, all map[*ssa.Function]bool, graph *callgraph.Graph, parks map[*ssa.Function]bool, prefix, out string) {
	makers := map[*ssa.Function]map[*ssa.Function]bool{}
	add := func(created, in *ssa.Function) {
		if makers[created] == nil {
			makers[created] = map[*ssa.Function]bool{}
		}

		makers[created][in] = true
	}

	// A bound method wrapper or a method-expression thunk stands for its method: DeleteSyntheticNodes routes the
	// call graph's edges to the method itself.
	underlying := func(f *ssa.Function) *ssa.Function {
		if f.Synthetic == "" {
			return f
		}

		if obj, ok := f.Object().(*types.Func); ok {
			if m := prog.FuncValue(obj); m != nil {
				return m
			}
		}

		return f
	}

	for fn := range all {
		for _, block := range fn.Blocks {
			for _, instr := range block.Instrs {
				if mc, ok := instr.(*ssa.MakeClosure); ok {
					if target, ok := mc.Fn.(*ssa.Function); ok {
						add(target, fn)
						add(underlying(target), fn)
					}

					continue
				}

				call, isCall := instr.(ssa.CallInstruction)
				ops := instr.Operands(nil)

				for i, op := range ops {
					if op == nil {
						continue
					}

					// The callee operand of a call is a call, not a value: skip it unless it is an invoke's receiver.
					if isCall && i == 0 && !call.Common().IsInvoke() {
						continue
					}

					if f, ok := (*op).(*ssa.Function); ok {
						add(f, fn)
						add(underlying(f), fn)
					}
				}
			}
		}
	}

	type siteRow struct {
		pos, caller, callerKind, verdict string
		creators                         []string
	}

	var rows []siteRow
	verdicts := map[string]int{}
	byCaller := map[string]map[string]int{}

	for fn := range all {
		if fn.Pkg == nil || !strings.HasPrefix(fn.Pkg.Pkg.Path(), prefix) || inRuntimeFamily(fn) {
			continue
		}

		node := graph.Nodes[fn]
		if node == nil {
			continue
		}

		edgesBySite := map[ssa.CallInstruction][]*callgraph.Edge{}
		for _, edge := range node.Out {
			edgesBySite[edge.Site] = append(edgesBySite[edge.Site], edge)
		}

		for site, edges := range edgesBySite {
			common := site.Common()

			if isGo(site) || common.IsInvoke() || common.StaticCallee() != nil || isBuiltin(common) {
				continue
			}

			var parking []*ssa.Function
			for _, edge := range edges {
				if parks[edge.Callee.Func] {
					parking = append(parking, edge.Callee.Func)
				}
			}

			if len(parking) == 0 {
				continue
			}

			kinds := map[string]bool{}
			var creators []string

			for _, callee := range parking {
				for g := range makers[callee] {
					kind := creationKind(g, parks)
					kinds[kind] = true
					creators = append(creators, kind+":"+g.String())
				}
			}

			twinCount, plainCount := 0, 0
			for kind := range kinds {
				if strings.HasPrefix(kind, "twin") {
					twinCount++
				} else {
					plainCount++
				}
			}

			verdict := "covered"
			switch {
			case len(kinds) == 0:
				verdict = "no-creation-found"
			case twinCount == 0:
				verdict = "uncovered"
			case plainCount > 0:
				verdict = "partial"
			}

			callerKind := creationKind(fn, parks)
			verdicts[verdict]++

			if byCaller[callerKind] == nil {
				byCaller[callerKind] = map[string]int{}
			}

			byCaller[callerKind][verdict]++
			sort.Strings(creators)
			rows = append(rows, siteRow{prog.Fset.Position(site.Pos()).String(), fn.String(), callerKind, verdict, creators})
		}
	}

	total := len(rows)
	fmt.Printf("CREATION CENSUS prefix=%s: %d can-park func-value call sites\n", prefix, total)

	for _, v := range []string{"covered", "partial", "uncovered", "no-creation-found"} {
		fmt.Printf("  %-18s %d\n", v, verdicts[v])
	}

	callers := make([]string, 0, len(byCaller))
	for k := range byCaller {
		callers = append(callers, k)
	}

	sort.Strings(callers)

	for _, k := range callers {
		fmt.Printf("  caller %-15s %v\n", k, byCaller[k])
	}

	if out != "" {
		sort.Slice(rows, func(i, j int) bool { return rows[i].pos < rows[j].pos })
		f, err := os.Create(out)
		if err != nil {
			fmt.Fprintln(os.Stderr, "creation-out:", err)
			return
		}

		defer f.Close()

		for _, r := range rows {
			fmt.Fprintf(f, "%s\t%s\t%s\t%s\t%s\n", r.verdict, r.callerKind, r.caller, r.pos, strings.Join(r.creators, " | "))
		}
	}
}
