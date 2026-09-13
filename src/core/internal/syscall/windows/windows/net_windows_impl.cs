// net_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of net_windows.go's two //go:linkname UDP SEND helpers --
// WSASendtoInet4 and WSASendtoInet6 -- the Windows half of the datagram seam whose Linux half is
// internal/syscall/unix/linux/net_linux_impl.cs (docs/phase4/DESIGN-linux-udp.md, S1).
//
// WHY NOW, AND BY WHOSE RULE. syscall/windows/syscall_windows_impl.cs (the L10 sockaddr mirror)
// named these three wrappers in its own header and deliberately left them: "WSASendto /
// wsaSendtoInet4 / wsaSendtoInet6 -- the UDP send path -- still pass the address returned by
// `sockaddr()`, which... is not a native image. They are out of this lane's scope (the board's
// ruling is to fix a censused wrapper when a suite REACHES it, never speculatively)... They are
// named here rather than left to be rediscovered: writeNativeSockaddr is what they would need."
// A suite has now reached them: the UdpLoopbackRoundTrip guard, written for the Linux seam, dies on
// WINDOWS with `WSASendtoInet4: external (assembly or cgo) function is not implemented` -- the
// PartialStubGenerator's stub for these two declarations. That is the trigger the ruling describes,
// and this file is the answer it pre-specified.
//
// TWO DEFECTS ARE FIXED HERE, NOT ONE.
//
//  1. The STUB. These are bodyless partials, so every UDP send on Windows threw before this file.
//
//  2. The ADDRESS, which the dead generated body in syscall/windows/syscall_windows.cs shows:
//         (var rsa, var len, err) = Ꮡto.sockaddr();
//         Syscall9(procWSASendTo.Addr(), 9, s, bufs, bufcnt, sent, flags, (uintptr)rsa, (uintptr)len, …)
//     `sockaddr()` hands back a pointer INTO A MANAGED BOX, so the kernel is given managed storage --
//     the struct-passing class, whose worst measured form on this project is an AccessViolation
//     reachable from a public API (syscall/linux's Recvfrom, net.Interfaces()). Writing the image
//     into a stack buffer first is the mirror's remedy, applied for the fifth time.
//
// THE ENCODE IS THE MIRROR'S, NOT A COPY. `syscall`'s `GoWriteNativeSockaddrInet4/6` is the seam the
// mirror exposes for exactly this (⟨OQ-2⟩, ruled): one definition of what a Go Sockaddr looks like
// to the kernel, so this file carries no layout knowledge and cannot drift from it. The Linux half
// consumes the identical seam from syscall/linux.
//
// WHY THE BODIES LIVE HERE rather than in `syscall` beside the mirror: the DECLARATIONS are here,
// and every linkname-into-another-package in this corpus is answered where it is declared (the ten
// runtime_poll* contracts, os/tempfile's runtime_rand, math/rand's two, sync's, net/dnsclient_impl's,
// and the Linux datagram half). `syscall`'s `wsaSendtoInet4`/`wsaSendtoInet6` remain as converted
// dead code -- nothing calls them, exactly as on Linux -- and are left alone.
//
// The proc is this package's own: `modws2_32` is the LazyDLL its generated zsyscall_windows.cs
// already declares, and the LazyProc below is looked up from it the same way every generated wrapper
// in that file does. Error handling is the dead body's, verbatim: r1 == socket_error means failure,
// and a zero errno there maps to EINVAL rather than to success.

using System;
using golib = go.golib;
using @unsafe = go.unsafe_package;

// Hand-owned (no net_windows_impl.go exists, so a reconvert never regenerates this file); the
// declarations it fills are registered in the converter's manualConversionFuncs, which is what turns
// the generated bodies into placeholders.
[module: go.GoManualConversion]
// This package's generated csproj already emits AllowUnsafeBlocks=true -- its netpoll-seam hand-own
// (zsyscall_windows_wsa_impl.cs) declares the same need -- so this marker is a consistency habit
// here rather than the load-bearing flip it is elsewhere. It is still declared, because the file
// genuinely needs /unsafe and a marker that depends on a SIBLING file's declaration is a trap.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using syscall = go.syscall_package;

