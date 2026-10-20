// deadline_prepay_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written: os's part of the first read deadline's one-time costs, paid when the poller starts
// (internal/poll/deadline_prepay_impl.cs says why and what poll pays). internal/poll cannot name these,
// because os imports it, so os registers them on poll's DeadlinePathPrepay hook when its module loads,
// and poll runs them inside the poller's start, after its own part:
//   - RuntimeHelpers.PrepareMethod over File.Read and File.read, the return path above poll's FD.Read;
//   - SetReadDeadline of the zero Time on a spare File whose file holds a never-opened FD (it returns
//     poll's ErrNoDeadline and touches no descriptor);
//   - wrapErr of ErrDeadlineExceeded on that File, which builds the *PathError an expired Read returns,
//     with its box.
// Nothing here opens, reads or arms anything.

using System;
using System.Runtime.CompilerServices;

[module: go.GoManualConversion]

namespace go;

using poll = @internal.poll_package;

partial class os_package
{
    [ModuleInitializer]
    internal static void registerDeadlinePathPrepay()
    {
        poll.DeadlinePathPrepay += prepayDeadlinePath;
    }

    private static void prepayDeadlinePath()
    {
        // Read and read are also promoted onto the types that embed *File; these casts pick File's own.
        RuntimeHelpers.PrepareMethod(((Func<ж<File>, slice<byte>, (nint, error)>)Read).Method.MethodHandle);
        RuntimeHelpers.PrepareMethod(((Func<ж<File>, slice<byte>, (nint, error)>)read).Method.MethodHandle);

        ж<File> spare = Ꮡ(new File(Ꮡ(new @file())));
        spare.SetReadDeadline(default);
        spare.DerefOrNull().wrapErr("read", ErrDeadlineExceeded);
    }
}
