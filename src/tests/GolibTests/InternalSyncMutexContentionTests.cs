using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.@internal;
using isync = go.@internal.sync_package;

namespace GolibTests;

/// <summary>
/// A CONTENDED <c>internal/sync.Mutex</c>: the path its runtime hooks sit on.
/// </summary>
/// <remarks>
/// <para>
/// Go 1.24 moved Mutex into <c>internal/sync</c>, and <c>lockSlow</c> calls five runtime hooks the
/// runtime pushes: <c>runtime_canSpin</c>, <c>runtime_doSpin</c>, <c>runtime_nanotime</c>,
/// <c>runtime_SemacquireMutex</c> and <c>runtime_Semrelease</c> (<c>internal/sync/runtime.go</c>).
/// Until <c>src/core/internal/sync/runtime_impl.cs</c> bodied them, all five were throwing
/// <c>PartialStubGenerator</c> stubs. An UNCONTENDED Lock never reached them, because the CAS fast
/// path returns before <c>lockSlow</c>. That is why <c>internal/sync</c>'s own suite validated while
/// any converted caller that contended one died on the <c>canSpin</c> stub.
/// </para>
/// <para>
/// So both arms here force contention rather than hoping for it. The first makes it deterministic:
/// the test thread holds the lock before the waiter starts, so the waiter's CAS must fail and
/// <c>lockSlow</c> must run. Every worker's exception is captured and asserted by name, because an
/// unhandled exception on a raw thread would take the test host down instead of failing an arm.
/// </para>
/// <para>
/// RED at <c>8fc439415f</c>: the waiter's first call in <c>lockSlow</c> is <c>runtime_canSpin</c>,
/// which throws <c>NotImplementedException</c>. Bodying <c>canSpin</c> ALONE would still leave
/// these arms red one line later, at <c>runtime_nanotime</c> and then
/// <c>runtime_SemacquireMutex</c>. Only all five hooks together make it green.
/// </para>
/// </remarks>
[TestClass]
public class InternalSyncMutexContentionTests
{
    private const int TimeoutMs = 30000;

    [TestMethod]
    public void AContendedLockParksUntilTheHolderUnlocks()
    {
        heap<isync.Mutex>(out var Ꮡmu);
        Ꮡmu.Lock();

        Exception? failure = null;
        // NOT disposed: if an arm times out (a lost wakeup), the waiter may still Set it after this
        // method returns, and Set on a disposed event throws on a background thread, which takes
        // the test host down instead of failing the arm.
        ManualResetEventSlim done = new(false);
        bool acquired = false;

        Thread waiter = new(() =>
        {
            try
            {
                Ꮡmu.Lock(); // the CAS fails: the test thread holds it, so lockSlow runs
                Volatile.Write(ref acquired, true);
                Ꮡmu.Unlock();
            }
            catch (Exception e)
            {
                failure = e;
            }
            finally
            {
                done.Set();
            }
        })
        { IsBackground = true, Name = "isync-waiter" };

        waiter.Start();

        // The waiter must still be blocked while this thread holds the lock. 200 ms is also well
        // past starvationThresholdNs (1 ms), so on waking the waiter has marked itself starving,
        // which puts the handoff logic of lockSlow on the path as well.
        bool finishedEarly = done.Wait(200);

        Assert.IsNull(failure, $"a contended internal/sync.Mutex.Lock threw: {failure}");
        Assert.IsFalse(finishedEarly, "the waiter acquired an internal/sync.Mutex another thread holds");
        Assert.IsFalse(Volatile.Read(ref acquired), "the waiter acquired an internal/sync.Mutex another thread holds");

        Ꮡmu.Unlock(); // unlockSlow: runtime_Semrelease must wake the parked waiter

        Assert.IsTrue(done.Wait(TimeoutMs), "Unlock did not wake the parked waiter (a lost wakeup)");
        Assert.IsNull(failure, $"the woken waiter threw: {failure}");
        Assert.IsTrue(Volatile.Read(ref acquired), "the waiter finished without acquiring the mutex");
    }

    [TestMethod]
    public void TwoGoroutinesCompleteEveryRoundUnderContention()
    {
        const int Rounds = 5000;

        heap<isync.Mutex>(out var Ꮡmu);

        int inside = 0;   // 1 while some thread is in the critical section
        int overlaps = 0; // times a thread entered while the other was inside
        long counter = 0; // incremented WITHOUT atomics: only the mutex protects it
        Exception?[] failures = new Exception?[2];
        Barrier start = new(2); // not disposed, for the same reason as the first arm's event

        Thread[] workers = new Thread[2];

        for (int w = 0; w < workers.Length; w++)
        {
            int id = w;

            workers[w] = new Thread(() =>
            {
                try
                {
                    start.SignalAndWait();

                    for (int i = 0; i < Rounds; i++)
                    {
                        Ꮡmu.Lock();

                        if (Interlocked.Exchange(ref inside, 1) != 0)
                            Interlocked.Increment(ref overlaps);

                        counter++;

                        // Now and then hold the lock past starvationThresholdNs (1 ms), so the other
                        // worker's wait crosses it. That drives the mutex into starvation mode and
                        // puts unlockSlow's handoff release (Semrelease with handoff = true) on the path.
                        if (i % 1000 == 0)
                            Thread.Sleep(2);

                        Volatile.Write(ref inside, 0);
                        Ꮡmu.Unlock();
                    }
                }
                catch (Exception e)
                {
                    failures[id] = e;
                }
            })
            { IsBackground = true, Name = $"isync-worker-{id}" };
        }

        foreach (Thread t in workers)
            t.Start();

        foreach (Thread t in workers)
            Assert.IsTrue(t.Join(TimeoutMs), $"{t.Name} did not finish: a lost wakeup or a deadlock in lockSlow");

        Assert.IsNull(failures[0], $"isync-worker-0 threw: {failures[0]}");
        Assert.IsNull(failures[1], $"isync-worker-1 threw: {failures[1]}");
        Assert.AreEqual(0, overlaps, "two threads were inside one internal/sync.Mutex at once");
        Assert.AreEqual(2L * Rounds, counter, "a round was lost or doubled under the mutex");
    }
}
