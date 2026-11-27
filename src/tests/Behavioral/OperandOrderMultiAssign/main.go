// Go's order on the right of a multi-value assignment: gc reads a plain operand after the right
// side's calls.
package main

import "fmt"

func main() {
	b := 1
	setB := func() int { b = 10; return 2 }
	m1, m2 := b, setB()
	fmt.Println("define:", m1, m2)

	b = 1
	var n1, n2 int
	n1, n2 = b, setB()
	fmt.Println("assign:", n1, n2)

	x, y := 1, 5
	setY := func() int { x = 10; return y }
	x, y = setY(), x
	fmt.Println("swap through a call:", x, y)
}
