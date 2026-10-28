package main

import "fmt"

// An unnamed slice literal converts to golib's builtin over a C# collection expression, `slice<nint>([1, 2, 3])`,
// in every position: a non-nil slice with len == cap and a fresh backing array per evaluation, as Go's `[]T{…}`.
// A keyed literal, a fixed array, a slice of arrays and a named slice type keep their own forms.

type box struct {
	data []int
	tags []string
}

type IntSlice []int

func sum(xs []int) int {
	total := 0
	for _, x := range xs {
		total += x
	}
	return total
}

func pair() []int {
	return []int{1, 2}
}

func first[T any](xs []T) T {
	return xs[0]
}

func count(xs ...int) int {
	return len(xs)
}

func main() {
	// typed locals
	x := []int{1, 2, 3}
	var y []string = []string{"a", "b"}
	fmt.Println(x, len(x), cap(x), y)

	// an empty literal is non-nil, unlike a nil slice; Go prints [] for both
	e := []int{}
	var n []int
	fmt.Printf("empty: %v nil=%v len=%d cap=%d\n", e, e == nil, len(e), cap(e))
	fmt.Printf("nil:   %v nil=%v len=%d cap=%d\n", n, n == nil, len(n), cap(n))

	// a fresh backing array per evaluation
	a, b := pair(), pair()
	a[0] = 99
	fmt.Println("fresh:", a, b)

	// assignment to an existing variable
	x = []int{4, 5}
	fmt.Println("assigned:", x, len(x), cap(x))

	// struct fields, and a field reached through a pointer
	bx := box{data: []int{4, 5}, tags: []string{"t"}}
	p := &bx
	p.data = []int{6, 7, 8}
	fmt.Println("box:", bx.data, bx.tags)

	// a map value and nested literals
	m := map[string][]int{"k": {1, 2}}
	grid := [][]int{{1, 2}, {3}}
	fmt.Println("map:", m["k"], "grid:", grid, len(grid[1]))

	// a call argument to a converted function, and an explicit generic instantiation
	fmt.Println("sum:", sum([]int{1, 2, 3}), "first:", first[int]([]int{9, 8}))

	// %T of a converted literal
	fmt.Printf("%T %T\n", x, grid)

	// an any slot, an inferred generic call, builtin and range operands and a spread convert too
	fmt.Println("any:", []int{1})
	fmt.Println("inferred:", first([]int{7, 8}), len([]int{1, 2}), append([]int{1}, 2))
	for _, v := range []int{10, 20} {
		fmt.Print(v, " ")
	}
	fmt.Println(count([]int{1, 2, 3}...))
	named := IntSlice{3, 1}
	fmt.Println("named:", named, len(named))

	// kept: a keyed literal, a fixed array, a slice of arrays
	keyed := []int{2: 5}
	arr := [3]int{1, 2, 3}
	pairs := [][2]int{{1, 2}, {3, 4}}
	fmt.Println("kept:", keyed, arr, pairs, len(pairs[1]))
}
