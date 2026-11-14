// cgtool: rapid type analysis (RTA) of a program's call graph from main and every package init, for one
// GOOS/GOARCH, answering whether each named function is reachable and, when it is, one shortest call path to it.
//
// usage: cgtool DIR GOOS GOARCH TAGS pkgpath.func [pkgpath.func ...]
package main

import (
	"fmt"
	"os"
	"sort"
	"strings"

	"golang.org/x/tools/go/callgraph"
	"golang.org/x/tools/go/callgraph/rta"
	"golang.org/x/tools/go/packages"
	"golang.org/x/tools/go/ssa"
	"golang.org/x/tools/go/ssa/ssautil"
)

func main() {
	dir, goos, goarch, tags := os.Args[1], os.Args[2], os.Args[3], os.Args[4]
	wanted := os.Args[5:]

	cfg := &packages.Config{
		Mode:       packages.LoadAllSyntax,
		Dir:        dir,
		Env:        append(os.Environ(), "GOOS="+goos, "GOARCH="+goarch, "CGO_ENABLED=0"),
		BuildFlags: []string{"-tags=" + tags},
	}

	pkgs, err := packages.Load(cfg, ".")
	if err != nil {
		fmt.Println("load:", err)
		os.Exit(2)
	}

	if n := packages.PrintErrors(pkgs); n > 0 {
		fmt.Println("load errors:", n)
		os.Exit(2)
	}

	prog, ssaPkgs := ssautil.AllPackages(pkgs, ssa.InstantiateGenerics)
	prog.Build()

	var roots []*ssa.Function

	for _, p := range prog.AllPackages() {
		if init := p.Func("init"); init != nil {
			roots = append(roots, init)
		}
	}

	for _, p := range ssaPkgs {
		if p != nil && p.Pkg.Name() == "main" {
			if m := p.Func("main"); m != nil {
				roots = append(roots, m)
			}
		}
	}

	result := rta.Analyze(roots, true)
	byName := map[string]*ssa.Function{}

	for fn := range result.Reachable {
		if fn.Pkg != nil && fn.Parent() == nil {
			byName[fn.Pkg.Pkg.Path()+"."+fn.Name()] = fn
		}
	}

	fmt.Printf("%s/%s tags=[%s]: %d roots, %d reachable functions\n", goos, goarch, tags, len(roots), len(result.Reachable))

	declared := map[string]bool{}

	for _, p := range prog.AllPackages() {
		for _, member := range p.Members {
			if fn, ok := member.(*ssa.Function); ok {
				declared[p.Pkg.Path()+"."+fn.Name()] = true
			}
		}
	}

	sort.Strings(wanted)

	for _, name := range wanted {
		fn, reached := byName[name]

		switch {
		case !declared[name]:
			fmt.Printf("  %-55s not in this build\n", name)
		case !reached:
			fmt.Printf("  %-55s declared, NOT reachable\n", name)
		default:
			path := callgraph.PathSearch(result.CallGraph.Root, func(n *callgraph.Node) bool { return n.Func == fn })
			steps := make([]string, 0, len(path))

			for _, edge := range path {
				steps = append(steps, edge.Callee.Func.String())
			}

			callers := []string{}

			if node := result.CallGraph.Nodes[fn]; node != nil {
				for _, in := range node.In {
					callers = append(callers, in.Caller.Func.String())
				}
			}

			fmt.Printf("  %-55s REACHABLE addrTaken=%v callers=%d %v path: %s\n", name, result.Reachable[fn].AddrTaken, len(callers), callers, strings.Join(steps, " -> "))
		}
	}
}
