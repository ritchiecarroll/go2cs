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
    internal static void entersyscall() => enterSyscall(NativeCallHint.MayBlock);

    // pc, sp and bp are Go's saved-frame coordinates; the managed host records none of them (see the
    // header), so the arguments are accepted for signature fidelity and not stored.
    internal static void reentersyscall(uintptr pc, uintptr sp, uintptr bp) => enterSyscall(NativeCallHint.MayBlock);

    // Go's entersyscallblock is entersyscall for a call known to block: it hands the P off at once
    // rather than letting sysmon retake it. The hint carries that to the seam, for the M:N scheduler.
    internal static void entersyscallblock() => enterSyscall(NativeCallHint.Blocks);

    // The g status moves FIRST, then golib's transition seam (GoNativeCall, docs/phase4/DESIGN-native-call-seam.md):
    // the status before the P release, as Go orders it (COORD's Q2 ruling). The seam does golib's half today (a
    // returning main reads a goroutine inside a system call as not runnable), and is where an M:N scheduler releases
    // the P.
    private static void enterSyscall(NativeCallHint hint)
    {
        ж<g> gp = getg();

        if (!gp.of(g.Ꮡatomicstatus).CompareAndSwap((uint32)_Grunning, (uint32)_Gsyscall))
            throw new PanicException($"runtime: entersyscall on goroutine {gp.Value.goid} whose status is {readgstatus(gp)}, not _Grunning ({(uint32)_Grunning})");

        GoNativeCall.Enter(hint);
    }

    // The mirror: the seam first (the P reacquired, under M:N), then the status. An exit from a status other than
    // _Gsyscall is a caller defect, refused by name BEFORE the seam is touched, so a stray exit leaves both halves
    // as they were.
    internal static void exitsyscall()
    {
        ж<g> gp = getg();

        if (readgstatus(gp) != (uint32)_Gsyscall)
            throw new PanicException($"runtime: exitsyscall on goroutine {gp.Value.goid} whose status is {readgstatus(gp)}, not _Gsyscall ({(uint32)_Gsyscall})");

        GoNativeCall.Exit();

        if (!gp.of(g.Ꮡatomicstatus).CompareAndSwap((uint32)_Gsyscall, (uint32)_Grunning))
            throw new PanicException($"runtime: exitsyscall on goroutine {gp.Value.goid} whose status is {readgstatus(gp)}, not _Gsyscall ({(uint32)_Gsyscall})");

        gp.Value.waitsince = 0;
    }

    // syscall's crossing into the family, managed_impl.cs's syscallXxx convention: a syscall-package native call
    // whose Go original goes through cgocall (windows' trampoline, its hand-owned rows) enters the system call here,
    // ONE path per transition (DESIGN-native-call-seam §12.2).
    public static void syscallEntersyscall() => entersyscall();

    public static void syscallEntersyscallblock() => entersyscallblock();

    public static void syscallExitsyscall() => exitsyscall();

    // A native-to-Go callback (syscall.NewCallback's shim) crosses as Go's cgocallbackg does: exitsyscall on the way
    // in, reentersyscall on the way out (DESIGN-native-call-seam §6.3, F-3). In order: the seam's ReenterManaged first
    // (the P reacquired, under M:N), then _Gsyscall -> _Grunning; on the way out, the status back first, then
    // ReturnToNative. Under M:N the goroutine is implicitly locked to its thread for the whole callback, because a
    // native frame is below it (§12.1's M7); under today's 1:1 model it always is.
    //
    // Where no native call is open the crossing re-enters nothing (§6.3's dated block), in two cases. A pointer
    // invoked outside any bracketed call: Go throws there (exitsyscall, "syscall frame is no longer valid"); go2cs
    // deliberately does not, since a throw would unwind into native code, and the callback runs as plain Go code. A
    // thread a C library created, with no goroutine identity: Go binds an extra M and runs the callback as a goroutine
    // (needm), dropping it on return (dropm); here the callback runs inside Goroutine.EnterForeignCallback's scope,
    // which gives the thread back clean (S-c2). The flags: 1, the seam was re-entered; 2, the status moved; 4, a
    // foreign thread was attached. The status moves only where it is _Gsyscall, so a crossing opened by GoNativeCall
    // alone (no runtime family below it) re-enters the seam and leaves the status as it found it.
    public readonly struct GoCallbackCrossing
    {
        internal readonly int Entered;
        internal readonly Goroutine.Scope Attached;

        internal GoCallbackCrossing(int entered, Goroutine.Scope attached = default)
        {
            Entered = entered;
            Attached = attached;
        }
    }

    public static GoCallbackCrossing syscallCallbackEnter()
    {
        int depth = GoNativeCall.TransitionDepth;

        if (depth < 0)
            return new GoCallbackCrossing(4, Goroutine.EnterForeignCallback());

        if ((depth & 1) == 0)
            return default;

        GoNativeCall.ReenterManaged();

        ж<g> gp = getg();

        return new GoCallbackCrossing(gp.of(g.Ꮡatomicstatus).CompareAndSwap((uint32)_Gsyscall, (uint32)_Grunning) ? 3 : 1);
    }

    public static void syscallCallbackExit(GoCallbackCrossing crossing)
    {
        int entered = crossing.Entered;

        if ((entered & 4) != 0)
        {
            crossing.Attached.Dispose();
            return;
        }

        if (entered == 0)
            return;

        if ((entered & 2) != 0)
        {
            ж<g> gp = getg();

            if (!gp.of(g.Ꮡatomicstatus).CompareAndSwap((uint32)_Grunning, (uint32)_Gsyscall))
                throw new PanicException($"runtime: a callback returns to native code on goroutine {gp.Value.goid} whose status is {readgstatus(gp)}, not _Grunning ({(uint32)_Grunning})");
        }

        GoNativeCall.ReturnToNative();
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
