// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using bytealg = @internal.bytealg_package;
using goarch = @internal.goarch_package;
using stringslite = @internal.stringslite_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// The code in this file implements stack trace walking for all architectures.
// The most important fact about a given architecture is whether it uses a link register.
// On systems with link registers, the prologue for a non-leaf function stores the
// incoming value of LR at the bottom of the newly allocated stack frame.
// On systems without link registers (x86), the architecture pushes a return PC during
// the call instruction, so the return PC ends up above the stack frame.
// In this file, the return PC is always called LR, no matter how it was found.
internal const bool usesLR = /* sys.MinFrameSize > 0 */ false;

internal static UntypedInt tracebackInnerFrames => 50;
internal static UntypedInt tracebackOuterFrames => 50;

[GoType("num:uint8")] partial struct unwindFlags;

internal static unwindFlags unwindPrintErrors => /* 1 << iota */ 1;
internal static unwindFlags unwindSilentErrors => 2;
internal static unwindFlags unwindTrap => 4;
internal static unwindFlags unwindJumpStack => 8;

// An unwinder iterates the physical stack frames of a Go sack.
//
// Typical use of an unwinder looks like:
//
//	var u unwinder
//	for u.init(gp, 0); u.valid(); u.next() {
//		// ... use frame info in u ...
//	}
//
// Implementation note: This is carefully structured to be pointer-free because
// tracebacks happen in places that disallow write barriers (e.g., signals).
// Even if this is stack-allocated, its pointer-receiver methods don't know that
// their receiver is on the stack, so they still emit write barriers. Here we
// address that by carefully avoiding any pointers in this type. Another
// approach would be to split this into a mutable part that's passed by pointer
// but contains no pointers itself and an immutable part that's passed and
// returned by value and can contain pointers. We could potentially hide that
// we're doing that in trivial methods that are inlined into the caller that has
// the stack allocation, but that's fragile.
[GoType] partial struct unwinder {
    // frame is the current physical stack frame, or all 0s if
    // there is no frame.
    internal stkframe frame;
    // g is the G who's stack is being unwound. If the
    // unwindJumpStack flag is set and the unwinder jumps stacks,
    // this will be different from the initial G.
    internal Δguintptr g;
    // cgoCtxt is the index into g.cgoCtxt of the next frame on the cgo stack.
    // The cgo stack is unwound in tandem with the Go stack as we find marker frames.
    internal nint cgoCtxt;
    // calleeFuncID is the function ID of the caller of the current
    // frame.
    internal abi.FuncID calleeFuncID;
    // flags are the flags to this unwind. Some of these are updated as we
    // unwind (see the flags documentation).
    internal unwindFlags flags;
}

// init initializes u to start unwinding gp's stack and positions the
// iterator on gp's innermost frame. gp must not be the current G.
//
// A single unwinder can be reused for multiple unwinds.
internal static void init(this ж<unwinder> Ꮡu, ж<g> Ꮡgp, unwindFlags flags) {
    // Implementation note: This starts the iterator on the first frame and we
    // provide a "valid" method. Alternatively, this could start in a "before
    // the first frame" state and "next" could return whether it was able to
    // move to the next frame, but that's both more awkward to use in a "for"
    // loop and is harder to implement because we have to do things differently
    // for the first frame.
    Ꮡu.initAt(~(uintptr)0, ~(uintptr)0, ~(uintptr)0, Ꮡgp, flags);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotTraceUserGoroutineˢ = "cannot trace user goroutine on its own stack"u8;
internal static readonly @string unknownPcˢ = "unknown pc"u8;

internal static void initAt(this ж<unwinder> Ꮡu, uintptr pc0, uintptr sp0, uintptr lr0, ж<g> Ꮡgp, unwindFlags flags) {
    ref var u = ref Ꮡu.DerefOrNull();
    ref var gp = ref Ꮡgp.DerefOrNull();

    // Don't call this "g"; it's too easy get "g" and "gp" confused.
    {
        var ourg = getg(); if (ourg == Ꮡgp && ourg == (~(~ourg).m).curg) {
            // The starting sp has been passed in as a uintptr, and the caller may
            // have other uintptr-typed stack references as well.
            // If during one of the calls that got us here or during one of the
            // callbacks below the stack must be grown, all these uintptr references
            // to the stack will not be updated, and traceback will continue
            // to inspect the old stack memory, which may no longer be valid.
            // Even if all the variables were updated correctly, it is not clear that
            // we want to expose a traceback that begins on one stack and ends
            // on another stack. That could confuse callers quite a bit.
            // Instead, we require that initAt and any other function that
            // accepts an sp for the current goroutine (typically obtained by
            // calling getcallersp) must not run on that goroutine's stack but
            // instead on the g0 stack.
            @throw(cannotTraceUserGoroutineˢ);
        }
    }
    if (pc0 == ~(uintptr)0 && sp0 == ~(uintptr)0) {
        // Signal to fetch saved values from gp.
        if (gp.syscallsp != 0){
            pc0 = gp.syscallpc;
            sp0 = gp.syscallsp;
            if (usesLR) {
                lr0 = 0;
            }
        } else {
            pc0 = gp.sched.pc;
            sp0 = gp.sched.sp;
            if (usesLR) {
                lr0 = gp.sched.lr;
            }
        }
    }
    ref var frame = ref heap(new stkframe(), out var Ꮡframe);
    frame.pc = pc0;
    frame.sp = sp0;
    if (usesLR) {
        frame.lr = lr0;
    }
    // If the PC is zero, it's likely a nil function call.
    // Start in the caller's frame.
    if (frame.pc == 0) {
        if (usesLR){
            frame.pc = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)frame.sp);
            frame.lr = 0;
        } else {
            frame.pc = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)frame.sp);
            frame.sp += goarch.PtrSize;
        }
    }
    // internal/runtime/atomic functions call into kernel helpers on
    // arm < 7. See internal/runtime/atomic/sys_linux_arm.s.
    //
    // Start in the caller's frame.
    if (GOARCH == "arm"u8 && goarm < 7 && GOOS == "linux"u8 && (uintptr)(frame.pc & (uintptr)0xffff0000U) == 0xffff0000U) {
        // Note that the calls are simple BL without pushing the return
        // address, so we use LR directly.
        //
        // The kernel helpers are frameless leaf functions, so SP and
        // LR are not touched.
        frame.pc = frame.lr;
        frame.lr = 0;
    }
    var f = findfunc(frame.pc);
    if (!f.valid()) {
        if ((unwindFlags)(flags & unwindSilentErrors) == 0) {
            print((@string)"runtime: g "u8, gp.goid, (@string)" gp="u8, Ꮡgp.OrTypedNil(), (@string)": unknown pc "u8, ((Δhex)(uint64)frame.pc), (@string)"\n"u8);
            tracebackHexdump(gp.stack, Ꮡframe, 0);
        }
        if ((unwindFlags)(flags & ((unwindFlags)(unwindPrintErrors | unwindSilentErrors))) == 0) {
            @throw(unknownPcˢ);
        }
        u = new unwinder(nil);
        return;
    }
    frame.fn = f;
    // Populate the unwinder.
    u = new unwinder(
        frame: frame,
        g: Ꮡgp.guintptr(),
        cgoCtxt: len(gp.cgoCtxt) - 1,
        calleeFuncID: abi.FuncIDNormal,
        flags: flags
    );
    var isSyscall = frame.pc == pc0 && frame.sp == sp0 && pc0 == gp.syscallpc && sp0 == gp.syscallsp;
    Ꮡu.resolveInternal(true, isSyscall);
}

