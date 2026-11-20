// Copyright 2017 The Memory Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using fmt = fmt_package;
using bits = math.bits_package;
using os = os_package;
using @unsafe = unsafe_package;
using io = io_package;
using math;
using memory = memory_package;
using static memory_package;

partial class memory_package {

internal static uintptr headerSize => /* unsafe.Sizeof(page{}) */ 32;
internal static uintptr mallocAllign => /* 2 * unsafe.Sizeof(uintptr(0)) */ 16;
internal static UntypedInt maxSlotSize => /* 1 << maxSlotSizeLog */ 16384;
internal static UntypedInt maxSlotSizeLog => /* pageSizeLog - 2 */ 14;
internal static uintptr pageAvail => /* pageSize - headerSize */ 65504;
internal static UntypedInt pageMask => /* pageSize - 1 */ 65535;
internal static UntypedInt pageSize => /* 1 << pageSizeLog */ 65536;

[GoInit] internal static void init() {
    if (/* unsafe.Sizeof(page{}) */ (uintptr)32 % mallocAllign != 0) {
        throw panic("internal error");
    }
}

internal static nint roundup(nint n, nint m) { return (n + m - 1) & ~(m - 1); }

partial struct node {
    internal uintptr prev, next;
}

partial struct page {
    internal nint brk;
    internal nuint log;
    internal nint size;
    internal nint used;
}

partial struct Allocator {
    public nint Allocs;
    public nint Bytes;
    internal array<nint> cap = new(64);
    internal array<uintptr> lists = new(64);
    public nint Mmaps;
    internal array<uintptr> pages = new(64);
    internal map<uintptr, EmptyStruct> regs;
    internal map<nint, slice<uintptr>> freed;
    internal nint freedSize;
    internal map<nint, nint> cold;
    internal nint live;
    internal nint hiLive;
}

} partial class memoryᴛAllocator {

internal static nint maxFreedSize(this ref Allocator a) { return (4 << 20) + a.hiLive; }

internal static void addLive(this ref Allocator a, nint n) {
    a.live += n;
    if (a.live > a.hiLive) {
        a.hiLive = a.live;
    }
}

internal static bool canRetain(this ref Allocator a, nint size) { return a.freedSize + size <= a.maxFreedSize(); }

} partial class memory_package {

internal static nint mmapSize(nint size) {
    switch (ᐧ) {
    case {} when size <= pageSize && (nint)pageSize % osPageSize == 0: {
        return pageSize;
    }
    case {} when size > pageSize && bits.UintSize == 64: {
        return roundup(((nint)1).Lsh((int64)(bits.Len((nuint)(size - 1)))), mmapGranularity);
    }
    default: {
        return roundup(size, mmapGranularity);
    }}

}

internal static UntypedInt slabBatch => 16;

} partial class memoryᴛAllocator {

internal static (uintptr, error) mmap(this ref Allocator a, nint size) {
    size = mmapSize(size);
    var s = a.freed[size]; if (len(s) != 0) {
        var pΔ1 = s[len(s) - 1];
        if (canDecommit) {
            nint c = a.cold[size]; if (c >= len(s)) {
                var errΔ1 = recommit(pΔ1, size); if (errΔ1 != nil) {
                    return (0, errΔ1);
                }

                a.cold[size] = len(s) - 1;
            }
        }
        a.freed[size] = s.slice(0, len(s) - 1);
        a.freedSize -= size;
        a.addLive(size);
        var pg = @unsafe.Pointer.To<page>((@unsafe.Pointer)pΔ1);
        pg.brk = 0;
        pg.used = 0;
        return (pΔ1, default);
    }

    if (canCarve && size == pageSize) {
        var (pΔ2, n, errΔ2) = memory.mmap(slabBatch * pageSize); if (errΔ2 == nil) {
            if (n % (nint)pageSize == 0 && a.canRetain(n - (nint)pageSize)) {
                for (nint off = pageSize; off < n; off += pageSize) {
                    var q = pΔ2 + (uintptr)off;
                    a.reg(q, pageSize);
                    a.retain(q, pageSize);
                }
                return (a.reg(pΔ2, pageSize), default);
            }

            var errΔ3 = memory.unmap(pΔ2, n); if (errΔ3 != nil) {
                return (0, errΔ3);
            }
        }
    }

    (var p, size, var err) = memory.mmap(size);
    if (err != nil) {
        return (0, err);
    }

    return (a.reg(p, size), default);
}

internal static uintptr reg(this ref Allocator a, uintptr p, nint size) {
    a.addLive(size);
    if (counters) {
        a.Mmaps++;
        a.Bytes += size;
    }
    if (a.regs == nil) {
        a.regs = new map<uintptr, EmptyStruct>{};
    }
    (@unsafe.Pointer.To<page>((@unsafe.Pointer)p)).Value.size = size;
    a.regs[p] = new EmptyStruct();
    return p;
}

internal static (uintptr, error) newPage(this ref Allocator a, nint size) {
    size += (nint)headerSize;
    var (p, err) = a.mmap(size);
    if (err != nil) {
        return (0, err);
    }

    (@unsafe.Pointer.To<page>((@unsafe.Pointer)p)).Value.log = 0;
    return (p, default);
}

internal static (uintptr, error) newSharedPage(this ref Allocator a, nuint log) {
    if (a.cap[log] == 0) {
        a.cap[log] = (nint)pageAvail / (((nint)1).Lsh(log));
    }
    nint size = (nint)headerSize + a.cap[log].Lsh(log);
    var (p, err) = a.mmap(size);
    if (err != nil) {
        return (0, err);
    }

    a.pages[log] = p;
    (@unsafe.Pointer.To<page>((@unsafe.Pointer)p)).Value.log = log;
    return (p, default);
}

internal static error unmap(this ref Allocator a, uintptr p) {
    delete(a.regs, p);
    nint size = (@unsafe.Pointer.To<page>((@unsafe.Pointer)p)).Value.size;
    if (counters) {
        a.Mmaps--;
        a.Bytes -= size;
    }
    return memory.unmap(p, size);
}

internal static error release(this ref Allocator a, uintptr p) {
    nint size = (@unsafe.Pointer.To<page>((@unsafe.Pointer)p)).Value.size;
    if (a.canRetain(size)) {
        a.retain(p, size);
        return default;
    }

    a.live -= size;
    return a.unmap(p);
}

} partial class memory_package {

internal static UntypedInt hotBytes => /* 4 << 20 */ 4194304;

} partial class memoryᴛAllocator {

internal static void retain(this ref Allocator a, uintptr p, nint size) {
    if (a.freed == nil) {
        a.freed = new map<nint, slice<uintptr>>{};
    }
    a.freed[size] = append(a.freed[size], p);
    a.freedSize += size;
    a.live -= size;
    if (!canDecommit) {
        return;
    }

    nint hot = (nint)hotBytes / size;
    if (hot < 1) {
        hot = 1;
    }
    var s = a.freed[size]; if (len(s) - a.cold[size] > hot) {
        if (a.cold == nil) {
            a.cold = new map<nint, nint>{};
        }
        decommit(s[a.cold[size]], size);
        a.cold[size]++;
    }
}

public static (uintptr r, error err) UintptrCalloc(this ж<Allocator> Ꮡa, nint size) {
    uintptr r = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (trace) {
            defer(() => {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "Calloc(%#x) %#x, %v\n"u8, size, r, err);
            }, ref ᒐ);
        }
        (r, err) = Ꮡa.UintptrMalloc(size); if (r == 0 || err != nil) {
            (r, err) = (0, err); goto ᒐdone;
        }
        var b = @unsafe.Pointer.View<byte>((@unsafe.Pointer)r, 0, size, size, 1125899906842623);
        foreach (var i in range(len(b))) {
            b[i] = 0;
        }
        (r, err) = (r, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (r, err);
}

public static error /*err*/ UintptrFree(this ж<Allocator> Ꮡa, uintptr p) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.DerefOrNull();

        if (trace) {
            defer(() => {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "Free(%#x) %v\n"u8, p, err);
            }, ref ᒐ);
        }
        if (p == 0) {
            err = default; goto ᒐdone;
        }

        if (counters) {
            a.Allocs--;
        }
        var pg = (uintptr)(p & ~(uintptr)(uintptr)pageMask);
        nuint log = (@unsafe.Pointer.To<page>((@unsafe.Pointer)pg)).Value.log;
        if (log == 0) {
            err = a.release(pg); goto ᒐdone;
        }

        (@unsafe.Pointer.To<node>((@unsafe.Pointer)p)).Value.prev = 0;
        (@unsafe.Pointer.To<node>((@unsafe.Pointer)p)).Value.next = a.lists[log];
        {
            var next = (@unsafe.Pointer.To<node>((@unsafe.Pointer)p)).Value.next; if (next != 0) {
                (@unsafe.Pointer.To<node>((@unsafe.Pointer)next)).Value.prev = p;
            }
        }
        a.lists[log] = p;
        (@unsafe.Pointer.To<page>((@unsafe.Pointer)pg)).Value.used--;
        if ((@unsafe.Pointer.To<page>((@unsafe.Pointer)pg)).Value.used != 0) {
            err = default; goto ᒐdone;
        }

        for (nint i = 0; i < (@unsafe.Pointer.To<page>((@unsafe.Pointer)pg)).Value.brk; i++) {
            var n = pg + headerSize + ((uintptr)i).Lsh(log);
            var next = (@unsafe.Pointer.To<node>((@unsafe.Pointer)n)).Value.next;
            var prev = (@unsafe.Pointer.To<node>((@unsafe.Pointer)n)).Value.prev;
            switch (ᐧ) {
            case {} when prev == 0: {
                a.lists[log] = next;
                if (next != 0) {
                    (@unsafe.Pointer.To<node>((@unsafe.Pointer)next)).Value.prev = 0;
                }
                break;
            }
            case {} when next == 0: {
                (@unsafe.Pointer.To<node>((@unsafe.Pointer)prev)).Value.next = 0;
                break;
            }
            default: {
                (@unsafe.Pointer.To<node>((@unsafe.Pointer)prev)).Value.next = next;
                (@unsafe.Pointer.To<node>((@unsafe.Pointer)next)).Value.prev = prev;
                break;
            }}

        }

        if (a.pages[log] == 0 || a.pages[log] == pg) {
            (@unsafe.Pointer.To<page>((@unsafe.Pointer)pg)).Value.brk = 0;
            a.pages[log] = pg;
            err = default; goto ᒐdone;
        }

        err = a.release(pg);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

