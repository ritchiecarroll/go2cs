// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go;

using abi = @internal.abi_package;
using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// sigTabT is the type of an entry in the global sigtable array.
// sigtable is inherently system dependent, and appears in OS-specific files,
// but sigTabT is the same for all Unixy systems.
// The sigtable array is indexed by a system signal number to get the flags
// and printable name of each signal.
[GoType] partial struct sigTabT {
    internal int32 flags;
    internal @string name;
}

//go:linkname os_sigpipe os.sigpipe
internal static void os_sigpipe() {
    systemstack(sigpipe);
}

internal static @string signame(uint32 sig) {
    if (sig >= (uint32)len(sigtable)) {
        return ""u8;
    }
    return sigtable[(nint)(sig)].name;
}

internal static uintptr _SIG_DFL => 0;
internal static uintptr _SIG_IGN => 1;

// sigPreempt is the signal used for non-cooperative preemption.
//
// There's no good way to choose this signal, but there are some
// heuristics:
//
// 1. It should be a signal that's passed-through by debuggers by
// default. On Linux, this is SIGALRM, SIGURG, SIGCHLD, SIGIO,
// SIGVTALRM, SIGPROF, and SIGWINCH, plus some glibc-internal signals.
//
// 2. It shouldn't be used internally by libc in mixed Go/C binaries
// because libc may assume it's the only thing that can handle these
// signals. For example SIGCANCEL or SIGSETXID.
//
// 3. It should be a signal that can happen spuriously without
// consequences. For example, SIGALRM is a bad choice because the
// signal handler can't tell if it was caused by the real process
// alarm or not (arguably this means the signal is broken, but I
// digress). SIGUSR1 and SIGUSR2 are also bad because those are often
// used in meaningful ways by applications.
//
// 4. We need to deal with platforms without real-time signals (like
// macOS), so those are out.
//
// We use SIGURG because it meets all of these criteria, is extremely
// unlikely to be used by an application for its "real" meaning (both
// because out-of-band data is basically unused and because SIGURG
// doesn't report which socket has the condition, making it pretty
// useless), and even if it is, the application has to be ready for
// spurious SIGURG. SIGIO wouldn't be a bad choice either, but is more
// likely to be used for real.
internal static UntypedInt sigPreempt => /* _SIGURG */ 16;

// Stores the signal handlers registered before Go installed its own.
// These signal handlers will be invoked in cases where Go doesn't want to
// handle a particular signal (e.g., signal occurred on a non-Go thread).
// See sigfwdgo for more information on when the signals are forwarded.
//
// This is read by the signal handler; accesses should use
// atomic.Loaduintptr and atomic.Storeuintptr.
internal static ж<array<uintptr>> ᏑfwdSig = new StandardBox<array<uintptr>>(new array<uintptr>(32));
internal static ref array<uintptr> fwdSig => ref ᏑfwdSig.Value;

// handlingSig is indexed by signal number and is non-zero if we are
// currently handling the signal. Or, to put it another way, whether
// the signal handler is currently set to the Go signal handler or not.
// This is uint32 rather than bool so that we can use atomic instructions.
internal static ж<array<uint32>> ᏑhandlingSig = new StandardBox<array<uint32>>(new array<uint32>(32));
internal static ref array<uint32> handlingSig => ref ᏑhandlingSig.Value;

// channels for synchronizing signal mask updates with the signal mask
// thread
internal static channel<uint32> disableSigChan;

internal static channel<uint32> enableSigChan;

internal static channel<EmptyStruct> maskUpdatedChan;

/* [GoInit] runtime bootstrap init - not run; .NET is the runtime */ internal static void initΔ8() {
    // _NSIG is the number of signals on this operating system.
    // sigtable should describe what to do for all the possible signals.
    if (len(sigtable) != _NSIG) {
        print((@string)"runtime: len(sigtable)="u8, len(sigtable), (@string)" _NSIG="u8, (nint)(_NSIG), (@string)"\n"u8);
        @throw("bad sigtable len"u8);
    }
}

internal static bool signalsOK;

// Initialize signals.
// Called by libpreinit so runtime may not be initialized.
//
//go:nosplit
//go:nowritebarrierrec
internal static void initsig(bool preinit) {
    if (!preinit) {
        // It's now OK for signal handlers to run.
        signalsOK = true;
    }
    // For c-archive/c-shared this is called by libpreinit with
    // preinit == true.
    if ((isarchive || islibrary) && !preinit) {
        return;
    }
    for (var i = (uint32)0; i < _NSIG; i++) {
        var t = Ꮡsigtable.at<sigTabT>((nint)(i));
        if ((~t).flags == 0 || (int32)((~t).flags & (int32)_SigDefault) != 0) {
            continue;
        }
        // We don't need to use atomic operations here because
        // there shouldn't be any other goroutines running yet.
        fwdSig[(nint)(i)] = getsig(i);
        if (!sigInstallGoHandler(i)) {
            // Even if we are not installing a signal handler,
            // set SA_ONSTACK if necessary.
            if (fwdSig[(nint)(i)] != _SIG_DFL && fwdSig[(nint)(i)] != _SIG_IGN){
                setsigstack(i);
            } else 
            if (fwdSig[(nint)(i)] == _SIG_IGN) {
                sigInitIgnored(i);
            }
            continue;
        }
        handlingSig[(nint)(i)] = 1;
        setsig(i, abi.FuncPCABIInternal(sighandler));
    }
}

//go:nosplit
//go:nowritebarrierrec
internal static bool sigInstallGoHandler(uint32 sig) {
    // For some signals, we respect an inherited SIG_IGN handler
    // rather than insist on installing our own default handler.
    // Even these signals can be fetched using the os/signal package.
    var exprᴛ1 = sig;
    if (exprᴛ1 == _SIGHUP || exprᴛ1 == _SIGINT) {
        if (atomic.Loaduintptr(ᏑfwdSig.at<uintptr>((nint)(sig))) == _SIG_IGN) {
            return false;
        }
    }

    if ((GOOS == "linux"u8 || GOOS == "android"u8) && !iscgo && sig == sigPerThreadSyscall) {
        // sigPerThreadSyscall is the same signal used by glibc for
        // per-thread syscalls on Linux. We use it for the same purpose
        // in non-cgo binaries.
        return true;
    }
    var t = Ꮡsigtable.at<sigTabT>((nint)(sig));
    if ((int32)((~t).flags & (int32)_SigSetStack) != 0) {
        return false;
    }
    // When built using c-archive or c-shared, only install signal
    // handlers for synchronous signals and SIGPIPE and sigPreempt.
    if ((isarchive || islibrary) && (int32)((~t).flags & (int32)_SigPanic) == 0 && sig != _SIGPIPE && sig != sigPreempt) {
        return false;
    }
    return true;
}

// go2cs generated this placeholder — func sigenable is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func sigdisable is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func sigignore is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// clearSignalHandlers clears all signal handlers that are not ignored
// back to the default. This is called by the child after a fork, so that
// we can enable the signal mask for the exec without worrying about
// running a signal handler in the child.
//
//go:nosplit
//go:nowritebarrierrec
internal static void clearSignalHandlers() {
    for (var i = (uint32)0; i < _NSIG; i++) {
        if (atomic.Load(ᏑhandlingSig.at<uint32>((nint)(i))) != 0) {
            setsig(i, _SIG_DFL);
        }
    }
}

