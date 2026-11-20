// Copyright (c) 2014 The mathutil Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using math = math_package;
using big = go.math.big_package;
using go.math;
using ꓸꓸꓸbyte = Span<byte>;
using ꓸꓸꓸint16 = Span<int16>;
using ꓸꓸꓸint32 = Span<int32>;
using ꓸꓸꓸint64 = Span<int64>;
using ꓸꓸꓸint8 = Span<int8>;
using ꓸꓸꓸnint = Span<nint>;
using ꓸꓸꓸnuint = Span<nuint>;
using ꓸꓸꓸuint16 = Span<uint16>;
using ꓸꓸꓸuint32 = Span<uint32>;
using ꓸꓸꓸuint64 = Span<uint64>;

partial class mathutil_package {

public static UntypedInt MaxInt => /* 1<<(IntBits-1) - 1 */ 9223372036854775807;
public static UntypedInt MinInt => /* -MaxInt - 1 */ -9223372036854775808;
public static UntypedInt MaxUint => /* 1<<IntBits - 1 */ 18446744073709551615;
public static UntypedInt IntBits => /* 1 << (^uint(0)>>32&1 + ^uint(0)>>16&1 + ^uint(0)>>8&1 + 3) */ 64;
public static UntypedInt UintPtrBits => /* 1 << (^uintptr(0)>>32&1 + ^uintptr(0)>>16&1 + ^uintptr(0)>>8&1 + 3) */ 64;

internal static ж<big.Int> _1 = big.NewInt(1);
internal static ж<big.Int> _2 = big.NewInt(2);

public static byte GCDByte(byte a, byte b) {
    while (b != 0) {
        (a, b) = (b, (byte)(a % b));
    }
    return a;
}

public static uint16 GCDUint16(uint16 a, uint16 b) {
    while (b != 0) {
        (a, b) = (b, (uint16)(a % b));
    }
    return a;
}

public static uint32 GCDUint32(uint32 a, uint32 b) {
    while (b != 0) {
        (a, b) = (b, a % b);
    }
    return a;
}

public static uint64 GCDUint64(uint64 a, uint64 b) {
    while (b != 0) {
        (a, b) = (b, a % b);
    }
    return a;
}

public static uint32 /*x*/ ISqrt(uint32 n) {
    uint32 x = default;

    if (n == 0) {
        return x;
    }

    if (n >= (uint32)(math.MaxUint16 * math.MaxUint16)) {
        return math.MaxUint16;
    }

    uint32 px = default;
    uint32 nx = default;
    for (x = n; ; (px, x) = (x, nx)) {
        nx = (x + n / x) / 2;
        if (nx == x || nx == px) {
            break;
        }
    }
    return x;
}

public static uint64 /*x*/ SqrtUint64(uint64 n) {
    uint64 x = default;

    if (n == 0) {
        return x;
    }

    if (n >= (uint64)(math.MaxUint32 * math.MaxUint32)) {
        return math.MaxUint32;
    }

    uint64 px = default;
    uint64 nx = default;
    for (x = n; ; (px, x) = (x, nx)) {
        nx = (x + n / x) / 2;
        if (nx == x || nx == px) {
            break;
        }
    }
    return x;
}

public static ж<big.Int> /*x*/ SqrtBig(ж<big.Int> Ꮡn) {
    ж<big.Int> x = default;
    ref var n = ref Ꮡn.DerefOrNull();

    switch (n.Sign()) {
    case var ᴛ when ᴛ == -1: {
        throw panic((nint)(-1));
        break;
    }
    case 0: {
        return big.NewInt(0);
    }}

    ref var px = ref heap(new big.Int(), out var Ꮡpx);
    ref var nx = ref heap(new big.Int(), out var Ꮡnx);
    x = big.NewInt(0);
    x.SetBit(x, n.BitLen() / 2 + 1, 1);
    for (;;) {
        Ꮡnx.Rsh(Ꮡnx.Add(x, Ꮡnx.Div(Ꮡn, x)), 1);
        if (Ꮡnx.Cmp(x) == 0 || Ꮡnx.Cmp(Ꮡpx) == 0) {
            break;
        }
        Ꮡpx.Set(x);
        x.Set(Ꮡnx);
    }
    return x;
}

public static nint Log2Byte(byte n) {
    return log2[n];
}

public static nint Log2Uint16(uint16 n) {
    var b = (uint16)((n >> 8)); if (b != 0) {
        return log2[b] + 8;
    }

    return log2[n];
}

public static nint Log2Uint32(uint32 n) {
    {
        var b = n >> 24; if (b != 0) {
            return log2[b] + 24;
        }
    }

    {
        var b = n >> 16; if (b != 0) {
            return log2[b] + 16;
        }
    }

    {
        var b = n >> 8; if (b != 0) {
            return log2[b] + 8;
        }
    }

    return log2[n];
}

public static nint Log2Uint64(uint64 n) {
    {
        var b = n >> 56; if (b != 0) {
            return log2[b] + 56;
        }
    }

    {
        var b = n >> 48; if (b != 0) {
            return log2[b] + 48;
        }
    }

    {
        var b = n >> 40; if (b != 0) {
            return log2[b] + 40;
        }
    }

    {
        var b = n >> 32; if (b != 0) {
            return log2[b] + 32;
        }
    }

    {
        var b = n >> 24; if (b != 0) {
            return log2[b] + 24;
        }
    }

    {
        var b = n >> 16; if (b != 0) {
            return log2[b] + 16;
        }
    }

    {
        var b = n >> 8; if (b != 0) {
            return log2[b] + 8;
        }
    }

    return log2[n];
}

public static byte ModPowByte(byte b, byte e, byte m) {
    if (b == 0 && e == 0) {
        throw panic((nint)0);
    }

    if (m == 1) {
        return 0;
    }

    var r = (uint16)1;
    for (var (bΔ1, mΔ1) = ((uint16)b, (uint16)m); e > 0; (bΔ1, e) = ((uint16)((uint16)(bΔ1 * bΔ1) % mΔ1), (byte)((e >> 1)))) {
        if ((byte)(e & 1) == 1) {
            r = (uint16)((uint16)(r * bΔ1) % mΔ1);
        }
    }
    return (byte)r;
}

public static uint16 ModPowUint16(uint16 b, uint16 e, uint16 m) {
    if (b == 0 && e == 0) {
        throw panic((nint)0);
    }

    if (m == 1) {
        return 0;
    }

    var r = (uint32)1;
    for (var (bΔ1, mΔ1) = ((uint32)b, (uint32)m); e > 0; (bΔ1, e) = (bΔ1 * bΔ1 % mΔ1, (uint16)((e >> 1)))) {
        if ((uint16)(e & 1) == 1) {
            r = r * bΔ1 % mΔ1;
        }
    }
    return (uint16)r;
}

public static uint32 ModPowUint32(uint32 b, uint32 e, uint32 m) {
    if (b == 0 && e == 0) {
        throw panic((nint)0);
    }

    if (m == 1) {
        return 0;
    }

    var r = (uint64)1;
    for (var (bΔ1, mΔ1) = ((uint64)b, (uint64)m); e > 0; (bΔ1, e) = (bΔ1 * bΔ1 % mΔ1, e >> 1)) {
        if ((e & 1) == 1) {
            r = r * bΔ1 % mΔ1;
        }
    }
    return (uint32)r;
}

public static uint64 /*r*/ ModPowUint64(uint64 b, uint64 e, uint64 m) {
    if (b == 0 && e == 0) {
        throw panic((nint)0);
    }

    if (m == 1) {
        return 0;
    }

    return modPowBigInt(big.NewInt(0).SetUint64(b), big.NewInt(0).SetUint64(e), big.NewInt(0).SetUint64(m)).Uint64();
}

internal static ж<big.Int> /*r*/ modPowBigInt(ж<big.Int> b, ж<big.Int> Ꮡe, ж<big.Int> m) {
    ж<big.Int> r = default;
    ref var e = ref Ꮡe.DerefOrNull();

    r = big.NewInt(1);
    for ((nint i, nint n) = (0, e.BitLen()); i < n; i++) {
        if (e.Bit(i) != 0) {
            r.Mod(r.Mul(r, b), m);
        }
        b.Mod(b.Mul(b, b), m);
    }
    return r;
}

public static ж<big.Int> /*r*/ ModPowBigInt(ж<big.Int> Ꮡb, ж<big.Int> Ꮡe, ж<big.Int> m) {
    ж<big.Int> r = default;
    ref var b = ref Ꮡb.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    if (b.Sign() == 0 && e.Sign() == 0) {
        throw panic((nint)0);
    }

    if (m.Cmp(_1) == 0) {
        return big.NewInt(0);
    }

    if (e.Sign() < 0) {
        return r;
    }

    return modPowBigInt(big.NewInt(0).Set(Ꮡb), big.NewInt(0).Set(Ꮡe), m);
}

internal static readonly ж<big.Int> Ꮡuint64ToBigIntDelta = new StaticBox<big.Int>(default(big.Int));
internal static ref big.Int uint64ToBigIntDelta => ref ((StaticBox<big.Int>)Ꮡuint64ToBigIntDelta).ValueSlot;

[GoInit] internal static void initΔ1() {
    Ꮡuint64ToBigIntDelta.SetBit(Ꮡuint64ToBigIntDelta, 63, 1);
}

internal static nint uintptrBits;

[GoInit] internal static void initΔ2() {
    var x = (uint64)math.MaxUint64;
    uintptrBits = BitLenUintptr((uintptr)x);
}

public static nint UintptrBits() {
    return uintptrBits;
}

public static (uint64 hi, uint64 lo) AddUint128_64(uint64 a, uint64 b) {
    uint64 hi = default;
    uint64 lo = default;

    lo = a + b;
    if (lo < a) {
        hi = 1;
    }
    return (hi, lo);
}

public static (uint64 hi, uint64 lo) MulUint128_64(uint64 a, uint64 b) {
    uint64 hi = default;
    uint64 lo = default;

    UntypedInt w = 32;
    const uint64 m = /* 1<<w - 1 */ 4294967295;
    var (ahi, bhi, alo, blo) = (a >> (int)(w), b >> (int)(w), (uint64)(a & m), (uint64)(b & m));
    lo = alo * blo;
    var mid1 = alo * bhi;
    var mid2 = ahi * blo;
    (var c1, lo) = AddUint128_64(lo, mid1 << (int)(w));
    (var c2, lo) = AddUint128_64(lo, mid2 << (int)(w));
    (_, hi) = AddUint128_64(ahi * bhi, (mid1 >> (int)(w)) + (mid2 >> (int)(w)) + c1 + c2);
    return (hi, lo);
}

public static (uint32 e, ж<big.Int> p) PowerizeBigInt(ж<big.Int> Ꮡb, ж<big.Int> Ꮡn) {
    uint32 e = default;
    ж<big.Int> p = default;
    ref var b = ref Ꮡb.DerefOrNull();
    ref var n = ref Ꮡn.DerefOrNull();

    switch (ᐧ) {
    case {} when Ꮡb.Cmp(_2) < 0 || n.Sign() < 0: {
        return (e, p);
    }
    case {} when n.Sign() == 0 || Ꮡn.Cmp(_1) == 0: {
        return (0, big.NewInt(1));
    }
    case {} when Ꮡb.Cmp(_2) is 0: {
        p = big.NewInt(0);
        e = (uint32)(n.BitLen() - 1);
        p.SetBit(p, (nint)e, 1);
        if (p.Cmp(Ꮡn) < 0) {
            p.Mul(p, _2);
            e++;
        }
        return (e, p);
    }}

    nint bw = b.BitLen();
    nint nw = n.BitLen();
    p = big.NewInt(1);
    ref var bb = ref heap(new big.Int(), out var Ꮡbb);
    ref var r = ref heap(new big.Int(), out var Ꮡr);
    for (;;) {
        switch (p.Cmp(Ꮡn)) {
        case var ᴛ when ᴛ == -1: {
            var x = (uint32)(quo((nw - p.BitLen()), bw));
            if (x == 0) {
                x = 1;
            }
            e += x;
            switch (x) {
            case 1: {
                p.Mul(p, Ꮡb);
                break;
            }
            default: {
                Ꮡr.Set(_1);
                Ꮡbb.Set(Ꮡb);
                var eΔ4 = x;
                for (;;) {
                    if ((eΔ4 & 1) != 0) {
                        Ꮡr.Mul(Ꮡr, Ꮡbb);
                    }
                    eΔ4 >>= 1; if (eΔ4 == 0) {
                        break;
                    }

                    Ꮡbb.Mul(Ꮡbb, Ꮡbb);
                }
                p.Mul(p, Ꮡr);
                break;
            }}

            break;
        }
        case 0 or 1: {
            return (e, p);
        }}

    }
}

public static (uint32 e, ж<big.Int> p) PowerizeUint32BigInt(uint32 b, ж<big.Int> Ꮡn) {
    uint32 e = default;
    ж<big.Int> p = default;
    ref var n = ref Ꮡn.DerefOrNull();

    switch (ᐧ) {
    case {} when b < 2 || n.Sign() < 0: {
        return (e, p);
    }
    case {} when n.Sign() == 0 || Ꮡn.Cmp(_1) == 0: {
        return (0, big.NewInt(1));
    }
    case {} when b is 2: {
        p = big.NewInt(0);
        e = (uint32)(n.BitLen() - 1);
        p.SetBit(p, (nint)e, 1);
        if (p.Cmp(Ꮡn) < 0) {
            p.Mul(p, _2);
            e++;
        }
        return (e, p);
    }}

    ref var bb = ref heap(new big.Int(), out var Ꮡbb);
    Ꮡbb.SetInt64((int64)b);
    return PowerizeBigInt(Ꮡbb, Ꮡn);
}

public static bool ProbablyPrimeUint32(uint32 n, uint32 a) {
    var d = n - 1;
    nint s = 0;
    for (; (d & 1) == 0; (d, s) = (d >> 1, s + 1)) {
    }
    var x = (uint64)ModPowUint32(a, d, n);
    if (x == 1 || (uint32)x == n - 1) {
        return true;
    }

    for (; s > 1; s--) {
        x = x * x % (uint64)n; if (x == 1) {
            return false;
        }

        if ((uint32)x == n - 1) {
            return true;
        }
    }
    return false;
}

public static bool ProbablyPrimeUint64_32(uint64 n, uint32 a) {
    var d = n - 1;
    nint s = 0;
    for (; (d & 1) == 0; (d, s) = (d >> 1, s + 1)) {
    }
    var x = ModPowUint64((uint64)a, d, n);
    if (x == 1 || x == n - 1) {
        return true;
    }

    var (bx, bn) = (big.NewInt(0).SetUint64(x), big.NewInt(0).SetUint64(n));
    for (; s > 1; s--) {
        x = bx.Mod(bx.Mul(bx, bx), bn).Uint64(); if (x == 1) {
            return false;
        }

        if (x == n - 1) {
            return true;
        }
    }
    return false;
}

public static bool ProbablyPrimeBigInt_32(ж<big.Int> n, uint32 a) {
    ref var d = ref heap(new big.Int(), out var Ꮡd);
    Ꮡd.Set(n);
    Ꮡd.Sub(Ꮡd, _1);
    nint s = 0;
    for (; d.Bit(s) == 0; s++) {
    }
    var nMinus1 = big.NewInt(0).Set(Ꮡd);
    Ꮡd.Rsh(Ꮡd, (nuint)s);

    var x = ModPowBigInt(big.NewInt((int64)a), Ꮡd, n);
    if (x.Cmp(_1) == 0 || x.Cmp(nMinus1) == 0) {
        return true;
    }

    for (; s > 1; s--) {
        x = x.Mod(x.Mul(x, x), n); if (x.Cmp(_1) == 0) {
            return false;
        }

        if (x.Cmp(nMinus1) == 0) {
            return true;
        }
    }
    return false;
}

public static bool ProbablyPrimeBigInt(ж<big.Int> n, ж<big.Int> a) {
    ref var d = ref heap(new big.Int(), out var Ꮡd);
    Ꮡd.Set(n);
    Ꮡd.Sub(Ꮡd, _1);
    nint s = 0;
    for (; d.Bit(s) == 0; s++) {
    }
    var nMinus1 = big.NewInt(0).Set(Ꮡd);
    Ꮡd.Rsh(Ꮡd, (nuint)s);

    var x = ModPowBigInt(a, Ꮡd, n);
    if (x.Cmp(_1) == 0 || x.Cmp(nMinus1) == 0) {
        return true;
    }

    for (; s > 1; s--) {
        x = x.Mod(x.Mul(x, x), n); if (x.Cmp(_1) == 0) {
            return false;
        }

        if (x.Cmp(nMinus1) == 0) {
            return true;
        }
    }
    return false;
}

public static nint Max(nint a, nint b) {
    if (a > b) {
        return a;
    }

    return b;
}

public static nint Min(nint a, nint b) {
    if (a < b) {
        return a;
    }

    return b;
}

public static ж<nint> MaxPtr(ж<nint> Ꮡa, ж<nint> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a > b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static ж<nint> MinPtr(ж<nint> Ꮡa, ж<nint> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a < b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static nint MaxVal(nint val, params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.sslice();

    nint res = val;
    foreach (var (_, v) in range(vals)) {
        if (v > res) {
            res = v;
        }
    }
    return res;
}

public static nint MinVal(nint val, params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.sslice();

    nint res = val;
    foreach (var (_, v) in range(vals)) {
        if (v < res) {
            res = v;
        }
    }
    return res;
}

public static nint Clamp(nint v, nint lo, nint hi) {
    return Min(Max(v, lo), hi);
}

public static nuint UMax(nuint a, nuint b) {
    if (a > b) {
        return a;
    }

    return b;
}

public static nuint UMin(nuint a, nuint b) {
    if (a < b) {
        return a;
    }

    return b;
}

public static ж<nuint> UMaxPtr(ж<nuint> Ꮡa, ж<nuint> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a > b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static ж<nuint> UMinPtr(ж<nuint> Ꮡa, ж<nuint> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a < b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static nuint UMaxVal(nuint val, params ꓸꓸꓸnuint valsʗp) {
    var vals = valsʗp.sslice();

    nuint res = val;
    foreach (var (_, v) in range(vals)) {
        if (v > res) {
            res = v;
        }
    }
    return res;
}

public static nuint UMinVal(nuint val, params ꓸꓸꓸnuint valsʗp) {
    var vals = valsʗp.sslice();

    nuint res = val;
    foreach (var (_, v) in range(vals)) {
        if (v < res) {
            res = v;
        }
    }
    return res;
}

public static nuint UClamp(nuint v, nuint lo, nuint hi) {
    return UMin(UMax(v, lo), hi);
}

public static byte MaxByte(byte a, byte b) {
    if (a > b) {
        return a;
    }

    return b;
}

public static byte MinByte(byte a, byte b) {
    if (a < b) {
        return a;
    }

    return b;
}

public static ж<byte> MaxBytePtr(ж<byte> Ꮡa, ж<byte> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a > b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static ж<byte> MinBytePtr(ж<byte> Ꮡa, ж<byte> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a < b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static byte MaxByteVal(byte val, params ꓸꓸꓸbyte valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v > res) {
            res = v;
        }
    }
    return res;
}

public static byte MinByteVal(byte val, params ꓸꓸꓸbyte valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v < res) {
            res = v;
        }
    }
    return res;
}

public static byte ClampByte(byte v, byte lo, byte hi) {
    return MinByte(MaxByte(v, lo), hi);
}

public static int8 MaxInt8(int8 a, int8 b) {
    if (a > b) {
        return a;
    }

    return b;
}

public static int8 MinInt8(int8 a, int8 b) {
    if (a < b) {
        return a;
    }

    return b;
}

public static ж<int8> MaxInt8Ptr(ж<int8> Ꮡa, ж<int8> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a > b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static ж<int8> MinInt8Ptr(ж<int8> Ꮡa, ж<int8> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a < b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static int8 MaxInt8Val(int8 val, params ꓸꓸꓸint8 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v > res) {
            res = v;
        }
    }
    return res;
}

public static int8 MinInt8Val(int8 val, params ꓸꓸꓸint8 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v < res) {
            res = v;
        }
    }
    return res;
}

public static int8 ClampInt8(int8 v, int8 lo, int8 hi) {
    return MinInt8(MaxInt8(v, lo), hi);
}

public static uint16 MaxUint16(uint16 a, uint16 b) {
    if (a > b) {
        return a;
    }

    return b;
}

public static uint16 MinUint16(uint16 a, uint16 b) {
    if (a < b) {
        return a;
    }

    return b;
}

public static ж<uint16> MaxUint16Ptr(ж<uint16> Ꮡa, ж<uint16> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a > b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static ж<uint16> MinUint16Ptr(ж<uint16> Ꮡa, ж<uint16> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a < b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static uint16 MaxUint16Val(uint16 val, params ꓸꓸꓸuint16 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v > res) {
            res = v;
        }
    }
    return res;
}

public static uint16 MinUint16Val(uint16 val, params ꓸꓸꓸuint16 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v < res) {
            res = v;
        }
    }
    return res;
}

