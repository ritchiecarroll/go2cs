package main

import (
	"fmt"
	"slices"
)

// A Go method named Clone on a named slice is THE method: every call below must reach it, never a copy the C#
// wrapper might offer under the same name (go-json-experiment's jsontext.Value.Clone is the shape).
type V []byte

func (v V) Clone() V {
	r := make(V, len(v)+1)
	copy(r, v)
	r[len(v)] = '!'
	return r
}

// A pointer-receiver Clone with a different result: the copy is not the only meaning of the name.
type W []int

func (w *W) Clone() (W, error) {
	if *w == nil {
		return nil, fmt.Errorf("nil W")
	}
	return append(W{-1}, (*w)...), nil
}

type Cloner interface{ Clone() V }

// A generic named slice declaring Clone.
type GS[T any] []T

func (g GS[T]) Clone() GS[T] { return append(append(GS[T]{}, g...), g...) }

// Control: a named map declaring Clone (net/http.Header's shape).
type M map[string]int

func (m M) Clone() M {
	r := M{"cloned": 1}
	for k, v := range m {
		r[k] = v
	}
	return r
}

// Control: a named slice WITHOUT a Clone method still copies by slices.Clone, keeping its type.
type U []string

func main() {
	v := V("ab")
	c := v.Clone()
	fmt.Println(string(c), len(c), len(v))

	var a any = v.Clone()
	fmt.Printf("%T %s\n", a, a)
	fmt.Println(a)

	p := &v
	fmt.Println(string(p.Clone()))

	var i Cloner = v
	fmt.Println(string(i.Clone()))

	f := v.Clone
	fmt.Println(string(f()))

	w := W{1, 2}
	wc, err := w.Clone()
	fmt.Println(wc, err)
	var wn W
	_, err = wn.Clone()
	fmt.Println(err)

	g := GS[int]{3}
	fmt.Println(g.Clone(), len(g))

	m := M{"x": 2}
	fmt.Println(m.Clone()["cloned"], m.Clone()["x"], len(m))

	u := U{"q"}
	uc := slices.Clone(u)
	uc[0] = "r"
	fmt.Printf("%T %v %v\n", uc, u, uc)
}
