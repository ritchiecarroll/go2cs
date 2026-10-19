// Package elem declares the element; it imports neither lib nor b1.
package elem

type Node struct{ K int }

func (n *Node) Less(o *Node) bool { return n.K < o.K }
