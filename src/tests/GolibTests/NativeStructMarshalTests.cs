// NativeStructMarshalTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// The linux keystone's layout remedy, driven through a FAKE kernel: a Go struct whose fixed array
/// makes it reference-bearing crosses as a Go-layout native copy and is decoded back, a
/// pointer-bearing one is refused by name, and anything unresolved or unprovable reaches the
/// kernel exactly as before (the token, and so EFAULT).
/// </summary>
/// <remarks>
/// The real-kernel arms (TCGETS on a pty, unix.Uname, an EFAULT for a miss) run on linux; these pin
/// the encode and decode byte by byte on any host.
/// </remarks>
[TestClass]
public unsafe class NativeStructMarshalTests
{
    // x/sys/unix's linux/amd64 Termios: four uint32 flags, a uint8 line discipline, Cc [19]uint8, then
    // two uint32 speeds. Go's layout: flags at 0/4/8/12, Line at 16, Cc at 17..35, Ispeed at 36 (aligned
    // up from 36), Ospeed at 40; size 44.
    public struct Termios
    {
        public uint Iflag;
        public uint Oflag;
        public uint Cflag;
        public uint Lflag;
        public byte Line;
        public array<byte> Cc;
        public uint Ispeed;
        public uint Ospeed;

        public Termios() => Cc = new array<byte>(19);
    }

    // A nested struct inside a reference-bearing one (Stat_t's Timespec fields beside X__unused [3]int64).
    public struct Timespec
    {
        public long Sec;
        public long Nsec;
    }

    public struct Stamped
    {
        public Timespec When;
        public array<long> Unused;

        public Stamped() => Unused = new array<long>(3);
    }

    // x/sys/unix's Iovec: a pointer field, which no encoding can give a native meaning.
    public struct Iovec
    {
        public ж<byte> Base;
        public ulong Len;
    }

    // A field whose size overflows the address space ([2^62]int64 is 2^65 bytes), beside an array that makes
    // the struct reference-bearing: no Go layout to encode it at, so not provable. Go refuses the same type
    // inside StructOf. (An array field left unsized is NOT this case: it reads as a Go [0]T, a legal
    // zero-size field.) Until seat #40 the unprovable field was a C# char, which then classified as Struct
    // and reached "unknown" only through the layout walk's depth cap; a char is Go's uint16 now, and a
    // struct holding one has a layout.
    public struct NoDims
    {
        [GoArrayDims(1L << 62)] public array<long> Huge;
        public array<byte> B;

        public NoDims() => B = new array<byte>(4);
    }

    // x/sys's InotifyEvent shape: a zero-size LAST field (Name [0]uint8) takes a trailing byte in Go.
    // Measured on go1.24.13: unsafe.Offsetof(Name) = 16, unsafe.Sizeof = 20.
    public struct InotifyLike
    {
        public int Wd;
        public uint Mask;
        public uint Cookie;
        public uint Len;
        public array<byte> Name;

        public InotifyLike() => Name = new array<byte>(0);
    }

    private const uint InotifyLikeGoSizeof = 20;

    private static readonly uintptr Zero = new(0);

    [TestMethod]
    public void AZeroSizeTailIsRefusedOrGetsGoSizeofNeverLess()
    {
        // A buffer short of Go's sizeof would let the kernel write past it: heap corruption rather than
        // EFAULT. So either the struct is refused (while a layout omits the trailing byte) or it gets a
        // buffer of exactly Go's size (once the layout includes it); a smaller buffer is the defect.
        nuint? size = NativeStructMarshal.MarshalledSizeOf(typeof(InotifyLike));

        Assert.IsTrue(size is null || size == InotifyLikeGoSizeof,
            $"a trailing-zero-size struct got a {size}-byte buffer; Go's sizeof is {InotifyLikeGoSizeof}");

        if (size is not null)
            return;

        // Refused: the token reaches the kernel unchanged, which answers EFAULT.
        heap(new InotifyLike(), out ж<InotifyLike> box);
        uintptr token = box;
        uintptr received = Zero;

        NativeStructMarshal.Call(new uintptr(1), token, Zero, Zero, Zero, Zero, Zero, (num, a1, a2, a3, a4, a5, a6) =>
        {
            received = a1;
            return (Zero, Zero, Zero);
        });

        Assert.AreEqual(token, received);
    }

    [TestMethod]
    public void AZeroSizeTailGetsExactlyGoSizeofOnceTheLayoutCarriesTheTrailingByte()
    {
        // reflect's Go size carries the trailing byte of a zero-size last field, so the refused branch above
        // is unreachable here: this arm pins the layout fix itself from the marshal's side (a regression
        // back to an omitted byte reads refused, which the arm above would still accept).
        Assert.AreEqual((nuint?)InotifyLikeGoSizeof, NativeStructMarshal.MarshalledSizeOf(typeof(InotifyLike)),
            "a trailing-zero-size struct must marshal at Go's sizeof");
    }

    [TestMethod]
    public void ATermiosCrossesAsItsGoLayoutAndComesBackDecoded()
    {
        ref Termios termios = ref heap(new Termios(), out ж<Termios> box);
        termios.Iflag = 0x11223344;
        termios.Line = 0x5A;
        termios.Cc[5] = 7;
        termios.Ispeed = 0xCAFE;

        uintptr token = box;
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken((nuint)token), "precondition: a reference-bearing box answers a token");
        Assert.IsTrue(NativeStructMarshal.AnyToken(Zero, Zero, token, Zero, Zero, Zero));

