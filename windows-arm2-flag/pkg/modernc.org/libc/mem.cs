// Copyright 2021 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using math = math_package;
using bits = go.math.bits_package;
using errno = go.modernc.org.libc.errno_package;
using types = go.modernc.org.libc.sys.types_package;
using memory = go.modernc.org.memory_package;
using System.Runtime.CompilerServices;
using go.math;
using go.modernc.org;
using go.modernc.org.libc;
using go.modernc.org.libc.sys;

partial class libc_package {

internal const bool memgrind = false;

internal static readonly ж<memory.Allocator> Ꮡallocator = new StaticBox<memory.Allocator>(new memory.Allocator());
internal static ref memory.Allocator allocator => ref ((StaticBox<memory.Allocator>)Ꮡallocator).ValueSlot;

public static partial uintptr Xmalloc(ж<TLS> t, typesꓸSize_t n) {
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v n=%v, (%v:)", t.OrTypedNil(), n, origin(2));
        }
        if (n == 0) {
            n = 1;
        }

        ᏑallocMu.Lock();

        defer(ᏑallocMu.Unlock, ref ᒐ);

        var (p, err) = Ꮡallocator.UintptrMalloc((nint)n);
        if (err != nil) {
            t.setErrno((nint)(errno.ENOMEM));
            return 0;
        }

        return p;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static partial uintptr Xcalloc(ж<TLS> t, typesꓸSize_t n, typesꓸSize_t size) {
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v n=%v size=%v, (%v:)", t.OrTypedNil(), n, size, origin(2));
        }
        var (hi, rq0) = bits.Mul((nuint)n, (nuint)size);
        if (hi != 0 || rq0 > math.MaxInt) {
            t.setErrno((nint)(errno.ENOMEM));
            return 0;
        }
        nint rq = (nint)rq0;
        if (rq == 0) {
            rq = 1;
        }

        ᏑallocMu.Lock();

        defer(ᏑallocMu.Unlock, ref ᒐ);

        var (p, err) = Ꮡallocator.UintptrCalloc(rq);
        if (err != nil) {
            t.setErrno((nint)(errno.ENOMEM));
            return 0;
        }

        return p;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static partial uintptr Xrealloc(ж<TLS> t, uintptr ptr, typesꓸSize_t size) {
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v ptr=%v size=%v, (%v:)", t.OrTypedNil(), ptr, size, origin(2));
        }
        ᏑallocMu.Lock();

        defer(ᏑallocMu.Unlock, ref ᒐ);

        var (p, err) = Ꮡallocator.UintptrRealloc(ptr, (nint)size);
        if (err != nil) {
            t.setErrno((nint)(errno.ENOMEM));
            return 0;
        }

        return p;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static partial void Xfree(ж<TLS> t, uintptr p) {
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v p=%v, (%v:)", t.OrTypedNil(), p, origin(2));
        }
        if (p == 0) {
            return;
        }

        ᏑallocMu.Lock();

        defer(ᏑallocMu.Unlock, ref ᒐ);

        Ꮡallocator.UintptrFree(p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial typesꓸSize_t /*r*/ Xmalloc_usable_size(ж<TLS> tls, uintptr p) {
    typesꓸSize_t r = default;
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

        ᏑallocMu.Lock();

        defer(ᏑallocMu.Unlock, ref ᒐ);

        r = (typesꓸSize_t)memory.UintptrUsableSize(p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static typesꓸSize_t UsableSize(uintptr p) {
    GoFrame ᒐ = default;
    try {
        ᏑallocMu.Lock();

        defer(ᏑallocMu.Unlock, ref ᒐ);

        return (typesꓸSize_t)memory.UintptrUsableSize(p);
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
        ᏑallocMu.Lock();
        defer(ᏑallocMu.Unlock, ref ᒐ);

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