// setProcessCPUProfilerTimer is called when the profiling timer changes.
// It is called with prof.signalLock held. hz is the new timer, and is 0 if
// profiling is being disabled. Enable or disable the signal as
// required for -buildmode=c-archive.
internal static void setProcessCPUProfilerTimer(int32 hz) {
    if (hz != 0){
        // Enable the Go signal handler if not enabled.
        if (atomic.Cas(ᏑhandlingSig.at<uint32>(_SIGPROF), 0, 1)) {
            var h = getsig(_SIGPROF);
            // If no signal handler was installed before, then we record
            // _SIG_IGN here. When we turn off profiling (below) we'll start
            // ignoring SIGPROF signals. We do this, rather than change
            // to SIG_DFL, because there may be a pending SIGPROF
            // signal that has not yet been delivered to some other thread.
            // If we change to SIG_DFL when turning off profiling, the
            // program will crash when that SIGPROF is delivered. We assume
            // that programs that use profiling don't want to crash on a
            // stray SIGPROF. See issue 19320.
            // We do the change here instead of when turning off profiling,
            // because there we may race with a signal handler running
            // concurrently, in particular, sigfwdgo may observe _SIG_DFL and
            // die. See issue 43828.
            if (h == _SIG_DFL) {
                h = _SIG_IGN;
            }
            atomic.Storeuintptr(ᏑfwdSig.at<uintptr>(_SIGPROF), h);
            setsig(_SIGPROF, abi.FuncPCABIInternal(sighandler));
        }
        ref var it = ref heap(new itimerval(), out var Ꮡit);
        it.it_interval.tv_sec = 0;
        it.it_interval.set_usec(1000000 / hz);
        it.it_value = it.it_interval.ΔClone();
        setitimer(_ITIMER_PROF, Ꮡit, nil);
    } else {
        setitimer(_ITIMER_PROF, Ꮡ(new itimerval(nil)), nil);
        // If the Go signal handler should be disabled by default,
        // switch back to the signal handler that was installed
        // when we enabled profiling. We don't try to handle the case
        // of a program that changes the SIGPROF handler while Go
        // profiling is enabled.
        if (!sigInstallGoHandler(_SIGPROF)) {
            if (atomic.Cas(ᏑhandlingSig.at<uint32>(_SIGPROF), 1, 0)) {
                var h = atomic.Loaduintptr(ᏑfwdSig.at<uintptr>(_SIGPROF));
                setsig(_SIGPROF, h);
            }
        }
    }
}

// setThreadCPUProfilerHz makes any thread-specific changes required to
// implement profiling at a rate of hz.
// No changes required on Unix systems when using setitimer.
internal static void setThreadCPUProfilerHz(int32 hz) {
    getg().Value.m.Value.profilehz = hz;
}

internal static void sigpipe() {
    if (signal_ignored(_SIGPIPE) || sigsend(_SIGPIPE)) {
        return;
    }
    dieFromSignal(_SIGPIPE);
}

// doSigPreempt handles a preemption signal on gp.
internal static void doSigPreempt(ж<g> Ꮡgp, ж<sigctxt> Ꮡctxt) {
    ref var gp = ref Ꮡgp.DerefOrNull();
    ref var ctxt = ref Ꮡctxt.DerefOrNull();

    // Check if this G wants to be preempted and is safe to
    // preempt.
    if (wantAsyncPreempt(Ꮡgp)) {
        {
            var (ok, newpc) = isAsyncSafePoint(Ꮡgp, ctxt.sigpc(), ctxt.sigsp(), ctxt.siglr()); if (ok) {
                // Adjust the PC and inject a call to asyncPreempt.
                ctxt.pushCall(abi.FuncPCABI0(asyncPreempt), newpc);
            }
        }
    }
    // Acknowledge the preemption.
    gp.m.of(m.ᏑpreemptGen).Add(1);
    gp.m.of(m.ᏑsignalPending).Store(0);
    if (GOOS == "darwin"u8 || GOOS == "ios"u8) {
        ᏑpendingPreemptSignals.Add(-1);
    }
}

internal const bool preemptMSupported = true;

// preemptM sends a preemption request to mp. This request may be
// handled asynchronously and may be coalesced with other requests to
// the M. When the request is received, if the running G or P are
// marked for preemption and the goroutine is at an asynchronous
// safe-point, it will preempt the goroutine. It always atomically
// increments mp.preemptGen after handling a preemption request.
internal static void preemptM(ж<m> Ꮡmp) {
    // On Darwin, don't try to preempt threads during exec.
    // Issue #41702.
    if (GOOS == "darwin"u8 || GOOS == "ios"u8) {
        ᏑexecLock.rlock();
    }
    if (Ꮡmp.of(m.ᏑsignalPending).CompareAndSwap(0, 1)) {
        if (GOOS == "darwin"u8 || GOOS == "ios"u8) {
            ᏑpendingPreemptSignals.Add(1);
        }
        // If multiple threads are preempting the same M, it may send many
        // signals to the same M such that it hardly make progress, causing
        // live-lock problem. Apparently this could happen on darwin. See
        // issue #37741.
        // Only send a signal if there isn't already one pending.
        signalM(ref (Ꮡmp).DerefOrNull(), sigPreempt);
    }
    if (GOOS == "darwin"u8 || GOOS == "ios"u8) {
        ᏑexecLock.runlock();
    }
}

// sigFetchG fetches the value of G safely when running in a signal handler.
// On some architectures, the g value may be clobbered when running in a VDSO.
// See issue #32912.
//
//go:nosplit
internal static ж<g> sigFetchG(ж<sigctxt> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    var exprᴛ1 = GOARCH;
    if (exprᴛ1 == "arm"u8 || exprᴛ1 == "arm64"u8 || exprᴛ1 == "loong64"u8 || exprᴛ1 == "ppc64"u8 || exprᴛ1 == "ppc64le"u8 || exprᴛ1 == "riscv64"u8 || exprᴛ1 == "s390x"u8) {
        if (!iscgo && inVDSOPage(c.sigpc())) {
            // When using cgo, we save the g on TLS and load it from there
            // in sigtramp. Just use that.
            // Otherwise, before making a VDSO call we save the g to the
            // bottom of the signal stack. Fetch from there.
            // TODO: in efence mode, stack is sysAlloc'd, so this wouldn't
            // work.
            var sp = getcallersp();
            var s = spanOf(sp);
            if (s != nil && s.of(mspan.Ꮡstate).get() == mSpanManual && s.@base() < sp && sp < (~s).limit) {
                var gp = ~(ж<ж<g>>)(uintptr)((@unsafe.Pointer)s.@base());
                return gp;
            }
            return default!;
        }
    }

    return getg();
}

