// A NAMED func type (`type AssignA func() int`) is assignable to its unnamed func type and back --
// identical underlying types, one side unnamed -- so reflect passes and stores a named func value
// wherever the unnamed type is wanted, and the reverse. go-cmp's EqualMethod/AssignA row calls
// `func (x AssignA) Equal(y func() int) bool` through Method(...).Func.Call; reflect refused it
// with "Call using AssignA as type func() int" because the generated named func is its own
// delegate type. Two DIFFERENT named func types stay unassignable, as in Go.
package main

import (
	"fmt"
	"reflect"
)

type AssignA func() int

// Other carries a method so it is its own named type in the conversion: a METHODLESS named func
// type is rendered as its base delegate, which reflect cannot tell from the unnamed type.
type Other func() int

func (o Other) Name() string { return "other" }

func (x AssignA) Equal(y func() int) bool { return x() == y() }

func apply(f func() int) int { return f() * 10 }

func main() {
	a := AssignA(func() int { return 7 })

	// go-cmp's shape: the method found by name, called with the named value as the unnamed argument.
	m, _ := reflect.TypeOf(a).MethodByName("Equal")
	fmt.Println("assignable:", m.Type.In(0).AssignableTo(m.Type.In(1)))
	fmt.Println("Equal via Call:", m.Func.Call([]reflect.Value{reflect.ValueOf(a), reflect.ValueOf(a)})[0].Bool())

	// A plain function taking the unnamed type, called with the named value.
	fmt.Println("apply via Call:", reflect.ValueOf(apply).Call([]reflect.Value{reflect.ValueOf(a)})[0].Int())

	// Set, both directions.
	var f func() int
	reflect.ValueOf(&f).Elem().Set(reflect.ValueOf(a))
	fmt.Println("named into unnamed:", f())

	var b AssignA
	reflect.ValueOf(&b).Elem().Set(reflect.ValueOf(func() int { return 9 }))
	fmt.Println("unnamed into named:", b())

	// The control: two different named func types are not assignable.
	func() {
		defer func() { fmt.Println("named into other named:", recover()) }()
		var o Other
		reflect.ValueOf(&o).Elem().Set(reflect.ValueOf(a))
	}()
}
