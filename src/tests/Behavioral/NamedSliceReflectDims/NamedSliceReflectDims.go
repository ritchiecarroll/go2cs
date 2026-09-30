package main

import (
	"fmt"
	"reflect"
)

// reflect must read a NAMED slice's element array length even when the slice is EMPTY, where there is no
// element to observe: `type UUIDs []UUID` over `type UUID [16]byte` (google/uuid), and `type Rows [][3]int`
// over an UNNAMED array. Each named value here is converted from an unnamed creation, so the length can only
// come from the dims recorded on the backing the named slice shares.

type UUID [16]byte

type UUIDs []UUID

type Rows [][3]int

type Names []string

func main() {
	var u UUIDs = []UUID{}
	var r Rows = [][3]int{}
	fmt.Println("named empty:", reflect.TypeOf(u).Elem().Len(), reflect.TypeOf(r).Elem().Len())

	// A named value over an EMPTY WINDOW of a populated backing: nothing to observe, the record still holds.
	rows := [][3]int{{1, 2, 3}}
	var window Rows = rows[:0]
	fmt.Println("named window:", reflect.TypeOf(window).Elem().Len())
	var full Rows = rows

	// CONTROLS: the unnamed values themselves, and a non-empty named value (answered by observation).
	fmt.Println("unnamed empty:", reflect.TypeOf([]UUID{}).Elem().Len(), reflect.TypeOf([][3]int{}).Elem().Len())
	fmt.Println("named non-empty:", reflect.TypeOf(full).Elem().Len())

	// A named slice of a NON-array element takes no part: its element kind is unchanged.
	fmt.Println("named non-array:", reflect.TypeOf(Names{}).Elem().Kind())
}
