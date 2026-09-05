// net_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Hand-written implementations of net.go's //go:linkname datagram helpers for the DARWIN flavor --
// the darwin companion of internal/syscall/unix/linux/net_linux_impl.cs (whose header carries the
// write-up: why the eight helpers are the ENTIRE datagram surface of the converted corpus, why
// syscall's own copies are dead code that stays auto, and the payload/EAGAIN rules), cut with the
// darwin sockaddr twin (syscall/darwin/sockaddr_darwin_impl.cs, increment 8, 2026-09-05).
//
// WHAT DIFFERS FROM LINUX, and it is the reason this is a separate file: darwin has no syscall
// NUMBERS. The linux companion calls the kernel through syscall.Syscall6(45/44, ...); on darwin every
// call goes through a libc trampoline the syscall package resolves by //go:cgo_import_dynamic, and
// those trampolines are `internal` to that assembly. So this file owns NO call of its own: the four
// it needs are the twin's `Go…Native` seams (GoRecvfromNative / GoSendtoNative / GoRecvmsgNative /
// GoSendmsgNative), beside the sockaddr pairs both companions share (GoWrite/GoReadNativeSockaddr
// Inet4/6). No native type and no trampoline crosses the assembly line, and there stays exactly ONE
// definition of what a Go Sockaddr, a msghdr and an iovec look like to the kernel.
//
// SCOPE. All eight: RecvfromInet4/6, SendtoInet4/6, RecvmsgInet4/6, SendmsgNInet4/6 -- S1 and S2
// together, on the evidence linux already measured for S2 (net's TestUDPIPVersionReadMsg,
// TestUDPConnSpecificMethods and TestAllocs dying on PartialStubGenerator's body) and on the five
// darwin behavioral rows whose paths are known (UdpLoopbackRoundTrip -> ReadFrom/WriteTo ->
// RecvfromInet4/SendtoInet4; UdpWriteMsgAddrPort -> WriteMsg -> SendmsgNInet4). There is no
// registration and no converter change: each helper is a `partial` declaration carrying its
// //go:linkname, and PartialStubGenerator skips any partial whose PartialImplementationPart is
// non-null, so writing a body displaces its stub by construction.
//
// THE FILE NAME IS LOAD-BEARING, the other way round from linux's: `net_darwin.cs` IS an emitted
// darwin-only principal (Go's net_darwin.go), so an `net_darwin_impl.cs` companion is routed by the
// L3 merge to darwin/ alone -- exactly where it belongs -- where the linux file had to stay
// principal-less to avoid being copied here.

using System;

// Hand-owned (no net_darwin_impl.go exists, so a reconvert never regenerates this file); marked per
// the hand-own rules so a -stdlib run cannot emit a Go version over it.
[module: go.GoManualConversion]
// Stack images of native sockaddrs are built here and handed to the twin's seams by address, so the
// package's emitted .csproj must allow unsafe -- the marker is how a hand-own declares that.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using syscall = syscall_package;

partial class unix_package
{
    // ---- RecvfromInet4 / RecvfromInet6 -----------------------------------------------------------

    // Go: recvfrom(fd, p, flags, &rsa, &socklen) then decode rsa into `from` (syscall_unix.go).
    // Here the kernel writes into a stack image through the twin's seam and the twin decodes it;
    // `from` is filled by ASSIGNMENT, exactly as Go's helper does, so no managed address is exposed.
    public static partial (nint, error) RecvfromInet4(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet4> from) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            uint32 addrlen = syscall.GoNativeSockaddrLen;

            var (n, err) = syscall.GoRecvfromNative(fd, p, flags, buffer, ref addrlen);

            if (err != default!) {
                return (0, err);
            }

            var decodeErr = syscall.GoReadNativeSockaddrInet4(buffer, (syscall._Socklen)addrlen, from);

            if (decodeErr != default!) {
                return (n, decodeErr);
            }

