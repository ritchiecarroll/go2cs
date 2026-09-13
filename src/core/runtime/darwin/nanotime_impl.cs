// nanotime_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.nanotime1 — darwin's monotonic clock, realized on the same golib source the other two
// flavors already read.
//
// Go implements this in assembly per platform. On darwin it is a mach_absolute_time trampoline:
// sys_darwin.cs's converted body heap-allocates a nanotime1_r, calls
// libcCall(FuncPCABI0(nanotime_trampoline), …) to fill it, and scales the raw timer by the
// mach_timebase numer/denom pair the same call returns. FuncPCABI0 of that trampoline is a class-C
// external stub — Go's own assembly, with nothing to resolve from — so the call throws, and the throw
// is NOT a dormant edge: nanotime is read by cpuprof, metrics, mgc, mgcmark, mgcpacer, mprof, netpoll
// and debuglog, and the first call into any of them dies. runtime/pprof's StartCPUProfile reaches it
// through SetCPUProfileRate and the panic on a profiling goroutine takes the whole host down.
//
// The contract is narrow and fully satisfiable here, which is what makes this a truthful realization
// rather than a stand-in: Go asks for a monotonic, nanosecond-denominated counter whose EPOCH IS
// ARBITRARY, because only differences are ever observed. golib's MonotonicClock is exactly that, and
// the mach_timebase scaling the generated body performs is the same conversion MonotonicClock has
// already done against Stopwatch.Frequency — so this is not a coarser clock than Go's, it is the same
// quantity read from the platform's own monotonic source.
//
// ⚠ Declared NON-partial, and that is the difference from the linux and windows files beside it.
// Those two flavors declare nanotime1 BODYLESS in stubs3.cs, so their hand-owns are `partial` bodies
// that pair with a generated declaration and need no converter change at all. Darwin's is a BODIED
// converted function, which is displaced ONLY through manualConversionFuncs — and that registration
// replaces the body with a COMMENT placeholder rather than a bodyless partial, so this file owns the
// whole declaration. Copying the linux file verbatim yields `partial` with nothing to pair against.
//
// One observation recorded rather than acted on: the linux file's header gives "darwin already has a
// real body" as the reason nanotime1's hand-own is per-GOOS rather than flat. This commit spends that
// reason — all three flavors now bind the same one-line body — so a flat consolidation becomes
// possible. It is deliberately NOT done here: it would touch two stable flavors to save two lines,
// against the minimal-footprint rule, and layout L3 routes per-GOOS hand-owns by their principal's
// platform set anyway. Noted so the next reader knows the option exists and why it was declined.
//
// Hand-owned (no nanotime_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

using go.golib;

partial class runtime_package {

internal static int64 nanotime1() {
    return MonotonicClock.Nanoseconds();
}

} // end runtime_package
