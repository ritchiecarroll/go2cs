// FloatToIntArchProbe is a MEASUREMENT, not a guard: it prints, as ONE stderr line, what a float converted to each
// integer kind through a type parameter reads for out-of-range values, NaN and the infinities. Go leaves those
// implementation-defined, so the answer is per GOARCH. The behavioral runner reports a stderr first-line mismatch with
// both lines verbatim, so a hosted arm64 leg prints gc's arm64 answers beside golib's (TS readiness C36).
package main

import "fmt"

type integer interface {
	~int | ~int8 | ~int16 | ~int32 | ~int64 | ~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
}

func conv[T integer](v any) T { return T(v.(float64)) }

func row(f float64) string {
	return fmt.Sprint(f, "=", conv[int](f), ",", conv[int8](f), ",", conv[int16](f), ",", conv[int32](f), ",", conv[int64](f), ",",
		conv[uint](f), ",", conv[uint8](f), ",", conv[uint16](f), ",", conv[uint32](f), ",", conv[uint64](f), ",", conv[uintptr](f))
}

func main() {
	inf := 1e308
	inf *= 10
	nan := inf - inf
	line := "kinds int,int8,int16,int32,int64,uint,uint8,uint16,uint32,uint64,uintptr"
	for _, f := range []float64{-1.9, -129.5, 300.7, 32768.1, 2147483648.1, -2147483649.1, 4294967296.1,
		9.2233720368547758e18, 1.8446744073709550e19, 1.8446744073709552e19, 3e19, -1e19, inf, -inf, nan} {
		line += " | " + row(f)
	}
	println(line)
}
