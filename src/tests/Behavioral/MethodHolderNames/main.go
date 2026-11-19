package main

import "fmt"

// A constant, a variable, a type and a function, each named like a method of the type that reads it.
// Go keeps a method in its receiver type's method set and never in the package scope, so every use
// below names the package's declaration.
const Limit = 7

var Count = 3

type Kind int

func Scale(n int) int { return n * 10 }

func (k Kind) String() string { return fmt.Sprintf("Kind(%d)", int(k)) }

type Box struct{ n int }

func (b Box) Limit() int { return b.n + Limit }

func (b *Box) Count() int {
	Count++
	return b.n + Count
}

func (b Box) Kind() Kind { return Kind(b.n) }

func (b Box) Scale() int {
	f := Scale
	return Scale(b.n) + f(1)
}

func (b Box) Describe(v any) string {
	switch k := v.(type) {
	case Kind:
		return "a Kind: " + k.String()
	case Box:
		return fmt.Sprint("a Box: ", k.n)
	}

	if _, ok := v.(Kind); ok {
		return "unreachable"
	}

	return fmt.Sprint("other: ", v, " zero ", Kind(0))
}

// Class names the type Kind in a case that binds no value, beside the method Kind.
func (b Box) Class(v any) string {
	switch v.(type) {
	case Kind:
		return "kind"
	case Box, *Box:
		return "box"
	}

	return "neither"
}

// A generic receiver, a pointer receiver and a value receiver read the same names.
type Pair[T any] struct{ a, b T }

func (p Pair[T]) Scale() int { return Scale(2) + Limit }

func (p *Pair[T]) Limit() int { return Limit * 2 }

// A method declared through an alias of its receiver type is a method of the type.
type Crate = Box

func (c Crate) Total() int { return c.Limit() + Limit + Scale(c.n) }

// A struct that embeds Box and declares one of the promoted names itself.
type Shelf struct {
	Box
	extra int
}

func (s Shelf) Limit() int { return s.Box.Limit() + s.extra + Limit }

func main() {
	b := Box{n: 1}
	fmt.Println("const", b.Limit(), Limit)
	fmt.Println("var", b.Count(), Count)
	fmt.Println("type", b.Kind(), Kind(5))
	fmt.Println("func", b.Scale(), Scale(4))
	fmt.Println(b.Describe(Kind(2)))
	fmt.Println(b.Describe(Box{n: 9}))
	fmt.Println(b.Describe("text"))
	fmt.Println("cases", b.Class(Kind(1)), b.Class(b), b.Class(&b), b.Class(1))

	p := Pair[string]{"x", "y"}
	fmt.Println("generic", p.Scale(), p.Limit(), p.a+p.b)

	fmt.Println("alias", Crate{n: 2}.Total())

	s := Shelf{Box: Box{n: 3}, extra: 100}
	fmt.Println("embedded", s.Limit(), s.Scale(), s.Kind(), s.Count())

	// Method expressions and method values name the method; the bare names stay the package's.
	limit := Box.Limit
	count := (*Box).Count
	scale := b.Scale
	fmt.Println("expressions", limit(b), count(&b), scale(), Scale(1), Limit)

	shadows()
	others()
	forms()
}
