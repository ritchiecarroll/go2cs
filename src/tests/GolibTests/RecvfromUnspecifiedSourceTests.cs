// RecvfromUnspecifiedSourceTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using unix = go.@internal.syscall.unix_package;

namespace GolibTests;

// net's TestUDPZeroByteBuffer failed on osx-arm64 (C1, 2026-11-08, run 42513405068): Go=pass, C#=fail with "read udp4
// ...: recvfrom: address family not supported by protocol family". It had passed there twice at 72043c1a66, so the failure
// is intermittent.
//
// The divergence, read at the tree: Go's recvfromInet4 / recvfromInet6 (syscall_unix.go:331 and :344) copy Port and Addr
// out of the raw sockaddr with NO family check. Go's generic Recvfrom (syscall_unix.go:319) skips the decode entirely when
// the family is AF_UNSPEC. The converted seams (internal/syscall/unix's RecvfromInet4/6, over syscall's
// GoReadNativeSockaddrInet4/6) DECODE the image and return EAFNOSUPPORT for any family other than the expected one,
// AF_UNSPEC included.
//
// When the kernel reports no source: on BSD a zero-length recvfrom can return 0 at once without writing an address, and
// darwin's loopback delivery is not synchronous, so TestUDPZeroByteBuffer's ReadFrom(nil) can race the datagram it just
// wrote. The first arm reproduces the no-source case deterministically: a zero-length read of an EMPTY non-blocking UDP
// socket, through the converted RecvfromInet4 itself. Go's contract there is "no decode error": on darwin (0, nil), and
// on linux EAGAIN, since linux's zero-length read of an empty socket waits for data.
//
// The second arm pins the decode itself on every unix flavour: a zeroed, zero-length image decodes the way Go's
// reinterpretation reads it (Port 0, Addr zero, no error). The third arm reads QUEUED datagrams with a zero-length buffer
// through syscall.Recvfrom and reports what it returns (count, source type, error) as READING lines; it only measures.
[TestClass]
public class RecvfromUnspecifiedSourceTests
{
    private static readonly byte[] Loopback = [127, 0, 0, 1];

    [TestMethod]
    public void AZeroLengthReadOfAnEmptyUdpSocketIsNotAnAddressFamilyError()
    {
        nint fd = OpenBoundUdpSocket(out _);

        try
        {
            Assert.IsNull(syscall_package.SetNonblock(fd, true), "set non-blocking");

            ж<syscall_package.SockaddrInet4> from = @new<syscall_package.SockaddrInet4>();
            from.Value.Port = 4321;

            (nint n, error err) = unix.RecvfromInet4(fd, default(slice<byte>), 0, from);

            Console.WriteLine($"READING empty-socket zero-length RecvfromInet4: n={n} err={(err is null ? "nil" : err.Error())} " +
                $"port={from.Value.Port} ({System.Runtime.InteropServices.RuntimeInformation.OSDescription.Trim()}, " +
                $"{System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture})");

            Assert.IsFalse(err is not null && err.Error() == syscall_package.EAFNOSUPPORT.Error(),
                "a zero-length read that the kernel answered without a source address must not fail with EAFNOSUPPORT: " +
                "Go's recvfromInet4 copies the (zero) source and returns the kernel's own result");
        }
        finally
        {
            syscall_package.Close(fd);
        }
    }

    [TestMethod]
    public unsafe void AZeroedSourceImageDecodesAsGoReinterpretsIt()
    {
        byte* buffer = stackalloc byte[syscall_package.GoNativeSockaddrLen];

        for (int i = 0; i < syscall_package.GoNativeSockaddrLen; i++)
            buffer[i] = 0;

        ж<syscall_package.SockaddrInet4> inet4 = @new<syscall_package.SockaddrInet4>();
        inet4.Value.Port = 4321;
        error err4 = syscall_package.GoReadNativeSockaddrInet4(buffer, (syscall_package._Socklen)0u, inet4);

        Assert.IsNull(err4, "a zeroed, zero-length image must decode without error, as Go's recvfromInet4 reads it");
        Assert.AreEqual((nint)0, inet4.Value.Port, "Port is copied from the image, so it reads 0");
        Assert.IsTrue(Enumerable.Range(0, 4).All(i => inet4.Value.Addr[i] == 0), "Addr is copied from the image, so it reads zero");

        ж<syscall_package.SockaddrInet6> inet6 = @new<syscall_package.SockaddrInet6>();
        inet6.Value.Port = 4321;
        inet6.Value.ZoneId = 7;
        error err6 = syscall_package.GoReadNativeSockaddrInet6(buffer, (syscall_package._Socklen)0u, inet6);

        Assert.IsNull(err6, "a zeroed, zero-length image must decode without error, as Go's recvfromInet6 reads it");
        Assert.AreEqual((nint)0, inet6.Value.Port, "Port is copied from the image, so it reads 0");
        Assert.AreEqual(0u, (uint)inet6.Value.ZoneId, "ZoneId is copied from the image, so it reads 0");
        Assert.IsTrue(Enumerable.Range(0, 16).All(i => inet6.Value.Addr[i] == 0), "Addr is copied from the image, so it reads zero");
    }

    [TestMethod]
    public void AZeroLengthReadOfQueuedDatagramsIsMeasured()
    {
        nint fd = OpenBoundUdpSocket(out syscall_package.Sockaddr self);

        try
        {
            var seen = new SortedDictionary<string, int>();
            slice<byte> payload = slice<byte>("zero byte buffer"u8);

            for (int i = 0; i < 200; i++)
            {
                Assert.IsNull(syscall_package.Sendto(fd, payload, 0, self), "sendto self");

                // syscall.Recvfrom, Go's generic form: it skips the source decode when the kernel reports AF_UNSPEC.
                (nint n, syscall_package.Sockaddr from, error err) = syscall_package.Recvfrom(fd, default(slice<byte>), 0);

                string source = from is null ? "nil" : (from as IжAdapter)?.Box?.GetType().GenericTypeArguments.FirstOrDefault()?.Name ?? from.GetType().Name;
                string key = $"err={(err is null ? "nil" : err.Error())} n={n} from={source}";
                seen[key] = seen.GetValueOrDefault(key) + 1;
            }

            foreach ((string key, int count) in seen)
                Console.WriteLine($"READING queued zero-length syscall.Recvfrom x{count}: {key}");
        }
        finally
        {
            syscall_package.Close(fd);
        }
    }

    // A UDP socket bound to the loopback address on an ephemeral port; `self` is its own address.
    private static nint OpenBoundUdpSocket(out syscall_package.Sockaddr self)
    {
        (nint fd, error err) = syscall_package.Socket(syscall_package.AF_INET, syscall_package.SOCK_DGRAM, 0);
        Assert.IsNull(err, "socket");

        ж<syscall_package.SockaddrInet4> bind = @new<syscall_package.SockaddrInet4>();

        for (int i = 0; i < 4; i++)
            bind.Value.Addr[i] = Loopback[i];

        Assert.IsNull(syscall_package.Bind(fd, new syscall_package.SockaddrInet4жSockaddr(bind)), "bind loopback");

        (self, err) = syscall_package.Getsockname(fd);
        Assert.IsNull(err, "getsockname");
        return fd;
    }
}