// sigtrampgo is called from the signal handler function, sigtramp,
// written in assembly code.
// This is called by the signal handler, and the world may be stopped.
//
// It must be nosplit because getg() is still the G that was running
// (if any) when the signal was delivered, but it's (usually) called
// on the gsignal stack. Until this switches the G to gsignal, the
// stack bounds check won't work.
//
//go:nosplit
//go:nowritebarrierrec
internal static void sigtrampgo(uint32 sig, ж<siginfo> Ꮡinfo, @unsafe.Pointer ctx) {
    if (sigfwdgo(sig, Ꮡinfo, ctx)) {
        return;
    }
    var c = Ꮡ(new sigctxt(Ꮡinfo, ctx.Value));
    var gp = sigFetchG(c);
    setg(gp);
    if (gp == nil || ((~gp).m != nil && (~(~gp).m).isExtraInC)) {
        if (sig == _SIGPROF) {
            // Some platforms (Linux) have per-thread timers, which we use in
            // combination with the process-wide timer. Avoid double-counting.
            if (validSIGPROF(ref ((ж<m>)default!).DerefOrNull(), ref (c).DerefOrNull())) {
                sigprofNonGoPC(c.sigpc());
            }
            return;
        }
        if (sig == sigPreempt && preemptMSupported && debug.asyncpreemptoff == 0) {
            // This is probably a signal from preemptM sent
            // while executing Go code but received while
            // executing non-Go code.
            // We got past sigfwdgo, so we know that there is
            // no non-Go signal handler for sigPreempt.
            // The default behavior for sigPreempt is to ignore
            // the signal, so badsignal will be a no-op anyway.
            if (GOOS == "darwin"u8 || GOOS == "ios"u8) {
                ᏑpendingPreemptSignals.Add(-1);
            }
            return;
        }
        c.fixsigcode(sig);
        // Set g to nil here and badsignal will use g0 by needm.
        // TODO: reuse the current m here by using the gsignal and adjustSignalStack,
        // since the current g maybe a normal goroutine and actually running on the signal stack,
        // it may hit stack split that is not expected here.
        if (gp != nil) {
            setg(nil);
        }
        badsignal((uintptr)sig, c);
        // Restore g
        if (gp != nil) {
            setg(gp);
        }
        return;
    }
    setg((~(~gp).m).gsignal);
    // If some non-Go code called sigaltstack, adjust.
    ref var gsignalStack = ref heap(new gsignalStack(), out var ᏑgsignalStack);
    var setStack = adjustSignalStack(sig, ref ((~gp).m).DerefOrNull(), ᏑgsignalStack);
    if (setStack) {
        gp.Value.m.Value.gsignal.Value.stktopsp = getcallersp();
    }
    if ((~gp).stackguard0 == stackFork) {
        signalDuringFork(sig);
    }
    c.fixsigcode(sig);
    sighandler(sig, Ꮡinfo, ctx, gp);
    setg(gp);
    if (setStack) {
        restoreGsignalStack(ᏑgsignalStack);
    }
}

// If the signal handler receives a SIGPROF signal on a non-Go thread,
// it tries to collect a traceback into sigprofCallers.
// sigprofCallersUse is set to non-zero while sigprofCallers holds a traceback.
internal static ΔcgoCallers sigprofCallers;

internal static ж<uint32> ᏑsigprofCallersUse = new StandardBox<uint32>(default(uint32));
internal static ref uint32 sigprofCallersUse => ref ᏑsigprofCallersUse.Value;

// sigprofNonGo is called if we receive a SIGPROF signal on a non-Go thread,
// and the signal handler collected a stack trace in sigprofCallers.
// When this is called, sigprofCallersUse will be non-zero.
// g is nil, and what we can do is very limited.
//
// It is called from the signal handling functions written in assembly code that
// are active for cgo programs, cgoSigtramp and sigprofNonGoWrapper, which have
// not verified that the SIGPROF delivery corresponds to the best available
// profiling source for this thread.
//
//go:nosplit
//go:nowritebarrierrec
internal static void sigprofNonGo(uint32 sig, ж<siginfo> Ꮡinfo, @unsafe.Pointer ctx) {
    if (Ꮡprof.of(profᴛ1.Ꮡhz).Load() != 0) {
        var c = Ꮡ(new sigctxt(Ꮡinfo, ctx.Value));
        // Some platforms (Linux) have per-thread timers, which we use in
        // combination with the process-wide timer. Avoid double-counting.
        if (validSIGPROF(ref ((ж<m>)default!).DerefOrNull(), ref (c).DerefOrNull())) {
            nint n = 0;
            while (n < len(sigprofCallers) && sigprofCallers[n] != 0) {
                n++;
            }
            cpuprof.addNonGo(sigprofCallers[..(int)(n)]);
        }
    }
    atomic.Store(ᏑsigprofCallersUse, 0);
}

// sigprofNonGoPC is called when a profiling signal arrived on a
// non-Go thread and we have a single PC value, not a stack trace.
// g is nil, and what we can do is very limited.
//
//go:nosplit
//go:nowritebarrierrec
internal static void sigprofNonGoPC(uintptr pc) {
    if (Ꮡprof.of(profᴛ1.Ꮡhz).Load() != 0) {
        var stk = new uintptr[]{
            pc,
            abi.FuncPCABIInternal(_ExternalCode) + (uintptr)sys.PCQuantum
        }.slice();
        cpuprof.addNonGo(stk);
    }
}

// adjustSignalStack adjusts the current stack guard based on the
// stack pointer that is actually in use while handling a signal.
// We do this in case some non-Go code called sigaltstack.
// This reports whether the stack was adjusted, and if so stores the old
// signal stack in *gsigstack.
//
//go:nosplit
internal static bool adjustSignalStack(uint32 sigʗp, ref m mp, ж<gsignalStack> ᏑgsigStack) {
    ref var sig = ref heap(sigʗp, out var Ꮡsig);

    var sp = (uintptr)Ꮡsig;
    if (sp >= (~mp.gsignal).stack.lo && sp < (~mp.gsignal).stack.hi) {
        return false;
    }
    ref var st = ref heap(new stackt(), out var Ꮡst);
    sigaltstack(nil, Ꮡst);
    var stsp = (uintptr)st.ss_sp;
    if ((int32)(st.ss_flags & (int32)_SS_DISABLE) == 0 && sp >= stsp && sp < stsp + st.ss_size) {
        setGsignalStack(ref st, ᏑgsigStack);
        return true;
    }
    if (sp >= (~mp.g0).stack.lo && sp < (~mp.g0).stack.hi) {
        // The signal was delivered on the g0 stack.
        // This can happen when linked with C code
        // using the thread sanitizer, which collects
        // signals then delivers them itself by calling
        // the signal handler directly when C code,
        // including C code called via cgo, calls a
        // TSAN-intercepted function such as malloc.
        //
        // We check this condition last as g0.stack.lo
        // may be not very accurate (see mstart).
        ref var stΔ1 = ref heap<stackt>(out var ᏑstΔ1);
        stΔ1 = new stackt(ss_size: (~mp.g0).stack.hi - (~mp.g0).stack.lo);
        setSignalstackSP(ᏑstΔ1, (~mp.g0).stack.lo);
        setGsignalStack(ref stΔ1, ᏑgsigStack);
        return true;
    }
    // sp is not within gsignal stack, g0 stack, or sigaltstack. Bad.
    setg(nil);
    needm(true);
    if ((int32)(st.ss_flags & (int32)_SS_DISABLE) != 0){
        noSignalStack(sig);
    } else {
        sigNotOnStack(sig, sp, ref mp);
    }
    dropm();
    return false;
}

// crashing is the number of m's we have waited for when implementing
// GOTRACEBACK=crash when a signal is received.
internal static ж<atomic.Int32> Ꮡcrashing = new StandardBox<atomic.Int32>(default(atomic.Int32));
internal static ref atomic.Int32 crashing => ref Ꮡcrashing.Value;

