// StackAppendMove2heapTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Guards golib's half of Go 1.26's move2heap (cmd/compile/internal/slice): a variable whose one escape is a single
/// statement appends into its stack buffer, and <c>move2heap</c> copies it to the heap ahead of that statement.
/// </summary>
/// <remarks>
/// The three shapes are runtime's slice_test.go byteSlice, ptrSlice and byteCapSlice, written as the converter emits
/// them, and the tables are Go's own (TestAppendByteInLoop, TestAppendPtrInLoop, TestAppendByteCapInLoop at
/// go1.26.10, amd64): every row's allocation count, as <c>testing.AllocsPerRun</c> reads it from
/// <see cref="AllocationCounter"/>, and capacity. byteSlice and ptrSlice never observe the capacity, so they take
/// the whole buffer at the first append and move to the size class of the length (moveSliceNoCap); byteCapSlice
/// returns <c>cap(r)</c>, so its appends grow inside the buffer a size class at a time (growsliceBuf) and the move
/// keeps the capacity (moveSlice).
/// </remarks>
[TestClass]
public class StackAppendMove2heapTests
{
    private const int Runs = 10;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static slice<byte> byteSlice(nint n)
    {
        var rᴷ = new stackAppend<byte>(moves: true);
        slice<byte> r = default!;
        foreach (var i in range(n))
        {
            r = append(rᴷ, r, (byte)i);
        }
        r = move2heap(rᴷ, r);
        return r;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ptrSlice(nint n, ref slice<ж<byte>> p)
    {
        var rᴷ = new stackAppend<ж<byte>>(moves: true);
        slice<ж<byte>> r = default!;
        foreach (var _ in range(n))
        {
            r = append(rᴷ, r, (ж<byte>)(nil));
        }
        r = move2heap(rᴷ, r);
        p = r;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (slice<byte>, nint) byteCapSlice(nint n)
    {
        var rᴷ = new stackAppend<byte>(moves: true, capacityUsed: true);
        slice<byte> r = default!;
        foreach (var i in range(n))
        {
            r = append(rᴷ, r, (byte)i);
        }
        r = move2heap(rᴷ, r);
        return (r, cap(r));
    }

    // testing.AllocsPerRun's reading: a warm-up run, then the counter's delta over Runs runs, per run.
    private static long AllocsPerRun(Action f)
    {
        f();
        long before = AllocationCounter.CurrentThreadCount;

        for (int i = 0; i < Runs; i++)
            f();

        return (AllocationCounter.CurrentThreadCount - before) / Runs;
    }

    private static readonly (nint n, long allocs, nint cap)[] ByteTable =
    [
        (0, 0, 0), (1, 1, 8), (2, 1, 8), (8, 1, 8), (9, 1, 16), (16, 1, 16), (17, 1, 24), (24, 1, 24),
        (25, 1, 32), (32, 1, 32), (33, 1, 64), (48, 1, 64), (49, 1, 64), (64, 1, 64), (65, 2, 128),
    ];

    [TestMethod]
    public void ByteSliceMatchesGosTable()
    {
        AllocationCounter.Enable();

        foreach (var (n, want, wantCap) in ByteTable)
        {
            slice<byte> r = default!;
            long got = AllocsPerRun(() => r = byteSlice(n));

            Assert.AreEqual(want, got, $"byteSlice({n}): allocs");
            Assert.AreEqual(wantCap, cap(r), $"byteSlice({n}): capacity");
            Assert.AreEqual(n, len(r), $"byteSlice({n}): length");

            for (nint i = 0; i < n; i++)
                Assert.AreEqual((byte)i, r[i], $"byteSlice({n})[{i}]");
        }
    }

    [TestMethod]
    public void PtrSliceMatchesGosTable()
    {
        AllocationCounter.Enable();

        (nint n, long allocs, nint cap)[] table =
        [
            (0, 0, 0), (1, 1, 1), (2, 1, 2), (3, 1, 3), (4, 1, 4), (5, 1, 8), (6, 1, 8), (7, 1, 8), (8, 1, 8), (9, 2, 16),
        ];

        foreach (var (n, want, wantCap) in table)
        {
            slice<ж<byte>> r = default!;
            long got = AllocsPerRun(() => ptrSlice(n, ref r));

            Assert.AreEqual(want, got, $"ptrSlice({n}): allocs");
            Assert.AreEqual(wantCap, cap(r), $"ptrSlice({n}): capacity");
            Assert.AreEqual(n, len(r), $"ptrSlice({n}): length");
        }
    }

    [TestMethod]
    public void ByteCapSliceMatchesGosTable()
    {
        AllocationCounter.Enable();

        foreach (var (n, want, wantCap) in ByteTable)
        {
            slice<byte> r = default!;
            nint c = 0;
            long got = AllocsPerRun(() => (r, c) = byteCapSlice(n));

            Assert.AreEqual(want, got, $"byteCapSlice({n}): allocs");
            Assert.AreEqual(wantCap, cap(r), $"byteCapSlice({n}): capacity");
            Assert.AreEqual(wantCap, c, $"byteCapSlice({n}): the returned cap(r)");

            for (nint i = 0; i < n; i++)
                Assert.AreEqual((byte)i, r[i], $"byteCapSlice({n})[{i}]");
        }
    }

    [TestMethod]
    public void Move2heapLeavesAHeapSliceAndANilAlone()
    {
        var rᴷ = new stackAppend<byte>(moves: true);
        slice<byte> nilSlice = default!;

        Assert.IsTrue(move2heap(rᴷ, nilSlice) == nil, "a nil slice stays nil");

        slice<byte> heap = new slice<byte>(new byte[] { 1, 2, 3 });
        slice<byte> moved = move2heap(rᴷ, heap);

        Assert.IsTrue(ReferenceEquals(heap.m_array, moved.m_array), "a slice outside the buffer is returned as it is");
    }

    [TestMethod]
    public void TheMovedSliceNoLongerSharesTheBuffer()
    {
        // The buffer is taken (cap 32), then moved to cap 8: a later write to the moved copy must not reach the
        // buffer, and the moved copy is zero past its length (moveSliceNoCap clears it).
        var rᴷ = new stackAppend<byte>(moves: true);
        slice<byte> r = default!;
        r = append(rᴷ, r, (byte)7);
        slice<byte> inBuffer = r;

        r = move2heap(rᴷ, r);
        r[0] = 9;

        Assert.AreEqual((byte)7, inBuffer[0], "the move copies; it does not alias");
        Assert.AreEqual((nint)8, cap(r));

        slice<byte> full = r[..8];

        for (nint i = 1; i < 8; i++)
            Assert.AreEqual((byte)0, full[i], $"moved[{i}] past the length");
    }

    [TestMethod]
    public void GrowingInTheBufferClimbsTheSizeClasses()
    {
        // growsliceBuf: 8, 16, 24, 32 inside a 32-byte buffer with no allocation, then growslice from 32 to 64.
        AllocationCounter.Enable();

        var rᴷ = new stackAppend<byte>(moves: true, capacityUsed: true);
        slice<byte> r = default!;
        long before = AllocationCounter.CurrentThreadCount;
        var caps = new System.Collections.Generic.List<nint>();

        for (int i = 0; i < 33; i++)
        {
            r = append(rᴷ, r, (byte)i);

            if (caps.Count == 0 || caps[^1] != cap(r))
                caps.Add(cap(r));
        }

        CollectionAssert.AreEqual(new nint[] { 8, 16, 24, 32, 64 }, caps);
        Assert.AreEqual(1L, AllocationCounter.CurrentThreadCount - before, "only the growth out of the buffer allocates");
    }

    // GC reachability. The buffer local is passed by reference, so the JIT reports it for the caller's whole frame:
    // a store it still held after the slice left it would keep the store's elements reachable where Go's dead stack
    // buffer does not, the retention runtime's TestGCTestIsReachable reads (two odd entries of `all`'s first store).
    // So only a moving variable's buffer keeps its store, it lets go when growth leaves the buffer, and the move lets go
    // of it either way. Retention in an optimized frame is not observable from a Debug build, so these arms read the
    // field the frame would report, and runtime's own row is the behavior.

    [TestMethod]
    public void ABufferThatDoesNotMoveNeverHoldsItsStore()
    {
        var rᴷ = new stackAppend<ж<byte>>();
        slice<ж<byte>> r = default!;
        r = append(rᴷ, r, (ж<byte>)(nil));

        Assert.AreEqual((nint)4, cap(r), "the append took the 4-element buffer");
        Assert.IsNull(rᴷ.store, "a non-moving buffer holds the store it handed out");
    }

    [TestMethod]
    public void AGrowthThatLeavesTheBufferLetsGoOfTheStore()
    {
        var rᴷ = new stackAppend<ж<byte>>(moves: true, capacityUsed: true);
        slice<ж<byte>> r = default!;

        for (int i = 0; i < 4; i++)
            r = append(rᴷ, r, (ж<byte>)(nil));

        Assert.IsNotNull(rᴷ.store, "while the slice is in the buffer, the buffer keeps it");

        r = append(rᴷ, r, (ж<byte>)(nil));

        Assert.AreEqual((nint)8, cap(r));
        Assert.IsNull(rᴷ.store, "the slice outgrew the buffer, and the buffer still holds the store");
    }

    [TestMethod]
    public void TheMoveLetsGoOfTheStore()
    {
        var rᴷ = new stackAppend<ж<byte>>(moves: true);
        slice<ж<byte>> r = default!;
        r = append(rᴷ, r, (ж<byte>)(nil));

        Assert.IsNotNull(rᴷ.store, "a moving buffer keeps the store until the move");

        r = move2heap(rᴷ, r);

        Assert.IsNull(rᴷ.store, "the move is the buffer's last use, and the buffer still holds the store");
        Assert.AreEqual((nint)1, cap(r));
    }
}
