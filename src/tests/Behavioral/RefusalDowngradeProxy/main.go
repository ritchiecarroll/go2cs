// A GO2CS2002 site converted under `-warn GO2CS2002`: the self-referential constraint proxy cannot forward
// Tree.Children (it returns []N, which shares the element's storage in Go), so go2cs-gen's forwarder PANICS when
// reached instead of returning a copy. Everything before the first Children call runs as Go does; the call panics
// with the refusal text. Go itself runs to the end.
package main

import "fmt"

type Tree[N any] interface {
	Name() string
	Children() []N
}

type Node struct {
	name string
	kids []*Node
}

func (n *Node) Name() string      { return n.name }
func (n *Node) Children() []*Node { return n.kids }

// Walk prints a node's name, then its children's: the Children call is the refused forwarder.
func Walk[N Tree[N]](n N) {
	fmt.Println("visit", n.Name())
	for _, kid := range n.Children() {
		Walk(kid)
	}
}

func main() {
	root := &Node{name: "root", kids: []*Node{{name: "a"}, {name: "b"}}}
	fmt.Println("before the walk")
	Walk(root)
	fmt.Println("after the walk")
}
