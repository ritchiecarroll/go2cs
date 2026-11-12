// %T renders the Go DYNAMIC TYPE of an interface value; generated adapter classes
// (pointer-sourced ж wrappers, value-sourced foreign ᴠ wrappers) must never leak their
// class names — the pointer adapter prints the *T it wraps, the value adapter the struct
// it copies (strings' TestPickAlgorithm asserts exactly this over its replacer algorithms).
package main

import (
	"fmt"

	"FormatTypeAdapters/typelib"
)

type greeter interface {
	greet() string
}

// loud implements greeter via a POINTER receiver — the interface conversion generates the
// ж pointer adapter.
type loud struct{ n int }

func (l *loud) greet() string { return "LOUD" }

// soft implements greeter via a VALUE receiver — the local value-boxing implementation.
type soft struct{ n int }

func (s soft) greet() string { return "soft" }

// stamper is satisfied by the FOREIGN typelib.Mark via a VALUE receiver — the conversion
// generates the ᴠ value adapter in this package.
type stamper interface {
	Stamp() string
}

func main() {
	var g greeter = &loud{n: 1} // pointer-sourced: %T is the wrapped *T
	fmt.Printf("%T\n", g)

	var h greeter = soft{n: 2} // local value implementer
	fmt.Printf("%T\n", h)

	var raw any = &loud{n: 3} // raw pointer, no adapter in its history
	fmt.Printf("%T\n", raw)

	fmt.Printf("%T\n", soft{n: 4}) // plain named struct value

	var m stamper = typelib.NewMark("x") // foreign value-sourced: %T is the wrapped struct
	fmt.Printf("%T\n", m)
	fmt.Printf("%v\n", m)

	// Keep every implementation live so the conversions above are real method-set casts.
	fmt.Println(g.greet(), h.greet(), m.Stamp())
}