[GoRecv] internal static bool valid(this ref unwinder u) {
    return u.frame.pc != 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tracebackˢ = "traceback"u8;

// resolveInternal fills in u.frame based on u.frame.fn, pc, and sp.
//
// innermost indicates that this is the first resolve on this stack. If
// innermost is set, isSyscall indicates that the PC/SP was retrieved from
// gp.syscall*; this is otherwise ignored.
//
// On entry, u.frame contains:
//   - fn is the running function.
//   - pc is the PC in the running function.
//   - sp is the stack pointer at that program counter.
//   - For the innermost frame on LR machines, lr is the program counter that called fn.
//
// On return, u.frame contains:
//   - fp is the stack pointer of the caller.
//   - lr is the program counter that called fn.
//   - varp, argp, and continpc are populated for the current frame.
//
// If fn is a stack-jumping function, resolveInternal can change the entire
// frame state to follow that stack jump.
//
// This is internal to unwinder.
internal static void resolveInternal(this ж<unwinder> Ꮡu, bool innermost, bool isSyscall) {
    ref var u = ref Ꮡu.DerefOrNull();

    var frame = Ꮡu.of(unwinder.Ꮡframe);
    var gp = u.g.ptr();
    var f = frame.Value.fn;
    if (f.pcsp == 0) {
        // No frame information, must be external function, like race support.
        // See golang.org/issue/13568.
        u.finishInternal();
        return;
    }
    // Compute function info flags.
    var flag = f.flag;
    if (f.funcID == abi.FuncID_cgocallback) {
        // cgocallback does write SP to switch from the g0 to the curg stack,
        // but it carefully arranges that during the transition BOTH stacks
        // have cgocallback frame valid for unwinding through.
        // So we don't need to exclude it with the other SP-writing functions.
        flag &= unchecked((abi.FuncFlag)~(abi.FuncFlag)(abi.FuncFlagSPWrite));
    }
    if (isSyscall) {
        // Some Syscall functions write to SP, but they do so only after
        // saving the entry PC/SP using entersyscall.
        // Since we are using the entry PC/SP, the later SP write doesn't matter.
        flag &= unchecked((abi.FuncFlag)~(abi.FuncFlag)(abi.FuncFlagSPWrite));
    }
    // Found an actual function.
    // Derive frame pointer.
    if ((~frame).fp == 0) {
        // Jump over system stack transitions. If we're on g0 and there's a user
        // goroutine, try to jump. Otherwise this is a regular call.
        // We also defensively check that this won't switch M's on us,
        // which could happen at critical points in the scheduler.
        // This ensures gp.m doesn't change from a stack jump.
        if ((unwindFlags)(u.flags & unwindJumpStack) != 0 && gp == (~(~gp).m).g0 && (~(~gp).m).curg != nil && (~(~(~gp).m).curg).m == (~gp).m) {
            var exprᴛ1 = f.funcID;
            if (exprᴛ1 == abi.FuncID_morestack) {
                gp = gp.Value.m.Value.curg;
                u.g.set(gp);
                frame.Value.pc = gp.Value.sched.pc;
                frame.Value.fn = findfunc((~frame).pc);
                f = frame.Value.fn;
                flag = f.flag;
                frame.Value.lr = gp.Value.sched.lr;
                frame.Value.sp = gp.Value.sched.sp;
                u.cgoCtxt = len((~gp).cgoCtxt) - 1;
            }
            else if (exprᴛ1 == abi.FuncID_systemstack) {
                do {
                    if (usesLR && funcspdelta(f, // morestack does not return normally -- newstack()
 // gogo's to curg.sched. Match that.
 // This keeps morestack() from showing up in the backtrace,
 // but that makes some sense since it'll never be returned
 // to.
 // systemstack returns normally, so just follow the
 // stack transition.
 (~frame).pc) == 0) {
                        // We're at the function prologue and the stack
                        // switch hasn't happened, or epilogue where we're
                        // about to return. Just unwind normally.
                        // Do this only on LR machines because on x86
                        // systemstack doesn't have an SP delta (the CALL
                        // instruction opens the frame), therefore no way
                        // to check.
                        flag &= unchecked((abi.FuncFlag)~(abi.FuncFlag)(abi.FuncFlagSPWrite));
                        break;
                    }
                    gp = gp.Value.m.Value.curg;
                    u.g.set(gp);
                    frame.Value.sp = gp.Value.sched.sp;
                    u.cgoCtxt = len((~gp).cgoCtxt) - 1;
                    flag &= unchecked((abi.FuncFlag)~(abi.FuncFlag)(abi.FuncFlagSPWrite));
                } while (false);
            }

        }
        frame.Value.fp = (~frame).sp + (uintptr)funcspdelta(f, (~frame).pc);
        if (!usesLR) {
            // On x86, call instruction pushes return PC before entering new function.
            frame.Value.fp += goarch.PtrSize;
        }
    }
    // Derive link register.
    if ((abi.FuncFlag)(flag & abi.FuncFlagTopFrame) != 0){
        // This function marks the top of the stack. Stop the traceback.
        frame.Value.lr = 0;
    } else 
    if ((abi.FuncFlag)(flag & abi.FuncFlagSPWrite) != 0 && (!innermost || (unwindFlags)(u.flags & ((unwindFlags)(unwindPrintErrors | unwindSilentErrors))) != 0)){
        // The function we are in does a write to SP that we don't know
        // how to encode in the spdelta table. Examples include context
        // switch routines like runtime.gogo but also any code that switches
        // to the g0 stack to run host C code.
        // We can't reliably unwind the SP (we might not even be on
        // the stack we think we are), so stop the traceback here.
        //
        // The one exception (encoded in the complex condition above) is that
        // we assume if we're doing a precise traceback, and this is the
        // innermost frame, that the SPWRITE function voluntarily preempted itself on entry
        // during the stack growth check. In that case, the function has
        // not yet had a chance to do any writes to SP and is safe to unwind.
        // isAsyncSafePoint does not allow assembly functions to be async preempted,
        // and preemptPark double-checks that SPWRITE functions are not async preempted.
        // So for GC stack traversal, we can safely ignore SPWRITE for the innermost frame,
        // but farther up the stack we'd better not find any.
        // This is somewhat imprecise because we're just guessing that we're in the stack
        // growth check. It would be better if SPWRITE were encoded in the spdelta
        // table so we would know for sure that we were still in safe code.
        //
        // uSE uPE inn | action
        //  T   _   _  | frame.lr = 0
        //  F   T   _  | frame.lr = 0
        //  F   F   F  | print; panic
        //  F   F   T  | ignore SPWrite
        if ((unwindFlags)(u.flags & ((unwindFlags)(unwindPrintErrors | unwindSilentErrors))) == 0 && !innermost) {
            println((@string)"traceback: unexpected SPWRITE function"u8, funcname(f));
            @throw(tracebackˢ);
        }
        frame.Value.lr = 0;
    } else {
        uintptr lrPtr = default!;
        if (usesLR){
            if (innermost && (~frame).sp < (~frame).fp || (~frame).lr == 0) {
                lrPtr = frame.Value.sp;
                frame.Value.lr = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)lrPtr);
            }
        } else {
            if ((~frame).lr == 0) {
                lrPtr = (~frame).fp - (uintptr)goarch.PtrSize;
                frame.Value.lr = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)lrPtr);
            }
        }
    }
    frame.Value.varp = frame.Value.fp;
    if (!usesLR) {
        // On x86, call instruction pushes return PC before entering new function.
        frame.Value.varp -= goarch.PtrSize;
    }
    // For architectures with frame pointers, if there's
    // a frame, then there's a saved frame pointer here.
    //
    // NOTE: This code is not as general as it looks.
    // On x86, the ABI is to save the frame pointer word at the
    // top of the stack frame, so we have to back down over it.
    // On arm64, the frame pointer should be at the bottom of
    // the stack (with R29 (aka FP) = RSP), in which case we would
    // not want to do the subtraction here. But we started out without
    // any frame pointer, and when we wanted to add it, we didn't
    // want to break all the assembly doing direct writes to 8(RSP)
    // to set the first parameter to a called function.
    // So we decided to write the FP link *below* the stack pointer
    // (with R29 = RSP - 8 in Go functions).
    // This is technically ABI-compatible but not standard.
    // And it happens to end up mimicking the x86 layout.
    // Other architectures may make different decisions.
    if ((~frame).varp > (~frame).sp && framepointer_enabled) {
        frame.Value.varp -= goarch.PtrSize;
    }
    frame.Value.argp = (~frame).fp + (uintptr)sys.MinFrameSize;
    // Determine frame's 'continuation PC', where it can continue.
    // Normally this is the return address on the stack, but if sigpanic
    // is immediately below this function on the stack, then the frame
    // stopped executing due to a trap, and frame.pc is probably not
    // a safe point for looking up liveness information. In this panicking case,
    // the function either doesn't return at all (if it has no defers or if the
    // defers do not recover) or it returns from one of the calls to
    // deferproc a second time (if the corresponding deferred func recovers).
    // In the latter case, use a deferreturn call site as the continuation pc.
    frame.Value.continpc = frame.Value.pc;
    if (u.calleeFuncID == abi.FuncID_sigpanic) {
        if ((~frame).fn.deferreturn != 0){
            frame.Value.continpc = (~frame).fn.entry() + (uintptr)(~frame).fn.deferreturn + 1;
        } else {
            // Note: this may perhaps keep return variables alive longer than
            // strictly necessary, as we are using "function has a defer statement"
            // as a proxy for "function actually deferred something". It seems
            // to be a minor drawback. (We used to actually look through the
            // gp._defer for a defer corresponding to this function, but that
            // is hard to do with defer records on the stack during a stack copy.)
            // Note: the +1 is to offset the -1 that
            // stack.go:getStackMap does to back up a return
            // address make sure the pc is in the CALL instruction.
            frame.Value.continpc = 0;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownCallerPcˢ = "unknown caller pc"u8;
internal static readonly @string tracebackStuckˢ = "traceback stuck"u8;

internal static void next(this ж<unwinder> Ꮡu) {
    ref var u = ref Ꮡu.DerefOrNull();

    var frame = Ꮡu.of(unwinder.Ꮡframe);
    var f = frame.Value.fn;
    var gp = u.g.ptr();
    // Do not unwind past the bottom of the stack.
    if ((~frame).lr == 0) {
        u.finishInternal();
        return;
    }
    var flr = findfunc((~frame).lr);
    if (!flr.valid()) {
        // This happens if you get a profiling interrupt at just the wrong time.
        // In that context it is okay to stop early.
        // But if no error flags are set, we're doing a garbage collection and must
        // get everything, so crash loudly.
        var fail = (unwindFlags)(u.flags & ((unwindFlags)(unwindPrintErrors | unwindSilentErrors))) == 0;
        var doPrint = (unwindFlags)(u.flags & unwindSilentErrors) == 0;
        if (doPrint && (~(~gp).m).incgo && f.funcID == abi.FuncID_sigpanic) {
            // We can inject sigpanic
            // calls directly into C code,
            // in which case we'll see a C
            // return PC. Don't complain.
            doPrint = false;
        }
        if (fail || doPrint) {
            print((@string)"runtime: g "u8, (~gp).goid, (@string)": unexpected return pc for "u8, funcname(f), (@string)" called from "u8, ((Δhex)(uint64)(~frame).lr), (@string)"\n"u8);
            tracebackHexdump((~gp).stack, frame, 0);
        }
        if (fail) {
            @throw(unknownCallerPcˢ);
        }
        frame.Value.lr = 0;
        u.finishInternal();
        return;
    }
    if ((~frame).pc == (~frame).lr && (~frame).sp == (~frame).fp) {
        // If the next frame is identical to the current frame, we cannot make progress.
        print((@string)"runtime: traceback stuck. pc="u8, ((Δhex)(uint64)(~frame).pc), (@string)" sp="u8, ((Δhex)(uint64)(~frame).sp), (@string)"\n"u8);
        tracebackHexdump((~gp).stack, frame, (~frame).sp);
        @throw(tracebackStuckˢ);
    }
    var injectedCall = f.funcID == abi.FuncID_sigpanic || f.funcID == abi.FuncID_asyncPreempt || f.funcID == abi.FuncID_debugCallV2;
    if (injectedCall){
        u.flags |= (unwindFlags)(unwindTrap);
    } else {
        u.flags &= unchecked((unwindFlags)~(unwindFlags)(unwindTrap));
    }
    // Unwind to next frame.
    u.calleeFuncID = f.funcID;
    frame.Value.fn = flr;
    frame.Value.pc = frame.Value.lr;
    frame.Value.lr = 0;
    frame.Value.sp = frame.Value.fp;
    frame.Value.fp = 0;
    // On link register architectures, sighandler saves the LR on stack
    // before faking a call.
    if (usesLR && injectedCall) {
        var x = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)(~frame).sp);
        frame.Value.sp += alignUp(sys.MinFrameSize, sys.StackAlign);
        f = findfunc((~frame).pc);
        frame.Value.fn = f;
        if (!f.valid()){
            frame.Value.pc = x;
        } else 
        if (funcspdelta(f, (~frame).pc) == 0) {
            frame.Value.lr = x;
        }
    }
    Ꮡu.resolveInternal(false, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tracebackDidNotUnwindˢ = "traceback did not unwind completely"u8;

// finishInternal is an unwinder-internal helper called after the stack has been
// exhausted. It sets the unwinder to an invalid state and checks that it
// successfully unwound the entire stack.
[GoRecv] internal static void finishInternal(this ref unwinder u) {
    u.frame.pc = 0;
    // Note that panic != nil is okay here: there can be leftover panics,
    // because the defers on the panic stack do not nest in frame order as
    // they do on the defer stack. If you have:
    //
    //	frame 1 defers d1
    //	frame 2 defers d2
    //	frame 3 defers d3
    //	frame 4 panics
    //	frame 4's panic starts running defers
    //	frame 5, running d3, defers d4
    //	frame 5 panics
    //	frame 5's panic starts running defers
    //	frame 6, running d4, garbage collects
    //	frame 6, running d2, garbage collects
    //
    // During the execution of d4, the panic stack is d4 -> d3, which
    // is nested properly, and we'll treat frame 3 as resumable, because we
    // can find d3. (And in fact frame 3 is resumable. If d4 recovers
    // and frame 5 continues running, d3, d3 can recover and we'll
    // resume execution in (returning from) frame 3.)
    //
    // During the execution of d2, however, the panic stack is d2 -> d3,
    // which is inverted. The scan will match d2 to frame 2 but having
    // d2 on the stack until then means it will not match d3 to frame 3.
    // This is okay: if we're running d2, then all the defers after d2 have
    // completed and their corresponding frames are dead. Not finding d3
    // for frame 3 means we'll set frame 3's continpc == 0, which is correct
    // (frame 3 is dead). At the end of the walk the panic stack can thus
    // contain defers (d3 in this case) for dead frames. The inversion here
    // always indicates a dead frame, and the effect of the inversion on the
    // scan is to hide those dead frames, so the scan is still okay:
    // what's left on the panic stack are exactly (and only) the dead frames.
    //
    // We require callback != nil here because only when callback != nil
    // do we know that gentraceback is being called in a "must be correct"
    // context as opposed to a "best effort" context. The tracebacks with
    // callbacks only happen when everything is stopped nicely.
    // At other times, such as when gathering a stack for a profiling signal
    // or when printing a traceback during a crash, everything may not be
    // stopped nicely, and the stack walk may not be able to complete.
    var gp = u.g.ptr();
    if ((unwindFlags)(u.flags & ((unwindFlags)(unwindPrintErrors | unwindSilentErrors))) == 0 && u.frame.sp != (~gp).stktopsp) {
        print((@string)"runtime: g"u8, (~gp).goid, (@string)": frame.sp="u8, ((Δhex)(uint64)u.frame.sp), (@string)" top="u8, ((Δhex)(uint64)(~gp).stktopsp), (@string)"\n"u8);
        print((@string)"\tstack=["u8, ((Δhex)(uint64)(~gp).stack.lo), (@string)"-"u8, ((Δhex)(uint64)(~gp).stack.hi), (@string)"\n"u8);
        @throw(tracebackDidNotUnwindˢ);
    }
}

// symPC returns the PC that should be used for symbolizing the current frame.
// Specifically, this is the PC of the last instruction executed in this frame.
//
// If this frame did a normal call, then frame.pc is a return PC, so this will
// return frame.pc-1, which points into the CALL instruction. If the frame was
// interrupted by a signal (e.g., profiler, segv, etc) then frame.pc is for the
// trapped instruction, so this returns frame.pc. See issue #34123. Finally,
// frame.pc can be at function entry when the frame is initialized without
// actually running code, like in runtime.mstart, in which case this returns
// frame.pc because that's the best we can do.
[GoRecv] internal static uintptr symPC(this ref unwinder u) {
    if ((unwindFlags)(u.flags & unwindTrap) == 0 && u.frame.pc > u.frame.fn.entry()) {
        // Regular call.
        return u.frame.pc - 1;
    }
    // Trapping instruction or we're at the function entry point.
    return u.frame.pc;
}

// cgoCallers populates pcBuf with the cgo callers of the current frame using
// the registered cgo unwinder. It returns the number of PCs written to pcBuf.
// If the current frame is not a cgo frame or if there's no registered cgo
// unwinder, it returns 0.
[GoRecv] internal static nint cgoCallers(this ref unwinder u, slice<uintptr> pcBuf) {
    if (cgoTraceback == nil || u.frame.fn.funcID != abi.FuncID_cgocallback || u.cgoCtxt < 0) {
        // We don't have a cgo unwinder (typical case), or we do but we're not
        // in a cgo frame or we're out of cgo context.
        return 0;
    }
    var ctxt = (~u.g.ptr()).cgoCtxt[u.cgoCtxt];
    u.cgoCtxt--;
    cgoContextPCs(ctxt, pcBuf);
    foreach (var (i, pc) in pcBuf) {
        if (pc == 0) {
            return i;
        }
    }
    return len(pcBuf);
}

// tracebackPCs populates pcBuf with the return addresses for each frame from u
// and returns the number of PCs written to pcBuf. The returned PCs correspond
// to "logical frames" rather than "physical frames"; that is if A is inlined
// into B, this will still return a PCs for both A and B. This also includes PCs
// generated by the cgo unwinder, if one is registered.
//
// If skip != 0, this skips this many logical frames.
//
// Callers should set the unwindSilentErrors flag on u.
internal static nint tracebackPCs(ж<unwinder> Ꮡu, nint skip, slice<uintptr> pcBuf) {
    ref var u = ref Ꮡu.DerefOrNull();

    array<uintptr> cgoBuf = new(32);
    nint n = 0;
    for (; n < len(pcBuf) && u.valid(); Ꮡu.next()) {
        var f = u.frame.fn;
        nint cgoN = u.cgoCallers(cgoBuf[..]);
        // TODO: Why does &u.cache cause u to escape? (Same in traceback2)
        for (var (iu, uf) = newInlineUnwinder(f, u.symPC()); n < len(pcBuf) && uf.valid(); uf = iu.next(uf)) {
            var sf = iu.srcFunc(uf);
            if (sf.funcID == abi.FuncIDWrapper && elideWrapperCalling(u.calleeFuncID)){
            } else 
            if (skip > 0){
                // ignore wrappers
                skip--;
            } else {
                // Callers expect the pc buffer to contain return addresses
                // and do the -1 themselves, so we add 1 to the call pc to
                // create a "return pc". Since there is no actual call, here
                // "return pc" just means a pc you subtract 1 from to get
                // the pc of the "call". The actual no-op we insert may or
                // may not be 1 byte.
                pcBuf[n] = uf.pc + 1;
                n++;
            }
            u.calleeFuncID = sf.funcID;
        }
        // Add cgo frames (if we're done skipping over the requested number of
        // Go frames).
        if (skip == 0) {
            n += copy(pcBuf[(int)(n)..], cgoBuf[..(int)(cgoN)]);
        }
    }
    return n;
}

// printArgs prints function arguments in traceback.
internal static void printArgs(ΔfuncInfo f, @unsafe.Pointer argp, uintptr pc) {
    var Δp = (ж<array<uint8>>)(uintptr)(funcdata(f, abi.FUNCDATA_ArgInfo));
    if (Δp == nil) {
        return;
    }
    @unsafe.Pointer liveInfo = (uintptr)funcdata(f, abi.FUNCDATA_ArgLiveInfo);
    var liveIdx = pcdatavalue(f, abi.PCDATA_ArgLiveIndex, pc);
    var startOffset = (uint8)0xff; // smallest offset that needs liveness info (slots with a lower offset is always live)
    if (liveInfo != nil) {
        startOffset = ~(ж<uint8>)(uintptr)(liveInfo);
    }
    bool isLive(uint8 off, uint8 slotIdxΔ1) {
        if (liveInfo == nil || liveIdx <= 0) {
            return true; // no liveness info, always live
        }
        if (off < startOffset) {
            return true;
        }
        var bits = ~(ж<uint8>)(uintptr)(add(liveInfo, (uintptr)liveIdx + (uintptr)(slotIdxΔ1 / 8)));
        return (uint8)(bits & ((uint8)(1 << (int)((slotIdxΔ1 % 8))))) != 0;
    }
    var isLiveʗ1 = isLive;
    void print1(uint8 off, uint8 sz, uint8 slotIdxΔ2) {
        var x = readUnaligned64((uintptr)add(argp, (uintptr)off));
        // mask out irrelevant bits
        if (sz < 8) {
            var shift = (uint8)(64 - sz * 8);
            if (goarch.BigEndian){
                x = x.Rsh((uint64)(shift));
            } else {
                x = (x.Lsh((uint64)(shift))).Rsh((uint64)(shift));
            }
        }
        print(((Δhex)x));
        if (!isLiveʗ1(off, slotIdxΔ2)) {
            print((@string)"?"u8);
        }
    }
    var start = true;
    void printcomma() {
        if (!start) {
            print((@string)", "u8);
        }
    }
    nint pi = 0;
    var slotIdx = (uint8)0; // register arg spill slot index
printloop:
    while (ᐧ) {
        var o = Δp.Value[pi];
        pi++;
        var exprᴛ1 = o;
        if (exprᴛ1 == abi.TraceArgsEndSeq) {
            goto break_printloop;
        }
        else if (exprᴛ1 == abi.TraceArgsStartAgg) {
            printcomma();
            print((@string)"{"u8);
            start = true;
            continue;
        }
        else if (exprᴛ1 == abi.TraceArgsEndAgg) {
            print((@string)"}"u8);
        }
        else if (exprᴛ1 == abi.TraceArgsDotdotdot) {
            printcomma();
            print((@string)"..."u8);
        }
        else if (exprᴛ1 == abi.TraceArgsOffsetTooLarge) {
            printcomma();
            print((@string)"_"u8);
        }
        else { /* default: */
            printcomma();
            var sz = Δp.Value[pi];
            pi++;
            print1(o, sz, slotIdx);
            if (o >= startOffset) {
                slotIdx++;
            }
        }

        start = false;
continue_printloop:;
    }
break_printloop:;
}

// funcNamePiecesForPrint returns the function name for printing to the user.
// It returns three pieces so it doesn't need an allocation for string
// concatenation.
internal static (@string, @string, @string) funcNamePiecesForPrint(@string name) {
    // Replace the shape name in generic function with "...".
    nint i = bytealg.IndexByteString(name, (rune)'[');
    if (i < 0) {
        return (name, "", "");
    }
    nint j = len(name) - 1;
    while (name[j] != (rune)']') {
        j--;
    }
    if (j <= i) {
        return (name, "", "");
    }
    return (name[..(int)(i)], "[...]", name[(int)(j + 1)..]);
}

// funcNameForPrint returns the function name for printing to the user.
internal static @string funcNameForPrint(@string name) {
    var (a, b, c) = funcNamePiecesForPrint(name);
    return a + b + c;
}

// printFuncName prints a function name. name is the function name in
// the binary's func data table.
internal static void printFuncName(@string name) {
    if (name == "runtime.gopanic"u8) {
        print((@string)"panic"u8);
        return;
    }
    var (a, b, c) = funcNamePiecesForPrint(name);
    print(a, b, c);
}

internal static void printcreatedby(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // Show what created goroutine, except main goroutine (goid 1).
    var pc = gp.gopc;
    var f = findfunc(pc);
    if (f.valid() && showframe(f.srcFunc(), Ꮡgp, false, abi.FuncIDNormal) && gp.goid != 1) {
        printcreatedby1(f, pc, gp.parentGoid);
    }
}

internal static void printcreatedby1(ΔfuncInfo f, uintptr pc, uint64 goid) {
    print((@string)"created by "u8);
    printFuncName(funcname(f));
    if (goid != 0) {
        print((@string)" in goroutine "u8, goid);
    }
    print((@string)"\n"u8);
    var tracepc = pc; // back up to CALL instruction for funcline.
    if (pc > f.entry()) {
        tracepc -= sys.PCQuantum;
    }
    var (@file, line) = funcline(f, tracepc);
    print((@string)"\t"u8, @file, (@string)":"u8, line);
    if (pc > f.entry()) {
        print((@string)" +"u8, ((Δhex)(uint64)(pc - f.entry())));
    }
    print((@string)"\n"u8);
}

internal static void traceback(uintptr pc, uintptr sp, uintptr lr, ж<g> Ꮡgp) {
    traceback1(pc, sp, lr, Ꮡgp, 0);
}

// tracebacktrap is like traceback but expects that the PC and SP were obtained
// from a trap, not from gp->sched or gp->syscallpc/gp->syscallsp or getcallerpc/getcallersp.
// Because they are from a trap instead of from a saved pair,
// the initial PC must not be rewound to the previous instruction.
// (All the saved pairs record a PC that is a return address, so we
// rewind it into the CALL instruction.)
// If gp.m.libcall{g,pc,sp} information is available, it uses that information in preference to
// the pc/sp/lr passed in.
internal static void tracebacktrap(uintptr pc, uintptr sp, uintptr lr, ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    if ((~gp.m).libcallsp != 0) {
        // We're in C code somewhere, traceback from the saved position.
        traceback1((~gp.m).libcallpc, (~gp.m).libcallsp, 0, (~gp.m).libcallg.ptr(), 0);
        return;
    }
    traceback1(pc, sp, lr, Ꮡgp, unwindTrap);
}

internal static void traceback1(uintptr pc, uintptr sp, uintptr lr, ж<g> Ꮡgp, unwindFlags flags) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // If the goroutine is in cgo, and we have a cgo traceback, print that.
    if (iscgo && gp.m != nil && (~gp.m).ncgo > 0 && gp.syscallsp != 0 && (~gp.m).cgoCallers != nil && (~gp.m).cgoCallers.Value[0] != 0) {
        // Lock cgoCallers so that a signal handler won't
        // change it, copy the array, reset it, unlock it.
        // We are locked to the thread and are not running
        // concurrently with a signal handler.
        // We just have to stop a signal handler from interrupting
        // in the middle of our copy.
        gp.m.of(m.ᏑcgoCallersUse).Store(1);
        var ΔcgoCallers = (~gp.m).cgoCallers.Value.Clone();
        gp.m.Value.cgoCallers.Value[0] = 0;
        gp.m.of(m.ᏑcgoCallersUse).Store(0);
        printCgoTraceback(ref ΔcgoCallers);
    }
    if ((uint32)(readgstatus(Ꮡgp) & ~(uint32)_Gscan) == _Gsyscall) {
        // Override registers if blocked in system call.
        pc = gp.syscallpc;
        sp = gp.syscallsp;
        flags &= unchecked((unwindFlags)~(unwindFlags)(unwindTrap));
    }
    if (gp.m != nil && (~gp.m).vdsoSP != 0) {
        // Override registers if running in VDSO. This comes after the
        // _Gsyscall check to cover VDSO calls after entersyscall.
        pc = gp.m.Value.vdsoPC;
        sp = gp.m.Value.vdsoSP;
        flags &= unchecked((unwindFlags)~(unwindFlags)(unwindTrap));
    }
    // Print traceback.
    //
    // We print the first tracebackInnerFrames frames, and the last
    // tracebackOuterFrames frames. There are many possible approaches to this.
    // There are various complications to this:
    //
    // - We'd prefer to walk the stack once because in really bad situations
    //   traceback may crash (and we want as much output as possible) or the stack
    //   may be changing.
    //
    // - Each physical frame can represent several logical frames, so we might
    //   have to pause in the middle of a physical frame and pick up in the middle
    //   of a physical frame.
    //
    // - The cgo symbolizer can expand a cgo PC to more than one logical frame,
    //   and involves juggling state on the C side that we don't manage. Since its
    //   expansion state is managed on the C side, we can't capture the expansion
    //   state part way through, and because the output strings are managed on the
    //   C side, we can't capture the output. Thus, our only choice is to replay a
    //   whole expansion, potentially discarding some of it.
    //
    // Rejected approaches:
    //
    // - Do two passes where the first pass just counts and the second pass does
    //   all the printing. This is undesirable if the stack is corrupted or changing
    //   because we won't see a partial stack if we panic.
    //
    // - Keep a ring buffer of the last N logical frames and use this to print
    //   the bottom frames once we reach the end of the stack. This works, but
    //   requires keeping a surprising amount of state on the stack, and we have
    //   to run the cgo symbolizer twice—once to count frames, and a second to
    //   print them—since we can't retain the strings it returns.
    //
    // Instead, we print the outer frames, and if we reach that limit, we clone
    // the unwinder, count the remaining frames, and then skip forward and
    // finish printing from the clone. This makes two passes over the outer part
    // of the stack, but the single pass over the inner part ensures that's
    // printed immediately and not revisited. It keeps minimal state on the
    // stack. And through a combination of skip counts and limits, we can do all
    // of the steps we need with a single traceback printer implementation.
    //
    // We could be more lax about exactly how many frames we print, for example
    // always stopping and resuming on physical frame boundaries, or at least
    // cgo expansion boundaries. It's not clear that's much simpler.
    flags |= (unwindFlags)(unwindPrintErrors);
    ref var u = ref heap(new unwinder(), out var Ꮡu);
    nint tracebackWithRuntime(bool showRuntime) {
        const nint maxInt = 0x7fffffff;
        Ꮡu.initAt(pc, sp, lr, Ꮡgp, flags);
        var (n, lastN) = traceback2(Ꮡu, showRuntime, 0, tracebackInnerFrames);
        if (n < tracebackInnerFrames) {
            // We printed the whole stack.
            return n;
        }
        // Clone the unwinder and figure out how many frames are left. This
        // count will include any logical frames already printed for u's current
        // physical frame.
        ref var u2 = ref heap<unwinder>(out var Ꮡu2);
        u2 = Ꮡu.Value;
        var (remaining, _) = traceback2(Ꮡu, showRuntime, maxInt, 0);
        nint elide = remaining - lastN - (nint)tracebackOuterFrames;
        if (elide > 0){
            print((@string)"..."u8, elide, (@string)" frames elided...\n"u8);
            traceback2(Ꮡu2, showRuntime, lastN + elide, tracebackOuterFrames);
        } else 
        if (elide <= 0) {
            // There are tracebackOuterFrames or fewer frames left to print.
            // Just print the rest of the stack.
            traceback2(Ꮡu2, showRuntime, lastN, tracebackOuterFrames);
        }
        return n;
    }
    // By default, omits runtime frames. If that means we print nothing at all,
    // repeat forcing all frames printed.
    if (tracebackWithRuntime(false) == 0) {
        tracebackWithRuntime(true);
    }
    printcreatedby(Ꮡgp);
    if (gp.ancestors == nil) {
        return;
    }
    foreach (var (_, ancestor) in gp.ancestors.ValueSlot) {
        printAncestorTraceback(ancestor);
    }
}

// traceback2 prints a stack trace starting at u. It skips the first "skip"
// logical frames, after which it prints at most "max" logical frames. It
// returns n, which is the number of logical frames skipped and printed, and
// lastN, which is the number of logical frames skipped or printed just in the
// physical frame that u references.
internal static (nint n, nint lastN) traceback2(ж<unwinder> Ꮡu, bool showRuntime, nint skip, nint max) {
    nint n = default!;
    nint lastN = default!;

    ref var u = ref Ꮡu.DerefOrNull();
    // commitFrame commits to a logical frame and returns whether this frame
    // should be printed and whether iteration should stop.
    var commitFrame = () => {
        if (skip == 0 && max == 0) {
            // Stop
            return (false, true);
        }
        n++;
        lastN++;
        if (skip > 0) {
            // Skip
            skip--;
            return (false, false);
        }
        // Print
        max--;
        return (true, false);
    };
    var gp = u.g.ptr();
    var (level, _, _) = gotraceback();
    array<uintptr> cgoBuf = new(32);
    for (; u.valid(); Ꮡu.next()) {
        lastN = 0;
        var f = u.frame.fn;
        for (var (iu, uf) = newInlineUnwinder(f, u.symPC()); uf.valid(); uf = iu.next(uf)) {
            var sf = iu.srcFunc(uf);
            var callee = u.calleeFuncID;
            u.calleeFuncID = sf.funcID;
            if (!(showRuntime || showframe(sf, gp, n == 0, callee))) {
                continue;
            }
            {
                var (pr, stop) = commitFrame(); if (stop){
                    return (n, lastN);
                } else 
                if (!pr) {
                    continue;
                }
            }
            @string name = sf.name();
            var (@file, line) = iu.fileLine(uf);
            // Print during crash.
            //	main(0x1, 0x2, 0x3)
            //		/home/rsc/go/src/runtime/x.go:23 +0xf
            //
            printFuncName(name);
            print((@string)"("u8);
            if (iu.isInlined(uf)){
                print((@string)"..."u8);
            } else {
                @unsafe.Pointer argp = (@unsafe.Pointer)u.frame.argp;
                printArgs(f, argp, u.symPC());
            }
            print((@string)")\n"u8);
            print((@string)"\t"u8, @file, (@string)":"u8, line);
            if (!iu.isInlined(uf)) {
                if (u.frame.pc > f.entry()) {
                    print((@string)" +"u8, ((Δhex)(uint64)(u.frame.pc - f.entry())));
                }
                if ((~gp).m != nil && (~(~gp).m).throwing >= throwTypeRuntime && gp == (~(~gp).m).curg || level >= 2) {
                    print((@string)" fp="u8, ((Δhex)(uint64)u.frame.fp), (@string)" sp="u8, ((Δhex)(uint64)u.frame.sp), (@string)" pc="u8, ((Δhex)(uint64)u.frame.pc));
                }
            }
            print((@string)"\n"u8);
        }
        // Print cgo frames.
        {
            nint cgoN = u.cgoCallers(cgoBuf[..]); if (cgoN > 0) {
                ref var arg = ref heap(new cgoSymbolizerArg(), out var Ꮡarg);
                var anySymbolized = false;
                var stop = false;
                foreach (var (_, pc) in cgoBuf[..(int)(cgoN)]) {
                    if (cgoSymbolizer == nil){
                        {
                            var (pr, stopΔ1) = commitFrame(); if (stopΔ1){
                                break;
                            } else 
                            if (pr) {
                                print((@string)"non-Go function at pc="u8, ((Δhex)(uint64)pc), (@string)"\n"u8);
                            }
                        }
                    } else {
                        stop = printOneCgoTraceback(pc, commitFrame, Ꮡarg);
                        anySymbolized = true;
                        if (stop) {
                            break;
                        }
                    }
                }
                if (anySymbolized) {
                    // Free symbolization state.
                    arg.pc = 0;
                    callCgoSymbolizer(Ꮡarg);
                }
                if (stop) {
                    return (n, lastN);
                }
            }
        }
    }
    return (n, 0);
}

// printAncestorTraceback prints the traceback of the given ancestor.
// TODO: Unify this with gentraceback and CallersFrames.
internal static void printAncestorTraceback(ancestorInfo ancestor) {
    print((@string)"[originating from goroutine "u8, ancestor.goid, (@string)"]:\n"u8);
    foreach (var (fidx, pc) in ancestor.pcs) {
        var fΔ1 = findfunc(pc); // f previously validated
        if (showfuncinfo(fΔ1.srcFunc(), fidx == 0, abi.FuncIDNormal)) {
            printAncestorTracebackFuncInfo(fΔ1, pc);
        }
    }
    if (len(ancestor.pcs) == tracebackInnerFrames) {
        print((@string)"...additional frames elided...\n"u8);
    }
    // Show what created goroutine, except main goroutine (goid 1).
    var f = findfunc(ancestor.gopc);
    if (f.valid() && showfuncinfo(f.srcFunc(), false, abi.FuncIDNormal) && ancestor.goid != 1) {
        // In ancestor mode, we'll already print the goroutine ancestor.
        // Pass 0 for the goid parameter so we don't print it again.
        printcreatedby1(f, ancestor.gopc, 0);
    }
}

// printAncestorTracebackFuncInfo prints the given function info at a given pc
// within an ancestor traceback. The precision of this info is reduced
// due to only have access to the pcs at the time of the caller
// goroutine being created.
internal static void printAncestorTracebackFuncInfo(ΔfuncInfo f, uintptr pc) {
    var (u, uf) = newInlineUnwinder(f, pc);
    var (@file, line) = u.fileLine(uf);
    printFuncName(u.srcFunc(uf).name());
    print((@string)"(...)\n"u8);
    print((@string)"\t"u8, @file, (@string)":"u8, line);
    if (pc > f.entry()) {
        print((@string)" +"u8, ((Δhex)(uint64)(pc - f.entry())));
    }
    print((@string)"\n"u8);
}

// go2cs generated this placeholder — func callers is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static nint gcallers(ж<g> Ꮡgp, nint skip, slice<uintptr> pcbuf) {
    ref var u = ref heap(new unwinder(), out var Ꮡu);
    Ꮡu.init(Ꮡgp, unwindSilentErrors);
    return tracebackPCs(Ꮡu, skip, pcbuf);
}

