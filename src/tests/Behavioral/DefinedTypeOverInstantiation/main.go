// DefinedTypeOverInstantiation guards a DEFINED type written over an INSTANTIATED generic struct, plain
// (`type Q pair[int]`) and generic (`type P[T any] pair[T]`), local and cross-package, with one and two type
// arguments (GO2CS2004's other half; BOARD, H, TS readiness C11). The defined type has the struct's fields and
// none of its methods; a conversion back to the instantiation reaches them.
package main

import (
	"fmt"

	lib "go/instlib"
)

type pair[T any] struct {
	A T
	B int
}

func (p pair[T]) First() T { return p.A }

type duo[K comparable, V any] struct {
	Key K
	Val V
}

// A plain defined type over a local instantiation.
type Q pair[int]

// A generic defined type over the generic struct, its parameter passed through.
type P[T any] pair[T]

// The same, with the parameter renamed and constrained.
type R[U comparable] pair[U]

// Over an instantiation of another package's generic struct.
type M lib.Pair[string]

// Over an instantiation with two type arguments.
type D duo[string, int]

// Over an instantiation of a generic DEFINED type, and a plain type over an instantiation of that.
type C[V any] P[V]

type E C[bool]

// A struct with the same fields, which every one of them converts to and from.
type S struct {
	A int
	B int
}

func main() {
	q := Q{A: 3, B: 4}
	q.B++
	qs := []Q{{A: 5}, {B: 6}}
	p := P[string]{A: "s", B: 1}
	pp := &p
	pp.B = 7
	r := R[int]{A: 9}
	m := M{A: "m", B: 2}
	d := D{Key: "k", Val: 8}
	lq, lp := lib.Same()
	lp.B = 10
	fmt.Println(q.A, q.B, qs[0].A, qs[1].B, p.A, p.B, r.A, m.A, m.B, d.Key, d.Val)
	fmt.Println(pair[int](q).First(), lib.Pair[string](m).First(), len(lq), lq[1].A, lp.A, lp.B)

	var z P[float64]
	fmt.Println(z.A, z.B, r == R[int]{A: 9})

	c := C[string]{A: "c", B: 11}
	var e E
	e.A = true
	e.B = c.B + 1
	fmt.Println(c.A, c.B, e.A, e.B, pair[bool](e).First(), pair[string](c).First(), P[string](c).A)

	back := C[string](pair[string]{A: "x"})
	eb := E(pair[bool]{B: 13})
	s := S(q)
	sp := S(P[int]{A: 14})
	fmt.Println(back.A, eb.B, s.A, s.B, sp.A, Q(s).B, P[int](s).A)
}
