// Conversions between two NAMED types over the same string or bool underlying. C# has no operator
// between two distinct [GoType] wrapper structs, so the conversion must route through the shared
// underlying (`((B)(@string)a)`), exactly as the numeric twin already does (`((M)(nint)n)`). Emitted
// as a plain `((B)a)` it is CS0030 (go-cmp's example_test `myString(in)`).
package main

import "fmt"

type A string
type B string

type otherString string
type pkgString otherString

type T bool
type U bool

func (a A) Len() int { return len(a) }

func main() {
	a := A("abc")
	b := B(a) // sibling named strings
	fmt.Println(b, len(b), a.Len())

	o := otherString("xyz")
	p := pkgString(o) // a defined type over a named string
	fmt.Println(p, otherString(p))

	type myString otherString // a FUNCTION-LOCAL defined type (go-cmp's example_test shape)
	m := myString(o)
	fmt.Println(m, otherString(m) == o)

	t := T(true)
	u := U(t) // sibling named bools
	fmt.Println(u, !bool(u))

	// Control: the numeric twin already routes through the underlying.
	type N int
	type M N
	fmt.Println(M(N(7)))
}
