package main

// A caller frame's line is the line of the CALL it is suspended in (Go's pc-1), for every call shape.
// Each callee reports its caller's line the way logrus does (runtime.Callers + CallersFrames), and the
// expected line is read with runtime.Caller beside the call. The shapes that once read the NEXT
// statement's line in the converted program: a struct literal argument (S6, S7) and a variadic call
// with one literal argument (S1, S8) -- each puts a value-type `newobj` last before the call, where the
// CLR reports the suspended call's own offset (runtime's suspendedAtReportedCall).

import (
	"fmt"
	"runtime"
)

type E struct{ n int }

type P struct{ a, b int }

var got int

func viaCallers() int {
	pcs := make([]uintptr, 8)
	n := runtime.Callers(3, pcs)
	frame, _ := runtime.CallersFrames(pcs[:n]).Next()
	return frame.Line
}

func (e *E) Info(args ...any) { got = viaCallers() }
func (e *E) Take(p P)         { got = viaCallers() }
func (e *E) One(a any)        { got = viaCallers() }
func (e *E) Bare()            { got = viaCallers() }
func (e *E) Value(a any) int  { return viaCallers() }
func takeP(p P)               { got = viaCallers() }
func plain(a any)             { got = viaCallers() }
func plainV(args ...any)      { got = viaCallers() }

func line() int {
	_, _, l, _ := runtime.Caller(1)
	return l
}

func report(name string, have, want int) {
	fmt.Println(name, have == want)
}

func main() {
	e := &E{}
	var l int

	e.Info("looks delicious")
	l = line()
	report("S1 method, variadic, one literal:", got, l-1)

	plain("x")
	l = line()
	report("S2 func, one converted literal:", got, l-1)

	e.Bare()
	e.n++
	report("S3 method, no args, then a non-call:", got, line()-2)

	r := e.Value("y")
	report("S4 method, result used:", r, line()-1)

	e.Info("a", 1, 2.5)
	l = line()
	report("S5 method, variadic, three args:", got, l-1)

	e.Take(P{1, 2})
	l = line()
	report("S6 method, struct literal arg:", got, l-1)

	takeP(P{3, 4})
	l = line()
	report("S7 func, struct literal arg:", got, l-1)

	plainV("lit")
	l = line()
	report("S8 func, variadic, one literal:", got, l-1)

	e.One("lit")
	l = line()
	report("S9 method, one literal, not variadic:", got, l-1)
}
