using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// D3: package time's real-clock timers and time.Sleep are on golib's one timer heap (GoTimer), beside
// internal/poll's deadlines, as Go keeps every runtime timer in one kind of heap. A Timer is in the heap once
// while armed, however often it is reset, and leaves it on Stop; a sleeping goroutine's timer is in the heap
// for the sleep; and the deadlock report's timer scan (GoTimer.AnyPending, Go's checkdead) counts a callback
// that is still running.
[TestClass]
[DoNotParallelize]
public class TimeOnGoTimerTests
{
    private static long now() => MonotonicClock.Nanoseconds();

    private const long Millisecond = 1_000_000;

    [TestMethod]
    public void ATimerIsOnTheOneHeapWhileArmed()
    {
        int before = GoTimer.Count;

        ж<time_package.Timer> timer = time_package.NewTimer(time_package.ΔHour);

        Assert.AreEqual(before + 1, GoTimer.Count, "an armed time.Timer is not on GoTimer's heap");
        Assert.IsTrue(timer.Stop(), "Stop of an armed timer reported it unarmed");
        Assert.AreEqual(before, GoTimer.Count, "a stopped time.Timer stayed on GoTimer's heap");
    }

    [TestMethod]
    public void AResetTimerIsOnTheHeapOnce()
    {
        int before = GoTimer.Count;

        ж<time_package.Timer> timer = time_package.NewTimer(time_package.ΔHour);

        for (int i = 0; i < 1_000; i++)
            timer.Reset(time_package.ΔHour);

        Assert.AreEqual(before + 1, GoTimer.Count, "a reset time.Timer is not on GoTimer's heap exactly once");
        timer.Stop();
        Assert.AreEqual(before, GoTimer.Count);
    }

    [TestMethod]
    public void ASleepIsOnTheOneHeapForItsDuration()
    {
        int before = GoTimer.Count;
        using ManualResetEventSlim started = new();

        Thread sleeper = new(() =>
        {
            started.Set();
            time_package.Sleep(300 * Millisecond);
        });

        sleeper.Start();
        Assert.IsTrue(started.Wait(5_000));

        // The sleeper arms its timer right after it starts; wait for that, well inside the sleep.
        long deadline = now() + 200 * Millisecond;

        while (GoTimer.Count == before && now() < deadline)
            Thread.Sleep(1);

        Assert.AreEqual(before + 1, GoTimer.Count, "a sleeping goroutine's timer is not on GoTimer's heap");
        Assert.IsTrue(sleeper.Join(5_000), "the sleep did not end");
        Assert.AreEqual(before, GoTimer.Count, "an ended sleep left its timer on GoTimer's heap");
    }

    [TestMethod]
    public void ARunningCallbackCountsAsPending()
    {
        using ManualResetEventSlim running = new();
        using ManualResetEventSlim release = new();
        using ManualResetEventSlim returned = new();

        new GoTimer(_ =>
        {
            running.Set();
            release.Wait(5_000);
            returned.Set();
        }, null).Reset(now());

        bool pendingWhileRunning;
        int armed;

        try
        {
            Assert.IsTrue(running.Wait(5_000), "the timer did not fire");
            armed = GoTimer.Count;
            pendingWhileRunning = GoTimer.AnyPending;
        }
        finally
        {
            // The callback must return before its events are disposed.
            release.Set();
            returned.Wait(5_000);
        }

        // The arm reads the running count only while no timer is armed; an armed one makes AnyPending true
        // on its own.
        if (armed != 0)
            Assert.Inconclusive($"{armed} timer(s) still armed from elsewhere in this process; the running count cannot be read");

        // Off the heap and still running: what it does next (start a goroutine, ready a waiter) has not
        // happened, so Go's checkdead would not call the program dead.
        Assert.IsTrue(pendingWhileRunning, "a running callback did not count as pending");

        // The count drops just after the callback returns.
        long deadline = now() + 1_000 * Millisecond;

        while (GoTimer.AnyPending && now() < deadline)
            Thread.Sleep(1);

        Assert.IsFalse(GoTimer.AnyPending, "a returned callback still counted as pending");
    }
}
