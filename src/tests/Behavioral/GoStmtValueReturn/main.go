package main

import "fmt"

// gch backs the nullary case below, which cannot take a channel parameter without becoming a
// param call.
var gch = make(chan int, 1)

// sum returns a value AND has a channel side effect. `go sum(out, 3, 4)` must wrap the call in a
// discarding Action lambda: goǃ only has void Action<...> overloads, so passing the bare method
// group (a Func<...>) is CS0407 "no overload matches the delegate". This mirrors x/net/nettest's
// `go chunkedCopy(c2, c2)` (chunkedCopy returns error), the defect this test guards.
func sum(out chan int, a, b int) int {
	r := a + b
	out <- r
	return r
}

// pair returns TWO values — guards the multi-result discard (the check is sig.Results().Len() > 0,
// not == 1).
func pair(out chan int, n int) (int, error) {
	out <- n * n
	return n * n, nil
}

// nib is a NULLARY value-returning callee — exercises the paramCount == 0 arm, which wraps the
// invocation as `goǃ(() => nib())` rather than trimming to a bare (Func) method group.
func nib() int {
	gch <- 5
	return 5
}

// emit returns nothing — its `go emit(out)` keeps the bare method-group form (`goǃ(emit, out)`),
// the control proving void method groups are UNAFFECTED by the fix.
func emit(out chan int) {
	out <- 8
}

func main() {
	out := make(chan int)

	go sum(out, 3, 4) // value-returning, 3 params -> goǃ(ᴛ1 => sum(ᴛ1, 3, 4), out)
	fmt.Println("sum:", <-out)

	go pair(out, 6) // value-returning, 2 params, multi-result
	fmt.Println("pair:", <-out)

	go nib() // value-returning, nullary -> goǃ(() => nib())
	fmt.Println("nib:", <-gch)

	go func() { out <- 1 }() // control: func-literal callee, unchanged by the fix
	fmt.Println("lit:", <-out)

	go emit(out) // control: void method group, unchanged bare method group
	fmt.Println("emit:", <-out)

	// A func-literal callee that RETURNS a value. The discarding wrap above deliberately
	// exempts literal callees, and this one cannot be wrapped anyway: its named result is
	// set by a defer, so the literal legitimately emits with an explicit `error` return type
	// and no Action<...> overload accepts it (CS8934). This is the shape that made goǃ grow
	// the Func<..., TResult> twins deferǃ always had -- net sendfile_test's
	// `go func(ln Listener) (retErr error) { ... }(ln)`.
	go func(o chan int) (retErr error) {
		defer func() { o <- 11 }()
		return nil
	}(out)
	fmt.Println("litval:", <-out)

	// The same with TWO parameters and a MULTI-result signature, so a second Func arity is
	// exercised rather than only the one net happens to need. (A NULLARY value-returning
	// literal is deliberately not covered: `Func<TResult>` can never win overload resolution
	// against the non-generic goǃ(Action) — a non-generic candidate beats a generic one — so
	// goǃ has no nullary twin, exactly as deferǃ has none. That shape is the converter's
	// discarding wrap to handle, and it does not currently reach func-literal callees; no
	// corpus site has it.)
	go func(o chan int, n int) error {
		o <- n * 2
		if n < 0 {
			return fmt.Errorf("negative %d", n)
		}
		return nil
	}(out, 6)
	fmt.Println("litmulti:", <-out)
}
