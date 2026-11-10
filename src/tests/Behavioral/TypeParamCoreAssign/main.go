package main

import (
	"fmt"
	"maps"
	"slices"
	"sort"
)

// A value of a type parameter whose core type is a slice or a map is assignable to that core type (Go spec,
// Assignability). Every arm below hands such a value to a position typed by the core type, then writes through
// the result: Go SHARES the backing, so the write must be visible through the original.

// tsc's internal/core/core.go:805, verbatim (TS readiness C24).
func CopyMapInto[M1 ~map[K]V, M2 ~map[K]V, K comparable, V any](dst M1, src M2) map[K]V {
	if dst == nil {
		return maps.Clone(src)
	}
	maps.Copy(dst, src)
	return dst
}

func MapBack[M ~map[K]V, K comparable, V any](m M) map[K]V { return m }

func MapVar[M ~map[K]V, K comparable, V any](m M) map[K]V {
	var r map[K]V = m
	return r
}

func MapAssign[M ~map[K]V, K comparable, V any](m M) map[K]V {
	var r map[K]V
	r = m
	return r
}

func takesMap(m map[string]int) int { return len(m) }

func MapArg[M ~map[string]int](m M) int { return takesMap(m) }

func SliceBack[S ~[]E, E any](s S) []E { return s }

func SliceClone[S ~[]E, E any](s S) []E { return slices.Clone(s) }

func SliceVar[S ~[]E, E any](s S) []E {
	var r []E = s
	return r
}

func SliceAssign[S ~[]E, E any](s S) []E {
	var r []E
	r = s
	return r
}

func takesSlice(s []int) int { return len(s) }

// Control: the argument form, which converts today.
func SliceArg[S ~[]int](s S) int { return takesSlice(s) }

type Named map[string]int

type Ints []int

// A self-containing named map: its wrapper holds the map in a reference holder.
type Tree map[string]Tree

func keys[K comparable, V any](m map[K]V) []string {
	var out []string
	for k := range m {
		out = append(out, fmt.Sprint(k))
	}
	sort.Strings(out)
	return out
}

func main() {
	var a map[string]int
	a = CopyMapInto(a, map[string]int{"x": 1})
	b := CopyMapInto(a, map[string]int{"y": 2})
	b["z"] = 3
	fmt.Println(len(a), len(b), keys(a)) // b shares a's map

	n := Named{"n": 1}
	for i, f := range []func(Named) map[string]int{MapBack[Named], MapVar[Named], MapAssign[Named]} {
		r := f(n)
		r[fmt.Sprint("w", i)] = i
	}
	fmt.Println(len(n), keys(n), MapArg(n))

	t := Tree{"leaf": nil}
	tr := MapBack(t)
	tr["added"] = Tree{}
	fmt.Println(len(t), keys(t))

	s := Ints{1, 2, 3}
	for i, f := range []func(Ints) []int{SliceBack[Ints], SliceVar[Ints], SliceAssign[Ints]} {
		r := f(s)
		r[i] = 10 * (i + 1)
	}
	c := SliceClone(s)
	c[0] = -1
	fmt.Println(s, c, SliceArg(s))
}
