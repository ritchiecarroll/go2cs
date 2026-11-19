#nullable enable

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// A semaphore word's bucket (its lock and waiter queue) lives only while someone needs it. The last waiter
/// to leave removes it, so a word that blocked once costs nothing afterwards and its owner can be collected.
/// </summary>
/// <remarks>
/// Before, the table had a GetOrAdd and no removal. Every word that ever blocked, and every word a Release
/// ever touched, kept a bucket for the life of the process, and the key (the pointer box) kept the word's
/// owner alive with it. sync.RWMutex blocks on this table since it took Go's layout, so N short-lived
/// RWMutexes that each blocked once left N buckets and N owners behind. The arms read the private table by
/// reflection, per word, so they hold under parallel test runs.
/// </remarks>
[TestClass]
public class RuntimeSemaphoreBucketTests
{
    private const int Words = 200;

    [TestMethod]
    public void AWordThatBlockedOnceKeepsNoBucket()
    {
        IDictionary table = SemaTable();
        ж<uint32>[] words = new ж<uint32>[Words];

        for (int i = 0; i < Words; i++)
        {
            heap<uint32>(out words[i]);
            BlockOnceAndRelease(words[i]);
        }

        int kept = 0;

        foreach (ж<uint32> word in words)
        {
            if (table.Contains(word))
                kept++;
        }

        Assert.AreEqual(0, kept, $"{kept} of {Words} words that each blocked once still have a bucket");
    }

    [TestMethod]
    public void AReleaseWithNoWaiterKeepsNoBucket()
    {
        heap<uint32>(out var word);
        RuntimeSemaphore.Release(word, handoff: false);

        Assert.AreEqual(1u, word.Value, "the permit is kept in the word");
        Assert.IsFalse(SemaTable().Contains(word), "a Release that found no waiter left a bucket behind");

        RuntimeSemaphore.Acquire(word, WaitReason.Semacquire);
        Assert.AreEqual(0u, word.Value, "the kept permit was taken without parking");
        Assert.IsFalse(SemaTable().Contains(word), "an Acquire that did not park left a bucket behind");
    }

    // Go's easy cases (cansemacquire, and semrelease1's nwait check) touch no semaRoot and allocate nothing.
    // A bucket made and retired on every uncontended pair cost 240 B a pair: runtime.ReadMemStats stops the
    // world on this table and is held to zero allocations (GcMeasurementSurfaceProbes).
    [TestMethod]
    public void AnUncontendedPairAllocatesNothing()
    {
        heap<uint32>(out var word);

        for (int i = 0; i < 64; i++)
        {
            RuntimeSemaphore.Release(word, handoff: true);
            RuntimeSemaphore.Acquire(word, WaitReason.Semacquire);
        }

        const int pairs = 2000;
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < pairs; i++)
        {
            RuntimeSemaphore.Release(word, handoff: true);
            RuntimeSemaphore.Acquire(word, WaitReason.Semacquire);
        }

        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.AreEqual(0L, allocated, $"{pairs} uncontended Release/Acquire pairs allocated {allocated} B ({allocated / (double)pairs:F1} B a pair)");
        Assert.AreEqual(0u, word.Value, "every permit released was acquired");
        Assert.IsFalse(SemaTable().Contains(word), "an uncontended pair left a bucket behind");
    }

    // The owner of a word that blocked once must be collectable once nothing else holds it.
    [TestMethod]
    public void TheOwnerOfAWordThatBlockedOnceIsCollected()
    {
        WeakReference owner = BlockOnceInAFreshOwner();

        for (int i = 0; i < 3 && owner.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        Assert.IsFalse(owner.IsAlive, "the semaphore table kept the owner of a word that blocked once");
    }

    // Removal must not lose a permit or a waiter: a word seeded with one permit, used as a lock by several
    // threads, keeps an unguarded counter exact, and has no bucket at the end.
    [TestMethod]
    public void RemovalLosesNoPermitAndNoWaiterUnderContention()
    {
        const int Rounds = 20000;

        foreach (int threads in new[] { 2, 4, 8 })
        {
            heap<uint32>(out var word);
            word.Value = 1;
            long total = 0;
            using ManualResetEventSlim start = new(false);
            Thread[] workers = new Thread[threads];

            for (int t = 0; t < threads; t++)
            {
                workers[t] = new Thread(() =>
                {
                    start.Wait();

                    for (int i = 0; i < Rounds; i++)
                    {
                        RuntimeSemaphore.Acquire(word, WaitReason.Semacquire);
                        total++;
                        RuntimeSemaphore.Release(word, handoff: false);
                    }
                }) { IsBackground = true };
                workers[t].Start();
            }

            start.Set();

            foreach (Thread worker in workers)
                Assert.IsTrue(worker.Join(TimeSpan.FromMinutes(2)), $"a worker did not finish: a lost wakeup ({threads} threads)");

            Assert.AreEqual((long)threads * Rounds, total, $"{threads} threads incrementing under the semaphore");
            Assert.AreEqual(1u, word.Value, "the one permit is back in the word");
            Assert.IsFalse(SemaTable().Contains(word), $"the word kept a bucket after every waiter left ({threads} threads)");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference BlockOnceInAFreshOwner()
    {
        heap<uint32>(out var word);
        BlockOnceAndRelease(word);
        return new WeakReference(word);
    }

    // One goroutine parks on the empty word; the release wakes it.
    private static void BlockOnceAndRelease(ж<uint32> word)
    {
        Thread waiter = new(() => RuntimeSemaphore.Acquire(word, WaitReason.Semacquire)) { IsBackground = true };
        waiter.Start();

        SpinWait spin = default;
        DateTime deadline = DateTime.UtcNow.AddSeconds(30);

        while (RuntimeSemaphore.Waiters(word) == 0)
        {
            Assert.IsTrue(DateTime.UtcNow < deadline, "the waiter never parked");
            spin.SpinOnce();
        }

        RuntimeSemaphore.Release(word, handoff: false);
        Assert.IsTrue(waiter.Join(TimeSpan.FromSeconds(30)), "the waiter was not woken");
    }

    private static IDictionary SemaTable() =>
        (IDictionary)typeof(RuntimeSemaphore).GetField("semaTable", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
}
