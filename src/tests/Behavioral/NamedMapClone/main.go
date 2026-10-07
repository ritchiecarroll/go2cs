// maps.Clone of a NAMED map type returns that named type: `func Clone[M ~map[K]V](m M) M` asserts
// its worker's result back to M (`clone(m).(M)`). go2cs' clone returned a plain map, so the assertion
// panicked "interface {} is map, not main.Fields" — logrus' `maps.Clone(entry.Data)` on its
// `type Fields map[string]interface{}`. Covers an ordinary named map, a nil one, and a SELF-CONTAINING
// named map (held in a reference holder by go2cs-gen), each read back through %T and an assertion,
// and checks the clone's storage is independent of the original.
package main

import (
	"fmt"
	"maps"
)

type Fields map[string]any

type Tree map[string]Tree

func main() {
	f := Fields{"a": 1}
	c := maps.Clone(f)
	c["b"] = 2
	fmt.Printf("%T %d %d\n", c, len(f), len(c))

	var nf Fields
	nc := maps.Clone(nf)
	fmt.Printf("%T %v\n", nc, nc == nil)

	t := Tree{"x": nil}
	tc := maps.Clone(t)
	tc["y"] = Tree{}
	fmt.Printf("%T %d %d\n", tc, len(t), len(tc))

	var i any = maps.Clone(f)
	_, ok := i.(Fields)
	_, plain := i.(map[string]any)
	fmt.Println(ok, plain)
}
