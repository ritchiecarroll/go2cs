// Guards the FIELD VIEW over a Go struct that HOLDS A REFERENCE once converted: Go's
// `(*[N]T)(unsafe.Pointer(&x.f))`, f not a T, where x's struct carries an array, a string or a pointer beside f.
// It is how golang.org/x/sys writes the port of a socket address: two bytes, in network order, through a view of
// the port field of a raw sockaddr whose address is an array (docs/phase4/DESIGN-array-pointer-doors.md,
// section 19).
//
// A converted struct that holds a reference has no address: the collector may move it, and nothing can hold it
// still. So no view of its memory can be made, and the form was refused by name. The view is now over the FIELD's
// own bytes, read again at every access: a store through it is a store to the field and to nothing else, the
// field's own accesses and the view's see each other, and a collection between a store and a read changes
// nothing. The view's address is the field's, so it compares equal to the field's pointer, as in Go.
//
// The view is a view of memory, so the lines are the same on every little-endian target, which all of go2cs's
// targets are.
package main

import (
	"fmt"
	"runtime"
	"unsafe"
)

// x/sys's raw socket address. The address is an array, so the converted struct holds a reference.
type rawInet4 struct {
	family uint16
	port   uint16
	addr   [4]byte
	zero   [8]uint8
}

type pair struct {
	a uint16
	b uint16
}

// A struct with a string beside its numbers, and a field that is itself a struct with no reference in it.
type record struct {
	label string
	word  uint32
	both  pair
	tail  uint16
}

var global = rawInet4{family: 2, port: 0x1234, addr: [4]byte{10, 0, 0, 1}}

// ---- x/sys's port: written in network order through the view, read back the same way

func encodePort(raw *rawInet4, port int) {
	p := (*[2]byte)(unsafe.Pointer(&raw.port))
	p[0] = byte(port >> 8)
	p[1] = byte(port)
}

func decodePort(raw *rawInet4) int {
	p := (*[2]byte)(unsafe.Pointer(&raw.port))
	return int(p[0])<<8 + int(p[1])
}

// ---- the view indexed where it is written

func directRead(raw *rawInet4) byte {
	return (*[2]byte)(unsafe.Pointer(&raw.port))[1]
}

func directWrite(raw *rawInet4, v byte) {
	(*[2]byte)(unsafe.Pointer(&raw.port))[0] = v
}

func directOpAssign(raw *rawInet4) {
	(*[2]byte)(unsafe.Pointer(&raw.port))[1] += 2
	(*[2]byte)(unsafe.Pointer(&raw.port))[1]++
}

func directLen(raw *rawInet4) int {
	return len((*[2]byte)(unsafe.Pointer(&raw.port)))
}

// ---- the field's own accesses and the view's name one storage

func aliasBothWays(raw *rawInet4) (byte, byte, uint16) {
	p := (*[2]byte)(unsafe.Pointer(&raw.port))
	raw.port = 0xBEEF
	lo, hi := p[0], p[1]
	p[1] = 0x01
	return lo, hi, raw.port
}

func twoViews(raw *rawInet4) byte {
	p := (*[2]byte)(unsafe.Pointer(&raw.port))
	q := (*[2]byte)(unsafe.Pointer(&raw.port))
	q[0] = 0x77
	return p[0]
}

// ---- other field kinds: a word as four bytes, a struct field as its halves and as its bytes

func wordBytes(r *record) (byte, byte) {
	p := (*[4]byte)(unsafe.Pointer(&r.word))
	p[3] = 0x7F
	return p[0], p[3]
}

func structHalves(r *record) uint16 {
	p := (*[2]uint16)(unsafe.Pointer(&r.both))
	p[1] = 0x3333
	return p[0]
}

func structBytes(r *record) byte {
	p := (*[4]byte)(unsafe.Pointer(&r.both))
	return p[3]
}

// ---- the whole array: a load is a copy, a store is N element stores

func wholeLoad(raw *rawInet4) [2]byte {
	p := (*[2]byte)(unsafe.Pointer(&raw.port))
	v := *p
	p[0] = 0xAB
	return v
}

func wholeStore(raw *rawInet4, v [2]byte) {
	p := (*[2]byte)(unsafe.Pointer(&raw.port))
	*p = v
}

func countRounds(raw *rawInet4) int {
	p := (*[2]byte)(unsafe.Pointer(&raw.port))
	n := 0

	for i := range p {
		n += i + 1
	}

	return n + len(p)
}

// ---- a collection between a store and a read

func storeCollectRead(raw *rawInet4, v byte) (byte, byte, uint16) {
	p := (*[2]byte)(unsafe.Pointer(&raw.port))
	p[0] = v
	p[1] = v + 1
	runtime.GC()
	lo, hi := p[0], p[1]
	p[1] = 0x5A
	runtime.GC()
	return lo, hi, raw.port
}

// ---- the view's address is the field's

func sameAsItsField(raw *rawInet4) bool {
	p := (*[2]byte)(unsafe.Pointer(&raw.port))
	return unsafe.Pointer(p) == unsafe.Pointer(&raw.port)
}

func sameAsAnotherField(raw *rawInet4) bool {
	p := (*[2]byte)(unsafe.Pointer(&raw.port))
	return unsafe.Pointer(p) == unsafe.Pointer(&raw.family)
}

func sameAcrossStructs(x, y *rawInet4) bool {
	p := (*[2]byte)(unsafe.Pointer(&x.port))
	q := (*[2]byte)(unsafe.Pointer(&y.port))
	return p == q || unsafe.Pointer(p) == unsafe.Pointer(&y.port)
}

