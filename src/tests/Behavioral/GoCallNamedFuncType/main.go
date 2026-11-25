package main

import (
	"context"
	"errors"
	"fmt"
)

// A `go` statement whose callee is a VALUE of a defined func type. Every defined func type is a
// C# delegate of its own, so the value converts to no Action/Func overload of goǃ by itself; the
// call site must convert it to the underlying signature's delegate, as the defer arm already does.
// context's own `go f(cause)` through a CancelCauseFunc is the shape that stopped compiling.

type none func()
type one func(n int)
type two func(n int, s string)
type valued func(n int) int

func main() {
	out := make(chan string)

	var f0 none = func() { out <- "none" }
	var f1 one = func(n int) { out <- fmt.Sprint("one ", n) }
	var f2 two = func(n int, s string) { out <- fmt.Sprint("two ", n, " ", s) }
	var fv valued = func(n int) int { out <- fmt.Sprint("valued ", n); return n }

	go f0()
	fmt.Println(<-out)

	go f1(1)
	fmt.Println(<-out)

	go f2(2, "b")
	fmt.Println(<-out)

	go fv(3)
	fmt.Println(<-out)

	// The arguments are evaluated at the go statement, not when the goroutine runs.
	n := 4
	go f1(n)
	n = 40
	fmt.Println(<-out, n)

	// A defined func type from another package: context.CancelCauseFunc.
	ctx, cancel := context.WithCancelCause(context.Background())
	done := make(chan struct{})
	go func() { <-ctx.Done(); close(done) }()
	go cancel(errors.New("stopped"))
	<-done
	fmt.Println("cause:", context.Cause(ctx))

	// Control: the same callees deferred, which already converted.
	func() {
		defer func() { fmt.Println(<-out) }()
		defer func() { go f2(5, "e") }()
	}()
}
