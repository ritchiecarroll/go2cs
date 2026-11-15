// Guards the FIELD VIEW over a GO struct: Go's `(*[N]T)(unsafe.Pointer(&p.f))`, f not a T, where p points at an
// ordinary Go value. It is how plain Go reads the bytes of one field: the port of a sockaddr, the two halves of a
// 64-bit word (docs/phase4/DESIGN-array-pointer-doors.md, section 18).
//
// The converted view answered only over NATIVE memory (NativeFieldPortAlias). Over a Go struct with no reference
// in it, every form but len failed: the view was a box that read the field's bytes as an array header, so an index
// panicked with a length that was never the view's, and a range by value ran no iterations and said nothing. And
// the view indexed where it is written, with no local, was refused over native memory too.
//
// The view has no array value of its own to hand out, so a form that needs the whole array goes through the
// elements: a dereference read for its value copies them, a store through the dereference writes them, and a
// range that reads the values ranges them where they are.
//
// Each block below is the same forms over one kind of root: a struct on the heap, a package variable, an element
// of a slice of structs, and a struct laid over a Go byte slice. A store through the view must reach the field,
// and nothing but the field. The view is a view of memory, so the lines are the same on every little-endian
// target, which all of go2cs's targets are.
package main

import (
	"fmt"
	"unsafe"
)

type hdr struct {
	family uint16
	pad    uint16
	port   uint16
	tail   uint16
	word   uint64
}

var global = hdr{port: 0x1234, tail: 0x5678, word: 0x1111111122222222}

// ---- the view indexed where it is written

func directRead(h *hdr) byte {
	return (*[2]byte)(unsafe.Pointer(&h.port))[1]
}

func directWrite(h *hdr, v byte) {
	(*[2]byte)(unsafe.Pointer(&h.port))[0] = v
}

func directOpAssign(h *hdr) {
	(*[2]byte)(unsafe.Pointer(&h.port))[1] += 2
	(*[2]byte)(unsafe.Pointer(&h.port))[1]++
}

func directDeref(h *hdr) uint32 {
	(*(*[2]uint32)(unsafe.Pointer(&h.word)))[0]++
	return (*(*[2]uint32)(unsafe.Pointer(&h.word)))[0]
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

func directWord(h *hdr) uint32 {
	return (*[2]uint32)(unsafe.Pointer(&h.word))[1]
}

// ---- the view bound to a local

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

// runtime's cheaprand, the step it takes on a 32-bit target.
func localWord(h *hdr) uint32 {
	t := (*[2]uint32)(unsafe.Pointer(&h.word))
	s1, s0 := t[0], t[1]
	s1 ^= s1 << 17
	s1 = s1 ^ s0 ^ s1>>7 ^ s0>>16
	t[0], t[1] = s0, s1
	return s0 + s1
}

func localSlice(h *hdr) []byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return p[:]
}

// A view longer than its field reads the fields after it.
func localPastTheField(h *hdr) uint16 {
	p := (*[4]byte)(unsafe.Pointer(&h.port))
	p[3] = 0x9A
	return h.tail
}

// ---- the whole array through the view: a load copies its N elements, a store writes them

func wholeDirect(h *hdr) [2]byte {
	return *(*[2]byte)(unsafe.Pointer(&h.port))
}

func wholeLocal(h *hdr) [2]byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return *p
}

func wholeWord(h *hdr) [2]uint32 {
	return *(*[2]uint32)(unsafe.Pointer(&h.word))
}

// The load is a copy: a store through the view after it does not reach it.
func wholeIsACopy(h *hdr) (byte, byte) {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	a := *p
	p[0]++
	return a[0], p[0]
}

func wholeCompare(h *hdr) bool {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return *p == [2]byte{byte(h.port), byte(h.port >> 8)}
}

func sum2(a [2]byte) int {
	return int(a[0]) + int(a[1])
}

func wholeArgument(h *hdr) int {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return sum2(*p)
}

func wholeStoreDirect(h *hdr, v [2]byte) {
	*(*[2]byte)(unsafe.Pointer(&h.port)) = v
}

func wholeStoreLocal(h *hdr, v [2]byte) {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	*p = v
}

// ---- a range that reads the values

func rangeValue(h *hdr) int {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	sum := 0

	for _, b := range p {
		sum += int(b)
	}

	return sum
}

func rangeValueDirect(h *hdr) int {
	sum := 0

	for i, b := range (*[2]byte)(unsafe.Pointer(&h.port)) {
		sum += int(b) << (8 * i)
	}

	return sum
}

// A range over a pointer to an array reads each element when its round comes.
func rangeValueSeesAStore(h *hdr) int {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	sum := 0

	for i, b := range p {
		if i == 0 {
			p[1] = 0x40
		}

		sum += int(b)
	}

	return sum
}

// A range over the dereference ranges a copy, taken before the first round.
func rangeDerefIsACopy(h *hdr) int {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	sum := 0

	for i, b := range *p {
		if i == 0 {
			p[1] = 0x50
		}

		sum += int(b)
	}

	return sum
}

