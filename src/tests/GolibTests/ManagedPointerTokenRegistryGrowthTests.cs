// ManagedPointerTokenRegistryGrowthTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
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

        int size = ManagedPointerTokens.RegisteredCount;

        Assert.IsTrue(size <= 2 * ManagedPointerTokens.SweepThreshold + 1,
            $"the token table holds {size} entries after {dead} dead registrations and a collection — " +
            $"the sweep is not bounding it (the bound is two thresholds, {2 * ManagedPointerTokens.SweepThreshold + 1})");

        Assert.AreSame(live, ManagedPointerTokens.Resolve(liveToken),
            "the live registration must survive every sweep the churn triggered");

        Assert.AreNotEqual((nuint)0, sink, "the takes must have produced tokens");

        GC.KeepAlive(live);
    }
}
