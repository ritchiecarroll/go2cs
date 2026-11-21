package main

import (
	"fmt"
	"runtime"
	"strings"
	"time"
)

// M:N item 7d. A call that parks on the SECOND line of a multi-line statement: Go reports the caller's frame at the
// call's own line, not at the line the statement starts on. Each frame is printed with its line.

var ready = make(chan bool, 1)

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

func leaf(c chan int) int {
	ready <- true
	v := <-c
	show("after")
	return v
}

func add(a, b int) int {
	return a + b
}

func mid(c chan int) int {
	v := add(1,
		leaf(c))
	return v
}

func root(c chan int, d chan int) {
	d <- mid(c)
}

func main() {
	c := make(chan int)
	d := make(chan int)

	go root(c, d)

	<-ready
	time.Sleep(20 * time.Millisecond)
	c <- 1
	fmt.Println("multi", <-d)
}
