// ArrayReassignForms: every assignment form that writes an EXISTING array location copies into that location's own
// storage, so a slice taken from it beforehand sees the new elements (docs/phase4/DESIGN-array-reassign-in-place.md,
// section 8: P5's C4 and C5, and the notes N1 and N2). Each line takes a slice of the destination before the assignment
// and prints that slice after it.
package main

import (
	"fmt"
	"reflect"
)

type N [4]int

type S struct {
	arr [4]int
	n   int
}

type E struct {
	v [2]int
	n int
}

func pair() ([4]int, [4]int) { return [4]int{1, 2, 3, 4}, [4]int{5, 6, 7, 8} }

func withErr() ([4]int, error) { return [4]int{9, 9, 9, 9}, nil }

func structs() (S, error) { return S{[4]int{4, 3, 2, 1}, 7}, nil }

func main() {
	// C5: a tuple of existing locations, `a, b = b, a`.
	a, b := [4]int{1, 1, 1, 1}, [4]int{2, 2, 2, 2}
	sa, sb := a[:], b[:]
	a, b = b, a
	fmt.Println("tuple swap:", sa, sb)

	// C5: a tuple from a call.
	c, d := [4]int{}, [4]int{}
	sc, sd := c[:], d[:]
	c, d = pair()
	fmt.Println("tuple call:", sc, sd)

	// C5: a comma-ok from a map.
	m := map[string][4]int{"k": {3, 3, 3, 3}}
	e := [4]int{}
	se := e[:]
	var ok bool
	e, ok = m["k"]
	fmt.Println("comma-ok:", se, ok)

	// C5: a `:=` that redeclares the array assigns it.
	f := [4]int{}
	sf := f[:]
	f, err := withErr()
	fmt.Println("redeclare:", sf, err)

	// C5: a range clause with `=`.
	g := [4]int{}
	sg := g[:]
	for _, g = range [][4]int{{1, 2, 3, 4}, {5, 6, 7, 8}} {
	}
	fmt.Println("range:", sg)

	// C5: a receive, and a select case.
	ch := make(chan [4]int, 2)
	ch <- [4]int{4, 4, 4, 4}
	ch <- [4]int{6, 6, 6, 6}
	h := [4]int{}
	sh := h[:]
	h = <-ch
	fmt.Println("receive:", sh)
	select {
	case h = <-ch:
	default:
	}
	fmt.Println("select:", sh)

	// C5: Go evaluates the left side's index operands first: `i, arrs[i] = 2, other` writes arrs[0].
	arrs := [][4]int{{}, {}, {}}
	s0 := arrs[0][:]
	i := 0
	other := [4]int{8, 8, 8, 8}
	i, arrs[i] = 2, other
	fmt.Println("tuple order:", i, s0, arrs)

	// C5: two locations through a pointer, swapped: each is taken first, as Go evaluates the left side's operands.
	hh := [2][4]int{{1, 1, 1, 1}, {2, 2, 2, 2}}
	hp := &hh
	sh0 := hh[0][:]
	(*hp)[0], (*hp)[1] = (*hp)[1], (*hp)[0]
	fmt.Println("tuple through a pointer:", sh0, hh)

	// C5: a struct holding an array, by a tuple from a call.
	var st S
	sst := st.arr[:]
	st, err = structs()
	fmt.Println("struct tuple:", sst, st.n, err)

	// C4: two windows over one backing, the destination above the source, then below it.
	w := [][2]int{{1, 1}, {2, 2}, {3, 3}}
	p := (*[2][2]int)(w[1:])
	q := (*[2][2]int)(w)
	*p = *q
	fmt.Println("overlap up:", w)
	w = [][2]int{{1, 1}, {2, 2}, {3, 3}}
	p = (*[2][2]int)(w[1:])
	q = (*[2][2]int)(w)
	*q = *p
	fmt.Println("overlap down:", w)

	// N1: a conversion between array types copies.
	var n N
	x := [4]int(n)
	x[0] = 9
	var y [4]int
	n2 := N(y)
	n2[1] = 7
	fmt.Println("conversion:", n[0], y[1], x[0], n2[1])

	// C3: a nested array's element pointer survives a whole assignment of the outer array.
	var na [2][2]int
	pna := &na[1]
	na = [2][2]int{{1, 2}, {3, 4}}
	fmt.Println("nested element:", *pna)

	// C3: a struct element's array field, and its by-value field.
	var ea [2]E
	pv := &ea[1].v
	ea = [2]E{{[2]int{1, 2}, 5}, {[2]int{3, 4}, 6}}
	fmt.Println("struct element:", *pv, ea[1].n)

	// The struct half: a single assignment of a struct holding an array, and of a named array.
	var hs S
	shs := hs.arr[:]
	hs = S{[4]int{1, 2, 3, 4}, 3}
	fmt.Println("struct:", shs, hs.n)
	var nn N
	snn := nn[:]
	nn = N{5, 6, 7, 8}
	fmt.Println("named:", snn)

	// N2: reflect's Set on an array copies into it.
	z := [4]int{}
	sz := z[:]
	reflect.ValueOf(&z).Elem().Set(reflect.ValueOf([4]int{1, 2, 3, 4}))
	fmt.Println("reflect set:", sz)
}
