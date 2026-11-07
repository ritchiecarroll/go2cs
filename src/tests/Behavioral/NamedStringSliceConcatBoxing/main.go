// A slice or a concatenation of a NAMED string type keeps that type in Go, so it boxes into an
// interface as the named type: gojq's funcOpNegate returns `v[1:]` and `"-" + v` for a json.Number,
// and its cli printed -1 as the string "-1" once the box lost the name.
package main

import (
	"encoding/json"
	"fmt"
)

type Token string

func (t Token) Kind() string { return "token:" + string(t) }

// negate is gojq's json.Number arm.
func negate(v any) any {
	switch v := v.(type) {
	case json.Number:
		if v[0] == '-' {
			return v[1:]
		}
		return "-" + v
	}
	return v
}

func describe(v any) string {
	switch v := v.(type) {
	case json.Number:
		return "number " + v.String()
	case Token:
		return v.Kind()
	case string:
		return "string " + v
	}
	return fmt.Sprintf("other %T", v)
}

func show(label string, v any) {
	fmt.Printf("%s: %T %v -> %s\n", label, v, v, describe(v))
}

func main() {
	for _, v := range []any{json.Number("1"), json.Number("-2.5")} {
		show("negate", negate(v))
	}

	t := Token("abcdef")
	show("t[1:]", t[1:])
	show("t[:2]", t[:2])
	show("t[1:3]", t[1:3])
	show("t[:]", t[:])
	show("t + \"!\"", t+"!")
	show("\"<\" + t", "<"+t)
	show("t + t", t+t)

	var boxed any = t[2:]
	show("var", boxed)

	items := []any{t[3:], "x" + t}
	items = append(items, t[:1]+"y")
	for i, item := range items {
		show(fmt.Sprint("item ", i), item)
	}

	m := map[string]any{"k": t[4:]}
	show("map", m["k"])

	// The result also keeps the named type's methods.
	fmt.Println(t[1:].Kind(), ("[" + t + "]").Kind())

	// A plain string slice or concatenation stays a string.
	s := "plain"
	show("s[1:]", s[1:])
	show("s + t-string", s+string(t))
}
