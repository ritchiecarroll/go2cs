package main

// An exported function whose parameter is an unexported func type, in a library package and in main itself.

import (
	"fmt"

	"ExportedFuncOfUnexportedFuncType/lib"
)

type cmpFunc func(a, b int) int

// Pick takes a variadic of main's unexported func type and returns the first non-zero comparison.
func Pick(a, b int, cmps ...cmpFunc) int {
	for _, c := range cmps {
		if r := c(a, b); r != 0 {
			return r
		}
	}

	return 0
}

func main() {
	byValue := func(a, b int) bool { return a < b }
	byParity := func(a, b int) bool { return a%2 == 0 && b%2 != 0 }

	s := lib.OrderedBy(byParity, byValue)
	fmt.Println(s.Less(2, 3), s.Less(3, 2), s.Less(4, 6), s.Less(6, 4))
	fmt.Println(lib.Apply(byValue, 1, 2), lib.Apply(byValue, 2, 1))

	h := lib.Holder{Less: lib.Reverse(byValue)}
	fmt.Println(h.Less(1, 2), h.Less(2, 1))

	same := func(a, b int) int { return 0 }
	diff := func(a, b int) int { return a - b }
	fmt.Println(Pick(5, 3, same, diff), Pick(5, 5, same))
}
