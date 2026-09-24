using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// The runtime's managed gopark / ready / injectglist (runtime park_impl.cs; S1b of
/// DESIGN-gopark-goready-synctest.md). Before it, every converted park ended in mcall(park_m), a
/// refusing stub, and the runtime row's host died there on TestScavenger's harness goroutine
/// (scavengerState.park -> goparkunlock -> gopark -> mcall; woken by injectglist, not goready).
/// </summary>
[TestClass]
public class RuntimeGoparkPairTests
{
    private const int TimeoutMs = 30000;

    [TestMethod]
    public void AGoparkIsCommittedParkedAndReadiedByInjectglist()
    {
        (bool unlockfRan, uint statusParked, string reasonParked, bool resumed, string? failure) = GoGoparkInjectglistProbe(TimeoutMs);

        Assert.IsNull(failure, $"the parker failed: {failure}");
        Assert.IsTrue(unlockfRan, "gopark did not run its unlockf (Go's commit)");
        Assert.AreEqual(GoStatusWaiting, statusParked, "a goparked g reads _Gwaiting");
        Assert.AreEqual("GC scavenge wait", reasonParked, "the g carries the runtime's own reason");
        Assert.IsTrue(resumed, "injectglist did not ready the parked goroutine");
    }

    [TestMethod]
    public void AGoparkWhoseCommitRefusesReturnsRunning()
    {
        (bool returned, uint statusAfter, string? failure) = GoGoparkRefusedCommitProbe(TimeoutMs);

        Assert.IsNull(failure, $"the parker failed: {failure}");
        Assert.IsTrue(returned, "a refused commit must resume the goroutine without a ready (Go's park_m)");
        Assert.AreEqual(GoStatusRunning, statusAfter, "and leave its g _Grunning");
    }

    [TestMethod]
    public void AGoparkOffAGoroutineRefusesByName() =>
        StringAssert.Contains(GoGoparkOffGoroutineRefusal(), "gopark on a thread with no goroutine identity");

    [TestMethod]
    public void ReadyResolvesAGToItsOwnGoroutineAndAStrangerToNone()
    {
        // ready's refusal for a g no goroutine owns is Go's fatal throw ("bad g->status in ready"): it
        // ends the process, as Go's does, so only the resolution that decides it is asserted here.
        (bool strangerResolvesToNone, bool ownResolvesToSelf) = GoGoroutineOfProbe(TimeoutMs);

        Assert.IsTrue(strangerResolvesToNone, "a g minted off any goroutine (goid 0) resolved to a goroutine");
        Assert.IsTrue(ownResolvesToSelf, "a goroutine's own g did not resolve to it");
    }
}
