// Command NativePtrAot reads a pointer array in memory the Go runtime did not allocate: the shape
// Windows' DNS_TXT_DATAW hands net.LookupTXT, built here on linux from an anonymous mapping that
// syscall.Mmap makes (not the runtime's own). A record at the mapping's base carries a count and the
// first word of an array of pointers, each naming a NUL-terminated UTF-16 string; net's spelling
// views that array through a window far larger than the memory, re-sliced to the count. A second
// record does the same over a converted struct element, so a builtin element and a struct element
// both reach the native reader.
package main

import (
	"fmt"
	"syscall"
	"unicode/utf16"
	"unsafe"
)

// The DNSTXTData shape: a count, then the first word of the array.
type txtData struct {
	StringCount uint32
	StringArray [1]*uint16
}

type point struct {
	X, Y int32
}

type pointData struct {
	Count  uint32
	Points [1]*point
}

func main() {
	b, err := syscall.Mmap(-1, 0, 4096, syscall.PROT_READ|syscall.PROT_WRITE, syscall.MAP_ANON|syscall.MAP_PRIVATE)
	if err != nil {
		panic(err)
	}

	base := uintptr(unsafe.Pointer(&b[0]))

	put := func(offset int, s string) {
		for i, r := range s {
			*(*uint16)(unsafe.Pointer(&b[offset+2*i])) = uint16(r)
		}
	}
	word := func(offset int, value uintptr) { *(*uintptr)(unsafe.Pointer(&b[offset])) = value }

	// txtData at 0: count 2, words at 8 and 16.
	put(256, "hello")
	put(512, "world")
	*(*uint32)(unsafe.Pointer(&b[0])) = 2
	word(8, base+256)
	word(16, base+512)

	// pointData at 64: count 2, words at 72 and 80.
	*(*point)(unsafe.Pointer(&b[1024])) = point{3, 4}
	*(*point)(unsafe.Pointer(&b[1032])) = point{5, 12}
	*(*uint32)(unsafe.Pointer(&b[64])) = 2
	word(72, base+1024)
	word(80, base+1032)

	// The address arrives as a number, as the DNS record's does from DnsQuery.
	d := (*txtData)(unsafe.Pointer(base))

	for i, v := range (*[1 << 10]*uint16)(unsafe.Pointer(&d.StringArray[0]))[:d.StringCount:d.StringCount] {
		units := (*[1 << 10]uint16)(unsafe.Pointer(v))[:]
		n := 0
		for units[n] != 0 {
			n++
		}
		fmt.Printf("%d %s\n", i, string(utf16.Decode(units[:n])))
	}

	pd := (*pointData)(unsafe.Pointer(base + 64))

	for i, p := range (*[1 << 10]*point)(unsafe.Pointer(&pd.Points[0]))[:pd.Count:pd.Count] {
		fmt.Printf("point %d %d %d\n", i, p.X, p.Y)
	}

	if err := syscall.Munmap(b); err != nil {
		panic(err)
	}
}
