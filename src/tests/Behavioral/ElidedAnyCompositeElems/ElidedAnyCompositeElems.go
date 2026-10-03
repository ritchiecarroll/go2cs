// An elided inner slice or array literal of an empty-interface element type boxes each element at its Go
// type: a string through @string (CS0029 as a span), an untyped constant at its default type, a pointer as its box.

package main

import "fmt"

func main() {
	var p *int
	a := [][2]any{{1, 2}, {"a", "b"}}
	b := [2][]any{{1.5, 'x'}, {"c" + "d", true}}
	c := []*[2]any{{1, "p"}}
	d := [][]any{{p}}
	_, isInt := a[0][0].(int)
	_, isFloat := b[0][0].(float64)
	_, isRune := b[0][1].(rune)
	s, isString := a[1][0].(string)
	fmt.Println(a, b, *c[0], isInt, isFloat, isRune, s, isString, d[0][0] != nil)
}
