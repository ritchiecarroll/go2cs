// Row G AOT probe (COORD 2026-10-09): reflect's pointee-keyed sites for CONTAINER value types -- the population row G
// takes the operations face off (array, slice, map, chan). Compared with Go's own output under Native AOT, full trim.
// Avoids Value.Field (family E's DynamicMethod).
package main

import (
	"fmt"
	"reflect"
)

type Point struct{ X, Y int }

func main() {
	a := reflect.New(reflect.TypeOf([4]int{}))
	a.Elem().Index(2).SetInt(7)
	fmt.Println("new [4]int:", *a.Interface().(*[4]int))

	s := reflect.New(reflect.TypeOf([]int{}))
	fmt.Println("new []int elem nil:", s.Elem().IsNil())

	m := reflect.New(reflect.TypeOf(map[string]int{}))
	fmt.Println("new map elem nil:", m.Elem().IsNil())

	c := reflect.New(reflect.TypeOf(make(chan int)))
	fmt.Println("new chan elem nil:", c.Elem().IsNil())

	ps := reflect.New(reflect.TypeOf([]Point{}))
	ps.Elem().Set(reflect.ValueOf([]Point{{1, 2}}))
	fmt.Println("new []Point len:", ps.Elem().Len())

	fmt.Println("zero *[4]int is nil:", reflect.Zero(reflect.TypeOf(&[4]int{})).IsNil())
	fmt.Println("zero *[]int is nil:", reflect.Zero(reflect.TypeOf(&[]int{})).IsNil())
}
