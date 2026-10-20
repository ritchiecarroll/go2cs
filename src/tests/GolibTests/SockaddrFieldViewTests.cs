// SockaddrFieldViewTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

using syscall = go.syscall_package;

namespace GolibTests;

/// <summary>
/// The spelling net's hand-owned lookup_windows.cs reads a resolved sockaddr with: field views
/// (<c>.of(ᏑAddr).slice()</c>, <c>.of(ᏑPort).Value</c>, <c>.of(ᏑScope_id).Value</c>) rather than a whole read
/// (<c>.Value.Addr</c>, <c>(~a).Port</c>).
/// </summary>
/// <remarks>
/// <para>
/// On Windows today the pointer lookupIP and lookupPort convert back is a MANAGED box: the GetAddrInfoW hand-own
/// copies the native chain and hands out a pointer-order token for a managed <c>RawSockaddrInet4/6</c>, so the
/// whole read is served by managed storage and the refusal below is not reached. The field views are the form
/// that also holds when the pointer names NATIVE memory, where <c>Addr [4]byte</c> is an <c>array&lt;byte&gt;</c>
/// reference in the CLR layout and golib refuses the whole read by name. Both arms run here, on every host,
/// because the sockaddr types are the same on every GOOS.
/// </para>
/// </remarks>
[TestClass]
public unsafe class SockaddrFieldViewTests
{
    // sockaddr_in at Go's (and the kernel's) layout: family 2, port 443 in network order, a TEST-NET-1 (RFC 5737) address.
    private static readonly byte[] s_inet4 = [2, 0, 0x01, 0xBB, 192, 0, 2, 7, 0, 0, 0, 0, 0, 0, 0, 0];

    // syscall.Ntohs is windows-only; the swap is the whole of it on a little-endian host.
    private static ushort ntohs(ushort value) => (ushort)((value << 8) | (value >> 8));

    private static nint nativeCopy(byte[] image)
    {
        nint native = (nint)NativeMemory.AllocZeroed((nuint)image.Length);
        Marshal.Copy(image, 0, native, image.Length);
        return native;
    }

    [TestMethod]
    public void AWholeReadOfANativeSockaddrIsRefusedByName()
    {
        nint native = nativeCopy(s_inet4);

        try
        {
            ж<syscall.RawSockaddrInet4> a = (ж<syscall.RawSockaddrInet4>)(uintptr)(nuint)native;

            PanicException refused = Assert.ThrowsException<PanicException>(() => a.Value.Addr.Clone());
            StringAssert.Contains(refused.Message, "RawSockaddrInet4");
        }
        finally
        {
            NativeMemory.Free((void*)native);
        }
    }

    [TestMethod]
    public void TheFieldViewsReadANativeSockaddrInet4AtGoOffsets()
    {
        nint native = nativeCopy(s_inet4);

        try
        {
            ж<syscall.RawSockaddrInet4> a = (ж<syscall.RawSockaddrInet4>)(uintptr)(nuint)native;

            slice<byte> addr = a.of(syscall.RawSockaddrInet4.ᏑAddr).slice();
            CollectionAssert.AreEqual(new byte[] { 192, 0, 2, 7 }, addr.ToArray());
            Assert.AreEqual((nint)443, (nint)ntohs(a.of(syscall.RawSockaddrInet4.ᏑPort).Value));
        }
        finally
        {
            NativeMemory.Free((void*)native);
        }
    }

    [TestMethod]
    public void TheFieldViewsReadANativeSockaddrInet6AtGoOffsets()
    {
        // sockaddr_in6: family 23 (windows AF_INET6; the value is only data here), port 53, flowinfo 0,
        // 2001:db8::1, scope id 9.
        byte[] image = new byte[28];
        image[0] = 23;
        image[2] = 0;
        image[3] = 53;
        byte[] address = [0x20, 0x01, 0x0D, 0xB8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1];
        Array.Copy(address, 0, image, 8, 16);
        image[24] = 9;

        nint native = nativeCopy(image);

        try
        {
            ж<syscall.RawSockaddrInet6> a = (ж<syscall.RawSockaddrInet6>)(uintptr)(nuint)native;

            CollectionAssert.AreEqual(address, a.of(syscall.RawSockaddrInet6.ᏑAddr).slice().ToArray());
            Assert.AreEqual(9u, (uint)a.of(syscall.RawSockaddrInet6.ᏑScope_id).Value);
            Assert.AreEqual((nint)53, (nint)ntohs(a.of(syscall.RawSockaddrInet6.ᏑPort).Value));
        }
        finally
        {
            NativeMemory.Free((void*)native);
        }
    }

    [TestMethod]
    public void TheFieldViewsAgreeWithAWholeReadOfAManagedSockaddrBox()
    {
        // The GetAddrInfoW path: a managed box behind a pointer-order token, recovered by the same
        // `(ж<T>)(uintptr)` conversion lookupIP makes. Both spellings must answer the same bytes.
        ref syscall.RawSockaddrInet4 sa = ref heap(new syscall.RawSockaddrInet4(), out ж<syscall.RawSockaddrInet4> box);
        sa.Port = ntohs(443);
        sa.Addr[0] = 192;
        sa.Addr[1] = 0;
        sa.Addr[2] = 2;
        sa.Addr[3] = 7;

        ж<syscall.RawSockaddrInet4> a = (ж<syscall.RawSockaddrInet4>)(uintptr)box;

        CollectionAssert.AreEqual(a.Value.Addr.Clone()[..].ToArray(), a.of(syscall.RawSockaddrInet4.ᏑAddr).slice().ToArray());
        Assert.AreEqual(a.Value.Port, a.of(syscall.RawSockaddrInet4.ᏑPort).Value);
    }
}
