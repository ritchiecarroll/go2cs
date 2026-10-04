package main

import (
	"fmt"
	"reflect"
	"unsafe"
)

type inner struct{ a string }

// Q holds no reference, so its storage is plain memory rather than a managed reference graph.
type Q struct {
	A int
	b int
	c int32
}

type P struct {
	Public  int
	private int
	s       inner
	ptr     *int
}

// field reads a struct field the way go-cmp's retrieveUnexportedField does: the struct's address
// plus the field's Go offset, re-typed through reflect.NewAt.
func field(v reflect.Value, i int) reflect.Value {
	f := v.Type().Field(i)
	return reflect.NewAt(f.Type, unsafe.Pointer(uintptr(unsafe.Pointer(v.UnsafeAddr()))+f.Offset)).Elem()
}

func main() {
	n := 7
	x := P{1, 2, inner{"x"}, &n}
	v := reflect.ValueOf(&x).Elem()

	for i := 0; i < v.NumField(); i++ {
		ve := field(v, i)
		fmt.Println(v.Type().Field(i).Name, ve.Kind(), ve.CanSet())
	}

	fmt.Println(field(v, 0).Interface(), field(v, 1).Interface(), field(v, 2).Interface(), *field(v, 3).Interface().(*int))

	// The result aliases the field: a write through it lands in x.
	field(v, 1).SetInt(20)
	field(v, 2).Set(reflect.ValueOf(inner{"z"}))
	fmt.Println(x.private, x.s.a)

	// Differing unexported fields compare unequal (go-cmp's IgnoreUnexported cases).
	y := P{1, 3, inner{"y"}, &n}
	w := reflect.ValueOf(&y).Elem()
	fmt.Println(field(v, 1).Interface() == field(w, 1).Interface())
	fmt.Println(reflect.DeepEqual(field(v, 2).Interface(), field(w, 2).Interface()))

	// The same reads and a write over a struct of plain values.
	q := Q{4, 5, 6}
	u := reflect.ValueOf(&q).Elem()
	fmt.Println(field(u, 0).Interface(), field(u, 1).Interface(), field(u, 2).Interface())
	field(u, 1).SetInt(50)
	fmt.Println(q.b)

	// NewAt over a pointer's own address aliases the variable.
	p := reflect.NewAt(reflect.TypeOf(0), unsafe.Pointer(&n))
	p.Elem().SetInt(9)
	fmt.Println(n)
}
