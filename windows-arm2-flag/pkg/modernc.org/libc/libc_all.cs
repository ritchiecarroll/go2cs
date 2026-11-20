// Copyright 2024 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using bytes = bytes_package;
using math = math_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;
using go.sync;

partial class libc_package {

partial interface integer<ΔT> /*operators = Sum, Arithmetic, Integer, Comparable, Ordered*/ {
    //  Type constraints: ~int | ~int32 | ~int64 | ~uint | ~uint32 | ~uint64 | ~uintptr
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

public static T X__sync_add_and_fetch<T>(ж<TLS> t, uintptr p, T v)
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    switch (@unsafe.Sizeof(v)) {
    case var ᴛ when ᴛ == 4: {
        return ConvertToType<T>(atomic.AddInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)p), (int32)ConvertToUInt64<T>(v)));
    }
    case var ᴛ when ᴛ == 8: {
        return ConvertToType<T>(atomic.AddInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)p), (int64)ConvertToUInt64<T>(v)));
    }
    default: {
        throw panic(todo(""));
        break;
    }}

}

public static T X__sync_sub_and_fetch<T>(ж<TLS> t, uintptr p, T v)
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    switch (@unsafe.Sizeof(v)) {
    case var ᴛ when ᴛ == 4: {
        return ConvertToType<T>(atomic.AddInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)p), -(int32)ConvertToUInt64<T>(v)));
    }
    case var ᴛ when ᴛ == 8: {
        return ConvertToType<T>(atomic.AddInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)p), -(int64)ConvertToUInt64<T>(v)));
    }
    default: {
        throw panic(todo(""));
        break;
    }}

}

public static @string GoString(uintptr s) {
    if (s == 0) {
        return "";
    }

    var n = strlen(s); if (n != 0) {
        return (@string)@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)s), n);
    }

    return "";
}

public static slice<byte> GoBytes(uintptr s, nint len) {
    return @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)s), len);
}

public static int32 X__isfinitef(ж<TLS> tls, float32 f) {
    var d = (float64)f;
    if (!math.IsInf(d, 0) && !math.IsNaN(d)) {
        return 1;
    }

    return 0;
}

public static int32 X__isfinite(ж<TLS> tls, float64 d) {
    if (!math.IsInf(d, 0) && !math.IsNaN(d)) {
        return 1;
    }

    return 0;
}

public static int32 X__isfinitel(ж<TLS> tls, float64 d) {
    if (!math.IsInf(d, 0) && !math.IsNaN(d)) {
        return 1;
    }

    return 0;
}

internal static Tsize_t /*r*/ strlen(uintptr s) {
    if (s == 0) {
        return 0;
    }

    if (strlenUseIndexByte) {
        return strlenIndexByte(s);
    }

    return strlenWords(s);
}

public static partial Tsize_t /*r*/ Xstrlen(ж<TLS> t, uintptr s) {
    Tsize_t r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v s=%v, (%v:)", t.OrTypedNil(), s, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = strlen(s);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static Tsize_t /*r*/ _strlen(ref TLS t, uintptr s) {
    return strlen(s);
}

internal static int32 memcmp(uintptr s1, uintptr s2, Tsize_t n) {
    if (n == 0) {
        return 0;
    }
    return (int32)bytes.Compare(@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)s1), n), @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)s2), n));
}

public static int32 X__builtin_ilogb(ж<TLS> tls, float64 x) {
    return (int32)math.Ilogb(x);
}

public static int32 X__builtin_ilogbl(ж<TLS> tls, float64 x) {
    return (int32)math.Ilogb(x);
}

public static int32 X__builtin_ilogbf(ж<TLS> tls, float32 x) {
    return (int32)math.Ilogb((float64)x);
}

public static partial float64 X__builtin_acosh(ж<TLS> t, float64 x) { return Xacosh(t, x); }

public static partial float64 X__builtin_asinh(ж<TLS> t, float64 x) { return Xasinh(t, x); }

public static partial float32 X__builtin_asinhf(ж<TLS> t, float32 x) { return Xasinhf(t, x); }

public static partial float64 X__builtin_atanh(ж<TLS> t, float64 x) { return Xatanh(t, x); }

public static partial float64 X__builtin_cbrt(ж<TLS> t, float64 x) { return Xcbrt(t, x); }

