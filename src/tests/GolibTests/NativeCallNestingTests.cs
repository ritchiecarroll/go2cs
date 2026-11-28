// NativeCallNestingTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

#nullable enable
#pragma warning disable CS1998 // the twins here need no await: what is measured is the P accounting around them

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// F1 (G's ruling, 2026-11-27; DESIGN-mn-n5-invisible-blocking.md 3b amendment): a native call entered with Blocks
// hands its P off; a callback from inside it (ReenterManaged, cgocallbackg's exitsyscall) must take a P back before Go
// code runs, or a Blocks call inside the callback hands off a SECOND P and the outer Exit takes none: +1 free P permit
// and +1 s_blocked for good, per occurrence. Each row runs K multiplexed goroutines through the crossing, then reads the
// scheduler's `blocked=` and `p-free=` at QUIESCENCE (a base before, a poll after every goroutine has ended): a leak
// never returns to base. GO2CS_MN is read once per process, so the rows run in CHILDREN, and the parent requires every
// row's line by name.
//
//   nested-blocks-balances  Blocks -> callback -> Blocks, K=8           red before the fix
//   nested-two-deep         Blocks -> callback -> Blocks -> callback -> Blocks, K=8   red before the fix
//   control-plain-blocks    a plain Blocks bracket, K=8                  green both sides
//   control-mayblock-outer  MayBlock -> callback -> Blocks, K=8          green both sides
//   (P=1 child)
//   control-retaken-outer   a MayBlock outer the monitor RETOOK -> callback -> Blocks, K=8; retakes must rise
//                           (G's trace: the inner Enter swaps Retaken 1 -> 0 with no second release)   green both sides
//   liveness-p1             Blocks -> callback -> Blocks, K=64 at one P: the callback's ExitBlocked waits for the one
//                           P, which another goroutine's native call holds 50 ms until the monitor retakes it (3b and
//                           the fix compose; retakes must rise). It completes, and the counts return to base.
//                           counts red before the fix
//   (this process, scheduler off) the same crossings move neither blocked nor p-free nor handoffs
//
// Made to fail: revert the ReenterManaged branch (GoNativeCall.cs) -> the two nested rows and liveness-p1 read
// blocked and p-free raised by K; the controls stay green. ReturnToNative's MayBlock form gets no plant: a Blocks
// ReturnToNative would balance too, so the form is Go's semantics (reentersyscall is entersyscall), not a count.
[TestClass]
public class NativeCallNestingTests
{
    private const string ChildVariable = "GO2CS_TEST_NATIVE_NESTING_CHILD";

    private static readonly string[] s_mnRows = ["nested-blocks-balances", "nested-two-deep", "control-plain-blocks", "control-mayblock-outer"];
    private static readonly string[] s_p1Rows = ["control-retaken-outer", "liveness-p1"];

    // The CHILD. Run on its own (no variable) it does nothing.
    [TestMethod]
    public void ChildNativeNestingRows()
    {
        string? mode = Environment.GetEnvironmentVariable(ChildVariable);

        if (mode is not ("mn" or "p1"))
            return;

        Assert.IsTrue(GoScheduler.Enabled, $"the child runs under the scheduler ({mode})");

        // Start the scheduler and let it settle, so the base is read at quiescence.
        RunConcurrently(Environment.ProcessorCount, () => { });
        (long blocked, long free) baseline = Quiescent();

        if (mode == "mn")
        {
            baseline = Balanced("nested-blocks-balances", baseline, () => RunConcurrently(8, () => Nest(2)));
            baseline = Balanced("nested-two-deep", baseline, () => RunConcurrently(8, () => Nest(3)));
            baseline = Balanced("control-plain-blocks", baseline, () => RunConcurrently(8, () => Nest(1)));
            Balanced("control-mayblock-outer", baseline, () => RunConcurrently(8, () => MayBlockOuter(holdMs: 1)));
        }
        else
        {
            // One P: queued twins make the monitor retake a native call's P (it retakes only while work waits).
            long retakes = Counter(GoScheduler.Reading(), "retakes");
            baseline = Balanced("control-retaken-outer", baseline, () => RunConcurrently(8, () => MayBlockOuter(holdMs: 50)));
            long retaken = Counter(GoScheduler.Reading(), "retakes") - retakes;
            Assert.IsTrue(retaken > 0, $"control-retaken-outer is vacuous: the monitor retook no outer call ({GoScheduler.Reading()})");

            // P3's SHOULDs: the hold after the callback is 50 ms, as control-retaken-outer's. The monitor retakes on its
            // second round in a native call, and at Windows' default timer resolution its Thread.Sleep(2) round is one
            // 15.6 ms tick, so a 20 ms hold was exactly two rounds and the second could land on the holder's own wake;
            // 50 ms is four ticks, a whole tick to spare. The one P the callback took is held until the monitor RETAKES
            // it, and the row asserts that it did: it proves 3b's retake and the fix compose, not luck.
            retakes = Counter(GoScheduler.Reading(), "retakes");
            Balanced("liveness-p1", baseline, () => RunConcurrently(64, () => Nest(2, holdAfterMs: 50)));
            retaken = Counter(GoScheduler.Reading(), "retakes") - retakes;
            Assert.IsTrue(retaken > 0, $"liveness-p1 is vacuous: the monitor retook no held P ({GoScheduler.Reading()})");
        }

        if (s_failures.Count > 0)
            Assert.Fail(string.Join("\n", s_failures));
    }

