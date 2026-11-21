package main

import (
	"fmt"

	"GenericHolderForeignConstraint/iterators"
)

// The generic types live in the iterators library: package main keeps its methods in its package class, while
// a library writes a method holder per type, and the holder's name must not depend on the constraint.
func main() {
	it := &iterators.Iterator[string]{}
	it.SetText("gopher")

	steps := 0
	for it.Next() {
		steps++
	}

	fmt.Println("string steps:", steps, "pos:", it.Pos())

	bytes := iterators.Iterator[[]byte]{}
	bytes.SetText([]byte("go"))
	bytes.Next()
	fmt.Println("bytes pos:", bytes.Pos())

	w := iterators.NewWindow(7, []byte("abc"))
	fmt.Println("window:", w.Key(), w.Len())

	named := iterators.NewWindow("k", "hello")
	fmt.Println("named window:", named.Key(), named.Len())
}
