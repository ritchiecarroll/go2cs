package main

import (
	"fmt"
	"math"
)

// A float64 compared with an imported UNTYPED integer constant (go-humanize's ParseBytes overflow guard,
// `if f >= math.MaxUint64`). Go converts the constant to float64; go2cs answered false for every value
// at or above 2^64 (and `<=` true), so the guard never fired and uint64(f) saturated.
func main() {
	for _, f := range []float64{1 << 63, 1 << 64, 1.8446744073709552e37} {
		fmt.Println(f, f >= math.MaxUint64, f <= math.MaxUint64, f > math.MaxInt64)
	}
}
