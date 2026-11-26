// Go compares pointers by ADDRESS: two unsafe.StringData results over the same string data
// are equal even though each call materializes a fresh pointer value. golib models StringData
// as a fresh view over the string's backing storage, so pointer equality must canonicalize
// to the storage identity — strings' TestMap asserts exactly this through Map's identity
// fast path (no rune changed ⟹ the original string, pointer-identical, comes back).
package main

import (
	"fmt"
	"strings"
	"unsafe"
)

func main() {
	s := "identity probe string"
	t := s // header copy — same backing data

	fmt.Println(unsafe.StringData(s) == unsafe.StringData(t)) // true: same data address
	fmt.Println(unsafe.StringData(s) == unsafe.StringData(s)) // true: same call repeated

	u := string(append([]byte(nil), s...))                    // runtime copy — distinct backing data
	fmt.Println(unsafe.StringData(s) == unsafe.StringData(u)) // false: different address
	fmt.Println(s == u)                                       // true: equal content regardless

	// A SUB-STRING shares its parent's backing, so its data pointer is an INTERIOR pointer into
	// that same allocation — stable across calls, and equal between two identical sub-strings.
	// golib's @string is an offset/length window, and a view that does not begin at the backing
	// array's start cannot be handed out as a PINNED pointer at all, so the pinned model had to
	// materialize a private copy per call: both lines below were false under it, and both are
	// true in Go. They are the guard for StringData modelling the window as an element reference.
	v := s[1:]
	w := s[1:]

	fmt.Println(unsafe.StringData(v) == unsafe.StringData(v)) // true: same sub-string, twice
	fmt.Println(unsafe.StringData(v) == unsafe.StringData(w)) // true: same element of one backing
	fmt.Println(unsafe.StringData(v) == unsafe.StringData(s)) // false: different element

	// unsafe.Add and unsafe.Slice from a literal's data pointer name the same elements StringData
	// names for its sub-strings. A literal's bytes are read-only data, which go2cs serves from one
	// private copy per literal: every pointer into the literal must still name that one copy.
	p := unsafe.StringData(s)
	fmt.Println(unsafe.Add(unsafe.Pointer(p), 1) == unsafe.Pointer(unsafe.StringData(s[1:]))) // true
	fmt.Println(&unsafe.Slice(p, len(s))[2] == unsafe.StringData(s[2:]))                      // true
	fmt.Println(unsafe.String(p, 8))                                                          // identity

	// A HEAP string's bytes are not read-only data: a write through its data pointer reaches every
	// string sharing them, in Go and here. A heap clone equal to a literal is the near miss: the
	// write changes the clone and its sub-strings, never the literal.
	h := strings.Clone("hello, heap")
	g := h[7:]
	*unsafe.StringData(h[7:]) = 'H'
	fmt.Println(h, g, "hello, heap")

	// A two-byte conversion is a heap string too (only a one-byte result is static data).
	b2 := string([]byte{'o', 'k'})
	*unsafe.StringData(b2) = 'O'
	fmt.Println(b2)
}
