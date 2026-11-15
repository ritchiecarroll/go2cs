// Guards the ELEMENT DOORS: Go's `(*A)(p)[i]`, `&(*A)(p)[i]` and `*(*A)(p)`, A an array type and p an
// unsafe.Pointer, read and write the memory p names. The shapes are the ones ccgo writes all through
// modernc.org/sqlite and modernc.org/libc, where an array type is a way to index memory and no array of that type
// exists anywhere (docs/phase4/DESIGN-array-pointer-doors.md, door B).
//
// The first block is over NATIVE memory: pages syscall.Mmap hands back, which the kernel owns. The second is over
// GO memory: an array named by its number, and Go values of another type. In both, a store through the form must
// reach the memory and a read must see what the memory holds now. The forms the doors replace went through a box
// of the whole array, which native memory cannot answer: every line of the first block was a panic.
//
// PLATFORM-EXCLUSIVE: [GoPlatformExclusive("linux")], following ArrayPointerViewDoors. syscall.Mmap with MAP_ANON
// is not on Windows.
package main

import (
	"fmt"
	"runtime"
	"syscall"
	"unsafe"
)

// modernc.org/libc, libc64.go.
type RawMem [1<<50 - 1]byte

// A named array small enough to exist.
type smallMem [64]byte

// modernc.org/libc/uuid/uuid on linux: an alias, so the type is the unnamed array.
type uuidT = [16]uint8

type pair struct {
	a int32
	b int32
}

type hdr struct {
	a   uint32
	b   uint16
	c   uint16
	buf [8]byte
}

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

// ccgo's own helper: a number as a pointer.
func ptr(p uintptr) unsafe.Pointer {
	return unsafe.Pointer(p)
}

func twice(v int32) int32 {
	return v * 2
}

func last(v [4]byte) byte {
	return v[3]
}

