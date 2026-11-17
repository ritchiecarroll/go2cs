// InteriorPointerShapes: a pointer converted from an address INSIDE a managed value, read and written, and held
// across a collection when it is the only reference. The shapes are numbered as in
// docs/phase4/DESIGN-interior-pointer-root.md section 1.1; shape 9 (the typed pointer's identity with the field's
// own address) is not here, it is a different item.
package main

import (
	"fmt"
	"runtime"
	"unsafe"
)

type methods struct {
	ver int32
	a   uintptr
	b   uintptr
	c   uintptr
}

type pt struct{ x, y, z int64 }

type holder struct {
	n   int32
	arr [4]uint32
}

var tbl = methods{ver: 1}

func add(x, y int) int { return x + y }

func wordOf(f func(int, int) int) uintptr { return *(*uintptr)(unsafe.Pointer(&f)) }

func callWord(x uintptr, a, b int) int {
	return (*(*func(int, int) int)(unsafe.Pointer(&struct{ uintptr }{x})))(a, b)
}

type mixed struct {
	s string
	n int64
	m int64
}

//go:noinline
func interiorOfMixed() *int64 {
	p := &mixed{"s", 5, 6}
	return (*int64)(unsafe.Add(unsafe.Pointer(p), unsafe.Offsetof(p.m)))
}

//go:noinline
func interiorOfFresh() *int64 {
	p := &pt{1, 2, 3}
	return (*int64)(unsafe.Add(unsafe.Pointer(p), 8))
}

var sink [][]byte

func churn() {
	for i := 0; i < 2000; i++ {
		sink = append(sink, make([]byte, 4096))
		if len(sink) > 64 {
			sink = sink[:0]
		}
	}
	runtime.GC()
	runtime.GC()
}

func main() {
	// 1. a func stored at a computed address inside a package var (vfs/patches64.go's shape).
	*(*func(int, int) int)(unsafe.Pointer(uintptr(unsafe.Pointer(&tbl)) + 8)) = add
	fmt.Println("1 func store at +8 landed:", tbl.a != 0)
	fmt.Println("1 slot is add's word:", tbl.a == wordOf(add))
	func() {
		defer func() {
			if r := recover(); r != nil {
				fmt.Println("1 call through slot: PANIC")
			}
		}()
		fmt.Println("1 call through slot:", callWord(tbl.a, 2, 3))
	}()

	// 2. a plain word stored at a computed address, uintptr arithmetic.
	*(*uintptr)(unsafe.Pointer(uintptr(unsafe.Pointer(&tbl)) + 16)) = 42
	fmt.Println("2 word store at +16:", tbl.b)

	// 3. the same through unsafe.Add.
	*(*uintptr)(unsafe.Add(unsafe.Pointer(&tbl), 24)) = 43
	fmt.Println("3 word store unsafe.Add +24:", tbl.c)

	// 4. a read at a computed address inside a local struct.
	p := pt{1, 2, 3}
	fmt.Println("4 read +8:", *(*int64)(unsafe.Add(unsafe.Pointer(&p), 8)))

	// 5. a write then base read, local struct.
	*(*int64)(unsafe.Add(unsafe.Pointer(&p), 16)) = 30
	fmt.Println("5 write +16:", p.z)

	// 6. a slice element's interior, written and read.
	b := []byte{1, 2, 3, 4, 5, 6, 7, 8}
	q := unsafe.Add(unsafe.Pointer(&b[0]), 4)
	*(*uint32)(q) = 0x0a0b0c0d
	fmt.Println("6 slice write +4:", b)
	fmt.Println("6 slice read +2:", *(*uint16)(unsafe.Add(unsafe.Pointer(&b[0]), 2)))

	// 7. an array field's interior.
	h := holder{n: 7, arr: [4]uint32{10, 20, 30, 40}}
	*(*uint32)(unsafe.Add(unsafe.Pointer(&h.arr[0]), 8)) = 33
	fmt.Println("7 array field write +8:", h.arr)
	fmt.Println("7 array field read via struct +12:", *(*uint32)(unsafe.Add(unsafe.Pointer(&h), 12)))

	// 8. the interior pointer is the only reference: the base must stay alive and readable across a collection.
	ip := interiorOfFresh()
	churn()
	fmt.Println("8 interior keeps base alive:", *ip)
	*ip = 22
	churn()
	fmt.Println("8 interior write survives:", *ip)

	// 10. the base slice stays referenced; the interior pointer is held across a collection.
	b2 := make([]byte, 16)
	q2 := (*uint32)(unsafe.Add(unsafe.Pointer(&b2[0]), 4))
	churn()
	*q2 = 0x01020304
	fmt.Println("10 slice interior held across GC:", b2[:8])

	// 11. the base struct stays referenced; the interior pointer is held across a collection.
	p3 := &pt{1, 2, 3}
	q3 := (*int64)(unsafe.Add(unsafe.Pointer(p3), 16))
	churn()
	*q3 = 33
	fmt.Println("11 struct interior held across GC:", p3.z)

	// 12. a struct that holds a string: the interior pointer is the only reference.
	mp := interiorOfMixed()
	churn()
	*mp = 66
	churn()
	fmt.Println("12 mixed interior:", *mp)
}
