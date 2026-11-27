package main

import (
	"context"
	"errors"
	"fmt"
)

type I interface{ Name() string }

type A struct{ n string }

func (a A) Name() string { return a.n }

type B struct{ n int }

func (b *B) Name() string { return fmt.Sprint("b", b.n) }

type cancelFunc func(error)

func pair() (int, string) { return 1, "x" }

func ab() (A, *B) { return A{"a"}, &B{2} }

func mixed() (I, *B) { return A{"m"}, &B{3} }

func funcs() (cancelFunc, cancelFunc) {
	_, cancel := context.WithCancelCause(context.Background())
	return cancelFunc(cancel), nil
}

// A typed multi-variable declaration fed by ONE multi-value call takes each result in turn, converted to the
// declared type, as an assignment does.
func main() {
	// The empty interface: each result is boxed on its own.
	var a, b any = pair()
	fmt.Println(a, b)

	// A non-empty interface: both results need a conversion (a value and a pointer).
	var x, y I = ab()
	fmt.Println(x.Name(), y.Name())

	// One result already has the declared type, the other needs a conversion.
	var p, q I = mixed()
	fmt.Println(p.Name(), q.Name())

	// Func-typed targets: a named func type to an unnamed one, and a nil value that must stay nil.
	var f, g func(error) = funcs()
	f(errors.New("f"))
	fmt.Println(g == nil)

	// A blank name, and a second declaration of the same shape in the same function.
	var _, r any = pair()
	var s, _ any = pair()
	fmt.Println(r, s)
}