public static uint16 ClampUint16(uint16 v, uint16 lo, uint16 hi) {
    return MinUint16(MaxUint16(v, lo), hi);
}

public static int16 MaxInt16(int16 a, int16 b) {
    if (a > b) {
        return a;
    }

    return b;
}

public static int16 MinInt16(int16 a, int16 b) {
    if (a < b) {
        return a;
    }

    return b;
}

public static ж<int16> MaxInt16Ptr(ж<int16> Ꮡa, ж<int16> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a > b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static ж<int16> MinInt16Ptr(ж<int16> Ꮡa, ж<int16> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a < b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static int16 MaxInt16Val(int16 val, params ꓸꓸꓸint16 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v > res) {
            res = v;
        }
    }
    return res;
}

public static int16 MinInt16Val(int16 val, params ꓸꓸꓸint16 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v < res) {
            res = v;
        }
    }
    return res;
}

public static int16 ClampInt16(int16 v, int16 lo, int16 hi) {
    return MinInt16(MaxInt16(v, lo), hi);
}

public static uint32 MaxUint32(uint32 a, uint32 b) {
    if (a > b) {
        return a;
    }

    return b;
}

public static uint32 MinUint32(uint32 a, uint32 b) {
    if (a < b) {
        return a;
    }

    return b;
}

