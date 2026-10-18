// ByteViewArrayFieldTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// An ARRAY field of a struct viewed over bytes is a window into those bytes at the field's Go offset: os's
/// createMountPoint shape, <c>buf := (*MountPointReparseBuffer)(unsafe.Pointer(&amp;b[0]))</c> followed by
/// <c>&amp;buf.PathBuffer[0]</c>.
/// </summary>
/// <remarks>
/// The struct holds an <see cref="array{T}"/>, a managed reference, so the reinterpret mints a native box over
/// the pinned bytes, and the CLR layout of the struct is not Go's. Before the field door, the element view read
/// the struct at the CLR layout and took the bytes under the field as an array header: a zeroed buffer gave a
/// null array of length 0, and <c>&amp;buf.PathBuffer[0]</c> panicked with index out of range.
/// </remarks>
[TestClass]
public class ByteViewArrayFieldTests
{
    // internal/syscall/windows.MountPointReparseBuffer. Go layout: four uint16 at 0, 2, 4, 6; PathBuffer [1]uint16
    // at 8; size 10.
    public struct MountPoint
    {
        public ushort SubstituteNameOffset;
        public ushort SubstituteNameLength;
        public ushort PrintNameOffset;
        public ushort PrintNameLength;
        public array<ushort> PathBuffer;

        public MountPoint() => PathBuffer = new array<ushort>(1);

        // Converter-named, so the field view can read the Go field name off the accessor.
        public static ref array<ushort> ᏑPathBuffer(ref MountPoint s) => ref s.PathBuffer;
    }

    [TestMethod]
    public void ArrayFieldOfAByteViewWritesTheBytesAtTheGoOffset()
    {
        slice<byte> blob = new(16);
        ж<MountPoint> buf = Ꮡ(blob, 0).Reinterpret<byte, MountPoint>();

        ж<ushort> first = buf.at<ushort>(MountPoint.ᏑPathBuffer, 0);
        first.Value = 0x4142;

        Assert.AreEqual((byte)0x42, blob[8], "PathBuffer[0] is the bytes at Go offset 8");
        Assert.AreEqual((byte)0x41, blob[9]);

        for (int i = 0; i < 8; i++)
            Assert.AreEqual((byte)0, blob[i], $"byte {i} belongs to the scalar fields and was not written");

        for (int i = 10; i < 16; i++)
            Assert.AreEqual((byte)0, blob[i], $"byte {i} lies past the field and was not written");

        Assert.AreEqual((ushort)0x4142, buf.at<ushort>(MountPoint.ᏑPathBuffer, 0).Value, "a second view reads the same bytes");
    }

    [TestMethod]
    public void ArrayFieldOfAByteViewReadsBytesWrittenThroughTheBuffer()
    {
        slice<byte> blob = new(16);
        ж<MountPoint> buf = Ꮡ(blob, 0).Reinterpret<byte, MountPoint>();

        blob[8] = 0x34;
        blob[9] = 0x12;

        Assert.AreEqual((ushort)0x1234, buf.at<ushort>(MountPoint.ᏑPathBuffer, 0).Value);
    }

    [TestMethod]
    public void TheFieldsElementIsTheAddressOfItsBytes()
    {
        slice<byte> blob = new(16);
        ж<MountPoint> buf = Ꮡ(blob, 0).Reinterpret<byte, MountPoint>();

        uintptr element = (uintptr)buf.at<ushort>(MountPoint.ᏑPathBuffer, 0);
        uintptr bytes = (uintptr)Ꮡ(blob, 8);

        Assert.AreEqual(bytes, element, "&buf.PathBuffer[0] == &b[8]");
    }

    [TestMethod]
    public void TheDeclaredLengthBoundsTheElementDoor()
    {
        slice<byte> blob = new(16);
        ж<MountPoint> buf = Ꮡ(blob, 0).Reinterpret<byte, MountPoint>();

        Assert.ThrowsException<PanicException>(() => buf.at<ushort>(MountPoint.ᏑPathBuffer, 1), "PathBuffer is [1]uint16");
    }

    [TestMethod]
    public void AManagedStructKeepsItsOwnArray()
    {
        ж<MountPoint> box = Ꮡ(new MountPoint());

        box.at<ushort>(MountPoint.ᏑPathBuffer, 0).Value = 7;

        Assert.AreEqual((ushort)7, box.Value.PathBuffer[0]);
    }

