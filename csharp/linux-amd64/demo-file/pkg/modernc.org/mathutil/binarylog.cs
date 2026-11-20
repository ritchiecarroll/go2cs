// Copyright (c) 2016 The mathutil Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using big = go.math.big_package;
using go.math;
using static mathutil_package;

partial class mathutil_package {

partial struct @float {
    internal ж<big.Int> n;
    internal nint fracBits;
    internal nint maxFracBits;
}

internal static @float newFloat(ж<big.Int> n, nint fracBits, nint maxFracBits) {
    var f = new @float(n: n, fracBits: fracBits, maxFracBits: maxFracBits);
    f.normalize();
    return f;
}

} partial class mathutilᴛfloat {

internal static void normalize(this ref @float f) {
    nint n = f.n.BitLen();
    if (n == 0) {
        return;
    }

    nint nΔ1 = f.fracBits - f.maxFracBits; if (nΔ1 > 0) {
        nuint bit = f.n.Bit(nΔ1 - 1);
        f.n.Rsh(f.n, (nuint)nΔ1);
        if (bit != 0) {
            f.n.Add(f.n, _1);
        }
        f.fracBits -= nΔ1;
    }

    nint i = default;
    for (; f.fracBits > 0 && i <= f.fracBits && f.n.Bit(i) == 0; i++) {
        f.fracBits--;
    }

    if (i != 0) {
        f.n.Rsh(f.n, (nuint)i);
    }
}

internal static bool eq1(this ref @float f) { return f.fracBits == 0 && f.n.BitLen() == 1; }
internal static bool ge2(this ref @float f) { return f.n.BitLen() > f.fracBits + 1; }

internal static void div2(this ref @float f) {
    f.fracBits++;
    f.normalize();
}

} partial class mathutil_package {

public static (nint characteristic, ж<big.Int> mantissa) BinaryLog(ж<big.Int> Ꮡn, nint mantissaBits) {
    nint characteristic = default;
    ж<big.Int> mantissa = default;
    ref var n = ref Ꮡn.DerefOrNull();

    if (n.Sign() <= 0 || mantissaBits < 0) {
        throw panic("invalid argument of BinaryLog");
    }

    characteristic = n.BitLen() - 1;
    mantissa = big.NewInt(0);
    var x = newFloat(Ꮡn, characteristic, mantissaBits);
    for (; mantissaBits != 0 && !x.eq1(); mantissaBits--) {
        x.sqr();
        mantissa.Lsh(mantissa, 1);
        if (x.ge2()) {
            mantissa.SetBit(mantissa, 0, 1);
            x.div2();
        }
    }
    return (characteristic, mantissa);
}

} // end mathutil_package
