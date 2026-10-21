// StackAppendTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// golib's <see cref="stackAppend{T}"/> is Go 1.25+'s stack backing store for a non-escaping append: at a
/// variable's first qualifying append, a slice that must grow, is empty, and whose new length fits K = 32 / the
/// element's Go size takes a K-capacity buffer, once per call; every other append grows as growslice does.
/// </summary>
/// <remarks>
/// The expected capacities are Go 1.26.9's for the same appends, recorded by the StackAppendBackingStore behavioral
/// fixture under go run (no -N -l, so the backing store applies). RED before golib modeled the buffer: the loop read
/// 1 2 4 4 8 where Go reads 4 4 4 4 8, and a byte slice read 8 where Go reads 32.
/// </remarks>
[TestClass]
public class StackAppendTests
{
    private struct S40 { public long a, b, c, d, e; }

    [ClassInitialize]
    public static void EnableCounting(TestContext _) => AllocationCounter.Enable();

    // The fixture's loop: the first append takes the 4-element buffer (int64, K = 4), the next three fit and the
    // fifth grows from it as growslice does.
    [TestMethod]
    public void TheFirstAppendTakesTheBufferAndTheFifthGrows()
    {
        stackAppend<long> xᴷ = new();
        slice<long> x = default!;
        List<long> caps = [];

        for (int i = 0; i < 5; i++)
        {
            x = append(xᴷ, x, i);
            caps.Add(cap(x));
        }

        CollectionAssert.AreEqual(new long[] { 4, 4, 4, 4, 8 }, caps);
    }

    // The flag lives in the frame: a slice reset to nil and refilled at the same site grows as growslice does.
    [TestMethod]
    public void AResetSliceDoesNotTakeTheBufferAgain()
    {
        stackAppend<long> xᴷ = new();
        slice<long> x = default!;
        List<long> caps = [];

        for (int i = 0; i < 3; i++)
        {
            x = append(xᴷ, x, 1L);
            caps.Add(cap(x));
        }

        x = default!;

        for (int i = 0; i < 3; i++)
        {
            x = append(xᴷ, x, 2L);
            caps.Add(cap(x));
        }

        CollectionAssert.AreEqual(new long[] { 4, 4, 4, 1, 2, 4 }, caps);
    }

    // A fresh frame is a fresh flag: each new local starts unused, as each Go call does.
    [TestMethod]
    public void EachNewBufferLocalIsUnused()
    {
        static long FirstCap()
        {
            stackAppend<long> wᴷ = new();
            slice<long> w = default!;
            w = append(wᴷ, w, 1L);
            return cap(w);
        }

        Assert.AreEqual(4L, FirstCap());
        Assert.AreEqual(4L, FirstCap());
    }

    // K = 32 / the element's Go size.
    [TestMethod]
    public void KIs32BytesOverTheElementSize()
    {
        stackAppend<byte> bᴷ = new();
        Assert.AreEqual(32L, (long)cap(append(bᴷ, default(slice<byte>)!, (byte)'a')), "byte: K = 32");

        stackAppend<@string> sᴷ = new();
        Assert.AreEqual(2L, (long)cap(append(sᴷ, default(slice<@string>)!, (@string)"x")), "string (16 bytes): K = 2");

        stackAppend<slice<byte>> rᴷ = new();
        slice<slice<byte>> rows = append(rᴷ, default(slice<slice<byte>>)!, default(slice<byte>)!);
        Assert.AreEqual(1L, (long)cap(rows), "[]byte (24 bytes): K = 1");
        Assert.AreEqual(2L, (long)cap(append(rᴷ, rows, default(slice<byte>)!)), "the second append grows from K = 1");
    }

    // An unnamed array element is sized from the value it appends: [2]uint64 is 16 bytes (K = 2), [3]uint64 is 24
    // (K = 1), one C# type.
    [TestMethod]
    public void AnUnnamedArrayIsSizedFromTheValue()
    {
        stackAppend<array<ulong>> twoᴷ = new();
        Assert.AreEqual(2L, (long)cap(append(twoᴷ, default(slice<array<ulong>>)!, new array<ulong>(2))), "[2]uint64: K = 2");

        stackAppend<array<ulong>> threeᴷ = new();
        Assert.AreEqual(1L, (long)cap(append(threeᴷ, default(slice<array<ulong>>)!, new array<ulong>(3))), "[3]uint64: K = 1");
    }

