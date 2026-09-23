using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using Δruntime = go.runtime_package;
using Δsync = go.sync_package;

namespace GolibTests;

/// <summary>
/// The READY half of golib's park seam (DESIGN-gopark-goready-synctest.md, S1a): a goroutine is
/// readied by its WAKER, before the waker signals the primitive, and every park site that has an
/// accounted waker parks in Go's commit order (park under the lock that publishes the waiter, then
/// unlock, then wait).
/// </summary>
/// <remarks>
/// <para>
/// FUNCTIONAL arms, one per wake site: a recorder wrapped around <see cref="Goroutine.ReadyTransition"/>
/// sees the parked goroutine readied, with its own wait reason, BEFORE the parker's thread has
/// resumed, and the runtime's g reads <c>_Grunnable</c> at that moment (Go's ready, on the waker's side).
/// </para>
/// <para>
/// STRESS arms, one per reordered site (COORD ruling 2026-09-22): a waker and a parker race for many
/// iterations. <see cref="Goroutine.Ready"/> panics by name on a goroutine that is not parked, so a
/// site that published its waiter before entering its park would let a waker reach a still-running
/// goroutine and fail here, by name.
/// </para>
/// <para>
/// MSTest runs this assembly serially (no [Parallelize]), which is what makes swapping the global
/// ReadyTransition slot for the length of one test safe; every swap is restored in <c>finally</c>.
/// </para>
/// </remarks>
[TestClass]
public class GoroutineReadyTests
{
    private const int TimeoutMs = 30000;

    // The runtime installs ParkTransition / ReadyTransition from a module initializer, which runs only
    // once the runtime assembly is first touched. A goroutine that parks before then mints no g, and
    // GoStatusOf can never read it waiting: an arm that happened to run first in the assembly measured
    // the load order, not the seam (found on this file's own first run). Touch it, then assert it.
    [TestInitialize]
    public void RuntimeHooksAreInstalled()
    {
        _ = Δruntime.GoStatusWaiting;
        Assert.IsNotNull(Goroutine.ParkTransition, "the runtime's park transition is not installed");
        Assert.IsNotNull(Goroutine.ReadyTransition, "the runtime's ready transition is not installed");
    }

    private sealed class Parker
    {
        internal volatile Goroutine? G;
        internal volatile bool Resumed;
        internal volatile Exception? Failure;
        internal readonly ManualResetEventSlim Done = new(false);
    }

    private readonly record struct ReadyEvent(Goroutine Target, WaitReason Reason, bool ResumedAtReady, uint StatusAfter);

    // Wraps the installed ReadyTransition (the runtime's) and records each ready as it happens.
    private sealed class ReadyRecorder : IDisposable
    {
        private readonly Action<Goroutine, WaitReason>? m_previous = Goroutine.ReadyTransition;
        internal readonly ConcurrentQueue<ReadyEvent> Seen = new();

        internal ReadyRecorder(Func<Goroutine, bool> resumed)
        {
            Goroutine.ReadyTransition = (target, reason) =>
            {
                bool resumedAtReady = resumed(target);
                m_previous?.Invoke(target, reason);
                Seen.Enqueue(new ReadyEvent(target, reason, resumedAtReady, Δruntime.GoStatusOf(target)));
            };
        }

        public void Dispose() => Goroutine.ReadyTransition = m_previous;
    }

    private static Parker StartParker(Action block)
    {
        Parker p = new();

        Goroutine.Start(() =>
        {
            try
            {
                p.G = Goroutine.Current;
                block();
                p.Resumed = true;
            }
            catch (Exception ex)
            {
                p.Failure = ex;
            }
            finally
            {
                p.Done.Set();
            }
        });

        return p;
    }

