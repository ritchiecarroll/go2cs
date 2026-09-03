// ArrayRangeAllocationTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards the allocation-free <c>for range</c> over <see cref="array{T}"/>, and the LIVE-read
/// contract the converter's range-expression snapshot depends on.
/// </summary>
/// <remarks>
/// <para>
/// The sibling <see cref="SliceRangeAllocationTests"/> locks the same property for
/// <see cref="slice{T}"/>, which shed ~136 B/loop when its <c>GetEnumerator</c> stopped being an
/// iterator method. <c>array&lt;T&gt;</c> kept that shape for a year longer because the semantics had
/// to be settled first: Go's <c>range</c> over an array VALUE iterates a COPY, so a snapshot has to
/// exist SOMEWHERE. It exists at the range EXPRESSION — the converter emits the same explicit
/// <c>.Clone()</c> every other Go array value-copy site takes — which leaves the enumerator free to be
/// the cheap, live-reading struct this file measures.
/// </para>
/// <para>
/// Both halves are guarded here, because each is silent to break: restoring the interface return type
/// still compiles and still produces correct output, it just allocates again; and making the
/// enumerator snapshot would allocate on every loop AND diverge from Go for <c>range p</c> over a
/// pointer-to-array and for <c>for i := range a</c>, where Go copies nothing. The Go-visible half is
/// output-compared by the <c>ArrayRangeSnapshot</c> behavioral test.
/// </para>
/// </remarks>
[TestClass]
public class ArrayRangeAllocationTests
{
    // Iteration count is high enough that a per-loop allocation is unmistakable against measurement
    // noise (the pre-fix cost was ~72 B per loop entry -> ~72 KB here) and low enough to stay fast.
    private const int LoopCount = 1000;

    /// <summary>
    /// Measures the bytes AND the golib object count charged by N ranged loops over the converter's
    /// range-expression snapshot — the exact shape a converted <c>for i, v := range a</c> emits.
    /// </summary>
    /// <remarks>
    /// Both meters are read because they answer different questions and a fix could satisfy only one:
    /// <see cref="AllocationCounter"/> is what <c>testing.AllocsPerRun</c> reports, while
    /// <c>GC.GetAllocatedBytesForCurrentThread</c> is the meter behind
    /// <c>runtime.ReadMemStats</c>'s <c>TotalAlloc</c>, which no counter could hide from. Go's array
    /// range copy is inline and costs zero on both.
    /// </remarks>
    private static long MeasureSnapshotBytes(array<byte> a, out long count, out long sum)
    {
        long total = 0;

        for (int warm = 0; warm < 32; warm++)
        {
            foreach (var (i, v) in a.ΔRangeSnapshot())
                total += i + v;
        }

        total = 0;

        long beforeCount = AllocationCounter.CurrentThreadCount;
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
        {
            foreach (var (i, v) in a.ΔRangeSnapshot())
                total += i + v;
        }

        long after = GC.GetAllocatedBytesForCurrentThread();

        count = AllocationCounter.CurrentThreadCount - beforeCount;
        sum = total;
        return after - before;
    }

    /// <summary>
    /// Measures bytes allocated by N ranged loops over an array through the PATTERN path — the shape
    /// a converted <c>for i, v := range a</c> binds. A struct enumerator allocates nothing.
    /// </summary>
    private static long MeasurePatternBytes(array<byte> a, out long sum)
    {
        long total = 0;

        // Warm up: JIT the loop body and settle any first-call statics BEFORE the measurement window,
        // otherwise the tiering/JIT allocations land in the measured delta and read as a regression.
        for (int warm = 0; warm < 32; warm++)
        {
            foreach (var (i, v) in a)
                total += i + v;
        }

        total = 0;

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
        {
            foreach (var (i, v) in a)
                total += i + v;
        }

        long after = GC.GetAllocatedBytesForCurrentThread();

        sum = total;
        return after - before;
    }

