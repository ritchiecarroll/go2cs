package main

import "fmt"

var sink = make(chan bool, 1)

type runner struct{ n int }

// gopkg.in/check.v1's forkCall shape: a parenthesized func literal as the go statement's callee.
func (r *runner) fork(c int, dispatch func(int)) int {
	done := make(chan bool)
	go (func() {
		defer func() { done <- true }()
		dispatch(c)
	})()
	<-done
	return c
}

// The deferred twin: the closure reads the parameter by reference, so it sees the reassignment.
func later(done chan bool) {
	defer (func() {
		fmt.Println("defer sees the reassigned channel:", done == nil)
	})()
	done = nil
}

func main() {
	r := &runner{}
	fmt.Println(r.fork(5, func(v int) { fmt.Println("dispatch", v) }))
	later(sink)
}
