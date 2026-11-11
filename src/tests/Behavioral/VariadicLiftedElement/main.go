package main

import "fmt"

// A variadic parameter whose element is an ANONYMOUS struct or interface: the element is lifted to a
// nested type of the package class, and the file-level `ꓸꓸꓸ` alias must qualify it (TS readiness C37,
// internal/printer's greatestEnd).

type span struct{ end int }

func (s span) End() int { return s.end }

// A named element: the control, already qualified.
type box struct{ n int }

func boxes(bs ...box) int {
	total := 0

	for _, b := range bs {
		total += b.n
	}

	return total
}

// The TS shape: a variadic anonymous interface.
func greatestEnd(end int, nodes ...interface{ End() int }) int {
	for _, n := range nodes {
		if n.End() > end {
			end = n.End()
		}
	}

	return end
}

// A variadic anonymous struct.
func sumPoints(points ...struct{ x, y int }) int {
	total := 0

	for _, p := range points {
		total += p.x*10 + p.y
	}

	return total
}

func main() {
	fmt.Println(greatestEnd(1, span{4}, span{2}), greatestEnd(7))

	nodes := []interface{ End() int }{span{5}, span{9}, span{3}}
	fmt.Println(greatestEnd(0, nodes...), leastEnd(100, nodes...))

	fmt.Println(sumPoints(struct{ x, y int }{1, 2}, struct{ x, y int }{3, 4}), sumPoints())
	fmt.Println(boxes(box{1}, box{2}), boxes())
}