        nint seen = 0;

        NativeStructMarshal.Call(new uintptr(16), new uintptr(1), new uintptr(0x5401), token, Zero, Zero, Zero, (num, a1, a2, a3, a4, a5, a6) =>
        {
            byte* p = (byte*)(nuint)a3;
            seen = (nint)p;

            // The copy the kernel sees is Go's layout, field by field.
            Assert.AreEqual(0x11223344u, *(uint*)(p + 0), "Iflag at 0");
            Assert.AreEqual((byte)0x5A, p[16], "Line at 16");
            Assert.AreEqual((byte)7, p[17 + 5], "Cc[5] at 22");
            Assert.AreEqual(0xCAFEu, *(uint*)(p + 36), "Ispeed at 36");

            // The kernel writes its answer.
            *(uint*)(p + 12) = 0x8A3B;
            p[17] = 3;

            return (Zero, Zero, Zero);
        });

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken((nuint)seen), "the kernel received an address, not the token");
        Assert.AreEqual(0x8A3Bu, box.Value.Lflag, "Lflag decoded back");
        Assert.AreEqual((byte)3, box.Value.Cc[0], "Cc[0] decoded back into the box's own array");
        Assert.AreEqual(0x11223344u, box.Value.Iflag, "a field the kernel did not touch keeps its value");
        Assert.AreEqual((byte)7, box.Value.Cc[5]);
    }

    [TestMethod]
    public void ANestedStructIsEncodedAndDecodedInPlace()
    {
        ref Stamped stamped = ref heap(new Stamped(), out ж<Stamped> box);
        stamped.When.Sec = 42;
        stamped.Unused[2] = -1;

        NativeStructMarshal.Call(new uintptr(1), box, Zero, Zero, Zero, Zero, Zero, (num, a1, a2, a3, a4, a5, a6) =>
        {
            long* p = (long*)(nuint)a1;

            Assert.AreEqual(42L, p[0], "When.Sec at 0");
            Assert.AreEqual(-1L, p[4], "Unused[2] at 32");

            p[1] = 999;
            return (Zero, Zero, Zero);
        });

        Assert.AreEqual(999L, box.Value.When.Nsec);
        Assert.AreEqual(42L, box.Value.When.Sec);
    }

    [TestMethod]
    public void APointerBearingStructIsRefusedByName()
    {
        heap(new Iovec(), out ж<Iovec> box);
        bool invoked = false;

        PanicException panic = Assert.ThrowsException<PanicException>(() =>
            NativeStructMarshal.Call(new uintptr(1), box, Zero, Zero, Zero, Zero, Zero, (num, a1, a2, a3, a4, a5, a6) =>
            {
                invoked = true;
                return (Zero, Zero, Zero);
            }));

        StringAssert.Contains(panic.Message, "Iovec");
        StringAssert.Contains(panic.Message, "Base");
        Assert.IsFalse(invoked, "the call must not run");
    }

    [TestMethod]
    public void AnUnresolvedTokenReachesTheKernelUnchanged()
    {
        // Tagged by construction (bit 63 set, bit 47 clear) and never registered: a MISS.
        uintptr miss = new(unchecked((nuint)((1UL << 63) | (0x1234UL << 32))));
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken((nuint)miss));

        uintptr received = Zero;

        NativeStructMarshal.Call(new uintptr(1), miss, Zero, Zero, Zero, Zero, Zero, (num, a1, a2, a3, a4, a5, a6) =>
        {
            received = a1;
            return (Zero, Zero, Zero);
        });

        Assert.AreEqual(miss, received, "a miss keeps the token, so the kernel answers EFAULT as before");
    }

    [TestMethod]
    public void AnUnprovableShapeReachesTheKernelUnchanged()
    {
        heap(new NoDims(), out ж<NoDims> box);
        uintptr token = box;
        uintptr received = Zero;

        NativeStructMarshal.Call(new uintptr(1), token, Zero, Zero, Zero, Zero, Zero, (num, a1, a2, a3, a4, a5, a6) =>
        {
            received = a1;
            return (Zero, Zero, Zero);
        });

        Assert.AreEqual(token, received);
    }

    [TestMethod]
    public void OrdinaryArgumentsAreNotTokensAndTheSameTokenIsOneBuffer()
    {
        Assert.IsFalse(NativeStructMarshal.AnyToken(new uintptr(1), new uintptr(unchecked((nuint)(-1L))), new uintptr(0x5401),
                                                    new uintptr(unchecked((nuint)(-100L))), Zero, new uintptr((nuint)0x7FFF_FFFF_F000)));

        heap(new Termios(), out ж<Termios> box);
        uintptr token = box;

        NativeStructMarshal.Call(new uintptr(1), token, token, Zero, Zero, Zero, Zero, (num, a1, a2, a3, a4, a5, a6) =>
        {
            Assert.AreEqual(a1, a2, "one box, one buffer");
            *(uint*)(nuint)a2 = 5;
            return (Zero, Zero, Zero);
        });

        Assert.AreEqual(5u, box.Value.Iflag);
    }
}