// showframe reports whether the frame with the given characteristics should
// be printed during a traceback.
internal static bool showframe(ΔsrcFunc sf, ж<g> Ꮡgp, bool firstFrame, abi.FuncID calleeID) {
    var mp = getg().Value.m;
    if ((~mp).throwing >= throwTypeRuntime && Ꮡgp != nil && (Ꮡgp == (~mp).curg || Ꮡgp == (~mp).caughtsig.ptr())) {
        return true;
    }
    return showfuncinfo(sf, firstFrame, calleeID);
}

// showfuncinfo reports whether a function with the given characteristics should
// be printed during a traceback.
internal static bool showfuncinfo(ΔsrcFunc sf, bool firstFrame, abi.FuncID calleeID) {
    var (level, _, _) = gotraceback();
    if (level > 1) {
        // Show all frames.
        return true;
    }
    if (sf.funcID == abi.FuncIDWrapper && elideWrapperCalling(calleeID)) {
        return false;
    }
    @string name = sf.name();
    // Special case: always show runtime.gopanic frame
    // in the middle of a stack trace, so that we can
    // see the boundary between ordinary code and
    // panic-induced deferred code.
    // See golang.org/issue/5832.
    if (name == "runtime.gopanic"u8 && !firstFrame) {
        return true;
    }
    return bytealg.IndexByteString(name, (rune)'.') >= 0 && (!stringslite.HasPrefix(name, runtimeˢ) || isExportedRuntime(name));
}

