package main

import (
	"fmt"
	"math/bits"
)

// The 64-bit leaves at their edges: zero, one, all ones, the top bit, and a carry or borrow
// in and out. go2cs hand-owns all three (Mul64 as one Math.BigMul), so the results must stay
// bit-exact with Go's.
var edges = []uint64{
	0, 1, 2, 1<<63 - 1, 1 << 63, 1<<63 + 1, 1<<64 - 2, 1<<64 - 1,
	1<<32 - 1, 1<<64 - 1<<32, 0x5555555555555555, 0xAAAAAAAAAAAAAAAA,
}

func main() {
	for _, x := range edges {
		for _, y := range edges {
			s0, c0 := bits.Add64(x, y, 0)
			s1, c1 := bits.Add64(x, y, 1)
			d0, b0 := bits.Sub64(x, y, 0)
			d1, b1 := bits.Sub64(x, y, 1)
			hi, lo := bits.Mul64(x, y)
			fmt.Printf("%x %x: add %x/%d %x/%d sub %x/%d %x/%d mul %x:%x\n", x, y, s0, c0, s1, c1, d0, b0, d1, b1, hi, lo)
		}
	}

	// a fixed sweep, folded into one checksum per leaf
	var state uint64 = 0x9E3779B97F4A7C15
	next := func() uint64 {
		state ^= state << 13
		state ^= state >> 7
		state ^= state << 17
		return state
	}
	var addSum, subSum, mulSum uint64
	for i := 0; i < 100000; i++ {
		x, y, bit := next(), next(), uint64(i&1)
		s, c := bits.Add64(x, y, bit)
		d, b := bits.Sub64(x, y, bit)
		hi, lo := bits.Mul64(x, y)
		addSum = addSum*31 + s + c
		subSum = subSum*31 + d + b
		mulSum = mulSum*31 + hi ^ lo
	}
	fmt.Printf("sweep: add %x sub %x mul %x\n", addSum, subSum, mulSum)
}
