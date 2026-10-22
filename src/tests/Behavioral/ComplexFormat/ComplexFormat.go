package main

import (
	"fmt"
	"math"
)

// Guards Go-style complex rendering through the converted fmt: complex128 (System.Numerics.Complex)
// and complex64 (go.complex64) must print as "(2+3i)" — imaginary part always signed — not the
// .NET ToString() forms "<2; 3>" / "(2, 3)".
//
// Also guards imaginary-literal MANTISSA emission (imaginary_lit = (decimals | int_lit |
// float_lit) "i"): a non-decimal mantissa must re-render as decimal in C# — appending the F/D
// suffix to a hex mantissa is a silent value change (0xabc + D lexes as the hex integer 43981,
// not 2748.0) and the other Go-only forms do not lex as C# at all — while decimal forms
// (including the legacy leading-zero 0123i, which Go reads as DECIMAL 123i) keep their source
// text.
func main() {
	var c128 complex128 = 2 + 3i
	var neg complex128 = 2 - 3i
	var frac complex128 = complex(2.5, -1.25)
	var realOnly complex128 = complex(5, 0)
	var imagOnly complex128 = 3i
	var c64 complex64 = complex(2.5, -3.5)
	var c64pos complex64 = 1 + 2i
	inf := complex(math.Inf(1), math.Inf(-1))
	nan := complex(math.NaN(), math.NaN())

	fmt.Println(c128)
	fmt.Println(neg)
	fmt.Println(frac)
	fmt.Println(realOnly)
	fmt.Println(imagOnly)
	fmt.Println(c64)
	fmt.Println(c64pos)
	fmt.Println(inf)
	fmt.Println(nan)
	fmt.Println(2 + 3i)
	fmt.Printf("%v;%v\n", c128, c64)
	fmt.Println("sprint:", fmt.Sprint(neg))

	// Exotic imaginary-literal mantissas: non-decimal forms re-render as decimal in C#.
	fmt.Println(0xabci)  // hex int mantissa = 2748i
	fmt.Println(0o123i)  // octal int mantissa = 83i
	fmt.Println(0b101i)  // binary int mantissa = 5i
	fmt.Println(0123i)   // legacy leading zero: DECIMAL 123i, not octal
	fmt.Println(0.i)     // trailing-dot float mantissa = 0i
	fmt.Println(1.e2i)   // dot-before-exponent float mantissa = 100i
	fmt.Println(0x1p-2i) // hex float mantissa = 0.25i

	// Decimal mantissa forms keep their source text (valid C# once suffixed).
	fmt.Println(.25i)
	fmt.Println(2.5e-3i)
	fmt.Println(1_000i)

	var c64hex complex64 = 0x10i // normalized mantissa through the F (complex64) overload
	fmt.Println(c64hex)
}
