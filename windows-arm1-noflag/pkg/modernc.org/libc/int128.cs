// Copyright 2020 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Some code is copied and adjusted from
// https://github.com/lukechampine/uint128, the original LICENSE file
// reproduced below in full as of 2021-01-19:

/*
The MIT License (MIT)

Copyright (c) 2019 Luke Champine

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace go.modernc.org;

using mbits = go.math.bits_package;
using mathutil = go.modernc.org.mathutil_package;
using go.modernc.org;
using static libc_package;

partial class libc_package {

partial struct Int128 /*global::go.modernc.org.mathutil_package.Int128*/;

internal static Int128 int128Minus1 = new Int128(new mathutil.Int128(-1, -1));
internal static Int128 int128Plus1 = new Int128(new mathutil.Int128(Lo: 1));

public static Int128 Int128FromFloat32(float32 n) { return (Int128)mathutil.NewInt128FromFloat32(n); }
public static Int128 Int128FromFloat64(float64 n) { return (Int128)mathutil.NewInt128FromFloat64(n); }
public static Int128 Int128FromInt16(int16 n) { return (Int128)mathutil.NewInt128FromInt64((int64)n); }
public static Int128 Int128FromInt32(int32 n) { return (Int128)mathutil.NewInt128FromInt64((int64)n); }
public static Int128 Int128FromInt64(int64 n) { return (Int128)mathutil.NewInt128FromInt64(n); }
public static Int128 Int128FromInt8(int8 n) { return (Int128)mathutil.NewInt128FromInt64((int64)n); }
public static Int128 Int128FromUint16(uint16 n) { return (Int128)mathutil.NewInt128FromInt64((int64)n); }
public static Int128 Int128FromUint32(uint32 n) { return (Int128)mathutil.NewInt128FromInt64((int64)n); }
public static Int128 Int128FromUint64(uint64 n) { return (Int128)mathutil.NewInt128FromUint64(n); }
public static Int128 Int128FromUint8(uint8 n) { return (Int128)mathutil.NewInt128FromInt64((int64)n); }
public static Int128 Int128FromUint128(Uint128 n) { return new Int128(new mathutil.Int128(Lo: (int64)n.Lo, Hi: (int64)n.Hi)); }

} partial class libcᴛInt128 {

public static void LValueDec(this ж<Int128> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    n = Ꮡn.Add(int128Minus1);
}

public static void LValueInc(this ж<Int128> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    n = Ꮡn.Add(int128Plus1);
}

public static void LValueShl(this ж<Int128> Ꮡn, int32 c) {
    ref var n = ref Ꮡn.DerefOrNull();

    n = Ꮡn.Shl(c);
}

public static void LValueShr(this ж<Int128> Ꮡn, int32 c) {
    ref var n = ref Ꮡn.DerefOrNull();

    n = Ꮡn.Shr(c);
}

public static Int128 And(this Int128 n, Int128 v) { return new Int128(new mathutil.Int128(n.Lo & v.Lo, n.Hi & v.Hi)); }
public static nint Cmp(this Int128 n, Int128 y) { return ((mathutil.Int128)n).Cmp((mathutil.Int128)y); }
public static int16 Int16(this Int128 n) { return (int16)n.Lo; }
public static int32 Int32(this Int128 n) { return (int32)n.Lo; }
public static int64 Int64(this Int128 n) { return n.Lo; }
public static int8 Int8(this Int128 n) { return (int8)n.Lo; }
public static Int128 Or(this Int128 n, Int128 v) { return new Int128(new mathutil.Int128(n.Lo | v.Lo, n.Hi | v.Hi)); }
public static Uint128 /*r*/ Uint128(this Int128 n) { return new Uint128(new mathutil.Uint128((uint64)n.Lo, (uint64)n.Hi)); }
public static uint16 Uint16(this Int128 n) { return (uint16)n.Lo; }
public static uint32 Uint32(this Int128 n) { return (uint32)n.Lo; }
public static uint64 Uint64(this Int128 n) { return (uint64)n.Lo; }
public static uint8 Uint8(this Int128 n) { return (uint8)n.Lo; }
public static Int128 Xor(this Int128 n, Int128 v) { return new Int128(new mathutil.Int128(n.Lo ^ v.Lo, n.Hi ^ v.Hi)); }

public static Int128 Neg(this Int128 n) {
    n.Lo ^= (int64)(-1);
    n.Hi ^= (int64)(-1);
    return n.Add(int128Plus1);
}

public static float32 Float32(this Int128 n) {
    switch (n.Hi) {
    case 0: {
        return (float32)(uint64)n.Lo;
    }
    case var ᴛ when ᴛ == -1: {
        return -(float32)(uint64)n.Lo;
    }}

    if (n.Hi < 0) {
        n = n.Neg();
        return -(float32)n.Hi * (18446744073709551616F) + (float32)(uint64)n.Lo;
    }

    return -(float32)n.Hi * (18446744073709551616F) + (float32)(uint64)n.Lo;
}

public static float64 Float64(this Int128 n) {
    switch (n.Hi) {
    case 0: {
        return (float64)(uint64)n.Lo;
    }
    case var ᴛ when ᴛ == -1: {
        return -(float64)(uint64)n.Lo;
    }}

    if (n.Hi < 0) {
        n = n.Neg();
        return -(float64)n.Hi * (18446744073709551616D) + (float64)(uint64)n.Lo;
    }

    return (float64)n.Hi * (18446744073709551616D) + (float64)(uint64)n.Lo;
}

public static Int128 /*r*/ Add(this Int128 n, Int128 m) {
    Int128 r = default;

    r.Lo = n.Lo + m.Lo;
    r.Hi = n.Hi + m.Hi;
    if ((uint64)r.Lo < (uint64)n.Lo) {
        r.Hi++;
    }
    return r;
}

public static Int128 Mul(this Int128 n, Int128 m) {
    var (hi, lo) = mbits.Mul64((uint64)n.Lo, (uint64)m.Lo);
    var (_, p1) = mbits.Mul64((uint64)n.Hi, (uint64)m.Lo);
    var (_, p3) = mbits.Mul64((uint64)n.Lo, (uint64)m.Hi);
    (hi, _) = mbits.Add64(hi, p1, 0);
    (hi, _) = mbits.Add64(hi, p3, 0);
    return new Int128(new mathutil.Int128((int64)lo, (int64)hi));
}

public static Int128 /*r*/ Shl(this Int128 n, int32 c) {
    Int128 r = default;

    if (c > 64) {
        r.Lo = 0;
        r.Hi = n.Lo.Lsh((int64)(c - 64));
    } else {
        r.Lo = n.Lo.Lsh((int64)(c));
        r.Hi = n.Hi.Lsh((int64)(c)) | n.Lo.Rsh((int64)(64 - c));
    }
    return r;
}

public static Int128 /*r*/ Shr(this Int128 n, int32 c) {
    Int128 r = default;

    if (c > 64) {
        r.Lo = n.Hi.Rsh((int64)(c - 64));
        switch (ᐧ) {
        case {} when n.Hi is < 0: {
            r.Hi = -1;
            break;
        }
        default: {
            r.Hi = 0;
            break;
        }}

    } else {
        r.Lo = n.Lo.Rsh((int64)(c)) | n.Hi.Lsh((int64)(64 - c));
        r.Hi = n.Hi.Rsh((int64)(c));
    }
    return r;
}

} partial class libc_package {

partial struct Uint128 /*global::go.modernc.org.mathutil_package.Uint128*/;

public static Uint128 Uint128FromFloat32(float32 n) { return (Uint128)mathutil.NewUint128FromFloat32(n); }
public static Uint128 Uint128FromFloat64(float64 n) { return (Uint128)mathutil.NewUint128FromFloat64(n); }
public static Uint128 Uint128FromInt128(Int128 n) { return new Uint128(new mathutil.Uint128(Lo: (uint64)n.Lo, Hi: (uint64)n.Hi)); }
public static Uint128 Uint128FromInt16(int16 n) { return Uint128FromInt64((int64)n); }
public static Uint128 Uint128FromInt32(int32 n) { return Uint128FromInt64((int64)n); }
public static Uint128 Uint128FromInt8(int8 n) { return Uint128FromInt64((int64)n); }
public static Uint128 Uint128FromUint16(uint16 n) { return new Uint128(new mathutil.Uint128(Lo: (uint64)n)); }
public static Uint128 Uint128FromUint32(uint32 n) { return new Uint128(new mathutil.Uint128(Lo: (uint64)n)); }
public static Uint128 Uint128FromUint64(uint64 n) { return new Uint128(new mathutil.Uint128(Lo: n)); }
public static Uint128 Uint128FromUint8(uint8 n) { return new Uint128(new mathutil.Uint128(Lo: (uint64)n)); }

public static Uint128 /*r*/ Uint128FromInt64(int64 n) {
    Uint128 r = default;

    r.Lo = (uint64)n;
    if (n < 0) {
        r.Hi = ~(uint64)0;
    }
    return r;
}

} partial class libcᴛUint128 {

public static void LValueShl(this ж<Uint128> Ꮡn, int32 c) {
    ref var n = ref Ꮡn.DerefOrNull();

    n = Ꮡn.Shl(c);
}

public static void LValueShr(this ж<Uint128> Ꮡn, int32 c) {
    ref var n = ref Ꮡn.DerefOrNull();

    n = Ꮡn.Shr(c);
}

public static Uint128 And(this Uint128 n, Uint128 m) { return new Uint128(new mathutil.Uint128(n.Lo & m.Lo, n.Hi & m.Hi)); }
public static Int128 Int128(this Uint128 n) { return new Int128(new mathutil.Int128((int64)n.Lo, (int64)n.Hi)); }
public static int16 Int16(this Uint128 n) { return (int16)n.Lo; }
public static int32 Int32(this Uint128 n) { return (int32)n.Lo; }
public static int64 Int64(this Uint128 n) { return (int64)n.Lo; }
public static int8 Int8(this Uint128 n) { return (int8)n.Lo; }
public static Uint128 Or(this Uint128 n, Uint128 m) { return new Uint128(new mathutil.Uint128(n.Lo | m.Lo, n.Hi | m.Hi)); }
public static uint16 Uint16(this Uint128 n) { return (uint16)n.Lo; }
public static uint32 Uint32(this Uint128 n) { return (uint32)n.Lo; }
public static uint64 Uint64(this Uint128 n) { return n.Lo; }
public static uint8 Uint8(this Uint128 n) { return (uint8)n.Lo; }
public static Uint128 Xor(this Uint128 n, Uint128 m) { return new Uint128(new mathutil.Uint128(n.Lo ^ m.Lo, n.Hi ^ m.Hi)); }

public static Uint128 /*r*/ Add(this Uint128 n, Uint128 m) {
    Uint128 r = default;

    uint64 carry = default;
    (r.Lo, carry) = mbits.Add64(n.Lo, m.Lo, 0);
    (r.Hi, _) = mbits.Add64(n.Hi, m.Hi, carry);
    return r;
}

public static Uint128 Mul(this Uint128 n, Uint128 m) {
    var (hi, lo) = mbits.Mul64(n.Lo, m.Lo);
    var (_, p1) = mbits.Mul64(n.Hi, m.Lo);
    var (_, p3) = mbits.Mul64(n.Lo, m.Hi);
    (hi, _) = mbits.Add64(hi, p1, 0);
    (hi, _) = mbits.Add64(hi, p3, 0);
    return new Uint128(new mathutil.Uint128(lo, hi));
}

public static Uint128 /*r*/ Shr(this Uint128 n, int32 c) {
    Uint128 r = default;

    if (c > 64) {
        r.Lo = n.Hi.Rsh((int64)(c - 64));
        r.Hi = 0;
    } else {
        r.Lo = n.Lo.Rsh((int64)(c)) | n.Hi.Lsh((int64)(64 - c));
        r.Hi = n.Hi.Rsh((int64)(c));
    }
    return r;
}

internal static (Uint128, bool ovf) mulOvf(this Uint128 n, Uint128 m) {
    bool ovf = default;

    var (hi, lo) = mbits.Mul64(n.Lo, m.Lo);
    var (p0, p1) = mbits.Mul64(n.Hi, m.Lo);
    var (p2, p3) = mbits.Mul64(n.Lo, m.Hi);
    (hi, var c0) = mbits.Add64(hi, p1, 0);
    (hi, var c1) = mbits.Add64(hi, p3, 0);
    ovf = p0 != 0 || p2 != 0 || c0 != 0 || c1 != 0;
    return (new Uint128(new mathutil.Uint128(lo, hi)), ovf);
}

internal static (Uint128 q, Uint128 r) quoRem(this Uint128 n, Uint128 m) {
    Uint128 q = default;
    Uint128 r = default;

    if (m.Hi == 0) {
        uint64 r64 = default;
        (q, r64) = n.quoRem64(m.Lo);
        r = Uint128FromUint64(r64);
    } else {
        nint nz = mbits.LeadingZeros64(m.Hi);
        var v1 = m.Shl((int32)nz);
        var u1 = n.Shr(1);
        var (tq, _) = mbits.Div64(u1.Hi, u1.Lo, v1.Hi);
        tq.RshAssign((int64)(63 - nz));
        if (tq != 0) {
            tq--;
        }
        q = Uint128FromUint64(tq);
        r = n.Sub(m.mul64(tq));
        if (r.Cmp(m) >= 0) {
            q = q.add64(1);
            r = r.Sub(m);
        }
    }
    return (q, r);
}

internal static (Uint128 q, uint64 r) quoRem64(this Uint128 n, uint64 m) {
    Uint128 q = default;
    uint64 r = default;

    if (n.Hi < m) {
        (q.Lo, r) = mbits.Div64(n.Hi, n.Lo, m);
    } else {
        (q.Hi, r) = mbits.Div64(0, n.Hi, m);
        (q.Lo, r) = mbits.Div64(r, n.Lo, m);
    }
    return (q, r);
}

public static Uint128 /*r*/ Div(this Uint128 n, Uint128 m) {
    Uint128 r = default;

    (r, _) = n.quoRem(m);
    return r;
}

public static Uint128 /*r*/ Shl(this Uint128 n, int32 c) {
    Uint128 r = default;

    if (c > 64) {
        r.Lo = 0;
        r.Hi = n.Lo.Lsh((int64)(c - 64));
    } else {
        r.Lo = n.Lo.Lsh((int64)(c));
        r.Hi = n.Hi.Lsh((int64)(c)) | n.Lo.Rsh((int64)(64 - c));
    }
    return r;
}

public static Uint128 Sub(this Uint128 n, Uint128 m) {
    var (lo, borrow) = mbits.Sub64(n.Lo, m.Lo, 0);
    var (hi, _) = mbits.Sub64(n.Hi, m.Hi, borrow);
    return new Uint128(new mathutil.Uint128(lo, hi));
}

internal static Uint128 mul64(this Uint128 n, uint64 m) {
    var (hi, lo) = mbits.Mul64(n.Lo, m);
    var (_, p1) = mbits.Mul64(n.Hi, m);
    (hi, _) = mbits.Add64(hi, p1, 0);
    return new Uint128(new mathutil.Uint128(lo, hi));
}

public static nint Cmp(this Uint128 n, Uint128 m) {
    if (n == m) {
        return 0;
    } else if (n.Hi < m.Hi || (n.Hi == m.Hi && n.Lo < m.Lo)) {
        return -1;
    } else {
        return 1;
    }
}

internal static Uint128 add64(this Uint128 n, uint64 m) {
    var (lo, carry) = mbits.Add64(n.Lo, m, 0);
    var (hi, _) = mbits.Add64(n.Hi, 0, carry);
    return new Uint128(new mathutil.Uint128(lo, hi));
}

public static float32 Float32(this Uint128 n) {
    if (n.Hi == 0) {
        return (float32)n.Lo;
    }

    return (float32)n.Hi * (18446744073709551616F) + (float32)n.Lo;
}

public static float64 Float64(this Uint128 n) {
    if (n.Hi == 0) {
        return (float64)n.Lo;
    }

    return (float64)n.Hi * (18446744073709551616D) + (float64)n.Lo;
}

} // end libcᴛUint128
