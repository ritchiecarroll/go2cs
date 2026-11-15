package main

import (
	"fmt"
	"os"
	"syscall"
	"unsafe"
)

func native(n int) uintptr {
	b, err := syscall.Mmap(-1, 0, n, syscall.PROT_READ|syscall.PROT_WRITE, syscall.MAP_ANON|syscall.MAP_PRIVATE)
	if err != nil {
		panic(err)
	}
	return uintptr(unsafe.Pointer(&b[0]))
}

func peek(p uintptr, n int) string {
	out := make([]byte, n)
	for i := 0; i < n; i++ {
		out[i] = *(*byte)(unsafe.Pointer(p + uintptr(i)))
	}
	return fmt.Sprintf("%q", out)
}

type smallMem [64]byte

type hdr struct {
	n   int32
	buf [64]byte
}

func viaByte(bp *byte) {
	(*smallMem)(unsafe.Pointer(bp))[5] = 7
	raw := (*smallMem)(unsafe.Pointer(bp))
	raw[6] = 8
	fmt.Println(raw[5], raw[6])
}

func viaStruct(h *hdr) {
	a := (*smallMem)(unsafe.Pointer(&h.buf))
	a[1] = 3
	b := (*smallMem)(unsafe.Pointer(&h.buf[0]))
	b[2] = 4
	fmt.Println(h.buf[1], h.buf[2])
}

func main() {
	z := native(4096)
	switch os.Args[1] {
	case "go":
		var h hdr
		viaStruct(&h)
		viaByte(&h.buf[0])
	case "byte": // a typed *byte over C memory, then (*smallMem)(unsafe.Pointer(bp))
		viaByte((*byte)(unsafe.Pointer(z)))
		fmt.Println(peek(z, 8))
	case "struct": // a typed *hdr over C memory, then (*smallMem)(unsafe.Pointer(&h.buf))
		viaStruct((*hdr)(unsafe.Pointer(z)))
		fmt.Println(peek(z, 12))
	}
}
