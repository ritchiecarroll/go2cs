#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using sync = go.sync_package;

namespace GolibTests;

/// <summary>
/// sync.Mutex has Go's layout, eight bytes and no managed reference, so it works in memory the managed heap
/// does not own (sqlite-cs W9a). sqlite's lib/mutex.go callocs a struct that embeds a sync.Mutex, and libc's
/// pthread mutex overlay holds one; a Mutex that carried a SemaphoreSlim reference could not be read out of
/// such memory at all ("field Mutex is a managed reference").
/// </summary>
/// <remarks>
/// The behavioral program MutexInNativeMemory runs sqlite's own shapes end to end; these arms pin the layout
/// and the lock protocol below any converted code.
/// </remarks>
[TestClass]
public unsafe class SyncMutexLayoutTests
{
    private const int Increments = 20000;

    [TestMethod]
    public void AMutexIsGosEightBytesWithNoManagedReference()
    {
        Assert.AreEqual(8, Unsafe.SizeOf<sync.Mutex>(), "Go's sync.Mutex is {state int32; sema uint32}");
        Assert.IsFalse(RuntimeHelpers.IsReferenceOrContainsReferences<sync.Mutex>(),
            "a Mutex must hold no managed reference, or it cannot live in native memory");
    }

    [TestMethod]
    public void AMutexInNativeMemoryCountsExactlyUnderContention()
    {
        foreach (int threads in new[] { 2, 4, 8 })
        {
            Assert.AreEqual((long)threads * Increments, CountInNativeMemory(threads, default),
                $"{threads} threads incrementing a native counter under a native Mutex");
        }
    }

    // Bytes the mutex never wrote, as recycled C memory that was not zeroed can hold: an unlocked state with a
    // queue handle no queue answers to. The first contention installs a real queue in its place.
    [TestMethod]
    public void AMutexWhoseHandleNamesNoQueueInstallsOneOnContention()
    {
        Assert.AreEqual(4L * Increments, CountInNativeMemory(4, 0xDEADBEEFu), "4 threads over a stale queue handle");
    }

    [TestMethod]
    public void TryLockFailsWhileHeldAndSucceedsWhenFree()
    {
        sync.Mutex* m = (sync.Mutex*)NativeMemory.AllocZeroed((nuint)sizeof(sync.Mutex));

        try
        {
            ref sync.Mutex mu = ref *m;
            mu.Lock();
            Assert.IsFalse(mu.TryLock(), "TryLock on a held mutex");
            mu.Unlock();
            Assert.IsTrue(mu.TryLock(), "TryLock on a free mutex");
            mu.Unlock();
        }
        finally
        {
            NativeMemory.Free(m);
        }
    }

    // A native block laid out as [Mutex][int64 counter], the counter incremented without atomics, so only the
    // mutex keeps the total exact.
    private static long CountInNativeMemory(int threads, uint staleHandle)
    {
        byte* block = (byte*)NativeMemory.AllocZeroed(64);

        try
        {
            sync.Mutex* m = (sync.Mutex*)block;
            long* total = (long*)(block + 16);
            ((uint*)block)[1] = staleHandle;

            using ManualResetEventSlim start = new(false);
            Thread[] workers = new Thread[threads];

            for (int t = 0; t < threads; t++)
            {
                workers[t] = new Thread(() =>
                {
                    start.Wait();

                    for (int i = 0; i < Increments; i++)
                    {
                        m->Lock();
                        *total += 1;
                        m->Unlock();
                    }
                }) { IsBackground = true };
                workers[t].Start();
            }

            start.Set();

            foreach (Thread worker in workers)
            {
                Assert.IsTrue(worker.Join(TimeSpan.FromMinutes(2)), "a worker did not finish: a lost wakeup");
            }

            Assert.AreEqual(0, ((int*)block)[0], "the mutex is unlocked after every worker unlocked it");
            return *total;
        }
        finally
        {
            NativeMemory.Free(block);
        }
    }
}