    /// <summary>
    /// The CONTROL: the same loop driven through <see cref="IEnumerable{T}"/>, which boxes the
    /// enumerator exactly as the old iterator method did. It must still allocate — a zero here would
    /// mean the measurement window, not the enumerator, is what changed.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasureBoxedBytes(array<byte> a, out long sum)
    {
        long total = 0;

        for (int warm = 0; warm < 32; warm++)
        {
            foreach ((nint i, byte v) in (IEnumerable<(nint, byte)>)a)
                total += i + v;
        }

        total = 0;

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
        {
            foreach ((nint i, byte v) in (IEnumerable<(nint, byte)>)a)
                total += i + v;
        }

        long after = GC.GetAllocatedBytesForCurrentThread();

        sum = total;
        return after - before;
    }

    [TestMethod]
    public void RangeOverArrayAllocatesNothing()
    {
        array<byte> a = new(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

        long bytes = MeasurePatternBytes(a, out long sum);

        // Sanity: the loop really ran and really read the elements (index 0..7 + values 1..8).
        Assert.AreEqual((0 + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 8) * (long)LoopCount, sum,
            "range loop did not observe the expected (index, value) pairs");

        Assert.AreEqual(0L, bytes,
            $"`foreach` over array<T> allocated {bytes} bytes across {LoopCount} loops " +
            $"({bytes / (double)LoopCount:F1} B/loop) — GetEnumerator() must return the concrete " +
            "struct enumerator, not IEnumerator<(nint, T)>.");
    }

    [TestMethod]
    public void BoxedRangeOverArrayStillAllocates()
    {
        // The control that makes the zero above trustworthy: the interface path is the SAME loop over
        // the SAME data with the SAME warm-up, differing only in which GetEnumerator binds. If this
        // ever reads zero, the instrument has stopped measuring and the assertion above is vacuous.
        array<byte> a = new(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

        long bytes = MeasureBoxedBytes(a, out long sum);

        Assert.AreEqual((0 + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 8) * (long)LoopCount, sum,
            "boxed range loop did not observe the expected (index, value) pairs");

        Assert.IsTrue(bytes > 0,
            $"the boxed control allocated {bytes} bytes — it must allocate, or the zero asserted by " +
            "RangeOverArrayAllocatesNothing proves nothing about the enumerator.");
    }

    [TestMethod]
    public void RangeOverAliasWindowAllocatesNothingAndIsWindowRelative()
    {
        // Non-zero m_low: Go's `(*[4]byte)(s[2:])` windows the slice's storage, and the enumerator
        // must report indices RELATIVE to that window (0-based), not to the backing store.
        array<byte> a = array<byte>.Alias(new slice<byte>([1, 2, 3, 4, 5, 6, 7, 8])[2..], 4);

        long bytes = MeasurePatternBytes(a, out long sum);

        Assert.AreEqual((0 + 1 + 2 + 3 + 3 + 4 + 5 + 6) * (long)LoopCount, sum,
            "range over an alias window did not produce window-relative indices");

        Assert.AreEqual(0L, bytes, $"`foreach` over an array alias window allocated {bytes} bytes");
    }

    [TestMethod]
    public void RangeOverZeroValueArrayAllocatesNothingAndYieldsNoElements()
    {
        // `default(array<T>)` ran no constructor, so its backing is null; enumerating it is a
        // zero-iteration loop, never a fault (the same null-safe zero value every other read takes).
        array<byte> a = default;

        long bytes = MeasurePatternBytes(a, out long sum);

        Assert.AreEqual(0L, sum, "range over a zero-value array must yield no elements");
        Assert.AreEqual(0L, bytes, $"`foreach` over a zero-value array allocated {bytes} bytes");
    }

    [TestMethod]
    public void EnumeratorReadsLiveStorageRatherThanASnapshot()
    {
        // The semantic half, stated from golib's side: the enumerator does NOT copy. Go's array-value
        // range DOES see a copy, but that copy is the range EXPRESSION's and the converter emits it as
        // an explicit `.Clone()`; snapshotting here as well would double the cost and would also copy
        // for the two shapes Go leaves shared — `range p` over a pointer-to-array, and `for i := range a`.
        array<byte> a = new(new byte[] { 1, 2, 3, 4 });

        List<byte> observed = [];

        foreach (var (i, v) in a)
        {
            if (i == 0)
                a[1] = 91;

            observed.Add(v);
        }

        CollectionAssert.AreEqual(new byte[] { 1, 91, 3, 4 }, observed,
            "array<T>'s enumerator must read live storage — the Go-visible copy is the converter's " +
            "range-expression Clone(), not a snapshot taken here.");

        // And the converter's snapshot, exercised through the same member it emits, hides the write —
        // which is what `for i, v := range a` must print.
        array<byte> b = new(new byte[] { 1, 2, 3, 4 });

        List<byte> viaClone = [];

        foreach (var (i, v) in b.Clone())
        {
            if (i == 0)
                b[1] = 91;

            viaClone.Add(v);
        }

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, viaClone,
            "the range-expression Clone() must snapshot the array Go copies");
    }

    [TestMethod]
    public void RangeSnapshotChargesNothingOnEitherMeter()
    {
        // Go's `for i, v := range a` over an array VALUE copies the array, and that copy is inline —
        // zero mallocs and zero TotalAlloc. golib's counter is the structural mirror of Go's
        // Mallocs, so a snapshot charged there would make every allocation assertion around such a
        // loop disagree with Go by construction; the byte meter behind runtime.ReadMemStats cannot
        // be hidden from at all, so the copy has to actually not allocate. It is pooled.
        AllocationCounter.Enable();

        array<byte> a = new(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

        long bytes = MeasureSnapshotBytes(a, out long count, out long sum);

        Assert.AreEqual((0 + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 8) * (long)LoopCount, sum,
            "the snapshot loop did not observe the expected (index, value) pairs");

        Assert.AreEqual(0L, count,
            $"the range snapshot charged {count} golib objects across {LoopCount} loops " +
            $"({count / (double)LoopCount:F3}/loop) — Go's array range copy is a stack copy and " +
            "charges zero mallocs, so testing.AllocsPerRun around such a loop would diverge.");

        Assert.AreEqual(0L, bytes,
            $"the range snapshot allocated {bytes} bytes across {LoopCount} loops " +
            $"({bytes / (double)LoopCount:F1} B/loop) — runtime.ReadMemStats' TotalAlloc reads the " +
            "CLR byte total directly, so a heap copy here shows up in every memory-budget assert.");
    }

    [TestMethod]
    public void CloneIsTheCountedCopyThatMakesTheSnapshotsZeroMeaningful()
    {
        // The POSITIVE CONTROL for the two zeros above, and the reason the range site does not simply
        // call Clone(): the same loop over the same data through Clone() — the copy every OTHER Go
        // array value-transfer site takes, correctly counted because its result outlives the
        // statement — must charge on both meters. If this ever reads zero, the meters have stopped
        // measuring and the assertions above prove nothing.
        AllocationCounter.Enable();

        array<byte> a = new(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        long total = 0;

        for (int warm = 0; warm < 32; warm++)
        {
            foreach (var (i, v) in a.Clone())
                total += i + v;
        }

        long beforeCount = AllocationCounter.CurrentThreadCount;
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
        {
            foreach (var (i, v) in a.Clone())
                total += i + v;
        }

        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        long count = AllocationCounter.CurrentThreadCount - beforeCount;

        Assert.IsTrue(total > 0, "the control loop did not run");

        Assert.AreEqual((long)LoopCount, count,
            $"Clone() charged {count} objects across {LoopCount} loops; it must charge exactly one " +
            "backing array per copy, or the snapshot's zero is being compared against nothing.");

        Assert.IsTrue(bytes > 0,
            $"Clone() allocated {bytes} bytes — it must allocate, or the byte meter is not measuring.");
    }

    [TestMethod]
    public void CloningAnArrayOfSlicesCopiesHeadersAndDoesNotThrow()
    {
        // The measured regression this file's sibling guard was written for. `ISlice<T>` derives from
        // `IArray<T>`, so a SLICE element passes an "is this an array wrapper?" test — and a named
        // slice wrapper's generated Clone() hands back the UNDERLYING slice<T> boxed, which the
        // element cast then cannot accept: InvalidCastException, which is how math/big's
        // TestFloatAdd/TestFloatMul died once anything cloned `[8]Bits` (its bitsList). Semantically
        // the re-clone is wrong too: Go's array-of-slices copy copies HEADERS and shares every
        // backing store. NamedSliceLike below stands in for the generated wrapper — it implements
        // ISlice (hence IArray) and returns a different type from ICloneable.Clone(), which is the
        // exact shape that threw.
        array<NamedSliceLike> arr = new(new[] { new NamedSliceLike(1), new NamedSliceLike(2) });

        array<NamedSliceLike> copy = arr.Clone();

        Assert.AreEqual(2, (int)copy.Length, "the array copy lost elements");
        Assert.AreSame(arr[0].Storage, copy[0].Storage,
            "a Go array-of-slices copy copies the slice HEADER and shares its backing store; " +
            "re-cloning the element would have replaced the backing.");

        // And the real thing: plain slice<T> elements take the same path.
        array<slice<byte>> slices = new(new[] { new slice<byte>([1, 2]), new slice<byte>([3, 4]) });
        array<slice<byte>> slicesCopy = slices.Clone();

        slicesCopy[0][0] = 99;

        Assert.AreEqual((byte)99, slices[0][0],
            "the copied slice header must still address the ORIGINAL backing store");
    }

    /// <summary>
    /// Minimal stand-in for a go2cs-gen named-slice wrapper: it implements <see cref="ISlice"/> — so
    /// it is assignable to <see cref="IArray"/> exactly as the generated wrapper is — while its
    /// <see cref="ICloneable.Clone"/> returns a DIFFERENT type, which is what turned the element
    /// re-clone into an <see cref="InvalidCastException"/>.
    /// </summary>
    private readonly struct NamedSliceLike : ISlice
    {
        public NamedSliceLike(byte seed) => Storage = new byte[] { seed, (byte)(seed + 1) };

        public byte[] Storage { get; }

        public Array? Source => Storage;

        public nint Length => Storage.Length;

        public nint Low => 0;

        public nint High => Storage.Length;

        public nint Capacity => Storage.Length;

        public nint Available => 0;

        public object? this[nint index]
        {
            get => Storage[index];
            set => Storage[index] = (byte)value!;
        }

        public ISlice? Append(object[] elems) => this;

        // The generated wrapper forwards to its underlying slice<T>, whose ICloneable.Clone() returns
        // a boxed slice<T> — NOT the wrapper type. Reproduced verbatim; that is the defect.
        public object Clone() => new slice<byte>(Storage);

        public IEnumerator GetEnumerator() => Storage.GetEnumerator();
    }

    [TestMethod]
    public void EnumeratorStillSatisfiesTheInterfaceContract()
    {
        // array<T> is IArray<T> is IEnumerable<(nint, T)>. The pattern path must not have cost the
        // interface path: LINQ, `foreach` over an interface-typed local, and anything holding the
        // array as IEnumerable<(nint, T)> still enumerate the same pairs (boxing, as they always did).
        array<byte> a = new(new byte[] { 10, 20, 30 });

        List<(nint, byte)> viaInterface = [];

        foreach ((nint i, byte v) in (IEnumerable<(nint, byte)>)a)
            viaInterface.Add((i, v));

        CollectionAssert.AreEqual(
            new[] { ((nint)0, (byte)10), ((nint)1, (byte)20), ((nint)2, (byte)30) },
            viaInterface,
            "IEnumerable<(nint, T)> enumeration diverged from the struct enumerator");

        // The T-typed interface view (IList<T>/IEnumerable<T>) is a separate, still-working path.
        List<byte> values = [];

        foreach (byte v in (IEnumerable<byte>)a)
            values.Add(v);

        CollectionAssert.AreEqual(new byte[] { 10, 20, 30 }, values,
            "IEnumerable<T> enumeration diverged");
    }
}
