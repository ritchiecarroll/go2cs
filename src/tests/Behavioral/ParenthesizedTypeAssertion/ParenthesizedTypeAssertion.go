// Parenthesized type-assertion target guard (objx's type_specific.go: `v.data.((Map))`).
//
// Go accepts parentheses around the asserted type. They are syntax only, but the converter rendered
// them into the C# type argument, `_<(Map)>()`, which is not valid C# (CS1525).
package main

import "fmt"

type Map map[string]interface{}

type point struct{ x, y int }

func main() {
	var data interface{} = Map{"a": 1}

	// Comma-ok form.
	if m, ok := data.((Map)); ok {
		fmt.Println("comma-ok:", len(m), m["a"])
	}

	// Single-value form.
	m := data.((Map))
	fmt.Println("single:", m["a"])

	// A failed assertion still reports false.
	var other interface{} = 5
	_, ok := other.((Map))
	fmt.Println("mismatch:", ok)

	// A parenthesized pointer type.
	var p interface{} = &point{1, 2}
	if pt, ok := p.((*point)); ok {
		fmt.Println("pointer:", pt.x+pt.y)
	}
}
