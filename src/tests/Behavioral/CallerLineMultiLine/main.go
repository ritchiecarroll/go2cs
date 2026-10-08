package main

import (
	"fmt"
	"os"
	"runtime"
)

// A call inside a MULTI-LINE statement: Go answers the caller's line with the line of the call's own
// `(`, never the statement's first line. Every value printed is a line number of this file.
//
// The converted program answers it exactly where the JIT's native-to-IL map is per call, which a
// Debug (unoptimized) build is. An optimized build maps only some of these shapes per call, so by
// default the program prints the two shapes an optimized build answers exactly (the behavioral suite
// runs Release), and with GO2CS_CALLER_LINE_ALL_SHAPES=1 it prints all eleven (GolibTests'
// CallerLineMultiLineDebugTests builds this project Debug and compares every shape with Go's).
var all = os.Getenv("GO2CS_CALLER_LINE_ALL_SHAPES") == "1"

func show(exactInRelease bool, args ...any) {
	if all || exactInRelease {
		fmt.Println(args...)
	}
}

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
	show(false, label, c.lines, l)
	show(true, label+"-terminal", l)
}

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
	show(false, "pkgSlice", pkgSlice)
	show(false, "pkgSum", pkgSum)
	show(false, "pkgStructs", pkgStructs)

	// S1: a method chain opened on one line and continued on later ones.
	(&chain{}).Add(1).
		Add(2).
		Add(3).
		Done("S1")

	// S2: a multi-line call, one argument call per line.
	show(false, "S2", three(line(),
		line(),
		line()))

	// S3: a multi-line sum.
	sum := line() +
		line() +
		line()
	show(false, "S3", sum)

	// S4: a func value called on a later line.
	f := line
	show(false, "S4", f(),
		f())

	// S5: a method called on a later line.
	var im impl
	show(false, "S5", im.Line(),
		im.Line())

	// S6: a multi-line if condition.
	if rec(line()) &&
		rec(line()) {
		show(true, "S6", recorded)
	}

	// S7: a composite literal in a function body.
	local := []int{line(),
		line(),
	}
	show(false, "S7", local)

	// S8: the return of a multi-line call.
	show(false, "S8", ret())
}
