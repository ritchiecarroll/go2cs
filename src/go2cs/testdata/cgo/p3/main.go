// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Command p3 is cgo Phase 3's end-to-end program for increment 3.1 (docs/phase4/DESIGN-cgo-phase3-marshaling.md §16):
// cmd/cgo's own helpers, C.malloc and C.free, a void pointer both ways, and the deferred C call. Its one argument names
// an arm, and it runs that arm alone, so an arm that panics takes no other with it. The test builds it with go and
// converts it with go2cs on the same host, runs each arm both ways and compares what they print.
//
// The last arms hand C, or one of the helpers, a pointer into Go memory. Go allows each of them. go2cs refuses each by
// name until increment 3.2 pins or images Go memory, so those arms are asserted one side at a time.
package main

/*
#include <stdlib.h>

const char *go2cs_probe_text(void);
char *go2cs_probe_text_copy(const char *text);
int go2cs_probe_text_fill(char *buffer, int size);
long go2cs_probe_sum(const void *bytes, int count);
void *go2cs_probe_same(void *pointer);
char *go2cs_probe_log_new(void);
void go2cs_probe_log_put(char *log, int value);
void go2cs_probe_release(void *pointer);

struct go2cs_probe_entry
{
    int family;
    char *name;
    struct go2cs_probe_entry *next;
};

struct go2cs_probe_entry *go2cs_probe_list(void);
void go2cs_probe_list_release(struct go2cs_probe_entry *node);
int go2cs_probe_next_family(struct go2cs_probe_entry *node);
void *go2cs_probe_address(void);
*/
import "C"

import (
	"fmt"
	"os"
	"unsafe"
)

// goAddress is a Go struct of scalars, the shape of go2cs_probe_address, that increment 3.2's reinterpret arm reads C
// memory through.
type goAddress struct {
	Family uint16
	Port   uint16
	Addr   uint32
}

// goHeader is a Go struct the derived-pointer arms view Go memory through.
type goHeader struct {
	pad  int32
	n    int32
	name [8]byte
}

// same returns the pointer it was handed: a user function whose result is an unsafe.Pointer.
//
//go:noinline
func same(p unsafe.Pointer) unsafe.Pointer { return p }

// entryNext returns the address of an entry's next field: net's _C_ai_next shape (go1.24.13 net/cgo_unix_cgo.go:71),
// which the resolver's walk loads through (DESIGN-cgo-phase3-marshaling 18.10, 18.11).
func entryNext(e *C.struct_go2cs_probe_entry) **C.struct_go2cs_probe_entry { return &e.next }

// setNext stores through a pointer to a C pointer: the out-parameter idiom inside Go code (18.11, [P3 S2]).
func setNext(pp **C.struct_go2cs_probe_entry, v *C.struct_go2cs_probe_entry) { *pp = v }

// roundTrip copies a Go string into C memory and reads it back up to its first NUL.
func roundTrip(s string) string {
	cs := C.CString(s)
	defer C.free(unsafe.Pointer(cs))

	return C.GoString(cs)
}

// deferred defers two C calls around a deferred Go function. The first deferred C call's argument is assigned again
// after its defer.
func deferred(log *C.char) {
	value := C.int(1)
	defer C.go2cs_probe_log_put(log, value)
	value = 9

	defer func() { C.go2cs_probe_log_put(log, 2) }()
	defer C.go2cs_probe_log_put(log, 3)
}

