// Seat #34's two Native AOT gaps in golib, each as a Go program reaches it (red first under Native
// AOT; under the JIT every line below matches Go).
//
//  1. TYPE OPS: lib.Hidden has no generated operations face (see package lib), so a reflect walk
//     that needs its operations reaches golib's dynamic fallback, which closes GoTypeOps<T> over
//     the type at run time. Under Native AOT a value type no compiled code names cannot be closed
//     there.
//  2. METHOD TYPE: reflect's Method(i).Type over a value-receiver method closes a Func<...> family
//     over the method's parameter types at run time. Under Native AOT that raised a raw .NET
//     exception that no Go recover() sees.
//
// Each scenario runs under its own recover, so a refusal reads as one named line and the rest still
// run. An argument selects one gap (`AotReflectGaps 1` or `AotReflectGaps 2`), so a gap that ends the
// process cannot hide the other; with none, both run.
package main

import (
	"fmt"
	"os"
	"reflect"

	"AotReflectGaps/lib"
)

// Holder's exported field is the only pointer to lib.Hidden anywhere.
type Holder struct {
	P *lib.Hidden
	N int
}

// Num has value-receiver methods only.
type Num struct {
	N int
}

func (n Num) Add(d int, label string) (int, string) { return n.N + d, label }

func (n Num) Scale(f float64) float64 { return float64(n.N) * f }

func run(name string, f func()) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: recovered: %v\n", name, r)
		}
	}()
	f()
}

func main() {
	only := ""
	if len(os.Args) > 1 {
		only = os.Args[1]
	}
	if only == "" || only == "1" {
		typeOps()
	}
	if only == "" || only == "2" {
		methodTypes()
	}
}

func typeOps() {
	var h Holder
	h.N = 3
	ft := reflect.TypeOf(h).Field(0).Type

	run("1a fmt %v nil field", func() { fmt.Printf("1a %v\n", h) })
	run("1b fmt %+v nil field", func() { fmt.Printf("1b %+v\n", h) })
	run("1c field type", func() { fmt.Println("1c", ft, ft.Elem(), ft.Elem().Kind()) })
	run("1d reflect.New elem", func() {
		p := reflect.New(ft.Elem())
		p.Elem().Field(0).SetInt(7)
		fmt.Printf("1d %v %T\n", p.Elem().Interface(), p.Interface())
	})
	run("1e reflect.Zero elem", func() { fmt.Printf("1e %+v\n", reflect.Zero(ft.Elem()).Interface()) })
	run("1f reflect.Zero ptr", func() { fmt.Printf("1f %v\n", reflect.Zero(ft).IsNil()) })
	run("1g set field through New", func() {
		v := reflect.ValueOf(&h).Elem()
		v.Field(0).Set(reflect.New(ft.Elem()))
		fmt.Printf("1g %+v\n", *h.P)
	})
}

func methodTypes() {
	nt := reflect.TypeOf(Num{})
	run("2a Method(0).Type value", func() { m := nt.Method(0); fmt.Println("2a", m.Name, m.Type) })
	run("2b Method(1).Type value", func() { m := nt.Method(1); fmt.Println("2b", m.Name, m.Type) })
	run("2c Method(0).Type pointer", func() { m := reflect.PtrTo(nt).Method(0); fmt.Println("2c", m.Name, m.Type) })
	run("2d Method call", func() {
		out := reflect.ValueOf(Num{N: 2}).Method(0).Call([]reflect.Value{reflect.ValueOf(5), reflect.ValueOf("x")})
		fmt.Println("2d", out[0].Int(), out[1].String())
	})
}
