// F1 probe 2: the field views that are NOT the hook's shape (one field hop off a pointer-typed identifier),
// over Go structs that hold no reference. One line per arm; a panic is printed and the next arm runs.
package main

import (
	"fmt"
	"unsafe"
)

type inner struct {
	family uint16
	port   uint16
	word   uint64
}

type outer struct {
	id    uint32
	inner inner
}

var global = inner{port: 0x1234, word: 0x1111111122222222}

func try(name string, f func() string) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: PANIC %v\n", name, r)
		}
	}()

	fmt.Printf("%s: %s\n", name, f())
}

// ---- two selections deep off a pointer

func twoHopRead(o *outer) byte {
	return (*[2]byte)(unsafe.Pointer(&o.inner.port))[1]
}

func twoHopWrite(o *outer, v byte) {
	(*[2]byte)(unsafe.Pointer(&o.inner.port))[0] = v
}

func twoHopLocalRead(o *outer) byte {
	p := (*[2]byte)(unsafe.Pointer(&o.inner.port))
	return p[1]
}

func twoHopLocalWrite(o *outer, v byte) {
	p := (*[2]byte)(unsafe.Pointer(&o.inner.port))
	p[0] = v
}

func twoHopLen(o *outer) int {
	return len((*[2]byte)(unsafe.Pointer(&o.inner.port)))
}

func twoHopSlice(o *outer) []byte {
	return (*[2]byte)(unsafe.Pointer(&o.inner.port))[:]
}

// ---- one selection off a struct VARIABLE (not a pointer)

func varRead() byte {
	var s inner
	s.port = 0x1234
	return (*[2]byte)(unsafe.Pointer(&s.port))[1]
}

func varWrite() uint16 {
	var s inner
	s.port = 0x1234
	(*[2]byte)(unsafe.Pointer(&s.port))[0] = 0xAB
	return s.port
}

func varLocalRead() byte {
	var s inner
	s.port = 0x1234
	p := (*[2]byte)(unsafe.Pointer(&s.port))
	return p[1]
}

func varLocalWrite() uint16 {
	var s inner
	s.port = 0x1234
	p := (*[2]byte)(unsafe.Pointer(&s.port))
	p[0] = 0xCD
	return s.port
}

func varLocalRangeValue() int {
	var s inner
	s.port = 0x1234
	p := (*[2]byte)(unsafe.Pointer(&s.port))
	sum := 0

	for _, b := range p {
		sum += int(b)
	}

	return sum
}

// ---- one selection off a package variable

func globalRead() byte {
	return (*[2]byte)(unsafe.Pointer(&global.port))[1]
}

func globalWrite() uint16 {
	(*[2]byte)(unsafe.Pointer(&global.port))[0] = 0xEF
	return global.port
}

func globalLocalRead() byte {
	p := (*[2]byte)(unsafe.Pointer(&global.port))
	return p[1]
}

// ---- a parenthesised or dereferenced root

func derefRootRead(o *inner) byte {
	return (*[2]byte)(unsafe.Pointer(&(*o).port))[1]
}

func wordHalves(o *outer) uint32 {
	return (*[2]uint32)(unsafe.Pointer(&o.inner.word))[1]
}

// ---- two selections deep over a struct that holds a reference: x/sys/unix's sockaddr shape
// (syscall_linux.go:394, `p := (*[2]byte)(unsafe.Pointer(&sa.raw.Port))`)

type rawSock struct {
	family uint16
	port   uint16
	addr   [4]byte
}

type sock struct {
	port int
	addr [4]byte
	raw  rawSock
}

func refTwoHopLocalWrite(sa *sock) {
	p := (*[2]byte)(unsafe.Pointer(&sa.raw.port))
	p[0] = byte(sa.port >> 8)
	p[1] = byte(sa.port)
}

func refTwoHopLocalRead(sa *sock) int {
	p := (*[2]byte)(unsafe.Pointer(&sa.raw.port))
	return int(p[0])<<8 + int(p[1])
}

func refTwoHopDirectWrite(sa *sock, v byte) {
	(*[2]byte)(unsafe.Pointer(&sa.raw.port))[0] = v
}

func refTwoHopDirectRead(sa *sock) byte {
	return (*[2]byte)(unsafe.Pointer(&sa.raw.port))[1]
}

