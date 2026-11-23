// MnGoOrder: the order gc evaluates an operand against a call that parks (row 7's ref-blocked seat,
// docs/phase4/DATA-mn-row7-refblock.md). Each line is one operand class, written in the shape the twin
// generator could not move before the cure: a Printf whose format comes first, then the operand under test,
// then a call that writes that operand and can park. gc reads the operand AFTER the call.
package main

import "fmt"

type T struct{ x int }

// ch is buffered, so set never blocks, but it can park: the twin generator twins it.
var ch = make(chan int, 1)

func Set(p *int, v int) int {
	*p = v
	ch <- 1
	<-ch
	return 3
}

var hit bool

func effect() int {
	hit = true
	ch <- 1
	<-ch
	return 7
}

func param(p int) {
	fmt.Printf("parameter %d %d\n", p, Set(&p, 2))
}

func Grow(s *[]int) int {
	*s = append(*s, 9)
	ch <- 1
	<-ch
	return 1
}

func main() {
	a := 1
	fmt.Printf("local %d %d\n", a, Set(&a, 2))

	param(1)

	s := T{x: 1}
	fmt.Printf("field-of-value %d %d\n", s.x, Set(&s.x, 2))

	ps := &T{x: 1}
	fmt.Printf("field-through-pointer %d %d\n", ps.x, Set(&ps.x, 2))

	nilPointer()

	u := 1
	fmt.Printf("unary %d %d\n", -u, Set(&u, 2))

	b := 1
	fmt.Printf("binary-left %d\n", b+Set(&b, 2))

	c := 1
	fmt.Printf("cast %v %d\n", float64(c), Set(&c, 2))

	sliceToArray()
}

// A named function, not a function literal called in place: a literal runs its plain half when invoked directly.
func nilPointer() {
	defer func() {
		r := recover()
		fmt.Printf("nil-pointer panicked=%v effect-before-panic=%v\n", r != nil, hit)
	}()
	var pn *T
	fmt.Printf("nil-pointer %d %d\n", pn.x, effect())
}

func sliceToArray() {
	defer func() {
		r := recover()
		fmt.Printf("slice-to-array panicked=%v\n", r != nil)
	}()
	sl := []int{1}
	fmt.Printf("slice-to-array %v %d\n", [2]int(sl), Grow(&sl))
}
