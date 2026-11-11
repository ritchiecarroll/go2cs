// AtomicByteStoreRaceTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using atomic = go.@internal.runtime.atomic_package;

namespace GolibTests;

/// <summary>
/// internal/runtime/atomic's 8-bit operations are atomic with respect to EACH OTHER on one byte, as Go's are: a
/// Store8 that lands while an And8 or Or8 on the same byte is in flight is never lost.
/// </summary>
/// <remarks>
/// A latched read-and-write-back And8/Or8 beside a lock-free volatile Store8 overwrites a Store8 that lands inside the
/// And8/Or8 window with the byte the RMW had read; atomic_impl.cs serves every 8-bit op lock-free (byte CAS loops and
/// Interlocked.Exchange), so none can be overwritten. One thread here loops Or8(0x01) and And8(0xFE), which never
/// clear the sentinel bit 0x80; this thread loops Store8 of the sentinel and reads it back. A read-back without the
/// sentinel can only be a lost Store8. Bounded by a round count and a wall budget; Inconclusive on one core, where the
/// two threads cannot overlap.
/// </remarks>
[TestClass]
public class AtomicByteStoreRaceTests
{
    private const uint8 Sentinel = 0x80;
    private const int Rounds = 500_000;
    private static readonly TimeSpan s_budget = TimeSpan.FromSeconds(20);

    [TestMethod]
    public void AStore8IsNeverLostToAConcurrentAnd8OrOr8()
    {
        if (Environment.ProcessorCount < 2)
            Assert.Inconclusive("one logical core: the two threads cannot overlap, so a lost store cannot be shown");

        ref uint8 cell = ref heap((uint8)0, out ж<uint8> Ꮡcell);
        _ = cell;

        using CancellationTokenSource stop = new();

        Thread rmw = new(() =>
        {
            while (!stop.IsCancellationRequested)
            {
                // Neither operation clears the sentinel bit: Or8 sets bit 0, And8 clears ONLY bit 0.
                atomic.Or8(Ꮡcell, 0x01);
                atomic.And8(Ꮡcell, 0xFE);
            }
        }) { IsBackground = true, Name = "And8/Or8 loop" };

        int lost = 0;
        int rounds = 0;
        Stopwatch clock = Stopwatch.StartNew();

        rmw.Start();

        try
        {
            for (; rounds < Rounds && clock.Elapsed < s_budget; rounds++)
            {
                atomic.Store8(Ꮡcell, Sentinel);

                // Nothing but this thread's own reset below clears the sentinel, so a read without it is a lost Store8.
                for (int look = 0; look < 4; look++)
                {
                    if ((atomic.Load8(Ꮡcell) & Sentinel) == 0)
                    {
                        lost++;
                        break;
                    }
                }

                atomic.Store8(Ꮡcell, 0);
            }
        }
        finally
        {
            stop.Cancel();
            rmw.Join();
        }

        Assert.AreEqual(0, lost, $"{lost} of {rounds} Store8s of the sentinel were lost to a concurrent And8/Or8 on the same byte ({clock.Elapsed.TotalSeconds:F1} s)");
    }

    /// <summary>
    /// The Xchg8 half of the same contract: an Xchg8 that lands while an And8 or Or8 on the same byte is in flight is
    /// never lost. Green wherever Xchg8 and And8/Or8 share ONE scheme (a common latch, or lock-free throughout), and red
    /// for the mixed shape: a lock-free Xchg8 beside a latched And8/Or8, which the RMW's write-back overwrites.
    /// </summary>
    [TestMethod]
    public void AnXchg8IsNeverLostToAConcurrentAnd8OrOr8()
    {
        if (Environment.ProcessorCount < 2)
            Assert.Inconclusive("one logical core: the two threads cannot overlap, so a lost exchange cannot be shown");

        ref uint8 cell = ref heap((uint8)0, out ж<uint8> Ꮡcell);
        _ = cell;

        using CancellationTokenSource stop = new();

        Thread rmw = new(() =>
        {
            while (!stop.IsCancellationRequested)
            {
                // Neither operation clears the sentinel bit: Or8 sets bit 0, And8 clears ONLY bit 0.
                atomic.Or8(Ꮡcell, 0x01);
                atomic.And8(Ꮡcell, 0xFE);
            }
        }) { IsBackground = true, Name = "And8/Or8 loop" };

        int lost = 0;
        int rounds = 0;
        Stopwatch clock = Stopwatch.StartNew();

        rmw.Start();

        try
        {
            for (; rounds < Rounds && clock.Elapsed < s_budget; rounds++)
            {
                atomic.Xchg8(Ꮡcell, Sentinel);

                // Only this thread clears the sentinel, and it does so HERE: the byte this exchange takes back must still
                // carry the sentinel the previous exchange put there, or that exchange was lost.
                if ((atomic.Xchg8(Ꮡcell, 0) & Sentinel) == 0)
                    lost++;
            }
        }
        finally
        {
            stop.Cancel();
            rmw.Join();
        }

        Assert.AreEqual(0, lost, $"{lost} of {rounds} Xchg8s of the sentinel were lost to a concurrent And8/Or8 on the same byte ({clock.Elapsed.TotalSeconds:F1} s)");
    }
}
