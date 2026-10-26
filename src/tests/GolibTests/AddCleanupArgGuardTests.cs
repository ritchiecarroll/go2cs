#nullable enable

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using @unsafe = go.unsafe_package;
using Δruntime = go.runtime_package;

namespace GolibTests;

/// <summary>
/// <c>runtime.AddCleanup</c>'s two arg guards (Go 1.26 mcleanup.go): <c>ptr is equal to arg</c> when a pointer-kinded
/// arg holds ptr's own ADDRESS, and <c>ptr is within arg</c> when it points anywhere else in ptr's object, except in a
/// tiny-allocator span (a pointer-free object of at most 16 bytes). Every expected message below is go1.26.10's own,
/// read through recover() for the same program (linux/amd64, measured 2026-10-24); null is no panic.
/// </summary>
[TestClass]
public class AddCleanupArgGuardTests
{
#pragma warning disable CS0649 // Go fields the arms address but never assign
    // The shapes go2cs-gen emits for Go's `struct{ a, b int32 }`, `struct{ a, b int64 }`, `struct{ a, b, c int64 }`
    // and `struct{ p *int; b int64 }`.
    private struct Small
    {
        internal int A;
        internal int B;

        internal static ref int ᏑB(ref Small instance) => ref instance.B;
    }

    private struct Sixteen
    {
        internal long A;
        internal long B;

        internal static ref long ᏑB(ref Sixteen instance) => ref instance.B;
    }

    private struct Big
    {
        internal long A;
        internal long B;
        internal long C;

        internal static ref long ᏑA(ref Big instance) => ref instance.A;
        internal static ref long ᏑB(ref Big instance) => ref instance.B;
        internal static ref long ᏑC(ref Big instance) => ref instance.C;
    }

    private struct WithPtr
    {
        internal ж<nint> P;
        internal long B;

        internal static ref long ᏑB(ref WithPtr instance) => ref instance.B;
    }

#pragma warning restore CS0649

    private const string Equal = "runtime.AddCleanup: ptr is equal to arg, cleanup will never run";
    private const string Within = "runtime.AddCleanup: ptr is within arg, cleanup will never run";

    // try(name, f): what recover() reads when f registers the cleanup.
    private static string? Recovered(Action register)
    {
        try
        {
            register();
            return null;
        }
        catch (PanicException panic)
        {
            return panic.State?.ToString();
        }
    }

    [TestMethod]
    public void AnArgHoldingPtrsOwnAddressIsEqual()
    {
        ж<Big> p = @new<Big>();

        Assert.AreEqual(Equal, Recovered(() => Δruntime.AddCleanup(p, (ж<Big> _) => { }, p)), "equal");
        Assert.AreEqual(Equal, Recovered(() => Δruntime.AddCleanup(p, (@unsafe.Pointer _) => { }, new @unsafe.Pointer(p))), "equal-unsafe");

        // &p.a is p's address: Go compares addresses, so the first field is EQUAL, not within.
        Assert.AreEqual(Equal, Recovered(() => Δruntime.AddCleanup(p, (ж<long> _) => { }, p.of(Big.ᏑA))), "arg-interior-first-field");
    }

    [TestMethod]
    public void AnArgElsewhereInPtrsObjectIsWithin()
    {
        ж<Big> p = @new<Big>();

        Assert.AreEqual(Within, Recovered(() => Δruntime.AddCleanup(p.of(Big.ᏑB), (ж<Big> _) => { }, p)), "interior-ptr-arg-base");
        Assert.AreEqual(Within, Recovered(() => Δruntime.AddCleanup(p, (ж<long> _) => { }, p.of(Big.ᏑC))), "arg-interior");

        ж<WithPtr> w = @new<WithPtr>();

        Assert.AreEqual(Within, Recovered(() => Δruntime.AddCleanup(w, (ж<long> _) => { }, w.of(WithPtr.ᏑB))), "withptr-interior");

        slice<long> s = make<slice<long>>(4);

        Assert.AreEqual(Within, Recovered(() => Δruntime.AddCleanup(Ꮡ(s, 0), (ж<long> _) => { }, Ꮡ(s, 2))), "slice-elem");
    }

    [TestMethod]
    public void ATinySpanObjectIsExempt()
    {
        ж<Small> small = @new<Small>();
        ж<Sixteen> sixteen = @new<Sixteen>();
        slice<int> s = make<slice<int>>(2);

        Assert.IsNull(Recovered(() => Δruntime.AddCleanup(small, (ж<int> _) => { }, small.of(Small.ᏑB))), "small-interior");
        Assert.IsNull(Recovered(() => Δruntime.AddCleanup(sixteen, (ж<long> _) => { }, sixteen.of(Sixteen.ᏑB))), "sixteen-interior");
        Assert.IsNull(Recovered(() => Δruntime.AddCleanup(Ꮡ(s, 0), (ж<int> _) => { }, Ꮡ(s, 1))), "small-slice-elem");
    }

    [TestMethod]
    public void ACleanupClosingOverAnUnsafePointerToPtrClosesOverPtr()
    {
        // u := unsafe.Pointer(p); runtime.AddCleanup(p, func(int) { _ = u }, 0): the converter passes the capture.
        // Go's slot scan has no tiny exemption, so the small object panics too.
        const string ClosesOver = "runtime.AddCleanup: cleanup function closes over ptr, cleanup will never run";
        ж<Big> p = @new<Big>();
        ж<Small> small = @new<Small>();
        @unsafe.Pointer u = new(p);
        @unsafe.Pointer v = new(small.of(Small.ᏑB));

        Assert.AreEqual(ClosesOver, Recovered(() => Δruntime.AddCleanup(p, (nint _) => { }, 0, [u])), "closes-over-unsafe");
        Assert.AreEqual(ClosesOver, Recovered(() => Δruntime.AddCleanup(small, (nint _) => { }, 0, [v])), "closes-over-unsafe-small");
    }

    [TestMethod]
    public void AnArgInAnotherObjectIsAccepted()
    {
        ж<Big> p = @new<Big>();
        ж<Big> q = @new<Big>();

        Assert.IsNull(Recovered(() => Δruntime.AddCleanup(p, (ж<Big> _) => { }, q)), "other-object");
    }
}
