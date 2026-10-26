// DefinedArrayChainSlice guards slicing a DEFINED array type, directly and through a pointer, at every depth of a
// definition chain: `type base [4]uint64`, `type one base`, `type two one` (Go 1.26 runtime's
// `type PallocBits pallocBits` over `type pallocBits pageBits`). Go's `want[:][2] = 0x3e` must write the array.
//
// Two defects met here. A chain two definitions deep is generated with go2cs-gen's array-VIEW template, which had
// no Range indexer or slice members, so `x[:]` did not compile (CS1503 x9 in runtime's mpallocbits_test.go).
// And a pointer's slice went through `(~p).Value`, a COPY of the wrapper whose lazy backing then took the write:
// every write through `p[:][i]` was lost, at depth 0 too. Every value below is read back after its write.
package main

import "fmt"

type base [4]uint64
type one base
type two one

func main() {
	a := new(base)
	a[:][1] = 5
	b := new(one)
	b[:][2] = 6
	c := new(two)
	c[:][3] = 7
	var d two
	d[:][0] = 8
	fmt.Println("full-slice writes:", a[1], b[2], c[3], d[0])

	s := c[:]
	s[0] = 9
	t := c[1:3]
	t[0] = 10
	n := 2
	u := a[n:]
	u[1] = 11
	fmt.Println("shared slices:", c[0], c[1], a[3], len(s), len(t), len(u))

	w := c[:2:3]
	w[1] = 12
	x := d[1:2:4]
	x[0] = 13
	fmt.Println("three-index:", c[1], d[1], len(w), cap(w), len(x), cap(x))
}
