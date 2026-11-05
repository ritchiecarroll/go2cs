package main

import (
	"errors"
	"fmt"
	"time"
)

// Deferred calls and panic+recover: what a converted defer costs per call, in three shapes, each
// timed on its own and printed as a named part (elapsed_ns.<part>) beside the row's total:
//
//   - defer_literal: a deferred func literal that writes the named result;
//   - defer_method:  a deferred method value, bound before the defer;
//   - panic_recover: a panic that a deferred func literal recovers, every call.

type counter struct{ n int }

func (c *counter) add(d int) { c.n += d }

var errBoom = errors.New("boom")

func viaLiteral(i int) (r int) {
	defer func() { r += i & 7 }()
	return i & 1
}

func viaMethod(c *counter, i int) int {
	add := c.add
	defer add(i & 3)
	return c.n & 1
}

func viaRecover(i int) (r int) {
	defer func() {
		if e := recover(); e == errBoom {
			r = i & 15
		}
	}()
	panic(errBoom)
}

const (
	deferCalls   = 5_000_000
	recoverCalls = 200_000
)

func main() {
	start := time.Now().UnixNano()

	sum := 0
	for i := 0; i < deferCalls; i++ {
		sum += viaLiteral(i)
	}
	afterLiteral := time.Now().UnixNano()

	c := &counter{}
	for i := 0; i < deferCalls; i++ {
		sum += viaMethod(c, i)
	}
	afterMethod := time.Now().UnixNano()

	for i := 0; i < recoverCalls; i++ {
		sum += viaRecover(i)
	}
	end := time.Now().UnixNano()

	fmt.Println("checksum:", sum, c.n)
	fmt.Println("elapsed_ns.defer_literal:", afterLiteral-start)
	fmt.Println("elapsed_ns.defer_method:", afterMethod-afterLiteral)
	fmt.Println("elapsed_ns.panic_recover:", end-afterMethod)
	fmt.Println("elapsed_ns:", end-start)
}
