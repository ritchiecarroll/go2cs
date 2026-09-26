package main

import (
	"fmt"

	al "AliasImportLib"
)

// The same package through an ALIASED import: a composite literal of an imported type alias, a
// conversion to it, and a func alias.
func aliased() {
	b := al.B2{V: 7}
	c := al.B2(al.Box{V: 8})
	var f al.IntFn = func(x int) int { return x * 3 }
	fmt.Println("aliased:", b, b.V, c.V, f(2))
}
