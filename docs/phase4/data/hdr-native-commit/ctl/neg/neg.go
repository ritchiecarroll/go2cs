// Negative control: a LOCAL type named StringHeader with the same fields, and a reflect.StringHeader that is only read.
// EXPECTED: none.
package main

import (
	"reflect"
	"unsafe"
)

type StringHeader struct {
	Data uintptr
	Len  int
}

func lookalike(s *string, p uintptr) {
	hdr := (*StringHeader)(unsafe.Pointer(s))
	hdr.Data = p
	hdr.Len = 1
}

func readOnly(s *string) uintptr {
	return (*reflect.StringHeader)(unsafe.Pointer(s)).Data
}

func main() {}
