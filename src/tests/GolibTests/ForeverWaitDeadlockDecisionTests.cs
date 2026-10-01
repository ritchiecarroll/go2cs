using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

// The deadlock report's decision table (Goroutine.IsProvableDeadlock), pinned row by row.
//
// Go reports "all goroutines are asleep - deadlock!" when nothing can run again (runtime.checkdead).
// A managed goroutine can also be woken by things that are not goroutines -- timer callbacks, I/O
// completions, signals, host threads -- so golib reports only what it can PROVE: every user goroutine,
// main included, parked in a FOREVER wait (a nil-channel receive or send, or a select with no live case),
// which nothing can ever end. The process-level arms are behavioral: ForeverWaitWorkersMainReturns,
// MainSelectForeverWorkerExits and MainSelectForeverAfterFunc must NOT report; ChannelReceiveFromNil and
// ChannelSendToNil (main alone) must.
//
// THE NAMED RESIDUAL, pinned here on purpose: a deadlock through an ORDINARY channel or sync wait blocks
// instead of reporting. Go would report `main nil-blocked while another goroutine waits on a real
// channel nobody will send to`; golib cannot tell that wait from one a timer or an I/O completion will
// end, so it does not. A later full checkdead (one that can account for every non-goroutine waker) flips
// the residual rows below DELIBERATELY.
[TestClass]
public class ForeverWaitDeadlockDecisionTests
{
    private static bool Decide(params (WaitReason Reason, bool Readied, bool IsMain)[] users) =>
        Goroutine.IsProvableDeadlock(users);

    [TestMethod]
    public void MainAloneInAForeverWaitIsADeadlock()
    {
        Assert.IsTrue(Decide((WaitReason.ChanReceiveNilChan, false, true)), "main alone on a nil-channel receive");
        Assert.IsTrue(Decide((WaitReason.ChanSendNilChan, false, true)), "main alone on a nil-channel send");
        Assert.IsTrue(Decide((WaitReason.SelectNoCases, false, true)), "main alone in select {}");
    }

    [TestMethod]
    public void EveryGoroutineInAForeverWaitIsADeadlock()
    {
        Assert.IsTrue(Decide(
            (WaitReason.SelectNoCases, false, true),
            (WaitReason.ChanReceiveNilChan, false, false),
            (WaitReason.ChanSendNilChan, false, false)));
    }

    // Go's `go serve(); select {}`: a worker that can still run keeps main's forever wait from being a
    // deadlock. Running (Zero), sleeping and I/O-waiting workers can all still make progress.
    [TestMethod]
    public void AWorkerThatCanStillRunIsNotADeadlock()
    {
        Assert.IsFalse(Decide((WaitReason.SelectNoCases, false, true), (WaitReason.Zero, false, false)), "a running worker");
        Assert.IsFalse(Decide((WaitReason.SelectNoCases, false, true), (WaitReason.Sleep, false, false)), "a sleeping worker");
        Assert.IsFalse(Decide((WaitReason.SelectNoCases, false, true), (WaitReason.IOWait, false, false)), "a worker in I/O");
    }

    // Main still running (the -tests host's main entry, a test framework thread): never a deadlock,
    // whatever the other goroutines do -- the net/rpc TestSendDeadlock shape, a leaked select {} goroutine.
    [TestMethod]
    public void MainNotInAForeverWaitIsNeverADeadlock()
    {
        Assert.IsFalse(Decide((WaitReason.Zero, false, true), (WaitReason.SelectNoCases, false, false)));
        Assert.IsFalse(Decide((WaitReason.Sleep, false, true), (WaitReason.ChanReceiveNilChan, false, false)));
        Assert.IsFalse(Decide((WaitReason.SelectNoCases, false, false)), "no main among the goroutines");
        Assert.IsFalse(Decide(), "no goroutines at all");
    }

    // A goroutine already readied by a waker is about to run.
    [TestMethod]
    public void AReadiedGoroutineIsNotADeadlock()
    {
        Assert.IsFalse(Decide((WaitReason.SelectNoCases, true, true)));
    }

    // THE NAMED RESIDUAL. Go reports each of these; golib blocks instead, because an ordinary channel or
    // sync wait can be ended by a non-goroutine waker it cannot see. Flip these deliberately, with a full
    // checkdead, never by accident.
    [TestMethod]
    public void ResidualAnOrdinaryChannelOrSyncWaitBlocksInsteadOfReporting()
    {
        Assert.IsFalse(Decide((WaitReason.ChanReceiveNilChan, false, true), (WaitReason.ChanReceive, false, false)), "main nil-blocked, a worker on a real channel receive");
        Assert.IsFalse(Decide((WaitReason.SelectNoCases, false, true), (WaitReason.ChanSend, false, false)), "main in select {}, a worker on a real channel send");
        Assert.IsFalse(Decide((WaitReason.SelectNoCases, false, true), (WaitReason.Select, false, false)), "main in select {}, a worker in a select with live cases");
        Assert.IsFalse(Decide((WaitReason.SelectNoCases, false, true), (WaitReason.SyncMutexLock, false, false)), "main in select {}, a worker on a mutex");
        Assert.IsFalse(Decide((WaitReason.ChanReceive, false, true)), "main alone on a real channel receive");
    }
}
