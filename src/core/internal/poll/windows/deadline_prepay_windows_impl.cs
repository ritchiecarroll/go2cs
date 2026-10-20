// deadline_prepay_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written: the windows part of the first read deadline's pre-pay (../deadline_prepay_impl.cs says
// why and what every flavor pays). On windows the deadline's wake does not end the read. execIO still
// owns a pending overlapped operation, so it cancels it (cancelIO: CancelIoEx, then
// pollDesc.waitCanceled until the kernel posts the aborted completion) and harvests it
// (GetOverlappedResult on a file or pipe, WSAGetOverlappedResult on a socket), which answers
// ERROR_OPERATION_ABORTED, mapped back to the deadline error. The aborted completion reaches this
// package on a CLR IO thread through golib's GoAsyncIO.Signal and netpollReady. None of that runs
// before a process's first deadline expires, so the first cut left it to compile after the deadline
// instant: the windows probe read the first deadline 3.6-5.1 ms late cold and 2.3-2.6 ms late after a
// first completion, against Go's 0.0-0.8.
//
// What runs, once per process, inside the poller's start:
//   - RuntimeHelpers.PrepareMethod over that path's methods in this package, syscall,
//     internal/syscall/windows and golib;
//   - syscall.LoadCancelIoEx, Go's own loader for CancelIoEx's kernel32 entry point; it resolves the
//     address CancelIoEx calls through and opens nothing.
// Nothing here cancels, waits or completes anything. Not reached: syscall's completion callback is
// private to syscall, so the first completion still compiles that one method.

using System;
using System.Runtime.CompilerServices;

[module: go.GoManualConversion]

namespace go.@internal;

using windows = go.@internal.syscall.windows_package;
using Δsyscall = syscall_package;

partial class poll_package
{
    static partial void prepayPlatformDeadlinePath()
    {
        prepare(
            (Action<ж<FD>, ж<operation>>)cancelIO, (pollDescModeAction)waitCanceled, runtime_pollWaitCanceled,
            netpollReady, golib.GoAsyncIO.Signal, Δsyscall.CancelIoEx, Δsyscall.Syscall,
            windows.GetOverlappedResult, windows.WSAGetOverlappedResult);

        Δsyscall.LoadCancelIoEx();
    }

    private delegate void pollDescModeAction(ref pollDesc pd, nint mode);
}
