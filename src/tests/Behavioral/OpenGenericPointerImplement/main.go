package main

import "fmt"

// A generic type converted to an interface ONLY at its own type parameters, inside a generic function. The
// conversion is written once for every instantiation, so the converter records the type by its definition
// (`typeof(sliceIter<>)`) and go2cs-gen emits one generic adapter over it. Before that record existed the cast
// site named an adapter nothing generated (CS0246); gojq's NewIter[T] is the shape.
// Covered: a generic named slice cast by pointer (NewIter), a two-parameter generic struct cast by value
// (asNamer), a pointer cast reached only through a type-parameter chain (outer calls innerIter), and the same
// pair also cast at a CLOSED instantiation in main, so an open and a closed record of one pair meet.

type Iter interface{ Next() (any, bool) }

type sliceIter[T any] []T

func (it *sliceIter[T]) Next() (any, bool) {
	if len(*it) == 0 {
		return nil, false
	}
	v := (*it)[0]
	*it = (*it)[1:]
	return v, true
}

func NewIter[T any](values ...T) Iter {
	it := sliceIter[T](values)
	return &it
}

type Namer interface{ Name() string }

type pair[K comparable, V any] struct {
	k K
	v V
}

func (p pair[K, V]) Name() string { return fmt.Sprint(p.k, "=", p.v) }

func asNamer[K comparable, V any](k K, v V) Namer { return pair[K, V]{k, v} }

type counter[T any] struct {
	n    int
	last T
}

func (c *counter[T]) Next() (any, bool) {
	if c.n == 2 {
		return nil, false
	}
	c.n++
	return fmt.Sprint(c.last, "#", c.n), true
}

func innerIter[T any](v T) Iter { return &counter[T]{last: v} }

func outer[T any](v T) Iter { return innerIter(v) }

func drain(name string, it Iter) {
	fmt.Print(name, ":")
	for v, ok := it.Next(); ok; v, ok = it.Next() {
		fmt.Print(" ", v)
	}
	fmt.Println()
}

func main() {
	drain("ints", NewIter(1, 2, 3))
	drain("strings", NewIter("a", "b"))
	drain("empty", NewIter[float64]())
	fmt.Println(asNamer("x", 1).Name(), asNamer(2, 3.5).Name())
	drain("chain", outer(true))

	closed := sliceIter[int]{7, 8}
	var it Iter = &closed
	drain("closed", it)
}
