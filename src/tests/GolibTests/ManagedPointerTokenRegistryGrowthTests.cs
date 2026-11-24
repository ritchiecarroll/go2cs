// ManagedPointerTokenRegistryGrowthTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

// THE TOKEN REGISTRY'S GROWTH BOUND, MEASURED. Q44 (docs/phase4/DESIGN-managed-pointer-token.md)
// makes every address take of a reference-bearing box register the box's order token in
// ManagedPointerTokens — weakly, and swept: Register sweeps dead entries once the table has grown by
// SweepThreshold since the last sweep, then re-arms one threshold above the survivors. The design
// STATES that the table is therefore bounded by the live population plus a threshold's worth of
// not-yet-swept entries; COORD's ruling on the Q44 item-2 reading (2026-09-05) made that a
// measurement rather than a sentence — a claim about growth is a prediction until a run reads it.
//
// THE ARITHMETIC the bound rests on, so the assertion is derived and not felt: with no collection
// during the churn, every 256th registration sweeps nothing and re-arms 256 higher, so after N dead
// registrations the table holds N (+ the live sentinel) and the next sweep line sits ≤ 256 above it.
// One forced collection kills every dropped box; the next 257 registrations cross that line, the
// sweep drops the N collected entries, and what survives is the sentinel plus the ≤ 257 entries
// registered since — under two thresholds. A registry whose sweep did nothing would hold N + 257 +
// 1 here, which is what the neutered-Sweep control reads (RED, 4,354 against a bound of 513).
[TestClass]
public class ManagedPointerTokenRegistryGrowthTests
{
    // Reference-bearing on purpose: the class whose address take registers a token (no pinnable
    // storage, so the token arm is the only arm such a box can take).
    private struct ReferenceBearingShape
    {
        internal slice<byte> Held;
    }

    // A fresh box, its token taken — and so registered — and the box dropped on return. NoInlining so
    // the caller's frame never roots it.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nuint TakeAndDrop()
    {
        ref ReferenceBearingShape held = ref heap(new ReferenceBearingShape { Held = new slice<byte>(2) }, out ж<ReferenceBearingShape> box);
        return (nuint)(uintptr)box;
    }

    [TestMethod]
    public void TheTokenTableIsBoundedByTheLivePopulationPlusOneSweepThreshold()
    {
        const int dead = 4096;
        Assert.IsTrue(dead >= 8 * ManagedPointerTokens.SweepThreshold,
            "the churn must dwarf the threshold, or a table that never swept could still pass");

        // A LIVE registration made before the churn: the sweeps must keep it — the positive property
        // a bound alone would not test (a table that dropped everything is bounded too).
        ref ReferenceBearingShape held = ref heap(new ReferenceBearingShape { Held = new slice<byte>(1) }, out ж<ReferenceBearingShape> live);
        nuint liveToken = (nuint)(uintptr)live;
        Assert.AreSame(live, ManagedPointerTokens.Resolve(liveToken), "the sentinel must resolve before the churn");

        nuint sink = 0;

        for (int i = 0; i < dead; i++)
            sink += TakeAndDrop();

        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();

        // Cross the next sweep line, so the collected entries are actually dropped before the read.
        for (int i = 0; i < ManagedPointerTokens.SweepThreshold + 1; i++)
            sink += TakeAndDrop();

        // The crossing's own sweep is skipped while another thread of the host (the runtime's sysmon) is already
        // sweeping, and that sweep can still be walking when the crossing ends (measured on linux: the gate held,
        // the table at 759 and falling, then 261; 1 run in 48). Read after it finishes, as the race row below does.
        WaitForAnySweep();

        int size = ManagedPointerTokens.RegisteredCount;

        AssertCountIsTable("after the crossing");

        Assert.IsTrue(size <= 2 * ManagedPointerTokens.SweepThreshold + 1,
            $"the token table holds {size} entries after {dead} dead registrations and a collection — " +
            $"the sweep is not bounding it (the bound is two thresholds, {2 * ManagedPointerTokens.SweepThreshold + 1})");

        Assert.AreSame(live, ManagedPointerTokens.Resolve(liveToken),
            "the live registration must survive every sweep the churn triggered");

        Assert.AreNotEqual((nuint)0, sink, "the takes must have produced tokens");

        GC.KeepAlive(live);
    }

