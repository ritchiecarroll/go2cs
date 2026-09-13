// net_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of net.go's //go:linkname datagram helpers for the LINUX flavor --
// S1 of docs/phase4/DESIGN-linux-udp.md (RATIFIED 2026-08-23, all six OQs as recommended; S1 =
// Recvfrom/Sendto x Inet4/Inet6, S2 = the msghdr pair, evidence-gated and still PROPOSED).
//
// WHY THIS FILE EXISTS. `net.cs` declares eight helpers, each `//go:linkname X syscall.<lowercase>`
// -- in Go they ARE syscall's unexported helpers, re-exported here by linkname. go2cs emits each as
// a bodyless partial, so the PartialStubGenerator fills them with throwing stubs, and they are the
// ENTIRE datagram surface of the converted corpus: internal/poll's ReadFromInet4 (fd_unix.cs:278),
// WriteToInet4 (:578), ReadMsgInet4 (:398) and WriteMsgInet4 (:734) call them and nothing else does.
// Measured consequence, from the readiness poller's S2: a loopback UDP round trip dies on the first
// ReadFrom with `RecvfromInet4: external (assembly or cgo) function is not implemented`, while
// ListenPacket binds fine -- and because Go's pure-Go resolver speaks DNS over UDP, NO converted
// program on Linux could resolve a name. That is this file's bill: DNS and everything downstream,
// plus crypto/tls's TestVerifyHostname.
//
// WHY NOT syscall's OWN copies. syscall/linux/syscall_unix.cs carries CONVERTED bodies for the same
// eight (recvfromInet4 at :339, sendtoInet4 at :472, …) and NOTHING CALLS THEM -- a census of the
// linux flavor finds zero call sites, because internal/poll reaches this linkname instead. They are
// dead code, and they are left exactly as they are: a reconvert regenerates them, and touching them
// is corpus churn for no behavior. They are still worth reading, because in eleven lines they
// confess both defects this file exists to avoid:
//
//     ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);   // (1) a MANAGED box …
//     (n, err) = recvfrom(fd, p, flags, Ꮡrsa, Ꮡsocklen);            //     … handed to the kernel BY ADDRESS
//     var port = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(pp.of(RawSockaddrInet4.ᏑPort)));
//     from.Port = ((nint)port.Value[0] << 8) + (nint)port.Value[1];  // (2) the (*[2]byte) port alias
//
// (1) is the struct-passing class the board keeps open; (2) is the same `(*[2]byte)` alias L10
// retired on Windows and sockaddr_linux_impl.cs retired on Linux for the ENCODE direction -- it
// converts to a length-zero array<byte> and reads garbage. So this file is, in one sentence, THE
// DECODE HALF OF THE SOCKADDR MIRROR, and it owns no layout knowledge of its own: every address
// crosses through that mirror's `Go…NativeSockaddrInet4/6` seam (its ⟨OQ-2⟩ ruling), so there stays
// exactly ONE definition of what a Go Sockaddr looks like to the kernel.
//
// THE RULE, unchanged from the mirror and the struct stat mirror: every byte the kernel reads or
// writes is a stack image this file owns, handed over as a uintptr; managed storage is copied in and
// out by hand. The one exception is deliberate and is Go's own -- the PAYLOAD `p` travels by pinned
// slice-element address (`Ꮡ(p, 0)`), which is what every generated wrapper in zsyscall does and what
// golib documents as the one managed storage the runtime can be asked to hold still (ж.cs). That
// avoids copying every datagram twice.
//
// WHAT THESE BODIES MUST NOT DO. The callers hold fdmu (one reader, one writer), have already run
// pollDesc.prepare, retry EINTR themselves, and treat EAGAIN as "park on the poller and retry"
// (fd_unix.cs). So EAGAIN and EINTR are RETURNED here, never handled: a body that retried internally
// would defeat the poller's deadline handling, and would read three arcs later as "deadlines do not
// work on UDP".
//
// THE FILE NAME IS LOAD-BEARING -- do not "correct" it to net_impl.cs. An `X_impl.cs` companion is
// routed by the L3 multi-platform merge to every platform folder its PRINCIPAL `X.cs` was emitted
// into (platformHandOwn.go), and `net.cs` is `//go:build unix`, so it exists in linux/ AND darwin/.
// Named net_impl.cs, this file is therefore COPIED INTO darwin/ by the merge -- measured, 2026-08-23
// -- which would ship darwin a body hardcoding LINUX syscall numbers (45/44; darwin's differ), i.e.
// exactly the unmeasured copy this design's §8 refuses. There is no `net_linux.cs`, so under this
// name the file is PRINCIPAL-LESS, and the merge's own contract for that case is to leave it where
// it is: "with no emitted principal there is no evidence of a platform set, and no evidence, no
// rule" (platformHandOwn_test.go, TestMergeLeavesPrincipalLessCompanionsWhereTheyAre). Verified by
// re-running the three-target emission after the rename.
//
// SCOPE. S1 AND S2 -- all eight: RecvfromInet4/6, SendtoInet4/6, RecvmsgInet4/6, SendmsgNInet4/6.
//
// S2's four were held PROPOSED here on the evidence gate this file wrote for itself: "no roster row
// consumes them today". The consumer arrived and was measured rather than argued -- net's own suite
// reports TestUDPIPVersionReadMsg, TestUDPConnSpecificMethods and TestAllocs as `Go="pass"
// C#="infrastructure-error"`, each dying on PartialStubGenerator's body for an unimplemented
// partial. Ratified on that evidence, with the qualification stated rather than glossed: `net` is
// not a BANKED linux row, so the consumer is a row in progress, and the ratification's spirit was a
// consumer rather than a banked one.
//
// The stage turned out smaller than its own sizing predicted, in two ways worth recording because
// they are the reason this is a paragraph after all. First, the msghdr + iovec machinery it named
// (56/16 bytes on amd64) already existed one assembly over, written for syscall's own
// recvmsgRaw/SendmsgN; it is FACTORED there into GoRecvmsgNative/GoSendmsgNative rather than
// copied, so there is one implementation and ScmRightsSeam re-proves it through both callers.
// Second, there is no registration and no converter change: the helpers are already `partial`
// declarations carrying their //go:linkname, and PartialStubGenerator skips any partial whose
// PartialImplementationPart is non-null, so writing a body displaces its stub by construction.
//
// Linux only; darwin has the same stubs and no run layer to measure them against.

