// Calls inside an address-of operand and inside a map literal. gc runs every call of a statement in lexical order,
// these included, and reads the plain operands after them; a map literal's elements are evaluated in place, at the
// literal's own position among the calls. Each hazard row has a call inside `&...` or a map literal and a later call,
// or a read the later call can write, so the later call must not move ahead of the inner one. The controls have no
// later call and no earlier read a call can write, so nothing moves.
package main

import (
	"fmt"
	"strings"
)

type T struct{ x int }

type P struct{ q []int }

var (
	g   int
	log []string
	arr [4]int
	p   *P
)

func f(tag string) int {
	log = append(log, tag)
	g++
	return g
}

func reset() {
	g, log, arr, p = 0, nil, [4]int{}, &P{q: make([]int, 4)}
}

func show(name string, values ...any) {
	fmt.Println(name, values, strings.Join(log, ","))
	reset()
}

func h3(a int, t *T, b int) { fmt.Println(a, t.x, b) }

func he(a int, e *int, b int) {
	*e = 100
	fmt.Println(a, b)
}

func hm(a, b int) { fmt.Println(a, b) }

func main() {
	reset()

	// HAZARD: a call inside &T{...}, then a later call.
	h3(g, &T{x: f("lit")}, f("last"))
	show("literal")

	// HAZARD: a call locating an array element under &, then a later call.
	he(g, &arr[f("idx")%4], f("last"))
	show("array element", arr)

	// HAZARD: a call inside a map literal, then a later call.
	fmt.Println(g, map[string]int{"k": f("m1")}["k"], f("m2"))
	show("map literal")

	// HAZARD: a call locating a slice element through a pointer under &, then a later call.
	he(g, &p.q[f("pq")%4], f("last"))
	show("slice element", p.q)

	// HAZARD: an earlier read inside &T{...}, then a call that writes it.
	h3(0, &T{x: g}, f("a"))
	show("literal read")

	// HAZARD: a map literal holding a call, after an earlier read and with no later call.
	hm(g, map[string]int{"k": f("m")}["k"])
	show("map unit")

	// HAZARD: a map literal reading a variable in place, then a later call that moves ahead of an earlier read.
	fmt.Println(g, map[string]int{"k": g}["k"], f("r"))
	show("map read")

	// HAZARD: an element of a slice literal reads a variable a later call writes; gc reads it after the call.
	fmt.Println([]int{g}[0], f("s"))
	show("slice literal read")

	// CONTROL: a call inside &T{...} and no later call: nothing moves.
	t := &T{x: f("only")}
	show("control literal", t.x)

	// CONTROL: a map literal holding a call and no earlier read: nothing moves.
	n := map[string]int{"k": f("only")}["k"]
	show("control map", n)
}