public static ж<uint32> MaxUint32Ptr(ж<uint32> Ꮡa, ж<uint32> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a > b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static ж<uint32> MinUint32Ptr(ж<uint32> Ꮡa, ж<uint32> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a < b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static uint32 MaxUint32Val(uint32 val, params ꓸꓸꓸuint32 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v > res) {
            res = v;
        }
    }
    return res;
}

public static uint32 MinUint32Val(uint32 val, params ꓸꓸꓸuint32 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v < res) {
            res = v;
        }
    }
    return res;
}

public static uint32 ClampUint32(uint32 v, uint32 lo, uint32 hi) {
    return MinUint32(MaxUint32(v, lo), hi);
}

public static int32 MaxInt32(int32 a, int32 b) {
    if (a > b) {
        return a;
    }

    return b;
}

public static int32 MinInt32(int32 a, int32 b) {
    if (a < b) {
        return a;
    }

    return b;
}

public static ж<int32> MaxInt32Ptr(ж<int32> Ꮡa, ж<int32> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a > b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static ж<int32> MinInt32Ptr(ж<int32> Ꮡa, ж<int32> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a < b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static int32 MaxInt32Val(int32 val, params ꓸꓸꓸint32 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v > res) {
            res = v;
        }
    }
    return res;
}

public static int32 MinInt32Val(int32 val, params ꓸꓸꓸint32 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v < res) {
            res = v;
        }
    }
    return res;
}

