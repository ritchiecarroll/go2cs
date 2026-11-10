package main

import "fmt"

type node struct {
	val  int
	next *node
}

// sumWalk REASSIGNS its `*node` parameter `p` to step through a (circular) list, reading the
// pointed-to value each step (`total += p.val; p = p.next`). Every named pointer parameter is
// deref-aliased in C# to a value var (`ref var p = ref Ꮡp.Value`), which cannot itself be
// repointed; the reassignment must repoint the box and re-alias the value var
// (`Ꮡp = p.next; p = ref Ꮡp.Value;`). Earlier it emitted `p = p.next` — assigning a `ж<node>`
// into the `node` value alias — which fails to compile (CS0266/CS0029). This is the no-unsafe
// analogue of the runtime's `bits = addb(bits, n)` *byte memory walk (a value-using,
// never-nil pointer-parameter walk).
func sumWalk(p *node, steps int) int {
	total := 0
	for i := 0; i < steps; i++ {
		total += p.val
		p = p.next
	}
	return total
}

// firstAfter advances the pointer parameter `steps` times and returns it (the reassigned
// parameter itself becomes the result pointer).
func firstAfter(p *node, steps int) *node {
	for i := 0; i < steps; i++ {
		p = p.next
	}
	return p
}

// markSeen mirrors encoding/gob's buildEncEngine: a map keyed by a POINTER type
// (`map[*node]bool`) indexed by a deref-aliased pointer parameter. `p` is emitted as
// `ref var p = ref Ꮡp.Value`, so both the read `seen[p]` and the write `seen[p] = true` must
// use the box `Ꮡp` — a bare `p` passes the node VALUE where the ж<node> key is expected
// (CS1503). The field read `p.val` forces the deref-alias.
func markSeen(p *node, seen map[*node]bool) int {
	if seen[p] {
		return p.val
	}
	seen[p] = true
	return -p.val
}

// walkChain mirrors go/ast resolve.go's scope-chain walk: a deref-aliased pointer PARAMETER
// repointed in the for-loop POST (`for ; p != nil; p = p.next`). The repoint expands to a
// box-repoint (`Ꮡp = p.next`) PLUS a value re-alias (`p = ref Ꮡp.DerefOrNil()`); the two
// cannot share the single for-post slot, so the converter keeps the box-repoint in the post
// and injects the re-alias at the TOP of the loop body (otherwise CS1003/CS1525).
func walkChain(p *node) int {
	count := 0
	for ; p != nil; p = p.next {
		count += p.val
	}
	return count
}

// visitLocal mirrors reflect's Type.FieldByNameFunc: a pointer-keyed map (`map[*node]bool`)
// indexed by a LOCAL variable that already holds a pointer (`t := scan; visited[t]`). Unlike a
// deref-aliased pointer PARAMETER (markSeen above, which needs the box `Ꮡp`), a local holding a
// ж<node> IS already the key, so the index must stay a bare `visited[t]` — the box form `ᏑtΔ1`
// has no accessor (CS0103). Guards the gate restricting the pointer-key box rendering to params.
func visitLocal(nodes []*node) int {
	visited := map[*node]bool{}
	sum := 0
	for _, scan := range nodes {
		t := scan // t is a LOCAL *node — already a pointer, so `visited[t]` needs no box
		if visited[t] {
			continue
		}
		visited[t] = true
		sum += t.val
	}
	return sum
}

// collect mirrors go/ast's CommentMap.addComment: a deref-aliased pointer PARAMETER used as a
// COMPOSITE-LITERAL ELEMENT (`[]*node{p}`) must render the box (`Ꮡp`) — not the value alias
// (`p`) — into the `ж<node>[]` array (CS0029); the sibling `append(list, p)` already took the
// box through the call-argument pointer arm.
func collect(list []*node, p *node) []*node {
	if len(list) == 0 {
		list = []*node{p}
	} else {
		list = append(list, p)
	}
	return list
}

// The functions below repoint a pointer PARAMETER inside a for CLAUSE. Each repoint is a box-repoint plus a
// re-alias of the value var, and Go runs the clause as one step: the condition, a `continue` and any read after
// the loop all see the NEW pointer.

// firstAtLeast reads through the pointer in the CONDITION, straight after the post has repointed it.
func firstAtLeast(p *node, limit int) int {
	steps := 0
	for ; p != nil && p.val < limit; p = p.next {
		steps++
	}
	if p == nil {
		return -steps
	}
	return steps
}

