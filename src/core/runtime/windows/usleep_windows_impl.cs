// usleep_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.usleep on Windows, hand-owned (COORD ruling 2026-09-22; manualConversionFuncs["runtime"],
// goosWindows).
//
// WHY. Go's Windows usleep runs on the system stack and waits `us` microseconds on the M's
// high-resolution waitable timer (SetWaitableTimer + WaitForSingleObject), or ms-grained on
// WaitForSingleObject(INVALID_HANDLE_VALUE, us/1000) when there is none -- both through
// stdcall6/stdcall2 -> asmcgocall, which has no managed body. Left converted, every usleep threw
// NotImplementedException: the runtime row's test host died on one inside
// TestRuntimeLockMetricsAndProfile/runtime.lock/sample-1's contention loop.
//
// WHAT. The same wait: golib's high-resolution waitable timer (GoHighResolutionTimer, the object Go's
// highResTimer is), armed for `us` and waited on. Where none can be created, the whole milliseconds
// through Thread.Sleep, then the sub-millisecond remainder by yielding against a Stopwatch deadline, so
// a usleep(100) waits ~100 µs rather than a rounded 0 or 1 ms. usleep(0) returns at once, as a
// zero-length wait does. The caller's goroutine is a dedicated thread (golib's executor), so blocking
// it is exactly what Go's M does, and systemstack collapses to a direct call.

using System.Diagnostics;
using System.Threading;
using go.golib;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    internal static void usleep(uint32 us)
    {
        if (us == 0)
            return;

        if (GoHighResolutionTimer.TrySleep((long)us * 1_000))
            return;

        long deadline = Stopwatch.GetTimestamp() + (long)us * Stopwatch.Frequency / 1_000_000;

        if (us >= 1000)
            Thread.Sleep((int)(us / 1000));

        while (Stopwatch.GetTimestamp() < deadline)
            Thread.Yield();
    }

    // TEST SEAM (pinner_impl.cs's pattern; GolibTests is not in the InternalsVisibleTo grant).
    public static void GoUsleep(uint us) => usleep(us);
}
