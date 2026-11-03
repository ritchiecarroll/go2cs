// syscall_managed_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The entersyscall FAMILY -- entersyscall, reentersyscall, entersyscallblock, exitsyscall --
// hand-owned (COORD ruling 2026-09-22; registered in manualConversionFuncs["runtime"]).
//
// WHY. Go's family hands the goroutine's P to the scheduler for the length of a system call and
// records the caller's PC/SP/FP (gp.syscallpc / syscallsp / syscallbp) for tracebacks taken while
// it is blocked. The managed host has no P to release and no M to hand off -- every goroutine is a
// dedicated thread (golib's executor) -- and a traceback over a managed syscall reads no Go PC, so
// those three fields stay ZERO. entersyscall's own first line,
// `reentersyscall(sys.GetCallerPC(), sys.GetCallerSP(), getcallerfp())`, reads two compiler
// intrinsics whose contract cannot be met here; they STAY THROWING by name (a synthetic PC would
// feed Go's PC arithmetic), which is why the family is displaced rather than the intrinsics given
// a value. Left converted, the runtime row's test host died on the first runtime.Entersyscall of
// TestPreemptionAfterSyscall's goroutines.
//
// WHAT MOVES: only the goroutine's status, _Grunning -> _Gsyscall on entry and back on exit, so a
// reader of gp's status (the goroutine profile, readgstatus) sees the truth. The CAS is made
// directly on atomicstatus rather than through casgstatus, for the reason stubs_impl.cs's park hook
// records: casgstatus's tracked arm into _Grunning records sched.timeToRun, whose inline histogram is
// unallocated in the managed zero-value `sched`. An entry from any status but _Grunning, or an exit
// from any status but _Gsyscall, is a caller defect and throws by name, as the park hook does.
//
// Hand-owned: there is no syscall_managed_impl.go, so a -stdlib reconvert never regenerates this file.

using go.golib;
using go.@internal.runtime;   // atomic.Uint32's CompareAndSwap is an extension method over ж<Uint32>

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    internal static void entersyscall() => reentersyscall(0, 0, 0);

    // pc, sp and bp are Go's saved-frame coordinates; the managed host records none of them (see the
    // header), so the arguments are accepted for signature fidelity and not stored.
    internal static void reentersyscall(uintptr pc, uintptr sp, uintptr bp)
    {
        ж<g> gp = getg();

        if (!gp.of(g.Ꮡatomicstatus).CompareAndSwap((uint32)_Grunning, (uint32)_Gsyscall))
            throw new PanicException($"runtime: entersyscall on goroutine {gp.Value.goid} whose status is {readgstatus(gp)}, not _Grunning ({(uint32)_Grunning})");

        // golib's half: a returning main reads a goroutine inside a system call as not runnable (Goroutine's
        // main-return marks).
        Goroutine.EnterBlocked();
    }

    // Go's entersyscallblock is entersyscall for a call known to block: it hands the P off at once
    // rather than letting sysmon retake it. With no P there is no difference to make.
    internal static void entersyscallblock() => reentersyscall(0, 0, 0);

    internal static void exitsyscall()
    {
        Goroutine.ExitBlocked();

        ж<g> gp = getg();

        if (!gp.of(g.Ꮡatomicstatus).CompareAndSwap((uint32)_Gsyscall, (uint32)_Grunning))
            throw new PanicException($"runtime: exitsyscall on goroutine {gp.Value.goid} whose status is {readgstatus(gp)}, not _Gsyscall ({(uint32)_Gsyscall})");

        gp.Value.waitsince = 0;
    }

    // TEST SEAMS (pinner_impl.cs's pattern; GolibTests is not in the InternalsVisibleTo grant).
    public static void GoEntersyscall() => entersyscall();

    public static void GoEntersyscallblock() => entersyscallblock();

    public static void GoExitsyscall() => exitsyscall();

    public static uint GoCurrentGStatus() => readgstatus(getg());

    public static (nuint pc, nuint sp, nuint bp) GoCurrentSyscallFrame()
    {
        ж<g> gp = getg();

        return (gp.Value.syscallpc.Value, gp.Value.syscallsp.Value, gp.Value.syscallbp.Value);
    }

    public static uint GoGSyscallStatus() => (uint)_Gsyscall;

    public static void GoGetCallerPC() => _ = @internal.runtime.sys_package.GetCallerPC();
}
