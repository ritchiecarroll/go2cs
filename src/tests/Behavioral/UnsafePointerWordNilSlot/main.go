package main

import (
	"fmt"
	"unsafe"
)

// Loads and stores through a *unsafe.Pointer PARAMETER whose slot is a managed box that holds nil,
// `p := new(unsafe.Pointer)`. Go stores into the slot and reads it back, and a fresh slot reads nil.
// Set and Get are exported so their parameters keep the box rather than lowering to a ref.
// The word doors (unsafe.Pointer.LoadWord / StoreWord) must not read the held value before the
// store or the load: a nil unsafe.Pointer is the box's held null, which is not a nil pointer.

var x = 42

func Set(dst *unsafe.Pointer, v unsafe.Pointer) { *dst = v }

func Get(src *unsafe.Pointer) unsafe.Pointer { return *src }

func main() {
	p := new(unsafe.Pointer)
	fmt.Println("fresh slot loads nil:", Get(p) == nil)

	v := unsafe.Pointer(&x)
	Set(p, v)
	fmt.Println("stored:", Get(p) == v, *(*int)(Get(p)))

	Set(p, nil)
	fmt.Println("cleared:", Get(p) == nil)

	var l unsafe.Pointer
	Set(&l, v)
	fmt.Println("escaped local:", l == v, Get(&l) == v)
}
