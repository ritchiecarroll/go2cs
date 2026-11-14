package main

import (
	"fmt"
	"unsafe"
)

// A parallel assignment whose targets are dereferenced pointer conversions writes through each
// pointer, and Go evaluates every right-hand operand before it stores any of them. The tuple
// form emitted `~(ж<T>)…` targets, which are values rather than variables (CS0131), so the
// statement did not compile. modernc.org/libc qsort.go swaps elements this way (sqlite-cs row S14).

// swap exchanges two uint64 values reached through uintptr addresses.
func swap(p, q uintptr) {
	*(*uint64)(unsafe.Pointer(p)), *(*uint64)(unsafe.Pointer(q)) = *(*uint64)(unsafe.Pointer(q)), *(*uint64)(unsafe.Pointer(p))
}

// rotate moves three int32 values one place left through unsafe.Pointer conversions.
func rotate(a, b, c unsafe.Pointer) {
	*(*int32)(a), *(*int32)(b), *(*int32)(c) = *(*int32)(b), *(*int32)(c), *(*int32)(a)
}

func main() {
	u := []uint64{1, 2}
	swap(uintptr(unsafe.Pointer(&u[0])), uintptr(unsafe.Pointer(&u[1])))
	fmt.Println(u)

	v := [3]int32{7, 8, 9}
	rotate(unsafe.Pointer(&v[0]), unsafe.Pointer(&v[1]), unsafe.Pointer(&v[2]))
	fmt.Println(v)

	// Both targets name the same address: the right-hand values are read first, and the later
	// store wins, as in Go.
	w := uint64(5)
	swap(uintptr(unsafe.Pointer(&w)), uintptr(unsafe.Pointer(&w)))
	fmt.Println(w)
}
