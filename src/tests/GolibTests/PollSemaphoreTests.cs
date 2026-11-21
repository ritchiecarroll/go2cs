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
using poll_package = go.@internal.poll_package;

namespace GolibTests;

/// <summary>
/// internal/poll's runtime semaphore (fd_mutex.go's runtime_Semacquire and runtime_Semrelease, which Go's runtime
/// provides as poll_runtime_Semacquire and poll_runtime_Semrelease in sema.go) keeps nothing for a word once its
/// waiters have left, so the word's owner can be collected.
/// </summary>
/// <remarks>
/// Until 2026-11-21 poll kept a private table whose bucket held the count, so no bucket could ever be removed: every
/// fdMutex word that once blocked kept its bucket, and the key (the pointer box over the word) kept the word's owner,
/// an os.File or a netFD, alive for the life of the process. The arms call poll's own entry points (internal, so by
/// reflection) and look in every semaphore table there is (also by reflection), so they hold whichever primitive
/// serves poll. RuntimeSemaphoreBucketTests holds the same properties for the shared primitive itself.
/// </remarks>
[TestClass]
public class PollSemaphoreTests
{
    private const int Words = 100;

    private static readonly Action<ж<uint32>> Semacquire = PollEntry("runtime_Semacquire");
    private static readonly Action<ж<uint32>> Semrelease = PollEntry("runtime_Semrelease");

    [TestMethod]
    public void APollWordThatBlockedOnceKeepsNoBucket()
    {
        ж<uint32>[] words = new ж<uint32>[Words];

        for (int i = 0; i < Words; i++)
        {
            heap<uint32>(out words[i]);
            BlockOnceAndRelease(words[i]);
        }

        int kept = 0;

        foreach (ж<uint32> word in words)
        {
            if (KeptAnywhere(word))
                kept++;
        }

        Assert.AreEqual(0, kept, $"{kept} of {Words} words that each blocked once through internal/poll still have a bucket");
    }

    // The owner of a word that blocked once through poll must be collectable once nothing else holds it. Like its
    // twin in RuntimeSemaphoreBucketTests, this arm depends on the GC collecting the owner within three passes, and
    // BlockOnceInAFreshOwner is NoInlining so that no local of this frame keeps the word alive.
    [TestMethod]
    public void TheOwnerOfAPollWordThatBlockedOnceIsCollected()
    {
        WeakReference owner = BlockOnceInAFreshOwner();

        for (int i = 0; i < 3 && owner.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        Assert.IsFalse(owner.IsAlive, "internal/poll's semaphore kept the owner of a word that blocked once");
    }

    // A word given one permit through poll's own Semrelease, then used as a lock by several threads through poll's
    // entry points, keeps an unguarded counter exact, still holds its one permit at the end (a last acquire takes it
    // without parking), and keeps no bucket.
    [TestMethod]
    public void PollLosesNoPermitAndNoWaiterUnderContention()
    {
        const int Rounds = 20000;

        foreach (int threads in new[] { 2, 4, 8 })
        {
            heap<uint32>(out var word);
            Semrelease(word);
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
                        Semacquire(word);
                        total++;
                        Semrelease(word);
                    }
                }) { IsBackground = true };
                workers[t].Start();
            }

            start.Set();

            foreach (Thread worker in workers)
                Assert.IsTrue(worker.Join(TimeSpan.FromMinutes(2)), $"a worker did not finish: a lost wakeup ({threads} threads)");

            Assert.AreEqual((long)threads * Rounds, total, $"{threads} threads incrementing under poll's semaphore");

            Thread last = new(() => Semacquire(word)) { IsBackground = true };
            last.Start();
            Assert.IsTrue(last.Join(TimeSpan.FromSeconds(30)), $"the one permit was lost: a last acquire parked ({threads} threads)");
            Assert.IsFalse(KeptAnywhere(word), $"the word kept a bucket after every waiter left ({threads} threads)");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference BlockOnceInAFreshOwner()
    {
        heap<uint32>(out var word);
        BlockOnceAndRelease(word);
        return new WeakReference(word);
    }

    // One thread parks on the empty word through poll's Semacquire; poll's Semrelease wakes it. The waiter is queued
    // before it waits, so a release that lands while it is still spinning finds it queued all the same.
    private static void BlockOnceAndRelease(ж<uint32> word)
    {
        Thread waiter = new(() => Semacquire(word)) { IsBackground = true };
        waiter.Start();

        SpinWait spin = default;
        DateTime deadline = DateTime.UtcNow.AddSeconds(30);

        while ((waiter.ThreadState & ThreadState.WaitSleepJoin) == 0)
        {
            Assert.IsTrue(DateTime.UtcNow < deadline, "the waiter never blocked");
            spin.SpinOnce();
        }

        Semrelease(word);
        Assert.IsTrue(waiter.Join(TimeSpan.FromSeconds(30)), "the waiter was not woken");
    }

    // Whether any semaphore table holds a bucket for the word: the shared primitive's, and poll's own if it has one.
    private static bool KeptAnywhere(ж<uint32> word)
    {
        if (Table(typeof(RuntimeSemaphore)) is { } shared && shared.Contains(word))
            return true;

        return Table(typeof(poll_package)) is { } own && own.Contains(word);
    }

    private static IDictionary? Table(Type owner) =>
        owner.GetField("semaTable", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as IDictionary;

    private static Action<ж<uint32>> PollEntry(string name) =>
        (typeof(poll_package).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(nameof(poll_package), name)).CreateDelegate<Action<ж<uint32>>>();
}