    // os's createMountPoint fills the reparse buffer through an array pointer over the field's first element:
    // copy((*[0xffff]uint16)(unsafe.Pointer(&buf.PathBuffer[0]))[:n:n], target.pathBuf). The element is in native
    // storage, so the array pointer is a native block at its address and the slice is a window onto the bytes.
    [TestMethod]
    public void AnArrayPointerOverTheFieldsElementSlicesItsBytes()
    {
        slice<byte> blob = new(16);
        ж<MountPoint> buf = Ꮡ(blob, 0).Reinterpret<byte, MountPoint>();

        slice<ushort> path = array<ushort>.AliasPointer(buf.at<ushort>(MountPoint.ᏑPathBuffer, 0), 0xffff).slice(0, 3, 3);

        Assert.AreEqual((nint)3, len(path));
        Assert.AreEqual((nint)3, cap(path));
        Assert.AreEqual((nint)3, copy(path, new slice<ushort>(new ushort[] { 0x0102, 0x0304, 0x0506 })));

        byte[] want = [0, 0, 0, 0, 0, 0, 0, 0, 0x02, 0x01, 0x04, 0x03, 0x06, 0x05, 0, 0];

        for (int i = 0; i < want.Length; i++)
            Assert.AreEqual(want[i], blob[i], $"byte {i}");
    }

    [TestMethod]
    public void TheArrayPointersTwoIndexFormsSliceFromTheElement()
    {
        slice<byte> blob = new(16);
        ж<MountPoint> buf = Ꮡ(blob, 0).Reinterpret<byte, MountPoint>();
        ж<array<ushort>> p = array<ushort>.AliasPointer(buf.at<ushort>(MountPoint.ᏑPathBuffer, 0), 4);

        p.slice(1, 3)[1] = 0x0a0b;
        Assert.AreEqual((byte)0x0b, blob[12], "p[1:3][1] is element 2, Go offset 8 + 4");
        Assert.AreEqual((byte)0x0a, blob[13]);

        Assert.AreEqual((nint)1, len(p.slice(3)));
        Assert.AreEqual((nint)4, len(p.slice()));
        Assert.AreEqual((nint)3, cap(p.slice(1, 2)), "an array's slice keeps the rest of the array as capacity");
    }

    // An array has no capacity beyond its length, so Go's bound panics over one say "with length", where a slice's
    // say "with capacity". The native window is a slice, so the texts are the array's by construction here.
    [TestMethod]
    public void TheArrayPointersBoundPanicsAreTheArrays()
    {
        slice<byte> blob = new(16);
        ж<MountPoint> buf = Ꮡ(blob, 0).Reinterpret<byte, MountPoint>();
        ж<array<ushort>> p = array<ushort>.AliasPointer(buf.at<ushort>(MountPoint.ᏑPathBuffer, 0), 4);

        Assert.AreEqual("runtime error: slice bounds out of range [::5] with length 4", panicText(() => p.slice(0, 1, 5)));
        Assert.AreEqual("runtime error: slice bounds out of range [:5] with length 4", panicText(() => p.slice(0, 5)));
        Assert.AreEqual("runtime error: slice bounds out of range [5:4]", panicText(() => p.slice(5)));

        // The same texts as a managed array's, through the dereference.
        ж<array<ushort>> managed = Ꮡ(new array<ushort>(4));

        Assert.AreEqual(panicText(() => managed.slice(0, 1, 5)), panicText(() => p.slice(0, 1, 5)));
        Assert.AreEqual(panicText(() => managed.slice(0, 5)), panicText(() => p.slice(0, 5)));
        Assert.AreEqual(panicText(() => managed.slice(5)), panicText(() => p.slice(5)));
    }

    [TestMethod]
    public void AManagedArrayPointerSlicesThroughItsArray()
    {
        ж<array<ushort>> managed = Ꮡ(new array<ushort>(4));

        managed.slice(1, 3)[0] = 9;

        Assert.AreEqual((ushort)9, managed.Value[1]);
        Assert.AreEqual((nint)3, cap(managed.slice(1, 2)));
    }

    private static string panicText(Action action)
    {
        try
        {
            action();
        }
        catch (PanicException ex)
        {
            return ex.Message;
        }

        return "no panic";
    }
}
