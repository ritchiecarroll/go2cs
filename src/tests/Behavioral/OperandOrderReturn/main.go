// Go's order in a return list: gc reads a plain operand after the list's calls.
package main

import "fmt"

type box struct{ f int }

var g int

func local() (int, int) {
	r := 1
	setR := func() int { r = 10; return 2 }
	return r, setR()
}

func global() (int, int) {
	g = 1
	setG := func() int { g = 10; return 2 }
	return g, setG()
}

func field(b *box) (int, int) {
	setF := func() int { b.f = 10; return 2 }
	return b.f, setF()
}

func named() (a, b int) {
	a = 1
	setA := func() int { a = 10; return 2 }
	return a, setA()
}

func main() {
	fmt.Println(local())
	fmt.Println(global())
	fmt.Println(field(&box{1}))
	fmt.Println(named())
}
