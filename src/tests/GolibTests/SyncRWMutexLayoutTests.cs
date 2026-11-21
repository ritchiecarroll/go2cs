#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using sync = go.sync_package;

namespace GolibTests;

/// <summary>
/// sync.RWMutex has Go's layout and runs Go's own code: {w Mutex; writerSem, readerSem uint32; readerCount,
/// readerWait atomic.Int32}, twenty-four bytes and no managed reference. A copy is a separate lock, it works in
/// memory the managed heap does not own, and reflect sees its five fields.
/// </summary>
/// <remarks>
/// Before, the RWMutex was hand-owned around one managed RWState (a monitor and four counters): a copy shared
/// that state, so unlocking the original unlocked the copy; a native box refused it ("a managed reference");
/// and reflect.DeepEqual saw one opaque handle, so a held lock and a free one compared equal. Each arm below
/// failed there.
/// </remarks>
[TestClass]
public unsafe class SyncRWMutexLayoutTests
{
    private const int Increments = 20000;

    [TestMethod]
    public void ARWMutexIsGosTwentyFourBytesWithNoManagedReference()
    {
        Assert.AreEqual(24, Unsafe.SizeOf<sync.RWMutex>(),
            "Go's sync.RWMutex is {w Mutex; writerSem, readerSem uint32; readerCount, readerWait atomic.Int32}");
        Assert.IsFalse(RuntimeHelpers.IsReferenceOrContainsReferences<sync.RWMutex>(),
            "an RWMutex must hold no managed reference, or it cannot live in native memory");
    }

    // Go copies the words: a copy of a write-locked RWMutex is a second, separately write-locked RWMutex, and
    // unlocking the original leaves the copy as it was.
    [TestMethod]
    public void ACopyOfAWriteLockedRWMutexStaysLockedAfterTheOriginalUnlocks()
    {
        heap<sync.RWMutex>(out var original);
        original.Lock();
        heap<sync.RWMutex>(out var copy);
        copy.Value = original.Value;
        original.Unlock();

        Assert.IsTrue(original.TryLock(), "the original is free after its Unlock");
        original.Unlock();
        Assert.IsFalse(copy.TryLock(), "the copy was taken while write-locked, so it is still write-locked");
        Assert.IsFalse(copy.TryRLock(), "...and admits no reader");
    }

    [TestMethod]
    public void ACopyOfAReadLockedRWMutexKeepsItsReader()
    {
        heap<sync.RWMutex>(out var original);
        original.RLock();
        heap<sync.RWMutex>(out var copy);
        copy.Value = original.Value;
        original.RUnlock();

        Assert.IsTrue(original.TryLock(), "the original has no reader left");
        original.Unlock();
        Assert.IsFalse(copy.TryLock(), "the copy still holds the reader it was copied with");
        Assert.IsTrue(copy.TryRLock(), "...and admits another reader");
    }

    // A native block laid out as [RWMutex][int64 counter]: writers increment the counter without atomics and
    // readers check it never moves under them, so only the RWMutex keeps both exact. The pointer comes back
    // through the uintptr conversion converted code uses for `(*T)(unsafe.Pointer(p))`.
    [TestMethod]
    public void ARWMutexInNativeMemoryKeepsWritersExclusiveAndReadersConsistent()
    {
        // Native memory can hold no managed reference: asked first, so a layout that has one fails here rather
        // than handing the collector a reference it cannot see.
        Assert.IsFalse(RuntimeHelpers.IsReferenceOrContainsReferences<sync.RWMutex>(),
            "an RWMutex that holds a managed reference cannot live in native memory");

        foreach (int writers in new[] { 1, 2, 4 })
        {
            (long total, long torn) = CountInNativeMemory(writers, readers: 2);
            Assert.AreEqual((long)writers * Increments, total, $"{writers} writers incrementing under the native RWMutex");
            Assert.AreEqual(0L, torn, $"a reader saw the counter move while it held the read lock ({writers} writers)");
        }
    }

    // Go's reflect.DeepEqual walks the RWMutex's words, so a write-locked RWMutex differs from a free one, and two
    // free ones are equal.
    [TestMethod]
    public void DeepEqualSeesAHeldRWMutexDifferFromAFreeOne()
    {
        heap<sync.RWMutex>(out var held);
        heap<sync.RWMutex>(out var free);
        heap<sync.RWMutex>(out var freeToo);
        held.Lock();

        try
        {
            Assert.IsFalse(reflect_package.DeepEqual(held, free), "a write-locked RWMutex and a free one");
            Assert.IsTrue(reflect_package.DeepEqual(free, freeToo), "two free RWMutexes");
        }
        finally
        {
            held.Unlock();
        }

        Assert.IsTrue(reflect_package.DeepEqual(held, free), "released, it is a free RWMutex again");
    }

    private static (long total, long torn) CountInNativeMemory(int writers, int readers)
    {
        byte* block = (byte*)NativeMemory.AllocZeroed(64);

        try
        {
            ж<sync.RWMutex> rw = (ж<sync.RWMutex>)(uintptr)(nuint)block;
            long* total = (long*)(block + 32);
            long torn = 0;
            int writersLeft = writers;

            using ManualResetEventSlim start = new(false);
            Thread[] workers = new Thread[writers + readers];

            for (int t = 0; t < writers; t++)
            {
                workers[t] = new Thread(() =>
                {
                    start.Wait();

                    for (int i = 0; i < Increments; i++)
                    {
                        rw.Lock();
                        *total += 1;
                        rw.Unlock();
                    }

                    Interlocked.Decrement(ref writersLeft);
                }) { IsBackground = true };
            }

            for (int t = writers; t < writers + readers; t++)
            {
                workers[t] = new Thread(() =>
                {
                    start.Wait();

                    while (Volatile.Read(ref writersLeft) > 0)
                    {
                        rw.RLock();
                        long seen = Volatile.Read(ref *total);
                        Thread.SpinWait(20);

                        if (Volatile.Read(ref *total) != seen)
                            Interlocked.Increment(ref torn);

                        rw.RUnlock();
                    }
                }) { IsBackground = true };
            }

            foreach (Thread worker in workers)
                worker.Start();

            start.Set();

            foreach (Thread worker in workers)
                Assert.IsTrue(worker.Join(TimeSpan.FromMinutes(2)), "a worker did not finish: a lost wakeup");

            Assert.IsTrue(rw.TryLock(), "the RWMutex is free after every worker released it");
            rw.Unlock();
            return (*total, torn);
        }
        finally
        {
            NativeMemory.Free(block);
        }
    }
}