// isExportedRuntime reports whether name is an exported runtime function.
// It is only for runtime functions, so ASCII A-Z is fine.
internal static bool isExportedRuntime(@string name) {
    // Check and remove package qualifier.
    nint n = len("runtime.");
    if (len(name) <= n || name[..(int)(n)] != "runtime.") {
        return false;
    }
    name = name[(int)(n)..];
    @string rcvr = ""u8;
    // Extract receiver type, if any.
    // For example, runtime.(*Func).Entry
    nint i = len(name) - 1;
    while (i >= 0 && name[i] != (rune)'.') {
        i--;
    }
    if (i >= 0) {
        rcvr = name[..(int)(i)];
        name = name[(int)(i + 1)..];
        // Remove parentheses and star for pointer receivers.
        if (len(rcvr) >= 3 && rcvr[0] == (rune)'(' && rcvr[1] == (rune)'*' && rcvr[len(rcvr) - 1] == (rune)')') {
            rcvr = rcvr[2..(int)(len(rcvr) - 1)];
        }
    }
    // Exported functions and exported methods on exported types.
    return len(name) > 0 && (rune)'A' <= name[0] && name[0] <= (rune)'Z' && (len(rcvr) == 0 || (rune)'A' <= rcvr[0] && rcvr[0] <= (rune)'Z');
}

