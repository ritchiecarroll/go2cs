// The sample program for github.com/ritchiecarroll/hashset v1.0.0. A HashSet is a map, so its order is unspecified:
// the program sorts every set it prints, and its output is the same on every run and every host.
package main

import (
	"fmt"
	"slices"

	"github.com/ritchiecarroll/hashset"
)

func sorted(hs hashset.HashSet[int]) []int {
	keys := hs.Keys()
	slices.Sort(keys)
	return keys
}

func main() {
	hs := hashset.NewHashSet([]int{3, 1, 2, 3})
	fmt.Println("new:", sorted(hs), len(hs))
	fmt.Println("add 4:", hs.Add(4), "add 1:", hs.Add(1))
	fmt.Println("contains 2:", hs.Contains(2), "contains 9:", hs.Contains(9))
	hs.UnionWith([]int{5, 6})
	fmt.Println("union:", sorted(hs))
	hs.IntersectWith([]int{2, 4, 6, 8})
	fmt.Println("intersect:", sorted(hs))
	fmt.Println("subset of 1..8:", hs.IsSubsetOf([]int{1, 2, 3, 4, 5, 6, 7, 8}), "equals {2,4,6}:", hs.SetEquals([]int{6, 4, 2}))
	fmt.Println("remove > 3:", hs.RemoveWhere(func(v int) bool { return v > 3 }), sorted(hs))
}
