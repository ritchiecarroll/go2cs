// Copyright 2024 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using fmt = fmt_package;
using math = math_package;
using mbits = go.math.bits_package;
using os = os_package;
using @unsafe = unsafe_package;
using mathutil = go.modernc.org.mathutil_package;
using go.modernc.org;
using io = io_package;

partial class libc_package {

public static float32 X__builtin_inff(ж<TLS> tls) {
    return (float32)math.Inf(1);
}

public static float32 X__builtin_nanf(ж<TLS> tls, uintptr s) {
    return (float32)math.NaN();
}

public static partial int32 /*r*/ X__builtin_printf(ж<TLS> tls, uintptr fmt, uintptr va) {
    return Xprintf(tls, fmt, va);
}

public static partial float64 /*r*/ X__builtin_round(ж<TLS> tls, float64 x) {
    return Xround(tls, x);
}

public static partial @long /*r*/ X__builtin_lround(ж<TLS> tls, float64 x) {
    return Xlround(tls, x);
}

public static partial float32 /*r*/ X__builtin_roundf(ж<TLS> tls, float32 x) {
    return Xroundf(tls, x);
}

public static @long X__builtin_expect(ж<TLS> t, @long exp, @long c) {
    return exp;
}

public static partial void X__builtin_bzero(ж<TLS> t, uintptr s, Tsize_t n) {
    Xbzero(t, s, n);
}

public static partial void X__builtin_abort(ж<TLS> t) {
    Xabort(t);
}

public static partial int32 X__builtin_abs(ж<TLS> t, int32 j) {
    return Xabs(t, j);
}

public static int32 X__builtin_ctz(ж<TLS> t, uint32 n) {
    return (int32)mbits.TrailingZeros32(n);
}

public static int32 X__builtin_clz(ж<TLS> t, uint32 n) {
    return (int32)mbits.LeadingZeros32(n);
}

public static int32 X__builtin_clzll(ж<TLS> t, uint64 n) {
    return (int32)mbits.LeadingZeros64(n);
}
public static partial void X__builtin_constant_p_impl() { throw panic(todo("internal error: should never be called")); }

public static partial float64 X__builtin_copysign(ж<TLS> t, float64 x, float64 y) {
    return Xcopysign(t, x, y);
}

public static partial float32 X__builtin_copysignf(ж<TLS> t, float32 x, float32 y) {
    return Xcopysignf(t, x, y);
}

public static partial float64 X__builtin_copysignl(ж<TLS> t, float64 x, float64 y) {
    return Xcopysign(t, x, y);
}

public static partial void X__builtin_exit(ж<TLS> t, int32 status) {
    Xexit(t, status);
}

public static partial float64 X__builtin_fabs(ж<TLS> t, float64 x) {
    return Xfabs(t, x);
}

public static partial float32 X__builtin_fabsf(ж<TLS> t, float32 x) {
    return Xfabsf(t, x);
}

public static partial float64 X__builtin_fabsl(ж<TLS> t, float64 x) {
    return Xfabsl(t, x);
}

public static partial void X__builtin_free(ж<TLS> t, uintptr ptr) {
    Xfree(t, ptr);
}

public static partial int32 X__builtin_getentropy(ж<TLS> t, uintptr buf, Tsize_t n) {
    return Xgetentropy(t, buf, n);
}

public static float64 X__builtin_huge_val(ж<TLS> t) {
    return math.Inf(1);
}

public static float32 X__builtin_huge_valf(ж<TLS> t) {
    return (float32)math.Inf(1);
}

public static float64 X__builtin_inf(ж<TLS> t) {
    return math.Inf(1);
}

public static float64 X__builtin_infl(ж<TLS> t) {
    return math.Inf(1);
}

public static partial uintptr X__builtin_malloc(ж<TLS> t, Tsize_t size) {
    return Xmalloc(t, size);
}

public static partial int32 X__builtin_memcmp(ж<TLS> t, uintptr s1, uintptr s2, Tsize_t n) {
    return Xmemcmp(t, s1, s2, n);
}

public static float64 X__builtin_nan(ж<TLS> t, uintptr s) {
    return math.NaN();
}

public static float64 X__builtin_nanl(ж<TLS> t, uintptr s) {
    return math.NaN();
}

public static void X__builtin_prefetch(ж<TLS> t, uintptr addr, uintptr args) {
}

public static partial uintptr X__builtin_strchr(ж<TLS> t, uintptr s, int32 c) {
    return Xstrchr(t, s, c);
}

public static partial int32 X__builtin_strcmp(ж<TLS> t, uintptr s1, uintptr s2) {
    return Xstrcmp(t, s1, s2);
}

public static partial uintptr X__builtin_strcpy(ж<TLS> t, uintptr dest, uintptr src) {
    return Xstrcpy(t, dest, src);
}

public static partial Tsize_t X__builtin_strlen(ж<TLS> t, uintptr s) {
    return Xstrlen(t, s);
}

public static partial void X__builtin_trap(ж<TLS> t) {
    Xabort(t);
}

public static int32 X__builtin_popcount(ж<TLS> t, uint32 x) {
    return (int32)mbits.OnesCount32(x);
}

public static partial uintptr X__builtin___strcpy_chk(ж<TLS> t, uintptr dest, uintptr src, Tsize_t os) {
    return Xstrcpy(t, dest, src);
}

public static partial uintptr X__builtin_mmap(ж<TLS> t, uintptr addr, Tsize_t length, int32 prot, int32 flags, int32 fd, Toff_t offset) {
    return Xmmap(t, addr, length, prot, flags, fd, offset);
}

public static uint16 X__builtin_bswap16(ж<TLS> t, uint16 x) {
    return (uint16)((uint16)(x << 8) | x >> 8);
}

public static uint32 X__builtin_bswap32(ж<TLS> t, uint32 x) {
    return x << 24 | (x & 0xff00) << 8 | (x & 0xff0000) >> 8 | x >> 24;
}

public static uint64 X__builtin_bswap64(ж<TLS> t, uint64 x) {
    return x << 56 | (x & 0xff00) << 40 | (x & 0xff0000) << 24 | (x & 0xff000000U) << 8 | (x & 0xff00000000UL) >> 8 | (x & 0xff0000000000UL) >> 24 | (x & 0xff000000000000UL) >> 40 | x >> 56;
}

public static int32 X__builtin_add_overflowInt64(ж<TLS> t, int64 a, int64 b, uintptr res) {
    var (r, ovf) = mathutil.AddOverflowInt64(a, b);
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)res)).Value = r;
    return Bool32(ovf);
}

