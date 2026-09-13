// syscall_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of syscall_windows.go's WSASendMsg, WSARecvMsg and the
// loadWSASendRecvMsg extension lookup beneath both -- the whole of the Windows WriteMsg/ReadMsg seam.
// Its sibling net_windows_impl.cs owns the sendto half of the same datagram surface and supplies the
// staging primitives this file reuses.
//
// The three below were not landed together. The SUBMIT direction came first (with the encode fix in
// internal/poll that exposed it); the HARVEST twin followed once a suite reached it. Their remedies
// are NOT mirror images, and the reason is at the top of WSARecvMsg's own note: a submit can finish
// its work before it returns and a receive cannot.
//
// THREE DEFECTS STOOD BETWEEN `net` AND A WORKING WriteMsgUDP / WriteMsgUDPAddrPort, all three the
// same struct-passing family, each hidden behind the one before it. They are listed in the order the
// call takes them, which is the REVERSE of the order they were found -- every one was measured only
// after its predecessor was fixed, and that is the honest account of why the first two looked
// complete at the time.
//
//  1. THE ENCODE, upstream. internal/poll's sockaddrInet4ToRaw/sockaddrInet6ToRaw filled the caller's
//     RawSockaddrAny by reinterpreting one reference-bearing managed struct as another and writing
//     through the result -- depositing a uint16 over the low half of a live object reference. That is
//     heap corruption, and it killed the whole `net` test host at a death site that moved from run to
//     run. Hand-owned in internal/poll/windows/fd_windows_impl.cs; see that file's header.
//
//  2. THE EXTENSION LOOKUP, here. With the encode fixed the crash is gone and the call reaches
//     Winsock, which answers, both families and both entry points:
//
//         write udp4 127.0.0.1:59174->127.0.0.1:59173: wsasendmsg: An invalid argument was supplied.
//
//     That reads like the submission being rejected. It is not: the submission was never made,
//     because loadWSASendRecvMsg had already failed. See its own note below -- syscall.GUID is not
//     blittable, so ws2_32 is asked for an extension whose GUID is sixteen bytes of something else.
//
//  3. THE SUBMIT, here. Only with the lookup fixed does the submission actually happen, and the
//     generated wrapper hands it `uintptr(unsafe.Pointer(msg))` -- the address of the MANAGED WSAMsg.
//     Native WSAMSG is 56 bytes of pointers and lengths; the converted struct carries a
//     `syscall.Pointer` class reference, a `ж<WSABuf>` and an inline WSABuf whose own Buf is another
//     managed reference, so no field lands where Winsock reads it. The established remedy: a
//     blittable mirror built in operation-owned memory.
//
// ⚠ THE LESSON, because it cost two rounds of measurement here: a WSAEINVAL naming a function is not
// evidence that the function's own arguments were wrong. On this seam a lazily-resolved extension
// pointer sits between the caller and the call, and its failure is reported with the CALLER's name.
// The tell was that the submit's diagnostics never printed at all.
//
// WHY THE ADDRESS ARRIVES AS A TOKEN, and why that is the good outcome. internal/poll sets
// `msg.Name` with golib's ManagedPointerTokens.MintOpaque, because Go's `syscall.Pointer` is
// `*struct{}` and the pointee here is reference-bearing -- so there is no address to hand out that
// would still be valid when the kernel read it. The token round trip (Resolve, below) recovers the
// very box internal/poll encoded into, which is what lets this file flatten a record it did not
// build. The alternative -- projecting the box to a scalar -- is exactly the ACCESS_VIOLATION class
// MintOpaque's own documentation was written to close.
//
// THE FLATTEN IS `syscall`'s, NOT A COPY. GoWriteNativeRawSockaddr is the seam the sockaddr mirror
// exposes for this (syscall/windows/syscall_windows_impl.cs), and it is the SAME flatten the decode
// side performs for RawSockaddrAny.Sockaddr. So the 116-byte record's layout is spelled in exactly
// one place in the corpus and this file carries none of it -- the rule net_windows_impl.cs states
// for the encode and ⟨OQ-2⟩ ruled for the seam.
//
// LIFETIME: OPERATION-OWNED, NOT `stackalloc`, and ⟨OQ-G⟩'s ruling is the reason, unchanged and if
// anything stronger here. WSASendMsg is submitted OVERLAPPED, so the kernel may read lpMsg after
// this function returns; a stack image would be a use-after-return handed to the kernel. Everything
// native this submit needs -- the WSAMSG, the WSABUF array, the address image -- is carved from ONE
// staged block the operation record owns and frees, exactly as the sendto half does.
//
// STILL NOT COVERED, and named so it is not rediscovered: TransmitFile, and the Unix-domain control
// path. Neither is reachable from the Windows ReadMsg/WriteMsg family this file completes.

