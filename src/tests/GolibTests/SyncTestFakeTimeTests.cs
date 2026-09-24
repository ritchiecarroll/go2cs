using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using time = go.time_package;

namespace GolibTests;

/// <summary>
/// Fake time in a synctest bubble (S3 of DESIGN-gopark-goready-synctest.md): a member reads its
/// bubble's clock, which starts at 2000-01-01 00:00:00 UTC and moves only when the bubble is idle --
/// straight to the next fake timer -- so a Sleep, a Timer or an AfterFunc of any length completes at
/// its exact fake deadline in (almost) no real time. A fake timer touched from outside the bubble
/// panics with Go's text.
/// </summary>
[TestClass]
public class SyncTestFakeTimeTests
{
    private const int TimeoutMs = 30000;

    private static Exception? OnGoroutine(Action body, int timeoutMs = TimeoutMs)
    {
        Exception? escaped = null;
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                body();
            }
            catch (Exception ex)
            {
                escaped = ex;
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(timeoutMs), "the goroutine never finished");
        return escaped;
    }

    private static void AssertNoFailure(Exception? escaped) =>
        Assert.IsNull(escaped, $"escaped: {escaped}");

    private static time.Duration Seconds(long n) => n * time.ΔSecond;

    [TestMethod]
    public void NowInABubbleIsMidnight2000Utc()
    {
        long insideUnix = 0, outsideUnix = 0;

        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() => insideUnix = time.Now().Unix());
            outsideUnix = time.Now().Unix();
        }));

        Assert.AreEqual(946684800L, insideUnix, "a bubble's clock starts at 2000-01-01T00:00:00Z (Go's synctestBaseTime)");
        Assert.IsTrue(outsideUnix > 1_700_000_000L, "outside a bubble the clock is the real one");
    }

    [TestMethod]
    public void AnHourLongSleepTakesExactlyAnHourOfFakeTimeAndNoRealTime()
    {
        long elapsedNs = 0;
        Stopwatch real = Stopwatch.StartNew();

        AssertNoFailure(OnGoroutine(() =>
            SyncTestBubble.Run(() =>
            {
                time.Time start = time.Now();
                time.Sleep(Seconds(3600));
                elapsedNs = (long)time.Since(start);
            })));

        real.Stop();
        Assert.AreEqual(3600L * 1_000_000_000L, elapsedNs, "the sleep ends at its exact fake deadline");
        Assert.IsTrue(real.ElapsedMilliseconds < 10000, $"an hour of fake time took {real.ElapsedMilliseconds} ms of real time");
    }

    [TestMethod]
    public void SleepersWakeInDeadlineOrder()
    {
        string order = "";
        object gate = new();

        AssertNoFailure(OnGoroutine(() =>
            SyncTestBubble.Run(() =>
            {
                void sleeper(string name, long seconds)
                {
                    time.Sleep(Seconds(seconds));
                    lock (gate) order += name;
                }

                Goroutine.Start(() => sleeper("c", 3));
                Goroutine.Start(() => sleeper("a", 1));
                Goroutine.Start(() => sleeper("b", 2));
            })));

        Assert.AreEqual("abc", order, "fake time advances to each deadline in turn");
    }

    [TestMethod]
    public void ATimerDeliversAtItsFakeDeadline()
    {
        long firedAtNs = 0;

        AssertNoFailure(OnGoroutine(() =>
            SyncTestBubble.Run(() =>
            {
                time.Time start = time.Now();
                ж<time.Timer> t = time.NewTimer(Seconds(5));
                time.Time fired = t.Value.C.Receive();
                firedAtNs = (long)fired.Sub(start);
            })));

        Assert.AreEqual(5L * 1_000_000_000L, firedAtNs, "the timer's channel carries its exact fake deadline");
    }

    [TestMethod]
    public void AnAfterFuncGoroutineRunsInTheBubble()
    {
        bool inBubble = false;
        long ranAtNs = 0;

        AssertNoFailure(OnGoroutine(() =>
            SyncTestBubble.Run(() =>
            {
                time.Time start = time.Now();

                time.AfterFunc(Seconds(7), () =>
                {
                    inBubble = SyncTestBubble.Current is not null;
                    ranAtNs = (long)time.Since(start);
                });
            })));

        Assert.IsTrue(inBubble, "an AfterFunc's goroutine is started by the bubble and belongs to it");
        Assert.AreEqual(7L * 1_000_000_000L, ranAtNs, "and runs at its exact fake deadline");
    }

    [TestMethod]
    public void AWaitDoesNotAdvanceTimeOverASleeper()
    {
        // A sleeper is durably blocked, so Wait returns at once -- and fake time has not moved: only
        // Run advances it, after the bubble idles.
        long beforeWaitNs = 0, afterWaitNs = 0;

        AssertNoFailure(OnGoroutine(() =>
            SyncTestBubble.Run(() =>
            {
                Goroutine.Start(() => time.Sleep(Seconds(60)));
                beforeWaitNs = time.Now().UnixNano();
                SyncTestBubble.Wait();
                afterWaitNs = time.Now().UnixNano();
            })));

        Assert.AreEqual(beforeWaitNs, afterWaitNs, "Wait returned over the sleeper without moving fake time");
    }

    [TestMethod]
    public void AFakeTimerFromOutsideTheBubblePanics()
    {
        ж<time.Timer>? leaked = null;
        AssertNoFailure(OnGoroutine(() => SyncTestBubble.Run(() => { leaked = time.NewTimer(Seconds(1)); leaked.Stop(); })));

        StringAssert.Contains(OnGoroutine(() => leaked!.Stop())?.Message, "stop of synctest timer from outside bubble");
        StringAssert.Contains(OnGoroutine(() => leaked!.Reset(Seconds(1)))?.Message, "reset of synctest timer from outside bubble");
    }

    [TestMethod]
    public void ABubbleBlockedForeverWithTimersPendingStillReportsItsDeadlock()
    {
        // Run keeps advancing fake time while timers remain; once none is pending and a member is
        // still blocked, it reports Go's deadlock.
        Exception? escaped = OnGoroutine(() =>
            SyncTestBubble.Run(() =>
            {
                channel<int> never = new(0);
                Goroutine.Start(() => { time.Sleep(Seconds(10)); never.Receive(); });
            }));

        StringAssert.Contains(escaped?.Message, "deadlock: all goroutines in bubble are blocked");
    }
}
