// Copyright 2023 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using math = math_package;
using mbits = go.math.bits_package;
using memory = go.modernc.org.memory_package;
using System.Runtime.CompilerServices;
using go.modernc.org;

partial class libc_package {

internal const bool isMemBrk = false;

public static partial uintptr /*r*/ Xmalloc(ж<TLS> tls, Tsize_t n) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v n=%v, (%v:)", tls.OrTypedNil(), n, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (n > math.MaxInt) {
            tls.setErrno(ENOMEM);
            r = 0; goto ᒐdone;
        }

        ᏑallocatorMu.Lock();

        defer(ᏑallocatorMu.Unlock, ref ᒐ);

        if (n == 0) {
            n = 1;
        }
        error err = default;
        (r, err) = Ꮡallocator.UintptrMalloc((nint)n); if (err != nil) {
            r = 0;
            tls.setErrno(ENOMEM);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial uintptr /*r*/ Xcalloc(ж<TLS> tls, Tsize_t m, Tsize_t n) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v m=%v n=%v, (%v:)", tls.OrTypedNil(), m, n, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var (hi, rq) = mbits.Mul((nuint)m, (nuint)n);
        if (hi != 0 || rq > math.MaxInt) {
            tls.setErrno(ENOMEM);
            r = 0; goto ᒐdone;
        }

        ᏑallocatorMu.Lock();

        defer(ᏑallocatorMu.Unlock, ref ᒐ);

        if (rq == 0) {
            rq = 1;
        }

        error err = default;
        (r, err) = Ꮡallocator.UintptrCalloc((nint)rq); if (err != nil) {
            r = 0;
            tls.setErrno(ENOMEM);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial uintptr /*r*/ Xrealloc(ж<TLS> tls, uintptr p, Tsize_t n) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v p=%v n=%v, (%v:)", tls.OrTypedNil(), p, n, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        ᏑallocatorMu.Lock();

        defer(ᏑallocatorMu.Unlock, ref ᒐ);

        error err = default;
        (r, err) = Ꮡallocator.UintptrRealloc(p, (nint)n); if (err != nil) {
            r = 0;
            tls.setErrno(ENOMEM);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial void Xfree(ж<TLS> tls, uintptr p) {
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v p=%v, (%v:)", tls.OrTypedNil(), p, origin(2));
        }
        ᏑallocatorMu.Lock();

        defer(ᏑallocatorMu.Unlock, ref ᒐ);

        Ꮡallocator.UintptrFree(p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial Tsize_t /*r*/ Xmalloc_usable_size(ж<TLS> tls, uintptr p) {
    Tsize_t r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v p=%v, (%v:)", tls.OrTypedNil(), p, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (p == 0) {
            r = 0; goto ᒐdone;
        }

        ᏑallocatorMu.Lock();

        defer(ᏑallocatorMu.Unlock, ref ᒐ);

        r = (Tsize_t)memory.UintptrUsableSize(p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static slice<ж<MemAuditError>> /*r*/ MemAudit() {
    return default;
}

public static Tsize_t UsableSize(uintptr p) {
    GoFrame ᒐ = default;
    try {
        ᏑallocatorMu.Lock();

        defer(ᏑallocatorMu.Unlock, ref ᒐ);

        return (Tsize_t)memory.UintptrUsableSize(p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

partial struct MemAllocatorStat {
    public nint Allocs;
    public nint Bytes;
    public nint Mmaps;
}

public static MemAllocatorStat MemStat() {
    GoFrame ᒐ = default;
    try {
        ᏑallocatorMu.Lock();
        defer(ᏑallocatorMu.Unlock, ref ᒐ);

        return new MemAllocatorStat(
            Allocs: allocator.Allocs,
            Bytes: allocator.Bytes,
            Mmaps: allocator.Mmaps
        );
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static void MemAuditStart() { }

public static error MemAuditReport() { return default; }

} // end libc_package