func twoViewsAreOnePointer(raw *rawInet4) bool {
	p := (*[2]byte)(unsafe.Pointer(&raw.port))
	q := (*[2]byte)(unsafe.Pointer(&raw.port))
	f := (*[2]byte)(unsafe.Pointer(&raw.family))
	return p == q && p != f && p != nil
}

// ---- a chain of field selections: x/sys/unix writes the port two selections deep, `&sa.raw.Port`
// (syscall_linux.go:394), and the root may be a struct variable or a package variable

type sockInet4 struct {
	port int
	addr [4]byte
	raw  rawInet4
}

func (sa *sockInet4) sockaddr() {
	p := (*[2]byte)(unsafe.Pointer(&sa.raw.port))
	p[0] = byte(sa.port >> 8)
	p[1] = byte(sa.port)
	sa.raw.addr = sa.addr
}

func anyToPort(sa *sockInet4) int {
	p := (*[2]byte)(unsafe.Pointer(&sa.raw.port))
	return int(p[0])<<8 + int(p[1])
}

func twoDeepDirect(sa *sockInet4) byte {
	(*[2]byte)(unsafe.Pointer(&sa.raw.port))[1]++
	return (*[2]byte)(unsafe.Pointer(&sa.raw.port))[1]
}

func variableRoot() uint16 {
	var s rawInet4
	p := (*[2]byte)(unsafe.Pointer(&s.port))
	p[0] = 0xCD
	p[1] = 0x12
	return s.port
}

func packageRoot() uint16 {
	p := (*[2]byte)(unsafe.Pointer(&global.port))
	p[1] = 0x77
	return global.port
}

// An array field viewed as an array of another element type of its size, in a struct variable: os's readdir on
// darwin reads a directory entry's name this way (dir_darwin.go:77). The view is of the field's own array.
func arrayFieldOfVariable() string {
	var raw rawInet4
	raw.addr = [4]byte{0x7f, 0xff, 0, 1}
	p := (*[4]int8)(unsafe.Pointer(&raw.addr))
	p[3] = -2
	s := (*[4]int8)(unsafe.Pointer(&raw.addr))[:]
	return fmt.Sprint(p[0], p[1], s[3], raw.addr[3], len(s))
}

// ---- an index outside the view is Go's panic

func outOfRange(raw *rawInet4, i int) (text string) {
	defer func() {
		text = fmt.Sprint(recover())
	}()

	p := (*[2]byte)(unsafe.Pointer(&raw.port))
	return fmt.Sprint(p[i])
}

func main() {
	raw := &rawInet4{family: 2, port: 0x1234, addr: [4]byte{127, 0, 0, 1}}

	encodePort(raw, 8080)
	fmt.Printf("encode: port=%#04x decode=%d family=%d addr=%v\n", raw.port, decodePort(raw), raw.family, raw.addr)

	raw.port = 0x1234
	fmt.Printf("direct read: %#02x\n", directRead(raw))
	directWrite(raw, 0xCD)
	directOpAssign(raw)
	fmt.Printf("direct write: port=%#04x len=%d\n", raw.port, directLen(raw))

	lo, hi, port := aliasBothWays(raw)
	fmt.Printf("alias: %#02x %#02x port=%#04x\n", lo, hi, port)
	fmt.Printf("two views: %#02x port=%#04x\n", twoViews(raw), raw.port)

	r := &record{label: "r", word: 0x0A0B0C0D, both: pair{a: 0x1111, b: 0x2222}, tail: 0x5678}
	b0, b3 := wordBytes(r)
	fmt.Printf("word: %#02x %#02x word=%#08x\n", b0, b3, r.word)
	fmt.Printf("struct field: %#04x b=%#04x byte=%#02x tail=%#04x label=%s\n", structHalves(r), r.both.b, structBytes(r), r.tail, r.label)

	raw.port = 0x1234
	v := wholeLoad(raw)
	fmt.Printf("whole load: %#02x %#02x port=%#04x\n", v[0], v[1], raw.port)
	wholeStore(raw, [2]byte{1, 2})
	fmt.Printf("whole store: port=%#04x family=%d addr=%v rounds=%d\n", raw.port, raw.family, raw.addr, countRounds(raw))

	lo, hi, port = storeCollectRead(raw, 0x41)
	fmt.Printf("collect: %#02x %#02x port=%#04x addr=%v\n", lo, hi, port, raw.addr)

	other := &rawInet4{port: 0x1234}
	fmt.Printf("address: field=%v otherField=%v otherStruct=%v twoViews=%v\n", sameAsItsField(raw), sameAsAnotherField(raw), sameAcrossStructs(raw, other), twoViewsAreOnePointer(raw))

	encodePort(&global, 443)
	fmt.Printf("global: port=%#04x decode=%d addr=%v\n", global.port, decodePort(&global), global.addr)

	structs := []rawInet4{{port: 1}, {port: 2}}
	encodePort(&structs[1], 0x0102)
	fmt.Printf("element: %#04x %#04x\n", structs[0].port, structs[1].port)

	sa := &sockInet4{port: 8443, addr: [4]byte{192, 168, 0, 1}}
	sa.sockaddr()
	fmt.Printf("two deep: raw.port=%#04x decode=%d raw.addr=%v\n", sa.raw.port, anyToPort(sa), sa.raw.addr)
	fmt.Printf("two deep direct: %#02x raw.port=%#04x\n", twoDeepDirect(sa), sa.raw.port)
	fmt.Printf("variable: %#04x package: %#04x\n", variableRoot(), packageRoot())
	fmt.Printf("array field of a variable: %s\n", arrayFieldOfVariable())

	fmt.Printf("out of range: %s\n", outOfRange(raw, 2))
}
