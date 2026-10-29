package main

import (
	"fmt"
	"runtime"
	"sync"
)

// A deferred or spawned call's caller is the function that deferred or spawned it, as Go's
// tracebacks report: Go compiles `defer f(args)` and `go f(args)` into a deferwrap/gowrap closure
// marked as a wrapper and elides that frame, so runtime.Caller(1) inside a deferred call names the
// deferring function at its deferreturn line (its closing brace, or the executed return). The
// converter's thunk lambda is that closure and is elided the same way; a method-group defer has no
// thunk at all. Controls: a Go func literal, alone or sharing a line with a thunk, is a frame of its
// own and reports its own line. Tiered compilation is left ON here (LineMarkerDeferEndTC0 reads the
// same rule with it off).

type T struct{ n int }

func where(tag string) {
	_, _, line, _ := runtime.Caller(1)
	fmt.Printf("%s: %d\n", tag, line)
}

func (t T) whereV(tag string) { where2(tag) }

func (t *T) whereP(tag string) { where2(tag) }

func where2(tag string) {
	_, _, line, _ := runtime.Caller(2)
	fmt.Printf("%s: %d\n", tag, line)
}

func result(tag string) int {
	_, _, line, _ := runtime.Caller(1)
	fmt.Printf("%s: %d\n", tag, line)
	return 1
}

func constArg() {
	defer where("defer, constant argument")
}

func varArg() {
	s := "defer, variable argument"
	defer where(s)
}

func valueMethod() {
	t := T{1}
	defer t.whereV("defer, value-receiver method")
}

func pointerMethod() {
	t := &T{2}
	s := "defer, pointer-receiver method"
	defer t.whereP(s)
}

func droppedResult() {
	defer result("defer, dropped result")
}

func endsInReturn() int {
	defer where("defer, function ends in return")
	x := 3
	return x
}

func funcLit() {
	defer func() {
		where("defer, func literal (control)")
	}()
}

func oneLiner() {
	defer func() { where("defer, one-line func literal (control)") }()
}

func shared() {
	defer where("defer, thunk sharing a line with a literal"); defer func() { where("defer, literal sharing a line with a thunk (control)") }()
}

func spawned(wg *sync.WaitGroup, tag string) {
	defer wg.Done()
	pc, _, _, _ := runtime.Caller(1)
	fmt.Printf("%s: %s\n", tag, runtime.FuncForPC(pc).Name())
}

func main() {
	constArg()
	varArg()
	valueMethod()
	pointerMethod()
	droppedResult()
	endsInReturn()
	funcLit()
	oneLiner()
	shared()

	var wg sync.WaitGroup
	wg.Add(1)
	go spawned(&wg, "go, constant argument: caller")
	wg.Wait()

	wg.Add(1)
	go func() {
		defer wg.Done()
		where("go, func literal (control)")
	}()
	wg.Wait()
}
