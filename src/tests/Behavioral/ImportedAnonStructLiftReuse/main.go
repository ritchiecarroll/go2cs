package main

import (
	"fmt"

	"ImportedAnonStructLiftReuse/lib"
)

// TLARGE_INTEGER aliases an anonymous struct identical to lib's largeInteger, so Go treats them as one type.
type TLARGE_INTEGER = struct {
	QuadPart int64
}

// TRECT is identical to lib's rect, though its fields are spelled through TLONG, an alias of int32: Go reads one
// type, so the converted value is lib's lifted type and passes to lib.Area as it is.
type TLONG = int32

type TRECT = struct {
	W TLONG
	H TLONG
}

func main() {
	var distance TLARGE_INTEGER
	distance.QuadPart = 20
	var moved TLARGE_INTEGER

	// A value of this package's type passed where lib's type is expected.
	fmt.Println(lib.SetFilePointerEx(distance, &moved), moved.QuadPart)

	// lib's function held in a variable of this package's function type.
	var seek func(TLARGE_INTEGER, *TLARGE_INTEGER) int64 = lib.SetFilePointerEx
	fmt.Println(seek(TLARGE_INTEGER{QuadPart: 5}, &moved), moved.QuadPart)
	fmt.Printf("%T\n", distance)
	fmt.Println(lib.Area(TRECT{W: 6, H: 7}))
}
