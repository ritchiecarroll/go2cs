// AddressOfParenthesizedOperand takes the address of a parenthesized operand and writes through it.
//
// Go's `&(x)` is `&x`: the parentheses change nothing, so the write through the address must reach the
// variable, field or element itself, never a copy. ccgo spells libc's pthread mutex owner this way,
// `atomic.CompareAndSwapInt32(&((*pthreadMutex)(unsafe.Pointer(m)).owner), 0, tls.ID)`. Each line prints the
// original after the writes.
package main

import (
	"fmt"
	"sync/atomic"
	"unsafe"
)

type mutex struct {
	owner int32
	count int32
}

type point struct{ X, Y int }

func main() {
	// A field reached through a pointer conversion, parenthesized (libc's pthread mutex owner).
	var m mutex
	p := unsafe.Pointer(&m)
	atomic.CompareAndSwapInt32(&((*mutex)(p).owner), 0, 7)
	pc := &((*mutex)(p).count)
	*pc = 3
	fmt.Println("converted:", m.owner, m.count)

	// The same field without the parentheses.
	atomic.AddInt32(&(*mutex)(p).count, 1)
	fmt.Println("unparenthesized:", m.count)

	// A field of a value, parenthesized two ways.
	var pt point
	px := &((pt).X)
	*px = 5
	py := &(pt.Y)
	*py = 6
	fmt.Println("field:", pt.X, pt.Y)

	// An array element.
	var arr [3]int
	pe := &(arr[1])
	*pe = 9
	fmt.Println("element:", arr)

	// A local, parenthesized.
	n := 1
	pn := &(n)
	*pn = 2
	fmt.Println("local:", n)

	// A field reached through a pointer, parenthesized.
	pp := &pt
	pf := &((*pp).X)
	*pf = 11
	pg := &(pp.Y)
	*pg = 12
	fmt.Println("pointer:", pt.X, pt.Y)
}
