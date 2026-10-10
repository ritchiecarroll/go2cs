package main

import (
	"fmt"
	"reflect"
)

type Pair[K, V any] struct {
	Key K
	Val V
}

type Box[T any] struct {
	inner T
	n     int
}

type Plain struct {
	a int
	s string
}

func main() {
	p := Pair[string, int]{"a", 1}
	fmt.Printf("%v %+v\n", p, p)
	b := Box[Plain]{Plain{2, "x"}, 3}
	fmt.Printf("%+v\n", b)
	t := reflect.TypeOf(p)
	fmt.Println(t.NumField(), t.Field(0).Name, t.Field(1).Type)
	fmt.Println(reflect.DeepEqual(b, Box[Plain]{Plain{2, "x"}, 3}))
	fmt.Printf("%v\n", Plain{7, "plain"})
}
