// Self-referential constraints instantiated at a POINTER from a package other than the interface's: the
// constraint proxy that satisfies them must be ONE C# type for every importer, as the Go type is.
//
//   - TWO packages: main's own element (*mnode) under lib's constraints.
//   - THREE packages: lib's constraints, elem's element, instantiated here.
//   - FOUR packages: b1 instantiates and returns lib.Ord2[*elem.Node]; main instantiates it too, assigns
//     b1's value, boxes it, type-asserts it and compares %T and reflect.TypeOf.
//   - REFLECT ONLY: a zero value of b1's type made by reflect.New, filled through reflect, then called.
//
// Each for the named constraint (Ord2) and the inline one (Ord).
package main

import (
	"fmt"
	"reflect"

	"ForeignConstraintProxy/b1"
	"ForeignConstraintProxy/elem"
	"ForeignConstraintProxy/lib"
)

type mnode struct{ k int }

func (n *mnode) Less(o *mnode) bool { return n.k < o.k }

type age int

func (a age) Less(b age) bool { return a < b }

func main() {
	// Two packages, value and pointer.
	fmt.Println(lib.Ord2[age]{V: 3}.Below(5), lib.Ord[age]{V: 3}.Below(2))
	fmt.Println(lib.Ord2[*mnode]{V: &mnode{1}}.Below(&mnode{0}), lib.Ord[*mnode]{V: &mnode{1}}.Below(&mnode{4}))

	// Three packages.
	fmt.Println(lib.Ord2[*elem.Node]{V: &elem.Node{K: 1}}.Below(&elem.Node{K: 0}), lib.Ord[*elem.Node]{V: &elem.Node{K: 1}}.Below(&elem.Node{K: 4}))

	// Four packages: b1's value IS main's type.
	v := b1.Make(2)
	var w lib.Ord2[*elem.Node] = v
	var a any = v
	x, ok := a.(lib.Ord2[*elem.Node])
	fmt.Println(w.Below(&elem.Node{K: 3}), ok, x.Below(&elem.Node{K: 1}))
	fmt.Printf("%T\n", v)
	fmt.Println(reflect.TypeOf(v) == reflect.TypeOf(lib.Ord2[*elem.Node]{}))

	vi := b1.MakeInline(2)
	var wi lib.Ord[*elem.Node] = vi
	var ai any = vi
	xi, oki := ai.(lib.Ord[*elem.Node])
	fmt.Println(wi.Below(&elem.Node{K: 3}), oki, xi.Below(&elem.Node{K: 1}))
	fmt.Printf("%T\n", vi)
	fmt.Println(reflect.TypeOf(vi) == reflect.TypeOf(lib.Ord[*elem.Node]{}))

	// Reflect only: a zero value of b1's type from reflect.New, filled through reflect, then called. The
	// proxy is the pointer to reflect: the field's type is *elem.Node, and a *elem.Node sets it.
	z := reflect.New(reflect.TypeOf(v)).Elem()
	z.Field(0).Set(reflect.ValueOf(&elem.Node{K: 9}))
	fmt.Println(z.Interface().(lib.Ord2[*elem.Node]).Below(&elem.Node{K: 1}), z.Field(0).Type(), z.Field(0).Kind())
	zi := reflect.New(reflect.TypeOf(vi)).Elem()
	zi.Field(0).Set(reflect.ValueOf(&elem.Node{K: 0}))
	fmt.Println(zi.Interface().(lib.Ord[*elem.Node]).Below(&elem.Node{K: 1}), zi.Field(0).Type(), zi.Field(0).Elem().Field(0).Int())
}
