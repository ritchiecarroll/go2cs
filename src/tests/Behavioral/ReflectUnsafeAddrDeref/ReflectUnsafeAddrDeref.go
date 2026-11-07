// A reflect address read back through a pointer of ANOTHER layout-identical type, as gojq's code tests do:
// `*(*int)(unsafe.Pointer(v.UnsafeAddr()))` over a field of a defined type (`op opcode`). Go's direct form of
// the same pointer, `(*int)(unsafe.Pointer(&r.op))`, aliases the field; through reflect's number it must too.
package main

import (
	"fmt"
	"reflect"
	"unsafe"
)

type opcode int

type code struct {
	v  any
	op opcode
}

type codeInt struct {
	v  any
	op int
}

type plainNamed struct {
	a  int
	op opcode
}

type plain struct {
	a int
	b int
}

type bits struct {
	n int
	f float64
}

func try(name string, f func() any) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println(name, "PANIC", r)
		}
	}()
	fmt.Println(name, f())
}

func main() {
	p := plain{4, 5}
	try("plain int as *int", func() any {
		return *(*int)(unsafe.Pointer(reflect.ValueOf(&p).Elem().Field(1).UnsafeAddr()))
	})
	pn := plainNamed{4, 6}
	try("plain opcode as *int", func() any {
		return *(*int)(unsafe.Pointer(reflect.ValueOf(&pn).Elem().Field(1).UnsafeAddr()))
	})
	try("plain opcode as *opcode", func() any {
		return int(*(*opcode)(unsafe.Pointer(reflect.ValueOf(&pn).Elem().Field(1).UnsafeAddr())))
	})
	ci := &codeInt{v: 7, op: 3}
	try("refbearing int as *int", func() any {
		return *(*int)(unsafe.Pointer(reflect.ValueOf(ci).Elem().Field(1).UnsafeAddr()))
	})
	c := &code{v: 7, op: 3}
	try("refbearing opcode as *int", func() any {
		return *(*int)(unsafe.Pointer(reflect.ValueOf(c).Elem().Field(1).UnsafeAddr()))
	})
	try("refbearing opcode as *opcode", func() any {
		return int(*(*opcode)(unsafe.Pointer(reflect.ValueOf(c).Elem().Field(1).UnsafeAddr())))
	})
	codes := []*code{{v: 7, op: 3}, {v: "s", op: 11}}
	try("slice elem opcode as *int", func() any {
		return *(*int)(unsafe.Pointer(reflect.ValueOf(codes).Index(1).Elem().FieldByName("op").UnsafeAddr()))
	})
	try("write opcode via *int", func() any {
		*(*int)(unsafe.Pointer(reflect.ValueOf(c).Elem().Field(1).UnsafeAddr())) = 42
		return int(c.op)
	})
	try("interface field as *any", func() any {
		return *(*any)(unsafe.Pointer(reflect.ValueOf(c).Elem().Field(0).UnsafeAddr()))
	})
	bs := bits{-3, 1.5}
	bv := reflect.ValueOf(&bs).Elem()
	try("int as *uint", func() any { return *(*uint)(unsafe.Pointer(bv.Field(0).UnsafeAddr())) })
	try("float64 as *uint64", func() any { return *(*uint64)(unsafe.Pointer(bv.Field(1).UnsafeAddr())) })
}
