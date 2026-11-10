using System;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards class F's sync.Mutex half (docs/phase4/DESIGN-managed-profiling.md I4, and the Mutex slice of
/// I3): a CONTENDED hand-owned <c>sync.Mutex</c> records a mutex-profile event on the unlocker (Go's
/// <c>semrelease1</c>) and a block-profile event on the waiter (Go's <c>semacquire1</c>), each only while
/// its rate is above zero -- and an UNCONTENDED Lock records nothing, because Go's fast path
/// (<c>cansemacquire</c>) returns before any profiling. Red against the hand-own that recorded nothing at
/// all (runtime/pprof TestMutexBlockFullAggregation: "did not see any samples in mutex profile").
/// </summary>
[TestClass]
public class SyncMutexProfileTests
{
    private sealed class Shared
    {
        public sync_package.Mutex M;
    }

    private static int64 Total(bool mutex)
    {
        var (n, _) = mutex ? MutexProfile(default) : BlockProfile(default);
        var records = new slice<BlockProfileRecord>((int)n + 16);
        var (m, ok) = mutex ? MutexProfile(records) : BlockProfile(records);
        Assert.IsTrue(ok, "the profile must fit a buffer sized from its own count");
        int64 count = 0;
        for (int i = 0; i < (int)m; i++)
            count += records[i].Count;
        return count;
    }

    // ONE contended handoff, deterministically: a holder takes the lock and keeps it until this thread
    // is PARKED on it (see Parked), then releases it to this thread. The wait is bounded, and a waiter that
    // never parked is named after the handoff. (Two free-running workers are not a fixture: one can finish
    // before the other starts, and SemaphoreSlim lets a releasing thread take the lock straight back, so
    // how many handoffs find a waiter depends on thread start-up.)
    private static void ContendOnce(Shared shared)
    {
        using var held = new ManualResetEventSlim();
        using var ready = new ManualResetEventSlim();
        Thread waiter = Thread.CurrentThread;
        object? gate = null;
        int inLock = 0;
        bool parked = false;
        var holder = new Thread(() =>
        {
            shared.M.Lock();
            held.Set();
            ready.Wait();
            var waited = System.Diagnostics.Stopwatch.StartNew();
            while (!(parked = Parked(gate!, waiter, ref inLock)) && waited.Elapsed < ParkBound)
                Thread.Yield();
            shared.M.Unlock();
        }) { IsBackground = true };
        holder.Start();
        held.Wait();
        gate = GateOf(shared);
        PrecreateStamps(gate);
        ready.Set();
        Volatile.Write(ref inLock, 1);
        shared.M.Lock();
        Volatile.Write(ref inLock, 0);
        shared.M.Unlock();
        holder.Join();
        Assert.IsTrue(parked, $"ContendOnce: this thread was not seen parked in Lock within {ParkBound}, so the holder released an unwaited lock");
    }

    private static readonly TimeSpan ParkBound = TimeSpan.FromMinutes(1);

