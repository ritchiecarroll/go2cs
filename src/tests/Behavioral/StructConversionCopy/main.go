// StructConversionCopy converts struct and array values held in existing storage and then writes to the result.
//
// Go converts a COPY of its operand: after `c := chord(lc)` a write to `c.Notes` or to `c.In.V` does not reach
// `lc`. In C# a fixed array is a golib `array<T>` over a shared backing store, and the conversion builds its
// result from the operand's fields, so the operand is cloned first wherever it is existing storage, as every
// other value transfer clones it. Each line prints the source after the write, then the result.
package main

import (
	"fmt"

	"StructConversionCopy/lib"
	"StructConversionCopy/other"
)

type chord struct {
	Name  string
	Notes [2]int
	In    lib.Inner
}

type chord2 struct {
	Name  string
	Notes [2]int
	In    lib.Inner
}

// wrapped is written over chord: go2cs-gen's wrapper operators copy, and the site adds no second copy.
type wrapped chord

type notes [2]int

type holder struct {
	L lib.Chord
}

func fresh() lib.Chord {
	return lib.Chord{Name: "f", Notes: [2]int{1, 2}, In: lib.Inner{V: [2]int{3, 4}}}
}

func main() {
	// A local type from a foreign one, and back.
	lc := fresh()
	c := chord(lc)
	c.Notes[0], c.In.V[1] = 9, 8
	fmt.Println("local from foreign:", lc.Notes, lc.In.V, c.Notes, c.In.V)

	back := lib.Chord(c)
	back.Notes[1], back.In.V[0] = 7, 6
	fmt.Println("foreign from local:", c.Notes, c.In.V, back.Notes, back.In.V)

	// Two local types.
	c2 := chord2(c)
	c2.Notes[0], c2.In.V[0] = 5, 5
	fmt.Println("local from local:", c.Notes, c.In.V, c2.Notes, c2.In.V)

	// A defined type over a named struct, each way.
	w := wrapped(c)
	w.Notes[1], w.In.V[1] = 17, 17
	u := chord(w)
	u.Notes[0] = 18
	fmt.Println("wrapper:", c.Notes, c.In.V, w.Notes, w.In.V, u.Notes)

	// Two foreign types (go2cs-gen's function for the pair).
	oc := other.Chord(lc)
	oc.Notes[0], oc.In.V[1] = 4, 4
	fmt.Println("foreign from foreign:", lc.Notes, lc.In.V, oc.Notes, oc.In.V)

	// An unnamed target, written as a struct type literal.
	x := struct {
		Name  string
		Notes [2]int
		In    lib.Inner
	}(c)
	x.Notes[0], x.In.V[0] = 3, 3
	fmt.Println("unnamed from named:", c.Notes, c.In.V, x.Notes, x.In.V)

	// The operand read from a field, an element and a pointer.
	h := holder{L: fresh()}
	hc := chord(h.L)
	hc.Notes[0] = 11
	fmt.Println("field:", h.L.Notes, hc.Notes)

	s := []lib.Chord{fresh()}
	sc := chord(s[0])
	sc.In.V[0] = 12
	fmt.Println("element:", s[0].In.V, sc.In.V)

	p := &lc
	pc := chord(*p)
	pc.Notes[1] = 13
	fmt.Println("pointer:", p.Notes, pc.Notes)

	// A named array from an array.
	raw := [2]int{1, 2}
	n := notes(raw)
	n[0] = 14
	fmt.Println("array:", raw, n)

	// A conversion to the operand's own type copies too.
	same := chord(c)
	same.Notes[1] = 15
	fmt.Println("same type:", c.Notes, same.Notes)

	// The result passed on directly: the callee's write stays in its copy.
	touch(chord(lc))
	fmt.Println("argument:", lc.Notes)
}

func touch(c chord) {
	c.Notes[0] = 16
}
