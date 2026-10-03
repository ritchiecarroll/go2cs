package main

import (
	"fmt"
	"reflect"
)

// A named FUNC type with a method, held NIL inside an interface. The interface is not nil (it has
// a dynamic type), so reflect hands out its method value; the method runs with a nil receiver,
// and only CALLING the receiver panics.

type Greeter func() string

func (g Greeter) Greet() string {
	if g == nil {
		return "nil greeter"
	}
	return g()
}

func (g Greeter) Shout() string { return g() + "!" }

type Greetable interface {
	Greet() string
	Shout() string
}

func call(label string, f func() string) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println(label, "panic:", r)
		}
	}()
	fmt.Println(label, "=", f())
}

func main() {
	var g Greeter
	var i Greetable = g
	fmt.Println("interface nil:", i == nil)

	v := reflect.ValueOf(&i).Elem()
	fmt.Println("kind:", v.Kind(), "elem type:", v.Elem().Type(), "elem nil:", v.Elem().IsNil())
	fmt.Println("NumMethod:", v.NumMethod())

	m := v.Method(0)
	fmt.Println("method kind:", m.Kind(), "type:", m.Type())
	call("Method(0).Call", func() string { return m.Call(nil)[0].String() })
	call("MethodByName(Shout).Call", func() string { return v.MethodByName("Shout").Call(nil)[0].String() })

	h := Greeter(func() string { return "hello" })
	i = h
	v = reflect.ValueOf(&i).Elem()
	call("non-nil Method(0).Call", func() string { return v.Method(0).Call(nil)[0].String() })
	call("non-nil Method(1).Call", func() string { return v.Method(1).Call(nil)[0].String() })
}
