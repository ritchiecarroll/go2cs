// Regression test: a Go package declares a type whose name collides with a top-level C# `System`
// type (`ValueType` — like internal/profile's `ValueType`, go/ast's `Object`, bytes' `Buffer`).
//
// The type implements a local interface (emitting `[assembly: GoImplement<ValueType, message>]`) and
// is used through a pointer (emitting `[assembly: GoImplicitConv<ValueType, ж<ValueType>>(...)]`).
// Those assembly attributes sit at FILE scope, where both `using System;` and
// `using static go.<pkg>_package;` are active, so a BARE `ValueType` is ambiguous between
// System.ValueType and the package type — CS0104. The converter must root the local reference at the
// package class (`go.main_package.ValueType`) so it resolves unambiguously.
package main

import "fmt"

type ValueType struct{ unit string }

func (v ValueType) describe() string { return v.unit }

type message interface{ describe() string }

func show(m message) { fmt.Println(m.describe()) }

// Int32 is named like System.Int32 (internal/runtime/atomic's shape). Its method takes a field's address, so
// package_info.cs records a field-access attribute naming the type at file scope, beside `using System;`.
type Int32 struct{ v int32 }

func (x *Int32) Add(d int32) *int32 {
	x.v += d
	return &x.v
}

func main() {
	// Value used as the interface — emits GoImplement<ValueType, message>.
	var m message = ValueType{unit: "cpu in nanoseconds"}
	show(m)

	// Pointer used as the interface (value-receiver method set) — emits the pointer-indirect
	// GoImplicitConv<ValueType, ж<ValueType>>.
	v := ValueType{unit: "heap in bytes"}
	var mp message = &v
	show(mp)
	fmt.Println(mp.describe())

	show(ValueType{unit: "v1.4"})

	var counter Int32
	p := counter.Add(5)
	*p += 1
	fmt.Println(counter.v)
	fmt.Println(*counter.Add(36))
}
