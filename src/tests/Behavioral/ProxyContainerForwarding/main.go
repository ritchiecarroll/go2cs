package main

import (
	"fmt"
	"iter"
)

// Tree's Children result shares the element's own storage. A POINTER element would need a constraint proxy, a
// separate C# object per pointer, that cannot forward that sharing, so go2cs refuses such an instantiation by
// name. A VALUE element needs no proxy: it converts, and the slice Children returns aliases the element's
// backing array exactly as in Go.
type Tree[N any] interface {
	Children() []N
	Label() string
}

type Leaf struct {
	name string
	kids []Leaf
}

func (l Leaf) Children() []Leaf { return l.kids }

func (l Leaf) Label() string { return l.name }

func Count[N Tree[N]](n N) int {
	total := 1

	for _, kid := range n.Children() {
		total += Count(kid)
	}

	return total
}

func Labels[N Tree[N]](n N) []string {
	out := []string{n.Label()}

	for _, kid := range n.Children() {
		out = append(out, Labels(kid)...)
	}

	return out
}

// Rename writes through the slice Children returns: Go shares it with the element, so the root sees the change.
func Rename[N Tree[N]](n N, rename func(N) N) {
	kids := n.Children()

	for i := range kids {
		kids[i] = rename(kids[i])
	}
}

// Stepper's methods all cross a constraint proxy exactly: a func result (a nil func stays nil), an array,
// which Go copies, and a NAMED func type (iter.Seq). A POINTER element converts through the proxy.
type Stepper[N any] interface {
	Next() func() N
	Triple() [3]N
	Path() iter.Seq[N]
	Label() string
}

type Node struct {
	name string
	next *Node
}

func (n *Node) Next() func() *Node {
	if n.next == nil {
		return nil
	}

	return func() *Node { return n.next }
}

func (n *Node) Triple() [3]*Node { return [3]*Node{n, n, n} }

func (n *Node) Path() iter.Seq[*Node] {
	return func(yield func(*Node) bool) {
		for p := n; p != nil; p = p.next {
			if !yield(p) {
				return
			}
		}
	}
}

func (n *Node) Label() string { return n.name }

func Walk[N Stepper[N]](n N) []string {
	var out []string

	for {
		triple := n.Triple()
		out = append(out, n.Label()+"/"+triple[2].Label())

		next := n.Next()

		if next == nil {
			break
		}

		n = next()
	}

	return out
}

func Names[N Stepper[N]](n N) []string {
	var out []string

	for p := range n.Path() {
		out = append(out, p.Label())
	}

	return out
}

func main() {
	root := Leaf{name: "root", kids: []Leaf{{name: "a"}, {name: "b", kids: []Leaf{{name: "c"}}}}}

	fmt.Println(Count(root))
	fmt.Println(Labels(root))

	Rename(root, func(l Leaf) Leaf {
		l.name += "!"
		return l
	})

	fmt.Println(Labels(root))

	c := &Node{name: "c"}
	b := &Node{name: "b", next: c}
	a := &Node{name: "a", next: b}

	fmt.Println(Walk(a))
	fmt.Println(Names(b))
}
