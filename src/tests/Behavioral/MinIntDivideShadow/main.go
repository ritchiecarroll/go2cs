package main

import (
	"fmt"
	"math"
)

// A package that declares its own quo and rem: the converted signed division by a variable divisor
// must still reach golib's helpers, which carry Go's MinInt / -1 wrap, not bind to these.
// MinIntDivide holds the division classes themselves.

func quo(a, b string) string { return a + "/" + b }

var rem = "remainder"

func main() {
	var i32 int32 = math.MinInt32
	var i64 int64 = math.MinInt64
	m1 := -1

	fmt.Println(quo("a", "b"), rem)
	fmt.Println(i32/int32(m1), i32%int32(m1))
	fmt.Println(i64/int64(m1), i64%int64(m1))
}
