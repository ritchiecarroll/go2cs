// Regression test for copying a pointer parameter into a local and walking a linked structure.
//
// Go `r := start` where start is a *T is a pointer copy: r is a *T. The converter deref's a
// pointer parameter to a value alias (`ref var start = ref Ꮡstart.Value`), so `r := start` used
// to emit `var r = start` — copying the pointed-to *value*, making r a T, not a *T. The rest
// of the converter already treats such a walked variable as a pointer (`r.next`, `r = r.next`),
// so the value-copy form failed to compile. The fix emits the pointer (box) form
// `var r = Ꮡstart`, so r is a ж<T>.
//
// A `var` declaration copies the pointer the same way: `var current *Node = node` (typescript-go's checker
// grammarchecks.go) emitted `ж<Node> current = node;`, the deref'd value alias where the box belongs (CS0029). The
// untyped `var r = start` and each name of `var x, y *node = a, b` took the same path. Each now emits the box, and
// the copy is the same pointer: `current == start` holds.
package main

import "fmt"

type node struct {
	val  int
	next *node
}

// `r := start` (pointer copy) then walk to the tail.
func last(start *node) *node {
	r := start
	for r.next != nil {
		r = r.next
	}
	return r
}

// pointer copy in a for-init clause, reassigned each iteration.
func length(start *node) int {
	n := 0
	for r := start; r != nil; r = r.next {
		n++
	}
	return n
}

// sum via an explicit pointer local seeded from the parameter.
func sum(start *node) int {
	total := 0
	p := start
	for p != nil {
		total += p.val
		p = p.next
	}
	return total
}

// `var current *node = start`, an explicitly typed pointer copy; start is also read through, so it is deref-aliased.
func depth(start *node) (int, bool) {
	label := start.val
	var current *node = start
	same := current == start
	d := 0
	for current != nil {
		d++
		current = current.next
	}
	return label + d, same
}

// the untyped `var r = a`, and a multi-name typed spec over two pointer parameters.
func ends(a, b *node) (int, int) {
	_ = a.val + b.val
	var r = a
	var x, y *node = a, b
	for r.next != nil {
		r = r.next
	}
	return r.val + x.val, y.val
}

func main() {
	third := &node{val: 3}
	second := &node{val: 2, next: third}
	first := &node{val: 1, next: second}

	fmt.Println(last(first).val)     // 3
	fmt.Println(length(first))       // 3
	fmt.Println(sum(first))          // 6
	fmt.Println(depth(first))        // 4 true
	fmt.Println(ends(first, second)) // 4 2
}
