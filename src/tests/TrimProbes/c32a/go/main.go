// 3c-2a AOT probe: the type-only reflect sites 3c-2a moved onto a zero value's face (Zero, sized nested arrays, Make*,
// SetBytes on named byte slices) and the field-wise struct conversion, read back after a forced GC. Compared with Go's
// own output under Native AOT with a full trim. Avoids reflect.New (3c-2b's site).
package main

import (
	"fmt"
	"reflect"
	"runtime"
)

type Point struct {
	X, Y int
	Name string
}

type Named []int

type MyBytes []byte

type B1 byte

type Tagged1 struct {
	A int `json:"a"`
	S string
	L []int
	P *int
}

type Tagged2 struct {
	A int `xml:"a"`
	S string
	L []int
	P *int
}

func main() {
	fmt.Println("zero struct:", reflect.Zero(reflect.TypeOf(Point{})).Interface())
	fmt.Println("zero slice nil:", reflect.Zero(reflect.TypeOf([]int{})).IsNil())
	fmt.Println("zero map nil:", reflect.Zero(reflect.TypeOf(map[string]int{})).IsNil())

	var arr [2][3]int
	z := reflect.Zero(reflect.TypeOf(arr)).Interface().([2][3]int)
	fmt.Println("zero [2][3]int:", z, len(z[1]))

	s := reflect.MakeSlice(reflect.TypeOf(Named{}), 2, 4)
	s.Index(1).SetInt(9)
	fmt.Println("make named slice:", s.Interface(), s.Len(), s.Cap(), s.Type().String())

	m := reflect.MakeMap(reflect.TypeOf(map[string]int{}))
	m.SetMapIndex(reflect.ValueOf("k"), reflect.ValueOf(3))
	fmt.Println("make map:", m.Interface())

	c := reflect.MakeChan(reflect.TypeOf(make(chan int)), 1)
	c.Send(reflect.ValueOf(7))
	v, ok := c.Recv()
	fmt.Println("make chan:", v.Int(), ok)

	mb := MyBytes{1, 2, 3}
	reflect.ValueOf(&mb).Elem().SetBytes([]byte{9, 8})
	fmt.Println("setbytes named:", mb)

	b1 := []B1{1, 2}
	reflect.ValueOf(&b1).Elem().SetBytes([]byte{5, 6, 7})
	fmt.Println("setbytes named elem:", b1)

	n := 42
	t1 := Tagged1{A: 1, S: "s", L: []int{1, 2}, P: &n}
	t2 := reflect.ValueOf(t1).Convert(reflect.TypeOf(Tagged2{})).Interface().(Tagged2)
	t1.S = "changed"
	runtime.GC()
	fmt.Println("convert after GC:", t2.A, t2.S, t2.L, *t2.P)
}