// testSigtrap and testSigusr1 are used by the runtime tests. If
// non-nil, it is called on SIGTRAP/SIGUSR1. If it returns true, the
// normal behavior on this signal is suppressed.
internal static Func<ж<siginfo>, ж<sigctxt>, ж<g>, bool> testSigtrap;

internal static Func<ж<g>, bool> testSigusr1;

// sigsysIgnored is non-zero if we are currently ignoring SIGSYS. See issue #69065.
internal static ж<uint32> ᏑsigsysIgnored = new StandardBox<uint32>(default(uint32));
internal static ref uint32 sigsysIgnored => ref ᏑsigsysIgnored.Value;

//go:linkname ignoreSIGSYS os.ignoreSIGSYS
internal static void ignoreSIGSYS() {
    atomic.Store(ᏑsigsysIgnored, 1);
}

//go:linkname restoreSIGSYS os.restoreSIGSYS
internal static void restoreSIGSYS() {
    atomic.Store(ᏑsigsysIgnored, 0);
}

// sighandler is invoked when a signal occurs. The global g will be
// set to a gsignal goroutine and we will be running on the alternate
// signal stack. The parameter gp will be the value of the global g
// when the signal occurred. The sig, info, and ctxt parameters are
// from the system signal handler: they are the parameters passed when
// the SA is passed to the sigaction system call.
//
// The garbage collector may have stopped the world, so write barriers
// are not allowed.
//
//go:nowritebarrierrec
internal static void sighandler(uint32 sig, ж<siginfo> Ꮡinfo, @unsafe.Pointer ctxt, ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // The g executing the signal handler. This is almost always
    // mp.gsignal. See delayedSignal for an exception.
    var gsignal = getg();
    var mp = gsignal.Value.m;
    var c = Ꮡ(new sigctxt(Ꮡinfo, ctxt.Value));
    // Cgo TSAN (not the Go race detector) intercepts signals and calls the
    // signal handler at a later time. When the signal handler is called, the
    // memory may have changed, but the signal context remains old. The
    // unmatched signal context and memory makes it unsafe to unwind or inspect
    // the stack. So we ignore delayed non-fatal signals that will cause a stack
    // inspection (profiling signal and preemption signal).
    // cgo_yield is only non-nil for TSAN, and is specifically used to trigger
    // signal delivery. We use that as an indicator of delayed signals.
    // For delayed signals, the handler is called on the g0 stack (see
    // adjustSignalStack).
    var delayedSignal = cgo_yield.Value != nil && mp != nil && (~gsignal).stack == (~(~mp).g0).stack;
    if (sig == _SIGPROF) {
        // Some platforms (Linux) have per-thread timers, which we use in
        // combination with the process-wide timer. Avoid double-counting.
        if (!delayedSignal && validSIGPROF(ref (mp).DerefOrNull(), ref (c).DerefOrNull())) {
            sigprof(c.sigpc(), c.sigsp(), c.siglr(), Ꮡgp, mp);
        }
        return;
    }
    if (sig == _SIGTRAP && testSigtrap != default! && testSigtrap(Ꮡinfo, c, Ꮡgp)) {
        return;
    }
    if (sig == _SIGUSR1 && testSigusr1 != default! && testSigusr1(Ꮡgp)) {
        return;
    }
    if ((GOOS == "linux"u8 || GOOS == "android"u8) && sig == sigPerThreadSyscall) {
        // sigPerThreadSyscall is the same signal used by glibc for
        // per-thread syscalls on Linux. We use it for the same purpose
        // in non-cgo binaries. Since this signal is not _SigNotify,
        // there is nothing more to do once we run the syscall.
        runPerThreadSyscall();
        return;
    }
    if (sig == sigPreempt && debug.asyncpreemptoff == 0 && !delayedSignal) {
        // Might be a preemption signal.
        doSigPreempt(Ꮡgp, c);
    }
    // Even if this was definitely a preemption signal, it
    // may have been coalesced with another signal, so we
    // still let it through to the application.
    var flags = (int32)_SigThrow;
    if (sig < (uint32)len(sigtable)) {
        flags = sigtable[(nint)(sig)].flags;
    }
    if (!c.sigFromUser() && (int32)(flags & (int32)_SigPanic) != 0 && (gp.throwsplit || Ꮡgp != (~mp).curg)) {
        // We can't safely sigpanic because it may grow the
        // stack. Abort in the signal handler instead.
        //
        // Also don't inject a sigpanic if we are not on a
        // user G stack. Either we're in the runtime, or we're
        // running C code. Either way we cannot recover.
        flags = _SigThrow;
    }
    if (isAbortPC(c.sigpc())) {
        // On many architectures, the abort function just
        // causes a memory fault. Don't turn that into a panic.
        flags = _SigThrow;
    }
    if (!c.sigFromUser() && (int32)(flags & (int32)_SigPanic) != 0) {
        // The signal is going to cause a panic.
        // Arrange the stack so that it looks like the point
        // where the signal occurred made a call to the
        // function sigpanic. Then set the PC to sigpanic.
        // Have to pass arguments out of band since
        // augmenting the stack frame would break
        // the unwinding code.
        gp.sig = sig;
        gp.sigcode0 = (uintptr)c.sigcode();
        gp.sigcode1 = c.fault();
        gp.sigpc = c.sigpc();
        c.preparePanic(sig, Ꮡgp);
        return;
    }
    if (c.sigFromUser() || (int32)(flags & (int32)_SigNotify) != 0) {
        if (sigsend(sig)) {
            return;
        }
    }
    if (c.sigFromUser() && signal_ignored(sig)) {
        return;
    }
    if (sig == _SIGSYS && c.sigFromSeccomp() && atomic.Load(ᏑsigsysIgnored) != 0) {
        return;
    }
    if ((int32)(flags & (int32)_SigKill) != 0) {
        dieFromSignal(sig);
    }
    // _SigThrow means that we should exit now.
    // If we get here with _SigPanic, it means that the signal
    // was sent to us by a program (c.sigFromUser() is true);
    // in that case, if we didn't handle it in sigsend, we exit now.
    if ((int32)(flags & ((int32)((int32)_SigThrow | (int32)_SigPanic))) == 0) {
        return;
    }
    mp.Value.throwing = throwTypeRuntime;
    mp.of(m.Ꮡcaughtsig).set(Ꮡgp);
    if (Ꮡcrashing.Load() == 0) {
        startpanic_m();
    }
    Ꮡgp = fatalsignal(sig, c, Ꮡgp, ref (mp).DerefOrNull()); gp = ref Ꮡgp.DerefOrNull();
    var (level, _, docrash) = gotraceback();
    if (level > 0) {
        goroutineheader(Ꮡgp);
        tracebacktrap(c.sigpc(), c.sigsp(), c.siglr(), Ꮡgp);
        if (Ꮡcrashing.Load() > 0 && Ꮡgp != (~mp).curg && (~mp).curg != nil && (uint32)(readgstatus((~mp).curg) & ~(uint32)_Gscan) == _Grunning){
            // tracebackothers on original m skipped this one; trace it now.
            goroutineheader((~mp).curg);
            traceback(~(uintptr)0, ~(uintptr)0, 0, (~mp).curg);
        } else 
        if (Ꮡcrashing.Load() == 0) {
            tracebackothers(Ꮡgp);
            print((@string)"\n"u8);
        }
        dumpregs(c);
    }
    if (docrash) {
        uint32 crashSleepMicros = 5000;
        uint32 watchdogTimeoutMicros = 2000 * crashSleepMicros;
        var isCrashThread = false;
        if (Ꮡcrashing.CompareAndSwap(0, 1)){
            isCrashThread = true;
        } else {
            Ꮡcrashing.Add(1);
        }
        if (Ꮡcrashing.Load() < mcount() - (int32)ᏑextraMLength.Load()) {
            // There are other m's that need to dump their stacks.
            // Relay SIGQUIT to the next m by sending it to the current process.
            // All m's that have already received SIGQUIT have signal masks blocking
            // receipt of any signals, so the SIGQUIT will go to an m that hasn't seen it yet.
            // The first m will wait until all ms received the SIGQUIT, then crash/exit.
            // Just in case the relaying gets botched, each m involved in
            // the relay sleeps for 5 seconds and then does the crash/exit itself.
            // The faulting m is crashing first so it is the faulting thread in the core dump (see issue #63277):
            // in expected operation, the first m will wait until the last m has received the SIGQUIT,
            // and then run crash/exit and the process is gone.
            // However, if it spends more than 10 seconds to send SIGQUIT to all ms,
            // any of ms may crash/exit the process after waiting for 10 seconds.
            print((@string)"\n-----\n\n"u8);
            raiseproc(_SIGQUIT);
        }
        if (isCrashThread){
            // Sleep for short intervals so that we can crash quickly after all ms have received SIGQUIT.
            // Reset the timer whenever we see more ms received SIGQUIT
            // to make it have enough time to crash (see issue #64752).
            var timeout = watchdogTimeoutMicros;
            var maxCrashing = Ꮡcrashing.Load();
            while (timeout > 0 && (Ꮡcrashing.Load() < mcount() - (int32)ᏑextraMLength.Load())) {
                usleep(crashSleepMicros);
                timeout -= crashSleepMicros;
                {
                    var cΔ1 = Ꮡcrashing.Load(); if (cΔ1 > maxCrashing) {
                        // We make progress, so reset the watchdog timeout
                        maxCrashing = cΔ1;
                        timeout = watchdogTimeoutMicros;
                    }
                }
            }
        } else {
            var maxCrashing = (int32)0;
            var cΔ2 = Ꮡcrashing.Load();
            while (cΔ2 > maxCrashing) {
                maxCrashing = cΔ2;
                usleep(watchdogTimeoutMicros);
                cΔ2 = Ꮡcrashing.Load();
            }
        }
        printDebugLog();
        crash();
    }
    printDebugLog();
    exit(2);
}

