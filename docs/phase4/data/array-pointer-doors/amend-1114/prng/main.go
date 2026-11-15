package main

import (
	"fmt"
	"os"
	"unsafe"
)

type RawMem [1<<50 - 1]byte

type prngType struct {
	s   [16]uint32
	out [64]uint8
	n   uint8
}

type pair struct {
	a, b uint32
	c    uint64
}

var prng prngType
var saved prngType
var chachaInit = [4]uint32{0x61707865, 0x3320646e, 0x79622d32, 0x6b206574}
var magic = [8]uint8{0xd9, 0xd5, 0x05, 0xf9, 0x20, 0xa1, 0x63, 0xd7}
var scalars pair

// libc's Xmemcpy on windows (libc.go:1959), both sides views of a number.
func memcpy(dest, src uintptr, n uintptr) uintptr {
	if n != 0 {
		copy((*RawMem)(unsafe.Pointer(dest))[:n:n], (*RawMem)(unsafe.Pointer(src))[:n:n])
	}
	return dest
}

func main() {
	switch os.Args[1] {
	case "pa": // sqlite3_randomness: memcpy(&sqlite3Prng.s, chacha20_init, 16)
		memcpy(uintptr(unsafe.Pointer(&prng)), uintptr(unsafe.Pointer(&chachaInit)), 16)
		fmt.Printf("%#x %#x %#x %#x\n", prng.s[0], prng.s[1], prng.s[2], prng.s[3])
	case "pb": // sqlite3PrngSaveState: memcpy(&saved, &prng, sizeof)
		prng.s[5] = 7
		prng.out[2] = 9
		prng.n = 3
		memcpy(uintptr(unsafe.Pointer(&saved)), uintptr(unsafe.Pointer(&prng)), unsafe.Sizeof(prng))
		fmt.Println(saved.s[5], saved.out[2], saved.n)
	case "pc": // a struct of scalars as the destination
		memcpy(uintptr(unsafe.Pointer(&scalars)), uintptr(unsafe.Pointer(&chachaInit)), 16)
		fmt.Printf("%#x %#x %#x\n", scalars.a, scalars.b, scalars.c)
	case "pd": // byte arrays on both sides
		var buf [8]byte
		memcpy(uintptr(unsafe.Pointer(&buf)), uintptr(unsafe.Pointer(&magic)), 8)
		fmt.Println(buf)
	case "pe": // an array of another element type as the destination
		var w [4]uint32
		memcpy(uintptr(unsafe.Pointer(&w)), uintptr(unsafe.Pointer(&magic)), 8)
		fmt.Printf("%#x %#x\n", w[0], w[1])
	case "pf": // the source is Go storage of another type, the destination a byte array
		var buf [16]byte
		memcpy(uintptr(unsafe.Pointer(&buf)), uintptr(unsafe.Pointer(&chachaInit)), 16)
		fmt.Println(buf)
	}
}
