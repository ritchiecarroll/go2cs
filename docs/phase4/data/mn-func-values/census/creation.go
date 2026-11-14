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

// createsParkingValue reports whether fn's body creates, as a value, a function that really parks (in sitePark): a
// MakeClosure of one, or one used as a non-callee operand. Fix 1 gives such a factory a synchronous twin.
func createsParkingValue(fn *ssa.Function, sitePark map[*ssa.Function]bool) bool {
	for _, block := range fn.Blocks {
		for _, instr := range block.Instrs {
			if mc, ok := instr.(*ssa.MakeClosure); ok {
				if target, ok := mc.Fn.(*ssa.Function); ok && sitePark[target] {
					return true
				}

				continue
			}

			call, isCall := instr.(ssa.CallInstruction)
			for i, op := range instr.Operands(nil) {
				if op == nil || isCall && i == 0 && !call.Common().IsInvoke() {
					continue
				}

				if f, ok := (*op).(*ssa.Function); ok && sitePark[f] {
					return true
				}
			}
		}
	}

	return false
}

// computeHoled returns the functions some plain root reaches through non-go calls (VTA edges: an upper bound), and
// each one's first-reached predecessor. A plain body calls the plain bodies of its callees, so a twinned function
// reached that way runs plain on that path. The roots are the untwinned functions and, with withInit, the package
// initializers, which run plain whatever the twin rule says (section 8).
func computeHoled(all map[*ssa.Function]bool, graph *callgraph.Graph, parks map[*ssa.Function]bool, withInit bool) (map[*ssa.Function]bool, map[*ssa.Function]*ssa.Function) {
	holed := map[*ssa.Function]bool{}
	from := map[*ssa.Function]*ssa.Function{}
	var queue []*ssa.Function

	for fn := range all {
		// A -cut function is no root either: the cut asserts its VTA reach is spurious, and as a plain root it would
		// reach the same spurious paths (TypeScript's 7 holes before this rule all came from cut 1).
		isInit := creationKind(fn, parks) == "package-init"
		if inRuntimeFamily(fn) || isInit && !withInit || cutSet[outermost(fn).String()] {
			continue
		}

		if !parks[outermost(fn)] || isInit {
			queue = append(queue, fn)
		}
	}

	visited := map[*ssa.Function]bool{}
	for len(queue) > 0 {
		fn := queue[0]
		queue = queue[1:]
		if visited[fn] {
			continue
		}

		visited[fn] = true
		node := graph.Nodes[fn]
		if node == nil || cutSet[outermost(fn).String()] {
			continue
		}

		for _, edge := range node.Out {
			callee := edge.Callee.Func
			if isGo(edge.Site) || inRuntimeFamily(callee) {
				continue
			}

			if !holed[callee] {
				from[callee] = fn
			}

			holed[callee] = true
			if !visited[callee] {
				queue = append(queue, callee)
			}
		}
	}

	return holed, from
}

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
	// A package initializer: a function named init (or the SSA's init#N) with NO receiver. A METHOD named init
	// ((*internal/poll.pollDesc).init) is an ordinary method (fixed 2026-11-14; the first readings misread it).
	case (outer.Name() == "init" || strings.HasPrefix(outer.Name(), "init#")) && outer.Signature.Recv() == nil:
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

// entry reports how a function's body is entered under the twins: "twin" when it is twinned and every caller that
// reaches it by a non-go call is twinned too, "twin-holed" when it is twinned but some caller is plain (that caller
// runs the plain body, and a pair at this site is not awaited: a SILENT hole), "plain" when it has no twin.
func entry(fn *ssa.Function, graph *callgraph.Graph, parks map[*ssa.Function]bool) string {
	outer := outermost(fn)
	if !parks[outer] {
		return "plain"
	}

	if node := graph.Nodes[outer]; node != nil {
		for _, edge := range node.In {
			if isGo(edge.Site) || inRuntimeFamily(edge.Caller.Func) {
				continue
			}

			if !parks[outermost(edge.Caller.Func)] {
				return "twin-holed"
			}
		}
	}

	return "twin"
}

