package main

// The unsafe string/slice-header view family as github.com/zeebo/xxh3 writes it (utils.go, hash64.go,
// hasher.go, consts.go): a two-field header over a string or a []byte, reads and writes at a uintptr offset
// from its data pointer, a pointer to a package-level array, and a pointer to a fresh array literal. Each arm
// recovers, so a failing arm prints its panic and the arms after it still run.

import (
	"encoding/binary"
	"fmt"
	"unsafe"
)

type (
	ptr = unsafe.Pointer
	ui  = uintptr
)

// xxh3's header: a data pointer and an UNSIGNED length, where Go's own string header has an int.
type str struct {
	p ptr
	l uint
}

var key = ptr(&[...]uint8{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16})

type hasher struct {
	acc uint64
	buf [16]byte
}

func readU8(p ptr, o ui) uint8 { return *(*uint8)(ptr(ui(p) + o)) }

func readU64(p ptr, o ui) uint64 {
	b := (*[8]byte)(ptr(ui(p) + o))
	return binary.LittleEndian.Uint64(b[:])
}

func writeU64(p ptr, o ui, v uint64) {
	b := (*[8]byte)(ptr(ui(p) + o))
	for i := 0; i < 8; i++ {
		b[i] = byte(v >> (8 * i))
	}
}

func try(name string, f func() string) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: panic: %v\n", name, r)
		}
	}()
	fmt.Printf("%s: %s\n", name, f())
}

func main() {
	s := "abcdefghijklmnop"
	b := []byte("ABCDEFGHIJKLMNOP")

	try("string header", func() string {
		h := *(*str)(ptr(&s))
		return fmt.Sprint(h.l, readU8(h.p, 0), readU8(h.p, 15), readU64(h.p, 8))
	})
	try("slice header", func() string {
		h := *(*str)(ptr(&b))
		return fmt.Sprint(h.l, readU8(h.p, 1), readU64(h.p, ui(h.l)-8))
	})
	try("slice as string", func() string { return *(*string)(ptr(&b)) })
	try("string data word", func() string { return fmt.Sprint(readU8(*(*ptr)(ptr(&s)), 2)) })
	try("pointer step", func() string {
		h := *(*str)(ptr(&s))
		q := ptr(ui(h.p) + 4)
		q = ptr(ui(q) + 4)
		return fmt.Sprint(readU8(q, 0))
	})
	try("package array reads", func() string { return fmt.Sprint(readU8(key, 0), readU8(key, 15), readU64(key, 8)) })
	try("package array view length", func() string { return fmt.Sprint(len((*[4]byte)(key)), (*[4]byte)(key)[3]) })
	try("fresh array writes", func() string {
		secret := ptr(&[16]byte{})
		writeU64(secret, 0, 0x0102030405060708)
		writeU64(secret, 8, 0x1112131415161718)
		return fmt.Sprint(readU8(secret, 0), readU8(secret, 8), readU64(secret, 8))
	})
	try("struct field array", func() string {
		var h hasher
		writeU64(ptr(&h.buf), 8, 42)
		return fmt.Sprint(h.buf[8], len((*[8]byte)(ptr(&h.buf))))
	})
}
