// RenamedGenericQualified references a GENERIC type from another package that the converted package Δ-renames
// because the same package also has a METHOD of that name: coll.Set[T] (beside OrderedMap.Set) and coll.Pair[K, V]
// (beside Entry.Pair). Every position a type can take: a struct field, a local, a parameter, a result, a pointer,
// a slice, a map value, a nested type argument, a composite literal, a conversion, a type assertion and a type switch.
// The controls are the non-generic renamed coll.Kind and the unrenamed generic coll.Bag[T]. TS readiness row C22
// (tsc's `collections.Set[T]`, 280 references).
package main

import (
	"fmt"

	"RenamedGenericQualified/coll"
)

type holder struct {
	seen  coll.Set[string]
	pairs []coll.Pair[string, int]
	byKey map[string]*coll.Set[int]
	bag   coll.Bag[coll.Set[string]]
}

func count(s *coll.Set[string]) int { return s.Len() }

func makePair(k string, v int) coll.Pair[string, int] { return coll.Pair[string, int]{Key: k, Val: v} }

type pairAlias = coll.Pair[string, int]

func main() {
	var h holder
	h.seen.Add("a")
	h.seen.Add("a")
	h.seen.Add("b")
	fmt.Println(h.seen.Len(), count(&h.seen), h.seen.Has("b"), h.seen.Has("z"))

	h.pairs = append(h.pairs, makePair("x", 1), coll.Pair[string, int]{Key: "y", Val: 2})
	h.byKey = map[string]*coll.Set[int]{"odd": coll.NewSet(1, 3, 5)}
	fmt.Println(len(h.pairs), h.pairs[1].Key, h.pairs[1].Val, h.byKey["odd"].Len())

	var o coll.OrderedMap[string, int]
	o.Set("k", 7)
	e := coll.Entry{}
	p := e.Pair()
	var pa pairAlias = p
	fmt.Println(pa.Key == "", pa.Val)

	h.bag.Put(coll.Set[string]{})
	fmt.Println(h.bag.Count())

	var anyv any = makePair("z", 26)
	if q, ok := anyv.(coll.Pair[string, int]); ok {
		fmt.Println("asserted", q.Key, q.Val)
	}
	switch v := anyv.(type) {
	case coll.Set[string]:
		fmt.Println("set", v.Len())
	case coll.Pair[string, int]:
		fmt.Println("pair", v.Val)
	}

	type local struct{ s coll.Set[int] }
	var l local
	l.s.Add(4)
	ps := []*coll.Set[int]{&l.s}
	fmt.Println(ps[0].Len(), coll.Kind(3), e.Kind())
}
