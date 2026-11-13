package main

import (
	"fmt"
	"unsafe"
)

// Stores through an EXPLICIT dereference of a pointer conversion, `(*(*T)(p)).F = x`
// (sqlite-cs S17, modernc.org/sqlite/lib's `(*(*TWalSehFindFrame)(__ccgo_ptr(bp))).Fpgno = pgno`),
// and the implicit form beside each as its control.

type frame struct {
	Pgno int32
	Read int32
}

func (f *frame) bump() { f.Read++ }

func ptr(p uintptr) unsafe.Pointer { return unsafe.Pointer(p) }

func main() {
	f := new(frame)
	bp := uintptr(unsafe.Pointer(f))

	(*(*frame)(ptr(bp))).Pgno = 7
	(*(*frame)(ptr(bp))).Read += 3
	(*(*frame)(ptr(bp))).bump()
	fmt.Println(f.Pgno, f.Read, (*(*frame)(ptr(bp))).Pgno)

	(*frame)(ptr(bp)).Pgno = 9
	(*frame)(ptr(bp)).Read += 10
	fmt.Println(f.Pgno, f.Read, (*frame)(ptr(bp)).Read)

	arr := new([4]int32)
	ap := uintptr(unsafe.Pointer(arr))
	(*(*[4]int32)(ptr(ap)))[1] = 11
	(*(*[4]int32)(ptr(ap)))[2] += 12
	(*[4]int32)(ptr(ap))[3] = 13
	fmt.Println(*arr, (*(*[4]int32)(ptr(ap)))[1])
}
