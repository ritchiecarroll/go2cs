// nanotime_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.nanotime1 — the monotonic clock the whole runtime reads, realized on the platform's own
// monotonic source.
//
// Go implements this in assembly per platform (a VDSO `clock_gettime` on Linux,
// `QueryPerformanceCounter` on Windows), so the converted declaration in stubs3.cs is a bodyless
// partial and PartialStubGenerator fills it with a throw. That throw is NOT a dormant edge: nanotime
// is read by cpuprof, metrics, mgc, mgcmark, mgcpacer, mprof, netpoll and debuglog, and the first
// call into any of them dies. runtime/pprof's StartCPUProfile reaches it through SetCPUProfileRate
// (cpuprof.cs:75) and the panic on a profiling goroutine takes the whole test host down — which is
// the wall that stood in front of that package's rows.
//
// The contract is narrow and fully satisfiable here, which is what makes this a truthful
// realization rather than a stand-in: Go asks for a monotonic, nanosecond-denominated counter whose
// EPOCH IS ARBITRARY, because only differences are ever observed. golib's MonotonicClock is that
// counter over System.Diagnostics.Stopwatch — the same source Go's own Windows implementation uses.
// The nanosecond scaling, and why it must not be the obvious ticks*1e9/Frequency, is documented
// there; the short version is that the naive product overflows int64 within minutes of uptime and
// would make a MONOTONIC clock run backwards.
//
// Per-GOOS rather than flat because darwin already has a real body (sys_darwin.cs's
// nanotime1 over its own `$INTERNAL` trap), and a flat implementation would collide with it. The
// windows and linux flavors are the two that declare it bodyless, and they share one clock, so the
// computation lives in golib and this file is the platform's binding to it.
//
// Hand-owned (no nanotime_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

using go.golib;

partial class runtime_package {

internal static partial int64 nanotime1() {
    return MonotonicClock.Nanoseconds();
}

} // end runtime_package
