// Probe: reflect Call, CallSlice and MakeFunc of variadic funcs whose families have value-type arguments, the shapes
// P1's variadic trampoline registry serves under Native AOT through the escape registry (each func value here is
// converted to an interface, so its signature registers).
package main

import (
	"fmt"
	"reflect"
	"strings"
)

type point struct{ x, y int }

func join(sep string, parts ...string) string { return strings.Join(parts, sep) }

func sum(base int, ps ...point) int {
	t := base
	for _, p := range ps {
		t += p.x + p.y
	}
	return t
}

func count(tag string, ns ...uint16) {
	fmt.Println("count", tag, len(ns))
}

func main() {
	var f any = join
	fmt.Println("call", reflect.ValueOf(f).Call([]reflect.Value{reflect.ValueOf("-"), reflect.ValueOf("a"), reflect.ValueOf("b")})[0].String())
	fmt.Println("callslice", reflect.ValueOf(f).CallSlice([]reflect.Value{reflect.ValueOf("+"), reflect.ValueOf([]string{"c", "d"})})[0].String())

	var g any = sum
	fmt.Println("struct tail", reflect.ValueOf(g).Call([]reflect.Value{reflect.ValueOf(1), reflect.ValueOf(point{2, 3}), reflect.ValueOf(point{4, 5})})[0].Int())

	var h any = count
	reflect.ValueOf(h).Call([]reflect.Value{reflect.ValueOf("u16"), reflect.ValueOf(uint16(7)), reflect.ValueOf(uint16(8))})

	var mk func(string, ...string) string
	fn := reflect.MakeFunc(reflect.TypeOf(mk), func(args []reflect.Value) []reflect.Value {
		return []reflect.Value{reflect.ValueOf(args[0].String() + fmt.Sprint(args[1].Len()))}
	})
	reflect.ValueOf(&mk).Elem().Set(fn)
	fmt.Println("makefunc", mk("n", "x", "y"))
}
