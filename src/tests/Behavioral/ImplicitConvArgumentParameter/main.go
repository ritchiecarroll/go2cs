package main

import "fmt"

type S struct{ A int }
type U struct{ B string }
type V struct{ A int }

func two(p *S, q *U) int       { return p.A + len(q.B) }
func pair(a S, b V) int        { return a.A*10 + b.A }
func tail(a S, rest ...*U) int { return a.A + len(rest) }
func spread(rest ...V) int     { return len(rest) }

func main() {
	s, u := &S{A: 1}, &U{B: "xy"}
	fmt.Println("two:", two(s, u))
	fmt.Println("pair, unnamed struct into the second parameter:", pair(S{1}, struct{ A int }{2}))
	fmt.Println("tail, variadic pointers:", tail(S{1}, u, u))
	fmt.Println("tail, expanded slice:", tail(S{1}, []*U{u}...))
	fmt.Println("spread, variadic values:", spread(V{1}, V{2}))
}
