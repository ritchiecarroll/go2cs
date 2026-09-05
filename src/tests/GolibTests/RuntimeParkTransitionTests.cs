using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards Q61, the park hook: golib's Goroutine.Park now drives the runtime's g between _Grunning and
/// _Gwaiting with Go's wait reason at the OUTERMOST scope, so Go's own GIsWaitingOnMutex predicate
/// becomes true while a goroutine waits on a mutex and casgstatus accumulates the mutex wait time
/// the /sync/mutex/wait/total:seconds metric reads (TestMutexWaitTimeMetric's whole mechanism). The
/// wait-reason map is checked by derivation — the two STRING tables agree — never by restating numbers.
/// </summary>
[TestClass]
public class RuntimeParkTransitionTests
{
    [TestMethod]
    public void AParkedGoroutineReadsGwaitingWithGosReasonAndReturnsToGrunning()
    {
        (uint running, uint waiting) = GoGStatusWords();
        (uint statusParked, string reasonParked, bool isMutexWait, uint statusAfter, string reasonAfter, long delta) = GoParkTransitionProbe(WaitReason.SyncMutexLock, 20);
        Assert.AreEqual(waiting, statusParked, "while parked the g is _Gwaiting — GIsWaitingOnMutex's first half");
        Assert.AreEqual("sync.Mutex.Lock", reasonParked, "with Go's own reason string");
        Assert.IsTrue(isMutexWait, "and it is a mutex wait — the predicate's second half");
        Assert.AreEqual(running, statusAfter, "released: _Grunning again");
        Assert.AreEqual("", reasonAfter, "and the reason cleared");
    }

    [TestMethod]
    public void TheMutexWaitMetricGrowsByAtLeastTheBlockedTimeUnderAlwaysTrack()
    {
        // Go multiplies the sampled wait by gTrackingPeriod (8); the metric test asserts >= the block
        // time, which holds for the sample and for the scaled estimate alike.
        (_, _, _, _, _, long delta) = GoParkTransitionProbe(WaitReason.SyncMutexLock, 50);
        Assert.IsTrue(delta >= 50L * 1_000_000L, $"totalMutexWaitTime grew by {delta} ns for a 50 ms block");
    }

    [TestMethod]
    public void ANonMutexWaitMovesTheStatusButNotTheMutexMetric()
    {
        (uint _, uint waiting) = GoGStatusWords();
        (uint statusParked, string reasonParked, bool isMutexWait, _, _, long delta) = GoParkTransitionProbe(WaitReason.ChanReceive, 20);
        Assert.AreEqual(waiting, statusParked);
        Assert.AreEqual("chan receive", reasonParked);
        Assert.IsFalse(isMutexWait);
        Assert.AreEqual(0L, delta, "a channel wait is not mutex wait time");
    }

    [TestMethod]
    public void NestedScopesMoveTheStatusOnlyAtTheOutermostBoundary()
    {
        (uint running, uint waiting) = GoGStatusWords();
        uint[] s = GoNestedParkProbe();
        CollectionAssert.AreEqual(new[] { running, waiting, waiting, waiting, running }, s, "before / outer / inner / after inner / after outer");
    }

    [TestMethod]
    public void TheWaitReasonMapIsGosOwnTableByDerivation()
    {
        (string golibText, string runtimeText, bool isMutexWait)[] rows = GoWaitReasonMapProbe();
        Assert.IsTrue(rows.Length >= 13, $"{rows.Length} parked reasons");

        foreach ((string golibText, string runtimeText, bool isMutexWait) in rows)
        {
            Assert.AreEqual(golibText, runtimeText, "golib's string for the reason must be the runtime table's string for the mapped value");
            Assert.AreEqual(golibText.StartsWith("sync.Mutex.Lock") || golibText.StartsWith("sync.RWMutex."), isMutexWait, $"isMutexWait for {golibText}");
        }
    }

    [TestMethod]
    public void AThreadWithoutGoroutineIdentityParksInertly()
    {
        // The test thread is not a goroutine: Park returns the inert scope and the hook is never invoked.
        Exception? thrown = null;
        Thread t = new(() =>
        {
            try { using (Goroutine.Park(WaitReason.SyncMutexLock)) { Thread.Sleep(5); } }
            catch (Exception e) { thrown = e; }
        });
        t.Start(); t.Join();
        Assert.IsNull(thrown, thrown?.ToString());
    }
}
