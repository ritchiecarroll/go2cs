// Guards math's trigonometric argument reduction for huge arguments. Sin, Cos, Sincos and Tan reduce
// an argument at or above reduceThreshold (1<<29) with Payne-Hanek (trigReduce), behind the test
// `x >= reduceThreshold` -- a float64 compared with an untyped integer constant, the shape of the
// float-vs-UntypedInt defect (UntypedIntFloatOperandTests). That defect does NOT reach these sites:
// the truncating conversion saturates for |x| >= 2^63 and the threshold is a small integer, so the
// truncated comparison agrees with Go (measured: this project passed before the fix). It is kept as a
// standing guard that the reduction runs for 2^63, 2^64 and 1e300. Output-compared with go run.
package main

import (
	"fmt"
	"math"
)

func main() {
	for _, x := range []float64{1 << 63, 1 << 64, 1e300, -(1 << 63)} {
		s, c := math.Sincos(x)
		fmt.Printf("%g: sin=%.17g cos=%.17g tan=%.17g sincos=(%.17g, %.17g)\n", x, math.Sin(x), math.Cos(x), math.Tan(x), s, c)
	}
}
