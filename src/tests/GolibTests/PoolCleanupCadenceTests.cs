// PoolCleanupCadenceTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using any = System.Object;
using sync = go.sync_package;
using Δdebug = go.runtime.debug_package;
using Δruntime = go.runtime_package;

namespace GolibTests;

/// <summary>
/// The standing guard over WHEN the converted <c>sync.Pool</c> ages its caches: Go runs
/// <c>clearpools</c> at the start of every GC cycle, automatic ones included, so a pooled object
/// that nobody takes back is dropped after two cycles whether or not the program ever calls
/// <c>runtime.GC()</c>.
/// </summary>
/// <remarks>
/// <para>
/// WHY THIS FILE EXISTS. Until the per-collection cadence, the converted runtime ran the pool
/// cleanup only inside its own GC cycle (<c>runtime.GC()</c>, or the periodic tick, which a CLR gen2
/// collection resets), so a busy program never aged its pools at all. TypeScript's converted
/// compiler suite showed the cost: its pooled emit contexts kept every node graph they had built,
/// about 150 MB per 454 test configurations, without bound (DATA-ts-conformance-scout.md, section
/// 8). Nothing else at master can see the cadence: CNR is transpile-only, sync's banked Pool tests
/// call <c>runtime.GC()</c> themselves, and the behavioral suite never runs long enough to matter.
/// </para>
/// <para>
/// WHAT IS AND IS NOT ASSERTED. Pool's contract is that an item MAY vanish at any time, so no arm
/// here asserts that a pooled item survives an automatic collection with the collector on. The arms
/// assert the three things Go does promise: an abandoned item is released by automatic cycles alone
/// (1); with the collector off (<c>SetGCPercent(-1)</c>, Go's "no automatic cycles"), nothing ages
/// except <c>runtime.GC()</c> (2); and one <c>runtime.GC()</c> is ONE aging, so the victim cache
/// still answers after it and is gone after a second one, which is the shape sync's own TestPool
/// reads (3). Each pool below is the arm's own, so no other test's use can move it.
/// </para>
/// <para>
/// THE GENERATION. Arm 1 ends each round with a full collection, so it is green on a cadence that
/// fires only at gen2. Arms 4 and 5 run gen0 collections and nothing else, so they are the arms that
/// tell the per-collection cadence from a gen2 one: both red when the sentinel re-registers itself
/// (<c>GC.ReRegisterForFinalize(this)</c>) instead of allocating a fresh one.
/// </para>
/// </remarks>
[TestClass]
public class PoolCleanupCadenceTests
{
    // Bound for the asynchronous arm: the cleanup runs from a finalizer, which the caller does not
    // schedule, so it may take more than one collection to come around. It is a flake guard, not a
    // weakening: a cadence that never fires never releases the item at any number of rounds.
    private const int Rounds = 8;

    private static sync.Pool s_abandoned;
    private static sync.Pool s_gcOff;
    private static sync.Pool s_onceNotTwice;
    private static sync.Pool s_gen0Only;

    private static void EphemeralCollection()
    {
        GC.Collect(0, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
    }

    private static void FullCollection()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
    }

    // Kept out of line so no local in the caller's frame still references the item.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference PutAbandoned(ref sync.Pool pool)
    {
        object item = new byte[1024];
        pool.Put(item);
        return new WeakReference(item);
    }

    /// <summary>
    /// The CADENCE arm: automatic (here ephemeral) collections alone age the pool, so an item
    /// nobody takes back is released with no <c>runtime.GC()</c> call anywhere.
    /// </summary>
    /// <remarks>
    /// The aging collections are GEN0 on purpose. A cadence filtered to gen2 (the BoringCaches
    /// identity) does not release the item here, which is the cadence the TypeScript measurement
    /// refuted (section 4.10: 14 gen2 cleanups in a 189 s run left the growth unchanged). One full
    /// collection at the end only reclaims what the agings released: the item has been promoted out
    /// of gen0 by then, so an ephemeral collection could not free it whatever the pool did.
    /// </remarks>
    [TestMethod]
    public void AnAbandonedItemIsReleasedByAutomaticCollections()
    {
        FullCollection();

        WeakReference item = PutAbandoned(ref s_abandoned);

        int rounds = 0;

        while (rounds < Rounds && item.IsAlive)
        {
            rounds++;
            EphemeralCollection();
            EphemeralCollection();
            EphemeralCollection();
            FullCollection();
        }

        Console.WriteLine($"[PoolCleanupCadence] abandoned item alive={item.IsAlive} after {rounds} round(s) " +
                          $"of three gen0 collections and one full collection; gen0 count {GC.CollectionCount(0)}.");

        Assert.IsFalse(item.IsAlive,
            $"a pooled item nobody took back was still reachable after {Rounds} rounds of automatic " +
            "collections with finalizers drained, and no runtime.GC() call. The pool cleanup is not running " +
            "at the collector's cadence: Go runs clearpools at the start of EVERY cycle, so two cycles drop it.");
    }

