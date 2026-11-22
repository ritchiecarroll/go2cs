using System;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// The Gosched-park seat (COORD's ruling of 2026-11-21, docs/phase4/inbox/G/20261121T231225Z-COORD.md), P3's check (a):
/// off a carrier, <c>builtin.Goschedꞏtwin</c> is runtime.Gosched's own thread yield and returns an ALREADY-COMPLETED
/// task, so an await of it runs on at once. GoScheduler.GoschedTwin decides by the calling thread alone (is it a
/// carrier with a goroutine mounted), so the two off-carrier cases are one path: the scheduler off (GO2CS_MN unset,
/// this test process), and the scheduler on with the call made on a thread that is not a carrier (a foreign
/// callback, a plain thread), which the second row stands for.
/// </summary>
[TestClass]
public class GoschedParkTests
{
    [TestMethod]
    public void OffACarrierTheGoschedTwinCompletesSynchronously()
    {
        GoTask task = builtin.Goschedꞏtwin();

        Assert.IsTrue(task.GetAwaiter().IsCompleted, "off a carrier, the Gosched twin returns a completed task");
        task.GetAwaiter().GetResult();
    }

    [TestMethod]
    public void OnAThreadThatIsNotACarrierTheGoschedTwinCompletesSynchronously()
    {
        bool completed = false;
        Exception? failure = null;

        Thread thread = new(() =>
        {
            try
            {
                GoTask task = builtin.Goschedꞏtwin();
                completed = task.GetAwaiter().IsCompleted;
                task.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.Start();
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(30)), "the call returned");
        Assert.IsNull(failure, failure?.ToString());
        Assert.IsTrue(completed, "on a non-carrier thread, the Gosched twin returns a completed task");
    }

    /// <summary>
    /// P3's read of the Gosched seat: the yield mark must not outlive a failed park. It lives on the SLOT (no carrier-wide
    /// flag), and the carrier's commit decision reads the pending slot alone (<c>GoScheduler.RequeuesOnCommit</c>), so a
    /// park attempt that throws leaves nothing behind that could turn the step's earlier park into a requeue. A mounted
    /// carrier cannot be made in-process here (the scheduler is switched on at type load), so the throw is the off-carrier
    /// one; the commit path itself is read end to end by MnSpinCap and MnLockPin's gosched shape.
    /// </summary>
    [TestMethod]
    public void TheYieldMarkLivesOnTheSlotSoAFailedParkCannotRequeueAnEarlierOne()
    {
        GoParkSlot earlier = new();
        GoParkSlot yield = GoScheduler.YieldSlot();

        Assert.IsFalse(GoScheduler.RequeuesOnCommit(earlier), "an ordinary park is committed");
        Assert.IsTrue(GoScheduler.RequeuesOnCommit(yield), "a Gosched yield is requeued");

        Assert.ThrowsException<InvalidOperationException>(() => GoScheduler.ParkPending(yield, () => { }), "a park off a carrier throws");

        Assert.IsFalse(GoScheduler.RequeuesOnCommit(earlier), "the failed yield park left no mark: the earlier park is still committed");
        Assert.IsFalse(new GoParkSlot().IsYield, "no other slot is born a yield");
    }
}
