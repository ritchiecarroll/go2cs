// t1: a generic FUNCTION with a []T parameter, T under a non-self method-set constraint, called with a slice of a
// POINTER type (T inferred as *item). Mirrors typescript-go's tsbaseline.GetErrorBaseline(WrapASTDiagnostics(...)).
// Go prints: a,b 2
package main

import (
	"fmt"
	"slices"
	"strings"
)

type Named interface{ Name() string }

type item struct{ n string }

func (i *item) Name() string { return i.n }

func compareItems(a, b *item) int { return strings.Compare(a.n, b.n) }

func wrap(ns []string) []*item {
	out := make([]*item, len(ns))
	for i, n := range ns {
		out[i] = &item{n}
	}
	return out
}

func join[T Named](xs []T, cmp func(a, b T) int) string {
	ys := slices.Clone(xs) // as tsbaseline.iterateErrorBaseline: a NEW local, the parameter is never written
	slices.SortFunc(ys, cmp)
	names := make([]string, len(ys))
	for i, x := range ys {
		names[i] = x.Name()
	}
	return strings.Join(names, ",")
}

func main() {
	items := wrap([]string{"b", "a"})
	fmt.Println(join(items, compareItems), len(items))
}
