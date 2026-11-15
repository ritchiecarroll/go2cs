// Guards the VIEW DOORS: Go's `(*A)(p)[lo:hi]`, A an array type and p an unsafe.Pointer, is a slice that ALIASES the
// memory p names. The shapes are the ones modernc.org/libc and modernc.org/sqlite write over `RawMem`, an array type
// far larger than any memory, used only for the slices cut from it (docs/phase4/DESIGN-array-pointer-doors.md, door A).
//
// The first block views NATIVE memory: pages syscall.Mmap hands back, which the kernel owns. The second views GO
// memory: an array reached through an element's address, through the array's own address, and through those
// addresses kept as numbers. In both, a write through the view must reach the memory, and a later write to the
// memory must show through the view. The form the doors replace built the slice from a copy, so every line below
// that prints what a write left behind printed the memory unchanged.
//
// The open form `(*RawMem)(p)[:]` has a length nothing can hold; Go only ever uses it under copy, which takes the
// other operand's length. Those are the "open" lines.
//
// PLATFORM-EXCLUSIVE: [GoPlatformExclusive("linux")], following NativeViewPointerEquality. syscall.Mmap with MAP_ANON
// is not on Windows. The Go-memory lines need no native memory, and PointerArrayViewForms and
// PointerCastSliceReinterpret carry the door emission on every platform.
package main

import (
	"fmt"
	"runtime"
	"syscall"
	"unsafe"
)

// modernc.org/libc, libc64.go.
type RawMem [1<<50 - 1]byte

// A named array small enough to exist: the open form away from copy is the whole array from its low bound.
type smallMem [64]byte

type hdr struct {
	a   uint32
	buf [12]byte
}

var chachaInit = [4]uint32{0x61707865, 0x3320646e, 0x79622d32, 0x6b206574}
var magic = [8]uint8{0xd9, 0xd5, 0x05, 0xf9, 0x20, 0xa1, 0x63, 0xd7}

// native answers the address of 4096 bytes the kernel owns, filled with fill.
func native(fill string) uintptr {
	b, err := syscall.Mmap(-1, 0, 4096, syscall.PROT_READ|syscall.PROT_WRITE, syscall.MAP_ANON|syscall.MAP_PRIVATE)

	if err != nil {
		panic(err)
	}

	p := uintptr(unsafe.Pointer(&b[0]))
	poke(p, fill)

	return p
}

func peek(p uintptr, n int) string {
	out := make([]byte, n)

	for i := 0; i < n; i++ {
		out[i] = *(*byte)(unsafe.Pointer(p + uintptr(i)))
	}

	return fmt.Sprintf("%q", out)
}

func poke(p uintptr, s string) {
	for i := 0; i < len(s); i++ {
		*(*byte)(unsafe.Pointer(p + uintptr(i))) = s[i]
	}
}

func fill(b []byte, c byte) int {
	for i := range b {
		b[i] = c
	}

	return len(b)
}

// libc's memcpy: both sides are views of a number.
func memcpy(dest, src uintptr, n uintptr) uintptr {
	if n != 0 {
		copy((*RawMem)(unsafe.Pointer(dest))[:n:n], (*RawMem)(unsafe.Pointer(src))[:n:n])
	}

	return dest
}

