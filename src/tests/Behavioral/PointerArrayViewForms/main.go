package main

import (
	"fmt"
	"unsafe"
)

// A slice of a pointer-to-array CONVERSION reads the memory it is taken over in every spelling: bare,
// in extra parentheses, and through an explicit dereference (sqlite-cs S20: memory.go's
// `((*rawmem)(unsafe.Pointer(r)))[:size:size]`, libc.go's `(*(*RawMem)(unsafe.Pointer(s)))[:n:n]`).
// A slice of a NAMED array type has the array's element type (S5), and a max bound sets the capacity.
// The arrays are huge, as ccgo's are, so a conversion that copied the whole array would not fit.

type rawmem [1<<50 - 1]byte

func main() {
	src := []byte{1, 2, 3, 4, 5, 6, 7, 8}
	p := uintptr(unsafe.Pointer(&src[0]))
	n := 8

	a := (*[1<<50 - 1]byte)(unsafe.Pointer(p))[:n:n]
	b := ((*[1<<50 - 1]byte)(unsafe.Pointer(p)))[:n:n]
	c := (*(*[1<<50 - 1]byte)(unsafe.Pointer(p)))[:n:n]
	d := (*rawmem)(unsafe.Pointer(p))[2:6]
	e := ((*rawmem)(unsafe.Pointer(p)))[:4]
	f := (*(*rawmem)(unsafe.Pointer(p)))[1:3:5]
	g := (*[1<<50 - 1]byte)(unsafe.Pointer(p))[2:3:7]

	fmt.Println(len(a), a[0], a[7])
	fmt.Println(len(b), b[0], b[7])
	fmt.Println(len(c), c[0], c[7])
	fmt.Println(len(d), d[0], d[3])
	fmt.Println(len(e), e[3])
	fmt.Println(len(f), cap(f), f[0], f[1])
	fmt.Println(len(g), cap(g), g[0])
}
