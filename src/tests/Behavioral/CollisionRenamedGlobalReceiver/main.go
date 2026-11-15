package main

import (
	"fmt"
	"sync/atomic"

	"CollisionRenamedGlobalReceiver/lib"
)

// A package-level var that shares its name with a method of the package (modernc.org/libc's musl
// `var tid atomic.Int32` beside `func (tls *TLS) tid() int32`) is collision-renamed, and its box is
// declared under the RENAMED name. A pointer-receiver method called on the var takes its address
// implicitly, so the call must name that same box -- the raw name's box does not exist.

var tid atomic.Int32

var count counter

type counter struct{ n int }

func (c *counter) bump() int {
	c.n++
	return c.n
}

// self makes a call on the renamed struct var NEED its box: `count.self()` passes the var's address on.
func (c *counter) self() *counter { return c }

// A LOCAL named like the renamed package var: its own box keeps the raw name, the package var's is `ᏑΔtid`.
func shadow() int32 {
	var tid atomic.Int32
	tid.Add(5)
	p := &tid
	p.Add(1)
	return tid.Load()
}

type TLS struct{ id int32 }

func (tls *TLS) tid() int32 { return tls.id }

func (tls *TLS) count() int { return int(tls.id) * 10 }

func main() {
	first := tid.Add(1)
	second := tid.Add(1)
	tls := &TLS{id: second}
	fmt.Println(first, second, tls.tid(), tid.Load())

	// The explicit address and the implicit one name the same var.
	p := &tid
	p.Add(40)
	fmt.Println(tid.Load(), p.Load())

	// A collision-renamed struct var, called through a pointer-receiver method of its own type.
	count.bump()
	fmt.Println(count.bump(), count.n, tls.count())

	// ...and through a method that hands its address on.
	count.self().bump()
	fmt.Println(count.self().bump(), count.n, count.self() == &count)

	// The local of the same name leaves the package var alone.
	before := tid.Load()
	fmt.Println(before, shadow(), tid.Load())

	// An importer's view of an exported renamed var: qualified, through its address, and dot-imported.
	lib.Tid.Add(1)
	q := &lib.Tid
	q.Add(10)
	fmt.Println(lib.Tid.Load(), q.Load(), lib.Next(), (&lib.T{ID: 9}).Tid())
	fmt.Println(viaDot(), lib.Tid.Load())
}
