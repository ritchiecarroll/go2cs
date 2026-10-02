package main

import (
	"fmt"
	"sort"
)

type void struct{}

// Set is a generic defined map over an unexported named empty struct, the hashset module's shape.
type Set[T comparable] map[T]void

// Bag is the same shape over struct{}.
type Bag[T comparable] map[T]struct{}

// Index is a generic defined map with two type parameters.
type Index[K comparable, V any] map[K]V

// Pipe is a generic defined channel.
type Pipe[T any] chan T

// Stack is a generic defined slice; its generated length constructor shared the map's defect.
type Stack[T any] []T

func NewSet[T comparable](items ...T) Set[T] {
	s := make(Set[T], len(items))

	for _, item := range items {
		s.Add(item)
	}

	return s
}

func (s Set[T]) Add(item T) bool {
	if _, ok := s[item]; ok {
		return false
	}

	s[item] = void{}
	return true
}

func (s Set[T]) Has(item T) bool {
	_, ok := s[item]
	return ok
}

func (s *Set[T]) Reset() {
	*s = make(Set[T])
}

func (b Bag[T]) Put(item T) {
	b[item] = struct{}{}
}

func (b *Bag[T]) Drop(item T) {
	delete(*b, item)
}

func (ix Index[K, V]) Get(key K) (V, bool) {
	value, ok := ix[key]
	return value, ok
}

func (ix *Index[K, V]) Put(key K, value V) {
	if *ix == nil {
		*ix = make(Index[K, V])
	}

	(*ix)[key] = value
}

func (p Pipe[T]) Send(value T) {
	p <- value
}

func (p *Pipe[T]) Drain() []T {
	close(*p)

	var out []T

	for value := range *p {
		out = append(out, value)
	}

	return out
}

func (s *Stack[T]) Push(value T) {
	*s = append(*s, value)
}

func (s Stack[T]) Peek() T {
	return s[len(s)-1]
}

func (s Set[T]) Keys() []T {
	keys := make([]T, 0, len(s))

	for key := range s {
		keys = append(keys, key)
	}

	return keys
}

func (b Bag[T]) Keys() []T {
	keys := make([]T, 0, len(b))

	for key := range b {
		keys = append(keys, key)
	}

	return keys
}

func (ix Index[K, V]) Keys() []K {
	keys := make([]K, 0, len(ix))

	for key := range ix {
		keys = append(keys, key)
	}

	return keys
}

func sortedInts(values []int) []int {
	sort.Ints(values)
	return values
}

func sortedStrings(values []string) []string {
	sort.Strings(values)
	return values
}

func main() {
	ints := NewSet(3, 1, 2, 3)
	fmt.Println("Set[int]:", len(ints), ints.Has(2), ints.Has(9), ints.Add(4), ints.Add(4), sortedInts(ints.Keys()))
	ints.Reset()
	fmt.Println("Set[int] after Reset:", len(ints), ints == nil)

	words := NewSet("go", "cs")
	fmt.Println("Set[string]:", len(words), words.Has("go"), sortedStrings(words.Keys()))

	bag := make(Bag[int], 2)
	bag.Put(7)
	bag.Put(8)
	bag.Drop(7)
	fmt.Println("Bag[int]:", len(bag), sortedInts(bag.Keys()))

	var names Bag[string] = Bag[string]{"a": {}}
	names.Put("b")
	fmt.Println("Bag[string]:", sortedStrings(names.Keys()))

	var ages Index[string, int]
	ages.Put("x", 1)
	ages.Put("y", 2)
	age, ok := ages.Get("y")
	_, missing := ages.Get("z")
	fmt.Println("Index[string, int]:", age, ok, missing, sortedStrings(ages.Keys()))

	labels := Index[int, string]{1: "one"}
	labels.Put(2, "two")
	label, _ := labels.Get(2)
	fmt.Println("Index[int, string]:", label, sortedInts(labels.Keys()))

	pipe := make(Pipe[int], 3)
	pipe.Send(1)
	pipe.Send(2)
	fmt.Println("Pipe[int]:", cap(pipe), len(pipe), pipe.Drain())

	texts := make(Pipe[string], 1)
	texts.Send("hi")
	fmt.Println("Pipe[string]:", texts.Drain())

	stack := make(Stack[int], 0, 4)
	stack.Push(5)
	stack.Push(6)
	fmt.Println("Stack[int]:", len(stack), cap(stack), stack.Peek())

	var tags Stack[string]
	tags.Push("t")
	fmt.Println("Stack[string]:", tags.Peek(), len(tags))
}
