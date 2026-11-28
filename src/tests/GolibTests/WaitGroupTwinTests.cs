// WaitGroupTwinTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

#nullable enable
#pragma warning disable CS1998 // the control row's twin has no await: it measures the plain Wait inside a twin

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;
using Δsync = go.sync_package;

namespace GolibTests;

// sync.WaitGroup.Waitꞏtwin (docs/phase4/DESIGN-mn-waitgroup-twin.md, the owner's .5 ruling). A goroutine on the M:N
// scheduler that waits in the plain Wait blocks its carrier until the count reaches zero, so K waiters hold K carriers.
// The twin registers its goroutine and a park slot under the WaitGroup's lock, parks through ParkTwin, and gives the
// carrier back; the Add that reaches zero readies it and wakes the slot.
//
// GO2CS_MN is read once per process, so the carrier rows run in a CHILD (GoroutineRunTwinTests' pattern). Each row prints
// its own line, and the parent requires each line by name. The TWIN row awaits Waitꞏtwin: once all K waiters are parked,
// none has handed a P off and no carrier has started, and every waiter finishes. The CONTROL row is the hole itself: K
// twinned goroutines that call the plain Wait each hand their P off. It is the instrument's positive control, and it
// does not depend on the twin.
// The rows in this process run with the scheduler off, so they read the twin's plain path.
[TestClass]
public class WaitGroupTwinTests
{
    private const string ChildVariable = "GO2CS_TEST_WGTWIN_CHILD";
    private const int TimeoutMs = 30_000;

    private static readonly string[] s_mnRows = ["twin-waiters-hold-no-carriers", "twin-waiters-in-a-bubble-are-durable", "control-plain-wait-holds-carriers", "twin-zero-count-on-a-carrier", "mixed-waiters-one-done-wakes-all", "twin-survives-a-done-race", "twin-refused-park-leaves-no-waiter", "reused-twins-panic-as-go-does", "reused-mixed-waiters-all-panic"];

    private static ж<Δsync.WaitGroup> NewWaitGroup()
    {
        heap(new Δsync.WaitGroup(), out ж<Δsync.WaitGroup> box);
        return box;
    }

    // ---- In this process: the scheduler is off, so the twin is its plain path ---------------------------------------

    [TestMethod]
    public void AZeroCountWaitTwinReturnsACompletedTask()
    {
        ж<Δsync.WaitGroup> wg = NewWaitGroup();
        GoTask task = wg.Waitꞏtwin();

        Assert.IsTrue(task.GetAwaiter().IsCompleted, "a zero count returns at once");
        task.GetAwaiter().GetResult();
    }

