// mem_persistent_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// persistentalloc1 and inPersistentAlloc, hand-owned (increment 6 of the runtime row, Q-memory W1;
// the two are displaced through manualConversionFuncs, so each flavour's malloc.cs carries a
// placeholder where the converted body was).
//
// WHY. Go keeps persistentChunks -- the list of every chunk persistentalloc ever mapped -- as a
// lock-free push through `atomic.Casuintptr((*uintptr)(unsafe.Pointer(&persistentChunks)), …)`, and
// reads it back with `atomic.Loaduintptr` over the same reinterpretation: a POINTER-typed global
// viewed as the integer it holds. The converter emits that view as
// `ᏑpersistentChunks.Reinterpret<ж<notInHeap>, uintptr>()`, and golib has no arm for it -- nothing
// can alias a managed reference slot as an integer -- so the address route mints a nil box and the
// first CAS dereferences it: the nil-dereference every persistentalloc row died on once sysMmap
// answered (the runtime row's increment-5 sizing probe, 2026-09-05; the S1/CS0030 fork's
// NATIVE-pointer side, since the chunks themselves are mmap'd memory outside the CLR heap). Those
// two lines are the only thing this file changes: the CAS becomes an Interlocked exchange of BOXES
// on the managed slot, compared by the native address each box carries; the Load becomes a volatile
// read of the slot. Every other line is the emission, kept line for line -- the size/align throws,
// the 64 KiB direct path, the per-P or global persistentAlloc under globalAlloc.mutex, the chunk
// carve, the sysStat accounting -- so the corpus's own algorithm is what runs.
//
// The chunk's first word still receives the previous head's address through the NativeBox exactly
// as Go writes it (`*(*uintptr)(unsafe.Pointer(persistent.base)) = chunks`), which is what lets
// inPersistentAlloc walk the list the way Go walks it: chunk address -> the word at it.
//
// Hand-owned: there is no mem_persistent_impl.go, so a -stdlib reconvert never regenerates it. The
// panic texts below are this file's own (a displaced body takes its hoisted literals with it).
//
// THE CAS COMPARAND (coordinator condition, 2026-09-05): Interlocked.CompareExchange on the head slot
// compares box IDENTITY, never native address. The comparand is therefore the EXACT box observed in
// the same iteration -- Go's `chunks := uintptr(unsafe.Pointer(persistentChunks))`, read ONCE and
// used for both the first-word store and the CAS -- so a re-minted box over the same address can
// never satisfy it (no spurious success over a stale head whose address was reused) and the loop
// can never livelock against itself (the comparand is the instance the slot really holds, which is
// what the exchange compares). pushPersistentChunk is that loop body; the guard drives it with N
// threads x M pushes and with a distinct box over the same address (RuntimeMemoryFamilyTests).
//
// DELIBERATELY NOT COVERED -- the 19 other emitted sites of the same view (21 in the corpus-wide
// census of 2026-09-05, 7 Go sites; none reached by any row), which the general remedy (B) -- a golib
// pointer-slot view for every `(*uintptr)(unsafe.Pointer(&p))` over a pointer-typed p, with atomic
// overloads and a converter emission, sized in the increment-6 design post -- closes at once when a
// second row reaches one of them: the persistentChunks CAS and Load in windows/malloc.cs and
// darwin/malloc.cs (the same two Go sites, unreached there because those flavours have no memory
// primitives yet); arena.cs's two stores (the user-arena chunk list; windows/linux/darwin, behind
// mallocinit); panic.cs's two defer-link stores (behind the converter's own defer lowering);
// mgcsweep.cs's largeType store; mgcstack.cs's stackObjectRecord store; os_linux.cs's and
// os_darwin.cs's ss_sp stores (the signal stack); debuglog.cs's three allDloggers sites. A second
// reaching site is what builds (B); nobody re-censuses.