using System;
using System.Runtime.InteropServices;
using golib = go.golib;
using @unsafe = go.unsafe_package;

// Hand-owned (no syscall_windows_impl.go exists, so a reconvert never regenerates this file); the
// declaration it replaces is registered in the converter's manualConversionFuncs, which is what turns
// the generated body into a placeholder.
[module: go.GoManualConversion]
// The mirror below needs a blittable struct and raw pointers into staged memory. Declared here rather
// than inherited from a sibling hand-own in this package: a marker that depends on another file's
// declaration is a trap (net_windows_impl.cs makes the same point about the same package).
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using syscall = go.syscall_package;

partial class windows_package
{
    // WSAMSG, native layout. On x64 the padding C# inserts for natural alignment is the padding the
    // OS header has: name 0, namelen 8, lpBuffers 16, dwBufferCount 24, Control 32 (a WSABUF, itself
    // 4 + 4 pad + 8), dwFlags 48 -- 56 bytes. Sequential layout with these field types reproduces it
    // without a single explicit offset, which is why none is written.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeWSAMsg
    {
        internal byte* Name;
        internal int32 Namelen;
        internal NativeWSABuf* Buffers;
        internal uint32 BufferCount;
        internal NativeWSABuf Control;
        internal uint32 Flags;
    }

    // ws2_32's WSAIoctl, from this package's own LazyDLL, looked up ON FIRST USE rather than in a
    // field initializer -- net_windows_impl.cs's procWSASendTo carries the measured reason (this type
    // is a partial class spread across generated files and hand-owns, and the order BETWEEN files is
    // not guaranteed, so an initializer can run while the generated `modws2_32` field is still null).
    private static ж<syscall.LazyProc>? s_procWSAIoctl;

    private static ж<syscall.LazyProc> procWSAIoctl => s_procWSAIoctl ??= modws2_32.NewProc("WSAIoctl"u8);

