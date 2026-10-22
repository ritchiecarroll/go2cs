// PanicFramesInlinedPanicTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// AN EXPLICIT PANIC RAISED INSIDE A GO FUNCTION THE JIT INLINED (COORD's frame-hint ruling, 2026-10-22). Go records
/// inlined frames, so its traceback always shows the function a panic was raised in. Under Release with
/// TieredCompilation=0 the JIT inlines a small Go function into its caller, the panic's trace then holds only the
/// caller, and the frame left the traceback (math/bits.Div32's arm, PanicFramesRound3Tests). The runtime restores it
/// from the IL alone: the faulting frame's statement names the one call the panic can have come from. Nothing is
/// emitted and nothing runs off the panic path.
///
/// The positive arms (Div32 in Round3, two levels here, and the crash renderer's side) read Go's frames in every mode.
/// The controls are the "never a guess" cases: where the IL cannot name ONE callee, the optimized trace keeps the
/// frames it has, a stated divergence, and tier-0 (which inlines nothing) reads Go's frames as real ones.
/// </summary>
[TestClass]
public class PanicFramesInlinedPanicTests
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

    private static List<string> Run(System.Func<List<string>> probe) => panicframesprobe_package.onGoroutine(probe);

    private static void AssertFrames(string[] expected, List<string> names, string owner) =>
        CollectionAssert.AreEqual(expected, Below(names, owner), $"got {string.Join(" | ", Below(names, owner))}");

    // Two inlined levels: Go shows both.
    [TestMethod]
    public void APanicTwoInlinedLevelsDeepShowsBothFrames() =>
        AssertFrames(["runtime.gopanic", Pkg + "inlinedInner", Pkg + "inlinedOuter", Pkg + "panicTwoLevels"],
            Run(() => panicframesprobe_package.panicTwoLevels(1)), "panicTwoLevels");

    // CONTROL: the statement panics itself (its callee returns), so no callee is named in any mode.
    [TestMethod]
    public void AStatementThatPanicsItselfNamesNoCallee() =>
        AssertFrames(["runtime.gopanic", Pkg + "panicInOwnStatement"],
            Run(() => panicframesprobe_package.panicInOwnStatement(1)), "panicInOwnStatement");

    // CONTROL: two candidate calls in the statement. Tier-0 shows the real frame, Go's; optimized, the IL cannot say
    // which call raised it, so none is named.
    [TestMethod]
    public void TwoCandidateCallsInTheStatementNameNone() =>
        AssertFrames(JitMode.OptimizedFromFirstCall()
                ? ["runtime.gopanic", Pkg + "panicTwoCandidates"]
                : ["runtime.gopanic", Pkg + "inlinedLoud", Pkg + "panicTwoCandidates"],
            Run(() => panicframesprobe_package.panicTwoCandidates(1)), "panicTwoCandidates");

    // CONTROL: one candidate with two panic sites. The callee is certain, its line is not, so it is not named.
    [TestMethod]
    public void ACalleeWithTwoPanicSitesIsNotNamed() =>
        AssertFrames(JitMode.OptimizedFromFirstCall()
                ? ["runtime.gopanic", Pkg + "panicTwoSites"]
                : ["runtime.gopanic", Pkg + "inlinedTwoSites", Pkg + "panicTwoSites"],
            Run(() => panicframesprobe_package.panicTwoSites(1)), "panicTwoSites");

    // The crash renderer's side: runtime.Stack in the deferred call renders the panic site (PanicTrace) through
    // appendGoFrames, which must name math/bits.Div32 above the owner, as Go's traceback does.
    [TestMethod]
    public void RuntimeStackDuringTheInlinedPanicShowsTheCallee()
    {
        string stack = panicframesprobe_package.onGoroutine(() => panicframesprobe_package.stackInDiv32Panic(1));

        int panicLine = stack.IndexOf("panic: ", System.StringComparison.Ordinal);
        Assert.IsTrue(panicLine >= 0, $"no panic section:\n{stack}");

        // Go inlines Div32 and prints an inlined frame as `name(...)`; so does the optimized JIT's restored frame. Tier-0
        // keeps Div32 as a real frame, printed as a call.
        string calleeLine = JitMode.OptimizedFromFirstCall() ? "\nmath/bits.Div32(...)\n" : "\nmath/bits.Div32()\n";
        int callee = stack.IndexOf(calleeLine, panicLine, System.StringComparison.Ordinal);
        int owner = stack.IndexOf("\n" + Pkg + "stackInDiv32Panic(", panicLine, System.StringComparison.Ordinal);

        Assert.IsTrue(owner > panicLine, $"the owner is not in the panic section:\n{stack}");
        Assert.IsTrue(callee > panicLine && callee < owner, $"math/bits.Div32 is not above the owner:\n{stack}");
    }
}
