// ReflectShellAccessors: reflect reads a named type of ANOTHER package that is held in a local interface. The
// value reaches the interface two ways: "va", a static conversion (the value adapter, or the pointer adapter), and
// "sh", an any-to-interface assertion (the interface shell). Go reads the value behind the interface in both: the
// formatting verbs, the accessors, Interface, the identity of two Values of one interface value, Set, and
// sort.Slice, which reads through internal/reflectlite.
package main

import (
	"fmt"
	"reflect"
	"sort"

	"ReflectShellAccessors/typelib"
)

type namer interface {
	Name() string
}

func viaAny(a any) namer {
	return a.(namer)
}

// show prints the held value with the verbs that go through reflect.
func show(label string, n namer) {
	fmt.Println(label, n.Name())
	fmt.Printf("%T\n", n)
	fmt.Printf("%v\n", n)
	fmt.Printf("%d\n", n)
	fmt.Printf("%+v\n", n)
}

// reads prints what the accessors of the held value's kind answer.
func reads(label string, n namer) {
	v := reflect.ValueOf(n)
	fmt.Println(label, v.Kind(), v.Type(), v.NumMethod(), v.IsZero())
	switch v.Kind() {
	case reflect.Int:
		fmt.Println(v.Int(), v.CanInt())
	case reflect.String:
		fmt.Println(v.String(), v.Len(), v.Index(1).Uint())
	case reflect.Slice:
		fmt.Println(v.Len(), v.Cap(), v.Index(1).Int(), v.Slice(1, 3).Len(), v.IsNil())
	case reflect.Map:
		fmt.Println(v.Len(), len(v.MapKeys()), v.MapIndex(reflect.ValueOf("a")).Int(), v.IsNil())
		it := v.MapRange()
		for it.Next() {
			fmt.Println(it.Key().String(), it.Value().Int())
		}
	case reflect.Struct:
		fmt.Println(v.NumField(), v.Field(0).String())
	case reflect.Pointer:
		fmt.Println(v.IsNil(), v.Elem().Field(0).String())
	}
}

// round prints what Interface hands back, whether two Values of one interface value are the same Value, and
// what Set stores into an interface slot.
func round(label string, n namer) {
	v := reflect.ValueOf(n)
	back := v.Interface()
	fmt.Println(label, back.(namer).Name(), reflect.ValueOf(n) == reflect.ValueOf(n))
	fmt.Printf("%T\n", back)
	var slot namer
	reflect.ValueOf(&slot).Elem().Set(v)
	fmt.Println(slot.Name())
}

// original prints whether Interface hands back a value equal to the one that went in, for the comparable kinds.
func original(label string, n namer) {
	fmt.Println(label, reflect.ValueOf(n).Interface() == n)
}

func each(label string, n namer) {
	show(label, n)
	reads(label, n)
	round(label, n)
}

func main() {
	var vaList namer = typelib.NewList()
	each("va-slice", vaList)
	each("sh-slice", viaAny(typelib.NewList()))

	var vaDict namer = typelib.NewDict()
	each("va-map", vaDict)
	each("sh-map", viaAny(typelib.NewDict()))

	var vaMark namer = typelib.NewMark()
	each("va-struct", vaMark)
	each("sh-struct", viaAny(typelib.NewMark()))
	original("va-struct", vaMark)
	original("sh-struct", viaAny(typelib.NewMark()))

	var vaBox namer = typelib.NewBox()
	each("va-pointer", vaBox)
	each("sh-pointer", viaAny(typelib.NewBox()))

	var vaLabel namer = typelib.Label("hey")
	each("va-string", vaLabel)
	each("sh-string", viaAny(typelib.Label("hey")))
	original("va-string", vaLabel)
	original("sh-string", viaAny(typelib.Label("hey")))

	var vaCount namer = typelib.Count(7)
	each("va-int", vaCount)
	each("sh-int", viaAny(typelib.Count(7)))
	original("va-int", vaCount)
	original("sh-int", viaAny(typelib.Count(7)))

	// sort.Slice takes its slice as an any and reads it through internal/reflectlite.
	vaSorted := typelib.NewList()
	var vaHeld namer = vaSorted
	sort.Slice(vaHeld, func(i, j int) bool { return vaSorted[i] < vaSorted[j] })
	fmt.Println("va-sort", vaSorted)

	shSorted := typelib.NewList()
	sort.Slice(viaAny(shSorted), func(i, j int) bool { return shSorted[i] < shSorted[j] })
	fmt.Println("sh-sort", shSorted)
}