partial class windows_package
{
    // ws2_32's WSASendTo, from this package's own LazyDLL. The generated zsyscall_windows.cs declares
    // procWSAGetOverlappedResult and procWSASocketW from `modws2_32` the same way; it has no
    // WSASendTo entry because nothing generated here calls it.
    //
    // ⚠ LOOKED UP ON FIRST USE, NOT IN A FIELD INITIALIZER, and that is load-bearing. C# runs static
    // field initializers in textual order WITHIN a type, but this type is a partial class spread
    // across generated files and this one, and the order BETWEEN files is not guaranteed. Written as
    // `= modws2_32.NewProc(...)` this initializer ran while the generated `modws2_32` field was still
    // null, so the LazyProc held a nil DLL and the first send died in LazyDLL.Load():
    //
    //   panic: runtime error: invalid memory address or nil pointer dereference
    //     at syscall.(*LazyDLL).Load -> (*LazyProc).Find -> mustFind -> Addr
    //     at internal/syscall/windows.wsaSendtoNative
    //
    // Measured, not theorised. Deferring to first use removes the ordering dependency entirely; the
    // race is benign in the same way golib's pinnedArrayData is (two lookups at worst, same answer).
    private static ж<syscall.LazyProc>? s_procWSASendTo;

    private static ж<syscall.LazyProc> procWSASendTo => s_procWSASendTo ??= modws2_32.NewProc("WSASendTo"u8);

    // Winsock's SOCKET_ERROR. `syscall`'s own `socket_error` is unexported and therefore not visible
    // across the assembly boundary, so the value is restated here rather than reached for -- it is
    // ^uint32(0), the same constant the dead generated body compares against.
    private static readonly uintptr socketError = unchecked((uintptr)(nuint)uint.MaxValue);

    // The submitting package's own mode value. golib treats it opaquely (it keys nothing and is
    // handed straight to the factory), and `syscall` spells the same 'w' for its write submits, so
    // the two agree without either exporting a constant.
    private const nint wsaModeWrite = 'w';

    // Go: wsaSendtoInet4 encodes `to` and calls WSASendTo (syscall_windows.go). Here the encode goes
    // through the mirror's seam into OPERATION-OWNED memory, and the SUBMIT goes through golib's
    // seam so this package and `syscall` share ONE operation record (netpoll design §4.7/§4.8).
    public static unsafe error /*err*/ WSASendtoInet4(syscallꓸHandle s, ж<syscall.WSABuf> bufs, uint32 bufcnt, ж<uint32> sent, uint32 flags, ж<syscall.SockaddrInet4> to, ж<syscall.Overlapped> overlapped, ж<byte> croutine) {
        nuint native = prepareSendto(s, bufs, bufcnt, overlapped, out NativeWSABuf* staged, out byte* addr);
        var (addrlen, err) = syscall.GoWriteNativeSockaddrInet4(to, addr);

        if (err != default!) {
            return err;
        }
        return submitSendto(s, staged, bufcnt, sent, flags, addr, addrlen, native, croutine);
    }

    public static unsafe error /*err*/ WSASendtoInet6(syscallꓸHandle s, ж<syscall.WSABuf> bufs, uint32 bufcnt, ж<uint32> sent, uint32 flags, ж<syscall.SockaddrInet6> to, ж<syscall.Overlapped> overlapped, ж<byte> croutine) {
        nuint native = prepareSendto(s, bufs, bufcnt, overlapped, out NativeWSABuf* staged, out byte* addr);
        var (addrlen, err) = syscall.GoWriteNativeSockaddrInet6(to, addr);

        if (err != default!) {
            return err;
        }
        return submitSendto(s, staged, bufcnt, sent, flags, addr, addrlen, native, croutine);
    }

    // WSABUF, native layout. DUPLICATED from syscall's mirror deliberately, and ⟨OQ-B⟩ ruled it so on
    // a principle worth restating where the copy lives: duplicated LOGIC drifts, but duplicated ABI
    // MIRRORS are independent re-derivations of the same external fact -- the OS pins this layout, so
    // neither copy can drift while remaining correct. Publishing a type from `syscall` to avoid eight
    // bytes of shape would trade a non-problem for the public-seam problem §4.7 exists to refuse.
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private unsafe struct NativeWSABuf
    {
        internal uint32 Len;
        internal byte* Buf;
    }