public static int32 ClampInt32(int32 v, int32 lo, int32 hi) {
    return MinInt32(MaxInt32(v, lo), hi);
}

public static uint64 MaxUint64(uint64 a, uint64 b) {
    if (a > b) {
        return a;
    }

    return b;
}

public static uint64 MinUint64(uint64 a, uint64 b) {
    if (a < b) {
        return a;
    }

    return b;
}

public static ж<uint64> MaxUint64Ptr(ж<uint64> Ꮡa, ж<uint64> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a > b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static ж<uint64> MinUint64Ptr(ж<uint64> Ꮡa, ж<uint64> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a < b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static uint64 MaxUint64Val(uint64 val, params ꓸꓸꓸuint64 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v > res) {
            res = v;
        }
    }
    return res;
}

public static uint64 MinUint64Val(uint64 val, params ꓸꓸꓸuint64 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v < res) {
            res = v;
        }
    }
    return res;
}

public static uint64 ClampUint64(uint64 v, uint64 lo, uint64 hi) {
    return MinUint64(MaxUint64(v, lo), hi);
}

public static int64 MaxInt64(int64 a, int64 b) {
    if (a > b) {
        return a;
    }

    return b;
}

public static int64 MinInt64(int64 a, int64 b) {
    if (a < b) {
        return a;
    }

    return b;
}

