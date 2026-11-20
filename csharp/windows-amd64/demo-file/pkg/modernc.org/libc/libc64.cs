// Copyright 2020 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using @unsafe = unsafe_package;
using limits = go.modernc.org.libc.limits_package;
using types = go.modernc.org.libc.sys.types_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.modernc.org.libc;
using go.modernc.org.libc.sys;
using static libc_package;

partial class libc_package {

internal static UntypedInt heapSize => /* 2 << 30 */ 2147483648;

/* [1125899906842623L]byte */
partial struct RawMem /*[1125899906842623]byte*/;

partial struct stackHeaderPadding {
    internal uintptr a;
}

partial struct bitz /*[]nint*/;

internal static bitz /*r*/ newBits(nint n) { return new bitz(((n + 63) >> 6)); }

} partial class libcᴛbitz {

internal static bool has(this bitz b, nint n) { return b != nil && (b[(n >> 6)] & (((nint)1).Lsh((nuint)(n & 63)))) != 0; }
internal static void set(this bitz b, nint n) { b[(n >> 6)] |= (nint)(((nint)1).Lsh((nuint)(n & 63))); }

} partial class libc_package {

public static partial uintptr /*r*/ Xstrchrnul(ж<TLS> tls, uintptr s, int32 c) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v s=%v c=%v, (%v:)", tls.OrTypedNil(), s, c, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        typesꓸSize_t k = default;
        uintptr w = default;
        _ = k;
        _ = w;
        c = (int32)(uint8)c;
        if (!(c != 0)) {
            r = s + (uintptr)Xstrlen(tls, s); goto ᒐdone;
        }
        for (;;) {
            if (!((uint64)s % Uint64FromInt64(8) != 0)) {
                break;
            }
            if (!(~@unsafe.Pointer.To<int8>((@unsafe.Pointer)s) != 0) || (int32)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)s)) == c) {
                r = s; goto ᒐdone;
            }
            goto _1;
_1:
            s++;
        }
        k = (uint64)(-Int32FromInt32(1)) / Uint64FromInt32(limits.UCHAR_MAX) * (uint64)c;
        w = s;
        for (;;) {
            if (!(!(((~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)w) - (uint64)(-Int32FromInt32(1)) / Uint64FromInt32(limits.UCHAR_MAX)) & ~~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)w) & ((uint64)(-Int32FromInt32(1)) / Uint64FromInt32(limits.UCHAR_MAX) * (uint64)(quo(Int32FromInt32(limits.UCHAR_MAX), Int32FromInt32(2)) + Int32FromInt32(1)))) != 0) && !((((~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)w) ^ k) - (uint64)(-Int32FromInt32(1)) / Uint64FromInt32(limits.UCHAR_MAX)) & ~(~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)w) ^ k) & ((uint64)(-Int32FromInt32(1)) / Uint64FromInt32(limits.UCHAR_MAX) * (uint64)(quo(Int32FromInt32(limits.UCHAR_MAX), Int32FromInt32(2)) + Int32FromInt32(1)))) != 0))) {
                break;
            }
            goto _2;
_2:
            w += 8;
        }
        s = w;
        for (;;) {
            if (!(~@unsafe.Pointer.To<int8>((@unsafe.Pointer)s) != 0 && (int32)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)s)) != c)) {
                break;
            }
            goto _3;
_3:
            s++;
        }
        r = s;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

} // end libc_package
