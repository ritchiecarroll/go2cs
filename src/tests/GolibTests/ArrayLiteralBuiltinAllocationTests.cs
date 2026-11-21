// ArrayLiteralBuiltinAllocationTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards allocation parity between the two spellings of a Go array literal: the converter's
/// <c>array&lt;nint&gt;([1, 2, 3])</c> (Tour visual-parity item 4) and the <c>new nint[]{1, 2, 3}.array()</c>
/// projection it replaced.
/// </summary>
/// <remarks>
/// <see cref="builtin.array{T}(T[])"/> must keep the collection expression's fresh array as the result's
/// backing, with no second copy, so a literal evaluated in a loop allocates exactly what the projection did.
/// <c>GC.GetAllocatedBytesForCurrentThread</c> is the meter behind <c>runtime.ReadMemStats</c>'s
/// <c>TotalAlloc</c>. The short form, <see cref="builtin.array{T}(nint, T[])"/>, pads as
/// <c>.array(length)</c> does, and is held to the same parity. A deliberate copy in the full-length builtin
/// turns <see cref="FullLengthLiteralAllocatesWhatTheProjectionDid"/> red.
/// </remarks>
[TestClass]
public class ArrayLiteralBuiltinAllocationTests
{
    private const int LoopCount = 1000;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static long MeasureProjection()
    {
        for (int warm = 0; warm < 32; warm++)
            AllocationProbe.Escape(new nint[] { 1, 2, 3 }.array());

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < LoopCount; i++)
            AllocationProbe.Escape(new nint[] { 1, 2, 3 }.array());

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static long MeasureBuiltin()
    {
        for (int warm = 0; warm < 32; warm++)
            AllocationProbe.Escape(builtin.array<nint>([1, 2, 3]));

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < LoopCount; i++)
            AllocationProbe.Escape(builtin.array<nint>([1, 2, 3]));

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static long MeasureShortProjection()
    {
        for (int warm = 0; warm < 32; warm++)
            AllocationProbe.Escape(new byte[] { 1, 2 }.array(8));

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < LoopCount; i++)
            AllocationProbe.Escape(new byte[] { 1, 2 }.array(8));

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static long MeasureShortBuiltin()
    {
        for (int warm = 0; warm < 32; warm++)
            AllocationProbe.Escape(builtin.array<byte>(8, [1, 2]));

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < LoopCount; i++)
            AllocationProbe.Escape(builtin.array<byte>(8, [1, 2]));

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [TestMethod]
    public void FullLengthLiteralAllocatesWhatTheProjectionDid()
    {
        long projection = MeasureProjection();
        long builtinForm = MeasureBuiltin();

        // Self-control: the meter sees the literal's own array, so an equal pair is not two blind zeros.
        Assert.IsTrue(projection >= LoopCount * 3 * IntPtr.Size, $"the projection read {projection} B: the meter is blind");
        Assert.AreEqual(projection, builtinForm, $"array<nint>([1, 2, 3]) allocated {builtinForm} B against the projection's {projection} B over {LoopCount} literals");
    }

    [TestMethod]
    public void ShortLiteralAllocatesWhatTheProjectionDid()
    {
        long projection = MeasureShortProjection();
        long builtinForm = MeasureShortBuiltin();

        Assert.IsTrue(projection >= LoopCount * 8, $"the short projection read {projection} B: the meter is blind");
        Assert.AreEqual(projection, builtinForm, $"array<byte>(8, [1, 2]) allocated {builtinForm} B against the projection's {projection} B over {LoopCount} literals");
    }

    [TestMethod]
    public void TheBuiltinKeepsTheCollectionArrayAsItsBacking()
    {
        nint[] source = [1, 2, 3];
        array<nint> wrapped = builtin.array<nint>(source);

        source[0] = 9;

        Assert.AreEqual((nint)9, wrapped[0], "array<T>(T[]) copied its source instead of wrapping it");
        Assert.AreEqual((nint)3, builtin.len(wrapped));
    }
}
