package main

import "fmt"

// Go copies *t into a value method's receiver after the call's arguments run, and a nil t panics there even
// when the value method never reads its receiver. Each pointer method below calls a value method on its own
// receiver, so it is emitted over the box and the call goes through the box twin.
// Covered: a callee that ignores its receiver, a side-effecting argument (Go runs it before the nil panic), an
// argument that writes *t (Go's copy sees the write), and a callee that reads a field; nil and non-nil each.
// Read with tiered compilation OFF (see the project file): the optimizing JIT drops a receiver copy whose value is
// unused, so a `this ref T` receiver's nil call returned there instead of panicking.

type T struct{ n int }

func (T) Hello() string { return "hello" }

func (t T) Show(s string) string { return fmt.Sprint(s, " n=", t.n) }

func (t T) N() int { return t.n }

func (t *T) set(v int) string { t.n = v; return "set" }

func mark(s string) string {
	fmt.Println("  evaluated:", s)
	return s
}

func (t *T) Via() string { return "via " + t.Hello() }

func (t *T) ViaArg() string { return t.Show(mark("arg")) }

func (t *T) ViaWrite() string { return t.Show(t.set(10)) }

func (t *T) ViaN() int { return t.N() }

func try(name string, f func() string) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println(name, "panicked:", r)
		}
	}()
	fmt.Println(name, "returned:", f())
}

func main() {
	var p *T
	try("nil, callee ignores its receiver", func() string { return p.Via() })
	try("nil, side-effecting argument", func() string { return p.ViaArg() })
	try("nil, callee reads a field", func() string { return fmt.Sprint(p.ViaN()) })

	q := &T{n: 7}
	try("non-nil, callee ignores its receiver", func() string { return q.Via() })
	try("non-nil, argument writes the pointee", func() string { return q.ViaWrite() })
	try("non-nil, callee reads a field", func() string { return fmt.Sprint(q.ViaN()) })
}