            return (n, default!);
        }
    }

    public static partial (nint n, error err) RecvfromInet6(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet6> from) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            uint32 addrlen = syscall.GoNativeSockaddrLen;

            var (n, err) = syscall.GoRecvfromNative(fd, p, flags, buffer, ref addrlen);

            if (err != default!) {
                return (0, err);
            }

            var decodeErr = syscall.GoReadNativeSockaddrInet6(buffer, (syscall._Socklen)addrlen, from);

            if (decodeErr != default!) {
                return (n, decodeErr);
            }

            return (n, default!);
        }
    }

    // ---- SendtoInet4 / SendtoInet6 ---------------------------------------------------------------

    // Go: to.sockaddr() then sendto(fd, p, flags, ptr, n). The encode half is the twin's
    // `sockaddr()`; the only thing the converted version gets wrong is the last step -- it hands
    // `sendto` the pointer the encoder returns, which names a MANAGED raw box. Writing the image
    // into a stack buffer and passing THAT address is what the twin's Bind and Connect already do.
    public static partial error /*err*/ SendtoInet4(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet4> to) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            var (addrlen, err) = syscall.GoWriteNativeSockaddrInet4(to, buffer);

            if (err != default!) {
                return err;
            }

            return syscall.GoSendtoNative(fd, p, flags, buffer, addrlen);
        }
    }

    public static partial error /*err*/ SendtoInet6(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet6> to) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            var (addrlen, err) = syscall.GoWriteNativeSockaddrInet6(to, buffer);

            if (err != default!) {
                return err;
            }

            return syscall.GoSendtoNative(fd, p, flags, buffer, addrlen);
        }
    }

    // ---- the four msghdr helpers ----------------------------------------------------------------

    // Each body is Go's own (syscall_unix.go recvmsgInet4/sendmsgNInet4), composed from the two
    // seams the twin exports: the sockaddr encode/decode and the msghdr/iovec/control machinery.
    public static partial (nint n, nint oobn, nint recvflags, error err) RecvmsgInet4(nint fd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet4> from) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            uint32 nameLen = syscall.GoNativeSockaddrLen;

            var (n, oobn, recvflags, err) = syscall.GoRecvmsgNative(fd, p, oob, flags, buffer, ref nameLen);

            if (err != default!) {
                return (0, 0, 0, err);
            }

            var decodeErr = syscall.GoReadNativeSockaddrInet4(buffer, (syscall._Socklen)nameLen, from);

            if (decodeErr != default!) {
                return (n, oobn, recvflags, decodeErr);
            }

            return (n, oobn, recvflags, default!);
        }
    }

    public static partial (nint n, nint oobn, nint recvflags, error err) RecvmsgInet6(nint fd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet6> from) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            uint32 nameLen = syscall.GoNativeSockaddrLen;

            var (n, oobn, recvflags, err) = syscall.GoRecvmsgNative(fd, p, oob, flags, buffer, ref nameLen);

            if (err != default!) {
                return (0, 0, 0, err);
            }

            var decodeErr = syscall.GoReadNativeSockaddrInet6(buffer, (syscall._Socklen)nameLen, from);

            if (decodeErr != default!) {
                return (n, oobn, recvflags, decodeErr);
            }

            return (n, oobn, recvflags, default!);
        }
    }

    public static partial (nint n, error err) SendmsgNInet4(nint fd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet4> to, nint flags) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            var (nameLen, err) = syscall.GoWriteNativeSockaddrInet4(to, buffer);

            if (err != default!) {
                return (0, err);
            }

            return syscall.GoSendmsgNative(fd, p, oob, buffer, (uint32)nameLen, flags);
        }
    }

    public static partial (nint n, error err) SendmsgNInet6(nint fd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet6> to, nint flags) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            var (nameLen, err) = syscall.GoWriteNativeSockaddrInet6(to, buffer);

            if (err != default!) {
                return (0, err);
            }

            return syscall.GoSendmsgNative(fd, p, oob, buffer, (uint32)nameLen, flags);
        }
    }
}
