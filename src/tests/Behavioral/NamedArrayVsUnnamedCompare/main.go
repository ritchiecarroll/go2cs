package main

import "fmt"

// A value of a NAMED array type compared with a value of the unnamed array type it is defined from.
// Go makes the comparison legal because the unnamed side is assignable to the named type.

type Hash [4]byte

func sum(b []byte) [4]byte {
	var out [4]byte
	for i, v := range b {
		out[i%4] += v
	}
	return out
}

func main() {
	var h Hash
	if h != sum(nil) {
		fmt.Println("differs")
		return
	}

	fmt.Println("equal")

	// Both operand orders, with == as well as !=.
	g := Hash(sum([]byte{1, 2, 3, 4}))
	fmt.Println(g == sum([]byte{1, 2, 3, 4}), sum([]byte{1, 2, 3, 4}) == g)
	fmt.Println(g != sum(nil), sum(nil) != g, sum(nil) == h)
}
