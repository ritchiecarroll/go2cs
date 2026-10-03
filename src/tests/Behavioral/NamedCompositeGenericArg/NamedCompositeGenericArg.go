// A named map, slice, channel or array passed to a generic function's unnamed composite parameter: the call
// spells its type arguments (CS0411 when C# must infer through the named type's implicit conversion).

package main

import (
	"fmt"
	"sort"
)

type Names map[int]string
type Scores []float64
type Feed chan int
type Pred func(int) bool
type Grid [2]int

func first[T any](a [2]T) T { return a[0] }

func keys[V any](m map[int]V) []int {
	out := make([]int, 0, len(m))
	for k := range m {
		out = append(out, k)
	}
	sort.Ints(out)
	return out
}

func total[E int | float64](s []E) E {
	var t E
	for _, v := range s {
		t += v
	}
	return t
}

func drain[T any](c chan T) []T {
	close(c)
	var out []T
	for v := range c {
		out = append(out, v)
	}
	return out
}

func count[T any](items []T, p func(T) bool) int {
	n := 0
	for _, it := range items {
		if p(it) {
			n++
		}
	}
	return n
}

func main() {
	names := Names{2: "b", 1: "a"}
	fmt.Println("named map:", keys(names))

	scores := Scores{1.5, 2.5}
	fmt.Println("named slice:", total(scores))

	feed := make(Feed, 2)
	feed <- 7
	fmt.Println("named chan:", drain(feed))

	var even Pred = func(v int) bool { return v%2 == 0 }
	fmt.Println("named func:", count([]int{1, 2, 4}, even))

	grid := Grid{4, 5}
	fmt.Println("named array:", first(grid))

	// Controls: an unnamed argument, and an explicit instantiation.
	fmt.Println("controls:", keys(map[int]string{3: "c"}), keys[string](names))
}
