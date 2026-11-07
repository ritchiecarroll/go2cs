using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

/// <summary>
/// Go's DIRECT HANDOFF (sema.go semrelease1): a release with handoff that actually hands the permit to a
/// waiter then calls goyield, queueing the RELEASER behind the readied waiter so the waiter runs first.
/// runtime's TestSemaHandoff asserts exactly that ordering (the waiter's CAS must win in >= 2/3 of its
/// trials; Go measures >90%). RuntimeSemaphore signalled the waiter and returned, so the releaser -- still on
/// its own thread -- won the race in nanoseconds: 1-8 per 10,000 on P1's linux readings.
/// </summary>
/// <remarks>
/// <para>
/// <b>What is asserted, and why not Go's ratio.</b> In Go both goroutines share one P, so the yield fixes
/// the order. Here each is its own OS thread, and once the waiter has resumed, which of the two reaches the
/// CAS first is the HOST scheduler's choice. A loaded box can read 14 of 1000 with the yield working as
/// designed. The losses split two ways, and neither is code. (A) The waiter's thread was not scheduled
/// within the 10 ms bound, so the releaser went on, as Go's sysmon preemption would let it. (B) The waiter
/// resumed and released the releaser, then lost the core before its next instruction. The ratio is printed
/// and not asserted.
/// </para>
/// <para>
/// What the yield GUARANTEES is asserted per trial, with no tolerance: unless the bound fired, the waiter
/// has left its park before Release returns. The waiter clears its wait reason when its park scope
/// disposes, and only then sets the event the releaser waits on, so this holds on any scheduler. A
/// release that does not yield returns while the waiter is still parked (readied, not yet running) and
/// fails it in nearly every trial. That is the regression the ratio existed to catch.
/// </para>
/// <para>
/// <b>The limit, the same for the old ratio and this check.</b> A waiter that never sets the event passes
/// both: every trial then rides the bound. The ratio passed it because the waiter wins the CAS in
/// microseconds once the releaser has waited 10 ms. No deterministic predicate separates that from a
/// loaded box, where the bound fires too, so it is named here and not asserted.
/// </para>
/// </remarks>
[TestClass]
public class RuntimeSemaphoreHandoffTests
{
    private const int Trials = 1000;
    private const int TimeoutMs = 60000;

    private readonly record struct HandoffReading(int Ok, long Yields, int BoundHits, int ParkedAtReturn);

    // The replica of runtime's testSemaHandoff over the primitive itself, run on a goroutine (the MSTest
    // thread holds a lent identity and must not be the one that parks or readies).
    private static HandoffReading runHandoffTrials(int trials)
    {
        int ok = 0, boundHits = 0, parkedAtReturn = 0;
        long yieldsBefore = Interlocked.Read(ref RuntimeSemaphore.s_handoffYields);
        using ManualResetEventSlim finished = new(false);

        Goroutine.Start(() =>
        {
            for (int i = 0; i < trials; i++)
            {
                ж<uint32> sema = new StandardBox<uint32>(0u);
                int res = 0;
                Goroutine? waiter = null;
                using ManualResetEventSlim waiterDone = new(false);

                Goroutine.Start(() =>
                {
                    Volatile.Write(ref waiter, Goroutine.Current);
                    RuntimeSemaphore.Acquire(sema, WaitReason.Semacquire);
                    Interlocked.CompareExchange(ref res, 1, 0);
                    RuntimeSemaphore.Release(sema, true);
                    waiterDone.Set();
                });

                // `for SemNwait(&sema) == 0 { Gosched() }` -- wait for the goroutine to block in Semacquire.
                while (RuntimeSemaphore.Waiters(sema) == 0)
                    Thread.Yield();

                // The crux: release with handoff, then race the waiter's CAS.
                long timeoutsBefore = Interlocked.Read(ref RuntimeSemaphore.s_handoffYieldTimeouts);
                RuntimeSemaphore.Release(sema, true);
                bool parked = Volatile.Read(ref waiter)!.IsParked;
                Interlocked.CompareExchange(ref res, 2, 0);

                if (Interlocked.Read(ref RuntimeSemaphore.s_handoffYieldTimeouts) != timeoutsBefore)
                    boundHits++;
                else if (parked)
                    parkedAtReturn++;

                waiterDone.Wait(TimeoutMs);

                if (Volatile.Read(ref res) == 1)
                    ok++;
            }

            finished.Set();
        });

        Assert.IsTrue(finished.Wait(TimeoutMs * 2), "the handoff trials did not finish");
        return new(ok, Interlocked.Read(ref RuntimeSemaphore.s_handoffYields) - yieldsBefore, boundHits, parkedAtReturn);
    }

    [TestMethod]
    public void AHandoffReleaseLetsTheWaiterRunFirst_TestSemaHandoffsRatio()
    {
        HandoffReading r = runHandoffTrials(Trials);

        // The reading, printed so a run records the RATIO and not only the pass: Go measures >90%.
        System.Console.WriteLine($"SEMAHANDOFF direct={r.Ok}/{Trials} yields={r.Yields} boundHits={r.BoundHits} parkedAtReturn={r.ParkedAtReturn}");

        Assert.AreEqual((long)Trials, r.Yields, "exactly one releaser yield per permit actually handed off");
        Assert.AreEqual(0, r.ParkedAtReturn,
            $"a handoff release returned while its waiter was still parked in {r.ParkedAtReturn} of {Trials} trials " +
            $"(the bound fired in {r.BoundHits}): the releaser did not yield to the waiter (Go's goyield)");
    }

    [TestMethod]
    public void AReleaseThatHandsNothingOffNeverWaits_TheControl()
    {
        long before = Interlocked.Read(ref RuntimeSemaphore.s_handoffYields);
        using ManualResetEventSlim finished = new(false);

        Goroutine.Start(() =>
        {
            // handoff=true with NO waiter: the permit goes back to the count, nothing is handed off.
            ж<uint32> idle = new StandardBox<uint32>(0u);
            RuntimeSemaphore.Release(idle, true);
            RuntimeSemaphore.Acquire(idle, WaitReason.Semacquire);

            // handoff=false WITH a parked waiter: it is readied but must re-compete, and Go does not yield.
            ж<uint32> sema = new StandardBox<uint32>(0u);
            using ManualResetEventSlim waiterDone = new(false);

            Goroutine.Start(() =>
            {
                RuntimeSemaphore.Acquire(sema, WaitReason.Semacquire);
                waiterDone.Set();
            });

            while (RuntimeSemaphore.Waiters(sema) == 0)
                Thread.Yield();

            RuntimeSemaphore.Release(sema, false);
            waiterDone.Wait(TimeoutMs);
            finished.Set();
        });

        Assert.IsTrue(finished.Wait(TimeoutMs), "the control did not finish");
        Assert.AreEqual(before, Interlocked.Read(ref RuntimeSemaphore.s_handoffYields),
            "a release that handed no permit off must not yield -- Go's goyield is gated on s.ticket == 1");
    }
}
