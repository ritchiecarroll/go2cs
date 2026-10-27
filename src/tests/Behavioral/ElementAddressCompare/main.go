package main

// ElementAddressCompare pins Go's `==`/`!=` between two element address-ofs (`&x[i] == &y[j]`), which
// go2cs emits as `Ꮡelem(x, i) == Ꮡelem(y, j)`: a compare of the backing storage and the absolute
// index that takes no pointer box. Element identity must hold across re-slices, an array and a
// slice of it, a pointer to an array, named slice and array types, and receiver fields; the index
// checks must still panic at the take; and the compare must allocate nothing (the AllocsPerRun
// lines). The last block pins three compares that are NOT two element address-ofs once the
// parentheses are removed, which keep the boxed form.

import (
	"fmt"
	"testing"
)

type words []uint64

type quad [4]int

type holder struct {
	f [4]int
	q quad
}

func (h *holder) sameField() (bool, bool) {
	return &h.f[1] == &h.f[1], &h.f[0] == &h.f[1]
}

func (h *holder) sameNamedField() (bool, bool) {
	return &h.q[2] == &h.q[2], &h.q[2] != &h.q[3]
}

func sameParam(x quad) (bool, bool) {
	return &x[1] == &x[1], &x[0] == &x[1]
}

// math/big's alias(): the last element of each full-capacity view.
func alias(x, y words) bool {
	return cap(x) > 0 && cap(y) > 0 && &x[0:cap(x)][cap(x)-1] == &y[0:cap(y)][cap(y)-1]
}

func overlaps(z, x []uint64) bool {
	return &z[0] != &x[0]
}

// A Go name `elem` boxes as the C# name Ꮡelem, so a compare beside it keeps the boxed form.
func besideElem(s []int) bool {
	elem := 1
	p := &elem
	*p = 2
	return &s[0] == &s[0] && &s[0] != &s[1] && elem == 2
}

type empty struct{}

var sink bool

func recovered(label string, f func() bool) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println(label, "panic:", r)
		}
	}()
	fmt.Println(label, f())
}

func main() {
	s := make([]int, 4, 8)

	// self and distinct
	fmt.Println("self       :", &s[0] == &s[0], &s[2] == &s[2])
	fmt.Println("distinct   :", &s[0] == &s[1], &s[0] != &s[1])

	// re-slices: s[lo:] moves the offset, s[:hi] does not
	t := s[1:]
	u := t[1:]
	r := s[1:2:3]
	h := s[:2]
	fmt.Println("reslice    :", &t[0] == &s[1], &t[0] == &s[0], &u[0] == &s[2], &r[0] == &s[1], &h[1] == &s[1])

	// append inside the capacity keeps the backing, past it does not
	w := append(s[:0:1], 99)
	g := append(s[:4:4], 5)
	fmt.Println("append     :", &w[0] == &s[0], &g[0] == &s[0])

	// an array and a slice of it
	var a [4]int
	v := a[:]
	fmt.Println("array      :", &a[1] == &a[1], &v[2] == &a[2], &v[0] == &a[1])

	// a pointer to an array, over a slice's backing and on its own
	p := (*[3]int)(s[1:4])
	q := &[3]int{}
	fmt.Println("ptr array  :", &p[0] == &s[1], &p[2] == &s[3], &q[0] == &q[0], &q[0] == &q[1])

	// named slice types, mixed with their unnamed form, and math/big's alias shape
	n := make(words, 3, 5)
	m := n[1:]
	base := []uint64(n)
	fmt.Println("named slice:", &m[0] == &n[1], &n[0] == &base[0], alias(n, m), alias(n, make(words, 1)))
	fmt.Println("overlaps   :", overlaps(base, base), overlaps(base[1:], base))
	fmt.Println("elem name  :", besideElem(s))

	// named array types, receiver fields and a parameter
	var hv holder
	fmt.Println("fields     :")
	fmt.Println(hv.sameField())
	fmt.Println(hv.sameNamedField())
	fmt.Println(sameParam(quad{}))

	// an element of an array that is itself an element
	ms := make([][4]int, 2)
	fmt.Println("nested     :", &ms[1][2] == &ms[1][2], &ms[0][2] == &ms[1][2])

	// zero-size elements over Go's shared zero-size storage
	es := make([]empty, 3)
	es2 := make([]empty, 2)
	fmt.Println("zero size  :", &es[0] == &es[1], &es[0] == &es2[0])

	// the index checks panic at the take
	var nilSlice []int
	recovered("nil slice  :", func() bool { return &nilSlice[0] == &s[0] })
	recovered("past len   :", func() bool { return &s[0] == &s[4] })
	idx := 9
	recovered("left first :", func() bool { return &s[idx] == &t[idx-1] })

	// no allocation
	fmt.Println("allocs     :", testing.AllocsPerRun(100, func() { sink = &s[0] == &t[0] }))
	fmt.Println("allocs named:", testing.AllocsPerRun(100, func() { sink = &m[0] == &n[1] }))
	fmt.Println("allocs ptr :", testing.AllocsPerRun(100, func() { sink = &p[1] == &s[2] }))

	// parenthesized operands are still two element address-ofs
	x, y := s, t
	fmt.Println("parens     :", (&x[1]) == (&y[0]), ((&x[0])) != &y[0])

	// not two element address-ofs once the parentheses are removed: these keep the boxed form
	fmt.Println("boxed      :", any(&x[1]) == any(&y[0]), (*int)(&x[0]) == &y[0])
	switch &x[1] {
	case &y[0]:
		fmt.Println("switch     : y[0]")
	default:
		fmt.Println("switch     : default")
	}
}