func nativeMemory() {
	const xs = "XXXXXXXXXXXXXXXX"

	// libc's getcwd: a terminator stored at an index of the huge array.
	p := native(xs)
	wd := []byte("/tmp")
	(*RawMem)(unsafe.Pointer(p))[len(wd)] = 0
	fmt.Println("index store, huge array :", peek(p, 8))

	// The same store over memory that is still zero.
	z := native("")
	(*RawMem)(unsafe.Pointer(z))[4] = 7
	fmt.Println("index store, zeros      :", peek(z, 8))

	// A named array that fits.
	p = native(xs)
	(*smallMem)(unsafe.Pointer(p))[5] = 0
	fmt.Println("index store, small array:", (*smallMem)(unsafe.Pointer(p))[4], peek(p, 8))

	// sqlite: an unnamed array of int32, stored and read back, through ccgo's helper.
	p = native("")
	(*[4]int32)(ptr(p))[1] = 7
	fmt.Println("store, then read        :", (*[4]int32)(ptr(p))[1], (*[4]int32)(ptr(p))[0], peek(p, 8))

	// Compound assignment and increment go to the memory.
	(*[4]int32)(ptr(p))[1] += 3
	(*[4]int32)(ptr(p))[1]++
	(*[4]int32)(ptr(p))[2] |= 0x0102
	fmt.Println("compound and increment  :", (*[4]int32)(ptr(p))[1], peek(p+8, 4))

	// The read is an argument, and is bound to a variable that does not follow the memory.
	bound := (*[4]int32)(ptr(p))[1]
	(*[4]int32)(ptr(p))[1] = 100
	fmt.Println("argument and bound      :", twice((*[4]int32)(ptr(p))[1]), bound)

	// An index wider than int, and one computed from a uintptr.
	var wide uint64 = 3
	var off uintptr = 2
	(*[4]int32)(ptr(p))[wide] = -1
	fmt.Println("wide index              :", (*[4]int32)(ptr(p))[off+1], peek(p+12, 4))

	// The address of an element is the element's address: a store through it reaches the memory, and it can
	// be advanced.
	p = native("")
	e := &(*[4]int32)(ptr(p))[2]
	*e = 9
	next := (*int32)(unsafe.Add(unsafe.Pointer(e), 4))
	*next = 10
	fmt.Println("address of an element   :", *(*int32)(unsafe.Pointer(p + 8)), (*[4]int32)(ptr(p))[3], uintptr(unsafe.Pointer(e)) == p+8)

	// An element that is a struct of scalars: its fields are in the memory.
	p = native("")
	(*[3]pair)(ptr(p))[1].a = 5
	(*[3]pair)(ptr(p))[1].b = (*[3]pair)(ptr(p))[1].a + 1
	whole := (*[3]pair)(ptr(p))[1]
	fmt.Println("struct element          :", whole, peek(p+8, 8))

	// libc: a whole array stored through the conversion.
	p = native(xs)
	*(*[2]byte)(unsafe.Pointer(p)) = [2]byte{'C', 0}
	fmt.Println("whole array stored      :", peek(p, 8))

	// A whole array loaded is a copy: it does not follow the memory.
	p = native("abcdefgh")
	v := *(*[4]byte)(ptr(p))
	poke(p, "WXYZ")
	fmt.Printf("whole array loaded      : %q %s %q\n", v, peek(p, 4), last(*(*[4]byte)(ptr(p))))

	// One loaded and stored in one statement, and two compared.
	q := native("12345678")
	*(*[4]byte)(ptr(q + 4)) = *(*[4]byte)(ptr(p))
	fmt.Println("stored from a load      :", peek(q, 8))
	fmt.Println("two compared            :", *(*[4]byte)(ptr(q + 4)) == *(*[4]byte)(ptr(p)), *(*[4]byte)(ptr(q)) != *(*[4]byte)(ptr(p)))

	// The runtime's own shape: two words cleared one at a time, then as one array.
	p = native(xs)
	(*[2]uint64)(ptr(p))[0] = 0
	fmt.Println("one word cleared        :", peek(p, 16))
	*(*[2]uint64)(ptr(p)) = [2]uint64{}
	fmt.Println("both words cleared      :", peek(p, 16))

	// A named array, whole: stored and loaded as the named type. The loaded value is a copy.
	type sixteen [16]byte

	nm := native(xs)
	var id sixteen

	for i := range id {
		id[i] = byte('a' + i)
	}

	*(*sixteen)(ptr(nm)) = id
	back := *(*sixteen)(ptr(nm))
	back[0] = '!'
	fmt.Println("named array, whole      :", peek(nm, 17), string(back[:4]), *(*sixteen)(ptr(nm)) == id)

	// libc's uuid copy: an alias of an unnamed array, a number on each side.
	dst := native(xs)
	*(*uuidT)(unsafe.Pointer(dst)) = *(*uuidT)(unsafe.Pointer(nm))
	fmt.Println("alias of an array, whole:", peek(dst, 17))

	// An insert-shaped loop over a large array type.
	c := native("")
	var sum uint32

	for i := 0; i < 1000; i++ {
		(*[1 << 20]uint32)(ptr(c))[i] = uint32(i)
		sum += (*[1 << 20]uint32)(ptr(c))[i]
	}

	fmt.Println("loop of stores and reads:", sum, (*[1 << 20]uint32)(ptr(c))[999])
}

func goMemory() {
	// A Go array named by its number: the form is a window on the array.
	var arr [4]int32
	n := uintptr(unsafe.Pointer(&arr))
	(*[4]int32)(ptr(n))[2] = 5
	arr[3] = 6
	fmt.Println("array by its number     :", arr, (*[4]int32)(ptr(n))[3])

	// An element's number, and an index from there.
	var buf [8]byte
	m := uintptr(unsafe.Pointer(&buf[4]))
	(*[4]byte)(ptr(m))[1] = 'e'
	fmt.Printf("element by its number   : %q\n", buf)

	// A Go value of another type: a uint64 indexed as bytes, with a collection before the store.
	x := new(uint64)
	*x = 0x0102030405060708
	xp := unsafe.Pointer(x)
	runtime.GC()
	(*[8]byte)(xp)[0] = 0xff
	fmt.Printf("uint64 as bytes         : %#x %#x\n", *x, (*[8]byte)(xp)[7])

	// A struct of scalars indexed as bytes and as words.
	h := &hdr{a: 1, b: 2, c: 3}
	hp := unsafe.Pointer(h)
	(*[16]byte)(hp)[4] = 9
	(*[4]uint32)(hp)[0] += 10
	fmt.Println("struct as bytes, words  :", h.a, h.b, h.c, (*[8]uint16)(hp)[3])

	// The address of an element of a Go value, kept across a collection.
	e := &(*[8]uint16)(hp)[3]
	runtime.GC()
	*e = 77
	fmt.Println("address, after a GC     :", h.c)

	// A whole array stored into, and loaded from, a Go value of another type.
	y := new(uint32)
	yp := unsafe.Pointer(y)
	*(*[4]byte)(yp) = [4]byte{1, 2, 3, 4}
	stored := *y
	loaded := *(*[4]byte)(yp)
	*y = 0
	fmt.Printf("whole array, a uint32   : %#x %v\n", stored, loaded)

	// A whole array stored into a Go array of the same element by its number.
	var dst [4]int32
	*(*[4]int32)(ptr(uintptr(unsafe.Pointer(&dst)))) = [4]int32{1, 2, 3, 4}
	fmt.Println("whole array, by number  :", dst)
}

