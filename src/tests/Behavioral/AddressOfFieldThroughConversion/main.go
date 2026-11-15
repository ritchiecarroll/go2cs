// Guards the address of a field selected through a pointer CONVERSION, `&(*T)(p).f` (sqlite-cs W9b). go2cs
// rendered it as Ꮡ(<conversion>.Value.f): the address-of builtin takes its argument BY VALUE and boxes a COPY,
// so every write through the result was lost. It is the address-of form of sqlite's mutex.go owner checks
// (`atomic.StoreInt32(&(*mutex)(unsafe.Pointer(m)).id, tls.ID)`), of libc's pthread mutex owner and count, and
// of ccgo's union writes (`*(*int32)(unsafe.Pointer(&(*TMem)(unsafe.Pointer(p)).Fu)) = n`).
//
// Every shape writes through the address and then reads the field directly, so a write that landed in a copy
// cannot print the expected value. The memory here is the Go heap; AddressOfFieldThroughConversionNative runs
// the same shapes over pages the kernel owns.
package main

import (
	"fmt"
	"sync/atomic"
	"unsafe"
)

type rec struct {
	cnt int32
	id  int32
}

// same underlying type as rec: (*rec)(p2) is a conversion between two named pointer types, no unsafe.
type rec2 struct {
	cnt int32
	id  int32
}

type outer struct {
	tag string
	in  rec
}

func main() {
	var g rec
	gp := unsafe.Pointer(&g)

	// atomic store through the address, then a plain read
	atomic.StoreInt32(&(*rec)(gp).id, 3)
	fmt.Println("atomic store:", g.id)

	// ccgo's union write: the address reinterpreted, then stored through
	*(*int32)(unsafe.Pointer(&(*rec)(gp).cnt)) = 11
	fmt.Println("union write:", g.cnt)

	// a kept address: written through later, and the same pointer as &g.id
	q := &(*rec)(gp).id
	*q = 9
	fmt.Println("kept address:", g.id, q == &g.id)

	// a parenthesized operand, as libc spells its pthread owner CAS
	fmt.Println("parenthesized CAS:", atomic.CompareAndSwapInt32(&((*rec)(gp)).id, 9, 4), g.id)

	// a field of a value field: &(*T)(p).a.b
	var o outer
	op := unsafe.Pointer(&o)
	atomic.AddInt32(&(*outer)(op).in.cnt, 5)
	fmt.Println("nested field:", o.in.cnt)

	// a conversion between named pointer types with one underlying type
	var h rec2
	atomic.StoreInt32(&(*rec)(&h).cnt, 6)
	fmt.Println("named pointer conversion:", h.cnt)

	// control: a pointer-returning CALL base already aliased before this fix
	mk := func() *rec { return &g }
	atomic.AddInt32(&mk().cnt, 1)
	fmt.Println("call base:", g.cnt)
}
