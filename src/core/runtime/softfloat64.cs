// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Software IEEE754 64-bit floating point.
// Only referred to (and thus linked in) by softfloat targets
// and by tests in this directory.
namespace go;

partial class runtime_package {

internal const nuint mantbits64 = 52;
internal const nuint expbits64 = 11;
internal static UntypedInt bias64 => /* -1<<(expbits64-1) + 1 */ -1023;
internal const uint64 nan64 = /* (1<<expbits64-1)<<mantbits64 + 1<<(mantbits64-1) */ 9221120237041090560;                                            // quiet NaN, 0 payload
internal const uint64 inf64 = /* (1<<expbits64 - 1) << mantbits64 */ 9218868437227405312;
internal const uint64 neg64 = /* 1 << (expbits64 + mantbits64) */ 9223372036854775808;
internal const nuint mantbits32 = 23;
internal const nuint expbits32 = 8;
internal static UntypedInt bias32 => /* -1<<(expbits32-1) + 1 */ -127;
internal const uint32 nan32 = /* (1<<expbits32-1)<<mantbits32 + 1<<(mantbits32-1) */ 2143289344;                                            // quiet NaN, 0 payload
internal const uint32 inf32 = /* (1<<expbits32 - 1) << mantbits32 */ 2139095040;
internal const uint32 neg32 = /* 1 << (expbits32 + mantbits32) */ 2147483648;

internal static (uint64 sign, uint64 mant, nint exp, bool inf, bool nan) funpack64(uint64 f) {
    uint64 sign = default!;
    uint64 mant = default!;
    nint exp = default!;
    bool inf = default!;
    bool nan = default!;

    sign = (uint64)(f & (((uint64)1 << (int)((mantbits64 + expbits64)))));
    mant = (uint64)(f & ((uint64)(4503599627370496L - 1)));
    exp = (nint)((nint)((f >> (int)(mantbits64))) & (nint)((1 << (int)(expbits64)) - 1));
    var exprᴛ1 = exp;
    if (exprᴛ1 == (1 << (int)(expbits64)) - 1) {
        if (mant != 0) {
            nan = true;
            return (sign, mant, exp, inf, nan);
        }
        inf = true;
        return (sign, mant, exp, inf, nan);
    }
    if (exprᴛ1 is 0) {
        if (mant != 0) {
            // denormalized
            exp += bias64 + 1;
            while (mant < ((uint64)1 << (int)(mantbits64))) {
                mant <<= (int)(1);
                exp--;
            }
        }
    }
    else { /* default: */
        mant |= (uint64)(((uint64)1 << (int)(mantbits64)));
        exp += bias64;
    }

    // add implicit top bit
    return (sign, mant, exp, inf, nan);
}

internal static (uint32 sign, uint32 mant, nint exp, bool inf, bool nan) funpack32(uint32 f) {
    uint32 sign = default!;
    uint32 mant = default!;
    nint exp = default!;
    bool inf = default!;
    bool nan = default!;

    sign = (uint32)(f & (((uint32)1 << (int)((mantbits32 + expbits32)))));
    mant = (uint32)(f & ((1 << (int)(mantbits32)) - 1));
    exp = (nint)((nint)((f >> (int)(mantbits32))) & (nint)((1 << (int)(expbits32)) - 1));
    var exprᴛ1 = exp;
    if (exprᴛ1 == (1 << (int)(expbits32)) - 1) {
        if (mant != 0) {
            nan = true;
            return (sign, mant, exp, inf, nan);
        }
        inf = true;
        return (sign, mant, exp, inf, nan);
    }
    if (exprᴛ1 is 0) {
        if (mant != 0) {
            // denormalized
            exp += bias32 + 1;
            while (mant < ((uint32)1 << (int)(mantbits32))) {
                mant <<= (int)(1);
                exp--;
            }
        }
    }
    else { /* default: */
        mant |= (uint32)(((uint32)1 << (int)(mantbits32)));
        exp += bias32;
    }

    // add implicit top bit
    return (sign, mant, exp, inf, nan);
}

internal static uint64 fpack64(uint64 sign, uint64 mant, nint exp, uint64 trunc) {
    var mant0 = mant;
    nint exp0 = exp;
    var trunc0 = trunc;
    if (mant == 0) {
        return sign;
    }
    while (mant < ((uint64)1 << (int)(mantbits64))) {
        mant <<= (int)(1);
        exp--;
    }
    while (mant >= ((uint64)4 << (int)(mantbits64))) {
        trunc |= (uint64)((uint64)(mant & 1));
        mant >>= (int)(1);
        exp++;
    }
    if (mant >= ((uint64)2 << (int)(mantbits64))) {
        if ((uint64)(mant & 1) != 0 && (trunc != 0 || (uint64)(mant & 2) != 0)) {
            mant++;
            if (mant >= ((uint64)4 << (int)(mantbits64))) {
                mant >>= (int)(1);
                exp++;
            }
        }
        mant >>= (int)(1);
        exp++;
    }
    if (exp >= (nint)((1 << (int)(expbits64)) - 1 + bias64)) {
        return (uint64)(sign ^ inf64);
    }
    if (exp < (nint)(bias64 + 1)) {
        if (exp < (nint)((nint)bias64 - (nint)mantbits64)) {
            return (uint64)(sign | 0);
        }
        // repeat expecting denormal
        (mant, exp, trunc) = (mant0, exp0, trunc0);
        while (exp < bias64) {
            trunc |= (uint64)((uint64)(mant & 1));
            mant >>= (int)(1);
            exp++;
        }
        if ((uint64)(mant & 1) != 0 && (trunc != 0 || (uint64)(mant & 2) != 0)) {
            mant++;
        }
        mant >>= (int)(1);
        exp++;
        if (mant < ((uint64)1 << (int)(mantbits64))) {
            return (uint64)(sign | mant);
        }
    }
    return (uint64)((uint64)(sign | ((uint64)(exp - (nint)bias64) << (int)(mantbits64))) | (uint64)(mant & ((uint64)(4503599627370496L - 1))));
}

internal static uint32 fpack32(uint32 sign, uint32 mant, nint exp, uint32 trunc) {
    var mant0 = mant;
    nint exp0 = exp;
    var trunc0 = trunc;
    if (mant == 0) {
        return sign;
    }
    while (mant < ((uint32)1 << (int)(mantbits32))) {
        mant <<= (int)(1);
        exp--;
    }
    while (mant >= ((uint32)4 << (int)(mantbits32))) {
        trunc |= (uint32)((uint32)(mant & 1));
        mant >>= (int)(1);
        exp++;
    }
    if (mant >= ((uint32)2 << (int)(mantbits32))) {
        if ((uint32)(mant & 1) != 0 && (trunc != 0 || (uint32)(mant & 2) != 0)) {
            mant++;
            if (mant >= ((uint32)4 << (int)(mantbits32))) {
                mant >>= (int)(1);
                exp++;
            }
        }
        mant >>= (int)(1);
        exp++;
    }
    if (exp >= (nint)((1 << (int)(expbits32)) - 1 + bias32)) {
        return (uint32)(sign ^ inf32);
    }
    if (exp < (nint)(bias32 + 1)) {
        if (exp < (nint)((nint)bias32 - (nint)mantbits32)) {
            return (uint32)(sign | 0);
        }
        // repeat expecting denormal
        (mant, exp, trunc) = (mant0, exp0, trunc0);
        while (exp < bias32) {
            trunc |= (uint32)((uint32)(mant & 1));
            mant >>= (int)(1);
            exp++;
        }
        if ((uint32)(mant & 1) != 0 && (trunc != 0 || (uint32)(mant & 2) != 0)) {
            mant++;
        }
        mant >>= (int)(1);
        exp++;
        if (mant < ((uint32)1 << (int)(mantbits32))) {
            return (uint32)(sign | mant);
        }
    }
    return (uint32)((uint32)(sign | ((uint32)(exp - (nint)bias32) << (int)(mantbits32))) | (uint32)(mant & ((1 << (int)(mantbits32)) - 1)));
}

internal static uint64 fadd64(uint64 f, uint64 g) {
    var (fs, fm, fe, fi, fn) = funpack64(f);
    var (gs, gm, ge, gi, gn) = funpack64(g);
    // Special cases.
    switch (ᐧ) {
    case {} when fn || gn: {
        return nan64;
    }
    case {} when fi && gi && fs != gs: {
        return nan64;
    }
    case {} when fi: {
        return f;
    }
    case {} when gi: {
        return g;
    }
    case {} when fm == 0 && gm == 0 && fs != 0 && gs != 0: {
        return f;
    }
    case {} when fm is 0: {
        if (gm == 0) {
            // NaN + x or x + NaN = NaN
            // +Inf + -Inf or -Inf + +Inf = NaN
            // ±Inf + g = ±Inf
            // f + ±Inf = ±Inf
            // -0 + -0 = -0
            // 0 + g = g but 0 + -0 = +0
            g ^= (uint64)(gs);
        }
        return g;
    }
    case {} when gm is 0: {
        return f;
    }}

    // f + 0 = f
    if (fe < ge || fe == ge && fm < gm) {
        (f, g, fs, fm, fe, gs, gm, ge) = (g, f, gs, gm, ge, fs, fm, fe);
    }
    nuint shift = (nuint)(fe - ge);
    fm <<= (int)(2);
    gm <<= (int)(2);
    var trunc = (uint64)(gm & (((uint64)1).Lsh(shift) - 1));
    gm >>= (int)(shift);
    if (fs == gs){
        fm += gm;
    } else {
        fm -= gm;
        if (trunc != 0) {
            fm--;
        }
    }
    if (fm == 0) {
        fs = 0;
    }
    return fpack64(fs, fm, fe - 2, trunc);
}

internal static uint64 fsub64(uint64 f, uint64 g) {
    return fadd64(f, fneg64(g));
}

internal static uint64 fneg64(uint64 f) {
    return (uint64)(f ^ (((uint64)1 << (int)((mantbits64 + expbits64)))));
}

internal static uint64 fmul64(uint64 f, uint64 g) {
    var (fs, fm, fe, fi, fn) = funpack64(f);
    var (gs, gm, ge, gi, gn) = funpack64(g);
    // Special cases.
    switch (ᐧ) {
    case {} when fn || gn: {
        return nan64;
    }
    case {} when fi && gi: {
        return (uint64)(f ^ gs);
    }
    case {} when (fi && gm == 0) || (fm == 0 && gi): {
        return nan64;
    }
    case {} when fm is 0: {
        return (uint64)(f ^ gs);
    }
    case {} when gm is 0: {
        return (uint64)(g ^ fs);
    }}

    // NaN * g or f * NaN = NaN
    // Inf * Inf = Inf (with sign adjusted)
    // 0 * Inf = Inf * 0 = NaN
    // 0 * x = 0 (with sign adjusted)
    // x * 0 = 0 (with sign adjusted)
    // 53-bit * 53-bit = 107- or 108-bit
    var (lo, hi) = mullu(fm, gm);
    nuint shift = mantbits64 - 1;
    var trunc = (uint64)(lo & (((uint64)1).Lsh(shift) - 1));
    var mant = (uint64)(hi.Lsh((64 - shift)) | lo.Rsh(shift));
    return fpack64((uint64)(fs ^ gs), mant, fe + ge - 1, trunc);
}

internal static uint64 fdiv64(uint64 f, uint64 g) {
    var (fs, fm, fe, fi, fn) = funpack64(f);
    var (gs, gm, ge, gi, gn) = funpack64(g);
    // Special cases.
    switch (ᐧ) {
    case {} when fn || gn: {
        return nan64;
    }
    case {} when fi && gi: {
        return nan64;
    }
    case {} when !fi && !gi && fm == 0 && gm == 0: {
        return nan64;
    }
    case {} when (fi) || (!gi && gm == 0): {
        return (uint64)((uint64)(fs ^ gs) ^ inf64);
    }
    case {} when (gi) || (fm == 0): {
        return (uint64)((uint64)(fs ^ gs) ^ 0);
    }}

    // NaN / g = f / NaN = NaN
    // ±Inf / ±Inf = NaN
    // 0 / 0 = NaN
    // Inf / g = f / 0 = Inf
    // f / Inf = 0 / g = Inf
    _ = fi;
    _ = fn;
    _ = gi;
    _ = gn;
    // 53-bit<<54 / 53-bit = 53- or 54-bit.
    nuint shift = mantbits64 + 2;
    var (q, r) = divlu(fm.Rsh((64 - shift)), fm.Lsh(shift), gm);
    return fpack64((uint64)(fs ^ gs), q, fe - ge - 2, r);
}

internal static uint32 f64to32(uint64 f) {
    var (fs, fm, fe, fi, fn) = funpack64(f);
    if (fn) {
        return nan32;
    }
    var fs32 = (uint32)((fs >> (int)(32)));
    if (fi) {
        return (uint32)(fs32 ^ inf32);
    }
    const nuint d = /* mantbits64 - mantbits32 - 1 */ 28;
    return fpack32(fs32, (uint32)((fm >> (int)(d))), fe - 1, (uint32)((uint64)(fm & ((1 << (int)(d)) - 1))));
}

internal static uint64 f32to64(uint32 f) {
    const nuint d = /* mantbits64 - mantbits32 */ 29;
    var (fs, fm, fe, fi, fn) = funpack32(f);
    if (fn) {
        return nan64;
    }
    var fs64 = ((uint64)fs << (int)(32));
    if (fi) {
        return (uint64)(fs64 ^ inf64);
    }
    return fpack64(fs64, ((uint64)fm << (int)(d)), fe, 0);
}

internal static (int32 cmp, bool isnan) fcmp64(uint64 f, uint64 g) {
    var (fs, fm, _, fi, fn) = funpack64(f);
    var (gs, gm, _, gi, gn) = funpack64(g);
    switch (ᐧ) {
    case {} when (fn) || (gn): {
        return (0, true);
    }
    case {} when !fi && !gi && fm == 0 && gm == 0: {
        return (0, false);
    }
    case {} when fs > gs: {
        return (-1, false);
    }
    case {} when fs < gs: {
        return (+1, false);
    }
    case {} when (fs == 0 && f < g) || (fs != 0 && f > g): {
        return (-1, false);
    }
    case {} when (fs == 0 && f > g) || (fs != 0 && f < g): {
        return (+1, false);
    }}

    // flag NaN
    // ±0 == ±0
    // f < 0, g > 0
    // f > 0, g < 0
    // Same sign, not NaN.
    // Can compare encodings directly now.
    // Reverse for sign.
    // f == g
    return (0, false);
}

internal static (int64 val, bool ok) f64toint(uint64 f) {
    int64 val = default!;

    var (fs, fm, fe, fi, fn) = funpack64(f);
    switch (ᐧ) {
    case {} when (fi) || (fn): {
        return (0, false);
    }
    case {} when fe < -1: {
        return (0, false);
    }
    case {} when fe is > 63: {
        if (fs != 0 && fm == 0) {
            // NaN
            // f < 0.5
            // f >= 2^63
            // f == -2^63
            return (-9223372036854775808L, true);
        }
        if (fs != 0) {
            return (0, false);
        }
        return (0, false);
    }}

    while (fe > (nint)mantbits64) {
        fe--;
        fm <<= (int)(1);
    }
    while (fe < (nint)mantbits64) {
        fe++;
        fm >>= (int)(1);
    }
    val = (int64)fm;
    if (fs != 0) {
        val = -val;
    }
    return (val, true);
}

internal static uint64 /*f*/ fintto64(int64 val) {
    var fs = (uint64)((uint64)val & (((uint64)1 << (int)(63))));
    var mant = (uint64)val;
    if (fs != 0) {
        mant = ((uint64)0 - mant);
    }
    return fpack64(fs, mant, (nint)mantbits64, 0);
}

internal static uint32 /*f*/ fintto32(int64 val) {
    var fs = (uint64)((uint64)val & (((uint64)1 << (int)(63))));
    var mant = (uint64)val;
    if (fs != 0) {
        mant = ((uint64)0 - mant);
    }
    // Reduce mantissa size until it fits into a uint32.
    // Keep track of the bits we throw away, and if any are
    // nonzero or them into the lowest bit.
    nint exp = (nint)mantbits32;
    uint32 trunc = default!;
    while (mant >= ((uint64)1 << (int)(32))) {
        trunc |= (uint32)((uint32)((uint32)mant & 1));
        mant >>= (int)(1);
        exp++;
    }
    return fpack32((uint32)((fs >> (int)(32))), (uint32)mant, exp, trunc);
}

// 64x64 -> 128 multiply.
// adapted from hacker's delight.
internal static (uint64 lo, uint64 hi) mullu(uint64 u, uint64 v) {
    UntypedInt s = 32;
    const uint64 mask = /* 1<<s - 1 */ 4294967295;
    var u0 = (uint64)(u & mask);
    var u1 = (u >> (int)(s));
    var v0 = (uint64)(v & mask);
    var v1 = (v >> (int)(s));
    var w0 = u0 * v0;
    var t = u1 * v0 + (w0 >> (int)(s));
    var w1 = (uint64)(t & mask);
    var w2 = (t >> (int)(s));
    w1 += u0 * v1;
    return (u * v, u1 * v1 + w2 + (w1 >> (int)(s)));
}

// 128/64 -> 64 quotient, 64 remainder.
// adapted from hacker's delight
internal static (uint64 q, uint64 r) divlu(uint64 u1, uint64 u0, uint64 v) {
    const uint64 b = /* 1 << 32 */ 4294967296;
    if (u1 >= v) {
        return (18446744073709551615UL, 18446744073709551615UL);
    }
    // s = nlz(v); v <<= s
    nuint s = (nuint)0;
    while ((uint64)(v & (((uint64)1 << (int)(63)))) == 0) {
        s++;
        v <<= (int)(1);
    }
    var vn1 = (v >> (int)(32));
    var vn0 = (uint64)(v & ((uint64)(4294967296L - 1)));
    var un32 = (uint64)(u1.Lsh(s) | u0.Rsh((64 - s)));
    var un10 = u0.Lsh(s);
    var un1 = (un10 >> (int)(32));
    var un0 = (uint64)(un10 & ((uint64)(4294967296L - 1)));
    var q1 = un32 / vn1;
    var rhat = un32 - q1 * vn1;
again1:
    if (q1 >= b || q1 * vn0 > b * rhat + un1) {
        q1--;
        rhat += vn1;
        if (rhat < b) {
            goto again1;
        }
    }
    var un21 = un32 * b + un1 - q1 * v;
    var q0 = un21 / vn1;
    rhat = un21 - q0 * vn1;
again2:
    if (q0 >= b || q0 * vn0 > b * rhat + un0) {
        q0--;
        rhat += vn1;
        if (rhat < b) {
            goto again2;
        }
    }
    return (q1 * b + q0, (un21 * b + un0 - q0 * v).Rsh(s));
}

internal static uint32 fadd32(uint32 x, uint32 y) {
    return f64to32(fadd64(f32to64(x), f32to64(y)));
}

internal static uint32 fmul32(uint32 x, uint32 y) {
    return f64to32(fmul64(f32to64(x), f32to64(y)));
}

internal static uint32 fdiv32(uint32 x, uint32 y) {
    // TODO: are there double-rounding problems here? See issue 48807.
    return f64to32(fdiv64(f32to64(x), f32to64(y)));
}

internal static bool feq32(uint32 x, uint32 y) {
    var (cmp, nan) = fcmp64(f32to64(x), f32to64(y));
    return cmp == 0 && !nan;
}

internal static bool fgt32(uint32 x, uint32 y) {
    var (cmp, nan) = fcmp64(f32to64(x), f32to64(y));
    return cmp >= 1 && !nan;
}

internal static bool fge32(uint32 x, uint32 y) {
    var (cmp, nan) = fcmp64(f32to64(x), f32to64(y));
    return cmp >= 0 && !nan;
}

internal static bool feq64(uint64 x, uint64 y) {
    var (cmp, nan) = fcmp64(x, y);
    return cmp == 0 && !nan;
}

internal static bool fgt64(uint64 x, uint64 y) {
    var (cmp, nan) = fcmp64(x, y);
    return cmp >= 1 && !nan;
}

internal static bool fge64(uint64 x, uint64 y) {
    var (cmp, nan) = fcmp64(x, y);
    return cmp >= 0 && !nan;
}

internal static uint32 fint32to32(int32 x) {
    return fintto32((int64)x);
}

internal static uint64 fint32to64(int32 x) {
    return fintto64((int64)x);
}

internal static uint32 fint64to32(int64 x) {
    return fintto32(x);
}

internal static uint64 fint64to64(int64 x) {
    return fintto64(x);
}

internal static int32 f32toint32(uint32 x) {
    var (val, _) = f64toint(f32to64(x));
    return (int32)val;
}

internal static int64 f32toint64(uint32 x) {
    var (val, _) = f64toint(f32to64(x));
    return val;
}

internal static int32 f64toint32(uint64 x) {
    var (val, _) = f64toint(x);
    return (int32)val;
}

internal static int64 f64toint64(uint64 x) {
    var (val, _) = f64toint(x);
    return val;
}

internal static uint64 f64touint64(uint64 x) {
    uint64 m = 0x43e0000000000000UL;  // float64 1<<63
    if (fgt64(m, x)) {
        return (uint64)f64toint64(x);
    }
    var y = fadd64(x, ((uint64)0 - m));
    var z = (uint64)f64toint64(y);
    return (uint64)(z | (((uint64)1 << (int)(63))));
}

internal static uint64 f32touint64(uint32 x) {
    uint32 m = 0x5f000000;     // float32 1<<63
    if (fgt32(m, x)) {
        return (uint64)f32toint64(x);
    }
    var y = fadd32(x, ((uint32)0 - m));
    var z = (uint64)f32toint64(y);
    return (uint64)(z | (((uint64)1 << (int)(63))));
}

internal static uint64 fuint64to64(uint64 x) {
    if ((int64)x >= 0) {
        return fint64to64((int64)x);
    }
    // See ../cmd/compile/internal/ssagen/ssa.go:uint64Tofloat
    var y = (uint64)(x & 1);
    var z = (x >> (int)(1));
    z = (uint64)(z | y);
    var r = fint64to64((int64)z);
    return fadd64(r, r);
}

internal static uint32 fuint64to32(uint64 x) {
    if ((int64)x >= 0) {
        return fint64to32((int64)x);
    }
    // See ../cmd/compile/internal/ssagen/ssa.go:uint64Tofloat
    var y = (uint64)(x & 1);
    var z = (x >> (int)(1));
    z = (uint64)(z | y);
    var r = fint64to32((int64)z);
    return fadd32(r, r);
}

} // end runtime_package
