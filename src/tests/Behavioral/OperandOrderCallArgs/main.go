// Go's order for a call's arguments: gc evaluates the calls and receives in an argument list first,
// left to right, and reads a plain operand (a variable, a field, a dereference, an element) after
// them. Each line passes an operand that a later argument changes, so it prints the value gc reads.
package main

import (
	"fmt"
	"time"
)

type box struct{ f int }

var g int

func pair(a, b int) string { return fmt.Sprint(a, " ", b) }

func main() {
	x := 1
	setX := func() int { x = 10; return 2 }
	fmt.Println("local:", pair(x, setX()))

	g = 1
	setG := func() int { g = 10; return 2 }
	fmt.Println("global:", pair(g, setG()))

	s := box{1}
	setF := func() int { s.f = 10; return 2 }
	fmt.Println("field:", pair(s.f, setF()))

	v := 1
	p := &v
	setP := func() int { *p = 10; return 2 }
	fmt.Println("deref:", pair(*p, setP()))

	a := []int{1}
	setA := func() int { a[0] = 10; return 2 }
	fmt.Println("element:", pair(a[0], setA()))

	ptr := &box{1}
	setPtr := func() int { ptr.f = 10; return 2 }
	fmt.Println("pointer field:", pair(ptr.f, setPtr()))

	// A goroutine changes y and then sends; the receive is the later argument.
	y := 1
	ch := make(chan int)
	go func() { time.Sleep(30 * time.Millisecond); y = 10; ch <- 2 }()
	fmt.Println("receive:", pair(y, <-ch))
}
