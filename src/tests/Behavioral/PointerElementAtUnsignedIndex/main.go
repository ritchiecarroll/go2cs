package main

import "fmt"

// The address of an element reached THROUGH A POINTER -- `&p[i]` with p a *[N]T, and `&ps.a[i]` with
// ps a *S whose field a is an array -- keeps an UNSIGNED index at its full value, as Go's does: a value
// past the length panics with the unsigned value and the length (goPanicIndexU). This route renders as
// the box's `.at<T>(i)` / `.at(field, i)`, which narrowed the index to nint first, so a uint64 at or
// above 2^63 read negative and reported `[-N]` without the length. Every case recovers its panic and
// prints it; the in-range writes are the guards that the route still addresses the right element.

type S struct {
	a [3]int
}

func try(name string, f func() *int) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: %v\n", name, r)
		}
	}()
	*f() = 7
	fmt.Printf("%s: wrote\n", name)
}

func main() {
	var arr [3]int
	p := &arr
	ps := &S{}

	var u64 uint64 = 1<<64 - 1
	var u uint = 1<<63 + 2
	var up uintptr = 1<<63 + 3
	var u32 uint32 = 1<<32 - 1
	var ok uint64 = 2

	try("pointer-to-array uint64 max", func() *int { return &p[u64] })
	try("pointer-to-array uint past 2^63", func() *int { return &p[u] })
	try("pointer-to-array uintptr past 2^63", func() *int { return &p[up] })
	try("pointer-to-array uint32 max", func() *int { return &p[u32] })
	try("pointer-to-array in range", func() *int { return &p[ok] })
	try("field array uint64 max", func() *int { return &ps.a[u64] })
	try("field array uint past 2^63", func() *int { return &ps.a[u] })
	try("field array in range", func() *int { return &ps.a[ok] })

	fmt.Println(arr, ps.a)
}
