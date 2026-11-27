// Program ForeignEmbedPromotedPointer guards an interface value made from a POINTER to a
// struct that embeds another package's struct by value, where the interface's methods are
// promoted from the embed: Go's `var c lib.Counter = &U{}`, which is how user code embeds
// a third-party type. The pointer adapter must reach the embed's methods across packages.
package main

import (
	"fmt"

	"ForeignEmbedPromotedPointer/lib"
)

type U struct {
	lib.T
	label string
}

func main() {
	u := &U{label: "u"}

	// Value-receiver method promoted through the embed, via a pointer.
	var named lib.Named = u
	fmt.Println(named.Name())

	// Pointer-receiver method promoted through the embed: writes reach u's own T.
	var counter lib.Counter = u
	fmt.Println(counter.Inc(), counter.Inc(), u.Inc())
	fmt.Println(counter.Name(), u.label)

	// A value U also satisfies Named, through the value adapter.
	var byValue lib.Named = U{label: "v"}
	fmt.Println(byValue.Name())
}