public static (uintptr r, error err) UintptrMalloc(this ж<Allocator> Ꮡa, nint size) {
    uintptr r = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.DerefOrNull();

        if (trace) {
            defer(() => {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "Malloc(%#x) %#x, %v\n"u8, size, r, err);
            }, ref ᒐ);
        }
        if (size < 0) {
            throw panic("invalid malloc size");
        }

        if (size == 0) {
            (r, err) = (0, default); goto ᒐdone;
        }
        if (size > unchecked((nint)(9223372036854775775L))) {
            (r, err) = (0, fmt.Errorf("memory: allocation size %d is too large"u8, size)); goto ᒐdone;
        }

        if (counters) {
            a.Allocs++;
        }
        nuint log = (nuint)bits.Len((nuint)((nint)((size + (nint)mallocAllign - 1) & ~(nint)(nint)(mallocAllign - 1)) - 1));
        if (log > maxSlotSizeLog) {
            var (pΔ1, errΔ1) = a.newPage(size);
            if (errΔ1 != nil) {
                (r, err) = (0, errΔ1); goto ᒐdone;
            }

            (r, err) = (pΔ1 + headerSize, default); goto ᒐdone;
        }

        if (a.lists[log] == 0 && a.pages[log] == 0) {
            var (_, errΔ2) = a.newSharedPage(log); if (errΔ2 != nil) {
                (r, err) = (0, errΔ2); goto ᒐdone;
            }
        }

        var pΔ2 = a.pages[log]; if (pΔ2 != 0) {
            (@unsafe.Pointer.To<page>((@unsafe.Pointer)pΔ2)).Value.used++;
            (@unsafe.Pointer.To<page>((@unsafe.Pointer)pΔ2)).Value.brk++;
            if ((@unsafe.Pointer.To<page>((@unsafe.Pointer)pΔ2)).Value.brk == a.cap[log]) {
                a.pages[log] = 0;
            }
            (r, err) = (pΔ2 + headerSize + ((uintptr)((@unsafe.Pointer.To<page>((@unsafe.Pointer)pΔ2)).Value.brk - 1)).Lsh(log), default); goto ᒐdone;
        }

        var n = a.lists[log];
        var p = (uintptr)(n & ~(uintptr)(uintptr)pageMask);
        a.lists[log] = (@unsafe.Pointer.To<node>((@unsafe.Pointer)n)).Value.next;
        var next = (@unsafe.Pointer.To<node>((@unsafe.Pointer)n)).Value.next; if (next != 0) {
            (@unsafe.Pointer.To<node>((@unsafe.Pointer)next)).Value.prev = 0;
        }
        (@unsafe.Pointer.To<page>((@unsafe.Pointer)p)).Value.used++;
        (r, err) = (n, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (r, err);
}

