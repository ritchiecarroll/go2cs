// ThreadCpuTimeTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

// The CPU-time seam a returning main reads (golib ThreadCpuTime, MainExit's budget): live, on this OS. A thread names
// itself by a token, another thread reads its CPU time through it. On windows the times advance at the clock tick
// (15.6 ms by default), so a sleeper may be charged up to one tick and a spinner's reading is coarse by one tick.
[TestClass]
public class ThreadCpuTimeTests
{
    private static readonly TimeSpan Tick = TimeSpan.FromMilliseconds(OperatingSystem.IsWindows() ? 15.7 : 1);

    // Runs `work` on a fresh thread that hands out its token first, and reads that thread's CPU time across `work`.
    private static (TimeSpan Cpu, TimeSpan Wall) Measure(Action work)
    {
        using ManualResetEventSlim tokenTaken = new(false), go = new(false), finished = new(false);
        long token = 0;

        Thread thread = new(() =>
        {
            token = ThreadCpuTime.CurrentThreadToken();
            tokenTaken.Set();
            go.Wait();
            work();
            finished.Set();
            go.Wait(); // stays alive until read
        });

        thread.Start();
        Assert.IsTrue(tokenTaken.Wait(5000), "the thread did not start");
        Assert.AreNotEqual(0L, token, "this OS has a seam, so the token is not 0");

        using ThreadCpuReader? reader = ThreadCpuTime.Open(token);
        Assert.IsNotNull(reader, "the reader opens");
        Assert.IsTrue(reader.TryRead(out TimeSpan before), "the first read");

        Stopwatch wall = Stopwatch.StartNew();
        go.Set();
        Assert.IsTrue(finished.Wait(10_000), "the work did not finish");
        wall.Stop();

        Assert.IsTrue(reader.TryRead(out TimeSpan after), "the second read");
        thread.Join();

        Console.WriteLine($"cpu {(after - before).TotalMilliseconds:F2} ms over {wall.Elapsed.TotalMilliseconds:F2} ms of wall");
        return (after - before, wall.Elapsed);
    }

    [TestMethod]
    public void ASpinningThreadsCpuTimeAdvancesWithItsWallTime()
    {
        (TimeSpan cpu, TimeSpan wall) = Measure(() =>
        {
            Stopwatch spin = Stopwatch.StartNew();

            while (spin.ElapsedMilliseconds < 200)
                Thread.SpinWait(64);
        });

        Assert.IsTrue(cpu >= wall * 0.8 - Tick, $"a spinner used {cpu.TotalMilliseconds:F1} ms of CPU in {wall.TotalMilliseconds:F1} ms");
    }

    [TestMethod]
    public void ASleepingThreadsCpuTimeStaysFlat()
    {
        (TimeSpan cpu, _) = Measure(() => Thread.Sleep(200));

        Assert.IsTrue(cpu <= Tick, $"a sleeper used {cpu.TotalMilliseconds:F2} ms of CPU");
    }

    [TestMethod]
    public void AThreadThatExitedReadsAsASeamFailure()
    {
        // Between Count() and the sample the counted thread may exit; then nothing reads, and MainExit keeps its
        // wall-clock fallback (MainExitPanicDefersTests.WithoutACpuReadingTheWaitFallsBackToItsWallClockBound).
        long token = 0;
        Thread thread = new(() => token = ThreadCpuTime.CurrentThreadToken());
        thread.Start();
        thread.Join();

        Assert.AreNotEqual(0L, token);

        // Let the OS retire the thread before the read.
        Thread.Sleep(50);

        using ThreadCpuReader? reader = ThreadCpuTime.Open(token);
        Assert.IsFalse(reader is not null && reader.TryRead(out _), "an exited thread's CPU time read as live");
    }

    [TestMethod]
    public void TheCallingThreadsTokenIsStable()
    {
        long first = ThreadCpuTime.CurrentThreadToken();
        Assert.AreEqual(first, ThreadCpuTime.CurrentThreadToken());
    }
}
