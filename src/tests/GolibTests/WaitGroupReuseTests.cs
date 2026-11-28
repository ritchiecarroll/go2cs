// WaitGroupReuseTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

#nullable enable

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;
using Δsync = go.sync_package;

namespace GolibTests;

// A WaitGroup whose count is raised again before a woken waiter has returned (docs/phase4/DESIGN-waitgroup-reuse.md).
// Go's Wait re-reads the state after its semaphore returns and panics when it is not zero: "sync: WaitGroup is reused
// before previous Wait has returned" (sync/waitgroup.go, the read after runtime_SemacquireWaitGroup). The plain Wait
// here slept on one event for the WaitGroup's whole life, which the Add from zero reset under the woken waiter, so the
// waiter neither returned nor panicked.
//
// The reuse is a race in Go too: a woken waiter that reads the state before the new Add returns normally. So the first
// row asks only that every waiter ENDS, and the rows that assert an outcome take the race away by holding the
// WaitGroup's own lock across the calls (by reflection: the state is internal to go.sync). Under that hold no waiter
// can read the state between the calls, and the lock is reentrant, so the calls run as written.
//
// These rows run with the scheduler off and read the plain Wait. The twin's rows are in WaitGroupTwinTests' child.
[TestClass]
public class WaitGroupReuseTests
{
    internal const string Reused = "sync: WaitGroup is reused before previous Wait has returned";

    private const string CostVariable = "GO2CS_TEST_WGREUSE_COST";
    private const int TimeoutMs = 30_000;
    private const int RoundDeadlineMs = 2_000;
    private const int Waiters = 8;
    private const int AsleepMicroseconds = 1_000;

    private static ж<Δsync.WaitGroup> NewWaitGroup()
    {
        heap(new Δsync.WaitGroup(), out ж<Δsync.WaitGroup> box);
        return box;
    }

