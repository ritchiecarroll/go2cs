// A func value passed where the parameter is a TYPE PARAMETER is not boxed: Go instantiates T as the func type, so the
// value, nil included, stays a func. Only a real `any` parameter is an interface boundary (TS readiness C39).
package main

import "fmt"

func IfElse[T any](b bool, whenTrue T, whenFalse T) T {
	if b {
		return whenTrue
	}
	return whenFalse
}

func first[T any](xs ...T) T { return xs[0] }

type NameGenerator struct {
	IsUnique func(name string, private bool) bool
}

type gen struct{ used map[string]bool }

func (g *gen) isUnique(name string, private bool) bool { return !g.used[name] }

func makeName(base string, check func(name string, private bool) bool) string {
	if check == nil || check(base, false) {
		return base
	}
	return base + "_1"
}

func isNilAny(v any) bool { return v == nil }

// pick passes func-typed PARAMETERS (nil when the caller passes nil) through the type parameter.
func pick(f func() int, cond bool) {
	g := IfElse(cond, f, nil)
	h := first(f)
	fmt.Println("param:", g == nil, h == nil, isNilAny(f), isNilAny(g))
}

func main() {
	used := map[string]bool{"x": true}

	// The TS shape: a func-typed FIELD.
	ng := &NameGenerator{IsUnique: func(name string, private bool) bool { return !used[name] }}
	fmt.Println("field:", makeName("x", IfElse(true, ng.IsUnique, nil)), makeName("x", IfElse(false, ng.IsUnique, nil)))

	// A func-typed VARIABLE.
	g := &gen{used: used}
	var f func(name string, private bool) bool = g.isUnique
	fmt.Println("variable:", makeName("x", IfElse(true, f, nil)), makeName("x", IfElse(false, f, nil)))

	// Controls: a method value and a func literal never box.
	fmt.Println("method value:", makeName("x", IfElse(true, g.isUnique, nil)))
	fmt.Println("literal:", makeName("y", IfElse(true, func(string, bool) bool { return false }, nil)))

	// A nil func through the type parameter is still nil; through a real `any` it is a typed nil, not nil.
	pick(nil, true)
	pick(func() int { return 1 }, true)
	pick(func() int { return 1 }, false)
}
