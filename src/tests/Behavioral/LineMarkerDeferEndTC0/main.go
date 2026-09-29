package main

import (
	"fmt"
	"runtime"
)

// A deferred call run when its function FALLS OFF THE END reports the function's closing brace as its
// caller's line, as Go's deferreturn does: a plain function, a defer inside a loop, and a func literal.
// Controls: a function ENDING in `return` and one ending in a switch whose every clause returns report the
// executed return's line, which the shared epilogue already inherits; the brace must not reach them.
// Read with tiered compilation OFF (see the project file): under tiering a frame inside the `finally`
// epilogue reports its method's first line, a stated residual of the marker this test guards.

func where(tag string) {
	_, _, line, _ := runtime.Caller(1)
	fmt.Printf("%s: %d\n", tag, line)
}

func fallsOffEnd() {
	defer where("falls off the end")
	x := 1
	_ = x
}

func loopDefer() {
	for i := 0; i < 1; i++ {
		defer where("defer in a loop, falls off the end")
	}
}

func endsInReturn() int {
	defer where("ends in return (control)")
	x := 7
	return x
}

func endsInSwitch(k int) int {
	defer where("ends in an all-return switch (control)")
	switch k {
	case 1:
		return 10
	default:
		return 20
	}
}

func main() {
	fallsOffEnd()
	loopDefer()
	func() {
		defer where("func literal, falls off the end")
		y := 2
		_ = y
	}()
	endsInReturn()
	endsInSwitch(5)
}