func nativeMemory() {
	const xs = "XXXXXXXXXXXXXXXX"
	q := native("abcdefghijklmnop")

	// A string copied into a three-index view.
	p := native(xs)
	s := "hello"
	n := len(s)
	c := copy((*RawMem)(unsafe.Pointer(p))[:n:n], s)
	fmt.Println("string into a view      :", c, peek(p, 8))

	// One view copied into another.
	p = native(xs)
	n = 6
	c = copy((*RawMem)(unsafe.Pointer(p))[:n:n], (*RawMem)(unsafe.Pointer(q))[:n:n])
	fmt.Println("view into a view        :", c, peek(p, 8))

	// The open form as copy's destination, over filled and over zero memory.
	p = native(xs)
	wd := []byte("/tmp")
	c = copy((*RawMem)(unsafe.Pointer(p))[:], wd)
	fmt.Println("open destination        :", c, peek(p, 8))
	z := native("")
	c = copy((*RawMem)(unsafe.Pointer(z))[:], wd)
	fmt.Println("open destination, zeros :", c, peek(z, 8))

	// The open form with a low bound, and a string source.
	p = native(xs)
	c = copy((*RawMem)(unsafe.Pointer(p))[2:], "lo")
	fmt.Println("open destination from 2 :", c, peek(p, 8))

	// The open form as copy's source.
	v := make([]byte, 6)
	c = copy(v, (*RawMem)(unsafe.Pointer(q))[:])
	fmt.Printf("open source             : %d %q\n", c, v)
	c = copy(v, (*RawMem)(unsafe.Pointer(q))[10:])
	fmt.Printf("open source from 10     : %d %q\n", c, v)

	// A two-index view written element by element, then re-sliced: the capacity is the array's.
	p = native(xs)
	b := (*RawMem)(unsafe.Pointer(p))[:5]

	for i := range b {
		b[i] = 0
	}

	b[0] = 'u'
	b = b[1:]
	fmt.Println("element writes, capacity:", len(b), cap(b) == 1<<50-2, peek(p, 8))

	// A view handed to a function that fills it.
	p = native(xs)
	fmt.Println("filled by a callee      :", fill((*RawMem)(unsafe.Pointer(p))[:4], 'r'), peek(p, 8))

	// A view as copy's source.
	c = copy(v, (*RawMem)(unsafe.Pointer(q))[:6:6])
	fmt.Printf("view as a source        : %d %q\n", c, v)

	// The memory changes after the view is made, and the view is written after that.
	k := native("abcdefghijklmnop")
	mem := (*RawMem)(unsafe.Pointer(k))[:4:4]
	poke(k, "WXYZ")
	mem[3] = '!'
	fmt.Printf("memory changed later    : %q %s\n", mem, peek(k, 4))

	// A bound that is a uintptr constant.
	p = native(xs)
	var r [16]byte

	for i := range r {
		r[i] = byte('0' + i%10)
	}

	c = copy((*RawMem)(unsafe.Pointer(p))[:unsafe.Sizeof(r)], r[:])
	fmt.Println("uintptr bound           :", c, peek(p, 16))

	// A named array that fits: its open form away from copy is the whole array from the low bound.
	p = native(xs)
	whole := (*smallMem)(unsafe.Pointer(p))[60:]
	whole[0] = 's'
	fmt.Println("open form of a small one:", len(whole), cap(whole), peek(p+60, 4))

	// A three-index view with a low bound: the capacity runs from the low bound to max.
	t := (*RawMem)(unsafe.Pointer(q))[2:5:9]
	fmt.Printf("low bound and max       : %d %d %q\n", len(t), cap(t), t)

	fmt.Println("size of the array type  :", uint64(unsafe.Sizeof(RawMem{})))
}

func goMemory() {
	// A view at an element of a Go array is a window on the array.
	var buf [16]byte
	c := copy((*RawMem)(unsafe.Pointer(&buf[0]))[:len(buf):len(buf)], "errno 7\x00")
	fmt.Printf("element of an array     : %d %q\n", c, buf[:8])

	// A field of a heap struct, with a collection between the view and the write.
	h := &hdr{}
	v := (*RawMem)(unsafe.Pointer(&h.buf[0]))[:12:12]
	runtime.GC()
	copy(v, "field")
	fmt.Printf("field, after a GC       : %q\n", h.buf[:6])

	// An interior address made by unsafe.Add.
	heap := make([]byte, 32)
	w := (*RawMem)(unsafe.Add(unsafe.Pointer(&heap[0]), 4))[:8:8]
	runtime.GC()
	copy(w, "interior")
	fmt.Printf("unsafe.Add, after a GC  : %q\n", heap[:14])

	// The open form over a Go array under copy, both ways, and from a low bound that leaves four elements.
	var name [16]byte
	n := copy((*RawMem)(unsafe.Pointer(&name[0]))[:], "name\x00")
	fmt.Printf("open destination        : %d %q\n", n, name[:6])
	var out [4]byte
	m := copy(out[:], (*RawMem)(unsafe.Pointer(&name[0]))[:])
	fmt.Printf("open source             : %d %q\n", m, out[:])
	k := copy((*RawMem)(unsafe.Pointer(&name[0]))[12:], "wxyz")
	fmt.Printf("open destination from 12: %d %q\n", k, name[10:])

	// Both sides named by a NUMBER: a byte array by its own address, and by an element's.
	var dst [8]byte
	memcpy(uintptr(unsafe.Pointer(&dst)), uintptr(unsafe.Pointer(&magic)), 8)
	fmt.Println("array's own number      :", dst)
	var half [8]byte
	memcpy(uintptr(unsafe.Pointer(&half[4])), uintptr(unsafe.Pointer(&magic[2])), 4)
	fmt.Println("element's number        :", half)

	// The source is Go storage of another type: its bytes are read.
	var words [16]byte
	memcpy(uintptr(unsafe.Pointer(&words)), uintptr(unsafe.Pointer(&chachaInit)), 16)
	fmt.Println("source of another type  :", words)
}

func bounds() {
	defer func() {
		fmt.Println("past the array's length :", recover())
	}()

	p := native("")
	n := 65
	_ = (*smallMem)(unsafe.Pointer(p))[:n]
}

func main() {
	nativeMemory()
	goMemory()
	bounds()
}