public static ж<int64> MaxInt64Ptr(ж<int64> Ꮡa, ж<int64> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a > b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static ж<int64> MinInt64Ptr(ж<int64> Ꮡa, ж<int64> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡa == nil) {
        return Ꮡb;
    }
    if (Ꮡb == nil) {
        return Ꮡa;
    }
    if (a < b) {
        return Ꮡa;
    }

    return Ꮡb;
}

public static int64 MaxInt64Val(int64 val, params ꓸꓸꓸint64 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v > res) {
            res = v;
        }
    }
    return res;
}

public static int64 MinInt64Val(int64 val, params ꓸꓸꓸint64 valsʗp) {
    var vals = valsʗp.sslice();

    var res = val;
    foreach (var (_, v) in range(vals)) {
        if (v < res) {
            res = v;
        }
    }
    return res;
}

public static int64 ClampInt64(int64 v, int64 lo, int64 hi) {
    return MinInt64(MaxInt64(v, lo), hi);
}

public static slice<nint> ToBase(ж<big.Int> n, nint b) {
    var rᴷ = new stackAppend<nint>(moves: true);

    ref var nn = ref heap(new big.Int(), out var Ꮡnn);
    Ꮡnn.Set(n);
    if (b < 2) {
        throw panic("invalid base");
    }

    nint k = 1;
    switch (nn.Sign()) {
    case var ᴛ when ᴛ == -1: {
        Ꮡnn.Neg(Ꮡnn);
        k = -1;
        break;
    }
    case 0: {
        return [0];
    }}

    var bb = big.NewInt((int64)b);
    slice<nint> r = default;
    var rem = big.NewInt(0);
    while (nn.Sign() != 0) {
        Ꮡnn.QuoRem(Ꮡnn, bb, rem);
        r = append(rᴷ, r, k * (nint)rem.Int64());
    }
    r = move2heap(rᴷ, r);
    return r;
}

