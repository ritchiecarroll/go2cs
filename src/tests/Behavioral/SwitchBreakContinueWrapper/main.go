package main

import "fmt"

// A Go switch lowered to an if/else-if chain wraps any case body holding a
// switch-targeting `break` in `do { ... } while (false)` so the break has a C#
// target. Since seat #28 only a switch WITH a `fallthrough` takes that chain: one
// without is a C# pattern switch, where a break and a continue are emitted bare,
// so sections (a)-(g) read that form and (h)-(i) keep the wrapper's case. That wrapper is itself a C# iteration statement, so a Go `continue`
// meaning "continue the enclosing for loop" must NOT be emitted as a bare C#
// `continue` inside it -- it would continue the wrapper, exit on the false
// condition, and fall through past the switch. Each section below prints
// evidence discriminating which construct a transfer actually bound to.
func main() {
	words := []string{"skip", "stop", "keep", "skip", "keep"}

	// (a) The defect shape: a for loop whose switch case contains BOTH a
	// switch-break and a loop-continue. The break exits the switch (the
	// after-switch line runs); the continue continues the LOOP (the
	// after-switch line must NOT run).
	for i := 0; i < len(words); i++ {
		switch words[i] {
		case "skip":
			if i == 0 {
				fmt.Println("a: breaking at", i)
				break
			}
			fmt.Println("a: continuing at", i)
			continue
		case "stop":
			fmt.Println("a: stop at", i)
		}
		fmt.Println("a: after switch", i)
	}

	// (b) Control: a case with a continue but NO switch-break -- the case is
	// not wrapped, and its continue already binds the loop.
	for i := 0; i < 4; i++ {
		switch words[i] {
		case "skip":
			fmt.Println("b: continuing at", i)
			continue
		}
		fmt.Println("b: after switch", i)
	}

	// (c) Control: a case with a switch-break but no continue -- wrapped and
	// correct today; must stay byte-identical (no label, no goto).
	for i := 0; i < 3; i++ {
		switch words[i] {
		case "skip", "stop":
			if words[i] == "stop" {
				fmt.Println("c: breaking at", i)
				break
			}
			fmt.Println("c: took case at", i)
		}
		fmt.Println("c: after switch", i)
	}

	// (d) A nested REAL loop inside a wrapped case: the inner loop's own
	// continue keeps binding the INNER loop, while the case-level continue
	// targets the outer one.
	for i := 0; i < 3; i++ {
		switch words[i] {
		case "skip", "keep":
			if words[i] == "keep" {
				fmt.Println("d: breaking at", i)
				break
			}
			for j := 0; j < 3; j++ {
				if j == 1 {
					continue
				}
				fmt.Println("d: inner", i, j)
			}
			fmt.Println("d: after inner loop", i)
			continue
		}
		fmt.Println("d: after switch", i)
	}

	// (e) A LABELED continue inside a wrapped case -- already lowered via
	// `goto continue_<label>`; must keep working unchanged.
outer:
	for i := 0; i < 3; i++ {
		switch words[i] {
		case "skip", "stop":
			if words[i] == "stop" {
				fmt.Println("e: breaking at", i)
				break
			}
			fmt.Println("e: labeled continue at", i)
			continue outer
		}
		fmt.Println("e: after switch", i)
	}

	// (f) The defect shape inside a RANGE loop.
	for idx, w := range []string{"go", "brk", "go", "end"} {
		switch w {
		case "go", "brk":
			if w == "brk" {
				fmt.Println("f: breaking at", idx)
				break
			}
			fmt.Println("f: continuing at", idx)
			continue
		}
		fmt.Println("f: after switch", idx)
	}

	// (g) Per-iteration loop-variable capture plus a body write: the wrapped
	// continue must flow through the loop's carrier copy-back on its way to
	// the post clause, or the next iteration re-reads a stale index. The
	// closure pins the per-iteration variable.
	var got []int
	for i := 0; i < 6; i++ {
		f := func() int { return i }
		switch fmt.Sprint(i % 2) {
		case "1":
			if i == 3 {
				fmt.Println("g: breaking at", i)
				break
			}
			i++
			got = append(got, f())
			continue
		}
		fmt.Println("g: after switch", i)
	}
	fmt.Println("g: got", got)

	// (h) The defect shape in a switch that also has a `fallthrough`, which
	// keeps the if/else chain and its wrapper: the wrapped continue must still
	// reach the loop, and the break must still skip the rest of the case.
	for i := 0; i < len(words); i++ {
		switch words[i] {
		case "skip":
			if i == 0 {
				fmt.Println("h: breaking at", i)
				break
			}
			fmt.Println("h: continuing at", i)
			continue
		case "stop":
			fmt.Println("h: stop at", i)
			fallthrough
		case "keep":
			fmt.Println("h: stop-or-keep at", i)
		}
		fmt.Println("h: after switch", i)
	}

	// (i) (g)'s per-iteration capture in a fallthrough switch: the wrapped
	// continue flows through the carrier copy-back.
	var gotFt []int
	for i := 0; i < 6; i++ {
		f := func() int { return i }
		switch fmt.Sprint(i % 3) {
		case "1":
			if i == 4 {
				fmt.Println("i: breaking at", i)
				break
			}
			i++
			gotFt = append(gotFt, f())
			continue
		case "2":
			fallthrough
		case "0":
			fmt.Println("i: two-or-zero at", i)
		}
		fmt.Println("i: after switch", i)
	}
	fmt.Println("i: got", gotFt)
}
