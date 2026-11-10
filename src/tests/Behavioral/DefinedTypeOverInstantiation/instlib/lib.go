// Package instlib declares a generic struct and a defined type over its instantiation, for the importing
// package's cross-package reads.
package instlib

type Pair[T any] struct {
	A T
	B int
}

func (p Pair[T]) First() T { return p.A }

type Q Pair[int]

func Same() ([]Q, *Q) {
	q := Q{A: 1, B: 2}
	return []Q{q, {A: 3}}, &q
}