// elideWrapperCalling reports whether a wrapper function that called
// function id should be elided from stack traces.
internal static bool elideWrapperCalling(abi.FuncID id) {
    // If the wrapper called a panic function instead of the
    // wrapped function, we want to include it in stacks.
    return !(id == abi.FuncID_gopanic || id == abi.FuncID_sigpanic || id == abi.FuncID_panicwrap);
}

internal static array<@string> gStatusStrings = new golib.SparseArray<@string>{
    [_Gidle] = "idle"u8,
    [_Grunnable] = "runnable"u8,
    [_Grunning] = "running"u8,
    [_Gsyscall] = "syscall"u8,
    [_Gwaiting] = "waiting"u8,
    [_Gdead] = "dead"u8,
    [_Gcopystack] = "copystack"u8,
    [_Gpreempted] = "preempted"u8
}.array(10);

internal static void goroutineheader(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    var (level, _, _) = gotraceback();
    var gpstatus = readgstatus(Ꮡgp);
    var isScan = (uint32)(gpstatus & (uint32)_Gscan) != 0;
    gpstatus &= unchecked((uint32)~(uint32)(_Gscan)); // drop the scan bit
    // Basic string status
    @string status = default!;
    if (0 <= gpstatus && gpstatus < (uint32)len(gStatusStrings)){
        status = gStatusStrings[(nint)(gpstatus)];
    } else {
        status = "???"u8;
    }
    // Override.
    if (gpstatus == _Gwaiting && gp.waitreason != waitReasonZero) {
        status = gp.waitreason.String();
    }
    // approx time the G is blocked, in minutes
    int64 waitfor = default!;
    if ((gpstatus == _Gwaiting || gpstatus == _Gsyscall) && gp.waitsince != 0) {
        waitfor = (nanotime() - gp.waitsince) / 60000000000;
    }
    print((@string)"goroutine "u8, gp.goid);
    if (gp.m != nil && (~gp.m).throwing >= throwTypeRuntime && Ꮡgp == (~gp.m).curg || level >= 2) {
        print((@string)" gp="u8, Ꮡgp.OrTypedNil());
        if (gp.m != nil){
            print((@string)" m="u8, (~gp.m).id, (@string)" mp="u8, gp.m.OrTypedNil());
        } else {
            print((@string)" m=nil"u8);
        }
    }
    print((@string)" ["u8, status);
    if (isScan) {
        print((@string)" (scan)"u8);
    }
    if (waitfor >= 1) {
        print((@string)", "u8, waitfor, (@string)" minutes"u8);
    }
    if (gp.lockedm != 0) {
        print((@string)", locked to thread"u8);
    }
    print((@string)"]:\n"u8);
}

