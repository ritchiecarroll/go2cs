// cpuprof_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.setProcessCPUProfiler / runtime.setThreadCPUProfiler on LINUX, hand-owned by
// manualConversionFuncs["runtime"] (goosAny) under COORD's ruling of 2026-09-26 (ledger 4a122cd994,
// increment I1 of docs/phase4/DESIGN-managed-profiling.md). The linux/darwin twin of
// windows/cpuprof_windows_impl.cs.
//
// WHAT GO'S LINUX BODIES DO. The process setter is setProcessCPUProfilerTimer (signal_unix.go): it installs
// the SIGPROF handler (getsig / setsig) and arms the process-wide ITIMER_PROF with setitimer. The thread
// setter records m.profilehz and creates a per-thread CLOCK_THREAD_CPUTIME_ID timer (timer_create /
// timer_settime) that delivers SIGPROF to that thread.
//
// WHAT THAT COST (measured on linux at db1bd885a2). The first StartCPUProfile in a process threw out of
// runtime.SetCPUProfileRate: setcpuprofilerate -> setProcessCPUProfiler -> setProcessCPUProfilerTimer ->
// getsig -> sigaction -> sysSigaction -> rt_sigaction, a PartialStubGenerator throw. By then pprof's
// cpu.profiling and runtime's cpuprof.on were set and cpuprof.lock was held, so every later
// StartCPUProfile answered "cpu profiling already in use": 16 of runtime/pprof's 36 divergences on linux
// (TestAtomicLoadStore64 first) and net/http/pprof's /debug/pprof/profile?seconds=1.
//
// WHAT THESE BODIES ARE. Go's OWN shape for a port with no profiling interrupts, runtime/os3_plan9.go:158-164,
// the same shape the Windows file holds. The process setter does nothing; the thread setter records
// m.profilehz. SetCPUProfileRate now completes, StartCPUProfile / StopCPUProfile round-trip, and the profile
// is VALID with ZERO samples. A test that asserts on sample content ends in Go's own CPUProfilingBroken skip
// or fails by its own cause, and a second StartCPUProfile without a Stop returns Go's own
// "cpu profiling already in use" error rather than throwing. The store keeps the Windows file's atomic form.
//
// Hand-owned (no cpuprof_linux_impl.go exists, so a reconvert never regenerates this file).
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