    private static void AwaitParked(Parker p)
    {
        long deadline = Environment.TickCount64 + TimeoutMs;

        // BOTH: the runtime's g is _Gwaiting AND golib counts the goroutine parked. Park moves the g
        // first and publishes golib's state second, so polling the g alone let an arm Ready a goroutine
        // golib did not yet count parked -- a one-in-five flake of ASecondReady..., found by S1c's
        // repeated runs.
        while (p.G is not { } g || Δruntime.GoStatusOf(g) != Δruntime.GoStatusWaiting || !g.IsParked)
        {
            if (p.Failure is not null)
                Assert.Fail($"the parker failed before parking: {p.Failure}");

            if (Environment.TickCount64 > deadline)
                Assert.Fail("the parker never reached _Gwaiting and golib's parked state");

            Thread.Sleep(1);
        }
    }

    private static void AwaitDone(Parker p)
    {
        Assert.IsTrue(p.Done.Wait(TimeoutMs), "the parker never finished");
        Assert.IsNull(p.Failure, $"the parker failed: {p.Failure}");
        Assert.IsTrue(p.Resumed, "the parker did not resume");
    }

    // One parker, one wake: exactly one ready, for that goroutine, for that reason, recorded BEFORE its
    // thread resumed, with its g _Grunnable at that moment.
    private static void AssertReadiedByWaker(Action block, Action wake, WaitReason reason)
    {
        Parker p = StartParker(block);
        AwaitParked(p);

        using (ReadyRecorder recorder = new(target => ReferenceEquals(target, p.G) && p.Resumed))
        {
            wake();
            AwaitDone(p);

            ReadyEvent[] seen = recorder.Seen.ToArray();
            Assert.AreEqual(1, seen.Length, $"the {WaitReasons.Text(reason)} waker readied {seen.Length} goroutines; the design readies exactly the one it wakes, before it signals");
            Assert.AreSame(p.G, seen[0].Target, "the waker readied a different goroutine than the one it woke");
            Assert.AreEqual(reason, seen[0].Reason, "readied under the wrong reason");
            Assert.IsFalse(seen[0].ResumedAtReady, "the goroutine had already resumed when it was readied: the wake was accounted by the wakee, not the waker");
            Assert.AreEqual(Δruntime.GoStatusRunnable, seen[0].StatusAfter, "the waker's ready did not move the g to _Grunnable");
        }
    }

    // ---- the API ------------------------------------------------------------------------------

    [TestMethod]
    public void ReadyOfARunningGoroutinePanicsByName()
    {
        Parker p = StartParker(() => Goroutine.Ready(Goroutine.Current));
        Assert.IsTrue(p.Done.Wait(TimeoutMs));
        Assert.IsInstanceOfType(p.Failure, typeof(PanicException));
        StringAssert.Contains(p.Failure!.Message, "which is not parked");
    }

    [TestMethod]
    public void ASecondReadyOfTheSameParkPanicsByName()
    {
        using ManualResetEventSlim gate = new(false);
        Parker p = StartParker(() =>
        {
            using (Goroutine.Park(WaitReason.ChanReceive))
                gate.Wait();
        });

        AwaitParked(p);
        Goroutine.Ready(p.G);

        PanicException second = Assert.ThrowsException<PanicException>(() => Goroutine.Ready(p.G));
        StringAssert.Contains(second.Message, "which is already readied (chan receive)");

        gate.Set();
        AwaitDone(p);
    }

    [TestMethod]
    public void ReadyOfNoGoroutineIsANoOp() => Goroutine.Ready(null);

    [TestMethod]
    public void AnUnreadiedParkStillReadiesItselfAtDispose()
    {
        // A timeout, a real deadline, a primitive whose waker does not call Ready: the scope ends
        // Parked and does both halves itself, as before the ready side existed.
        using ManualResetEventSlim gate = new(false);
        Parker p = StartParker(() =>
        {
            using (Goroutine.Park(WaitReason.Sleep))
                gate.Wait();
        });

        AwaitParked(p);
        gate.Set();
        AwaitDone(p);
    }

    // ---- one functional arm per wake site ---------------------------------------------------------

    [TestMethod]
    public void AChannelReceiveIsReadiedByItsSender()
    {
        channel<int> ch = new(0);
        AssertReadiedByWaker(() => ch.Receive(), () => ch.Send(1), WaitReason.ChanReceive);
    }