func refTwoHopDirectWrite1(sa *sock, v byte) {
	(*[2]byte)(unsafe.Pointer(&sa.raw.port))[1] = v
}

func refTwoHopDirectAddr(sa *sock) *byte {
	return &(*[2]byte)(unsafe.Pointer(&sa.raw.port))[1]
}

func refTwoHopDirectSlice(sa *sock) []byte {
	return (*[2]byte)(unsafe.Pointer(&sa.raw.port))[:]
}

func main() {
	o := &outer{id: 7, inner: inner{port: 0x1234, word: 0x1111111122222222}}

	try("two-hop direct read [1]", func() string { return fmt.Sprintf("%#x", twoHopRead(o)) })
	try("two-hop direct write [0]", func() string { twoHopWrite(o, 0xAB); return fmt.Sprintf("port %#x", o.inner.port) })
	try("two-hop local read [1]", func() string { return fmt.Sprintf("%#x", twoHopLocalRead(o)) })
	try("two-hop local write [0]", func() string { twoHopLocalWrite(o, 0x22); return fmt.Sprintf("port %#x", o.inner.port) })
	try("two-hop len", func() string { return fmt.Sprint(twoHopLen(o)) })
	try("two-hop slice", func() string {
		s := twoHopSlice(o)
		s[1] = 0x33
		return fmt.Sprintf("len %d cap %d port %#x", len(s), cap(s), o.inner.port)
	})
	try("two-hop word [1]", func() string { return fmt.Sprintf("%#x", wordHalves(o)) })
	try("variable direct read [1]", func() string { return fmt.Sprintf("%#x", varRead()) })
	try("variable direct write [0]", func() string { return fmt.Sprintf("port %#x", varWrite()) })
	try("variable local read [1]", func() string { return fmt.Sprintf("%#x", varLocalRead()) })
	try("variable local write [0]", func() string { return fmt.Sprintf("port %#x", varLocalWrite()) })
	try("variable local range value", func() string { return fmt.Sprintf("%#x", varLocalRangeValue()) })
	try("package direct read [1]", func() string { return fmt.Sprintf("%#x", globalRead()) })
	try("package direct write [0]", func() string { return fmt.Sprintf("port %#x", globalWrite()) })
	try("package local read [1]", func() string { return fmt.Sprintf("%#x", globalLocalRead()) })
	try("deref root direct read [1]", func() string { return fmt.Sprintf("%#x", derefRootRead(&o.inner)) })

	// The direct forms first, over a fresh struct, so that each line stands by itself.
	sb := &sock{port: 1, raw: rawSock{port: 0x1234}}
	try("ref two-hop fresh direct read [1]", func() string { return fmt.Sprintf("%#x", refTwoHopDirectRead(sb)) })
	try("ref two-hop fresh direct write [0]", func() string { refTwoHopDirectWrite(sb, 0xAB); return fmt.Sprintf("raw.port %#x", sb.raw.port) })
	try("ref two-hop fresh direct write [1]", func() string { refTwoHopDirectWrite1(sb, 0xCD); return fmt.Sprintf("raw.port %#x", sb.raw.port) })
	try("ref two-hop fresh direct address [1]", func() string {
		q := refTwoHopDirectAddr(sb)
		*q = 0xEE
		return fmt.Sprintf("raw.port %#x", sb.raw.port)
	})
	try("ref two-hop fresh direct slice", func() string {
		s := refTwoHopDirectSlice(sb)
		s[0] = 0x11
		return fmt.Sprintf("len %d cap %d raw.port %#x", len(s), cap(s), sb.raw.port)
	})

	sa := &sock{port: 0x1F90}
	try("ref two-hop local write", func() string { refTwoHopLocalWrite(sa); return fmt.Sprintf("raw.port %#x", sa.raw.port) })
	try("ref two-hop local read", func() string { return fmt.Sprintf("%#x", refTwoHopLocalRead(sa)) })
	try("ref two-hop direct write [0]", func() string { refTwoHopDirectWrite(sa, 0xAB); return fmt.Sprintf("raw.port %#x", sa.raw.port) })
	try("ref two-hop direct read [1]", func() string { return fmt.Sprintf("%#x", refTwoHopDirectRead(sa)) })
}