internal static void tracebackothers(ж<g> Ꮡme) {
    var (level, _, _) = gotraceback();
    // Show the current goroutine first, if we haven't already.
    var curgp = getg().Value.m.Value.curg;
    if (curgp != nil && curgp != Ꮡme) {
        print((@string)"\n"u8);
        goroutineheader(curgp);
        traceback(~(uintptr)0, ~(uintptr)0, 0, curgp);
    }
    // We can't call locking forEachG here because this may be during fatal
    // throw/panic, where locking could be out-of-order or a direct
    // deadlock.
    //
    // Instead, use forEachGRace, which requires no locking. We don't lock
    // against concurrent creation of new Gs, but even with allglock we may
    // miss Gs created after this loop.
    var curgpʗ1 = curgp;
    forEachGRace((ж<g> gp) => {
        if (gp == Ꮡme || gp == curgpʗ1 || readgstatus(gp) == _Gdead || isSystemGoroutine(ref (gp).DerefOrNull(), false) && level < 2) {
            return;
        }
        print((@string)"\n"u8);
        goroutineheader(gp);
        // Note: gp.m == getg().m occurs when tracebackothers is called
        // from a signal handler initiated during a systemstack call.
        // The original G is still in the running state, and we want to
        // print its stack.
        if ((~gp).m != (~getg()).m && (uint32)(readgstatus(gp) & ~(uint32)_Gscan) == _Grunning){
            print((@string)"\tgoroutine running on other thread; stack unavailable\n"u8);
            printcreatedby(gp);
        } else {
            traceback(~(uintptr)0, ~(uintptr)0, 0, gp);
        }
    });
}

