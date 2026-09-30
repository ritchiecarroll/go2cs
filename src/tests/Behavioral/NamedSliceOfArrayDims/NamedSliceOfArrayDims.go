package main

import "fmt"

// A NAMED slice whose element is an ARRAY -- google/uuid's `type UUIDs []UUID` over `type UUID [16]byte`.
// Its creation sites must keep the named type (so its methods resolve) while still recording the element
// array's length against the backing the named slice wraps.

type UUID [16]byte

type UUIDs []UUID

func (us UUIDs) Strings() []string {
	out := make([]string, len(us))
	for i, u := range us {
		out[i] = fmt.Sprintf("%x", u[:2])
	}
	return out
}

// A named slice of an UNNAMED array: the element type carries no length of its own.
type Rows [][3]int

func (r Rows) Sum() int {
	total := 0
	for _, row := range r {
		for _, v := range row {
			total += v
		}
	}
	return total
}

func main() {
	a, b := UUID{0xab, 0x01}, UUID{0xcd, 0x02}

	// The composite literal, with a method called straight on it.
	fmt.Println(UUIDs{a, b}.Strings())

	us := UUIDs{a, b}
	fmt.Println(len(us), us.Strings())

	// make, and a reslice of the named slice.
	made := make(UUIDs, 2, 4)
	fmt.Println(len(made), cap(made), made.Strings())
	fmt.Println(us[1:].Strings())

	rows := Rows{{1, 2, 3}, {4, 5, 6}}
	fmt.Println(rows.Sum(), make(Rows, 1).Sum())

	// EMPTY creations, the ones whose element length exists only as recorded dims.
	fmt.Println(len(UUIDs{}), len(make(UUIDs, 0)), UUIDs{}.Strings(), Rows{}.Sum(), len(make(Rows, 0, 2)))
}
