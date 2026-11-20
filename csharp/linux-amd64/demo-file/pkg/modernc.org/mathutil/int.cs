// Copyright (c) 2018 The mathutil Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using fmt = fmt_package;
using math = math_package;
using big = go.math.big_package;
using go.math;
using static mathutil_package;

partial class mathutil_package {

public static ж<big.Int> MaxInt128;
public static ж<big.Int> MinInt128;
public static ж<big.Int> MaxUint128;

[GoInit] internal static void init() {
    bool ok = default;
    (MaxInt128, ok) = big.NewInt(0).SetString("0x7fffffff_ffffffff_ffffffff_ffffffff", 0);
    if (!ok) {
        throw panic("internal error");
    }

    MinInt128 = big.NewInt(0).Set(MaxInt128);
    MinInt128.Add(MinInt128, _1);
    MinInt128.Neg(MinInt128);

    (MaxUint128, ok) = big.NewInt(0).SetString("0xffffffff_ffffffff_ffffffff_ffffffff", 0);
    if (!ok) {
        throw panic("internal error");
    }
}

internal static readonly GoBigConst maxInt128 = /* 1<<127 - 1 */
    GoBigConst.Parse("170141183460469231731687303715884105727");
internal static readonly GoBigConst maxUint128 = /* 1<<128 - 1 */
    GoBigConst.Parse("340282366920938463463374607431768211455");
internal static readonly GoBigConst minInt128 = /* -maxInt128 - 1 */
    GoBigConst.Parse("-170141183460469231731687303715884105728");

partial struct Int128 {
    public int64 Lo;
    public int64 Hi;
}

} partial class mathutilᴛInt128 {

public static (Int128 r, bool cy) Add(this Int128 x, Int128 y) {
    Int128 r = default;

    r.Lo = x.Lo + y.Lo;
    r.Hi = x.Hi + y.Hi;
    if ((uint64)r.Lo < (uint64)x.Lo) {
        r.Hi++;
    }
    return (r, (r.Cmp(x) < 0) == (y.Sign() >= 0));
}

public static ж<big.Int> BigInt(this Int128 x) {
    var r = big.NewInt(x.Hi);
    r.Lsh(r, 64);
    var lo = big.NewInt(0);
    lo.SetUint64((uint64)x.Lo);
    return r.Add(r, lo);
}

public static nint Cmp(this Int128 x, Int128 y) {
    if (x.Hi > y.Hi) {
        return 1;
    }

    if (x.Hi < y.Hi) {
        return -1;
    }

    if ((uint64)x.Lo > (uint64)y.Lo) {
        return 1;
    }

    if ((uint64)x.Lo < (uint64)y.Lo) {
        return -1;
    }

    return 0;
}

public static (Int128 r, bool ok) Neg(this Int128 x) {
    Int128 r = default;

    if (x == (new Int128(Hi: math.MinInt64))) {
        return (x, false);
    }

    x.Lo = ~x.Lo;
    x.Hi = ~x.Hi;
    (r, _) = x.Add(new Int128(Lo: 1));
    return (r, true);
}

public static (Int128 r, error err) SetBigInt(this ж<Int128> Ꮡx, ж<big.Int> Ꮡy) {
    Int128 r = default;
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    if (Ꮡy.Cmp(MaxInt128) > 0) {
        var ᴛ1 = fmt.Errorf("%T.SetInt: overflow"u8, Ꮡx.OrTypedNil());
        return (x, ᴛ1);
    }
    if (Ꮡy.Cmp(MinInt128) < 0) {
        var ᴛ2 = fmt.Errorf("%T.SetInt: underflow"u8, Ꮡx.OrTypedNil());
        return (x, ᴛ2);
    }
    var neg = y.Sign() < 0;
    ref var z = ref heap(new big.Int(), out var Ꮡz);
    Ꮡz.Set(Ꮡy);
    if (neg) {
        Ꮡz.Neg(Ꮡz);
    }
    r.Lo = z.Int64();
    Ꮡz.Rsh(Ꮡz, 64);
    r.Hi = z.Int64();
    if (neg) {
        (r, _) = r.Neg();
    }
    x = r;
    return (r, default);
}

public static Int128 /*r*/ SetInt64(this ref Int128 x, int64 y) {
    Int128 r = default;

    r.Lo = y;
    if (y >= 0) {
        r.Hi = 0;
        x = r;
        return r;
    }

    r.Hi = -1;
    x = r;
    return r;
}

public static Int128 /*r*/ SetUint64(this ref Int128 x, uint64 y) {
    Int128 r = default;

    r = new Int128(Lo: (int64)y);
    x = r;
    return r;
}

public static nint Sign(this Int128 x) {
    if (x.Hi < 0) {
        return -1;
    }

    if (x.Hi != 0 || x.Lo != 0) {
        return 1;
    }

    return 0;
}

public static @string String(this Int128 x) { return x.BigInt().String(); }

} partial class mathutil_package {

public static Int128 /*r*/ NewInt128FromInt64(int64 n) {
    Int128 r = default;

    r.Lo = n;
    if (n < 0) {
        r.Hi = -1;
    }
    return r;
}

public static Int128 /*r*/ NewInt128FromUint64(uint64 n) { return new Int128(Lo: (int64)n); }

public static Int128 /*r*/ NewInt128FromFloat32(float32 n) {
    ref var r = ref heap(new Int128(), out var Ꮡr);

    if (n >= (float32)minInt128 && n <= (float32)maxInt128) {
        if (n >= math.MinInt64 && n <= math.MaxInt64) {
            return NewInt128FromInt64((int64)n);
        }

        var f = big.NewFloat((float64)n);
        var (bi, _) = f.Int(nil);
        Ꮡr.SetBigInt(bi);
    }
    return r;
}

public static Int128 /*r*/ NewInt128FromFloat64(float64 n) {
    ref var r = ref heap(new Int128(), out var Ꮡr);

    if (n >= (float64)minInt128 && n <= (float64)maxInt128) {
        if (n >= math.MinInt64 && n <= math.MaxInt64) {
            return NewInt128FromInt64((int64)n);
        }

        var f = big.NewFloat(n);
        var (bi, _) = f.Int(nil);
        Ꮡr.SetBigInt(bi);
    }
    return r;
}

partial struct Uint128 {
    public uint64 Lo;
    public uint64 Hi;
}

public static Uint128 /*r*/ NewUint128FromInt64(int64 n) {
    Uint128 r = default;

    r.Lo = (uint64)n;
    if (n < 0) {
        r.Hi = ~(uint64)0;
    }
    return r;
}

public static Uint128 /*r*/ NewUint128FromUint64(uint64 n) { return new Uint128(Lo: n); }

public static Uint128 /*r*/ NewUint128FromFloat32(float32 n) {
    ref var r = ref heap(new Uint128(), out var Ꮡr);

    if (n >= 0F) {
        if (n <= math.MaxUint64) {
            return NewUint128FromUint64((uint64)n);
        }

        var f = big.NewFloat((float64)n);
        var (bi, _) = f.Int(nil);
        Ꮡr.SetBigInt(bi);
    }
    return r;
}

public static Uint128 /*r*/ NewUint128FromFloat64(float64 n) {
    ref var r = ref heap(new Uint128(), out var Ꮡr);

    if (n >= 0D && n <= (float64)maxUint128) {
        if (n <= math.MaxUint64) {
            return NewUint128FromUint64((uint64)n);
        }

        var f = big.NewFloat(n);
        var (bi, _) = f.Int(nil);
        Ꮡr.SetBigInt(bi);
    }
    return r;
}

} partial class mathutilᴛUint128 {

public static (Uint128 r, error err) SetBigInt(this ж<Uint128> Ꮡx, ж<big.Int> Ꮡy) {
    Uint128 r = default;
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    if (y.Sign() < 0 || Ꮡy.Cmp(MaxUint128) > 0) {
        var ᴛ3 = fmt.Errorf("%T.SetInt: overflow"u8, Ꮡx.OrTypedNil());
        return (x, ᴛ3);
    }

    ref var z = ref heap(new big.Int(), out var Ꮡz);
    Ꮡz.Set(Ꮡy);
    r.Lo = z.Uint64();
    Ꮡz.Rsh(Ꮡz, 64);
    r.Hi = z.Uint64();
    x = r;
    return (r, default);
}

} // end mathutilᴛUint128