    // THE EXTENSION LOOKUP -- the defect BENEATH WSASendMsg, and the one that actually fires first.
    //
    // Both WSASendMsg and WSARecvMsg are Winsock EXTENSIONS: they have no export to link, and their
    // addresses are fetched at run time by handing WSAIoctl a GUID. Go's loadWSASendRecvMsg does that
    // with `(*byte)(unsafe.Pointer(&WSAID_WSASENDMSG))` -- and syscall.GUID's converted form is NOT
    // blittable, because `Data4 [8]byte` is an `array<byte>` MANAGED REFERENCE where the native
    // record has eight inline octets. So ws2_32 reads sixteen bytes that are not the GUID, finds no
    // such extension, and answers WSAEINVAL.
    //
    // That is why the encode fix alone left `wsasendmsg: An invalid argument was supplied` behind:
    // the error was never WSASendMsg's own submission being rejected -- the submission was never
    // made, because the once-guarded lookup ahead of it had already failed. It is hand-owned here
    // rather than left, because a WSASendMsg hand-own on top of a failing lookup would be dead code.
    //
    // The remedy is the mirror pattern at its smallest: sixteen bytes of native GUID in a stack
    // buffer. `stackalloc` is legitimate HERE and would not be for the overlapped submit below,
    // and the difference is the whole of ⟨OQ-G⟩'s reasoning -- WSAIoctl is SYNCHRONOUS, so the
    // kernel is finished with both buffers before this frame returns. The function address comes
    // back into a stack local for the same reason, and is assigned to the record only on success,
    // so a failed lookup cannot leave a half-written address behind for the next caller.
    //
    // WSAIoctl is reached through this package's own LazyProc rather than syscall's generated wrapper
    // for the reason Getsockname bypasses its own: that wrapper takes typed `ж<byte>` buffers, and a
    // stack image is not one. Error handling is the generated wrapper's, verbatim.
    internal static unsafe error loadWSASendRecvMsg() {
        ᏑsendRecvMsgFunc.of(sendRecvMsgFuncᴛ1.Ꮡonce).Do(() => {
            var (s, err) = syscall.Socket(syscall.AF_INET, syscall.SOCK_DGRAM, syscall.IPPROTO_UDP);

            if (err != default!) {
                sendRecvMsgFunc.err = err;
                return;
            }

            try {
                sendRecvMsgFunc.err = lookupExtensionFunction(s, ᏑWSAID_WSARECVMSG, out uintptr recvAddr);

                if (sendRecvMsgFunc.err != default!) {
                    return;
                }

                sendRecvMsgFunc.recvAddr = recvAddr;
                sendRecvMsgFunc.err = lookupExtensionFunction(s, ᏑWSAID_WSASENDMSG, out uintptr sendAddr);

                if (sendRecvMsgFunc.err != default!) {
                    return;
                }

                sendRecvMsgFunc.sendAddr = sendAddr;
            }
            finally {
                // Go defers this; the converted body's `defer` runs on the same boundary a finally
                // does here, and the scratch socket exists only for these two lookups.
                syscall.CloseHandle(s);
            }
        });

        return sendRecvMsgFunc.err;
    }

    // One SIO_GET_EXTENSION_FUNCTION_POINTER round trip: the GUID in, the function address out, both
    // through native stack images. The field order below is the native record's own -- Data1 a DWORD,
    // Data2/Data3 WORDs, Data4 eight inline bytes -- which is also Go's declaration, so the transcribe
    // is field-for-field with no reordering and no endianness question (every field is host order on
    // both sides).
    private static unsafe error lookupExtensionFunction(syscallꓸHandle s, ж<syscall.GUID> Ꮡguid, out uintptr addr) {
        ref var guid = ref Ꮡguid.Value;

        byte* image = stackalloc byte[16];

        *(uint32*)image = guid.Data1;
        *(uint16*)(image + 4) = guid.Data2;
        *(uint16*)(image + 6) = guid.Data3;

        for (nint i = 0; i < 8; i++) {
            image[8 + i] = guid.Data4[i];
        }

        uintptr fn = 0;
        uint32 returned = 0;

        var (r1, _, e1) = syscall.Syscall9(procWSAIoctl.Addr(), 9, (uintptr)s, (uintptr)syscall.SIO_GET_EXTENSION_FUNCTION_POINTER,
                                           (uintptr)(void*)image, (uintptr)16, (uintptr)(void*)(&fn), (uintptr)sizeof(uintptr),
                                           (uintptr)(void*)(&returned), 0, 0);

        addr = fn;

        if (r1 == socket_error) {
            if (e1 != 0) {
                return errnoErr(e1);
            }

            return syscall.EINVAL;
        }

        return default!;
    }