// lengths: Go answers the length of `*(*A)(p)` and of `(*A)(p)` from the type A, and a range over the dereference
// runs A's length, whatever the address holds: native memory, a Go array of another length, or nothing at all.
// Only a call inside the operand, or a range that reads the values, evaluates it. The conversion itself, ranged
// for its values, reads each element through the pointer.
func lengths() {
	p := native("0123456789")
	n := ptr(p)

	var arr [64]byte

	for i := range arr {
		arr[i] = byte(i + 1)
	}

	g := unsafe.Pointer(&arr)

	var none unsafe.Pointer

	fmt.Println("len, cap native         :", len(*(*[4]byte)(n)), cap(*(*[4]byte)(n)), len((*[4]byte)(n)), len(*(*smallMem)(n)))
	fmt.Println("len, cap Go, 64 behind  :", len(*(*[4]byte)(g)), cap(*(*[4]byte)(g)), cap((*[4]byte)(g)))
	fmt.Println("len, cap nil            :", len(*(*[4]byte)(none)), cap((*[4]byte)(none)))
	fmt.Println("len with a call         :", len(*(*[4]byte)(ptr(p))))

	sum, rounds := 0, 0

	for i := range *(*[4]byte)(g) {
		rounds++
		sum += i
	}

	fmt.Println("range index, 64 behind  :", rounds, sum)

	rounds = 0

	for range *(*[4]byte)(none) {
		rounds++
	}

	fmt.Println("range index, nil        :", rounds)

	sum, rounds = 0, 0

	for _, b := range *(*[4]byte)(g) {
		rounds++
		sum += int(b)
	}

	fmt.Println("range values, 64 behind :", rounds, sum)

	sum, rounds = 0, 0

	// Go ranges a copy of the array: the store in the body does not reach the values ranged.
	for i, b := range *(*[4]byte)(n) {
		(*[4]byte)(n)[3] = 'z'
		rounds++
		sum += i + int(b-'0')
	}

	fmt.Println("range values, native    :", rounds, sum, peek(p, 4))

	sum, rounds = 0, 0

	// The conversion itself, ranged: Go reads each element through the pointer as the loop reaches it, so the
	// store in the body is seen, and the loop still runs the type's length over the 64 bytes behind it.
	for i, b := range (*[4]byte)(g) {
		if i == 0 {
			(*[4]byte)(g)[3] = 50
		}

		rounds++
		sum += int(b)
	}

	fmt.Println("range pointer, 64 behind:", rounds, sum, arr[3])

	sum, rounds = 0, 0

	for _, b := range (*[4]byte)(n) {
		rounds++
		sum += int(b - '0')
	}

	fmt.Println("range pointer, native   :", rounds, sum)
}

func pastTheEnd() {
	defer func() {
		fmt.Println("index past the length   :", recover())
	}()

	p := native("")
	i := 4
	(*[4]int32)(ptr(p))[i] = 1
}

func negative() {
	defer func() {
		fmt.Println("negative index          :", recover())
	}()

	p := native("")
	i := -1
	_ = (*[4]int32)(ptr(p))[i]
}

func nilPointer() {
	defer func() {
		fmt.Println("nil pointer             :", recover())
	}()

	var p unsafe.Pointer
	(*[4]int32)(p)[1] = 1
}

func main() {
	nativeMemory()
	goMemory()
	lengths()
	pastTheEnd()
	negative()
	nilPointer()
}
