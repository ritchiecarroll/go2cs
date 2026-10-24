// MainExitPanicDefersTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;

namespace GolibTests;

// Go's runningPanicDefers, as golib.MainExit keeps it: a returning main waits while a goroutine runs a panic's
// deferred calls, so that goroutine's report wins (the `defer wg.Done(); panic(v)` race, whose end-to-end arm is
// the GoroutinePanicAfterDeferredDone behavioral test: exit 2 and Go's stderr). These arms pin the count itself:
// a goroutine counts while a panic runs its deferred calls and stops counting the moment none is in flight, so a
// main that returns after a RECOVERED panic, a Goexit or a swallowed panic never waits. MSTest runs this assembly
// serially (no [Parallelize]), which is what makes reading the process-wide count safe here.
[TestClass]
public class MainExitPanicDefersTests
{
    private static readonly TimeSpan Settle = TimeSpan.FromSeconds(5);

    // Runs body as a goroutine and returns once the goroutine has ENDED (the count returns to its baseline then, or
    // the arm fails on the timeout).
    private static void RunGoroutine(Action body, int baseline)
    {
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                body();
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(Settle), "the goroutine did not finish");
        Assert.IsTrue(SpinWait.SpinUntil(() => MainExit.RunningPanicDefers == baseline, Settle),
            $"the goroutine ended, but the count reads {MainExit.RunningPanicDefers}, not its baseline {baseline}");
    }

    // A frame that panics with `value` and runs `deferred` (registered first, so it runs last) and `inner` (run
    // first) as its deferred calls: `defer deferred(); defer inner(); panic(value)`.
    private static void PanickingFrame(object value, Action deferred, Action inner)
    {
        GoFrame frame = default;

        try
        {
            frame.Push(deferred);
            frame.Push(inner);

            throw panic(value);
        }
        catch (Exception ex) when (GoFrame.IsPanic(ex, out PanicException? p))
        {
            GoFrame.Capture(p);
        }
        finally
        {
            frame.Run();
        }
    }

    [TestMethod]
    public void AGoroutineCountsWhileAPanicRunsItsDeferredCalls()
    {
        int baseline = MainExit.RunningPanicDefers;
        int during = -1, afterRecovery = -1;
        object? recovered = null;

        RunGoroutine(() =>
        {
            PanickingFrame("A", () => recovered = recover(), () => during = MainExit.RunningPanicDefers);
            afterRecovery = MainExit.RunningPanicDefers;
        }, baseline);

        Assert.AreEqual("A", recovered?.ToString());
        Assert.AreEqual(baseline + 1, during, "a deferred call run by the panic sees its goroutine counted");
        Assert.AreEqual(baseline, afterRecovery, "the frame that recovered returned: nothing is in flight, so the count is back");
    }

    [TestMethod]
    public void ANestedRecoveryDoesNotEndTheOuterPanicsCount()
    {
        int baseline = MainExit.RunningPanicDefers;
        int afterInner = -1;

        RunGoroutine(() =>
        {
            // The outer panic A runs its deferred calls; one of them panics B and recovers it in a frame of its own.
            // A is still unrecovered there, so the goroutine stays counted until A is recovered.
            PanickingFrame("A", () => recover(), () =>
            {
                PanickingFrame("B", () => recover(), () => { });
                afterInner = MainExit.RunningPanicDefers;
            });
        }, baseline);

        Assert.AreEqual(baseline + 1, afterInner, "B's recovery returns into A's deferred calls: A is still running them");
    }

    [TestMethod]
    public void AGoexitDuringAPanicEndsTheCountWithTheGoroutine()
    {
        int baseline = MainExit.RunningPanicDefers;
        bool ran = false;

        // go func() { defer recover(); defer runtime.Goexit(); panic("A") }(): A is abandoned, the goroutine exits.
        RunGoroutine(() => PanickingFrame("A", () => ran = true, () => throw new GoexitException()), baseline);

        Assert.IsTrue(ran, "the Goexit still ran the remaining deferred call");
    }

    [TestMethod]
    public void APanicACSharpCatchStoppedEndsTheCountWithTheGoroutine()
    {
        int baseline = MainExit.RunningPanicDefers;
        int afterCatch = -1;

        // Hand-owned code can stop a panic with a C# catch and no recover(): nothing tells the count, so the
        // goroutine's end must.
        RunGoroutine(() =>
        {
            try
            {
                PanickingFrame("A", () => { }, () => { });
            }
            catch (PanicException)
            {
                afterCatch = MainExit.RunningPanicDefers;
            }
        }, baseline);

        Assert.AreEqual(baseline + 1, afterCatch, "the catch told nothing: still counted until the goroutine ends");
    }

    [TestMethod]
    public void TheMainGoroutineIsNeverCounted()
    {
        int baseline = MainExit.RunningPanicDefers;
        int during = -1;

        // The test thread is not a golib goroutine: its own panic cannot race its own return.
        PanickingFrame("A", () => recover(), () => during = MainExit.RunningPanicDefers);

        Assert.AreEqual(baseline, during);
    }

    [TestMethod]
    public void AMainReturnDoesNotWaitLongOnADeferredCallThatBlocks()
    {
        // The wait stops at once when a report has begun, so no earlier test's fatal start may be left set (the full
        // run failed this arm in 0.1 ms before GoroutineRootPanicTests forgot theirs).
        MainExit.ResetFatalStartForTest();

        int baseline = MainExit.RunningPanicDefers;
        using ManualResetEventSlim inDeferred = new(false), release = new(false);

        // The panicking goroutine blocks in a deferred call, as C1's "blocked" probe does (`select {}`). Go's
        // runtime.main yields briefly and exits 0. The wait here must end at its short bound, not at a second.
        Goroutine.Start(() => PanickingFrame("A", () => recover(), () =>
        {
            inDeferred.Set();
            release.Wait();
        }));

        try
        {
            Assert.IsTrue(inDeferred.Wait(Settle), "the goroutine did not reach its deferred call");
            Assert.AreEqual(baseline + 1, MainExit.RunningPanicDefers, "the blocked goroutine is still counted");

            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            MainExit.AwaitPanickingGoroutines();
            watch.Stop();

            Assert.IsTrue(watch.Elapsed >= MainExit.PanicDefersWait, $"returned after {watch.Elapsed.TotalMilliseconds:F1} ms, before its bound");
            Assert.IsTrue(watch.Elapsed < TimeSpan.FromMilliseconds(200), $"waited {watch.Elapsed.TotalMilliseconds:F1} ms for a blocked deferred call");
        }
        finally
        {
            release.Set();
        }

        Assert.IsTrue(SpinWait.SpinUntil(() => MainExit.RunningPanicDefers == baseline, Settle), "the released goroutine recovered and ended");
    }
}
