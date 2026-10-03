// Guards Go's PORT ALIAS over a sockaddr that lives in NATIVE memory -- `(*[2]byte)(unsafe.Pointer(&sa.Port))`,
// the shape net's darwin cgoLookupServicePort (net/cgo_unix.go:154/158) reads over the addrinfo chain libc
// returns, and syscall's sockaddr encoders write.
//
// The converted alias failed twice over a native root. The array view had no array<T> to build -- the floor
// refused it by name, "cannot view native memory as array<Byte>", which is LookupServicePort's darwin
// reading -- and the address it would view was the field's CLR slot, base plus the field's CLR offset,
// which is not Go's for a struct carrying array fields. The converter now emits golib's native field-view
// door ahead of the raw route, which views the field at its GO offset, and reads the alias through
// ElementRef.
//
// This row is the arm that runs off darwin. syscall.Mmap hands back pages the kernel owns, and a
// RawSockaddrInet4 laid over them is a native root, exactly as libc's sockaddr is on darwin. On linux the
// struct keeps Port at its Go offset by coincidence (a two-byte family precedes it), so this row measures
// the door and the read; the offset defect itself is measured on darwin's declaration by GolibTests'
// NativeFieldArrayViewTests and on darwin by LookupServicePort.
//
// The bytes are written and read through TWO aliases and also read directly off the mapping, so a write
// that missed the mapping, or a read from anywhere but Port, cannot print the expected lines.
//
// PLATFORM-EXCLUSIVE: [GoPlatformExclusive("linux")], following SendtoSeam's recorded convention. syscall.Mmap
// with MAP_ANON is not on Windows, and the package's emission differs by platform.
package main

import (
	"fmt"
	"syscall"
	"unsafe"
)

func main() {
	page, err := syscall.Mmap(-1, 0, 4096, syscall.PROT_READ|syscall.PROT_WRITE, syscall.MAP_ANON|syscall.MAP_PRIVATE)

	if err != nil {
		fmt.Println("mmap:", err)
		return
	}

	defer syscall.Munmap(page)

	sa := (*syscall.RawSockaddrInet4)(unsafe.Pointer(&page[0]))

	// The write, as syscall's sockaddr encoders spell it: big-endian through the alias.
	port := 8080
	p := (*[2]byte)(unsafe.Pointer(&sa.Port))
	p[0] = byte(port >> 8)
	p[1] = byte(port)

	fmt.Println("bytes at Go offsets 2 and 3:", page[2], page[3])

	// The read, as net's cgoLookupServicePort spells it, through a second alias.
	q := (*[2]byte)(unsafe.Pointer(&sa.Port))
	fmt.Println("port read through the alias:", int(q[0])<<8|int(q[1]))

	// Nothing else moved: the alias wrote two bytes, at the field.
	fmt.Println("bytes at 0, 1, 4:", page[0], page[1], page[4])
}
