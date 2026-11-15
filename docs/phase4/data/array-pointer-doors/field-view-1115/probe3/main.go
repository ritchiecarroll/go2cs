package main

import (
	"fmt"
	"unsafe"
)

type hdr struct {
	family uint16
	port   uint16
	word   uint64
}

type outer struct {
	inner hdr
}

var global hdr

// the view local, sliced
func localOpen(h *hdr) []byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return p[:]
}

func localBounded(h *hdr) []byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return p[:1]
}

func localFrom(h *hdr) []byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return p[1:]
}

func localThree(h *hdr) []byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return p[0:1:2]
}

// the explicit dereference of the local, sliced
func derefOpen(h *hdr) []byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return (*p)[:]
}

func derefBounded(h *hdr) []byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return (*p)[:1]
}

func derefFrom(h *hdr) []byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return (*p)[1:]
}

func derefThree(h *hdr) []byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return (*p)[0:1:2]
}

// the conversion where it is written, sliced, bare and dereferenced
func directBounded(h *hdr) []byte {
	return (*[2]byte)(unsafe.Pointer(&h.port))[:1]
}

func directDerefOpen(h *hdr) []byte {
	return (*(*[2]byte)(unsafe.Pointer(&h.port)))[:]
}

func directDerefBounded(h *hdr) []byte {
	return (*(*[2]byte)(unsafe.Pointer(&h.port)))[:1]
}

// whole-array forms in other positions
func loadField(h *hdr) byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return (*p)[1]
}

func loadLen(h *hdr) int {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return len(*p) + len(p)
}

func loadMulti(h *hdr) ([2]byte, [2]byte) {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	a, b := *p, *p
	return a, b
}

func loadVar(h *hdr) [2]byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	var a [2]byte = *p
	return a
}

func loadInComposite(h *hdr) [][2]byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return [][2]byte{*p}
}

func loadToInterface(h *hdr) any {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return *p
}

type raw struct {
	port [2]byte
	word [8]byte
}

// a struct literal's field values, the conversion where it is written: x/sys/windows' SockaddrBth shape
func loadInStructLiteral(h *hdr) raw {
	return raw{
		port: *(*[2]byte)(unsafe.Pointer(&h.port)),
		word: *(*[8]byte)(unsafe.Pointer(&h.word)),
	}
}

func loadInStructLiteralLocal(h *hdr) raw {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	return raw{port: (*p)}
}

func loadSent(h *hdr) [2]byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	ch := make(chan [2]byte, 1)
	ch <- *p
	return <-ch
}

func loadParen(h *hdr) [2]byte {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	v := (*p)
	return v
}

func storeLiteral(h *hdr) {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	*p = [2]byte{1, 2}
}

func storeFromView(h *hdr, g *hdr) {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	q := (*[2]byte)(unsafe.Pointer(&g.port))
	*p = *q
}

func storeMulti(h *hdr, g *hdr) {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	q := (*[2]byte)(unsafe.Pointer(&g.port))
	*p, *q = *q, *p
}

func rangeIndexOnly(h *hdr) int {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	n := 0
	for i := range p {
		n += i
	}
	return n
}

func rangeDerefIndexOnly(h *hdr) int {
	p := (*[2]byte)(unsafe.Pointer(&h.port))
	n := 0
	for i := range *p {
		n += i
	}
	return n
}

func rangeWord(h *hdr) uint32 {
	t := (*[2]uint32)(unsafe.Pointer(&h.word))
	var s uint32
	for _, w := range t {
		s += w
	}
	return s
}

// controls: not the field view's shape, or a re-pointed local
func ctlRepointed(h *hdr, other *[2]byte) [2]byte {
	q := (*[2]byte)(unsafe.Pointer(&h.port))
	q = other
	*q = [2]byte{9, 9}
	for _, b := range q {
		_ = b
	}
	_ = (*q)[:1]
	return *q
}

func ctlTwoHop(o *outer) [2]byte {
	n := (*[2]byte)(unsafe.Pointer(&o.inner.port))
	for _, b := range n {
		_ = b
	}
	return *n
}

func ctlGlobal() [2]byte {
	g := (*[2]byte)(unsafe.Pointer(&global.port))
	return *g
}

func ctlSameElem(xs *[4]byte) [2]byte {
	a := (*[2]byte)(unsafe.Pointer(&xs[0]))
	*a = [2]byte{7, 8}
	return *a
}

func ctlManaged(xs *[2]byte) [2]byte {
	*xs = [2]byte{5, 6}
	for _, b := range xs {
		_ = b
	}
	_ = (*xs)[:1]
	return *xs
}

func main() {
	h, g := &hdr{port: 0x1234}, &hdr{port: 0x5678}
	fmt.Println(localOpen(h), localBounded(h), localFrom(h), localThree(h))
	fmt.Println(derefOpen(h), derefBounded(h), derefFrom(h), derefThree(h))
	fmt.Println(directBounded(h), directDerefOpen(h), directDerefBounded(h))
	fmt.Println(loadField(h), loadLen(h), loadVar(h), loadInComposite(h), loadToInterface(h))
	fmt.Println(loadInStructLiteral(h), loadInStructLiteralLocal(h), loadSent(h), loadParen(h))
	a, b := loadMulti(h)
	fmt.Println(a, b)
	storeLiteral(h)
	fmt.Printf("%#x\n", h.port)
	storeFromView(h, g)
	fmt.Printf("%#x\n", h.port)
	h.word = 0x0000000200000001
	fmt.Println(rangeIndexOnly(h), rangeDerefIndexOnly(h), rangeWord(h))
	fmt.Println(ctlRepointed(h, &[2]byte{}), ctlSameElem(&[4]byte{}), ctlManaged(&[2]byte{}))
	_ = ctlTwoHop
	_ = ctlGlobal

	// last: the one form that stays refused by name, a tuple assignment that stores through two views
	h.port = 0x1111
	storeMulti(h, g)
	fmt.Printf("%#x %#x\n", h.port, g.port)
}
