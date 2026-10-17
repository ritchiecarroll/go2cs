package main

import (
	"fmt"
	"runtime"
)

// Guards Go 1.26's runtime.AddCleanup refusal of a cleanup function that closes over ptr
// (mcleanup.go: "cleanup function closes over ptr, cleanup will never run"). Go scans the
// closure object's pointer slots for an address inside ptr's object; the managed AddCleanup
// (core/runtime/mcleanup.cs) scans the fields the cleanup body reads.
//
// Refusing cases: the closure captures ptr, or a pointer to one of ptr's fields.
// Accepting cases: the closure captures another object, a plain value, or nothing, and the
// case where a SIBLING closure in the same scope captures ptr while the cleanup closure
// captures only another variable of that scope (a C# closure object can hold both).

type T struct {
	p *byte // a pointer field keeps T out of Go's tiny allocator
	f int
}

func try(name string, register func()) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: panic: %v\n", name, r)
		}
	}()
	register()
	fmt.Printf("%s: registered\n", name)
}

func nothing(int) {}

func main() {
	try("captures ptr", func() {
		v := &T{f: 1}
		runtime.AddCleanup(v, func(int) { _ = v.f }, 0)
	})

	try("captures field pointer", func() {
		v := &T{f: 2}
		fp := &v.f
		runtime.AddCleanup(v, func(int) { _ = *fp }, 0)
	})

	try("captures other object", func() {
		v := &T{f: 3}
		w := &T{f: 4}
		runtime.AddCleanup(v, func(int) { _ = w.f }, 0)
		runtime.KeepAlive(w)
	})

	try("captures value", func() {
		v := &T{f: 5}
		n := v.f
		runtime.AddCleanup(v, func(int) { _ = n }, 0)
	})

	try("captures nothing", func() {
		v := &T{f: 6}
		runtime.AddCleanup(v, nothing, 0)
	})

	try("sibling captures ptr", func() {
		v := &T{f: 7}
		w := &T{f: 8}
		sibling := func() int { return v.f }
		runtime.AddCleanup(v, func(int) { _ = w.f }, 0)
		fmt.Println("sibling reads", sibling())
		runtime.KeepAlive(w)
	})

	try("arg is ptr", func() {
		v := &T{f: 9}
		runtime.AddCleanup(v, func(*T) {}, v)
	})
}
