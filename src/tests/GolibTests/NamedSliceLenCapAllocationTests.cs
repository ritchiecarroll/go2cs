// NamedSliceLenCapAllocationTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Guards the allocation-free <c>len</c> and <c>cap</c> over a NAMED slice type (Go's <c>type S []E</c>,
/// emitted as a go2cs-gen wrapper struct) and over a <c>~[]E</c> type parameter.
/// </summary>
/// <remarks>
/// <para>
/// <c>builtin.len&lt;T&gt;(in slice&lt;T&gt;)</c> cannot bind a wrapper struct: generic inference does not
/// consider the wrapper's implicit conversion to <c>slice&lt;T&gt;</c>. The only overloads that used to
/// apply were <c>len(ISlice)</c> and <c>cap(ISlice)</c>, whose interface parameter BOXES the caller's
/// struct on every call: 56 B per call at Release with tiering off, measured on
/// <c>sort.IntSlice</c>. That is what math/big's <c>TestMulUnbalanced</c> was reading. A 50,000-word
/// by 40-word multiply called <c>len</c>/<c>cap</c> on <c>nat</c> often enough to allocate about
/// 94 % of its 10.5 MB (docs/phase4/BOARD-next-validation-candidates.md, 2026-09-24, G). The
/// constrained <c>len&lt;TS&gt;</c>/<c>cap&lt;TS&gt;</c> overloads dispatch on the value type instead:
/// the <c>len&lt;TSeq&gt;(TSeq) where TSeq : IByteSeq</c> precedent, applied to <see cref="ISlice"/>.
/// </para>
/// <para>
/// The assertion is on measured bytes, not on shape, because the regression is silent: removing the
/// constrained overloads still compiles and still returns the right length, and it just allocates
/// again.
/// </para>
/// </remarks>
[TestClass]
public class NamedSliceLenCapAllocationTests
{
    // High enough that a per-call box (56 B) is unmistakable against noise (~56 KB here).
    private const int LoopCount = 1000;

    // The warm-up count clears tier 0: its unoptimized code keeps the wrapper's own interface cast
    // (`((IArray)m_value).Length`) as a real box, so the measurement window must see optimized code.
    private const int WarmupCount = 50_000;

    private static sort_package.IntSlice Named(int length, int capacity) =>
        new slice<nint>(length, capacity);

    private static long MeasureNamedLen(sort_package.IntSlice s, out long sum)
    {
        long total = 0;

        for (int warm = 0; warm < WarmupCount; warm++)
            total += len(s);

        total = 0;
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
            total += len(s);

        long after = GC.GetAllocatedBytesForCurrentThread();
        sum = total;
        return after - before;
    }

    private static long MeasureNamedCap(sort_package.IntSlice s, out long sum)
    {
        long total = 0;

        for (int warm = 0; warm < WarmupCount; warm++)
            total += cap(s);

        total = 0;
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
            total += cap(s);

        long after = GC.GetAllocatedBytesForCurrentThread();
        sum = total;
        return after - before;
    }

    // The shape converted `~[]E` generic code has (slices.Grow's `cap(s) - len(s)`): S is a type
    // parameter constrained to ISlice<E>, so a struct S boxed through len(ISlice)/cap(ISlice) too.
    private static nint Headroom<S, E>(S s) where S : ISlice<E> => cap(s) - len(s);

    private static long MeasureGenericHeadroom(sort_package.IntSlice s, out long sum)
    {
        long total = 0;

        for (int warm = 0; warm < WarmupCount; warm++)
            total += Headroom<sort_package.IntSlice, nint>(s);

        total = 0;
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
            total += Headroom<sort_package.IntSlice, nint>(s);

        long after = GC.GetAllocatedBytesForCurrentThread();
        sum = total;
        return after - before;
    }

    [TestMethod]
    public void LenOfNamedSliceAllocatesNothing()
    {
        long bytes = MeasureNamedLen(Named(40, 64), out long sum);

        Assert.AreEqual(40L * LoopCount, sum, "len of a named slice did not return its length");
        Assert.AreEqual(0L, bytes,
            $"len(named slice) allocated {bytes} bytes across {LoopCount} calls " +
            $"({bytes / (double)LoopCount:F1} B/call); it must bind the constrained len<TS>, not len(ISlice).");
    }

    [TestMethod]
    public void CapOfNamedSliceAllocatesNothing()
    {
        long bytes = MeasureNamedCap(Named(40, 64), out long sum);

        Assert.AreEqual(64L * LoopCount, sum, "cap of a named slice did not return its capacity");
        Assert.AreEqual(0L, bytes,
            $"cap(named slice) allocated {bytes} bytes across {LoopCount} calls " +
            $"({bytes / (double)LoopCount:F1} B/call); it must bind the constrained cap<TS>, not cap(ISlice).");
    }

    [TestMethod]
    public void LenAndCapOfSliceTypeParameterAllocateNothing()
    {
        long bytes = MeasureGenericHeadroom(Named(40, 64), out long sum);

        Assert.AreEqual(24L * LoopCount, sum, "cap(s) - len(s) over a type parameter read the wrong window");
        Assert.AreEqual(0L, bytes,
            $"len/cap over a ~[]E type parameter allocated {bytes} bytes across {LoopCount} calls " +
            $"({bytes / (double)LoopCount:F1} B/call).");
    }

    [TestMethod]
    public void NamedSliceWindowAndNilKeepGoSemantics()
    {
        // A window keeps Go's arithmetic: s[2:5] of a cap-64 slice has len 3 and cap 62.
        sort_package.IntSlice window = Named(40, 64)[2..5];
        Assert.AreEqual((nint)3, len(window));
        Assert.AreEqual((nint)62, cap(window));

        // The nil named slice reads 0 for both, never a fault.
        sort_package.IntSlice nil = default;
        Assert.AreEqual((nint)0, len(nil));
        Assert.AreEqual((nint)0, cap(nil));
    }

    [TestMethod]
    public void InterfaceTypedArgumentStillBindsTheInterfaceOverload()
    {
        // A value already typed as the interface is unaffected: the non-generic overload is preferred
        // for it, and it reads the same length.
        ISlice boxed = Named(7, 9);
        Assert.AreEqual((nint)7, len(boxed));
        Assert.AreEqual((nint)9, cap(boxed));
    }
}
