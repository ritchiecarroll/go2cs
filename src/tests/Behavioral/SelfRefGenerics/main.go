package main

import "fmt"

// Go 1.26 lifted the restriction that a generic type may not refer to itself in its own type
// parameter list. Every type below was "invalid recursive type" at Go 1.25.

// An interface constraining its own type parameter by itself, satisfied by VALUE types.
type Adder[A Adder[A]] interface {
	Add(A) A
}

type vec struct{ x, y int }

func (v vec) Add(o vec) vec { return vec{v.x + o.x, v.y + o.y} }

type num int

func (n num) Add(o num) num { return n + o }

func Sum[A Adder[A]](xs ...A) A {
	var total A
	for _, x := range xs {
		total = total.Add(x)
	}
	return total
}

type Ordered[T Ordered[T]] interface {
	Less(T) bool
}

type age int

func (a age) Less(b age) bool { return a < b }

func Min[T Ordered[T]](a, b T) T {
	if b.Less(a) {
		return b
	}
	return a
}

// A generic container over the self-referential constraint.
type MinSet[T Ordered[T]] struct{ items []T }

func (s *MinSet[T]) Put(x T) { s.items = append(s.items, x) }

func (s *MinSet[T]) Least() T {
	m := s.items[0]
	for _, x := range s.items[1:] {
		m = Min(m, x)
	}
	return m
}

// A generic STRUCT whose parameter's constraint names the struct itself, through an interface that
// constrains itself: the GraphNode <-> Graph cycle. Satisfied by a POINTER type, so its type
// argument is the constraint proxy.
type GraphNode[N GraphNode[N]] interface {
	Next() N
	Owner() *Graph[N]
	Label() string
}

type Graph[N GraphNode[N]] struct {
	head N
	size int
}

func (g *Graph[N]) Labels() string {
	out := ""
	x := g.head
	for range g.size {
		if x.Owner() != g {
			out += "?"
		}
		out += x.Label()
		x = x.Next()
	}
	return out
}

type gnode struct {
	g    *Graph[*gnode]
	next *gnode
	name string
}

func (n *gnode) Next() *gnode          { return n.next }
func (n *gnode) Owner() *Graph[*gnode] { return n.g }
func (n *gnode) Label() string         { return n.name }

func main() {
	fmt.Println(Sum(vec{1, 2}, vec{3, 4}, vec{5, 6}))
	fmt.Println(Sum[num](1, 2, 3, 4))
	fmt.Println(Min(age(7), age(3)))

	var ms MinSet[age]
	ms.Put(5)
	ms.Put(2)
	ms.Put(8)
	fmt.Println(ms.Least())

	g := &Graph[*gnode]{size: 4}
	a := &gnode{g: g, name: "a"}
	b := &gnode{g: g, name: "b"}
	c := &gnode{name: "c"}
	a.next, b.next, c.next = b, c, a
	g.head = a
	fmt.Println(g.Labels())
}
