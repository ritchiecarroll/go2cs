// canpark is the M:N spike's P1 census (docs/phase4/DESIGN-mn-scheduler.md §8 item 5; COORD's G1 stop line): which
// functions of the converted Go code can PARK, that is, reach a Go-semantic wait through any call, direct, through an
// interface method or through a func value. Read-only; it loads Go source, never the C# tree.
//
// THE LEAVES are the cooperative scheduler's park inventory (docs/phase4/DESIGN-cooperative-scheduler.md §6), in Go
// terms: rows 1-3 a channel send or receive or a blocking select (SSA instructions); row 4 sync's semaphore family
// and row 6 internal/poll's (the bodyless runtime_Semacquire* linknames); row 5 sync.runtime_notifyListWait; row 7
// time.Sleep; row 9 internal/poll.runtime_pollWait*. Row 8 is rows 1-2; rows 10-12 are never goroutine parks.
//
// THE GRAPH is VTA (golang.org/x/tools/go/callgraph/vta) over a CHA seed: it resolves interface invokes and func-value
// calls to the functions that can actually flow there, which is the population a twin's dynamic dispatch must close
// over (net/http's HandlerFunc is a delegate). A `go` statement's edge does NOT propagate: starting a goroutine never
// parks the starter. A `defer` edge does: the deferred call runs before the function returns.
//
// Usage: canpark [-goos windows] [-exclude-runtime] [-top 25] [-json out.json] <patterns...>   (e.g. std)
package main

import (
	"encoding/json"
	"flag"
	"fmt"
	"go/token"
	"os"
	"sort"
	"strings"

	"golang.org/x/tools/go/callgraph"
	"golang.org/x/tools/go/callgraph/cha"
	"golang.org/x/tools/go/callgraph/vta"
	"golang.org/x/tools/go/packages"
	"golang.org/x/tools/go/ssa"
	"golang.org/x/tools/go/ssa/ssautil"
)

// leafKind names why a function parks directly, for the report's per-row counts.
type leafKind string

const (
	leafChanSend  leafKind = "chan-send"
	leafChanRecv  leafKind = "chan-recv"
	leafSelect    leafKind = "select"
	leafSyncLock  leafKind = "sync-lock" // Mutex / RWMutex contention: SHORT
	leafSyncWait  leafKind = "sync-wait" // WaitGroup and the plain semaphore: LONG
	leafSyncCond  leafKind = "sync-cond"
	leafPollSema  leafKind = "poll-fdlock" // internal/poll's fdMutex contention: SHORT
	leafSleep     leafKind = "time-sleep"
	leafPollWait  leafKind = "netpoll-wait"
	leafNotLeaf   leafKind = ""
	runtimeFamily          = "runtime"
)

// shortLeaf reports a lock-contention park: real in Go (a contended Mutex parks), but brief, and not what a program
// holding 10^5 goroutines parks them on. -long-only drops these leaves, so the census can say how much of the
// can-park set exists only because some callee takes a lock.
func shortLeaf(kind leafKind) bool { return kind == leafSyncLock || kind == leafPollSema }

// calleeLeaf classifies a bodyless (linknamed) callee as a park leaf, or not.
func calleeLeaf(fn *ssa.Function) leafKind {
	if fn == nil || fn.Pkg == nil {
		return leafNotLeaf
	}

	path, name := fn.Pkg.Pkg.Path(), fn.Name()

	switch {
	case path == "sync" && (strings.HasPrefix(name, "runtime_SemacquireMutex") || strings.HasPrefix(name, "runtime_SemacquireRWMutex")):
		return leafSyncLock
	case path == "sync" && strings.HasPrefix(name, "runtime_Semacquire"):
		return leafSyncWait
	case path == "sync" && name == "runtime_notifyListWait":
		return leafSyncCond
	case path == "internal/poll" && strings.HasPrefix(name, "runtime_Semacquire"):
		return leafPollSema
	case path == "internal/poll" && strings.HasPrefix(name, "runtime_pollWait"):
		return leafPollWait
	case path == "time" && name == "Sleep":
		return leafSleep
	}

	return leafNotLeaf
}

// directLeaves lists the park leaves in fn's own body: its channel and select instructions and its calls to a leaf.
func directLeaves(fn *ssa.Function) []leafKind {
	var leaves []leafKind

	for _, block := range fn.Blocks {
		for _, instr := range block.Instrs {
			switch v := instr.(type) {
			case *ssa.Send:
				leaves = append(leaves, leafChanSend)
			case *ssa.UnOp:
				if v.Op == token.ARROW {
					leaves = append(leaves, leafChanRecv)
				}
			case *ssa.Select:
				if v.Blocking {
					leaves = append(leaves, leafSelect)
				}
			case ssa.CallInstruction:
				if _, isGo := v.(*ssa.Go); isGo {
					continue
				}

				if kind := calleeLeaf(v.Common().StaticCallee()); kind != leafNotLeaf && !(longOnly && shortLeaf(kind)) {
					leaves = append(leaves, kind)
				}
			}
		}
	}

	return leaves
}

