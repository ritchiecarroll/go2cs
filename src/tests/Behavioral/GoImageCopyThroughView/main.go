package main

import (
	"fmt"
	"unsafe"
)

// Guards copy through a byte view of a REFERENCE-BEARING struct (docs/phase4/DESIGN-reinterpret-seam.md): syscall's
// getdirentries_test.go:67 decodes each record with
//
//	copy((*[unsafe.Sizeof(dirent)]byte)(unsafe.Pointer(&dirent))[:], data)
//
// Dirent holds an inline array, which go2cs represents as a managed array<sbyte>, so the struct has no address and
// the view was a token that panicked on first use. Each block below prints what Go prints.

// Dirent is darwin's syscall.Dirent: scalars, an inline name array, then padding.
type Dirent struct {
	Ino     uint64
	Seekoff uint64
	Reclen  uint16
	Namlen  uint16
	Type    uint8
	Name    [1024]int8
	Pad_cgo [3]byte
}

// Pair is a smaller record with Go padding between its fields (2 bytes after A, 4 after B).
type Pair struct {
	A uint16
	B [2]uint8
	C uint64
}

func main() {
	// Write through the view: a whole record decoded from bytes.
	data := make([]byte, unsafe.Sizeof(Dirent{}))
	data[0] = 7
	data[16] = 32
	data[18] = 3
	data[21], data[22], data[23] = 'a', 'b', 'c'

	var dirent Dirent
	n := copy((*[unsafe.Sizeof(dirent)]byte)(unsafe.Pointer(&dirent))[:], data)
	fmt.Println(n, dirent.Ino, dirent.Reclen, dirent.Namlen, string([]byte{byte(dirent.Name[0]), byte(dirent.Name[1]), byte(dirent.Name[2])}))

	// A partial final record: fewer source bytes than the struct, so the bytes past them keep their values.
	dirent.Seekoff = 99
	n = copy((*[unsafe.Sizeof(dirent)]byte)(unsafe.Pointer(&dirent))[:], data[:10])
	fmt.Println(n, dirent.Ino, dirent.Seekoff, dirent.Reclen)

	// Read through the view: the struct's Go-layout bytes, padding zero.
	p := Pair{A: 0x0102, B: [2]uint8{5, 6}, C: 0x1122334455667788}
	out := make([]byte, 16)
	n = copy(out, (*[unsafe.Sizeof(p)]byte)(unsafe.Pointer(&p))[:])
	fmt.Println(n, out)

	// A window of the view, through a pointer variable: write bytes 8..10 only.
	q := &p
	n = copy((*[unsafe.Sizeof(p)]byte)(unsafe.Pointer(q))[8:10], []byte{0xAA, 0xBB, 0xCC})
	fmt.Printf("%d %#x %v\n", n, p.C, p.B)
}
