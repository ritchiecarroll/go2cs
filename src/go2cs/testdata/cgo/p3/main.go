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
#include <stdint.h>
#include <stdlib.h>
#include <string.h>

const char *go2cs_probe_text(void);
char *go2cs_probe_text_copy(const char *text);
int go2cs_probe_text_fill(char *buffer, int size);
long go2cs_probe_sum(const void *bytes, int count);
void *go2cs_probe_same(void *pointer);
uintptr_t go2cs_probe_word(uintptr_t word);
void go2cs_probe_keep(void *pointer);
int go2cs_probe_kept(void);
char *go2cs_probe_log_new(void);
void go2cs_probe_log_put(char *log, int value);
void go2cs_probe_release(void *pointer);

struct go2cs_probe_entry
{
    int family;
    char *name;
    struct go2cs_probe_entry *next;
};

struct go2cs_probe_pair { int a; int b; };
int go2cs_probe_add_into(struct go2cs_probe_pair *dst, const struct go2cs_probe_pair *x, const struct go2cs_probe_pair *y);
int go2cs_probe_pair_and_int(struct go2cs_probe_pair *dst, int *n);

struct go2cs_probe_entry *go2cs_probe_list(void);
void go2cs_probe_list_release(struct go2cs_probe_entry *node);
int go2cs_probe_next_family(struct go2cs_probe_entry *node);
void *go2cs_probe_address(void);

struct go2cs_probe_pair
{
    int a;
    int b;
};

struct go2cs_probe_cursor
{
    int n;
    char *cur;
};

struct go2cs_probe_node
{
    int value;
    struct go2cs_probe_node *next;
};

int go2cs_probe_cursor_set(struct go2cs_probe_cursor *cursor, char *buffer, int k);
struct go2cs_probe_cursor go2cs_probe_cursor_at(char *buffer, int k);
int go2cs_probe_pick(int *a, int **out);
int go2cs_probe_pick_b(struct go2cs_probe_pair *pair, int **out);
void go2cs_probe_node_ring(struct go2cs_probe_node *node);
int go2cs_probe_pick_as_char(int *a, char **out);
int go2cs_probe_argv_count(char **argv);
int go2cs_probe_argv_count_void(void *argv);
int go2cs_probe_argv_fill(char **argv, int n);
void go2cs_probe_argv_self(char **argv);

struct go2cs_probe_gauge
{
	int count;
	long long total;
};

extern int go2cs_probe_counter;
extern struct go2cs_probe_gauge go2cs_probe_gauge_now;
extern int go2cs_probe_last_error;
int go2cs_probe_bump(int step);
*/
import "C"