    // THE SAME BOUND WITH A SECOND REGISTRAR. The row above runs alone; a real process does not. A darwin golib leg
    // read 4,375 against 513 here: 4,096 dead + 257 + the sentinel + 21 registrations the test never made, so another
    // thread was registering. Two things then happen, both measured on linux with this shape. The sweep can run on
    // the OTHER thread and still be walking when this thread reads, so a read has to wait for it (the sweep lock).
    // And the walk's re-sync of the running count fails whenever a registration lands during it, after which
    // RegisteredCount reports the pre-sweep size until the next collection: 4,957 against a table of 259, 4 of 4.
    // So this row reads only after any sweep has finished, and asserts the count IS the table, then the bound.
    // The second registrar on a real host is the runtime's own sysmon thread (see AssertCountIsTable).
    [TestMethod]
    public void TheRunningCountIsTheTableAfterASweepThatRacesAnotherRegistrar()
    {
        const int dead = 4096;

        ref ReferenceBearingShape held = ref heap(new ReferenceBearingShape { Held = new slice<byte>(1) }, out ж<ReferenceBearingShape> live);
        nuint liveToken = (nuint)(uintptr)live;
        nuint sink = 0;

        for (int i = 0; i < dead; i++)
            sink += TakeAndDrop();

        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();

        // Two more registrars run for the whole window, so whichever thread sweeps, another registers during its
        // walk. (With one, the registrar is often the sweeper itself, its walk sees no registration, and the fault
        // stays hidden: 1 red in 15 at the base with one registrar.)
        int registered = 0;
        bool stop = false;
        Thread[] registrars = new Thread[2];

        for (int r = 0; r < registrars.Length; r++)
        {
            registrars[r] = new Thread(() =>
            {
                while (!Volatile.Read(ref stop))
                {
                    TakeAndDrop();
                    Interlocked.Increment(ref registered);
                }
            })
            { IsBackground = true };

            registrars[r].Start();
        }

        while (Volatile.Read(ref registered) == 0)
            Thread.Yield();

        // Cross the sweep line from this thread too; either thread may be the one that sweeps.
        for (int i = 0; i < ManagedPointerTokens.SweepThreshold + 1; i++)
            sink += TakeAndDrop();

        Thread.Sleep(200);
        Volatile.Write(ref stop, true);
        foreach (Thread registrar in registrars)
            registrar.Join();
        WaitForAnySweep();

        AssertCountIsTable("after a sweep that raced another registrar");

        // The registrar's boxes are dead now: one more collection and crossing, alone, bring the table to the bound.
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();

        for (int i = 0; i < ManagedPointerTokens.SweepThreshold + 1; i++)
            sink += TakeAndDrop();

        WaitForAnySweep();
        int size = ManagedPointerTokens.RegisteredCount;

        AssertCountIsTable("after the crossing alone");
        Assert.IsTrue(size <= 2 * ManagedPointerTokens.SweepThreshold + 1,
            $"the token table holds {size} entries; the bound is two thresholds, {2 * ManagedPointerTokens.SweepThreshold + 1}");
        Assert.AreSame(live, ManagedPointerTokens.Resolve(liveToken), "the live registration must survive every sweep");
        Assert.AreNotEqual((nuint)0, sink, "the takes must have produced tokens");

        GC.KeepAlive(live);
    }

    // The registry keeps its table and its sweep lock private; the row reads them by reflection so it also runs
    // against a registry that predates it.
    private static int TableCount()
    {
        object table = typeof(ManagedPointerTokens).GetField("s_table", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        return (int)table.GetType().GetProperty("Count")!.GetValue(table)!;
    }

    // The running count against the table, read as a pair. The host is never quiet: the runtime's sysmon thread
    // registers a pinned box on every GOMAXPROCS check (getCPUCount's sched_getaffinity buffer), so the two reads can
    // straddle a few of its registrations, and one of them can sit between its table store and its increment. The
    // fault this guards left thousands between the two (4,957 against 259); a handful is the host, not the count.
    private const int InFlight = 8;

    private static void AssertCountIsTable(string when)
    {
        int table = TableCount();
        int count = ManagedPointerTokens.RegisteredCount;

        Assert.IsTrue(Math.Abs(table - count) <= InFlight,
            $"{when}, the running count must be the table's size: count {count}, table {table} (allowed in flight {InFlight})");
    }

    private static void WaitForAnySweep()
    {
        object gate = typeof(ManagedPointerTokens).GetField("s_sweepLock", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        lock (gate) { }
    }
}