// tracebackHexdump hexdumps part of stk around frame.sp and frame.fp
// for debugging purposes. If the address bad is included in the
// hexdumped range, it will mark it as well.
internal static void tracebackHexdump(Δstack stk, ж<stkframe> Ꮡframe, uintptr bad) {
    ref var frame = ref Ꮡframe.DerefOrNull();

    uintptr expand = /* 32 * goarch.PtrSize */ 256;
    uintptr maxExpand = /* 256 * goarch.PtrSize */ 2048;
    // Start around frame.sp.
    var (lo, hi) = (frame.sp, frame.sp);
    // Expand to include frame.fp.
    if (frame.fp != 0 && frame.fp < lo) {
        lo = frame.fp;
    }
    if (frame.fp != 0 && frame.fp > hi) {
        hi = frame.fp;
    }
    // Expand a bit more.
    (lo, hi) = (lo - expand, hi + expand);
    // But don't go too far from frame.sp.
    if (lo < frame.sp - maxExpand) {
        lo = frame.sp - maxExpand;
    }
    if (hi > frame.sp + maxExpand) {
        hi = frame.sp + maxExpand;
    }
    // And don't go outside the stack bounds.
    if (lo < stk.lo) {
        lo = stk.lo;
    }
    if (hi > stk.hi) {
        hi = stk.hi;
    }
    // Print the hex dump.
    print((@string)"stack: frame={sp:"u8, ((Δhex)(uint64)frame.sp), (@string)", fp:"u8, ((Δhex)(uint64)frame.fp), (@string)"} stack=["u8, ((Δhex)(uint64)stk.lo), (@string)","u8, ((Δhex)(uint64)stk.hi), (@string)")\n"u8);
    hexdumpWords(lo, hi, (uintptr Δp) => {
        var exprᴛ1 = Δp;
        if (exprᴛ1 == Ꮡframe.Value.fp) {
            return (rune)'>';
        }
        if (exprᴛ1 == Ꮡframe.Value.sp) {
            return (rune)'<';
        }
        if (exprᴛ1 == bad) {
            return (rune)'!';
        }

        return 0;
    });
}

// isSystemGoroutine reports whether the goroutine g must be omitted
// in stack dumps and deadlock detector. This is any goroutine that
// starts at a runtime.* entry point, except for runtime.main,
// runtime.handleAsyncEvent (wasm only) and sometimes runtime.runfinq.
//
// If fixed is true, any goroutine that can vary between user and
// system (that is, the finalizer goroutine) is considered a user
// goroutine.
internal static bool isSystemGoroutine(ref g gp, bool @fixed) {
    // Keep this in sync with internal/trace.IsSystemGoroutine.
    var f = findfunc(gp.startpc);
    if (!f.valid()) {
        return false;
    }
    if (f.funcID == abi.FuncID_runtime_main || f.funcID == abi.FuncID_corostart || f.funcID == abi.FuncID_handleAsyncEvent) {
        return false;
    }
    if (f.funcID == abi.FuncID_runfinq) {
        // We include the finalizer goroutine if it's calling
        // back into user code.
        if (@fixed) {
            // This goroutine can vary. In fixed mode,
            // always consider it a user goroutine.
            return false;
        }
        return (uint32)(ᏑfingStatus.Load() & fingRunningFinalizer) == 0;
    }
    return stringslite.HasPrefix(funcname(f), runtimeˢ);
}

