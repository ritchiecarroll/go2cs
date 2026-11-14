package main

import (
	"errors"
	"fmt"
)

type point struct{ x, y int }

type named struct{ n string }

func (n named) String() string { return "named " + n.n }

func main() {
	fmt.Println("hello")
	fmt.Println(point{1, 2})
	fmt.Println(&point{3, 4})
	fmt.Println(named{"a"})
	fmt.Println(errors.New("e"))
	fmt.Printf("%v %d\n", []int{1}, 7)
}
