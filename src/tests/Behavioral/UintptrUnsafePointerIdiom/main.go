// Guards the converter's dead-`unsafe.Pointer`-object peephole.
//
// Go's syscall idiom `uintptr(unsafe.Pointer(x))` used to convert to
// `(uintptr)new @unsafe.Pointer(x)`, materializing a wrapper whose only job was to hold the address
// the constructor had ALREADY computed and the enclosing cast then read straight back out. The
// wrapper is emitted no longer: the operand converts to uintptr directly, by the same operator, so
// the address — and the pin the operand's own box holds behind it — are unchanged.
//
// Every assertion below is an IDENTITY between two addresses, or a stride between two addresses in
// one allocation, so the program prints the same booleans under `go run` and under the converted C#
// even though the numbers themselves differ. The operand kinds the peephole covers are all here —
// address-of, a pointer parameter, the nil pointer, an array element, an existing unsafe.Pointer
// value, and a dereferenced pointer-to-pointer, which is the one shape the emission has to
// parenthesize for itself — alongside pointer arithmetic written in one expression, which is emitted
// as unsafe.Add over the pointer, and the same arithmetic over the nil pointer, which is the offset.
package main

import (
	"fmt"
	"unsafe"
)

// Reference-free, so the managed model can hold its storage still and report one stable address for
// it (a struct carrying references has no native layout, and no address a syscall could be given).
type point struct{ x, y int32 }

var origin = point{x: 3, y: 4}

// A pointer PARAMETER operand: the parameter's BOX is what carries the address, so `unsafe.Pointer`
// must never reach through it to the pointed-to value — including when it is nil.
func addrOf(p *point) uintptr {
	return uintptr(unsafe.Pointer(p))
}

func main() {
	// 1. A package-level variable's address, taken twice: one storage, one address.
	g1 := uintptr(unsafe.Pointer(&origin))
	g2 := uintptr(unsafe.Pointer(&origin))
	fmt.Println("global:", g1 != 0, g1 == g2)

	// 2. A pointer parameter — and the nil pointer, whose address Go defines as 0.
	link := &origin
	fmt.Println("param:", addrOf(link) == g1)

	var missing *point
	fmt.Println("nil param:", addrOf(missing) == 0)

	// 3. Two elements of one fixed array: a real stride apart only if both addresses name the same
	//    backing storage and that storage is held still while they are read.
	var buf [4]int32
	e0, e2 := &buf[0], &buf[2]
	a0 := uintptr(unsafe.Pointer(e0))
	a2 := uintptr(unsafe.Pointer(e2))
	fmt.Println("stride:", a0 != 0, a2-a0 == 2*unsafe.Sizeof(buf[0]))

	// 4. An operand that is ALREADY an unsafe.Pointer value round-trips exactly.
	up := unsafe.Pointer(e0)
	fmt.Println("round trip:", uintptr(up) == a0)

	// 5. Pointer arithmetic in one expression: the sum is emitted as unsafe.Add over the pointer, and
	//    the enclosing conversion reads the moved pointer's number. The stride from element 0 lands
	//    on element 2 only if the moved pointer names the same storage as the pointer it came from.
	shifted := uintptr(unsafe.Pointer(uintptr(unsafe.Pointer(e0)) + 2*unsafe.Sizeof(buf[0])))
	fmt.Println("arithmetic:", shifted == a2)

	// 6. A DEREFERENCED pointer-to-pointer operand: a pointer expression that is not primary, and
	//    the one such shape the wrapper-free emission has to parenthesize for itself.
	pp := &link
	fmt.Println("deref operand:", uintptr(unsafe.Pointer(*pp)) == g1)

	// 7. The address names LIVE storage: written and read back through the same pointer.
	link.y = 7
	fmt.Println("written:", link.x, link.y, addrOf(link) == g1)

	// 8. Arithmetic over the nil pointer is the offset, as a number: Go defines `uintptr(nil)` as 0, and
	//    the sum of 0 and an offset is that offset whichever way the pointer is moved.
	var none unsafe.Pointer
	offset := 3 * unsafe.Sizeof(buf[0])
	fmt.Println("nil arithmetic:", uintptr(unsafe.Pointer(uintptr(none)+offset)) == offset, uintptr(unsafe.Add(none, 24)) == 24)
}
