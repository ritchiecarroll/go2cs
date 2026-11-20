// Guards Go's value semantics when copy and append move elements that are arrays, or structs holding an array:
// each moved element is a copy of the source's VALUE, so a later write to either side is never seen by the other.
//
// An array element is a struct over its own storage in C#, so a bulk move of the elements (a span copy) carried
// that storage along and left both slices holding one inner array: `copy(dst, src)` followed by `src[0][1] = 9`
// changed dst too, and so did `src[0] = [2]int{}`, which writes the element in place. copy writes INTO the
// destination's own elements, so a slice taken from one of them beforehand sees the copied values, as in Go.

package main

import "fmt"

type pair struct {
	a [2]int
	n int
}

type grid [][2]int

func main() {
	copyArms()
	overlapArms()
	appendArms()
	structArms()
	otherArms()
}

func copyArms() {
	src := [][2]int{{1, 2}, {3, 4}}
	dst := make([][2]int, 2)
	copy(dst, src)
	src[0] = [2]int{}
	src[1][1] = 9
	fmt.Println("copy, then writes to src:", dst, src)

	dst[0][0] = 7
	fmt.Println("copy, then a write to dst:", dst, src)

	// A slice of dst's element, taken before the copy, names dst's own storage.
	in := make([][2]int, 1)
	q := in[0][:]
	copy(in, [][2]int{{5, 6}})
	fmt.Println("slice of an element taken before the copy:", q, in)
	in[0][1] = 8
	fmt.Println("...and after a write through the element:", q, in)

	// Nested arrays copy all the way down.
	n1 := [][2][2]int{{{1, 2}, {3, 4}}}
	n2 := make([][2][2]int, 1)
	copy(n2, n1)
	n1[0][1][0] = 9
	fmt.Println("nested arrays:", n2, n1)

	// The source an array's slice.
	var arr [2][2]int
	arr[0] = [2]int{1, 1}
	from := make([][2]int, 2)
	copy(from, arr[:])
	arr[0][0] = 5
	fmt.Println("from an array:", from, arr)

	// A named slice type.
	g := grid{{1, 2}}
	h := make(grid, 1)
	copy(h, g)
	g[0][0] = 9
	fmt.Println("named slice:", h, g)
}

func overlapArms() {
	up := [][2]int{{1, 2}, {3, 4}, {5, 6}}
	copy(up[1:], up)
	fmt.Println("overlap, destination above:", up)
	up[0][0] = 9
	up[1][1] = 8
	fmt.Println("...then writes to two elements:", up)

	down := [][2]int{{1, 2}, {3, 4}, {5, 6}}
	copy(down, down[1:])
	fmt.Println("overlap, destination below:", down)
	down[2][0] = 9
	down[1][1] = 8
	fmt.Println("...then writes to two elements:", down)
}

func appendArms() {
	src := [][2]int{{3, 4}, {5, 6}}
	cl := append([][2]int(nil), src...)
	src[0] = [2]int{}
	src[1][0] = 9
	fmt.Println("append to nil, then writes to src:", cl, src)

	// Growth moves the old elements into new storage.
	old := [][2]int{{1, 2}}
	grown := append(old, [2]int{3, 4})
	old[0][1] = 9
	fmt.Println("growth, then a write to the old slice:", grown, old)
	grown[0][0] = 7
	fmt.Println("...and a write to the grown one:", grown, old)

	// Growth by a spread.
	more := [][2]int{{5, 6}}
	grown2 := append(old, more...)
	more[0][1] = 0
	old[0][0] = 0
	fmt.Println("growth by a spread, then writes to both sources:", grown2, old, more)

	// Within capacity: the spread's values land in the shared backing.
	base := make([][2]int, 1, 4)
	add := [][2]int{{1, 2}}
	in := append(base, add...)
	add[0][0] = 9
	fmt.Println("in capacity, then a write to the spread:", in, add)

	// A variadic pack spread into append, and copied from.
	packed := addAll(nil, src...)
	src[0][1] = 5
	fmt.Println("variadic pack appended:", packed, src)

	filled := make([][2]int, 2)
	fill(filled, src...)
	src[1][1] = 4
	fmt.Println("variadic pack copied:", filled, src)

	// One element appended (the converter's own copy) stays right.
	one := [2]int{1, 2}
	single := append([][2]int{}, one)
	one[0] = 9
	fmt.Println("one element:", single, one)
}

func addAll(dst [][2]int, more ...[2]int) [][2]int {
	return append(dst, more...)
}

func fill(dst [][2]int, v ...[2]int) {
	copy(dst, v)
}

func structArms() {
	src := []pair{{[2]int{1, 2}, 1}, {[2]int{3, 4}, 2}}
	dst := make([]pair, 2)
	copy(dst, src)
	src[0].a[1] = 9
	src[1] = pair{}
	fmt.Println("structs copied, then writes to src:", dst, src)

	cl := append([]pair(nil), dst...)
	dst[0].a[0] = 7
	fmt.Println("structs appended to nil, then a write to the source:", cl, dst)

	old := []pair{{[2]int{1, 2}, 1}}
	grown := append(old, pair{[2]int{3, 4}, 2})
	old[0].a[1] = 9
	fmt.Println("structs grown, then a write to the old slice:", grown, old)
}

func otherArms() {
	// Conversion of a slice to an array copies the elements.
	s := [][2]int{{1, 2}, {3, 4}}
	a := [2][2]int(s)
	s[0][0] = 9
	fmt.Println("slice to array:", a, s)

	// A pointer to a slice's element names the slice's storage after a copy into it.
	dst := make([][2]int, 1)
	p := &dst[0]
	copy(dst, [][2]int{{4, 5}})
	p[1] = 6
	fmt.Println("element pointer:", *p, dst)
}
