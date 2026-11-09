// NamedStringIndexCompare compares an INDEX or SLICE expression of a named (defined) string type with an untyped
// string constant: map, slice and array elements, sub-slices of a named string, a concatenation of either, either
// operand order, and every comparison operator. The controls are shapes that already compiled: a local, a field
// through a value and through a pointer, a call result, a dereferenced map element, a conversion, a named
// concatenation, a switch, and a PLAIN string element compared with a literal. TS readiness C6+C12, addendum B.
package main

import "fmt"

type Kind string

type S struct{ K Kind }

func get() Kind { return "x" }

func main() {
	vals := map[string]Kind{"a": "x", "b": "y"}
	s := []Kind{"x", "y"}
	a := [2]Kind{"x", "y"}
	var k Kind = "xy"

	// Index expressions of a named string against a literal (each was CS0019).
	fmt.Println(vals["a"] == "x", vals["a"] != "y", "x" == vals["a"], "y" != vals["b"])
	fmt.Println(s[0] == "x", s[1] != "x", a[0] == "x", a[1] == "y", ("x") == (s[0]))
	fmt.Println(vals["a"] < "y", vals["b"] <= "x", vals["b"] > "x", vals["a"] >= "x", "a" < s[0])
	fmt.Println(vals["missing"] == "", s[0] == "x" && a[1] == "y")

	// A concatenation of one, against a literal (also CS0019).
	fmt.Println(s[0]+"z" == "xz", vals["a"]+"z" != "xz", "xz" == a[0]+"z", k[1:]+"!" == "y!")

	// Slice expressions of a named string against a literal (each was CS0019).
	fmt.Println(k[1:] == "y", k[:1] != "x", k[0:2] == "xy", "y" == k[1:], k[1:] > "a")

	// Controls: shapes that already emitted the u8 literal.
	st := S{"x"}
	p := &st
	ps := map[string]*Kind{"k": &k}
	fmt.Println(k == "xy", st.K == "x", p.K == "x", get() == "x", *ps["k"] == "xy")
	fmt.Println(vals["a"] == Kind("x"), string(vals["a"]) == "x", vals["a"]+"z")
	switch vals["a"] {
	case "x":
		fmt.Println("switch x")
	default:
		fmt.Println("switch other")
	}

	// Control: a PLAIN string element against a literal keeps its emission.
	plain := []string{"x", "y"}
	pm := map[string]string{"a": "x"}
	fmt.Println(plain[0] == "x", pm["a"] != "x", plain[1][0:1] == "y")
}
