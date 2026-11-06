package main

import (
	"fmt"
	"runtime"
	"strings"
)

// A panic a deferred Go CLOSURE raises runs the rest of the deferred sequence on top of runtime.gopanic, and Go's
// runtime.Callers from inside that sequence reads the closure that raised it, gopanic, and the deferring function
// (normalReturnPanic.func1 | runtime.gopanic | normalReturnPanic.func2 | normalReturnPanic). The converter records
// each Go func literal handed straight to defer in the file's GoPositionMap record, which is how the runtime tells
// such a closure from its own defer wrappers (a thunk Go elides) and splices Go's chain. Every shape here is one
// the record covers: a literal deferred plainly, parenthesized, or with arguments; panicking on a normal return,
// after a completed recovery, after recovering in the same call, and two links deep.

// frames names every frame from its caller down to and including owner, as runtime.CallersFrames reports them.
func frames(owner string) string {
	pcs := make([]uintptr, 64)
	n := runtime.Callers(2, pcs)
	iter := runtime.CallersFrames(pcs[:n])
	var names []string

	for {
		frame, more := iter.Next()
		names = append(names, frame.Function)

		if frame.Function == "main."+owner || !more {
			break
		}
	}

	return strings.Join(names, " | ")
}

// A deferred closure panics during its function's NORMAL return.
func normalReturnPanic() (got string) {
	defer func() { recover(); got = frames("normalReturnPanic") }()
	defer func() { panic("d") }()
	return
}

// A deferred closure panics AFTER an earlier panic's recovery completed.
func panicAfterRecovery() (got string) {
	defer func() { recover(); got = frames("panicAfterRecovery") }()
	defer func() {
		panic("p3")
	}()
	defer func() { recover() }()
	panic("p1")
}

// recover() then panic() in ONE deferred closure.
func recoverThenPanic() (got string) {
	defer func() { recover(); got = frames("recoverThenPanic") }()
	defer func() { recover(); panic("p2") }()
	panic("p1")
}

// A parenthesized literal, and one taking an argument (the argument form passes its literal to defer itself).
func parenAndArgument() (got string) {
	defer func() { recover(); got = frames("parenAndArgument") }()
	defer (func() { panic("paren") })()
	defer func(v string) { recover(); panic(v) }("argument")
	panic("p1")
}

// A two-link chain: each closure raises over the panic before it.
func twoLinkChain() (got string) {
	defer func() { recover() }()
	defer func() { got = frames("twoLinkChain") }()
	defer func() { panic("p3") }()
	defer func() { panic("p2") }()
	panic("p1")
}

func main() {
	fmt.Println("normal return:   ", normalReturnPanic())
	fmt.Println("after recovery:  ", panicAfterRecovery())
	fmt.Println("recover, panic:  ", recoverThenPanic())
	fmt.Println("paren, argument: ", parenAndArgument())
	fmt.Println("two links:       ", twoLinkChain())
}