    [TestMethod]
    public void AChannelSendIsReadiedByItsReceiver()
    {
        channel<int> ch = new(0);
        AssertReadiedByWaker(() => ch.Send(1), () => ch.Receive(), WaitReason.ChanSend);
    }

    [TestMethod]
    public void AChannelReceiveIsReadiedByClose()
    {
        channel<int> ch = new(0);
        AssertReadiedByWaker(() => ch.Receive(), () => close(ch), WaitReason.ChanReceive);
    }

    [TestMethod]
    public void ASelectIsReadiedByTheOperationThatClaimsIt()
    {
        channel<int> a = new(0);
        channel<int> b = new(0);
        AssertReadiedByWaker(() => select(a.Sending(1), b.Sending(2)), () => b.Receive(), WaitReason.Select);
    }

    [TestMethod]
    public void ASemaphoreAcquireIsReadiedByItsRelease()
    {
        ж<uint32> s = new StandardBox<uint32>(0);
        AssertReadiedByWaker(() => RuntimeSemaphore.Acquire(s, WaitReason.Semacquire), () => RuntimeSemaphore.Release(s, false), WaitReason.Semacquire);
    }

    [TestMethod]
    public void ACondWaitIsReadiedBySignal()
    {
        ж<Δsync.Mutex> mu = @new<Δsync.Mutex>();
        ж<Δsync.Cond> c = Δsync.NewCond(new Δsync.MutexжLocker(mu));

        AssertReadiedByWaker(
            () => { mu.Lock(); c.Wait(); mu.Unlock(); },
            () => { mu.Lock(); c.Signal(); mu.Unlock(); },
            WaitReason.SyncCondWait);
    }

    [TestMethod]
    public void EveryCondWaiterIsReadiedByBroadcast()
    {
        ж<Δsync.Mutex> mu = @new<Δsync.Mutex>();
        ж<Δsync.Cond> c = Δsync.NewCond(new Δsync.MutexжLocker(mu));
        void block() { mu.Lock(); c.Wait(); mu.Unlock(); }

        Parker p1 = StartParker(block);
        Parker p2 = StartParker(block);
        AwaitParked(p1);
        AwaitParked(p2);

        using ReadyRecorder recorder = new(target => (ReferenceEquals(target, p1.G) && p1.Resumed) || (ReferenceEquals(target, p2.G) && p2.Resumed));

        mu.Lock();
        c.Broadcast();
        mu.Unlock();

        AwaitDone(p1);
        AwaitDone(p2);

        ReadyEvent[] seen = recorder.Seen.ToArray();
        Assert.AreEqual(2, seen.Length, "Broadcast readies every waiter it wakes");

        foreach (ReadyEvent e in seen)
        {
            Assert.IsTrue(ReferenceEquals(e.Target, p1.G) || ReferenceEquals(e.Target, p2.G));
            Assert.IsFalse(e.ResumedAtReady, "readied after it resumed");
            Assert.AreEqual(Δruntime.GoStatusRunnable, e.StatusAfter);
        }

        Assert.AreNotSame(seen[0].Target, seen[1].Target, "one waiter readied twice");
    }

    // ---- one stress arm per reordered site -------------------------------------------------------

    // Two goroutines race a waker against a parker `iterations` times. Any panic (Ready of a goroutine
    // that is not yet parked is the one this exists to catch) is recorded and fails the arm by name; a
    // waker that panics before signalling strands its peer, so a deadline also fails it.
    private static void Race(int iterations, Action<int> left, Action<int> right)
    {
        ConcurrentQueue<Exception> failures = new();
        using CountdownEvent done = new(2);

        void run(Action<int> body) => Goroutine.Start(() =>
        {
            try
            {
                for (int i = 0; i < iterations; i++)
                    body(i);
            }
            catch (Exception ex)
            {
                failures.Enqueue(ex);
            }
            finally
            {
                done.Signal();
            }
        });

        run(left);
        run(right);

        bool finished = done.Wait(TimeoutMs * 4);

        Assert.IsTrue(failures.IsEmpty, $"the race failed: {(failures.TryPeek(out Exception? first) ? first.Message : "")}");
        Assert.IsTrue(finished, "the race never finished (a stranded peer: a waker failed before it signalled)");
    }

