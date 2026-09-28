package main

import (
	"fmt"
	"unsafe"
)

// Assigning to an unsafe.Pointer PARAMETER rebinds the parameter, exactly as assigning to a local
// does: the caller's own pointer is untouched. go2cs rendered the parameter form as a write
// through golib's pointer object (`p.Value = q`), which moved the caller's pointer instead.

// rebind is `p = q`.
func rebind(p, q unsafe.Pointer) unsafe.Pointer {
	p = q
	return p
}

// advance is `p = unsafe.Pointer(uintptr(p) + n)`, runtime's frame-pointer walk shape.
func advance(p unsafe.Pointer, n uintptr) unsafe.Pointer {
	p = unsafe.Pointer(uintptr(p) + n)
	return p
}

// step is `p = add(p, n)` through unsafe.Add, runtime's cgocheck and memhash shape.
func step(p unsafe.Pointer, n int) unsafe.Pointer {
	p = unsafe.Add(p, n)
	return p
}

// clear assigns nil.
func clear(p unsafe.Pointer) bool {
	p = nil
	return p == nil
}

// local is the control: the same assignment to a local.
func local(q unsafe.Pointer) unsafe.Pointer {
	var p unsafe.Pointer
	p = q
	return p
}

func main() {
	a, b := 1, 2
	pa, pb := unsafe.Pointer(&a), unsafe.Pointer(&b)

	r := rebind(pa, pb)
	fmt.Println("rebind:", *(*int)(r), "caller's pa still reads", *(*int)(pa))

	arr := [4]int32{10, 20, 30, 40}
	p0 := unsafe.Pointer(&arr[0])
	s := advance(p0, unsafe.Sizeof(arr[0]))
	fmt.Println("advance:", *(*int32)(s), "caller's p0 still reads", *(*int32)(p0))

	t := step(p0, 2*int(unsafe.Sizeof(arr[0])))
	fmt.Println("step:", *(*int32)(t), "caller's p0 still reads", *(*int32)(p0))

	fmt.Println("clear:", clear(pa), "caller's pa still reads", *(*int)(pa))
	fmt.Println("local:", *(*int)(local(pb)), a, b)
}
