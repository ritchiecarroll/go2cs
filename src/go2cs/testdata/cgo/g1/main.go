// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Command g1 is cgo Phase 2's exit-gate program (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.22). It is extern-only:
// every C function it calls is declared by a header and defined by a shared library. Its one argument names an arm,
// and it runs that arm alone, so an arm that panics takes no other with it. The gate builds it with go and converts
// it with go2cs on the same host, runs each arm both ways and compares what they print.
package main

/*
#cgo LDFLAGS: -lm
#include <math.h>
#include <stdio.h>
#include <stdlib.h>

struct go2cs_probe_pair { int a; int b; };
struct go2cs_probe_node { int value; struct go2cs_probe_node *next; };
struct go2cs_probe_link { int value; struct go2cs_probe_node *node; };

long go2cs_probe_node_sum(struct go2cs_probe_node *node, long base);
void go2cs_probe_node_touch(struct go2cs_probe_node *node);
int go2cs_probe_link_value(struct go2cs_probe_link link);
int go2cs_probe_pair_scaled(struct go2cs_probe_pair pair, int k);
const char *go2cs_probe_text(void);
*/
import "C"

import (
	"fmt"
	"os"
	"runtime"
	"syscall"
	"unsafe"
)

// errnoOf is the errno an errno-form call returned: 0 for none, -1 for an error that is not an errno.
func errnoOf(err error) int {
	if err == nil {
		return 0
	}

	if errno, ok := err.(syscall.Errno); ok {
		return int(errno)
	}

	return -1
}

// goNode is a Go struct with a C struct's layout.
type goNode struct {
	value C.int
	next  *C.struct_go2cs_probe_node
}

// packageNode is a C struct in Go's data segment.
var packageNode C.struct_go2cs_probe_node

// outer holds a C struct beside a Go pointer.
type outer struct {
	node C.struct_go2cs_probe_node
	held *int
}

// mutate changes the pair it is handed and returns a scale.
func mutate(pair *C.struct_go2cs_probe_pair) C.int {
	pair.a = 100
	return 2
}

func main() {
	if len(os.Args) != 2 {
		fmt.Println("usage: g1 <arm>")
		os.Exit(64)
	}

	arm := os.Args[1]

	switch arm {
	case "cos":
		// A C double in and out.
		fmt.Printf("cos %.6f\n", float64(C.cos(C.double(0))))
	case "div":
		// A struct of integers by value.
		r := C.div(C.int(7), C.int(2))
		fmt.Println("div", int(r.quot), int(r.rem))
	case "exp":
		// Go's two-result form, which reads errno: none, then ERANGE.
		fine, none := C.exp(C.double(0))
		over, err := C.exp(C.double(1000))
		fmt.Println("exp", float64(fine), errnoOf(none), float64(over) > 1e300, errnoOf(err))
	case "frexp":
		// A pointer to one C int, copied back.
		var e C.int
		f := C.frexp(C.double(8), &e)
		fmt.Println("frexp", float64(f), int(e))
	case "conv":
		// Conversions between C numeric types, with no C call: one C type to another by a typedef (size_t), a
		// widening and a narrowing. Each needs a conversion operator the generator hosts in the C type.
		n := C.int(7)
		size := C.size_t(n)
		wide := C.long(n)
		big := C.long(70000)
		narrow := C.short(big)
		fmt.Println("conv", uint64(size), int64(wide), int16(narrow))
	case "puts":
		// A C string C owns, printed by C. The result is the arm; whether C's line reaches a captured stdout is read.
		r := C.puts(C.go2cs_probe_text())
		fmt.Println("puts", r >= 0)
	case "nodesum":
		// cmd/cgo's variable form of its pointer check, over a struct whose pointer member is nil.
		node := &C.struct_go2cs_probe_node{value: 5}
		fmt.Println("nodesum", int64(C.go2cs_probe_node_sum(node, 0)))
	case "nested":
		// One check closure inside another's argument.
		var a, b C.struct_go2cs_probe_node
		fmt.Println("nested", int64(C.go2cs_probe_node_sum(&a, C.go2cs_probe_node_sum(&b, 1))))
	case "tworesults":
		// The check closure's two-result form, with a result and with C's void.
		var node C.struct_go2cs_probe_node
		r, err := C.go2cs_probe_node_sum(&node, 2)
		_, touched := C.go2cs_probe_node_touch(&node)
		fmt.Println("tworesults", int64(r), errnoOf(err), errnoOf(touched), int(node.value))
	case "statement":
		// A void C function through the check closure, as a statement, writing through the pointer.
		var node C.struct_go2cs_probe_node
		C.go2cs_probe_node_touch(&node)
		fmt.Println("statement", int(node.value))
	case "ordered":
		// A struct by value and, after it, an argument that changes that struct. Go's spec orders the calls in an
		// expression and leaves a plain variable read unordered against them; gc runs the call first, so C is
		// handed the changed struct. go2cs reads left to right and hands C the struct as it was. A ruled divergence
		// of the converter, not of cgo (§11.22).
		pair := C.struct_go2cs_probe_pair{a: 1, b: 2}
		fmt.Println("ordered", int(C.go2cs_probe_pair_scaled(pair, mutate(&pair))))
	case "reinterpreted":
		// A Go struct's address reinterpreted as the C struct's. The two share a layout, so the reinterpretation is
		// a view of the same Go memory, and the call proceeds as it does in Go (§11.22).
		var node goNode
		fmt.Println("reinterpreted", int64(C.go2cs_probe_node_sum((*C.struct_go2cs_probe_node)(unsafe.Pointer(&node)), 0)))
	case "reinterpretedwrite":
		// The same pointer handed to a C function that WRITES through it: the Go struct must show C's write.
		var node goNode
		C.go2cs_probe_node_touch((*C.struct_go2cs_probe_node)(unsafe.Pointer(&node)))
		fmt.Println("reinterpretedwrite", int(node.value))
	case "packagelevel":
		// A pointer into the data segment through the variable form: Go's check panics, go2cs proceeds (§11.16).
		node := &packageNode
		fmt.Println("packagelevel", int64(C.go2cs_probe_node_sum(node, 0)))
	case "sibling":
		// A pointer to a field whose heap object holds another Go pointer: Go's check panics, go2cs proceeds (§11.16).
		o := &outer{held: new(int)}
		node := &o.node
		fmt.Println("sibling", int64(C.go2cs_probe_node_sum(node, 0)))
	case "pinned":
		// A member pointing at pinned Go memory: Go proceeds, go2cs refuses (§11.16).
		var pinner runtime.Pinner
		defer pinner.Unpin()

		next := new(C.struct_go2cs_probe_node)
		pinner.Pin(next)

		var node C.struct_go2cs_probe_node
		node.next = next
		fmt.Println("pinned", int64(C.go2cs_probe_node_sum(&node, 0)))
	case "byvalue":
		// A by-value struct whose pointer member points into pointer-free Go memory: Go proceeds, go2cs refuses (§11.16).
		var link C.struct_go2cs_probe_link
		link.node = new(C.struct_go2cs_probe_node)
		fmt.Println("byvalue", int(C.go2cs_probe_link_value(link)))
	default:
		if !osArm(arm) {
			fmt.Println("unknown arm", arm)
			os.Exit(64)
		}
	}
}
