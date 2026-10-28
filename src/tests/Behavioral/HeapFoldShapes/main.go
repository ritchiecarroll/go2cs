package main

import "fmt"

// Each local below has its address taken, so its declaration boxes it on the heap. A single-LHS x := rhs
// folds the box and its value into one statement; the forms that keep the box apart from the value are
// here too, and every value is read back through its pointer so the output shows the box holds it.

type grid struct {
	cells [3]int
	name  string
}

func bump(p *int) { *p++ }

func mark(g *grid, i int) { g.cells[i] = 9 }

func pair() (int, error) { return 7, nil }

// Pointers stored here outlive the call that took them, so each local they point at must live on the heap.
var (
	keptInts  []*int
	keptGrids []*grid
)

func main() {
	// An untyped constant: the box is the Go type (int), not the constant's C# default.
	a := 10
	keptInts = append(keptInts, &a)
	bump(&a)
	fmt.Println("a:", a)

	// A string literal.
	s := "hi"
	ps := &s
	*ps += "!"
	fmt.Println("s:", s)

	// A struct holding an array, then a COPY of it boxed by := and mutated through its pointer after the
	// fold: the copy is independent, so the source keeps its cells.
	g := grid{name: "g"}
	keptGrids = append(keptGrids, &g)
	mark(&g, 0)
	h := g
	keptGrids = append(keptGrids, &h)
	mark(&h, 1)
	h.name = "h"
	fmt.Println("g:", g.cells, g.name, "h:", h.cells, h.name, "kept:", keptGrids[0].cells, keptGrids[1].cells, *keptInts[0])

	// A closure that takes the address of a folded local and writes through it.
	c := 1
	inc := func() { p := &c; *p += 10 }
	inc()
	inc()
	fmt.Println("c:", c)

	// An anonymous struct.
	anon := struct{ v int }{v: 3}
	pa := &anon
	pa.v *= 2
	fmt.Println("anon:", anon.v)

	// A multi-LHS define keeps the box apart from the value: Go evaluates every RHS first.
	x, y := 1, 2
	px, py := &x, &y
	*px, *py = *py, *px
	fmt.Println("x y:", x, y)

	// A multi-value define keeps the split too.
	n, err := pair()
	pn, pe := &n, &err
	*pn += 1
	fmt.Println("n err:", n, *pe)

	// A for-init variable boxed per iteration keeps its box ahead of the loop.
	var keep []*int
	for i := 0; i < 3; i++ {
		keep = append(keep, &i)
	}
	fmt.Println("loop:", *keep[0], *keep[1], *keep[2])

	// A declaration and a later assignment are two Go statements and stay two.
	var z int
	pz := &z
	z = 5
	fmt.Println("z:", *pz)
}
