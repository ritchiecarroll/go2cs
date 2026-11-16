// Guards the array pointer doors over an array whose ELEMENT is an anonymous struct that nothing else in the
// function declares: `(*[13]struct{ n int32; a uintptr })(p)[i].a`. It is the form C compiled to Go writes for a
// function-local array all through modernc.org/sqlite: the array lives in the function's frame block, and its only
// declaration is a blank placeholder, `var _ /* aSub at bp+0 */ [13]struct{...}`.
//
// The doors render from the index, the slice, the dereference or the range, so the conversion itself is never
// rendered, and rendering it is what lifted its anonymous struct to a C# type. The doors named the element with no
// lift behind it and the conversion of the package ended ("unresolved dynamic type(s) were emitted as raw Go
// source"). Every line here needs the element lifted before a door names it.
//
// The memory is a Go array of words, so every store is read back through the words as well as through the form.

package main

import (
	"fmt"
	"unsafe"
)

type tokenizer struct {
	create uintptr
	remove uintptr
}

// The element type is written only in a blank placeholder and in the conversions: nothing else declares it.
func subLists() {
	var _ /* aSub at bp+0 */ [3]struct {
		nList int32
		aList uintptr
	}
	var buf [6]uint64
	bp := unsafe.Pointer(&buf)

	// An element's field stored and read through the index.
	(*[3]struct {
		nList int32
		aList uintptr
	})(bp)[1].aList = 7
	(*[3]struct {
		nList int32
		aList uintptr
	})(bp)[1].nList = 5
	fmt.Println("index:", (*[3]struct {
		nList int32
		aList uintptr
	})(bp)[1].nList, (*[3]struct {
		nList int32
		aList uintptr
	})(bp)[1].aList, buf[2], buf[3])

	// An element's address.
	for i := 0; i < 3; i++ {
		p := &(*[3]struct {
			nList int32
			aList uintptr
		})(bp)[i]
		p.nList += int32(i)
		fmt.Println("address:", i, p.nList, p.aList)
	}
	fmt.Println("words:", buf)
}

// The same with a field of a named struct type, a whole store, a whole load, a slice and a range.
func builtins() {
	var _ /* aBuiltin at bp+0 */ [2]struct {
		name uintptr
		x    tokenizer
	}
	var buf [6]uintptr
	bp := unsafe.Pointer(&buf)

	*(*[2]struct {
		name uintptr
		x    tokenizer
	})(bp) = [2]struct {
		name uintptr
		x    tokenizer
	}{
		0: {name: 1, x: tokenizer{create: 2, remove: 3}},
		1: {name: 4, x: tokenizer{create: 5, remove: 6}},
	}
	fmt.Println("store:", buf)

	for i := 0; i < 2; i++ {
		fmt.Println("index:", (*[2]struct {
			name uintptr
			x    tokenizer
		})(bp)[i].name, (*[2]struct {
			name uintptr
			x    tokenizer
		})(bp)[i].x.remove)
	}

	buf[4] = 50
	all := *(*[2]struct {
		name uintptr
		x    tokenizer
	})(bp)
	buf[4] = 51
	fmt.Println("load:", all[1].x.create, len(all))

	tail := (*[2]struct {
		name uintptr
		x    tokenizer
	})(bp)[1:2]
	tail[0].name = 40
	fmt.Println("slice:", len(tail), buf[3])

	for i, e := range (*[2]struct {
		name uintptr
		x    tokenizer
	})(bp) {
		fmt.Println("range:", i, e.name, e.x.create)
	}
}

// A literal of the same shape bound to a variable first, then stored whole through the conversion and loaded back.
func bound() {
	var buf [4]uintptr
	bp := unsafe.Pointer(&buf)
	v := [2]struct {
		lo uintptr
		hi uintptr
	}{{1, 2}, {3, 4}}

	*(*[2]struct {
		lo uintptr
		hi uintptr
	})(bp) = v
	v[0].lo = 9
	back := *(*[2]struct {
		lo uintptr
		hi uintptr
	})(bp)
	v = back
	fmt.Println("bound:", buf, v[0].lo, back[1].hi)
}

func main() {
	subLists()
	builtins()
	bound()
}
