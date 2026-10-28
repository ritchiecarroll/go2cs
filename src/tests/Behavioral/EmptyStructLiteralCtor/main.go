package main

import (
	"fmt"
	"strings"
	"time"
)

// Guards parity seat #21 (Z1): an empty struct literal `T{}` emits `new T()`. The parameterless
// constructor must build the same zero value as before: array fields allocated, nested structs
// zeroed, generic structs included. A named slice or map `T{}` is NOT nil and keeps its form.

type point struct{ x, y int }

type grid struct {
	cells [3]int
	name  string
}

type nested struct {
	at    point
	board grid
	tags  []string
	seen  map[string]bool
}

type pair[T any] struct{ a, b T }

type boxed[T any] struct {
	items [2]T
	last  T
}

type wrapped point

type embeds struct {
	point
	grid
	label string
}

type empty struct{}

type IntSlice []int

type StrMap map[string]int

func main() {
	p := point{}
	p.x = 3
	fmt.Println("point:", p, p.x+p.y)

	g := grid{}
	g.cells[1] = 7
	fmt.Println("grid:", g.cells, len(g.cells), g.name == "")

	n := nested{}
	n.board.cells[2] = 9
	fmt.Println("nested:", n.at, n.board.cells, n.tags == nil, n.seen == nil)

	q := pair[string]{}
	q.a = "a"
	fmt.Printf("pair: %q %q\n", q.a, q.b)

	bx := boxed[int]{}
	bx.items[1] = 5
	fmt.Println("boxed:", bx.items, bx.last)

	w := wrapped{}
	w.y = 4
	fmt.Println("wrapped:", w.x, w.y)

	e := embeds{}
	e.cells[0] = 1
	e.x = 2
	fmt.Println("embeds:", e.point, e.grid.cells, e.label == "")

	z := empty{}
	fmt.Println("empty:", z == empty{})

	pp := &point{}
	pp.y = 8
	fmt.Println("pointer:", *pp)

	pg := &grid{}
	pg.cells[0] = 6
	fmt.Println("pointer grid:", pg.cells)

	var b strings.Builder = strings.Builder{}
	b.WriteString("built")
	fmt.Println("builder:", b.String(), b.Len())

	t := time.Time{}
	fmt.Println("time zero:", t.IsZero(), t.Year())

	fmt.Println("equal zero values:", point{} == point{}, grid{} == grid{}, p == point{})

	s := IntSlice{}
	fmt.Println("IntSlice{} == nil:", s == nil, len(s))

	m := StrMap{}
	m["k"] = 1
	fmt.Println("StrMap{} == nil:", m == nil, len(m))
}