    // The WaitGroup's state object. It exists after the first Add.
    private static object StateOf(ж<Δsync.WaitGroup> wg) =>
        typeof(Δsync.WaitGroup).GetField("st", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(wg.Value)!;

    // The lock Add, Wait and the twin take.
    internal static object GateOf(ж<Δsync.WaitGroup> wg)
    {
        object state = StateOf(wg);
        return state.GetType().GetField("Gate", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(state)!;
    }

    // The registered waiters, as a count that can be read without the lock (the cost reading waits on it).
    private static ICollection ParkedOf(ж<Δsync.WaitGroup> wg)
    {
        object state = StateOf(wg);
        return (ICollection)state.GetType().GetField("Parked", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(state)!;
    }

    // K goroutines in the plain Wait. Each records what its Wait threw, if anything.
    private sealed class PlainWaiters
    {
        private readonly Goroutine?[] m_goroutines;
        private readonly Exception?[] m_thrown;
        private int m_ended;

        internal PlainWaiters(int count, ж<Δsync.WaitGroup> wg)
        {
            m_goroutines = new Goroutine?[count];
            m_thrown = new Exception?[count];

            for (int i = 0; i < count; i++)
            {
                int index = i;

                Goroutine.Start(() =>
                {
                    Volatile.Write(ref m_goroutines[index], Goroutine.Current);

                    try
                    {
                        wg.Wait();
                    }
                    catch (Exception ex)
                    {
                        m_thrown[index] = ex;
                    }

                    Interlocked.Increment(ref m_ended);
                });
            }
        }

        internal int Count => m_goroutines.Length;

        internal int Ended => Volatile.Read(ref m_ended);

        // Read only after AwaitEnded answered true.
        internal Exception?[] Thrown => m_thrown;

        internal int Panicked => m_thrown.Count(static ex => ex is PanicException);

        // Every waiter parked with the given reason, or the row fails by name at the deadline.
        internal void AwaitParked(WaitReason reason) =>
            Assert.IsTrue(SpinWait.SpinUntil(() => m_goroutines.All(g => Volatile.Read(ref g) is { State: GoroutineState.Parked } parked && parked.Reason == reason), TimeoutMs),
                $"the waiters parked with {reason}: " + string.Join(", ", m_goroutines.Select(static g => g is null ? "unstarted" : $"{g.State}/{g.Reason}")));

        internal bool AwaitEnded(int milliseconds) =>
            SpinWait.SpinUntil(() => Volatile.Read(ref m_ended) == Count, milliseconds);

        // Every waiter ended with Go's panic, its text byte for byte.
        internal void AssertAllPanickedAsGoDoes()
        {
            for (int i = 0; i < Count; i++)
            {
                Assert.IsTrue(m_thrown[i] is PanicException, $"waiter {i} panicked: it {(m_thrown[i] is null ? "returned normally" : $"threw {m_thrown[i]}")}");
                Assert.AreEqual(Reused, m_thrown[i]!.Message, $"waiter {i} panicked with Go's text");
            }
        }

        internal void AssertAllReturned()
        {
            for (int i = 0; i < Count; i++)
                Assert.IsNull(m_thrown[i], $"waiter {i} returned normally: {m_thrown[i]}");
        }
    }

    // ---- U1: the hang ---------------------------------------------------------------------------------------------

    // The measurement of the BOARD row of 2026-11-27 as a row: one goroutine in the plain Wait on a count of 1, and once
    // it is parked the test calls Done and then Add(1) at once. Go ends the waiter either way: it returns (it read the
    // state before the Add) or it panics (it read it after). Before the reuse seat the waiter did neither, in 100 of 100
    // rounds. The row stops at the first waiter that misses its deadline; the split is printed, not asserted.
    [TestMethod]
    public void AWaiterEndsWhenTheCountIsRaisedAgainAtOnce()
    {
        const int rounds = 100;
        int returned = 0, panicked = 0;

        for (int round = 1; round <= rounds; round++)
        {
            ж<Δsync.WaitGroup> wg = NewWaitGroup();
            wg.Add(1);
            PlainWaiters run = new(1, wg);
            run.AwaitParked(WaitReason.SyncWaitGroupWait);

            wg.Done();
            wg.Add(1);

            bool ended = run.AwaitEnded(RoundDeadlineMs);
            wg.Done();      // the count back to zero: a waiter that slept through its generation is let go here

            Assert.IsTrue(ended, $"round {round}: the waiter was still parked {RoundDeadlineMs} ms after the Done that reached zero (Go's waiter returns or panics)");
            Exception? thrown = run.Thrown[0];

            if (thrown is null)
            {
                returned++;
            }
            else
            {
                Assert.IsTrue(thrown is PanicException, $"round {round}: the waiter threw {thrown}");
                Assert.AreEqual(Reused, thrown.Message, $"round {round}: the waiter panicked with Go's text");
                panicked++;
            }
        }

        Console.WriteLine($"WGREUSE READING hang rounds={rounds} returned={returned} panicked={panicked}");
    }

    // ---- U2: the panic, exact ---------------------------------------------------------------------------------------

    // K waiters parked on one count. Done and Add(1) run under one hold of the WaitGroup's lock, so every waiter reads a
    // count of 1 after its wake. Go's `wg.state.Load() != 0` is true for each, and each panics.
    [TestMethod]
    public void EveryWaiterPanicsAsGoDoesWhenTheCountIsRaisedBeforeItsRead()
    {
        ж<Δsync.WaitGroup> wg = NewWaitGroup();
        wg.Add(1);
        PlainWaiters run = new(Waiters, wg);
        run.AwaitParked(WaitReason.SyncWaitGroupWait);

        lock (GateOf(wg))
        {
            wg.Done();
            wg.Add(1);
        }

        bool ended = run.AwaitEnded(RoundDeadlineMs);
        wg.Done();

        Assert.IsTrue(ended, $"{run.Ended} of {run.Count} waiters ended: the rest were still parked {RoundDeadlineMs} ms after the Done that reached zero");
        run.AssertAllPanickedAsGoDoes();
    }

    // ---- U3: no panic at zero ---------------------------------------------------------------------------------------

    // The bound on U2. Done, Add(1), Done under the hold: the count went up and came back, and it is zero at every
    // waiter's read. Go reads the state, not its history, and returns. A check that asked "was there a later Add" would
    // panic here.
    [TestMethod]
    public void NoWaiterPanicsWhenTheCountIsBackAtZeroAtItsRead()
    {
        ж<Δsync.WaitGroup> wg = NewWaitGroup();
        wg.Add(1);
        PlainWaiters run = new(Waiters, wg);
        run.AwaitParked(WaitReason.SyncWaitGroupWait);

        lock (GateOf(wg))
        {
            wg.Done();
            wg.Add(1);
            wg.Done();
        }

        Assert.IsTrue(run.AwaitEnded(TimeoutMs), $"{run.Ended} of {run.Count} waiters ended");
        run.AssertAllReturned();
    }

    // ---- U4: the bubble flag ----------------------------------------------------------------------------------------

    // Go's state word holds the synctest bubble flag beside the counter and the waiter count, and the waiter's read is of
    // the whole word. Inside a bubble, Done then Add(0) under the hold leaves the counter at zero and no waiter, and the
    // Add(0) sets the flag again (waitgroup.go: `wg.state.Or(waitGroupBubbleFlag)` runs for every Add from a bubble). So
    // the word is not zero and Go's waiter panics. Nothing but the flag says so here.
    [TestMethod]
    public void TheBubbleFlagAloneMakesTheWaiterPanicAsGoDoes()
    {
        Exception? thrown = null, waiterThrew = null;
        int ended = 0;
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                SyncTestBubble.Run(() =>
                {
                    ж<Δsync.WaitGroup> wg = NewWaitGroup();
                    wg.Add(1);

                    Goroutine.Start(() =>
                    {
                        try
                        {
                            wg.Wait();
                        }
                        catch (Exception ex)
                        {
                            waiterThrew = ex;
                        }

                        Volatile.Write(ref ended, 1);
                    });

                    SyncTestBubble.Wait();      // returns once the waiter is durably blocked in Wait

                    lock (GateOf(wg))
                    {
                        wg.Done();
                        wg.Add(0);
                    }

                    Assert.IsTrue(SpinWait.SpinUntil(() => Volatile.Read(ref ended) == 1, TimeoutMs), "the waiter ended");
                });
            }
            catch (Exception ex)
            {
                thrown = ex;
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(TimeoutMs), "the bubble finished");
        Assert.IsNull(thrown, thrown?.ToString());
        Assert.IsTrue(waiterThrew is PanicException, $"the waiter panicked on the flag: it {(waiterThrew is null ? "returned normally" : $"threw {waiterThrew}")}");
        Assert.AreEqual(Reused, waiterThrew!.Message);
    }

    // ---- U5: two generations, used correctly --------------------------------------------------------------------------

    // One WaitGroup, used twice the way Go allows: every waiter of the first count has returned before the count is
    // raised again. Nobody panics, and the second count's waiters are released by the second Done, not the first.
    [TestMethod]
    public void AWaitGroupUsedAgainAfterEveryWaiterReturnedDoesNotPanic()
    {
        ж<Δsync.WaitGroup> wg = NewWaitGroup();

        for (int generation = 1; generation <= 3; generation++)
        {
            wg.Add(1);
            PlainWaiters run = new(Waiters, wg);
            run.AwaitParked(WaitReason.SyncWaitGroupWait);
            Assert.AreEqual(0, run.Ended, $"generation {generation}: a waiter ended before its Done");

            wg.Done();
            Assert.IsTrue(run.AwaitEnded(TimeoutMs), $"generation {generation}: {run.Ended} of {run.Count} waiters ended");
            run.AssertAllReturned();
        }
    }

    // ---- The default-mode cost reading (G's required reading; reported, never asserted) ------------------------------

    // Every program's plain Wait runs through the changed path, so the seat reads its cost before and after on one box.
    // The row does nothing unless GO2CS_TEST_WGREUSE_COST=1 (it is a measurement, and takes minutes):
    //
    //   GO2CS_TEST_WGREUSE_COST=1 dotnet test GolibTests.dll --filter FullyQualifiedName~ChildWaitGroupCostReading
    //
    // Five shapes, five timed repetitions each after a warm-up, every repetition and the median printed:
    //   one-waiter-handoff   1 waiter, 100,000 generations. The Done comes from a second thread the moment the waiter
    //                        is registered, so the waiter is still in its spin when it is released.
    //   one-waiter-go        1 waiter, 100,000 generations, as a Go program writes it: Add(1); go Done(); Wait().
    //                        The Wait blocks or not as the two race.
    //   many-waiters         64 waiters on one WaitGroup, 10,000 generations. Each generation's Done comes once all 64
    //                        are registered. The waiters are started through a semaphore, whose cost is in the number
    //                        for both sides alike.
    //   one-waiter-asleep    as one-waiter-handoff, 5,000 generations, with the Done held back 1 ms after the
    //   many-waiters-asleep  registration (64 waiters: 1,000 generations, after the last one's), so every waiter has
    //                        spun out and sleeps. The 1 ms is taken off the printed number.
    [TestMethod]
    public void ChildWaitGroupCostReading()
    {
        if (Environment.GetEnvironmentVariable(CostVariable) != "1")
            return;

        Assert.IsFalse(GoScheduler.Enabled, "the reading is of the default mode");

        // The plain Wait spins as long as this event does before it sleeps (waitgroup.cs); the line keeps the number honest.
        using (ManualResetEventSlim slim = new(false))
            Console.WriteLine($"WGREUSE COST processors={Environment.ProcessorCount} event-spin-count={slim.SpinCount}");

        Cost("one-waiter-handoff", waiters: 1, generations: 100_000, static generations => OneWaiterHandOff(generations, 0));
        Cost("one-waiter-go", waiters: 1, generations: 100_000, OneWaiterGoShaped);
        Cost("many-waiters", waiters: 64, generations: 10_000, static generations => ManyWaiters(64, generations, 0));
        Cost("one-waiter-asleep", waiters: 1, generations: 5_000, static generations => OneWaiterHandOff(generations, AsleepMicroseconds) - AsleepMicroseconds * 1e3);
        Cost("many-waiters-asleep", waiters: 64, generations: 1_000, static generations => ManyWaiters(64, generations, AsleepMicroseconds) - AsleepMicroseconds * 1e3);
    }

    private static void Cost(string shape, int waiters, int generations, Func<int, double> run)
    {
        run(Math.Max(200, generations / 50));       // warm-up, not timed

        double[] reads = new double[5];

        for (int i = 0; i < reads.Length; i++)
            reads[i] = run(generations);

        double[] sorted = [.. reads.OrderBy(static ns => ns)];
        Console.WriteLine($"WGREUSE COST {shape} waiters={waiters} generations={generations} ns-per-generation: " +
            $"{string.Join(" ", reads.Select(static ns => ns.ToString("F0")))} median={sorted[sorted.Length / 2]:F0}");
    }

    // A spin with a deadline, so a defect stops the reading by name instead of hanging the host.
    private static void SpinUntil(Func<bool> condition, string what)
    {
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 60;

        for (int i = 1; !condition(); i++)
        {
            Thread.SpinWait(4);

            if ((i & 0xFFFF) == 0 && Stopwatch.GetTimestamp() > deadline)
                throw new TimeoutException($"the cost reading waited 60 s for {what}");
        }
    }

    // Holds the calling thread for the given time without giving it up (a sleep would add its own wake to the reading).
    private static void Hold(int microseconds)
    {
        if (microseconds == 0)
            return;

        long until = Stopwatch.GetTimestamp() + Stopwatch.Frequency * microseconds / 1_000_000;

        while (Stopwatch.GetTimestamp() < until)
            Thread.SpinWait(4);
    }

    // Runs the timed body on a goroutine and returns its nanoseconds per generation.
    private static double OnGoroutine(int generations, Action body)
    {
        Exception? thrown = null;
        long ticks = 0;
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                long start = Stopwatch.GetTimestamp();
                body();
                ticks = Stopwatch.GetTimestamp() - start;
            }
            catch (Exception ex)
            {
                thrown = ex;
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(600_000), "the cost reading finished");
        Assert.IsNull(thrown, thrown?.ToString());
        return ticks * (1e9 / Stopwatch.Frequency) / generations;
    }

    private static double OneWaiterHandOff(int generations, int holdMicroseconds)
    {
        ж<Δsync.WaitGroup> wg = NewWaitGroup();
        wg.Add(1);
        wg.Done();
        ICollection parked = ParkedOf(wg);
        int kicked = 0;

        Thread helper = new(() =>
        {
            for (int generation = 1; generation <= generations; generation++)
            {
                int wanted = generation;
                SpinUntil(() => Volatile.Read(ref kicked) == wanted && parked.Count == 1, "the waiter to register");
                Hold(holdMicroseconds);
                wg.Done();
            }
        })
        { IsBackground = true };

        helper.Start();

        double ns = OnGoroutine(generations, () =>
        {
            for (int generation = 1; generation <= generations; generation++)
            {
                wg.Add(1);
                Volatile.Write(ref kicked, generation);
                wg.Wait();
            }
        });

        Assert.IsTrue(helper.Join(TimeoutMs), "the helper finished");
        return ns;
    }

    private static double OneWaiterGoShaped(int generations)
    {
        ж<Δsync.WaitGroup> wg = NewWaitGroup();

        return OnGoroutine(generations, () =>
        {
            for (int generation = 1; generation <= generations; generation++)
            {
                wg.Add(1);
                Goroutine.Start(() => wg.Done());
                wg.Wait();
            }
        });
    }

    private static double ManyWaiters(int waiters, int generations, int holdMicroseconds)
    {
        ж<Δsync.WaitGroup> wg = NewWaitGroup();
        wg.Add(1);
        wg.Done();
        ICollection parked = ParkedOf(wg);
        using SemaphoreSlim start = new(0);
        long finished = 0;
        int stop = 0, exited = 0;
        Exception? thrown = null;

        for (int i = 0; i < waiters; i++)
        {
            Goroutine.Start(() =>
            {
                try
                {
                    while (true)
                    {
                        start.Wait();

                        if (Volatile.Read(ref stop) == 1)
                            break;

                        wg.Wait();
                        Interlocked.Increment(ref finished);
                    }
                }
                catch (Exception ex)
                {
                    Volatile.Write(ref thrown, ex);
                }

                Interlocked.Increment(ref exited);
            });
        }

        long begin = Stopwatch.GetTimestamp();

        for (int generation = 1; generation <= generations; generation++)
        {
            long target = (long)generation * waiters;
            wg.Add(1);
            start.Release(waiters);
            SpinUntil(() => parked.Count == waiters || Volatile.Read(ref thrown) is not null, "every waiter to register");
            Hold(holdMicroseconds);
            wg.Done();
            SpinUntil(() => Interlocked.Read(ref finished) == target || Volatile.Read(ref thrown) is not null, "every waiter to return");
            Assert.IsNull(Volatile.Read(ref thrown), thrown?.ToString());
        }

        long ticks = Stopwatch.GetTimestamp() - begin;

        Volatile.Write(ref stop, 1);
        start.Release(waiters);
        SpinUntil(() => Volatile.Read(ref exited) == waiters, "the waiters to exit");
        return ticks * (1e9 / Stopwatch.Frequency) / generations;
    }
}
