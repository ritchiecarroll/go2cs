package main

import "fmt"

// Go zeroes a shift whose count reaches the operand width (a signed right shift sign-extends); C#'s
// native shift MASKS the count. Runtime counts come from a slice so nothing folds to a constant, and
// several are >= width. The converter must route these through golib's Go-semantics Rsh/Lsh guard,
// while a syntactically MASKED / MODULO'd count (provably < width) must stay the native shift.

// A NAMED integer type takes a different route entirely: its shift resolves through the go2cs-gen
// wrapper operator, not through the converter's guard, so that operator has to carry the same
// semantics. math/big's Word is exactly this shape — lehmerSimulate reads `A.abs[n-2] >> (_W - h)`,
// which for a normalized operand is a shift by exactly the width. Masked, it returns the word
// instead of 0, and the corrupted Lehmer cosequences made GCD's loop stop converging: an infinite
// loop in math/big, reached from crypto/elliptic's generic CurveParams path.
type word uint    // wide: underlying uint -> C# nuint
type halfword uint32
type signedword int64

func main() {
	c := []uint{0, 1, 63, 64, 65, 200}
	var u uint64 = 0x8000000000000001

	// Unsigned 64-bit: count >= 64 -> 0 (both directions).
	for _, k := range c {
		fmt.Println(u>>k, u<<k)
	}

	// Signed right shift sign-extends when count >= width.
	var neg int64 = -8
	var pos int64 = 1 << 40
	fmt.Println(neg>>c[3], neg>>c[5], pos>>c[3]) // -8>>64=-1, -8>>200=-1, (1<<40)>>64=0

	// 32-bit width threshold.
	var u32 uint32 = 0xDEADBEEF
	for _, k := range []uint{31, 32, 40} {
		fmt.Println(u32 >> k)
	}

	// R2 mask / R3 modulo: provably < width, must stay NATIVE and correct.
	fmt.Println(u >> (c[3] & 63)) // 64 & 63 = 0 -> u
	fmt.Println(u >> (c[4] % 64)) // 65 % 64 = 1 -> u >> 1

	// NAMED integer types, shifted by a runtime count that reaches the width. The result stays the
	// named type (a wrapper operator, not the guard), and every count >= width must yield 0 / the
	// sign — the Word case, written the way lehmerSimulate writes it: `w >> (W - h)` with h == 0.
	var w word = 0x8000000000000001
	const W = 64
	h := c[0] // 0 at runtime, so the count below is exactly the width
	fmt.Println(uint(w>>(W-h)), uint(w<<(W-h)))
	for _, k := range c {
		fmt.Println(uint(w>>k), uint(w<<k))
	}

	var hw halfword = 0xDEADBEEF
	for _, k := range []uint{31, 32, 40} {
		fmt.Println(uint32(hw >> k))
	}

	// A named SIGNED type sign-extends on an over-wide right shift, like its basic twin.
	var sw signedword = -8
	var swp signedword = 1 << 40
	fmt.Println(int64(sw>>c[3]), int64(sw>>c[5]), int64(swp>>c[3]))

	// An UNBOUNDED count, not merely one at the width: a count past int32's range reaches the named
	// wrapper widened, never narrowed to `int` (which made `1 << 32` a shift by 0 and `1<<32 + 1` a
	// shift by 1), in the binary and the compound form, and a NEGATIVE signed count raises Go's panic
	// rather than reading as a huge count.
	far := []uint{1 << 32, 1<<32 + 1}
	for _, k := range far {
		fmt.Println(uint32(hw<<k), uint32(hw>>k))
	}
	hc := hw
	hc <<= far[1]
	fmt.Println(uint32(hc))
	fmt.Println(namedNegativeShift(hw, -1))
}

func namedNegativeShift(x halfword, n int) (msg string) {
	defer func() {
		if e := recover(); e != nil {
			msg = fmt.Sprint(e)
		}
	}()
	return fmt.Sprint(uint32(x << n))
}
