// Go's order in an assignment: gc reads the operands of an index or a dereference on the left, and a
// plain operand on the right, after the right side's calls. Each line changes such an operand in a
// call on the right.
package main

import "fmt"

func main() {
	arr := []int{0, 0, 0}
	i := 0
	setI := func() int { i = 2; return 7 }
	arr[i] = setI()
	fmt.Println("left index:", arr)

	v1, v2 := 0, 0
	p := &v1
	setP := func() int { p = &v2; return 7 }
	*p = setP()
	fmt.Println("left dereference:", v1, v2)

	x := 1
	setX := func() int { x = 10; return 2 }
	var y int
	y = x + setX()
	fmt.Println("right operand:", y)

	m := map[int]int{}
	k := 1
	setK := func() int { k = 5; return 7 }
	m[k] = setK()
	fmt.Println("left map key:", m)
}
