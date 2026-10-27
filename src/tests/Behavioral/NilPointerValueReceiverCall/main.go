package main

import "fmt"

// Go dereferences the receiver to call a VALUE-receiver method through a pointer, so the call
// panics on a nil pointer even when the method never touches its receiver. A POINTER-receiver
// method is called with the nil pointer itself and does not panic until it dereferences it.
// Covered: a call in a closure, a direct call in a named function, a method value (Go panics at
// the binding), and a call through an interface holding the nil pointer.

type T struct{ n int }

func (T) Hello() string { return "hello" }

func (t T) N() int { return t.n }

func (*T) PtrHello() string { return "ptr hello" }

type Helloer interface{ Hello() string }

func direct(p *T) string { return p.Hello() }

func try(name string, f func() string) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println(name, "panicked:", r)
		}
	}()
	fmt.Println(name, "returned:", f())
}

func main() {
	var p *T = nil
	try("value receiver, receiver unused", func() string { return p.Hello() })
	try("value receiver, reads a field", func() string { return fmt.Sprint(p.N()) })
	try("pointer receiver, receiver unused", func() string { return p.PtrHello() })
	try("direct call in a named func", func() string { return direct(p) })
	try("method value", func() string { f := p.Hello; return "bound " + f() })
	try("through an interface", func() string { var h Helloer = p; return h.Hello() })

	q := &T{n: 7}
	try("value receiver via non-nil pointer", func() string { return fmt.Sprint(q.Hello(), " ", q.N()) })
}