// longOnly is the -long-only flag: lock-contention leaves (shortLeaf) do not count as parks.
var longOnly bool

// staticOnly is the -static-only flag: propagate through static calls only (the census's lower bound).
var staticOnly bool

func inRuntimeFamily(fn *ssa.Function) bool {
	if fn.Pkg == nil {
		return false
	}

	path := fn.Pkg.Pkg.Path()

	return path == runtimeFamily || strings.HasPrefix(path, "runtime/") || strings.HasPrefix(path, "internal/runtime/")
}

// counted reports whether fn is a function the census counts: a declared function or method (not a closure, not a
// synthetic wrapper, not a generic instantiation, whose origin is counted instead) with a body.
func counted(fn *ssa.Function) bool {
	return fn.Pkg != nil && fn.Synthetic == "" && fn.Parent() == nil && fn.Origin() == nil && len(fn.Blocks) > 0
}

type siteTally struct {
	Total      int `json:"total"`
	CanPark    int `json:"canPark"`
	Unresolved int `json:"unresolved"`
}

func (t *siteTally) add(edges []*callgraph.Edge, parks map[*ssa.Function]bool) {
	t.Total++

	if len(edges) == 0 {
		t.Unresolved++
		return
	}

	for _, edge := range edges {
		if parks[edge.Callee.Func] {
			t.CanPark++
			return
		}
	}
}

type packageShare struct {
	Package   string  `json:"package"`
	Functions int     `json:"functions"`
	CanPark   int     `json:"canPark"`
	Share     float64 `json:"share"`
}

type report struct {
	Patterns          []string       `json:"patterns"`
	GOOS              string         `json:"goos"`
	ExcludeRuntime    bool           `json:"excludeRuntime"`
	LongOnly          bool           `json:"longOnly"`
	StaticOnly        bool           `json:"staticOnly"`
	Functions         int            `json:"functions"`
	CanPark           int            `json:"canPark"`
	Share             float64        `json:"share"`
	DirectParkers     int            `json:"directParkers"`
	DirectByLeaf      map[string]int `json:"directByLeaf"`
	GoStatements      siteTally      `json:"goStatements"`
	InterfaceCalls    siteTally      `json:"interfaceCalls"`
	FuncValueCalls    siteTally      `json:"funcValueCalls"`
	Closures          int            `json:"closures"`
	ClosuresCanPark   int            `json:"closuresCanPark"`
	TopPackages       []packageShare `json:"topPackages"`
	Packages          []packageShare `json:"packages"`
	CanParkNames      []string       `json:"canParkNames,omitempty"`
	DirectParkerNames []string       `json:"directParkerNames,omitempty"`
}