func creationCensus(prog *ssa.Program, all map[*ssa.Function]bool, graph *callgraph.Graph, sitePark, parks map[*ssa.Function]bool, prefix, out string) {
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
	entries := map[string]int{}
	awaited := 0
	deep := map[string]int{}
	awaitedDeep, awaitedStrict := 0, 0
	deepNoInit := map[string]int{}
	awaitedNoInit := 0

	holed, holedFrom := computeHoled(all, graph, parks, true)
	holedNoInit, holedFromNoInit := computeHoled(all, graph, parks, false)

	// holedPathIn names the chain from a plain root down to fn (first-reached predecessors in from).
	holedPathIn := func(from map[*ssa.Function]*ssa.Function, fn *ssa.Function) string {
		var steps []string
		seen := map[*ssa.Function]bool{}
		for cur := fn; cur != nil && !seen[cur]; cur = from[cur] {
			seen[cur] = true
			tag := "twin"
			switch {
			case creationKind(cur, parks) == "package-init":
				tag = "PLAIN-INIT"
			case !parks[outermost(cur)]:
				tag = "PLAIN"
			}

			steps = append([]string{tag + ":" + cur.String()}, steps...)
			if tag != "twin" {
				break
			}
		}

		return strings.Join(steps, " -> ")
	}

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
				if sitePark[edge.Callee.Func] {
					parking = append(parking, edge.Callee.Func)
				}
			}

			if len(parking) == 0 {
				continue
			}

			kinds := map[string]bool{}
			var creators []string

			creatorsDeep := true
			for _, callee := range parking {
				for g := range makers[callee] {
					kind := creationKind(g, parks)
					kinds[kind] = true
					creators = append(creators, kind+":"+g.String())
					if !strings.HasPrefix(kind, "twin") || holed[outermost(g)] {
						creatorsDeep = false
					}
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

			// Shape (a): a call through a callback-only parameter is a FALLBACK by rule (plain, warned), never awaited.
			if call, isCall := site.(*ssa.Call); isCall && optOutCallbacks && isCallbackOnlyCall(call, optOnly) {
				verdict = "fallback(opt-out)"
			}

			callerKind := creationKind(fn, parks)
			entered := entry(fn, graph, parks)
			entries[entered]++
			if verdict == "covered" && entered == "twin" {
				awaited++
			}

			if callerKind != "package-init" && parks[outermost(fn)] && !holedNoInit[outermost(fn)] {
				deepNoInit["twin"]++
				if verdict == "covered" {
					awaitedNoInit++
				}
			} else if callerKind != "package-init" && parks[outermost(fn)] {
				deepNoInit["twin-holed"]++
				creators = append(creators, "NOINIT-HOLED-BY "+holedPathIn(holedFromNoInit, outermost(fn)))
			}

			if callerKind == "package-init" {
				deep["plain"]++
			} else if parks[outermost(fn)] && !holed[outermost(fn)] {
				deep["twin"]++
				if verdict == "covered" {
					awaitedDeep++
					if creatorsDeep {
						awaitedStrict++
					}
				}
			} else if parks[outermost(fn)] {
				deep["twin-holed"]++
				creators = append(creators, "HOLED-BY "+holedPathIn(holedFrom, outermost(fn)))
			} else {
				deep["plain"]++
			}

			verdicts[verdict]++

			if byCaller[callerKind] == nil {
				byCaller[callerKind] = map[string]int{}
			}

			byCaller[callerKind][verdict]++
			sort.Strings(creators)
			rows = append(rows, siteRow{prog.Fset.Position(site.Pos()).String(), fn.String(), callerKind + "/" + entered, verdict, creators})
		}
	}

	total := len(rows)
	fmt.Printf("CREATION CENSUS prefix=%s: %d can-park func-value call sites\n", prefix, total)

	for _, v := range []string{"covered", "partial", "uncovered", "no-creation-found", "fallback(opt-out)"} {
		fmt.Printf("  %-18s %d\n", v, verdicts[v])
	}

	fmt.Printf("  caller entered: twin %d, twin-holed %d, plain %d\n", entries["twin"], entries["twin-holed"], entries["plain"])
	fmt.Printf("  AWAITED (every creator a twin AND the caller entered as a twin): %d of %d\n", awaited, total)
	fmt.Printf("  caller entered, TRANSITIVE: twin %d, twin-holed %d, plain %d; AWAITED (transitive) %d of %d\n",
		deep["twin"], deep["twin-holed"], deep["plain"], awaitedDeep, total)
	fmt.Printf("  AWAITED STRICT (also every creator entered as a twin, transitive): %d of %d\n", awaitedStrict, total)
	fmt.Printf("  TRANSITIVE, initializers NOT counted as roots (the rule's own holes): twin %d, twin-holed %d; AWAITED %d of %d\n",
		deepNoInit["twin"], deepNoInit["twin-holed"], awaitedNoInit, total)

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
