// Go's order in a composite literal: gc reads a plain element after the literal's calls, for a
// slice, an array and a struct. A map literal is the exception: gc reads its elements in place.
package main

import "fmt"

type pt struct{ A, B int }

func main() {
	a := 1
	setA := func() int { a = 10; return 2 }
	fmt.Println("slice:", []int{a, setA()})

	a = 1
	fmt.Println("array:", [2]int{a, setA()})

	a = 1
	fmt.Println("struct:", pt{A: a, B: setA()})

	a = 1
	fmt.Println("nested:", [][]int{{a}, {setA()}})

	a = 1
	fmt.Println("map (in place in Go):", map[string]int{"a": a, "b": setA()})
}
