// Positive control for the header-store census. EXPECTED (written before the run):
//   string-data-then-len/other   2  (dataThenLen, and the FuncLit inside literal)
//   string-data-then-len/go-derived 1 (goDerived)
//   string-len-then-data/other   1  (lenThenData)
//   string-data-only/other       1  (dataOnly)
//   string-value-header/other    1  (valueHeader)
//   slice-data-then-len/other    1  (sliceDataThenLen)
// and nothing else.
package main

import (
	"reflect"
	"unsafe"
)

func dataThenLen(s *string, p uintptr, n int) {
	hdr := (*reflect.StringHeader)(unsafe.Pointer(s))
	hdr.Data = p
	hdr.Len = n
}

func goDerived(s *string, b []byte) {
	hdr := (*reflect.StringHeader)(unsafe.Pointer(s))
	hdr.Data = uintptr(unsafe.Pointer(&b[0]))
	hdr.Len = len(b)
}

func lenThenData(s *string, p uintptr, n int) {
	hdr := (*reflect.StringHeader)(unsafe.Pointer(s))
	hdr.Len = n
	hdr.Data = p
}

func dataOnly(s *string, p uintptr) {
	hdr := (*reflect.StringHeader)(unsafe.Pointer(s))
	hdr.Data = p
}

func valueHeader(p uintptr) string {
	var h reflect.StringHeader
	h.Data = p
	h.Len = 1
	return *(*string)(unsafe.Pointer(&h))
}

func sliceDataThenLen(b *[]byte, p uintptr, n int) {
	hdr := (*reflect.SliceHeader)(unsafe.Pointer(b))
	hdr.Data = p
	hdr.Len = n
	hdr.Cap = n
}

func literal(s *string, p uintptr) func() {
	return func() {
		hdr := (*reflect.StringHeader)(unsafe.Pointer(s))
		hdr.Data = p
		hdr.Len = 3
	}
}

func main() {}

// Freshness control. EXPECTED: freshEmpty -> string-data-then-len-fresh-empty/other 1; inLoop and reassigned ->
// string-data-then-len-maybe-nonempty/other 1 each (so maybe-nonempty totals 4 with dataThenLen, goDerived's
// go-derived one, and literal: dataThenLen 1 + literal 1 + inLoop 1 + reassigned 1 = 4 other, goDerived 1 go-derived).
func freshEmpty(p uintptr, n int) string {
	var x string
	hdr := (*reflect.StringHeader)(unsafe.Pointer(&x))
	hdr.Data = p
	hdr.Len = n
	return x
}

func inLoop(ps []uintptr, n int) (out []string) {
	var x string
	for _, p := range ps {
		hdr := (*reflect.StringHeader)(unsafe.Pointer(&x))
		hdr.Data = p
		hdr.Len = n
		out = append(out, x)
	}
	return
}

func reassigned(p uintptr, n int) string {
	var x string
	x = "prior"
	hdr := (*reflect.StringHeader)(unsafe.Pointer(&x))
	hdr.Data = p
	hdr.Len = n
	return x
}
