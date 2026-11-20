// ThreadCpuTimeTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
        using ManualResetEventSlim tokenTaken = new(false), go = new(false), finished = new(false), release = new(false);
        long token = 0;

        Thread thread = new(() =>
        {
            token = ThreadCpuTime.CurrentThreadToken();
            tokenTaken.Set();
            go.Wait();
            work();
            finished.Set();

            // Alive until the second read: on linux and macOS an exited thread's CPU clock reads as a failure, so a
            // thread that exits first fails "the second read". `go` cannot hold it here, being set already.
            release.Wait();
        });

        thread.Start();

        try
        {
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

            Console.WriteLine($"cpu {(after - before).TotalMilliseconds:F2} ms over {wall.Elapsed.TotalMilliseconds:F2} ms of wall");
            return (after - before, wall.Elapsed);
        }
        finally
        {
            // Released on every path, so a failed assertion never leaves the worker waiting on a disposed event.
            go.Set();
            release.Set();
            thread.Join();
        }
    }

    // The spinner's work is a fixed count of chunks, each a fixed run of a serial xorshift chain: every step depends on
    // the one before and the result is published, so the JIT cannot shorten it. Optimized from the first call, so the
    // first chunk runs the same code as the last.
    private const int SpinChunks = 2000;
    private const int SpinChunkSteps = 1 << 16;
    private static ulong s_spinSink;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static ulong SpinChunk(ulong state)
    {
        for (int i = 0; i < SpinChunkSteps; i++)
        {
            state ^= state << 13;
            state ^= state >> 7;
            state ^= state << 17;
        }

        return state;
    }

    [TestMethod]
    public void ASpinningThreadsCpuTimeAdvancesWithItsWallTime()
    {
        // The CPU reading is bounded by the spinner's own work, not by a wall window: on a shared core (another
        // process on it, a VM whose host is busy) a spinner is runnable for its whole wall time and runs for only
        // part of it, so a bound against wall time measures the machine's load, not the clock.
        long fastestChunk = long.MaxValue;
        ulong state = 0x9E3779B97F4A7C15UL;

        (TimeSpan cpu, TimeSpan wall) = Measure(() =>
        {
            for (int chunk = 0; chunk < SpinChunks; chunk++)
            {
                long start = Stopwatch.GetTimestamp();
                state = SpinChunk(state);
                long elapsed = Stopwatch.GetTimestamp() - start;

                if (elapsed < fastestChunk)
                    fastestChunk = elapsed;
            }

            Volatile.Write(ref s_spinSink, state);
        });

        // Preemption, a lost VM slice or a slower core only lengthen a chunk's wall time, so the fastest chunk is
        // one that ran without interruption, on the fastest core the thread saw. Every chunk does the same work,
        // so the whole run cost the thread at least SpinChunks of those.
        TimeSpan fastest = TimeSpan.FromSeconds((double)fastestChunk / Stopwatch.Frequency);
        TimeSpan work = fastest * SpinChunks;
        Console.WriteLine($"work {work.TotalMilliseconds:F2} ms ({SpinChunks} chunks, fastest {fastest.TotalMilliseconds:F4} ms)");

        Assert.IsTrue(cpu >= work * 0.8 - Tick, $"a spinner used {cpu.TotalMilliseconds:F1} ms of CPU for {work.TotalMilliseconds:F1} ms of work, in {wall.TotalMilliseconds:F1} ms");

        // One thread cannot use more CPU time than the wall time it ran in.
        Assert.IsTrue(cpu <= wall + Tick, $"a spinner used {cpu.TotalMilliseconds:F1} ms of CPU in {wall.TotalMilliseconds:F1} ms");
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
