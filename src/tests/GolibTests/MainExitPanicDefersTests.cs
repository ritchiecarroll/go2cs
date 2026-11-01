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

    // ---- the main-return wait: Go's "can it run?" (MainExit's remarks) ----

    // Starts a goroutine whose panic runs `blocking` as a deferred call, waits until it is inside it, times one
    // main-return wait, and releases it. The wait stops at once when a report has begun, so no earlier test's fatal
    // start may be left set (the full run failed an arm in 0.1 ms before GoroutineRootPanicTests forgot theirs).
    private static TimeSpan TimeMainReturnWait(Action<ManualResetEventSlim> blocking, Action release)
    {
        MainExit.ResetFatalStartForTest();

        int baseline = MainExit.RunningPanicDefers;
        using ManualResetEventSlim inDeferred = new(false);

        Goroutine.Start(() => PanickingFrame("A", () => recover(), () => blocking(inDeferred)));

        try
        {
            Assert.IsTrue(inDeferred.Wait(Settle), "the goroutine did not reach its deferred call");
            Assert.AreEqual(baseline + 1, MainExit.RunningPanicDefers, "the goroutine in its deferred call is counted");

            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            MainExit.AwaitPanickingGoroutines();
            watch.Stop();

            Console.WriteLine($"main-return wait: {watch.Elapsed.TotalMilliseconds:F2} ms");
            return watch.Elapsed;
        }
        finally
        {
            release();
            Assert.IsTrue(SpinWait.SpinUntil(() => MainExit.RunningPanicDefers == baseline, Settle), "the released goroutine recovered and ended");
        }
    }

    [TestMethod]
    public void AMainReturnIsReleasedByADeferredCallParkedOnAChannel()
    {
        // Go: the goroutine is parked ("chan receive"), the yield loop has nothing to run it with, and main exits 0 at
        // once. Here the park lasts BlockedConfirm (1 ms) and releases main.
        channel<int> ch = new(0);
        TimeSpan waited = TimeMainReturnWait(inDeferred => { inDeferred.Set(); ch.Receive(); }, () => ch.Send(1));

        Assert.IsTrue(waited < TimeSpan.FromMilliseconds(100), $"waited {waited.TotalMilliseconds:F1} ms for a parked goroutine");
    }

    [TestMethod]
    public void AMainReturnIsReleasedByADeferredCallInASystemCall()
    {
        // Go: _Gsyscall, not runnable; the loop runs out. The block is marked as the syscall trampolines mark one.
        using ManualResetEventSlim release = new(false);
        TimeSpan waited = TimeMainReturnWait(inDeferred =>
        {
            Goroutine.EnterBlocked();

            try
            {
                inDeferred.Set();
                release.Wait();
            }
            finally
            {
                Goroutine.ExitBlocked();
            }
        }, release.Set);

        Assert.IsTrue(waited < TimeSpan.FromMilliseconds(100), $"waited {waited.TotalMilliseconds:F1} ms for a goroutine in a system call");
    }

    [TestMethod]
    public void AnUnmarkedBlockHoldsMainToTheHangGuard()
    {
        // The RED CONTROL: a raw C# wait golib does not mark reads as a goroutine that can run and is given no CPU, so
        // only the hang guard ends the wait (scaled down here). This is the shape the census found nowhere in the
        // converted population (docs/phase4/data/main-return-wait).
        TimeSpan guard = MainExit.HangGuard;
        MainExit.HangGuard = TimeSpan.FromMilliseconds(300);

        try
        {
            using ManualResetEventSlim release = new(false);
            TimeSpan waited = TimeMainReturnWait(inDeferred => { inDeferred.Set(); release.Wait(); }, release.Set);

            Assert.IsTrue(waited >= TimeSpan.FromMilliseconds(300), $"returned after {waited.TotalMilliseconds:F1} ms, before the guard");
            Assert.IsTrue(waited < TimeSpan.FromSeconds(3), $"waited {waited.TotalMilliseconds:F1} ms, far past the guard");
        }
        finally
        {
            MainExit.HangGuard = guard;
        }
    }

    [TestMethod]
    public void ABusyDeferredCallIsReleasedByItsCpuBudgetNotTheGuard()
    {
        // A deferred call running user code: it can run, so main waits while it uses its CPU budget (the role of Go's
        // 1000 turns), then stops. Never the guard, which is raised here so reaching it would fail the arm.
        TimeSpan guard = MainExit.HangGuard;
        MainExit.HangGuard = TimeSpan.FromSeconds(30);
        int stop = 0;

        try
        {
            TimeSpan waited = TimeMainReturnWait(inDeferred =>
            {
                inDeferred.Set();

                while (Volatile.Read(ref stop) == 0)
                    Thread.SpinWait(64);
            }, () => Volatile.Write(ref stop, 1));

            Assert.IsTrue(waited >= TimeSpan.FromMilliseconds(5), $"released after {waited.TotalMilliseconds:F1} ms, before any budget was used");
            Assert.IsTrue(waited < TimeSpan.FromSeconds(2), $"waited {waited.TotalMilliseconds:F1} ms for a busy goroutine");
        }
        finally
        {
            MainExit.HangGuard = guard;
        }
    }

    [TestMethod]
    public void RepeatedWaitsLeaveTheProcessHandleCountUnchanged()
    {
        // Each wait opens one reader per counted goroutine (a thread handle on windows) and closes it on every path.
        using ManualResetEventSlim inDeferred = new(false), release = new(false);
        MainExit.ResetFatalStartForTest();
        int baseline = MainExit.RunningPanicDefers;

        Goroutine.Start(() => PanickingFrame("A", () => recover(), () =>
        {
            using (Goroutine.Park(WaitReason.Select))
            {
                inDeferred.Set();
                release.Wait();
            }
        }));

        try
        {
            Assert.IsTrue(inDeferred.Wait(Settle), "the goroutine did not reach its deferred call");

            using System.Diagnostics.Process self = System.Diagnostics.Process.GetCurrentProcess();
            MainExit.AwaitPanickingGoroutines(); // first use: any lazy setup happens before the count is taken
            self.Refresh();
            int before = self.HandleCount;

            for (int i = 0; i < 500; i++)
                MainExit.AwaitPanickingGoroutines();

            self.Refresh();
            int after = self.HandleCount;

            Console.WriteLine($"handles: {before} before 500 waits, {after} after");
            Assert.IsTrue(after - before < 10, $"500 waits left {after - before} more handles open");
        }
        finally
        {
            release.Set();
        }

        Assert.IsTrue(SpinWait.SpinUntil(() => MainExit.RunningPanicDefers == baseline, Settle), "the released goroutine recovered and ended");
    }

    // ---- the decision itself (MainExit.Holds), sample by sample ----

    private static MainExit.PanickerSample Runnable(double cpuMs) => new(false, 0, TimeSpan.FromMilliseconds(cpuMs));

    private static TimeSpan Ms(double ms) => TimeSpan.FromMilliseconds(ms);

    [TestMethod]
    public void ARunnableGoroutineHoldsMainUntilItsCpuBudgetIsUsed()
    {
        MainExit.PanickerWatch watch = default;

        Assert.IsTrue(MainExit.Holds(ref watch, Runnable(100), Ms(0)), "the wait's first look");
        Assert.IsTrue(MainExit.Holds(ref watch, Runnable(119), Ms(500)), "19 ms of CPU: under budget, however long the wall");
        Assert.IsFalse(MainExit.Holds(ref watch, Runnable(120), Ms(501)), "20 ms of CPU: the budget is used");
    }

    [TestMethod]
    public void AStarvedGoroutineHoldsMainForAsLongAsItGetsNoCpu()
    {
        // Behind other threads for a CPU: no CPU used, so Holds never lets go. Only the wait loop's guard does.
        MainExit.PanickerWatch watch = default;

        foreach (double wall in new[] { 0.0, 20, 1000, 9000 })
            Assert.IsTrue(MainExit.Holds(ref watch, Runnable(50), Ms(wall)), $"no CPU used by {wall} ms");
    }

    [TestMethod]
    public void TheSameBlockReleasesMainOnceItHasLastedTheConfirmation()
    {
        MainExit.PanickerWatch watch = default;
        MainExit.PanickerSample blocked = new(true, 7, Ms(10));

        Assert.IsTrue(MainExit.Holds(ref watch, blocked, Ms(0)), "a block first seen");
        Assert.IsTrue(MainExit.Holds(ref watch, blocked, Ms(0.5)), "the same block, half a confirmation later");
        Assert.IsFalse(MainExit.Holds(ref watch, blocked, Ms(1)), "the same block, a confirmation later");
    }

    [TestMethod]
    public void ANewBlockRestartsTheConfirmation()
    {
        // Short blocks in a row (a goroutine trading messages) are progress, not one long block.
        MainExit.PanickerWatch watch = default;

        Assert.IsTrue(MainExit.Holds(ref watch, new(true, 1, Ms(10)), Ms(0)));
        Assert.IsTrue(MainExit.Holds(ref watch, new(true, 2, Ms(10)), Ms(0.9)), "block 2 began: its own clock");
        Assert.IsTrue(MainExit.Holds(ref watch, new(true, 2, Ms(10)), Ms(1.5)), "block 2 has lasted 0.6 ms");
        Assert.IsTrue(MainExit.Holds(ref watch, new(false, 2, Ms(10)), Ms(1.6)), "running again");
        Assert.IsTrue(MainExit.Holds(ref watch, new(true, 3, Ms(10)), Ms(1.7)));
        Assert.IsFalse(MainExit.Holds(ref watch, new(true, 3, Ms(10)), Ms(2.7)), "block 3 lasted the confirmation");
    }

    [TestMethod]
    public void WithoutACpuReadingTheWaitFallsBackToItsWallClockBound()
    {
        // No seam on this OS, or the thread exited between Count() and the sample (ThreadCpuTimeTests' arm): the
        // bound is the wall-clock PanicDefersWait the wait had before.
        MainExit.PanickerWatch watch = default;
        MainExit.PanickerSample unread = new(false, 0, null);

        Assert.IsTrue(MainExit.Holds(ref watch, unread, Ms(0)));
        Assert.IsTrue(MainExit.Holds(ref watch, unread, MainExit.PanicDefersWait - Ms(1)));
        Assert.IsFalse(MainExit.Holds(ref watch, unread, MainExit.PanicDefersWait));
    }
}
