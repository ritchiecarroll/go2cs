// A parallel := of function values: func literals, method groups, method values and generic func
// instantiations, each declared on its own (CS8130 when emitted as one tuple deconstruction).

package main

import "fmt"

type box struct{ n int }

func (b box) get() int    { return b.n }
func (b *box) bump(d int) { b.n += d }

func double(v int) int { return v * 2 }
func triple(v int) int { return v * 3 }

func identity[T any](v T) T           { return v }
func square[T ~int | ~float64](v T) T { return v * v }

func main() {
	n := 3

	// Two func literals: the hashset test's shape.
	small, even := func(v int) bool { return v < n }, func(v int) bool { return v%2 == 0 }
	fmt.Println("func literals:", small(2), even(2), small(5), even(5))

	// Two method groups.
	d, t := double, triple
	fmt.Println("method groups:", d(2), t(2))

	// Two method values, one through a pointer receiver.
	b := &box{n: 2}
	get, bump := b.get, b.bump
	bump(3)
	fmt.Println("method values:", get(), b.n)

	// A func literal beside a pointer.
	show, p := func() int { return 1 }, b
	fmt.Println("func literal beside a pointer:", show(), p.n)

	// Two generic func instantiations.
	id, sq := identity[string], square[float64]
	fmt.Println("generic instantiations:", id("go"), sq(1.5))
}
