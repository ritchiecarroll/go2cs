package main

import "fmt"

// A method's receiver type may be written through an ALIAS (`type AttrNode = Node`, then
// `func (n *AttrNode) Mode()`), and such a method is the method of the type the alias denotes. This
// guards that the converted C# gives it the receiver form the same method takes when its receiver is
// written with the type's own name: the box form when the body observes the receiver as a pointer
// (a nil guard, a return, a store, a capture, a field address), by reference otherwise.
//
// Before the fix the alias-typed receiver was always taken by reference. Of the ways a body can then
// use its receiver as a pointer, the nil comparison alone compiled: it bound the struct's own
// comparison with nil, which reads the struct, so a nil receiver panicked ahead of its own guard and
// a real pointer to a zero-valued struct took the nil branch. Every case runs on a nil pointer, on a
// real pointer, and on a real pointer to a zero value.

type Node struct {
	kind int
	name string
	next *Node
}

type (
	AttrNode = Node     // an alias of the struct
	Chain    = AttrNode // an alias of an alias
)

type List []int
type ListAlias = List

var registry []*Node

// 1. nil guard
func (n *AttrNode) Mode(def int) (int, bool) {
	if n == nil {
		return def, false
	}
	return n.kind, true
}

// 1b. the same guard on the type's own name: the control
func (node *Node) ModeNamed(def int) (int, bool) {
	if node == nil {
		return def, false
	}
	return node.kind, true
}

// 2. returns the receiver
func (n *AttrNode) Self() *Node { return n }

// 3. passes the receiver on
func describe(n *Node) string {
	if n == nil {
		return "<nil>"
	}
	return n.name
}

func (n *AttrNode) Describe() string { return describe(n) }

func (n *AttrNode) Register() { registry = append(registry, n) }

// 4. address of a receiver field
func (n *AttrNode) KindPtr() *int { return &n.kind }

// 5. stores the receiver
func (n *AttrNode) LinkFrom(other *Node) { other.next = n }

// 6. closure over the receiver
func (n *AttrNode) Namer() func() string { return func() string { return n.name } }

// 7. method value on the receiver
func (n *AttrNode) ModeFunc() func(int) (int, bool) { return n.Mode }

// 8. field read after a side effect, receiver never used as a pointer
func (n *AttrNode) Announce() string {
	fmt.Println("  side effect ran")
	return n.name
}

// 9. delegated guard
func (n *AttrNode) SafeName() string {
	if _, ok := n.Mode(0); !ok {
		return "none"
	}
	return n.name
}

// 10. an alias of an alias
func (n *Chain) IsNil() bool { return n == nil }

// 12. a value receiver through the alias
func (n AttrNode) Kind() int { return n.kind }

// 13. an alias of a named slice
func (l *ListAlias) Push(v int) { *l = append(*l, v) }

func (l *ListAlias) Len() int {
	if l == nil {
		return -1
	}
	return len(*l)
}

// 14. defer and recover around a receiver read
func (n *AttrNode) Deferred() (s string) {
	defer func() {
		if r := recover(); r != nil {
			s = "recovered"
		}
	}()
	return n.name
}

// 15. a nil-tolerant Stringer
func (n *AttrNode) String() string {
	if n == nil {
		return "Node(nil)"
	}
	return "Node(" + n.name + ")"
}

func try(label string, fn func()) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println(label, "panic:", r)
		}
	}()
	fn()
}

type holder struct {
	attrs *AttrNode
	list  *ListAlias
}

func run(tag string, h *holder) {
	fmt.Println("==", tag)
	try("Mode", func() { fmt.Println(h.attrs.Mode(7)) })
	try("ModeNamed", func() { fmt.Println(h.attrs.ModeNamed(7)) })
	try("Self", func() { fmt.Println(h.attrs.Self() == h.attrs) })
	try("Describe", func() { fmt.Println(h.attrs.Describe()) })
	try("Register", func() { h.attrs.Register(); fmt.Println(len(registry), registry[len(registry)-1] == h.attrs) })
	try("KindPtr", func() { p := h.attrs.KindPtr(); *p = *p + 1; fmt.Println(*p, h.attrs.kind) })
	try("LinkFrom", func() { var o Node; h.attrs.LinkFrom(&o); fmt.Println(o.next == h.attrs) })
	try("Namer", func() { f := h.attrs.Namer(); fmt.Println("namer made"); fmt.Println(f()) })
	try("ModeFunc", func() { f := h.attrs.ModeFunc(); fmt.Println(f(9)) })
	try("Announce", func() { fmt.Println(h.attrs.Announce()) })
	try("SafeName", func() { fmt.Println(h.attrs.SafeName()) })
	try("IsNil", func() { fmt.Println(h.attrs.IsNil()) })
	try("Kind", func() { fmt.Println(h.attrs.Kind()) })
	try("Len", func() { fmt.Println(h.list.Len()) })
	try("Push", func() { h.list.Push(4); fmt.Println(h.list.Len(), *h.list) })
	try("Deferred", func() { fmt.Println(h.attrs.Deferred()) })
	try("String", func() { fmt.Println(h.attrs.String()) })
	try("Stringer", func() { fmt.Println(h.attrs) })
}

func main() {
	run("nil", &holder{})
	run("non-nil", &holder{attrs: &Node{kind: 3, name: "attrs"}, list: &List{1, 2}})
	// A real pointer to a struct that holds its zero value is not nil.
	run("zero value", &holder{attrs: &Node{}, list: &List{}})
}
