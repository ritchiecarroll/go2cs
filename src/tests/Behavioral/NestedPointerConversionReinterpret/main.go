package main

import (
	"fmt"
	"unsafe"
)

// A pointer conversion whose operand is unsafe.Pointer of another pointer conversion,
// `(*U)(unsafe.Pointer((*T)(up)))` (found by the cgo 3.1 review): the result points at the
// same bytes, so a read sees them and a write lands in them. The inner conversion rendered
// as a C# cast with `.Reinterpret` appended, and the cast bound looser (CS1929).

func main() {
	var x int32 = 0x01020304
	up := unsafe.Pointer(&x)

	q := (*int8)(unsafe.Pointer((*int32)(up)))
	fmt.Println(*q)

	*q = 9
	fmt.Println(x == 0x01020309)

	w := (*uint16)(unsafe.Pointer(((*int32)(up))))
	fmt.Println(*w)
}
