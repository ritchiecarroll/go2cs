package main

// Go 1.25+ gives a non-escaping append a stack backing store of 32 bytes: at the first qualifying append of a
// variable in a function, when the slice must grow, is empty, and the new length fits K = 32 / element size, the
// slice takes the K-element buffer instead of growslice's capacity. It does so once per call: a used flag, set when
// the buffer is taken, keeps every later empty append on growslice. Each arm prints the capacities Go gives.

import "fmt"

// 4 4 4 4 8: the first append takes the 4-element buffer (int64, K = 4), the next three fit, the fifth grows.
//
//go:noinline
func loopCaps() {
	var x []int64
	for i := 0; i < 5; i++ {
		x = append(x, int64(i))
		fmt.Print(cap(x), " ")
	}
	fmt.Println()
}

// 4 4 4 then 1 2 4: after x = nil the buffer is already used, so the refill grows as growslice does.
//
//go:noinline
func resetCaps() {
	var x []int64
	for i := 0; i < 3; i++ {
		x = append(x, 1)
		fmt.Print(cap(x), " ")
	}
	x = nil
	for i := 0; i < 3; i++ {
		x = append(x, 2)
		fmt.Print(cap(x), " ")
	}
	fmt.Println()
}

//go:noinline
func keep(p *[]int64) int { return len(*p) }

// 1: a variable whose address is taken is not SSA-able, so its append is lowered in place and never sees the buffer.
//
//go:noinline
func addressTakenCap() {
	var x []int64
	x = append(x, 1)
	fmt.Println(cap(x), keep(&x))
}

// 1: append(x, s...) never takes the buffer.
//
//go:noinline
func spreadCap() {
	src := []int64{7}
	var x []int64
	x = append(x, src...)
	fmt.Println(cap(x))
}

// 32: a byte element gives K = 32 (growslice alone would give 8).
//
//go:noinline
func byteCap() {
	var b []byte
	b = append(b, 'a')
	fmt.Println(cap(b))
}

// 2 2: a string element (16 bytes: a pointer and a length) gives K = 2.
//
//go:noinline
func stringCaps() {
	var s []string
	s = append(s, "x")
	fmt.Print(cap(s), " ")
	s = append(s, "y")
	fmt.Println(cap(s))
}

// 1 2: a slice element (24 bytes) gives K = 1, so the second append grows.
//
//go:noinline
func sliceCaps() {
	var rows [][]byte
	rows = append(rows, nil)
	fmt.Print(cap(rows), " ")
	rows = append(rows, nil)
	fmt.Println(cap(rows))
}

// 4, then 6: four elements at once fit K exactly; five do not, and growslice sizes them.
//
//go:noinline
func boundaryCaps() {
	var exact []int64
	exact = append(exact, 1, 2, 3, 4)
	var over []int64
	over = append(over, 1, 2, 3, 4, 5)
	fmt.Println(cap(exact), cap(over))
}

// 4: a variable declared inside a closure takes the buffer in the closure's own frame, per call.
//
//go:noinline
func closureLocalCaps() {
	f := func() int {
		var w []int64
		w = append(w, 1)
		return cap(w)
	}
	fmt.Println(run(f), run(f))
}

//go:noinline
func run(f func() int) int { return f() }

// 1: an outer variable appended inside a closure is captured by reference, so its append is in place and no buffer
// is used, in either frame.
//
//go:noinline
func closureOuterCap() {
	var w []int64
	f := func() int {
		w = append(w, 1)
		return len(w)
	}
	run(f)
	fmt.Println(cap(w))
}

// 4 4: two variables named buf in sibling scopes each take a buffer of their own.
//
//go:noinline
func siblingScopeCaps(n int) {
	switch n {
	case 0:
		var buf []int64
		buf = append(buf, 1)
		fmt.Print(cap(buf), " ")
	}
	switch n {
	case 0:
		var buf []int64
		buf = append(buf, 2)
		fmt.Println(cap(buf))
	}
}

// 4 4: a closure's own buf takes its buffer as an outer buf in a sibling block takes its own.
//
//go:noinline
func literalNameCaps() {
	{
		var buf []int64
		buf = append(buf, 1)
		fmt.Print(cap(buf), " ")
	}
	f := func() int {
		var buf []int64
		buf = append(buf, 1)
		return cap(buf)
	}
	fmt.Println(run(f))
}

// 2: a slice that is not empty when it must grow takes growslice's capacity.
//
//go:noinline
func nonEmptyCap() {
	x := make([]int64, 1)
	x = append(x, 2)
	fmt.Println(cap(x))
}

func main() {
	loopCaps()
	resetCaps()
	addressTakenCap()
	spreadCap()
	byteCap()
	stringCaps()
	sliceCaps()
	boundaryCaps()
	nonEmptyCap()
	closureLocalCaps()
	closureOuterCap()
	siblingScopeCaps(0)
	literalNameCaps()
}
