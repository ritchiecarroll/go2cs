// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted pool.go output — a member of the
// native sync family alongside Mutex/RWMutex/WaitGroup, the runtime hooks in runtime_impl.cs, and the
// poolDequeue slot fork in poolqueue.cs). Go's Pool is a per-P sharded cache co-designed with the
// scheduler: procPin pins the goroutine to a P, the P's id indexes a [P]poolLocal block reached by
// POINTER ARITHMETIC through an unsafe.Pointer (indexLocal), and the GC drives the victim-cache aging.
// Converted literally that is raw metal on a managed referent — the block is a managed array, so the
// pointer arithmetic has no meaning here — and the pin, which is what gives each shard's dequeue its
// single producer, has nothing to pin to.
//
// So the whole type is reimplemented on managed primitives, keeping every piece of Go's ALGORITHM
// (private slot, then the shard's shared chain, then stealing from other shards, then the victim
// cache; poolCleanup ages local → victim → dropped) and replacing only the three pieces that cannot
// exist here:
//
//   - The [P]poolLocal block becomes a poolLocal[] held directly (poolState), so a shard is an object
//     reference instead of an offset. The cache-line pad Go adds against false sharing has no managed
//     expression — each shard is separately allocated — so it is gone.
//   - The P pin becomes a THREAD-affine shard index (runtime_procPin in runtime_impl.cs). Threads
//     outnumber shards, so two threads can share one shard, which a real pin rules out; this file
//     closes that gap where it matters: the private slot is taken and filled with a single interlocked
//     step, and the shard's shared-chain HEAD — single-producer by poolqueue.go's contract — is
//     serialized by a per-shard producer gate. Stealing (popTail) stays lock-free, as designed.
//   - poolCleanup is registered with the runtime exactly as Go registers it, and runs when a garbage
//     collection is REQUESTED (runtime.GC() → the pool hook; see runtime's managed_impl.cs) rather
//     than at the start of every cycle including automatic ones. Two honest consequences: a program
//     that never calls runtime.GC() holds its cached items longer than Go's would (a retention
//     divergence, not a correctness one — Pool only promises that items MAY vanish), and the cleanup
//     runs on the caller's thread instead of with the world stopped, so a Put racing it can land in a
//     shard that just became a victim and age one cycle early. Pool's contract permits exactly that.
using System.Collections.Generic;
using System.Threading;

// Hand-owned native replacement of the converted pool.go output — the converter skips regenerating a
// file that carries this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go;

using static sync_package;

partial class sync_package {

// A Pool is a set of temporary objects that may be individually saved and
// retrieved.
//
// Any item stored in the Pool may be removed automatically at any time without
// notification. If the Pool holds the only reference when this happens, the
// item might be deallocated.
//
// A Pool is safe for use by multiple goroutines simultaneously.
//
// A Pool must not be copied after first use.
[GoType] public partial struct Pool {
    // state materializes through the Pool's heap box on first Put/Get, so every access through the
    // same boxed Pool shares it (never through a copy — Go forbids copying a Pool after first use).
    // Go keeps this state in the Pool struct itself, as an unsafe.Pointer to the shard block plus its
    // length; holding it in one object instead also gives the cleanup registry a reference identity,
    // which a `ref Pool` receiver cannot supply.
    internal poolState? state;

    // New optionally specifies a function to generate
    // a value when Get would otherwise return nil.
    // It may not be changed concurrently with calls to Get.
    public Func<any> New;
}

// A Pool's mutable state; see Pool.state.
internal sealed class poolState
{
    // local is this cycle's shard array and victim is the previous cycle's. poolCleanup swaps them
    // without stopping the world, so both are read and written with Volatile.
    internal poolLocal[]? local;
    internal poolLocal[]? victim;
}

// poolLocal is one shard of a Pool's cache — Go's poolLocal, minus the false-sharing pad (see the
// file header).
internal sealed class poolLocal
{
    // Can be used only by the shard's current producer, and only through interlocked access (two
    // threads can share a shard here).
    internal any? @private;

