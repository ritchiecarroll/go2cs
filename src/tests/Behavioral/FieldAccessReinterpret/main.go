// Guards Go's field access through a pointer REINTERPRETED from unsafe.Pointer over a byte buffer -- os's
// createMountPoint shape, `buf := (*windows.MountPointReparseBuffer)(unsafe.Pointer(&byteblob[0]))` followed
// by `buf.SubstituteNameOffset = …`.
//
// A struct that holds an array field has a CLR layout that is not its Go layout: the array is a managed
// reference, which the CLR places first. The converted pointer aliases the pinned buffer, so a whole-value
// read through it refused by name, and a field's CLR slot is not where Go put the field anyway. The
// converter now reads each field through a generated field property, which reaches the field at its GO
// offset in native memory. Each field is checked against the buffer's own bytes, so a write that landed
// at a CLR offset, or nowhere, cannot print the expected lines.
//
// The local record covers a declared field, a compound assignment, an element and a slice of an array
// field, a field promoted through a value embed, and the address of a field. syscall.RawSockaddrInet4
// covers a struct from another package; its Port sits at Go offset 2 on every target.
package main

import (
	"fmt"
	"syscall"
	"unsafe"
)

type tail struct {
	A, B uint16
}

type record struct {
	Kind   uint32
	Length uint16
	Off    uint16
	Path   [4]uint16
	tail
}

func main() {
	blob := make([]byte, 24)
	rec := (*record)(unsafe.Pointer(&blob[0]))

	rec.Kind = 0x01020304
	rec.Length = 0x0506
	rec.Off = 7
	rec.Off += 1
	rec.Path[1] = 0x0a0b
	n := copy(rec.Path[2:], []uint16{0x0c0d})
	rec.B = 0x1112

	p := &rec.Length
	*p += 1

	fmt.Println("bytes:", blob[:20])
	fmt.Println("fields:", rec.Kind == 0x01020304, rec.Length, rec.Off, rec.Path[1], rec.Path[2], rec.B, n)

	raw := make([]byte, 16)
	sa := (*syscall.RawSockaddrInet4)(unsafe.Pointer(&raw[0]))
	sa.Port = 0x901f
	fmt.Println("port bytes:", raw[2], raw[3], "port:", sa.Port)
}
