// deadline_prepay_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written: syscall's part of the first read deadline's pre-pay on windows
// (internal/poll/deadline_prepay_impl.cs says why; internal/poll/windows/deadline_prepay_windows_impl.cs
// says what the windows path is). internal/poll cannot name these, because they are private to this
// package, so this package registers them with golib's GoAsyncIO.AddDeadlinePathPrepay when its module
// loads, and the poller runs them when it starts:
//   - RuntimeHelpers.PrepareMethod over the completion callback, which a CLR IO thread runs when the
//     cancelled operation's aborted completion arrives;
//   - the native address of a record on no descriptor: with no binding it allocates nothing and
//     associates nothing, and its address reads zero;
//   - a three-element span of the kind Syscall passes SyscallN, so the compiler's inline-array helpers
//     for it are compiled (CancelIoEx is Syscall's caller on the deadline path).
// Nothing here submits, cancels or completes anything.

using System;
using System.Runtime.CompilerServices;
using System.Threading;

[module: go.GoManualConversion]

namespace go;

using GoAsyncIO = golib.GoAsyncIO;

partial class syscall_package
{
    [ModuleInitializer]
    internal static void registerDeadlinePathPrepay() => GoAsyncIO.AddDeadlinePathPrepay(prepayDeadlinePath);

    private static unsafe void prepayDeadlinePath()
    {
        RuntimeHelpers.PrepareMethod(((IOCompletionCallback)completed).Method.MethodHandle);

        _ = new OverlappedOp(binding: null, wsaModeRead, key: new object()).NativeAddress;

        uintptr none = 0;
        Span<uintptr> three = [none, none, none];
        _ = three[0];
    }
}
