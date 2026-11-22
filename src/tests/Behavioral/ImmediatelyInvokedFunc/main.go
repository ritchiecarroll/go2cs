// Regression test for the immediately-invoked function literal (IIFE): `func(){ … }()`.
//
// A bare C# lambda cannot be invoked directly — `() => {…}()` is CS0149 — so the converter
// previously emitted uncompilable code for an IIFE. And an IIFE that uses defer/recover needs
// its OWN defer/recover scope: the deferred code (and any recover) must apply to the IIFE,
// letting execution continue after it. A no-argument IIFE is now emitted as a
// `func((defer, recover) => body)` execution-context call, which both wraps (its own
// defer/recover scope) and runs immediately.
package main

import "fmt"

// drain has a lone blank parameter and a select that receives and drops the value. The parameter must not stay
// `_` in C#, or the dropped receive's `out _` binds to it.
func drain(_ string, ch chan struct{}) string {
	select {
	case <-ch:
		return "drained"
	}
}

func main() {
	// (1) void IIFE
	func() {
		fmt.Println("a")
	}()

	// (2) value-returning IIFE, assigned
	x := func() int {
		return 6 * 7
	}()
	fmt.Println(x) // 42

	// (3) IIFE that recovers a panic in its own scope; execution continues afterward.
	func() {
		defer func() {
			if r := recover(); r != nil {
				fmt.Println("recovered:", r)
			}
		}()
		panic("boom")
	}()
	fmt.Println("after recover") // printed — the panic was scoped to the IIFE

	// (4) IIFE whose result feeds directly into an expression.
	total := 10 + func() int {
		sum := 0
		for i := 1; i <= 4; i++ {
			sum += i
		}
		return sum
	}()
	fmt.Println(total) // 10 + (1+2+3+4) = 20

	// (5) nested IIFEs.
	y := func() int {
		return func() int { return 5 }() * 2
	}()
	fmt.Println(y) // 10

	// (6) argument-taking IIFE: arguments are bound as locals.
	func(n int) {
		fmt.Println("n =", n)
	}(7)

	// (7) multi-argument, value-returning IIFE.
	tri := func(a, b, c int) int {
		return a + b + c
	}(1, 2, 3)
	fmt.Println(tri) // 6

	// (8) the loop-capture idiom: each iteration binds its own copy of k.
	for k := 0; k < 3; k++ {
		func(x int) {
			fmt.Print(x, " ")
		}(k)
	}
	fmt.Println() // 0 1 2

	// (9) argument-taking IIFE that recovers in its own scope.
	func(label string) {
		defer func() {
			if r := recover(); r != nil {
				fmt.Println(label, "recovered:", r)
			}
		}()
		panic("argboom")
	}("scope")
	fmt.Println("after arg recover")

	// (10) blank parameter discards its argument (still evaluated).
	calls := 0
	bump := func() int { calls++; return 99 }
	func(_ int) {
		fmt.Println("ignored arg; calls =", calls)
	}(bump())

	done := make(chan struct{})
	close(done)
	fmt.Println(drain("unused", done))
}
