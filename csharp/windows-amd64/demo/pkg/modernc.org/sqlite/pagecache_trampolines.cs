// Copyright 2026 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using fmt = fmt_package;
using bits = go.math.bits_package;
using sync = sync_package;
using @unsafe = unsafe_package;
using Δlibc = go.modernc.org.libc_package;
using types = go.modernc.org.libc.sys.types_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using go.math;
using go.modernc.org;
using go.modernc.org.libc.sys;
using static sqlite_package;

partial class sqlite_package {

partial struct pcacheBinding {
    internal sync.Mutex mu;
    internal Cache cache;
    internal map<uint32, uintptr> byKey;
    internal map<uintptr, pcacheEntry> byStub;
}

partial struct pcacheEntry {
    internal Page page;
    internal uint32 key;
    internal bool pinned;
}

internal static void pcacheContractViolation(uint32 key, @string got) {
    throw panic(fmt.Sprintf("sqlite: page cache contract violation: Cache.Fetch returned %s for page %d while SQLite holds it pinned"u8, got, key));
}


partial struct pcacheBindingsᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<uintptr, ж<pcacheBinding>> m;
    internal pcacheIDGen gen;
}
[GoLazy] partial class lazyᴛpcacheBindings { internal static readonly ж<pcacheBindingsᴛ1> ᏑpcacheBindings = new StaticBox<pcacheBindingsᴛ1>(new pcacheBindingsᴛ1(
    m: new map<uintptr, ж<pcacheBinding>>{}
)); }
internal static ref pcacheBindingsᴛ1 pcacheBindings => ref ((StaticBox<pcacheBindingsᴛ1>)ᏑpcacheBindings).ValueSlot;

