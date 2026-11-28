// Guards a named-numeric type used as the type argument of a generic constrained by a
// `~integer` type set whose body applies modulus / bitwise / shift operators — the shape of
// internal/trace's `dataTable[EI ~uint64, E]` instantiated with `type stringID uint64`. The
// converter lifts the `~uint64` constraint to IModulus/IBitwise/IShiftOperators; the
// [GoType num:] wrapper for the named type must DECLARE those interfaces (its operators already
// existed) or the instantiation fails CS0315. Kept minimal (a generic func, no maps/generic
// methods) so it isolates the operator-constraint satisfaction.
//
// The second half converts THROUGH such a type parameter: `T(x)`, `T(f)`, `uint64(v)`, `float64(v)` and
// `float32(v)`, each instantiated with a named type. golib's ConvertToType and its siblings reach a
// named numeric type through the IGoNumericWrapper<T> members go2cs-gen gives the wrapper, which
// convert as the underlying kind does: Go truncates, so `small(300)` is 44 and `stringID(1<<64-1)`
// keeps every bit. The reflective probe those members replaced converted with System.Convert, which
// is CHECKED: each out-of-range row threw OverflowException, and a named uintptr had no conversion.
package main

import (
	"fmt"
	"math"
)

type stringID uint64 // named unsigned
type offset int32    // named signed (exercises the signed shift/complement path)
type small uint8     // named narrow unsigned: a conversion into it truncates
type handle uintptr  // named uintptr
type celsius float64 // named float

// mix applies every operator the `~integer` constraint lifts: modulus, and (all in
// GetComplementOperator) complement, both shifts, and bitwise and/or/xor — on the type param.
func mix[K ~uint64 | ~int32](id K, step K) K {
	h := id
	h = h ^ (h >> 2)    // shift + xor
	h = h | (step << 1) // shift + or
	h = h & step        // bitwise-and
	h = h % (step + 7)  // modulus
	return h
}

func conv[T ~uint8 | ~uintptr | ~int32 | ~uint64](x uint64) T { return T(x) }

func convSigned[T ~uint8 | ~int32](x int64) T { return T(x) }

func widen[T ~uint8 | ~uintptr | ~int32 | ~uint64](v T) uint64 { return uint64(v) }

func asFloat64[T ~int32 | ~float64](v T) float64 { return float64(v) }

func asFloat32[T ~int32 | ~float64](v T) float32 { return float32(v) }

func fromFloat[T ~int32 | ~uint8](f float64) T { return T(f) }

func main() {
	var a stringID = 12345
	var b stringID = 11
	fmt.Println(mix(a, b)) // unsigned named uint64 type argument

	var x offset = -8
	var y offset = 5
	fmt.Println(mix(x, y)) // signed named int32 type argument

	// bare instantiations to force the constraint check on both named types
	fmt.Println(mix(stringID(255), stringID(3)), mix(offset(100), offset(9)))

	// T(x) of an integer into a named type argument: truncation, never a panic
	fmt.Println(conv[small](300), conv[offset](1<<32+5), conv[stringID](math.MaxUint64), conv[handle](1<<40))
	fmt.Println(convSigned[small](-1), convSigned[offset](-7), convSigned[small](-256))

	// uint64(v) of a named type argument: sign-extended, or every bit of an unsigned
	fmt.Println(widen(small(200)), widen(offset(-1)), widen(stringID(math.MaxUint64)), widen(handle(7)))

	// float64(v) and float32(v) of a named type argument, and T(f) of a float into one
	fmt.Println(asFloat64(celsius(21.5)), asFloat64(offset(-3)), asFloat32(celsius(0.1)), asFloat32(offset(16777217)))
	fmt.Println(fromFloat[offset](-7.9), fromFloat[small](200.7), fromFloat[offset](2147483647.4))
}