    // No buffer at all: an element over 32 bytes, and a zero-size element (Go's getBackingStoreInfo returns nil for
    // both), so the append is growslice's.
    [TestMethod]
    public void AnElementOver32BytesOrOfZeroSizeTakesNoBuffer()
    {
        stackAppend<S40> wideᴷ = new();
        Assert.AreEqual(1L, (long)cap(append(wideᴷ, default(slice<S40>)!, default(S40))), "40-byte element: growslice's 1");

        stackAppend<EmptyStruct> zeroᴷ = new();
        Assert.AreEqual(1L, (long)cap(append(zeroᴷ, default(slice<EmptyStruct>)!, default(EmptyStruct))), "zero-size element: growslice's len");
    }

    // Exactly K elements fit; K + 1 do not, and growslice sizes them; a non-empty slice never takes the buffer.
    [TestMethod]
    public void OnlyAnEmptySliceWhoseNewLengthFitsTakesTheBuffer()
    {
        stackAppend<long> exactᴷ = new();
        Assert.AreEqual(4L, (long)cap(append(exactᴷ, default(slice<long>)!, 1L, 2L, 3L, 4L)), "four of K = 4");

        stackAppend<long> overᴷ = new();
        Assert.AreEqual(6L, (long)cap(append(overᴷ, default(slice<long>)!, 1L, 2L, 3L, 4L, 5L)), "five: growslice's 6");

        stackAppend<long> nonEmptyᴷ = new();
        Assert.AreEqual(2L, (long)cap(append(nonEmptyᴷ, new slice<long>(1), 2L)), "len 1: growslice's 2");
    }

    // An append that fits the capacity it has is in place and takes nothing, and leaves the buffer unused.
    [TestMethod]
    public void AnAppendWithinCapacityLeavesTheBufferUnused()
    {
        stackAppend<long> xᴷ = new();
        slice<long> x = new slice<long>(0, 8);
        x = append(xᴷ, x, 1L);

        Assert.AreEqual(8L, (long)cap(x), "within capacity: in place");
        Assert.AreEqual(4L, (long)cap(append(xᴷ, default(slice<long>)!, 1L)), "the buffer is still unused");
    }

    // Go's buffer lives in the frame, so taking it charges no allocation; growing past it charges growslice's one.
    [TestMethod]
    public void TakingTheBufferChargesNoAllocation()
    {
        Assert.AreEqual(0L, Charge(() =>
        {
            stackAppend<long> xᴷ = new();
            _ = append(xᴷ, default(slice<long>)!, 1L);
        }), "the stack buffer");

        Assert.AreEqual(1L, Charge(() =>
        {
            stackAppend<long> xᴷ = new();
            slice<long> x = append(xᴷ, default(slice<long>)!, 1L, 2L, 3L, 4L);
            _ = append(xᴷ, x, 5L);
        }), "growing past it");
    }

    // The overload must stay out of line. Inlined, its store-taking return builds the slice in a stack temp of the
    // caller's own that the JIT reports for the whole frame and nothing overwrites, so the first store, and every
    // element it held, stays reachable after the slice outgrows it: runtime's TestGCTestIsReachable read two extra
    // reachable objects (gcroot: the test's frame, that temp, the 4-element store). Retention in an optimized frame
    // cannot be observed from this Debug build, so this arm pins the attribute and runtime's row is the behavior.
    [TestMethod]
    public void TheBufferOverloadIsNeverInlined()
    {
        MethodInfo overload = typeof(builtin).GetMethods().Single(static m =>
            m.Name == "append" && m.GetParameters() is [{ ParameterType: { IsByRef: true } first }, ..] &&
            first.GetElementType() is { IsGenericType: true } buffer && buffer.GetGenericTypeDefinition() == typeof(stackAppend<>));

        Assert.IsTrue(overload.MethodImplementationFlags.HasFlag(MethodImplAttributes.NoInlining),
            "append(in stackAppend<T>, ...) lost [MethodImpl(NoInlining)]");
    }

    private static long Charge(Action action)
    {
        action();

        long before = AllocationCounter.CurrentThreadCount;
        action();

        return AllocationCounter.CurrentThreadCount - before;
    }
}
