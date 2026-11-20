// Copyright (c) 2014 The mathutil Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using fmt = fmt_package;
using math = math_package;
using big = go.math.big_package;
using go.math;
using static mathutil_package;

partial class mathutil_package {

partial struct FC32 {
    internal int64 cycle;
    internal int64 delta;
    internal slice<slice<int64>> factors;
    internal nint lo;
    internal slice<nint> mods;
    internal int64 pos;
    internal slice<int64> primes;
    internal slice<int64> set;
}

public static (ж<FC32> r, error err) NewFC32(nint lo, nint hi, bool hq) {
    ж<FC32> r = default;
    error err = default;

    if (lo > hi) {
        return (default, fmt.Errorf("invalid range %d > %d"u8, lo, hi));
    }

    if ((uint64)hi - (uint64)lo > math.MaxUint32) {
        return (default, fmt.Errorf("range out of int32 limits %d, %d"u8, lo, hi));
    }

    ref var delta = ref heap<int64>((int64)hi - (int64)lo, out var Ꮡdelta);
    ref var n = ref heap<int64>(out var Ꮡn);
    n = (int64)1;
    var set = slice<int64>([]);
    var p = (uint32)2;
    if (hq) {
        p++;
    }
    for (;;) {
        set = append(set, (int64)p);
        n *= (int64)p;
        if (n > delta) {
            break;
        }
        (p, _) = NextPrime(p);
    }

    nint i = -1;
    if (n > 2 * (delta + 1)) {
        foreach (var (j, pΔ1) in range(set)) {
            var q = quo(n, pΔ1);
            if (q < delta + 1) {
                break;
            }

            i = j;
        }
    }
    if (i >= 0) {
        n = quo(n, set[i]);
        set = delete(set, i);
    }
    r = Ꮡ(new FC32(
        cycle: n,
        delta: delta,
        factors: new slice<slice<int64>>(len(set)),
        lo: lo,
        mods: new slice<nint>(len(set)),
        primes: set
    ));
    r.Seed(1);
    return (r, err);
}

} partial class mathutilᴛFC32 {

public static int64 Cycle(this ref FC32 r) {
    return r.cycle;
}

public static nint Next(this ref FC32 r) {
    return r.step(1);
}

public static int64 Pos(this ref FC32 r) {
    return r.pos;
}

public static nint Prev(this ref FC32 r) {
    return r.step(-1);
}

public static void Seed(this ref FC32 r, int64 seed) {
    var u = (uint64)seed;
    r.set = mix(r.primes, ref u);
    var n = (int64)1;
    foreach (var (i, p) in range(r.set)) {
        var k = new slice<int64>((nint)(p));
        var v = (int64)0;
        foreach (var j in range(len(k))) {
            k[j] = v;
            v += n;
        }
        n *= p;
        r.factors[i] = mix(k, ref u);
    }
}

public static void Seek(this ref FC32 r, int64 pos) {
    if (pos < 0) {
        pos = -pos;
    }
    pos = rem(pos, r.cycle);
    r.pos = pos;
    foreach (var (i, p) in range(r.set)) {
        r.mods[i] = (nint)(rem(pos, p));
    }
}

internal static nint step(this ref FC32 r, nint dir) {
    for (;;) {
        var y = (int64)0;
        var pos = r.pos;
        pos += (int64)dir;
        switch (ᐧ) {
        case {} when pos is < 0: {
            pos = r.cycle - 1;
            break;
        }
        case {} when pos >= r.cycle: {
            pos = 0;
            break;
        }}

        r.pos = pos;
        foreach (var (i, vᴛ1) in range(r.mods)) {
            var mod = vᴛ1;

            mod += dir;
            nint p = (nint)r.set[i];
            switch (ᐧ) {
            case {} when mod is < 0: {
                mod = p - 1;
                break;
            }
            case {} when mod >= p: {
                mod = 0;
                break;
            }}

            r.mods[i] = mod;
            y += r.factors[i][mod];
        }
        if (y <= r.delta) {
            return (nint)y + r.lo;
        }
    }
}

} partial class mathutil_package {

internal static slice<int64> /*y*/ delete(slice<int64> set, nint i) {
    slice<int64> y = default;

    foreach (var (j, v) in range(set)) {
        if (j != i) {
            y = append(y, v);
        }
    }
    return y;
}

internal static slice<int64> /*y*/ mix(slice<int64> set, ref uint64 seed) {
    slice<int64> y = default;

    while (len(set) != 0) {
        seed = rol(seed);
        nint i = (nint)(seed % (uint64)len(set));
        y = append(y, set[i]);
        set = delete(set, i);
    }
    return y;
}

internal static uint64 /*y*/ rol(uint64 u) {
    uint64 y = default;

    y = u << 1;
    if ((int64)u < 0) {
        y |= (uint64)(1);
    }
    return y;
}

partial struct FCBig {
    internal ж<big.Int> cycle;
    internal ж<big.Int> delta;
    internal slice<slice<ж<big.Int>>> factors;
    internal ж<big.Int> lo;
    internal slice<nint> mods;
    internal ж<big.Int> pos;
    internal slice<int64> primes;
    internal slice<int64> set;
}

public static (ж<FCBig> r, error err) NewFCBig(ж<big.Int> lo, ж<big.Int> hi, bool hq) {
    ж<FCBig> r = default;
    error err = default;

    if (lo.Cmp(hi) > 0) {
        return (default, fmt.Errorf("invalid range %d > %d"u8, lo.OrTypedNil(), hi.OrTypedNil()));
    }

    var delta = big.NewInt(0);
    delta.Add(delta, hi).Sub(delta, lo);

    var (n, set, pp, p) = (big.NewInt(1), slice<int64>([]), big.NewInt(0), (uint32)2);
    if (hq) {
        p++;
    }
    for (;;) {
        set = append(set, (int64)p);
        pp.SetInt64((int64)p);
        n.Mul(n, pp);
        if (n.Cmp(delta) > 0) {
            break;
        }
        (p, _) = NextPrime(p);
    }

    var dd1 = big.NewInt(1);
    dd1.Add(dd1, delta);
    var dd2 = big.NewInt(0);
    dd2.Lsh(dd1, 1);
    nint i = -1;
    if (n.Cmp(dd2) > 0) {
        var q = big.NewInt(0);
        foreach (var (j, pΔ1) in range(set)) {
            pp.SetInt64(pΔ1);
            q.Set(n);
            q.Div(q, pp);
            if (q.Cmp(dd1) < 0) {
                break;
            }

            i = j;
        }
    }
    if (i >= 0) {
        pp.SetInt64(set[i]);
        n.Div(n, pp);
        set = delete(set, i);
    }
    r = Ꮡ(new FCBig(
        cycle: n,
        delta: delta,
        factors: new slice<slice<ж<big.Int>>>(len(set)),
        lo: lo,
        mods: new slice<nint>(len(set)),
        pos: big.NewInt(0),
        primes: set
    ));
    r.Seed(1);
    return (r, err);
}

} partial class mathutilᴛFCBig {

public static ж<big.Int> Cycle(this ref FCBig r) {
    return r.cycle;
}

public static ж<big.Int> Next(this ref FCBig r) {
    return r.step(1);
}

public static ж<big.Int> Pos(this ref FCBig r) {
    return r.pos;
}

public static ж<big.Int> Prev(this ref FCBig r) {
    return r.step(-1);
}

public static void Seed(this ref FCBig r, int64 seed) {
    var u = (uint64)seed;
    r.set = mix(r.primes, ref u);
    var n = big.NewInt(1);
    var v = big.NewInt(0);
    var pp = big.NewInt(0);
    foreach (var (i, p) in range(r.set)) {
        var k = new slice<ж<big.Int>>((nint)(p));
        v.SetInt64(0);
        foreach (var j in range(len(k))) {
            k[j] = big.NewInt(0);
            k[j].Set(v);
            v.Add(v, n);
        }
        pp.SetInt64(p);
        n.Mul(n, pp);
        r.factors[i] = mixBig(k, ref u);
    }
}

public static void Seek(this ref FCBig r, ж<big.Int> Ꮡpos) {
    ref var pos = ref Ꮡpos.DerefOrNull();

    r.pos.Set(Ꮡpos);
    r.pos.Abs(r.pos);
    r.pos.Mod(r.pos, r.cycle);
    var mod = big.NewInt(0);
    var pp = big.NewInt(0);
    foreach (var (i, p) in range(r.set)) {
        pp.SetInt64(p);
        r.mods[i] = (nint)mod.Mod(r.pos, pp).Int64();
    }
}

internal static ж<big.Int> /*y*/ step(this ref FCBig r, nint dir) {
    ж<big.Int> y = default;

    y = big.NewInt(0);
    var d = big.NewInt((int64)dir);
    for (;;) {
        r.pos.Add(r.pos, d);
        switch (ᐧ) {
        case {} when r.pos.Sign() is < 0: {
            r.pos.Add(r.pos, r.cycle);
            break;
        }
        case {} when r.pos.Cmp(r.cycle) is >= 0: {
            r.pos.SetInt64(0);
            break;
        }}

        foreach (var (i, vᴛ1) in range(r.mods)) {
            var mod = vᴛ1;

            mod += dir;
            nint p = (nint)r.set[i];
            switch (ᐧ) {
            case {} when mod is < 0: {
                mod = p - 1;
                break;
            }
            case {} when mod >= p: {
                mod = 0;
                break;
            }}

            r.mods[i] = mod;
            y.Add(y, r.factors[i][mod]);
        }
        if (y.Cmp(r.delta) <= 0) {
            y.Add(y, r.lo);
            return y;
        }
        y.SetInt64(0);
    }
}

} partial class mathutil_package {

internal static slice<ж<big.Int>> /*y*/ deleteBig(slice<ж<big.Int>> set, nint i) {
    slice<ж<big.Int>> y = default;

    foreach (var (j, v) in range(set)) {
        if (j != i) {
            y = append(y, v);
        }
    }
    return y;
}

internal static slice<ж<big.Int>> /*y*/ mixBig(slice<ж<big.Int>> set, ref uint64 seed) {
    slice<ж<big.Int>> y = default;

    while (len(set) != 0) {
        seed = rol(seed);
        nint i = (nint)(seed % (uint64)len(set));
        y = append(y, set[i]);
        set = deleteBig(set, i);
    }
    return y;
}

} // end mathutil_package
