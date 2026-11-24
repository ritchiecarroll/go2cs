package main

import (
	"fmt"
	"reflect"
	"runtime"
	"unsafe"

	"go/ualias"
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

// A GENERIC alias of unsafe.Pointer: an instance has no C# name of its own and spells unsafe.Pointer.
type gptr[T any] = unsafe.Pointer

// A named uintptr, converted through the alias as unsafe.Pointer converts it.
type word uintptr

// The alias over a pointer receiver: the receiver's own reference, as unsafe.Pointer(c) is.
func (c *cursor) self() ptr { return ptr(c) }

// Unsafe rule 6 through the alias: a pointer formed from a reflect header's Data word keeps what the header
// describes alive, and where it is, across a collection. Spelled through the alias it took the number route and
// read freed or moved memory (C45, P3's read). Each round reads after a forced collection.
func headerData(rounds int) int {
	sum := 0

	for i := 0; i < rounds; i++ {
		data := []byte{byte(i), byte(i + 1), byte(i + 2), byte(i + 3)}
		h := (*reflect.SliceHeader)(ptr(&data))
		d := ptr(h.Data)
		_ = make([]byte, 4096)
		runtime.GC()
		sum += int(*(*byte)(unsafe.Add(d, 3)))
	}

	return sum
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

	fmt.Println(headerData(64))
	fmt.Println(c.self() != nil, ptr(word(0)) == nil)

	y := uint64(11)
	qa := ualias.Ptr(&y)
	z := uint64(12)
	ga := gptr[int](&z)
	runtime.GC()
	fmt.Println(*(*uint64)(qa), *(*uint64)(ga))
}
