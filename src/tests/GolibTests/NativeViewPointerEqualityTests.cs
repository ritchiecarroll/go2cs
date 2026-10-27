// NativeViewPointerEqualityTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// A pointer into NATIVE memory is its address, whichever box kind minted it (1.26.10.2 seat #16). Native memory is
/// reached through several kinds: <c>Ꮡ(s, i)</c> on a native slice mints a <see cref="NativeBox{T}"/>, an element take
/// through a native array view (<c>p.at&lt;T&gt;(i)</c> on a NativeArrayBox) an <see cref="ElemRefBox{T}"/>, a field
/// of a native struct a native-rooted field reference, and a uintptr turned back into a pointer a NativeBox. Each
/// kind compared only against its own, so two pointers to one native element compared UNEQUAL across kinds, where Go
/// answers equal. A converted program reaches it on linux: <c>b, _ := syscall.Mmap(...)</c>,
/// <c>p := (*[8]byte)(unsafe.Pointer(&amp;b[0]))</c>, then <c>q := &amp;b[4]; q == &amp;p[4]</c> (or
/// <c>(*byte)(unsafe.Pointer(&amp;b[4])) == &amp;p[4]</c>). Managed storage still never equals native.
/// </summary>
[TestClass]
public class NativeViewPointerEqualityTests
{
    private const int Length = 8;

    [TestMethod]
    public void ANativeSliceElementAndTheSameElementThroughANativeArrayViewAreOnePointer()
    {
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(Length * sizeof(ulong));

        try
        {
            slice<ulong> s = global::go.slice<ulong>.OverNativeMemory(addr, Length);
            ж<array<ulong>> p = NativeArrayBox<ulong>.Over(addr, Length);

            ж<ulong> viaSlice = Ꮡ(s, 4);
            ж<ulong> viaArray = p.at<ulong>(4);

            Assert.IsTrue(viaSlice == viaArray, "one native address, one Go pointer");
            Assert.IsTrue(viaArray == viaSlice, "and the comparison is symmetric");
            Assert.IsTrue(viaSlice.Equals((object)viaArray), "Equals(object) answers through the same rule");
            Assert.AreEqual(viaSlice.GetHashCode(), viaArray.GetHashCode(), "equal pointers hash equally");

            Assert.IsFalse(viaSlice == p.at<ulong>(3), "a different element is a different pointer");
            Assert.IsFalse(p.at<ulong>(5) == viaSlice);

            // A Go map keyed by one pointer finds the other.
            Dictionary<ж<ulong>, int> byPointer = new() { [viaSlice] = 1 };
            Assert.IsTrue(byPointer.ContainsKey(viaArray));
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void APointerRebuiltFromAnElementsAddressIsThatElement()
    {
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(Length * sizeof(ulong));

        try
        {
            ж<array<ulong>> p = NativeArrayBox<ulong>.Over(addr, Length);
            ж<ulong> element = p.at<ulong>(4);
            ж<ulong> rebuilt = (ж<ulong>)(uintptr)(addr + 4 * sizeof(ulong));

            Assert.IsTrue(rebuilt == element, $"{rebuilt.GetType().Name} against {element.GetType().Name}, one address");
            Assert.IsTrue(element == rebuilt);
            Assert.AreEqual(rebuilt.GetHashCode(), element.GetHashCode());
            Assert.IsFalse(rebuilt == p.at<ulong>(5));
            Assert.IsFalse(p.at<ulong>(3) == rebuilt);
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public unsafe void ANativeStructFieldAndTheSameBytesAsAnArrayElementAreOnePointer()
    {
        nint block = Marshal.AllocHGlobal(sizeof(ulong) * 2);

        try
        {
            NativeMemory.Clear((void*)block, sizeof(ulong) * 2);

            // RawSockaddrInet4: Family uint16 at offset 0, Port uint16 at offset 2.
            var sockaddr = (ж<syscall_package.RawSockaddrInet4>)(uintptr)(nuint)block;
            ж<ushort> port = sockaddr.of(syscall_package.RawSockaddrInet4.ᏑPort);
            ж<array<ushort>> words = NativeArrayBox<ushort>.Over((nuint)block, 8);
            ж<ushort> second = words.at<ushort>(1);

            Assert.IsTrue(port == second, $"{port.GetType().Name} against {second.GetType().Name}, one address");
            Assert.IsTrue(second == port);
            Assert.AreEqual(port.GetHashCode(), second.GetHashCode());
            Assert.IsFalse(port == words.at<ushort>(0), "Family is a different address");
            Assert.IsFalse(words.at<ushort>(0) == port);
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    [TestMethod]
    public void AManagedElementNeverEqualsANativeOne()
    {
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(Length * sizeof(ulong));

        try
        {
            slice<ulong> managed = new(new ulong[Length]);
            slice<ulong> native = global::go.slice<ulong>.OverNativeMemory(addr, Length);
            ж<array<ulong>> p = NativeArrayBox<ulong>.Over(addr, Length);

            Assert.IsFalse(Ꮡ(managed, 4) == p.at<ulong>(4));
            Assert.IsFalse(p.at<ulong>(4) == Ꮡ(managed, 4));
            Assert.IsFalse(Ꮡ(managed, 4) == Ꮡ(native, 4));
            Assert.IsFalse(Ꮡ(native, 4) == Ꮡ(managed, 4));

            // The CONTROL: managed elements keep their (backing, index) identity.
            Assert.IsTrue(Ꮡ(managed, 4) == Ꮡ(managed, 4));
            Assert.IsFalse(Ꮡ(managed, 4) == Ꮡ(managed, 3));
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }
}