    // Default mode (P3's NIT): with the scheduler off there is no P to hand off, so the same crossings move neither
    // count. Run in this process, on goroutine threads; under a GO2CS_MN=1 test run it is NOT MEASURED.
    [TestMethod]
    public void WithTheSchedulerOffNestedNativeCallbacksMoveNoCount()
    {
        if (GoScheduler.Enabled)
            Assert.Inconclusive("GO2CS_MN=1 is set for this run: the scheduler-off row is NOT MEASURED");

        string before = GoScheduler.Reading();
        Exception? failure = null;
        Thread[] threads = new Thread[8];

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                try
                {
                    using Goroutine.Scope scope = Goroutine.Enter();
                    Nest(3);
                    MayBlockOuter(holdMs: 1);
                }
                catch (Exception ex)
                {
                    Interlocked.CompareExchange(ref failure, ex, null);
                }
            });

            threads[i].Start();
        }

        foreach (Thread thread in threads)
            Assert.IsTrue(thread.Join(60_000), "a crossing finished");

        if (failure is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();

        string after = GoScheduler.Reading();

        foreach (string name in new[] { "blocked", "p-free", "handoffs" })
        {
            Assert.IsTrue(Counter(before, name) >= 0, $"the reading names {name}: {before}");
            Assert.AreEqual(Counter(before, name), Counter(after, name), $"{name} moved with the scheduler off: {before} -> {after}");
        }
    }

    [TestMethod]
    public void NestedNativeCallbacksBalanceUnderTheScheduler()
    {
        RunChild("mn", s_mnRows);
    }

    [TestMethod]
    public void NestedNativeCallbacksBalanceAndCompleteAtOneP()
    {
        RunChild("p1", s_p1Rows);
    }

    // depth Blocks calls, each but the innermost calling back into Go, which makes the next. After its callback returns,
    // an outer call stays in native code holdAfterMs more: it holds the P its callback took (ReturnToNative is the
    // MayBlock form) until the monitor retakes it.
    private static void Nest(int depth, int holdAfterMs = 0)
    {
        GoNativeCall.Enter(NativeCallHint.Blocks);

        try
        {
            Thread.Sleep(1);

            if (depth > 1)
            {
                GoNativeCall.ReenterManaged();

                try
                {
                    Nest(depth - 1, holdAfterMs);
                }
                finally
                {
                    GoNativeCall.ReturnToNative();
                }

                if (holdAfterMs > 0)
                    Thread.Sleep(holdAfterMs);
            }
        }
        finally
        {
            GoNativeCall.Exit();
        }
    }

    // A MayBlock outer that stays in native code holdMs before it calls back into Go, which makes a Blocks call.
    private static void MayBlockOuter(int holdMs)
    {
        GoNativeCall.Enter(NativeCallHint.MayBlock);

        try
        {
            Thread.Sleep(holdMs);
            GoNativeCall.ReenterManaged();

            try
            {
                Nest(1);
            }
            finally
            {
                GoNativeCall.ReturnToNative();
            }
        }
        finally
        {
            GoNativeCall.Exit();
        }
    }

    // A row: run it, then the counts must come back to the base once every goroutine has ended.
    private static (long blocked, long free) Balanced(string row, (long blocked, long free) baseline, Action run)
    {
        run();

        Stopwatch clock = Stopwatch.StartNew();
        string reading;

        while (true)
        {
            reading = GoScheduler.Reading();

            if (Counter(reading, "blocked") == baseline.blocked && Counter(reading, "p-free") == baseline.free)
                break;

            if (clock.ElapsedMilliseconds > 5_000)
            {
                // Every row runs and names its own verdict, so a plant reads exactly which rows it reddens; the child
                // fails at its end (s_failures). A red row leaves the counts raised, so the next row is read against
                // where this one left them.
                s_failures.Add($"{row}: blocked={Counter(reading, "blocked")} p-free={Counter(reading, "p-free")} 5 s after every goroutine ended; the base was blocked={baseline.blocked} p-free={baseline.free} ({reading})");
                Console.WriteLine($"NATIVE NESTING ROW {row} RED");
                return (Counter(reading, "blocked"), Counter(reading, "p-free"));
            }

            Thread.Sleep(20);
        }

        Console.WriteLine($"NATIVE NESTING ROW {row} ok");
        return baseline;
    }

    private static readonly List<string> s_failures = [];

    // The counts once they hold still: two readings 100 ms apart that agree.
    private static (long blocked, long free) Quiescent()
    {
        (long, long) last = (-1, -1);

        for (int i = 0; i < 50; i++)
        {
            string reading = GoScheduler.Reading();
            (long, long) now = (Counter(reading, "blocked"), Counter(reading, "p-free"));
            Assert.IsTrue(now.Item1 >= 0 && now.Item2 >= 0, $"the reading carries blocked= and p-free=: {reading}");

            if (now == last)
                return now;

            last = now;
            Thread.Sleep(100);
        }

        Assert.Fail($"the scheduler's counts never held still: {GoScheduler.Reading()}");
        return default;
    }

    // Each of k goroutines, on its own thread, leaves it for a carrier (RunTwin) and runs body there, multiplexed.
    private static void RunConcurrently(int k, Action body)
    {
        Exception? failure = null;
        int ran = 0;
        Thread[] threads = new Thread[k];

        for (int i = 0; i < k; i++)
        {
            threads[i] = new Thread(() =>
            {
                try
                {
                    using Goroutine.Scope scope = Goroutine.Enter();

                    Goroutine.RunTwin(() => throw new InvalidOperationException("the plain path ran under the scheduler"), async () =>
                    {
                        Assert.IsTrue(Goroutine.Current is { IsMultiplexed: true }, "the body runs multiplexed");
                        body();
                        Interlocked.Increment(ref ran);
                    });
                }
                catch (Exception ex)
                {
                    Interlocked.CompareExchange(ref failure, ex, null);
                }
            });

            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            if (!thread.Join(60_000))
                Assert.Fail($"a goroutine did not finish in 60 s ({GoScheduler.Reading()})");
        }

        if (failure is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();

        Assert.AreEqual(k, ran, "every goroutine ran its body");
    }

    private static long Counter(string reading, string name)
    {
        Match match = Regex.Match(reading, $@"(?:^|\s){Regex.Escape(name)}=(?<value>-?\d+)");
        return match.Success ? long.Parse(match.Groups["value"].Value) : -1;
    }

    private static void RunChild(string mode, IReadOnlyList<string> rows)
    {
        string filter = $"FullyQualifiedName={typeof(NativeCallNestingTests).FullName}.{nameof(ChildNativeNestingRows)}";

        ProcessStartInfo start = new("dotnet", ["test", typeof(NativeCallNestingTests).Assembly.Location, "--filter", filter, "--logger", "console;verbosity=detailed"])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[ChildVariable] = mode;
        start.Environment["GO2CS_MN"] = "1";

        if (mode == "p1")
            start.Environment["GO2CS_MN_PROCS"] = "1";
        else
            start.Environment.Remove("GO2CS_MN_PROCS");

        using Process process = Process.Start(start)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(180_000))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail($"the native-nesting child ({mode}) did not finish in 180 s");
        }

        string child = stdout.Result + stderr.Result;

        Assert.AreEqual(0, process.ExitCode, $"the child ({mode}) failed:\n{child}");
        Assert.IsTrue(child.Contains("Passed!", StringComparison.Ordinal) || child.Contains("Passed:", StringComparison.Ordinal), $"the child test ran and passed:\n{child}");

        foreach (string row in rows)
            Assert.IsTrue(child.Contains($"NATIVE NESTING ROW {row} ok", StringComparison.Ordinal), $"the child ran row {row}:\n{child}");
    }
}
