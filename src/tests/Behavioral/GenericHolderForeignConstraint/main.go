package main

import (
	"fmt"

	"GenericHolderForeignConstraint/text"
)

// Iterator and Window are generic types constrained by a type of another package whose import path has a
// slash. Each takes a method holder named by the type alone, so the two holders are distinct.
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

func (w Window[K, T]) Len() int { return len(w.data) }

func (w *Window[K, T]) Key() K { return w.key }

func main() {
	it := &Iterator[string]{}
	it.SetText("gopher")

	steps := 0
	for it.Next() {
		steps++
	}

	fmt.Println("string steps:", steps, "pos:", it.Pos())

	bytes := Iterator[[]byte]{}
	bytes.SetText([]byte("go"))
	bytes.Next()
	fmt.Println("bytes pos:", bytes.Pos())

	w := Window[int, []byte]{key: 7, data: []byte("abc")}
	fmt.Println("window:", w.Key(), w.Len())

	named := Window[string, string]{key: "k", data: "hello"}
	fmt.Println("named window:", named.Key(), named.Len())
}