    public static unsafe error WSASendMsg(syscallꓸHandle fd, ж<WSAMsg> Ꮡmsg, uint32 flags, ж<uint32> ᏑbytesSent, ж<syscall.Overlapped> Ꮡoverlapped, ж<byte> Ꮡcroutine) {
        // The generated body's first act, kept: the extension function pointer is resolved through a
        // WSAIoctl on a scratch socket, once, and its failure is this call's failure.
        var err = loadWSASendRecvMsg();

        if (err != default!) {
            return err;
        }

        ref var msg = ref Ꮡmsg.Value;

        // Keyed by the WAITER -- the same Ꮡoverlapped execIO will harvest by -- so this submit and
        // that harvest name one operation. net_windows_impl.cs's prepareSendto has the full note.
        nuint native = golib.GoAsyncIO.RearmOperation((nuint)(uintptr)fd, Ꮡoverlapped, wsaModeWrite);

        // ONE staged block, carved into three regions in declaration order: the WSAMSG, the WSABUF
        // array it points at, and the address image. A zero-buffer submit is legal (a zero-length
        // datagram) and still needs the other two, so the array is sized for at least one entry.
        uint32 slots = msg.BufferCount == 0 ? 1 : msg.BufferCount;
        int bufBytes = checked((int)slots * sizeof(NativeWSABuf));
        int msgBytes = sizeof(NativeWSAMsg);
        byte* block = (byte*)golib.GoAsyncIO.StageOperationBuffer(Ꮡoverlapped, checked(msgBytes + bufBytes + syscall.GoNativeSockaddrLen));

        NativeWSAMsg* nativeMsg = (NativeWSAMsg*)block;
        NativeWSABuf* staged = (NativeWSABuf*)(block + msgBytes);

        stageWSABufs(msg.Buffers, msg.BufferCount, staged);

        // The address is optional: internal/poll's WriteMsg leaves Name unset on a CONNECTED socket,
        // where the kernel already knows the destination, and Winsock reads lpMsg->name only when
        // namelen says there is one.
        byte* addr = null;
        int32 addrlen = 0;

        if (msg.Namelen > 0 && msg.Name != nil) {
            // Namespace `go`, not `go.golib` -- the token table is a ж seam rather than a runtime
            // service, and internal/poll's generated MintOpaque call spells it the same way.
            if (ManagedPointerTokens.Resolve((nuint)(uintptr)msg.Name) is not ж<syscall.RawSockaddrAny> Ꮡrsa) {
                // Every producer of this field in the corpus mints it with MintOpaque, so a token
                // that does not resolve means the record was collected or never minted -- neither of
                // which has a destination to send to. EINVAL is what Winsock itself answers for a
                // malformed address, and it is what the generated wrapper maps a lost errno to.
                return syscall.EINVAL;
            }

            addr = block + msgBytes + bufBytes;
            syscall.GoWriteNativeRawSockaddr(Ꮡrsa, addr);

            // The record is 116 bytes; the family's image inside it is 16 or 28, and internal/poll's
            // encoder is what knows which. Passing the record's size instead would have Winsock read
            // a sockaddr_in6 out of a sockaddr_in.
            addrlen = msg.Namelen;
        }

        nativeMsg->Name = addr;
        nativeMsg->Namelen = addrlen;
        nativeMsg->Buffers = staged;
        nativeMsg->BufferCount = msg.BufferCount;
        nativeMsg->Control.Len = msg.Control.Len;
        nativeMsg->Control.Buf = msg.Control.Buf == nil ? null : (byte*)(void*)msg.Control.Buf;
        nativeMsg->Flags = msg.Flags;

        // lpNumberOfBytesSent takes a local rather than the caller's box, mirroring submitSendto:
        // the real count comes back through execIO's WSAGetOverlappedResult, and this parameter is
        // only meaningful on immediate completion.
        uint32 sentBytes = 0;
        var (r1, _, e1) = syscall.Syscall6(sendRecvMsgFunc.sendAddr, 6, (uintptr)fd, (uintptr)(void*)nativeMsg,
                                           (uintptr)flags, (uintptr)(void*)(&sentBytes), (uintptr)native, (uintptr)Ꮡcroutine);

        if (ᏑbytesSent != nil) {
            ᏑbytesSent.Value = sentBytes;
        }

        // The generated body's mapping, verbatim: a socket_error with no errno is EINVAL, not success.
        if (r1 == socket_error) {
            if (e1 != 0) {
                return errnoErr(e1);
            }

            return syscall.EINVAL;
        }

        return default!;
    }

    // The submitting package's own read mode, the sibling of net_windows_impl.cs's wsaModeWrite.
    // golib treats it opaquely and `syscall` spells the same 'r' for its read submits, so the two
    // agree without either exporting a constant.
    private const nint wsaModeRead = 'r';

