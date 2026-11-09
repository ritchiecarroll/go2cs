// PointerUnionNilCompare guards `x == nil` / `x != nil` on a type parameter whose type set holds only POINTER types
// and is not erased (catalogue row C4 of EXPERIMENT-ts-readiness-2026-11-07.md; TypeScript's checker relater.go uses
// `*ast.Node | *ast.Symbol | *checker.Type`). The parameter erases to an unconstrained C# T, so the comparison needs
// golib's nilness rule rather than an operator. A nil pointer reaches T both as C# null (a zero value) and as the
// typed nil box (a nil in a composite literal); Go treats both as nil.
package main

import (
	"fmt"

	ast "go/ptrunion"
)

type Type struct{ ID int }

type entity interface {
	*ast.Node | *ast.Symbol | *Type
}

// describe: nil comparison in a generic function over the union, then a type switch.
func describe[T entity](x T) string {
	if x == nil {
		return "nil"
	}
	switch v := any(x).(type) {
	case *ast.Node:
		return fmt.Sprintf("node:%d", v.Kind)
	case *ast.Symbol:
		return "symbol:" + v.Name
	case *Type:
		return fmt.Sprintf("type:%d", v.ID)
	}
	return "?"
}

// present: the != form, with an inline union constraint.
func present[T *ast.Node | *ast.Symbol | *Type](x T) bool { return x != nil }

func zero[T entity]() T { var z T; return z }

func eq[T entity](a, b T) bool { return a == b }

// link: a generic type over the union whose method compares its fields with nil.
type link[T entity] struct{ from, to T }

func (l link[T]) String() string {
	if l.to == nil {
		return describe(l.from) + "->(none)"
	}
	return describe(l.from) + "->" + describe(l.to)
}

// holder / holderOf: a single pointer core on a generic TYPE (not erased, unlike a function's `[P *T]`).
type holder[P *Type,] struct{ p P }

func (h holder[P]) empty() bool { return h.p == nil }

type holderOf[P *T, T any] struct{ p P }

func (h holderOf[P, T]) empty() bool { return h.p == nil }

func main() {
	n := &ast.Node{Kind: 4}
	fmt.Println(describe(n), describe(&ast.Symbol{Name: "x"}), describe(&Type{ID: 9}))
	fmt.Println(describe(zero[*Type]()), describe[*ast.Node](nil), present(n), present[*ast.Symbol](nil))
	fmt.Println(link[*ast.Node]{n, n}, link[*ast.Node]{n, nil}, link[*Type]{})
	fmt.Println(eq(n, n), eq(n, &ast.Node{Kind: 4}), eq[*Type](nil, nil))
	fmt.Println(holder[*Type]{}.empty(), holder[*Type]{&Type{1}}.empty())
	fmt.Println(holderOf[*int, int]{}.empty(), holderOf[*int, int]{new(int)}.empty())
}