public static int32 X__builtin_add_overflowUint32(ж<TLS> t, uint32 a, uint32 b, uintptr res) {
    var r = a + b;
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)res)).Value = r;
    return Bool32(r < a);
}

public static int32 X__builtin_add_overflowUint64(ж<TLS> t, uint64 a, uint64 b, uintptr res) {
    var r = a + b;
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)res)).Value = r;
    return Bool32(r < a);
}

public static int32 X__builtin_sub_overflowInt64(ж<TLS> t, int64 a, int64 b, uintptr res) {
    var (r, ovf) = mathutil.SubOverflowInt64(a, b);
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)res)).Value = r;
    return Bool32(ovf);
}

public static int32 X__builtin_mul_overflowInt64(ж<TLS> t, int64 a, int64 b, uintptr res) {
    var (r, ovf) = mathutil.MulOverflowInt64(a, b);
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)res)).Value = r;
    return Bool32(ovf);
}

public static int32 X__builtin_mul_overflowUint64(ж<TLS> t, uint64 a, uint64 b, uintptr res) {
    var (hi, lo) = mbits.Mul64(a, b);
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)res)).Value = lo;
    return Bool32(hi != 0);
}

public static int32 X__builtin_mul_overflowUint128(ж<TLS> t, Uint128 a, Uint128 b, uintptr res) {
    var (r, ovf) = a.mulOvf(b);
    (@unsafe.Pointer.To<Uint128>((@unsafe.Pointer)res)).Value = r;
    return Bool32(ovf);
}

public static partial void X__builtin_unreachable(ж<TLS> t) {
    fmt.Fprintf(new os.FileжWriter(os.Stderr), "unrechable\n"u8);
    os.Stderr.Sync();
    Xexit(t, 1);
}

public static partial int32 X__builtin_snprintf(ж<TLS> t, uintptr str, Tsize_t size, uintptr format, uintptr args) {
    return Xsnprintf(t, str, size, format, args);
}

public static partial int32 /*r*/ X__builtin_sprintf(ж<TLS> t, uintptr str, uintptr format, uintptr args) {
    return Xsprintf(t, str, format, args);
}

public static partial uintptr /*r*/ X__builtin_memcpy(ж<TLS> t, uintptr dest, uintptr src, Tsize_t n) {
    return Xmemcpy(t, dest, src, n);
}

public static partial uintptr /*r*/ X__builtin___memcpy_chk(ж<TLS> t, uintptr dest, uintptr src, Tsize_t n, Tsize_t os) {
    if (os != ~((Tsize_t)0) && n < os) {
        Xabort(t);
    }

    return Xmemcpy(t, dest, src, n);
}

public static partial uintptr X__builtin_memset(ж<TLS> t, uintptr s, int32 c, Tsize_t n) {
    return Xmemset(t, s, c, n);
}

public static partial uintptr X__builtin___memset_chk(ж<TLS> t, uintptr s, int32 c, Tsize_t n, Tsize_t os) {
    if (os < n) {
        Xabort(t);
    }

    return Xmemset(t, s, c, n);
}

public static Tsize_t X__builtin_object_size(ж<TLS> t, uintptr p, int32 typ) {
    switch (typ) {
    case 0 or 1: {
        return ~((Tsize_t)0);
    }
    default: {
        return 0;
    }}

}

public static partial int32 /*r*/ X__builtin___sprintf_chk(ж<TLS> t, uintptr s, int32 flag, Tsize_t os, uintptr format, uintptr args) {
    return Xsprintf(t, s, format, args);
}

public static partial int32 X__builtin_vsnprintf(ж<TLS> t, uintptr str, Tsize_t size, uintptr format, uintptr va) {
    return Xvsnprintf(t, str, size, format, va);
}

