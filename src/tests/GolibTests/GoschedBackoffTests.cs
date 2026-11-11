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
    // THE HOST'S OWN YIELDS (COORD rulings M and A, 2026-11-11). The hosted macOS runners have a slow
    // mode: 27 full-suite osx-arm64 readings since 2026-11-06 split into 0.09-1 s (25) and 11.1 / 14.7 s
    // (2), nothing between, the same tree passing and failing in the same minute. So every run brackets
    // the timed Gosched phase G with two raw Thread.Yield phases of the same shape (R1 before, R2 after:
    // same worker count, same 25,000 calls, the stopwatch started before the threads are made, as G's is),
    // counts G's sleep-tier entries (GoschedBackoff.Escalations, process-wide; GolibTests runs serially),
    // and prints one READING line to stdout (the trx). Ruling M's reading caught the slow mode in R1, a
    // raw yield that never enters GoschedBackoff: R1 4.498 s against 0.15-0.22 s, then G 0.155 s with 0
    // escalations seconds later (osx-arm64, run 43064847754). The mode is the HOST's, and it is transient.
    //
    // THE BAR. Linux and windows keep the absolute 10 s. On macOS the bar is RELATIVE to the host:
    //   - G minus the slower of R1 and R2 under 5.0 s. Derivation: the largest normal-mode excess
    //     measured is 0.80 s (osx-arm64, run 43064826815: G 1.019, R 0.217); the other darwin legs read
    //     -0.33, -0.23 and -4.34 s. 5.0 s is more than 6x that, and it sits under the excess at which the
    //     absolute 10 s first reddened in the normal mode: 9.8 s on osx-arm64 (G at 10 s over an R of about
    //     0.2 s), and 6.4-8.7 s on osx-x64 (whose R reads 1.3-3.6 s). So it reds everything the absolute
    //     bar caught there, and more. An unconditional-sleep regression (every
    //     call sleeping its 1 ms) adds about 25 s per worker.
    //   - G under 60 s absolute, the backstop, so a backoff regression that lands on a slow host still
    //     reds. Derivation: the slowest G ever read on a mac leg is 24.5 s (osx-x64, a filtered probe,
    //     run 43001263448); 60 s is about 2.4x that, and 6x the old bar.
    //   - up to 3 attempts of the whole R1, G, R2 bracket, passing on the first that meets both. The slow
    //     mode lasts seconds and can land on G alone, as it landed on R1 alone; a real regression fails
    //     every attempt.
    // There is NO escalation-count bar: the count is OS-shaped, not a property (hosted linux escalates
    // ~350 times per worker of a possible 390 under this shape, windows ~5, darwin 0-3; BOARD 2026-11-11).
    [TestMethod]
    public void EffectiveGoschedYieldsStayCheap()
    {
        int workerCount = Environment.ProcessorCount * 2;

        if (!OperatingSystem.IsMacOS())
        {
            (_, double seconds, _) = MeasureContended(workerCount, attempt: 1);

            Assert.IsTrue(seconds < 10.0,
                $"{workerCount} contended workers × 25,000 effective Gosched calls took {seconds:F1} s — " +
                "effective yields must never reach the sleep tier; the backoff's escalation is firing on the fast path.");

            return;
        }

        for (int attempt = 1; ; attempt++)
        {
            (double r1, double g, double r2) = MeasureContended(workerCount, attempt);
            double excess = g - Math.Max(r1, r2);

            if (excess < MacExcessBoundSeconds && g < MacBackstopSeconds)
                return;

            if (attempt == MacAttempts)
                Assert.Fail($"{workerCount} contended workers × 25,000 effective Gosched calls took {g:F1} s, " +
                    $"{excess:F1} s beyond the host's own raw Thread.Yield over the same shape (R1 {r1:F3} s, R2 {r2:F3} s; " +
                    $"bound {MacExcessBoundSeconds} s beyond, {MacBackstopSeconds} s absolute) on each of {MacAttempts} attempts — " +
                    "the backoff's escalation is firing on the fast path.");
        }
    }

    // macOS's bar (COORD ruling A): G's excess over the slower bracket, its absolute backstop, and the attempts
    // that absorb a slow mode landing on G alone. The derivations are in the comment above the test.
    private const double MacExcessBoundSeconds = 5.0;
    private const double MacBackstopSeconds = 60.0;
    private const int MacAttempts = 3;

    // One R1, G, R2 bracket: the host's raw yields, the timed Gosched phase, the host's raw yields again, and the
    // READING line (to the test's stdout, the trx). G is timed exactly as the bar has always timed it.
    private static (double r1, double g, double r2) MeasureContended(int workerCount, int attempt)
    {
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
            $"R2 raw Thread.Yield {r2:F3} s, G escalations {escalations}, attempt {attempt}");

        return (r1, timer.Elapsed.TotalSeconds, r2);
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
