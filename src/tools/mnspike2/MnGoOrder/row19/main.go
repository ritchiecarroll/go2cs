// MnGoOrder/row19: the gc probes behind two of row 19's movability rules
// (docs/phase4/DESIGN-mn-row19-movability.md, section 3.3). Go only: it records gc's own order, which the
// twin rows compare against. Run it twice, `go run ./row19` and `go run -gcflags=-l ./row19`.
//
// M4: `x op= f()` with f writing x. gc reads x AFTER the call in every form here, with and without -l.
// M2: an operand converted to an interface at the call, with a later call writing it. gc reads it AFTER the
// call except for the lines marked "varies": a closure-captured local of concrete type, where gc reads it
// BEFORE the call when it inlines the closure and AFTER it with -l. The twin's later call parks and is never
// inlined, so M2 takes the after order, which is legal Go either way.
package main

import (
	"fmt"
	"io"
	"strings"
)

// M4 shapes.

var s string

func setS() string { s = "new"; return "+f" }

//go:noinline
func setSNoInline() string { s = "new"; return "+f" }

// M2 shapes.

type sb struct{ b strings.Builder }

func (w *sb) Write(p []byte) (int, error) { return w.b.Write(p) }

var w io.Writer

var pw *sb

type val struct{ x int }

func (v val) String() string { return fmt.Sprint(v.x) }

var pv val

func use(st fmt.Stringer, _ int) string { return st.String() }

func useAny(a any, _ int) string { return fmt.Sprint(a) }

func name(x io.Writer) string {
	if x == nil {
		return "nil"
	}
	if q, ok := x.(*sb); ok {
		return q.b.String()
	}
	return "?"
}

func main() {
	s = "old"
	s += setS()
	fmt.Println("M4 package var:", s) // new+f

	s = "old"
	s += setSNoInline()
	fmt.Println("M4 package var, noinline f:", s) // new+f

	ls := "old"
	g := func() string { ls = "new"; return "+f" }
	ls += g()
	fmt.Println("M4 captured string local:", ls) // new+f

	n := 1
	h := func() int { n = 100; return 10 }
	n += h()
	fmt.Println("M4 captured int local:", n) // 110

	first, second := &sb{}, &sb{}
	first.b.WriteString("first")
	second.b.WriteString("second")

	w = first
	setW := func() int { w = second; return 0 }
	fmt.Println("M2 interface package var:", useW(w, setW())) // second

	var lw io.Writer = first
	setLW := func() int { lw = second; return 0 }
	fmt.Println("M2 interface captured local:", useW(lw, setLW())) // second

	pw = first
	setPW := func() int { pw = second; return 0 }
	fmt.Println("M2 pointer package var to io.Writer:", useW(pw, setPW())) // second

	lp := first
	setLP := func() int { lp = second; return 0 }
	fmt.Println("M2 pointer captured local to io.Writer (varies):", useW(lp, setLP())) // first inlined, second with -l

	pv = val{1}
	setPV := func() int { pv = val{2}; return 0 }
	fmt.Println("M2 value package var to Stringer:", use(pv, setPV())) // 2

	lv := val{1}
	setLV := func() int { lv = val{2}; return 0 }
	fmt.Println("M2 value captured local to Stringer:", use(lv, setLV())) // 2

	li := 1
	setLI := func() int { li = 2; return 0 }
	fmt.Println("M2 int captured local to any:", useAny(li, setLI())) // 2
}

func useW(x io.Writer, _ int) string { return name(x) }