public static (uintptr r, error err) UintptrRealloc(this ж<Allocator> Ꮡa, uintptr p, nint size) {
    uintptr r = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (trace) {
            defer(() => {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "UnsafeRealloc(%#x, %#x) %#x, %v\n"u8, p, size, r, err);
            }, ref ᒐ);
        }
        switch (ᐧ) {
        case {} when p == 0: {
            (r, err) = Ꮡa.UintptrMalloc(size); goto ᒐdone;
        }
        case {} when size == 0 && p != 0: {
            (r, err) = (0, Ꮡa.UintptrFree(p)); goto ᒐdone;
        }}

        nint us = UintptrUsableSize(p);
        if (us >= size) {
            (r, err) = (p, default); goto ᒐdone;
        }

        (r, err) = Ꮡa.UintptrMalloc(size); if (err != nil) {
            (r, err) = (0, err); goto ᒐdone;
        }

        if (us < size) {
            size = us;
        }
        copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)r, 0, size, size, 1125899906842623), @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, size, size, 1125899906842623));
        (r, err) = (r, Ꮡa.UintptrFree(p));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (r, err);
}

} partial class memory_package {

public static nint /*r*/ UintptrUsableSize(uintptr p) {
    nint r = default;
    GoFrame ᒐ = default;
    try {
        if (trace) {
            defer(() => {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "UsableSize(%#x) %#x\n"u8, p, r);
            }, ref ᒐ);
        }
        if (p == 0) {
            r = 0; goto ᒐdone;
        }

        r = usableSize(p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static nint /*r*/ usableSize(uintptr p) {
    var pg = (uintptr)(p & ~(uintptr)(uintptr)pageMask);
    nuint log = (@unsafe.Pointer.To<page>((@unsafe.Pointer)pg)).Value.log; if (log != 0) {
        return ((nint)1).Lsh(log);
    }

    return (@unsafe.Pointer.To<page>((@unsafe.Pointer)pg)).Value.size - (nint)headerSize;
}

} partial class memoryᴛAllocator {

public static (slice<byte> r, error err) Calloc(this ж<Allocator> Ꮡa, nint size) {
    error err = default;

    (var p, err) = Ꮡa.UintptrCalloc(size);
    if (err != nil) {
        return (default, err);
    }

    return (@unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, size, usableSize(p), 1125899906842623), default);
}

public static error /*err*/ Close(this ref Allocator a) {
    error err = default;

    foreach (var (p, _) in a.regs) {
        var e = a.unmap(p); if (e != nil && err == nil) {
            err = e;
        }
    }
    a.AssignFrom(new Allocator());
    return err;
}

public static error /*err*/ Free(this ж<Allocator> Ꮡa, slice<byte> b) {
    b = b.slice(0, cap(b)); if (len(b) == 0) {
        return default;
    }

    return Ꮡa.UintptrFree((uintptr)Ꮡ(b, 0));
}

public static (slice<byte> r, error err) Malloc(this ж<Allocator> Ꮡa, nint size) {
    error err = default;

    (var p, err) = Ꮡa.UintptrMalloc(size);
    if (p == 0 || err != nil) {
        return (default, err);
    }

    return (@unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, size, usableSize(p), 1125899906842623), default);
}

public static (slice<byte> r, error err) Realloc(this ж<Allocator> Ꮡa, slice<byte> b, nint size) {
    error err = default;

    uintptr p = default;
    b = b.slice(0, cap(b)); if (len(b) != 0) {
        p = (uintptr)Ꮡ(b, 0);
    }
    (p, err) = Ꮡa.UintptrRealloc(p, size); if (p == 0 || err != nil) {
        return (default, err);
    }

    return (@unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, size, usableSize(p), 1125899906842623), default);
}

