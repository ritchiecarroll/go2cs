package main

import (
	"fmt"
	"runtime"
	"strings"
	"time"
)

// M:N item 7d. Two types declare a method of one name, and each one parks. Converted, both methods are members of
// one package class, so a twin's state machine has to be traced back to the method it was generated from by more
// than its name. A goroutine parks inside U.b, and once it resumes it reads its own stack: the frame of the parked
// caller has to name U's method, at U's line. T.b is the same shape, read the same way.

// ready is buffered: the goroutine says it is about to receive, and does not park to say so.
var ready = make(chan bool, 1)

// Prints every frame's function, and the line of each frame in this package.
func show(tag string) {
	pcs := make([]uintptr, 16)
	n := runtime.Callers(2, pcs)
	frames := runtime.CallersFrames(pcs[:n])

	var b strings.Builder

	for {
		f, more := frames.Next()

		if strings.HasPrefix(f.Function, "main.") {
			fmt.Fprintf(&b, " %s:%d", f.Function, f.Line)
		} else {
			fmt.Fprintf(&b, " %s", f.Function)
		}

		if !more {
			break
		}
	}

	fmt.Println(tag + b.String())
}

func wait(tag string, c chan int) int {
	ready <- true
	v := <-c
	show(tag)
	return v
}

type T struct{ x int }

type U struct{ y int }

func (t *T) b(c chan int) int {
	v := wait("T after", c)
	t.x = v
	return v
}

func (u *U) b(c chan int) int {
	u.y = 1
	v := wait("U after", c)
	u.y += v
	return u.y
}

func runT(c chan int, d chan int) {
	t := &T{}
	d <- t.b(c)
}

func runU(c chan int, d chan int) {
	u := &U{}
	d <- u.b(c)
}

func main() {
	c := make(chan int)
	d := make(chan int)

	go runT(c, d)

	// The goroutine is parked in wait before the value is sent.
	<-ready
	time.Sleep(20 * time.Millisecond)
	c <- 1
	fmt.Println("T", <-d)

	go runU(c, d)

	<-ready
	time.Sleep(20 * time.Millisecond)
	c <- 2
	fmt.Println("U", <-d)
}
