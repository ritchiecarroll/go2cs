package main

import "fmt"

// A value-receiver method called through a nil pointer PARAMETER panics in Go at the call, even when the
// method never reads its receiver, because Go copies *p into the receiver first. The same holds for a
// pointer receiver calling one of its value methods. Read with tiered compilation OFF (see the project
// file): the optimizing JIT drops a dereference whose result is unused, so a call that reads the value
// only through a dereferenced copy returns normally there instead of panicking.
// Covered: a parameter used only for the call, one that also reads a field, one captured by a closure,
// one that escapes to a package variable, the call as a middle argument (Go evaluates arguments left to
// right, so the first runs and the third does not), and a pointer receiver that keeps itself calling its
// value method.
// Controls: the same calls through a non-nil pointer, and a pointer-receiver method on a nil parameter.

type T struct{ n int }

func (T) Hello() string { return "hello" }

func (*T) PtrHello() string { return "ptr hello" }

var kept *T

// Via keeps its receiver, so the converter emits it over the box (`this ж<T>`).
func (t *T) Via() string {
	kept = t
	return "via " + t.Hello()
}

var sink *T

func onlyCall(p *T) string { return p.Hello() }

func alsoField(p *T) string {
	s := p.Hello()
	return s + " " + fmt.Sprint(p.n)
}

func inClosure(p *T) string {
	f := func() string { return p.Hello() }
	return f()
}

func escapes(p *T) string {
	sink = p
	return p.Hello()
}

func mark(s string) string {
	fmt.Println("  evaluated:", s)
	return s
}

func join(a, b, c string) string { return a + " " + b + " " + c }

func argOrder(p *T) string { return join(mark("first"), p.Hello(), mark("third")) }

func ptrParam(p *T) string { return p.PtrHello() }

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
	try("parameter, call only", func() string { return onlyCall(p) })
	try("parameter, also reads a field", func() string { return alsoField(p) })
	try("parameter captured by a closure", func() string { return inClosure(p) })
	try("parameter escapes", func() string { return escapes(p) })
	try("call as the middle argument", func() string { return argOrder(p) })
	try("pointer receiver calls its value method", func() string { return p.Via() })
	try("pointer-receiver method on a nil parameter", func() string { return ptrParam(p) })

	q := &T{n: 7}
	try("non-nil, call only", func() string { return onlyCall(q) })
	try("non-nil, also reads a field", func() string { return alsoField(q) })
	try("non-nil, closure", func() string { return inClosure(q) })
	try("non-nil, escapes", func() string { return escapes(q) })
	try("non-nil, middle argument", func() string { return argOrder(q) })
	try("non-nil, pointer receiver", func() string { return q.Via() })
	fmt.Println("sink is q:", sink == q, "kept is q:", kept == q)
}
