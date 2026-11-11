using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using runtime = go.runtime_package;

namespace GolibTests;

[TestClass]
public class GoschedBackoffTests
{
    // The finding these guards pin closed (board 2026-08-21, "Gosched ring starvation"): converted
    // goroutines are dedicated OS threads and Gosched mapped to a bare Thread.Yield(), which Linux
    // lowers to sched_yield(2) — near-inert for CPU-bound threads under CFS. sync/atomic's
    // TestValueCompareAndSwapConcurrent is a strict token-passing ring (value k advanceable only by
    // one specific goroutine), so every one of its 100,000 handoffs paid a fair-share epoch: ≥45 min
    // on Linux against 183 s on Windows on the SAME silicon. The ratified remedy is an adaptive
    // backoff INSIDE Gosched: consecutive provably-inert yields escalate to Thread.Sleep(1), which
    // leaves the run queue so the one thread that can make progress gets the processor.
    //
    // "Inert" is decided by measurement, not by Thread.Yield's return value alone, because that
    // value LIES on Linux: measured 2026-08-21 (both hosts, 12 logical processors) — Windows idle
    // yield returns false at ~300 ns and contended returns true at ~4.1 µs; Linux returns true
    // 10000/10000 in BOTH states, idle ~431 ns vs contended ~6.5 µs. Timing separates the states
    // on both hosts with ≥7× margin around 2 µs, so the predicate is
    // `!switched || elapsed < 2 µs`.

    // A quiet single thread spinning on Gosched is the deterministic inert case on both hosts:
    // there is (almost) never another ready thread to switch to, so every yield is inert and the
    // escalation tier must engage. Before the backoff existed this loop completed in a few
    // milliseconds (6,400 × ~0.4 µs); with escalation at 64 consecutive inert yields it must pay
    // roughly 6,400/64 ≈ 100 one-millisecond sleeps. The 50 ms bar sits far above the pre-fix
    // ceiling and far below the post-fix expectation. Three attempts absorb the freak case where
    // ambient machine load makes some yields effective (an effective yield legitimately resets
    // the escalation counter).
    [TestMethod]
    public void InertGoschedSpinEscalatesToSleep()
    {
        double bestMilliseconds = 0;

        for (int attempt = 0; attempt < 3; attempt++)
        {
            Stopwatch timer = Stopwatch.StartNew();

            for (int i = 0; i < 6_400; i++)
                runtime.Gosched();

            timer.Stop();
            bestMilliseconds = Math.Max(bestMilliseconds, timer.Elapsed.TotalMilliseconds);

            if (bestMilliseconds >= 50.0)
                return;
        }

        Assert.Fail($"6,400 back-to-back Gosched calls on a quiet thread finished in {bestMilliseconds:F1} ms " +
            "on every attempt — inert yields are not escalating to sleep, which is the exact starvation " +
            "shape that made sync/atomic's CAS ring exceed 45 minutes on Linux.");
    }

    // The fast path must stay free: when yields are EFFECTIVE (real context switches, ≥4 µs on
    // both hosts), the escalation counter resets and no sleep tier is ever reached. Saturating
    // every processor with Gosched spinners makes each yield find a ready sibling, so 25,000
    // calls per worker must complete at context-switch cost (~ hundreds of ms total), never at
    // sleep cost. The bound is generous — its job is to catch an unconditional-sleep regression
    // (which would add ~390 ms per worker at minimum and scale with any tuning mistake), not to
    // benchmark the scheduler.
    //
    // THE MEASUREMENT ARM (COORD ruling M, 2026-11-11; the bar below is unchanged). The hosted macOS
    // runners have a slow mode: 27 full-suite osx-arm64 readings since 2026-11-06 split into 0.09-1 s
    // (25) and 11.1 / 14.7 s (2), with nothing between, the same tree passing and failing in the same
    // minute. Whether that mode reaches the sleep tier (the backoff misjudging the host) or slows the
    // host's own yields is not yet known. So the timed Gosched phase G is bracketed by two raw
    // Thread.Yield phases of the same shape (R1 before, R2 after: same worker count, same 25,000 calls,
    // the stopwatch started before the threads are made, as G's is), and G's sleep-tier entries are
    // counted (GoschedBackoff.Escalations, process-wide; GolibTests runs serially, as
    // GoschedBackoffEscalationLatencyTests relies on). The READING line goes to the test's stdout
    // (the trx) on every OS; the bar that follows will be chosen from these readings.
    [TestMethod]
    public void EffectiveGoschedYieldsStayCheap()
    {
        int workerCount = Environment.ProcessorCount * 2;
        double r1 = RawYieldSeconds(workerCount);
        long escalationsBefore = GoschedBackoff.Escalations;
        Stopwatch timer = Stopwatch.StartNew();

        Thread[] workers = Enumerable.Range(0, workerCount).Select(_ => new Thread(() =>
        {
            for (int i = 0; i < 25_000; i++)
                runtime.Gosched();
        })
        { IsBackground = true }).ToArray();

        foreach (Thread worker in workers)
            worker.Start();

        foreach (Thread worker in workers)
            worker.Join();

        timer.Stop();
        long escalations = GoschedBackoff.Escalations - escalationsBefore;
        double r2 = RawYieldSeconds(workerCount);

        Console.WriteLine($"READING EffectiveGoschedYieldsStayCheap: workers {workerCount} (ProcessorCount " +
            $"{Environment.ProcessorCount}), R1 raw Thread.Yield {r1:F3} s, G Gosched {timer.Elapsed.TotalSeconds:F3} s, " +
            $"R2 raw Thread.Yield {r2:F3} s, G escalations {escalations}");

        Assert.IsTrue(timer.Elapsed.TotalSeconds < 10.0,
            $"{workerCount} contended workers × 25,000 effective Gosched calls took {timer.Elapsed.TotalSeconds:F1} s — " +
            "effective yields must never reach the sleep tier; the backoff's escalation is firing on the fast path.");
    }

    // The host's own reference for G: the same contended shape with a raw Thread.Yield, which never
    // enters GoschedBackoff, so it reads what the scheduler itself charges for these yields.
    private static double RawYieldSeconds(int workerCount)
    {
        Stopwatch timer = Stopwatch.StartNew();

        Thread[] workers = Enumerable.Range(0, workerCount).Select(_ => new Thread(() =>
        {
            for (int i = 0; i < 25_000; i++)
                Thread.Yield();
        })
        { IsBackground = true }).ToArray();

        foreach (Thread worker in workers)
            worker.Start();

        foreach (Thread worker in workers)
            worker.Join();

        timer.Stop();
        return timer.Elapsed.TotalSeconds;
    }
}
