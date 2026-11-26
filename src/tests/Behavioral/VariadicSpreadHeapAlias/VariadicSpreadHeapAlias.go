package main

import (
	"fmt"
	"reflect"
)

// Go's f(s...) passes the slice itself: the callee's pack IS the caller's slice, same backing array,
// same length and same capacity. A callee that writes an element, writes through &xs[i], or appends
// within capacity writes the CALLER's storage. These callees all use their pack in a way that keeps it
// on the heap (passing it on, slicing it, returning it, appending to it, capturing it), the shapes a
// stack view of the pack cannot serve. modernc.org/sqlite's (*SQLitePreUpdateData).New is the first
// shape: it passes its pack to row, which fills it through &dest[i].

func assign(dest, src any) {
	switch d := dest.(type) {
	case *any:
		*d = src
	}
}

func row(dest []any) {
	for i := range dest {
		assign(&dest[i], i*10)
	}
}

// fillAll passes its pack on; row fills it through &dest[i].
func fillAll(dest ...any) { row(dest) }

// setTail slices its pack and writes through the subslice.
func setTail(xs ...int) {
	ys := xs[1:]
	ys[0] = 99
}

// keep returns its pack: the result aliases the caller's slice.
func keep(xs ...int) []int { return xs }

// grow appends to its pack: within capacity Go writes the caller's spare capacity.
func grow(xs ...int) []int { return append(xs, 4) }

// later captures its pack in a closure that writes it.
func later(xs ...int) func() { return func() { xs[0] = 7 } }

// boxed stores its pack in an interface and writes through a type assertion.
func boxed(xs ...int) {
	var v any = xs
	v.([]int)[0] = -1
}

// sum only reads its pack (the stack view); it must not disturb the shapes above.
func sum(xs ...int) int {
	t := 0
	for _, x := range xs {
		t += x
	}
	return t
}

// stash keeps its pack on the heap and writes it.
var stashed []int

func stash(xs ...int) {
	stashed = xs
	xs[0] = 42
}

// relay passes its pack on: Go hands stash the caller's slice, so stash's write reaches it.
func relay(xs ...int) { stash(xs...) }

// relayRead passes its pack on and reads it afterwards: stash's write is visible here too.
func relayRead(xs ...int) int {
	stash(xs...)
	return xs[0]
}

// bump writes its own pack in place; all-constant arguments must still be writable.
func bump(xs ...int) int {
	xs[0]++
	return xs[0] + xs[2]
}

// ints is a named slice type: its spread is the slice too.
type ints []int

// viaConstraint spreads a slice reached through a type-parameter constraint.
func viaConstraint[S ~[]E, E any](s S, g func(...E)) { g(s...) }

// writeFirst writes its pack in place through a stack-view candidate's only other shapes.
func writeFirst(xs ...any) { xs[0] = "w" }

func main() {
	vals := make([]any, 3)
	fillAll(vals...)
	fmt.Println("fillAll spread:", vals)

	a, b := any(nil), any(nil)
	fillAll(a, b)
	fmt.Println("fillAll literal args stay local:", a, b)

	s := []int{1, 2, 3}
	setTail(s...)
	fmt.Println("setTail:", s)

	setTail(s[1:]...)
	fmt.Println("setTail of a subslice:", s)

	k := keep(s...)
	k[0] = 5
	fmt.Println("keep:", s, len(k), cap(k))

	c := make([]int, 3, 4)
	g := grow(c...)
	fmt.Println("grow within cap:", c[:4], g)

	d := make([]int, 3, 4)
	g2 := grow(d[0:2:2]...)
	fmt.Println("grow past a capped spread reallocates:", d[:4], g2)

	t := []int{1, 2, 3}
	f := later(t...)
	fmt.Println("sum before:", sum(t...))
	f()
	fmt.Println("later:", t)

	u := []int{1, 2, 3}
	boxed(u...)
	fmt.Println("boxed:", u)

	var n []int
	fmt.Println("keep of nil is nil:", keep(n...) == nil)
	fmt.Println("keep of no args is nil:", keep() == nil)
	e := []int{}
	fmt.Println("keep of empty is not nil:", keep(e...) != nil)

	w := []int{1, 2, 3}
	fmt.Println("sum of a spread:", sum(w...))
	fmt.Println("keep literal args:", keep(8, 9))

	r := []int{1, 2, 3}
	relay(r...)
	fmt.Println("relay spread:", r, stashed[0] == r[0])

	fmt.Println("relayRead literal args:", relayRead(1, 2))
	r2 := []int{1, 2, 3}
	fmt.Println("relayRead spread:", relayRead(r2...), r2)

	fmt.Println("bump of constants:", bump(1, 2, 3), bump(1, 2, 3))

	ns := ints{1, 2, 3}
	stash(ns...)
	fmt.Println("named slice spread:", ns)

	c2 := []int{1, 2, 3}
	viaConstraint(c2, stash)
	fmt.Println("spread through a constraint:", c2)

	r3 := ints{1, 2, 3}
	reflect.ValueOf(stash).CallSlice([]reflect.Value{reflect.ValueOf(r3)})
	fmt.Println("reflect CallSlice of a named slice:", r3)

	r4 := []int{1, 2, 3}
	reflect.ValueOf(stash).CallSlice([]reflect.Value{reflect.ValueOf(r4)})
	fmt.Println("reflect CallSlice:", r4)

	reflect.ValueOf(stash).Call([]reflect.Value{reflect.ValueOf(5), reflect.ValueOf(6)})
	fmt.Println("reflect Call builds its own slice:", stashed)

	for i := 0; i < 2; i++ {
		var a, b any
		writeFirst(a, b)
		writeFirst(nil, nil)
		fmt.Println("nil constants stay nil:", a, b)
	}
}
