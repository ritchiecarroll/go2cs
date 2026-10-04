package main

import (
	"fmt"

	"AnonStructNamedConversion/structs"
)

type local struct{ A int }

type tagged struct {
	A int `json:"a"`
}

func main() {
	// anonymous struct literal to a named struct declared in another package
	b := structs.AssignB(struct{ A int }{3})
	fmt.Println(b.A, b.Sum())

	// anonymous struct variable to a named struct declared in this package
	var s struct{ A int }
	s.A = 4
	l := local(s)
	fmt.Println(l.A)

	// anonymous struct variable to the foreign named struct
	c := structs.AssignB(s)
	fmt.Println(c.Sum())

	// control: the field tags differ, so the written right-hand side is not identical
	t := tagged(s)
	fmt.Println(t.A)
}
