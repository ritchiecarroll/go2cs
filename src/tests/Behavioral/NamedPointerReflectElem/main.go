// reflect's Elem of a DEFINED pointer type (`type sPtr *s`) must alias the same storage, and hand back
// the same addressable Value, as Elem of the plain `*s` it converts from. fmt's %#v walks the struct
// behind it with Field(i) (testify's assert.Same prints exactly this on failure, and died there with a
// nil dereference), Index(i) reaches an array behind a named pointer, and Addr() of the pointee is a
// plain *s in Go -- a named pointer type is a pointer TYPE, not a different box around the storage.
package main

import (
	"fmt"
	"reflect"
)

type s struct{ i int }

type sPtr *s

type pub struct{ I int }

type pubPtr *pub

type arrPtr *[3]int

func main() {
	ps := &s{1}
	dps := sPtr(ps)
	fmt.Println(fmt.Sprintf("%#v", dps))
	fmt.Println(fmt.Sprintf("%v", dps), fmt.Sprintf("%[1]T", dps))
	fmt.Println("Addr of Elem:", reflect.ValueOf(dps).Elem().Addr().Type())

	pp := pubPtr(&pub{2})
	f := reflect.ValueOf(pp).Elem().Field(0)
	f.SetInt(5)
	fmt.Println("field write through:", (*pp).I, f.Addr().Type())

	ap := arrPtr(&[3]int{1, 2, 3})
	reflect.ValueOf(ap).Elem().Index(1).SetInt(9)
	fmt.Println("index write through:", (*ap)[1], *ap)

	// The control: the plain pointer the named one converts from.
	fmt.Println(fmt.Sprintf("%#v", ps), reflect.ValueOf(ps).Elem().Addr().Type())
}
