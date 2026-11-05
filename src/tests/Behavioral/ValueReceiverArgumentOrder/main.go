package main

import "fmt"

// Go calls a VALUE-receiver method through a pointer on a copy of *p, and takes that copy after the
// call's arguments are evaluated: an argument that writes through p is seen by the method, and a nil p
// panics only after the arguments have run. The method still works on a copy: what it writes to its
// receiver never reaches *p. Covered: a plain struct, a generic struct, a call result as the pointer,
// and a nil pointer.

type V struct{ X int }

func (v V) Show(n int) string { return fmt.Sprint(v.X, " ", n) }

func (v V) Bump() int { v.X++; return v.X }

type Box[T any] struct{ Item T }

func (b Box[T]) Get(tag string) string { return fmt.Sprint(b.Item, " ", tag) }

var shared = &V{X: 1}

func current() *V { return shared }

func main() {
	p := &V{X: 1}
	set := func(x int) int { p.X = x; return x }
	fmt.Println("argument writes through p:", p.Show(set(10)))

	bumped := p.Bump()
	fmt.Println("method writes its copy:", bumped, p.X)

	b := &Box[string]{Item: "old"}
	retag := func() string { b.Item = "new"; return "tag" }
	fmt.Println("generic receiver:", b.Get(retag()))

	fmt.Println("call result as the pointer:", current().Show(func() int { shared.X = 5; return 6 }()))

	var q *V
	ran := false
	side := func() int { ran = true; return 0 }
	func() {
		defer func() { fmt.Println("nil pointer:", recover(), "| argument ran:", ran) }()
		fmt.Println(q.Show(side()))
	}()
}
