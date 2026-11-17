// EmbeddedArrayCopy copies struct values whose only array is reached through an EMBEDDED struct, and then writes to
// the copy.
//
// Go copies the whole value: after `c := e` a write to `c.V` (the embedded Inner's array, promoted) does not reach
// `e`. In C# an embed is an inline field and the array is a golib `array<T>` over a shared backing store, so the
// struct is stamped to clone its embed and every copy site clones it. Each line prints the source after the write,
// then the copy.
package main

import (
	"fmt"

	"EmbeddedArrayCopy/lib"
	"EmbeddedArrayCopy/other"
)

type emb struct {
	lib.Inner
	N int
}

type emb2 struct {
	lib.Inner
	N int
}

// pair is an array type embedded by name: its field is named after the type.
type pair [2]int

type tagged struct {
	pair
	Tag string
}

// outer reaches the array two embeds down.
type outer struct {
	lib.Emb
}

func fresh() lib.Emb {
	return lib.Emb{Inner: lib.Inner{V: [2]int{1, 2}}, N: 1}
}

func bump(e emb) int {
	e.V[0] = 100
	return e.V[0]
}

func keep(e emb) emb {
	return e
}

func main() {
	le := fresh()

	// Assignment of existing storage.
	c := le
	c.V[0], c.N = 9, 9
	fmt.Println("assign:", le.V, le.N, c.V, c.N)

	// The four conversion routes.
	e := emb(le)
	e.V[0], e.N = 8, 8
	fmt.Println("local from foreign:", le.V, le.N, e.V, e.N)

	eb := lib.Emb(e)
	eb.V[1], eb.N = 7, 7
	fmt.Println("foreign from local:", e.V, e.N, eb.V, eb.N)

	e2 := emb2(e)
	e2.Inner.V[0], e2.N = 6, 6
	fmt.Println("local from local:", e.V, e.N, e2.V, e2.N)

	oe := other.Emb(le)
	oe.V[1], oe.N = 5, 5
	fmt.Println("foreign from foreign:", le.V, le.N, oe.V, oe.N)

	// A parameter is the callee's own copy; a returned value is the caller's.
	got := bump(e)
	fmt.Println("parameter:", e.V, got)

	k := keep(e)
	k.V[1] = 44
	fmt.Println("return:", e.V, k.V)

	// A range value and an element copied out of a slice.
	list := []lib.Emb{le, le}
	for _, x := range list {
		x.V[0] = 33
	}
	first := list[0]
	first.V[1] = 22
	fmt.Println("range and element:", list[0].V, list[1].V, first.V)

	// An embedded array type, and an array two embeds down.
	t := tagged{pair: pair{1, 2}, Tag: "t"}
	t2 := t
	t2.pair[0] = 11
	fmt.Println("embedded array:", t.pair, t2.pair)

	o := outer{Emb: le}
	o2 := o
	o2.V[0] = 77
	fmt.Println("two levels:", o.V, o2.V, le.V)
}