public static (int64 sum, bool gt) CheckAddInt64(int64 a, int64 b) {
    return (a + b, a > 0 && b > (int64)math.MaxInt64 - a || a < 0 && b < (int64)math.MinInt64 - a);
}

public static (int64 sum, bool lt) CheckSubInt64(int64 a, int64 b) {
    return (a - b, a > 0 && a - (int64)math.MaxInt64 > b || a < 0 && a - (int64)math.MinInt64 < b);
}

public static (int8 r, bool ovf) AddOverflowInt8(int8 a, int8 b) {
    int8 r = default;

    r = (int8)(a + b);
    if (a > 0 && b > 0) {
        return (r, (uint8)r > math.MaxInt8);
    }

    if (a < 0 && b < 0) {
        return (r, (uint8)r <= math.MaxInt8);
    }

    return (r, false);
}

public static (int16 r, bool ovf) AddOverflowInt16(int16 a, int16 b) {
    int16 r = default;

    r = (int16)(a + b);
    if (a > 0 && b > 0) {
        return (r, (uint16)r > math.MaxInt16);
    }

    if (a < 0 && b < 0) {
        return (r, (uint16)r <= math.MaxInt16);
    }

    return (r, false);
}

public static (int32 r, bool ovf) AddOverflowInt32(int32 a, int32 b) {
    int32 r = default;

    r = a + b;
    if (a > 0 && b > 0) {
        return (r, (uint32)r > math.MaxInt32);
    }

    if (a < 0 && b < 0) {
        return (r, (uint32)r <= math.MaxInt32);
    }

    return (r, false);
}

