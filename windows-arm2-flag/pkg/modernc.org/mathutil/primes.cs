// Copyright (c) 2014 The mathutil Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using math = math_package;

partial class mathutil_package {

public static bool IsPrimeUint16(uint16 n) {
    return n > 0 && primes16[(uint16)(n - 1)] == 1;
}

public static (uint16 p, bool ok) NextPrimeUint16(uint16 n) {
    return ((uint16)(n + (uint16)primes16[n]), n < 65521);
}

public static bool IsPrime(uint32 n) {
    switch (ᐧ) {
    case {} when (n & 1) == 0: {
        return n == 2;
    }
    case {} when n % 3 is 0: {
        return n == 3;
    }
    case {} when n % 5 is 0: {
        return n == 5;
    }
    case {} when n % 7 is 0: {
        return n == 7;
    }
    case {} when n % 11 is 0: {
        return n == 11;
    }
    case {} when n % 13 is 0: {
        return n == 13;
    }
    case {} when n % 17 is 0: {
        return n == 17;
    }
    case {} when n % 19 is 0: {
        return n == 19;
    }
    case {} when n % 23 is 0: {
        return n == 23;
    }
    case {} when n % 29 is 0: {
        return n == 29;
    }
    case {} when n % 31 is 0: {
        return n == 31;
    }
    case {} when n % 37 is 0: {
        return n == 37;
    }
    case {} when n % 41 is 0: {
        return n == 41;
    }
    case {} when n % 43 is 0: {
        return n == 43;
    }
    case {} when n % 47 is 0: {
        return n == 47;
    }
    case {} when n % 53 is 0: {
        return n == 53;
    }
    case {} when n is < 65536: {
        return IsPrimeUint16((uint16)n);
    }
    default: {
        var mod = ModPowUint32(2, (n + 1) / 2, n);
        if (mod != 2 && mod != n - 2) {
            return false;
        }
        var blk = Ꮡlohi.at<lohiᴛ1>((ulong)((n >> 24)));
        nint lo = blk.Value.lo;
        nint hi = blk.Value.hi;
        while (lo <= hi) {
            nint index = (lo + hi) >> 1;
            var liar = liars[index];
            switch (ᐧ) {
            case {} when n > liar: {
                lo = index + 1;
                break;
            }
            case {} when n < liar: {
                hi = index - 1;
                break;
            }
            default: {
                return false;
            }}

        }
        return true;
    }}

}

public static bool IsPrimeUint64(uint64 n) {
    switch (ᐧ) {
    case {} when n % 2 is 0: {
        return n == 2;
    }
    case {} when n % 3 is 0: {
        return n == 3;
    }
    case {} when n % 5 is 0: {
        return n == 5;
    }
    case {} when n % 7 is 0: {
        return n == 7;
    }
    case {} when n % 11 is 0: {
        return n == 11;
    }
    case {} when n % 13 is 0: {
        return n == 13;
    }
    case {} when n % 17 is 0: {
        return n == 17;
    }
    case {} when n % 19 is 0: {
        return n == 19;
    }
    case {} when n % 23 is 0: {
        return n == 23;
    }
    case {} when n % 29 is 0: {
        return n == 29;
    }
    case {} when n % 31 is 0: {
        return n == 31;
    }
    case {} when n % 37 is 0: {
        return n == 37;
    }
    case {} when n % 41 is 0: {
        return n == 41;
    }
    case {} when n % 43 is 0: {
        return n == 43;
    }
    case {} when n % 47 is 0: {
        return n == 47;
    }
    case {} when n % 53 is 0: {
        return n == 53;
    }
    case {} when n % 59 is 0: {
        return n == 59;
    }
    case {} when n % 61 is 0: {
        return n == 61;
    }
    case {} when n % 67 is 0: {
        return n == 67;
    }
    case {} when n % 71 is 0: {
        return n == 71;
    }
    case {} when n % 73 is 0: {
        return n == 73;
    }
    case {} when n % 79 is 0: {
        return n == 79;
    }
    case {} when n % 83 is 0: {
        return n == 83;
    }
    case {} when n % 89 is 0: {
        return n == 89;
    }
    case {} when n <= math.MaxUint16: {
        return IsPrimeUint16((uint16)n);
    }
    case {} when n <= math.MaxUint32: {
        return ProbablyPrimeUint32((uint32)n, 11000544) && ProbablyPrimeUint32((uint32)n, 31481107);
    }
    case {} when n is < 105936894253UL: {
        return ProbablyPrimeUint64_32(n, 2) && ProbablyPrimeUint64_32(n, 1005905886) && ProbablyPrimeUint64_32(n, 1340600841);
    }
    case {} when n is < 31858317218647UL: {
        return ProbablyPrimeUint64_32(n, 2) && ProbablyPrimeUint64_32(n, 642735) && ProbablyPrimeUint64_32(n, 553174392) && ProbablyPrimeUint64_32(n, 3046413974U);
    }
    case {} when n is < 3071837692357849UL: {
        return ProbablyPrimeUint64_32(n, 2) && ProbablyPrimeUint64_32(n, 75088) && ProbablyPrimeUint64_32(n, 642735) && ProbablyPrimeUint64_32(n, 203659041) && ProbablyPrimeUint64_32(n, 3613982119U);
    }
    default: {
        return ProbablyPrimeUint64_32(n, 2) && ProbablyPrimeUint64_32(n, 325) && ProbablyPrimeUint64_32(n, 9375) && ProbablyPrimeUint64_32(n, 28178) && ProbablyPrimeUint64_32(n, 450775) && ProbablyPrimeUint64_32(n, 9780504) && ProbablyPrimeUint64_32(n, 1795265022);
    }}

}

public static (uint32 p, bool ok) NextPrime(uint32 n) {
    uint32 p = default;
    bool ok = default;

    switch (ᐧ) {
    case {} when n is < 65521: {
        var (p16, _) = NextPrimeUint16((uint16)n);
        return ((uint32)p16, true);
    }
    case {} when n >= (uint32)(math.MaxUint32 - 4): {
        return (p, ok);
    }}

    n++;
    uint32 d0 = default;
    uint32 d = default;
    var mod = n % 6;
    switch (mod) {
    case 0: {
        (d0, d) = (1, 4);
        break;
    }
    case 1: {
        d = 4;
        break;
    }
    case 2 or 3 or 4: {
        (d0, d) = (5 - mod, 2);
        break;
    }
    case 5: {
        d = 2;
        break;
    }}

    p = n + d0;
    if (p < n) {
        return (p, ok);
    }

    for (;;) {
        if (IsPrime(p)) {
            return (p, true);
        }

        var p0 = p;
        p += d;
        if (p < p0) {
            break;
        }

        d ^= (uint32)(6);
    }
    return (p, ok);
}

public static (uint64 p, bool ok) NextPrimeUint64(uint64 n) {
    uint64 p = default;
    bool ok = default;

    switch (ᐧ) {
    case {} when n is < 65521: {
        var (p16, _) = NextPrimeUint16((uint16)n);
        return ((uint64)p16, true);
    }
    case {} when n is >= 18446744073709551557UL: {
        return (p, ok);
    }}

    n++;
    uint64 d0 = default;
    uint64 d = default;
    var mod = n % 6;
    switch (mod) {
    case 0: {
        (d0, d) = (1, 4);
        break;
    }
    case 1: {
        d = 4;
        break;
    }
    case 2 or 3 or 4: {
        (d0, d) = (5 - mod, 2);
        break;
    }
    case 5: {
        d = 2;
        break;
    }}

    p = n + d0;
    if (p < n) {
        return (p, ok);
    }

    for (;;) {
        ok = IsPrimeUint64(p); if (ok) {
            break;
        }

        var p0 = p;
        p += d;
        if (p < p0) {
            break;
        }

        d ^= (uint64)(6);
    }
    return (p, ok);
}

partial struct FactorTerm {
    public uint32 Prime;
    public uint32 Power;
}

partial struct FactorTerms /*[]FactorTerm*/;

public static FactorTerms /*f*/ FactorInt(uint32 n) {
    FactorTerms f = default;

    switch (ᐧ) {
    case {} when n is < 2: {
        return f;
    }
    case {} when IsPrime(n): {
        return slice<FactorTerm>([new(n, 1)]);
    }}

    f = new slice<FactorTerm>(9);
    nint w = 0;
    for (nint p = 2; p < len(primes16); p += (nint)primes16[p]) {
        if ((nuint)(p * p) > (nuint)n) {
            break;
        }

        var power = (uint32)0;
        while (n % (uint32)p == 0) {
            n /= (uint32)p;
            power++;
        }
        if (power != 0) {
            f[w] = new FactorTerm((uint32)p, power);
            w++;
        }
        if (n == 1) {
            break;
        }
    }
    if (n != 1) {
        f[w] = new FactorTerm(n, 1);
        w++;
    }
    return f.slice(0, w);
}

public static slice<uint32> /*r*/ PrimorialProductsUint32(uint32 lo, uint32 hi, uint32 max) {
    slice<uint32> r = default;

    var (lo64, hi64) = ((int64)lo, (int64)hi);
    if (max > 31) {
        max = 31;
    }

    ref var f = ref heap<Action<int64, int64, uint32>>(out var Ꮡf);
    f = (int64 n, int64 p, uint32 emax) => {
        var e = (uint32)1;
        while (n <= hi64 && e <= emax) {
            n *= p;
            if (n >= lo64 && n <= hi64) {
                r = append(r, (uint32)n);
            }
            if (n < hi64) {
                var (pΔ1, _) = NextPrime((uint32)p);
                Ꮡf.ValueSlot(n, (int64)pΔ1, e);
            }
            e++;
        }
    };

    f(1, 2, max);
    return r;
}

} // end mathutil_package
