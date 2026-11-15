// F1 scope probe: every sibling shape of the field view (*[N]T)(unsafe.Pointer(&h.f)), f not a T, over
// each kind of root. One line per arm; a panic is printed and the next arm runs.
package main

import (
	"fmt"
	"syscall"
	"unsafe"
)

type hdr struct {
	family uint16
	pad    uint16
	port   uint16
	tail   uint16
	word   uint64
}

type refhdr struct {
	name string
	port uint16
	tail uint16
	word uint64
}

var global = hdr{port: 0x1234, tail: 0x5678, word: 0x1111111122222222}

func try(name string, f func() string) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: PANIC %v\n", name, r)
		}
	}()

	fmt.Printf("%s: %s\n", name, f())
}

// ---- the direct forms

func directRead(h *hdr) byte {
	return (*[2]byte)(unsafe.Pointer(&h.port))[1]
}

func directWrite(h *hdr, v byte) {
	(*[2]byte)(unsafe.Pointer(&h.port))[0] = v
}

func directAddr(h *hdr) *byte {
	return &(*[2]byte)(unsafe.Pointer(&h.port))[1]
}

func directLen(h *hdr) int {
	return len((*[2]byte)(unsafe.Pointer(&h.port)))
}

func directSlice(h *hdr) []byte {
	return (*[2]byte)(unsafe.Pointer(&h.port))[:]
}

func directWhole(h *hdr) [2]byte {
	return *(*[2]byte)(unsafe.Pointer(&h.port))
}

func directWord(h *hdr) uint32 {
	return (*[2]uint32)(unsafe.Pointer(&h.word))[1]
}

func directOpAssign(h *hdr) {
	(*[2]byte)(unsafe.Pointer(&h.port))[1] += 2
}

// ---- the forms that bind a local

func localRead(h *hdr) byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return p[1]
}

func localWrite(h *hdr, v byte) {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	p[0] = v
}

func localRange(h *hdr) int {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	sum := 0

	for i := range p {
		sum += int(p[i]) << (8 * i)
	}

	return sum
}

func localRangeValue(h *hdr) int {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	sum := 0

	for _, b := range p {
		sum += int(b)
	}

	return sum
}

func localWord(h *hdr) uint64 {
	p := (*[2]uint32)(unsafe.Pointer(&h.word))
	return uint64(p[0]) + uint64(p[1])
}

func localSlice(h *hdr) []byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return p[:]
}

func localWhole(h *hdr) [2]byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return *p
}

// ---- the same two reads and writes over a struct that holds a reference

func refDirectRead(h *refhdr) byte {
	return (*[2]byte)(unsafe.Pointer(&h.port))[1]
}

func refDirectWrite(h *refhdr, v byte) {
	(*[2]byte)(unsafe.Pointer(&h.port))[0] = v
}

func refLocalRead(h *refhdr) byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return p[1]
}

func refLocalWrite(h *refhdr, v byte) {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	p[0] = v
}

func arms(root string, h *hdr) {
	try(root+" direct read [1]", func() string { return fmt.Sprintf("%#x", directRead(h)) })
	try(root+" direct write [0]", func() string { directWrite(h, 0xAB); return fmt.Sprintf("port %#x", h.port) })
	try(root+" direct address", func() string { q := directAddr(h); *q = 0xCD; return fmt.Sprintf("port %#x", h.port) })
	try(root+" direct op-assign", func() string { directOpAssign(h); return fmt.Sprintf("port %#x", h.port) })
	try(root+" direct len", func() string { return fmt.Sprint(directLen(h)) })
	try(root+" direct slice", func() string {
		s := directSlice(h)
		s[0] = 0x11
		return fmt.Sprintf("len %d cap %d port %#x", len(s), cap(s), h.port)
	})
	try(root+" direct whole", func() string { a := directWhole(h); return fmt.Sprintf("%#x %#x", a[0], a[1]) })
	try(root+" direct word [1]", func() string { return fmt.Sprintf("%#x", directWord(h)) })
	try(root+" local read [1]", func() string { return fmt.Sprintf("%#x", localRead(h)) })
	try(root+" local write [0]", func() string { localWrite(h, 0x22); return fmt.Sprintf("port %#x", h.port) })
	try(root+" local range index", func() string { return fmt.Sprintf("%#x", localRange(h)) })
	try(root+" local range value", func() string { return fmt.Sprintf("%#x", localRangeValue(h)) })
	try(root+" local word", func() string { return fmt.Sprintf("%#x", localWord(h)) })
	try(root+" local slice", func() string {
		s := localSlice(h)
		s[1] = 0x33
		return fmt.Sprintf("len %d cap %d port %#x", len(s), cap(s), h.port)
	})
	try(root+" local whole", func() string { a := localWhole(h); return fmt.Sprintf("%#x %#x", a[0], a[1]) })
}

func main() {
	// A Go struct with no reference in it, on the heap.
	managed := &hdr{port: 0x1234, tail: 0x5678, word: 0x1111111122222222}
	arms("managed", managed)

	// A package variable.
	arms("global", &global)

	// A struct laid over a Go byte slice.
	buf := make([]byte, 64)
	over := (*hdr)(unsafe.Pointer(&buf[0]))
	over.port, over.tail, over.word = 0x1234, 0x5678, 0x1111111122222222
	arms("over-bytes", over)

	// A struct in memory the kernel owns.
	page, err := syscall.Mmap(-1, 0, 4096, syscall.PROT_READ|syscall.PROT_WRITE, syscall.MAP_ANON|syscall.MAP_PRIVATE)

	if err != nil {
		fmt.Println("mmap:", err)
		return
	}

	defer syscall.Munmap(page)

	native := (*hdr)(unsafe.Pointer(&page[0]))
	native.port, native.tail, native.word = 0x1234, 0x5678, 0x1111111122222222
	arms("native", native)

	// A Go struct that holds a reference.
	ref := &refhdr{name: "x", port: 0x1234, tail: 0x5678}
	try("ref direct read [1]", func() string { return fmt.Sprintf("%#x", refDirectRead(ref)) })
	try("ref direct write [0]", func() string { refDirectWrite(ref, 0xAB); return fmt.Sprintf("port %#x name %q", ref.port, ref.name) })
	try("ref local read [1]", func() string { return fmt.Sprintf("%#x", refLocalRead(ref)) })
	try("ref local write [0]", func() string { refLocalWrite(ref, 0x22); return fmt.Sprintf("port %#x name %q", ref.port, ref.name) })
}
