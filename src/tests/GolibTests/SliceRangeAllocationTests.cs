// SliceRangeAllocationTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards the allocation-free <c>for range</c> over <see cref="slice{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// A converted <c>for i, v := range s</c> emits <c>foreach (var (i, v) in s)</c>. C# binds
/// <c>GetEnumerator</c> by PATTERN — the concrete return type, no interface — so the enumerator only
/// stays off the heap while <c>slice&lt;T&gt;.GetEnumerator()</c> returns a struct. It used to return
/// <c>IEnumerator&lt;(nint, T)&gt;</c> from an iterator method, which cost two heap objects on entry
/// to EVERY ranged loop in the corpus (the compiler-generated state machine plus the inner
/// <c>SliceEnumerator</c> class): Go code that allocates nothing allocated per loop in C#. That is the
/// 136-byte figure measured inside <c>time.parseRFC3339</c> and what
/// <c>time.TestUnmarshalTextAllocations</c> was reporting.
/// </para>
/// <para>
/// The regression this locks in is silent and cheap to reintroduce — restoring the interface return
/// type still compiles and still produces correct output, it just allocates again. So the assertion
/// is on measured bytes, not on shape.
/// </para>
/// </remarks>
[TestClass]
public class SliceRangeAllocationTests
{
    // Iteration count is high enough that a per-loop allocation is unmistakable against measurement
    // noise (the pre-fix cost was ~136 B per loop entry -> ~136 KB here) and low enough to stay fast.
    private const int LoopCount = 1000;

    /// <summary>
    /// Measures bytes allocated by N ranged loops over a slice. A struct enumerator allocates nothing.
    /// </summary>
    private static long MeasureRangeBytes(slice<byte> s, out long sum)
    {
        long total = 0;

        // Warm up: JIT the loop body and settle any first-call statics BEFORE the measurement window,
        // otherwise the tiering/JIT allocations land in the measured delta and read as a regression.
        for (int warm = 0; warm < 32; warm++)
        {
            foreach (var (i, v) in s)
                total += i + v;
        }

        total = 0;

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
        {
            foreach (var (i, v) in s)
                total += i + v;
        }

        long after = GC.GetAllocatedBytesForCurrentThread();

        sum = total;
        return after - before;
    }

    [TestMethod]
    public void RangeOverSliceAllocatesNothing()
    {
        slice<byte> s = new([1, 2, 3, 4, 5, 6, 7, 8]);

        long bytes = MeasureRangeBytes(s, out long sum);

        // Sanity: the loop really ran and really read the elements (index 0..7 + values 1..8).
        Assert.AreEqual((0 + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 8) * (long)LoopCount, sum,
            "range loop did not observe the expected (index, value) pairs");

        Assert.AreEqual(0L, bytes,
            $"`foreach` over slice<T> allocated {bytes} bytes across {LoopCount} loops " +
            $"({bytes / (double)LoopCount:F1} B/loop) — GetEnumerator() must return the concrete " +
            "struct enumerator, not IEnumerator<(nint, T)>.");
    }

    [TestMethod]
    public void RangeOverSlicedWindowAllocatesNothing()
    {
        // Non-zero m_low: the enumerator must report indices RELATIVE to the window (Go's
        // `for i := range s[2:]` starts at 0), and must still allocate nothing.
        slice<byte> s = new slice<byte>([1, 2, 3, 4, 5, 6, 7, 8])[2..6];

        long bytes = MeasureRangeBytes(s, out long sum);

        Assert.AreEqual((0 + 1 + 2 + 3 + 3 + 4 + 5 + 6) * (long)LoopCount, sum,
            "range over a sliced window did not produce window-relative indices");

        Assert.AreEqual(0L, bytes, $"`foreach` over a slice window allocated {bytes} bytes");
    }

    [TestMethod]
    public void RangeOverNilSliceAllocatesNothingAndYieldsNoElements()
    {
        // Go's `for range` over a nil slice is a zero-iteration loop, never a fault.
        slice<byte> s = default;

        long bytes = MeasureRangeBytes(s, out long sum);

        Assert.AreEqual(0L, sum, "range over a nil slice must yield no elements");
        Assert.AreEqual(0L, bytes, $"`foreach` over a nil slice allocated {bytes} bytes");
    }

    [TestMethod]
    public void EnumeratorStillSatisfiesTheInterfaceContract()
    {
        // slice<T> is IArray<T>, whose own GetEnumerator() yields (nint, T). The pattern path must not
        // have cost the interface path: `foreach` over an IArray<T>-typed local or a constrained type
        // parameter still enumerates the same pairs (boxing, as it always did). LINQ sees the values.
        slice<byte> s = new([10, 20, 30]);

        List<(nint, byte)> viaInterface = [];

        foreach ((nint i, byte v) in (IArray<byte>)s)
            viaInterface.Add((i, v));

        CollectionAssert.AreEqual(
            new[] { ((nint)0, (byte)10), ((nint)1, (byte)20), ((nint)2, (byte)30) },
            viaInterface,
            "IArray<T> pair enumeration diverged from the struct enumerator");

        // The T-typed interface view (IList<T>/IEnumerable<T>) is a separate, still-working path.
        List<byte> values = [];

        foreach (byte v in (IEnumerable<byte>)s)
            values.Add(v);

        CollectionAssert.AreEqual(new byte[] { 10, 20, 30 }, values,
            "IEnumerable<T> enumeration diverged");
    }
}