using System;

// Hand-owned (no net_impl.go exists, so a reconvert never regenerates this file); marked per the
// hand-own rules so a -stdlib run cannot emit a Go version over it.
[module: go.GoManualConversion]
// This file builds native sockaddr images on the stack and hands their addresses to the kernel, so
// the package's emitted .csproj must allow unsafe -- the marker is how a hand-own declares that
// (projectFileWriter.go's allowUnsafeBlocks union). DESIGN-linux-udp.md ⟨OQ-3⟩.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using syscall = syscall_package;

partial class unix_package
{
    // linux/amd64 syscall numbers; the same values zsysnum_linux_amd64.cs carries, kept local so
    // this file's kernel contract reads in one place.
    private const nuint sysRecvfrom = 45;
    private const nuint sysSendto = 44;

    // ---- RecvfromInet4 / RecvfromInet6 -----------------------------------------------------------

    // Go: recvfrom(fd, p, flags, &rsa, &socklen) then decode rsa into `from` (syscall_unix.go:331).
    // Here the kernel writes into a stack image and the mirror decodes it; `from` is filled by
    // ASSIGNMENT, exactly as Go's helper does, so no managed address is ever exposed.
    public static partial (nint, error) RecvfromInet4(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet4> from) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            uint32 addrlen = syscall.GoNativeSockaddrLen;
            byte zero = 0;

            // Go passes a valid pointer even for an empty buffer; `Ꮡ(p, 0)` is the pinned slice-element
            // route (ж<T> converts straight to uintptr, as at_fstatat.cs does with Ꮡstat), and an empty
            // slice has no element to take -- hence the stack byte. The pin is the BOX's, and lasts
            // only while the box is reachable, so the box is held across the call below.
            ж<byte> ᴋp = default!;
            uintptr payload;

            if (len(p) > 0) {
                ᴋp = Ꮡ(p, 0);
                payload = (uintptr)ᴋp;
            } else {
                payload = (uintptr)(void*)(&zero);
            }

            var (r1, _, errno) = syscall.Syscall6((uintptr)sysRecvfrom, (uintptr)fd, payload, (uintptr)len(p),
                                                  (uintptr)flags, (uintptr)(void*)buffer, (uintptr)(void*)(&addrlen));
            System.GC.KeepAlive(ᴋp);

