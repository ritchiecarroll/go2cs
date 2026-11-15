package main

import (
	"fmt"
	"os"
	"unsafe"
)

type smallMem [64]byte

type hdr struct {
	n   int32
	buf [64]byte
}

func main() {
	switch os.Args[1] {
	case "ga": // the address of an array field, through unsafe.Pointer
		h := &hdr{}
		a := (*smallMem)(unsafe.Pointer(&h.buf))
		a[1] = 3
		fmt.Println(h.buf[1], a[1], h.buf[:8])
	case "gb": // the address of the field's first element
		h := &hdr{}
		b := (*smallMem)(unsafe.Pointer(&h.buf[0]))
		b[2] = 4
		fmt.Println(h.buf[2], b[2], h.buf[:8])
	case "gd": // a local array
		var arr [64]byte
		d := (*smallMem)(unsafe.Pointer(&arr))
		d[4] = 6
		fmt.Println(arr[4], d[4], arr[:8])
	case "ge": // the reverse: a named array viewed as the unnamed one
		var sm smallMem
		e := (*[64]byte)(unsafe.Pointer(&sm))
		e[5] = 7
		fmt.Println(sm[5], e[5], sm[:8])
	case "gf": // the same type
		var sm smallMem
		f := (*smallMem)(unsafe.Pointer(&sm))
		f[6] = 8
		fmt.Println(sm[6], f[6], sm[:8])
	case "gg": // slice memory
		s := make([]byte, 64)
		g := (*smallMem)(unsafe.Pointer(&s[0]))
		g[7] = 9
		fmt.Println(s[7], g[7], s[:8])
	case "gh": // an array of another element type
		var w [8]uint64
		x := (*smallMem)(unsafe.Pointer(&w))
		x[0] = 1
		fmt.Println(w[0], x[0], w[:2])
	}
}
