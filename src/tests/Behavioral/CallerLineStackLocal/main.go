package main

// A caller frame's line is the line of the CALL it is suspended in, in a function whose locals the C#
// compiler keeps on the evaluation stack. `shapes` holds no string literal and uses `e` across every
// statement, so Release Roslyn stack-schedules `e` (`dup` where a local would be `ldloc`), the stack is
// never empty between statements, and the JIT records an IL mapping only where it is: the frame inside
// line() reports the previous statement, a whole statement before the call it is suspended in. Each
// shape records its line through a void call, so the call is the only line() call between two mappings.

import (
	"fmt"
	"os"
	"runtime"
	"time"
)

type E struct{ n int }

func (e *E) Bare()             {}
func (e *E) Value(a int) int   { return a }
func (e *E) Pair(a, b int) int { return a + b }

func line() int {
	_, _, l, _ := runtime.Caller(1)
	return l
}

var got [6]int

// record is a VOID call: the JIT records a mapping after it, so each shape's region holds one line() call.
func record(i, l int) { got[i] = l }

func shapes() {
	e := &E{}

	// S1: a void call, a statement with a call that returns a value, then the line.
	e.Bare()
	e.n++
	record(0, line())

	// S2: the line in the statement after a call whose value is kept.
	r := e.Value(2)
	record(1, line()+r-r)

	// S3: a value call discarded, then the line.
	e.Value(3)
	record(2, line())

	// S4: the line as an argument of a value call.
	r = e.Pair(line(), 0)
	record(3, r)

	// S5: the line after a field update through the pointer.
	e.n += 2
	record(4, line())

	// S6 (control, right before the fix too): the line right after a void call, nothing between.
	e.Bare()
	record(5, line())

	e.Bare()
}

// GO2CS_CALLER_LINE_WARM=1 runs shapes until the JIT has recompiled it optimized (tiered compilation's
// Tier1), so the same lines are read from an optimized frame; the default run reads its first compilation.
func main() {
	runs := 1
	if os.Getenv("GO2CS_CALLER_LINE_WARM") == "1" {
		runs = 400
	}
	for i := 0; i < runs; i++ {
		shapes()
		if i%100 == 99 {
			time.Sleep(200 * time.Millisecond)
		}
	}
	fmt.Println(got[0], got[1], got[2], got[3], got[4], got[5])
}
