// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

// This file implements the Eisel-Lemire ParseFloat algorithm, published in
// 2020 and discussed extensively at
// https://nigeltao.github.io/blog/2020/eisel-lemire.html
//
// The original C++ implementation is at
// https://github.com/lemire/fast_double_parser/blob/644bef4306059d3be01a04e77d3cc84b379c596f/include/fast_double_parser.h#L840
//
// This Go re-implementation closely follows the C re-implementation at
// https://github.com/google/wuffs/blob/ba3818cb6b473a2ed0b38ecfc07dbbd3a97e8ae7/internal/cgen/base/floatconv-submodule-code.c#L990
//
// Additional testing (on over several million test strings) is done by
// https://github.com/nigeltao/parse-number-fxx-test-data/blob/5280dcfccf6d0b02a65ae282dad0b6d9de50e039/script/test-go-strconv.go
using bits = math.bits_package;
using math;

partial class strconv_package {

internal static (float64 f, bool ok) eiselLemire64(uint64 man, nint exp10, bool neg) {
    float64 f = default!;
    bool ok = default!;

    // The terse comments in this function body refer to sections of the
    // https://nigeltao.github.io/blog/2020/eisel-lemire.html blog post.
    // Exp10 Range.
    if (man == 0) {
        if (neg) {
            f = float64frombits(0x8000000000000000UL); // Negative zero.
        }
        return (f, true);
    }
    (var pow, var exp2, ok) = pow10(exp10);
    if (!ok) {
        return (0D, false);
    }

    // Normalization.
    nint clz = bits.LeadingZeros64(man);
    man.LshAssign((nuint)clz);
    var retExp2 = (uint64)(exp2 + 63 - (nint)float64Bias) - (uint64)clz;

    // Multiplication.
    var (xHi, xLo) = bits.Mul64(man, pow.Hi);

    // Wider Approximation.
    if ((xHi & 0x1FF) == 0x1FF && xLo + man < man) {
        var (yHi, yLo) = bits.Mul64(man, pow.Lo);
        var (mergedHi, mergedLo) = (xHi, xLo + yHi);
        if (mergedLo < xLo) {
            mergedHi++;
        }
        if ((mergedHi & 0x1FF) == 0x1FF && mergedLo + 1 == 0 && yLo + man < man) {
            return (0D, false);
        }
        (xHi, xLo) = (mergedHi, mergedLo);
    }

    // Shifting to 54 Bits.
    var msb = xHi >> 63;
    var retMantissa = xHi.Rsh((msb + 9));
    retExp2 -= (1 ^ msb);

    // Half-way Ambiguity.
    if (xLo == 0 && (xHi & 0x1FF) == 0 && (retMantissa & 3) == 1) {
        return (0D, false);
    }

    // From 54 to 53 Bits.
    retMantissa += (retMantissa & 1);
    retMantissa >>= (int)(1);
    if (retMantissa >> 53 > 0) {
        retMantissa >>= (int)(1);
        retExp2 += 1;
    }
    // retExp2 is a uint64. Zero or underflow means that we're in subnormal
    // float64 space. 0x7FF or above means that we're in Inf/NaN float64 space.
    //
    // The if block is equivalent to (but has fewer branches than):
    //   if retExp2 <= 0 || retExp2 >= 0x7FF { etc }
    if (retExp2 - 1 >= 0x7FF - 1) {
        return (0D, false);
    }
    var retBits = retExp2 << (int)(float64MantBits) | retMantissa & (uint64)((uint64)(4503599627370496L - 1));
    if (neg) {
        retBits |= (uint64)(0x8000000000000000UL);
    }
    return (float64frombits(retBits), true);
}

internal static (float32 f, bool ok) eiselLemire32(uint64 man, nint exp10, bool neg) {
    float32 f = default!;
    bool ok = default!;

    // The terse comments in this function body refer to sections of the
    // https://nigeltao.github.io/blog/2020/eisel-lemire.html blog post.
    //
    // That blog post discusses the float64 flavor (11 exponent bits with a
    // -1023 bias, 52 mantissa bits) of the algorithm, but the same approach
    // applies to the float32 flavor (8 exponent bits with a -127 bias, 23
    // mantissa bits). The computation here happens with 64-bit values (e.g.
    // man, xHi, retMantissa) before finally converting to a 32-bit float.
    // Exp10 Range.
    if (man == 0) {
        if (neg) {
            f = float32frombits(0x80000000U); // Negative zero.
        }
        return (f, true);
    }
    (var pow, var exp2, ok) = pow10(exp10);
    if (!ok) {
        return (0F, false);
    }

    // Normalization.
    nint clz = bits.LeadingZeros64(man);
    man.LshAssign((nuint)clz);
    var retExp2 = (uint64)(exp2 + 63 - (nint)float32Bias) - (uint64)clz;

    // Multiplication.
    var (xHi, xLo) = bits.Mul64(man, pow.Hi);

    // Wider Approximation.
    if ((xHi & 0x3FFFFFFFFFUL) == 0x3FFFFFFFFFUL && xLo + man < man) {
        var (yHi, yLo) = bits.Mul64(man, pow.Lo);
        var (mergedHi, mergedLo) = (xHi, xLo + yHi);
        if (mergedLo < xLo) {
            mergedHi++;
        }
        if ((mergedHi & 0x3FFFFFFFFFUL) == 0x3FFFFFFFFFUL && mergedLo + 1 == 0 && yLo + man < man) {
            return (0F, false);
        }
        (xHi, xLo) = (mergedHi, mergedLo);
    }

    // Shifting to 54 Bits (and for float32, it's shifting to 25 bits).
    var msb = xHi >> 63;
    var retMantissa = xHi.Rsh((msb + 38));
    retExp2 -= (1 ^ msb);

    // Half-way Ambiguity.
    if (xLo == 0 && (xHi & 0x3FFFFFFFFFUL) == 0 && (retMantissa & 3) == 1) {
        return (0F, false);
    }

    // From 54 to 53 Bits (and for float32, it's from 25 to 24 bits).
    retMantissa += (retMantissa & 1);
    retMantissa >>= (int)(1);
    if (retMantissa >> 24 > 0) {
        retMantissa >>= (int)(1);
        retExp2 += 1;
    }
    // retExp2 is a uint64. Zero or underflow means that we're in subnormal
    // float32 space. 0xFF or above means that we're in Inf/NaN float32 space.
    //
    // The if block is equivalent to (but has fewer branches than):
    //   if retExp2 <= 0 || retExp2 >= 0xFF { etc }
    if (retExp2 - 1 >= 0xFF - 1) {
        return (0F, false);
    }
    var retBits = retExp2 << (int)(float32MantBits) | retMantissa & (uint64)((1 << (int)(float32MantBits)) - 1);
    if (neg) {
        retBits |= (uint64)(0x80000000U);
    }
    return (float32frombits((uint32)retBits), true);
}

} // end strconv_package
