// Regression test: ranging (with index+value) over a pointer-to-array PARAMETER. A pointer param
// is implicitly dereferenced to a value local (`ref var buf = ref Ꮡbuf.Value`), so `buf` already
// denotes the array; the range conversion must not add a second `.Value` (`buf.Value` on the value
// array is CS1061, which also broke the foreach deconstruction inference, CS8130). This is the
// internal/chacha8rand `block_generic(buf *[32]uint64)` / `for i, x := range buf` pattern.
//
// An INDEX-ONLY range over a NIL pointer to an array ranges N times without dereferencing it (Go does not
// evaluate the operand when at most one iteration variable is present and the length is constant), for a
// parameter (ref-lowered, or aliased over its box) and for a pointer receiver alike.
package main

import "fmt"

func swap(buf *[4]uint64) {
	for i, x := range buf { // pointer param -> deref'd value; no extra `.Value`
		buf[i] = x>>32 | x<<32
	}
}

// lowered: every use of p is the range, so the parameter lowers to a ref.
func lowered(p *[3]int) (n int) {
	for i := range p {
		n += i + 1
	}
	return
}

// aliased: p also reaches a call, so it keeps its box and its entry alias.
func aliased(p *[6]int) (n int) {
	report(p)
	for i, _ := range p {
		n += i
	}
	return
}

func report(p *[6]int) { fmt.Println("aliased nil:", p == nil) }

type words [5]uint64

func (w *words) count() (n int) {
	for range w {
		n++
	}
	return
}

func main() {
	b := [4]uint64{0x100000002, 0x300000004, 5, 0}
	swap(&b)
	fmt.Println(b[0], b[1], b[2], b[3])

	var w *words
	fmt.Println("lowered:", lowered(nil), "aliased:", aliased(nil), "receiver:", w.count())
}
