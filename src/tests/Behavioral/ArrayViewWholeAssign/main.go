// Guards a view kept past a WHOLE assignment to the value it views. Go's `f := (*[16]uint32)(unsafe.Pointer(&a))` is
// a window over a's memory, and `a = b` copies b's elements INTO that memory, so a write through f afterwards lands in
// a's new elements and a read through f sees them. Each function below takes the window, assigns the whole value,
// writes f[0] and prints the value's first element and the window's element 2 (the low half of the value's element 1).
//
// The window is door C for an unnamed array kept in a local (docs/phase4/DESIGN-array-pointer-doors.md 20) and the
// field view for an array field. It is cut once, over the storage the value holds at the conversion, so it is right
// only while a whole assignment keeps that storage: the converter emits `a.Assign(b)` (and AssignFrom for a struct
// holding an array), which copies into the existing storage (docs/phase4/DESIGN-array-reassign-in-place.md). With the
// earlier form, `a = b.Clone()`, every line below printed the old element and 0, and the write was lost.
package main

import (
	"fmt"
	"unsafe"
)

type holder struct {
	n int32
	x [8]uint64
}

var global [8]uint64

func mk() [8]uint64 { return [8]uint64{11, 12} }

func variable() {
	var a [8]uint64
	b := [8]uint64{1, 2, 3, 4, 5, 6, 7, 8}
	f := (*[16]uint32)(unsafe.Pointer(&a))
	a = b
	f[0] = 0xAAAA
	fmt.Println("variable:", a[0], f[2])
}

func literal() {
	var a [8]uint64
	f := (*[16]uint32)(unsafe.Pointer(&a))
	a = [8]uint64{9, 9, 9}
	f[0] = 0xAAAA
	fmt.Println("literal :", a[0], a[1], f[2])
}

func call() {
	var a [8]uint64
	f := (*[16]uint32)(unsafe.Pointer(&a))
	a = mk()
	f[0] = 0xAAAA
	fmt.Println("call    :", a[0], f[2])
}

func tuple() {
	var a [8]uint64
	f := (*[16]uint32)(unsafe.Pointer(&a))
	var n int
	a, n = [8]uint64{21, 22}, 1
	f[0] = 0xAAAA
	fmt.Println("tuple   :", a[0], f[2], n)
}

func swap() {
	var a [8]uint64
	b := [8]uint64{31, 32}
	f := (*[16]uint32)(unsafe.Pointer(&a))
	a, b = b, a
	f[0] = 0xAAAA
	fmt.Println("swap    :", a[0], f[2], b[0])
}

func field() {
	var h holder
	f := (*[16]uint32)(unsafe.Pointer(&h.x))
	h.x = [8]uint64{41, 42}
	f[0] = 0xAAAA
	fmt.Println("field   :", h.x[0], f[2])
}

func structWhole() {
	var h holder
	f := (*[16]uint32)(unsafe.Pointer(&h.x))
	h = holder{n: 1, x: [8]uint64{5, 6}}
	f[0] = 0xBBBB
	fmt.Println("struct  :", h.x[0], f[2], h.n)
}

func element() {
	var m [2][8]uint64
	f := (*[16]uint32)(unsafe.Pointer(&m[1]))
	m[1] = [8]uint64{51, 52}
	f[0] = 0xAAAA
	fmt.Println("element :", m[1][0], f[2])
}

func outer() {
	var m [2][8]uint64
	f := (*[16]uint32)(unsafe.Pointer(&m[1]))
	m = [2][8]uint64{{}, {61, 62}}
	f[0] = 0xAAAA
	fmt.Println("outer   :", m[1][0], f[2])
}

func throughPointer() {
	p := new([8]uint64)
	f := (*[16]uint32)(unsafe.Pointer(p))
	*p = [8]uint64{7, 8}
	f[0] = 0xCCCC
	fmt.Println("pointer :", p[0], f[2])
}

func glob() {
	f := (*[16]uint32)(unsafe.Pointer(&global))
	global = [8]uint64{3, 4}
	f[0] = 0xDDDD
	fmt.Println("global  :", global[0], f[2])
}

func sliceElement() {
	hs := make([]holder, 2)
	f := (*[16]uint32)(unsafe.Pointer(&hs[1].x))
	hs[1] = holder{n: 2, x: [8]uint64{71, 72}}
	f[0] = 0xAAAA
	fmt.Println("slice el:", hs[1].x[0], f[2])
}

func main() {
	variable()
	literal()
	call()
	tuple()
	swap()
	field()
	structWhole()
	element()
	outer()
	throughPointer()
	glob()
	sliceElement()
}
