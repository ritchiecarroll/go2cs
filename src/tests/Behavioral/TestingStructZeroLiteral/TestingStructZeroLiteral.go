// Empty composite literals of the hand-owned testing structs (testify's require tests: `t :=
// &testing.T{}`).
//
// An empty literal of a foreign struct converts to `new T(nil)`, which binds the zero-value
// constructor go2cs-gen gives every converted struct. testing's T, M, B, PB and F are hand-owned C#
// structs with no generator behind them, so the literal was CS1729 until each declared that
// constructor by hand.
package main

import (
	"fmt"
	"testing"
)

func main() {
	t := &testing.T{}
	m := &testing.M{}
	f := &testing.F{}
	b := testing.B{}
	pb := testing.PB{}

	fmt.Println("T:", t != nil)
	fmt.Println("M:", m != nil)
	fmt.Println("F:", f != nil)
	fmt.Println("B.N:", b.N)
	_ = pb
	fmt.Println("PB: constructed")
}
