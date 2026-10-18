// PartialInstantiationAnonymousTypeArg guards a PARTIAL explicit instantiation whose written type argument is an
// ANONYMOUS type, Go 1.26 reflect's `testTypeAssert[interface{ A() }](t, vv, nil, false)`. C# has no partial
// instantiation, so the converter completes the list from go/types' resolved instance. The written anonymous type
// must be lifted to a C# name as a complete list lifts it, and the type assertion through that lifted interface
// must answer as Go's does: false for a method set without A, true for one with M.
package main

import "fmt"

type withM struct{ s string }

func (w withM) M()             {}
func (w withM) String() string { return w.s }

func assertAs[T any, V any](v V) (T, bool) {
	t, ok := any(v).(T)
	return t, ok
}

func main() {
	_, okA := assertAs[interface{ A() }](withM{"x"})
	_, okAPtr := assertAs[interface{ A() }](&withM{"y"})
	m, okM := assertAs[interface{ M() }](withM{"z"})
	s, okS := assertAs[interface{ String() string }](withM{"w"})
	p, okP := assertAs[struct{ X, Y int }](struct{ X, Y int }{1, 2})

	fmt.Println(okA, okAPtr, okM, m != nil, okS, s.String(), okP, p.X+p.Y)
}