using System.Threading;
using go.golib;
using atomic = go.@internal.runtime.atomic_package;
using goarch = go.@internal.goarch_package;
using @unsafe = go.unsafe_package;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    private static readonly @string persistentallocSizeIsZeroˢ = "persistentalloc: size == 0"u8;
    private static readonly @string persistentallocAlignNotPow2ˢ = "persistentalloc: align is not a power of 2"u8;
    private static readonly @string persistentallocAlignTooLargeˢ = "persistentalloc: align is too large"u8;
    private static readonly @string persistentallocCannotAllocateˢ = "runtime: cannot allocate memory"u8;

    // Wrapper around sysAlloc that can allocate small chunks. There is no associated free
    // operation. Intended for things like function/type/debug-related persistent data. If align is
    // 0, uses default align (currently 8). The returned memory will be zeroed. sysStat must be
    // non-nil. Consider marking persistentalloc'd types not in heap by embedding
    // internal/runtime/sys.NotInHeap.
    internal static ж<notInHeap> persistentalloc1(uintptr size, uintptr align, ж<sysMemStat> ᏑsysStat)
    {
        uintptr maxBlock = /* 64 << 10 */ 65536; // VM reservation granularity is 64K on windows
        if (size == 0) {
            @throw(persistentallocSizeIsZeroˢ);
        }
        if (align != 0){
            if ((uintptr)(align & (align - 1)) != 0) {
                @throw(persistentallocAlignNotPow2ˢ);
            }
            if (align > _PageSize) {
                @throw(persistentallocAlignTooLargeˢ);
            }
        } else {
            align = 8;
        }
        if (size >= maxBlock) {
            return (ж<notInHeap>)(uintptr)(sysAlloc(size, ᏑsysStat));
        }
        var mp = acquirem();
        ж<persistentAlloc> persistent = default!;
        if (mp != nil && (~mp).p != 0){
            persistent = (~mp).p.ptr().of(runtime_package.Δp.Ꮡpalloc);
        } else {
            @lock(ᏑglobalAlloc.of(globalAllocᴛ1.Ꮡmutex));
            persistent = ᏑglobalAlloc.of(globalAllocᴛ1.ᏑpersistentAlloc);
        }
        persistent.Value.off = alignUp((~persistent).off, align);
        if ((~persistent).off + size > persistentChunkSize || (~persistent).@base == nil) {
            persistent.Value.@base = (ж<notInHeap>)(uintptr)(sysAlloc(persistentChunkSize, Ꮡmemstats.of(mstats.Ꮡother_sys)));
            if ((~persistent).@base == nil) {
                if (persistent == ᏑglobalAlloc.of(globalAllocᴛ1.ᏑpersistentAlloc)) {
                    unlock(ᏑglobalAlloc.of(globalAllocᴛ1.Ꮡmutex));
                }
                @throw(persistentallocCannotAllocateˢ);
            }
            // Add the new chunk to the persistentChunks list -- W1: the lock-free push of a NATIVE
            // chunk onto a MANAGED pointer slot (see the header for the comparand).
            pushPersistentChunk((~persistent).@base);
            persistent.Value.off = alignUp(goarch.PtrSize, align);
        }
        var Δp = (~persistent).@base.add((~persistent).off);
        persistent.Value.off += size;
        releasem(ref (mp).DerefOrNull());
        if (persistent == ᏑglobalAlloc.of(globalAllocᴛ1.ᏑpersistentAlloc)) {
            unlock(ᏑglobalAlloc.of(globalAllocᴛ1.Ꮡmutex));
        }
        if (ᏑsysStat != Ꮡmemstats.of(mstats.Ꮡother_sys)) {
            ᏑsysStat.add((int64)size);
            Ꮡmemstats.of(mstats.Ꮡother_sys).add(-(int64)size);
        }
        return Δp;
    }

    // pushPersistentChunk is Go's push loop, line for line: observe the head ONCE, write its address
    // into the new chunk's first word (through the NativeBox, exactly as Go writes it), and exchange
    // the slot against THAT observed instance; retry until the exchange lands. Returns the number of
    // iterations the push took (1 = uncontended), which the guard reads.
    private static int pushPersistentChunk(ж<notInHeap> chunk)
    {
        int iterations = 0;
        while (true) {
            iterations++;
            ж<notInHeap> head = persistentChunks;
            var chunks = (uintptr)head;
            (chunk.Reinterpret<notInHeap, uintptr>()).Value = chunks;
            if (ReferenceEquals(Interlocked.CompareExchange(ref ᏑpersistentChunks.ValueSlot, chunk, head), head)) {
                return iterations;
            }
        }
    }

    // inPersistentAlloc reports whether p points to memory allocated by persistentalloc. This must
    // be nosplit because it is called by the cgo checker code, which is called by the write barrier
    // code.
    internal static bool inPersistentAlloc(uintptr Δp)
    {
        // W1: Go's atomic.Loaduintptr over the pointer slot is a volatile read of the box it holds.
        var chunk = (uintptr)Volatile.Read(ref ᏑpersistentChunks.ValueSlot);
        while (chunk != 0) {
            if (Δp >= chunk && Δp < chunk + (uintptr)persistentChunkSize) {
                return true;
            }
            chunk = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)chunk);
        }
        return false;
    }

    // ---- the guard's view (GolibTests RuntimeMemoryFamilyTests) ----

    /// <summary>Walks persistentChunks the way inPersistentAlloc does and counts its nodes.</summary>
    public static int GoPersistentChunkCount()
    {
        int n = 0;
        var chunk = (uintptr)Volatile.Read(ref ᏑpersistentChunks.ValueSlot);
        while (chunk != 0) {
            n++;
            chunk = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)chunk);
        }
        return n;
    }

    /// <summary>Pushes one freshly mapped chunk (persistentChunkSize bytes) through the real push loop; returns the iterations it took.</summary>
    public static int GoPersistentChunkPushProbe()
    {
        ж<notInHeap> chunk = (ж<notInHeap>)(uintptr)(sysAlloc(persistentChunkSize, Ꮡmemstats.of(mstats.Ꮡother_sys)));
        return pushPersistentChunk(chunk);
    }

    /// <summary>
    /// The comparand condition, made observable: a DISTINCT box minted over the head's own address must
    /// NOT satisfy the exchange (identity, not address), while the observed box must. Returns
    /// (the head's address, whether the re-minted box was a different instance, whether the exchange
    /// against the re-minted box failed, whether the exchange against the observed box then succeeded);
    /// the slot is restored to the observed head afterwards, so the list is unchanged.
    /// </summary>
    public static (nuint headAddress, bool remintedIsDistinct, bool remintedRefused, bool observedAccepted) GoPersistentChunkCasIdentityProbe()
    {
        ж<notInHeap> observed = persistentChunks;
        if (observed is null || (uintptr)observed == 0) {
            GoPersistentChunkPushProbe();
            observed = persistentChunks;
        }
        nuint address = (uintptr)observed;
        ж<notInHeap> reminted = (ж<notInHeap>)(uintptr)(@unsafe.Pointer)(uintptr)address;
        bool distinct = !ReferenceEquals(reminted, observed) && (uintptr)reminted == address;
        ж<notInHeap> probe = (ж<notInHeap>)(uintptr)(sysAlloc(persistentChunkSize, Ꮡmemstats.of(mstats.Ꮡother_sys)));
        (probe.Reinterpret<notInHeap, uintptr>()).Value = (uintptr)address;
        bool remintedRefused = !ReferenceEquals(Interlocked.CompareExchange(ref ᏑpersistentChunks.ValueSlot, probe, reminted), reminted);
        bool observedAccepted = ReferenceEquals(Interlocked.CompareExchange(ref ᏑpersistentChunks.ValueSlot, probe, observed), observed);
        if (observedAccepted) {
            Interlocked.Exchange(ref ᏑpersistentChunks.ValueSlot, observed);   // restore: the probe chunk leaves the list
        }
        return (address, distinct, remintedRefused, observedAccepted);
    }

    /// <summary>Two persistentalloc'd blocks as native addresses (distinct, inside the chunk list, zeroed).</summary>
    public static (nuint first, nuint second, bool firstInList, bool secondInList, bool strangerInList) GoPersistentAllocProbe(nuint size)
    {
        ж<notInHeap> a = persistentalloc1((uintptr)size, 0, Ꮡmemstats.of(mstats.Ꮡother_sys));
        ж<notInHeap> b = persistentalloc1((uintptr)size, 0, Ꮡmemstats.of(mstats.Ꮡother_sys));
        nuint pa = (uintptr)a, pb = (uintptr)b;
        return (pa, pb, inPersistentAlloc((uintptr)pa), inPersistentAlloc((uintptr)pb), inPersistentAlloc((uintptr)0x10));
    }
}
