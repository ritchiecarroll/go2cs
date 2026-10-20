// TypeParamFloatConversion guards a conversion of a TYPE-PARAMETER value to a basic float when the type set holds
// a float member: Go 1.26 math/big's calibrate_test.go `math.IsInf(float64(v), +1)` in
// `func csv[T int | float64](name string, values []T)`. A C# cast on such a type parameter is CS0030, so the
// conversion goes through golib's ConvertToFloat64/ConvertToFloat32, decided per instantiation.
//
// Go's float64(int) rounds to nearest: 1<<53 + 1 is not representable and rounds to 1<<53. float32 rounds ONCE,
// directly from the integer, so 1<<24 + 1 rounds to 1<<24 and 16777217.0 (a float64) rounds the same way.
package main

import (
	"fmt"
	"math"
)

func toFloat64[T int | float64](v T) float64 { return float64(v) }

func toFloat32[T int64 | float32 | float64](v T) float32 { return float32(v) }

func csv[T int | float64](name string, values []T) string {
	out := name
	for _, v := range values {
		if math.IsInf(float64(v), +1) {
			out += ",inf"
		} else {
			out += fmt.Sprintf(",%v", float64(v))
		}
	}
	return out
}

func main() {
	fmt.Println(toFloat64(7), toFloat64(2.5), toFloat64(1<<53+1) == 1<<53, toFloat64(-3))
	fmt.Println(toFloat32[int64](1<<24+1) == 1<<24, toFloat32(float32(1.5)), toFloat32(16777217.0) == 1<<24)
	fmt.Println(csv("ints", []int{1, 2, 3}))
	fmt.Println(csv("floats", []float64{0.5, math.Inf(1), -2}))
}
