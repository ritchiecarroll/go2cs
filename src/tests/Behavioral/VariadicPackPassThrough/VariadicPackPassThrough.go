package main

import "fmt"

// A variadic pack that is only READ, COPIED FROM or FORWARDED is converted as a stack view of the
// incoming pack (REC-C, docs/phase4/DESIGN-slice-idiom-allocations.md §A) instead of a heap copy.
// The forwarded case is also a semantics case: Go's `f(xs...)` passes the slice itself, so a callee
// that writes an element writes the CALLER's storage. A converted pack copy used to hide that write.

func bump(xs ...int) {
	for i := range xs {
		xs[i] += 10
	}
}

// forward only passes its pack on: the view reaches bump, and bump's writes reach the caller.
func forward(xs ...int) {
	bump(xs...)
}

func sum(xs ...int) int {
	t := 0
	for _, x := range xs {
		t += x
	}
	return t
}

// gather appends FROM its pack: append copies the elements in, so nothing retains the view.
func gather(dst []int, xs ...int) []int {
	return append(dst, xs...)
}

// fill copies FROM its pack.
func fill(dst []int, xs ...int) int {
	return copy(dst, xs)
}

// describe forwards into a variadic of another element type's library call.
func describe(format string, args ...any) string {
	return fmt.Sprintf(format, args...)
}

// Refused shapes keep the heap slice: appending INTO the pack, writing it as copy's destination,
// returning it, and capturing it (a deferred closure included). Their calls below pass FRESH
// argument lists. A spread of a caller's slice into one hands over the caller's slice itself, so
// the callee writes and returns the caller's storage, as in Go; VariadicSpreadHeapAlias tests
// that.
func into(xs ...int) []int { return append(xs, 4) }

func copyInto(xs ...int) int { return copy(xs, []int{7, 7}) }

func keep(xs ...int) []int { return xs }

func capture(xs ...int) func() int { return func() int { return sum(xs...) } }

func deferred(out *int, xs ...int) {
	defer func() { *out = sum(xs...) }()
}

func main() {
	a := []int{1, 2, 3}
	forward(a...)
	fmt.Println("forward wrote through:", a)

	fmt.Println("sum:", sum(a...), sum(), sum(5))
	fmt.Println("gather:", gather([]int{0}, 1, 2), gather(nil, a...))

	d := make([]int, 2)
	n := fill(d, 8, 9, 10)
	fmt.Println("fill:", n, d)

	fmt.Println("describe:", describe("%d-%s-%v", 1, "two", 3.5))

	fmt.Println("into:", into(1, 2))
	fmt.Println("copyInto:", copyInto(1, 2, 3))
	k := keep(5, 6)
	k[0] = 100
	fmt.Println("keep:", k)
	f := capture(1, 2)
	fmt.Println("capture:", f())
	var total int
	deferred(&total, 1, 2)
	fmt.Println("deferred:", total)
}
