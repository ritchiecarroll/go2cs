// closurefixture: P3's three closure shapes (second read, finding 3), each forwarding its OWN type parameter into a
// selected declaration. The census must add each one through the closure, labelled by its shape.
package main

import "fmt"

type Script interface{ Text() string }

type SourceFile struct{ text string }

func (f *SourceFile) Text() string    { return f.text }
func (f *SourceFile) me() *SourceFile { return f }

type MappedSpan[T Script] struct {
	Script T
	Start  int
}

// Selected (an exported function with T in a result).
func FromLSPRange[T Script](script T, start int) []MappedSpan[T] {
	return []MappedSpan[T]{{script, start + len(script.Text())}}
}

type Named interface{ Name() string }

type Export struct{ name string }

func (e *Export) Name() string { return e.name }
func (e *Export) me() *Export  { return e }

// Selected (a generic named type whose own method calls Name).
type Index[T Named] struct{ entries []T }

func (idx *Index[T]) Find(name string) []T {
	var out []T
	for _, e := range idx.entries {
		if e.Name() == name {
			out = append(out, e)
		}
	}
	return out
}

// Shape 1: a generic named type's FIELD mentions a selected type with its own T. Its constraint is self-referential
// (me() T), so selection alone does not pick it; only the closure does.
type Cache[T interface {
	Named
	me() T
}] struct{ ix Index[T] }

// Shape 2: a METHOD of a generic type instantiates a selected function with the type's own T (constraint self-referential
// for the same reason).
type Holder[T interface {
	Script
	me() T
}] struct{ x T }

func (h Holder[T]) spans() []MappedSpan[T] { return FromLSPRange(h.x, 0) }

// Shape 3: type arguments that only MENTION an own parameter (named2[T], *named2[T]).
type named2[T any] struct{ v T }

func (n *named2[T]) Name() string { return fmt.Sprint(n.v) }

func wrapAll[T any](xs []T) Index[*named2[T]] {
	var idx Index[*named2[T]]
	for _, x := range xs {
		idx.entries = append(idx.entries, &named2[T]{x})
	}
	return idx
}

func main() {
	c := Cache[*Export]{}
	c.ix.entries = append(c.ix.entries, &Export{"x"})
	h := Holder[*SourceFile]{&SourceFile{"abc"}}
	idx := wrapAll([]int{1, 2})
	fmt.Println(len(c.ix.Find("x")), h.spans()[0].Start, len(idx.Find("2")))
}
