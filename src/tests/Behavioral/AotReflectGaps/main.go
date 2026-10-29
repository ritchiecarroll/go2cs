// Seat #34's two Native AOT gaps in golib, each as a Go program reaches it (red first under Native
// AOT; under the JIT every line below matches Go).
//
//  1. TYPE OPS: lib.Hidden has no generated operations face (see package lib), so a reflect walk
//     that needs its operations reaches golib's dynamic fallback, which closes GoTypeOps<T> over
//     the type at run time. Under Native AOT a value type no compiled code names cannot be closed
//     there.
//
//  2. METHOD TYPE: reflect's Method(i).Type over a value-receiver method closes a Func<...> family
//     over the method's parameter types at run time. Under Native AOT that raised a raw .NET
//     exception that no Go recover() sees.
//
//     Under Native AOT the family over Num's methods closed at the base: go2cs-gen's method set
//     (IGoMethodSet) names those delegate types in compiled code. Two shapes name none:
//
//  3. an interface method, whose func type no generated method set carries (Shaper's signature
//     is spelled nowhere else);
//
//  4. a struct no code ever builds as a value (Ghost is reached only through a nil *Ghost), whose
//     method set Native AOT has no reason to compile.
//
// Each scenario runs under its own recover, so a refusal reads as one named line and the rest still
// run. An argument selects one group (`AotReflectGaps 1` through `AotReflectGaps 4`), so a group that
// ends the process cannot hide another; with none, all run.
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

// Shaper's method signature appears nowhere but here.
type Shaper interface {
	Area(a int8, b uint16) complex64
}

// Ghost is never built as a value: only a nil *Ghost reaches reflect.
type Ghost struct {
	X int
}

func (g Ghost) Haunt(b bool, r rune) uint32 { return uint32(g.X) + uint32(r) }

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
	if only == "" || only == "3" {
		run("3a interface Method(0).Type", func() {
			m := reflect.TypeOf((*Shaper)(nil)).Elem().Method(0)
			fmt.Println("3a", m.Name, m.Type)
		})
	}
	if only == "" || only == "4" {
		gt := reflect.TypeOf((*Ghost)(nil))
		run("4a never-built Method(0).Type value", func() { m := gt.Elem().Method(0); fmt.Println("4a", m.Name, m.Type) })
		run("4b never-built Method(0).Type pointer", func() { m := gt.Method(0); fmt.Println("4b", m.Name, m.Type) })
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