public static partial float64 X__builtin_erf(ж<TLS> t, float64 x) { return Xerf(t, x); }

public static partial float64 X__builtin_erfc(ж<TLS> t, float64 x) { return Xerfc(t, x); }

public static partial float64 X__builtin_exp2(ж<TLS> t, float64 x) { return Xexp2(t, x); }

public static partial float64 X__builtin_expm1(ж<TLS> t, float64 x) { return Xexpm1(t, x); }

public static partial float64 X__builtin_fdim(ж<TLS> t, float64 x, float64 y) { return Xfdim(t, x, y); }

public static partial float32 X__builtin_fmaf(ж<TLS> t, float32 x, float32 y, float32 z) { return Xfmaf(t, x, y, z); }

public static partial float64 X__builtin_lgamma(ж<TLS> t, float64 x) { return Xlgamma(t, x); }

public static partial float64 X__builtin_log1p(ж<TLS> t, float64 x) { return Xlog1p(t, x); }

public static partial float32 X__builtin_log1pf(ж<TLS> t, float32 x) { return Xlog1pf(t, x); }

public static partial float64 X__builtin_logb(ж<TLS> t, float64 x) { return Xlogb(t, x); }

public static partial float64 X__builtin_nextafter(ж<TLS> t, float64 x, float64 y) { return Xnextafter(t, x, y); }

public static partial float32 X__builtin_nextafterf(ж<TLS> t, float32 x, float32 y) { return Xnextafterf(t, x, y); }

public static partial float64 X__builtin_nexttoward(ж<TLS> t, float64 x, float64 y) { return Xnexttoward(t, x, y); }

public static partial float64 X__builtin_remainder(ж<TLS> t, float64 x, float64 y) { return Xremainder(t, x, y); }

public static partial float64 X__builtin_remquo(ж<TLS> t, float64 x, float64 y, uintptr quo) { return Xremquo(t, x, y, quo); }

public static partial float64 X__builtin_rint(ж<TLS> t, float64 x) { return Xrint(t, x); }

public static partial float64 X__builtin_scalbn(ж<TLS> t, float64 x, int32 n) { return Xscalbn(t, x, n); }

public static partial float64 X__builtin_tgamma(ж<TLS> t, float64 x) { return Xtgamma(t, x); }

public static int32 X__flt_rounds(ж<TLS> t) {
    return 1;
}

public static int32 X__builtin_flt_rounds(ж<TLS> t) {
    return 1;
}

public static partial float32 /*r*/ Xfmaf(ж<TLS> t, float32 x, float32 y, float32 z) {
    float32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v x=%v y=%v z=%v, (%v:)", t.OrTypedNil(), x, y, z, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var xy = (float64)x * (float64)y;
        var fz = (float64)z;
        var s = xy + fz;
        if (math.IsNaN(s) || math.IsInf(s, 0)) {
            r = (float32)s; goto ᒐdone;
        }

        var bb = s - xy;
        var err = (xy - (s - bb)) + (fz - bb);
        if (err != 0D) {
            var u = math.Float64bits(s); if ((u & 1) == 0) {
                if ((err > 0D) == (s > 0D)) {
                    u++;
                } else {
                    u--;
                }
                s = math.Float64frombits(u);
            }
        }
        r = (float32)s;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial uintptr /*r*/ X__builtin___strncat_chk(ж<TLS> t, uintptr dest, uintptr src, Tsize_t n, Tsize_t destlen) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v dest=%v src=%v n=%v destlen=%v, (%v:)", t.OrTypedNil(), dest, src, n, destlen, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var dlen = strlen(dest);
        Tsize_t slen = default;
        while (slen < n && ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)(src + (uintptr)slen)) != 0) {
            slen++;
        }
        if (destlen != ~((Tsize_t)0) && dlen + slen + 1 > destlen) {
            Xabort(t);
        }

        var d = dest + (uintptr)dlen;
        for (var i = (Tsize_t)0; i < slen; i++) {
            (@unsafe.Pointer.To<byte>((@unsafe.Pointer)(d + (uintptr)i))).Value = (@unsafe.Pointer.To<byte>((@unsafe.Pointer)(src + (uintptr)i))).Value;
        }
        (@unsafe.Pointer.To<byte>((@unsafe.Pointer)(d + (uintptr)slen))).Value = 0;
        r = dest;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

} // end libc_package
