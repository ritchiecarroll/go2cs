// Statement preludes and labels, plus the statement forms the design review asked for: a goto to a labeled statement
// whose call moves ahead of its operands, a labeled range whose operand moves, a labeled if, an else-if with an init,
// a func literal with a prelude of its own inside a statement that has one, and a loop condition whose call moves.
package main

import "fmt"

var g int

func bump() int { g++; return g * 10 }

func pick(a, b int) []int { return []int{a, b} }

func show(a, b, c int) bool {
	fmt.Println("else-if condition:", a, b, c)
	return true
}

func gotoCase() {
	n := 0
again:
	fmt.Println("goto target:", g, bump())
	n++

	if n < 2 {
		goto again
	}
}

func rangeCase() {
outer:
	for _, x := range pick(g, bump()) {
		for j := 0; j < 2; j++ {
			if j == 1 {
				continue outer
			}

			fmt.Println("labeled range:", x, j)
		}
	}
}

func ifCase() {
	n := 0
check:
	if g+bump() > 0 {
		fmt.Println("labeled if:", g)
	}

	n++

	if n < 2 {
		goto check
	}
}

func elseIfCase() {
	if g > 1000 {
		fmt.Println("unreachable")
	} else if v := bump(); show(g, v, bump()) {
		fmt.Println("else-if body:", g)
	}
}

func literalCase() {
	fmt.Println("literal:", g, func() int {
		fmt.Println("literal body:", g, bump())
		return 1
	}(), bump())
}

func less(a, b int) bool {
	fmt.Println("loop condition:", a, b)
	return a < b
}

// The condition's prelude runs inside the loop, ahead of the spilled test, so it runs again on every pass.
func loopCase() {
	for n := 0; less(g, bump()); n++ {
		fmt.Println("loop pass:", n)

		if n == 1 {
			break
		}
	}
}

func main() {
	gotoCase()
	rangeCase()
	ifCase()
	elseIfCase()
	literalCase()
	loopCase()
}
