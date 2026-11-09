// GoImageCopyTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

// copy through a Go-layout byte view of a REFERENCE-BEARING struct (docs/phase4/DESIGN-reinterpret-seam.md, COORD
// ruling (A)): syscall's darwin getdirentries_test.go:67 decodes each record with
// `copy((*[unsafe.Sizeof(dirent)]byte)(unsafe.Pointer(&dirent))[:], data)`, which the converter now emits as
// `copy(@unsafe.GoImage(Ꮡdirent).slice(), data)`. A fixed array in the struct is a managed array<T>, so the struct has
// no address and the old view was an order token that panicked on first use (the GoImageCopyThroughView behavioral
// project is the red). Each arm states what Go does.
[TestClass]
public class GoImageCopyTests
{
    // darwin's syscall.Dirent: Go layout Ino 0, Seekoff 8, Reclen 16, Namlen 18, Type 20, Name 21 (1024), Pad_cgo 1045
    // (3) -> size 1048.
    public struct DirentLike
    {
        public ulong Ino;
        public ulong Seekoff;
        public ushort Reclen;
        public ushort Namlen;
        public byte Type;
        public array<sbyte> Name;
        public array<byte> Pad_cgo;

        public DirentLike()
        {
            Name = new array<sbyte>(1024);
            Pad_cgo = new array<byte>(3);
        }
    }

    // Go layout: A 0, B 2 (two bytes), padding 4..7, C 8 -> size 16.
    public struct PairLike
    {
        public ushort A;
        public array<byte> B;
        public ulong C;

        public PairLike() => B = new array<byte>(2);
    }

    // A pointer field: no Go-layout image exists.
    public struct WithPointer
    {
        public ulong N;
        public ж<byte> P;
        public array<byte> B;

        public WithPointer() => B = new array<byte>(2);
    }

    // Larger than the 4 KiB stack image: the pooled path.
    public struct BigRecord
    {
        public uint Head;
        public array<byte> Body;

        public BigRecord() => Body = new array<byte>(5000);
    }

    private static slice<byte> DirentBytes()
    {
        slice<byte> data = make<slice<byte>>(1048);
        data[0] = 7;
        data[16] = 32;
        data[18] = 3;
        data[21] = (byte)'a';
        data[22] = (byte)'b';
        data[23] = (byte)'c';
        return data;
    }

    [TestMethod]
    public void AWholeRecordDecodesThroughTheView()
    {
        heap(new DirentLike(), out ж<DirentLike> dirent);

        nint n = copy(GoImage<DirentLike>.Of(dirent).slice(), DirentBytes());

        Assert.AreEqual((nint)1048, n, "copy returns min(len(view), len(src)), the struct's Go size");
        Assert.AreEqual(7UL, dirent.Value.Ino);
        Assert.AreEqual((ushort)32, dirent.Value.Reclen);
        Assert.AreEqual((ushort)3, dirent.Value.Namlen);
        Assert.AreEqual("abc", $"{(char)dirent.Value.Name[0]}{(char)dirent.Value.Name[1]}{(char)dirent.Value.Name[2]}");
    }

    [TestMethod]
    public void APartialCopyLeavesTheBytesPastIt()
    {
        heap(new DirentLike(), out ж<DirentLike> dirent);
        dirent.Value.Seekoff = 99;
        dirent.Value.Reclen = 5;

        nint n = copy(GoImage<DirentLike>.Of(dirent).slice(), DirentBytes()[..10]);

        // Go: bytes 0..9 land (Ino = 7; Seekoff's low two bytes become 0, so 99 -> 0), Reclen at 16 is untouched.
        Assert.AreEqual((nint)10, n);
        Assert.AreEqual(7UL, dirent.Value.Ino);
        Assert.AreEqual(0UL, dirent.Value.Seekoff);
        Assert.AreEqual((ushort)5, dirent.Value.Reclen);
    }

