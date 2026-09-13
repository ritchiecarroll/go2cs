// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bcache implements a GC-friendly cache (see [Cache]) for BoringCrypto.

// go2cs HAND-OWNED (whole-file), and ONE method deep: everything below is the converted cache.go
// output verbatim except `Register`, whose Go body hands the runtime a raw ADDRESS. The converter
// skips regenerating a file carrying this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker) and drops a cache.cs.auto review sibling beside it.
//
// WHAT GO DOES. `registerCache(unsafe.Pointer(&c.ptable))` gives the runtime the address of the
// cache's table word; runtime.clearpools — which gcStart runs at the head of every cycle — stores
// nil into each registered address with atomicstorep. The declaration this replaces,
// `func registerCache(unsafe.Pointer)`, is bodyless in Go (provided by the runtime via linkname),
// which the converter emits as a bodyless partial and go2cs-gen's PartialStubGenerator fills with
// a throwing stub when no companion implements it. That stub was reached from this package's own
// init — Register must be called during package initialization — so the suite died in its static
// constructor with NotImplementedException before a single test ran.
//
// WHY THE ADDRESS CANNOT BE THE CURRENCY. The registered word is an
// atomic.Pointer[cacheTable[K,V]], whose managed slot holds a ж<T> REFERENCE rather than a machine
// word. Storage containing references is not pinnable, so the ж<T> → uintptr conversion gets no pin
// back, the provenance record it writes can never satisfy IsPinnedAt, and ManagedPointerTokens
// answers MISS for the number by design. The number that reached registerCache named nothing
// recoverable — a structural fact about the slot's type, not a gap to widen around. Pinning it to
// force the issue would also defeat the package's entire purpose, which is to let the collector
// reclaim what this cache holds.
//
// WHAT REPLACES IT. A registration is a clear DELEGATE — the same currency the runtime's two other
// clearpools arms already use — and the delegate handed over is this package's OWN Clear. Go
// documents that method as precisely this mechanism ("the runtime does this automatically at each
// garbage collection; this method is exposed only for testing"), so the managed model performs the
// operation Go names, by the route Go's own comment describes, instead of simulating the address
// store it happens to be implemented with. The cadence, the gen2-cycle identity it follows, and the
// two named differences from Go all live on golib.BoringCaches; runtime.GC() clears the registry
// directly before returning, exactly as it invokes poolcleanup directly.
[module: go.GoManualConversion]

namespace go.crypto.@internal.boring;