public static (int64 r, bool ovf) AddOverflowInt64(int64 a, int64 b) {
    int64 r = default;

    r = a + b;
    if (a > 0 && b > 0) {
        return (r, (uint64)r > math.MaxInt64);
    }

    if (a < 0 && b < 0) {
        return (r, (uint64)r <= math.MaxInt64);
    }

    return (r, false);
}

public static (int8 r, bool ovf) SubOverflowInt8(int8 a, int8 b) {
    int8 r = default;

    r = (int8)(a - b);
    if (a >= 0 && b < 0) {
        return (r, (uint8)r >= (uint8)(math.MaxInt8 + 1));
    }

    if (a < 0 && b > 0) {
        return (r, (uint8)r <= math.MaxInt8);
    }

    return (r, false);
}

public static (int16 r, bool ovf) SubOverflowInt16(int16 a, int16 b) {
    int16 r = default;

    r = (int16)(a - b);
    if (a >= 0 && b < 0) {
        return (r, (uint16)r >= (uint16)(math.MaxInt16 + 1));
    }

    if (a < 0 && b > 0) {
        return (r, (uint16)r <= math.MaxInt16);
    }

    return (r, false);
}

public static (int32 r, bool ovf) SubOverflowInt32(int32 a, int32 b) {
    int32 r = default;

    r = a - b;
    if (a >= 0 && b < 0) {
        return (r, (uint32)r >= (uint32)(math.MaxInt32 + 1));
    }

    if (a < 0 && b > 0) {
        return (r, (uint32)r <= math.MaxInt32);
    }

    return (r, false);
}

