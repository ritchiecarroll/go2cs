// Guards a `go` call that takes the LAMBDA form (a value-returning callee) with an untyped
// numeric constant argument. Here the constant renders inside the thunk's call (seat #33):
// `goǃ(() => compute(poly))`. In the standard library's hoisted mode it stays eager, and the
// converter casts it to the PARAMETER type, not its default Go type — otherwise ᴛ1 infers
// the default (`nint`) and `f(ᴛ1)` fails (CS1503). That is hash/crc32's test
// `go MakeTable(Castagnoli)` (Castagnoli an untyped uint32 poly). See convCallExpr.go's
// deferred/go untyped-numeric-const branch.
package main

import "fmt"

// Castagnoli-like untyped polynomial constant: default type is int, but it feeds a uint32
// parameter, so the emitted cast must target uint32 (not the default nint).
const poly = 0x82f63b78

var done = make(chan uint32, 1)

// compute RETURNS a value (forces the lambda form for `go compute(...)`) and reports its
// result over a buffered channel so main can observe it deterministically.
func compute(p uint32) uint32 {
	r := p ^ 0xffffffff
	done <- r
	return r
}

func main() {
	go compute(poly)
	stored := <-done
	fmt.Printf("%#x\n", stored)
}
