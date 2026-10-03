// Guards math's trigonometric argument reduction for huge arguments. Sin, Cos, Sincos and Tan reduce
// an argument at or above reduceThreshold (1<<29) with Payne-Hanek (trigReduce), through the test
// `x >= reduceThreshold` -- a float64 compared with an untyped integer constant. go2cs emitted that
// comparison against golib's UntypedInt, which converted x with a truncating int64 cast, so for
// |x| >= 2^63 the test read false, the reduction was skipped, and the results were wrong. Output-
// compared with go run.
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
