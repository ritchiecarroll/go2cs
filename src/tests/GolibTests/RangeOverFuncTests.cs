// RangeOverFuncTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

/// <summary>
/// Guards golib's range-over-func adapter (<c>range(seq)</c>, the <c>foreach</c> a Go
/// <c>for v := range seq</c> converts to): Go runs <c>seq</c> as a call on the ranging goroutine, so
/// what happens inside it -- a panic, a <c>runtime.Goexit</c>, the unwinding after an early
/// <c>break</c> -- belongs to the ranging side.
/// </summary>
/// <remarks>
/// The adapter drives <c>seq</c> on a <see cref="Coro"/>. Its predecessor ran <c>seq</c> on a
/// thread-pool task, where a panic or Goexit faulted the task unobserved and the loop silently ended,
/// and an early break left <c>seq</c> unwinding on its own time. internal/synctest's TestIteratorPush
/// is the bubble-side proof (its <c>seq</c> reads the bubble's clock); these are the rest.
/// </remarks>
[TestClass]
public class RangeOverFuncTests
{
    [TestMethod]
    public void PanicInSequence_ReachesTheRangingSide()
    {
        List<int> seen = [];
        Action<Func<int, bool>> seq = yield =>
        {
            yield(1);
            throw new PanicException("seq panicked");
        };

        PanicException? caught = null;

        try
        {
            foreach (int v in range(seq))
                seen.Add(v);
        }
        catch (PanicException ex)
        {
            caught = ex;
        }

        Assert.IsNotNull(caught, "a panic in seq must reach the ranging side, not end the loop silently");
        Assert.AreEqual("seq panicked", caught.State);
        CollectionAssert.AreEqual(new[] { 1 }, seen);
    }

    [TestMethod]
    public void GoexitInSequence_EndsTheRangingGoroutine()
    {
        Action<Func<int, bool>> seq = yield =>
        {
            yield(1);
            throw new GoexitException();
        };

        bool afterLoop = false, deferRan = false;
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                foreach (int _ in range(seq))
                {
                }

                afterLoop = true;
            }
            finally
            {
                deferRan = true;
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(10_000), "the ranging goroutine never finished");
        Assert.IsTrue(deferRan, "the ranging goroutine's defers run on a Goexit");
        Assert.IsFalse(afterLoop, "a Goexit in seq ends the RANGING goroutine: nothing after the loop runs");
    }

    [TestMethod]
    public void EarlyBreak_SequenceHasUnwoundWhenTheLoopStatementEnds()
    {
        // Repeated, because the predecessor's failure here was a RACE: seq finished unwinding on a pool
        // thread at some point after the loop, so a single iteration could pass by luck.
        for (int trial = 0; trial < 200; trial++)
        {
            bool unwound = false;
            Action<Func<int, bool>> seq = yield =>
            {
                try
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (!yield(i))
                            return;
                    }
                }
                finally
                {
                    unwound = true;
                }
            };

            foreach (int v in range(seq))
            {
                if (v == 1)
                    break;
            }

            Assert.IsTrue(unwound, $"trial {trial}: after a break, seq must have unwound (its defers run) before the loop statement completes");
        }
    }

    [TestMethod]
    public void SequenceThatKeepsYieldingAfterFalse_GetsGosPanic()
    {
        // A misbehaving seq that ignores yield's false.
        Action<Func<int, bool>> seq = yield =>
        {
            yield(0);
            yield(1);
        };

        PanicException? caught = null;

        try
        {
            foreach (int _ in range(seq))
                break;
        }
        catch (PanicException ex)
        {
            caught = ex;
        }

        Assert.IsNotNull(caught, "Go panics when seq calls yield after the loop body returned false");
        StringAssert.Contains(caught.Message, "range function continued iteration after function for loop body returned false");
    }

    [TestMethod]
    public void ManyEarlyExitLoops_LeaveNoGoroutineOrThreadBehind()
    {
        Action<Func<int, bool>> seq = yield =>
        {
            for (int i = 0; i < 10; i++)
            {
                if (!yield(i))
                    return;
            }
        };

        int goroutines = Goroutine.Count;
        int threads = Process.GetCurrentProcess().Threads.Count;

        for (int loop = 0; loop < 100; loop++)
        {
            foreach (int v in range(seq))
            {
                if (v == 2)
                    break;
            }
        }

        // The coro retires its goroutine identity BEFORE it releases the ranging side, so the count is
        // exact the moment the loop ends. An OS thread's exit can lag its last managed instruction, so
        // the thread count is polled.
        Assert.AreEqual(goroutines, Goroutine.Count, "every loop's sequence goroutine is retired when the loop ends");

        Stopwatch clock = Stopwatch.StartNew();
        int now;

        while ((now = Process.GetCurrentProcess().Threads.Count) > threads + 4 && clock.ElapsedMilliseconds < 5_000)
            Thread.Sleep(50);

        Assert.IsTrue(now <= threads + 4, $"100 finished loops left threads behind: {threads} before, {now} after");
    }
}