// SetCgoTraceback records three C functions to use to gather
// traceback information from C code and to convert that traceback
// information into symbolic information. These are used when printing
// stack traces for a program that uses cgo.
//
// The traceback and context functions may be called from a signal
// handler, and must therefore use only async-signal safe functions.
// The symbolizer function may be called while the program is
// crashing, and so must be cautious about using memory.  None of the
// functions may call back into Go.
//
// The context function will be called with a single argument, a
// pointer to a struct:
//
//	struct {
//		Context uintptr
//	}
//
// In C syntax, this struct will be
//
//	struct {
//		uintptr_t Context;
//	};
//
// If the Context field is 0, the context function is being called to
// record the current traceback context. It should record in the
// Context field whatever information is needed about the current
// point of execution to later produce a stack trace, probably the
// stack pointer and PC. In this case the context function will be
// called from C code.
//
// If the Context field is not 0, then it is a value returned by a
// previous call to the context function. This case is called when the
// context is no longer needed; that is, when the Go code is returning
// to its C code caller. This permits the context function to release
// any associated resources.
//
// While it would be correct for the context function to record a
// complete a stack trace whenever it is called, and simply copy that
// out in the traceback function, in a typical program the context
// function will be called many times without ever recording a
// traceback for that context. Recording a complete stack trace in a
// call to the context function is likely to be inefficient.
//
// The traceback function will be called with a single argument, a
// pointer to a struct:
//
//	struct {
//		Context    uintptr
//		SigContext uintptr
//		Buf        *uintptr
//		Max        uintptr
//	}
//
// In C syntax, this struct will be
//
//	struct {
//		uintptr_t  Context;
//		uintptr_t  SigContext;
//		uintptr_t* Buf;
//		uintptr_t  Max;
//	};
//
// The Context field will be zero to gather a traceback from the
// current program execution point. In this case, the traceback
// function will be called from C code.
//
// Otherwise Context will be a value previously returned by a call to
// the context function. The traceback function should gather a stack
// trace from that saved point in the program execution. The traceback
// function may be called from an execution thread other than the one
// that recorded the context, but only when the context is known to be
// valid and unchanging. The traceback function may also be called
// deeper in the call stack on the same thread that recorded the
// context. The traceback function may be called multiple times with
// the same Context value; it will usually be appropriate to cache the
// result, if possible, the first time this is called for a specific
// context value.
//
// If the traceback function is called from a signal handler on a Unix
// system, SigContext will be the signal context argument passed to
// the signal handler (a C ucontext_t* cast to uintptr_t). This may be
// used to start tracing at the point where the signal occurred. If
// the traceback function is not called from a signal handler,
// SigContext will be zero.
//
// Buf is where the traceback information should be stored. It should
// be PC values, such that Buf[0] is the PC of the caller, Buf[1] is
// the PC of that function's caller, and so on.  Max is the maximum
// number of entries to store.  The function should store a zero to
// indicate the top of the stack, or that the caller is on a different
// stack, presumably a Go stack.
//
// Unlike runtime.Callers, the PC values returned should, when passed
// to the symbolizer function, return the file/line of the call
// instruction.  No additional subtraction is required or appropriate.
//
// On all platforms, the traceback function is invoked when a call from
// Go to C to Go requests a stack trace. On linux/amd64, linux/ppc64le,
// linux/arm64, and freebsd/amd64, the traceback function is also invoked
// when a signal is received by a thread that is executing a cgo call.
// The traceback function should not make assumptions about when it is
// called, as future versions of Go may make additional calls.
//
// The symbolizer function will be called with a single argument, a
// pointer to a struct:
//
//	struct {
//		PC      uintptr // program counter to fetch information for
//		File    *byte   // file name (NUL terminated)
//		Lineno  uintptr // line number
//		Func    *byte   // function name (NUL terminated)
//		Entry   uintptr // function entry point
//		More    uintptr // set non-zero if more info for this PC
//		Data    uintptr // unused by runtime, available for function
//	}
//
// In C syntax, this struct will be
//
//	struct {
//		uintptr_t PC;
//		char*     File;
//		uintptr_t Lineno;
//		char*     Func;
//		uintptr_t Entry;
//		uintptr_t More;
//		uintptr_t Data;
//	};
//
// The PC field will be a value returned by a call to the traceback
// function.
//
// The first time the function is called for a particular traceback,
// all the fields except PC will be 0. The function should fill in the
// other fields if possible, setting them to 0/nil if the information
// is not available. The Data field may be used to store any useful
// information across calls. The More field should be set to non-zero
// if there is more information for this PC, zero otherwise. If More
// is set non-zero, the function will be called again with the same
// PC, and may return different information (this is intended for use
// with inlined functions). If More is zero, the function will be
// called with the next PC value in the traceback. When the traceback
// is complete, the function will be called once more with PC set to
// zero; this may be used to free any information. Each call will
// leave the fields of the struct set to the same values they had upon
// return, except for the PC field when the More field is zero. The
// function must not keep a copy of the struct pointer between calls.
//
// When calling SetCgoTraceback, the version argument is the version
// number of the structs that the functions expect to receive.
// Currently this must be zero.
//
// The symbolizer function may be nil, in which case the results of
// the traceback function will be displayed as numbers. If the
// traceback function is nil, the symbolizer function will never be
// called. The context function may be nil, in which case the
// traceback function will only be called with the context field set
// to zero.  If the context function is nil, then calls from Go to C
// to Go will not show a traceback for the C portion of the call stack.
//
// SetCgoTraceback should be called only once, ideally from an init function.
public static void SetCgoTraceback(nint version, @unsafe.Pointer traceback, @unsafe.Pointer context, @unsafe.Pointer symbolizer) {
    if (version != 0) {
        throw panic("unsupported version");
    }
    if (cgoTraceback != nil && cgoTraceback != traceback || cgoContext != nil && cgoContext != context || cgoSymbolizer != nil && cgoSymbolizer != symbolizer) {
        throw panic("call SetCgoTraceback only once");
    }
    cgoTraceback = traceback;
    cgoContext = context;
    cgoSymbolizer = symbolizer;
    // The context function is called when a C function calls a Go
    // function. As such it is only called by C code in runtime/cgo.
    if (_cgo_set_context_function != nil) {
        cgocall(_cgo_set_context_function, context);
    }
}

internal static @unsafe.Pointer cgoTraceback;

internal static @unsafe.Pointer cgoContext;

internal static @unsafe.Pointer cgoSymbolizer;

// cgoTracebackArg is the type passed to cgoTraceback.
[GoType] partial struct cgoTracebackArg {
    internal uintptr context;
    internal uintptr sigContext;
    internal ж<uintptr> buf;
    internal uintptr max;
}

// cgoContextArg is the type passed to the context function.
[GoType] partial struct cgoContextArg {
    internal uintptr context;
}

// cgoSymbolizerArg is the type passed to cgoSymbolizer.
[GoType] partial struct cgoSymbolizerArg {
    internal uintptr pc;
    internal ж<byte> @file;
    internal uintptr lineno;
    internal ж<byte> funcName;
    internal uintptr entry;
    internal uintptr more;
    internal uintptr data;
}

// printCgoTraceback prints a traceback of callers.
internal static void printCgoTraceback(ref ΔcgoCallers callers) {
    if (cgoSymbolizer == nil) {
        foreach (var (_, c) in callers) {
            if (c == 0) {
                break;
            }
            print((@string)"non-Go function at pc="u8, ((Δhex)(uint64)c), (@string)"\n"u8);
        }
        return;
    }
    var commitFrame = () => {
        return (true, false);
    };
    ref var arg = ref heap(new cgoSymbolizerArg(), out var Ꮡarg);
    foreach (var (_, c) in callers) {
        if (c == 0) {
            break;
        }
        printOneCgoTraceback(c, commitFrame, Ꮡarg);
    }
    arg.pc = 0;
    callCgoSymbolizer(Ꮡarg);
}

// printOneCgoTraceback prints the traceback of a single cgo caller.
// This can print more than one line because of inlining.
// It returns the "stop" result of commitFrame.
internal static bool printOneCgoTraceback(uintptr pc, Func<(bool, bool)> commitFrame, ж<cgoSymbolizerArg> Ꮡarg) {
    ref var arg = ref Ꮡarg.DerefOrNull();

    arg.pc = pc;
    while (ᐧ) {
        {
            var (pr, stop) = commitFrame(); if (stop){
                return true;
            } else 
            if (!pr) {
                continue;
            }
        }
        callCgoSymbolizer(Ꮡarg);
        if (arg.funcName != nil){
            // Note that we don't print any argument
            // information here, not even parentheses.
            // The symbolizer must add that if appropriate.
            println(gostringnocopy(arg.funcName));
        } else {
            println((@string)"non-Go function"u8);
        }
        print((@string)"\t"u8);
        if (arg.@file != nil) {
            print(gostringnocopy(arg.@file), (@string)":"u8, arg.lineno, (@string)" "u8);
        }
        print((@string)"pc="u8, ((Δhex)(uint64)pc), (@string)"\n"u8);
        if (arg.more == 0) {
            return false;
        }
    }
}

// callCgoSymbolizer calls the cgoSymbolizer function.
internal static void callCgoSymbolizer(ж<cgoSymbolizerArg> Ꮡarg) {
    var call = cgocall;
    if (Ꮡpanicking.Load() > 0 || (~(~getg()).m).curg != getg()) {
        // We do not want to call into the scheduler when panicking
        // or when on the system stack.
        call = asmcgocall;
    }
    if (msanenabled) {
        msanwrite(@unsafe.Pointer.FromPinnedBox(Ꮡarg), /* unsafe.Sizeof(cgoSymbolizerArg{}) */ (uintptr)56);
    }
    if (asanenabled) {
        asanwrite(@unsafe.Pointer.FromPinnedBox(Ꮡarg), /* unsafe.Sizeof(cgoSymbolizerArg{}) */ (uintptr)56);
    }
    call(cgoSymbolizer, (uintptr)noescape(@unsafe.Pointer.FromPinnedBox(Ꮡarg)));
}

// cgoContextPCs gets the PC values from a cgo traceback.
internal static void cgoContextPCs(uintptr ctxt, slice<uintptr> buf) {
    if (cgoTraceback == nil) {
        return;
    }
    var call = cgocall;
    if (Ꮡpanicking.Load() > 0 || (~(~getg()).m).curg != getg()) {
        // We do not want to call into the scheduler when panicking
        // or when on the system stack.
        call = asmcgocall;
    }
    ref var arg = ref heap<cgoTracebackArg>(out var Ꮡarg);
    arg = new cgoTracebackArg(
        context: ctxt,
        buf: Ꮡ(buf, 0),
        max: (uintptr)len(buf)
    );
    if (msanenabled) {
        msanwrite(@unsafe.Pointer.FromPinnedBox(Ꮡarg), /* unsafe.Sizeof(arg) */ (uintptr)32);
    }
    if (asanenabled) {
        asanwrite(@unsafe.Pointer.FromPinnedBox(Ꮡarg), /* unsafe.Sizeof(arg) */ (uintptr)32);
    }
    call(cgoTraceback, (uintptr)noescape(@unsafe.Pointer.FromPinnedBox(Ꮡarg)));
    System.GC.KeepAlive(Ꮡarg);
}

} // end runtime_package
