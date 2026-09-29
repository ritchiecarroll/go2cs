// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Though the debug call function feature is not enabled on
// ppc64, inserted ppc64 to avoid missing Go declaration error
// for debugCallPanicked while building runtime.test
//go:build amd64 || arm64 || loong64 || ppc64le || ppc64
namespace go;

using abi = @internal.abi_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

internal static readonly @string debugCallSystemStack = "executing on Go runtime stack"u8;
internal static readonly @string debugCallUnknownFunc = "call from unknown function"u8;
internal static readonly @string debugCallRuntime = "call from within the Go runtime"u8;
internal static readonly @string debugCallUnsafePoint = "call not at safe point"u8;

internal static partial void debugCallV2();

internal static partial void debugCallPanicked(any val);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeˢ = "runtime."u8;

// debugCallCheck checks whether it is safe to inject a debugger
// function call with return PC pc. If not, it returns a string
// explaining why.
//
//go:nosplit
internal static @string debugCallCheck(uintptr pc) {
    // No user calls from the system stack.
    if (getg() != (~(~getg()).m).curg) {
        return debugCallSystemStack;
    }
    {
        var sp = sys.GetCallerSP(); if (!((~getg()).stack.lo < sp && sp <= (~getg()).stack.hi)) {
            // Fast syscalls (nanotime) and racecall switch to the
            // g0 stack without switching g. We can't safely make
            // a call in this state. (We can't even safely
            // systemstack.)
            return debugCallSystemStack;
        }
    }
    // Switch to the system stack to avoid overflowing the user
    // stack.
    @string ret = default!;
    systemstack(() => {
        var f = findfunc(pc);
        if (!f.valid()) {
            ret = debugCallUnknownFunc;
            return;
        }
        @string name = funcname(f);
        var exprᴛ1 = name;
        if (exprᴛ1 == "debugCall32"u8 || exprᴛ1 == "debugCall64"u8 || exprᴛ1 == "debugCall128"u8 || exprᴛ1 == "debugCall256"u8 || exprᴛ1 == "debugCall512"u8 || exprᴛ1 == "debugCall1024"u8 || exprᴛ1 == "debugCall2048"u8 || exprᴛ1 == "debugCall4096"u8 || exprᴛ1 == "debugCall8192"u8 || exprᴛ1 == "debugCall16384"u8 || exprᴛ1 == "debugCall32768"u8 || exprᴛ1 == "debugCall65536"u8) {
            return;
        }

        // These functions are allowed so that the debugger can initiate multiple function calls.
        // See: https://golang.org/cl/161137/
        // Disallow calls from the runtime. We could
        // potentially make this condition tighter (e.g., not
        // when locks are held), but there are enough tightly
        // coded sequences (e.g., defer handling) that it's
        // better to play it safe.
        {
            @string pfx = runtimeˢ; if (len(name) > len(pfx) && name.slice(0, len(pfx)) == pfx) {
                ret = debugCallRuntime;
                return;
            }
        }
        // Check that this isn't an unsafe-point.
        if (pc != f.entry()) {
            pc--;
        }
        var up = pcdatavalue(f, abi.PCDATA_UnsafePoint, pc);
        if (up != abi.UnsafePointSafe) {
            // Not at a safe point.
            ret = debugCallUnsafePoint;
        }
    });
    return ret;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string inconsistentLockedmˢ = "inconsistent lockedm"u8;

// debugCallWrap starts a new goroutine to run a debug call and blocks
// the calling goroutine. On the goroutine, it prepares to recover
// panics from the debug call, and then calls the call dispatching
// function at PC dispatch.
//
// This must be deeply nosplit because there are untyped values on the
// stack from debugCallV2.
//
//go:nosplit
internal static void debugCallWrap(uintptr dispatch) {
    uint32 lockedExt = default!;
    var callerpc = sys.GetCallerPC();
    var gp = getg();
    // Lock ourselves to the OS thread.
    //
    // Debuggers rely on us running on the same thread until we get to
    // dispatch the function they asked as to.
    //
    // We're going to transfer this to the new G we just created.
    lockOSThread();
    // Create a new goroutine to execute the call on. Run this on
    // the system stack to avoid growing our stack.
    var gpʗ1 = gp;
    systemstack(() => {
        // TODO(mknyszek): It would be nice to wrap these arguments in an allocated
        // closure and start the goroutine with that closure, but the compiler disallows
        // implicit closure allocation in the runtime.
        ref var fn = ref heap<Action>(out var Ꮡfn);
        fn = debugCallWrap1;
        var newg = newproc1(~Ꮡfn.Reinterpret<Action, ж<funcval>>(), gpʗ1, callerpc, false, waitReasonZero);
        var args = Ꮡ(new debugCallWrapArgs(
            dispatch: dispatch,
            callingG: gpʗ1
        ));
        newg.Value.param = @unsafe.Pointer.FromPinnedBox(args);
        // Transfer locked-ness to the new goroutine.
        // Save lock state to restore later.
        var mpΔ1 = gpʗ1.Value.m;
        if (mpΔ1 != (~gpʗ1).lockedm.ptr()) {
            @throw(inconsistentLockedmˢ);
        }
        // Save the external lock count and clear it so
        // that it can't be unlocked from the debug call.
        // Note: we already locked internally to the thread,
        // so if we were locked before we're still locked now.
        lockedExt = mpΔ1.Value.lockedExt;
        mpΔ1.Value.lockedExt = 0;
        mpΔ1.of(m.Ꮡlockedg).set(newg);
        newg.of(g.Ꮡlockedm).set(mpΔ1);
        gpʗ1.Value.lockedm = 0;
        // Mark the calling goroutine as being at an async
        // safe-point, since it has a few conservative frames
        // at the bottom of the stack. This also prevents
        // stack shrinks.
        gpʗ1.Value.asyncSafePoint = true;
        // Stash newg away so we can execute it below (mcall's
        // closure can't capture anything).
        gpʗ1.of(g.Ꮡschedlink).set(newg);
    });
    // Switch to the new goroutine.
    mcall((ж<g> gpΔ1) => {
        // Get newg.
        var newg = (~gpΔ1).schedlink.ptr();
        gpΔ1.Value.schedlink = 0;
        // Park the calling goroutine.
        var Δtrace = traceAcquire();
        if (Δtrace.ok()) {
            // Trace the event before the transition. It may take a
            // stack trace, but we won't own the stack after the
            // transition anymore.
            Δtrace.GoPark(traceBlockDebugCall, 1);
        }
        casGToWaiting(gpΔ1, _Grunning, waitReasonDebugCall);
        if (Δtrace.ok()) {
            traceRelease(Δtrace);
        }
        dropg();
        // Directly execute the new goroutine. The debug
        // protocol will continue on the new goroutine, so
        // it's important we not just let the scheduler do
        // this or it may resume a different goroutine.
        execute(newg, true);
    });
    // We'll resume here when the call returns.
    // Restore locked state.
    var mp = gp.Value.m;
    mp.Value.lockedExt = lockedExt;
    mp.of(m.Ꮡlockedg).set(gp);
    gp.of(g.Ꮡlockedm).set(mp);
    // Undo the lockOSThread we did earlier.
    unlockOSThread();
    gp.Value.asyncSafePoint = false;
}

[GoType] partial struct debugCallWrapArgs {
    internal uintptr dispatch;
    internal ж<g> callingG;
}

// debugCallWrap1 is the continuation of debugCallWrap on the callee
// goroutine.
internal static void debugCallWrap1() {
    var gp = getg();
    var args = (ж<debugCallWrapArgs>)(uintptr)((~gp).param);
    var (dispatch, callingG) = (args.Value.dispatch, args.Value.callingG);
    gp.Value.param = default!;
    // Dispatch call and trap panics.
    debugCallWrap2(dispatch);
    // Resume the caller goroutine.
    getg().of(g.Ꮡschedlink).set(callingG);
    mcall((ж<g> gpΔ1) => {
        var callingGΔ1 = (~gpΔ1).schedlink.ptr();
        gpΔ1.Value.schedlink = 0;
        // Unlock this goroutine from the M if necessary. The
        // calling G will relock.
        if ((~gpΔ1).lockedm != 0) {
            gpΔ1.Value.lockedm = 0;
            gpΔ1.Value.m.Value.lockedg = 0;
        }
        // Switch back to the calling goroutine. At some point
        // the scheduler will schedule us again and we'll
        // finish exiting.
        var Δtrace = traceAcquire();
        if (Δtrace.ok()) {
            // Trace the event before the transition. It may take a
            // stack trace, but we won't own the stack after the
            // transition anymore.
            Δtrace.GoSched();
        }
        casgstatus(gpΔ1, _Grunning, _Grunnable);
        if (Δtrace.ok()) {
            traceRelease(Δtrace);
        }
        dropg();
        @lock(Ꮡsched.of(schedt.Ꮡlock));
        globrunqput(gpΔ1);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        Δtrace = traceAcquire();
        casgstatus(callingGΔ1, _Gwaiting, _Grunnable);
        if (Δtrace.ok()) {
            Δtrace.GoUnpark(callingGΔ1, 0);
            traceRelease(Δtrace);
        }
        execute(callingGΔ1, true);
    });
}

internal static void debugCallWrap2(uintptr dispatch) {
    GoFrame ᒐ = default;
    try {
        // Call the dispatch function and trap panics.
        ref var dispatchF = ref heap<Action>(out var ᏑdispatchF);
        ref var dispatchFV = ref heap<funcval>(out var ᏑdispatchFV);
        dispatchFV = new funcval(dispatch);
        (Ꮡ(new @unsafe.Pointer((uintptr)ᏑdispatchF))).Value = (uintptr)noescape(@unsafe.Pointer.FromPinnedBox(ᏑdispatchFV));
        bool ok = default!;
        defer(() => {
            if (!ok) {
                var err = recover();
                debugCallPanicked(err);
            }
        }, ref ᒐ);
        dispatchF();
        ok = true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_package
