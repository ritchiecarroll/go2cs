// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class strconv_package {

// FormatComplex converts the complex number c to a string of the
// form (a+bi) where a and b are the real and imaginary parts,
// formatted according to the format fmt and precision prec.
//
// The format fmt and precision prec have the same meaning as in [FormatFloat].
// It rounds the result assuming that the original was obtained from a complex
// value of bitSize bits, which must be 64 for complex64 and 128 for complex128.
public static @string FormatComplex(complex128 c, byte fmt, nint prec, nint bitSize) {
    array<byte> buf = new(64);
    return (@string)AppendComplex(buf[..0], c, fmt, prec, bitSize);
}

// AppendComplex appends the result of FormatComplex to dst.
// It is here for the runtime.
// There is no public strconv.AppendComplex.
public static slice<byte> AppendComplex(slice<byte> dst, complex128 c, byte fmt, nint prec, nint bitSize) {
    if (bitSize != 64 && bitSize != 128) {
        throw panic("invalid bitSize");
    }
    bitSize >>= 1; // complex64 uses float32 internally

    dst = append(dst, (byte)'(');
    dst = AppendFloat(dst, real(c), fmt, prec, bitSize);
    nint i = len(dst);
    dst = AppendFloat(dst, imag(c), fmt, prec, bitSize);
    // Check if imaginary part has a sign. If not, add one.
    if (dst[i] != '+' && dst[i] != '-') {
        dst = append(dst, (byte)0);
        copy(dst.slice(i + 1), dst.slice(i));
        dst[i] = (byte)'+';
    }
    dst = append(dst, ((@string)"i)"u8).ꓸꓸꓸ);
    return dst;
}

} // end strconv_package