    // Re-arms the operation and hands back its native control block plus TWO regions carved out of
    // the operation's own staging: the WSABUF array, and the address image after it.
    //
    // ⚠ THE ADDRESS IMAGE IS OPERATION-OWNED, NOT `stackalloc`, AND THAT IS ⟨OQ-G⟩'s RULING.
    // The first cut of this file wrote the sockaddr into a `stackalloc` buffer, which is correct only
    // if Winsock captures `lpTo` during the call. It does not say that it does. The contract is
    // explicit about lifetime in two places and silent in the third: `lpBuffers` is captured before
    // return (*"the Winsock service provider's responsibility to capture the WSABUF structures before
    // returning from this call… enables applications to build stack-based WSABUF arrays"*),
    // `lpOverlapped` *"must be valid for the duration of the overlapped operation"*, and `lpTo`
    // carries no statement at all. Undefined is worse than either answer: an implementation may
    // capture it today and not tomorrow, and the failure is a silent wrong destination or a read of
    // freed stack. The coordinator ruled this FIX-BY-DEFAULT rather than measure-then-fix, on the
    // grounds that a use-after-return handed to the kernel is the struct-passing family's lifetime
    // sibling and that class does not get empirical exoneration -- "has not misbehaved" proves
    // nothing about a race.
    //
    // ONE staged block carved into two regions rather than two staged blocks: golib's seam owns the
    // ALLOCATION and never the layout ("what the bytes MEAN is the caller's business"), so carving is
    // within the ratified contract and needs no further primitive.
    // Pointers cannot be tuple elements in C#, so the two carved regions come back through `out`.
    private static unsafe nuint prepareSendto(syscallꓸHandle s, ж<syscall.WSABuf> bufs, uint32 bufcnt, ж<syscall.Overlapped> overlapped, out NativeWSABuf* staged, out byte* addr) {
        // The record is keyed by the WAITER -- the same `Ꮡoverlapped` execIO will later harvest by --
        // so a submit issued here and a harvest issued from `syscall` name one operation. Creating it
        // is `syscall`'s job (only it can bind the socket to the completion port); this call reaches
        // that factory through golib without either package learning the other's internals.
        nuint native = golib.GoAsyncIO.RearmOperation((nuint)(uintptr)s, overlapped, wsaModeWrite);

        // A zero-buffer submit is legal (a zero-length datagram), and still needs its address image,
        // so the WSABUF region is sized for at least one entry rather than collapsing to nothing.
        uint32 slots = bufcnt == 0 ? 1 : bufcnt;
        int bufBytes = checked((int)slots * sizeof(NativeWSABuf));
        byte* block = (byte*)golib.GoAsyncIO.StageOperationBuffer(overlapped, checked(bufBytes + syscall.GoNativeSockaddrLen));
        staged = (NativeWSABuf*)block;

        stageWSABufs(bufs, bufcnt, staged);

        addr = block + bufBytes;
        return native;
    }

    // Transcribes a Go WSABuf array into its native form. Shared by the two submits in this package
    // (WSASendTo above, WSASendMsg in syscall_windows_impl.cs) so the one thing they genuinely have
    // in common is written once; everything else about their staging differs.
    private static unsafe void stageWSABufs(ж<syscall.WSABuf> bufs, uint32 bufcnt, NativeWSABuf* staged) {
        for (uint32 i = 0; i < bufcnt; i++) {
            // Index 0 is the common case and the only one a struct-FIELD reference can answer; a
            // multi-buffer submit names a slice element, which unsafe.Add steps through.
            ж<syscall.WSABuf> Ꮡbuf = i == 0 ? bufs : @unsafe.Add(bufs, (uintptr)i);
            ref syscall.WSABuf buf = ref Ꮡbuf.Value;

            staged[i].Len = buf.Len;
            staged[i].Buf = buf.Buf == nil ? null : (byte*)(void*)buf.Buf;
        }
    }

    // The one submit path both families share, so the kernel contract lives in exactly one place.
    private static unsafe error submitSendto(syscallꓸHandle s, NativeWSABuf* staged, uint32 bufcnt, ж<uint32> sent, uint32 flags, byte* addr, int32 addrlen, nuint native, ж<byte> croutine) {
        uint32 sentBytes = 0;
        var (r1, _, e1) = syscall.Syscall9(procWSASendTo.Addr(), 9, (uintptr)s, (uintptr)(void*)staged, (uintptr)bufcnt,
                                           (uintptr)(void*)(&sentBytes), (uintptr)flags, (uintptr)(void*)addr, (uintptr)addrlen,
                                           (uintptr)native, (uintptr)croutine);

        if (sent != nil) {
            sent.Value = sentBytes;
        }

        if (r1 != socketError) {
            return default!;
        }
        // The generated body's mapping, kept verbatim: a socket_error with no errno is EINVAL, not
        // success -- WSASendTo reports failure through the return value and the error through
        // WSAGetLastError, and a lost errno must not read as "sent".
        if (e1 != 0) {
            // errnoErr is THIS package's own (the generated wrappers beside this file use it
            // unqualified); syscall's is unexported and not reachable from here.
            return errnoErr(e1);
        }
        return syscall.EINVAL;
    }
}
