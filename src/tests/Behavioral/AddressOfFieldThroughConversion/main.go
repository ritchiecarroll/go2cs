// Guards the address of a field selected through a pointer CONVERSION, `&(*T)(p).f` (sqlite-cs W9b). go2cs
// rendered it as Ꮡ(<conversion>.Value.f): the address-of builtin takes its argument BY VALUE and boxes a COPY,
// so every write through the result was lost. It is the address-of form of sqlite's mutex.go owner checks
// (`atomic.StoreInt32(&(*mutex)(unsafe.Pointer(m)).id, tls.ID)`), of libc's pthread mutex owner and count, and
// of ccgo's union writes (`*(*int32)(unsafe.Pointer(&(*TMem)(unsafe.Pointer(p)).Fu)) = n`).
//
// Every shape writes through the address and then reads the field directly, so a write that landed in a copy
// cannot print the expected value. The memory here is the Go heap; AddressOfFieldThroughConversionNative runs
// the same shapes over pages the kernel owns.
//
// Three spellings reach the same defect and each has its arms: the conversion with its star left implicit,
// `&(*T)(p).f`; the star written out, `&(*(*T)(p)).f`, which ccgo emits under unsafe.Add; and the address of the
// indirection itself, `&*p`, which is p for ANY pointer (after Go's nil check) and was a box over a copy of the
// whole pointee. ccgo spells a C union that is the whole object the third way.
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

// a named pointer type: &*p on it yields a plain *rec
type recp *rec

// &*p with a nil p: Go evaluates the indirection, so it panics here although nothing is read.
//
//go:noinline
func same(p *rec) *rec { return &*p }

// a type parameter whose only type is a pointer: &*p is still p
func through[P *T, T any](p P) P { return &*p }

func nilIndirection() (text any) {
	defer func() { text = recover() }()

	var z *rec
	q := same(z)
	fmt.Println("unreachable:", q == nil)

	return nil
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

	// the star written out: &(*(*T)(p)).f
	atomic.StoreInt32(&(*(*rec)(gp)).id, 13)
	fmt.Println("explicit star:", g.id)

	// ccgo's union member at an offset: the explicit star under unsafe.Add
	*(*int32)(unsafe.Add(unsafe.Pointer(&(*(*rec)(gp)).cnt), 4)) = 14
	fmt.Println("explicit star, added:", g.id)

	// the address reinterpreted to ANOTHER type than the field's, then stored through
	*(*uint16)(unsafe.Pointer(&(*rec)(gp).id)) = 0xBEEF
	fmt.Printf("narrow store: %#x\n", g.id)

	// &*p is p: a write through it is a write through p
	r := &g
	s := &*r
	s.cnt = 21
	fmt.Println("address of indirection:", g.cnt, s == r)

	// ccgo's whole-object union: the address of the indirection of a conversion, reinterpreted
	*(*uint32)(unsafe.Pointer(&*(*rec)(gp))) = 31
	*(*uint32)(unsafe.Pointer(&*(*rec)(gp))) <<= 1
	fmt.Println("whole-object union:", g.cnt, *(*uint32)(unsafe.Pointer(&*(*rec)(gp))))

	// through a named pointer type, and through two indirections
	var np recp = &g
	t := &*np
	t.id = 22
	pp := &r
	u := &**pp
	u.cnt = 23
	fmt.Println("named pointer, two stars:", g.id, g.cnt, t == r, u == r)

	// through a type parameter
	w := through(r)
	w.id = 24
	fmt.Println("type parameter:", g.id, w == r)

	// increment and decrement: through a dereferenced conversion, and through an ordinary pointer
	*(*int32)(unsafe.Pointer(&(*rec)(gp).cnt))++
	*(*int32)(unsafe.Pointer(&(*rec)(gp).id))--
	ip := &g.cnt
	*ip++
	*ip++
	*ip--
	fmt.Println("increment:", g.cnt, g.id)

	// a nil pointer panics at the indirection
	fmt.Println("nil indirection:", nilIndirection())
}
