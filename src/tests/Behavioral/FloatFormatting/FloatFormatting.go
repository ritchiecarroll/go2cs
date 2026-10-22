// Guards Go-style float rendering through the converted fmt: the %v/Println default is %g at the
// shortest precision that round-trips, which selects exponent form from the decimal exponent
// alone and writes that exponent in lower case padded to a minimum of two digits.
//
// A former golib fmt stub handed floats to .NET's ToString(), whose thresholds and casing differ: it
// printed 1e+10 as "10000000000", 1.610612736e+09 as "1610612736" and, where it did reach
// exponent form, spelled it "9.223372036854776E+18". Precision was honored for %f only, so a
// bare %f dropped Go's six-digit default and %e/%g were left to .NET entirely.
package main

import "fmt"

func main() {
	// The original repro: q is built by runtime multiplication, so no constant folding is
	// involved — this is purely a formatting concern.
	q := 1.0
	for i := 0; i < 10; i++ {
		q = q * 10.0
	}
	fmt.Println(q)

	// %g switches to exponent form once the decimal exponent reaches 6 — the shortest form's
	// stand-in precision — and stays fixed-point below it, however many integer digits that
	// leaves. Below the decimal point it switches at an exponent under -4.
	fmt.Println(0.0001, 0.00001)
	fmt.Println(100000.0, 1000000.0)
	fmt.Println(999999.0, 1000001.0)
	fmt.Println(123456.0, 1234567.0)
	fmt.Println(1.610612736e9)
	fmt.Println(1e20, 1e21, 1e22)
	fmt.Println(1e100, 1e-100)

	// The extremes of the float64 range, including the smallest subnormal
	fmt.Println(1.7976931348623157e+308, 5e-324)

	// Exponent form is not reserved for round numbers
	fmt.Println(4.611686018427388e+18, 9.223372036854776e+18)
	fmt.Println(123456789.0, 0.000123456789)

	// Signs, zeroes and the non-finite values. Go has no negative zero constant — constant
	// arithmetic is exact — so the sign has to be applied at run time.
	zero := 0.0
	negZero := -zero
	posInf := 1.0 / zero
	negInf := -1.0 / zero
	nan := zero / zero
	fmt.Println(0.0, negZero, -1e10, -0.0001)
	fmt.Println(posInf, negInf, nan)

	// Values whose shortest form is unremarkable must stay unremarkable
	fmt.Println(3.14, 2.5, 0.1, 100.0, -7.5)

	// The shortest form that round-trips depends on the width of the value: a float32 needs
	// fewer digits than the float64 it widens to
	var f32 float32 = 0.1
	fmt.Println(f32, float64(f32))
	fmt.Println(float32(1e10), float32(16777216), float32(1e-7))
	fmt.Println(float32(3.4028235e+38), float32(3.14))

	// Verbs at their default precision: %e and %f default to six digits, %g to the shortest
	// form. %E/%G case the exponent up, %F is %f.
	v := 1234.5678
	fmt.Printf("[%e] [%E] [%f] [%F] [%g] [%G]\n", v, v, v, v, v, v)
	fmt.Printf("[%e] [%f] [%g]\n", 0.0, 0.0, 0.0)
	fmt.Printf("[%e] [%g] [%G]\n", 1e10, 1e10, 1e10)

	// Explicit precision overrules the default, and the exponent keeps its minimum width
	fmt.Printf("[%.3e] [%.3E] [%.3f] [%.3g] [%.3G]\n", v, v, v, v, v)
	fmt.Printf("[%.0e] [%.0f] [%.0g] [%.1g]\n", v, v, v, v)
	fmt.Printf("[%.4g] [%.8g] [%.10g]\n", v, v, v)
	fmt.Printf("[%e] [%.2e] [%.2e]\n", 1e-7, 1e-7, 1e100)

	// %g trims the trailing zeros an explicit precision would otherwise pad
	fmt.Printf("[%.5g] [%.5g] [%.5g]\n", 1.5, 100000.0, 1e10)

	// Precision beyond the shortest form exposes the exact binary value, so the digits must
	// come from an exact expansion rather than a round-tripped approximation
	fmt.Printf("[%.17g] [%.20g] [%.20e]\n", 0.1, 0.1, 0.1)
	fmt.Printf("[%.20f] [%.30f]\n", 0.1, 0.1)

	// Rounding is half-to-even on the exact value: 2.5 and 3.5 both round to 4's neighbour,
	// while 2.675 is really 2.67499... and rounds down
	fmt.Printf("[%.0f] [%.0f] [%.0f] [%.0f]\n", 0.5, 1.5, 2.5, 3.5)
	fmt.Printf("[%.2f] [%.1f] [%.0e]\n", 2.675, 0.25, 2.5)

	// Width and flags still apply on top of the float verbs
	fmt.Printf("[%12.3e] [%-12.3e] [%012.3e]\n", v, v, v)
	fmt.Printf("[%+.2f] [% .2f] [%+g] [% g]\n", v, v, v, v)
	fmt.Printf("[%8.2f] [%-8.2f] [%08.2f] [%08.2f]\n", v, v, v, -v)

	// Sprint/Sprintf render identically to Println
	fmt.Println(fmt.Sprint(1e10), fmt.Sprintf("%v", 1e10), fmt.Sprintf("%g", 1e10))
}
