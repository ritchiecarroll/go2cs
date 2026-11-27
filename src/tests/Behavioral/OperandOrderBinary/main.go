// Go's order in a binary expression: gc reads a plain operand after the expression's calls and
// receives. && and || are the exception: the right side runs only after the left is known.
package main

import (
	"fmt"
	"time"
)

func main() {
	x := 1
	setX := func() int { x = 10; return 2 }
	fmt.Println("plus:", x+setX())

	x = 1
	fmt.Println("times:", x*setX())

	x = 1
	fmt.Println("equal:", x == setX()*5)

	s := "a"
	setS := func() string { s = "Z"; return "b" }
	fmt.Println("concat:", s+setS())

	y := 1
	ch := make(chan int)
	go func() { time.Sleep(30 * time.Millisecond); y = 10; ch <- 2 }()
	fmt.Println("receive:", y+<-ch)

	q := 1
	setQ := func() bool { q = 10; return true }
	fmt.Println("and-and (left first in Go):", q == 1 && setQ())
}
