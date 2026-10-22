using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// The shapes COORD's check of the third round named (ledger 02:24), under its principle: every MODELLED
/// splice keys on an EMITTER-provided marker; anything unmarked is refused. Each probe runs on a goroutine
/// of its own.
/// </summary>
[TestClass]
public class PanicFramesRound4Tests
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

    // A promoted method's wrapper with no marker: a name starting "(*" is not Go's panicwrap.
    [TestMethod]
    public void AnUnmarkedPromotedWrapperOnANilPointerSplicesNothing() =>
        AssertNoSplice(Run(panicframesprobe_package.nilThroughPromotedWrapper), "nilThroughPromotedWrapper");

    // A NON-nil receiver whose inlined callee faults inside the wrapper frame: never the wrapper's own panic.
    [TestMethod]
    public void AFaultingCalleeInlinedIntoAWrapperSplicesNothing() =>
        AssertNoSplice(Run(panicframesprobe_package.nonNilFaultingCalleeThroughWrapper), "nonNilFaultingCalleeThroughWrapper");

    // An intrinsified atomic on a nil address. Go is the oracle (COORD ruling 2026-10-22): a DIRECT call of the
    // atomic is intrinsified, so Go's traceback reads gopanic | panicmem | sigpanic | owner (the probe's own comment,
    // PanicFramesProbe.Round4.cs atomicOnNil). Code optimized from its first call -- the banks' Release
    // TieredCompilation=0 -- inlines AddInt32 and reads exactly that. Tier-0 inlines nothing, keeps the atomic's
    // frame and splices nothing, which is Go's answer only for a func-value call; it is pinned here as tier-0's
    // reading, never matched by deoptimizing every atomic.
    [TestMethod]
    public void AnIntrinsifiedAtomicOnANilAddressSplicesNothing()
    {
        List<string> names = Run(panicframesprobe_package.atomicOnNil);

        if (!JitMode.OptimizedFromFirstCall())
        {
            AssertNoSplice(names, "atomicOnNil");
            return;
        }

        List<string> below = Below(names, "atomicOnNil");
        CollectionAssert.AreEqual(new[] { "runtime.gopanic", "runtime.panicmem", "runtime.sigpanic", Pkg + "atomicOnNil" }, below,
            $"expected Go's intrinsic frames; got {string.Join(" | ", below)}");
    }

    // A panic in a range-over-func loop body, recovered by the ranger's deferred call.
    [TestMethod]
    public void ALoopBodyPanicRecoveredByTheRangerSplicesNothing() =>
        AssertNoSplice(Run(panicframesprobe_package.rangerL), "rangerL");

    // The adapter shape (a control: refused at round 3 already, now guarded).
    [TestMethod]
    public void ANilBoxInsideAnInterfaceAdapterSplicesNothing() =>
        AssertNoSplice(Run(panicframesprobe_package.nilThroughAdapter), "nilThroughAdapter");
}
