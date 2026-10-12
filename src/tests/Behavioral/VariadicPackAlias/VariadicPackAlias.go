// A variadic callee that KEEPS its pack (returns it, stores it, captures it) when the caller spread a slice into it
// (`f(xs...)`). In Go the pack is the caller's slice itself, sharing its backing array and its capacity, so a write
// through either side is seen by the other, and an append into spare capacity lands in the caller's array.
package main

import "fmt"

func keep(xs ...int) []int { return xs }

type box struct{ items []int }

func (b *box) set(xs ...int) { b.items = xs }

func copyInto(dst *[]int, xs ...int) { *dst = xs }

func capture(xs ...int) func() int { return func() int { return xs[0] } }

func main() {
	xs := []int{7, 2}
	ys := keep(xs...)
	ys[1] = 8
	fmt.Println("keep", xs)

	b := &box{}
	b.set(xs...)
	xs[0] = 1
	fmt.Println("store", b.items)

	var d []int
	copyInto(&d, xs...)
	d[1] = 3
	fmt.Println("copyInto", xs)

	f := capture(xs...)
	xs[0] = 42
	fmt.Println("capture", f())

	zs := make([]int, 1, 4)
	w := keep(zs...)
	w = append(w, 5)
	fmt.Println("append", zs[:2], len(w), cap(w))

	lit := keep(1, 2)
	lit[0] = 9
	fmt.Println("literal", lit)
}
