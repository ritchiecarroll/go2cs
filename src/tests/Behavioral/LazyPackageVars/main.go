// LazyPackageVars: package vars go2cs builds on their first read (docs/DESIGN-lazy-static-data.md) must read exactly as
// Go's init-time build: the initializers that run code run in Go's order, an importer's assignment to an exported var
// before a lazy initializer reads it does not reach that initializer (Go built it at init), an allocation is one pointer,
// and func literal values call as declared.
package main

import (
	"fmt"

	"LazyPackageVars/tables"
)

func main() {
	// Init order: the code-running initializers and the init function, as Go ran them before main.
	first, second := tables.Seen()
	fmt.Println("order:", tables.Order, first, second)

	// An importer assigns the exported Heavy BEFORE ByName is first read. Go built ByName at init, from Heavy's initial
	// value; the converted ByName, built now, must hold that same value.
	tables.Heavy = []tables.Entry{{Name: "replaced", Weight: -1}}
	fmt.Println("heavy now:", tables.Heavy[0].Name, len(tables.Heavy))
	fmt.Println("ByName heavy:", len(tables.ByName["heavy"]), tables.ByName["heavy"][0].Name, tables.ByName["heavy"][2].Weight)
	fmt.Println("ByName all:", len(tables.ByName["all"]), tables.ByName["all"][0].Name)

	// An allocation is one pointer: a write through one read is seen through the next.
	p := tables.Roots[1]
	p.Weight = 20
	fmt.Println("roots:", tables.Roots[1] == p, tables.Roots[1].Weight, tables.Roots[3].Name)

	// Func literal values call as declared.
	for _, rule := range tables.Rules {
		fmt.Println("rule:", rule.Name, rule.Apply(7))
	}

	// Writes to a lazy var go through to it.
	tables.Roots[0] = &tables.Entry{Name: "swapped"}
	fmt.Println("swapped:", tables.Roots[0].Name)
}
