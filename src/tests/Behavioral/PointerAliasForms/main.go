package main

import "fmt"

// A pointer type may be written through an ALIAS (`type NodePtr = *Node`), and a value of that type is
// a pointer whatever its spelling: as a method's receiver (`func (p NodePtr) Mode()`, which Go accepts
// as the pointer method `(*Node).Mode`), as the type of a parameter, a local, a field, an element, a
// map value or a channel element, and as a type parameter's constraint. This guards that the converted
// C# treats each as it treats the same program written with `*Node`: a receiver takes the box form
// when its body observes it as a pointer (a nil guard, a return, a store, a capture, a field address,
// a reassignment) and is taken by reference otherwise, and every call reaches the method, on a
// pointer, on an addressable value, as a method value and as a method expression.
//
// Before the fix the converter asked "is this a pointer?" of the type as written, so an alias of a
// pointer type answered no: such a receiver was read as a value receiver, a call on such a variable
// took the variable's address, and the converted program did not compile. Every receiver case runs
// on a nil pointer, on a real pointer, and on a real pointer to a zero value.

type Node struct {
	kind int
	name string
	next *Node
}

type (
	NodePtr  = *Node   // an alias of the pointer type
	ChainPtr = NodePtr // an alias of that alias
)

type List []int
type ListPtr = *List

var registry []*Node

// 1. nil guard
func (p NodePtr) Mode(def int) (int, bool) {
	if p == nil {
		return def, false
	}
	return p.kind, true
}

// 1b. the same guard with the receiver written as a pointer: the control
func (node *Node) ModeNamed(def int) (int, bool) {
	if node == nil {
		return def, false
	}
	return node.kind, true
}

// 2. returns the receiver
func (p NodePtr) Self() *Node { return p }

// 3. passes the receiver on
func describe(n *Node) string {
	if n == nil {
		return "<nil>"
	}
	return n.name
}

func (p NodePtr) Describe() string { return describe(p) }

func (p NodePtr) Register() { registry = append(registry, p) }

// 4. address of a receiver field
func (p NodePtr) KindPtr() *int { return &p.kind }

// 5. stores the receiver
func (p NodePtr) LinkFrom(other *Node) { other.next = p }

// 6. closure over the receiver
func (p NodePtr) Namer() func() string { return func() string { return p.name } }

// 7. method value on the receiver
func (p NodePtr) ModeFunc() func(int) (int, bool) { return p.Mode }

// 8. field read after a side effect, receiver never used as a pointer
func (p NodePtr) Announce() string {
	fmt.Println("  side effect ran")
	return p.name
}

// 8b. field write, receiver never used as a pointer
func (p NodePtr) SetKind(kind int) { p.kind = kind }

// 9. delegated guard
func (p NodePtr) SafeName() string {
	if _, ok := p.Mode(0); !ok {
		return "none"
	}
	return p.name
}

// 10. an alias of an alias of the pointer type
func (p ChainPtr) IsNil() bool { return p == nil }

// 11. the receiver is reassigned
func (p NodePtr) Tail() *Node {
	for p != nil && p.next != nil {
		p = p.next
	}
	return p
}

// 12. a value receiver: the control
func (n Node) Kind() int { return n.kind }

// 13. an alias of a pointer to a named slice
func (l ListPtr) Push(v int) { *l = append(*l, v) }

func (l ListPtr) Len() int {
	if l == nil {
		return -1
	}
	return len(*l)
}

// 14. defer and recover around a receiver read
func (p NodePtr) Deferred() (s string) {
	defer func() {
		if r := recover(); r != nil {
			s = "recovered"
		}
	}()
	return p.name
}

// 15. a nil-tolerant Stringer
func (p NodePtr) String() string {
	if p == nil {
		return "Node(nil)"
	}
	return "Node(" + p.name + ")"
}

// The same alias as the type of a parameter, a local, an element, a map value and a channel element.

func viaParam(q NodePtr) int {
	if q == nil {
		return -1
	}
	q.kind++
	return q.kind + q.Kind()
}

