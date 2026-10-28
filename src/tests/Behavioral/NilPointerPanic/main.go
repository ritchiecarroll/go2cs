// NilPointerPanic guards that a NIL-POINTER dereference is a RECOVERABLE panic carrying Go's own
// message — `runtime error: invalid memory address or nil pointer dereference`.
//
// Go's nil dereference is recoverable and real code depends on it (sync's TestNilPool calls Get/Put
// on a `var p *Pool` and asserts recover() catches the panic). The converted equivalent — reading
// `Ꮡp.Value` through a nil ж<T>, or any nil reference deref in emitted code — raises .NET's
// NullReferenceException, which recover() could not see: the panic escaped past every deferred
// recover and surfaced as an unrecoverable host error. RuntimeErrorPanic.TryAsPanic (the single
// predicate every `func((defer, recover) => …)` execution context shares) now maps it, so recover()
// behaves exactly as in Go.
//
// The recovered VALUE's text is compared, not just the fact of recovery — a recovery that reported a
// different message would still "pass" a bare non-nil check.
package main

import (
	"fmt"
	"runtime"
)

type node struct {
	name string
	next *node
}

// A pointer-receiver method that dereferences the receiver: calling it on nil panics.
func (n *node) label() string { return "node:" + n.name }

// A pointer-receiver method that does NOT dereference: legal on nil in Go, and must stay legal.
func (n *node) isNil() bool { return n == nil }

type box struct {
	p *node
}

// catch runs f and reports what recover() saw, so the exact panic value's text is compared.
func catch(what string, f func()) {
	defer func() {
		if r := recover(); r != nil {
			_, isRuntimeError := r.(runtime.Error)
			fmt.Printf("%s recovered: %v runtime.Error: %v\n", what, r, isRuntimeError)
		} else {
			fmt.Printf("%s NO PANIC\n", what)
		}
	}()
	f()
}

func main() {
	// 1. Pointer-receiver method call on a nil pointer that dereferences the receiver.
	var p *node
	catch("method-call", func() { fmt.Println(p.label()) })

	// 2. Field read through a nil struct pointer.
	catch("field-read", func() { fmt.Println(p.name) })

	// 3. Field WRITE through a nil struct pointer.
	catch("field-write", func() { p.name = "x" })

	// 4. Explicit dereference of a nil pointer.
	catch("explicit-deref", func() { fmt.Println(*p) })

	// 5. A nil pointer reached through another struct's field.
	var b box
	catch("nested-field", func() { fmt.Println(b.p.name) })

	// 6. A nil pointer element of a slice.
	nodes := make([]*node, 2)
	catch("slice-element", func() { fmt.Println(nodes[0].name) })

	// 7. A nil pointer value from a map.
	m := map[string]*node{}
	catch("map-value", func() { fmt.Println(m["absent"].name) })

	// 8. A nil pointer walked one link deep from a real node.
	real := &node{name: "head"}
	catch("chain-walk", func() { fmt.Println(real.next.name) })

	// 9. A TYPED nil pointer: `(*T)(nil)` converts to golib's canonical nil box, not a null reference, so its field
	// read and write reach the box's own nil check (StandardBox.Value) rather than .NET's null dereference.
	q := (*node)(nil)
	catch("typed-nil-field-read", func() { fmt.Println(q.name) })
	catch("typed-nil-field-write", func() { q.name = "x" })

	// NEGATIVE CONTROLS — these must NOT panic.
	catch("nil-safe-method", func() { fmt.Println(p.isNil()) })
	catch("real-method", func() { fmt.Println(real.label()) })
	catch("nil-compare", func() { fmt.Println(p == nil, b.p == nil, nodes[0] == nil) })

	// Recovery leaves the program running normally.
	fmt.Println("still running:", real.label())
}
