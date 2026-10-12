// Stringer probe (COORD 2026-10-12): under Native AOT with a full trim, fmt printed reflect.Kind as "17" where Go prints
// "array". Each line below reaches a String() method a different way, so the failing lines name the class.
package main

import (
	"fmt"
	"reflect"
	"time"
)

type K uint

func (k K) String() string { return "kay" }

type S struct{ n int }

func (s S) String() string { return "ess" }

type P struct{ n int }

func (p *P) String() string { return "pee" }

func main() {
	fmt.Println("1 reflect.Kind:", reflect.Array)
	fmt.Println("2 time.Duration:", 1500*time.Millisecond)
	fmt.Println("3 main named int:", K(3))
	fmt.Println("4 main struct:", S{1})
	fmt.Println("5 main pointer receiver:", &P{1})
	fmt.Printf("6 %%v of a Kind: %v, %%s: %s, %%d: %d\n", reflect.Slice, reflect.Map, reflect.Int)

	var x any = reflect.Struct
	s, ok := x.(fmt.Stringer)
	fmt.Println("7 assert Kind to fmt.Stringer:", ok, s != nil)

	fmt.Println("8 direct call:", reflect.Chan.String())
}
