#nullable enable

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using sync = go.sync_package;

namespace GolibTests;

/// <summary>
/// The life of a sync.Mutex's wait queue (sqlite-cs W9a). A Mutex is Go's eight bytes, so its wait queue lives
/// in a process-wide table and the mutex holds only the queue's handle, in its sema word. These arms pin what
/// that indirection must not change: an idle mutex holds no queue, so a copy of it is its own mutex as in Go;
/// a wakeup that finds no waiter is kept once and not counted up; a handle is never given to two queues; and a
/// queue given up by one mutex serves the next, so a contended Lock stops allocating.
/// </summary>
/// <remarks>
/// Every mutex here lives in native memory, which gives the test a stable address to read the two words at:
/// state at offset 0 and sema at offset 4.
/// <para>
/// Not parallel: several arms read what one stripe of the queue table holds, and threads share stripes, so a
/// thread of another test class running at the same moment could take or leave a spare queue between two reads.
/// GolibTests runs its classes one at a time today (the assembly has no Parallelize attribute); the attribute
/// keeps these arms out of any parallel run it gains.
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public unsafe class SyncMutexQueueTests
{
    private static readonly TimeSpan Bound = TimeSpan.FromSeconds(20);

    private static readonly MethodInfo? s_gateOf =
        typeof(sync).GetMethod("gateOf", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly MethodInfo? s_leave =
        typeof(sync).GetMethod("leave", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly MethodInfo? s_queueOf =
        typeof(sync).GetMethod("queueOf", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly MethodInfo? s_stripeOf =
        typeof(sync).GetMethod("stripeOf", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo? s_stripeBits =
        typeof(sync).GetField("StripeBits", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo? s_semaphoreLock =
        typeof(SemaphoreSlim).GetField("m_lockObjAndDisposed", BindingFlags.NonPublic | BindingFlags.Instance);

    // Go: a Mutex "must not be copied after first use", and vet says so, but a copy of an UNLOCKED mutex that
    // no goroutine waits on is a working mutex in Go: its state word is 0 and its sema word is 0. Here the copy
    // must not carry the original's wait queue. With one queue shared by both, the wakeup the original's Unlock
    // sends goes to the queue's first waiter, which is the copy's: it finds the copy still held and waits again,
    // and the original's waiter is never woken.
    [TestMethod]
    public void ACopyOfAnIdleMutexThatOnceContendedIsItsOwnMutex()
    {
        sync.Mutex* block = (sync.Mutex*)NativeMemory.AllocZeroed((nuint)(2 * sizeof(sync.Mutex)));
        sync.Mutex* original = block, copy = block + 1;
        bool leak = false;

        try
        {
            ContendOnce(original);
            *copy = *original;

            copy->Lock();
            Thread copyWaiter = LockAndUnlockOnAThread(copy);
            Assert.IsTrue(BlockedWithin(copyWaiter), "the copy's waiter did not block on the held copy");

            original->Lock();
            Thread originalWaiter = LockAndUnlockOnAThread(original);
            Assert.IsTrue(BlockedWithin(originalWaiter), "the original's waiter did not block on the held original");

            original->Unlock();
            bool woken = originalWaiter.Join(Bound);

            copy->Unlock();
            bool copyWoken = copyWaiter.Join(Bound);
            leak = !(woken && copyWoken);

            Assert.IsTrue(woken, "the original's Unlock did not wake the original's waiter: the copy shares its wait queue");
            Assert.IsTrue(copyWoken, "the copy's Unlock did not wake the copy's waiter");
        }
        finally
        {
            // A waiter still blocked reads this memory when it wakes, so a failed run keeps the block.
            if (!leak)
                NativeMemory.Free(block);
        }
    }

    // A copy of a LOCKED mutex is locked, in Go and here: the lock is the state word and the copy has it. The
    // copy's Unlock frees the copy and nothing else. Both mutexes have a blocked waiter when it runs, the
    // original's first, so a wakeup sent to a queue the two shared would go to the original's waiter and leave
    // the copy's asleep on a free mutex.
    [TestMethod]
    public void ACopyOfALockedMutexIsLockedAndItsUnlockFreesOnlyTheCopy()
    {
        sync.Mutex* block = (sync.Mutex*)NativeMemory.AllocZeroed((nuint)(2 * sizeof(sync.Mutex)));
        sync.Mutex* original = block, copy = block + 1;
        bool leak = false;

        try
        {
            ContendOnce(original);
            original->Lock();
            *copy = *original;

            Assert.IsFalse(copy->TryLock(), "a copy of a locked mutex is locked");
            leak = true;

            Thread originalWaiter = LockAndUnlockOnAThread(original);
            Assert.IsTrue(BlockedWithin(originalWaiter), "the original's waiter did not block on the held original");

            Thread copyWaiter = LockAndUnlockOnAThread(copy);
            Assert.IsTrue(BlockedWithin(copyWaiter), "the copy's waiter did not block on the locked copy");

            copy->Unlock();
            bool copyWoken = copyWaiter.Join(Bound);
            bool originalStillHeld = !originalWaiter.Join(TimeSpan.FromMilliseconds(200)) && State(original) != 0;

            original->Unlock();
            bool woken = originalWaiter.Join(Bound);
            leak = !(woken && copyWoken);

            Assert.IsTrue(copyWoken, "the copy's Unlock did not wake the copy's waiter: the copy shares the original's wait queue");
            Assert.IsTrue(originalStillHeld, "the copy's Unlock freed the original");
            Assert.IsTrue(woken, "the original's Unlock did not wake the original's waiter");
            Assert.AreEqual(0u, Sema(original), "the original is idle and holds no queue handle");
            Assert.AreEqual(0u, Sema(copy), "the copy is idle and holds no queue handle");
        }
        finally
        {
            // A waiter still blocked reads this memory when it wakes, so a failed run keeps the block.
            if (!leak)
                NativeMemory.Free(block);
        }
    }

    // Go's sema word is 0 whenever no goroutine waits on the mutex. The queue handle lives in that word, so it
    // is 0 again once the last waiter has the lock, and the queue is gone from the table.
    [TestMethod]
    public void AnIdleMutexHoldsNoQueue()
    {
        sync.Mutex* m = (sync.Mutex*)NativeMemory.AllocZeroed((nuint)sizeof(sync.Mutex));

        try
        {
            uint during = ContendOnce(m);

            Assert.AreNotEqual(0u, during, "a contended mutex holds its queue's handle while a waiter is blocked");
            Assert.AreEqual(0, State(m), "the mutex is unlocked");
            Assert.AreEqual(0u, Sema(m), "an idle mutex holds no queue handle, as Go's sema word is 0 with no waiter");
            Assert.IsNull(QueueOf(during), "the queue of an idle mutex is removed from the table");
        }
        finally
        {
            NativeMemory.Free(m);
        }
    }

    // A waiter that takes the lock leaves the state at "a waiter may be blocked", so its Unlock sends a wakeup
    // whether or not anyone waits. A wakeup nobody takes is kept for the next waiter, and one kept wakeup is
    // enough: each further one would cost a later waiter a turn round its loop, without bound. The queue is
    // held here by gateOf, as a waiter holds it, so it outlives the Unlocks.
    [TestMethod]
    public void AWakeupThatFindsNoWaiterIsKeptOnce()
    {
        sync.Mutex* m = (sync.Mutex*)NativeMemory.AllocZeroed((nuint)sizeof(sync.Mutex));

        try
        {
            SemaphoreSlim gate = GateOf(m);

            for (int i = 0; i < 100; i++)
            {
                *(int*)m = 2;     // locked, as a waiter that took the lock leaves it
                m->Unlock();
            }

            Assert.IsTrue(gate.CurrentCount <= 1, $"100 Unlocks with no waiter kept {gate.CurrentCount} wakeups; one is enough");
            Assert.AreEqual(0, State(m), "the mutex is unlocked");
        }
        finally
        {
            NativeMemory.Free(m);
        }
    }

    // The rule above with waiters present, which is where a lost wakeup would cost a goroutine: two waiters are
    // blocked, a wakeup is waiting to be taken, and Unlock runs. It sends no second wakeup, and both waiters
    // still get the lock: the one that takes the waiting wakeup finds the mutex free, and its own Unlock wakes
    // the other. The wakeup is held in the "waiting to be taken" state by this thread holding the semaphore's
    // own monitor, which the woken waiter needs before it can take the wakeup.
    [TestMethod]
    public void UnlockSendsNoSecondWakeupWhileOneWaitsAndBothWaitersGetTheLock()
    {
        Assert.IsTrue(s_semaphoreLock is not null, "SemaphoreSlim.m_lockObjAndDisposed not found: a waiting wakeup cannot be held (re-read the field at each .NET major hop)");
        sync.Mutex* m = (sync.Mutex*)NativeMemory.AllocZeroed((nuint)sizeof(sync.Mutex));
        bool leak = true;

        try
        {
            m->Lock();
            Thread first = LockAndUnlockOnAThread(m);
            Assert.IsTrue(BlockedWithin(first), "the first waiter did not block on the held mutex");
            Thread second = LockAndUnlockOnAThread(m);
            Assert.IsTrue(BlockedWithin(second), "the second waiter did not block on the held mutex");

            SemaphoreSlim? gate = QueueOf(Sema(m));
            Assert.IsNotNull(gate, "a mutex with blocked waiters holds its queue's handle");
            object monitor = s_semaphoreLock!.GetValue(gate)!;
            int afterWakeup, afterUnlock;

            lock (monitor)
            {
                gate!.Release();
                afterWakeup = gate.CurrentCount;
                m->Unlock();
                afterUnlock = gate.CurrentCount;
            }

            bool firstDone = first.Join(Bound), secondDone = second.Join(Bound);
            leak = !(firstDone && secondDone);

            Assert.AreEqual(1, afterWakeup, "the wakeup is waiting to be taken while this thread holds the semaphore's monitor");
            Assert.AreEqual(1, afterUnlock, "Unlock sent a second wakeup while one was waiting to be taken");
            Assert.IsTrue(firstDone && secondDone, "a waiter was left blocked: the Unlock that sent no wakeup lost one");
            Assert.AreEqual(0, State(m), "the mutex is unlocked");
            Assert.AreEqual(0u, Sema(m), "the mutex is idle and holds no queue handle");
        }
        finally
        {
            if (!leak)
                NativeMemory.Free(m);
        }
    }

    // A handle is a stripe's sequence number, which wraps. A handle a live queue holds is skipped: handing it
    // out again would put a second mutex on the first one's entry and leave the first mutex's handle naming
    // the wrong queue. Both queues are installed by this thread, so both come from this thread's stripe.
    [TestMethod]
    public void AHandleInUseIsNotGivenToASecondQueue()
    {
        Assert.IsNotNull(s_stripeOf, "the hand-own's stripeOf(uint32)");
        Assert.IsNotNull(s_stripeBits, "the hand-own's StripeBits");
        int stripeBits = (int)s_stripeBits!.GetRawConstantValue()!;
        sync.Mutex* block = (sync.Mutex*)NativeMemory.AllocZeroed((nuint)(2 * sizeof(sync.Mutex)));
        sync.Mutex* first = block, second = block + 1;

        try
        {
            SemaphoreSlim firstGate = GateOf(first);
            uint firstHandle = Sema(first);
            Assert.AreNotEqual(0u, firstHandle, "gateOf installs the first mutex's queue");

            // The stripe is about to produce firstHandle again, as it does after a wrap.
            object stripe = s_stripeOf!.Invoke(null, [firstHandle])!;
            FieldInfo? sequence = stripe.GetType().GetField("m_sequence", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(sequence, "the stripe's m_sequence");

            lock (stripe)
                sequence!.SetValue(stripe, (firstHandle >> stripeBits) - 1);

            SemaphoreSlim secondGate = GateOf(second);
            uint secondHandle = Sema(second);

            Assert.AreSame(stripe, s_stripeOf.Invoke(null, [secondHandle]), "this thread installs both queues in one stripe");
            Assert.AreNotSame(firstGate, secondGate, "each mutex has its own queue");
            Assert.AreNotEqual(firstHandle, secondHandle, "the second queue took a handle the first queue holds");
            Assert.AreSame(firstGate, QueueOf(firstHandle), "the first mutex's handle still names its own queue");
            Assert.AreSame(secondGate, QueueOf(secondHandle), "the second mutex's handle names its own queue");
        }
        finally
        {
            NativeMemory.Free(block);
        }
    }

    // A queue nobody waits on is kept and serves the next mutex that needs one, so contention does not
    // allocate: every round here is one whole contention episode on the waiter (it installs a queue, blocks or
    // is about to, takes the lock and gives the queue up), and after the first rounds the waiter allocates
    // nothing. The waiter measures its own thread, and the two threads take turns through two counters, so
    // nothing in the measured loop allocates but Lock and Unlock themselves.
    [TestMethod]
    public void AContendedLockAllocatesNothingOnceItsThreadHasASpareQueue()
    {
        const int Warmup = 200, Rounds = 2000;
        sync.Mutex* m = (sync.Mutex*)NativeMemory.AllocZeroed((nuint)sizeof(sync.Mutex));
        int go = 0, done = 0, episodes = 0;
        long allocated = -1;
        bool leak = true;

        try
        {
            Thread waiter = new(() =>
            {
                long before = 0;

                for (int round = 1; round <= Warmup + Rounds; round++)
                {
                    while (Volatile.Read(ref go) != round)
                        Thread.Yield();

                    if (round == Warmup + 1)
                        before = GC.GetAllocatedBytesForCurrentThread();

                    m->Lock();
                    m->Unlock();
                    Volatile.Write(ref done, round);
                }

                Volatile.Write(ref allocated, GC.GetAllocatedBytesForCurrentThread() - before);
            }) { IsBackground = true };
            waiter.Start();

            var waited = System.Diagnostics.Stopwatch.StartNew();

            for (int round = 1; round <= Warmup + Rounds && waited.Elapsed < TimeSpan.FromMinutes(2); round++)
            {
                m->Lock();
                Volatile.Write(ref go, round);

                // The handle appears once the waiter is counted into its queue, before it blocks: from then
                // on this Unlock is the one that must wake it.
                while (Sema(m) == 0 && waited.Elapsed < TimeSpan.FromMinutes(2))
                    Thread.Yield();

                if (Sema(m) != 0)
                    episodes++;

                m->Unlock();

                while (Volatile.Read(ref done) != round && waited.Elapsed < TimeSpan.FromMinutes(2))
                    Thread.Yield();
            }

            bool finished = waiter.Join(Bound);
            leak = !finished;

            Assert.IsTrue(finished, "the waiter did not finish its rounds");
            Assert.AreEqual(Warmup + Rounds, episodes, "every round is a contention episode: the waiter installed a queue");
            Assert.AreEqual(0L, Volatile.Read(ref allocated), $"the waiter allocated over {Rounds} contention episodes; a spare queue serves each one");
            Assert.AreEqual(0u, Sema(m), "the mutex is idle and holds no queue handle");
        }
        finally
        {
            if (!leak)
                NativeMemory.Free(m);
        }
    }

    // A stripe keeps every queue it created. A thread that installed a queue moves on while other threads are
    // still counted on it, so one thread can have several queues in the table at once, and when they are given
    // up they all wait in its stripe for the next installs. Here one thread holds a queue on each of six
    // mutexes, as a waiter holds it, gives the six up and takes six again: the second six are the first six,
    // so nothing was created and nothing was dropped. With a limit of four spares, two of the second six were
    // new, and a stripe with more than four queues in use paid an allocation every time it came back to them.
    // No other thread may touch this thread's stripe between the six leaves and the six retakes (it could
    // take one of the spares), which is why the class is not parallel.
    [TestMethod]
    public void TheQueuesAThreadInstalledAtOnceAllServeItAgain()
    {
        const int Mutexes = 6;
        sync.Mutex* ms = (sync.Mutex*)NativeMemory.AllocZeroed((nuint)(Mutexes * sizeof(sync.Mutex)));

        try
        {
            SemaphoreSlim[] first = new SemaphoreSlim[Mutexes], second = new SemaphoreSlim[Mutexes];

            for (int i = 0; i < Mutexes; i++)
            {
                first[i] = GateOf(ms + i);

                for (int earlier = 0; earlier < i; earlier++)
                    Assert.AreNotSame(first[earlier], first[i], "two mutexes with a waiter each have two queues");
            }

            for (int i = 0; i < Mutexes; i++)
                Leave(ms + i, first[i]);

            for (int i = 0; i < Mutexes; i++)
                Assert.AreEqual(0u, Sema(ms + i), "a mutex nobody waits on holds no queue handle");

            int created = 0;

            for (int i = 0; i < Mutexes; i++)
            {
                second[i] = GateOf(ms + i);

                if (Array.IndexOf(first, second[i]) < 0)
                    created++;
            }

            for (int i = 0; i < Mutexes; i++)
                Leave(ms + i, second[i]);

            Assert.AreEqual(0, created, $"of {Mutexes} queues the thread took the second time, {created} were not among the {Mutexes} it gave up: the stripe dropped spare queues");
        }
        finally
        {
            NativeMemory.Free(ms);
        }
    }

    // Unlock sends its wakeup outside the stripe's lock, so the queue it looked up can be given up, and
    // installed for ANOTHER mutex, before the wakeup arrives. The late wakeup is played here by a Release on
    // the queue object this thread kept. Arriving while the queue is a spare, it is dropped when the queue is
    // installed again. Arriving after, it is one extra wakeup on the other mutex: its waiter takes it, finds
    // the mutex still held and blocks again, and the mutex's own Unlock is what gives it the lock. Both
    // queues are installed by this thread, so the second mutex gets the first one's queue from the stripe.
    [TestMethod]
    public void AWakeupThatArrivesAfterItsQueueServesAnotherMutexCostsOneReCheck()
    {
        sync.Mutex* block = (sync.Mutex*)NativeMemory.AllocZeroed((nuint)(2 * sizeof(sync.Mutex)));
        sync.Mutex* first = block, second = block + 1;
        bool leak = false;

        try
        {
            SemaphoreSlim firstGate = GateOf(first);
            uint firstHandle = Sema(first);
            Leave(first, firstGate);
            Assert.AreEqual(0u, Sema(first), "the last waiter out clears the first mutex's handle");
            Assert.IsNull(QueueOf(firstHandle), "the first mutex's handle names nothing once its queue is given up");

            firstGate.Release();     // late, onto the spare queue
            second->Lock();
            SemaphoreSlim secondGate = GateOf(second);
            Assert.AreSame(firstGate, secondGate, "the second mutex is served by the queue the first gave up");
            Assert.AreNotEqual(firstHandle, Sema(second), "under a new handle");
            Assert.AreEqual(0, secondGate.CurrentCount, "a wakeup sent to a spare queue is dropped when the queue is installed");

            firstGate.Release();     // late, onto the second mutex's queue
            leak = true;
            Thread waiter = LockAndUnlockOnAThread(second);
            bool blocked = BlockedWithin(waiter);
            int heldState = State(second);
            int kept = secondGate.CurrentCount;

            // Unlocking a mutex the waiter already unlocked would end the process: a failed run stops here.
            if (heldState != 0)
                second->Unlock();

            bool woken = waiter.Join(Bound);
            leak = !woken;

            Assert.IsTrue(blocked, "the waiter took the late wakeup for the lock: it did not block on the held mutex");
            Assert.AreEqual(2, heldState, "the second mutex is still held, with a waiter recorded");
            Assert.AreEqual(0, kept, "the waiter took the late wakeup and waits again");
            Assert.IsTrue(woken, "the second mutex's own Unlock did not wake its waiter");
            Assert.AreEqual(0, State(first), "the first mutex was never locked");
            Assert.IsTrue(first->TryLock(), "the first mutex is free");
        }
        finally
        {
            // A waiter still blocked reads this memory when it wakes, so a failed run keeps the block.
            if (!leak)
                NativeMemory.Free(block);
        }
    }

    // Queues come and go while other threads read the handle: eight threads move across four mutexes, and the
    // holder yields often enough that waiters block and queues empty many times. Every counter is exact, no
    // worker is left waiting, and afterwards no mutex holds a queue.
    [TestMethod]
    public void MutexesWhoseQueuesComeAndGoCountExactly() => ComeAndGo(mutexes: 4, threads: 8, rounds: 20000);

    // The same with many more of both: sixteen threads, which is several to a processor, across sixty-four
    // mutexes, so queues are installed and given up in many stripes at once and spare queues change hands.
    [TestMethod]
    public void ManyThreadsOverManyMutexesCountExactly() => ComeAndGo(mutexes: 64, threads: 16, rounds: 20000);

    private static void ComeAndGo(int mutexes, int threads, int rounds)
    {
        byte* block = (byte*)NativeMemory.AllocZeroed((nuint)(mutexes * 16));
        bool leak = false;

        try
        {
            using ManualResetEventSlim start = new(false);
            Thread[] workers = new Thread[threads];

            for (int t = 0; t < threads; t++)
            {
                int self = t;
                workers[t] = new Thread(() =>
                {
                    start.Wait();

                    for (int i = 0; i < rounds; i++)
                    {
                        byte* slot = block + ((i + self) % mutexes) * 16;
                        sync.Mutex* m = (sync.Mutex*)slot;
                        m->Lock();
                        *(long*)(slot + 8) += 1;

                        if ((i & 15) == self)
                            Thread.Yield();

                        m->Unlock();
                    }
                }) { IsBackground = true };
                workers[t].Start();
            }

            start.Set();
            bool finished = true;

            foreach (Thread worker in workers)
                finished &= worker.Join(TimeSpan.FromMinutes(2));

            // A blocked worker still reads the block, so a failed run keeps it.
            leak = !finished;
            Assert.IsTrue(finished, "a worker did not finish: a lost wakeup");

            long total = 0;

            for (int i = 0; i < mutexes; i++)
            {
                sync.Mutex* m = (sync.Mutex*)(block + i * 16);
                total += *(long*)(block + i * 16 + 8);
                Assert.AreEqual(0, State(m), $"mutex {i} is unlocked after every worker unlocked it");
                Assert.AreEqual(0u, Sema(m), $"mutex {i} is idle and holds no queue handle");
            }

            Assert.AreEqual((long)threads * rounds, total, "the counters under the mutexes");
        }
        finally
        {
            if (!leak)
                NativeMemory.Free(block);
        }
    }

    private static int State(sync.Mutex* m) => Volatile.Read(ref *(int*)m);

    private static uint Sema(sync.Mutex* m) => Volatile.Read(ref ((uint*)m)[1]);

    // The queue a handle names in the table, or null.
    private static SemaphoreSlim? QueueOf(uint handle)
    {
        Assert.IsNotNull(s_queueOf, "the hand-own's queueOf(uint32)");
        return (SemaphoreSlim?)s_queueOf!.Invoke(null, [handle]);
    }

    // The mutex's wait queue, installed if the mutex has none, and held as a waiter holds it. No other thread
    // touches the mutex while the reflection call copies it out and back.
    private static SemaphoreSlim GateOf(sync.Mutex* m)
    {
        Assert.IsNotNull(s_gateOf, "the hand-own's gateOf(ref Mutex)");
        object?[] args = [*m];
        object? gate = s_gateOf!.Invoke(null, args);
        *m = (sync.Mutex)args[0]!;
        Assert.IsTrue(gate is SemaphoreSlim, "gateOf returns the mutex's wait queue");
        return (SemaphoreSlim)gate!;
    }

    // Gives back a queue GateOf returned, as a waiter that has the lock does.
    private static void Leave(sync.Mutex* m, SemaphoreSlim gate)
    {
        Assert.IsNotNull(s_leave, "the hand-own's leave(ref Mutex, WaitQueue)");
        object?[] args = [*m, gate];
        s_leave!.Invoke(null, args);
        *m = (sync.Mutex)args[0]!;
    }

    // One contention episode, start to finish: this thread holds m, a waiter blocks in Lock, this thread
    // unlocks, the waiter takes the lock and unlocks it. Returns the sema word read while the waiter was
    // blocked.
    private static uint ContendOnce(sync.Mutex* m)
    {
        m->Lock();
        Thread waiter = LockAndUnlockOnAThread(m);
        Assert.IsTrue(BlockedWithin(waiter), "the waiter did not block on the held mutex");
        uint during = Sema(m);
        m->Unlock();
        Assert.IsTrue(waiter.Join(Bound), "the waiter was not woken by Unlock");
        return during;
    }

    private static Thread LockAndUnlockOnAThread(sync.Mutex* m)
    {
        Thread thread = new(() =>
        {
            m->Lock();
            m->Unlock();
        }) { IsBackground = true };
        thread.Start();
        return thread;
    }

    // The thread goes straight into Lock, so the only place it can stay in WaitSleepJoin is the wait on the
    // mutex's queue. One sighting can be the queue's own short monitor section, so it must still hold a
    // moment later.
    private static bool BlockedWithin(Thread thread)
    {
        var waited = System.Diagnostics.Stopwatch.StartNew();

        while (waited.Elapsed < Bound)
        {
            if ((thread.ThreadState & ThreadState.WaitSleepJoin) != 0)
            {
                Thread.Sleep(20);

                if ((thread.ThreadState & ThreadState.WaitSleepJoin) != 0)
                    return true;
            }

            Thread.Yield();
        }

        return false;
    }
}
