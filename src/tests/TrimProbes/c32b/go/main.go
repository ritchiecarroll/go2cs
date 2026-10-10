// 3c-2b AOT probe: reflect's pointee-keyed sites (reflect.New, reflect.Zero of a pointer type) through the generated
// type operations at fixed depth, the builtins table, and the one dynamic fallback (an interface, a pointer to a pointer),
// compared with Go's own output under Native AOT with a full trim. Avoids Value.Field (family E's DynamicMethod).
package main

import (
	"fmt"
	"reflect"
)

type Point struct{ X, Y int }

type Named []int

func main() {
	p := reflect.New(reflect.TypeOf(Point{}))
	p.Elem().Set(reflect.ValueOf(Point{1, 2}))
	fmt.Println("new struct:", *p.Interface().(*Point))

	n := reflect.New(reflect.TypeOf(Named{}))
	n.Elem().Set(reflect.ValueOf(Named{3, 4}))
	fmt.Println("new named slice:", *n.Interface().(*Named))

	i := reflect.New(reflect.TypeOf(0))
	i.Elem().SetInt(5)
	fmt.Println("new int:", *i.Interface().(*int))

	fmt.Println("zero *Point is nil:", reflect.Zero(reflect.TypeOf(&Point{})).IsNil())

	pp := reflect.New(reflect.TypeOf(&Point{}))
	fmt.Println("new *Point (box pointee, fixed depth) elem nil:", pp.Elem().IsNil())

	e := reflect.New(reflect.TypeOf((*error)(nil)).Elem())
	fmt.Println("new error (builtins table) elem nil:", e.Elem().IsNil())

	ppp := reflect.New(reflect.TypeOf((**Point)(nil)))
	fmt.Println("new **Point (fallback) elem nil:", ppp.Elem().IsNil())

	st := reflect.New(reflect.TypeOf((*fmt.Stringer)(nil)).Elem())
	fmt.Println("new fmt.Stringer (fallback) elem nil:", st.Elem().IsNil())
}