public static partial int32 /*r*/ X__builtin___snprintf_chk(ж<TLS> t, uintptr str, Tsize_t maxlen, int32 flag, Tsize_t os, uintptr format, uintptr args) {
    if (os != ~((Tsize_t)0) && maxlen > os) {
        Xabort(t);
    }

    return Xsnprintf(t, str, maxlen, format, args);
}

public static partial int32 /*r*/ X__builtin___vsnprintf_chk(ж<TLS> t, uintptr str, Tsize_t maxlen, int32 flag, Tsize_t os, uintptr format, uintptr args) {
    if (os != ~((Tsize_t)0) && maxlen > os) {
        Xabort(t);
    }

    return Xsnprintf(t, str, maxlen, format, args);
}

public static partial int32 /*r*/ X__builtin___vsprintf_chk(ж<TLS> t, uintptr s, int32 flag, Tsize_t os, uintptr format, uintptr args) {
    return Xvsprintf(t, s, format, args);
}

public static int32 Xisnan(ж<TLS> t, float64 x) {
    return X__builtin_isnan(t, x);
}

public static int32 X__isnan(ж<TLS> t, float64 x) {
    return X__builtin_isnan(t, x);
}

public static int32 X__builtin_isnan(ж<TLS> t, float64 x) {
    return Bool32(math.IsNaN(x));
}

public static int32 Xisnanf(ж<TLS> t, float32 arg) {
    return X__builtin_isnanf(t, arg);
}

public static int32 X__isnanf(ж<TLS> t, float32 arg) {
    return X__builtin_isnanf(t, arg);
}

public static int32 X__builtin_isnanf(ж<TLS> t, float32 x) {
    return Bool32(math.IsNaN((float64)x));
}

public static int32 Xisnanl(ж<TLS> t, float64 arg) {
    return X__builtin_isnanl(t, arg);
}

public static int32 X__isnanl(ж<TLS> t, float64 arg) {
    return X__builtin_isnanl(t, arg);
}

public static int32 X__builtin_isnanl(ж<TLS> t, float64 x) {
    return Bool32(math.IsNaN(x));
}

public static partial int64 X__builtin_llabs(ж<TLS> tls, int64 a) {
    return Xllabs(tls, a);
}

public static partial float64 X__builtin_log2(ж<TLS> t, float64 x) {
    return Xlog2(t, x);
}

public static partial uintptr /*r*/ X__builtin___strncpy_chk(ж<TLS> t, uintptr dest, uintptr src, Tsize_t n, Tsize_t os) {
    if (n != ~((Tsize_t)0) && os < n) {
        Xabort(t);
    }

    return Xstrncpy(t, dest, src, n);
}

public static partial uintptr /*r*/ X__builtin___strcat_chk(ж<TLS> t, uintptr dest, uintptr src, Tsize_t os) {
    return Xstrcat(t, dest, src);
}

public static partial uintptr X__builtin___memmove_chk(ж<TLS> t, uintptr dest, uintptr src, Tsize_t n, Tsize_t os) {
    if (os != ~((Tsize_t)0) && os < n) {
        Xabort(t);
    }

    return Xmemmove(t, dest, src, n);
}

public static int32 X__builtin_isunordered(ж<TLS> t, float64 a, float64 b) {
    return Bool32(math.IsNaN(a) || math.IsNaN(b));
}

public static partial int32 /*r*/ X__builtin_ffs(ж<TLS> tls, int32 i) {
    return Xffs(tls, i);
}

public static partial float32 /*r*/ X__builtin_rintf(ж<TLS> tls, float32 x) {
    return Xrintf(tls, x);
}

public static partial @long /*r*/ X__builtin_lrintf(ж<TLS> tls, float32 x) {
    return Xlrintf(tls, x);
}

public static partial @long /*r*/ X__builtin_lrint(ж<TLS> tls, float64 x) {
    return Xlrint(tls, x);
}

public static float64 /*r*/ X__builtin_fma(ж<TLS> tls, float64 x, float64 y, float64 z) {
    return math.FMA(x, y, z);
}

public static partial uintptr X__builtin_alloca(ж<TLS> tls, Tsize_t size) {
    return Xalloca(tls, size);
}

public static partial int32 /*r*/ X__builtin_isprint(ж<TLS> tls, int32 c) {
    return Xisprint(tls, c);
}

public static partial int32 /*r*/ X__builtin_isblank(ж<TLS> tls, int32 c) {
    return Xisblank(tls, c);
}

public static partial float64 /*r*/ X__builtin_trunc(ж<TLS> tls, float64 x) {
    return Xtrunc(tls, x);
}

public static partial float64 /*r*/ X__builtin_hypot(ж<TLS> tls, float64 x, float64 y) {
    return Xhypot(tls, x, y);
}

public static partial float64 /*r*/ X__builtin_fmax(ж<TLS> tls, float64 x, float64 y) {
    return Xfmax(tls, x, y);
}

public static partial float64 /*r*/ X__builtin_fmin(ж<TLS> tls, float64 x, float64 y) {
    return Xfmin(tls, x, y);
}

} // end libc_package
