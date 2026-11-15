package main

import (
	"fmt"
	"unsafe"
)

func main() {
	a := [3]int{10, 20, 30}
	p := unsafe.Pointer(&a)
	sum := 0
	// (*[3]int)(p) converts an unsafe.Pointer to *[3]int — renders as a C# cast (ж<...>)(uintptr)(...).
	for i, x := range (*[3]int)(p) {
		sum += i + x
	}
	fmt.Println(sum)

	// The same conversion with an element that is itself an array (crypto/internal/fips140/nistec ranges
	// (*[N][8]byte)(ptr)). An array is a managed element, so no door takes it: the conversion keeps the
	// pointer route, parenthesized before the dereference.
	rows := [2][2]int{{1, 2}, {3, 4}}
	q := unsafe.Pointer(&rows)
	total := 0
	for i, row := range (*[2][2]int)(q) {
		total += i + row[0]*10 + row[1]
	}
	fmt.Println(total)
}
