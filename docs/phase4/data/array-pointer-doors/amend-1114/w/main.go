package main

import (
	"fmt"
	"unsafe"
)

type RawMem [1<<50 - 1]byte

func main() {
	// W: w5b2's twin. A RawMem view over a local [16]byte, the open form directly under copy.
	var buf [16]byte
	n := copy((*RawMem)(unsafe.Pointer(&buf[0]))[:], "name\x00")
	fmt.Printf("%d %q\n", n, buf[:6])
	// W2: the same array as a copy source through the open form.
	var out [4]byte
	m := copy(out[:], (*RawMem)(unsafe.Pointer(&buf[0]))[:])
	fmt.Printf("%d %q\n", m, out[:])
	// W3: an interior start: lo = 12 leaves 4 elements of the Go array.
	k := copy((*RawMem)(unsafe.Pointer(&buf[0]))[12:], "wxyz")
	fmt.Printf("%d %q\n", k, buf[10:])
}
