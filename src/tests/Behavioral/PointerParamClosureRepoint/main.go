// A pointer parameter is an ordinary local variable in Go, and a closure that captures it shares
// that one variable: when the closure assigns it (`n = k`), every later read in the enclosing body
// sees the new target.
//
// The converter deref-aliases a pointer parameter at entry (`ref var n = ref Ꮡn.DerefOrNull();`).
// A C# closure cannot capture that ref local, so inside the closure the parameter is its box `Ꮡn`
// and the assignment repoints the box (`Ꮡn = k;`); but the closure cannot re-alias the outer ref
// local, so without the fix the body went on reading the OLD target. `deepest` then looped forever
// on the root (the test caps it), and the other functions printed the root's values. A parameter a func literal
// assigns now keeps its box with no alias, and every value read in the body goes through it.
//
// The body's own repoint (`n = n.kids[0]`, also through a goto loop) re-aliases in place and is
// covered here as the control. A range assignment into the parameter or receiver (`for _, n =
// range n.kids`) is a repoint too: it wrote a `ж<node>` into the value alias and did not compile
// (CS0029); it now repoints the box. Each function's comment gives the value Go prints.
package main

import "fmt"

type node struct {
	val  int
	kids []*node
}

func (n *node) each(f func(*node) bool) bool {
	for _, k := range n.kids {
		if f(k) {
			return true
		}
	}
	return false
}

// deepest walks down by letting a callback repoint the parameter: 3. The step cap only keeps a
// stale read from looping forever (it printed -1).
func deepest(n *node) int {
	for i := 0; i < 10; i++ {
		if len(n.kids) == 0 {
			return n.val
		}
		n.each(func(k *node) bool { n = k; return true })
	}
	return -1
}

// afterCall reads a field once, after a closure repointed the parameter: 2.
func afterCall(n *node) int {
	step := func() { n = n.kids[0] }
	step()
	return n.val
}

// writeAfter writes through the parameter after the closure moved it, so the write lands in the
// child and the root is untouched: root 1, child 12.
func writeAfter(n *node) {
	root := n
	step := func() { n = n.kids[0] }
	step()
	n.val += 10
	fmt.Println("writeAfter:", root.val, n.val)
	n.val -= 10
}

// copyAfter copies the pointee after the closure moved it: 2.
func copyAfter(n *node) int {
	step := func() { n = n.kids[0] }
	step()
	c := *n
	return c.val
}

// goMoved repoints the parameter in a go statement's literal; the body reads after the
// goroutine signals: 2.
func goMoved(n *node) int {
	done := make(chan bool)
	go func() {
		n = n.kids[0]
		done <- true
	}()
	<-done
	return n.val
}

// deferMoved repoints the parameter in a deferred literal, and a deferred literal registered
// earlier (so it runs later) reads the result through it: 2.
func deferMoved(n *node) (v int) {
	defer func() { v = n.val }()
	defer func() { n = n.kids[0] }()
	return n.val
}

// rangeMoved repoints the parameter with a range assignment inside a closure: 2.
func rangeMoved(n *node) int {
	f := func() {
		for _, n = range n.kids {
		}
	}
	f()
	return n.val
}

// rangeBody repoints the parameter with a range assignment in the body: 2.
func rangeBody(n *node) int {
	for _, n = range n.kids {
	}
	return n.val
}

// lastKid repoints a pointer RECEIVER with a range assignment in the body: 2.
func (n *node) lastKid() int {
	for _, n = range n.kids {
	}
	return n.val
}

// mixed repoints the parameter both in the body and in a closure: 3.
func mixed(n *node) int {
	n = n.kids[0]
	step := func() { n = n.kids[0] }
	step()
	return n.val
}

// last repoints a pointer RECEIVER inside a callback: 2.
func (n *node) last() int {
	n.each(func(k *node) bool { n = k; return false })
	return n.val
}

// errAfter repoints an `*error` parameter at a nil error; reading the held value must not panic: true.
func errAfter(e *error) bool {
	var other error
	set := func() { e = &other }
	set()
	return *e == nil
}

// sliceAfter repoints a `*[]int` parameter and reads the new slice: 1 7.
func sliceAfter(s *[]int) (int, int) {
	other := []int{7}
	set := func() { s = &other }
	set()
	return len(*s), (*s)[0]
}

// count is nil-tolerant: it compares its receiver to nil before any dereference.
func (n *node) count() int {
	if n == nil {
		return 0
	}
	return 1
}

// tag runs before it dereferences its receiver, so a nil receiver still prints the line.
func (n *node) tag() int {
	fmt.Println("tag runs before its deref")
	return n.val
}

// nilCount calls a nil-tolerant method on a parameter a deferred literal repoints: 0 for nil, 1.
func nilCount(n *node) int {
	defer func() { n = nil }()
	return n.count()
}

// nilTag calls a method that dereferences only after a side effect. A nil parameter is legal to
// hold and to pass, so the method runs, prints, and then faults on its own dereference: the line,
// then -1 from the recover.
func nilTag(n *node) (v int) {
	defer func() {
		if recover() != nil {
			v = -1
		}
	}()
	defer func() { n = nil }()
	return n.tag()
}

// gotoWalk repoints the parameter in the body through a goto loop (the control): 3.
func gotoWalk(n *node) int {
loop:
	if len(n.kids) == 0 {
		return n.val
	}
	n = n.kids[0]
	goto loop
}

func main() {
	leaf := &node{val: 3}
	root := &node{val: 1, kids: []*node{{val: 2, kids: []*node{leaf}}}}

	fmt.Println("deepest:", deepest(root))
	fmt.Println("afterCall:", afterCall(root))
	writeAfter(root)
	fmt.Println("copyAfter:", copyAfter(root))
	fmt.Println("goMoved:", goMoved(root))
	fmt.Println("deferMoved:", deferMoved(root))
	fmt.Println("rangeMoved:", rangeMoved(root))
	fmt.Println("rangeBody:", rangeBody(root))
	fmt.Println("lastKid:", root.lastKid())
	fmt.Println("mixed:", mixed(root))
	fmt.Println("last:", root.last())
	failed := fmt.Errorf("failed")
	fmt.Println("errAfter:", errAfter(&failed))
	fmt.Println("sliceAfter:", fmt.Sprint(sliceAfter(&[]int{1, 2, 3})))
	fmt.Println("nilCount:", nilCount(nil), nilCount(leaf))
	fmt.Println("nilTag:", nilTag(nil))
	fmt.Println("gotoWalk:", gotoWalk(root))
	fmt.Println("root:", root.val, root.kids[0].val, leaf.val)
}
