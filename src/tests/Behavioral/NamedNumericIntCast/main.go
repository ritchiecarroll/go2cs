package main

import "fmt"

// A NAMED numeric type ([GoType] struct over a basic) used where the converter inserts a C# `(int)`
// cast — a slice bound (`a[lo:hi]`) or a shift count (`1 << sh`) — takes that one cast: the
// generated struct declares an explicit operator to every basic numeric, so `(int)(lo)` binds, the
// form a plain basic operand takes. Mirrors runtime's `chunkIdx` slice bounds (`summary[sc+1:ec]`)
// and `statDep` shift counts (`1 << (d % 64)`).

type idx uint // underlying uint -> C# nuint

type cnt int // underlying int -> C# nint

func main() {
	a := []int{10, 20, 30, 40, 50}

	var lo idx = 1
	var hi idx = 4
	fmt.Println(a[lo:hi]) // [20 30 40] — named-numeric slice bounds

	var sh cnt = 3
	fmt.Println(uint64(1) << sh) // 8 — named-numeric shift count

	// Arithmetic on the named type as a bound, too.
	fmt.Println(a[lo+1 : hi]) // [30 40]
}
