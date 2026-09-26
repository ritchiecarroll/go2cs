package main

import (
	"fmt"
	"time"

	"AliasImportLib"
)

// Every exported alias of AliasImportLib is imported as a `global using` in this package, so each
// record must name a type that exists here, whether or not this file names the alias.
func main() {
	var f AliasImportLib.IntFn = func(x int) int { return x + 1 }
	var g AliasImportLib.BoxFn = func(b AliasImportLib.Box) AliasImportLib.Box { return AliasImportLib.Box{V: b.V * 2} }
	var d AliasImportLib.DurFn = func(t time.Duration) int { return int(t / time.Second) }
	var a AliasImportLib.Act = func() { fmt.Println("act") }
	var m AliasImportLib.Multi = func(x int) (int, error) { return x * 10, nil }
	var v AliasImportLib.Var = func(xs ...int) int { return len(xs) }
	var nr AliasImportLib.Named = func(x int) (n int, ok bool) { return x + 100, x > 0 }

	a()
	r, err := m(5)
	fmt.Println("func:", f(1), g(AliasImportLib.B2{V: 3}).V, d(2*time.Second), r, err, v(1, 2, 3), AliasImportLib.ApplyVar(v, 4, 5))
	n, ok := nr(7)
	fmt.Println("named:", n, ok)
	fmt.Println("struct:", AliasImportLib.B2{V: 4})
	aliased()
}
