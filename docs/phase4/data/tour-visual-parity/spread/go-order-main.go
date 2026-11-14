// Go's order for a multi-value spread f(g()) when the CALLEE expression has work of its own (gc, go1.26.10).
package main

import "fmt"

type pair struct{ a, b int }

func (p pair) scaled(x, y int) int { return p.a*1000 + p.b*100 + x*10 + y }

var log []string

func pick() func(int, int) int {
	log = append(log, "pick")
	return func(a, b int) int { return a + b }
}

func g() (int, int) {
	log = append(log, "g")
	return 1, 2
}

func main() {
	// 1. A callee expression that CALLS: the spec orders calls left to right, so pick() before g().
	r := pick()(g())
	fmt.Println("1. pick()(g()):", r, log)

	// 2. A VALUE receiver that the argument's call changes: the spec leaves the read of p unordered against g2().
	p := pair{1, 2}
	g2 := func() (int, int) { p.a = 9; return 3, 4 }
	fmt.Println("2. p.scaled(g2()):", p.scaled(g2()))

	// 3. A func VARIABLE that the argument's call reassigns.
	h := func(a, b int) int { return a + b }
	g3 := func() (int, int) { h = func(a, b int) int { return a * b }; return 3, 4 }
	fmt.Println("3. h(g3()):", h(g3()))
}
