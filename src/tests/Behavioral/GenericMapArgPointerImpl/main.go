// GenericMapArgPointerImpl guards C31: a pointer cast to an interface of a generic
// instantiated over a MAP (`&f.Imports`, an `Expected[map[string]string]`) records
// `GoImplement<Expected<map<@string, @string>>, Field>(Pointer = true)`. The converter
// rendered that argument through its type-name renderer twice, once in the generic
// pass and once more in the pointer branch, and the second pass left a dangling
// separator: `map<@string, @string, >` (CS1031). Func types re-rendered the same way.
//
// The non-nested rows are the controls: `Expected[string]` (no map inside the generic)
// rendered correctly before the fix and must still.
package main

import "fmt"

type Field interface {
	IsPresent() bool
	Describe() string
}

type Expected[T any] struct {
	Value T
	Valid bool
}

func (e *Expected[T]) IsPresent() bool { return e.Valid }

func (e *Expected[T]) Describe() string { return fmt.Sprintf("%v", e.Value) }

// Pair carries TWO map arguments, each of which took its own dangling separator.
type Pair[K, V any] struct {
	Key K
	Val V
}

func (p *Pair[K, V]) IsPresent() bool { return true }

func (p *Pair[K, V]) Describe() string { return fmt.Sprintf("%v=%v", p.Key, p.Val) }

type Fields struct {
	Name    Expected[string]            // control: no map inside the generic
	Imports Expected[map[string]string] // the reported shape
	Mapping Pair[map[string]int, map[int]string]
}

func main() {
	f := Fields{}
	f.Name = Expected[string]{Value: "pkg", Valid: true}
	f.Imports = Expected[map[string]string]{Value: map[string]string{"a": "b"}, Valid: true}
	f.Mapping = Pair[map[string]int, map[int]string]{Key: map[string]int{"one": 1}, Val: map[int]string{2: "two"}}

	fields := []Field{&f.Name, &f.Imports, &f.Mapping}

	for i, x := range fields {
		fmt.Println(i, x.IsPresent(), x.Describe())
	}

	// A func type whose parameter, and one whose result, is the generic over a map.
	var count func(*Expected[map[string]string]) int = func(e *Expected[map[string]string]) int { return len(e.Value) }
	var make1 func() Expected[map[string]string] = func() Expected[map[string]string] {
		return Expected[map[string]string]{Value: map[string]string{"x": "y", "z": "w"}, Valid: true}
	}

	fmt.Println(count(&f.Imports), len(make1().Value))
}