func main() {
	goos := flag.String("goos", "windows", "GOOS the packages load under (the converted corpus is per-GOOS)")
	excludeRuntime := flag.Bool("exclude-runtime", false, "leave runtime, runtime/* and internal/runtime/* out of the counts (they still propagate)")
	top := flag.Int("top", 25, "packages listed in the text report, by can-park count")
	jsonOut := flag.String("json", "", "write the full report, every can-park function named, to this file")
	why := flag.String("why", "", "comma-separated functions (ssa names, e.g. strings.ToUpper or (*sync.WaitGroup).Wait): print each one's verdict and, if it can park, the shortest call path to a park leaf")
	flag.BoolVar(&longOnly, "long-only", false, "count only LONG parks: drop Mutex/RWMutex and fdMutex contention leaves")
	flag.BoolVar(&staticOnly, "static-only", false, "propagate through static calls only: the lower bound (the default, through VTA's dynamic edges too, is the upper)")
	flag.Parse()

	patterns := flag.Args()
	if len(patterns) == 0 {
		fmt.Fprintln(os.Stderr, "usage: canpark [flags] <patterns...>")
		os.Exit(2)
	}

	cfg := &packages.Config{
		Mode: packages.NeedName | packages.NeedFiles | packages.NeedImports | packages.NeedDeps | packages.NeedTypes |
			packages.NeedSyntax | packages.NeedTypesInfo | packages.NeedTypesSizes,
		Env: append(os.Environ(), "GOOS="+*goos, "GOARCH=amd64", "CGO_ENABLED=0"),
	}

	pkgs, err := packages.Load(cfg, patterns...)
	if err != nil {
		fmt.Fprintln(os.Stderr, "load:", err)
		os.Exit(1)
	}

	if n := packages.PrintErrors(pkgs); n > 0 {
		fmt.Fprintf(os.Stderr, "load: %d package error(s)\n", n)
		os.Exit(1)
	}

	prog, _ := ssautil.AllPackages(pkgs, ssa.InstantiateGenerics)
	prog.Build()

	all := ssautil.AllFunctions(prog)
	graph := vta.CallGraph(all, cha.CallGraph(prog))
	graph.DeleteSyntheticNodes()

	// Direct parkers, then the reverse closure over non-`go` edges.
	parks := map[*ssa.Function]bool{}
	byLeaf := map[string]int{}
	var queue []*ssa.Function

	// The runtime family neither parks nor propagates (park inventory rows 11-12): its scheduler, GC and finalizer
	// paths are converted-dead in go2cs, where golib owns goroutine parks and the CLR owns GC. Without this rule every
	// allocation "parks" through mallocgc -> gcStart -> gcBgMarkStartWorkers' channel receive (the first census's
	// witness for fmt.Sprintf, 2026-11-09).
	for fn := range all {
		if inRuntimeFamily(fn) {
			continue
		}

		leaves := directLeaves(fn)
		if len(leaves) == 0 {
			continue
		}

		parks[fn] = true
		queue = append(queue, fn)

		seen := map[leafKind]bool{}
		for _, kind := range leaves {
			if !seen[kind] {
				seen[kind] = true
				byLeaf[string(kind)]++
			}
		}
	}

	for len(queue) > 0 {
		fn := queue[0]
		queue = queue[1:]

		node := graph.Nodes[fn]
		if node == nil {
			continue
		}

		for _, edge := range node.In {
			if _, isGo := edge.Site.(*ssa.Go); isGo {
				continue
			}

			// -static-only: the LOWER bound. Only calls whose callee is fixed at the call site propagate; VTA's
			// context-insensitive interface and func-value edges (every io.Writer passed anywhere reaches every
			// Fprintf) make the default the UPPER bound.
			if staticOnly && edge.Site.Common().StaticCallee() == nil {
				continue
			}

			caller := edge.Caller.Func
			if inRuntimeFamily(caller) {
				continue
			}

			if !parks[caller] {
				parks[caller] = true
				queue = append(queue, caller)
			}
		}
	}

	if *why != "" {
		explain(strings.Split(*why, ","), all, graph, parks)
	}

	rep := report{Patterns: patterns, GOOS: *goos, ExcludeRuntime: *excludeRuntime, LongOnly: longOnly, StaticOnly: staticOnly, DirectByLeaf: byLeaf}
	perPackage := map[string]*packageShare{}

	for fn := range all {
		if *excludeRuntime && inRuntimeFamily(fn) {
			continue
		}

		if fn.Parent() != nil && fn.Pkg != nil && fn.Synthetic == "" {
			rep.Closures++
			if parks[fn] {
				rep.ClosuresCanPark++
			}
		}

		if counted(fn) {
			path := fn.Pkg.Pkg.Path()
			share := perPackage[path]
			if share == nil {
				share = &packageShare{Package: path}
				perPackage[path] = share
			}

			share.Functions++
			rep.Functions++

			if parks[fn] {
				share.CanPark++
				rep.CanPark++
				rep.CanParkNames = append(rep.CanParkNames, fn.String())
			}

			if len(directLeaves(fn)) > 0 {
				rep.DirectParkers++
				rep.DirectParkerNames = append(rep.DirectParkerNames, fn.String())
			}
		}

		node := graph.Nodes[fn]
		if node == nil {
			continue
		}

		// Classify every call site in this function's body by its kind, with its VTA edges.
		edgesBySite := map[ssa.CallInstruction][]*callgraph.Edge{}
		for _, edge := range node.Out {
			edgesBySite[edge.Site] = append(edgesBySite[edge.Site], edge)
		}

		for _, block := range fn.Blocks {
			for _, instr := range block.Instrs {
				site, ok := instr.(ssa.CallInstruction)
				if !ok {
					continue
				}

				common := site.Common()

				switch {
				case isGo(site):
					rep.GoStatements.add(edgesBySite[site], parks)
				case common.IsInvoke():
					rep.InterfaceCalls.add(edgesBySite[site], parks)
				case common.StaticCallee() == nil && !isBuiltin(common):
					rep.FuncValueCalls.add(edgesBySite[site], parks)
				}
			}
		}
	}

	if rep.Functions > 0 {
		rep.Share = float64(rep.CanPark) / float64(rep.Functions)
	}

	for _, share := range perPackage {
		if share.Functions > 0 {
			share.Share = float64(share.CanPark) / float64(share.Functions)
		}

		rep.Packages = append(rep.Packages, *share)
	}

	sort.Slice(rep.Packages, func(i, j int) bool {
		if rep.Packages[i].CanPark != rep.Packages[j].CanPark {
			return rep.Packages[i].CanPark > rep.Packages[j].CanPark
		}

		return rep.Packages[i].Package < rep.Packages[j].Package
	})

	rep.TopPackages = rep.Packages
	if len(rep.TopPackages) > *top {
		rep.TopPackages = rep.TopPackages[:*top]
	}

	sort.Strings(rep.CanParkNames)

	fmt.Printf("canpark %s goos=%s exclude-runtime=%v long-only=%v static-only=%v\n", strings.Join(patterns, " "), *goos, *excludeRuntime, longOnly, staticOnly)
	fmt.Printf("FUNCTIONS %d can-park %d share %.1f%% (direct parkers %d)\n", rep.Functions, rep.CanPark, 100*rep.Share, rep.DirectParkers)
	fmt.Printf("DIRECT BY LEAF %v\n", byLeaf)
	fmt.Printf("CLOSURES %d can-park %d\n", rep.Closures, rep.ClosuresCanPark)
	fmt.Printf("GO STATEMENTS %d can-park %d unresolved %d\n", rep.GoStatements.Total, rep.GoStatements.CanPark, rep.GoStatements.Unresolved)
	fmt.Printf("INTERFACE CALLS %d can-park %d unresolved %d\n", rep.InterfaceCalls.Total, rep.InterfaceCalls.CanPark, rep.InterfaceCalls.Unresolved)
	fmt.Printf("FUNC-VALUE CALLS %d can-park %d unresolved %d\n", rep.FuncValueCalls.Total, rep.FuncValueCalls.CanPark, rep.FuncValueCalls.Unresolved)
	fmt.Printf("PACKAGES %d; top %d by can-park:\n", len(rep.Packages), len(rep.TopPackages))

	for _, share := range rep.TopPackages {
		fmt.Printf("  %-40s %6d / %6d  %5.1f%%\n", share.Package, share.CanPark, share.Functions, 100*share.Share)
	}

	if *jsonOut != "" {
		data, err := json.MarshalIndent(rep, "", "  ")
		if err == nil {
			err = os.WriteFile(*jsonOut, data, 0o644)
		}

		if err != nil {
			fmt.Fprintln(os.Stderr, "json:", err)
			os.Exit(1)
		}
	}
}