internal static ж<g> fatalsignal(uint32 sig, ж<sigctxt> Ꮡc, ж<g> Ꮡgp, ref m mp) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var gp = ref Ꮡgp.DerefOrNull();

    if (sig < (uint32)len(sigtable)){
        print(sigtable[(nint)(sig)].name, (@string)"\n"u8);
    } else {
        print((@string)"Signal "u8, sig, (@string)"\n"u8);
    }
    if (isSecureMode()) {
        exit(2);
    }
    print((@string)"PC="u8, ((Δhex)(uint64)c.sigpc()), (@string)" m="u8, mp.id, (@string)" sigcode="u8, c.sigcode());
    if (sig == _SIGSEGV || sig == _SIGBUS) {
        print((@string)" addr="u8, ((Δhex)(uint64)c.fault()));
    }
    print((@string)"\n"u8);
    if (mp.incgo && Ꮡgp == mp.g0 && mp.curg != nil) {
        print((@string)"signal arrived during cgo execution\n"u8);
        // Switch to curg so that we get a traceback of the Go code
        // leading up to the cgocall, which switched from curg to g0.
        Ꮡgp = mp.curg; gp = ref Ꮡgp.DerefOrNull();
    }
    if (sig == _SIGILL || sig == _SIGFPE) {
        // It would be nice to know how long the instruction is.
        // Unfortunately, that's complicated to do in general (mostly for x86
        // and s930x, but other archs have non-standard instruction lengths also).
        // Opt to print 16 bytes, which covers most instructions.
        UntypedInt maxN = 16;
        var n = (uintptr)maxN;
        // We have to be careful, though. If we're near the end of
        // a page and the following page isn't mapped, we could
        // segfault. So make sure we don't straddle a page (even though
        // that could lead to printing an incomplete instruction).
        // We're assuming here we can read at least the page containing the PC.
        // I suppose it is possible that the page is mapped executable but not readable?
        var pc = c.sigpc();
        if (n > physPageSize - pc % physPageSize) {
            n = physPageSize - pc % physPageSize;
        }
        print((@string)"instruction bytes:"u8);
        var b = (ж<array<byte>>)(uintptr)((@unsafe.Pointer)pc);
        for (var i = (uintptr)0; i < n; i++) {
            print((@string)" "u8, ((Δhex)(uint64)b.Value[i]));
        }
        println();
    }
    print((@string)"\n"u8);
    return Ꮡgp;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedSignalDuringˢ = "unexpected signal during runtime execution"u8;
internal static readonly @string faultˢ = "fault"u8;
internal static readonly @string unexpectedSignalValueˢ = "unexpected signal value"u8;