    // ---- THE HARVEST TWIN -------------------------------------------------------------------------
    //
    // WSARecvMsg is WSASendMsg's mirror and carries the identical struct-passing defect -- the
    // generated wrapper hands the kernel `uintptr(unsafe.Pointer(msg))`, the address of the managed
    // WSAMsg -- but the REMEDY is not a mirror, and that asymmetry is the whole of this function.
    //
    // A submit can finish its work before it returns: the caller wrote the values, this file
    // transcribes them, the kernel reads them. A RECEIVE cannot. The kernel fills the record AFTER
    // the wrapper has returned, so there is no moment inside it at which a decode could run -- the
    // decode has to be carried by the operation and run when it completes. That is what
    // golib.GoAsyncIO.SetOperationCompletion exists for, and this package's own
    // WSAGetOverlappedResult (zsyscall_windows_wsa_impl.cs) is where every asynchronous exit of
    // execIO funnels through and runs it. `syscall`'s WSARecvFrom is the same shape one package over
    // and is this function's template, down to capturing the staged addresses as INTEGERS -- a C#
    // closure cannot capture a pointer local, and the staging outlives the operation by construction,
    // so an address is the honest thing to carry.
    //
    // WHAT COMES BACK, and why each part matters:
    //   * the ADDRESS -- transcribed into the very RawSockaddrAny box internal/poll then reads with
    //     rawToSockaddrInet4/6 or RawSockaddrAny.Sockaddr, through syscall's ONE native->managed
    //     transcription (GoReadNativeRawSockaddr). Without it ReadMsg reports whatever that box held
    //     from the previous receive.
    //   * Control.Len -- internal/poll returns it as `oobn`, the caller's count of control bytes.
    //   * Flags -- internal/poll returns it as the third value; MSG_TRUNC/MSG_CTRUNC live here.
    //
    // WHAT DOES NOT NEED CARRYING BACK: the DATA. stageWSABufs points the native WSABUFs straight at
    // the Go buffers' addresses, and golib's ж->pointer conversion holds that storage still for the
    // box's lifetime (EnsureStableAddress), so the kernel writes into the caller's slice directly.
    // `syscall` additionally pins through its own operation record; here the boxes stay reachable for
    // the whole call by construction, because execIO blocks on the completion it submitted. The same
    // reasoning covers the control buffer, which is the caller's own `oob` slice.
    //
    // ⚠ NET'S OWN WINDOWS TESTS NEVER PASS A NON-EMPTY oob (censused across the 1.23.12 suite: every
    // Windows-reachable ReadMsg call site passes literal nil; the only non-empty case is
    // unixsock_readmsg_test.go's SCM_RIGHTS, which is //go:build unix and never compiles here). So
    // the control path below is structural rather than measured -- written to be correct rather than
    // because something exercises it, and said plainly so a later reader does not mistake the guard's
    // silence for coverage.
    public static unsafe error WSARecvMsg(syscallꓸHandle fd, ж<WSAMsg> Ꮡmsg, ж<uint32> ᏑbytesReceived, ж<syscall.Overlapped> Ꮡoverlapped, ж<byte> Ꮡcroutine) {
        var err = loadWSASendRecvMsg();

        if (err != default!) {
            return err;
        }

        ref var msg = ref Ꮡmsg.Value;

        // Resolved BEFORE the rearm, so an early return owes the operation nothing. Unlike WriteMsg,
        // internal/poll's ReadMsg ALWAYS sets Name -- a receive has nowhere to report the sender
        // otherwise -- so a token that does not resolve is a real defect rather than a legal shape.
        if (ManagedPointerTokens.Resolve((nuint)(uintptr)msg.Name) is not ж<syscall.RawSockaddrAny> Ꮡrsa) {
            return syscall.EINVAL;
        }

        nuint native = golib.GoAsyncIO.RearmOperation((nuint)(uintptr)fd, Ꮡoverlapped, wsaModeRead);

        uint32 slots = msg.BufferCount == 0 ? 1 : msg.BufferCount;
        int bufBytes = checked((int)slots * sizeof(NativeWSABuf));
        int msgBytes = sizeof(NativeWSAMsg);
        byte* block = (byte*)golib.GoAsyncIO.StageOperationBuffer(Ꮡoverlapped, checked(msgBytes + bufBytes + syscall.GoNativeSockaddrLen));

        NativeWSAMsg* nativeMsg = (NativeWSAMsg*)block;
        NativeWSABuf* staged = (NativeWSABuf*)(block + msgBytes);
        byte* addr = block + msgBytes + bufBytes;

        stageWSABufs(msg.Buffers, msg.BufferCount, staged);

        // ⚠ ZEROED EVERY SUBMIT, load-bearing rather than tidy. The staging block belongs to the
        // OPERATION and is reused by every receive on this waiter, so the PREVIOUS datagram's sender
        // address is still sitting here. The harvest transcribes the full record (see there), which
        // is only exact because these bytes start at zero.
        for (nint i = 0; i < syscall.GoNativeSockaddrLen; i++) {
            addr[i] = 0;
        }

        nativeMsg->Name = addr;
        // What the kernel is told it has ROOM for -- the sockaddr_storage buffer, not the 116-byte Go
        // record the harvest transcribes into. Conflating the two is how a 128-byte write lands in a
        // 116-byte struct.
        nativeMsg->Namelen = syscall.GoNativeSockaddrLen;
        nativeMsg->Buffers = staged;
        nativeMsg->BufferCount = msg.BufferCount;
        nativeMsg->Control.Len = msg.Control.Len;
        nativeMsg->Control.Buf = msg.Control.Buf == nil ? null : (byte*)(void*)msg.Control.Buf;
        // Flags is IN/OUT: internal/poll seeds it with the caller's flags and reads it back after.
        nativeMsg->Flags = msg.Flags;

        nuint msgAddress = (nuint)nativeMsg;
        nuint addrAddress = (nuint)addr;
        ж<WSAMsg> Ꮡdst = Ꮡmsg;

        golib.GoAsyncIO.SetOperationCompletion(Ꮡoverlapped, _ => {
            NativeWSAMsg* completed = (NativeWSAMsg*)msgAddress;

            // The FULL record, not the returned namelen. Winsock's contract does not promise to
            // update lpMsg->namelen on receive, and it does not need to: the buffer was zeroed at
            // submit, so transcribing all 116 bytes reproduces exactly what the kernel wrote with
            // zeros behind it. Depending on a length the contract does not guarantee would buy
            // nothing and could silently truncate.
            syscall.GoReadNativeRawSockaddr(Ꮡrsa, (byte*)addrAddress, syscall.GoRawSockaddrAnyLen);

            Ꮡdst.Value.Control.Len = completed->Control.Len;
            Ꮡdst.Value.Flags = completed->Flags;
        });

        uint32 recvd = 0;
        var (r1, _, e1) = syscall.Syscall6(sendRecvMsgFunc.recvAddr, 5, (uintptr)fd, (uintptr)(void*)nativeMsg,
                                           (uintptr)(void*)(&recvd), (uintptr)native, (uintptr)Ꮡcroutine, 0);

        if (ᏑbytesReceived != nil) {
            ᏑbytesReceived.Value = recvd;
        }

        if (r1 == socket_error) {
            error e = e1 != 0 ? errnoErr(e1) : syscall.EINVAL;

            // ERROR_IO_PENDING is the normal overlapped path: the harvest runs the transcription. Any
            // OTHER error means no completion will arrive, so drop the pending work rather than leave
            // it for the next submit on this waiter to inherit -- WSARecvFrom's own rule.
            if (!AreEqual(e, syscall.ERROR_IO_PENDING)) {
                golib.GoAsyncIO.CompleteOperation(Ꮡoverlapped, 0);
            }

            return e;
        }

        // Completed SYNCHRONOUSLY: the record is already filled and a descriptor carrying
        // skipSyncNotif never harvests, so run the decode now. CompleteOperation removes before it
        // runs, so a later harvest of the same operation cannot decode twice.
        golib.GoAsyncIO.CompleteOperation(Ꮡoverlapped, (nint)recvd);
        return default!;
    }
}
