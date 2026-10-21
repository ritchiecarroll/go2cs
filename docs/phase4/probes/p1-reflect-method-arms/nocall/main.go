// Probe for the generated reflect method arms (IGoMethodSet), with NO reflect method access:
// fmt reaches reflect, but nothing calls Type.Method, Value.Method or MethodByName.
// The prediction and the measurement are in ../README.md.
package main

import "fmt"

type Plain struct{ p int }

func (x Plain) Describe(s string) string { return fmt.Sprintf("%s[p=%d]", s, x.p) }
func (x Plain) Get() int                 { return x.p }
func (x *Plain) Set(v int)               { x.p = v }

type Box[T any] struct{ v T }

func (b Box[T]) Get() T   { return b.v }
func (b *Box[T]) Set(v T) { b.v = v }
func (b Box[T]) Len() int { return 1 }

type Celsius float64

func (c Celsius) String() string { return fmt.Sprintf("%.1fC", float64(c)) }

func main() {
	p := Plain{7}
	p.Set(9)
	b := Box[int]{1}
	b.Set(2)
	var s fmt.Stringer = Celsius(21.5)
	fmt.Println(p.Describe("x"), p.Get(), b.Get(), b.Len(), s, p, b)
}
