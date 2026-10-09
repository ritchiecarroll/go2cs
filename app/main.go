// Command hashsetdemo is the nugetgo rehearsal's sample consumer: it imports
// github.com/ritchiecarroll/hashset and prints results that do not depend on
// map iteration order.
package main

import (
	"fmt"
	"sort"

	"github.com/ritchiecarroll/hashset"
)

func sorted(s hashset.HashSet[int]) []int {
	keys := s.Keys()
	sort.Ints(keys)
	return keys
}

func main() {
	a := hashset.NewHashSet([]int{1, 2, 3, 4})
	b := hashset.NewHashSet([]int{3, 4, 5})

	fmt.Println("add 5:", a.Add(5), "add 1:", a.Add(1))
	fmt.Println("contains 2:", a.Contains(2), "contains 9:", a.Contains(9))
	fmt.Println("len:", len(a))

	a.IntersectWith(b.Keys())
	fmt.Println("intersect:", sorted(a))

	words := hashset.NewHashSet([]string{"go", "cs", "go"})
	fmt.Println("words:", len(words), words.Contains("cs"))

	removed := b.RemoveWhere(func(v int) bool { return v%2 == 1 })
	fmt.Println("removed odd:", removed, "left:", sorted(b))
}
