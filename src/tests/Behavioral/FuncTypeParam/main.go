package main

import (
	"fmt"
	"sync/atomic"
)

// A func-typed parameter whose signature names a type from a SLASH-bearing package path
// (sync/atomic). The func-type rendering previously chopped the type string at the last '/'
// (`func(*sync/atomic.Int32) int` → `*sync/atomic.Int32) int`), producing un-compilable C#. The
// runtime package hits this (e.g. mprof.go's memProfileInternal takes a func(profilerecord.…)).
func apply(v *atomic.Int32, f func(*atomic.Int32) int) int {
	return f(v)
}

// A plain Action-style func param (no result) over a cross-package type.
func consume(v *atomic.Int32, sink func(*atomic.Int32)) {
	sink(v)
}

// runner holds a func-typed FIELD whose signature names a pointer to a type from a SLASH-bearing
// (multi-segment) import path — the exact shape of testing/quick's
// `Config.Values func([]reflect.Value, *rand.Rand)`. A struct field renders via the string display
// path (getDisplayTypeName), which stringifies the signature as `func(*sync/atomic.Int32) int` and
// then splits the slash-bearing import path on '/', mis-qualifying it (CS0426). The field must
// render structurally as a delegate so the pointer element keeps its `atomic` package-name alias.
type runner struct {
	gen func(*atomic.Int32) int
}

func (r runner) invoke(v *atomic.Int32) int {
	return r.gen(v)
}

// METHODLESS named func types are rendered inline as their base delegates and declare no C# type. A
// `:=` local taking a function VALUE of the same signature must not be declared with the collapsed
// name, which names a type that was never emitted (CS0246): Go 1.26's internal/runtime/gc/scan tests
// do exactly this (`expR := scan.ExpandReference` beside `type expandFunc func(...)`).
type loadFunc func(*int32) int32

type readFunc func(*atomic.Int32) int

func loadTwice(f loadFunc, p *int32) int32 {
	return f(p) + f(p)
}

func readPlusOne(v *atomic.Int32) int {
	return int(v.Load()) + 1
}

func main() {
	var v atomic.Int32
	v.Store(10)

	n := apply(&v, func(x *atomic.Int32) int {
		return int(x.Load()) + 5
	})
	fmt.Println(n)

	consume(&v, func(x *atomic.Int32) {
		x.Store(99)
	})
	fmt.Println(v.Load())

	// A struct with a func-typed field over a cross-package (slash-path) pointer type.
	double := runner{gen: func(x *atomic.Int32) int {
		return int(x.Load()) * 2
	}}
	fmt.Println(double.invoke(&v))

	seven := runner{gen: func(x *atomic.Int32) int {
		return 7
	}}
	fmt.Println(seven.invoke(&v))

	// A cross-package function value and a same-package one, each matching a collapsed func type.
	var raw int32 = 21
	ld := atomic.LoadInt32
	fmt.Println(ld(&raw), loadTwice(ld, &raw))

	rd := readPlusOne
	fmt.Println(rd(&v), apply(&v, rd))
}
