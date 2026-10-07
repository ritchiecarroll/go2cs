// The address of a field reached through a pointer DEREFERENCE -- `&(*p).f`, Go's explicit spelling
// of `&p.f` -- aliases the field the pointer boxes, for a plain `*T` and a named `type P *T` alike,
// and so does the implicit `&pp.f` through a named pointer, held in a local, a parameter or a struct
// field. Boxing the dereferenced VALUE instead takes the address of a copy, so every write through
// the result is lost.
package main

import (
	"fmt"
	"reflect"
)

type pub struct{ I int }

type pubPtr *pub

type holder struct{ p pubPtr }

func viaParam(pp pubPtr, q *pub) (*int, *int, *int) {
	return &pp.I, &(*pp).I, &(*q).I
}

func main() {
	pp := pubPtr(&pub{3})
	a := &(*pp).I
	*a = 7
	fmt.Println("explicit named:", (*pp).I, a == &pp.I)

	b := &pp.I
	*b = 8
	fmt.Println("implicit named:", (*pp).I)

	// The field's address agrees with reflect's address of the same field.
	f := reflect.ValueOf(pp).Elem().Field(0)
	fmt.Println("reflect agrees:", f.Addr().Interface().(*int) == &(*pp).I)

	h := holder{p: pubPtr(&pub{1})}
	c, d := &(*h.p).I, &h.p.I
	*c = 4
	*d += 1
	fmt.Println("named in a field:", (*h.p).I)

	q := &pub{10}
	x, y, z := viaParam(pp, q)
	*x, *y, *z = 20, 21, 30
	fmt.Println("through parameters:", (*pp).I, q.I)

	// The plain-pointer controls: the implicit form was always right, the explicit one was not.
	p := &pub{1}
	*(&p.I) = 2
	e := &(*p).I
	*e = 5
	fmt.Println("plain:", p.I, e == &p.I)
}