using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class bcache_package {


// A Cache is a GC-friendly concurrent map from unsafe.Pointer to
// unsafe.Pointer. It is meant to be used for maintaining shadow
// BoringCrypto state associated with certain allocated structs, in
// particular public and private RSA and ECDSA keys.
//
// The cache is GC-friendly in the sense that the keys do not
// indefinitely prevent the garbage collector from collecting them.
// Instead, at the start of each GC, the cache is cleared entirely. That
// is, the cache is lossy, and the loss happens at the start of each GC.
// This means that clients need to be able to cope with cache entries
// disappearing, but it also means that clients don't need to worry about
// cache entries keeping the keys from being collected.
[GoType] partial struct Cache<K, V> {
    // The runtime atomically stores nil to ptable at the start of each GC.
    internal atomic.Pointer<cacheTable<K, V>> ptable;
}

[GoType("[1021]sync.atomic_package.Pointer<cacheEntry<K, V>>")] /* [cacheSize]sync.atomic_package.Pointer<cacheEntry<K, V>> */
partial struct cacheTable<K, V>;

// A cacheEntry is a single entry in the linked list for a given hash table entry.
[GoType] partial struct cacheEntry<K, V> {
    internal ж<K> k;             // immutable once created
    internal atomic.Pointer<V> v; // read and written atomically to allow updates
    internal ж<cacheEntry<K, V>> next; // immutable once linked into table
}

// provided by runtime
//
// go2cs: Go's `func registerCache(unsafe.Pointer)` is bodyless here and linknamed to
// runtime.boring_registerCache, which appends the address to the slice clearpools walks. The
// managed registry takes a clear delegate instead of an address (see the file header), so the
// declaration has nothing left to stand for and is replaced by the registration below.

// Register registers the cache with the runtime,
// so that c.ptable can be cleared at the start of each GC.
// Register must be called during package initialization.
public static void Register<K, V>(this ж<Cache<K, V>> Ꮡc) {
    // Clear IS the operation Go's runtime performs on this cache at each collection — its own doc
    // comment below says so. Registering it keeps the CACHE alive for the process, exactly as Go's
    // boringCaches slice keeps every registered &c.ptable alive; what the clear releases is the
    // entries, which is the whole of the package's GC-friendliness.
    golib.BoringCaches.Register(() => Ꮡc.Clear());
}

// cacheSize is the number of entries in the hash table.
// The hash is the pointer value mod cacheSize, a prime.
// Collisions are resolved by maintaining a linked list in each hash slot.
internal static UntypedInt cacheSize => 1021;

// table returns a pointer to the current cache hash table,
// coping with the possibility of the GC clearing it out from under us.
internal static ж<cacheTable<K, V>> table<K, V>(this ж<Cache<K, V>> Ꮡc) {
    while (ᐧ) {
        var p = Ꮡc.of(Cache<K, V>.Ꮡptable).Load();
        if (p == nil) {
            p = @new<cacheTable<K, V>>();
            if (!Ꮡc.of(Cache<K, V>.Ꮡptable).CompareAndSwap(nil, p)) {
                continue;
            }
        }
        return p;
    }
}

// Clear clears the cache.
// The runtime does this automatically at each garbage collection;
// this method is exposed only for testing.
public static void Clear<K, V>(this ж<Cache<K, V>> Ꮡc) {
    // The runtime does this at the start of every garbage collection
    // (itself, not by calling this function).
    Ꮡc.of(Cache<K, V>.Ꮡptable).Store(nil);
}

// Get returns the cached value associated with v,
// which is either the value v corresponding to the most recent call to Put(k, v)
// or nil if that cache entry has been dropped.
public static ж<V> Get<K, V>(this ж<Cache<K, V>> Ꮡc, ж<K> Ꮡk) {
    ref var k = ref Ꮡk.DerefOrNull();

    var head = Ꮡc.table().at<atomic.Pointer<cacheEntry<K, V>>>((nint)((uintptr)Ꮡk % (uintptr)cacheSize));
    var e = head.Load();
    for (; e != nil; e = e.Value.next) {
        if ((~e).k == Ꮡk) {
            return e.of(cacheEntry<K, V>.Ꮡv).Load();
        }
    }
    return default!;
}

// Put sets the cached value associated with k to v.
public static void Put<K, V>(this ж<Cache<K, V>> Ꮡc, ж<K> Ꮡk, ж<V> Ꮡv) {
    ref var k = ref Ꮡk.DerefOrNull();
    ref var v = ref Ꮡv.DerefOrNull();

    var head = Ꮡc.table().at<atomic.Pointer<cacheEntry<K, V>>>((nint)((uintptr)Ꮡk % (uintptr)cacheSize));
    // Strategy is to walk the linked list at head,
    // same as in Get, to look for existing entry.
    // If we find one, we update v atomically in place.
    // If not, then we race to replace the start = *head
    // we observed with a new k, v entry.
    // If we win that race, we're done.
    // Otherwise, we try the whole thing again,
    // with two optimizations:
    //
    //  1. We track in noK the start of the section of
    //     the list that we've confirmed has no entry for k.
    //     The next time down the list, we can stop at noK,
    //     because new entries are inserted at the front of the list.
    //     This guarantees we never traverse an entry
    //     multiple times.
    //
    //  2. We only allocate the entry to be added once,
    //     saving it in add for the next attempt.
    ж<cacheEntry<K, V>> add = default!;
    ж<cacheEntry<K, V>> noK = default!;
    nint n = 0;
    while (ᐧ) {
        var e = head.Load();
        var start = e;
        for (; e != nil && e != noK; e = e.Value.next) {
            if ((~e).k == Ꮡk) {
                e.of(cacheEntry<K, V>.Ꮡv).Store(Ꮡv);
                return;
            }
            n++;
        }
        if (add == nil) {
            add = Ꮡ(new cacheEntry<K, V>(k: Ꮡk));
            add.of(cacheEntry<K, V>.Ꮡv).Store(Ꮡv);
        }
        add.Value.next = start;
        if (n >= 1000) {
            // If an individual list gets too long, which shouldn't happen,
            // throw it away to avoid quadratic lookup behavior.
            add.Value.next = default!;
        }
        if (head.CompareAndSwap(start, add)) {
            return;
        }
        noK = start;
    }
}

} // end bcache_package
