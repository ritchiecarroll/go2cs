// Guards MUTATION VISIBILITY through a duck-typed shell that golib may hand out more than once.
//
// An assert with no compile-time adapter resolves through a runtime shell (golib's AdapterBinder).
// The P3 experiment (docs/phase4/DESIGN-iface-shell-caching.md, section 6.4) keeps ONE shell per Go
// dynamic value, so a re-assert of the same value returns the shell an earlier assert built. That is
// only sound if the shell holds the dynamic value by REFERENCE: a write through the pointer after the
// shell exists must be seen by the old shell and by every later assert, and a second pointer of the
// same type must never be answered by the first pointer's shell.
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
