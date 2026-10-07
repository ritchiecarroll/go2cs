package main

import (
	"fmt"
	"runtime"
)

// A call inside a MULTI-LINE statement: Go answers the caller's line with the line of the call's own
// `(`, never the statement's first line. Every value printed is a line number of this file.

func line() int {
	_, _, l, _ := runtime.Caller(1)
	return l
}

type chain struct{ lines []int }

func (c *chain) Add(n int) *chain {
	_, _, l, _ := runtime.Caller(1)
	c.lines = append(c.lines, l)
	return c
}

func (c *chain) Done(label string) {
	_, _, l, _ := runtime.Caller(1)
	fmt.Println(label, c.lines, l)
}

type liner interface{ Line() int }

type impl struct{}

func (impl) Line() int {
	_, _, l, _ := runtime.Caller(1)
	return l
}

func three(a, b, c int) []int { return []int{a, b, c} }

var recorded []int

func rec(l int) bool {
	recorded = append(recorded, l)
	return true
}

var pkgSlice = []int{line(),
	line(), line(),
	line(),
}

var pkgSum = line() +
	line() +
	line()

var pkgStructs = []struct{ a, b int }{
	{line(), line()},
	{
		line(),
		line(),
	},
}

func ret() []int {
	return three(line(),
		line(),
		line())
}

func main() {
	fmt.Println("pkgSlice", pkgSlice)
	fmt.Println("pkgSum", pkgSum)
	fmt.Println("pkgStructs", pkgStructs)

	// S1: a method chain opened on one line and continued on later ones.
	(&chain{}).Add(1).
		Add(2).
		Add(3).
		Done("S1")

	// S2: a multi-line call, one argument call per line.
	fmt.Println("S2", three(line(),
		line(),
		line()))

	// S3: a multi-line sum.
	sum := line() +
		line() +
		line()
	fmt.Println("S3", sum)

	// S4: a func value called on a later line.
	f := line
	fmt.Println("S4", f(),
		f())

	// S5: a method called on a later line.
	var im impl
	fmt.Println("S5", im.Line(),
		im.Line())

	// S6: a multi-line if condition.
	if rec(line()) &&
		rec(line()) {
		fmt.Println("S6", recorded)
	}

	// S7: a composite literal in a function body.
	local := []int{line(),
		line(),
	}
	fmt.Println("S7", local)

	// S8: the return of a multi-line call.
	fmt.Println("S8", ret())
}
