// Program ForeignEmbedPromotedPointer guards an interface value made from a POINTER to a
// struct that embeds another package's struct by value, where the interface's methods are
// promoted from the embed: Go's `var c lib.Counter = &U{}`, which is how user code embeds
// a third-party type. The pointer adapter must reach the embed's methods across packages.
package main

import (
	"fmt"

	"ForeignEmbedPromotedPointer/fam/left"
	"ForeignEmbedPromotedPointer/lib"
)

type U struct {
	lib.T
	label string
}

// W embeds lib.V, which embeds lib.Inner: Tag is promoted two hops deep.
type W struct {
	lib.V
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

	// A promoted method with arguments and two results, through the pointer adapter.
	var adder lib.Adder = u
	sum, err := adder.Add(3, 4)
	fmt.Println(sum, err)
	sum, err = adder.Add(50, 50)
	fmt.Println(sum, err, u.Inc())

	// Two hops: W -> lib.V -> lib.Inner.
	w := &W{}
	w.SetTag("w")
	var tagger lib.Tagger = w
	fmt.Println(tagger.Tag())

	// From a package one namespace down: a sibling's embed and an ancestor namespace's embed.
	fmt.Println(left.Right(), left.Lib())
}
