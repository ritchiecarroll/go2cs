// GoroutinePoolTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

/// <summary>
/// Guards goroutines starting on reused threads (<see cref="GoroutineThreadPool"/>), not one new thread each.
/// </summary>
/// <remarks>
/// <para>
/// Creating a thread costs ~100 µs and does not get faster with more creators, so a program that starts
/// millions of goroutines spent its life creating threads: runtime's TestChanSendBarrier (10M goroutines)
/// did not finish inside an hour on a laptop, and TestConcurrentReadsAfterGrowth starved beside it (measured
/// 2026-10-23). These arms hold what reuse must keep: a thread is reused (REUSE), a goroutine that exits
/// locked to its thread takes the thread with it as Go's does (LOCKED EXIT), a reused thread has a
/// goroutine's stack (STACK), carries nothing of its previous goroutine (STATE; the census of what it could
/// carry is ThreadStateCensusTests), and runs each goroutine under its own creator's ExecutionContext
/// (CONTEXT).
/// </para>
/// <para>
/// Each arm detects a reused thread with a thread-static of its own, which golib's reset never touches
/// because nothing registers it: a goroutine that finds it set is running on a thread an earlier goroutine
/// used.
/// </para>
/// </remarks>
[TestClass]
public class GoroutinePoolTests
{
    [ThreadStatic]
    private static object? t_marker;

    // ---- REUSE ----

    [TestMethod]
    public void SequentialGoroutines_RunOnReusedThreads()
    {
        const int count = 1_000;
        int fresh = 0;

        for (int i = 0; i < count; i++)
        {
            RunGoroutine(() =>
            {
                if (t_marker is null)
                {
                    t_marker = new object();
                    Interlocked.Increment(ref fresh);
                }
            });
        }

        // One goroutine at a time, each waited for: a reusing executor needs a few dozen threads at most. A
        // goroutine signals done before its worker is back in the idle set, so a quick next start sometimes
        // finds none and creates one (measured: up to 17 of 1,000); those join the idle set, which is capped
        // at GoroutineThreadPool.MaxIdle. A thread per goroutine needs all thousand.
        Assert.IsTrue(fresh <= count / 10, $"{count} sequential goroutines started on {fresh} fresh threads; a reused thread was expected");
    }

    // ---- LOCKED EXIT ----

    [TestMethod]
    public void GoroutineExitingLocked_TakesItsThreadWithIt()
    {
        // Control: an unlocked goroutine's thread stays alive, idle, for the next one.
        Thread unlocked = ThreadOfGoroutine(lockThread: false);
        Assert.IsFalse(WaitForExit(unlocked, TimeSpan.FromSeconds(2)), "control: an unlocked goroutine's thread must stay alive for reuse");

        // Go: "If the calling goroutine exits without unlocking the thread, the thread will be terminated."
        Thread locked = ThreadOfGoroutine(lockThread: true);
        Assert.IsTrue(WaitForExit(locked, TimeSpan.FromSeconds(5)), "a goroutine that exited locked to its thread left the thread alive for reuse");
    }

    private static Thread ThreadOfGoroutine(bool lockThread)
    {
        Thread? thread = null;

        RunGoroutine(() =>
        {
            thread = Thread.CurrentThread;

            if (lockThread)
                go.runtime_package.LockOSThread();
        });

        return thread!;
    }

    private static bool WaitForExit(Thread thread, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow + timeout;

        while (thread.IsAlive && DateTime.UtcNow < deadline)
            Thread.Sleep(10);

        return !thread.IsAlive;
    }

    // ---- STACK ----

    // Each frame holds this much of the stack, so the recursion reaches past any default thread's ~1 MB
    // quickly and stays well inside a goroutine's 256 MiB reservation.
    private const int FrameBytes = 16 * 1024;
    private const int DeepBytes = 64 * 1024 * 1024;

    [TestMethod]
    public void ReusedThread_HasAGoroutineStack()
    {
        // Warm a worker, then recurse on it: the stack under test is a REUSED thread's.
        RunGoroutine(static () => t_marker ??= new object());
        WaitForIdleWorker();

        bool reused = false;
        int reached = 0;

        RunGoroutine(() =>
        {
            reused = t_marker is not null;
            reached = Recurse(DeepBytes / FrameBytes);
        });

        if (!reused)
            Assert.Inconclusive("the second goroutine did not land on a reused thread; nothing to assert");

        Assert.AreEqual(DeepBytes / FrameBytes, reached,
            $"a reused goroutine thread ran out of stack after {reached * (long)FrameBytes / 1024} KiB of recursion; a goroutine's thread reserves {Goroutine.StackReserve / (1024 * 1024)} MiB");
    }

    // Returns how many levels it reached before the runtime reported the stack near its end. Checked before
    // each frame is spent, so an undersized stack fails this test instead of the process.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Recurse(int levels)
    {
        if (levels == 0 || !RuntimeHelpers.TryEnsureSufficientExecutionStack())
            return 0;

        // Touch both ends of the frame, so its whole size is really on the stack.
        Span<byte> frame = stackalloc byte[FrameBytes];
        frame[0] = 1;
        frame[^1] = 1;

        return frame[0] + frame[^1] - 1 + Recurse(levels - 1);
    }

    // ---- STATE ----

    [TestMethod]
    public void ReusedThread_CarriesNoThreadStateFromItsPreviousGoroutine()
    {
        // The census arm's own reuse scenario, driven by plain goroutines instead of coros.
        ThreadStateCensusTests.RunReuseScenario(static work => RunGoroutine(work));
    }

    // ---- CONTEXT ----

    private static readonly AsyncLocal<string?> s_flowed = new();

    [TestMethod]
    public void EachGoroutine_RunsUnderItsOwnCreatorsContext()
    {
        string? seenByFirst = null, seenBySecond = null;
        bool reused = false;

        s_flowed.Value = "creator-1";

        RunGoroutine(() =>
        {
            t_marker ??= new object();
            seenByFirst = s_flowed.Value;

            // Set INSIDE the goroutine: it must stay this goroutine's, never the worker's.
            s_flowed.Value = "goroutine-1";
        });

        WaitForIdleWorker();
        s_flowed.Value = "creator-2";

        RunGoroutine(() =>
        {
            reused = t_marker is not null;
            seenBySecond = s_flowed.Value;
        });

        s_flowed.Value = null;

        Assert.AreEqual("creator-1", seenByFirst, "a goroutine must see the AsyncLocals of the code that started it");

        if (!reused)
            Assert.Inconclusive("the second goroutine did not land on a reused thread; nothing to assert");

        Assert.AreEqual("creator-2", seenBySecond, "a goroutine on a reused thread saw the previous goroutine's AsyncLocal");
    }

    // ---- helpers ----

    // Starts body as a goroutine and waits for it; a failure inside is rethrown here, since a failure on a
    // worker would reach golib's process-killing backstop.
    internal static void RunGoroutine(Action body)
    {
        Exception? error = null;
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                body();
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(30_000), "the goroutine never finished");

        if (error is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
    }

    // A goroutine signals done before its worker is back in the idle set; give the worker that moment, so
    // the next start finds it.
    private static void WaitForIdleWorker()
    {
        int before = GoroutineThreadPool.IdleCount;

        for (int spin = 0; spin < 200 && GoroutineThreadPool.IdleCount <= before; spin++)
            Thread.Sleep(5);
    }
}
