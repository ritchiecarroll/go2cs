namespace go.github.com.remyoudompheng;

using big = math.big_package;
using math;
using static bigfft_package;

partial class bigfft_package {

partial struct fermat /*nat*/;

} partial class bigfftᴛfermat {

internal static @string String(this fermat n) { return ((nat)n).String(); }

internal static void norm(this fermat z) {
    nint n = len(z) - 1;
    big.Word c = z[n];
    if (c == 0) {
        return;
    }
    if (z[0] >= c) {
        z[n] = 0;
        z[0] -= c;
        return;
    }
    subVW(z, z, c);
    if (c > 1) {
        z[n] -= c - 1;
        c = 1;
    }
    if (z[n] == 1) {
        z[n] = 0;
        return;
    } else {
        addVW(z, z, 1);
    }
}

internal static void Shift(this fermat z, fermat x, nint k) {
    if (len(z) != len(x)) {
        throw panic("len(z) != len(x) in Shift");
    }
    nint n = len(x) - 1;
    k = rem(k, 2 * n * _W);
    if (k < 0) {
        k += 2 * n * _W;
    }
    var neg = false;
    if (k >= n * _W) {
        k -= n * _W;
        neg = true;
    }

    nint kw = k / _W;
    nint kb = k % _W;

    z[n] = 1;
    if (!neg) {
        for (nint i = 0; i < kw; i++) {
            z[i] = 0;
        }
        copy(z.slice(kw), x.slice(0, n - kw));
        Word b = subVV(z.slice(0, kw + 1), z.slice(0, kw + 1), x.slice(n - kw));
        if (z[kw + 1] > 0) {
            z[kw + 1] -= b;
        } else {
            subVW(z.slice(kw + 1), z.slice(kw + 1), b);
        }
    } else {
        for (nint i = kw + 1; i < n; i++) {
            z[i] = 0;
        }
        copy(z.slice(0, kw + 1), x.slice(n - kw, n + 1));
        Word b = subVV(z.slice(kw, n), z.slice(kw, n), x.slice(0, n - kw));
        z[n] -= b;
    }
    if (z[n] > 0) {
        z[n]--;
    } else if (z[0] < ~((big.Word)((big.Word)0))) {
        z[0]++;
    } else {
        addVW(z, z, 1);
    }
    shlVU(z, z, (nuint)kb);
    z.norm();
}

internal static void ShiftHalf(this fermat z, fermat x, nint k, fermat tmp) {
    nint n = len(z) - 1;
    if (k % 2 == 0) {
        z.Shift(x, k / 2);
        return;
    }
    nint u = (k - 1) / 2;
    nint a = u + (3 * _W / 4) * n;
    nint b = u + (_W / 4) * n;
    z.Shift(x, a);
    tmp.Shift(x, b);
    z.Sub(z, tmp);
}

internal static fermat Add(this fermat z, fermat x, fermat y) {
    if (len(z) != len(x)) {
        throw panic("Add: len(z) != len(x)");
    }
    addVV(z, x, y);
    z.norm();
    return z;
}

internal static fermat Sub(this fermat z, fermat x, fermat y) {
    if (len(z) != len(x)) {
        throw panic("Add: len(z) != len(x)");
    }
    nint n = len(y) - 1;
    Word b = subVV(z.slice(0, n), x.slice(0, n), y.slice(0, n));
    b += y[n];
    z[n] = x[n];
    if (z[0] <= ~((big.Word)((big.Word)0)) - b) {
        z[0] += b;
    } else {
        addVW(z, z, b);
    }
    z.norm();
    return z;
}

internal static fermat Mul(this fermat z, fermat x, fermat y) {
    if (len(x) != len(y)) {
        throw panic("Mul: len(x) != len(y)");
    }
    nint n = len(x) - 1;
    if (n < 30) {
        z = z.slice(0, 2 * n + 2);
        basicMul(z, x, y);
        z = z.slice(0, 2 * n + 1);
    } else {
        ref var xi = ref heap(new big.Int(), out var Ꮡxi);
        ref var yi = ref heap(new big.Int(), out var Ꮡyi);
        ref var zi = ref heap(new big.Int(), out var Ꮡzi);
        Ꮡxi.SetBits(x);
        Ꮡyi.SetBits(y);
        Ꮡzi.SetBits(z);
        var zb = Ꮡzi.Mul(Ꮡxi, Ꮡyi).Bits();
        if (len(zb) <= n) {
            copy(z, zb);
            for (nint i = len(zb); i < len(z); i++) {
                z[i] = 0;
            }
            return z;
        }
        z = zb;
    }
    if (len(z) > 2 * n + 1) {
        throw panic("len(z) > 2n+1");
    }
    big.Word c1 = (big.Word)0;
    if (len(z) > 2 * n) {
        c1 = addVW(z.slice(0, n), z.slice(0, n), z[2 * n]);
    }
    big.Word c2 = (big.Word)0;
    if (len(z) >= 2 * n) {
        c2 = subVV(z.slice(0, n), z.slice(0, n), z.slice(n, 2 * n));
    } else {
        nint m = len(z) - n;
        c2 = subVV(z.slice(0, m), z.slice(0, m), z.slice(n));
        c2 = subVW(z.slice(m, n), z.slice(m, n), c2);
    }
    z = z.slice(0, n + 1);
    z[n] = c1;
    Word c = addVW(z, z, c2);
    if (c != 0) {
        throw panic("impossible");
    }
    z.norm();
    return z;
}

} partial class bigfft_package {

internal static void basicMul(fermat z, fermat x, fermat y) {
    for (nint i = 0; i < len(z); i++) {
        z[i] = 0;
    }
    foreach (var (i, d) in range<big.Word>(y)) {
        if (d != 0) {
            z[len(x) + i] = addMulVVW(z.slice(i, i + len(x)), x, d);
        }
    }
}

} // end bigfft_package