    [TestMethod]
    public void ChannelSendAndReceiveSurviveAWakerParkerRace()
    {
        channel<int> ping = new(0);
        channel<int> pong = new(0);
        Race(20000, i => { ping.Send(i); pong.Receive(); }, i => { ping.Receive(); pong.Send(i); });
    }

    [TestMethod]
    public void SelectSurvivesAWakerParkerRace()
    {
        channel<int> a = new(0);
        channel<int> b = new(0);
        Race(20000, i => select(a.Sending(i), b.Sending(i)), i => { if ((i & 1) == 0) a.Receive(); else b.Receive(); });
    }

    [TestMethod]
    public void TheSemaphoreSurvivesAWakerParkerRace()
    {
        ж<uint32> s1 = new StandardBox<uint32>(0);
        ж<uint32> s2 = new StandardBox<uint32>(0);
        Race(20000,
            _ => { RuntimeSemaphore.Release(s1, false); RuntimeSemaphore.Acquire(s2, WaitReason.Semacquire); },
            _ => { RuntimeSemaphore.Acquire(s1, WaitReason.Semacquire); RuntimeSemaphore.Release(s2, false); });
    }

    // ---- S1a2: WaitGroup and Coro ------------------------------------------------------------------

    [TestMethod]
    public void AWaitGroupWaitIsReadiedByTheDoneThatReachesZero()
    {
        ж<Δsync.WaitGroup> wg = @new<Δsync.WaitGroup>();
        wg.Add(1);
        AssertReadiedByWaker(() => wg.Wait(), () => wg.Done(), WaitReason.SyncWaitGroupWait);
    }

    [TestMethod]
    public void EveryWaitGroupWaiterIsReadiedByTheDoneThatReachesZero()
    {
        ж<Δsync.WaitGroup> wg = @new<Δsync.WaitGroup>();
        wg.Add(1);

        Parker p1 = StartParker(() => wg.Wait());
        Parker p2 = StartParker(() => wg.Wait());
        AwaitParked(p1);
        AwaitParked(p2);

        using ReadyRecorder recorder = new(target => (ReferenceEquals(target, p1.G) && p1.Resumed) || (ReferenceEquals(target, p2.G) && p2.Resumed));

        wg.Done();
        AwaitDone(p1);
        AwaitDone(p2);

        ReadyEvent[] seen = recorder.Seen.ToArray();
        Assert.AreEqual(2, seen.Length, "the Done that reaches zero readies every waiter");
        Assert.AreNotSame(seen[0].Target, seen[1].Target, "one waiter readied twice");

        foreach (ReadyEvent e in seen)
        {
            Assert.AreEqual(WaitReason.SyncWaitGroupWait, e.Reason);
            Assert.IsFalse(e.ResumedAtReady, "readied after it resumed");
        }
    }

    [TestMethod]
    public void AWaitGroupWaitAtZeroReturnsWithoutParking()
    {
        // Go: `if v == 0 { return }` -- no park, so nothing to ready and no transition.
        ж<Δsync.WaitGroup> wg = @new<Δsync.WaitGroup>();
        ConcurrentQueue<WaitReason> parks = new();
        Action<WaitReason, bool>? previous = Goroutine.ParkTransition;
        Goroutine.ParkTransition = (reason, entering) => { if (entering) parks.Enqueue(reason); previous?.Invoke(reason, entering); };

        try
        {
            Parker p = StartParker(() => wg.Wait());
            AwaitDone(p);
        }
        finally
        {
            Goroutine.ParkTransition = previous;
        }

        Assert.AreEqual(-1, Array.IndexOf(parks.ToArray(), WaitReason.SyncWaitGroupWait), "Wait on a zero counter parked");
    }

    private sealed class Flag
    {
        private volatile bool m_value;
        internal bool Value { get => m_value; set => m_value = value; }
    }

