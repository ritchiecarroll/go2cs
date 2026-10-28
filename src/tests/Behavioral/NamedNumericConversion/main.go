package main

import "fmt"

// Conversions between a NAMED numeric type and a basic numeric type. The [GoType] wrapper converts
// implicitly between the named type and its EXACT underlying, and explicitly to every basic numeric:
//   - TO a named numeric (`traceArg(procs)`, `arenaIdx(x)`): a plain `(traceArg)int32Expr` is CS0030,
//     so the converter coerces through the underlying first — `((traceArg)(uint64)procs)`.
//   - FROM a named numeric to any basic (`uint64(nameOff)`, `int(idx)`): one cast,
//     `((uint64)nameOff)`, through the wrapper's explicit operator to every basic
//     numeric type.
// When the basic IS already the named type's underlying, no extra cast is added.

type traceArg uint64 // underlying uint64
type arenaIdx uint   // underlying uint (C# nuint)
type nameOff int32   // underlying int32
type idx uint        // underlying uint (C# nuint)

func main() {
	var procs int32 = 5
	a := traceArg(procs) // int32 -> traceArg: ((traceArg)(uint64)procs)
	fmt.Println(uint64(a))

	var x int = 1 << 4
	b := arenaIdx(x) // int -> arenaIdx: ((arenaIdx)(nuint)x)
	fmt.Println(uint(b))

	// A named-to-named conversion (different underlying widths).
	var g traceGoStatus = 3
	c := traceArg(g) // traceGoStatus(uint8) -> traceArg(uint64)
	fmt.Println(uint64(c))

	// Argument already the underlying basic — no extra cast, still works.
	var u uint64 = 9
	d := traceArg(u)
	fmt.Println(uint64(d))

	// FROM a named numeric to a DIFFERENT basic numeric (the mirror direction).
	var s nameOff = 7
	e := uint64(s) // nameOff(int32) -> uint64: ((uint64)s)
	fmt.Println(e)

	var i idx = 9
	f := int(i) // idx(nuint) -> int: ((nint)i)
	fmt.Println(f)

	// FROM a named numeric to its EXACT underlying basic — no extra cast.
	var s2 nameOff = 13
	h := int32(s2) // nameOff(int32) -> int32: ((int32)s2)
	fmt.Println(h)
}

type traceGoStatus uint8