func isGo(site ssa.CallInstruction) bool {
	_, ok := site.(*ssa.Go)
	return ok
}

func isBuiltin(common *ssa.CallCommon) bool {
	_, ok := common.Value.(*ssa.Builtin)
	return ok
}

// explain prints, for each named function, whether it can park and the shortest non-`go` call path from it to a
// function that parks directly, with the leaf it reaches: the witness a census number needs before it is believed.
func explain(names []string, all map[*ssa.Function]bool, graph *callgraph.Graph, parks map[*ssa.Function]bool) {
	byName := map[string]*ssa.Function{}
	for fn := range all {
		byName[fn.String()] = fn
	}

	for _, name := range names {
		fn := byName[strings.TrimSpace(name)]
		if fn == nil {
			fmt.Printf("WHY %s: not found\n", name)
			continue
		}

		if !parks[fn] {
			fmt.Printf("WHY %s: CANNOT PARK\n", name)
			continue
		}

		prev := map[*ssa.Function]*callgraph.Edge{fn: nil}
		queue := []*ssa.Function{fn}
		var hit *ssa.Function

		for len(queue) > 0 && hit == nil {
			cur := queue[0]
			queue = queue[1:]

			if len(directLeaves(cur)) > 0 {
				hit = cur
				break
			}

			if node := graph.Nodes[cur]; node != nil {
				for _, edge := range node.Out {
					if isGo(edge.Site) || !parks[edge.Callee.Func] || (staticOnly && edge.Site.Common().StaticCallee() == nil) {
						continue
					}

					if _, seen := prev[edge.Callee.Func]; !seen {
						prev[edge.Callee.Func] = edge
						queue = append(queue, edge.Callee.Func)
					}
				}
			}
		}

		var path []string
		for cur := hit; cur != nil; {
			edge := prev[cur]
			step := cur.String()
			if edge != nil {
				kind := "static"
				if edge.Site.Common().IsInvoke() {
					kind = "interface"
				} else if edge.Site.Common().StaticCallee() == nil {
					kind = "func-value"
				}
				step = fmt.Sprintf("-[%s]-> %s", kind, step)
				cur = edge.Caller.Func
			} else {
				cur = nil
			}
			path = append([]string{step}, path...)
		}

		fmt.Printf("WHY %s: CAN PARK via %v; path: %s\n", name, directLeaves(hit), strings.Join(path, " "))
	}
}