    private static readonly FieldInfo? s_gateField =
        typeof(sync_package.Mutex).GetField("gate", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo? s_waitStampsField =
        typeof(sync_package).GetField("s_waitStamps", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo? s_semaphoreWaitCount =
        typeof(SemaphoreSlim).GetField("m_waitCount", BindingFlags.NonPublic | BindingFlags.Instance);

    // The mutex's gate, once a Lock has created it.
    private static object GateOf(Shared shared) => GateOf(shared.M);

    private static object GateOf(sync_package.Mutex m)
    {
        Assert.IsNotNull(s_gateField, "the hand-own's Mutex.gate field");
        Assert.IsNotNull(s_semaphoreWaitCount, "SemaphoreSlim's m_waitCount field (re-read at each .NET major hop)");
        object? gate = s_gateField.GetValue(m);
        Assert.IsNotNull(gate, "a mutex that has been locked has its gate");
        return gate;
    }

    // The gate's stamp record, created before any waiter needs it, so Lock's GetValue finds it without the
    // ConditionalWeakTable's add path and its lock.
    private static void PrecreateStamps(object gate)
    {
        Assert.IsNotNull(s_waitStampsField, "the hand-own's s_waitStamps table");
        object table = s_waitStampsField.GetValue(null)!;
        table.GetType().GetMethod("GetOrCreateValue")!.Invoke(table, [gate]);
    }

    // A thread is PARKED in Lock when its in-Lock flag is set, the gate's SemaphoreSlim counts a thread
    // inside its monitor section (m_waitCount, read first), and the thread is then in WaitSleepJoin. Read by
    // the holder, which holds the gate and touches nothing else, about a gate whose stamp record exists
    // (PrecreateStamps): the thread's try has failed, and the only place in Lock where it can wait after
    // that is inside gate.Wait(), past its stamp. WaitSleepJoin alone is not enough: a thread blocked on the
    // gate's monitor INSIDE the Wait(0) try, before the try fails, is in WaitSleepJoin too, and m_waitCount
    // counts only a thread that has entered the monitor. (Measured: WaitSleepJoin alone read 13 of 20 idle
    // 300-episode runs red on unchanged golib, mostly at an episode's first Lock.) Plain threads, so Lock's
    // park scope is inert.
    private static bool Parked(object gate, Thread thread, ref int inLock) =>
        Volatile.Read(ref inLock) == 1 &&
        (int)s_semaphoreWaitCount!.GetValue(gate)! >= 1 &&
        (thread.ThreadState & ThreadState.WaitSleepJoin) != 0;

    private static void WithRates(int mutexFraction, nint blockRate, System.Action body)
    {
        nint previous = SetMutexProfileFraction(mutexFraction);
        SetBlockProfileRate(blockRate);
        try
        {
            body();
        }
        finally
        {
            SetMutexProfileFraction(previous);
            SetBlockProfileRate(0);
        }
    }

    [TestMethod]
    public void AContendedMutexRecordsOneMutexAndOneBlockEvent()
    {
        WithRates(1, 1, () =>
        {
            int64 mutexBefore = Total(mutex: true), blockBefore = Total(mutex: false);
            ContendOnce(new Shared());
            // Fraction 1: the event is always sampled and adds count += rate = 1 (Go's saveblockevent);
            // block rate 1: a wait of at least one tick adds count + 1.
            Assert.AreEqual(mutexBefore + 1, Total(mutex: true), "the handoff's Unlock must record exactly one mutex event");
            Assert.AreEqual(blockBefore + 1, Total(mutex: false), "the waiting Lock must record exactly one block event");
        });
    }

    [TestMethod]
    public void AnUncontendedMutexRecordsNothing()
    {
        WithRates(1, 1, () =>
        {
            int64 mutexBefore = Total(mutex: true), blockBefore = Total(mutex: false);
            var shared = new Shared();
            for (int i = 0; i < 100; i++)
            {
                shared.M.Lock();
                shared.M.Unlock();
            }
            Assert.AreEqual(mutexBefore, Total(mutex: true), "an uncontended Unlock hands off to nobody: no mutex event");
            Assert.AreEqual(blockBefore, Total(mutex: false), "an uncontended Lock never blocks: no block event");
        });
    }

    [TestMethod]
    public void AtRateZeroContentionRecordsNothing()
    {
        WithRates(0, 0, () =>
        {
            int64 mutexBefore = Total(mutex: true), blockBefore = Total(mutex: false);
            ContendOnce(new Shared());
            Assert.AreEqual(mutexBefore, Total(mutex: true), "at fraction 0 the mutex profile records nothing");
            Assert.AreEqual(blockBefore, Total(mutex: false), "at rate 0 the block profile records nothing");
        });
    }

    // runtime/pprof TestMutexBlockFullAggregation's own shape: two workers, each re-locking right after its
    // own Unlock and holding across a 1 ms sleep. Every Lock after the first comes nanoseconds after the
    // locker's own Release, so a CurrentCount snapshot reads the gate free (the woken waiter has not
    // consumed the release yet) and the Lock goes unstamped, then blocks anyway. Probed: 0-2 of 200 Locks
    // classed contended, so the test recorded nothing in 3 of 8 solo runs. Go's semacquire1 records every
    // wait whose fast path failed; a waiter blocked through most of the other worker's holds must make most
    // of those Unlocks mutex events. The bound is loose on purpose (a tenth of the Locks): the defect reads
    // about one in a hundred.
    [TestMethod]
    public void AWorkerThatReLocksRightAfterUnlockStillRecordsItsContention()
    {
        const int iterations = 50;
        WithRates(1, 1, () =>
        {
            int64 mutexBefore = Total(mutex: true), blockBefore = Total(mutex: false);
            var shared = new Shared();
            using var start = new ManualResetEventSlim();
            Thread[] workers = new Thread[2];
            for (int w = 0; w < workers.Length; w++)
            {
                workers[w] = new Thread(() =>
                {
                    start.Wait();
                    for (int i = 0; i < iterations; i++)
                    {
                        shared.M.Lock();
                        Thread.Sleep(1);
                        shared.M.Unlock();
                    }
                });
                workers[w].Start();
            }
            start.Set();
            foreach (Thread worker in workers)
                worker.Join();

            int64 mutexEvents = Total(mutex: true) - mutexBefore, blockEvents = Total(mutex: false) - blockBefore;
            int locks = iterations * workers.Length;
            Assert.IsTrue(mutexEvents >= locks / 10, $"{mutexEvents} mutex events for {locks} Locks by two workers that contend on every hold");
            Assert.IsTrue(blockEvents >= 1, $"{blockEvents} block events for {locks} contended Locks");
        });
    }

    // R's review arm: a stamped waiter that acquires WITHOUT a handoff (the snapshot race the hand-own
    // names: it saw the gate held, stamped, and the gate came free before it blocked) must not leave its
    // stamp behind. Otherwise the next contention episode's handoff charges the whole idle gap since that
    // stale stamp as one mutex event. Reached by reflection: WaitStamps is private to the hand-own.
    [TestMethod]
    public void AStampThatAcquiresWithoutAHandoffDoesNotLeakIntoTheNextEpisode()
    {
        Type type = typeof(sync_package).GetNestedType("WaitStamps", BindingFlags.NonPublic)!;
        Assert.IsNotNull(type, "the hand-own's WaitStamps type");
        object stamps = Activator.CreateInstance(type, nonPublic: true)!;
        MethodInfo enqueue = type.GetMethod("Enqueue", BindingFlags.NonPublic | BindingFlags.Instance)!;
        MethodInfo acquired = type.GetMethod("Acquired", BindingFlags.NonPublic | BindingFlags.Instance)!;
        MethodInfo tryHandoff = type.GetMethod("TryHandoff", BindingFlags.NonPublic | BindingFlags.Instance)!;

        const long stale = 1_000_000_000_000L;
        long now = GoCputicks();
        enqueue.Invoke(stamps, [now - stale]);   // episode 1: stamped, then acquired with no handoff
        acquired.Invoke(stamps, null);
        enqueue.Invoke(stamps, [GoCputicks()]);  // episode 2: a fresh contended wait
        object[] args = [0L];
        bool handed = (bool)tryHandoff.Invoke(stamps, args)!;
        acquired.Invoke(stamps, null);           // balance the process-wide stamped-waiter count

        Assert.IsTrue(handed, "episode 2's waiter is stamped, so its handoff charges it");
        Assert.IsTrue((long)args[0] < stale / 2, $"the handoff charged {args[0]} cycles: episode 1's stale stamp leaked into episode 2");
    }

    // A record's frame names read the way Go's own test reads them (runtime/pprof getProfileStacks):
    // FuncForPC(pc - 1), because BlockProfile and MutexProfile hand back expanded RETURN PCs.
    private static System.Collections.Generic.List<string> FrameNames(slice<uintptr> stack)
    {
        var names = new System.Collections.Generic.List<string>();
        foreach (var (_, pc) in range(stack))
            names.Add((string)FuncForPC(pc - 1).Name());
        return names;
    }

    // A record's stack, read through a by-value copy: Stack is a pointer-receiver method of the embedded
    // StackRecord, and a foreach iteration variable's embed FIELD cannot be passed by ref (CS1655) -- Go's
    // `r.Stack()` on a range variable works on the copy too.
    private static slice<uintptr> StackOf(BlockProfileRecord record) => record.StackRecord.Stack();

    private static slice<BlockProfileRecord> Records(bool mutex)
    {
        var (n, _) = mutex ? MutexProfile(default) : BlockProfile(default);
        // The element factory allocates each record's Stack0 array, as the converted Go test does;
        // default records have none, and the copy into them writes no frame.
        var records = new slice<BlockProfileRecord>((int)n + 16, static () => new BlockProfileRecord(nil));
        var (m, ok) = mutex ? MutexProfile(records) : BlockProfile(records);
        Assert.IsTrue(ok, "the profile must fit a buffer sized from its own count");
        return records[..(int)m];
    }

    // The count of the profile's records whose TOP frame is the named function.
    private static int64 TotalAt(bool mutex, string function)
    {
        int64 count = 0;
        foreach (var (_, record) in range(Records(mutex)))
        {
            var names = FrameNames(StackOf(record));
            if (names.Count > 0 && names[0] == function)
                count += record.Count;
        }
        return count;
    }

    // Every record's first two frames, for a failure message.
    private static string TopFrames(bool mutex)
    {
        var all = new System.Collections.Generic.List<string>();
        foreach (var (_, record) in range(Records(mutex)))
        {
            var names = FrameNames(StackOf(record));
            all.Add($"[{(names.Count > 0 ? names[0] : "")} < {(names.Count > 1 ? names[1] : "-")} x{record.Count}]");
        }
        return string.Join(" ", all);
    }

    // Each event's TOP frame is Go's: the mutex event is recorded on the unlocker's stack at
    // sync.(*Mutex).Unlock (semrelease1's caller), and the block event on the waiter's at
    // sync.(*Mutex).Lock. Both depend on Lock and Unlock keeping their own frames (NoInlining): an
    // inlined Lock would move the block event's top frame to Lock's caller.
    [TestMethod]
    public void EachEventsTopFrameIsGosLockOrUnlock()
    {
        WithRates(1, 1, () =>
        {
            int64 unlockBefore = TotalAt(mutex: true, "sync.(*Mutex).Unlock"), lockBefore = TotalAt(mutex: false, "sync.(*Mutex).Lock");
            ContendOnce(new Shared());
            Assert.AreEqual(unlockBefore + 1, TotalAt(mutex: true, "sync.(*Mutex).Unlock"), $"the handoff's mutex event must sit at sync.(*Mutex).Unlock; top frames: {TopFrames(mutex: true)}");
            Assert.AreEqual(lockBefore + 1, TotalAt(mutex: false, "sync.(*Mutex).Lock"), $"the waiter's block event must sit at sync.(*Mutex).Lock; top frames: {TopFrames(mutex: false)}");
        });
    }

    // The count of the profile's records whose first two frames are the named functions.
    private static int64 TotalAt(bool mutex, string top, string caller)
    {
        int64 count = 0;
        foreach (var (_, record) in range(Records(mutex)))
        {
            var names = FrameNames(StackOf(record));
            if (names.Count > 1 && names[0] == top && names[1] == caller)
                count += record.Count;
        }
        return count;
    }

    // R's review question: converted code reaches Lock and Unlock through a *Mutex (the go2cs-gen
    // pointer-receiver overload) and through a sync.Locker (the generated MutexжLocker adapter), not only
    // on the field. Neither must leave a frame between the event's top frame and its Go caller, or frame [1]
    // differs from Go's and pprof splits the stack. Both paths, both events, each pinned to frame [1].
    [TestMethod]
    public void TheEventsSecondFrameIsTheGoCallerThroughAPointerOrALocker()
    {
        WithRates(1, 1, () =>
        {
            foreach (bool waiterViaLocker in new[] { true, false })
            {
                heap(new sync_package.Mutex(), out ж<sync_package.Mutex> box);
                sync_package.Locker locker = new sync_package.MutexжLocker(box);
                string waiterCaller = waiterViaLocker ? "mutexframeprobe.lockViaLocker" : "mutexframeprobe.lockViaPointer";
                string unlockerCaller = waiterViaLocker ? "mutexframeprobe.unlockViaPointer" : "mutexframeprobe.unlockViaLocker";

                int64 blockBefore = TotalAt(mutex: false, "sync.(*Mutex).Lock", waiterCaller);
                int64 mutexBefore = TotalAt(mutex: true, "sync.(*Mutex).Unlock", unlockerCaller);

                // ContendOnce's handshake: the holder releases only once this thread is Parked in Lock.
                using var held = new ManualResetEventSlim();
                using var ready = new ManualResetEventSlim();
                Thread waiter = Thread.CurrentThread;
                object? gate = null;
                int inLock = 0;
                bool parked = false;
                var holder = new Thread(() =>
                {
                    mutexframeprobe_package.lockViaPointer(box);
                    held.Set();
                    ready.Wait();
                    var waited = System.Diagnostics.Stopwatch.StartNew();
                    while (!(parked = Parked(gate!, waiter, ref inLock)) && waited.Elapsed < ParkBound)
                        Thread.Yield();
                    if (waiterViaLocker)
                        mutexframeprobe_package.unlockViaPointer(box);
                    else
                        mutexframeprobe_package.unlockViaLocker(locker);
                }) { IsBackground = true };
                holder.Start();
                held.Wait();
                gate = GateOf(box.Value);
                PrecreateStamps(gate);
                ready.Set();
                Volatile.Write(ref inLock, 1);
                if (waiterViaLocker)
                    mutexframeprobe_package.lockViaLocker(locker);
                else
                    mutexframeprobe_package.lockViaPointer(box);
                Volatile.Write(ref inLock, 0);
                mutexframeprobe_package.unlockViaPointer(box);
                holder.Join();
                Assert.IsTrue(parked, $"this thread was not seen parked in Lock within {ParkBound}, so the holder released an unwaited lock");

                Assert.AreEqual(blockBefore + 1, TotalAt(mutex: false, "sync.(*Mutex).Lock", waiterCaller), $"block event's frame [1] must be {waiterCaller}; records: {TopFrames(mutex: false)}");
                Assert.AreEqual(mutexBefore + 1, TotalAt(mutex: true, "sync.(*Mutex).Unlock", unlockerCaller), $"mutex event's frame [1] must be {unlockerCaller}; records: {TopFrames(mutex: true)}");
            }
        });
    }

    // The handoff restart, deterministically, through the private side record: go1.24.13 sema.go's
    // dequeue restarts the remaining waiters' acquire times at the release (L438-440), and a waiter
    // that loses the race re-queues with that clock (L311), so successive charges TELESCOPE.
    private static (object stamps, MethodInfo enqueue, MethodInfo acquired, MethodInfo tryHandoff) Stamps()
    {
        Type type = typeof(sync_package).GetNestedType("WaitStamps", BindingFlags.NonPublic)!;
        Assert.IsNotNull(type, "the hand-own's WaitStamps type");
        return (Activator.CreateInstance(type, nonPublic: true)!,
            type.GetMethod("Enqueue", BindingFlags.NonPublic | BindingFlags.Instance)!,
            type.GetMethod("Acquired", BindingFlags.NonPublic | BindingFlags.Instance)!,
            type.GetMethod("TryHandoff", BindingFlags.NonPublic | BindingFlags.Instance)!);
    }

    private static (bool handed, long dt) Handoff(object stamps, MethodInfo tryHandoff)
    {
        object[] args = [0L];
        bool handed = (bool)tryHandoff.Invoke(stamps, args)!;
        return (handed, (long)args[0]);
    }

    [TestMethod]
    public void AHandoffRestartsTheStampSoChargesTelescope()
    {
        var (stamps, enqueue, acquired, tryHandoff) = Stamps();
        const long gap = 1_000_000_000_000L;
        enqueue.Invoke(stamps, [GoCputicks() - gap]);   // one waiter, stamped long ago
        var first = Handoff(stamps, tryHandoff);          // the releaser barges back: the waiter stays
        var second = Handoff(stamps, tryHandoff);         // the next release charges it again
        acquired.Invoke(stamps, null);

        Assert.IsTrue(first.handed && first.dt >= gap, $"the first release charges the whole wait ({first.dt})");
        Assert.IsTrue(second.handed, "a waiter still blocked after a charge is charged again at the next release, as Go re-queues it");
        Assert.IsTrue(second.dt < gap / 2, $"the second charge runs from the first ({second.dt}): no wait is counted twice");
    }

    [TestMethod]
    public void NoChargeAfterTheLastStampedWaiterRetires()
    {
        var (stamps, enqueue, acquired, tryHandoff) = Stamps();
        enqueue.Invoke(stamps, [GoCputicks()]);
        acquired.Invoke(stamps, null);                    // retires before any release
        Assert.IsFalse(Handoff(stamps, tryHandoff).handed, "a release after the last stamped waiter left charges nothing");

        enqueue.Invoke(stamps, [GoCputicks()]);
        Assert.IsTrue(Handoff(stamps, tryHandoff).handed, "charged while waiting");
        acquired.Invoke(stamps, null);                    // the reverse order: charged, then retires
        Assert.IsFalse(Handoff(stamps, tryHandoff).handed, "the restart must not outlive the waiter it restarted");
    }

    [TestMethod]
    public void AHandoffWithWaitersRemainingAddsGosTailAverage()
    {
        var (stamps, enqueue, acquired, tryHandoff) = Stamps();
        const long unit = 1_000_000_000L;
        long now = GoCputicks();
        enqueue.Invoke(stamps, [now - 2 * unit]);        // head: waiting 2 units
        enqueue.Invoke(stamps, [now - unit]);            // tail: waiting 1 unit
        var (handed, dt) = Handoff(stamps, tryHandoff);
        acquired.Invoke(stamps, null);
        acquired.Invoke(stamps, null);

        // dt0 + (dtail + dt0) / 2 * remaining = 2 + (1 + 2) / 2 * 1 = 3.5 units, plus the few ticks
        // between `now` above and the charge.
        Assert.IsTrue(handed, "a stamped head is charged");
        Assert.IsTrue(dt >= 7 * unit / 2 && dt < 7 * unit / 2 + unit / 10, $"charged {dt} ticks; Go's formula reads 3.5 units ({7 * unit / 2})");
    }

    // The Wait(0) half's guard, by what the code guarantees rather than by what a scheduler does. The try
    // is linearizable, so a Lock that BLOCKS has always stamped, and the Unlock that releases a stamped
    // waiter records a mutex event (runtime/pprof TestMutexBlockFullAggregation read 3/20 solo before the
    // try). Each episode makes one such block deterministically:
    // - this thread locks a fresh mutex, so the gate exists and is held for the whole episode;
    // - the gate's stamp record is created up front (PrecreateStamps), so the waiter's Lock never takes
    //   the side table's add path, whose lock another thread can hold;
    // - a waiter thread calls Lock, and this thread waits until the waiter is Parked;
    // - the Unlock that releases it must record an event.
    // On a loaded runner, two free-running workers can finish one's ten short holds before the other's
    // first Lock: an episode with no contention and so, correctly, no event (osx-x64: "1 of 30 contended
    // episodes recorded no mutex event"). The racing shape is kept, with an honest precondition, below.
    [TestMethod]
    public void EveryParkedLockIsChargedByTheUnlockThatReleasesIt()
    {
        const int episodes = 30;
        WithRates(1, 1, () =>
        {
            var missed = new System.Collections.Generic.List<int>();
            for (int episode = 0; episode < episodes; episode++)
            {
                var shared = new Shared();
                shared.M.Lock();
                object gate = GateOf(shared);
                PrecreateStamps(gate);
                int inLock = 0;
                var waiter = new Thread(() =>
                {
                    Volatile.Write(ref inLock, 1);
                    shared.M.Lock();
                    Volatile.Write(ref inLock, 0);
                    shared.M.Unlock();
                }) { IsBackground = true };
                waiter.Start();
                var waited = System.Diagnostics.Stopwatch.StartNew();
                while (!Parked(gate, waiter, ref inLock))
                {
                    Assert.IsTrue(waited.Elapsed < ParkBound, $"episode {episode}: the waiter was not seen parked in Lock within {ParkBound}");
                    Thread.Yield();
                }
                int64 before = Total(mutex: true);
                shared.M.Unlock();
                if (Total(mutex: true) == before)
                    missed.Add(episode);
                Assert.IsTrue(waiter.Join(ParkBound), $"episode {episode}: the waiter did not finish");
            }
            Assert.AreEqual(0, missed.Count, $"{missed.Count} of {episodes} Unlocks released a parked waiter and recorded no mutex event: {string.Join(", ", missed)}");
        });
    }

    // The racing guard, beside the deterministic one: TestMutexBlockFullAggregation's shape, two workers
    // started together, each re-locking right after its own Unlock, so the start race and every
    // re-lock-after-unlock are exercised (a CurrentCount snapshot read 3/20 there). Whether an episode
    // contends at all is the scheduler's call, so the guarantee is asserted only where contention is
    // PROVEN:
    // - before each Unlock the holder looks, for up to 10 ms, for the other worker Parked in Lock;
    // - every Unlock made after such a sighting must record a mutex event;
    // - an episode with a sighting is CONTENDED. Episodes run until 20 are, under caps of 300 episodes and
    //   60 s (an oversubscribed box makes few sightings); reaching a cap first is Inconclusive with the
    //   counts, never red;
    // - OVERLAPPING episodes (the late worker's first Lock call before the early worker's last Unlock
    //   returned) and the rest are counted and printed, never asserted. Overlap does not prove a blocked
    //   Lock: the late worker's try can still come after that Unlock.
    [TestMethod]
    public void EveryEpisodeOfTwoAlternatingWorkersRecordsItsContention()
    {
        const int iterations = 10, MinContended = 20, MaxEpisodes = 300;
        TimeSpan budget = TimeSpan.FromSeconds(60);
        long window = System.Diagnostics.Stopwatch.Frequency / 100;   // 10 ms
        WithRates(1, 1, () =>
        {
            var missed = new System.Collections.Generic.List<string>();
            var elapsed = System.Diagnostics.Stopwatch.StartNew();
            int episode = 0, contended = 0, overlapping = 0, sightings = 0;
            for (; episode < MaxEpisodes && contended < MinContended && elapsed.Elapsed < budget; episode++)
            {
                var shared = new Shared();
                shared.M.Lock();                  // creates the gate; uncontended, so it records nothing
                shared.M.Unlock();
                object gate = GateOf(shared);
                PrecreateStamps(gate);
                using var start = new ManualResetEventSlim();
                int[] inLock = new int[2];
                long[] first = new long[2], last = new long[2];
                int seen = 0, current = episode;
                Thread[] workers = new Thread[2];
                for (int w = 0; w < workers.Length; w++)
                {
                    int self = w, other = 1 - w;
                    workers[w] = new Thread(() =>
                    {
                        start.Wait();
                        for (int i = 0; i < iterations; i++)
                        {
                            if (i == 0)
                                first[self] = System.Diagnostics.Stopwatch.GetTimestamp();
                            Volatile.Write(ref inLock[self], 1);
                            shared.M.Lock();
                            Volatile.Write(ref inLock[self], 0);
                            bool parked = false;
                            for (long t0 = System.Diagnostics.Stopwatch.GetTimestamp(); !parked && System.Diagnostics.Stopwatch.GetTimestamp() - t0 < window; Thread.Yield())
                                parked = Parked(gate, workers[other], ref inLock[other]);
                            if (!parked)
                            {
                                shared.M.Unlock();
                                continue;
                            }
                            int64 before = Total(mutex: true);
                            shared.M.Unlock();
                            Interlocked.Increment(ref seen);
                            if (Total(mutex: true) == before)
                                lock (missed)
                                    missed.Add($"{current}.{i}");
                        }
                        last[self] = System.Diagnostics.Stopwatch.GetTimestamp();
                    }) { IsBackground = true };
                    workers[w].Start();
                }
                start.Set();
                foreach (Thread worker in workers)
                    Assert.IsTrue(worker.Join(ParkBound), $"episode {episode}: a worker did not finish");
                int early = first[0] <= first[1] ? 0 : 1;
                if (first[1 - early] < last[early])
                    overlapping++;
                if (seen > 0)
                    contended++;
                sightings += seen;
            }
            string counts = $"{episode} episodes in {elapsed.Elapsed.TotalSeconds:F1} s: {contended} contended ({sightings} parked waiters seen), {overlapping} overlapping, {episode - overlapping} not overlapping";
            Console.WriteLine(counts);
            Assert.AreEqual(0, missed.Count, $"{missed.Count} Unlock(s) released a waiter parked in Lock and recorded no mutex event (episode.iteration): {string.Join(", ", missed)}; {counts}");
            if (contended < MinContended)
                Assert.Inconclusive($"the race was not exercised enough: {counts}");
        });
    }
}