import (
	"fmt"
	"os"
	"runtime"
	"syscall"
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

// goCHeader is goHeader with a C-typed array field, so &h.name[0] is a *C.char with no conversion: an element of a native
// window over a view of Go memory (P3's review of 1b, item 2).
type goCHeader struct {
	pad  int32
	n    int32
	name [8]C.char
}

// goArgvHost holds an array of C pointers as a FIELD, so &h.ptrs[0] is item 2's argv shape inside a Go struct (P3's
// unconfirmed row, carried by item 2).
type goArgvHost struct {
	n    int32
	ptrs [4]*C.char
}

// goRefHost holds a Go reference, so no door can pin a view of one of its fields: its storage is a managed object
// (P3's review of 1b, item 3).
type goRefHost struct {
	p *int
	n int32
	c int8
}

// goPairHost holds a Go array, and the converter lays it out inline (GoInlineLayout), so a view of it as a C struct
// pins its own storage and C gets one memory with no image: the control for the struct image door's arms, whose root
// is net's own syscall.RawSockaddrInet4, which the corpus emits with its arrays as managed references (19.9).
type goPairHost struct {
	a, b int32
	pad  [8]byte
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
	case "govoidnumberwrite":
		// S4 (design 20.4), an offset inside: a number one byte into a Go array, through a variable, names the array's
		// storage at that offset. memset writes two bytes there, and Go reads them back: [0 7 7 0].
		var x [4]byte
		n := uintptr(unsafe.Pointer(&x))
		C.memset(unsafe.Pointer(n+1), 7, 2)
		fmt.Println("govoidnumberwrite", x)
	case "wordtoken":
		// S4, P3's read: a uintptr_t parameter handed the number of a Go value whose type holds Go pointers. Go hands C
		// its address; go2cs's number for it is a token that names no memory, so the call is refused by name.
		s := []int{1}
		u := uintptr(unsafe.Pointer(&s))
		fmt.Println("wordtoken", uintptr(C.go2cs_probe_word(C.uintptr_t(u))) == u)
	case "pinnerkeep":
		// cgo S6 (design 21): C keeps a Go pointer past the call, which Go allows while a runtime.Pinner pins the memory.
		// A collection runs, Go writes through its own pointer, and C reads through the one it kept: 9 both ways.
		var pinner runtime.Pinner
		x := new(C.int)
		*x = 7
		pinner.Pin(x)
		C.go2cs_probe_keep(unsafe.Pointer(x))
		runtime.GC()
		*x = 9
		fmt.Println("pinnerkeep", int(C.go2cs_probe_kept()))
		C.go2cs_probe_keep(nil)
		pinner.Unpin()
	case "wordint":
		// Its near-miss: an ordinary integer crosses a uintptr_t parameter as it is.
		fmt.Println("wordint", uint64(C.go2cs_probe_word(C.uintptr_t(42))))
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
	// Increment 3.2 seat 1b (DESIGN-cgo-phase3-marshaling 18.12 item 5): cmd/cgo's element and slice check closures,
	// written as Go code writes them, so cmd/cgo itself makes the closures (checkIndex, checkSlice).
	case "goindex":
		// &b[2] straight into a void pointer: cmd/cgo's index form.
		b := make([]byte, 16)
		fmt.Println("goindex", C.go2cs_probe_same(unsafe.Pointer(&b[2])) != nil, len(b))
	case "goindexfill":
		// &b[4] as a typed pointer C writes through: the index form under conversions.
		b := make([]byte, 16)
		n := C.go2cs_probe_text_fill((*C.char)(unsafe.Pointer(&b[4])), 8)
		fmt.Printf("goindexfill %d %q\n", int(n), string(b[4:4+int(n)]))
	case "goviewcelemstring":
		// P3's review of 1b, item 2: C.GoString(&h.name[0]) with name [8]C.char, h a view over a Go slice, read by the
		// helper with no conversion. Refused before the fold (the helper asked the element with number 0).
		b := make([]byte, 16)
		copy(b[8:], "hi")
		h := (*goCHeader)(unsafe.Pointer(&b[0]))
		fmt.Printf("goviewcelemstring %q %d\n", C.GoString(&h.name[0]), len(b))
	case "goviewcelemfill":
		// The same element as a char * C fills, through a variable (written in the call, cmd/cgo checks the element).
		b := make([]byte, 16)
		h := (*goCHeader)(unsafe.Pointer(&b[0]))
		p := &h.name[0]
		n := C.go2cs_probe_text_fill(p, 8)
		fmt.Printf("goviewcelemfill %d %q\n", int(n), string(b[8:8+int(n)]))
	case "outstrtodelem":
		// P3's review of 1b, item 1: endptr is an element of a Go slice of C pointers, through a variable. Go writes it;
		// go2cs refuses by name, since the out-parameter's one-word mirror is not the array C may read or fill.
		cs := C.CString("1.5abc")
		defer C.free(unsafe.Pointer(cs))
		ends := make([]*C.char, 2)
		p := &ends[0]
		n := C.strtod(cs, p)
		fmt.Println("outstrtodelem", float64(n), C.GoString(ends[0]))
	case "outstrtodgo":
		// P3's review of 1b, item 4: strtod over a Go buffer leaves endptr INSIDE that buffer, which is Go memory. Go reads
		// it; go2cs refuses by name inside the call's pin, never minting a native box over Go memory that can move.
		b := []byte("2.5xyz\x00")
		var end *C.char
		n := C.strtod((*C.char)(unsafe.Pointer(&b[0])), &end)
		fmt.Println("outstrtodgo", float64(n), C.GoString(end))
	case "resultstrtokgo":
		// P3's read of the fold, 2a: a pointer RESULT inside the Go buffer C was handed. strtok skips the leading spaces
		// and returns &b[2]. Go prints the word; go2cs refuses by name inside the call's pin. strtok, not strchr: glibc's
		// C23 headers make strchr a const-preserving macro.
		b := []byte("  xy\x00")
		delim := C.CString(" ")
		defer C.free(unsafe.Pointer(delim))
		p := C.strtok((*C.char)(unsafe.Pointer(&b[0])), delim)
		fmt.Println("resultstrtokgo", C.GoString(p))
	case "resultstrcpyself":
		// Its near-miss: strcpy returns the destination it was handed, a pinned Go buffer. That is the identity return,
		// which maps back to the caller's own pointer and prints as Go does.
		b := make([]byte, 8)
		src := C.CString("abc")
		defer C.free(unsafe.Pointer(src))
		p := C.strcpy((*C.char)(unsafe.Pointer(&b[0])), src)
		fmt.Println("resultstrcpyself", C.GoString(p), string(b[:3]))
	case "goviewwide":
		// P3's read of the fold, 2b (item 3's shape): a char * over a 4-byte field of a struct that holds a Go reference.
		// Go lets C fill the field; go2cs refuses by name, since no door can pin it and a one-char copy would put C's
		// other bytes on the stack.
		s := goRefHost{p: new(int)}
		n := C.go2cs_probe_text_fill((*C.char)(unsafe.Pointer(&s.n)), 4)
		fmt.Println("goviewwide", int(n), s.n, s.p != nil)
	case "goviewsame":
		// Its near-miss: a char * over a one-byte field of the same struct keeps the one-char copy.
		s := goRefHost{p: new(int)}
		n := C.go2cs_probe_text_fill((*C.char)(unsafe.Pointer(&s.c)), 1)
		fmt.Println("goviewsame", int(n), s.c, s.p != nil)
	case "imgaddinto":
		// The struct image door, B5: three views of ONE Go value are one object to C, as under Go. The root is net's
		// sockaddr: a is Family (2, Port 0) and b is Addr (3) on a little-endian host. dst == x, and the second store
		// reads the first (a = 2+2, b = 4+3): Go prints 1 4 7, where three copies would read 0 4 5.
		sa := syscall.RawSockaddrInet4{Family: 2, Addr: [4]byte{3}}
		v := (*C.struct_go2cs_probe_pair)(unsafe.Pointer(&sa))
		same := C.go2cs_probe_add_into(v, v, v)
		fmt.Println("imgaddinto", int(same), sa.Family, sa.Addr[0])
	case "imgmixed":
		// B5's mixed case: a view of sa and a pointer into sa.Addr name one Go value. Go runs it on one memory; go2cs
		// refuses by name rather than hand C an image of sa and a pin of Addr, two copies of the same bytes.
		sa := syscall.RawSockaddrInet4{Family: 2, Addr: [4]byte{3}}
		v := (*C.struct_go2cs_probe_pair)(unsafe.Pointer(&sa))
		n := C.go2cs_probe_pair_and_int(v, (*C.int)(unsafe.Pointer(&sa.Addr)))
		fmt.Println("imgmixed", int(n), sa.Family, sa.Addr[0])
	case "inlinemixed":
		// The control: the same call over a root the converter lays out inline pins that root's own storage, so C gets
		// one memory and the program runs as Go does, with no image and no refusal.
		g := goPairHost{a: 2, b: 3}
		v := (*C.struct_go2cs_probe_pair)(unsafe.Pointer(&g))
		n := C.go2cs_probe_pair_and_int(v, (*C.int)(unsafe.Pointer(&g.b)))
		fmt.Println("inlinemixed", int(n), g.a, g.b)
	case "outstrtod":
		// The out-parameter, shape (i) (18.12 item 4): strtod's char ** endptr is &end, a Go variable C writes. strtod,
		// not strtol: glibc's C23 headers rename strtol to __isoc23_strtol, which go2cs refuses by name (GO2CS2001).
		cs := C.CString("1.5abc")
		defer C.free(unsafe.Pointer(cs))
		var end *C.char
		n := C.strtod(cs, &end)
		fmt.Println("outstrtod", float64(n), C.GoString(end))
	case "goslicefill":
		// unsafe.SliceData(b[4:]) as a typed pointer C writes through: cmd/cgo's slice form.
		b := make([]byte, 16)
		n := C.go2cs_probe_text_fill((*C.char)(unsafe.Pointer(unsafe.SliceData(b[4:]))), 8)
		fmt.Printf("goslicefill %d %q\n", int(n), string(b[4:4+int(n)]))
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
	// The .5 cgo items 1(a) and 1(b) (DESIGN-cgo-phase3-marshaling 18.12.3): pointers C hands BACK, inside a struct it
	// was handed, inside a struct it returns, and through an out-parameter. Go prints each; go2cs maps a stack copy's
	// own address to the argument and refuses an address inside Go memory or inside a copy, by name.
	case "membergo":
		// C writes &b[3] into a member of a Go struct: a pointer into the Go memory the call pinned.
		b := make([]byte, 8)
		var c C.struct_go2cs_probe_cursor
		k := C.go2cs_probe_cursor_set(&c, (*C.char)(unsafe.Pointer(&b[0])), 3)
		fmt.Println("membergo", int(k), int(c.n), c.cur == (*C.char)(unsafe.Pointer(&b[3])))
	case "memberc":
		// Its near-miss over C memory: the member is a C pointer.
		buf := (*C.char)(C.malloc(8))
		defer C.free(unsafe.Pointer(buf))
		var c C.struct_go2cs_probe_cursor
		k := C.go2cs_probe_cursor_set(&c, buf, 3)
		fmt.Println("memberc", int(k), int(c.n), c.cur == (*C.char)(unsafe.Add(unsafe.Pointer(buf), 3)))
	case "resultmembergo":
		// The same cursor returned by value, its member pointing into the Go buffer.
		b := make([]byte, 8)
		c := C.go2cs_probe_cursor_at((*C.char)(unsafe.Pointer(&b[0])), 5)
		fmt.Println("resultmembergo", int(c.n), c.cur == (*C.char)(unsafe.Pointer(&b[5])))
	case "resultmemberc":
		buf := (*C.char)(C.malloc(8))
		defer C.free(unsafe.Pointer(buf))
		c := C.go2cs_probe_cursor_at(buf, 5)
		fmt.Println("resultmemberc", int(c.n), c.cur == (*C.char)(unsafe.Add(unsafe.Pointer(buf), 5)))
	case "outself":
		// *out = a: out == &a, and a write through out is a write to a.
		a := C.int(41)
		var out *C.int
		r := C.go2cs_probe_pick(&a, &out)
		same := out == &a
		*out = 42
		fmt.Println("outself", int(r), same, int(a))
	case "outc":
		// Its near-miss: a in C memory, so out is that C pointer.
		ca := (*C.int)(C.malloc(4))
		defer C.free(unsafe.Pointer(ca))
		*ca = 7
		var out *C.int
		r := C.go2cs_probe_pick(ca, &out)
		fmt.Println("outc", int(r), out == ca)
	case "outothertype":
		// H's near-miss: *out is a's own address, through char * rather than int *. Go's answer is that address; go2cs has
		// no box of a's that names a char there, so it refuses by name rather than mapping.
		a := C.int(41)
		var out *C.char
		r := C.go2cs_probe_pick_as_char(&a, &out)
		fmt.Println("outothertype", int(r), unsafe.Pointer(out) == unsafe.Pointer(&a))
	case "outinterior":
		// *out = &pair.b, inside the struct C was handed: Go's answer is &pair.b, which go2cs refuses by name.
		pair := C.struct_go2cs_probe_pair{a: 1, b: 2}
		var out *C.int
		r := C.go2cs_probe_pick_b(&pair, &out)
		fmt.Println("outinterior", int(r), out == &pair.b)
	case "ringself":
		// node->next = node: the member is the node's own address.
		var n C.struct_go2cs_probe_node
		n.value = 9
		C.go2cs_probe_node_ring(&n)
		fmt.Println("ringself", int(n.value), n.next == &n)
	// The .5 cgo item 2, the argv shape (18.12.3; H's verdict (A), the word mirror): C reads or fills a whole Go array of
	// C pointers it was handed the first of. Go runs each; go2cs mirrors the backing array as words, pins it for the
	// call and copies it back, and refuses a Go pointer among the words (cgo's own panic) and a word C stores INTO the
	// array (inside the pin), by name.
	case "argvgo":
		a, bb := C.CString("a"), C.CString("bb")
		defer C.free(unsafe.Pointer(a))
		defer C.free(unsafe.Pointer(bb))
		argv := []*C.char{a, bb, nil}
		fmt.Println("argvgo", int(C.go2cs_probe_argv_count(&argv[0])))
	case "argvfill":
		z := C.CString("z")
		defer C.free(unsafe.Pointer(z))
		argv := []*C.char{nil, nil, z}
		n := C.go2cs_probe_argv_fill(&argv[0], 2)
		fmt.Println("argvfill", int(n), C.GoString(argv[0]), C.GoString(argv[1]), argv[2] == z)
	case "argvgoptr":
		// A Go pointer among the words: cgo's panic on both sides.
		b := []byte("g\x00")
		argv := []*C.char{(*C.char)(unsafe.Pointer(&b[0])), nil}
		fmt.Println("argvgoptr", int(C.go2cs_probe_argv_count(&argv[0])))
	case "argvself":
		// C stores &argv[2] into argv[0]. Go keeps it; go2cs refuses by name, since the mirror is one of the call's pins.
		argv := make([]*C.char, 3)
		C.go2cs_probe_argv_self(&argv[0])
		fmt.Println("argvself", unsafe.Pointer(argv[0]) == unsafe.Pointer(&argv[2]))
	case "argvc":
		// The near-miss: an argv in C memory, viewed as a Go slice, crosses by its own address.
		p := (**C.char)(C.malloc(C.size_t(3 * unsafe.Sizeof(uintptr(0)))))
		defer C.free(unsafe.Pointer(p))
		s := unsafe.Slice(p, 3)
		abc := C.CString("abc")
		defer C.free(unsafe.Pointer(abc))
		s[0], s[1], s[2] = abc, nil, nil
		fmt.Println("argvc", int(C.go2cs_probe_argv_count(&s[0])))
	case "argvfield":
		// The array as a field of a Go struct (P3's row).
		x, yy := C.CString("x"), C.CString("yy")
		defer C.free(unsafe.Pointer(x))
		defer C.free(unsafe.Pointer(yy))
		var h goArgvHost
		h.ptrs[0], h.ptrs[1] = x, yy
		fmt.Println("argvfield", int(C.go2cs_probe_argv_count(&h.ptrs[0])))
	case "argvvoid":
		// The void door.
		x, yy, zzz := C.CString("x"), C.CString("yy"), C.CString("zzz")
		defer C.free(unsafe.Pointer(x))
		defer C.free(unsafe.Pointer(yy))
		defer C.free(unsafe.Pointer(zzz))
		argv := []*C.char{x, yy, zzz, nil}
		fmt.Println("argvvoid", int(C.go2cs_probe_argv_count_void(unsafe.Pointer(&argv[0]))))
	case "cvar":
		// A C int global: read, written from Go, then changed by C and read again.
		before := int(C.go2cs_probe_counter)
		C.go2cs_probe_counter = 10
		after := int(C.go2cs_probe_bump(5))
		fmt.Println("cvar", before, after, int(C.go2cs_probe_counter))
	case "cvarstruct":
		// A C struct global: whole-value read, field write from Go, a write by C, and a whole-value write from Go.
		g := C.go2cs_probe_gauge_now
		C.go2cs_probe_gauge_now.total = 1000
		C.go2cs_probe_bump(2)
		h := C.go2cs_probe_gauge_now
		C.go2cs_probe_gauge_now = C.struct_go2cs_probe_gauge{count: 40, total: 41}
		fmt.Println("cvarstruct", int(g.count), int64(g.total), int(h.count), int64(h.total),
			int(C.go2cs_probe_gauge_now.count), int64(C.go2cs_probe_gauge_now.total))
	case "cvarerrno":
		// An errno-style global: C sets it on a refused call and clears it on the next good one.
		r := int(C.go2cs_probe_bump(-1))
		bad := int(C.go2cs_probe_last_error)
		C.go2cs_probe_bump(1)
		fmt.Println("cvarerrno", r, bad, int(C.go2cs_probe_last_error))
	case "cvaraddr":
		// The global's address: a Go pointer to C memory, the same address on each use, written through.
		p := &C.go2cs_probe_counter
		*p = 77
		fmt.Println("cvaraddr", p == &C.go2cs_probe_counter, int(C.go2cs_probe_counter))
	default:
		fmt.Println("unknown arm", arm)
		os.Exit(64)
	}
}
