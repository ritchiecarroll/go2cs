package main

import "fmt"

type digits [3]int

type triple = [3]int

type cell struct {
	marks [2]bool
}

var primes = [6]int{2, 3, 5, 7, 11, 13}

var table = [4]byte{0x10, 0x20, 0x30, 0x40}

var calls int

func next() int {
	calls++
	return calls
}

func fresh() [3]int {
	return [3]int{1, 2, 3}
}

func main() {
	// Full length, short, and an ellipsis literal.
	fmt.Println(primes, len(primes))
	short := [8]byte{1, 2}
	fmt.Println(short, len(short))
	dots := [...]string{"a", "b", "c"}
	fmt.Println(dots, len(dots))
	fmt.Println(table[3])

	// Each evaluation is a fresh array.
	a, b := fresh(), fresh()
	a[0] = 9
	fmt.Println(a, b)

	// An alias converts; a named array type keeps its constructor.
	var t triple = triple{4, 5, 6}
	d := digits{7, 8, 9}
	fmt.Println(t, d)

	// Keyed, empty and interface-element literals.
	keyed := [5]int{1: 10, 3: 30}
	empty := [4]int{}
	mixed := [2]any{1, "one"}
	fmt.Println(keyed, empty, mixed)

	// An array of arrays keeps its outer form; its inner literals convert, a short one too.
	grid := [2][3]int{{1, 2, 3}, {4}}
	fmt.Println(grid, len(grid[1]))

	// Elided arrays inside a slice and behind a pointer.
	rows := [][2]string{{"x", "y"}, {"z"}}
	ptrs := []*[4]byte{{1}, {2, 3}}
	fmt.Println(rows, len(rows[1]), *ptrs[0], *ptrs[1])

	// An element whose zero value is constructed keeps the factory form when short.
	cells := [3]cell{{marks: [2]bool{true, false}}}
	fmt.Println(cells[0].marks, cells[2].marks, len(cells[2].marks))

	// A multi-line literal evaluates its elements in order.
	lines := [2]int{
		next(),
		next(),
	}
	fmt.Println(lines[1] - lines[0])
}
