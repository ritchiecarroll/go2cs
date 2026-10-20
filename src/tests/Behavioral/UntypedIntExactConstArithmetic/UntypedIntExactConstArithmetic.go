// UntypedIntExactConstArithmetic guards constant arithmetic around a NAMED untyped integer constant whose value
// leaves int64. Go evaluates every constant expression exactly at compile time; the converter renders a named
// untyped constant as a golib UntypedInt, whose operators are int64, so such an expression must reach the C# as
// Go's value.
//
// Go 1.26's internal/strconv writes both shapes below: a compile-time assert `_ = assert[(m*5^N)%(1<<64)-1]`
// (the index is the constant 0, and -1 under int64 arithmetic, where 1<<64 is 1) and a table of `maxUint64 / 5^N`
// (where -1 / 5 is 0). Its shortest float formatter runs that assert, so printing 0.5 is part of the guard.
package main

import "fmt"

const maxUint64 = 1<<64 - 1

const small = 10

// A named type over uint, as math/big's Word is, with the mask idiom `_M / 4`.
type word uint

const wordMask = 1<<64 - 1

var quotients = [3]uint64{maxUint64 / 5, maxUint64 / 5 / 5, maxUint64/10 + 1}

var words = []word{0, 1, wordMask / 4, wordMask / 2, wordMask - 1, wordMask}

// trimmed is internal/strconv's trimZeros prologue: the compile-time asserts must hold at run time too.
func trimmed() int {
	const (
		div1e8m = 0xc767074b22e90e21
		div1e4m = 0xd288ce703afb7e91
		div1e2m = 0x8f5c28f5c28f5c29
		div1e1m = 0xcccccccccccccccd
	)

	var assert [1]struct{}
	_ = assert[(div1e8m*5*5*5*5*5*5*5*5)%(1<<64)-1]
	_ = assert[(div1e4m*5*5*5*5)%(1<<64)-1]
	_ = assert[(div1e2m*5*5)%(1<<64)-1]
	_ = assert[(div1e1m*5)%(1<<64)-1]

	return len(assert)
}

func main() {
	for _, q := range quotients {
		fmt.Println(q)
	}

	for _, w := range words {
		fmt.Println(uint64(w))
	}

	fmt.Println("asserts:", trimmed())

	// Controls: an int64 evaluation that wraps back to Go's value, and one that never leaves int64.
	var wraps uint64 = maxUint64 - 1
	var inRange int64 = small * 3
	fmt.Println(wraps, inRange)

	// The shortest float formatter (Dragonbox) runs trimZeros.
	fmt.Println(0.5, 1.25, 3.0, 1e21, 123456.789)
	fmt.Printf("%v %g %v\n", 0.1, 2.5e-8, float32(0.75))
}
