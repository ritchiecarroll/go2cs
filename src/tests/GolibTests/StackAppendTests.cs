// StackAppendTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

    // Every builtin that can take the buffer or hold its store must stay out of line. Inlined, the append's
    // store-taking return builds the slice in a stack temp of the caller's own that the JIT reports for the whole frame
    // and nothing overwrites, so the first store, and every element it held, stays reachable after the slice outgrows
    // it: runtime's TestGCTestIsReachable read two extra reachable objects (gcroot: the test's frame, that temp, the
    // 4-element store). move2heap's slice in and slice out would sit in such temps the same way. The public append is
    // only an inlined check in front of appendBuffered, whose return is the one the inlined head returns: the head is
    // pinned as inlinable and every method behind it as never inlined. AnOutgrownStoreIsNotRetainedByTheFrame below
    // observes the retention in Release, and runtime's row is the behavior.
    [TestMethod]
    public void TheBufferOverloadIsNeverInlined()
    {
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        MethodInfo[] takers = typeof(builtin).GetMethods(all).Where(static m =>
            m.GetParameters() is [{ IsIn: true, ParameterType: { IsByRef: true } first }, ..] &&
            first.GetElementType() is { IsGenericType: true } buffer && buffer.GetGenericTypeDefinition() == typeof(stackAppend<>)).ToArray();

        CollectionAssert.AreEquivalent(new[] { "append", "appendBuffered", "move2heap" }, takers.Select(static m => m.Name).ToArray(),
            "the builtins taking `in stackAppend<T>` changed: pin each one below");

        foreach (MethodInfo taker in takers)
        {
            if (taker.Name == "append")
                Assert.IsTrue(taker.MethodImplementationFlags.HasFlag(MethodImplAttributes.AggressiveInlining),
                    "append(in stackAppend<T>, ...) is the inlined head in front of appendBuffered");
            else
                Assert.IsTrue(taker.MethodImplementationFlags.HasFlag(MethodImplAttributes.NoInlining),
                    $"{taker.Name}(in stackAppend<T>, ...) lost [MethodImpl(NoInlining)]");
        }
    }

    // TestGCTestIsReachable's shape, converted, in an optimized frame: sixteen fresh objects appended through a
    // buffered site (an interface is 16 bytes in Go, so K = 2), the two the first store held cleared from the heap
    // array the slice outgrew to, and the slice dropped. Only the outgrown store can still reach those two. The
    // control is the same site through LeakingAppend, the take inlined into the frame: it must retain them, or the
    // probe cannot see what it guards. Debug frames keep every local alive, so both arms are meaningful in Release only.
    [TestMethod]
    public void AnOutgrownStoreIsNotRetainedByTheFrame()
    {
        if (JitOptimizerDisabled(typeof(stackAppend<>).Assembly) || JitOptimizerDisabled(typeof(StackAppendTests).Assembly))
        {
            Assert.Inconclusive("a JIT-optimizer-disabled (Debug) build keeps every local of the frame alive, so this guard " +
                "measures nothing: run GolibTests -c Release");
            return;
        }

        const int rounds = 20;

        int leaked = Rounds(rounds, RunLeaking);

        if (leaked == 0)
        {
            Assert.Inconclusive($"the inlined take retained its outgrown store in 0/{rounds} rounds: this JIT reports no " +
                "frame temp for it, so the probe cannot see the retention it guards");
            return;
        }

        Assert.AreEqual(0, Rounds(rounds, RunBuffered),
            $"append(in stackAppend<T>, ...) retained its outgrown store in an optimized frame (the inlined take: {leaked}/{rounds})");
    }

    private static int Rounds(int rounds, Func<WeakReference[], int> run)
    {
        int retained = 0;

        for (int round = 0; round < rounds; round++)
            retained += run(new WeakReference[16]) > 0 ? 1 : 0;

        return retained;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static int RunBuffered(WeakReference[] weaks)
    {
        var allᴷ = new stackAppend<object>();
        slice<object> all = default!;

        for (int i = 0; i < weaks.Length; i++)
            all = append(allᴷ, all, Fresh(weaks, i));

        all[0] = null!;
        all[1] = null!;
        all = default!;

        return Alive(weaks);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static int RunLeaking(WeakReference[] weaks)
    {
        bool used = false;
        slice<object> all = default!;

        for (int i = 0; i < weaks.Length; i++)
            all = LeakingAppend(ref used, all, Fresh(weaks, i));

        all[0] = null!;
        all[1] = null!;
        all = default!;

        return Alive(weaks);
    }

    // The take as it read before the head split: inlined, so its slice is built in a temp of the caller's frame.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static slice<object> LeakingAppend(ref bool used, slice<object> slice, object elem)
    {
        if (slice.Length == 0 && slice.Capacity == 0 && !used)
        {
            used = true;
            object[] store = new object[2];
            store[0] = elem;

            return new slice<object>(store, 0, 1);
        }

        return go.slice<object>.Append(slice, elem);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object Fresh(WeakReference[] weaks, int i)
    {
        object o = new();
        weaks[i] = new WeakReference(o);
        return o;
    }

    // The first two objects are alive only if the first store is.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Alive(WeakReference[] weaks)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        return (weaks[0].IsAlive ? 1 : 0) + (weaks[1].IsAlive ? 1 : 0);
    }

    private static bool JitOptimizerDisabled(Assembly assembly) =>
        assembly.GetCustomAttribute<DebuggableAttribute>()?.IsJITOptimizerDisabled == true;

    private static long Charge(Action action)
    {
        action();

        long before = AllocationCounter.CurrentThreadCount;
        action();

        return AllocationCounter.CurrentThreadCount - before;
    }
}
