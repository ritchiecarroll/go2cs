package main

import (
	"context"
	"errors"
	"fmt"
)

type cancelFunc func(error)

func named(parent context.Context) (context.Context, cancelFunc) {
	ctx, cancel := context.WithCancelCause(parent)
	return ctx, cancelFunc(cancel)
}

// A multi-value call whose results are a NAMED func type, returned through an UNNAMED func result.
func unnamed(parent context.Context) (context.Context, func(error)) {
	return named(parent)
}

// The same through the standard library's own named type (context.CancelCauseFunc), as x/sync v0.11.0's errgroup does.
func stdlib(parent context.Context) (context.Context, func(error)) {
	return context.WithCancelCause(parent)
}

// And the reverse direction: an unnamed func result returned through a named one.
func back(parent context.Context) (context.Context, cancelFunc) {
	return stdlib(parent)
}

// A nil named func value forwarded through an unnamed func result stays nil, as Go's assignment does.
func none() (int, cancelFunc) { return 0, nil }

func forwardNone() (int, func(error)) {
	return none()
}

func main() {
	for _, f := range []func(context.Context) (context.Context, func(error)){unnamed, stdlib} {
		ctx, cancel := f(context.Background())
		cancel(errors.New("boom"))
		fmt.Println(ctx.Err(), context.Cause(ctx))
	}
	ctx, cancel := back(context.Background())
	cancel(errors.New("back"))
	fmt.Println(ctx.Err(), context.Cause(ctx))
	n, c := forwardNone()
	fmt.Println(n, c == nil)
}
