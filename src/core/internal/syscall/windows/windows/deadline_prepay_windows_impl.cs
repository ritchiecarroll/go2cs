// deadline_prepay_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written: internal/syscall/windows' part of the first read deadline's pre-pay
// (internal/poll/deadline_prepay_impl.cs says why; internal/poll/windows/deadline_prepay_windows_impl.cs
// says what the windows path is). The harvest of a cancelled operation runs here: GetOverlappedResult
// calls SyscallN with four arguments and maps the failure through errnoErr. This package registers its
// part with golib's GoAsyncIO.AddDeadlinePathPrepay when its module loads (the poller runs this
// package's module initialization before it runs the parts):
//   - errnoErr of ERROR_OPERATION_ABORTED, the error a cancelled operation's harvest returns;
//   - a four-element span of the kind GetOverlappedResult passes SyscallN, so the compiler's
//     inline-array helpers for it are compiled.
// Nothing here calls the kernel.

using System;
using System.Runtime.CompilerServices;

[module: go.GoManualConversion]

namespace go.@internal.syscall;

using GoAsyncIO = go.golib.GoAsyncIO;
using syscall = go.syscall_package;

partial class windows_package
{
    [ModuleInitializer]
    internal static void registerDeadlinePathPrepay() => GoAsyncIO.AddDeadlinePathPrepay(prepayDeadlinePath);

    private static void prepayDeadlinePath()
    {
        errnoErr(syscall.ERROR_OPERATION_ABORTED);

        uintptr none = 0;
        Span<uintptr> four = [none, none, none, none];
        _ = four[0];
    }
}
