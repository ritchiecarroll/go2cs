// Parenthesized type-assertion target guard (objx's type_specific.go: `v.data.((Map))`).
//
// Go accepts parentheses around the asserted type. They are syntax only, but the converter rendered
// them into the C# type argument, `_<(Map)>()`, which is not valid C# (CS1525).
//
// The same parentheses around a TYPE-SWITCH case type (typescript-go's internal/printer tryGetEnd,
// `case (*ast.Node):`) were rendered into the case label, `case (ж<node>) v:`, which C# reads as a cast of v
// (CS0103). Each case below is parenthesized: a single type with a binding, a multi-type clause, an interface case,
// nil, and a switch that binds nothing.
package main

import "fmt"

type Map map[string]interface{}

type point struct{ x, y int }

type named struct{ name string }

func (n named) String() string { return "named " + n.name }

func classify(x interface{}) string {
	switch v := x.(type) {
	case (*point):
		return fmt.Sprint("pointer ", v.x+v.y)
	case (Map), (int):
		return fmt.Sprint("map or int ", v)
	case (fmt.Stringer):
		return v.String()
	case (nil):
		return "nil"
	}
	return "other"
}

func kind(x interface{}) string {
	switch x.(type) {
	case (point):
		return "point value"
	case ([]int):
		return "int slice"
	}
	return "unmatched"
}

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

	for _, x := range []interface{}{&point{3, 4}, Map{"b": 2}, 7, named{"n"}, nil, 2.5} {
		fmt.Println("switch:", classify(x))
	}

	for _, x := range []interface{}{point{1, 1}, []int{1}, "s"} {
		fmt.Println("kind:", kind(x))
	}
}
