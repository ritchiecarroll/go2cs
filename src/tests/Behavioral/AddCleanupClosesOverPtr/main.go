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
// Refusing cases: the closure captures ptr, or a pointer to one of ptr's fields, or the cleanup is
// a method value whose receiver is ptr.
// The cleanup may also be a func-typed local whose every assignment is a func literal or a method
// value; the converter passes the union of their captures.
// Accepting cases: the closure captures another object, a plain value, or nothing, and the
// case where a SIBLING closure in the same scope captures ptr while the cleanup closure
// captures only another variable of that scope (a C# closure object can hold both), and the case
// where the captured variable is reassigned afterwards, which Go captures by reference.

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

func (t *T) release(int) {}

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

	try("method value on ptr", func() {
		v := &T{f: 10}
		runtime.AddCleanup(v, v.release, 0)
	})

	try("reassigned after capture", func() {
		v := &T{f: 11}
		runtime.AddCleanup(v, func(int) { _ = v }, 0)
		v = nil
		fmt.Println("reassigned is nil:", v == nil)
	})

	try("func variable captures ptr", func() {
		v := &T{f: 12}
		cleanup := func(int) { _ = v.f }
		runtime.AddCleanup(v, cleanup, 0)
	})

	try("func variable captures other object", func() {
		v := &T{f: 13}
		w := &T{f: 14}
		cleanup := func(int) { _ = w.f }
		runtime.AddCleanup(v, cleanup, 0)
		runtime.KeepAlive(w)
	})

	try("func variable reassigned to method value", func() {
		v := &T{f: 15}
		cleanup := func(int) {}
		cleanup = v.release
		runtime.AddCleanup(v, cleanup, 0)
	})

	try("arg is ptr", func() {
		v := &T{f: 9}
		runtime.AddCleanup(v, func(*T) {}, v)
	})
}