    // The shard's producer can pushHead/popHead; any thread can popTail.
    internal readonly ж<poolChain> shared = @new<poolChain>();
}

// Pools with a shard array in this cycle, and those that had one in the previous cycle. Both are
// rebuilt by pin/poolCleanup exactly as Go rebuilds allPools/oldPools, so an abandoned Pool drops out
// of both within two cleanups and becomes collectible.
private static readonly object allPoolsMu = new();
private static List<poolState> allPools = [];
private static List<poolState> oldPools = [];

} partial class syncᴛPool {

// Put adds x to the pool.
[GoRecv] public static void Put(this ref Pool p, any x) {
    if (x is null or NilType) {
        return;
    }

    var (l, _) = p.pin();

    // Claim the private slot if it is free. One interlocked step, because the shard may have more
    // than one producer here (see the file header).
    if (Interlocked.CompareExchange(ref l.@private, x, null) is not null) {
        lock (l) {
            l.shared.pushHead(x);
        }
    }

    runtime_procUnpin();
}

// Get selects an arbitrary item from the [Pool], removes it from the
// Pool, and returns it to the caller.
// Get may choose to ignore the pool and treat it as empty.
// Callers should not assume any relation between values passed to [Pool.Put] and
// the values returned by Get.
//
// If Get would otherwise return nil and p.New is non-nil, Get returns
// the result of calling p.New.
[GoRecv] public static any Get(this ref Pool p) {
    var (l, pid) = p.pin();
    any? x = Interlocked.Exchange(ref l.@private, null);

    if (x is null) {
        // Try to pop the head of the local shard. We prefer the head over the tail for temporal
        // locality of reuse.
        lock (l) {
            (x, _) = l.shared.popHead();
        }

        if (x is null) {
            x = p.getSlow(pid);
        }
    }

    runtime_procUnpin();

    if (x is null && p.New != default!) {
        x = p.New();
    }

    return x!;
}

// getSlow looks for an item outside this shard: first the tail of every other shard (stealing), then
// the victim cache.
[GoRecv] internal static any getSlow(this ref Pool p, nint pid) {
    poolState state = stateOf(ref p);

    // Try to steal one element from other shards.
    poolLocal[]? locals = Volatile.Read(ref state.local);
    nint size = locals is null ? 0 : locals.Length;

    for (nint i = 0; i < size; i++) {
        var (x, _) = locals![(pid + i + 1) % size].shared.popTail();

        if (x is not null) {
            return x;
        }
    }

    // Try the victim cache. We do this after attempting to steal from all primary caches because we
    // want objects in the victim cache to age out if at all possible.
    locals = Volatile.Read(ref state.victim);
    size = locals is null ? 0 : locals.Length;

    if (pid >= size) {
        return default!;
    }
    {
        any? x = Interlocked.Exchange(ref locals![pid].@private, null);

        if (x is not null) {
            return x;
        }
    }

    for (nint i = 0; i < size; i++) {
        var (x, _) = locals![(pid + i) % size].shared.popTail();

        if (x is not null) {
            return x;
        }
    }

    // Mark the victim cache as empty so future gets don't bother with it. Go zeroes victimSize;
    // dropping the array does the same and releases the shards with it.
    Volatile.Write(ref state.victim, null);
    return default!;
}

} partial class sync_package {

// stateOf returns the Pool's state, creating it once on first use (race-safe).
internal static poolState stateOf(ref Pool p) {
    poolState? s = Volatile.Read(ref p.state);

    if (s is not null) {
        return s;
    }

    var created = new poolState();

    return Interlocked.CompareExchange(ref p.state, created, null) ?? created;
}

} partial class syncᴛPool {

// pin returns the poolLocal shard for the calling goroutine and the shard's index. Go pins the
// goroutine to its P for the duration and the caller must runtime_procUnpin(); the index here is
// thread-affine, so the pin costs nothing and the unpin has nothing to undo (see runtime_procPin).
[GoRecv] internal static (poolLocal, nint) pin(this ref Pool p) {
    // A nil Pool has already panicked by now: the receiver deref happens before this body runs, which
    // is what Go's explicit `if p == nil` check in pin buys (it panics rather than taking a nil deref
    // while pinned, where it would be a fatal error).
    poolState state = stateOf(ref p);
    nint pid = runtime_procPin();
    poolLocal[]? locals = Volatile.Read(ref state.local);

    if (locals is not null && pid < locals.Length) {
        return (locals[pid], pid);
    }

    return pinSlow(state);
}

} partial class sync_package {

// pinSlow allocates this cycle's shard array — Go's [P]poolLocal block — and enrolls the pool for
// cleanup. allPoolsMu serializes it against both other pins and poolCleanup.
internal static (poolLocal, nint) pinSlow(poolState state) {
    lock (allPoolsMu) {
        // Retry under the lock: another thread may have built the array, or poolCleanup may have
        // taken it away, since pin looked.
        nint pid = runtime_procPin();
        poolLocal[]? locals = state.local;

        if (locals is not null && pid < locals.Length) {
            return (locals[pid], pid);
        }

        if (locals is null) {
            allPools.Add(state);
        }

        // If GOMAXPROCS changes between cleanups, re-allocate the array and lose the old one. The
        // index came from the GOMAXPROCS in force a moment ago, so keep room for it either way.
        nint size = runtime_package.GOMAXPROCS(0);

        if (size <= pid) {
            size = pid + 1;
        }

        var local = new poolLocal[size];

        for (nint i = 0; i < size; i++) {
            local[i] = new poolLocal();
        }

        Volatile.Write(ref state.local, local);
        return (local[pid], pid);
    }
}

// poolCleanup drops every pool's victim cache and moves its primary cache into the victim slot. This
// is the aging step that lets a Pool release what it cached; Go's runtime runs it at the start of each
// GC cycle with the world stopped, and here it runs when a collection is requested (see the file
// header for both divergences).
internal static void poolCleanup() {
    lock (allPoolsMu) {
        // Drop victim caches from all pools.
        foreach (poolState state in oldPools) {
            Volatile.Write(ref state.victim, null);
        }

        // Move primary cache to victim cache.
        foreach (poolState state in allPools) {
            Volatile.Write(ref state.victim, Volatile.Read(ref state.local));
            Volatile.Write(ref state.local, null);
        }

        // The pools with non-empty primary caches now have non-empty victim caches and no pools have
        // primary caches.
        oldPools.Clear();
        (oldPools, allPools) = (allPools, oldPools);
    }
}

[GoInit] internal static void init() {
    runtime_registerPoolCleanup(poolCleanup);
}

// The runtime-provided hooks below stay declared here (their home file in Go is pool.go);
// sync's native runtime_impl.cs supplies the implementing parts, and other sync files
// (map.cs, once, cond) reference some of them.

internal static partial uint32 runtime_randn(uint32 n);

internal static partial void runtime_registerPoolCleanup(Action cleanup);

internal static partial nint runtime_procPin();

internal static partial void runtime_procUnpin();

// The below are implemented in internal/runtime/atomic and the
// compiler also knows to intrinsify the symbol we linkname into this
// package.

internal static partial uintptr runtime_LoadAcquintptr(ж<uintptr> ptr);

internal static partial uintptr runtime_StoreReluintptr(ж<uintptr> ptr, uintptr val);

} // end sync_package
