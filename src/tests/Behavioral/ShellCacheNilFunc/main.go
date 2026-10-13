// Guards a TYPED NIL FUNC asserted to an interface its type implements, beside non-nil funcs of the
// same type.
//
// `any(fn(nil))` holds dynamic type fn and a nil value, and fn's methods run on that nil receiver, so
// the assert must succeed. golib resolves it through a runtime shell built over a NULL delegate. The
// P3 experiment (docs/phase4/DESIGN-iface-shell-caching.md, section 6.4) keeps one shell per Go
// dynamic value, and a nil value has no identity to key that on, so it must be built fresh rather than
// lost as a miss. Two non-nil funcs of the same type must each answer with their own body.
//
// The values sit in `any` and the target is an interface literal, so every assert takes the shell
// tier, the same construction src/tests/Performance/PerfIfaceShell uses.
package main

import "fmt"

type fn func() int

func (f fn) Len() int {
	if f == nil {
		return -1
	}

	return f()
}

func main() {
	var none fn
	seven := fn(func() int { return 7 })
	eight := fn(func() int { return 8 })
	values := []any{none, seven, eight, none}

	for round := 0; round < 3; round++ {
		for i, a := range values {
			if v, ok := a.(interface{ Len() int }); ok {
				fmt.Println("round", round, "value", i, "ok", v.Len())
			} else {
				fmt.Println("round", round, "value", i, "MISS")
			}
		}
	}
}
