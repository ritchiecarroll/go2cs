// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

internal static UntypedInt _SEM_FAILCRITICALERRORS => 0x0001;
internal static UntypedInt _SEM_NOGPFAULTERRORBOX => 0x0002;
internal static UntypedInt _SEM_NOOPENFILEERRORBOX => 0x8000;
internal static UntypedInt _WER_FAULT_REPORTING_NO_UI => 0x0020;

internal static void preventErrorDialogs() {
    var errormode = stdcall0(_GetErrorMode);
    stdcall1(_SetErrorMode, (uintptr)((uintptr)((uintptr)(errormode | (uintptr)_SEM_FAILCRITICALERRORS) | (uintptr)_SEM_NOGPFAULTERRORBOX) | (uintptr)_SEM_NOOPENFILEERRORBOX));
    // Disable WER fault reporting UI.
    // Do this even if WER is disabled as a whole,
    // as WER might be enabled later with setTraceback("wer")
    // and we still want the fault reporting UI to be disabled if this happens.
    ref var werflags = ref heap(new uintptr(), out var Ꮡwerflags);
    stdcall2(_WerGetFlags, currentProcess, (uintptr)@unsafe.Pointer.FromBox(Ꮡwerflags));
    stdcall1(_WerSetFlags, (uintptr)(werflags | (uintptr)_WER_FAULT_REPORTING_NO_UI));
}

// go2cs generated this placeholder — func enableWER is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// in sys_windows_386.s, sys_windows_amd64.s, sys_windows_arm.s, and sys_windows_arm64.s
internal static partial void exceptiontramp();

internal static partial void firstcontinuetramp();

internal static partial void lastcontinuetramp();

internal static partial void sehtramp();

internal static partial void sigresume();

internal static void initExceptionHandler() {
    stdcall2(_AddVectoredExceptionHandler, 1, abi.FuncPCABI0(exceptiontramp));
    if (GOARCH == "386"u8){
        // use SetUnhandledExceptionFilter for windows-386.
        // note: SetUnhandledExceptionFilter handler won't be called, if debugging.
        stdcall1(_SetUnhandledExceptionFilter, abi.FuncPCABI0(lastcontinuetramp));
    } else {
        stdcall2(_AddVectoredContinueHandler, 1, abi.FuncPCABI0(firstcontinuetramp));
        stdcall2(_AddVectoredContinueHandler, 0, abi.FuncPCABI0(lastcontinuetramp));
    }
}

// isAbort returns true, if context r describes exception raised
// by calling runtime.abort function.
//
//go:nosplit
internal static bool isAbort(ж<context> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    var pc = r.ip();
    if (GOARCH == "386"u8 || GOARCH == "amd64"u8 || GOARCH == "arm"u8) {
        // In the case of an abort, the exception IP is one byte after
        // the INT3 (this differs from UNIX OSes). Note that on ARM,
        // this means that the exception IP is no longer aligned.
        pc--;
    }
    return isAbortPC(pc);
}

// isgoexception reports whether this exception should be translated
// into a Go panic or throw.
//
// It is nosplit to avoid growing the stack in case we're aborting
// because of a stack overflow.
//
//go:nosplit
internal static bool isgoexception(ref exceptionrecord info, ж<context> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    // Only handle exception if executing instructions in Go binary
    // (not Windows library code).
    // TODO(mwhudson): needs to loop to support shared libs
    if (r.ip() < firstmoduledata.text || firstmoduledata.etext < r.ip()) {
        return false;
    }
    // Go will only handle some exceptions.
    var exprᴛ1 = info.exceptioncode;
    if (exprᴛ1 == _EXCEPTION_ACCESS_VIOLATION) {
    }
    else if (exprᴛ1 == _EXCEPTION_IN_PAGE_ERROR) {
    }
    else if (exprᴛ1 == _EXCEPTION_INT_DIVIDE_BY_ZERO) {
    }
    else if (exprᴛ1 == _EXCEPTION_INT_OVERFLOW) {
    }
    else if (exprᴛ1 == _EXCEPTION_FLT_DENORMAL_OPERAND) {
    }
    else if (exprᴛ1 == _EXCEPTION_FLT_DIVIDE_BY_ZERO) {
    }
    else if (exprᴛ1 == _EXCEPTION_FLT_INEXACT_RESULT) {
    }
    else if (exprᴛ1 == _EXCEPTION_FLT_OVERFLOW) {
    }
    else if (exprᴛ1 == _EXCEPTION_FLT_UNDERFLOW) {
    }
    else if (exprᴛ1 == _EXCEPTION_BREAKPOINT) {
    }
    else if (exprᴛ1 == _EXCEPTION_ILLEGAL_INSTRUCTION) {
    }
    else { /* default: */
        return false;
    }

    // breakpoint arrives this way on arm64
    return true;
}

