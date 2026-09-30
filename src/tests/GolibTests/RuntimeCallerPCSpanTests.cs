using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards the SPAN each caller-frame PC owns (runtime/managed_impl.cs). Go's consumers do arithmetic
/// on a return PC: runtime.expandFrames and runtime/pprof's expandInlinedFrames write <c>f.PC + 1</c>,
/// and Go's Frames.Next takes <c>pc--</c> on a return PC, so the pair round-trips. A token that owns
/// only its exact value turns that <c>+1</c> into the NEXT minted call site, which is how every block
/// profile stack symbolized as the wrong function (TestBlockProfileBias, measured at the I2 store).
/// Red against dense tokens (1, 2, 3, ...) resolved by exact match. Identity must hold as well: one call
/// site answers one PC on every call, and two sites never share a span.
/// </summary>
[TestClass]
public class RuntimeCallerPCSpanTests
{
    [TestMethod]
    public void AReturnPcRoundTripsThroughFramesNextPlusOne()
    {
        var (pc, _) = GoCallerSitesProbe();

        var (frame, _) = CallersFrames(new uintptr[] { pc }.slice()).Next();
        Assert.AreEqual("runtime.GoCallerSitesProbe", (string)frame.Function, "the recorded PC must name the probe");

        // Go's consumers: Frame.PC is the CALL pc, and "+1" makes it a return pc again.
        var (again, _) = CallersFrames(new uintptr[] { frame.PC + 1 }.slice()).Next();
        Assert.AreEqual((string)frame.Function, (string)again.Function, "Frame.PC + 1 must resolve to the same call site");
        Assert.AreEqual(frame.Entry, again.Entry, "Frame.PC + 1 must resolve to the same span");
        Assert.AreEqual(frame.PC, again.PC, "Frame.PC + 1 must round-trip to the same Frame.PC");
    }

    [TestMethod]
    public void APcMinusOneResolvesToTheSameCallSite()
    {
        var (pc, _) = GoCallerSitesProbe();

        Assert.AreEqual("runtime.GoCallerSitesProbe", (string)FuncForPC(pc).Name());
        Assert.AreEqual("runtime.GoCallerSitesProbe", (string)FuncForPC(pc - 1).Name(), "pc - 1 must stay inside the call site's span");
        Assert.AreEqual(FuncForPC(pc).Entry(), FuncForPC(pc - 1).Entry(), "pc and pc - 1 must share one entry");
    }

    [TestMethod]
    public void OneCallSiteAnswersOnePcOnEveryCall()
    {
        var (first1, second1) = GoCallerSitesProbe();
        var (first2, second2) = GoCallerSitesProbe();

        Assert.AreEqual(first1, first2, "the first call site must answer the same PC on every call");
        Assert.AreEqual(second1, second2, "the second call site must answer the same PC on every call");
    }

    [TestMethod]
    public void TwoCallSitesNeverShareASpan()
    {
        var (first, second) = GoCallerSitesProbe();

        Assert.AreNotEqual(first, second, "two call sites must answer two PCs");

        // Near each site's PC, each resolves to ITS OWN entry and never to the other's.
        Assert.AreEqual(FuncForPC(first).Entry(), FuncForPC(first + 1).Entry(), "first + 1 must stay in the first site's span");
        Assert.AreEqual(FuncForPC(second).Entry(), FuncForPC(second - 1).Entry(), "second - 1 must stay in the second site's span");
        Assert.AreNotEqual(FuncForPC(first).Entry(), FuncForPC(second).Entry(), "two sites must own two spans");
        Assert.AreNotEqual(FuncForPC(second).Entry(), FuncForPC(first + 1).Entry(), "first + 1 must never land in the second site's span");
    }

    // THE BAND'S NEIGHBOURS. A caller PC must never equal a value of another space: identity tokens
    // (ManagedPointerTokens.IdentityBand, reflect's Pointer() of a map, slice, func or channel), tagged
    // pointer tokens (bit 63 set, bit 47 clear; ManagedPointerTokens),
    // synthetic PCs (from 0xFFFF_8000_0000_0000; GoSyntheticPC), and user-mode addresses, which a
    // pinned data pointer or a marshal buffer can be. The band first started at 2^32, a valid x64
    // user-mode address (the i9's hardening note on 96ce90f997); 0x8000_0000_0000_0000 would be tagged.
    private static uintptr[] BandSamples()
    {
        var (first, last) = GoCallerSpanBand();
        var (site1, site2) = GoCallerSitesProbe();
        return new[] { first, last, site1, site2 };
    }

    // x86-64 canonicality: an address has bits 63..47 all equal.
    private static bool IsCanonicalAddress(uintptr value)
    {
        ulong top = (ulong)value >> 47;
        return top == 0 || top == 0x1FFFF;
    }

    [TestMethod]
    public void TheCallerBandIsNeverAnAddress()
    {
        foreach (var value in BandSamples())
            Assert.IsFalse(IsCanonicalAddress(value), $"caller PC 0x{(ulong)value:X16} is a canonical x64 address");
    }

    [TestMethod]
    public void TheCallerBandIsNeverATaggedToken()
    {
        foreach (var value in BandSamples())
            Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(value), $"caller PC 0x{(ulong)value:X16} reads as a tagged pointer token");
    }

    [TestMethod]
    public void TheCallerBandIsDisjointFromIdentityTokensAndSyntheticPCs()
    {
        foreach (var value in BandSamples())
        {
            Assert.IsFalse(ManagedPointerTokens.IsIdentityToken(value), $"caller PC 0x{(ulong)value:X16} is inside the identity band");
            Assert.IsTrue((ulong)value < 0xFFFF_8000_0000_0000UL, $"caller PC 0x{(ulong)value:X16} is inside the synthetic-PC space");
        }
    }
}
