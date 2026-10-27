// ElementAddressCompareTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// The ELEMENT-ADDRESS COMPARE (1.26.10.2 seat #10, docs/phase4/DESIGN-element-address-compare.md): when both
/// operands of <c>==</c> or <c>!=</c> are element address-ofs the converter emits each as <c>Ꮡelem(x, i)</c> or
/// <c>p.Ꮡelem&lt;E&gt;(i)</c>, which return the element's identity as a value (<see cref="ElementAddress{T}"/>)
/// instead of minting an element box.
/// </summary>
/// <remarks>
/// The GUARD is case by case: for every shape, <c>Ꮡelem(a, i) == Ꮡelem(b, j)</c> must answer what
/// <c>Ꮡ(a, i) == Ꮡ(b, j)</c> answers, so the rewrite changes what a compare costs and never what it says. The one
/// shape where they differ (a native slice element against the same element reached through a native array view)
/// is pinned on its own with Go's answer. The CHARGE arm measures heap bytes with
/// <see cref="GC.GetAllocatedBytesForCurrentThread"/> around a loop of compares: zero, where Go's AllocsPerRun reads 0.
/// </remarks>
[TestClass]
public class ElementAddressCompareTests
{
    // Both compares, and the hash a dictionary keyed on the identity would use.
    private static void AssertAgrees<T>(ElementAddress<T> left, ElementAddress<T> right, ж<T> boxedLeft, ж<T> boxedRight, bool expected, string shape)
    {
        Assert.AreEqual(expected, boxedLeft == boxedRight, $"{shape}: Ꮡ's answer");
        Assert.AreEqual(expected, left == right, $"{shape}: Ꮡelem must answer what Ꮡ answers");
        Assert.AreEqual(!expected, left != right, $"{shape}: != is the negation");

        if (expected)
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode(), $"{shape}: equal identities hash alike");
    }

    [TestMethod]
    public void SliceElementsCompareAsTheBoxesDo()
    {
        slice<int> s = new(new[] { 10, 20, 30, 40, 50 });
        slice<int> other = new(new[] { 10, 20, 30, 40, 50 });

        AssertAgrees(Ꮡelem(s, 2), Ꮡelem(s, 2), Ꮡ(s, 2), Ꮡ(s, 2), true, "one element twice");
        AssertAgrees(Ꮡelem(s, 1), Ꮡelem(s, 2), Ꮡ(s, 1), Ꮡ(s, 2), false, "two elements of one slice");
        AssertAgrees(Ꮡelem(s, 1), Ꮡelem(other, 1), Ꮡ(s, 1), Ꮡ(other, 1), false, "equal contents, two backings");

        slice<int> tail = s[1..];
        AssertAgrees(Ꮡelem(tail, 0), Ꮡelem(s, 1), Ꮡ(tail, 0), Ꮡ(s, 1), true, "a re-slice names the absolute element");
        AssertAgrees(Ꮡelem(tail, 1), Ꮡelem(s, 1), Ꮡ(tail, 1), Ꮡ(s, 1), false, "a re-slice's relative index");

        AssertAgrees(Ꮡelem(s, (nint)3), Ꮡelem(s, 3UL), Ꮡ(s, (nint)3), Ꮡ(s, 3UL), true, "the nint and ulong index forms");
    }

    [TestMethod]
    public void ArrayElementsCompareAsTheBoxesDo()
    {
        array<long> a = new(new long[] { 1, 2, 3, 4 });
        array<long> b = new(new long[] { 1, 2, 3, 4 });

        AssertAgrees(Ꮡelem(a, 3), Ꮡelem(a, 3), Ꮡ(a, 3), Ꮡ(a, 3), true, "one array element twice");
        AssertAgrees(Ꮡelem(a, 0), Ꮡelem(b, 0), Ꮡ(a, 0), Ꮡ(b, 0), false, "two arrays");

        // an array and a slice over its backing name one element
        slice<long> whole = a[..];
        AssertAgrees(Ꮡelem(a, 2), Ꮡelem(whole, 2), Ꮡ(a, 2), Ꮡ(whole, 2), true, "an array and a slice of it");

        // Go's (*[3]T)(s[1:4]): an array WINDOW over a slice's backing (array<T>.Alias)
        slice<long> s = new(new long[] { 5, 6, 7, 8, 9 });
        array<long> window = array<long>.Alias(s[1..4], 3);
        AssertAgrees(Ꮡelem(window, 0), Ꮡelem(s, 1), Ꮡ(window, 0), Ꮡ(s, 1), true, "an alias window's first element");
        AssertAgrees(Ꮡelem(window, 2), Ꮡelem(s, 2), Ꮡ(window, 2), Ꮡ(s, 2), false, "an alias window's index is relative");
    }

    [TestMethod]
    public void ZerobaseElementsCompareAsTheBoxesDo()
    {
        // every element of a make([]struct{}, n) is zerobase, whatever the index
        slice<EmptyStruct> empty = new(4);
        slice<EmptyStruct> another = new(2);
        AssertAgrees(Ꮡelem(empty, 0), Ꮡelem(empty, 3), Ꮡ(empty, 0), Ꮡ(empty, 3), true, "two zero-size elements");
        AssertAgrees(Ꮡelem(empty, 1), Ꮡelem(another, 0), Ꮡ(empty, 1), Ꮡ(another, 0), true, "two zero-size slices");

        // A zero-size element over a REAL array backing keeps its index. Go answers true here (every zero-size
        // element is one address); Ꮡ answers false today, and Ꮡelem answers what Ꮡ answers (the design note's c6).
        array<EmptyStruct> real = new(new EmptyStruct[3]);
        AssertAgrees(Ꮡelem(real, 0), Ꮡelem(real, 1), Ꮡ(real, 0), Ꮡ(real, 1), false, "zero-size elements of a real array");

        // the zero-capacity slot names zerobase for a non-zero-size element type too
        slice<int> slot = new(GoZeroCapacitySlot<int>.Backing);
        AssertAgrees(Ꮡelem(slot, 0), Ꮡelem(slot, 0), Ꮡ(slot, 0), GoZeroCapacityElement<int>.Element, true, "the zero-capacity slot");
    }

    [TestMethod]
    public void NativeSliceElementsCompareAsTheBoxesDo()
    {
        nint bytes = 8 * sizeof(ulong);
        nuint first = (nuint)(nint)Marshal.AllocHGlobal(bytes);
        nuint second = (nuint)(nint)Marshal.AllocHGlobal(bytes);

        try
        {
            slice<ulong> a = global::go.slice<ulong>.OverNativeMemory(first, 8);
            slice<ulong> sameBlock = global::go.slice<ulong>.OverNativeMemory(first, 8);
            slice<ulong> b = global::go.slice<ulong>.OverNativeMemory(second, 8);
            slice<ulong> managed = new(8);

            AssertAgrees(Ꮡelem(a, 2), Ꮡelem(sameBlock, 2), Ꮡ(a, 2), Ꮡ(sameBlock, 2), true, "two native windows over one block");
            AssertAgrees(Ꮡelem(a, 2), Ꮡelem(a, 3), Ꮡ(a, 2), Ꮡ(a, 3), false, "two native elements");
            AssertAgrees(Ꮡelem(a, 2), Ꮡelem(b, 2), Ꮡ(a, 2), Ꮡ(b, 2), false, "two native blocks");
            AssertAgrees(Ꮡelem(a[1..], 1), Ꮡelem(a, 2), Ꮡ(a[1..], 1), Ꮡ(a, 2), true, "a native re-slice");
            AssertAgrees(Ꮡelem(a, 0), Ꮡelem(managed, 0), Ꮡ(a, 0), Ꮡ(managed, 0), false, "a native element and a managed one");
        }
        finally
        {
            Marshal.FreeHGlobal((nint)first);
            Marshal.FreeHGlobal((nint)second);
        }
    }

    [TestMethod]
    public void ArrayPointerElementsCompareAsTheBoxesDo()
    {
        array<int> backing = new(new[] { 1, 2, 3, 4 });
        ж<array<int>> p = new StandardBox<array<int>>(backing);
        ж<array<int>> q = new StandardBox<array<int>>(new array<int>(4));

        AssertAgrees(p.Ꮡelem<int>(1), p.Ꮡelem<int>(1), p.at<int>(1), p.at<int>(1), true, "one element through one pointer");
        AssertAgrees(p.Ꮡelem<int>(1), p.Ꮡelem<int>(2), p.at<int>(1), p.at<int>(2), false, "two elements through one pointer");
        AssertAgrees(p.Ꮡelem<int>(0), q.Ꮡelem<int>(0), p.at<int>(0), q.at<int>(0), false, "two arrays");
        AssertAgrees(p.Ꮡelem<int>(3), Ꮡelem(backing, 3), p.at<int>(3), Ꮡ(backing, 3), true, "the pointer's array and the array itself");
        AssertAgrees(p.Ꮡelem<int>((nint)2), p.Ꮡelem<int>(2UL), p.at<int>((nint)2), p.at<int>(2UL), true, "the nint and ulong index forms");

        // a native array view takes at()'s element box and answers its identity (the fallback arm)
        nint bytes = 8 * sizeof(ulong);
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(bytes);

        try
        {
            ж<array<ulong>> native = NativeArrayBox<ulong>.Over(addr, 8);
            AssertAgrees(native.Ꮡelem<ulong>(5), native.Ꮡelem<ulong>(5), native.at<ulong>(5), native.at<ulong>(5), true, "one native array element twice");
            AssertAgrees(native.Ꮡelem<ulong>(5), native.Ꮡelem<ulong>(6), native.at<ulong>(5), native.at<ulong>(6), false, "two native array elements");
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    // The one STATED difference: Ꮡ mints a NativeBox for a native SLICE element, and a NativeBox compares false against
    // the element box a native array view hands out over the SAME address. Both name one Go address, so Go answers
    // true, and so does Ꮡelem.
    [TestMethod]
    public void ANativeSliceElementAndTheSameElementThroughANativeArrayPointerAreOneAddress()
    {
        nint bytes = 8 * sizeof(ulong);
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(bytes);

        try
        {
            slice<ulong> s = global::go.slice<ulong>.OverNativeMemory(addr, 8);
            ж<array<ulong>> p = NativeArrayBox<ulong>.Over(addr, 8);

            Assert.IsTrue(Ꮡelem(s, 4) == p.Ꮡelem<ulong>(4), "one native address, Go's answer");
            Assert.IsFalse(Ꮡelem(s, 4) == p.Ꮡelem<ulong>(3));
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void AnOutOfRangeIndexPanicsAsTheBoxTakeDoes()
    {
        slice<int> s = new(new[] { 1, 2, 3, 4 });
        array<int> a = new(new[] { 1, 2, 3, 4 });
        ж<array<int>> p = new StandardBox<array<int>>(a);

        AssertSamePanic(() => Ꮡ(s, 4), () => Ꮡelem(s, 4), "slice, past len");
        AssertSamePanic(() => Ꮡ(s[..2], 2), () => Ꮡelem(s[..2], 2), "slice, past len inside cap");
        AssertSamePanic(() => Ꮡ(s, (nint)(-1)), () => Ꮡelem(s, (nint)(-1)), "slice, negative");
        AssertSamePanic(() => Ꮡ(s, ulong.MaxValue), () => Ꮡelem(s, ulong.MaxValue), "slice, unsigned at its full value");
        AssertSamePanic(() => Ꮡ(a, 9), () => Ꮡelem(a, 9), "array");
        AssertSamePanic(() => Ꮡ(default(slice<int>), 0), () => Ꮡelem(default(slice<int>), 0), "nil slice");
        AssertSamePanic(() => p.at<int>(4), () => p.Ꮡelem<int>(4), "pointer to array");
        AssertSamePanic(() => p.at<int>(ulong.MaxValue), () => p.Ꮡelem<int>(ulong.MaxValue), "pointer to array, unsigned");
    }

    private static void AssertSamePanic(Action boxed, Action unboxed, string shape)
    {
        PanicException expected = Assert.ThrowsException<PanicException>(boxed, $"{shape}: Ꮡ panics");
        PanicException actual = Assert.ThrowsException<PanicException>(unboxed, $"{shape}: Ꮡelem panics");
        Assert.AreEqual(expected.Message, actual.Message, $"{shape}: with the same runtime error");
    }

    // Go's &x[i] == &y[j] allocates nothing; neither does this. Every allocation-free arm runs in the loop: a slice,
    // a re-slice, an array, an alias window, zerobase, a native slice and a pointer to an array.
    [TestMethod]
    public void AnElementAddressCompareAllocatesNothing()
    {
        slice<int> s = new(new[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        array<int> a = new(new[] { 1, 2, 3, 4 });
        array<int> window = array<int>.Alias(s[2..6], 4);
        slice<EmptyStruct> empty = new(4);
        ж<array<int>> p = new StandardBox<array<int>>(a);
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(8 * sizeof(long));

        try
        {
            slice<long> native = global::go.slice<long>.OverNativeMemory(addr, 8);

            int compares()
            {
                int equal = 0;

                for (int i = 0; i < 4; i++)
                {
                    if (Ꮡelem(s, i + 1) == Ꮡelem(s[1..], i)) equal++;
                    if (Ꮡelem(window, i) == Ꮡelem(s, i + 2)) equal++;
                    if (Ꮡelem(a, i) != Ꮡelem(s, i)) equal++;
                    if (Ꮡelem(empty, i) == Ꮡelem(empty, 0)) equal++;
                    if (Ꮡelem(native, i) == Ꮡelem(native[1..], i)) equal++;
                    if (p.Ꮡelem<int>(i) == Ꮡelem(a, (nint)i)) equal++;
                    if (p.Ꮡelem<int>((ulong)i) != p.Ꮡelem<int>(3 - i)) equal++;
                }

                return equal;
            }

            // warm-up: JIT and first-call initialization land outside the window
            int expected = compares();
            Assert.AreEqual(4 + 4 + 4 + 4 + 0 + 4 + 4, expected, "the loop's own answers");

            long before = GC.GetAllocatedBytesForCurrentThread();
            int total = 0;

            for (int run = 0; run < 1000; run++)
                total += compares();

            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.AreEqual(1000 * expected, total);
            Assert.AreEqual(0L, allocated, "28,000 element-address compares must allocate nothing, as Go's do");
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }
}