    [TestMethod]
    public void ACoroutineSwitchReadiesItsPeer()
    {
        ConcurrentQueue<ReadyEvent> seen = new();
        Flag bodyRan = new();
        Coro coro = Coro.Start(() => bodyRan.Value = true);
        Goroutine? resumer = null;

        Action<Goroutine, WaitReason>? previous = Goroutine.ReadyTransition;
        Goroutine.ReadyTransition = (target, reason) =>
        {
            bool resumed = !ReferenceEquals(target, resumer) && bodyRan.Value;
            previous?.Invoke(target, reason);
            seen.Enqueue(new ReadyEvent(target, reason, resumed, Δruntime.GoStatusOf(target)));
        };

        try
        {
            Parker p = StartParker(() => { resumer = Goroutine.Current; coro.Switch(); });
            AwaitDone(p);
        }
        finally
        {
            Goroutine.ReadyTransition = previous;
        }

        ReadyEvent[] events = seen.ToArray();
        Assert.AreEqual(2, events.Length, "a first switch readies the coro goroutine, and its exit readies the resumer");
        Assert.AreNotSame(resumer, events[0].Target, "the first ready is the coro goroutine");
        Assert.AreEqual(WaitReason.Coroutine, events[0].Reason, "a coro goroutine parks as Go's 'coroutine'");
        Assert.IsFalse(events[0].ResumedAtReady, "the coro body ran before its goroutine was readied");
        Assert.AreEqual(Δruntime.GoStatusRunnable, events[0].StatusAfter);
        Assert.AreSame(resumer, events[1].Target, "the coro's exit readies the goroutine that resumed it");
        Assert.AreEqual(WaitReason.Coroutine, events[1].Reason);
        Assert.IsTrue(bodyRan.Value);
    }

    [TestMethod]
    public void AWaitGroupSurvivesAWakerParkerRace()
    {
        ж<Δsync.WaitGroup> wg = @new<Δsync.WaitGroup>();
        channel<int> turn = new(0);
        channel<int> back = new(0);

        // Each round: one side Adds and hands the turn over, then Dones while the other Waits -- and
        // does not Add again until that Wait has RETURNED (`back`). Go forbids reusing a WaitGroup
        // before a previous Wait has returned ("WaitGroup is reused before previous Wait has
        // returned"); the first cut of this arm did exactly that and hung, measuring the test's
        // contract violation rather than the seam.
        Race(20000,
            _ => { wg.Add(1); turn.Send(0); wg.Done(); back.Receive(); },
            _ => { turn.Receive(); wg.Wait(); back.Send(0); });
    }

    [TestMethod]
    public void ACoroutineSurvivesAWakerParkerRace()
    {
        const int switches = 20000;
        ConcurrentQueue<Exception> failures = new();
        Coro? coro = null;

        coro = Coro.Start(() =>
        {
            try
            {
                for (int i = 0; i < switches; i++)
                    coro!.Switch();
            }
            catch (Exception ex)
            {
                failures.Enqueue(ex);
            }
        });

        Parker p = StartParker(() =>
        {
            for (int i = 0; i <= switches; i++)
                coro.Switch();
        });

        Assert.IsTrue(p.Done.Wait(TimeoutMs * 4), "the switch race never finished");
        Assert.IsNull(p.Failure, $"the resumer failed: {p.Failure?.Message}");
        Assert.IsTrue(failures.IsEmpty, $"the coro side failed: {(failures.TryPeek(out Exception? first) ? first.Message : "")}");
        Assert.IsTrue(coro.Exited);
    }

    [TestMethod]
    public void TheNotifyListSurvivesAWakerParkerRace()
    {
        ж<Δsync.Mutex> mu = @new<Δsync.Mutex>();
        ж<Δsync.Cond> c = Δsync.NewCond(new Δsync.MutexжLocker(mu));
        int turn = 0;

        void take(int mine)
        {
            mu.Lock();

            while (turn != mine)
                c.Wait();

            turn = 1 - mine;
            c.Signal();
            mu.Unlock();
        }

        Race(10000, _ => take(0), _ => take(1));
    }
}