public static (int64 r, bool ovf) SubOverflowInt64(int64 a, int64 b) {
    int64 r = default;

    r = a - b;
    if (a >= 0 && b < 0) {
        return (r, (uint64)r >= (uint64)(math.MaxInt64 + 1));
    }

    if (a < 0 && b > 0) {
        return (r, (uint64)r <= math.MaxInt64);
    }

    return (r, false);
}

public static (int8 r, bool ovf) MulOverflowInt8(int8 a, int8 b) {
    if (a == 0 || b == 0) {
        return (0, false);
    }

    var z = (int16)((int16)a * (int16)b);
    return ((int8)z, z < math.MinInt8 || z > math.MaxInt8);
}

public static (int16 r, bool ovf) MulOverflowInt16(int16 a, int16 b) {
    if (a == 0 || b == 0) {
        return (0, false);
    }

    var z = (int32)a * (int32)b;
    return ((int16)z, z < math.MinInt16 || z > math.MaxInt16);
}

public static (int32 r, bool ovf) MulOverflowInt32(int32 a, int32 b) {
    if (a == 0 || b == 0) {
        return (0, false);
    }

    var z = (int64)a * (int64)b;
    return ((int32)z, z < math.MinInt32 || z > math.MaxInt32);
}

public static (int64 r, bool ovf) MulOverflowInt64(int64 a, int64 b) {
    int64 r = default;

    UntypedInt mostPositive = /* 1<<63 - 1 */ 9223372036854775807;
    const int64 mostNegative = /* -(mostPositive + 1) */ -9223372036854775808;
    r = a * b;
    if (a == 0 || b == 0 || a == 1 || b == 1) {
        return (r, false);
    }

    if (a == mostNegative || b == mostNegative) {
        return (r, true);
    }

    return (r, quo(r, b) != a);
}

} // end mathutil_package
