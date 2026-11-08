// An anonymous struct written as a TYPE ARGUMENT of a var declaration's type,
// `var seen Set[struct{...}]` (the TypeScript compiler's ls.ProvideDocumentSymbols). Go treats
// identical struct types as one type, so the var's type argument and the composite literal
// beside it must be the same type.
package main

import "fmt"

type Kind int32

type Set[T comparable] struct{ m map[T]struct{} }

func (s *Set[T]) AddIfAbsent(v T) bool {
	if _, ok := s.m[v]; ok {
		return false
	}

	if s.m == nil {
		s.m = map[T]struct{}{}
	}

	s.m[v] = struct{}{}
	return true
}

func (s *Set[T]) Len() int { return len(s.m) }

type Pair[K comparable, V any] struct {
	Key K
	Val V
}

// The repro: a zero-value var whose type argument is an anonymous struct.
func symbols() {
	var seen Set[struct {
		name string
		kind Kind
	}]

	for _, n := range []string{"a", "b", "a"} {
		key := struct {
			name string
			kind Kind
		}{n, 1}

		fmt.Println("add", n, seen.AddIfAbsent(key))
	}

	fmt.Println("symbols:", seen.Len())
}

// The same shape in a second function unifies with the first (one C# type for one Go type).
func again() {
	var other Set[struct {
		name string
		kind Kind
	}]

	key := struct {
		name string
		kind Kind
	}{"z", 2}

	other.AddIfAbsent(key)

	fmt.Println("again:", other.Len())
}

// Through a pointer, with an initializer, and as one of two type arguments.
func composed() {
	var ps *Set[struct{ id int }] = &Set[struct{ id int }]{}
	ps.AddIfAbsent(struct{ id int }{7})
	ps.AddIfAbsent(struct{ id int }{7})
	fmt.Println("pointer:", ps.Len())

	var p Pair[string, struct {
		x, y int
	}]

	p.Key = "origin"
	p.Val.x = 3
	fmt.Printf("pair: %s %d %d %+v\n", p.Key, p.Val.x, p.Val.y, p)
}

// A package-level var.
var registry Set[struct {
	pkg  string
	line int
}]

func main() {
	symbols()
	again()
	composed()

	entry := struct {
		pkg  string
		line int
	}{"main", 1}

	registry.AddIfAbsent(entry)

	fmt.Println("registry:", registry.Len())
}
