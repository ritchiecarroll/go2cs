package main

// Go compares an empty interface against a pointer by POINTER IDENTITY, whatever path the
// pointer took into the interface. A pointer that first passed through a NON-EMPTY interface
// (here `error`) and was then widened to `any` is still the same *T: `a == p` is true.
//
// context's Value(key any) has this shape: `key == &cancelCtxKey`, an empty interface against
// a raw pointer. The converter routed non-empty interfaces through AreEqual (which unwraps the
// generated pointer adapter) but left the empty-interface case on C# `==`, a reference compare
// that sees the adapter object rather than the box it wraps.

import "fmt"

type node struct {
	id int
}

func (n *node) Error() string { return fmt.Sprintf("node %d", n.id) }

func main() {
	p := &node{id: 1}
	q := &node{id: 2}

	// The pointer goes straight into `any`.
	var direct any = p
	fmt.Println("direct == p:", direct == p)
	fmt.Println("direct != p:", direct != p)
	fmt.Println("direct == q:", direct == q)

	// The pointer goes through `error` first, then into `any`.
	var err error = p
	var widened any = err
	fmt.Println("widened == p:", widened == p)
	fmt.Println("widened != p:", widened != p)
	fmt.Println("p == widened:", p == widened)
	fmt.Println("widened == q:", widened == q)

	// The same comparison as a map-free lookup key, the context.Value shape.
	fmt.Println("lookup:", lookup(err, p))
	fmt.Println("lookup other:", lookup(err, q))
}

func lookup(key any, want *node) string {
	if key == want {
		return "found"
	}

	return "missing"
}
