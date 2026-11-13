package main

import (
	"fmt"
	"unsafe"
)

type T struct{ n int }

// holder keeps a pointer of the receiver's type in a field. It is its own type
// so that T stays a struct of one int, laid out the same in Go and in C#.
type holder struct{ p *T }

var hold holder

// q is both a type and a method name, so the converter renames the type and a
// receiver named q (`Δq`); that receiver is still the value alias.
type q struct{}

func (h *T) q() {}

func poke(p unsafe.Pointer, v int) { *(*int)(p) = v }

// The receiver itself: the C# name is an alias of the pointed-to value.
func (h *T) viaReceiver() { poke(unsafe.Pointer(h), 7) }

// A local copy of the receiver holds the box, not the value.
func (h *T) viaLocal() {
	p := h
	poke(unsafe.Pointer(p), 8)
}

// A pointer parameter of the receiver's type: an alias of the value, like the receiver.
func (h *T) viaParam(other *T) { poke(unsafe.Pointer(other), 9) }

// The conversion folded into a reinterpret, not consumed as a number.
func (h *T) viaFolded() {
	p := h
	*(*int)(unsafe.Pointer(p)) = 10
}

// A field holding a pointer of the receiver's type.
func (h *T) viaField() { poke(unsafe.Pointer(hold.p), 11) }

// A receiver renamed because a package-level type has its name.
func (q *T) viaRenamedReceiver() { poke(unsafe.Pointer(q), 12) }

// A local copy assigned through a second local.
func (h *T) viaLocalChain() {
	p := h
	r := p
	poke(unsafe.Pointer(r), 13)
}

func main() {
	var a, b, c, d, e, f, g, k T
	a.viaReceiver()
	b.viaLocal()
	c.viaParam(&d)
	e.viaFolded()
	hold.p = &g
	f.viaField()
	k.viaRenamedReceiver()
	var m T
	m.viaLocalChain()

	fmt.Println(a.n, b.n, c.n, d.n, e.n, f.n, g.n, k.n, m.n)

	var _ q
}
