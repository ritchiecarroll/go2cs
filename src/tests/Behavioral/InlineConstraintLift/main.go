package main

import "fmt"

// Anonymous INLINE constraints that carry methods. Each used to land in the C# `where` clause as Go
// text (`where T : interface{Name() string}`), a syntax cascade that took the whole package down; each
// now lifts to a named interface (`Show_T`), as an anonymous parameter interface already did.

type namer struct{ s string }

func (n namer) Name() string { return n.s }

type tagged struct {
	s   string
	tag int
}

func (t tagged) Name() string { return t.s }
func (t tagged) Tag() int     { return t.tag }

// An exported generic function: its lift must be public, or it is less accessible than the function.
func Show[T interface{ Name() string }](t T) string { return "<" + t.Name() + ">" }

// An unexported one.
func show[T interface{ Name() string }](t T) string { return "[" + t.Name() + "]" }

// Two methods in one constraint.
func Label[T interface {
	Name() string
	Tag() int
}](t T) string {
	return fmt.Sprintf("%s#%d", t.Name(), t.Tag())
}

// An exported generic TYPE, with methods whose receiver repeats the constraint.
type Box[T interface{ Name() string }] struct{ items []T }

func (b *Box[T]) Add(t T) { b.items = append(b.items, t) }

func (b *Box[T]) Names() string {
	out := ""
	for _, it := range b.items {
		out += it.Name() + ";"
	}
	return out
}

// An unexported generic type.
type pair[T interface{ Name() string }] struct{ a, b T }

func (p pair[T]) joined() string { return p.a.Name() + "+" + p.b.Name() }

// A NAMED constraint on a generic type instantiated only at a type position, never converted by a
// call: its nominal record comes from the instantiation itself.
type nameI interface{ Name() string }

type slot[T nameI] struct{ v T }

func (s slot[T]) get() string { return s.v.Name() }

// Two constraints in one list, one of them shared by two parameters.
func Both[A, B interface{ Name() string }, C interface{ Tag() int }](a A, b B, c C) string {
	return a.Name() + "/" + b.Name() + "/" + fmt.Sprint(c.Tag())
}

// A constraint naming the type parameter it constrains lifts GENERIC (`Min_T<T>`), and each value
// instantiation records the constraint closed over its argument (`GoImplement<age, Min_T<age>>`).
func Min[T interface{ Less(T) bool }](a, b T) T {
	if b.Less(a) {
		return b
	}
	return a
}

func main() {
	fmt.Println(Show(namer{"x"}))
	fmt.Println(show(namer{"y"}))
	fmt.Println(Label(tagged{"t", 7}))

	b := &Box[namer]{}
	b.Add(namer{"a"})
	b.Add(namer{"b"})
	fmt.Println(b.Names())

	fmt.Println(pair[tagged]{tagged{"p", 1}, tagged{"q", 2}}.joined())
	fmt.Println(slot[tagged]{tagged{"s", 9}}.get())
	fmt.Println(Both(namer{"n"}, tagged{"m", 0}, tagged{"k", 3}))

	// The generic TYPE Ord (ord.go) at a value, a pointer and a nested generic argument; the
	// instantiations are in this file and the declaration in another.
	fmt.Println(Min(age(5), age(3)))
	fmt.Println(Ord[age]{v: 3}.Below(5), Ord[age]{v: 3}.Below(2))
	fmt.Println(Ord[*node]{v: &node{1}}.Below(&node{0}), Ord[*node]{v: &node{1}}.Below(&node{4}))
	fmt.Println(Ord[wrap[string]]{v: wrap[string]{2}}.Below(wrap[string]{5}), Ord[wrap[int]]{v: wrap[int]{7}}.Below(wrap[int]{3}))

	// A generic value type implementing a generic interface at its own instantiation, no constraint.
	var l Lesser[wrap[string]] = wrap[string]{1}
	fmt.Println(l.Less(wrap[string]{2}), l.Less(wrap[string]{0}))
}
