// The native-memory arm of AddressOfFieldThroughConversion (sqlite-cs W9b): `&(*T)(p).f` where p addresses pages
// the kernel owns, as libc's calloc'd memory does for sqlite's mutexes and Mem cells. go2cs rendered the address
// as Ꮡ(<conversion>.Value.f), a box over a COPY of the struct, so a write through it never reached the mapping.
// Each line writes through the address and reads back off the mapping's own bytes.
//
// The second half is ccgo's own spelling of a C union, over a uintptr as ccgo holds its pointers. A union member
// is the address of a struct of the union's size, reinterpreted: `*(*X)(unsafe.Pointer(&(*T)(unsafe.Pointer(p)).Fu))
// = v` (every integer literal of a sqlite expression is stored this way, Expr.u.iValue, and read 0); the same with
// the star written out under unsafe.Add; and, where the union is the whole object, the address of the indirection,
// `&*(*struct{ Fi [0]uint64; Ff float64 })(p)`, which libc's fmod and remquo shift through.
//
// PLATFORM-EXCLUSIVE: [GoPlatformExclusive("linux")], following NativeFieldPortAlias. syscall.Mmap with MAP_ANON
// is not on Windows.
package main

import (
	"encoding/binary"
	"fmt"
	"sync/atomic"
	"syscall"
	"unsafe"
)

type rec struct {
	cnt int32
	id  int32
}

// ccgo's spelling of `struct Expr { u8 op; u32 flags; union { int iValue; char *zToken; } u; ... }`.
type expr struct {
	op    uint8
	flags uint32
	u     struct {
		iValue [0]int32
		zToken uintptr
	}
	left uintptr
	w    struct {
		iOfst [0]int32
		iJoin int32
	}
	n int32
}

func store(a uintptr, v int32) { *(*int32)(unsafe.Pointer(a)) = v }

func main() {
	page, err := syscall.Mmap(-1, 0, 4096, syscall.PROT_READ|syscall.PROT_WRITE, syscall.MAP_ANON|syscall.MAP_PRIVATE)
	if err != nil {
		fmt.Println("mmap:", err)
		return
	}
	defer syscall.Munmap(page)

	p := unsafe.Pointer(&page[0])

	atomic.StoreInt32(&(*rec)(p).id, 7)
	fmt.Println("atomic store:", (*rec)(p).id, binary.LittleEndian.Uint32(page[4:]))

	*(*int32)(unsafe.Pointer(&(*rec)(p).cnt)) = 11
	fmt.Println("union write:", (*rec)(p).cnt, binary.LittleEndian.Uint32(page[0:]))

	q := &(*rec)(p).id
	*q = 9
	fmt.Println("kept address:", (*rec)(p).id, binary.LittleEndian.Uint32(page[4:]), uintptr(unsafe.Pointer(q))-uintptr(p))

	fmt.Println("parenthesized CAS:", atomic.CompareAndSwapInt32(&((*rec)(p)).id, 9, 4), binary.LittleEndian.Uint32(page[4:]))

	// ccgo's unions: b is a uintptr, and e the bytes it addresses
	b := uintptr(p) + 256
	e := page[256:]
	le := binary.LittleEndian

	// the union store: sqlite's Expr.u.iValue
	*(*int32)(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).u)) = 77
	fmt.Println("union store:", le.Uint32(e[8:]))

	// the union read
	le.PutUint32(e[8:], 78)
	fmt.Println("union read:", *(*int32)(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).u)))

	// compound assignment and increment through the union member
	*(*int32)(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).u)) += 2
	*(*int32)(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).u))++
	fmt.Println("union op-assign:", le.Uint32(e[8:]))

	// and decrement, twice
	*(*int32)(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).u))--
	*(*int32)(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).u))--
	fmt.Println("union decrement:", le.Uint32(e[8:]))

	// the pointer-sized member of the same union
	*(*uintptr)(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).u)) = 0x1122334455
	fmt.Println("union pointer member:", le.Uint64(e[8:]) == 0x1122334455)

	// a second union further down the struct, and a plain field after it
	*(*int32)(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).w)) = 41
	(*expr)(unsafe.Pointer(b)).n = 42
	fmt.Println("second union:", le.Uint32(e[24:]), le.Uint32(e[28:]))

	// a scalar field reinterpreted to another width
	*(*uint16)(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).flags)) = 0xBEEF
	fmt.Printf("narrow store: %#x\n", le.Uint32(e[4:]))

	// the address as a number: handed to a callee, and compared with the struct's own address
	store(uintptr(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).w)), 43)
	fmt.Println("address as uintptr:", le.Uint32(e[24:]), uintptr(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).w))-b)

	// an atomic operation on the reinterpreted address
	atomic.AddInt32((*int32)(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).w)), 2)
	fmt.Println("atomic on member:", le.Uint32(e[24:]))

	// a kept reinterpreted address, written through later
	k := (*int32)(unsafe.Pointer(&(*expr)(unsafe.Pointer(b)).u))
	*k = 5
	fmt.Println("kept member address:", le.Uint32(e[8:]))

	// the star written out under unsafe.Add, as ccgo stores a pointer member
	*(*uintptr)(unsafe.Add(unsafe.Pointer(&(*(*expr)(unsafe.Pointer(b))).u), 0)) = 0x66
	*(*int32)(unsafe.Add(unsafe.Pointer(&(*(*expr)(unsafe.Pointer(b))).u), 4)) = 0x67
	fmt.Printf("explicit star: %#x\n", le.Uint64(e[8:]))

	// the union that is the whole object: libc's fmod shifts the bits of a double this way
	d := b + 64
	*(*float64)(unsafe.Pointer(d)) = 1.5
	*(*uint64)(unsafe.Pointer(&*(*struct {
		i [0]uint64
		f float64
	})(unsafe.Pointer(d)))) <<= 1
	fmt.Printf("whole-object union: %#x %#x\n", le.Uint64(e[64:]), *(*uint64)(unsafe.Pointer(&*(*struct {
		i [0]uint64
		f float64
	})(unsafe.Pointer(d)))))

	// control: the member reached through a pointer VARIABLE, which aliased before this fix
	v := (*expr)(unsafe.Pointer(b))
	*(*int32)(unsafe.Pointer(&v.u)) = 6
	fmt.Println("pointer variable base:", le.Uint32(e[8:]))
}
