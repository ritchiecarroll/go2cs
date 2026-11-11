// fixture.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Package cgophase2 is cgo Phase 2's extern-only fixture: every C function it calls is declared by a header and
// defined by a library, so each is one the converter binds rather than one it would have to compile. The first group
// is the system C library's. The second is the probe library's (src/tests/GolibTests/cgo-probe/probe.c), declared
// here as its header would declare it, so that each of cmd/cgo's call rewrites appears in this file's generated tree:
// the pointer-check closure in each of its forms, and the shapes Phase 2 refuses (docs/phase4/DESIGN-cgo-phase2-pinvoke.md
// §7, §11.7, §11.16, as amended).
package cgophase2

/*
#cgo LDFLAGS: -lm
#include <math.h>
#include <stdlib.h>
#include <time.h>

struct go2cs_probe_pair { int a; int b; };
struct go2cs_probe_node { int value; struct go2cs_probe_node *next; };
struct go2cs_probe_link { int value; struct go2cs_probe_node *node; };

long go2cs_probe_node_sum(struct go2cs_probe_node *node, long base);
void go2cs_probe_node_touch(struct go2cs_probe_node *node);
int go2cs_probe_link_value(struct go2cs_probe_link link);
int go2cs_probe_pair_scaled(struct go2cs_probe_pair pair, int k);
*/
import "C"

import (
	"runtime"
	"unsafe"
)

// Abs passes and returns a C int.
func Abs(x int) int { return int(C.abs(C.int(x))) }

// Labs passes and returns a C long, whose width is the target's.
func Labs(x int64) int64 { return int64(C.labs(C.long(x))) }

// Div returns a struct of integers by value.
func Div(a, b int) (int, int) {
	r := C.div(C.int(a), C.int(b))
	return int(r.quot), int(r.rem)
}

// Cos passes and returns a C double.
func Cos(x float64) float64 { return float64(C.cos(C.double(x))) }

// Exp calls a C function in Go's two-result form, which reads errno.
func Exp(x float64) (float64, error) {
	r, err := C.exp(C.double(x))
	return float64(r), err
}

// Frexp passes a pointer to one C int. Its pointee holds no pointer, so cmd/cgo checks nothing.
func Frexp(x float64) (float64, int) {
	var e C.int
	f := C.frexp(C.double(x), &e)
	return float64(f), int(e)
}

// Mktime passes &x of a struct that holds a pointer (glibc's tm_zone): cmd/cgo's address form of the check closure.
func Mktime(year, month, day int) int64 {
	var tm C.struct_tm
	tm.tm_year = C.int(year - 1900)
	tm.tm_mon = C.int(month - 1)
	tm.tm_mday = C.int(day)
	tm.tm_isdst = -1
	return int64(C.mktime(&tm))
}

// NodeSum passes a pointer held in a variable, cmd/cgo's variable form of the check closure, and an untyped constant,
// which the closure declares at the parameter's type.
func NodeSum(node *C.struct_go2cs_probe_node) int64 {
	return int64(C.go2cs_probe_node_sum(node, 0))
}

// Nested puts one check closure inside another's argument.
func Nested() int64 {
	var a, b C.struct_go2cs_probe_node
	return int64(C.go2cs_probe_node_sum(&a, C.go2cs_probe_node_sum(&b, 1)))
}

// TwoResults takes the check closure's two-result form, with a result and with C's void.
func TwoResults() (int64, error, error) {
	var node C.struct_go2cs_probe_node
	r, err := C.go2cs_probe_node_sum(&node, 2)
	_, touched := C.go2cs_probe_node_touch(&node)
	return int64(r), err, touched
}

// Statement calls a void C function through the check closure as a statement of its own.
func Statement() {
	var node C.struct_go2cs_probe_node
	C.go2cs_probe_node_touch(&node)
}

// Deferred defers a call through the check closure: cmd/cgo's func() func() form, which Phase 2 refuses.
func Deferred() {
	var node C.struct_go2cs_probe_node
	defer C.go2cs_probe_node_touch(&node)
}

// Started starts a goroutine on a call through the check closure, whose arguments cmd/cgo evaluates inside the new
// goroutine: refused, as the deferred form is.
func Started() {
	var node C.struct_go2cs_probe_node
	go C.go2cs_probe_node_touch(&node)
}

// goNode is a Go struct with a C struct's layout.
type goNode struct {
	value C.int
	next  *C.struct_go2cs_probe_node
}

// Reinterpreted passes &x under conversions, a Go struct's address reinterpreted as the C struct's: refused when the
// wrapper is handed a box minted by reinterpretation (§11.1).
func Reinterpreted() int64 {
	var node goNode
	return int64(C.go2cs_probe_node_sum((*C.struct_go2cs_probe_node)(unsafe.Pointer(&node)), 0))
}

// packageNode is a C struct in Go's data segment.
var packageNode C.struct_go2cs_probe_node

// PackageLevel passes a pointer to package-level memory through the variable form: Go's check panics here, since the
// variable form refuses any pointer into the data or bss segment (§11.16).
func PackageLevel() int64 {
	node := &packageNode
	return int64(C.go2cs_probe_node_sum(node, 0))
}

// outer holds a C struct beside a Go pointer.
type outer struct {
	node C.struct_go2cs_probe_node
	held *int
}

// Sibling passes a pointer to one field through the variable form: Go's check scans the whole heap object, meets the
// sibling's Go pointer and panics (§11.16).
func Sibling() int64 {
	o := &outer{held: new(int)}
	node := &o.node
	return int64(C.go2cs_probe_node_sum(node, 0))
}

// Pinned passes a struct whose member points at pinned Go memory: Go accepts it, and Phase 2 refuses it (§11.16).
func Pinned() int64 {
	var pinner runtime.Pinner
	defer pinner.Unpin()

	next := new(C.struct_go2cs_probe_node)
	pinner.Pin(next)

	var node C.struct_go2cs_probe_node
	node.next = next
	return int64(C.go2cs_probe_node_sum(&node, 0))
}

// ByValue passes by value a struct whose pointer member points into Go memory that holds no pointer: cmd/cgo writes
// no closure, Go accepts it, and Phase 2's copy-in refuses the managed pointer (§11.16).
func ByValue() int {
	var link C.struct_go2cs_probe_link
	link.node = new(C.struct_go2cs_probe_node)
	return int(C.go2cs_probe_link_value(link))
}

// mutate changes the pair it is handed and returns a scale.
func mutate(pair *C.struct_go2cs_probe_pair) C.int {
	pair.a = 100
	return 2
}

// Ordered passes a struct by value and, after it, an argument that changes that struct: Go copies the struct when the
// argument is evaluated, before the change.
func Ordered() int {
	pair := C.struct_go2cs_probe_pair{a: 1, b: 2}
	return int(C.go2cs_probe_pair_scaled(pair, mutate(&pair)))
}
