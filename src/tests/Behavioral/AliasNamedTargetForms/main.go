// A defined type reached through a type ALIAS must convert, make and append exactly as the defined type
// itself does: go/types reports the alias (*types.Alias) where the converter's arms test for the named type.
package main

import "fmt"

// make of a defined map: Go's make returns a NON-nil empty map, through the alias as directly.
type M map[string]int

type MA = M

// A conversion between a defined struct and its written base, through an alias of the defined type.
type Base struct{ A int }

type Def Base

type DA = Def

// A slice whose ELEMENT is an alias of a named slice: append converts the operand to the element type.
type Row []int

type RowA = Row

type Table []RowA

// A conversion between a defined numeric and its written base, through an alias.
type Handle uint32

type Key Handle

type KA = Key

// A function value converted to an alias of a generic named func type.
type Seq[K any] func(yield func(K) bool)

type SeqInt = Seq[int]

func main() {
	m := make(MA)
	fmt.Println("make alias:", m == nil, len(m))
	m["a"] = 1
	fmt.Println(m)

	direct := make(M)
	sized := make(MA, 4)
	fmt.Println("make direct:", direct == nil, "make sized:", sized == nil)

	b := Base{1}
	d := DA(b)
	fmt.Println("struct wrapper:", d.A, Base(d).A)

	var t Table
	t = append(t, []int{1, 2})
	t = append(t, Row{3})
	fmt.Println("append:", t, len(t))

	h := Handle(7)
	k := KA(h)
	fmt.Println("numeric wrapper:", k, Handle(k)+1)

	f := func(yield func(int) bool) {
		for i := 0; i < 3; i++ {
			if !yield(i) {
				return
			}
		}
	}
	s := SeqInt(f)
	s(func(i int) bool { fmt.Println("seq", i); return i < 1 })
	var none SeqInt = SeqInt(nil)
	fmt.Println("nil seq:", none == nil)
}