internal static ж<pcacheBinding> lookupPcacheBinding(uintptr h) {
    GoFrame ᒐ = default;
    try {
        ᏑpcacheBindings.of(pcacheBindingsᴛ1.Ꮡmu).RLock();
        defer(ᏑpcacheBindings.of(pcacheBindingsᴛ1.Ꮡmu).RUnlock, ref ᒐ);
        return pcacheBindings.m[h];
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static uintptr registerPcacheBinding(ж<pcacheBinding> b) {
    GoFrame ᒐ = default;
    try {
        ᏑpcacheBindings.of(pcacheBindingsᴛ1.Ꮡmu).Lock();
        defer(ᏑpcacheBindings.of(pcacheBindingsᴛ1.Ꮡmu).Unlock, ref ᒐ);
        var h = ᏑpcacheBindings.of(pcacheBindingsᴛ1.Ꮡgen).next();
        pcacheBindings.m[h] = b;
        return h;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static void unregisterPcacheBinding(uintptr h) {
    GoFrame ᒐ = default;
    try {
        ᏑpcacheBindings.of(pcacheBindingsᴛ1.Ꮡmu).Lock();
        defer(ᏑpcacheBindings.of(pcacheBindingsᴛ1.Ꮡmu).Unlock, ref ᒐ);
        delete(pcacheBindings.m, h);
        ᏑpcacheBindings.of(pcacheBindingsᴛ1.Ꮡgen).reclaim(h);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct pcacheIDGen {
    internal slice<uint64> bitset;
}

} partial class sqliteᴛpcacheIDGen {

internal static uintptr next(this ref pcacheIDGen g) {
    var @base = (uintptr)1;
    for (nint i = 0; i < len(g.bitset); (i, @base) = (i + 1, @base + 64)) {
        var b = g.bitset[i];
        if (b != 18446744073709551615UL) {
            var n = (uintptr)bits.TrailingZeros64(~b);
            g.bitset[i] |= (uint64)(((uint64)1).Lsh((uint64)(n)));
            return @base + n;
        }
    }
    g.bitset = append(g.bitset, (uint64)1);
    return @base;
}

internal static void reclaim(this ref pcacheIDGen g, uintptr id) {
    var bit = id - 1;
    g.bitset[bit / 64] &= unchecked((uint64)~(uint64)(((uint64)1 << (int)(bit % 64))));
}

} partial class sqlite_package {

internal static uintptr pcacheNullStub => 0;

internal static typesꓸSize_t pcacheStubSize = ((typesꓸSize_t)(/* unsafe.Sizeof(uintptr(0)) */ (uintptr)8 * 2));

internal static int32 pcacheTrampolineInit(ж<Δlibc.TLS> tls, uintptr arg) {
    return sqlite3.SQLITE_OK;
}

internal static void pcacheTrampolineShutdown(ж<Δlibc.TLS> tls, uintptr arg) { }

internal static uintptr pcacheTrampolineCreate(ж<Δlibc.TLS> tls, int32 szPage, int32 szExtra, int32 bPurgeable) {
    var module = pcacheState.registered;
    if (module == nil) {
        return pcacheNullStub;
    }
    var (cache, err) = module.Create((nint)szPage, (nint)szExtra, bPurgeable != 0);
    if (err != nil || cache == nil) {
        return pcacheNullStub;
    }
    var b = Ꮡ(new pcacheBinding(
        cache: cache,
        byKey: new map<uint32, uintptr>{},
        byStub: new map<uintptr, pcacheEntry>{}
    ));
    return registerPcacheBinding(b);
}

internal static void pcacheTrampolineCachesize(ж<Δlibc.TLS> tls, uintptr pCache, int32 nCachesize) {
    var b = lookupPcacheBinding(pCache);
    if (b == nil) {
        return;
    }
    b.cache.SetSize((nint)nCachesize);
}

internal static int32 pcacheTrampolinePagecount(ж<Δlibc.TLS> tls, uintptr pCache) {
    var b = lookupPcacheBinding(pCache);
    if (b == nil) {
        return 0;
    }
    return (int32)b.cache.PageCount();
}

internal static uintptr pcacheTrampolineFetch(ж<Δlibc.TLS> tls, uintptr pCache, uint32 key, int32 createFlag) {
    GoFrame ᒐ = default;
    try {
        var b = lookupPcacheBinding(pCache);
        if (b == nil) {
            return pcacheNullStub;
        }
        var page = b.cache.Fetch(key, (FetchMode)createFlag);

        b.Ꮡmu.Lock();
        var bʗ1 = b;
        defer(bʗ1.Ꮡmu.Unlock, ref ᒐ);
        var (old, had) = b.byKey[key, ꟷ];
        pcacheEntry held = default;
        if (had) {
            held = b.byStub[old];
        }

        if (page == nil) {
            if (had) {
                if (held.pinned) {
                    pcacheContractViolation(key, "nil");
                }
                delete(b.byKey, key);
                delete(b.byStub, old);
                Δlibc.Xfree(tls, old);
            }
            return pcacheNullStub;
        }
        if (had && AreEqual(held.page, page)) {
            held.pinned = true;
            b.byStub[old] = held;
            return old;
        }
        if (had) {
            if (held.pinned) {
                pcacheContractViolation(key, "a different Page");
            }
            delete(b.byStub, old);
            Δlibc.Xfree(tls, old);
        }
        var stub = Δlibc.Xcalloc(tls, 1, pcacheStubSize);
        if (stub == 0) {
            delete(b.byKey, key);
            return pcacheNullStub;
        }
        var f = @unsafe.Pointer.To<array<uintptr>>((@unsafe.Pointer)stub);
        (f.Value[0], f.Value[1]) = ((uintptr)page.Buf(), (uintptr)page.Extra());
        b.byKey[key] = stub;
        b.byStub[stub] = new pcacheEntry(page: page, key: key, pinned: true);
        return stub;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static void pcacheTrampolineUnpin(ж<Δlibc.TLS> tls, uintptr pCache, uintptr pPage, int32 discard) {
    var b = lookupPcacheBinding(pCache);
    if (b == nil) {
        return;
    }
    b.Ꮡmu.Lock();
    var (entry, ok) = b.byStub[pPage, ꟷ];
    b.Ꮡmu.Unlock();
    if (!ok) {
        return;
    }
    b.cache.Unpin(entry.page, discard != 0);
    if (discard == 0) {
        b.Ꮡmu.Lock();
        var (e, okΔ1) = b.byStub[pPage, ꟷ]; if (okΔ1) {
            e.pinned = false;
            b.byStub[pPage] = e;
        }
        b.Ꮡmu.Unlock();
        return;
    }
    b.Ꮡmu.Lock();
    delete(b.byKey, entry.key);
    delete(b.byStub, pPage);
    b.Ꮡmu.Unlock();
    Δlibc.Xfree(tls, pPage);
}

internal static void pcacheTrampolineRekey(ж<Δlibc.TLS> tls, uintptr pCache, uintptr pPage, uint32 oldKey, uint32 newKey) {
    GoFrame ᒐ = default;
    try {
        var b = lookupPcacheBinding(pCache);
        if (b == nil) {
            return;
        }
        b.Ꮡmu.Lock();
        var (entry, ok) = b.byStub[pPage, ꟷ];
        b.Ꮡmu.Unlock();
        if (!ok) {
            return;
        }
        b.cache.Rekey(entry.page, oldKey, newKey);
        b.Ꮡmu.Lock();
        var bʗ1 = b;
        defer(bʗ1.Ꮡmu.Unlock, ref ᒐ);
        var (collider, okΔ1) = b.byKey[newKey, ꟷ]; if (okΔ1 && collider != pPage) {
            delete(b.byKey, newKey);
            delete(b.byStub, collider);
            Δlibc.Xfree(tls, collider);
        }
        delete(b.byKey, oldKey);
        b.byKey[newKey] = pPage;
        entry.key = newKey;
        b.byStub[pPage] = entry;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void pcacheTrampolineTruncate(ж<Δlibc.TLS> tls, uintptr pCache, uint32 iLimit) {
    var b = lookupPcacheBinding(pCache);
    if (b == nil) {
        return;
    }
    b.cache.Truncate(iLimit);
    b.Ꮡmu.Lock();
    foreach (var (key, stub) in b.byKey) {
        if (key >= iLimit) {
            delete(b.byKey, key);
            delete(b.byStub, stub);
            Δlibc.Xfree(tls, stub);
        }
    }
    b.Ꮡmu.Unlock();
}

internal static void pcacheTrampolineDestroy(ж<Δlibc.TLS> tls, uintptr pCache) {
    var b = lookupPcacheBinding(pCache);
    if (b == nil) {
        return;
    }
    b.cache.Destroy();
    b.Ꮡmu.Lock();
    foreach (var (_, stub) in b.byKey) {
        Δlibc.Xfree(tls, stub);
    }
    b.byKey = default;
    b.byStub = default;
    b.Ꮡmu.Unlock();
    unregisterPcacheBinding(pCache);
}

internal static void pcacheTrampolineShrink(ж<Δlibc.TLS> tls, uintptr pCache) {
    var b = lookupPcacheBinding(pCache);
    if (b == nil) {
        return;
    }
    b.cache.Shrink();
}

} // end sqlite_package
