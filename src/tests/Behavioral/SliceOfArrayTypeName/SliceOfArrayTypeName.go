// An ARRAY used as a container's ELEMENT must keep its length in the Go type name. A top-level
// array already did; a slice's element did not -- `[][6]uint8` rendered `[][]uint8` -- so two
// distinct Go types collapsed onto one string. That is invisible until something PRINTS the name:
// reflect's own TestDeepEqualAllocs names its subtests `ValueOf(x).Type().String()`, and the
// collapse made `t.Run` dedup the second to `#01`, so the two sides ran different-named subtests
// and the comparison reported an EMPTY verdict beside a name Go never produced.
//
// The nested rows are the point: a fix applied at one path level under-reaches, and only
// `[][2][3]int` and the map rows say whether the dims travel all the way down.
package main

import (
	"fmt"
	"reflect"
)

type Grid [3]int

func show(label string, v any) {
	fmt.Printf("%-16s %%T=%-18T String()=%s\n", label, v, reflect.TypeOf(v).String())
}

func main() {
	show("[6]uint8", [6]uint8{})
	show("[][6]uint8", [][6]uint8{{}})
	show("[][3]int", [][3]int{{}})
	show("[2][3]int", [2][3]int{})
	show("[][2][3]int", [][2][3]int{{{1, 2, 3}, {4, 5, 6}}}) // populated: the zero nested element is a routed emission gap
	show("map[[2]int][]int", map[[2]int][]int{{}: nil}) // present entry: an EMPTY map is increment B's stated boundary
	show("[]Grid", []Grid{{}})
	show("[]*[4]byte", []*[4]byte{&[4]byte{}}) // explicit &: the elided form trips a converter gap, routed

	// Elem() is the type-side question the value-side fix cannot answer on its own.
	fmt.Printf("slice-of-array Elem().String()=%s Len()=%d\n",
		reflect.TypeOf([][6]uint8{{}}).Elem().String(), // present element: the empty literal is the stated boundary
		reflect.TypeOf([][6]uint8{{}}).Elem().Len())
}
