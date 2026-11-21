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

type outerHolder struct {
	k int32
	h holder
}

type named [8]uint64

type aliasArr = [8]uint64

var global [8]uint64

func mk() [8]uint64 { return [8]uint64{11, 12} }

func pair() ([8]uint64, error) { return [8]uint64{17, 18}, nil }

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

func forClause() {
	var a [8]uint64
	b := [8]uint64{101, 102}
	f := (*[16]uint32)(unsafe.Pointer(&a))
	var n int
	for a, n = b, 0; n < 1; n++ {
		f[0] = 0xAAAA
	}
	fmt.Println("for cl  :", a[0], f[2], n)
}

func forPost() {
	var a [8]uint64
	b := [8]uint64{171, 172}
	f := (*[16]uint32)(unsafe.Pointer(&a))
	n := 0
	for ; n < 2; a, n = b, n+1 {
		if n == 0 {
			continue
		}
	}
	f[0] = 0xAAAA
	fmt.Println("for post:", a[0], f[2], n)
}

func forField() {
	var h holder
	f := (*[16]uint32)(unsafe.Pointer(&h.x))
	var n int
	for h.x, n = [8]uint64{181, 182}, 0; n < 1; n++ {
		f[0] = 0xAAAA
	}
	fmt.Println("for fld :", h.x[0], f[2], n)
}

func ifInit() {
	var a [8]uint64
	b := [8]uint64{111, 112}
	f := (*[16]uint32)(unsafe.Pointer(&a))
	var n int
	if a, n = b, 1; n > 0 {
		f[0] = 0xAAAA
	}
	fmt.Println("if init :", a[0], f[2], n)
}

func switchInit() {
	var a [8]uint64
	b := [8]uint64{121, 122}
	f := (*[16]uint32)(unsafe.Pointer(&a))
	var n int
	switch a, n = b, 1; n {
	case 1:
		f[0] = 0xAAAA
	}
	fmt.Println("sw init :", a[0], f[2], n)
}

func multiValue() {
	var a [8]uint64
	var err error
	f := (*[16]uint32)(unsafe.Pointer(&a))
	a, err = pair()
	f[0] = 0xAAAA
	fmt.Println("multi   :", a[0], f[2], err == nil)
}

func rangeAssign() {
	var a [8]uint64
	arrs := [][8]uint64{{131, 132}}
	f := (*[16]uint32)(unsafe.Pointer(&a))
	for _, a = range arrs {
		f[0] = 0xAAAA
	}
	fmt.Println("range   :", a[0], f[2])
}

func nested() {
	var o outerHolder
	f := (*[16]uint32)(unsafe.Pointer(&o.h.x))
	o = outerHolder{k: 1, h: holder{n: 2, x: [8]uint64{141, 142}}}
	f[0] = 0xAAAA
	fmt.Println("nested  :", o.h.x[0], f[2], o.k, o.h.n)
}

func pointerStruct() {
	ph := new(holder)
	f := (*[16]uint32)(unsafe.Pointer(&ph.x))
	*ph = holder{n: 3, x: [8]uint64{151, 152}}
	f[0] = 0xAAAA
	fmt.Println("ptr str :", ph.x[0], f[2], ph.n)
}

func namedArray() {
	var a named
	f := (*[16]uint32)(unsafe.Pointer(&a))
	a = named{161, 162}
	f[0] = 0xAAAA
	fmt.Println("named   :", a[0], f[2])
}

func selectRecv() {
	var a [8]uint64
	f := (*[16]uint32)(unsafe.Pointer(&a))
	ch := make(chan [8]uint64, 1)
	ch <- [8]uint64{211, 212}
	select {
	case a = <-ch:
	}
	f[0] = 0xAAAA
	fmt.Println("select  :", a[0], f[2])
}

func recvOk() {
	var a [8]uint64
	var ok bool
	f := (*[16]uint32)(unsafe.Pointer(&a))
	ch := make(chan [8]uint64, 1)
	ch <- [8]uint64{221, 222}
	a, ok = <-ch
	f[0] = 0xAAAA
	fmt.Println("recv ok :", a[0], f[2], ok)
}

func namedResultIndex() (a [8]uint64) {
	p := &a
	defer func() { a[0] = 0xBBBB }()
	p[1] = 202
	return
}

func forSingle() {
	var a [8]uint64
	b := [8]uint64{201, 202}
	f := (*[16]uint32)(unsafe.Pointer(&a))
	n := 0
	for a = b; n < 1; n++ {
		f[0] = 0xAAAA
	}
	fmt.Println("for one :", a[0], f[2], n)
}

func forSinglePost() {
	var a [8]uint64
	b := [8]uint64{211, 212}
	f := (*[16]uint32)(unsafe.Pointer(&a))
	n := 0
	for ; n < 2; a = b {
		n++
	}
	f[0] = 0xAAAA
	fmt.Println("for 1pst:", a[0], f[2], n)
}

func forLabelled() {
	var a [8]uint64
	b := [8]uint64{221, 222}
	f := (*[16]uint32)(unsafe.Pointer(&a))
	n := 0
outer:
	for ; n < 2; a, n = b, n+1 {
		for k := 0; k < 2; k++ {
			if k == 0 {
				continue outer
			}
		}
	}
	f[0] = 0xAAAA
	fmt.Println("for lbl :", a[0], f[2], n)
}

func namedStructResultIndex() (h holder) {
	p := &h
	defer func() { h.x[0] = 0xDDDD }()
	p.x[1] = 404
	return
}

func aliasedResultIndex() (a aliasArr) {
	p := &a
	defer func() { a[0] = 0xCCCC }()
	p[1] = 303
	return
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
	forClause()
	forPost()
	forField()
	ifInit()
	switchInit()
	multiValue()
	rangeAssign()
	nested()
	pointerStruct()
	namedArray()
	selectRecv()
	recvOk()
	r := namedResultIndex()
	fmt.Println("res idx :", r[0], r[1], len(r))
	forSingle()
	forSinglePost()
	forLabelled()
	h := namedStructResultIndex()
	fmt.Println("res sidx:", h.x[0], h.x[1], len(h.x))
	ra := aliasedResultIndex()
	fmt.Println("res aidx:", ra[0], ra[1], len(ra))
}