func viaLocal(n *Node) string {
	var q NodePtr = n
	if q == nil {
		return "<nil>"
	}
	return q.Describe() + "/" + q.name
}

func walk(q NodePtr) int {
	count := 0
	for q != nil {
		count++
		q = q.next
	}
	return count
}

func kindOf(q NodePtr) *int { return &q.kind }

func copyOf(q NodePtr) Node { return *q }

func elements(n *Node) {
	nodes := []NodePtr{n, nil, {kind: 8, name: "lit"}}
	for i, q := range nodes {
		mode, ok := q.Mode(-1)
		fmt.Println(i, mode, ok, q.Describe())
	}

	byName := map[string]NodePtr{"n": n}
	fmt.Println(byName["n"] == n, byName["missing"] == nil, byName["missing"].IsNil())

	ch := make(chan NodePtr, 1)
	ch <- n
	fmt.Println(<-ch == n)

	var typedNil NodePtr
	var boxed any = typedNil
	fmt.Println(boxed == nil, boxed.(NodePtr) == nil)

	switch q := boxed.(type) {
	case NodePtr:
		fmt.Println("pointer case", q == nil)
	default:
		fmt.Println("default case")
	}
}

// The alias as a type parameter's constraint: the type set is the one pointer type.

func setKind[P NodePtr](p P, kind int) { (*p).kind = kind }

func isNilOf[P NodePtr](p P) bool { return p == nil }

func kindVia[P interface{ NodePtr }](p P) int { return (*p).kind }

func try(label string, fn func()) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println(label, "panic:", r)
		}
	}()
	fn()
}

type holder struct {
	attrs NodePtr
	list  ListPtr
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
	try("SetKind", func() { h.attrs.SetKind(h.attrs.kind + 10); fmt.Println(h.attrs.kind) })
	try("SafeName", func() { fmt.Println(h.attrs.SafeName()) })
	try("IsNil", func() { fmt.Println(h.attrs.IsNil()) })
	try("Tail", func() { fmt.Println(h.attrs.Tail() == h.attrs) })
	try("Kind", func() { fmt.Println(h.attrs.Kind()) })
	try("Len", func() { fmt.Println(h.list.Len()) })
	try("Push", func() { h.list.Push(4); fmt.Println(h.list.Len(), *h.list) })
	try("Deferred", func() { fmt.Println(h.attrs.Deferred()) })
	try("String", func() { fmt.Println(h.attrs.String()) })
	try("Stringer", func() { fmt.Println(h.attrs) })
	// A method expression of the pointer type.
	try("expression", func() { f := (NodePtr).Mode; fmt.Println(f(h.attrs, 12)) })
}

func main() {
	run("nil", &holder{})
	run("non-nil", &holder{attrs: &Node{kind: 3, name: "attrs"}, list: &List{1, 2}})
	// A real pointer to a struct that holds its zero value is not nil.
	run("zero value", &holder{attrs: &Node{}, list: &List{}})

	// On an addressable value Go takes the address for the call.
	var v Node
	v.SetKind(5)
	fmt.Println(v.Mode(0))
	fmt.Println(v.Announce(), v.Tail() == &v, v.Kind())

	linked := &Node{name: "head", next: &Node{name: "tail"}}
	var q NodePtr = linked
	fmt.Println(linked.Tail().name, q.Tail() == linked.next)

	// A method value on a variable of the alias type.
	mode := q.Mode
	fmt.Println(mode(11))

	fmt.Println(viaParam(linked), viaParam(nil))
	fmt.Println(viaLocal(linked), viaLocal(nil))
	fmt.Println(walk(linked), walk(nil))
	*kindOf(linked) = 9
	fmt.Println(copyOf(linked).kind)
	try("copyOf", func() { fmt.Println(copyOf(nil).kind) })
	elements(linked)

	setKind(linked, 21)
	fmt.Println(linked.kind, isNilOf(linked), isNilOf[NodePtr](nil), kindVia(linked))
}
