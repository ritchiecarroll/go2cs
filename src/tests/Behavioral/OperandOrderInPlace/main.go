// The controls: operands gc reads IN PLACE, left to right, even when a later call changes them:
// len and cap of a slice variable, a slice-to-array conversion, and a map literal's elements.
package main

import "fmt"

func pair(a, b int) string { return fmt.Sprint(a, " ", b) }

func main() {
	sl := []int{1}
	grow := func() int { sl = append(sl, 5, 6); return 2 }
	fmt.Println("len:", pair(len(sl), grow()))

	sl = make([]int, 1, 1)
	fmt.Println("cap:", pair(cap(sl), grow()))

	sl = []int{1, 2}
	swap := func() int { sl = []int{8, 9}; return 2 }
	fmt.Println("slice to array:", [2]int(sl), swap())

	a := 1
	setA := func() int { a = 10; return 2 }
	fmt.Println("map literal:", map[string]int{"a": a, "b": setA()})
}
