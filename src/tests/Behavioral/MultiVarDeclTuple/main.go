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

func ptrs() (*B, *B) { return &B{4}, nil }

func anys() (any, any) { return 1, nil }

// Package-level typed specs: one multi-value call, and a comma-ok type assertion. The call's results already
// have the declared type: Go 1.26.11's compiler fails (internal compiler error) on a package-level spec whose
// call results need an interface conversion, so that shape has no Go output to compare against.
var ga, gb any = anys()

var gi any = 5

var gy, gok any = gi.(int)

// Package-level comma-ok specs, typed and untyped: map index and channel receive. And a parenthesized call.
var pm = map[string]int{"k": 7}

var gmv, gmok any = pm["k"]

var guv, guok = pm["k"]

var pch = func() chan int {
	c := make(chan int, 1)
	c <- 5
	return c
}()

var grv, grok = <-pch

var qa, qb any = (anys())

// A typed comma-ok index on a type-parameter map.
func tpm[M ~map[string]int](m M) {
	var v, ok any = m["k"]
	var w, wok = m["z"]
	fmt.Println(v, ok, w, wok)
}

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

	// An address-taken name keeps its box: the write through the pointer is seen by the name.
	var h, k any = pair()
	hp := &h
	*hp = "h2"
	fmt.Println(h, k, *hp)

	// The empty interface over pointer results: a non-nil pointer, and a nil pointer that is a non-nil interface.
	var pm, pn any = ptrs()
	fmt.Println(pm, pm != nil, pn, pn != nil)

	// Package-level typed specs.
	fmt.Println(ga, gb, gy, gok)

	// Comma-ok forms with a typed declaration: map index, type assertion and channel receive.
	m := map[string]int{"k": 7}
	var mv, mok any = m["k"]
	var mz, mzok any = m["z"]
	fmt.Println(mv, mok, mz, mzok)

	var tx any = 3
	var ty, tok any = tx.(int)
	var tn, tnok any = tx.(string)
	fmt.Println(ty, tok, tn, tnok)

	ch := make(chan int, 1)
	ch <- 9
	var cv, cok any = <-ch
	close(ch)
	var cz, czok any = <-ch
	fmt.Println(cv, cok, cz, czok)

	// Package-level comma-ok and parenthesized specs.
	fmt.Println(gmv, gmok, guv, guok, grv, grok, qa, qb)

	tpm(map[string]int{"k": 7})

	// Untyped comma-ok forms, and an untyped spec with an address-taken name.
	var uv, uok = m["k"]
	ch2 := make(chan int, 1)
	ch2 <- 8
	var rv, rok = <-ch2
	var ba, bb = anys()
	bp := &ba
	*bp = "ba2"
	fmt.Println(uv, uok, rv, rok, ba, bb)

	// Parenthesized comma-ok forms.
	ch3 := make(chan int, 1)
	ch3 <- 6
	var qv, qok any = (m["k"])
	var qr, qrok any = (<-ch3)
	var qt, qtok any = (tx.(int))
	var qc, qd = (pair())
	fmt.Println(qv, qok, qr, qrok, qt, qtok, qc, qd)
}