func main() {
	if len(os.Args) != 2 {
		fmt.Println("usage: p3 <arm>")
		os.Exit(64)
	}

	arm := os.Args[1]

	switch arm {
	case "text":
		// A C string the library owns, read into a Go string.
		fmt.Printf("text %q\n", C.GoString(C.go2cs_probe_text()))
	case "roundtrip":
		// A Go string to C memory and back, the free deferred.
		fmt.Printf("roundtrip %q %q\n", roundTrip("go2cs"), roundTrip(""))
	case "invalid":
		// The helpers copy bytes: text that is not UTF-8 comes back as it went.
		fmt.Printf("invalid %q\n", roundTrip("a\xffb\xc0\x80c"))
	case "nul":
		// C.CString copies the whole string, and C.GoString reads to the first NUL. C.GoStringN reads past it.
		s := "ab\x00cd"
		cs := C.CString(s)
		defer C.free(unsafe.Pointer(cs))
		fmt.Printf("nul %q %q\n", C.GoString(cs), C.GoStringN(cs, C.int(len(s))))
	case "nilstring":
		// A nil C string is the empty Go string.
		fmt.Printf("nilstring %q\n", C.GoString(nil))
	case "counted":
		// A counted read of C memory, as a string and as bytes.
		cs := C.CString("go2cs counted")
		defer C.free(unsafe.Pointer(cs))
		fmt.Printf("counted %q %v\n", C.GoStringN(cs, 5), C.GoBytes(unsafe.Pointer(cs), 5))
	case "zero":
		// A count of zero.
		cs := C.CString("go2cs")
		defer C.free(unsafe.Pointer(cs))
		b := C.GoBytes(unsafe.Pointer(cs), 0)
		fmt.Printf("zero %q %d %v\n", C.GoStringN(cs, 0), len(b), b == nil)
	case "negstring":
		// A negative count: how Go fails here is read on its own side first.
		cs := C.CString("go2cs")
		defer C.free(unsafe.Pointer(cs))
		fmt.Printf("negstring %q\n", C.GoStringN(cs, -1))
	case "negbytes":
		cs := C.CString("go2cs")
		defer C.free(unsafe.Pointer(cs))
		fmt.Printf("negbytes %v\n", C.GoBytes(unsafe.Pointer(cs), -1))
	case "cbytes":
		// A Go byte slice to C memory, summed by C behind a void pointer, read back, and freed by the caller.
		b := []byte{1, 0, 255, 2}
		p := C.CBytes(b)
		defer C.free(p)
		fmt.Println("cbytes", int64(C.go2cs_probe_sum(p, C.int(len(b)))), C.GoBytes(p, C.int(len(b))))
	case "emptybytes":
		// An empty slice: C.CBytes still allocates, and the caller still frees.
		p := C.CBytes(nil)
		defer C.free(p)
		fmt.Println("emptybytes", p != nil, int64(C.go2cs_probe_sum(p, 0)))
	case "copy":
		// C makes its own copy of a C string. The library that allocated it releases it, through a void pointer.
		cs := C.CString("copied by C")
		defer C.free(unsafe.Pointer(cs))
		copied := C.go2cs_probe_text_copy(cs)
		defer C.go2cs_probe_release(unsafe.Pointer(copied))
		fmt.Printf("copy %q %v\n", C.GoString(copied), copied != cs)
	case "fill":
		// C writes a string into a buffer from C.malloc.
		buffer := (*C.char)(C.malloc(8))
		defer C.free(unsafe.Pointer(buffer))
		n := C.go2cs_probe_text_fill(buffer, 8)
		fmt.Printf("fill %d %q\n", int(n), C.GoStringN(buffer, n))
	case "same":
		// A void pointer in and out: nil, C memory, and C memory through a uintptr held in a variable.
		p := C.malloc(8)
		defer C.free(p)
		number := uintptr(p)
		fmt.Println("same", C.go2cs_probe_same(nil) == nil, C.go2cs_probe_same(p) == p, uintptr(C.go2cs_probe_same(unsafe.Pointer(number))) == number)
	case "deferred":
		// Deferred C calls run last in, first out among Go's own, with the arguments they had at the defer: 3, 2, 1.
		log := C.go2cs_probe_log_new()
		defer C.go2cs_probe_release(unsafe.Pointer(log))
		deferred(log)
		fmt.Printf("deferred %q\n", C.GoString(log))
	case "innerdefer":
		// A deferred Go function that calls C further in is an ordinary defer. C memory crosses at the call inside it.
		p := C.malloc(8)
		defer C.free(p)

		func() {
			defer func() { fmt.Println("innerdefer", C.go2cs_probe_same(p) == p) }()
		}()
	case "innergo":
		// A goroutine on a Go function that calls C further in is an ordinary goroutine.
		p := C.malloc(8)
		defer C.free(p)
		done := make(chan bool)

		go func() { done <- C.go2cs_probe_same(p) == p }()

		fmt.Println("innergo", <-done)
	case "goinnerdefer":
		// The same deferred Go function over the address of a Go variable. Nothing about the defer lets it past the
		// void pointer's rule: go2cs refuses it at the call inside, when the deferred function runs.
		var x C.int = 7

		func() {
			defer func() { fmt.Println("goinnerdefer", C.go2cs_probe_same(unsafe.Pointer(&x)) != nil, int(x)) }()
		}()
	case "goinnergo":
		// And the same goroutine over the address of a Go variable.
		var x C.int = 7
		done := make(chan bool)

		go func() { done <- C.go2cs_probe_same(unsafe.Pointer(&x)) != nil }()

		fmt.Println("goinnergo", <-done, int(x))
	case "govoid":
		// The address of a Go variable through a void pointer. Go allows it; go2cs refuses it until 3.2.
		var x C.int = 7
		fmt.Println("govoid", C.go2cs_probe_same(unsafe.Pointer(&x)) != nil, int(x))
	case "govoidnumber":
		// The same address as a number held in a variable.
		var x C.int = 7
		number := uintptr(unsafe.Pointer(&x))
		fmt.Println("govoidnumber", C.go2cs_probe_same(unsafe.Pointer(number)) != nil, int(x))
	case "govoidexpr":
		// The same address through a uintptr and back in one expression.
		var x C.int = 7
		fmt.Println("govoidexpr", C.go2cs_probe_same(unsafe.Pointer(uintptr(unsafe.Pointer(&x)))) != nil, int(x))
	case "gobytes":
		// C.GoBytes over Go memory: a Go function reads it, and C is not involved. go2cs refuses it until 3.2.
		var x [4]C.char
		x[0], x[1] = 'g', 'o'
		fmt.Println("gobytes", C.GoBytes(unsafe.Pointer(&x), 2))
	case "gostring":
		// C.GoString over Go memory, the same way.
		var x [4]C.char
		x[0], x[1] = 'g', 'o'
		fmt.Printf("gostring %q\n", C.GoString((*C.char)(unsafe.Pointer(&x))))
	// The arms below hand C a pointer DERIVED from a view of Go memory, with no uintptr anywhere (the review of
	// increment 3.1, DESIGN-cgo-phase3-marshaling §16.11). Go allows each. go2cs refuses each until 3.2.
	case "goviewfield":
		// A field of a struct viewed over a slice's bytes.
		b := make([]byte, 16)
		h := (*goHeader)(unsafe.Pointer(&b[0]))
		fmt.Println("goviewfield", C.go2cs_probe_same(unsafe.Pointer(&h.n)) != nil, len(b))
	case "goviewfieldarray":
		// The same field, of a struct viewed over a Go array.
		var a [16]byte
		h := (*goHeader)(unsafe.Pointer(&a))
		fmt.Println("goviewfieldarray", C.go2cs_probe_same(unsafe.Pointer(&h.n)) != nil, len(a))
	case "goviewagain":
		// A second reinterpret of the view, read by a helper.
		b := make([]byte, 16)
		h := (*goHeader)(unsafe.Pointer(&b[0]))
		fmt.Printf("goviewagain %q %d\n", C.GoString((*C.char)(unsafe.Pointer(h))), len(b))
	case "goviewelem":
		// An element of an array field of the view, through a variable. Written in the call itself, `&h.name[1]` is an
		// element address cmd/cgo checks, and the route refuses that at conversion (read on the hosted leg 43375049312).
		b := make([]byte, 16)
		h := (*goHeader)(unsafe.Pointer(&b[0]))
		p := &h.name[1]
		fmt.Println("goviewelem", C.go2cs_probe_same(unsafe.Pointer(p)) != nil, len(b))
	case "goviewelemstring":
		// The same element, reinterpreted and read by a helper.
		b := make([]byte, 16)
		h := (*goHeader)(unsafe.Pointer(&b[0]))
		fmt.Printf("goviewelemstring %q %d\n", C.GoString((*C.char)(unsafe.Pointer(&h.name[0]))), len(b))
	case "goslice":
		// An element of unsafe.Slice over a view, through a variable, for the same reason.
		b := make([]byte, 16)
		sl := unsafe.Slice((*int32)(unsafe.Pointer(&b[0])), 4)
		p := &sl[1]
		fmt.Println("goslice", C.go2cs_probe_same(unsafe.Pointer(p)) != nil, len(b))
	case "goslicedata":
		// unsafe.SliceData of the same slice.
		b := make([]byte, 16)
		sl := unsafe.Slice((*int32)(unsafe.Pointer(&b[0])), 4)
		fmt.Println("goslicedata", C.go2cs_probe_same(unsafe.Pointer(unsafe.SliceData(sl))) != nil, len(b))
	case "goadd":
		// unsafe.Add over the address of a slice's element.
		b := make([]byte, 16)
		fmt.Println("goadd", C.go2cs_probe_same(unsafe.Add(unsafe.Pointer(&b[0]), 4)) != nil, len(b))
	case "goaddlocal":
		// unsafe.Add over the address of a local.
		var x [2]int64
		fmt.Println("goaddlocal", C.go2cs_probe_same(unsafe.Add(unsafe.Pointer(&x), 8)) != nil, len(x))
	// A pointer that reaches C as the RESULT OF A CALL (DESIGN-unsafe-pointer-call-result): through a variable, and
	// through a user function, for unsafe.Add and for a pointer derived from a view. goadd and goaddlocal above hand
	// the call's result to C as the argument itself.
	case "goaddvar":
		b := make([]byte, 16)
		p := unsafe.Add(unsafe.Pointer(&b[0]), 4)
		fmt.Println("goaddvar", C.go2cs_probe_same(p) != nil, len(b))
	case "goaddfunc":
		b := make([]byte, 16)
		fmt.Println("goaddfunc", C.go2cs_probe_same(same(unsafe.Add(unsafe.Pointer(&b[0]), 4))) != nil, len(b))
	case "goviewfieldfunc":
		b := make([]byte, 16)
		h := (*goHeader)(unsafe.Pointer(&b[0]))
		fmt.Println("goviewfieldfunc", C.go2cs_probe_same(same(unsafe.Pointer(&h.n))) != nil, len(b))
	case "goviewfieldvar":
		b := make([]byte, 16)
		h := (*goHeader)(unsafe.Pointer(&b[0]))
		p := same(unsafe.Pointer(&h.n))
		fmt.Println("goviewfieldvar", C.go2cs_probe_same(p) != nil, len(b))
	// The second review of increment 3.1 (DESIGN-cgo-phase3-marshaling §16.12), with no uintptr anywhere. MUST-FIX 1:
	// a typed pointer converted from an unsafe.Pointer VALUE that holds an interior Go address. MUST-FIX 2: an array
	// field of a struct viewed over Go memory. Go allows each, and go2cs is to refuse each until 3.2. The c* arms are
	// their twins over C memory, which run to their end both ways.
	case "goaddtyped":
		b := make([]byte, 16)
		copy(b[4:], "go")
		p := unsafe.Add(unsafe.Pointer(&b[0]), 4)
		fmt.Printf("goaddtyped %q %d\n", C.GoString((*C.char)(p)), len(b))
	case "goaddfill":
		// C writes through the typed pointer.
		b := make([]byte, 16)
		p := unsafe.Add(unsafe.Pointer(&b[0]), 4)
		n := C.go2cs_probe_text_fill((*C.char)(p), 8)
		fmt.Printf("goaddfill %d %q\n", int(n), string(b[4:4+int(n)]))
	case "gofieldtyped":
		b := make([]byte, 16)
		h := (*goHeader)(unsafe.Pointer(&b[0]))
		p := unsafe.Pointer(&h.n)
		n := C.go2cs_probe_text_fill((*C.char)(p), 4)
		fmt.Printf("gofieldtyped %d %q\n", int(n), string(b[4:4+int(n)]))
	case "goviewnamestring":
		b := make([]byte, 16)
		h := (*goHeader)(unsafe.Pointer(&b[0]))
		copy(h.name[:], "go")
		fmt.Printf("goviewnamestring %q %d\n", C.GoString((*C.char)(unsafe.Pointer(&h.name))), len(b))
	case "goviewnameadd":
		b := make([]byte, 16)
		h := (*goHeader)(unsafe.Pointer(&b[0]))
		fmt.Println("goviewnameadd", C.go2cs_probe_same(unsafe.Add(unsafe.Pointer(&h.name), 1)) != nil, len(b))
	case "goviewnameslice":
		b := make([]byte, 16)
		h := (*goHeader)(unsafe.Pointer(&b[0]))
		s := h.name[:]
		p := &s[1]
		fmt.Println("goviewnameslice", C.go2cs_probe_same(unsafe.Pointer(p)) != nil, len(b))
	case "goviewnamedata":
		// C writes through unsafe.SliceData of the field's slice.
		b := make([]byte, 16)
		h := (*goHeader)(unsafe.Pointer(&b[0]))
		s := h.name[:]
		n := C.go2cs_probe_text_fill((*C.char)(unsafe.Pointer(unsafe.SliceData(s))), 8)
		fmt.Printf("goviewnamedata %d %q\n", int(n), string(s[:n]))
	// Increment 3.2's first reds (DESIGN-cgo-phase3-marshaling section 6): C memory that Go reads. Go runs each to its
	// end; what go2cs does with each is READ before 3.2 changes anything.
	case "s6field":
		// A scalar field of a C struct that also holds pointer fields.
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		fmt.Println("s6field", int(l.family))
	case "s6next":
		// The address of a pointer field, loaded through.
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		p := &l.next
		fmt.Println("s6next", int((*p).family), C.GoString((*p).name))
	// s6next's discriminators (DESIGN-cgo-phase3-marshaling 18.1): which read of a pointer FIELD of C memory fails.
	case "s6nextnil":
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		fmt.Println("s6nextnil", l.next == nil)
	case "s6nextdirect":
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		fmt.Println("s6nextdirect", int(l.next.family))
	case "s6name":
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		fmt.Println("s6name", C.GoString(l.name))
	case "s6reinterp":
		// C memory reinterpreted as a Go struct of scalars, and a field read.
		raw := C.go2cs_probe_address()
		defer C.go2cs_probe_release(raw)
		sa := (*goAddress)(raw)
		fmt.Println("s6reinterp", sa.Family, sa.Port, sa.Addr)
	case "s6slice":
		// An element of a slice over C memory, reinterpreted as a C struct pointer and handed back to C: C memory.
		buf := C.malloc(64)
		defer C.free(buf)
		mem := unsafe.Slice((*byte)(buf), 64)
		r := (*C.struct_go2cs_probe_entry)(unsafe.Pointer(&mem[0]))
		fmt.Println("s6slice", C.go2cs_probe_same(unsafe.Pointer(r)) == buf)
	// Increment 3.2's stores into a pointer field of C memory (DESIGN-cgo-phase3-marshaling 18.9, D-4), each read back
	// in C.
	case "s6store":
		// nil, then a pointer to C memory: C reads each back.
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		second := l.next
		l.next = second.next
		none := int(C.go2cs_probe_next_family(l))
		l.next = second
		fmt.Println("s6store", none, int(C.go2cs_probe_next_family(l)))
	case "s6storego":
		// A pointer to Go memory. Go stores it, and C reads it back; g's last use keeps it alive until then. go2cs
		// refuses the store by name: C memory must not hold a Go pointer.
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		second := l.next
		g := &C.struct_go2cs_probe_entry{family: 7}
		l.next = g
		fmt.Println("s6storego", int(C.go2cs_probe_next_family(l)), int(g.family))
		l.next = second
	// Increment 3.2 seat 1c (DESIGN-cgo-phase3-marshaling 18.11, E-3): the load and the store through a pointer to a C
	// pointer.
	case "s6nextstore":
		// Through pp := &l.next: nil, then the C pointer back. C reads each.
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		pp := &l.next
		second := *pp
		*pp = nil
		none := int(C.go2cs_probe_next_family(l))
		*pp = second
		fmt.Println("s6nextstore", none, int(C.go2cs_probe_next_family(l)))
	case "s6nextstorego":
		// A Go pointer stored through pp. Go keeps it and C reads it back; go2cs refuses the store by name.
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		pp := &l.next
		second := *pp
		g := &C.struct_go2cs_probe_entry{family: 7}
		*pp = g
		fmt.Println("s6nextstorego", int(C.go2cs_probe_next_family(l)), int(g.family))
		*pp = second
	case "s6nextwalk":
		// The resolver's own walk: for r := res; r != nil; r = *_C_ai_next(r).
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		n, sum := 0, 0
		for r := l; r != nil; r = *entryNext(r) {
			n++
			sum += int(r.family)
		}
		fmt.Println("s6nextwalk", n, sum)
	case "s6nextset":
		// The out-parameter idiom: set(&l.next, v). Go stores through it; go2cs's ref-lowered call reaches the floor.
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		second := l.next
		setNext(&l.next, nil)
		none := int(C.go2cs_probe_next_family(l))
		setNext(&l.next, second)
		fmt.Println("s6nextset", none, int(C.go2cs_probe_next_family(l)))
	case "s6nexttuple":
		// A tuple target through pp keeps `.val`; over C memory the floor refuses it.
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		pp := &l.next
		second := *pp
		var n int
		*pp, n = nil, 1
		fmt.Println("s6nexttuple", n, int(C.go2cs_probe_next_family(l)))
		*pp = second
	case "s6nilslot":
		// A Go slot that holds a nil C pointer: *pp reads nil, and a C pointer stored there reads back.
		var p *C.struct_go2cs_probe_entry
		pp := &p
		isNil := *pp == nil
		l := C.go2cs_probe_list()
		defer C.go2cs_probe_list_release(l)
		*pp = l
		fmt.Println("s6nilslot", isNil, int((*pp).family))
	case "caddtyped":
		p := C.malloc(16)
		defer C.free(p)
		q := unsafe.Add(p, 4)
		n := C.go2cs_probe_text_fill((*C.char)(q), 8)
		fmt.Printf("caddtyped %d %q\n", int(n), C.GoString((*C.char)(q)))
	case "cfieldtyped":
		h := (*goHeader)(C.malloc(16))
		defer C.free(unsafe.Pointer(h))
		q := unsafe.Pointer(&h.n)
		n := C.go2cs_probe_text_fill((*C.char)(q), 4)
		fmt.Printf("cfieldtyped %d %q\n", int(n), C.GoString((*C.char)(q)))
	case "cviewnamestring":
		h := (*goHeader)(C.malloc(16))
		defer C.free(unsafe.Pointer(h))
		n := C.go2cs_probe_text_fill((*C.char)(unsafe.Pointer(&h.name)), 8)
		fmt.Printf("cviewnamestring %d %q\n", int(n), C.GoString((*C.char)(unsafe.Pointer(&h.name))))
	case "cviewnameadd":
		h := (*goHeader)(C.malloc(16))
		defer C.free(unsafe.Pointer(h))
		q := unsafe.Add(unsafe.Pointer(&h.name), 1)
		fmt.Println("cviewnameadd", C.go2cs_probe_same(q) == q)
	case "cviewnameslice":
		h := (*goHeader)(C.malloc(16))
		defer C.free(unsafe.Pointer(h))
		s := h.name[:]
		p := &s[1]
		fmt.Println("cviewnameslice", C.go2cs_probe_same(unsafe.Pointer(p)) == unsafe.Pointer(p))
	case "cviewnamedata":
		h := (*goHeader)(C.malloc(16))
		defer C.free(unsafe.Pointer(h))
		s := h.name[:]
		n := C.go2cs_probe_text_fill((*C.char)(unsafe.Pointer(unsafe.SliceData(s))), 8)
		fmt.Printf("cviewnamedata %d %q\n", int(n), string(s[:n]))
	default:
		fmt.Println("unknown arm", arm)
		os.Exit(64)
	}
}
