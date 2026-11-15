#nullable enable

using System;
using System.Collections;
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
/// a wakeup that finds no waiter is kept once and not counted up; and a handle is never given to two queues.
/// </summary>
/// <remarks>
/// Every mutex here lives in native memory, which gives the test a stable address to read the two words at:
/// state at offset 0 and sema at offset 4.
/// </remarks>
[TestClass]
public unsafe class SyncMutexQueueTests
{
    private static readonly TimeSpan Bound = TimeSpan.FromSeconds(20);

    private static readonly MethodInfo? s_gateOf =
        typeof(sync).GetMethod("gateOf", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo? s_queues =
        typeof(sync).GetField("s_queues", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo? s_lastQueue =
        typeof(sync).GetField("s_lastQueue", BindingFlags.NonPublic | BindingFlags.Static);

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
            Assert.IsFalse(Queues().Contains(during), "the queue of an idle mutex is removed from the table");
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

    // Handles come from a 32-bit counter that wraps. A handle a live queue holds is skipped: handing it out
    // again would put a second mutex on the first one's entry and leave the first mutex's handle naming the
    // wrong queue.
    [TestMethod]
    public void AHandleInUseIsNotGivenToASecondQueue()
    {
        Assert.IsNotNull(s_lastQueue, "the hand-own's s_lastQueue counter");
        sync.Mutex* block = (sync.Mutex*)NativeMemory.AllocZeroed((nuint)(2 * sizeof(sync.Mutex)));
        sync.Mutex* first = block, second = block + 1;
        int counter = (int)s_lastQueue.GetValue(null)!;

        try
        {
            SemaphoreSlim firstGate = GateOf(first);
            uint firstHandle = Sema(first);
            Assert.AreNotEqual(0u, firstHandle, "gateOf installs the first mutex's queue");
            counter = (int)s_lastQueue.GetValue(null)!;

            // The counter is about to produce firstHandle again, as it does after a wrap.
            s_lastQueue.SetValue(null, unchecked((int)(firstHandle - 1)));
            SemaphoreSlim secondGate = GateOf(second);

            Assert.AreNotSame(firstGate, secondGate, "each mutex has its own queue");
            Assert.AreNotEqual(firstHandle, Sema(second), "the second queue took a handle the first queue holds");
            Assert.AreSame(firstGate, Queues()[firstHandle], "the first mutex's handle still names its own queue");
        }
        finally
        {
            // Past every handle this test produced, so later queues do not land on the two kept here.
            s_lastQueue.SetValue(null, Math.Max(counter, (int)s_lastQueue.GetValue(null)!) + 2);
            NativeMemory.Free(block);
        }
    }

    // Queues come and go while other threads read the handle: eight threads move across four mutexes, and the
    // holder yields often enough that waiters block and queues empty many times. Every counter is exact, no
    // worker is left waiting, and afterwards no mutex holds a queue.
    [TestMethod]
    public void MutexesWhoseQueuesComeAndGoCountExactly()
    {
        const int Mutexes = 4, Threads = 8, Rounds = 20000;
        byte* block = (byte*)NativeMemory.AllocZeroed(Mutexes * 16);
        bool leak = false;

        try
        {
            using ManualResetEventSlim start = new(false);
            Thread[] workers = new Thread[Threads];

            for (int t = 0; t < Threads; t++)
            {
                int self = t;
                workers[t] = new Thread(() =>
                {
                    start.Wait();

                    for (int i = 0; i < Rounds; i++)
                    {
                        byte* slot = block + ((i + self) % Mutexes) * 16;
                        sync.Mutex* m = (sync.Mutex*)slot;
                        m->Lock();
                        *(long*)(slot + 8) += 1;

                        if ((i & 63) == self)
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

            for (int i = 0; i < Mutexes; i++)
            {
                sync.Mutex* m = (sync.Mutex*)(block + i * 16);
                total += *(long*)(block + i * 16 + 8);
                Assert.AreEqual(0, State(m), $"mutex {i} is unlocked after every worker unlocked it");
                Assert.AreEqual(0u, Sema(m), $"mutex {i} is idle and holds no queue handle");
            }

            Assert.AreEqual((long)Threads * Rounds, total, "the counters under the four mutexes");
        }
        finally
        {
            if (!leak)
                NativeMemory.Free(block);
        }
    }

    private static int State(sync.Mutex* m) => Volatile.Read(ref *(int*)m);

    private static uint Sema(sync.Mutex* m) => Volatile.Read(ref ((uint*)m)[1]);

    private static IDictionary Queues()
    {
        Assert.IsNotNull(s_queues, "the hand-own's s_queues table");
        return (IDictionary)s_queues.GetValue(null)!;
    }

    // The mutex's wait queue, installed if the mutex has none, and held as a waiter holds it. No other thread
    // touches the mutex while the reflection call copies it out and back.
    private static SemaphoreSlim GateOf(sync.Mutex* m)
    {
        Assert.IsNotNull(s_gateOf, "the hand-own's gateOf(ref Mutex)");
        object?[] args = [*m];
        object? gate = s_gateOf.Invoke(null, args);
        *m = (sync.Mutex)args[0]!;
        Assert.IsTrue(gate is SemaphoreSlim, "gateOf returns the mutex's wait queue");
        return (SemaphoreSlim)gate!;
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
