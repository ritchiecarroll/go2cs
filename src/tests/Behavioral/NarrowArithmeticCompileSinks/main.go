package main

import "fmt"

// Narrow-integer arithmetic (int8/uint8/int16/uint16) reaching a consumer that takes it at its Go
// type, where C#'s int-promoted result does not even compile: a map key, a range-over-int bound, a
// channel send, a map-literal value or key, a keyed array element, a parenthesized assignment and a
// map indexed through a type parameter's map core. The
// converter narrows the result at its own width, so each compiles and wraps as Go does.
// NarrowArithmeticSinks holds the consumers that compiled but read the unwrapped value.

var h int8 = 101

// genericKey indexes a map through a type parameter; the key h>>1 is int-typed in C# until narrowed.
func genericKey[M ~map[int8]string](m M) string { return m[h>>1] }

func main() {
	var u uint8 = 200
	var d uint8 = 250
	var w int16 = 30000

	mk := map[uint8]string{144: "wrapped"}
	fmt.Println("map key:", mk[u+u])

	n := 0
	for range d + 10 {
		n++
	}
	fmt.Println("range bound:", n)

	ch := make(chan uint8, 1)
	ch <- u + u
	fmt.Println("chan send:", <-ch)

	m := map[string]int16{"k": w + w}
	fmt.Println("map value:", m["k"])

	arr := [2]uint8{1: u * 2}
	fmt.Println("keyed array element:", arr)

	var x uint8
	x = (u + u)
	fmt.Println("paren assign:", x)

	mk2 := map[uint8]string{u + u: "wrapped key"}
	fmt.Println("map-literal key:", mk2[144])

	fmt.Println("type-parameter map key:", genericKey(map[int8]string{50: "ok"}))
}
