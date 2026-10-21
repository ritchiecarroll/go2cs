// RangeOverIntAllocationTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Guards the allocation-free range-over-int: <c>for i := range n</c> converts to
/// <c>foreach (var i in range(n))</c>, and Go's loop allocates nothing.
/// </summary>
/// <remarks>
/// <para>
/// While <c>range</c> was a C# iterator method, every loop allocated its iterator object (56 B on x64), which no
/// golib counter charges. runtime's TestAppendByteInLoop, TestAppendPtrInLoop and TestAppendByteCapInLoop read
/// that object as their <c>{0, 0, 0}</c> row's allocation: Go 0, the converted test 56. The siblings
/// <see cref="SliceRangeAllocationTests"/> and <see cref="ArrayRangeAllocationTests"/> lock the same property for
/// <c>range</c> over a slice and an array.
/// </para>
/// <para>
/// Both meters are read: <see cref="AllocationCounter"/> is what <c>testing.AllocsPerRun</c> reports when it is
/// nonzero, and the bytes are what it reports when the counter saw nothing, which is exactly how the iterator
/// surfaced. The boxed control drives the same loop through <see cref="IEnumerable{T}"/> and must still allocate,
/// or the zeros prove nothing.
/// </para>
/// </remarks>
[TestClass]
public class RangeOverIntAllocationTests
{
    private const int LoopCount = 1000;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasureNint(nint n, out long count, out long sum)
    {
        long total = 0;

        for (int warm = 0; warm < 32; warm++)
        {
            foreach (var i in range(n))
                total += i;
        }

        total = 0;

        long beforeCount = AllocationCounter.CurrentThreadCount;
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
        {
            foreach (var i in range(n))
                total += i;
        }

        long after = GC.GetAllocatedBytesForCurrentThread();

        count = AllocationCounter.CurrentThreadCount - beforeCount;
        sum = total;
        return after - before;
    }

    // `for i := range 8`: a Go untyped constant renders as a C# int operand.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasureConstant(out long sum)
    {
        long total = 0;

        for (int warm = 0; warm < 32; warm++)
        {
            foreach (var i in range(8))
                total += i;
        }

        total = 0;

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
        {
            foreach (var i in range(8))
                total += i;
        }

        long after = GC.GetAllocatedBytesForCurrentThread();

        sum = total;
        return after - before;
    }

    // `for i := range n` over a uintptr: the converter names the type argument.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasureTyped(uintptr n, out long sum)
    {
        long total = 0;

        for (int warm = 0; warm < 32; warm++)
        {
            foreach (var i in range<uintptr>(n))
                total += (long)(nuint)i;
        }

        total = 0;

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
        {
            foreach (var i in range<uintptr>(n))
                total += (long)(nuint)i;
        }

        long after = GC.GetAllocatedBytesForCurrentThread();

        sum = total;
        return after - before;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasureBoxed(nint n, out long sum)
    {
        long total = 0;

        for (int warm = 0; warm < 32; warm++)
        {
            foreach (nint i in (IEnumerable<nint>)range(n))
                total += i;
        }

        total = 0;

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
        {
            foreach (nint i in (IEnumerable<nint>)range(n))
                total += i;
        }

        long after = GC.GetAllocatedBytesForCurrentThread();

        sum = total;
        return after - before;
    }

    [TestMethod]
    public void RangeOverIntAllocatesNothing()
    {
        foreach (nint n in new nint[] { 0, 1, 8 })
        {
            long bytes = MeasureNint(n, out long count, out long sum);

            Assert.AreEqual(n * (n - 1) / 2 * (long)LoopCount, sum, $"range({n}) did not yield 0..{n - 1}");
            Assert.AreEqual(0L, count, $"range({n}) charged {count} golib objects across {LoopCount} loops");
            Assert.AreEqual(0L, bytes,
                $"`foreach` over range({n}) allocated {bytes} bytes across {LoopCount} loops " +
                $"({bytes / (double)LoopCount:F1} B/loop): range must return a struct with a struct enumerator, " +
                "not an iterator method's IEnumerable<nint>.");
        }
    }

    [TestMethod]
    public void RangeOverIntConstantAllocatesNothing()
    {
        long bytes = MeasureConstant(out long sum);

        Assert.AreEqual(28L * LoopCount, sum, "range(8) did not yield 0..7");
        Assert.AreEqual(0L, bytes, $"`foreach` over range(8) allocated {bytes} bytes across {LoopCount} loops");
    }

    [TestMethod]
    public void RangeOverTypedIntAllocatesNothing()
    {
        long bytes = MeasureTyped(5, out long sum);

        Assert.AreEqual(10L * LoopCount, sum, "range<uintptr>(5) did not yield 0..4");
        Assert.AreEqual(0L, bytes, $"`foreach` over range<uintptr>(5) allocated {bytes} bytes across {LoopCount} loops");
    }

    [TestMethod]
    public void BoxedRangeOverIntStillAllocates()
    {
        long bytes = MeasureBoxed(8, out long sum);

        Assert.AreEqual(28L * LoopCount, sum, "boxed range(8) did not yield 0..7");
        Assert.IsTrue(bytes > 0,
            $"the boxed control allocated {bytes} bytes: it must allocate, or the zeros asserted beside it prove " +
            "nothing about the enumerator.");
    }

    [TestMethod]
    public void RangeOverIntYieldsGoValues()
    {
        // Go: no iterations for n <= 0, and the loop variable has the operand's type.
        var none = new List<nint>();

        foreach (var i in range((nint)(-3)))
            none.Add(i);

        CollectionAssert.AreEqual(Array.Empty<nint>(), none);

        var ints = new List<nint>();

        foreach (var i in range(3))
        {
            nint typed = i; // a C# int operand still yields Go's int
            ints.Add(typed);
        }

        CollectionAssert.AreEqual(new nint[] { 0, 1, 2 }, ints);

        var bytes = new List<byte>();

        foreach (var i in range<byte>(255))
        {
            byte typed = i;
            bytes.Add(typed);
        }

        Assert.AreEqual(255, bytes.Count);
        Assert.AreEqual((byte)254, bytes[^1]);

        // An enumerable is still one, for any consumer that is not foreach.
        var copied = new List<nint>(range((nint)4));

        CollectionAssert.AreEqual(new nint[] { 0, 1, 2, 3 }, copied);
    }
}
