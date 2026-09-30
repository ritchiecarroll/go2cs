package main

import (
	"errors"
	"fmt"
)

// A package var whose initializer forwards a multi-value call -- `var A = must(build(B))`, the shape of
// google/uuid's `var NameSpaceDNS = Must(Parse("..."))` -- and depends on a var declared in a LATER file
// (values.go). Go initializes by dependency, so B is ready before A; C#'s static field initializers run
// in file order, so the converter must relocate A, and its hoisted tuple WITH it, into the ordered init.

type Sum [4]byte

func build(src [4]byte) (Sum, error) {
	if src[0] == 0 {
		return Sum{}, errors.New("B was read before it was initialized")
	}

	return Sum(src), nil
}

func must(s Sum, err error) Sum {
	if err != nil {
		panic(err)
	}

	return s
}

var A = must(build(B))

// ADDRESSED: its address is taken below, so it takes the ref-box relocation path.
var Addressed = must(build(B))

func main() {
	p := &Addressed
	fmt.Println("A:", A)
	fmt.Println("Addressed:", *p)
}
