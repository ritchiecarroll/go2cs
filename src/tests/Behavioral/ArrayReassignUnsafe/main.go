package main

import (
	"fmt"
	"unsafe"
)

type small [8]byte

type holder struct {
	n   int32
	arr [4]uint32
}

var pkg [4]uint32

func main() {
	// 1. A local array: its address is taken, the array is assigned whole, a write goes through the address.
	var a [4]uint32
	p := unsafe.Pointer(&a)
	a = [4]uint32{1, 2, 3, 4}
	*(*uint32)(p) = 41
	fmt.Println("local:", a[0])

	// 2. The same with the address held as a number.
	var b [4]uint32
	n := uintptr(unsafe.Pointer(&b))
	b = [4]uint32{1, 2, 3, 4}
	*(*uint32)(unsafe.Pointer(n)) = 42
	fmt.Println("local by number:", b[0])

	// 3. Assigned from another variable.
	var c, other [4]uint32
	other[0] = 9
	q := unsafe.Pointer(&c)
	c = other
	*(*uint32)(q) = 43
	fmt.Println("from a variable:", c[0], other[0])

	// 4. A named array.
	var s small
	s[0] = 1
	r := unsafe.Pointer(&s)
	s = small{9, 9}
	*(*byte)(r) = 44
	fmt.Println("named:", s[0])

	// 5. A package variable, by number.
	m := uintptr(unsafe.Pointer(&pkg))
	pkg = [4]uint32{5, 6, 7, 8}
	*(*uint32)(unsafe.Pointer(m)) = 45
	fmt.Println("package:", pkg[0])

	// 6. A struct field.
	var h holder
	f := unsafe.Pointer(&h.arr)
	h.arr = [4]uint32{1, 2, 3, 4}
	*(*uint32)(f) = 46
	fmt.Println("field:", h.arr[0])

	// 7. A read through the address after the assignment.
	var d [4]uint32
	t := unsafe.Pointer(&d)
	d = [4]uint32{7, 7, 7, 7}
	fmt.Println("read:", *(*uint32)(t))

	// 8. Through the array pointer of the same element type.
	var e [4]uint32
	u := unsafe.Pointer(&e)
	e = [4]uint32{1, 2, 3, 4}
	(*[4]uint32)(u)[0] = 47
	fmt.Println("array pointer:", e[0])

	// 9. A view of the bytes (door A, V2) taken after the assignment.
	var g [4]uint32
	v := unsafe.Pointer(&g)
	_ = uintptr(v)
	g = [4]uint32{1, 2, 3, 4}
	(*[16]byte)(v)[:4][0] = 48
	fmt.Println("byte view:", g[0])

	// 10. Control: the array is written element by element and never assigned whole.
	var k [4]uint32
	w := unsafe.Pointer(&k)
	k[0] = 1
	*(*uint32)(w) = 49
	fmt.Println("control:", k[0])
}
