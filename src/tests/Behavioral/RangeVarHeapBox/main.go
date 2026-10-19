package main

import "fmt"

// A range variable whose address is taken (`for i := range s { p := &i }`) escapes to the heap.
// The combined `var (i, _) = …` foreach cannot also declare it as `ref var i = ref heap(…)` (and
// would clash — CS0136). The converter iterates a TEMP foreach var and, PER ITERATION, allocates a
// fresh box and copies the temp into it — `foreach (var (iᴛ1, _) in s) { ref var i = ref heap(…,
// out var Ꮡi); i = iᴛ1; … }`. Per-iteration boxing is required for Go 1.22 loop-variable semantics:
// a stored `&i` must point to a DISTINCT box each pass. Mirrors runtime's `for i := range stackpool`
// and `for _, f := range s.Fields` (where `&f.Name`'s method makes `f` escape).

func sumWithinIter(s []int) int {
	sum := 0
	for i := range s {
		p := &i // address of the range var, used within the iteration
		sum += *p + s[i]
	}
	return sum
}

func collectPointers(s []int) []*int {
	var ptrs []*int
	for i := range s {
		ptrs = append(ptrs, &i) // Go 1.22: each &i is a DISTINCT box
	}
	return ptrs
}

// The same rule on the INT, STRING and CHANNEL range forms, where each closure reads its own
// iteration's variable through a pointer. These arms once declared ONE box before the loop, so
// every closure shared it (pre-Go-1.22 semantics); each must see a distinct box.

func intClosures() []func() int {
	var fs []func() int
	for i := range 3 {
		p := &i
		fs = append(fs, func() int { return *p })
	}
	return fs
}

func stringClosures(s string) []func() string {
	var fs []func() string
	for i, r := range s {
		pi, pr := &i, &r
		fs = append(fs, func() string { return fmt.Sprint(*pi, ":", string(*pr)) })
	}
	return fs
}

func chanClosures(ch chan int) []func() int {
	var fs []func() int
	for v := range ch {
		p := &v
		fs = append(fs, func() int { return *p })
	}
	return fs
}

func main() {
	s := []int{10, 20, 30}
	fmt.Println(sumWithinIter(s)) // (0+10)+(1+20)+(2+30) = 63

	ptrs := collectPointers(s)
	for _, p := range ptrs {
		fmt.Print(*p, " ")
	}
	fmt.Println() // 0 1 2  (distinct per-iteration values, not 2 2 2)

	for _, f := range intClosures() {
		fmt.Print(f(), " ")
	}
	fmt.Println() // 0 1 2

	for _, f := range stringClosures("aé!") {
		fmt.Print(f(), " ")
	}
	fmt.Println() // 0:a 1:é 3:!

	ch := make(chan int, 3)
	ch <- 7
	ch <- 8
	ch <- 9
	close(ch)
	for _, f := range chanClosures(ch) {
		fmt.Print(f(), " ")
	}
	fmt.Println() // 7 8 9
}
