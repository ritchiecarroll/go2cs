// optout.go: N3's shape (a), COORD's ruling 2026-11-14: the propagating caller rule with an OPT-OUT for
// callback-only callers. A func-value PARAMETER is callback-only when every use of it is a call through it, or a
// pass of it to a callback-only parameter of a statically known callee (to a fixpoint). It is never stored,
// returned, sent, captured by a literal, converted, compared, or handed to a goroutine. A function whose every
// func-value call goes through its own callback-only parameters takes no twin for that reason; those call sites
// fall back to plain, warned once per function.
package main

import (
	"fmt"
	"go/types"
	"sort"
	"strings"

	"golang.org/x/tools/go/callgraph"
	"golang.org/x/tools/go/ssa"
)

// optOutCallbacks is the -opt-out-callbacks flag; optOnly its callback-only parameters, once computed.
var (
	optOutCallbacks bool
	optOnly         map[*ssa.Parameter]bool
)

// callbackOnlyParams computes, to a fixpoint, the func-typed parameters that are only ever called or passed on to
// another callback-only parameter.
func callbackOnlyParams(all map[*ssa.Function]bool) map[*ssa.Parameter]bool {
	only := map[*ssa.Parameter]bool{}
	for fn := range all {
		for _, p := range fn.Params {
			if _, isFunc := p.Type().Underlying().(*types.Signature); isFunc {
				only[p] = true
			}
		}
	}

	for changed := true; changed; {
		changed = false
		for p := range only {
			if !only[p] || paramUsesOK(p, only) {
				continue
			}

			only[p] = false
			changed = true
		}
	}

	return only
}

func paramUsesOK(p *ssa.Parameter, only map[*ssa.Parameter]bool) bool {
	refs := p.Referrers()
	if refs == nil {
		return true
	}

	for _, instr := range *refs {
		switch use := instr.(type) {
		case *ssa.DebugRef:
			continue
		case *ssa.Call, *ssa.Defer:
			common := use.(ssa.CallInstruction).Common()
			if common.Value == p && !common.IsInvoke() {
				// A call through the parameter, and nowhere among the arguments.
				if !argsHold(common.Args, p) {
					continue
				}
			}

			callee := common.StaticCallee()
			if callee == nil || common.Value == p {
				return false
			}

			for i, arg := range common.Args {
				if arg != p {
					continue
				}

				if i >= len(callee.Params) || !only[callee.Params[i]] {
					return false
				}
			}
		default:
			return false
		}
	}

	return true
}

func argsHold(args []ssa.Value, p *ssa.Parameter) bool {
	for _, arg := range args {
		if arg == p {
			return true
		}
	}

	return false
}

// funcValueCalls lists fn's direct calls through a func value (not go, not defer), as the seed rule counts them.
func funcValueCalls(fn *ssa.Function) []*ssa.Call {
	var calls []*ssa.Call
	for _, block := range fn.Blocks {
		for _, instr := range block.Instrs {
			call, ok := instr.(*ssa.Call)
			if !ok {
				continue
			}

			common := call.Common()
			if common.StaticCallee() == nil && !common.IsInvoke() && !isBuiltin(common) {
				calls = append(calls, call)
			}
		}
	}

	return calls
}

// isCallbackOnlyCall reports whether call goes through one of its function's callback-only parameters.
func isCallbackOnlyCall(call *ssa.Call, only map[*ssa.Parameter]bool) bool {
	p, ok := call.Common().Value.(*ssa.Parameter)
	return ok && only[p]
}

// seedsUnderOptOut reports whether fn takes the caller rule under the opt-out: it has at least one func-value call
// that is NOT through a callback-only parameter.
func seedsUnderOptOut(fn *ssa.Function, only map[*ssa.Parameter]bool) bool {
	for _, call := range funcValueCalls(fn) {
		if !isCallbackOnlyCall(call, only) {
			return true
		}
	}

	return false
}

// reportOptOut prints what the opt-out gives up: the functions it opts out (wholly, or the callback-only sites
// of a function seeded for another call), the sites that fall back, and every site a REALLY parking value reaches
// (baseParks), which is the opt-out's real cost.
func reportOptOut(prog *ssa.Program, all map[*ssa.Function]bool, graph *callgraph.Graph, only map[*ssa.Parameter]bool,
	baseParks map[*ssa.Function]bool, prefix string) {
	whole, partial, sites := 0, 0, 0
	var parkingLines, wholeNames []string

	for fn := range all {
		if inRuntimeFamily(fn) {
			continue
		}

		calls := funcValueCalls(fn)
		if len(calls) == 0 {
			continue
		}

		fallbacks := 0
		for _, call := range calls {
			if !isCallbackOnlyCall(call, only) {
				continue
			}

			fallbacks++
			sites++

			if node := graph.Nodes[fn]; node != nil {
				for _, edge := range node.Out {
					if edge.Site == ssa.CallInstruction(call) && baseParks[edge.Callee.Func] {
						parkingLines = append(parkingLines, fmt.Sprintf("    %s at %s reaches parking %s", fn, prog.Fset.Position(call.Pos()), edge.Callee.Func))
					}
				}
			}
		}

		switch {
		case fallbacks == 0:
		case fallbacks == len(calls):
			whole++
			if prefix == "" || fn.Pkg != nil && strings.HasPrefix(fn.Pkg.Pkg.Path(), prefix) || len(wholeNames) < 400 {
				wholeNames = append(wholeNames, fn.String())
			}
		default:
			partial++
		}
	}

	sort.Strings(wholeNames)
	sort.Strings(parkingLines)
	fmt.Printf("OPT-OUT functions wholly opted out %d, seeded but with callback-only sites %d; fallback sites %d\n", whole, partial, sites)
	fmt.Printf("OPT-OUT sites a REALLY parking value reaches (the opt-out's real cost): %d\n", len(parkingLines))
	for _, line := range parkingLines {
		fmt.Println(line)
	}

	fmt.Println("OPT-OUT wholly opted-out functions:")
	for _, name := range wholeNames {
		fmt.Println("    " + name)
	}
}
