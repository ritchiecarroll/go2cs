using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

/// <summary>
/// The shapes COORD's verification of the panic-frames re-cut named (the addendum to
/// review-r-panic-frames-2026-09-27.md), each with Go's go1.24.13 answer on its probe. Every probe runs on
/// a goroutine of its own. Arms that need an ACCEPTED raiser use the nil-func thunk, so that removing the
/// fix under test turns them red.
/// </summary>
[TestClass]
public class PanicFramesVerificationTests
{
    private const string Pkg = "panicframesprobe.";

    private static List<string> Below(List<string> names, string owner)
    {
        Assert.IsTrue(names.Count > 3, $"too few frames: {string.Join(" | ", names)}");
        Assert.AreEqual("runtime.Callers", names[0], string.Join(" | ", names));

        int end = names.IndexOf(Pkg + owner, 3);
        Assert.IsTrue(end >= 3, $"the owner {owner} is missing: {string.Join(" | ", names)}");

        return names.GetRange(3, end - 3 + 1);
    }

    private static void AssertNoSplice(List<string> names, string owner)
    {
        List<string> below = Below(names, owner);
        CollectionAssert.AreEqual(new[] { Pkg + owner }, below, $"expected NO splice; got {string.Join(" | ", below)}");
    }

    private static List<string> Run(System.Func<List<string>> probe) => panicframesprobe_package.onGoroutine(probe);

    // B1: the accepted re-raise is decided by the popped delegate's method, not by frames a tail call removed.
    [TestMethod]
    public void ATailCallingForwarderDeferredIsNotTheCatcher() =>
        AssertNoSplice(Run(panicframesprobe_package.deferATailCallingForwarder), "deferATailCallingForwarder");

    // M0: the deferred delegate caught first although its Run registered no defer.
    [TestMethod]
    public void ACatcherWhoseRunRegisteredNoDeferIsStillTheDeferredCall() =>
        CollectionAssert.AreEqual(
            new[] { "runtime.gopanic", Pkg + "dcUnreached", "runtime.gopanic", Pkg + "deferACatcherWithNoDefer" },
            Below(Run(panicframesprobe_package.deferACatcherWithNoDefer), "deferACatcherWithNoDefer"));

    // B2 NA / NAR / NC: nil funcs reached through a golib or converter wrapper: Go's deferwrap is not modelled.
    [TestMethod]
    public void ANilFuncWithArgumentsWhilePanickingSplicesNothing() =>
        AssertNoSplice(Run(panicframesprobe_package.nilFuncWithArgWhilePanicking), "nilFuncWithArgWhilePanicking");

    [TestMethod]
    public void ANilFuncWithArgumentsOnANormalReturnSplicesNothing() =>
        AssertNoSplice(Run(panicframesprobe_package.nilFuncWithArgOnNormalReturn), "nilFuncWithArgOnNormalReturn");

    [TestMethod]
    public void ANilNamedFuncTypeThroughTheConvertersLambdaSplicesNothing() =>
        AssertNoSplice(Run(panicframesprobe_package.nilNamedFuncTypeWhilePanicking), "nilNamedFuncTypeWhilePanicking");

    // C2: runtime errors tagged where raised; none of these runtime frames is modelled.
    [DataTestMethod]
    [DataRow(0, DisplayName = "array index (goPanicIndex)")]
    [DataRow(1, DisplayName = "string index (goPanicIndex)")]
    [DataRow(2, DisplayName = "nil map write (mapassign_faststr)")]
    [DataRow(3, DisplayName = "close of a closed channel (closechan)")]
    [DataRow(4, DisplayName = "failed type assertion (panicdottypeE)")]
    [DataRow(5, DisplayName = "slice bounds (goPanicSliceB)")]
    public void AnUnmodelledRuntimeErrorSplicesNothing(int which) =>
        AssertNoSplice(Run(() => panicframesprobe_package.runtimeErrorShape(which)), "runtimeErrorShape");

    // C3: the test host's Goexit (t.FailNow / t.SkipNow on the test's own thread).
    [DataTestMethod]
    [DataRow(false, DisplayName = "FailNow")]
    [DataRow(true, DisplayName = "SkipNow")]
    public void ANilDeferredFuncFaultingDuringTheTestHostsGoexitSplicesNothing(bool skip)
    {
        TestReporter reporter = new("guard", json: false);
        TestRunner runner = new(new TestRegistry("guard", []), new TestOptions(), reporter, ".", ".");
        TestExecution parent = new(runner, "TestHostGoexitGuard", null, "guard.go", 1);
        List<string>? got = null;

        parent.Run("child", t =>
            panicframesprobe_package.nilDeferDuringHostGoexit(
                () => { if (skip) t.Value.Execution.SkipNow(); else t.Value.Execution.FailNow(); },
                names => got = names));

        Assert.IsNotNull(got, "the probe's deferred reader never ran");
        AssertNoSplice(got!, "nilDeferDuringHostGoexit");
    }

    // C3 / GX: runtime.Goexit, on the same thread and re-raised across the range-over-func adapter.
    [TestMethod]
    public void ANilDeferredFuncFaultingDuringGoexitSplicesNothing() =>
        AssertNoSplice(panicframesprobe_package.goexitShapeNilThunk(), "nilDeferDuringGoexit");

    [TestMethod]
    public void ANilDeferredFuncFaultingDuringARangeFuncsGoexitSplicesNothing() =>
        AssertNoSplice(panicframesprobe_package.rangeGoexitShape(), "nilDeferDuringRangeGoexit");

    // C1 / R1: a site owned on another thread, with the activation numbers made to collide.
    [TestMethod]
    public void ASiteOwnedOnAnotherThreadSplicesNothingEvenWhenActivationsCollide() =>
        AssertNoSplice(Run(panicframesprobe_package.crossThreadOwner), "crossThreadOwner");

    // C4 / C5: the chain is not bounded, and a small buffer truncates it from the top as Go's does.
    [TestMethod]
    public void ALongChainThroughASmallBufferFillsFromTheTop()
    {
        List<string> names = Run(() => panicframesprobe_package.longThunkChain(70, 64));

        Assert.AreEqual(64, names.Count, string.Join(" | ", names));
        Assert.AreEqual("runtime.Callers", names[0]);
        StringAssert.StartsWith(names[1], Pkg + "longThunkChain.func", string.Join(" | ", names));

        string[] link = ["runtime.gopanic", "runtime.panicmem", "runtime.sigpanic"];

        for (int i = 2; i < 64; i++)
            Assert.AreEqual(link[(i - 2) % 3], names[i], $"frame {i}: {string.Join(" | ", names)}");
    }

    [TestMethod]
    public void A65LinkChainIsSplicedWhole()
    {
        // The reader calls Callers itself (no callersHere frame), so the splice starts at index 2.
        List<string> names = Run(() => panicframesprobe_package.longThunkChain(65, 256));
        int end = names.IndexOf(Pkg + "longThunkChain", 2);
        Assert.IsTrue(end > 2, string.Join(" | ", names));
        List<string> below = names.GetRange(2, end - 2 + 1);

        List<string> expected = [];

        for (int i = 0; i < 65; i++)
            expected.AddRange(["runtime.gopanic", "runtime.panicmem", "runtime.sigpanic"]);

        expected.Add("runtime.gopanic");
        expected.Add(Pkg + "longThunkChain");

        CollectionAssert.AreEqual(expected, below, $"{below.Count} frames");
    }
}
