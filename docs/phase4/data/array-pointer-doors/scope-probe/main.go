package main

import (
	"fmt"
	"unsafe"
)

type RawMem [1<<50 - 1]byte

type small [64]byte

type sibling [64]byte

type hdr struct {
	n    uint32
	port uint16
	tail [8]byte
}

// identity: the source is already a pointer to the array type.
func identityLocal() []int {
	arr := [4]int{1, 2, 3, 4}
	return (*[4]int)(unsafe.Pointer(&arr))[:4]
}

func identityParam(a *[4]int) []int {
	return (*[4]int)(unsafe.Pointer(a))[1:3]
}

func identityThree(a *[4]int, n int) []int {
	return (*[4]int)(unsafe.Pointer(a))[:n:n]
}

func identityNamed(a *small) []byte {
	return (*small)(unsafe.Pointer(a))[:4]
}

func identityNamedOpen(a *small) []byte {
	return (*small)(unsafe.Pointer(a))[2:]
}

func identityField(h *hdr) []byte {
	return (*[8]byte)(unsafe.Pointer(&h.tail))[:4]
}

// alias: the source is a pointer to one element.
func aliasElem(buf *uint16, n int) []uint16 {
	return (*[10000]uint16)(unsafe.Pointer(buf))[:n:n]
}

func aliasNamed(b *byte, n int) []byte {
	return (*RawMem)(unsafe.Pointer(b))[:n:n]
}

// sibling named arrays of one shape.
func siblingNamed(a *sibling) []byte {
	return (*small)(unsafe.Pointer(a))[:4]
}

func siblingUnnamed(a *[64]byte) []byte {
	return (*small)(unsafe.Pointer(a))[:4]
}

func unnamedFromNamed(a *small) []byte {
	return (*[64]byte)(unsafe.Pointer(a))[:4]
}

// a field of another type, one hop off a pointer.
func fieldView(h *hdr) []byte {
	return (*[2]byte)(unsafe.Pointer(&h.port))[:2]
}

func fieldViewOpen(h *hdr) []byte {
	return (*[2]byte)(unsafe.Pointer(&h.port))[:]
}

func fieldViewThree(h *hdr, n int) []byte {
	return (*[4]byte)(unsafe.Pointer(&h.n))[:n:n]
}

// another type: a scalar, a local struct, an array of another element.
func scalar(x *uint32) []byte {
	return (*[4]byte)(unsafe.Pointer(x))[:4]
}

func scalarLocal() byte {
	x := uint32(0x01020304)
	return (*[4]byte)(unsafe.Pointer(&x))[:4][0]
}

func otherElem(w *[4]uint32) []byte {
	return (*[16]byte)(unsafe.Pointer(w))[:8]
}

func structBytes(h *hdr) []byte {
	return (*[16]byte)(unsafe.Pointer(h))[:4]
}

// numbers and plain unsafe.Pointer values.
func number(p uintptr, n int) []byte {
	return (*RawMem)(unsafe.Pointer(p))[:n:n]
}

func plain(p unsafe.Pointer, n int) []byte {
	return (*[1 << 20]byte)(p)[:n]
}

func added(p unsafe.Pointer, off uintptr) []byte {
	return (*[8]byte)(unsafe.Add(p, off))[:8]
}

// forms that wrap the conversion.
func parenthesized(a *[4]int) []int {
	return ((*[4]int)(unsafe.Pointer(a)))[:2]
}

func dereferenced(a *[4]int) []int {
	return (*(*[4]int)(unsafe.Pointer(a)))[:2]
}

func copies(a *[4]int, b *small, src []int) int {
	n := copy((*[4]int)(unsafe.Pointer(a))[:], src)
	n += copy((*small)(unsafe.Pointer(b))[:], "ab")
	return n
}

func main() {
	var a [4]int
	var s small
	var sib sibling
	var arr64 [64]byte
	var h hdr
	var x uint32
	var w [4]uint32
	var buf [16]uint16
	fmt.Println(len(identityLocal()), len(identityParam(&a)), len(identityThree(&a, 2)), len(identityNamed(&s)), len(identityNamedOpen(&s)), len(identityField(&h)))
	fmt.Println(len(aliasElem(&buf[0], 3)), len(aliasNamed(&s[0], 3)), len(siblingNamed(&sib)), len(siblingUnnamed(&arr64)), len(unnamedFromNamed(&s)))
	fmt.Println(len(fieldView(&h)), len(fieldViewOpen(&h)), len(fieldViewThree(&h, 2)), len(scalar(&x)), scalarLocal(), len(otherElem(&w)), len(structBytes(&h)))
	p := unsafe.Pointer(&arr64)
	fmt.Println(len(number(uintptr(p), 2)), len(plain(p, 3)), len(added(p, 8)), len(parenthesized(&a)), len(dereferenced(&a)), copies(&a, &s, []int{1, 2}))
}