            if (errno != 0) {
                return (0, errno);
            }
            {
                var err = syscall.GoReadNativeSockaddrInet4(buffer, (syscall._Socklen)addrlen, from);

                if (err != default!) {
                    return ((nint)r1, err);
                }
            }
            return ((nint)r1, default!);
        }
    }

    public static partial (nint n, error err) RecvfromInet6(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet6> from) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            uint32 addrlen = syscall.GoNativeSockaddrLen;
            byte zero = 0;
            ж<byte> ᴋp = default!;
            uintptr payload;

            if (len(p) > 0) {
                ᴋp = Ꮡ(p, 0);
                payload = (uintptr)ᴋp;
            } else {
                payload = (uintptr)(void*)(&zero);
            }

            var (r1, _, errno) = syscall.Syscall6((uintptr)sysRecvfrom, (uintptr)fd, payload, (uintptr)len(p),
                                                  (uintptr)flags, (uintptr)(void*)buffer, (uintptr)(void*)(&addrlen));
            System.GC.KeepAlive(ᴋp);

            if (errno != 0) {
                return (0, errno);
            }
            {
                var err = syscall.GoReadNativeSockaddrInet6(buffer, (syscall._Socklen)addrlen, from);

                if (err != default!) {
                    return ((nint)r1, err);
                }
            }
            return ((nint)r1, default!);
        }
    }

    // ---- SendtoInet4 / SendtoInet6 ---------------------------------------------------------------

    // Go: to.sockaddr() then sendto(fd, p, flags, ptr, n) (syscall_unix.go:428). The encode half is
    // ALREADY hand-owned -- `sockaddr()` is the mirror's -- so the only thing the converted version
    // gets wrong is the last step: it hands `sendto` the pointer the encoder returns, which points
    // at a MANAGED raw box. Writing the image into a stack buffer and passing THAT address is what
    // Bind and Connect already do, three lines away in the mirror.
    public static partial error /*err*/ SendtoInet4(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet4> to) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            var (addrlen, err) = syscall.GoWriteNativeSockaddrInet4(to, buffer);

            if (err != default!) {
                return err;
            }
            return sendtoNative(fd, p, flags, buffer, addrlen);
        }
    }

    public static partial error /*err*/ SendtoInet6(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet6> to) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            var (addrlen, err) = syscall.GoWriteNativeSockaddrInet6(to, buffer);

            if (err != default!) {
                return err;
            }
            return sendtoNative(fd, p, flags, buffer, addrlen);
        }
    }

    // The one send path both families share, so the payload rule lives in exactly one place.

    // ---- S2: the four msghdr helpers ------------------------------------------------------------
    //
    // These were PROPOSED-and-stubbed when this file was written (see the SCOPE note in the header),
    // on the rule that a hand-own lands when a consumer REACHES it. The consumer arrived and was
    // measured: net's own suite reports TestUDPIPVersionReadMsg, TestUDPConnSpecificMethods and
    // TestAllocs as `Go="pass" C#="infrastructure-error"`, each dying on
    //
    //     NotImplementedException: RecvmsgInet4: external (assembly or cgo) function is not implemented
    //
    // which is PartialStubGenerator's body for an unimplemented partial. Writing these four
    // DISPLACES those stubs by construction -- its predicate skips any partial whose
    // PartialImplementationPart is non-null -- so there is no registration and no converter change.
    //
    // Each body is Go's own (syscall_unix.go recvmsgInet4/sendmsgNInet4), composed from the two
    // seams this file and syscall already share: the sockaddr encode/decode is S1's
    // GoWrite/GoReadNativeSockaddrInet4/6, and the msghdr/iovec/control machinery is
    // GoRecvmsgNative/GoSendmsgNative. No native type crosses the assembly boundary.

    public static partial (nint n, nint oobn, nint recvflags, error err) RecvmsgInet4(nint fd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet4> from) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            uint32 nameLen = syscall.GoNativeSockaddrLen;

            var (n, oobn, recvflags, err) = syscall.GoRecvmsgNative(fd, p, oob, flags, buffer, ref nameLen);

            if (err != default!) {
                return (0, 0, 0, err);
            }

            // Go reinterprets the raw image as a RawSockaddrInet4 and copies Port/Addr; here the
            // shared decoder does it, filling `from` by ASSIGNMENT so no managed address is exposed.
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

    // Go: `ptr, salen, err := to.sockaddr(); return sendmsgN(fd, p, oob, ptr, salen, flags)`. The
    // encode is S1's writer into a stack image, so the kernel never sees a managed address -- which
    // is the whole defect this family carries.
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

    private static unsafe error sendtoNative(nint fd, slice<byte> p, nint flags, byte* addr, syscall._Socklen addrlen) {
        byte zero = 0;
        ж<byte> ᴋp = default!;
        uintptr payload;

        if (len(p) > 0) {
            ᴋp = Ꮡ(p, 0);
            payload = (uintptr)ᴋp;
        } else {
            payload = (uintptr)(void*)(&zero);
        }

        var (_, _, errno) = syscall.Syscall6((uintptr)sysSendto, (uintptr)fd, payload, (uintptr)len(p),
                                             (uintptr)flags, (uintptr)(void*)addr, (uintptr)(uint32)addrlen);
        System.GC.KeepAlive(ᴋp);

        if (errno != 0) {
            return errno;
        }
        return default!;
    }
}