    [TestMethod]
    public void OffACarrierTheWaitTwinBlocksInThePlainWaitUntilTheCountDrains()
    {
        ж<Δsync.WaitGroup> wg = NewWaitGroup();
        wg.Add(1);

        bool onCarrier = true, completed = false;
        int returned = 0;
        Exception? failure = null;

        Thread thread = new(() =>
        {
            try
            {
                onCarrier = GoScheduler.OnCarrier;
                GoTask task = wg.Waitꞏtwin();      // blocks in the plain Wait until Done below
                completed = task.GetAwaiter().IsCompleted;
                task.GetAwaiter().GetResult();
                Volatile.Write(ref returned, 1);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.Start();
        Thread.Sleep(100);
        int returnedEarly = Volatile.Read(ref returned);
        wg.Done();

        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(30)), "the call returned");
        Assert.IsNull(failure, failure?.ToString());
        Assert.IsFalse(onCarrier, "a plain thread is no carrier");
        Assert.AreEqual(0, returnedEarly, "the twin returned before the count drained");
        Assert.IsTrue(completed, "off a carrier the twin waited in plain Wait and returned a completed task");
    }

    // ---- The CHILD: GO2CS_MN=1 ------------------------------------------------------------------------------------

    [TestMethod]
    public void ChildWaitGroupTwinRows()
    {
        if (Environment.GetEnvironmentVariable(ChildVariable) != "mn")
            return;

        Assert.IsTrue(GoScheduler.Enabled, "the child runs on the scheduler");

        int procs = GoScheduler.Procs;
        int waiters = Math.Max(8, 4 * procs);

        // The reading: a carrier that blocks in a plain park hands its P off first (Goroutine.HandOffBeforeBlocking), so
        // `handoffs` counts the waiters that hold a carrier. Every reading is taken only once all K waiters are PARKED
        // (each reports the WaitGroup wait reason), so a read taken before they wait cannot pass for a twin that holds
        // nothing; that gate has a deadline that fails the row by name. The twin rows run first, before the control has
        // started any carrier past the P count, so their carriers-started is a reading too.

        // WARM-UP, REPORTED and not asserted (G): one full round of K twinned waiters, printed as `twin-cold` so the cold
        // reading stays in the record beside the warm one. Measured cold: 6 to 17 carriers started with 0 handoffs. That
        // cost is the scheduler's first start, not this twin's: no waiter hands a P off. The first twin start makes the
        // pool's own P carriers (GoScheduler.Start). Any carrier past those is the monitor's, which gives a P out on its
        // own in two ways, and the line prints both counts for the round: `injections` past a step that runs over its
        // threshold with work queued, and `retakes` and `sensed` from a carrier it reads as blocked. A first step that
        // compiles can do either (the JIT finding of row 16a); which one a host shows is on the line, not assumed here.
        {
            ж<Δsync.WaitGroup> wg = NewWaitGroup();
            wg.Add(1);
            long startedBefore = GoScheduler.CarriersStarted;
            Counts before = Counts.Read();

            Waiters run = StartWaiters(waiters, () => wg.Wait(), () => wg.Waitꞏtwin());
            run.AwaitEntered();
            run.AwaitParked(WaitReason.SyncWaitGroupWait);
            long started = GoScheduler.CarriersStarted - startedBefore;
            Counts round = Counts.Read() - before;

            wg.Done();
            run.AwaitFinished();

            Console.WriteLine($"WGTWIN READING twin-cold procs={procs} waiters={waiters} {round} carriers-started={started}");
        }

        // TWIN: every waiter parks through ParkTwin and gives its carrier back. `handoffs` is the claim, asserted in every
        // round: a waiter that holds a carrier hands its P off. `carriers-started` is asserted only in a round the
        // monitor gave no P out in, because a P the monitor gives out can start a carrier that no waiter holds (on a
        // loaded host, a carrier descheduled past the threshold while waiters queue). A round the monitor acted in is
        // printed and repeated, three rounds at most. `started` is read BEFORE the counters: the monitor counts first and
        // starts the carrier after, so a carrier in `started` has its cause in the later read.
        {
            bool carriersAsserted = false;

            for (int attempt = 1; attempt <= 3 && !carriersAsserted; attempt++)
            {
                ж<Δsync.WaitGroup> wg = NewWaitGroup();
                wg.Add(1);
                long startedBefore = GoScheduler.CarriersStarted;
                Counts before = Counts.Read();

                Waiters run = StartWaiters(waiters, () => wg.Wait(), () => wg.Waitꞏtwin());
                run.AwaitEntered();
                run.AwaitParked(WaitReason.SyncWaitGroupWait);
                long started = GoScheduler.CarriersStarted - startedBefore;
                Counts round = Counts.Read() - before;

                wg.Done();
                run.AwaitFinished();

                Console.WriteLine($"WGTWIN READING twin attempt={attempt} procs={procs} waiters={waiters} {round} carriers-started={started}");
                Assert.AreEqual(0, round.Handoffs, $"the twinned waiters held carriers: {round.Handoffs} handed their P off for {waiters} waiters");

                if (round.Monitor != 0)
                    continue;

                Assert.AreEqual(0, started, $"the twinned waiters held carriers: {started} started for {waiters} waiters on {procs} Ps");
                carriersAsserted = true;
            }

            if (!carriersAsserted)
                Console.WriteLine("WGTWIN READING twin carriers-started NOT ASSERTED: the monitor gave a P out in each of three rounds");
        }

        Row("twin-waiters-hold-no-carriers");

        // BUBBLE: a goroutine started inside a synctest bubble by goǃtwin is multiplexed and carries the bubble
        // (Goroutine.StartTwin takes the creator's bubble). Twinned waiters on a WaitGroup added in the bubble park
        // durably, so synctest.Wait returns once all K wait, as it does for plain waiters; a twin the bubble did not
        // count would hold synctest.Wait until the deadline below fails the row by name.
        {
            long started = -1, handoffs = -1;
            Exception? thrown = null;
            using ManualResetEventSlim done = new(false);

            Goroutine.Start(() =>
            {
                try
                {
                    SyncTestBubble.Run(() =>
                    {
                        ж<Δsync.WaitGroup> wg = NewWaitGroup();
                        wg.Add(1);
                        long startedBefore = GoScheduler.CarriersStarted, handoffsBefore = Counter("handoffs");

                        Waiters run = StartWaiters(waiters, () => wg.Wait(), () => wg.Waitꞏtwin());
                        SyncTestBubble.Wait();     // returns only once every member is durably blocked
                        run.AwaitParked(WaitReason.SynctestWaitGroupWait);
                        started = GoScheduler.CarriersStarted - startedBefore;
                        handoffs = Counter("handoffs") - handoffsBefore;

                        wg.Done();
                        run.AwaitFinished();
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

            Assert.IsTrue(done.Wait(TimeoutMs), "synctest.Wait never returned: the bubble did not count the twinned waiters as durably blocked");
            Assert.IsNull(thrown, thrown?.ToString());
            Console.WriteLine($"WGTWIN READING bubble procs={procs} waiters={waiters} handoffs={handoffs} carriers-started={started}");
            Assert.AreEqual(0, handoffs, $"the twinned waiters in the bubble held carriers: {handoffs} handed their P off");
        }

        Row("twin-waiters-in-a-bubble-are-durable");

        // CONTROL: the plain Wait inside a twin blocks its carrier, which hands its P off: the hole this seat closes,
        // and the instrument's positive control.
        {
            ж<Δsync.WaitGroup> wg = NewWaitGroup();
            wg.Add(1);
            long startedBefore = GoScheduler.CarriersStarted, handoffsBefore = Counter("handoffs");

            Waiters run = StartWaiters(waiters, () => wg.Wait(), async () => wg.Wait());
            run.AwaitEntered();
            run.AwaitParked(WaitReason.SyncWaitGroupWait);

            // A waiter publishes its wait reason before its hand-off is counted, so the count is waited for, not read once.
            SpinWait.SpinUntil(() => Counter("handoffs") - handoffsBefore >= waiters, TimeoutMs);
            long started = GoScheduler.CarriersStarted - startedBefore, handoffs = Counter("handoffs") - handoffsBefore;

            wg.Done();
            run.AwaitFinished();

            Console.WriteLine($"WGTWIN READING control procs={procs} waiters={waiters} handoffs={handoffs} carriers-started={started}");
            Assert.IsTrue(handoffs >= waiters, $"the plain Wait held its carriers: {handoffs} handoffs for {waiters} waiters");
        }

        Row("control-plain-wait-holds-carriers");

        // ZERO COUNT on a carrier: Go's `if v == 0 { return }`, so the twin hands back a completed task and never parks.
        {
            ж<Δsync.WaitGroup> wg = NewWaitGroup();
            bool onCarrier = false, completed = false;
            using ManualResetEventSlim done = new(false);

            goǃtwin(() => done.Set(), async () =>
            {
                onCarrier = GoScheduler.OnCarrier;
                GoTask task = wg.Waitꞏtwin();
                completed = task.GetAwaiter().IsCompleted;
                await task;
                done.Set();
            });

            Assert.IsTrue(done.Wait(TimeoutMs), "the zero-count waiter finished");
            Assert.IsTrue(onCarrier, "the twin ran on a carrier");
            Assert.IsTrue(completed, "a zero count on a carrier returns a completed task");
        }

        Row("twin-zero-count-on-a-carrier");

        // MIXED: K twinned waiters and one goroutine in the plain Wait share a WaitGroup. The waker serves both halves
        // of each (goroutine, slot) entry: the slot for a twin, the event for a plain waiter. One Done must finish all
        // K+1, so a defect in either half fails here by name.
        {
            ж<Δsync.WaitGroup> wg = NewWaitGroup();
            wg.Add(1);
            Goroutine? plain = null;
            using ManualResetEventSlim plainDone = new(false);

            Waiters run = StartWaiters(waiters, () => wg.Wait(), () => wg.Waitꞏtwin());

            Goroutine.Start(() =>
            {
                Volatile.Write(ref plain, Goroutine.Current);
                wg.Wait();
                plainDone.Set();
            });

            run.AwaitEntered();
            run.AwaitParked(WaitReason.SyncWaitGroupWait);
            Assert.IsTrue(SpinWait.SpinUntil(() => Volatile.Read(ref plain) is { State: GoroutineState.Parked, Reason: WaitReason.SyncWaitGroupWait }, TimeoutMs), "the plain waiter parked");

            wg.Done();
            run.AwaitFinished();
            Assert.IsTrue(plainDone.Wait(TimeoutMs), "the plain waiter finished on the Done that woke the twins");
        }

        Row("mixed-waiters-one-done-wakes-all");

        // RACE: the twin reads the count, registers and enters its park under ONE hold of the WaitGroup's lock, the lock
        // Add writes the count under. So a Done that reaches zero runs either before the read (the twin returns at once)
        // or after the waiter is registered (it wakes the slot); a twin that read the count, let the lock go and
        // registered later would sleep through a Done in between, and this row would stop at its deadline.
        //
        // The row must put the Done on BOTH sides of the registration, or it shows nothing. Each round records whether
        // the twin's task was incomplete at return (it registered and its park is pending: the Done came after) or
        // complete (the Done came first, or inside the few instructions between the registration and the await). The
        // delay before the Done follows the last outcome, longer after a Done that came first and shorter after one that
        // came second, so the rounds settle at the crossing on any host. Both counts are printed and both must be above
        // zero.
        {
            const int rounds = 20_000;
            int doneFirst = 0, twinFirst = 0, spin = 0, maxSpin = 0;

            for (int round = 0; round < rounds; round++)
            {
                ж<Δsync.WaitGroup> wg = NewWaitGroup();
                wg.Add(1);
                int pending = -1;
                using ManualResetEventSlim finished = new(false);

                goǃtwin(() => finished.Set(), async () =>
                {
                    GoTask task = wg.Waitꞏtwin();
                    Volatile.Write(ref pending, task.GetAwaiter().IsCompleted ? 0 : 1);
                    await task;
                    finished.Set();
                });

                Thread.SpinWait(spin);
                wg.Done();
                Assert.IsTrue(finished.Wait(TimeoutMs), $"round {round}: the waiter slept through the Done that reached zero");

                if (Volatile.Read(ref pending) == 1)
                {
                    twinFirst++;
                    spin -= Math.Max(1, spin / 8);
                    spin = Math.Max(0, spin);
                }
                else
                {
                    doneFirst++;
                    spin = Math.Min(1 << 20, spin + Math.Max(1, spin / 8));
                }

                maxSpin = Math.Max(maxSpin, spin);
            }

            Console.WriteLine($"WGTWIN READING done-race rounds={rounds} done-first={doneFirst} twin-first={twinFirst} last-spin={spin} max-spin={maxSpin}");
            Assert.IsTrue(doneFirst > 0 && twinFirst > 0, $"the Done landed on one side of the twin's registration only: done-first={doneFirst} twin-first={twinFirst}");
        }

        Row("twin-survives-a-done-race");

        // REFUSED PARK: a twin records one park in a step and the scheduler refuses a second (GoScheduler.ParkPending
        // throws). The body here keeps a park on a slot of its own pending and calls Waitꞏtwin in the same step, so the
        // twin's park is refused AFTER it registered its waiter and published its wait reason. Three things must hold.
        // The refusal reaches the caller. The goroutine's wait reason is cleared, so it no longer reads as waiting on the
        // WaitGroup. And the WaitGroup does not keep the entry: nothing will ever wake that slot, and the Done below
        // would ready a goroutine that does not wait here, which panics in Goroutine.Ready before Add clears its list and
        // sets Idle, so every other waiter would stay blocked.
        {
            ж<Δsync.WaitGroup> wg = NewWaitGroup();
            wg.Add(1);
            GoParkSlot held = new();
            Exception? refusal = null, failure = null;
            WaitReason reasonAfter = WaitReason.SyncWaitGroupWait;
            int refused = 0;
            using ManualResetEventSlim done = new(false);

            goǃtwin(() => done.Set(), async () =>
            {
                try
                {
                    GoTask pending = ParkOn(held);                  // this step's park, kept pending

                    try
                    {
                        wg.Waitꞏtwin().GetAwaiter().GetResult();     // a second park in the same step
                    }
                    catch (Exception ex)
                    {
                        refusal = ex;
                    }

                    reasonAfter = Goroutine.Current!.Reason;
                    Volatile.Write(ref refused, 1);
                    await pending;
                }
                catch (Exception ex)
                {
                    failure = ex;
                }

                done.Set();
            });

            Assert.IsTrue(SpinWait.SpinUntil(() => Volatile.Read(ref refused) == 1, TimeoutMs), "the body reached its second park");
            Assert.IsTrue(refusal is InvalidOperationException, $"the second park in one step is refused to its caller: {refusal}");
            StringAssert.Contains(refusal!.Message, "parked twice in one step");
            Assert.AreEqual(WaitReason.Zero, reasonAfter, "the refused park's scope ended: the goroutine does not read as waiting on the WaitGroup");

            wg.Done();          // nobody waits here: it readies nobody
            held.Wake();
            Assert.IsTrue(done.Wait(TimeoutMs), "the body finished on its own slot's wake");
            Assert.IsNull(failure, failure?.ToString());
        }

        Row("twin-refused-park-leaves-no-waiter");

        // REUSED, the twins (docs/phase4/DESIGN-waitgroup-reuse.md). Go's Wait re-reads the state after its wake and
        // panics when it is not zero: the count was raised again before the waiter returned. K twinned waiters park;
        // Done and Add(1) run under one hold of the WaitGroup's lock (WaitGroupReuseTests says why), so every waiter
        // reads a count of 1. Each awaited twin throws Go's panic, and none hands a P off to do it.
        {
            ж<Δsync.WaitGroup> wg = NewWaitGroup();
            wg.Add(1);
            long handoffsBefore = Counter("handoffs");

            Waiters run = StartWaiters(waiters, () => wg.Wait(), () => wg.Waitꞏtwin());
            run.AwaitEntered();
            run.AwaitParked(WaitReason.SyncWaitGroupWait);

            lock (WaitGroupReuseTests.GateOf(wg))
            {
                wg.Done();
                wg.Add(1);
            }

            IReadOnlyList<Exception> thrown = run.AwaitEnded();
            long handoffs = Counter("handoffs") - handoffsBefore;
            wg.Done();

            Console.WriteLine($"WGTWIN READING reused-twins procs={procs} waiters={waiters} panicked={thrown.Count(static ex => ex is PanicException)} handoffs={handoffs}");
            Assert.AreEqual(waiters, thrown.Count, $"every twinned waiter panicked: {waiters - thrown.Count} of {waiters} returned normally");

            foreach (Exception ex in thrown)
            {
                Assert.IsTrue(ex is PanicException, $"a twinned waiter threw {ex}");
                Assert.AreEqual(WaitGroupReuseTests.Reused, ex.Message, "a twinned waiter panicked with Go's text");
            }

            Assert.AreEqual(0, handoffs, $"the twinned waiters held carriers: {handoffs} handed their P off for {waiters} waiters");
        }

        Row("reused-twins-panic-as-go-does");

        // REUSED, mixed: K twinned waiters and one goroutine in the plain Wait on one count, the same calls under the
        // same hold. All K+1 panic: the slot wakes the twins, the lock's pulse wakes the plain waiter, and each reads
        // the state for itself.
        {
            ж<Δsync.WaitGroup> wg = NewWaitGroup();
            wg.Add(1);
            Goroutine? plain = null;
            Exception? plainThrew = null;
            using ManualResetEventSlim plainDone = new(false);

            Waiters run = StartWaiters(waiters, () => wg.Wait(), () => wg.Waitꞏtwin());

            Goroutine.Start(() =>
            {
                Volatile.Write(ref plain, Goroutine.Current);

                try
                {
                    wg.Wait();
                }
                catch (Exception ex)
                {
                    plainThrew = ex;
                }

                plainDone.Set();
            });

            run.AwaitEntered();
            run.AwaitParked(WaitReason.SyncWaitGroupWait);
            Assert.IsTrue(SpinWait.SpinUntil(() => Volatile.Read(ref plain) is { State: GoroutineState.Parked, Reason: WaitReason.SyncWaitGroupWait }, TimeoutMs), "the plain waiter parked");

            lock (WaitGroupReuseTests.GateOf(wg))
            {
                wg.Done();
                wg.Add(1);
            }

            IReadOnlyList<Exception> thrown = run.AwaitEnded();
            bool plainEnded = plainDone.Wait(2_000);
            wg.Done();      // the count back to zero: a plain waiter that slept through its generation is let go here

            Assert.AreEqual(waiters, thrown.Count(static ex => ex is PanicException && ex.Message == WaitGroupReuseTests.Reused), $"every twinned waiter panicked as Go does: {string.Join("; ", thrown.Select(static ex => ex.Message))}");
            Assert.IsTrue(plainEnded, "the plain waiter was still parked 2 s after the Done that reached zero");
            Assert.IsTrue(plainThrew is PanicException, $"the plain waiter panicked: it {(plainThrew is null ? "returned normally" : $"threw {plainThrew}")}");
            Assert.AreEqual(WaitGroupReuseTests.Reused, plainThrew!.Message, "the plain waiter panicked with Go's text");
        }

        Row("reused-mixed-waiters-all-panic");
    }

    [TestMethod]
    public void WaitGroupTwinRowsUnderTheScheduler()
    {
        string filter = $"FullyQualifiedName={typeof(WaitGroupTwinTests).FullName}.{nameof(ChildWaitGroupTwinRows)}";

        ProcessStartInfo start = new("dotnet", ["test", typeof(WaitGroupTwinTests).Assembly.Location, "--filter", filter, "--logger", "console;verbosity=detailed"])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[ChildVariable] = "mn";
        start.Environment["GO2CS_MN"] = "1";

        using Process process = Process.Start(start)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(180_000))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail("the WaitGroup twin child did not finish in 180 s");
        }

        string child = stdout.Result + stderr.Result;

        Assert.AreEqual(0, process.ExitCode, $"the child failed:\n{child}");
        Assert.IsTrue(child.Contains("Passed!", StringComparison.Ordinal) || child.Contains("Passed:", StringComparison.Ordinal), $"the child test ran and passed:\n{child}");

        foreach (string row in s_mnRows)
            Assert.IsTrue(child.Contains($"WGTWIN ROW {row} ok", StringComparison.Ordinal), $"the child ran row {row}:\n{child}");

        foreach (string line in child.Split('\n').Where(static line => line.Contains("WGTWIN READING", StringComparison.Ordinal)))
            Console.WriteLine(line.Trim());
    }

    // One counter off the scheduler's READING line, the counters GO2CS_MN_READING=1 prints at exit.
    private static long Counter(string name) =>
        Counter(GoScheduler.Reading(), name);

    // A counter's name is matched whole: `handoffs` is not the tail of `idle-handoffs`.
    private static long Counter(string reading, string name)
    {
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(reading, $@"(?<![\w-]){name}=(\d+)");
        Assert.IsTrue(match.Success, $"the scheduler's reading carries {name}");
        return long.Parse(match.Groups[1].Value);
    }

    // The scheduler's counters a carrier row reads, off one READING line. Monitor is every P the monitor gave out on
    // its own: an injection past a long step, and a retake from a carrier it read or sensed as blocked.
    private readonly record struct Counts(long Handoffs, long Injections, long Retakes, long Sensed)
    {
        internal long Monitor => Injections + Retakes + Sensed;

        internal static Counts Read()
        {
            string reading = GoScheduler.Reading();
            return new Counts(Counter(reading, "handoffs"), Counter(reading, "injections"), Counter(reading, "retakes"), Counter(reading, "sensed"));
        }

        public static Counts operator -(Counts after, Counts before) =>
            new(after.Handoffs - before.Handoffs, after.Injections - before.Injections, after.Retakes - before.Retakes, after.Sensed - before.Sensed);

        public override string ToString() =>
            $"handoffs={Handoffs} injections={Injections} retakes={Retakes} sensed={Sensed}";
    }

    // A park on a caller's own slot, with no wait reason: the refused-park row's first park of the step.
    private static async GoTask ParkOn(GoParkSlot slot) =>
        await slot;

    private static void Row(string name) =>
        Console.WriteLine($"WGTWIN ROW {name} ok");

    // K twinned goroutines, each recording itself and then waiting through the given body or twin.
    private sealed class Waiters
    {
        private readonly int m_count;
        private readonly Goroutine?[] m_goroutines;
        private int m_entered;
        private int m_finished;
        private readonly List<Exception> m_failures = [];

        internal Waiters(int count)
        {
            m_count = count;
            m_goroutines = new Goroutine?[count];
        }

        internal void Enter(int index)
        {
            Volatile.Write(ref m_goroutines[index], Goroutine.Current);
            Interlocked.Increment(ref m_entered);
        }

        internal void Finish(Exception? failure)
        {
            if (failure is not null)
            {
                lock (m_failures)
                    m_failures.Add(failure);
            }

            Interlocked.Increment(ref m_finished);
        }

        internal void AwaitEntered() =>
            Assert.IsTrue(SpinWait.SpinUntil(() => Volatile.Read(ref m_entered) == m_count, TimeoutMs), $"{Volatile.Read(ref m_entered)} of {m_count} waiters entered");

        // Every waiter parked with the given reason, or the row fails by name at the deadline.
        internal void AwaitParked(WaitReason reason) =>
            Assert.IsTrue(SpinWait.SpinUntil(() => m_goroutines.All(g => Volatile.Read(ref g) is { State: GoroutineState.Parked } parked && parked.Reason == reason), TimeoutMs),
                $"{m_goroutines.Count(g => g is { State: GoroutineState.Parked } parked && parked.Reason == reason)} of {m_count} waiters parked with {reason}: " +
                string.Join(", ", m_goroutines.Select(static g => g is null ? "unstarted" : $"{g.State}/{g.Reason}")));

        // Every waiter finished; what each threw, for a row whose waiters are meant to throw.
        internal IReadOnlyList<Exception> AwaitEnded()
        {
            Assert.IsTrue(SpinWait.SpinUntil(() => Volatile.Read(ref m_finished) == m_count, TimeoutMs), $"{Volatile.Read(ref m_finished)} of {m_count} waiters finished");

            lock (m_failures)
                return [.. m_failures];
        }

        internal void AwaitFinished()
        {
            Assert.IsTrue(SpinWait.SpinUntil(() => Volatile.Read(ref m_finished) == m_count, TimeoutMs), $"{Volatile.Read(ref m_finished)} of {m_count} waiters finished");

            lock (m_failures)
                Assert.AreEqual(0, m_failures.Count, string.Join(Environment.NewLine, m_failures));
        }
    }

    private static Waiters StartWaiters(int count, Action wait, Func<GoTask> waitTwin)
    {
        Waiters run = new(count);

        for (int i = 0; i < count; i++)
        {
            int index = i;

            goǃtwin(() =>
            {
                run.Enter(index);
                wait();
                run.Finish(null);
            }, async () =>
            {
                Exception? failure = null;

                try
                {
                    run.Enter(index);
                    await waitTwin();
                }
                catch (Exception ex)
                {
                    failure = ex;
                }

                run.Finish(failure);
            });
        }

        return run;
    }
}
