// ManagedPointerTokenRunningCountTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

// THE REGISTRY'S COUNT IS A RUNNING COUNT, NOT ConcurrentDictionary.Count (i9 sizing, ledger 6e59daa04c).
// Every NEW registration used to read `s_table.Count`, which takes EVERY bucket lock, and the lock array
// grows with the table (1,536 locks measured at 10k-120k entries) and never shrinks. So once a process
// had registered a few thousand pointers, each fresh-box ж -> uintptr conversion cost ~14-15 us
// (os.ReadFile makes 7 such registrations: ~93 of its 131-136 us). With a running count the same
// conversion measured 0.19-0.21 us.
//
// Two arms. PERF: a fresh-box conversion after 20k live registrations stays under 5 us -- a 25x margin
// over the measured 0.2 us, and still 2.8x under the ~14 us before. The bound was 1 us (5x) until the hosted
// macOS slow mode read 1217 ns on osx-x64 (run 43064826815, a leg whose raw yields also ran 2.6-3.6 s): an
// absolute wall-clock bar that runs on darwin needs at least 10x its typical, or a paired host reference
// (COORD, 2026-11-11). The method keeps its name (fail sets and history are keyed by it), which predates the
// widened bound. The arm prints a READING line on every run (the best batch and each round, ns per
// conversion), so darwin's own typical is measured; if it proves well above 0.2 us, the bar becomes paired
// (the same batch allocated but not converted, barring the excess). INVARIANT: the count may OVER-count (an overwrite of a
// live address increments it) but must never read BELOW the table between sweeps, because Resolve's
// `s_count == 0` fast path would then skip a live entry; and an overwritten entry still resolves to the
// box that last registered it.
[TestClass]
public class ManagedPointerTokenRunningCountTests
{
    private const int Live = 20_000;
    private const int Batch = 2_000;
    private const double BoundNs = 5_000;

    private static uintptr s_sink;

    [TestMethod]
    public void AFreshBoxConversionStaysUnderAMicrosecondAfterTwentyThousandRegistrations()
    {
        // Grow the table (and with it ConcurrentDictionary's lock array) with LIVE registrations, so no
        // sweep can shrink it underneath the measurement.
        List<ж<long>> held = new(Live);

        for (int i = 0; i < Live; i++)
        {
            ж<long> box = Ꮡ((long)i);
            held.Add(box);
            s_sink = box;
        }

        // Best of five batches: the bound is about the per-registration cost, not a scheduling hiccup.
        double best = double.MaxValue;
        double[] rounds = new double[5];

        for (int round = 0; round < 5; round++)
        {
            Stopwatch clock = Stopwatch.StartNew();

            for (int i = 0; i < Batch; i++)
                s_sink = Ꮡ((long)i);

            clock.Stop();
            rounds[round] = clock.Elapsed.TotalMilliseconds * 1_000_000 / Batch;
            best = Math.Min(best, rounds[round]);
        }

        GC.KeepAlive(held);

        Console.WriteLine($"READING AFreshBoxConversion: best {best:F0} ns per conversion after {Live} live registrations " +
            $"(rounds {string.Join(" ", Array.ConvertAll(rounds, r => r.ToString("F0")))} ns; bound {BoundNs} ns)");

        Assert.IsTrue(best < BoundNs,
            $"a fresh-box ж -> uintptr conversion costs {best:F0} ns after {Live} live registrations (bound {BoundNs} ns; " +
            $"~14,000 is the per-registration ConcurrentDictionary.Count, ~200 the running count)");
    }

    [TestMethod]
    public void AnOverwrittenEntryStillResolvesAndTheCountNeverReadsBelowTheTable()
    {
        // One element address, registered again and again by FRESH boxes: the wrapper shape
        // (`var ᴋ = Ꮡ(buf, 0); (uintptr)ᴋ`), where every registration overwrites a live entry.
        slice<byte> buffer = new(16);
        ж<byte> last = Ꮡ(buffer, 0);
        uintptr address = last;

        int slackBefore = ManagedPointerTokens.RegisteredCount - ManagedPointerTokens.TableCount;
        Assert.IsTrue(slackBefore >= 0, $"the count already reads {-slackBefore} below the table");

        // Enough overwrites to drive a count that fell on each one below any slack it started with.
        int overwrites = slackBefore + 64;

        for (int i = 0; i < overwrites; i++)
        {
            last = Ꮡ(buffer, 0);
            uintptr again = last;
            Assert.AreEqual(address, again, "the same element must answer the same address");
        }

        int slackAfter = ManagedPointerTokens.RegisteredCount - ManagedPointerTokens.TableCount;
        Assert.IsTrue(slackAfter >= 0,
            $"after {overwrites} overwrites the count reads {-slackAfter} BELOW the table: Resolve's empty-table fast path could skip a live entry");

        Assert.AreSame(last, ManagedPointerTokens.Resolve((nuint)address),
            "the overwritten address must resolve to the box that last registered it");

        GC.KeepAlive(buffer);
    }
}