public static error /*err*/ Trim(this ж<Allocator> Ꮡa) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.DerefOrNull();

        if (trace) {
            defer(() => {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "Trim() %v\n"u8, err);
            }, ref ᒐ);
        }
        foreach (var log in range(len(a.pages))) {
            var pg = a.pages[log];
            if (pg == 0 || (@unsafe.Pointer.To<page>((@unsafe.Pointer)pg)).Value.used != 0) {
                continue;
            }

            a.pages[log] = 0;
            a.live -= (@unsafe.Pointer.To<page>((@unsafe.Pointer)pg)).Value.size;
            var e = a.unmap(pg); if (e != nil && err == nil) {
                err = e;
            }
        }
        foreach (var (_, s) in a.freed) {
            foreach (var (_, pg) in range(s)) {
                var e = a.unmap(pg); if (e != nil && err == nil) {
                    err = e;
                }
            }
        }
        a.freed = default;
        a.freedSize = 0;
        a.cold = default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return err;
}

} partial class memory_package {

public static nint /*r*/ UsableSize(ж<byte> p) { return UintptrUsableSize((uintptr)p); }

} partial class memoryᴛAllocator {

public static (@unsafe.Pointer r, error err) UnsafeCalloc(this ж<Allocator> Ꮡa, nint size) {
    error err = default;

    (var p, err) = Ꮡa.UintptrCalloc(size);
    if (err != nil) {
        return (default, err);
    }

    return ((@unsafe.Pointer)p, default);
}

public static error /*err*/ UnsafeFree(this ж<Allocator> Ꮡa, @unsafe.Pointer p) { return Ꮡa.UintptrFree((uintptr)p); }

public static (@unsafe.Pointer r, error err) UnsafeMalloc(this ж<Allocator> Ꮡa, nint size) {
    error err = default;

    (var p, err) = Ꮡa.UintptrMalloc(size);
    if (err != nil) {
        return (default, err);
    }

    return ((@unsafe.Pointer)p, default);
}

public static (@unsafe.Pointer r, error err) UnsafeRealloc(this ж<Allocator> Ꮡa, @unsafe.Pointer p, nint size) {
    error err = default;

    (var q, err) = Ꮡa.UintptrRealloc((uintptr)p, size);
    if (err != nil) {
        return (default, err);
    }

    return ((@unsafe.Pointer)q, default);
}

} partial class memory_package {

public static nint /*r*/ UnsafeUsableSize(@unsafe.Pointer p) { return UintptrUsableSize((uintptr)p); }

} // end memory_package
