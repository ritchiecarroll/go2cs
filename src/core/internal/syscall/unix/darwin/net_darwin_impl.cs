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
// SCOPE. All eight datagram helpers, plus -- since darwin increment 10 (a), 2026-09-05 -- the five
// //go:linkname PULLS net_darwin.go makes of syscall's keystone family (syscall_syscall,
// syscall_syscallPtr, syscall_syscall6, syscall_syscall6X, syscall_syscall9), at the bottom of the
// file. The eight: RecvfromInet4/6, SendtoInet4/6, RecvmsgInet4/6, SendmsgNInet4/6 -- S1 and S2
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

    // ---- syscall's keystone family, pulled by //go:linkname (net_darwin.go) --------------------

    // Go: `//go:linkname syscall_syscall6 syscall.syscall6` and four siblings -- two-argument PULLS
    // of the syscall package's runtime-provided family (syscall/darwin/syscall_darwin_impl.cs, the
    // keystone hand-own). Their targets are unexported in that assembly and carry no one-arg handle,
    // so no forwarding property can reach them across the assembly boundary: the converter emits
    // each as a bodyless partial and PartialStubGenerator's throw filled it -- the wall the
    // SigIgnoreDisposition probe found on both mac legs (2026-09-05): exec.Command by bare name ->
    // os/exec.LookPath -> unix.Eaccess -> faccessat -> syscall_syscall6 -> NotImplementedException,
    // with os/user, the pty family and net's resolver path behind the same five names. Same shape
    // as the eight above: a body here displaces the stub by construction, no registration and no
    // converter change. The bodies stand on the syscall assembly's PUBLIC family -- Syscall,
    // Syscall6 and Syscall9 carry the same Int32MinusOne failure test as their lowercase twins (the
    // keystone hand-own: raw and cooked are the same call there) -- and on the two Go-prefixed doors
    // added beside them for the rules that have no exported twin: syscall6X (all 64 bits == -1) and
    // syscallPtr (NULL is the error). Darwin increment 10 (a).
    internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
        return syscall.Syscall(fn, a1, a2, a3);
    }

    internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscallPtr(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
        return syscall.GoSyscallPtr(fn, a1, a2, a3);
    }

    internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscall6(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
        return syscall.Syscall6(fn, a1, a2, a3, a4, a5, a6);
    }

    internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscall6X(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
        return syscall.GoSyscall6X(fn, a1, a2, a3, a4, a5, a6);
    }

    internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscall9(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9) {
        return syscall.Syscall9(fn, a1, a2, a3, a4, a5, a6, a7, a8, a9);
    }

    // gostring is the SECOND member of that family and the one nothing had ever CALLED, which is why
    // it outlived increment 10 (a) as a live throw. This package's darwin flavour declares it
    // BODYLESS (net_darwin.cs) as the destination of runtime's
    // `//go:linkname internal_syscall_gostring internal/syscall/unix.gostring`, and that push cannot
    // be emitted: this package already references `runtime` in its own csproj, so the edge the push
    // would add closes a two-project cycle. That is W1's boundary, where the remedy is a body at the
    // DESTINATION rather than an edge the graph cannot carry
    // (docs/phase4/DESIGN-linkname-push-cycles.md); a body displaces the stub by construction, with
    // no registration and no converter change.
    //
    // Nothing reached it until darwin increment 11. `unix.GoString` has exactly two darwin consumers
    // -- GaiStrerror's error path, and os/user's `_C_GoString` -- and the second sat behind the
    // getpwuid_r ERANGE that increment answered, so the first caller ever to arrive was buildUser,
    // one frame past the fix. The acceptance run read it as `C# 2 vs Go 0` with
    // "gostring: external (assembly or cgo) function is not implemented" on stderr.
    //
    // The body is runtime's own two steps (string.cs: findnull, then copy), reached through the
    // pointer's ADDRESS rather than through runtime's helper, which is `internal` and invisible to
    // this assembly. `(uintptr)Ꮡp` is the operator both pointer kinds answer correctly here: `byte`
    // is reference-free, so a managed element box has pinnable storage and is pinned for the box's
    // life, and a native box already carries an address -- there is no token case to consider at
    // this element type. The KeepAlive holds that pin across the scan and the copy, which is the
    // whole window the address is read in.
    internal static partial @string gostring(ж<byte> Ꮡp) {
        if (Ꮡp == nil) {
            return ""u8;
        }

        unsafe {
            byte* q = (byte*)(void*)(uintptr)Ꮡp;

            if (q == null) {
                return ""u8;
            }

            nint n = 0;

            while (q[n] != 0) {
                n++;
            }

            var b = new slice<byte>(n);

            for (nint i = 0; i < n; i++) {
                b[i] = q[i];
            }

            System.GC.KeepAlive(Ꮡp);

            return ((@string)b);
        }
    }
}
