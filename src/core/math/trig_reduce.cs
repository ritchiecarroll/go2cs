// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bits = math.bits_package;
using math;

partial class math_package {

// reduceThreshold is the maximum value of x where the reduction using Pi/4
// in 3 float64 parts still gives accurate results. This threshold
// is set by y*C being representable as a float64 without error
// where y is given by y = floor(x * (4 / Pi)) and C is the leading partial
// terms of 4/Pi. Since the leading terms (PI4A and PI4B in sin.go) have 30
// and 32 trailing zero bits, y should have less than 30 significant bits.
//
//	y < 1<<30  -> floor(x*4/Pi) < 1<<30 -> x < (1<<30 - 1) * Pi/4
//
// So, conservatively we can take x < 1<<29.
// Above this threshold Payne-Hanek range reduction must be used.
internal static UntypedInt reduceThreshold => /* 1 << 29 */ 536870912;

// trigReduce implements Payne-Hanek range reduction by Pi/4
// for x > 0. It returns the integer part mod 8 (j) and
// the fractional part (z) of x / (Pi/4).
// The implementation is based on:
// "ARGUMENT REDUCTION FOR HUGE ARGUMENTS: Good to the Last Bit"
// K. C. Ng et al, March 24, 1992
// The simulated multi-precision calculation of x*B uses 64-bit integer arithmetic.
internal static (uint64 j, float64 z) trigReduce(float64 x) {
    uint64 j = default!;
    float64 z = default!;

    const float64 PI4 = /* Pi / 4 */ 0.7853981633974483;
    if (x < PI4) {
        return (0, x);
    }
    // Extract out the integer and exponent such that,
    // x = ix * 2 ** exp.
    var ix = Float64bits(x);
    nint exp = (nint)(ix >> (int)(shift) & (uint64)mask) - (nint)bias - (nint)shift;
    ix &= unchecked((uint64)~(uint64)(((uint64)mask << (int)(shift))));
    ix |= (uint64)(((uint64)1 << (int)(shift)));
    // Use the exponent to extract the 3 appropriate uint64 digits from mPi4,
    // B ~ (z0, z1, z2), such that the product leading digit has the exponent -61.
    // Note, exp >= -53 since x >= PI4 and exp < 971 for maximum float64.
    nuint digit = (nuint)(exp + 61) / 64;
    nuint bitshift = (nuint)(exp + 61) % 64;
    var z0 = (mPi4[digit].Lsh(bitshift)) | (mPi4[digit + 1].Rsh((64 - bitshift)));
    var z1 = (mPi4[digit + 1].Lsh(bitshift)) | (mPi4[digit + 2].Rsh((64 - bitshift)));
    var z2 = (mPi4[digit + 2].Lsh(bitshift)) | (mPi4[digit + 3].Rsh((64 - bitshift)));
    // Multiply mantissa by the digits and extract the upper two digits (hi, lo).
    var (z2hi, _) = bits.Mul64(z2, ix);
    var (z1hi, z1lo) = bits.Mul64(z1, ix);
    var z0lo = z0 * ix;
    var (lo, c) = bits.Add64(z1lo, z2hi, 0);
    var (hi, _) = bits.Add64(z0lo, z1hi, c);
    // The top 3 bits are j.
    j = hi >> 61;
    // Extract the fraction and find its magnitude.
    hi = hi << 3 | lo >> 61;
    nuint lz = (nuint)bits.LeadingZeros64(hi);
    var e = (uint64)((nuint)bias - (lz + 1));
    // Clear implicit mantissa bit and shift into place.
    hi = (hi.Lsh((lz + 1))) | (lo.Rsh((64 - (lz + 1))));
    hi >>= (int)(64 - shift);
    // Include the exponent and convert to a float.
    hi |= (uint64)((e << (int)(shift)));
    z = Float64frombits(hi);
    // Map zeros to origin.
    if ((j & 1) == 1) {
        j++;
        j &= (uint64)(7);
        z--;
    }
    // Multiply the fractional part by pi/4.
    return (j, z * PI4);
}

// mPi4 is the binary digits of 4/pi as a uint64 array,
// that is, 4/pi = Sum mPi4[i]*2^(-64*i)
// 19 64-bit digits and the leading one bit give 1217 bits
// of precision to handle the largest possible float64 exponent.
internal static array<uint64> mPi4 = new uint64[]{
    0x0000000000000001,
    0x45f306dc9c882a53UL,
    0xf84eafa3ea69bb81UL,
    0xb6c52b3278872083UL,
    0xfca2c757bd778ac3UL,
    0x6e48dc74849ba5c0UL,
    0x0c925dd413a32439UL,
    0xfc3bd63962534e7dUL,
    0xd1046bea5d768909UL,
    0xd338e04d68befc82UL,
    0x7323ac7306a673e9UL,
    0x3908bf177bf25076UL,
    0x3ff12fffbc0b301fUL,
    0xde5e2316b414da3eUL,
    0xda6cfd9e4f96136eUL,
    0x9e8c7ecd3cbfd45aUL,
    0xea4f758fd7cbe2f6UL,
    0x7a0e73ef14a525d4UL,
    0xd7f6bf623f1aba10UL,
    0xac06608df8f6d757UL
}.array();

} // end math_package
