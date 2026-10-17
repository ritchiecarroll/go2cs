package main

import (
	"fmt"
	"unsafe"
)

// Guards the zero value and size of a generic's type parameter when the type argument is an ARRAY.
// A C# array<E> carries its Go length in the instance, so a type parameter instantiated with [N]E
// has no length to read in `var x T` or `new(T)`: before the array-dims companion both read as a
// length-0 array and unsafe.Sizeof answered 0. runtime's testFreegc[[16]byte] has this shape.

type holder struct {
	a [4]byte
}

func z[T any]() (uintptr, uintptr) {
	var x T
	p := new(T)
	return unsafe.Sizeof(x), unsafe.Sizeof(*p)
}

func zeroLen[T any]() int {
	var x T
	return len(fmt.Sprint(x))
}

func main() {
	a, b := z[[16]byte]()
	fmt.Println("[16]byte:", a, b)

	a, b = z[[2][3]int32]()
	fmt.Println("[2][3]int32:", a, b)

	a, b = z[holder]()
	fmt.Println("struct holding [4]byte:", a, b)

	a, b = z[int64]()
	fmt.Println("int64:", a, b)

	f := z[[64]byte]
	a, b = f()
	fmt.Println("func value [64]byte:", a, b)

	fmt.Println("zero [3]int prints as", zeroLen[[3]int](), "chars")
}
