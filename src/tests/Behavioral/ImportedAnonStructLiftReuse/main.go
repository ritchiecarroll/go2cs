package main

import (
	"fmt"

	"ImportedAnonStructLiftReuse/lib"
)

// TLARGE_INTEGER aliases an anonymous struct identical to lib's largeInteger, so Go treats them as one type.
type TLARGE_INTEGER = struct {
	QuadPart int64
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
}
