package main

import (
	"fmt"
	"unsafe"
)

type small [8]byte

type uuidT = [16]uint8

func main() {
	// R's line: a byte view of a Go array of another element type, sliced, then indexed and stored.
	g := [2]uint64{1, 2}
	v := unsafe.Pointer(&g)
	(*[16]byte)(v)[:4][0] = 48
	fmt.Println("byte view, pointer var  :", g[0])

	h := [2]uint64{1, 2}
	u := uintptr(unsafe.Pointer(&h))
	(*[16]byte)(unsafe.Pointer(u))[:4][0] = 48
	fmt.Println("byte view, number       :", h[0])

	k := [2]uint64{1, 2}
	(*[16]byte)(unsafe.Pointer(&k))[:4][0] = 48
	fmt.Println("byte view, address-of   :", k[0])

	// A named array, whole, over a Go array of its shape and over a uint64.
	var arr [8]byte
	p := unsafe.Pointer(&arr)
	*(*small)(p) = small{1, 2, 3, 4, 5, 6, 7, 8}
	x := *(*small)(p)
	x[0] = 99
	fmt.Println("named whole, same shape :", arr, x[0], *(*small)(p) == small{1, 2, 3, 4, 5, 6, 7, 8})

	var w uint64
	*(*small)(unsafe.Pointer(&w)) = small{1, 0, 0, 0, 0, 0, 0, 1}
	y := *(*small)(unsafe.Pointer(&w))
	fmt.Printf("named whole, a uint64   : %#x %v\n", w, y)

	// The libc shape: an alias of an unnamed array, numbers on both sides.
	var a, b [16]uint8
	for i := range a {
		a[i] = byte(i)
	}
	dst, src := uintptr(unsafe.Pointer(&b)), uintptr(unsafe.Pointer(&a))
	*(*uuidT)(unsafe.Pointer(dst)) = *(*uuidT)(unsafe.Pointer(src))
	fmt.Println("alias whole             :", b)
}
