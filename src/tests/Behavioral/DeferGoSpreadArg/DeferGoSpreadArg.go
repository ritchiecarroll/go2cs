// A defer or go statement whose call SPREADS a slice into a variadic parameter (`defer f(xs...)`).
//
// Go evaluates the call's arguments at the statement, and a spread argument is the slice itself:
// the callee's pack IS the caller's slice, sharing its backing array. So a write through xs after
// the statement is seen by the call, while re-assigning xs to a new slice is not.
package main

import (
	"fmt"
	"sync"
)

func show(tag string, xs ...int) {
	fmt.Println(tag, len(xs), xs)
}

type collector struct {
	seen []string
}

func (c *collector) note(tag string, xs ...int) {
	c.seen = append(c.seen, fmt.Sprint(tag, xs))
}

func deferSpread() {
	xs := []int{1, 2}
	defer show("defer", xs...)
	xs[0] = 9
	xs = []int{7, 7, 7}
	fmt.Println("after defer", xs)
}

func deferMethodSpread(c *collector) {
	xs := []int{5, 6}
	defer c.note("method", xs...)
	xs[1] = 60
	xs = nil
}

func report(wg *sync.WaitGroup, gate chan struct{}, tag string, xs ...int) {
	defer wg.Done()
	<-gate
	fmt.Println(tag, len(xs), xs)
}

func goSpread() {
	var wg sync.WaitGroup
	gate := make(chan struct{})
	xs := []int{3, 4}
	wg.Add(1)
	go report(&wg, gate, "go", xs...)
	xs[0] = 8
	xs = []int{0}
	close(gate)
	wg.Wait()
	fmt.Println("after go", xs)
}

func main() {
	deferSpread()

	c := &collector{}
	deferMethodSpread(c)
	fmt.Println(c.seen)

	goSpread()
}