// countOdd reaches the post through a `continue`.
func countOdd(p *node) int {
	count := 0
	for ; p != nil; p = p.next {
		if p.val%2 == 0 {
			continue
		}
		count++
	}
	return count
}

// lastVal has a condition that only dereferences, and reads the pointer again AFTER the loop.
func lastVal(p *node) int {
	depth := 0
	for ; p.next != nil; p = p.next {
		depth++
	}
	return depth*10 + p.val
}

// after repoints the parameter in the INIT (the tree-walker idiom `for n = n.Parent; n != nil; n = n.Parent`);
// the condition reads through it at once.
func after(p *node, limit int) int {
	steps := 0
	for p = p.next; p != nil && p.val < limit; p = p.next {
		steps++
	}
	if p == nil {
		return -steps - 1
	}
	return steps*10 + p.val
}

// initOnly repoints in the init and again in the body; the loop has no post.
func initOnly(p *node) int {
	sum := 0
	for p = p.next; p != nil; {
		sum += p.val
		p = p.next
	}
	return sum
}

// from assigns one pointer parameter from another in the init.
func from(a, b *node) int {
	sum := 0
	for a = b; a != nil; a = a.next {
		sum += a.val
	}
	return sum
}

// swapWalk repoints two parameters at once, in the init and in the post.
func swapWalk(a, b *node) int {
	sum := 0
	for a, b = b, a; a != nil && b != nil; a, b = a.next, b.next {
		sum += a.val*10 + b.val
	}
	return sum
}

// nested runs two loops with an init on the same parameter; a labeled continue reaches the outer post.
func nested(p, start *node) int {
	sum := 0
outer:
	for p = start; p != nil; p = p.next {
		m := p
		for p = p.next; p != nil; p = p.next {
			if p.val == 4 {
				p = m
				continue outer
			}
			sum += p.val
		}
		p = m
		sum += 100
	}
	return sum
}

func main() {
	a := &node{val: 1}
	b := &node{val: 2}
	c := &node{val: 3}
	a.next = b
	b.next = c
	c.next = a // circular: next is never nil

	fmt.Println(sumWalk(a, 6))      // two laps: 2*(1+2+3) = 12
	fmt.Println(firstAfter(a, 4).val) // a->b->c->a->b => 2

	seen := map[*node]bool{}
	fmt.Println(markSeen(a, seen), markSeen(a, seen), markSeen(b, seen)) // -1 1 -2

	// Linear (nil-terminated) chain for the for-post pointer-param walk.
	x := &node{val: 100}
	y := &node{val: 20}
	x.next = y // y.next stays nil, terminating the walk
	fmt.Println("chain:", walkChain(x)) // 120

	// Local-pointer-var indexing a pointer-keyed map (reflect FieldByNameFunc shape).
	dup := &node{val: 5}
	fmt.Println("visitLocal:", visitLocal([]*node{dup, dup, x})) // dup once + x: 5 + 100 = 105

	// Pointer param as a composite-literal element (go/ast addComment shape): first call takes
	// the `[]*node{p}` literal arm, second the append arm; the boxes must alias the originals.
	var list []*node
	list = collect(list, a)
	list = collect(list, b)
	a.val = 11 // prove the collected pointers alias the original nodes
	fmt.Println("collect:", list[0].val, list[1].val) // 11 2

	// A nil-terminated chain 1 -> 4 -> 7 for the for-clause repoints.
	k7 := &node{val: 7}
	k4 := &node{val: 4, next: k7}
	k1 := &node{val: 1, next: k4}
	fmt.Println("cond:", firstAtLeast(k1, 7), firstAtLeast(k1, 4), firstAtLeast(k1, 9), firstAtLeast(nil, 1)) // 2 1 -3 0
	fmt.Println("continue:", countOdd(k1), countOdd(k4), countOdd(nil))                                     // 2 1 0
	fmt.Println("after loop:", lastVal(k1), lastVal(k4), lastVal(k7))                                       // 27 17 7
	fmt.Println("init:", after(k1, 7), after(k1, 4), after(k1, 9), after(k7, 1))                            // 17 4 -3 -1
	fmt.Println("init only:", initOnly(k1), initOnly(k7))                                                   // 11 0
	fmt.Println("init from:", from(nil, k1), from(k1, nil))                                                 // 12 0
	fmt.Println("tuple:", swapWalk(k1, k4), swapWalk(k7, k1))                                               // 115 17
	fmt.Println("nested:", nested(nil, k1), nested(k1, k7))                                                 // 207 100
}
