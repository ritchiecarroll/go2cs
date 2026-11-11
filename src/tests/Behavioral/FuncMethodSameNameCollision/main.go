// FuncMethodSameNameCollision guards C40: a package-level function and a method of the same name in one
// package, where the method's receiver type is the function's first parameter type. Both land in the C#
// package class with one signature (CS0111), so the free function is Δ-renamed, and every reference
// follows: local calls, a function value, and calls from another package. Size is the control: its
// free function has a different signature, so nothing is renamed.
package main

import (
	"fmt"

	"FuncMethodSameNameCollision/shapes"
)

type Node struct {
	Depth int
}

func (n *Node) Deep() bool { return n.Depth > 2 }

func Deep(n *Node) bool { return n.Depth > 5 }

func main() {
	// A local pair.
	n := &Node{Depth: 3}
	fmt.Println("local", n.Deep(), Deep(n))

	// A function value of the renamed function.
	check := Deep
	fmt.Println("value", check(&Node{Depth: 9}))

	// The pairs declared in another package.
	t := &shapes.Type{Flags: 3}
	fmt.Println("foreign", t.IsTupleType(), shapes.IsTupleType(t), shapes.IsTupleType(&shapes.Type{}))
	fmt.Println("extra", t.Combine(4), shapes.Combine(t, 4))
	fmt.Println("control", t.Size(), shapes.Size(t, 10))

	// The foreign renamed function as a value.
	isTuple := shapes.IsTupleType
	fmt.Println("foreign value", isTuple(t))
}
