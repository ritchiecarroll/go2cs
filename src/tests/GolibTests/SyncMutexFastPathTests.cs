#nullable enable

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using sync = go.sync_package;

namespace GolibTests;

/// <summary>
/// sync.Mutex's uncontended Lock takes the free lock before any wait accounting, as Go's Lock takes it with one
/// CompareAndSwap before lockSlow: an uncontended Lock/Unlock pair allocates nothing and does not park. A contended
/// Lock still parks, with Go's reason.
/// </summary>
/// <remarks>
/// Both run ON A GOROUTINE: Goroutine.Park is inert on a thread with no goroutine identity, so a pair measured on the
/// test thread would read zero bytes whatever Lock does. The M:N spike's cost table found the uncontended pair at
/// 230 ns and 64 B on Windows x64 (Go: 3.8 ns, 0 B), all of it the park bracket Lock entered before trying the lock
/// (BOARD, 2026-11-11, G).
/// </remarks>
[TestClass]
public class SyncMutexFastPathTests
{
    private const int TimeoutMs = 30000;

    private const int Pairs = 1000;

    private sealed class Holder
    {
        internal sync.Mutex Mu;
    }

    [TestMethod]
    public void AnUncontendedLockUnlockPairAllocatesNothing()
    {
        long bytes = -1;
        Exception? failure = null;
        using ManualResetEventSlim finished = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                Holder h = new();

                // Warm up: the gate is created on first use, and JIT and first-call initialization land here.
                for (int i = 0; i < 16; i++)
                {
                    h.Mu.Lock();
                    h.Mu.Unlock();
                }

                long before = GC.GetAllocatedBytesForCurrentThread();

                for (int i = 0; i < Pairs; i++)
                {
                    h.Mu.Lock();
                    h.Mu.Unlock();
                }

                bytes = GC.GetAllocatedBytesForCurrentThread() - before;
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                finished.Set();
            }
        });

        Assert.IsTrue(finished.Wait(TimeoutMs), "the goroutine did not finish");
        Assert.IsNull(failure, failure?.ToString());
        Assert.AreEqual(0L, bytes, $"{Pairs} uncontended Lock/Unlock pairs on a goroutine allocated {bytes} B " +
            $"({(double)bytes / Pairs:F1} B a pair); an uncontended Lock must take the free lock before it parks");
    }

    [TestMethod]
    public void AContendedLockStillParksWithGosReason()
    {
        Holder h = new();
        h.Mu.Lock();

        Goroutine? waiter = null;
        using ManualResetEventSlim started = new(false);
        using ManualResetEventSlim acquired = new(false);

        Goroutine.Start(() =>
        {
            waiter = Goroutine.Current;
            started.Set();
            h.Mu.Lock();
            acquired.Set();
            h.Mu.Unlock();
        });

        Assert.IsTrue(started.Wait(TimeoutMs), "the waiter did not start");

        // The waiter blocks in Lock; its park names Go's reason.
        bool parked = SpinWait.SpinUntil(
            () => waiter!.State == GoroutineState.Parked && waiter.Reason == WaitReason.SyncMutexLock, TimeoutMs);

        WaitReason seen = waiter!.Reason;
        h.Mu.Unlock();

        Assert.IsTrue(acquired.Wait(TimeoutMs), "the waiter did not acquire after Unlock");
        Assert.IsTrue(parked, $"a goroutine blocked in Lock reads {seen}, not a park on {WaitReason.SyncMutexLock}");
    }
}