    [TestMethod]
    public void AReadThroughTheViewIsGosLayoutWithZeroPadding()
    {
        PairLike pair = new() { A = 0x0102, C = 0x1122334455667788 };
        pair.B[0] = 5;
        pair.B[1] = 6;
        heap(pair, out ж<PairLike> p);
        slice<byte> @out = make<slice<byte>>(16);

        nint n = copy(@out, GoImage<PairLike>.Of(p).slice());

        Assert.AreEqual((nint)16, n);
        CollectionAssert.AreEqual(new byte[] { 2, 1, 5, 6, 0, 0, 0, 0, 136, 119, 102, 85, 68, 51, 34, 17 }, @out.ToArray());
    }

    [TestMethod]
    public void AWindowWritesOnlyItsOwnBytes()
    {
        PairLike pair = new() { A = 0x0102, C = 0x1122334455667788 };
        pair.B[0] = 5;
        pair.B[1] = 6;
        heap(pair, out ж<PairLike> p);

        nint n = copy(GoImage<PairLike>.Of(p).slice(8, 10), new slice<byte>([0xAA, 0xBB, 0xCC]));

        // Go: "2 0x112233445566bbaa [5 6]".
        Assert.AreEqual((nint)2, n);
        Assert.AreEqual(0x112233445566bbaaUL, p.Value.C);
        Assert.AreEqual((byte)5, p.Value.B[0]);
        Assert.AreEqual((byte)6, p.Value.B[1]);
        Assert.AreEqual((ushort)0x0102, p.Value.A);
    }

    [TestMethod]
    public void AStructHoldingAPointerIsRefusedByName()
    {
        heap(new WithPointer(), out ж<WithPointer> w);

        PanicException refusal = Assert.ThrowsException<PanicException>(() => GoImage<WithPointer>.Of(w));

        StringAssert.Contains(refusal.Message, "field P holds a managed reference");
        StringAssert.Contains(refusal.Message, "no Go-layout image");
    }

    [TestMethod]
    public void TheViewsBoundsPanicAsGosDo()
    {
        heap(new PairLike(), out ж<PairLike> p);

        Assert.ThrowsException<PanicException>(() => GoImage<PairLike>.Of(p).slice(0, 17), "p[0:17] over a 16-byte view");
        Assert.ThrowsException<PanicException>(() => GoImage<PairLike>.Of(p).slice(10, 8), "p[10:8]");
        Assert.ThrowsException<PanicException>(() => GoImage<PairLike>.Of(null), "a nil pointer");
    }

    [TestMethod]
    public void ARecordLargerThanTheStackImageCopiesThroughThePool()
    {
        heap(new BigRecord(), out ж<BigRecord> big);
        slice<byte> data = make<slice<byte>>(5004);
        data[0] = 1;
        data[4] = 2;
        data[5003] = 3;

        nint n = copy(GoImage<BigRecord>.Of(big).slice(), data);

        Assert.AreEqual((nint)5004, n);
        Assert.AreEqual(1U, big.Value.Head);
        Assert.AreEqual((byte)2, big.Value.Body[0]);
        Assert.AreEqual((byte)3, big.Value.Body[4999]);
    }

    [TestMethod]
    public void ACopyAllocatesNothingOnTheHeap()
    {
        heap(new DirentLike(), out ж<DirentLike> dirent);
        slice<byte> data = DirentBytes();
        slice<byte> @out = make<slice<byte>>(1048);

        // Warm: the per-type plan is built once, here.
        copy(GoImage<DirentLike>.Of(dirent).slice(), data);
        copy(@out, GoImage<DirentLike>.Of(dirent).slice());

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < 100; i++)
        {
            copy(GoImage<DirentLike>.Of(dirent).slice(), data);
            copy(@out, GoImage<DirentLike>.Of(dirent).slice());
        }

        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.AreEqual(0L, allocated, $"{allocated} bytes allocated across 200 copies through the view");
    }
}
