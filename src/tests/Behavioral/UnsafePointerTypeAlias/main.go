package main

import (
	"fmt"
	"unsafe"
)

// A package-level alias to unsafe.Pointer, the shape of zeebo/xxh3's utils.go.
type ptr = unsafe.Pointer

type cursor struct {
	base ptr
	off  uintptr
}

func readU8(p ptr, o uintptr) uint8 {
	return *(*uint8)(ptr(uintptr(p) + o))
}

func readU32(p ptr, o uintptr) uint32 {
	return *(*uint32)(unsafe.Add(p, o))
}

func (c *cursor) next() uint8 {
	b := readU8(c.base, c.off)
	c.off++
	return b
}

func main() {
	x := uint64(42)
	var p ptr = unsafe.Pointer(&x)
	fmt.Println(*(*uint64)(p))

	buf := [8]byte{1, 2, 3, 4, 5, 6, 7, 8}
	q := ptr(&buf[0])
	fmt.Println(readU8(q, 3), readU32(q, 4) == uint32(5)|uint32(6)<<8|uint32(7)<<16|uint32(8)<<24)

	c := &cursor{base: q}
	fmt.Println(c.next(), c.next(), c.off)
	fmt.Println(p != nil, q == unsafe.Pointer(&buf[0]))
}
