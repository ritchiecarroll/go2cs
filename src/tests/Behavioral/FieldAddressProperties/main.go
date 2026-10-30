// Parity seat #32 (P2): a field's address, Go's `&p.f`, reads `p.Ꮡf` in the converted C#, the address property
// go2cs-gen emits beside each field property. Each shape below takes a field's address and then reads or writes
// through it, so the Output phase compares behavior and the Target phase pins the spelling:
//   - container/list's shapes: a list's root sentinel addressed through the receiver's box (`&l.root`) and through a
//     pointer field (`&e.list.root`), compared by identity on every step of the walk;
//   - one field addressed at two sites, which must be the same pointer;
//   - a field addressed through a pointer to a pointer (`&(*pp).hits`);
//   - a field of a heap-escaping value, and a field of a generic struct.
package main

import "fmt"

type Element struct {
	next, prev *Element
	list       *List
	Value      any
}

type List struct {
	root Element
	len  int
}

func (l *List) Init() *List {
	l.root.next = &l.root
	l.root.prev = &l.root
	l.len = 0
	return l
}

func (l *List) PushBack(v any) *Element {
	e := &Element{Value: v, list: l}
	at := l.root.prev
	e.prev = at
	e.next = at.next
	e.prev.next = e
	e.next.prev = e
	l.len++
	return e
}

func (l *List) Front() *Element {
	if l.len == 0 {
		return nil
	}
	return l.root.next
}

func (e *Element) Next() *Element {
	if p := e.next; e.list != nil && p != &e.list.root {
		return p
	}
	return nil
}

type counter struct {
	hits int
	name string
}

type pair[T any] struct {
	First T
	Count int
}

var kept []*int

func keep(p *int) *int {
	kept = append(kept, p)
	return p
}

func bump(p *int) { *p++ }

func main() {
	l := new(List).Init()
	for i := 1; i <= 3; i++ {
		l.PushBack(i)
	}
	for e := l.Front(); e != nil; e = e.Next() {
		fmt.Print(e.Value, " ")
	}
	fmt.Println(l.len, l.root.next.Value, l.root.prev.Value)

	c := &counter{name: "c"}
	a, b := keep(&c.hits), keep(&c.hits)
	fmt.Println("same field, two sites:", a == b)
	bump(a)
	bump(b)
	fmt.Println("hits:", c.hits)

	pp := &c
	q := keep(&(*pp).hits)
	bump(q)
	fmt.Println("through a pointer to a pointer:", c.hits, q == a)

	var v counter
	vh := keep(&v.hits)
	bump(vh)
	fmt.Println("heap value:", v.hits, vh == &v.hits)

	g := &pair[string]{First: "x"}
	gf := &g.First
	*gf = "y"
	gc := keep(&g.Count)
	bump(gc)
	fmt.Println("generic:", g.First, g.Count)
}
