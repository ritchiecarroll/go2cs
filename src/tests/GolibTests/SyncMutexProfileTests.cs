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
    // is waiting on it, then releases it to this thread. (Two free-running workers are not a fixture:
    // one can finish before the other starts, and SemaphoreSlim lets a releasing thread take the lock
    // straight back, so how many handoffs find a waiter depends on thread start-up.)
    private static void ContendOnce(Shared shared)
    {
        using var held = new ManualResetEventSlim();
        var holder = new Thread(() =>
        {
            shared.M.Lock();
            held.Set();
            Thread.Sleep(50);
            shared.M.Unlock();
        });
        holder.Start();
        held.Wait();
        shared.M.Lock();
        shared.M.Unlock();
        holder.Join();
    }

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
        foreach (var (_, pc) in stack)
            names.Add((string)FuncForPC(pc - 1).Name());
        return names;
    }

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
        foreach (var (_, record) in Records(mutex))
        {
            var names = FrameNames(record.StackRecord.Stack());
            if (names.Count > 0 && names[0] == function)
                count += record.Count;
        }
        return count;
    }

    // Every record's first two frames, for a failure message.
    private static string TopFrames(bool mutex)
    {
        var all = new System.Collections.Generic.List<string>();
        foreach (var (_, record) in Records(mutex))
        {
            var names = FrameNames(record.StackRecord.Stack());
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
        foreach (var (_, record) in Records(mutex))
        {
            var names = FrameNames(record.StackRecord.Stack());
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

                using var held = new ManualResetEventSlim();
                var holder = new Thread(() =>
                {
                    mutexframeprobe_package.lockViaPointer(box);
                    held.Set();
                    Thread.Sleep(50);
                    if (waiterViaLocker)
                        mutexframeprobe_package.unlockViaPointer(box);
                    else
                        mutexframeprobe_package.unlockViaLocker(locker);
                });
                holder.Start();
                held.Wait();
                if (waiterViaLocker)
                    mutexframeprobe_package.lockViaLocker(locker);
                else
                    mutexframeprobe_package.lockViaPointer(box);
                mutexframeprobe_package.unlockViaPointer(box);
                holder.Join();

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

    // The Wait(0) half's guard: many short episodes of two workers started together, each worker
    // re-locking right after its own Unlock. Every episode contends, so every episode must record at
    // least one mutex event. A contention test that reads the gate's count before waiting misses the
    // start race and every re-lock-after-unlock, and whole episodes went unrecorded (runtime/pprof
    // TestMutexBlockFullAggregation read 3/20 solo with the restart alone); the try is linearizable, so
    // a Lock that blocks has always stamped.
    [TestMethod]
    public void EveryEpisodeOfTwoAlternatingWorkersRecordsItsContention()
    {
        const int episodes = 30, iterations = 10;
        WithRates(1, 1, () =>
        {
            var silent = new System.Collections.Generic.List<int>();
            for (int episode = 0; episode < episodes; episode++)
            {
                int64 before = Total(mutex: true);
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
                if (Total(mutex: true) == before)
                    silent.Add(episode);
            }
            Assert.AreEqual(0, silent.Count, $"{silent.Count} of {episodes} contended episodes recorded no mutex event: {string.Join(", ", silent)}");
        });
    }
}
