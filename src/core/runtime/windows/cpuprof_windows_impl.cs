// cpuprof_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.setProcessCPUProfiler / runtime.setThreadCPUProfiler on WINDOWS, hand-owned by
// manualConversionFuncs["runtime"] (goosWindows) under COORD's ruling of 2026-09-22.
//
// WHAT GO'S WINDOWS BODIES DO. The process setter creates a waitable timer (high-resolution when
// available) and starts profileLoop on a new m. That thread wakes on the timer and SuspendThread /
// GetThreadContext-samples every m. The thread setter arms that timer with SetWaitableTimer and
// records m.profilehz. Every OS call goes through stdcall -> asmcgocall, which has no managed body.
//
// WHAT THAT COST. setcpuprofilerate's FIRST call is setThreadCPUProfiler(0), so the first
// StartCPUProfile in a process threw NotImplementedException out of runtime.SetCPUProfileRate. By then
// pprof's cpu.profiling and runtime's cpuprof.on/cpuprof.log were already set, cpuprof.lock was held
// (SetCPUProfileRate locks before setcpuprofilerate, and the unlock was skipped), and m.locks++ was not
// undone. The test's own `defer StopCPUProfile()` comes AFTER StartCPUProfile returns, so it was never
// registered. Every later StartCPUProfile answered "cpu profiling already in use": 12 of runtime/pprof's
// 37 divergences at 1.24.13 (TestAtomicLoadStore64 first, then TestCPUProfile and eleven more). A stop
// from the test host could not have repaired it: it would block on the held lock or re-throw at the same
// stdcall, then wait forever on the profile writer that never started.
//
// WHAT THESE BODIES ARE. Go's OWN shape for a port with no profiling interrupts, runtime/os3_plan9.go:158-164.
// The process setter does nothing; the thread setter records m.profilehz ("TODO: Enable profiling
// interrupts"). The managed model has no interrupt sampler, which is exactly that port. SetCPUProfileRate
// now completes, StartCPUProfile / StopCPUProfile round-trip, and the profile is VALID with ZERO samples.
// A test that asserts on sample content fails by its own cause, and a second StartCPUProfile
// without a Stop returns Go's own "cpu profiling already in use" error rather than throwing. The
// profilehz store keeps the Windows body's own atomic form (profileLoop reads it concurrently in Go);
// nothing reads it here, but the field keeps its meaning.
//
// The linux and darwin setters take the same shape in their own cpuprof_<goos>_impl.cs (2026-09-26,
// ledger 4a122cd994): their converted signal/timer bodies throw at the signal-installation path too.
//
// Hand-owned (no cpuprof_windows_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

using atomic = @internal.runtime.atomic_package;

partial class runtime_package {

internal static void setProcessCPUProfiler(int32 hz) {
    // Go's no-sampler shape, plus the opt-in sampler seam (cpusampler_impl.cs): a no-op unless a
    // program registered a sampler.
    cpuSamplerSetRate(hz);
}

internal static void setThreadCPUProfiler(int32 hz) {
    atomic.Store((~getg()).m.of(m.Ꮡprofilehz).Reinterpret<int32, uint32>(), (uint32)hz);
}

// ---- the guard's view (RuntimeCPUProfilerTests) ----

/// <summary>What the calling goroutine's m records as its profiling rate.</summary>
public static int GoThreadProfileHz => (int)(~(~getg()).m).profilehz;

} // end runtime_package
