// Package iterators declares generic types constrained by a type of another package whose import path has a
// slash (GenericHolderForeignConstraint/text). A library keeps its method holders, and each is named by the
// type alone, so the two holders are distinct.
package iterators

import "GenericHolderForeignConstraint/text"

type Iterator[T text.Text] struct {
	data T
	pos  int
}

func (it *Iterator[T]) SetText(data T) {
	it.data = data
	it.pos = 0
}

func (it *Iterator[T]) Next() bool {
	if it.pos >= len(it.data) {
		return false
	}

	it.pos++

	return true
}

func (it Iterator[T]) Pos() int { return it.pos }

type Window[K comparable, T text.Text] struct {
	key  K
	data T
}

// NewWindow returns a Window over data, keyed by key.
func NewWindow[K comparable, T text.Text](key K, data T) Window[K, T] {
	return Window[K, T]{key: key, data: data}
}

func (w Window[K, T]) Len() int { return len(w.data) }

func (w *Window[K, T]) Key() K { return w.key }
