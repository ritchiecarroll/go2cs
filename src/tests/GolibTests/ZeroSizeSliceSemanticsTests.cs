using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// A Go ZERO-SIZE element type occupies no storage, so <c>make([]struct{}, n)</c> allocates nothing
/// and succeeds for any <c>n</c> up to <c>math.MaxInt</c>. golib allocated a real backing array and
/// panicked <c>makeslice: len out of range</c> at <see cref="Array.MaxLength"/> — a ceiling Go does
/// not have, because Go has nothing to allocate.
/// </summary>
/// <remarks>
/// These are golib-level guards on purpose: the slices suite measures the same defect
/// (<c>TestRepeat</c> and <c>TestConcat_too_large</c> both die on their OWN
/// <c>make([]struct{}, MaxInt)</c> before reaching the function under test), but a package suite is a
/// coarse instrument for a per-operation contract. Every operation Go answers from length arithmetic
/// alone is pinned here — make, len/cap, index, range, subslice, append, copy, clear, equality —
/// because a fix that got any ONE of them wrong would still let the package row go green while
/// leaving the storage-free path unsound for the next consumer.
/// </remarks>
[TestClass]
public class ZeroSizeSliceSemanticsTests
{
    // The two shapes a Go `struct{}` arrives as: golib's own EmptyStruct (an anonymous `struct{}`)
    // and a converted NAMED empty struct, which the converter emits as a fieldless partial struct.
    private struct Void
    {
    }

    // A struct built ONLY from zero-size fields is itself zero-size in Go — the recursive half of
    // the rule, and the one a naive "no fields" predicate would miss.
    private struct VoidPair
    {
#pragma warning disable CS0169 // the fields exist to BE the shape under test; nothing reads them
        private readonly Void m_first;
        private readonly EmptyStruct m_second;
#pragma warning restore CS0169
    }

    [TestMethod]
    public void MakeAtGoMaxIntAllocatesNothingAndKeepsItsLength()
    {
        // The measurement that named the defect. nint.MaxValue IS Go's math.MaxInt on a 64-bit host,
        // and it is ~4.3 billion times Array.MaxLength: nothing but a storage-free path can answer.
        slice<Void> s = new(nint.MaxValue);

        Assert.AreEqual(nint.MaxValue, len(s), "a zero-size slice must carry Go's own length");
        Assert.AreEqual(nint.MaxValue, cap(s), "and Go's own capacity");
        Assert.IsFalse(s == nil, "a `make`d slice is never nil, whatever its element type");
    }

    [TestMethod]
    public void MakeAtGoMaxIntWorksForEveryZeroSizeShape()
    {
        Assert.AreEqual(nint.MaxValue, len(new slice<EmptyStruct>(nint.MaxValue)), "anonymous struct{}");
        Assert.AreEqual(nint.MaxValue, len(new slice<VoidPair>(nint.MaxValue)), "a struct of only zero-size fields is zero-size too");
    }

    [TestMethod]
    public void MakeWithSeparateCapacityKeepsBothNumbers()
    {
        slice<Void> s = new(nint.MaxValue / 2, nint.MaxValue);

        Assert.AreEqual(nint.MaxValue / 2, len(s));
        Assert.AreEqual(nint.MaxValue, cap(s));
    }

    [TestMethod]
    public void NegativeLengthStillPanicsLikeGo()
    {
        // The ceiling goes; Go's OWN rule stays. A storage-free path that accepted everything would
        // have traded one divergence for another.
        PanicException panic = Assert.ThrowsException<PanicException>(() => _ = new slice<Void>(-1));

        StringAssert.Contains(panic.Message, "makeslice", "a negative make length must raise Go's own recoverable panic");
    }

    [TestMethod]
    public void OrdinaryElementTypesKeepTheirAllocationCeiling()
    {
        // The positive control for the gate itself: the zero-size exemption must not leak into a
        // type that genuinely needs storage, or `make([]int, MaxInt)` would report success and fault
        // on first use.
        Assert.ThrowsException<PanicException>(() => _ = new slice<int>(nint.MaxValue), "an int slice has a real backing and must still refuse MaxInt");
        Assert.ThrowsException<PanicException>(() => _ = new slice<Void[]>(nint.MaxValue), "an array of zero-size elements is a REFERENCE in C# and is not zero-size");
    }

    [TestMethod]
    public void IndexingStaysInBoundsAndReadsTheZeroValue()
    {
        slice<Void> s = new(nint.MaxValue);

        // Go's &s[i] is data + i*0 — every index names the same address — so any in-range index is
        // readable and out-of-range still panics against the LENGTH.
        Assert.AreEqual(default(Void), s[(nint)0]);
        Assert.AreEqual(default(Void), s[nint.MaxValue - 1], "the last element of a MaxInt-long slice is in range");
        Assert.ThrowsException<PanicException>(() => _ = s[nint.MaxValue], "one past the end must still panic");
        Assert.ThrowsException<PanicException>(() => _ = s[(nint)(-1)]);
    }

