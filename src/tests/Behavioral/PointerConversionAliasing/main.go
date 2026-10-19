package main

import (
	"fmt"
	"unsafe"
)

type Employee struct {
	Name string
	Age  int
}

type Person struct {
	Name string
	Age  int
}

type Staff Employee

type Tagged struct {
	Name string `json:"name"`
	Age  int    `json:"age"`
}

func rename(p *Person, name string) {
	p.Name = name
}

func named() {
	e := &Employee{Name: "ann", Age: 30}
	p := (*Person)(e)

	p.Name = "bob"
	fmt.Println("named, write through converted:", e.Name)

	e.Age = 31
	fmt.Println("named, write through original:", p.Age)

	fmt.Println("named, same address:", unsafe.Pointer(p) == unsafe.Pointer(e))
	fmt.Println("named, round trip identity:", (*Employee)(p) == e)
}

func nilPointer() {
	var e *Employee
	p := (*Person)(e)
	fmt.Println("nil, stays nil:", p == nil)
}

func viaUnsafe() {
	e := &Employee{Name: "cat", Age: 5}
	p := (*Person)(unsafe.Pointer(e))

	p.Age = 6
	fmt.Println("unsafe, write through converted:", e.Age)
	fmt.Println("unsafe, round trip identity:", (*Employee)(unsafe.Pointer(p)) == e)
}

func anonymous() {
	a := &struct {
		Name string
		Age  int
	}{"dot", 7}
	p := (*Person)(a)

	p.Age = 8
	fmt.Println("anonymous, write through converted:", a.Age)

	a.Name = "dov"
	fmt.Println("anonymous, write through original:", p.Name)
	fmt.Println("anonymous, same address:", unsafe.Pointer(p) == unsafe.Pointer(a))
}

func anonymousNil() {
	var a *struct {
		Name string
		Age  int
	}
	p := (*Person)(a)
	fmt.Println("anonymous nil, stays nil:", p == nil)
}

func tagsOnlyNamed() {
	t := &Tagged{Name: "kim", Age: 18}
	p := (*Person)(t)

	p.Age = 19
	fmt.Println("tags only named, write through converted:", t.Age)
	fmt.Println("tags only named, round trip identity:", (*Tagged)(p) == t)
}

func equality() {
	e := &Employee{Name: "cy", Age: 17}
	p := (*Person)(e)
	fmt.Println("equality, two conversions:", (*Person)(e) == (*Person)(e))
	fmt.Println("equality, conversion and variable:", p == (*Person)(e))
}

func definedType() {
	e := &Employee{Name: "eve", Age: 9}
	s := (*Staff)(e)

	s.Name = "fay"
	fmt.Println("defined, write through converted:", e.Name)
}

func callArgument() {
	e := &Employee{Name: "gus", Age: 10}
	rename((*Person)(e), "hal")
	fmt.Println("argument, write through converted:", e.Name)
}

func sliceElement() {
	es := []*Employee{{Name: "ivy", Age: 11}}
	p := (*Person)(es[0])

	p.Age = 12
	fmt.Println("slice element, write through converted:", es[0].Age)
}

func fieldAddress() {
	type pair struct {
		First  Employee
		Second Employee
	}

	var pr pair
	p := (*Person)(&pr.Second)

	p.Name = "jan"
	fmt.Println("field address, write through converted:", pr.Second.Name)
}

func main() {
	named()
	nilPointer()
	viaUnsafe()
	anonymous()
	anonymousNil()
	tagsOnlyNamed()
	equality()
	definedType()
	callArgument()
	sliceElement()
	fieldAddress()
}
