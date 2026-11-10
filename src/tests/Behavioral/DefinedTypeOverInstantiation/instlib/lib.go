// Package instlib declares a generic struct and a defined type over its instantiation, for the importing
// package's cross-package reads.
package instlib

type Pair[T any] struct {
	A T
	B int
}

func (p Pair[T]) First() T { return p.A }

type Q Pair[int]

// Alias names an instantiation, and Alias2 names it through a second alias. Neither is a type of its own: a
// type defined over one is defined over Pair[string].
type Alias = Pair[string]

type Alias2 = Alias

func Same() ([]Q, *Q) {
	q := Q{A: 1, B: 2}
	return []Q{q, {A: 3}}, &q
}
