package main

import (
	"fmt"

	st "AnonStructAliasedConversion/structs"
)

func main() {
	// anonymous struct literal to a named struct declared in a package imported under an explicit alias
	b := st.AssignB(struct{ A int }{3})
	fmt.Println(b.A, b.Sum())

	// anonymous struct variable to the same foreign named struct
	var s struct{ A int }
	s.A = 4
	fmt.Println(st.AssignB(s).Sum())
}
