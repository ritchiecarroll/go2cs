// Guards MUTATION VISIBILITY through a duck-typed shell that golib may hand out more than once.
//
// An assert with no compile-time adapter resolves through a runtime shell (golib's AdapterBinder).
// The P3 cache (docs/phase4/DESIGN-iface-shell-caching.md, sections 6.4 and 12) lets a re-assert of
// the same dynamic value return the shell an earlier assert built. That is only sound if the shell
// holds the dynamic value by REFERENCE: a write through the pointer after the shell exists must be
// seen by the old shell and by every later assert, and a second pointer of the same type must never be
// answered by the first pointer's shell. And it is only sound if a VALUE source is keyed on the
// interface value, not on the variable: converting a struct copies it, so a write to the variable
// after one conversion is seen by the next conversion and never by the first.
//
// The dynamic values sit in an `any` slice and the target is an interface literal, so no adapter can
// be recorded at conversion time and every assert below takes the shell tier, the same construction
// src/tests/Performance/PerfIfaceShell uses. A pointer source takes the delegate-bound generic shell
// and a value source takes the reflective object shell, so both cached tiers are exercised.
package main

import "fmt"

type box struct{ n int }

func (b *box) Len() int { return b.n }

type counter int

func (c counter) Len() int { return int(c) }

type pair struct{ x int }

func (p pair) Len() int { return p.x }

func main() {
	p := &box{n: 1}
	q := &box{n: 10}
	values := []any{p, q, counter(3), counter(4)}

	first, ok := values[0].(interface{ Len() int })
	fmt.Println("first assert:", ok, first.Len())

	// Write through the pointer AFTER the shell exists: the shell must read the box, not a copy.
	p.n = 2
	fmt.Println("old shell after write:", first.Len())

	again, ok := values[0].(interface{ Len() int })
	fmt.Println("re-assert after write:", ok, again.Len())

	// A second pointer of the same type gets its own answer, before and after a write through it.
	other, ok := values[1].(interface{ Len() int })
	fmt.Println("other box:", ok, other.Len())

	q.n = 11
	fmt.Println("other box after write:", other.Len(), "first box still:", again.Len())

	// Value sources: each value answers with its own contents.
	for i := 2; i < len(values); i++ {
		v, ok := values[i].(interface{ Len() int })
		fmt.Println("value", i, ok, v.Len())
	}

	// A value source written AFTER a conversion: any(t) copies t, so the earlier interface value keeps
	// the old contents and the next conversion sees the write, on first assert and on every re-assert.
	t := pair{x: 1}
	before := any(t)
	t.x = 2
	after := any(t)
	b, okBefore := before.(interface{ Len() int })
	a, okAfter := after.(interface{ Len() int })
	fmt.Println("value written after conversion:", okBefore, b.Len(), okAfter, a.Len())

	alternating := 0

	for i := 0; i < 10; i++ {
		src := before
		if i%2 == 1 {
			src = after
		}

		if v, ok := src.(interface{ Len() int }); ok {
			alternating = alternating*10 + v.Len()
		}
	}

	fmt.Println("alternating re-asserts:", alternating, "variable now:", t.Len())

	// Many re-asserts of the same values, interleaved, with a write part way through.
	sum := 0

	for i := 0; i < 1000; i++ {
		if i == 500 {
			p.n = 3
		}

		for _, a := range values {
			if v, ok := a.(interface{ Len() int }); ok {
				sum += v.Len()
			}
		}
	}

	fmt.Println("sum:", sum)
	fmt.Println("old shells at the end:", first.Len(), again.Len(), other.Len())
}
