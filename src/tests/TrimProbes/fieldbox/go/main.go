// Trim stage 3d, family E probe (COORD 2026-10-12): reflect field access through a pointer, which golib serves with a
// DynamicMethod field accessor (buildFieldAccessor) that Native AOT cannot run. Each line reaches it a different way.
// Prints only shape and values the program set, never an address.
package main

import (
	"fmt"
	"reflect"
)

type T struct {
	A int
	S string
}

type Inner struct{ X int }

type Outer struct {
	Inner
	Y int
}

type Base struct{ A int }

type Def Base

func main() {
	// 1. Value.Addr on a field, written through the pointer it hands out.
	s := T{1, "x"}
	p := reflect.ValueOf(&s).Elem().Field(0).Addr().Interface().(*int)
	*p = 41
	fmt.Println("1 Addr write-through:", s.A)

	// 2. fmt's %v of a pointer to a struct with no String method (fmt walks the fields).
	fmt.Println("2 fmt &struct:", &T{2, "y"})

	// 3. SetInt through Field(0) of a pointed-to struct, read back through the Go variable.
	reflect.ValueOf(&s).Elem().Field(0).SetInt(42)
	fmt.Println("3 SetInt read back:", s.A)

	// 4. Pointer identity through reflect: &s.A == &s.A, and equal to the address converted code takes.
	a1 := reflect.ValueOf(&s).Elem().Field(0).Addr().Interface().(*int)
	a2 := reflect.ValueOf(&s).Elem().Field(0).Addr().Interface().(*int)
	fmt.Println("4 identity:", a1 == a2, a1 == &s.A)

	// 5. A promoted embedded field, set by name and read back through its own pointer; the embed's identity against &o.Inner.
	// (The promoted field's own identity, x == &o.X, is a separate pre-existing defect, sized on its own: 2026-10-12.)
	var o Outer
	reflect.ValueOf(&o).Elem().FieldByName("X").SetInt(7)
	x := reflect.ValueOf(&o).Elem().FieldByName("X").Addr().Interface().(*int)
	in := reflect.ValueOf(&o).Elem().Field(0).Addr().Interface().(*Inner)
	fmt.Println("5 promoted:", o.X, *x, in == &o.Inner)

	// 6. A defined type over a struct.
	var d Def
	reflect.ValueOf(&d).Elem().Field(0).SetInt(9)
	fmt.Println("6 defined over struct:", d.A)
}
