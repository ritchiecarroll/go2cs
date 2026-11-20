namespace go.github.com.remyoudompheng;

using big = math.big_package;
using @unsafe = unsafe_package;
using math;
using static bigfft_package;

partial class bigfft_package {

internal const nint _W = /* int(unsafe.Sizeof(big.Word(0)) * 8) */ 64;

partial struct nat /*[]global::go.math.big_package.Word*/;

} partial class bigfftᴛnat {

internal static @string String(this nat n) {
    var v = @new<big.Int>();
    v.SetBits(n);
    return v.String();
}

} partial class bigfft_package {

internal static nint fftThreshold = 1800;

public static ж<big.Int> Mul(ж<big.Int> Ꮡx, ж<big.Int> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    nint xwords = len(x.Bits());
    nint ywords = len(y.Bits());
    if (xwords > fftThreshold && ywords > fftThreshold) {
        return mulFFT(Ꮡx, Ꮡy);
    }
    return @new<big.Int>().Mul(Ꮡx, Ꮡy);
}

internal static ж<big.Int> mulFFT(ж<big.Int> Ꮡx, ж<big.Int> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    nat xb = x.Bits();
    nat yb = y.Bits();
    var zb = fftmul(xb, yb);
    var z = @new<big.Int>();
    z.SetBits(zb);
    if (x.Sign() * y.Sign() < 0) {
        z.Neg(z);
    }
    return z;
}

internal static nat fftmul(nat x, nat y) {
    var (k, m) = fftSize(x, y);
    var xp = polyFromNat(x, k, m);
    ref var yp = ref heap<poly>(polyFromNat(y, k, m), out var Ꮡyp);
    var rp = xp.Mul(Ꮡyp);
    return rp.Int();
}

[GoLazy] partial class lazyᴛfftSizeThreshold { internal static array<int64> fftSizeThreshold = new int64[]{0, 0, 0,
    (int64)4 << 10, (int64)8 << 10, (int64)16 << 10,
    (int64)32 << 10, (int64)64 << 10, (int64)1 << 18, (int64)1 << 20, (int64)3 << 20,
    (int64)8 << 20, (int64)30 << 20, (int64)100 << 20, (int64)300 << 20, (int64)600 << 20
}.array(); }

internal static (nuint k, nint m) fftSize(nat x, nat y) {
    nuint k = default;
    nint m = default;

    nint words = len(x) + len(y);
    var bits = (int64)words * (int64)_W;
    k = (nuint)len(fftSizeThreshold);
    foreach (var i in range(len(fftSizeThreshold))) {
        if (fftSizeThreshold[i] > bits) {
            k = (nuint)i;
            break;
        }
    }
    m = words.Rsh(k) + 1;
    return (k, m);
}

internal static nint valueSize(nuint k, nint m, nuint extra) {
    nint n = 2 * m * _W + (nint)k;
    nint K = ((nint)1).Lsh((k - extra));
    if (K < _W) {
        K = _W;
    }
    n = ((quo(n, K)) + 1) * K;
    return n / _W;
}

partial struct poly {
    internal nuint k;
    internal nint m;
    internal slice<nat> a;
}

internal static poly polyFromNat(nat x, nuint k, nint m) {
    var p = new poly(k: k, m: m);
    nint length = len(x) / m + 1;
    p.a = new slice<nat>(length);
    foreach (var i in range(len(p.a))) {
        if (len(x) < m) {
            p.a[i] = new nat(m);
            copy(p.a[i], x);
            break;
        }
        p.a[i] = x.slice(0, m);
        x = x.slice(m);
    }
    return p;
}

} partial class bigfftᴛpoly {

internal static nat Int(this ref poly p) {
    nint length = len(p.a) * p.m + 1;
    nint na = len(p.a); if (na > 0) {
        length += len(p.a[na - 1]);
    }
    var n = new nat(length);
    nint m = p.m;
    var np = n;
    foreach (var i in range(len(p.a))) {
        nint l = len(p.a[i]);
        Word c = addVV(np.slice(0, l), np.slice(0, l), p.a[i]);
        if (np[l] < ~((big.Word)((big.Word)0))) {
            np[l] += c;
        } else {
            addVW(np.slice(l), np.slice(l), c);
        }
        np = np.slice(m);
    }
    n = trim(n);
    return n;
}

} partial class bigfft_package {

internal static nat trim(nat n) {
    foreach (var i in range(len(n))) {
        if (n[len(n) - 1 - i] != 0) {
            return n.slice(0, len(n) - i);
        }
    }
    return default;
}

} partial class bigfftᴛpoly {

internal static poly Mul(this ref poly p, ж<poly> Ꮡq) {
    ref var q = ref Ꮡq.DerefOrNull();

    nint n = valueSize(p.k, p.m, 2);

    var pv = p.Transform(n);
    ref var qv = ref heap<polValues>(out var Ꮡqv);
    qv = q.Transform(n);
    var rv = pv.Mul(Ꮡqv);
    var r = rv.InvTransform();
    r.m = p.m;
    return r;
}

} partial class bigfft_package {

partial struct polValues {
    internal nuint k;
    internal nint n;
    internal slice<fermat> values;
}

} partial class bigfftᴛpoly {

internal static polValues Transform(this ref poly p, nint n) {
    nuint k = p.k;
    var inputbits = new slice<big.Word>((n + 1).Lsh(k));
    var input = new slice<fermat>(((nint)1).Lsh(k));
    var valbits = new slice<big.Word>((n + 1).Lsh(k));
    var values = new slice<fermat>(((nint)1).Lsh(k));
    foreach (var i in range(len(values))) {
        input[i] = inputbits.slice(i * (n + 1), (i + 1) * (n + 1));
        if (i < len(p.a)) {
            copy(input[i], p.a[i]);
        }
        values[i] = (fermat)(valbits.slice(i * (n + 1), (i + 1) * (n + 1)));
    }
    fourier(values, input, false, n, k);
    return new polValues(k, n, values);
}

} partial class bigfftᴛpolValues {

internal static poly InvTransform(this ref polValues v) {
    nuint k = v.k;
    nint n = v.n;

    var pbits = new slice<big.Word>((n + 1).Lsh(k));
    var p = new slice<fermat>(((nint)1).Lsh(k));
    foreach (var i in range(len(p))) {
        p[i] = (fermat)(pbits.slice(i * (n + 1), (i + 1) * (n + 1)));
    }
    fourier(p, v.values, true, n, k);
    var u = new fermat(n + 1);
    var a = new slice<nat>(((nint)1).Lsh(k));
    foreach (var i in range(len(p))) {
        u.Shift(p[i], -(nint)k);
        copy(p[i], u);
        a[i] = (nat)p[i];
    }
    return new poly(k: k, m: 0, a: a);
}

} partial class bigfftᴛpoly {

internal static polValues NTransform(this ref poly p, nint n) {
    nuint k = p.k;
    if (len(p.a) >= ((nint)1).Lsh(k)) {
        throw panic("Transform: len(p.a) >= 1<<k");
    }
    nint θshift = (n * _W).Rsh(k);
    var tbits = new slice<big.Word>((n + 1).Lsh(k));
    var twisted = new slice<fermat>(((nint)1).Lsh(k));
    var src = new fermat(n + 1);
    foreach (var i in range(len(twisted))) {
        twisted[i] = (fermat)(tbits.slice(i * (n + 1), (i + 1) * (n + 1)));
        if (i < len(p.a)) {
            foreach (var iΔ1 in range(len(src))) {
                src[iΔ1] = 0;
            }
            copy(src, p.a[i]);
            twisted[i].Shift(src, θshift * i);
        }
    }

    var valbits = new slice<big.Word>((n + 1).Lsh(k));
    var values = new slice<fermat>(((nint)1).Lsh(k));
    foreach (var i in range(len(values))) {
        values[i] = (fermat)(valbits.slice(i * (n + 1), (i + 1) * (n + 1)));
    }
    fourier(values, twisted, false, n, k);
    return new polValues(k, n, values);
}

} partial class bigfftᴛpolValues {

internal static poly InvNTransform(this ref polValues v) {
    nuint k = v.k;
    nint n = v.n;
    nint θshift = (n * _W).Rsh(k);

    var qbits = new slice<big.Word>((n + 1).Lsh(k));
    var q = new slice<fermat>(((nint)1).Lsh(k));
    foreach (var i in range(len(q))) {
        q[i] = (fermat)(qbits.slice(i * (n + 1), (i + 1) * (n + 1)));
    }
    fourier(q, v.values, true, n, k);

    var u = new fermat(n + 1);
    var a = new slice<nat>(((nint)1).Lsh(k));
    foreach (var i in range(len(q))) {
        u.Shift(q[i], -(nint)k - i * θshift);
        copy(q[i], u);
        a[i] = (nat)q[i];
    }
    return new poly(k: k, m: 0, a: a);
}

} partial class bigfft_package {

internal static void fourier(slice<fermat> dst, slice<fermat> src, bool backward, nint n, nuint k) {
    ref var rec = ref heap<Action<slice<fermat>, slice<fermat>, nuint>>(out var Ꮡrec);
    var tmp = new fermat(n + 1);
    var tmp2 = new fermat(n + 1);

    var tmpʗ1 = tmp;
    var tmp2ʗ1 = tmp2;
    rec = (slice<fermat> dstΔ1, slice<fermat> srcΔ1, nuint size) => {
        nuint idxShift = k - size;
        nint ω2shift = (4 * n * _W).Rsh(size);
        if (backward) {
            ω2shift = -ω2shift;
        }

        if (len(srcΔ1[0]) != n + 1 || len(dstΔ1[0]) != n + 1) {
            throw panic("len(src[0]) != n+1 || len(dst[0]) != n+1");
        }
        switch (size) {
        case 0: {
            copy(dstΔ1[0], srcΔ1[0]);
            return;
        }
        case 1: {
            dstΔ1[0].Add(srcΔ1[0], srcΔ1[((nint)1).Lsh(idxShift)]);
            dstΔ1[1].Sub(srcΔ1[0], srcΔ1[((nint)1).Lsh(idxShift)]);
            return;
        }}

        var dst1 = dstΔ1.slice(0, ((nint)1).Lsh((size - 1)));
        var dst2 = dstΔ1.slice(((nint)1).Lsh((size - 1)));
        Ꮡrec.ValueSlot(dst1, srcΔ1, size - 1);
        Ꮡrec.ValueSlot(dst2, srcΔ1.slice(((nint)1).Lsh(idxShift)), size - 1);

        foreach (var i in range(len(dst1))) {
            tmpʗ1.ShiftHalf(dst2[i], i * ω2shift, tmp2ʗ1);
            dst2[i].Sub(dst1[i], tmpʗ1);
            dst1[i].Add(dst1[i], tmpʗ1);
        }
    };
    rec(dst, src, k);
}

} partial class bigfftᴛpolValues {

internal static polValues /*r*/ Mul(this ref polValues p, ж<polValues> Ꮡq) {
    polValues r = default;
    ref var q = ref Ꮡq.DerefOrNull();

    nint n = p.n;
    (r.k, r.n) = (p.k, p.n);
    r.values = new slice<fermat>(len(p.values));
    var bits = new slice<big.Word>(len(p.values) * (n + 1));
    var buf = new fermat(8 * n);
    foreach (var i in range(len(r.values))) {
        r.values[i] = bits.slice(i * (n + 1), (i + 1) * (n + 1));
        var z = buf.Mul(p.values[i], q.values[i]);
        copy(r.values[i], z);
    }
    return r;
}

} // end bigfftᴛpolValues