    /// <summary>
    /// The NEGATIVE arm: with the collector off, automatic collections do not age the pool.
    /// </summary>
    /// <remarks>
    /// Go's <c>SetGCPercent(-1)</c> stops automatic cycles, and sync's TestPool relies on it to
    /// control when aging happens. The CLR still collects, so the cadence has to read the same
    /// switch; a cadence that ignores it empties this pool and reds this arm alone.
    /// </remarks>
    [TestMethod]
    public void WithTheCollectorOffAutomaticCollectionsDoNotAge()
    {
        nint previous = Δdebug.SetGCPercent(-1);

        try
        {
            s_gcOff.Put("kept");

            EphemeralCollection();
            EphemeralCollection();
            EphemeralCollection();
            FullCollection();
            FullCollection();

            object? got = s_gcOff.Get();

            Assert.AreEqual("kept", got,
                "with SetGCPercent(-1) the pool lost its item to collections nobody asked for. Go runs no " +
                "automatic cycle with the collector off, so only runtime.GC() may age the pool then.");
        }
        finally
        {
            Δdebug.SetGCPercent(previous);
        }
    }

    /// <summary>
    /// The DOUBLE-AGING arm: one <c>runtime.GC()</c> is one aging, as in Go.
    /// </summary>
    /// <remarks>
    /// <c>runtime.GC()</c> runs the cleanup itself, at the head of its cycle, and then collects
    /// twice. Each of those collections would fire the per-collection cadence too, and three agings
    /// in one call would drop the victim cache that Go keeps for exactly one cycle; the cadence skips
    /// them because their firings land while the call holds the GC cycle lock. This is sync's
    /// TestPool shape (one GC keeps the item through the victim cache, a second drops it), run with
    /// the collector ON so the cadence is live; a full collection first leaves gen0 empty, so no
    /// automatic collection lands between the Put and the call.
    /// </remarks>
    [TestMethod]
    public void OneRuntimeGCIsOneAging()
    {
        for (int i = 0; i < 100; i++)
            s_onceNotTwice.Put("c");

        FullCollection();

        for (int i = 0; i < 100; i++)
            s_onceNotTwice.Put("c");

        Δruntime.GC();

        object? afterOne = s_onceNotTwice.Get();

        Assert.AreEqual("c", afterOne,
            "after ONE runtime.GC() the pool's victim cache no longer answered. The call aged the pool more " +
            "than once: the per-collection cadence fired for collections runtime.GC() had already served.");

        Δruntime.GC();
        Δruntime.GC();

        Assert.IsNull(s_onceNotTwice.Get(),
            "after further runtime.GC() calls the pool still answered: runtime.GC() is not aging the pool at all.");
    }

    /// <summary>
    /// The GEN0 arm: ephemeral collections alone, with no full collection anywhere in the arm, age
    /// the pool, so an item put and never taken back is gone from it.
    /// </summary>
    /// <remarks>
    /// The arm reads the pool (<c>Get</c>), not the item's reachability: an item promoted out of gen0
    /// is not freed by a gen0 collection whatever the pool did, but two agings take it out of the pool
    /// (local to victim, victim dropped). The full collection before the Put only settles the heap; a
    /// sentinel that re-registers itself is in gen2 after it and never fires here.
    /// </remarks>
    [TestMethod]
    public void AnAbandonedItemIsReleasedByGen0CollectionsAlone()
    {
        FullCollection();

        s_gen0Only.Put("abandoned");

        int rounds = 0;
        object? got = "abandoned";

        while (rounds < Rounds)
        {
            rounds++;
            EphemeralCollection();
            EphemeralCollection();
            EphemeralCollection();

            if (rounds >= 2)
            {
                got = s_gen0Only.Get();

                if (got is null)
                    break;

                s_gen0Only.Put(got);
            }
        }

        Console.WriteLine($"[PoolCleanupCadence] gen0-only: pool answered {got ?? "nothing"} after {rounds} round(s) " +
                          $"of three gen0 collections; gen0 count {GC.CollectionCount(0)}, agings {Δruntime.GoPoolCleanupAgings}.");

        Assert.IsNull(got,
            $"the pool still held an item put before {Rounds * 3} gen0 collections, with no full collection and no " +
            "runtime.GC(). The cleanup is not running at every collection: a cadence that fires only at gen2 (a " +
            "sentinel that re-registers itself) never ages a pool between full collections.");
    }

    /// <summary>
    /// The COUNTER arm: the cadence's aging count advances under gen0 collections, and its failure
    /// count does not.
    /// </summary>
    /// <remarks>
    /// The finalizer swallows a cleanup that throws, so the failure count is the only trace of one.
    /// </remarks>
    [TestMethod]
    public void AgingsAdvanceUnderGen0Collections()
    {
        FullCollection();

        long agings = Δruntime.GoPoolCleanupAgings;
        long failures = Δruntime.GoPoolCleanupFailures;

        for (int i = 0; i < 6; i++)
            EphemeralCollection();

        long agingsAfter = Δruntime.GoPoolCleanupAgings;
        long failuresAfter = Δruntime.GoPoolCleanupFailures;

        Console.WriteLine($"[PoolCleanupCadence] agings {agings} -> {agingsAfter}, failures {failures} -> {failuresAfter} " +
                          "over six gen0 collections.");

        Assert.IsTrue(agingsAfter > agings,
            $"six gen0 collections ran no pool cleanup from the cadence (agings {agings} -> {agingsAfter}).");

        Assert.AreEqual(failures, failuresAfter,
            "the pool cleanup threw from the cadence's finalizer during six gen0 collections.");
    }
}
