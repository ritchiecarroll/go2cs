// One nil per pointer type (TS C49): a nil *T is one value however it was made, whether from the `nil`
// literal, a zero-valued local or field, *new(T), or a declared zero, and Go's == says so in every place
// it compares pointers: generic == (in this package and in another), a map key, an interface, a struct
// field and an array element. Only an interface HOLDING a nil *T differs from a nil interface.
package main

import (
	"fmt"
	"slices"

	lib "NilPtrIdentityLib"
)

type node struct {
	Code int
}

// nodePtr is a named pointer type: a generated wrapper class, not a bare box.
type nodePtr *node

type holder struct {
	p *node
}

func orElse[T comparable](value T, orElse T) T {
	if value != *new(T) {
		return value
	}

	return orElse
}

func same[T comparable](a, b T) bool {
	return a == b
}

func keyCount[K comparable](keys ...K) int {
	m := map[K]int{}

	for _, k := range keys {
		m[k]++
	}

	return len(m)
}

// one wraps a value in a one-element array, so a nil made inside generic code becomes an array element.
func one[T any](v T) [1]T {
	return [1]T{v}
}

func asAny[T any](v T) any {
	return v
}

// code and nodeCode print a nil pointer as "nil" rather than dereferencing it, so one wrong line does not end the run.
func code(m *lib.Message) any {
	if m == nil {
		return "nil"
	}

	return m.Code
}

func nodeCode(n *node) any {
	if n == nil {
		return "nil"
	}

	return n.Code
}

func main() {
	var local *lib.Message
	var field holder

	// Phase 1: the TS pair, cross-package. A nil literal argument (RED at base) and a nil local (GREEN).
	fmt.Println("phase 1:")
	fmt.Println(code(lib.OrElse(nil, lib.Default)), lib.OrElse(nil, lib.Default) == lib.Default)
	fmt.Println(code(lib.OrElse(local, lib.Default)), lib.OrElse(local, lib.Default) == lib.Default)
	fmt.Println(code(lib.OrElse((*lib.Message)(nil), lib.Default)), code(lib.OrElse(*new(*lib.Message), lib.Default)))
	fmt.Println(lib.IsZero[*lib.Message](nil), lib.IsZero(local), lib.IsZero(lib.Default))

	// Phase 2: generic == in this package, each pair of nil sources.
	fmt.Println("phase 2:")
	fmt.Println(nodeCode(orElse(nil, &node{Code: 7})), nodeCode(orElse(field.p, &node{Code: 8})))
	fmt.Println(same[*node](nil, nil), same(nil, field.p), same(field.p, nil), same(field.p, *new(*node)))
	fmt.Println(lib.Same[*lib.Message](nil, local), lib.Same(local, nil), lib.Same(lib.Default, nil))
	n := &node{Code: 1}
	fmt.Println(same(n, n), same(n, &node{Code: 1}), same(n, nil))

	// Phase 3: map keys. Every nil *T is one key.
	fmt.Println("phase 3:")
	m := map[*node]int{}
	m[nil]++
	m[field.p]++
	m[*new(*node)]++
	fmt.Println(len(m), m[nil], m[field.p])
	delete(m, field.p)
	fmt.Println(len(m))
	fmt.Println(keyCount[*node](nil, field.p, *new(*node)), keyCount(n, nil, n, field.p))

	// Phase 4: interfaces. An interface holding a nil *T is NOT a nil interface, and two such interfaces are
	// equal however the nil was made.
	fmt.Println("phase 4:")
	var fromLit any = (*node)(nil)
	var fromField any = field.p
	fmt.Println(fromLit == nil, fromField == nil, fromLit == fromField)
	fmt.Println(asAny[*node](nil) == fromField, asAny(field.p) == fromLit, asAny[*node](nil) == nil)
	fmt.Println(fromLit == any((*holder)(nil)))
	fmt.Println(same[any](fromLit, fromField), same[any](fromLit, nil))

	// Phase 5: a nil pointer inside a struct and an array, compared as values.
	fmt.Println("phase 5:")
	fmt.Println(holder{p: nil} == field, holder{p: nil} == holder{}, holder{p: n} == field)
	fmt.Println([2]*node{nil, field.p} == [2]*node{}, same(holder{p: nil}, field))

	// Phase 6: a named pointer type, and the standard library's own generic ==.
	fmt.Println("phase 6:")
	var named nodePtr
	fmt.Println(same[nodePtr](nil, named), same(named, nodePtr(nil)), keyCount[nodePtr](nil, named))
	fmt.Println(slices.Contains([]*node{field.p}, nil), slices.Index([]*node{n, nil}, field.p))
	fmt.Println(slices.Contains([]*node{nil}, field.p), slices.Equal([]*node{nil}, []*node{field.p}))

	// Phase 7: a nil pointer as an array element and inside map keys of array and struct type.
	fmt.Println("phase 7:")
	fmt.Println(one[*node](nil) == [1]*node{}, one(field.p) == one[*node](nil), one(n) == one[*node](nil))
	a := [2]*node{n, n}
	a[1] = nil
	a[0] = field.p
	fmt.Println(a == [2]*node{}, same(a, [2]*node{}))
	byArray := map[[1]*node]int{}
	byArray[one[*node](nil)]++
	byArray[[1]*node{}]++
	byArray[one(field.p)]++
	fmt.Println(len(byArray), byArray[[1]*node{nil}])
	byStruct := map[holder]int{}
	byStruct[holder{p: nil}]++
	byStruct[field]++
	byStruct[holder{}]++
	fmt.Println(len(byStruct), byStruct[holder{p: nil}], keyCount(holder{p: nil}, field, holder{}))
}
