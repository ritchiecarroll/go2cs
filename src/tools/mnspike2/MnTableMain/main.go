// MnTableMain is the func-value seat's program row (P5's S2 arm 3 shape): a table-driven main, its rows held in a
// slice of structs and each called through its func field, `r.run()`. Before the seat, N3's pair test reached only a
// call through a bare identifier, so main had no twin and the whole program ran on threads under GO2CS_MN=1; now the
// field callee is read once and tested for a pair, and each stored function is a pair, so main is twinned and its
// rows park. Each row parks: a channel receive a goroutine answers, and a wait on a goroutine that parks itself. A
// third row calls through an element; a fourth calls a func(int) int element for its value, `fs[nextIndex()](arg())`,
// and prints the evaluation trace: Go evaluates the function value (its index, once) before the argument. A fifth
// stores its function in a one-field struct literal, `job{run: rowReceive}`, which converts to `new job(rowReceive)`.
//
// Read: the generated entry (Generated/go2cs-gen/go2cs.TwinGenerator/go2cs.twins.entry.g.cs) calls
// builtin.RunMainꞏtwin, and the output is EQUAL to Go's, with GO2CS_MN unset and set.
package main

import "fmt"

func answer(ch chan int, v int) {
	ch <- v
}

func rowReceive() {
	ch := make(chan int)
	go answer(ch, 1)
	fmt.Println("receive:", <-ch)
}

func rowWait() {
	done := make(chan bool)
	relay := make(chan int)
	go func() {
		relay <- 2
	}()
	go func() {
		fmt.Println("wait:", <-relay)
		done <- true
	}()
	<-done
}

func rowElement() {
	steps := []func(){rowReceive, rowWait}
	for i := range steps {
		steps[i]()
	}
}

var trace []string

func nextIndex() int {
	trace = append(trace, "index")
	return 0
}

func arg() int {
	trace = append(trace, "arg")
	return 20
}

func double(v int) int {
	ch := make(chan int)
	go func() {
		ch <- v * 2
	}()
	return <-ch
}

func rowValue() {
	fs := []func(int) int{double}
	x := fs[nextIndex()](arg())
	fmt.Println("value:", x, trace)
}

type job struct {
	run func()
}

func rowJob() {
	j := job{run: rowReceive}
	j.run()
}

func main() {
	rows := []struct {
		name string
		run  func()
	}{
		{"receive", rowReceive},
		{"wait", rowWait},
		{"element", rowElement},
		{"value", rowValue},
		{"job", rowJob},
	}

	for _, r := range rows {
		fmt.Println("row", r.name)
		r.run()
	}
}
