// Copyright 2026 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using math = math_package;
using bits = go.math.bits_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;
using go.math;

partial class libc_package {

public static partial uintptr /*r*/ Xmemcpy(ж<TLS> tls, uintptr dest, uintptr src, Tsize_t n) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v dest=%v src=%v n=%v, (%v:)", tls.OrTypedNil(), dest, src, n, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (n != 0) {
            copy(@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)dest), n), @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)src), n));
        }
        r = dest;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial uintptr /*r*/ Xmemmove(ж<TLS> tls, uintptr dest, uintptr src, Tsize_t n) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v dest=%v src=%v n=%v, (%v:)", tls.OrTypedNil(), dest, src, n, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (n != 0) {
            copy(@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)dest), n), @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)src), n));
        }
        r = dest;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial uintptr /*r*/ Xmemset(ж<TLS> tls, uintptr dest, int32 c, Tsize_t n) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v dest=%v c=%v n=%v, (%v:)", tls.OrTypedNil(), dest, c, n, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (n == 0) {
            r = dest; goto ᒐdone;
        }
        var b = @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)dest), n);
        var bc = (byte)c;
        if (bc == 0) {
            clear(b);
            r = dest; goto ᒐdone;
        }
        if (len(b) < 8) {
            foreach (var iΔ1 in range(len(b))) {
                b[iΔ1] = bc;
            }
            r = dest; goto ᒐdone;
        }
        UntypedInt ws = /* bits.UintSize / 8 */ 8;
        nuint x = (nuint)(unchecked((nuint)(/* math.MaxUint / 0xff */ 72340172838076673UL))) * (nuint)bc;
        nint seed = min(len(b), 512);
        nint i = 0;
        for (; i < seed && (uintptr)Ꮡ(b, i) % (uintptr)ws != 0; i++) {
            b[i] = bc;
        }
        for (; i + (nint)(4 * ws) <= seed; i += 4 * ws) {
            var q = @unsafe.Pointer.To<array<nuint>>(@unsafe.Pointer.FromPinnedBox(Ꮡ(b, i)));
            q.Value[0] = x;
            q.Value[1] = x;
            q.Value[2] = x;
            q.Value[3] = x;
        }
        for (; i + (nint)ws <= seed; i += ws) {
            (Ꮡ(b, i).Reinterpret<byte, nuint>()).Value = x;
        }
        for (; i < seed; i++) {
            b[i] = bc;
        }
        for (nint iΔ2 = seed; iΔ2 < len(b); ) {
            iΔ2 += copy(b.slice(iΔ2), b.slice(0, iΔ2));
        }
        r = dest;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial int32 /*r1*/ Xmemcmp(ж<TLS> tls, uintptr vl, uintptr vr, Tsize_t n) {
    int32 r1 = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v vl=%v vr=%v n=%v, (%v:)", tls.OrTypedNil(), vl, vr, n, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r1);
            }, ref ᒐ);
        }
        r1 = memcmp(vl, vr, n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r1;
}

public static partial float64 /*r*/ Xfabs(ж<TLS> Ꮡtls, float64 x) {
    float64 r = default;
    GoFrame ᒐ = default;
    try {
        ref var tls = ref Ꮡtls.DerefOrNull();

        if (__ccgo_strace) {
            trc("tls=%v x=%v, (%v:)", Ꮡtls.OrTypedNil(), x, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = math.Abs(x);
        if (tls.checkSignals) {
            Ꮡtls.checkSignal();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial void Xqsort(ж<TLS> tls, uintptr @base, Tsize_t nel, Tsize_t width, Tcmpfun __ccgo_fp_cmp) {
    if (__ccgo_strace) {
        trc("tls=%v base=%v nel=%v width=%v __ccgo_fp_cmp=%v, (%v:)", tls.OrTypedNil(), @base, nel, width, __ccgo_fp_cmp, origin(2));
    }
    qsort(tls, @base, (uintptr)nel, (uintptr)width, (uintptr)__ccgo_fp_cmp);
}

public static partial Tsize_t /*r*/ Xstrcspn(ж<TLS> Ꮡtls, uintptr s, uintptr c) {
    Tsize_t r = default;
    GoFrame ᒐ = default;
    try {
        ref var tls = ref Ꮡtls.DerefOrNull();

        if (__ccgo_strace) {
            trc("tls=%v s=%v c=%v, (%v:)", Ꮡtls.OrTypedNil(), s, c, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        array<nuint> set = new(4); /* 256 / bits.UintSize */
        for (var p = c; ; p++) {
            var ch = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)p);
            if (ch == 0) {
                break;
            }
            set[ch / (byte)bits.UintSize] |= (nuint)(((nuint)1 << (int)(ch % (byte)bits.UintSize)));
        }
        for (var q = s; ; q++) {
            var ch = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)q);
            if (ch == 0 || (set[ch / (byte)bits.UintSize] & ((nuint)1 << (int)(ch % (byte)bits.UintSize))) != 0) {
                r = (Tsize_t)(q - s);
                if (tls.checkSignals) {
                    Ꮡtls.checkSignal();
                }
                goto ᒐdone;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

} // end libc_package
