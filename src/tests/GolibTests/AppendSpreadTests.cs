// AppendSpreadTests.cs - Gbtc
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
/// <c>appendꓸꓸꓸ(s, t)</c>, Go's <c>append(s, t...)</c> with a slice operand: the exact
/// <c>appendꓸꓸꓸ&lt;T&gt;(slice&lt;T&gt;, in slice&lt;T&gt;)</c> overload, which takes the operand as the slice it is.
/// </summary>
/// <remarks>
/// <para>
/// Before it existed the only overload a <see cref="slice{T}"/> operand could bind was
/// <c>appendꓸꓸꓸ&lt;T&gt;(slice&lt;T&gt;, ISlice&lt;T&gt;)</c>, whose interface parameter BOXES the struct: 56 B per call
/// that AllocationCounter never sees. Go 1.26's bytes <c>TestReplace</c> read it as 56 "objects" per run for
/// <c>Replace("", "x", "X", -1)</c>, whose <c>append([]byte(nil), s...)</c> allocates nothing in Go.
/// </para>
/// <para>
/// A NAMED slice (<c>type S []E</c>, a go2cs-gen wrapper struct) now binds the same overload through its
/// implicit conversion to <see cref="slice{T}"/>, a view of the same window, rather than through the box.
/// <see cref="NamedSliceSpreadMatchesTheBoxedPath"/> holds the two bindings to the same answer.
/// </para>
/// </remarks>
[TestClass]
public class AppendSpreadTests
{
    private const int LoopCount = 1000;

    // Clears tier 0, so the window measures optimized code (the NamedSliceLenCapAllocationTests precedent).
    private const int WarmupCount = 50_000;

    private static slice<nint> Counting(int length, int capacity)
    {
        slice<nint> s = new(length, capacity);

        for (int i = 0; i < length; i++)
            s[i] = i + 1;

        return s;
    }

    private static long Measure(Func<slice<nint>> append)
    {
        for (int warm = 0; warm < WarmupCount; warm++)
            append();

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
            append();

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [TestMethod]
    public void SliceSpreadWithinCapacityAllocatesNothing()
    {
        slice<nint> dst = Counting(0, 64);
        slice<nint> elems = Counting(8, 8);

        long bytes = Measure(() => appendꓸꓸꓸ(dst, elems));

        Assert.AreEqual(0L, bytes, $"append(dst, elems...) within capacity allocated {bytes} B over {LoopCount} calls");
    }

    [TestMethod]
    public void NamedSliceSpreadWithinCapacityAllocatesNothing()
    {
        slice<nint> dst = Counting(0, 64);
        sort_package.IntSlice elems = Counting(8, 8);

        long bytes = Measure(() => appendꓸꓸꓸ(dst, elems));

        Assert.AreEqual(0L, bytes, $"append(dst, named...) within capacity allocated {bytes} B over {LoopCount} calls");
    }

    // bytes.Replace's no-match arm: append([]byte(nil), s...) with an empty s is nil, and costs nothing.
    [TestMethod]
    public void EmptySpreadOntoNilIsNilAndAllocatesNothing()
    {
        slice<byte> empty = new(0, 0);

        Assert.IsTrue(appendꓸꓸꓸ(slice<byte>(default!), empty) == nil, "append(nil, empty...) must stay nil");

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
            appendꓸꓸꓸ(slice<byte>(default!), empty);

        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.AreEqual(0L, bytes, $"append(nil, empty...) allocated {bytes} B over {LoopCount} calls");
    }

    // Each case builds its destination twice, once per binding, because an in-place append writes into
    // the destination's backing.
    private static readonly (string Name, Func<slice<nint>> Destination, Func<slice<nint>, slice<nint>> Operand)[] s_cases =
    [
        ("onto nil", () => default!, _ => Counting(5, 7)),
        ("within capacity", () => Counting(3, 10), _ => Counting(4, 4)),
        ("past capacity", () => Counting(3, 4), _ => Counting(4, 4)),
        ("empty operand", () => Counting(3, 4), _ => Counting(0, 2)),
        ("operand is the destination's own window", () => Counting(6, 8).slice(0, 2), dst => dst.slice(0, 4, 8)),
    ];

    [TestMethod]
    public void NamedSliceSpreadMatchesTheBoxedPath()
    {
        foreach ((string name, Func<slice<nint>> destination, Func<slice<nint>, slice<nint>> operand) in s_cases)
        {
            slice<nint> exactDst = destination();
            sort_package.IntSlice exactOperand = operand(exactDst);
            slice<nint> exact = appendꓸꓸꓸ(exactDst, exactOperand);

            slice<nint> boxedDst = destination();
            sort_package.IntSlice boxedOperand = operand(boxedDst);
            slice<nint> boxed = appendꓸꓸꓸ(boxedDst, (ISlice<nint>)boxedOperand);

            Assert.AreEqual(len(boxed), len(exact), $"{name}: len");
            Assert.AreEqual(cap(boxed), cap(exact), $"{name}: cap");
            Assert.AreEqual(boxed == nil, exact == nil, $"{name}: nil-ness");

            for (nint i = 0; i < len(boxed); i++)
                Assert.AreEqual(boxed[i], exact[i], $"{name}: element {i}");

            // The result shares the destination's backing exactly when the append fit in place.
            Assert.AreEqual(SharesBacking(boxed, boxedDst), SharesBacking(exact, exactDst), $"{name}: aliases the destination");

            // The operand is read, never aliased by a grown result.
            Assert.AreEqual(SharesBacking(boxed, ((slice<nint>)boxedOperand)), SharesBacking(exact, ((slice<nint>)exactOperand)), $"{name}: aliases the operand");
        }
    }

    private static bool SharesBacking(slice<nint> a, slice<nint> b) =>
        cap(a) > 0 && cap(b) > 0 && Ꮡ(a.slice(0, 1, cap(a)), 0) == Ꮡ(b.slice(0, 1, cap(b)), 0);
}
