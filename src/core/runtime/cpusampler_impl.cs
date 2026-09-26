// cpusampler_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The CPU SAMPLER SEAM: where an opt-in sampler plugs into Go's own profile path. Section 11 of
// docs/phase4/DESIGN-managed-profiling.md, the owner's class C ruling (BUILD the sampler, OPT-IN ONLY),
// approved by COORD on 2026-09-26.
//
// WHY HERE. Go starts and stops CPU profiling in setcpuprofilerate (proc.go), which takes
// prof.signalLock and calls setProcessCPUProfiler(hz) BEFORE it stores the new prof.hz. That setter is
// hand-owned on every target (cpuprof_<goos>_impl.cs) as Go's no-sampler shape, so it is the one place a
// sampler starts and stops. On stop the profile log is still open and prof.hz still reads the old rate,
// which is the state cpuprof.add writes in. But signalLock is HELD, so a drain through cpuprof.add would
// spin forever: the drain writes the log directly (cpuSampleWrite below), as add does once it has the lock.
//
// WHAT AN ORDINARY PROGRAM CARRIES. This file only. No sampler is registered unless a program opts in
// (<GoCpuProfiler>true</GoCpuProfiler> adds the go2cs.CpuProfiler companion and a module initializer that
// calls GoRegisterCpuSampler). With none registered, the setters stay I1's no-op and a CPU profile is
// valid with zero samples. A sampler that throws is caught here and leaves the same zero-sample profile:
// the profile path never throws for a sampler's sake.
//
// Hand-owned (no cpusampler_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

using System;
using System.Threading;
using @unsafe = unsafe_package;
using @internal.runtime;

/// <summary>Writes one CPU-profile sample: its time (<c>nanotime</c>), its stack of Go PCs, and its label
/// tag (<c>nil</c> for none). Valid only on the thread inside <see cref="IGoCpuSampler.Stop"/>.</summary>
public delegate void GoCpuSampleWriter(int64 nanotime, slice<uintptr> stack, @unsafe.Pointer tag);

/// <summary>An opt-in CPU sampler, registered with <c>runtime_package.GoRegisterCpuSampler</c>.</summary>
public interface IGoCpuSampler
{
    /// <summary>Profiling turned on at <paramref name="hz"/>. Called with prof.signalLock held: start
    /// sampling and return.</summary>
    void Start(int hz);

    /// <summary>Profiling turned off. Called with prof.signalLock held and the profile log still open:
    /// stop sampling and hand every sample to <paramref name="write"/> before returning.</summary>
    void Stop(GoCpuSampleWriter write);
}

partial class runtime_package {

private static IGoCpuSampler? s_cpuSampler;

[ThreadStatic] private static bool t_cpuSamplerDraining;

/// <summary>Registers the process's CPU sampler. The first registration wins; a later one is refused
/// (returns false), since swapping samplers while a profile runs would split its samples.</summary>
public static bool GoRegisterCpuSampler(IGoCpuSampler sampler)
{
    ArgumentNullException.ThrowIfNull(sampler);
    return Interlocked.CompareExchange(ref s_cpuSampler, sampler, null) is null;
}

/// <summary>The PC a CPU sample records for a sampled frame's method: its synthetic PC when the method is
/// a frame Go's unwinder would report (the test <c>runtime.Callers</c> applies), else 0, which the sampler
/// drops. The profile builder resolves the PC through <c>runtime.CallersFrames</c> to the method's Go
/// name.</summary>
public static uintptr GoCpuSamplePC(System.Reflection.MethodBase method)
{
    ArgumentNullException.ThrowIfNull(method);
    return isGoSourceFrame(method) ? GoSyntheticPC.Of(method) : 0;
}

// Called by each target's setProcessCPUProfiler, with prof.signalLock held.
private static void cpuSamplerSetRate(int32 hz) {
    IGoCpuSampler? sampler = Volatile.Read(ref s_cpuSampler);

    if (sampler is null) {
        return;
    }

    try {
        if (hz != 0) {
            sampler.Start(hz);
            return;
        }

        t_cpuSamplerDraining = true;

        try {
            sampler.Stop(cpuSampleWrite);
        }
        finally {
            t_cpuSamplerDraining = false;
        }
    }
    catch (Exception) {
        // A sampler's failure leaves the zero-sample profile, never a throw out of SetCPUProfileRate
        // (which would leave cpuprof.lock held: see cpuprof_<goos>_impl.cs).
    }
}

// cpuprof.add's body without its lock (the caller, setcpuprofilerate, holds prof.signalLock).
private static void cpuSampleWrite(int64 nanotime, slice<uintptr> stack, @unsafe.Pointer tag) {
    if (!t_cpuSamplerDraining || Ꮡprof.of(profᴛ1.Ꮡhz).Load() == 0) {
        return;
    }

    if (cpuprof.numExtra > 0 || cpuprof.lostExtra > 0 || cpuprof.lostAtomic > 0) {
        cpuprof.addExtra();
    }

    var hdr = new uint64[]{1}.array();
    cpuprof.log.write(tag == nil ? nil : Ꮡ(tag), nanotime, hdr[..], stack);
}

} // end runtime_package