internal static UntypedInt callbackVEH => iota;
internal static UntypedInt callbackFirstVCH => 1;
internal static UntypedInt callbackLastVCH => 2;

// sigFetchGSafe is like getg() but without panicking
// when TLS is not set.
// Only implemented on windows/386, which is the only
// arch that loads TLS when calling getg(). Others
// use a dedicated register.
internal static partial ж<g> sigFetchGSafe();

internal static ж<g> sigFetchG() {
    if (GOARCH == "386"u8) {
        return sigFetchGSafe();
    }
    return getg();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownSigtrampCallbackˢ = "unknown sigtramp callback"u8;

// sigtrampgo is called from the exception handler function, sigtramp,
// written in assembly code.
// Return EXCEPTION_CONTINUE_EXECUTION if the exception is handled,
// else return EXCEPTION_CONTINUE_SEARCH.
//
// It is nosplit for the same reason as exceptionhandler.
//
//go:nosplit
internal static int32 sigtrampgo(ж<exceptionpointers> Ꮡep, nint kind) {
    ref var ep = ref Ꮡep.DerefOrNull();

    var gp = sigFetchG();
    if (gp == nil) {
        return _EXCEPTION_CONTINUE_SEARCH;
    }
    Func<ж<exceptionrecord>, ж<context>, ж<g>, int32> fn = default!;
    var exprᴛ1 = kind;
    if (exprᴛ1 == callbackVEH) {
        fn = exceptionhandler;
    }
    else if (exprᴛ1 == callbackFirstVCH) {
        fn = firstcontinuehandler;
    }
    else if (exprᴛ1 == callbackLastVCH) {
        fn = lastcontinuehandler;
    }
    else { /* default: */
        @throw(unknownSigtrampCallbackˢ);
    }

    // Check if we are running on g0 stack, and if we are,
    // call fn directly instead of creating the closure.
    // for the systemstack argument.
    //
    // A closure can't be marked as nosplit, so it might
    // call morestack if we are at the g0 stack limit.
    // If that happens, the runtime will call abort
    // and end up in sigtrampgo again.
    // TODO: revisit this workaround if/when closures
    // can be compiled as nosplit.
    //
    // Note that this scenario should only occur on
    // TestG0StackOverflow. Any other occurrence should
    // be treated as a bug.
    int32 ret = default!;
    if (gp != (~(~gp).m).g0){
        var fnʗ1 = fn;
        var gpʗ1 = gp;
        systemstack(() => {
            ret = fnʗ1(Ꮡep.Value.record, Ꮡep.Value.context, gpʗ1);
        });
    } else {
        ret = fn(ep.record, ep.context, gp);
    }
    if (ret == _EXCEPTION_CONTINUE_SEARCH) {
        return ret;
    }
    // Check if we need to set up the control flow guard workaround.
    // On Windows, the stack pointer in the context must lie within
    // system stack limits when we resume from exception.
    // Store the resume SP and PC in alternate registers
    // and return to sigresume on the g0 stack.
    // sigresume makes no use of the stack at all,
    // loading SP from RX and jumping to RY, being RX and RY two scratch registers.
    // Note that blindly smashing RX and RY is only safe because we know sigpanic
    // will not actually return to the original frame, so the registers
    // are effectively dead. But this does mean we can't use the
    // same mechanism for async preemption.
    if (ep.context.ip() == abi.FuncPCABI0(sigresume)) {
        // sigresume has already been set up by a previous exception.
        return ret;
    }
    prepareContextForSigResume(ref (ep.context).DerefOrNull());
    ep.context.set_sp((~(~(~gp).m).g0).sched.sp);
    ep.context.set_ip(abi.FuncPCABI0(sigresume));
    return ret;
}

// Called by sigtramp from Windows VEH handler.
// Return value signals whether the exception has been handled (EXCEPTION_CONTINUE_EXECUTION)
// or should be made available to other handlers in the chain (EXCEPTION_CONTINUE_SEARCH).
//
// This is nosplit to avoid growing the stack until we've checked for
// _EXCEPTION_BREAKPOINT, which is raised by abort() if we overflow the g0 stack.
//
//go:nosplit
internal static int32 exceptionhandler(ж<exceptionrecord> Ꮡinfo, ж<context> Ꮡr, ж<g> Ꮡgp) {
    ref var info = ref Ꮡinfo.DerefOrNull();
    ref var r = ref Ꮡr.DerefOrNull();
    ref var gp = ref Ꮡgp.DerefOrNull();

    if (!isgoexception(ref (Ꮡinfo).DerefOrNull(), Ꮡr)) {
        return _EXCEPTION_CONTINUE_SEARCH;
    }
    if (gp.throwsplit || isAbort(Ꮡr)) {
        // We can't safely sigpanic because it may grow the stack.
        // Or this is a call to abort.
        // Don't go through any more of the Windows handler chain.
        // Crash now.
        winthrow(Ꮡinfo, Ꮡr, Ꮡgp);
    }
    // After this point, it is safe to grow the stack.
    // Make it look like a call to the signal func.
    // Have to pass arguments out of band since
    // augmenting the stack frame would break
    // the unwinding code.
    gp.sig = info.exceptioncode;
    gp.sigcode0 = info.exceptioninformation[0];
    gp.sigcode1 = info.exceptioninformation[1];
    gp.sigpc = r.ip();
    // Only push runtime·sigpanic if r.ip() != 0.
    // If r.ip() == 0, probably panicked because of a
    // call to a nil func. Not pushing that onto sp will
    // make the trace look like a call to runtime·sigpanic instead.
    // (Otherwise the trace will end at runtime·sigpanic and we
    // won't get to see who faulted.)
    // Also don't push a sigpanic frame if the faulting PC
    // is the entry of asyncPreempt. In this case, we suspended
    // the thread right between the fault and the exception handler
    // starting to run, and we have pushed an asyncPreempt call.
    // The exception is not from asyncPreempt, so not to push a
    // sigpanic call to make it look like that. Instead, just
    // overwrite the PC. (See issue #35773)
    if (r.ip() != 0 && r.ip() != abi.FuncPCABI0(asyncPreempt)) {
        @unsafe.Pointer sp = (@unsafe.Pointer)r.sp();
        var delta = (uintptr)sys.StackAlign;
        sp = (uintptr)add(sp, ((uintptr)0 - delta));
        r.set_sp((uintptr)sp);
        if (usesLR){
            ((ж<uintptr>)(uintptr)(sp)).Value = r.lr();
            r.set_lr(r.ip());
        } else {
            ((ж<uintptr>)(uintptr)(sp)).Value = r.ip();
        }
    }
    r.set_ip(abi.FuncPCABI0(sigpanic0));
    return _EXCEPTION_CONTINUE_EXECUTION;
}

// sehhandler is reached as part of the SEH chain.
//
// It is nosplit for the same reason as exceptionhandler.
//
//go:nosplit
internal static int32 sehhandler(ж<exceptionrecord> _Δp0, uint64 _Δp1, ж<context> _Δp2, ж<_DISPATCHER_CONTEXT> Ꮡdctxt) {
    ref var dctxt = ref Ꮡdctxt.DerefOrNull();

    var g0 = getg();
    if (g0 == nil || (~(~g0).m).curg == nil) {
        // No g available, nothing to do here.
        return _EXCEPTION_CONTINUE_SEARCH_SEH;
    }
    // The Windows SEH machinery will unwind the stack until it finds
    // a frame with a handler for the exception or until the frame is
    // outside the stack boundaries, in which case it will call the
    // UnhandledExceptionFilter. Unfortunately, it doesn't know about
    // the goroutine stack, so it will stop unwinding when it reaches the
    // first frame not running in g0. As a result, neither non-Go exceptions
    // handlers higher up the stack nor UnhandledExceptionFilter will be called.
    //
    // To work around this, manually unwind the stack until the top of the goroutine
    // stack is reached, and then pass the control back to Windows.
    var gp = g0.Value.m.Value.curg;
    var ctxt = dctxt.ctx();
    ref var @base = ref heap(new uintptr(), out var Ꮡbase);
    ref var sp = ref heap(new uintptr(), out var Ꮡsp);
    while (ᐧ) {
        var entry = stdcall3(_RtlLookupFunctionEntry, ctxt.ip(), (uintptr)@unsafe.Pointer.FromBox(Ꮡbase), 0);
        if (entry == 0) {
            break;
        }
        stdcall8(_RtlVirtualUnwind, 0, @base, ctxt.ip(), entry, (uintptr)ctxt, 0, (uintptr)@unsafe.Pointer.FromBox(Ꮡsp), 0);
        if (sp < (~gp).stack.lo || (~gp).stack.hi <= sp) {
            break;
        }
    }
    return _EXCEPTION_CONTINUE_SEARCH_SEH;
}

// It seems Windows searches ContinueHandler's list even
// if ExceptionHandler returns EXCEPTION_CONTINUE_EXECUTION.
// firstcontinuehandler will stop that search,
// if exceptionhandler did the same earlier.
//
// It is nosplit for the same reason as exceptionhandler.
//
//go:nosplit
internal static int32 firstcontinuehandler(ж<exceptionrecord> Ꮡinfo, ж<context> Ꮡr, ж<g> Ꮡgp) {
    if (!isgoexception(ref (Ꮡinfo).DerefOrNull(), Ꮡr)) {
        return _EXCEPTION_CONTINUE_SEARCH;
    }
    return _EXCEPTION_CONTINUE_EXECUTION;
}

// lastcontinuehandler is reached, because runtime cannot handle
// current exception. lastcontinuehandler will print crash info and exit.
//
// It is nosplit for the same reason as exceptionhandler.
//
//go:nosplit
internal static int32 lastcontinuehandler(ж<exceptionrecord> Ꮡinfo, ж<context> Ꮡr, ж<g> Ꮡgp) {
    ref var info = ref Ꮡinfo.DerefOrNull();
    ref var r = ref Ꮡr.DerefOrNull();

    if (islibrary || isarchive) {
        // Go DLL/archive has been loaded in a non-go program.
        // If the exception does not originate from go, the go runtime
        // should not take responsibility of crashing the process.
        return _EXCEPTION_CONTINUE_SEARCH;
    }
    // VEH is called before SEH, but arm64 MSVC DLLs use SEH to trap
    // illegal instructions during runtime initialization to determine
    // CPU features, so if we make it to the last handler and we're
    // arm64 and it's an illegal instruction and this is coming from
    // non-Go code, then assume it's this runtime probing happen, and
    // pass that onward to SEH.
    if (GOARCH == "arm64"u8 && info.exceptioncode == _EXCEPTION_ILLEGAL_INSTRUCTION && (r.ip() < firstmoduledata.text || firstmoduledata.etext < r.ip())) {
        return _EXCEPTION_CONTINUE_SEARCH;
    }
    winthrow(Ꮡinfo, Ꮡr, Ꮡgp);
    return 0; // not reached
}

// Always called on g0. gp is the G where the exception occurred.
//
//go:nosplit
internal static void winthrow(ж<exceptionrecord> Ꮡinfo, ж<context> Ꮡr, ж<g> Ꮡgp) {
    ref var info = ref Ꮡinfo.DerefOrNull();
    ref var r = ref Ꮡr.DerefOrNull();
    ref var gp = ref Ꮡgp.DerefOrNull();

    var g0 = getg();
    if (Ꮡpanicking.Load() != 0) {
        // traceback already printed
        exit(2);
    }
    Ꮡpanicking.Store(1);
    // In case we're handling a g0 stack overflow, blow away the
    // g0 stack bounds so we have room to print the traceback. If
    // this somehow overflows the stack, the OS will trap it.
    g0.Value.stack.lo = 0;
    g0.Value.stackguard0 = (~g0).stack.lo + (uintptr)stackGuard;
    g0.Value.stackguard1 = g0.Value.stackguard0;
    print((@string)"Exception "u8, ((Δhex)(uint64)info.exceptioncode), (@string)" "u8, ((Δhex)(uint64)info.exceptioninformation[0]), (@string)" "u8, ((Δhex)(uint64)info.exceptioninformation[1]), (@string)" "u8, ((Δhex)(uint64)r.ip()), (@string)"\n"u8);
    print((@string)"PC="u8, ((Δhex)(uint64)r.ip()), (@string)"\n"u8);
    if ((~(~g0).m).incgo && Ꮡgp == (~(~g0).m).g0 && (~(~g0).m).curg != nil) {
        if (iscgo) {
            print((@string)"signal arrived during external code execution\n"u8);
        }
        Ꮡgp = g0.Value.m.Value.curg; gp = ref Ꮡgp.DerefOrNull();
    }
    print((@string)"\n"u8);
    g0.Value.m.Value.throwing = throwTypeRuntime;
    (~g0).m.of(m.Ꮡcaughtsig).set(Ꮡgp);
    var (level, _, docrash) = gotraceback();
    if (level > 0) {
        tracebacktrap(r.ip(), r.sp(), r.lr(), Ꮡgp);
        tracebackothers(Ꮡgp);
        dumpregs(ref (Ꮡr).DerefOrNull());
    }
    if (docrash) {
        dieFromException(Ꮡinfo, Ꮡr);
    }
    exit(2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedSignalDuringˢ = "unexpected signal during runtime execution"u8;
internal static readonly @string faultˢ = "fault"u8;

internal static void sigpanic() {
    var gp = getg();
    if (!canpanic()) {
        @throw(unexpectedSignalDuringˢ);
    }
    var exprᴛ1 = (~gp).sig;
    if (exprᴛ1 == _EXCEPTION_ACCESS_VIOLATION || exprᴛ1 == _EXCEPTION_IN_PAGE_ERROR) {
        if ((~gp).sigcode1 < 0x1000) {
            panicmem();
        }
        if ((~gp).paniconfault) {
            panicmemAddr((~gp).sigcode1);
        }
        if (inUserArenaChunk((~gp).sigcode1)){
            // We could check that the arena chunk is explicitly set to fault,
            // but the fact that we faulted on accessing it is enough to prove
            // that it is.
            print((@string)"accessed data from freed user arena "u8, ((Δhex)(uint64)(~gp).sigcode1), (@string)"\n"u8);
        } else {
            print((@string)"unexpected fault address "u8, ((Δhex)(uint64)(~gp).sigcode1), (@string)"\n"u8);
        }
        @throw(faultˢ);
    }
    else if (exprᴛ1 == _EXCEPTION_INT_DIVIDE_BY_ZERO) {
        panicdivide();
    }
    else if (exprᴛ1 == _EXCEPTION_INT_OVERFLOW) {
        panicoverflow();
    }
    else if (exprᴛ1 == _EXCEPTION_FLT_DENORMAL_OPERAND || exprᴛ1 == _EXCEPTION_FLT_DIVIDE_BY_ZERO || exprᴛ1 == _EXCEPTION_FLT_INEXACT_RESULT || exprᴛ1 == _EXCEPTION_FLT_OVERFLOW || exprᴛ1 == _EXCEPTION_FLT_UNDERFLOW) {
        panicfloat();
    }

    @throw(faultˢ);
}

// Following are not implemented.
internal static void initsig(bool preinit) {
}

internal static void sigenable(uint32 sig) {
}

internal static void sigdisable(uint32 sig) {
}

internal static void sigignore(uint32 sig) {
}

internal static @string signame(uint32 sig) {
    return ""u8;
}

//go:nosplit
internal static void crash() {
    dieFromException(nil, nil);
}

// dieFromException raises an exception that bypasses all exception handlers.
// This provides the expected exit status for the shell.
//
//go:nosplit
internal static void dieFromException(ж<exceptionrecord> Ꮡinfo, ж<context> Ꮡr) {
    ref var info = ref Ꮡinfo.DerefOrNull();

    if (Ꮡinfo == nil) {
        var gp = getg();
        if ((~gp).sig != 0){
            // Try to reconstruct an exception record from
            // the exception information stored in gp.
            Ꮡinfo = Ꮡ(new exceptionrecord(
                exceptionaddress: (~gp).sigpc,
                exceptioncode: (~gp).sig,
                numberparameters: 2
            )); info = ref Ꮡinfo.DerefOrNull();
            info.exceptioninformation[0] = gp.Value.sigcode0;
            info.exceptioninformation[1] = gp.Value.sigcode1;
        } else {
            // By default, a failing Go application exits with exit code 2.
            // Use this value when gp does not contain exception info.
            Ꮡinfo = Ꮡ(new exceptionrecord(
                exceptioncode: 2
            )); info = ref Ꮡinfo.DerefOrNull();
        }
    }
    uintptr FAIL_FAST_GENERATE_EXCEPTION_ADDRESS = 0x1;
    stdcall3(_RaiseFailFastException, (uintptr)Ꮡinfo, (uintptr)Ꮡr, FAIL_FAST_GENERATE_EXCEPTION_ADDRESS);
}

// gsignalStack is unused on Windows.
partial struct gsignalStack {
}

} // end runtime_package
