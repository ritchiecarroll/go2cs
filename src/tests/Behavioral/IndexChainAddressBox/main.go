package main

import (
	"fmt"
	"sync/atomic"
)

// A local whose storage is reached through an ARRAY INDEX under a field chain, or an array element
// receiving an imported pointer method, must be heap-boxed like `&x` and `&x.f` are: the pointer
// (explicit, or the implicit receiver address) aliases the local's own storage.

type counter struct{ n int }

func (c *counter) inc() { c.n++ }

type hist struct {
	counts [4]atomic.Uint64
	local  [2]counter
	plain  [3]int
	sl     []int
}

type wrap struct{ h hist }

// The shape the i9 found: a closure-local, its element's imported pointer method called in the closure.
func closureLocal() uint64 {
	record := func(samples []int) uint64 {
		var h hist
		for _, s := range samples {
			h.counts[s%4].Add(1)
		}
		return h.counts[1].Load()
	}
	return record([]int{1, 5, 2, 9})
}

// The same outside a closure.
func fieldArrayMethod() uint64 {
	var h hist
	h.counts[2].Add(3)
	h.counts[2].Add(4)
	return h.counts[2].Load()
}

// Through two fields.
func nestedFieldArrayMethod() uint64 {
	var w wrap
	w.h.counts[3].Add(5)
	return w.h.counts[3].Load()
}

// An imported pointer method on an element of a local ARRAY (no field).
func localArrayMethod() uint64 {
	var a [4]atomic.Uint64
	a[2].Add(6)
	return a[2].Load()
}

// An explicit address of an element under a field: writes through it must land in h.
func explicitAddress() int {
	var h hist
	p := &h.plain[1]
	*p = 7
	*p += 1
	return h.plain[1]
}

// The parameter twin: a value parameter whose element address is taken.
func paramAddress(h hist) int {
	p := &h.plain[2]
	*p = 9
	return h.plain[2]
}

// The method-value twin: `h.local[1].inc` evaluated as a func binds &h.local[1].
func methodValue() int {
	var h hist
	f := h.local[1].inc
	f()
	f()
	return h.local[1].n
}

// Controls. A local-type pointer method in CALL position binds the element directly, and a SLICE
// field's element lives in the backing array, not in h.
func localTypeCall() int {
	var h hist
	h.local[0].inc()
	return h.local[0].n
}

func sliceFieldAddress() int {
	var h hist
	h.sl = make([]int, 2)
	p := &h.sl[1]
	*p = 11
	return h.sl[1]
}

func main() {
	fmt.Println("closureLocal:", closureLocal())
	fmt.Println("fieldArrayMethod:", fieldArrayMethod())
	fmt.Println("nestedFieldArrayMethod:", nestedFieldArrayMethod())
	fmt.Println("localArrayMethod:", localArrayMethod())
	fmt.Println("explicitAddress:", explicitAddress())
	fmt.Println("paramAddress:", paramAddress(hist{}))
	fmt.Println("methodValue:", methodValue())
	fmt.Println("localTypeCall:", localTypeCall())
	fmt.Println("sliceFieldAddress:", sliceFieldAddress())
}
