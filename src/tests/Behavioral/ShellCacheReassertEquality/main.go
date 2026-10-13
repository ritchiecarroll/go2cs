// Guards INTERFACE EQUALITY across re-asserts that golib may answer with a shell it built earlier.
//
// An assert with no compile-time adapter resolves through a runtime shell (golib's AdapterBinder).
// The P3 experiment (docs/phase4/DESIGN-iface-shell-caching.md, section 6.4) keeps ONE shell per Go
// dynamic value. Go compares two interface values by dynamic type and dynamic value, so whether the
// two shells are one instance or two must never show: the same value asserted twice compares equal,
// equal values held in two separate `any`s compare equal, and different values compare unequal even
// though they share a type, for a pointer source and a value source alike.
//
// The dynamic values sit in `any` and the target is an interface literal, so every assert takes the
// shell tier (a pointer source takes the delegate-bound generic shell, a value source the reflective
// object shell), the same construction src/tests/Performance/PerfIfaceShell uses.
package main

import "fmt"

type box struct{ n int }

func (b *box) Len() int { return b.n }

type counter int

func (c counter) Len() int { return int(c) }

func lener(a any) interface{ Len() int } {
	v, _ := a.(interface{ Len() int })
	return v
}

func main() {
	p := &box{n: 1}
	q := &box{n: 1}
	values := []any{p, q, counter(3), counter(3), counter(4)}

	fmt.Println("pointer, same any twice:", lener(values[0]) == lener(values[0]))
	fmt.Println("pointer, same box in a new any:", lener(values[0]) == lener(any(p)))
	fmt.Println("pointer, two boxes with equal contents:", lener(values[0]) == lener(values[1]))

	fmt.Println("value, same any twice:", lener(values[2]) == lener(values[2]))
	fmt.Println("value, equal values in two anys:", lener(values[2]) == lener(values[3]))
	fmt.Println("value, different values:", lener(values[2]) == lener(values[4]))

	// Equality must also hold against the dynamic value itself, held in a plain `any`.
	fmt.Println("pointer shell against any(p):", any(lener(values[0])) == any(p))
	fmt.Println("value shell against any(counter(3)):", any(lener(values[2])) == any(counter(3)))
	fmt.Println("value shell against any(counter(4)):", any(lener(values[2])) == any(counter(4)))

	// Repeated re-asserts keep answering the same way.
	same, different := 0, 0

	for i := 0; i < 100; i++ {
		for j, a := range values {
			for k, b := range values {
				if lener(a) == lener(b) {
					same++
				} else if j != k {
					different++
				}
			}
		}
	}

	fmt.Println("pairs equal:", same, "pairs unequal:", different)
}
