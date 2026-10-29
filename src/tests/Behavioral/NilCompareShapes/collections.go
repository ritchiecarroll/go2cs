package main

import (
	"fmt"
	"unsafe"
)

// Untyped nil compared with a slice, a map or a chan, for every way such a value is made. Only
// the nil slice, map and chan compare equal to nil: an empty slice or map that was made, or a
// slice of zero length over real storage, is not nil.

type byteSlice []byte

type counts map[string]int

type events chan int

type bag struct {
	items []string
	index map[string]int
	done  chan struct{}
}

func variadic(xs ...int) bool {
	return xs == nil
}

func sliceIsNil[S ~[]E, E any](s S) bool {
	return s == nil
}

func collections() {
	var s []int
	fmt.Println("nil slice:", s == nil, nil == s, s != nil)
	fmt.Println("nil slice resliced:", s[:] == nil, s[0:0] == nil)
	fmt.Println("append nothing to nil:", append(s, []int{}...) == nil)
	fmt.Println("make([]int, 0):", make([]int, 0) == nil, nil != make([]int, 0, 8))
	fmt.Println("empty literal:", []int{} == nil)

	arr := [3]int{1, 2, 3}
	fmt.Println("array resliced to zero:", arr[:0] == nil, arr[3:] == nil)
	fmt.Println("[]byte(\"\"):", []byte("") == nil)
	fmt.Println("unsafe.Slice(nil, 0):", unsafe.Slice((*byte)(nil), 0) == nil)
	fmt.Println("unsafe.Slice(&arr[0], 0):", unsafe.Slice(&arr[0], 0) == nil)

	var bs byteSlice
	fmt.Println("nil named slice:", bs == nil, nil == bs)
	bs = byteSlice{}
	fmt.Println("empty named slice:", bs == nil, bs != nil)

	var m map[string]int
	fmt.Println("nil map:", m == nil, nil == m)
	fmt.Println("made map:", make(map[string]int) == nil, map[string]int{} != nil)

	var c counts
	fmt.Println("nil named map:", c == nil)
	c = counts{"a": 1}
	fmt.Println("named map with an entry:", c != nil, nil != c)

	var ch chan int
	fmt.Println("nil chan:", ch == nil, nil == ch)
	var send chan<- int
	var recv <-chan int
	fmt.Println("nil directional chans:", send == nil, recv == nil)
	ch = make(chan int, 1)
	send, recv = ch, ch
	fmt.Println("made chan:", ch == nil, send != nil, nil != recv)

	var ev events
	fmt.Println("nil named chan:", ev == nil)
	ev = make(events)
	fmt.Println("made named chan:", ev == nil)

	var b bag
	fmt.Println("zero struct fields:", b.items == nil, b.index == nil, b.done == nil)
	b = bag{items: []string{}, index: map[string]int{}, done: make(chan struct{})}
	fmt.Println("assigned struct fields:", b.items == nil, b.index == nil, b.done == nil)

	p := &s
	fmt.Println("through a pointer:", *p == nil)

	lookup := map[string][]int{"x": {1}}
	fmt.Println("map lookups:", lookup["x"] == nil, lookup["y"] == nil)

	fmt.Println("variadic:", variadic(), variadic(1), variadic(s...))
	fmt.Println("type parameter:", sliceIsNil(s), sliceIsNil([]int{}), sliceIsNil(bs))
}
