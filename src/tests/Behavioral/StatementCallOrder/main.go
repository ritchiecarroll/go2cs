// gc runs every call in a statement before the statement's plain reads: its order pass copies each
// call into a temporary ahead of the statement, in source order (cmd/compile/internal/walk/order.go),
// so a read of a LOCAL variable sees what a later call in the same statement wrote. C# evaluates
// left to right, so go2cs spills those calls to temporaries first (evalOrderSpill.go). One arm per
// statement form; each read precedes a call that changes it, and each line prints what gc computes.
// The neg- arms read storage the call does not write, and must keep their emission as written.
// The record is docs/phase4/SCOUT-evaluation-order.md (section 3, the forms).
package main

import "fmt"

type pair struct{ a, b int32 }

func mutate(p *pair) int32         { p.a = 100; return 2 }
func scaled(p pair, k int32) int32 { return (p.a*1000 + p.b) * k }
func readOnly(p *pair) int32       { return p.a + p.b }
func writeB(p *pair) int32         { p.b = 9; return 1 }

type counter struct{ n int }

func (c *counter) bump() int { c.n += 10; return 1 }
func (c *counter) peek() int { return c.n }

type holder struct{ p *pair }

func repoint(pp **pair) int32  { *pp = &pair{50, 0}; return 1 }
func (h *holder) touch() int32 { h.p.a = 5; return 1 }

// small is SSA-able and needs an address to become an interface (an int8 beside an int); key and
// kstr are map keys gc hashes on its slow path.
type small struct {
	f int8
	g int
}

func (s *small) poke() int { s.f, s.g = 9, 9; return 1 }

type key struct {
	a int
	s string
}

func sum3(a, b, c int) int { return a*100 + b*10 + c }
func fill(s []int) int     { s[0] = 9; return 1 }
func other() int           { return 1 }

func main() {
	// a call argument, beside a call taking its address
	p := pair{1, 2}
	fmt.Println("args-addr", scaled(p, mutate(&p)))

	// a binary operand, beside a closure that assigns it
	n := int32(1)
	bump := func() int32 { n = 50; return 3 }
	fmt.Println("binary-closure", n+bump())

	// call arguments, beside a pointer method on the same value
	t := counter{n: 1}
	fmt.Println("args-method", sum3(t.n, t.bump(), t.n))

	// composite literal elements
	k := 1
	setK := func() int { k = 7; return 1 }
	s := []int{k, setK(), k}
	fmt.Println("complit", s[0], s[1], s[2])

	// an assignment's target slice, reassigned by the right side
	ys := []int{0, 0}
	swap := func() int { ys = []int{5, 5}; return 4 }
	ys[1] = swap()
	fmt.Println("lhs-index", ys[0], ys[1])

	// compound assignment
	c := 1
	setC := func() int { c = 7; return 1 }
	c += setC()
	fmt.Println("compound", c)

	// a multi-assignment's right side
	m := 1
	setM := func() int { m = 7; return 1 }
	a, b := m, setM()
	fmt.Println("multi-assign", a, b)

	// a send's value
	ch := make(chan int, 1)
	q := 1
	setQ := func() int { q = 7; return 1 }
	ch <- q + setQ()
	fmt.Println("send", <-ch)

	// a conversion's operand
	r := 1
	setR := func() int { r = 7; return 1 }
	fmt.Println("conversion", int64(r)+int64(setR()))

	// a single-value return
	fmt.Println("return1", ret1())

	// string concatenation
	str := "a"
	cat := func() string { str = "Z"; return "b" }
	fmt.Println("concat", str+cat())

	// a pointer method through a pointer
	pt := &counter{n: 1}
	fmt.Println("ptr-recv", pt.n+pt.bump())

	// a slice argument whose element is read
	sl := []int{1, 2}
	fmt.Println("slice-arg", sl[0]+fill(sl))

	// a read through a pointer the call reassigns
	rp := &pair{1, 2}
	fmt.Println("repoint", rp.a+repoint(&rp))

	// the callee only reads, writes another field or only through the pointer read, or the read
	// comes after the call
	p2 := pair{1, 2}
	fmt.Println("neg-readonly", scaled(p2, readOnly(&p2)))
	p3 := pair{1, 2}
	fmt.Println("neg-otherfield", p3.a+writeB(&p3))
	t2 := counter{n: 1}
	fmt.Println("neg-peek", t2.n+t2.peek())
	x := 1
	fmt.Println("neg-after", other()+x)
	h := &holder{p: &pair{1, 2}}
	fmt.Println("neg-pointee", h.p == nil, h.touch())

	// gc copies an operand in place, before later calls, where it becomes an interface through a
	// helper that takes its address, or is a slow-path map key (order.go addrTemp): always for an
	// operand that is not addressable, and for a local of the function that is SSA-able and never
	// has its address taken, which is what default gc gives for a closure it inlines
	se := small{1, 1}
	fmt.Println("inplace-iface-expr", se.f+1, se.poke())
	sk := small{1, 1}
	mk := map[key]int{{1, "a"}: 10, {9, "a"}: 90}
	fmt.Println("inplace-mapkey-lit", mk[key{sk.g, "a"}]+sk.poke())
	sc := small{1, 1}
	setSc := func() int { sc.f = 9; return 1 }
	fmt.Println("inplace-iface-closure", sc, setSc())
	kc := key{1, "a"}
	setKc := func() int { kc.a = 9; return 1 }
	fmt.Println("inplace-mapkey-closure", mk[kc]+setKc())

	// the same contexts where gc reads after the call: the local has its address taken, or the
	// operand is a machine word the conversion takes by value
	sa := small{1, 1}
	fmt.Println("inplace-neg-addr", sa, sa.poke())
	sw := small{1, 1}
	fmt.Println("inplace-neg-word", sw.g+1, sw.poke())
}

func ret1() int {
	v := 1
	setV := func() int { v = 7; return 1 }
	return v + setV()
}
