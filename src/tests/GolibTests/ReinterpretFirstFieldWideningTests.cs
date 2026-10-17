// ReinterpretFirstFieldWideningTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// A REINTERPRET of a struct's first field, <c>(*byte)(unsafe.Pointer(&amp;rdb.header))</c>, names the struct's own
/// address, and a native callee given it may read past the field into the rest of the struct. Where that struct is
/// reference-bearing in C# but has a deep native image, the view crosses as its root's order token and the windows
/// funnel's deep form copies the whole root at Go's layout.
/// </summary>
/// <remarks>
/// The shape is os_windows_test.go's createDirLink: <c>DeviceIoControl(FSCTL_SET_REPARSE_POINT)</c> reads
/// <c>8 + ReparseDataLength</c> bytes from the header's address, so the MountPointReparseBuffer in
/// <c>detail [16384]byte</c> must follow the header inline. With the raw address of the header the kernel read a
/// managed reference there and refused the buffer (ERROR_INVALID_REPARSE_DATA, os TestRootMulti* on windows,
/// 2026-10-17). The callee here is a FAKE that reads and writes the bytes at the address it is given, so the arms
/// run on every host.
/// </remarks>
[TestClass]
public unsafe class ReinterpretFirstFieldWideningTests
{
    // REPARSE_DATA_BUFFER_HEADER. Go layout: Tag 0, Length 4, Reserved 6; size 8.
    public struct Header
    {
        public uint Tag;
        public ushort Length;
        public ushort Reserved;
    }

    // _REPARSE_DATA_BUFFER, cut down: the header first, then the detail bytes inline at Go offset 8.
    public struct Blob
    {
        public Header Head;
        public array<byte> Detail;

        public Blob() => Detail = new array<byte>(64);
    }

    // The same header first, beside a field with no native image: the root cannot be copied.
    public struct NamedBlob
    {
        public Header Head;
        public @string Name;
        public array<byte> Detail;

        public NamedBlob() => Detail = new array<byte>(8);
    }

    // The header at Go offset 4: a reinterpret of it names an interior address, not the root's.
    public struct PaddedBlob
    {
        public uint Pad;
        public Header Head;
        public array<byte> Detail;

        public PaddedBlob() => Detail = new array<byte>(8);
    }

    // Reference-free: the CLR layout is Go's, so the raw address already reaches every byte.
    public struct FlatBlob
    {
        public Header Head;
        public ulong Body;
    }

    // Field accessors named as the converter names them (Ꮡ + field), so each view resolves its Go offset.
    private static ref Header ᏑHead(object p) => ref ((ж<Blob>)p).Value.Head;

    private static ref Header ᏑNamedHead(object p) => ref ((ж<NamedBlob>)p).Value.Head;

    private static ref Header ᏑPaddedHead(object p) => ref ((ж<PaddedBlob>)p).Value.Head;

    private static ref Header ᏑFlatHead(object p) => ref ((ж<FlatBlob>)p).Value.Head;

    private sealed class Probe
    {
        public Func<ReadOnlySpan<nuint>, nuint>? Body;
    }

    private static nuint invoke(Probe probe, ReadOnlySpan<nuint> args) => probe.Body!(args);

    private static nuint callDeep(Probe probe, params nuint[] args) => NativeStructMarshal.CallDeep(args, probe, &invoke);

    private static ж<Blob> newBlob()
    {
        ref Blob blob = ref heap(new Blob(), out ж<Blob> root);
        blob.Head.Tag = 0xA0000003;
        blob.Head.Length = 16;

        for (int i = 0; i < 16; i++)
            blob.Detail[i] = (byte)(i + 1);

        return root;
    }

