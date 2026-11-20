// Copyright (c) 2016 The mathutil Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using fmt = fmt_package;
using big = go.math.big_package;
using go.math;

partial class mathutil_package {

internal static uint64 abs(nint n) {
    if (n >= 0) {
        return (uint64)n;
    }

    return (uint64)(-n);
}

public static (nint ds, nint d, error) QuadPolyDiscriminant(nint a, nint b, nint c) {
    nint ds = default;

    if (2 * BitLenUint64(abs(b)) > (nint)(IntBits - 1) || 2 + BitLenUint64(abs(a)) + BitLenUint64(abs(c)) > (nint)(IntBits - 1)) {
        return (0, 0, fmt.Errorf("overflow"u8));
    }

    ds = b * b - 4 * a * c;
    nint s = ds;
    if (s < 0) {
        s = -s;
    }
    var d64 = SqrtUint64((uint64)s);
    if (d64 * d64 != (uint64)s) {
        return (ds, -1, default);
    }

    return (ds, (nint)d64, default);
}

partial struct PolyFactor {
    public nint P, Q;
}

public static (nint content, slice<PolyFactor> primitivePart, error) QuadPolyFactors(nint a, nint b, nint c) {
    nint content = default;

    content = (nint)GCDUint64(abs(a), GCDUint64(abs(b), abs(c)));
    switch (ᐧ) {
    case {} when content is 0: {
        content = 1;
        break;
    }
    case {} when content is > 0: {
        if (a < 0 || a == 0 && b < 0) {
            content = -content;
        }
        break;
    }}

    a = quo(a, content);
    b = quo(b, content);
    c = quo(c, content);
    if (a == 0) {
        if (b == 0) {
            return (content, slice<PolyFactor>([new(0, c)]), default);
        }

        if (b < 0 && c < 0) {
            b = -b;
            c = -c;
        }
        if (b < 0) {
            b = -b;
            c = -c;
        }
        return (content, slice<PolyFactor>([new(b, c)]), default);
    }

    var (ds, d, err) = QuadPolyDiscriminant(a, b, c);
    if (err != nil) {
        return (0, default, err);
    }

    if (ds < 0 || d < 0) {
        return (0, default, default);
    }

    nint x1num = -b + d;
    nint x1denom = 2 * a;
    nint gcd = (nint)GCDUint64(abs(x1num), abs(x1denom));
    x1num = quo(x1num, gcd);
    x1denom = quo(x1denom, gcd);

    nint x2num = -b - d;
    nint x2denom = 2 * a;
    gcd = (nint)GCDUint64(abs(x2num), abs(x2denom));
    x2num = quo(x2num, gcd);
    x2denom = quo(x2denom, gcd);

    return (content, slice<PolyFactor>([new(x1denom, -x1num), new(x2denom, -x2num)]), default);
}

public static (ж<big.Int> ds, ж<big.Int> d) QuadPolyDiscriminantBig(ж<big.Int> a, ж<big.Int> b, ж<big.Int> c) {
    ж<big.Int> ds = default;
    ж<big.Int> d = default;

    ds = big.NewInt(0).Set(b);
    ds.Mul(ds, b);
    var x = big.NewInt(4);
    x.Mul(x, a);
    x.Mul(x, c);
    ds.Sub(ds, x);

    var s = big.NewInt(0).Set(ds);
    if (s.Sign() < 0) {
        s.Neg(s);
    }

    if (s.Bit(1) != 0) {
        return (ds, default);
    }

    d = SqrtBig(s);
    x.Set(d);
    x.Mul(x, x);
    if (x.Cmp(s) != 0) {
        d = default;
    }
    return (ds, d);
}

partial struct PolyFactorBig {
    public ж<big.Int> P, Q;
}

public static (ж<big.Int> content, slice<PolyFactorBig> primitivePart) QuadPolyFactorsBig(ж<big.Int> Ꮡa, ж<big.Int> Ꮡb, ж<big.Int> Ꮡc) {
    ж<big.Int> content = default;
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();
    ref var c = ref Ꮡc.DerefOrNull();

    content = bigGCD(bigAbs(Ꮡa), bigGCD(bigAbs(Ꮡb), bigAbs(Ꮡc)));
    switch (ᐧ) {
    case {} when content.Sign() is 0: {
        content.SetInt64(1);
        break;
    }
    case {} when content.Sign() is > 0: {
        if (a.Sign() < 0 || a.Sign() == 0 && b.Sign() < 0) {
            content = bigNeg(content);
        }
        break;
    }}

    Ꮡa = bigDiv(Ꮡa, content); a = ref Ꮡa.DerefOrNull();
    Ꮡb = bigDiv(Ꮡb, content); b = ref Ꮡb.DerefOrNull();
    Ꮡc = bigDiv(Ꮡc, content); c = ref Ꮡc.DerefOrNull();

    if (a.Sign() == 0) {
        if (b.Sign() == 0) {
            return (content, slice<PolyFactorBig>([new(big.NewInt(0), Ꮡc)]));
        }

        if (b.Sign() < 0 && c.Sign() < 0) {
            Ꮡb = bigNeg(Ꮡb); b = ref Ꮡb.DerefOrNull();
            Ꮡc = bigNeg(Ꮡc); c = ref Ꮡc.DerefOrNull();
        }
        if (b.Sign() < 0) {
            Ꮡb = bigNeg(Ꮡb); b = ref Ꮡb.DerefOrNull();
            Ꮡc = bigNeg(Ꮡc); c = ref Ꮡc.DerefOrNull();
        }
        return (content, slice<PolyFactorBig>([new(Ꮡb, Ꮡc)]));
    }

    var (ds, d) = QuadPolyDiscriminantBig(Ꮡa, Ꮡb, Ꮡc);
    if (ds.Sign() < 0 || d == nil) {
        return (default, default);
    }

    var x1num = bigAdd(bigNeg(Ꮡb), d);
    var x1denom = bigMul(_2, Ꮡa);
    var gcd = bigGCD(bigAbs(x1num), bigAbs(x1denom));
    x1num = bigDiv(x1num, gcd);
    x1denom = bigDiv(x1denom, gcd);

    var x2num = bigAdd(bigNeg(Ꮡb), bigNeg(d));
    var x2denom = bigMul(_2, Ꮡa);
    gcd = bigGCD(bigAbs(x2num), bigAbs(x2denom));
    x2num = bigDiv(x2num, gcd);
    x2denom = bigDiv(x2denom, gcd);

    return (content, slice<PolyFactorBig>([new(x1denom, bigNeg(x1num)), new(x2denom, bigNeg(x2num))]));
}

internal static ж<big.Int> bigAbs(ж<big.Int> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    Ꮡn = big.NewInt(0).Set(Ꮡn); n = ref Ꮡn.DerefOrNull();
    if (n.Sign() >= 0) {
        return Ꮡn;
    }

    return Ꮡn.Neg(Ꮡn);
}

internal static ж<big.Int> bigDiv(ж<big.Int> Ꮡa, ж<big.Int> b) {
    ref var a = ref Ꮡa.DerefOrNull();

    Ꮡa = big.NewInt(0).Set(Ꮡa); a = ref Ꮡa.DerefOrNull();
    return Ꮡa.Div(Ꮡa, b);
}

internal static ж<big.Int> bigGCD(ж<big.Int> Ꮡa, ж<big.Int> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    Ꮡa = big.NewInt(0).Set(Ꮡa); a = ref Ꮡa.DerefOrNull();
    Ꮡb = big.NewInt(0).Set(Ꮡb); b = ref Ꮡb.DerefOrNull();
    while (b.Sign() != 0) {
        var c = big.NewInt(0);
        c.Mod(Ꮡa, Ꮡb);
        (Ꮡa, Ꮡb) = (Ꮡb, c); a = ref Ꮡa.DerefOrNull(); b = ref Ꮡb.DerefOrNull();
    }
    return Ꮡa;
}

internal static ж<big.Int> bigNeg(ж<big.Int> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    Ꮡn = big.NewInt(0).Set(Ꮡn); n = ref Ꮡn.DerefOrNull();
    return Ꮡn.Neg(Ꮡn);
}

internal static ж<big.Int> bigMul(ж<big.Int> a, ж<big.Int> b) {
    var r = big.NewInt(0).Set(a);
    return r.Mul(r, b);
}

internal static ж<big.Int> bigAdd(ж<big.Int> a, ж<big.Int> b) {
    var r = big.NewInt(0).Set(a);
    return r.Add(r, b);
}

} // end mathutil_package
