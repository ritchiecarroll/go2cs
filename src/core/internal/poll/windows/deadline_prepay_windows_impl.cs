// deadline_prepay_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written: the windows part of the first read deadline's pre-pay (../deadline_prepay_impl.cs says
// why and what every flavor pays). On windows the deadline's wake does not end the read. execIO still
// owns a pending overlapped operation, so it cancels it (cancelIO: CancelIoEx, then
// pollDesc.waitCanceled until the kernel posts the aborted completion), harvests it (GetOverlappedResult
// on a file or pipe, WSAGetOverlappedResult on a socket), classifies the result against errno values
// (ERROR_OPERATION_ABORTED maps back to the deadline error), and unpins the operation. The aborted
// completion reaches this package on a CLR IO thread through syscall's completion callback, golib's
// GoAsyncIO.Signal and the descriptor's readiness sink. None of that runs before a process's first
// deadline expires, so it compiled after the deadline instant: the i9 read the first deadline 6.2-7.8 ms
// late cold and 1.3-3.4 ms late after a first completion with only the methods prepared (895445983a),
// against Go's 0.4-0.8, and its JIT list named the 24 methods still compiled between the fire and the
// return.
//
// What runs, once per process, inside the poller's start:
//   - RuntimeHelpers.PrepareMethod over this package's cancel, wait and wake methods, the readiness
//     sink, and the syscall and golib methods they call;
//   - on a spare operation nobody submitted: its overlapped's field view, compared with itself, and
//     CancelIoEx on it, which finds no record for it and answers ERROR_NOT_FOUND without calling the
//     kernel; then a pin and unpin of it on a spare FD's read pinner, as execIO's defer does;
//   - the comparisons a woken read makes, on values nobody holds: the poll error codes, the deadline
//     error against itself, and an errno error against the errno values Read and execIO test. Each
//     errno comparison runs twice, because golib compares an error with an errno by reflection, and the
//     invoke stub for the errno's == is built on its second use (named, as a per-call cost, on the BOARD);
//   - syscall.LoadCancelIoEx, Go's own loader for CancelIoEx's kernel32 entry point; it resolves the
//     address CancelIoEx calls through and opens nothing;
//   - internal/syscall/windows' module initialization, which registers that package's part (as syscall
//     registers its own) with GoAsyncIO.AddDeadlinePathPrepay before the poller runs every part.
// Nothing here cancels, waits on or completes a real operation.

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
            readinessSink(0), (Action<ж<FD>, ж<operation>>)cancelIO, (pollDescModeAction)waitCanceled,
            runtime_pollWaitCanceled, netpollReady, golib.GoAsyncIO.Signal, Δsyscall.CancelIoEx, Δsyscall.Syscall,
            windows.GetOverlappedResult, windows.WSAGetOverlappedResult);

        ж<operation> o = Ꮡ(new operation());
        ж<Δsyscall.Overlapped> overlapped = o.of(operation.Ꮡo);
        overlapped.Equals(o.of(operation.Ꮡo));
        Δsyscall.CancelIoEx(Δsyscall.InvalidHandle, overlapped);

        ж<FD> spare = Ꮡ(new FD());
        spare.DerefOrNull().pin('r', o.OrTypedNil());
        spare.of(FD.ᏑreadPinner).Unpin();

        _ = (pollErrTimeout, pollErrClosing);
        AreEqual(ErrDeadlineExceeded, ErrDeadlineExceeded);

        error aborted = Δsyscall.ERROR_OPERATION_ABORTED;
        for (int use = 0; use < 2; use++)
        {
            AreEqual(aborted, Δsyscall.ERROR_OPERATION_ABORTED);
            AreEqual(aborted, Δsyscall.ERROR_HANDLE_EOF);
            AreEqual(aborted, Δsyscall.ERROR_BROKEN_PIPE);
            AreEqual(aborted, windows.ERROR_IO_INCOMPLETE);
        }

        Δsyscall.LoadCancelIoEx();
        RuntimeHelpers.RunModuleConstructor(typeof(windows).Module.ModuleHandle);
    }

    private delegate void pollDescModeAction(ref pollDesc pd, nint mode);
}
