// RangeIndexOnlyTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// golib's index-only <c>range(x)</c> over a slice or an array, the converter's emission of Go's <c>for i := range x</c>:
/// it yields 0..len(x)-1, reads the length once, and never reads an element, as Go's index-only range does.
/// </summary>
/// <remarks>
/// The (index, value) enumerator the converter used before reads every element even when the value is discarded:
/// crypto/internal/fips140cache's TestCache never finished because that read kept the element it was waiting to see
/// collected reachable from the loop's frame. The element read is observable here without a GC: a native-backed slice
/// over memory that refuses a read panics on the first element. Red first: an overload that ranged through the pair
/// enumerator failed AnIndexOnlyRangeReadsNoElement with that panic.
/// </remarks>
[TestClass]
public class RangeIndexOnlyTests
{
    // A tagged order-token shape (bit 63 set, bit 47 clear) that no box has ever held: reading through it is refused.
    private const ulong UnregisteredOrderToken = (1UL << 63) | (0x1234UL << 32);

    private static List<nint> Indices(RangeOverInt<nint> range)
    {
        List<nint> indices = [];

        foreach (nint i in range)
            indices.Add(i);

        return indices;
    }

    [TestMethod]
    public void AnIndexOnlyRangeReadsNoElement()
    {
        slice<long> refusing = go.slice<long>.OverNativeMemory((nuint)UnregisteredOrderToken, 3);

        // The precondition: any element read of this slice is refused, as the pair enumerator's is.
        Assert.ThrowsException<PanicException>(() =>
        {
            foreach (var (_, _) in refusing) { }
        }, "the pair enumerator reads the element");

        CollectionAssert.AreEqual(new List<nint> { 0, 1, 2 }, Indices(range(refusing)));
    }

    [TestMethod]
    public void EachSequenceShapeYieldsItsIndices()
    {
        slice<long> s = new long[] { 7, 8, 9, 10 };
        array<long> a = new long[] { 1, 2 };
        ж<array<long>> p = new StandardBox<array<long>>(new long[] { 1, 2, 3 });

        CollectionAssert.AreEqual(new List<nint> { 0, 1, 2, 3 }, Indices(range(s)), "a slice");
        CollectionAssert.AreEqual(new List<nint> { 0, 1 }, Indices(range(a)), "an array");
        CollectionAssert.AreEqual(new List<nint> { 0, 1, 2 }, Indices(range(p)), "a pointer to an array");
        CollectionAssert.AreEqual(new List<nint> { 0, 1 }, Indices(range(s[2..])), "a window starts at its own 0");
        CollectionAssert.AreEqual(new List<nint>(), Indices(range(default(slice<long>))), "a nil slice ranges no times");
        CollectionAssert.AreEqual(new List<nint> { 0, 1, 2, 3 }, Indices(range((IArray)s)), "a sequence through IArray");
        CollectionAssert.AreEqual(new List<nint> { 0, 1, 2, 3 }, Indices(ThroughATypeParameter(s)), "a type parameter");
        CollectionAssert.AreEqual(new List<nint> { 0, 1, 2 }, Indices(range(new sslice<long>(stackalloc long[] { 4, 5, 6 }))), "a variadic parameter's stack slice");
    }

    // A converted generic over a slice-shaped type parameter binds the constrained range(in TSeq), with no boxing.
    private static RangeOverInt<nint> ThroughATypeParameter<S>(S s) where S : ISlice<long> => range(s);

    [TestMethod]
    public void TheOperandIsEvaluatedOnceAndItsLengthReadAtTheStart()
    {
        int evaluations = 0;
        slice<long> s = new long[] { 1, 2, 3 };

        slice<long> Operand()
        {
            evaluations++;
            return s;
        }

        int iterations = 0;

        foreach (nint _ in range(Operand()))
        {
            // Go fixes the iteration count when the loop starts: growing the variable in the body adds none.
            s = append(s, 4L);
            iterations++;
        }

        Assert.AreEqual(1, evaluations, "the range operand is evaluated once");
        Assert.AreEqual(3, iterations, "the length is read once, at the start");
    }
}
