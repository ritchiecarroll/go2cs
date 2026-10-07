// Conversion of a string constant to an interface TYPE LITERAL (objx: `interface{}("something")`).
//
// A string constant renders as a u8 literal, a ReadOnlySpan<byte> that cannot box. The `any("y")`
// spelling boxed it through @string first; the `interface{}(...)` spelling did not, and emitted
// `(any)("something"u8)` (CS0030). A constant of a NAMED string type keeps that type: %T shows it.
package main

import "fmt"

const named = "n1"

type sname string

const typed sname = "t1"

func main() {
	values := []interface{}{
		interface{}("lit"),
		(interface{})("paren"),
		interface{}(named),
		interface{}(typed),
		any("control"),
	}
	for _, v := range values {
		fmt.Printf("%v %T\n", v, v)
	}

	var s string = "v"
	fmt.Printf("%v %T\n", interface{}(s), interface{}(s))
}
