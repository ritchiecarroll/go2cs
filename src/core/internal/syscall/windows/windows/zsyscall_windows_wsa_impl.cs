// zsyscall_windows_wsa_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of the HARVEST half of the overlapped socket surface -- the one member
// of the submit seam that lives outside the `syscall` package. Design:
// docs/phase4/DESIGN-netpoll-managed-poller.md §4.3/§4.5.
//
// THE ONE THING IT NEEDS, AND WHY THAT IS ALL. execIO's harvest names the SAME `&o.o` the submit
// named (internal/poll/windows/fd_windows.cs:220), and the operation's real control block is the
// native OVERLAPPED the record in syscall/windows/zsyscall_windows_wsa_impl.cs allocated -- not the
// managed `operation.o` field, whose address is transient for the reasons that file's header gives.
// So the entire contract between the two packages is ONE property: the operation's native address,
// read through golib's GoAsyncIO (the same descriptor/operation rendezvous the readiness signal
// uses). syscall cannot expose it directly -- a public seam on a PUBLISHED package's API surface
// would be a non-Go symbol -- and this package cannot reach into syscall's record type anyway.
//
// WHY THE REAL OS ROUTINE, NOT THE CALLBACK'S RESULT. An earlier plan had the completion callback
// deposit errorCode/numBytes for this function to report. That would have re-derived Windows' own
// error mapping, and derived it WRONG: a completion callback's errorCode is a WIN32 code, while
// execIO and net branch on WSA codes (ERROR_NETNAME_DELETED where the suites expect WSAECONNRESET;
// WSAEMSGSIZE and ERROR_MORE_DATA are both load-bearing in execIO's own harvest branch). Calling the
// real WSAGetOverlappedResult against the real control block answers in the namespace the callers
// speak: the kernel wrote Internal/InternalHigh at completion time, so `wait: false` is correct after
// the CLR has already dequeued the packet.
//
// The out-parameters marshal through call-local natives and are copied back -- the mirror-is-a-local
// doctrine, which async does not break for out-params (they are written only during this call).
// The generated wrapper's error handling is reproduced verbatim (r1 == 0 -> errnoErr(e1)).

using System;

using GoAsyncIO = go.golib.GoAsyncIO;

// Hand-owned (no zsyscall_windows_wsa_impl.go exists, so a reconvert never regenerates this file).
// The declaration it replaces is registered in the converter's manualConversionFuncs, which is what
// turns the generated body into a placeholder.
[module: go.GoManualConversion]

// Unlike `syscall`, this package's generated csproj emits AllowUnsafeBlocks=false, so the marker is
// load-bearing here rather than a consistency habit: without it the pointer work below does not
// compile. The regenerated .csproj flipping to true is part of this change's intended footprint.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using syscall = go.syscall_package;

partial class windows_package
{
    public static unsafe error /*err*/ WSAGetOverlappedResult(syscallꓸHandle h, ж<syscall.Overlapped> Ꮡo, ж<uint32> Ꮡbytes, bool wait, ж<uint32> Ꮡflags) {
        if (!GoAsyncIO.TryGetOperationAddress(Ꮡo, out nuint overlapped)) {
            // Not reachable from converted code: execIO harvests only an operation it submitted
            // through the seam, and the seam creates the record at submit. Loud by name rather than
            // silently answering for an operation nobody owns.
            throw new InvalidOperationException(
                "internal/syscall/windows: WSAGetOverlappedResult on an overlapped with no submit-seam record");
        }

        uint32 bytes = 0;
        uint32 flags = 0;
        uint32 _p0 = wait ? 1u : 0u;

        var (r1, _, e1) = syscall.Syscall6(procWSAGetOverlappedResult.Addr(), 5, (uintptr)h, (uintptr)overlapped, (uintptr)(void*)(&bytes), (uintptr)_p0, (uintptr)(void*)(&flags), 0);

        if (Ꮡbytes != nil) {
            Ꮡbytes.Value = bytes;
        }

        if (Ꮡflags != nil) {
            Ꮡflags.Value = flags;
        }

        if (r1 == 0) {
            return errnoErr(e1);
        }

        // THE DECODE HOOK (netpoll design §4.8, RATIFIED). This call is the one place every
        // ASYNCHRONOUS completion funnels through -- execIO has exactly three exits after a submit,
        // and the two that observe a completion both land here (the third, skipSyncNotif, never
        // harvests and is transcribed inline by the submitting wrapper). An operation that owes
        // work -- today, WSARecvFrom transcribing the kernel's native sockaddr into the managed
        // RawSockaddrAny internal/poll will read back -- runs it now, with the transfer count.
        //
        // Most operations owe NOTHING: every send, every TCP read. That is not an error and costs a
        // dictionary miss. What the work IS stays entirely unknown here; this package owns the
        // harvest, not the layout.
        GoAsyncIO.CompleteOperation(Ꮡo, (nint)bytes);

        return default!;
    }
}
