// A []T argument under a NON-self-referential method-set constraint, where T is ALSO reached outside the slice and is
// instantiated with a pointer (TS readiness class B, tsc's tsbaseline.GetErrorBaseline[T diagnosticwriter.Diagnostic]
// with a compareDiagnostics func(a, b T) int). Go instantiates T as the pointer: the callback receives the caller's
// pointers, a T result IS the caller's pointer, and the caller's slice is read, never written.
package main

import (
	"fmt"
	"slices"
	"strings"
)

type Named interface{ Name() string }

type item struct{ n string }

func (i *item) Name() string { return i.n }

func compareItems(a, b *item) int { return strings.Compare(a.n, b.n) }

func wrap(ns ...string) []*item {
	out := make([]*item, len(ns))
	for i, n := range ns {
		out[i] = &item{n}
	}
	return out
}

// tsc's two levels: the outer callee hands T on under the SAME constraint; the inner one clones and sorts.
func join[T Named](xs []T, cmp func(a, b T) int) string {
	return strings.Join(sortedNames(xs, cmp), ",")
}

func sortedNames[T Named](xs []T, cmp func(a, b T) int) []string {
	ys := slices.Clone(xs)
	slices.SortFunc(ys, cmp)
	names := make([]string, len(ys))
	for i, y := range ys {
		names[i] = y.Name()
	}
	return names
}

// T is a direct result beside the []T parameter.
func find[T Named](xs []T, name string) (T, bool) {
	for _, x := range xs {
		if x.Name() == name {
			return x, true
		}
	}
	var zero T
	return zero, false
}

// T is a func result beside the []T parameter, and a direct result.
func firstOr[T Named](xs []T, fallback func() T) T {
	if len(xs) == 0 {
		return fallback()
	}
	return xs[0]
}

// T is a direct parameter beside the []T parameter.
func countLike[T Named](xs []T, like T) int {
	n := 0
	for _, x := range xs {
		if x.Name() == like.Name() {
			n++
		}
	}
	return n
}

func main() {
	items := wrap("b", "a")

	// The callback receives the caller's pointers, and the caller's slice keeps its order.
	fmt.Println("join:", join(items, compareItems), len(items), items[0].n)

	// A func literal as the callback.
	fmt.Println("literal:", join(items, func(a, b *item) int { return strings.Compare(b.n, a.n) }))

	// A T result is the caller's pointer: same identity, and a write through it is seen through the slice.
	got, ok := find(items, "a")
	fmt.Println("find:", ok, got == items[1])
	got.n = "a2"
	fmt.Println("write-through:", items[1].n)
	missing, ok := find(items, "zz")
	fmt.Println("missing:", ok, missing == nil)

	// A func result beside the slice: the fallback's own pointer comes back.
	spare := &item{"spare"}
	fmt.Println("firstOr:", firstOr(items, func() *item { return spare }) == items[0], firstOr(nil, func() *item { return spare }) == spare)

	// A direct parameter beside the slice.
	fmt.Println("countLike:", countLike(items, &item{"b"}), countLike(wrap("b", "c", "b"), items[0]))
}
