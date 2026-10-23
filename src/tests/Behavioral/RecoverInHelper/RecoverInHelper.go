package main

import "fmt"

// Go's rule (spec, "Handling panics"): recover returns nil when it is not called DIRECTLY by a deferred
// function. Whether a function is "the deferred function" depends on how it was called, not on where it
// was written: the same function recovers when deferred and reads nil when a deferred function calls it.

func main() {
	got, outer := helperGetsNil()
	fmt.Println("1 helper:", got, "| outer:", outer)

	fmt.Println("2 direct:", directWorks())

	fmt.Println("3 defer f(&err):", deferNamedWithPointer(), "| gob shape:", deferGobShape())

	g := deferMethodValue()
	fmt.Println("4 method value:", g.got, "| with args:", deferMethodValueWithArgs())

	fmt.Println("5 defer recover():", deferRecoverItself())

	inner, outer6 := namedFunctionCalledNormally()
	fmt.Println("6 called normally:", inner, "| outer:", outer6)

	fmt.Println("7 func value:", deferFuncValue())
}

func helperRecover() any { return recover() }

// 1. A deferred literal calls a helper that recovers: the helper reads nil, and the panic continues to the
// next deferred call, which recovers it.
func helperGetsNil() (got any, outer any) {
	defer func() { outer = recover() }()
	func() {
		defer func() { got = helperRecover() }()
		panic("one")
	}()
	return
}

// 2. recover called directly by the deferred literal.
func directWorks() (got any) {
	defer func() { got = recover() }()
	panic("two")
}

// 3. A named function deferred with an argument, the encoding/gob catchError shape.
func catchError(err *error) {
	if e := recover(); e != nil {
		*err = fmt.Errorf("caught: %v", e)
	}
}

func deferNamedWithPointer() (err error) {
	defer catchError(&err)
	panic("three")
}

// 3b. encoding/gob's catchError, verbatim but for the error type: its pointer parameter is only written
// through, so the converter takes it by ref and the deferred call goes through a thunk.
type gobError struct {
	err error
}

func catchGobError(err *error) {
	if e := recover(); e != nil {
		ge, ok := e.(gobError)
		if !ok {
			panic(e)
		}
		*err = ge.err
	}
}

func deferGobShape() (err error) {
	defer catchGobError(&err)
	panic(gobError{fmt.Errorf("three-b")})
}

// 4. A deferred method value, nullary and with an argument (fmt's catchPanic shape).
type guard struct {
	got any
}

func (g *guard) catch() { g.got = recover() }

func (g *guard) catchTagged(tag string) { g.got = fmt.Sprint(tag, ":", recover()) }

func deferMethodValue() (g *guard) {
	g = &guard{}
	defer g.catch()
	panic("four")
}

func deferMethodValueWithArgs() any {
	g := &guard{}
	func() {
		defer g.catchTagged("t")
		panic("four-b")
	}()
	return g.got
}

// 5. `defer recover()`: the deferred function is recover itself, not a function that calls it, so it
// returns nil and the panic continues.
func deferRecoverItself() (outer any) {
	defer func() { outer = recover() }()
	func() {
		defer recover()
		panic("five")
	}()
	return
}

// 6. The function arm 3 defers, now called normally from a deferred literal: it reads nil there.
func namedFunctionCalledNormally() (inner error, outer any) {
	defer func() { outer = recover() }()
	func() {
		defer func() { catchError(&inner) }()
		panic("six")
	}()
	return
}

// 7. The same function deferred through a func value: it is the deferred function, so it recovers.
func deferFuncValue() (err error) {
	h := catchError
	defer h(&err)
	panic("seven")
}