// ---- the dereference of a local, indexed and sliced: no copy, the view's own elements

func derefIndex(h *hdr) byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	(*p)[0] = 0x77
	(*p)[1]++
	return (*p)[0]
}

func derefSlice(h *hdr) []byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return (*p)[:1]
}

// ---- a struct literal's field values, a sent value, and the forms for which Go reads no array

type raw struct {
	port [2]byte
	word [8]byte
}

func loadInLiteral(h *hdr) raw {
	return raw{
		port: *(*[2]byte)(unsafe.Pointer(&h.port)),
		word: *(*[8]byte)(unsafe.Pointer(&h.word)),
	}
}

func loadSent(h *hdr) [2]byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	ch := make(chan [2]byte, 1)
	ch <- (*p)
	return <-ch
}

func derefUnread(h *hdr) int {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	n := len(*p) * 10

	for i := range *p {
		n += i
	}

	return n
}

func arms(root string, h *hdr) {
	h.family, h.pad, h.port, h.tail, h.word = 2, 0, 0x1234, 0x5678, 0x1111111122222222

	fmt.Printf("%s direct read [1]: %#x\n", root, directRead(h))
	directWrite(h, 0xAB)
	fmt.Printf("%s direct write [0]: port %#x\n", root, h.port)
	*directAddr(h) = 0xCD
	fmt.Printf("%s direct address: port %#x\n", root, h.port)
	directOpAssign(h)
	fmt.Printf("%s direct compound and step: port %#x\n", root, h.port)
	fmt.Printf("%s direct explicit dereference: %#x, word %#x\n", root, directDeref(h), h.word)
	fmt.Printf("%s direct len: %d\n", root, directLen(h))
	s := directSlice(h)
	s[0] = 0x11
	fmt.Printf("%s direct slice: len %d cap %d port %#x\n", root, len(s), cap(s), h.port)
	fmt.Printf("%s direct word [1]: %#x\n", root, directWord(h))

	fmt.Printf("%s local read [1]: %#x\n", root, localRead(h))
	localWrite(h, 0x22)
	fmt.Printf("%s local write [0]: port %#x\n", root, h.port)
	fmt.Printf("%s local range by index: %#x\n", root, localRange(h))
	fmt.Printf("%s local word: %#x, word %#x\n", root, localWord(h), h.word)
	s = localSlice(h)
	s[1] = 0x33
	fmt.Printf("%s local slice: len %d cap %d port %#x\n", root, len(s), cap(s), h.port)
	fmt.Printf("%s local view past the field: tail %#x\n", root, localPastTheField(h))
	h.port, h.word = 0x1234, 0x1111111122222222
	fmt.Printf("%s whole load: %v %v\n", root, wholeDirect(h), wholeLocal(h))
	fmt.Printf("%s whole load of a word: %#x\n", root, wholeWord(h))
	before, after := wholeIsACopy(h)
	fmt.Printf("%s whole load is a copy: %#x then %#x, port %#x\n", root, before, after, h.port)
	fmt.Printf("%s whole load compared: %v, as an argument: %#x\n", root, wholeCompare(h), wholeArgument(h))
	wholeStoreDirect(h, [2]byte{0xA1, 0xA2})
	fmt.Printf("%s whole store: port %#x\n", root, h.port)
	wholeStoreLocal(h, [2]byte{0xB1, 0xB2})
	fmt.Printf("%s whole store through a local: port %#x, tail %#x\n", root, h.port, h.tail)
	fmt.Printf("%s range by value: %#x, where written: %#x\n", root, rangeValue(h), rangeValueDirect(h))
	fmt.Printf("%s range by value sees a store: %#x, port %#x\n", root, rangeValueSeesAStore(h), h.port)
	fmt.Printf("%s range of the dereference is a copy: %#x, port %#x\n", root, rangeDerefIsACopy(h), h.port)
	fmt.Printf("%s dereference indexed: %#x, port %#x\n", root, derefIndex(h), h.port)
	s = derefSlice(h)
	s[0] = 0x66
	fmt.Printf("%s dereference sliced: len %d cap %d port %#x\n", root, len(s), cap(s), h.port)
	h.word = 0x0102030405060708
	fmt.Printf("%s in a struct literal: %v, sent: %v\n", root, loadInLiteral(h), loadSent(h))
	fmt.Printf("%s length and index range of the dereference: %d\n", root, derefUnread(h))
	fmt.Printf("%s nothing else moved: family %d pad %d\n", root, h.family, h.pad)
}

func main() {
	// A Go struct with no reference in it, on the heap.
	arms("heap", &hdr{})

	// A package variable.
	arms("package", &global)

	// An element of a slice of structs: the root that is held still is the slice's array.
	hs := make([]hdr, 3)
	arms("element", &hs[1])
	fmt.Println("its neighbours:", hs[0], hs[2])

	// A struct laid over a Go byte slice.
	buf := make([]byte, 32)
	arms("over bytes", (*hdr)(unsafe.Pointer(&buf[8])))
	fmt.Println("the bytes before and after it:", buf[:8], buf[24:])
}