// sigpanic turns a synchronous signal into a run-time panic.
// If the signal handler sees a synchronous panic, it arranges the
// stack to look like the function where the signal occurred called
// sigpanic, sets the signal's PC value to sigpanic, and returns from
// the signal handler. The effect is that the program will act as
// though the function that got the signal simply called sigpanic
// instead.
//
// This must NOT be nosplit because the linker doesn't know where
// sigpanic calls can be injected.
//
// The signal handler must not inject a call to sigpanic if
// getg().throwsplit, since sigpanic may need to grow the stack.
//
// This is exported via linkname to assembly in runtime/cgo.
//
//go:linkname sigpanic
internal static void sigpanic() {
    var gp = getg();
    if (!canpanic()) {
        @throw(unexpectedSignalDuringˢ);
    }
    var exprᴛ1 = (~gp).sig;
    if (exprᴛ1 == _SIGBUS) {
        if ((~gp).sigcode0 == _BUS_ADRERR && (~gp).sigcode1 < 0x1000) {
            panicmem();
        }
        if ((~gp).paniconfault) {
            // Support runtime/debug.SetPanicOnFault.
            panicmemAddr((~gp).sigcode1);
        }
        print((@string)"unexpected fault address "u8, ((Δhex)(uint64)(~gp).sigcode1), (@string)"\n"u8);
        @throw(faultˢ);
    }
    else if (exprᴛ1 == _SIGSEGV) {
        if (((~gp).sigcode0 == 0 || (~gp).sigcode0 == _SEGV_MAPERR || (~gp).sigcode0 == _SEGV_ACCERR) && (~gp).sigcode1 < 0x1000) {
            panicmem();
        }
        if ((~gp).paniconfault) {
            // Support runtime/debug.SetPanicOnFault.
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
    else if (exprᴛ1 == _SIGFPE) {
        var exprᴛ2 = (~gp).sigcode0;
        if (exprᴛ2 == _FPE_INTDIV) {
            panicdivide();
        }
        else if (exprᴛ2 == _FPE_INTOVF) {
            panicoverflow();
        }

        panicfloat();
    }

    if ((~gp).sig >= (uint32)len(sigtable)) {
        // can't happen: we looked up gp.sig in sigtable to decide to call sigpanic
        @throw(unexpectedSignalValueˢ);
    }
    throw panic(((errorString)sigtable[(nint)((~gp).sig)].name));
}

// dieFromSignal kills the program with a signal.
// This provides the expected exit status for the shell.
// This is only called with fatal signals expected to kill the process.
//
//go:nosplit
//go:nowritebarrierrec
internal static void dieFromSignal(uint32 sig) {
    unblocksig(sig);
    // Mark the signal as unhandled to ensure it is forwarded.
    atomic.Store(ᏑhandlingSig.at<uint32>((nint)(sig)), 0);
    raise(sig);
    // That should have killed us. On some systems, though, raise
    // sends the signal to the whole process rather than to just
    // the current thread, which means that the signal may not yet
    // have been delivered. Give other threads a chance to run and
    // pick up the signal.
    osyield();
    osyield();
    osyield();
    // If that didn't work, try _SIG_DFL.
    setsig(sig, _SIG_DFL);
    raise(sig);
    osyield();
    osyield();
    osyield();
    // If we are still somehow running, just exit with the wrong status.
    exit(2);
}

// raisebadsignal is called when a signal is received on a non-Go
// thread, and the Go program does not want to handle it (that is, the
// program has not called os/signal.Notify for the signal).
internal static void raisebadsignal(uint32 sig, ж<sigctxt> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (sig == _SIGPROF) {
        // Ignore profiling signals that arrive on non-Go threads.
        return;
    }
    uintptr handler = default!;
    int32 flags = default!;
    if (sig >= _NSIG){
        handler = _SIG_DFL;
    } else {
        handler = atomic.Loaduintptr(ᏑfwdSig.at<uintptr>((nint)(sig)));
        flags = sigtable[(nint)(sig)].flags;
    }
    // If the signal is ignored, raising the signal is no-op.
    if (handler == _SIG_IGN || (handler == _SIG_DFL && (int32)(flags & (int32)_SigIgn) != 0)) {
        return;
    }
    // Reset the signal handler and raise the signal.
    // We are currently running inside a signal handler, so the
    // signal is blocked. We need to unblock it before raising the
    // signal, or the signal we raise will be ignored until we return
    // from the signal handler. We know that the signal was unblocked
    // before entering the handler, or else we would not have received
    // it. That means that we don't have to worry about blocking it
    // again.
    unblocksig(sig);
    setsig(sig, handler);
    // If we're linked into a non-Go program we want to try to
    // avoid modifying the original context in which the signal
    // was raised. If the handler is the default, we know it
    // is non-recoverable, so we don't have to worry about
    // re-installing sighandler. At this point we can just
    // return and the signal will be re-raised and caught by
    // the default handler with the correct context.
    //
    // On FreeBSD, the libthr sigaction code prevents
    // this from working so we fall through to raise.
    if (GOOS != "freebsd"u8 && (isarchive || islibrary) && handler == _SIG_DFL && !c.sigFromUser()) {
        return;
    }
    raise(sig);
    // Give the signal a chance to be delivered.
    // In almost all real cases the program is about to crash,
    // so sleeping here is not a waste of time.
    usleep(1000);
    // If the signal didn't cause the program to exit, restore the
    // Go signal handler and carry on.
    //
    // We may receive another instance of the signal before we
    // restore the Go handler, but that is not so bad: we know
    // that the Go program has been ignoring the signal.
    setsig(sig, abi.FuncPCABIInternal(sighandler));
}

//go:nosplit
internal static void crash() {
    dieFromSignal(_SIGABRT);
}

// ensureSigM starts one global, sleeping thread to make sure at least one thread
// is available to catch signals enabled for os/signal.
internal static void ensureSigM() {
    if (maskUpdatedChan != default!) {
        return;
    }
    maskUpdatedChan = new channel<EmptyStruct>(0);
    disableSigChan = new channel<uint32>(0);
    enableSigChan = new channel<uint32>(0);
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            // Signal masks are per-thread, so make sure this goroutine stays on one
            // thread.
            LockOSThread();
            defer(UnlockOSThread, ref ᒐ);
            // The sigBlocked mask contains the signals not active for os/signal,
            // initially all signals except the essential. When signal.Notify()/Stop is called,
            // sigenable/sigdisable in turn notify this thread to update its signal
            // mask accordingly.
            ref var sigBlocked = ref heap<sigset>(out var ᏑsigBlocked);
            sigBlocked = sigset_all;
            foreach (var (i, _) in sigtable) {
                if (!blockableSig((uint32)i)) {
                    sigdelset(ref (ᏑsigBlocked).DerefOrNull(), i);
                }
            }
            sigprocmask(_SIG_SETMASK, ᏑsigBlocked, nil);
            while (ᐧ) {
                var selᴛ2 = enableSigChan;
                var selᴛ3 = disableSigChan;
                switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
                case 0 when selᴛ2.ꟷᐳ(out var sig): {
                    if (sig > 0) {
                        sigdelset(ref (ᏑsigBlocked).DerefOrNull(), (nint)sig);
                    }
                    break;
                }
                case 1 when selᴛ3.ꟷᐳ(out var sig): {
                    if (sig > 0 && blockableSig(sig)) {
                        sigaddset(ref (ᏑsigBlocked).DerefOrNull(), (nint)sig);
                    }
                    break;
                }}
                sigprocmask(_SIG_SETMASK, ᏑsigBlocked, nil);
                maskUpdatedChan.ᐸꟷ(new EmptyStruct());
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonGoCodeDisabledˢ = "non-Go code disabled sigaltstack"u8;

// This is called when we receive a signal when there is no signal stack.
// This can only happen if non-Go code calls sigaltstack to disable the
// signal stack.
internal static void noSignalStack(uint32 sig) {
    println((@string)"signal"u8, sig, (@string)"received on thread with no signal stack"u8);
    @throw(nonGoCodeDisabledˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonGoCodeSetUpSignalˢ = "non-Go code set up signal handler without SA_ONSTACK flag"u8;

// This is called if we receive a signal when there is a signal stack
// but we are not on it. This can only happen if non-Go code called
// sigaction without setting the SS_ONSTACK flag.
internal static void sigNotOnStack(uint32 sig, uintptr sp, ref m mp) {
    println((@string)"signal"u8, sig, (@string)"received but handler not on signal stack"u8);
    print((@string)"mp.gsignal stack ["u8, ((Δhex)(uint64)(~mp.gsignal).stack.lo), (@string)" "u8, ((Δhex)(uint64)(~mp.gsignal).stack.hi), (@string)"], "u8);
    print((@string)"mp.g0 stack ["u8, ((Δhex)(uint64)(~mp.g0).stack.lo), (@string)" "u8, ((Δhex)(uint64)(~mp.g0).stack.hi), (@string)"], sp="u8, ((Δhex)(uint64)sp), (@string)"\n"u8);
    @throw(nonGoCodeSetUpSignalˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string signalReceivedDuringForkˢ = "signal received during fork"u8;

// signalDuringFork is called if we receive a signal while doing a fork.
// We do not want signals at that time, as a signal sent to the process
// group may be delivered to the child process, causing confusion.
// This should never be called, because we block signals across the fork;
// this function is just a safety check. See issue 18600 for background.
internal static void signalDuringFork(uint32 sig) {
    println((@string)"signal"u8, sig, (@string)"received during fork"u8);
    @throw(signalReceivedDuringForkˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fatalBadGInSignalHandlerˢ = "fatal: bad g in signal handler\n"u8;

// This runs on a foreign stack, without an m or a g. No stack split.
//
//go:nosplit
//go:norace
//go:nowritebarrierrec
internal static void badsignal(uintptr sig, ж<sigctxt> Ꮡc) {
    if (!iscgo && !cgoHasExtraM) {
        // There is no extra M. needm will not be able to grab
        // an M. Instead of hanging, just crash.
        // Cannot call split-stack function as there is no G.
        writeErrStr(fatalBadGInSignalHandlerˢ);
        exit(2);
        ((ж<uintptr>)(uintptr)((@unsafe.Pointer)(uintptr)123)).Value = 2;
    }
    needm(true);
    if (!sigsend((uint32)sig)) {
        // A foreign thread received the signal sig, and the
        // Go code does not want to handle it.
        raisebadsignal((uint32)sig, Ꮡc);
    }
    dropm();
}

//go:noescape
internal static partial void sigfwd(uintptr fn, uint32 sig, ж<siginfo> info, @unsafe.Pointer ctx);

// Determines if the signal should be handled by Go and if not, forwards the
// signal to the handler that was installed before Go's. Returns whether the
// signal was forwarded.
// This is called by the signal handler, and the world may be stopped.
//
//go:nosplit
//go:nowritebarrierrec
internal static bool sigfwdgo(uint32 sig, ж<siginfo> Ꮡinfo, @unsafe.Pointer ctx) {
    if (sig >= (uint32)len(sigtable)) {
        return false;
    }
    var fwdFn = atomic.Loaduintptr(ᏑfwdSig.at<uintptr>((nint)(sig)));
    var flags = sigtable[(nint)(sig)].flags;
    // If we aren't handling the signal, forward it.
    if (atomic.Load(ᏑhandlingSig.at<uint32>((nint)(sig))) == 0 || !signalsOK) {
        // If the signal is ignored, doing nothing is the same as forwarding.
        if (fwdFn == _SIG_IGN || (fwdFn == _SIG_DFL && (int32)(flags & (int32)_SigIgn) != 0)) {
            return true;
        }
        // We are not handling the signal and there is no other handler to forward to.
        // Crash with the default behavior.
        if (fwdFn == _SIG_DFL) {
            setsig(sig, _SIG_DFL);
            dieFromSignal(sig);
            return false;
        }
        sigfwd(fwdFn, sig, Ꮡinfo, ctx);
        return true;
    }
    // This function and its caller sigtrampgo assumes SIGPIPE is delivered on the
    // originating thread. This property does not hold on macOS (golang.org/issue/33384),
    // so we have no choice but to ignore SIGPIPE.
    if ((GOOS == "darwin"u8 || GOOS == "ios"u8) && sig == _SIGPIPE) {
        return true;
    }
    // If there is no handler to forward to, no need to forward.
    if (fwdFn == _SIG_DFL) {
        return false;
    }
    var c = Ꮡ(new sigctxt(Ꮡinfo, ctx.Value));
    // Only forward synchronous signals and SIGPIPE.
    // Unfortunately, user generated SIGPIPEs will also be forwarded, because si_code
    // is set to _SI_USER even for a SIGPIPE raised from a write to a closed socket
    // or pipe.
    if ((c.sigFromUser() || (int32)(flags & (int32)_SigPanic) == 0) && sig != _SIGPIPE) {
        return false;
    }
    // Determine if the signal occurred inside Go code. We test that:
    //   (1) we weren't in VDSO page,
    //   (2) we were in a goroutine (i.e., m.curg != nil), and
    //   (3) we weren't in CGO.
    //   (4) we weren't in dropped extra m.
    var gp = sigFetchG(c);
    if (gp != nil && (~gp).m != nil && (~(~gp).m).curg != nil && !(~(~gp).m).isExtraInC && !(~(~gp).m).incgo) {
        return false;
    }
    // Signal not handled by Go, forward it.
    if (fwdFn != _SIG_IGN) {
        sigfwd(fwdFn, sig, Ꮡinfo, ctx);
    }
    return true;
}

// sigsave saves the current thread's signal mask into *p.
// This is used to preserve the non-Go signal mask when a non-Go
// thread calls a Go function.
// This is nosplit and nowritebarrierrec because it is called by needm
// which may be called on a non-Go thread with no g available.
//
//go:nosplit
//go:nowritebarrierrec
internal static void sigsave(ж<sigset> Ꮡp) {
    sigprocmask(_SIG_SETMASK, nil, Ꮡp);
}

// msigrestore sets the current thread's signal mask to sigmask.
// This is used to restore the non-Go signal mask when a non-Go thread
// calls a Go function.
// This is nosplit and nowritebarrierrec because it is called by dropm
// after g has been cleared.
//
//go:nosplit
//go:nowritebarrierrec
internal static void msigrestore(sigset sigmaskʗp) {
    ref var sigmask = ref heap(sigmaskʗp, out var Ꮡsigmask);

    sigprocmask(_SIG_SETMASK, Ꮡsigmask, nil);
}

// Apply GOOS-specific overrides here, rather than in osinit,
// because osinit may be called before sigsetAllExiting is
// initialized (#51913).
// #42494 glibc and musl reserve some signals for
// internal use and require they not be blocked by
// the rest of a normal C runtime. When the go runtime
// blocks...unblocks signals, temporarily, the blocked
// interval of time is generally very short. As such,
// these expectations of *libc code are mostly met by
// the combined go+cgo system of threads. However,
// when go causes a thread to exit, via a return from
// mstart(), the combined runtime can deadlock if
// these signals are blocked. Thus, don't block these
// signals when exiting threads.
// - glibc: SIGCANCEL (32), SIGSETXID (33)
// - musl: SIGTIMER (32), SIGCANCEL (33), SIGSYNCCALL (34)
// sigsetAllExiting is used by sigblock(true) when a thread is
// exiting.
internal static ж<sigset> ᏑsigsetAllExiting = new StandardBox<sigset>(default(sigset));
internal static ref sigset sigsetAllExiting => ref ᏑsigsetAllExiting.Value;
internal static void initᴛsigsetAllExiting() { sigsetAllExiting = ((Func<sigset>)(() => {
    ref var res = ref heap<sigset>(out var Ꮡres);
    res = sigset_all;
    if (GOOS == "linux"u8 && iscgo) {
        sigdelset(ref (Ꮡres).DerefOrNull(), 32);
        sigdelset(ref (Ꮡres).DerefOrNull(), 33);
        sigdelset(ref (Ꮡres).DerefOrNull(), 34);
    }
    return res;
}))(); }

// sigblock blocks signals in the current thread's signal mask.
// This is used to block signals while setting up and tearing down g
// when a non-Go thread calls a Go function. When a thread is exiting
// we use the sigsetAllExiting value, otherwise the OS specific
// definition of sigset_all is used.
// This is nosplit and nowritebarrierrec because it is called by needm
// which may be called on a non-Go thread with no g available.
//
//go:nosplit
//go:nowritebarrierrec
internal static void sigblock(bool exiting) {
    if (exiting) {
        sigprocmask(_SIG_SETMASK, ᏑsigsetAllExiting, nil);
        return;
    }
    sigprocmask(_SIG_SETMASK, Ꮡsigset_all, nil);
}

// unblocksig removes sig from the current thread's signal mask.
// This is nosplit and nowritebarrierrec because it is called from
// dieFromSignal, which can be called by sigfwdgo while running in the
// signal handler, on the signal stack, with no g available.
//
//go:nosplit
//go:nowritebarrierrec
internal static void unblocksig(uint32 sig) {
    ref var set = ref heap(new sigset(), out var Ꮡset);
    sigaddset(ref set, (nint)sig);
    sigprocmask(_SIG_UNBLOCK, Ꮡset, nil);
}

// minitSignals is called when initializing a new m to set the
// thread's alternate signal stack and signal mask.
internal static void minitSignals() {
    minitSignalStack();
    minitSignalMask();
}

// minitSignalStack is called when initializing a new m to set the
// alternate signal stack. If the alternate signal stack is not set
// for the thread (the normal case) then set the alternate signal
// stack to the gsignal stack. If the alternate signal stack is set
// for the thread (the case when a non-Go thread sets the alternate
// signal stack and then calls a Go function) then set the gsignal
// stack to the alternate signal stack. We also set the alternate
// signal stack to the gsignal stack if cgo is not used (regardless
// of whether it is already set). Record which choice was made in
// newSigstack, so that it can be undone in unminit.
internal static void minitSignalStack() {
    var mp = getg().Value.m;
    ref var st = ref heap(new stackt(), out var Ꮡst);
    sigaltstack(nil, Ꮡst);
    if ((int32)(st.ss_flags & (int32)_SS_DISABLE) != 0 || !iscgo){
        signalstack((~mp).gsignal.of(g.Ꮡstack));
        mp.Value.newSigstack = true;
    } else {
        setGsignalStack(ref st, mp.of(m.ᏑgoSigStack));
        mp.Value.newSigstack = false;
    }
}

// minitSignalMask is called when initializing a new m to set the
// thread's signal mask. When this is called all signals have been
// blocked for the thread.  This starts with m.sigmask, which was set
// either from initSigmask for a newly created thread or by calling
// sigsave if this is a non-Go thread calling a Go function. It
// removes all essential signals from the mask, thus causing those
// signals to not be blocked. Then it sets the thread's signal mask.
// After this is called the thread can receive signals.
internal static void minitSignalMask() {
    ref var nmask = ref heap<sigset>(out var Ꮡnmask);
    nmask = getg().Value.m.Value.sigmask;
    foreach (var (i, _) in sigtable) {
        if (!blockableSig((uint32)i)) {
            sigdelset(ref nmask, i);
        }
    }
    sigprocmask(_SIG_SETMASK, Ꮡnmask, nil);
}

// unminitSignals is called from dropm, via unminit, to undo the
// effect of calling minit on a non-Go thread.
//
//go:nosplit
internal static void unminitSignals() {
    if ((~(~getg()).m).newSigstack){
        ref var st = ref heap<stackt>(out var Ꮡst);
        st = new stackt(ss_flags: _SS_DISABLE);
        sigaltstack(Ꮡst, nil);
    } else {
        // We got the signal stack from someone else. Restore
        // the Go-allocated stack in case this M gets reused
        // for another thread (e.g., it's an extram). Also, on
        // Android, libc allocates a signal stack for all
        // threads, so it's important to restore the Go stack
        // even on Go-created threads so we can free it.
        restoreGsignalStack((~getg()).m.of(m.ᏑgoSigStack));
    }
}

// blockableSig reports whether sig may be blocked by the signal mask.
// We never want to block the signals marked _SigUnblock;
// these are the synchronous signals that turn into a Go panic.
// We never want to block the preemption signal if it is being used.
// In a Go program--not a c-archive/c-shared--we never want to block
// the signals marked _SigKill or _SigThrow, as otherwise it's possible
// for all running threads to block them and delay their delivery until
// we start a new thread. When linked into a C program we let the C code
// decide on the disposition of those signals.
internal static bool blockableSig(uint32 sig) {
    var flags = sigtable[(nint)(sig)].flags;
    if ((int32)(flags & (int32)_SigUnblock) != 0) {
        return false;
    }
    if (sig == sigPreempt && preemptMSupported && debug.asyncpreemptoff == 0) {
        return false;
    }
    if (isarchive || islibrary) {
        return true;
    }
    return (int32)(flags & ((int32)((int32)_SigKill | (int32)_SigThrow))) == 0;
}

// gsignalStack saves the fields of the gsignal stack changed by
// setGsignalStack.
[GoType] partial struct gsignalStack {
    internal Δstack stack;
    internal uintptr stackguard0;
    internal uintptr stackguard1;
    internal uintptr stktopsp;
}

// setGsignalStack sets the gsignal stack of the current m to an
// alternate signal stack returned from the sigaltstack system call.
// It saves the old values in *old for use by restoreGsignalStack.
// This is used when handling a signal if non-Go code has set the
// alternate signal stack.
//
//go:nosplit
//go:nowritebarrierrec
internal static void setGsignalStack(ref stackt st, ж<gsignalStack> Ꮡold) {
    ref var old = ref Ꮡold.DerefOrNull();

    var gp = getg();
    if (Ꮡold != nil) {
        old.stack = gp.Value.m.Value.gsignal.Value.stack;
        old.stackguard0 = gp.Value.m.Value.gsignal.Value.stackguard0;
        old.stackguard1 = gp.Value.m.Value.gsignal.Value.stackguard1;
        old.stktopsp = gp.Value.m.Value.gsignal.Value.stktopsp;
    }
    var stsp = (uintptr)st.ss_sp;
    gp.Value.m.Value.gsignal.Value.stack.lo = stsp;
    gp.Value.m.Value.gsignal.Value.stack.hi = stsp + st.ss_size;
    gp.Value.m.Value.gsignal.Value.stackguard0 = stsp + (uintptr)stackGuard;
    gp.Value.m.Value.gsignal.Value.stackguard1 = stsp + (uintptr)stackGuard;
}

// restoreGsignalStack restores the gsignal stack to the value it had
// before entering the signal handler.
//
//go:nosplit
//go:nowritebarrierrec
internal static void restoreGsignalStack(ж<gsignalStack> Ꮡst) {
    ref var st = ref Ꮡst.DerefOrNull();

    var gp = getg().Value.m.Value.gsignal;
    gp.Value.stack = st.stack;
    gp.Value.stackguard0 = st.stackguard0;
    gp.Value.stackguard1 = st.stackguard1;
    gp.Value.stktopsp = st.stktopsp;
}

// signalstack sets the current thread's alternate signal stack to s.
//
//go:nosplit
internal static void signalstack(ж<Δstack> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    ref var st = ref heap<stackt>(out var Ꮡst);
    st = new stackt(ss_size: s.hi - s.lo);
    setSignalstackSP(Ꮡst, s.lo);
    sigaltstack(Ꮡst, nil);
}

// setsigsegv is used on darwin/arm64 to fake a segmentation fault.
//
// This is exported via linkname to assembly in runtime/cgo.
//
//go:nosplit
//go:linkname setsigsegv
internal static void setsigsegv(uintptr pc) {
    var gp = getg();
    gp.Value.sig = _SIGSEGV;
    gp.Value.sigpc = pc;
    gp.Value.sigcode0 = _SEGV_MAPERR;
    gp.Value.sigcode1 = 0; // TODO: emulate si_addr
}

} // end runtime_package