    [TestMethod]
    public void TheHeaderViewsNumberIsTheRootsToken()
    {
        ж<Blob> root = newBlob();
        ж<byte> view = root.of(ᏑHead).Reinterpret<Header, byte>();

        nuint viewNumber = ((uintptr)view).Value;
        nuint rootNumber = ((uintptr)root).Value;

        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(rootNumber), "precondition: a reference-bearing root answers a token");
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(viewNumber), "the byte view of the first field crosses as a token, not a raw interior address");
        Assert.AreEqual(rootNumber, viewNumber, "uintptr(unsafe.Pointer(&rdb.header)) == uintptr(unsafe.Pointer(&rdb)), as in Go");
    }

    [TestMethod]
    public void AUintptrRoundTripOfTheByteViewReadsAndWritesTheHeader()
    {
        ж<Blob> root = newBlob();
        ж<byte> view = root.of(ᏑHead).Reinterpret<Header, byte>();

        ж<byte> back = (ж<byte>)(uintptr)view;

        Assert.AreEqual((byte)0x03, back.Value, "the round trip reads the header's first byte");

        back.Value = 0x07;

        Assert.AreEqual(0xA0000007u, root.Value.Head.Tag, "a write through the round trip lands in the header");
        Assert.AreEqual((byte)0x07, view.Value, "and reads back through the original view");
    }

    [TestMethod]
    public void TheDeepFunnelHandsTheCalleeTheWholeRootAtGoLayout()
    {
        ж<Blob> root = newBlob();
        ж<byte> view = root.of(ᏑHead).Reinterpret<Header, byte>();

        Probe probe = new()
        {
            Body = args =>
            {
                byte* p = (byte*)args[0];

                Assert.AreEqual(0xA0000003u, *(uint*)p, "the header's tag at 0");
                Assert.AreEqual((ushort)16, *(ushort*)(p + 4), "the header's length at 4");

                for (int i = 0; i < 16; i++)
                    Assert.AreEqual((byte)(i + 1), p[8 + i], $"detail[{i}] inline at Go offset {8 + i}, after the header");

                // The callee writes into the detail, as FSCTL_GET_REPARSE_POINT would.
                p[8] = 0xEE;
                return 0;
            }
        };

        callDeep(probe, ((uintptr)view).Value);

        Assert.AreEqual((byte)0xEE, root.Value.Detail[0], "the copy decodes back into the root's own detail array");
        Assert.AreEqual(0xA0000003u, root.Value.Head.Tag, "a header the callee left alone keeps its value");
    }

    [TestMethod]
    public void ADirectPointerToTheFirstFieldKeepsItsRawAddress()
    {
        // `&o.o` overlapped words are direct field pointers the kernel keeps writing after the call returns; they
        // must stay the caller's own memory, never a copy freed at return.
        ж<Blob> root = newBlob();
        ж<Header> head = root.of(ᏑHead);

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(((uintptr)head).Value), "a direct field pointer is not widened");
    }

    [TestMethod]
    public void AReinterpretInARootWithNoNativeImageKeepsItsRawAddress()
    {
        ref NamedBlob named = ref heap(new NamedBlob(), out ж<NamedBlob> root);
        named.Name = "x";
        ж<byte> view = root.of(ᏑNamedHead).Reinterpret<Header, byte>();

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(((uintptr)view).Value), "a root with a string field has no copy to widen to");
    }

    [TestMethod]
    public void AReinterpretOfANonFirstFieldKeepsItsRawAddress()
    {
        heap(new PaddedBlob(), out ж<PaddedBlob> root);
        ж<byte> view = root.of(ᏑPaddedHead).Reinterpret<Header, byte>();

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(((uintptr)view).Value), "a field at Go offset 4 is not its root's address");
    }

    [TestMethod]
    public void AReinterpretInAReferenceFreeRootKeepsItsAddress()
    {
        heap(new FlatBlob(), out ж<FlatBlob> root);
        ж<byte> view = root.of(ᏑFlatHead).Reinterpret<Header, byte>();

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(((uintptr)view).Value), "a blittable root's CLR layout is Go's, so its address is already right");
    }
}
