// TypeParamFloatConversion guards a conversion of a TYPE-PARAMETER value to a basic float when the type set holds
// a float member: Go 1.26 math/big's calibrate_test.go `math.IsInf(float64(v), +1)` in
// `func csv[T int | float64](name string, values []T)`. A C# cast on such a type parameter is CS0030, so the
// conversion goes through golib's ConvertToFloat64/ConvertToFloat32, decided per instantiation.
//
// Go's float64(int) rounds to nearest: 1<<53 + 1 is not representable and rounds to 1<<53. float32 rounds ONCE,
// directly from the integer, so 1<<24 + 1 rounds to 1<<24 and 16777217.0 (a float64) rounds the same way.
//
// The REVERSE direction, a float into an integer-constrained type parameter (`T(value.(float64))` with `T ~int32`,
// typescript-go's internal/tsoptions), goes through golib's ConvertToType<T>(double): Go truncates toward zero. Every
// value here is in range for its kind, because an out-of-range conversion is implementation-defined in Go and differs
// between amd64 and arm64; golib's answer for those is pinned by GolibTests' TypeParamFloatConversionTests.
package main

import (
	"fmt"
	"math"
)

func toFloat64[T int | float64](v T) float64 { return float64(v) }

func toFloat32[T int64 | float32 | float64](v T) float32 { return float32(v) }

type integer interface {
	~int | ~int8 | ~int16 | ~int32 | ~int64 | ~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
}

type count int32

func fromAny[T ~int32](value any) T { return T(value.(float64)) }

func toInteger[T integer](v float64) T { return T(v) }

func toIntegerFrom32[T integer](v float32) T { return T(v) }

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
	fmt.Println(fromAny[int32](float64(41.99)), fromAny[count](-7.5), fromAny[int32](-2147483648.9))
	fmt.Println(toInteger[int](-1.9), toInteger[int8](127.9), toInteger[int8](-128.9), toInteger[int16](-32768.5), toInteger[int64](9.2e18))
	fmt.Println(toInteger[uint](3.99), toInteger[uint8](255.9), toInteger[uint16](65535.5), toInteger[uint32](4294967295.5))
	fmt.Println(toInteger[uint64](1.8e19), toInteger[uintptr](2.5), toInteger[count](-0.5), toIntegerFrom32[int32](-3.75))
}
