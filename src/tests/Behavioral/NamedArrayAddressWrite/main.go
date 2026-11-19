package main

import (
	"fmt"
	"unsafe"
)

type small [8]byte

type holder struct {
	n   int32
	buf small
}

var global small

// A write at an address, as a syscall or libc's C ABI makes it.
func fill(p unsafe.Pointer, b byte) {
	*(*byte)(p) = b
}

func fillNumber(p uintptr, b byte) {
	*(*byte)(unsafe.Pointer(p)) = b
}

// libc's Xmemcpy shape.
func memcpy(dest, src uintptr, n uintptr) {
	copy((*[1 << 20]byte)(unsafe.Pointer(dest))[:n:n], (*[1 << 20]byte)(unsafe.Pointer(src))[:n:n])
}

func main() {
	// la: a local named array, never used before its address is taken.
	var local small
	fill(unsafe.Pointer(&local), 41)
	fmt.Println("la", local[0])

	// lb: a package variable, by number.
	fillNumber(uintptr(unsafe.Pointer(&global)), 42)
	fmt.Println("lb", global[0])

	// lc: a named array field of a struct.
	h := &holder{}
	fill(unsafe.Pointer(&h.buf), 43)
	fmt.Println("lc", h.buf[0])

	// ld: new(small), by number.
	x := new(small)
	fillNumber(uintptr(unsafe.Pointer(x)), 44)
	fmt.Println("ld", x[0])

	// le: memcpy into a named array never used before.
	var dst small
	src := [8]byte{1, 2, 3, 4, 5, 6, 7, 8}
	memcpy(uintptr(unsafe.Pointer(&dst)), uintptr(unsafe.Pointer(&src)), 8)
	fmt.Println("le", dst)

	// lf: the control, a named array used before its address is taken.
	var used small
	used[7] = 9
	fill(unsafe.Pointer(&used), 45)
	fmt.Println("lf", used[0], used[7])

	// lg: the control, an unnamed array never used before.
	var plain [8]byte
	fill(unsafe.Pointer(&plain), 46)
	fmt.Println("lg", plain[0])
}