    [TestMethod]
    public void SubslicingIsPureArithmeticAndKeepsGoBounds()
    {
        slice<Void> s = new(nint.MaxValue);
        slice<Void> tail = s.slice(nint.MaxValue - 10);

        Assert.AreEqual((nint)10, len(tail));
        Assert.AreEqual((nint)10, cap(tail));

        slice<Void> head = s.slice(0, 4);
        Assert.AreEqual((nint)4, len(head));
        Assert.AreEqual(nint.MaxValue, cap(head), "cap runs to the end of the source window, exactly as Go's does");

        Assert.ThrowsException<PanicException>(() => s.slice(0, nint.MaxValue, 4), "max < high must still panic");
    }

    [TestMethod]
    public void AppendGrowsTheLengthWithoutStorage()
    {
        slice<Void> s = new(0, 4);

        s = append(s, default(Void));
        Assert.AreEqual((nint)1, len(s));
        Assert.AreEqual((nint)4, cap(s), "an append within capacity does not regrow");

        s = append(s, default(Void), default(Void), default(Void));
        Assert.AreEqual((nint)4, len(s));
        Assert.AreEqual((nint)4, cap(s));

        s = append(s, default(Void));
        Assert.AreEqual((nint)5, len(s));
        Assert.IsTrue(cap(s) >= 5, "beyond capacity the slice regrows, as any other would");
    }

    [TestMethod]
    public void AppendOntoANilZeroSizeSliceProducesANonNilSlice()
    {
        slice<Void> s = default;

        Assert.IsTrue(s == nil, "the zero header is nil for a zero-size element type too");

        s = append(s, default(Void), default(Void));

        Assert.IsFalse(s == nil);
        Assert.AreEqual((nint)2, len(s));
        Assert.AreEqual((nint)2, cap(s));
    }

    [TestMethod]
    public void AppendAtGoScaleNeitherAllocatesNorTruncates()
    {
        // The shape slices.Grow reaches: a huge zero-size window appended within capacity. A path
        // that materialized storage — or that let the length cross a 32-bit boundary — would fail
        // here rather than deep inside a package suite.
        slice<Void> s = new(nint.MaxValue - 4, nint.MaxValue);

        s = append(s, default(Void), default(Void));

        Assert.AreEqual(nint.MaxValue - 2, len(s));
        Assert.AreEqual(nint.MaxValue, cap(s));
    }

    [TestMethod]
    public void CopyReportsTheMinimumAndMovesNothing()
    {
        // Go's copy is memmove(dst, src, n * elemSize): for a zero-size element that is zero bytes,
        // and the RETURN is the whole observable result.
        slice<Void> destination = new(nint.MaxValue);
        slice<Void> source = new(nint.MaxValue / 2);

        Assert.AreEqual(nint.MaxValue / 2, copy(destination, source), "copy must report min(len(dst), len(src))");
        Assert.AreEqual(nint.MaxValue / 2, copy(source, destination), "and the minimum is symmetric");
    }

    [TestMethod]
    public void ClearIsANoOpRatherThanASpanRequest()
    {
        slice<Void> s = new(nint.MaxValue);

        clear(s);

        Assert.AreEqual(nint.MaxValue, len(s), "clear zeroes elements; it never changes the length");
    }

    [TestMethod]
    public void RangeVisitsEveryElementOfASmallZeroSizeSlice()
    {
        slice<Void> s = new(3);
        nint count = 0;
        nint lastIndex = -1;

        foreach ((nint index, Void value) in range(s))
        {
            count++;
            lastIndex = index;
            Assert.AreEqual(default(Void), value);
        }

        Assert.AreEqual((nint)3, count, "a range over a zero-size slice is a counted loop, as it is in Go");
        Assert.AreEqual((nint)2, lastIndex);
    }

    [TestMethod]
    public void NilAndEmptyStayDistinguishable()
    {
        // The invariant every slice construction path maintains, held here for the storage-free one:
        // Go distinguishes a nil slice from a non-nil empty one observably.
        Assert.IsTrue(default(slice<Void>) == nil);
        Assert.IsFalse(new slice<Void>(0) == nil, "make([]struct{}, 0) is empty but NOT nil");
        Assert.AreEqual((nint)0, len(new slice<Void>(0)));
    }

    [TestMethod]
    public void ASpanOfAnUnrepresentableLengthPanicsRatherThanTruncating()
    {
        // The honest edge. A Span<T> carries an int32 length while a zero-size slice's length is a Go
        // `int`, so a window past Array.MaxLength cannot be handed across that boundary. Truncating
        // would make `append` produce a slice of the wrong LENGTH — the one property of a zero-size
        // slice that IS observable — so the boundary raises Go's recoverable panic instead, and the
        // failure names the ceiling rather than arriving as a mangled result somewhere downstream.
        slice<Void> huge = new(nint.MaxValue);

        PanicException panic = Assert.ThrowsException<PanicException>(() => _ = huge.ToSpan());
        StringAssert.Contains(panic.Message, "ceiling", "the panic must name the limit it hit");

        // Below the ceiling the span is real, full length, and reads as the zero value.
        Span<Void> span = new slice<Void>(6).ToSpan();
        Assert.AreEqual(6, span.Length);
        Assert.AreEqual(default(Void), span[5]);
    }
}
