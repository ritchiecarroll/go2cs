// UntypedFloatTypedCompare pins a comparison between a typed operand and a NAMED untyped float constant.
// Go converts the constant to the operand's type and compares at that type: beside a float32 the constant
// is rounded to float32 first, beside a 64-bit integer it is that exact integer. The converter emits the
// constant as golib's UntypedFloat, whose implicit conversion from float64 C# chains after a built-in
// widening, so without a cast at the comparison the wrapper's DOUBLE operator runs: float32(0.1) compares
// against the unrounded 0.1, and 2^53+1 rounds to 2^53. Arithmetic beside such a constant already takes
// the cast (convBinaryExpr); a literal renders as a C# literal and is unaffected -- both are controls.
package main

import "fmt"

const tenth = 0.1
const twoTo53 = 9007199254740992.0 // 2^53, an untyped FLOAT constant with an integral value
const third = 1.0 / 3

func main() {
	f := float32(0.1)
	fmt.Println("float32(0.1) <= tenth:", f <= tenth)
	fmt.Println("float32(0.1) == tenth:", f == tenth)
	fmt.Println("float32(1/3) == third:", float32(1.0/3) == third)

	big := int64(1)<<53 + 1
	fmt.Println("int64(2^53+1) > twoTo53:", big > twoTo53)
	fmt.Println("int64(2^53+1) != twoTo53:", big != twoTo53)

	ubig := uint64(1)<<53 + 1
	fmt.Println("uint64(2^53+1) > twoTo53:", ubig > twoTo53)

	n := 1<<53 + 1
	fmt.Println("int(2^53+1) >= twoTo53+1:", n >= twoTo53+1)

	// Controls: arithmetic beside the constant already computes at the operand's type, and a literal is
	// not a named constant.
	fmt.Println("float32 arithmetic:", f*tenth)
	fmt.Println("float32 vs literal:", f <= 0.1)
	fmt.Println("float64 vs constant:", float64(f) <= tenth)
}
