// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using stringslite = @internal.stringslite_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

partial struct mOS {
    internal bool initialized;
    internal pthreadmutex mutex;
    internal pthreadcond cond;
    internal nint count;
}

internal static void unimplemented(@string name) {
    println(name, (@string)"not implemented"u8);
    ((ж<nint>)(uintptr)((@unsafe.Pointer)(uintptr)1231)).Value = 1231;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pthreadMutexInitˢ = "pthread_mutex_init"u8;
internal static readonly @string pthreadCondInitˢ = "pthread_cond_init"u8;

//go:nosplit
internal static void semacreate(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    if (mp.initialized) {
        return;
    }
    mp.initialized = true;
    {
        var err = pthread_mutex_init(Ꮡmp.of(m.Ꮡmutex), nil); if (err != 0) {
            @throw(pthreadMutexInitˢ);
        }
    }
    {
        var err = pthread_cond_init(Ꮡmp.of(m.Ꮡcond), nil); if (err != 0) {
            @throw(pthreadCondInitˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string semasleepOnDarwinSignalˢ = "semasleep on Darwin signal stack"u8;

//go:nosplit
internal static int32 semasleep(int64 ns) {
    int64 start = default!;
    if (ns >= 0) {
        start = nanotime();
    }
    var g = getg();
    var mp = g.Value.m;
    if (g == (~mp).gsignal) {
        // sema sleep/wakeup are implemented with pthreads, which are not async-signal-safe on Darwin.
        @throw(semasleepOnDarwinSignalˢ);
    }
    pthread_mutex_lock(mp.of(m.Ꮡmutex));
    while (ᐧ) {
        if ((~mp).count > 0) {
            mp.Value.count--;
            pthread_mutex_unlock(mp.of(m.Ꮡmutex));
            return 0;
        }
        if (ns >= 0){
            var spent = nanotime() - start;
            if (spent >= ns) {
                pthread_mutex_unlock(mp.of(m.Ꮡmutex));
                return -1;
            }
            ref var t = ref heap(new timespec(), out var Ꮡt);
            t.setNsec(ns - spent);
            var err = pthread_cond_timedwait_relative_np(mp.of(m.Ꮡcond), mp.of(m.Ꮡmutex), Ꮡt);
            if (err == _ETIMEDOUT) {
                pthread_mutex_unlock(mp.of(m.Ꮡmutex));
                return -1;
            }
        } else {
            pthread_cond_wait(mp.of(m.Ꮡcond), mp.of(m.Ꮡmutex));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string semawakeupOnDarwinSignalˢ = "semawakeup on Darwin signal stack"u8;

//go:nosplit
internal static void semawakeup(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    {
        var g = getg(); if (g == (~(~g).m).gsignal) {
            @throw(semawakeupOnDarwinSignalˢ);
        }
    }
    pthread_mutex_lock(Ꮡmp.of(m.Ꮡmutex));
    mp.count++;
    if (mp.count > 0) {
        pthread_cond_signal(Ꮡmp.of(m.Ꮡcond));
    }
    pthread_mutex_unlock(Ꮡmp.of(m.Ꮡmutex));
}

// The read and write file descriptors used by the sigNote functions.
internal static int32 sigNoteRead;
internal static int32 sigNoteWrite;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string duplicateSigNoteSetupˢ = "duplicate sigNoteSetup"u8;
internal static readonly @string pipeFailedˢ = "pipe failed"u8;

// sigNoteSetup initializes a single, there-can-only-be-one, async-signal-safe note.
//
// The current implementation of notes on Darwin is not async-signal-safe,
// because the functions pthread_mutex_lock, pthread_cond_signal, and
// pthread_mutex_unlock, called by semawakeup, are not async-signal-safe.
// There is only one case where we need to wake up a note from a signal
// handler: the sigsend function. The signal handler code does not require
// all the features of notes: it does not need to do a timed wait.
// This is a separate implementation of notes, based on a pipe, that does
// not support timed waits but is async-signal-safe.
internal static void sigNoteSetup(ж<note> _) {
    if (sigNoteRead != 0 || sigNoteWrite != 0) {
        // Generalizing this would require avoiding the pipe-fork-closeonexec race, which entangles syscall.
        @throw(duplicateSigNoteSetupˢ);
    }
    int32 errno = default!;
    (sigNoteRead, sigNoteWrite, errno) = pipe();
    if (errno != 0) {
        @throw(pipeFailedˢ);
    }
    closeonexec(sigNoteRead);
    closeonexec(sigNoteWrite);
    // Make the write end of the pipe non-blocking, so that if the pipe
    // buffer is somehow full we will not block in the signal handler.
    // Leave the read end of the pipe blocking so that we will block
    // in sigNoteSleep.
    setNonblock(sigNoteWrite);
}

// sigNoteWakeup wakes up a thread sleeping on a note created by sigNoteSetup.
internal static void sigNoteWakeup(ж<note> _) {
    ref var b = ref heap(new byte(), out var Ꮡb);
    write((uintptr)sigNoteWrite, @unsafe.Pointer.FromPinnedBox(Ꮡb), 1);
}

// sigNoteSleep waits for a note created by sigNoteSetup to be woken.
internal static void sigNoteSleep(ж<note> _) {
    while (ᐧ) {
        ref var b = ref heap(new byte(), out var Ꮡb);
        entersyscallblock();
        var n = read(sigNoteRead, @unsafe.Pointer.FromPinnedBox(Ꮡb), 1);
        exitsyscall();
        if (n != (int32)(-_EINTR)) {
            return;
        }
    }
}

// BSD interface for threading.
internal static void osinit() {
    // pthread_create delayed until end of goenvs so that we
    // can look at the environment first.
    ncpu = getncpu();
    physPageSize = getPageSize();
    osinit_hack();
}

internal static (int32, int32) sysctlbynameInt32(slice<byte> name) {
    ref var @out = ref heap<int32>(out var Ꮡout);
    @out = (int32)0;
    ref var nout = ref heap<uintptr>(out var Ꮡnout);
    nout = /* unsafe.Sizeof(out) */ (uintptr)4;
    var ret = sysctlbyname(Ꮡ(name, 0), Ꮡout.Reinterpret<int32, byte>(), Ꮡnout, nil, 0);
    return (ret, @out);
}

//go:linkname internal_cpu_getsysctlbyname internal/cpu.getsysctlbyname
internal static (int32, int32) internal_cpu_getsysctlbyname(slice<byte> name) {
    return sysctlbynameInt32(name);
}

internal static UntypedInt _CTL_HW => 6;
internal static UntypedInt _HW_NCPU => 3;
internal static UntypedInt _HW_PAGESIZE => 7;

internal static int32 getncpu() {
    // Use sysctl to fetch hw.ncpu.
    ref var mib = ref heap<array<uint32>>(out var Ꮡmib);
    mib = new uint32[]{_CTL_HW, _HW_NCPU}.array();
    ref var @out = ref heap<uint32>(out var Ꮡout);
    @out = (uint32)0;
    ref var nout = ref heap<uintptr>(out var Ꮡnout);
    nout = /* unsafe.Sizeof(out) */ (uintptr)4;
    var ret = sysctl(Ꮡmib.at<uint32>(0), 2, Ꮡout.Reinterpret<uint32, byte>(), Ꮡnout, nil, 0);
    if (ret >= 0 && (int32)@out > 0) {
        return (int32)@out;
    }
    return 1;
}

internal static uintptr getPageSize() {
    // Use sysctl to fetch hw.pagesize.
    ref var mib = ref heap<array<uint32>>(out var Ꮡmib);
    mib = new uint32[]{_CTL_HW, _HW_PAGESIZE}.array();
    ref var @out = ref heap<uint32>(out var Ꮡout);
    @out = (uint32)0;
    ref var nout = ref heap<uintptr>(out var Ꮡnout);
    nout = /* unsafe.Sizeof(out) */ (uintptr)4;
    var ret = sysctl(Ꮡmib.at<uint32>(0), 2, Ꮡout.Reinterpret<uint32, byte>(), Ꮡnout, nil, 0);
    if (ret >= 0 && (int32)@out > 0) {
        return (uintptr)@out;
    }
    return 0;
}

//go:nosplit
internal static nint readRandom(slice<byte> r) {
    arc4random_buf(@unsafe.Pointer.FromPinnedBox(Ꮡ(r, 0)), (int32)len(r));
    return len(r);
}

internal static void goenvs() {
    goenvs_unix();
}

// May run with m.p==nil, so write barriers are not allowed.
//
//go:nowritebarrierrec
internal static void newosproc(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    @unsafe.Pointer stk = (@unsafe.Pointer)(~mp.g0).stack.hi;
    if (false) {
        print((@string)"newosproc stk="u8, @unsafe.Pointer.OrTypedNil(stk), (@string)" m="u8, Ꮡmp.OrTypedNil(), (@string)" g="u8, mp.g0.OrTypedNil(), (@string)" id="u8, mp.id, (@string)" ostk="u8, Ꮡ(mp), (@string)"\n"u8);
    }
    // Initialize an attribute object.
    ref var attr = ref heap(new pthreadattr(), out var Ꮡattr);
    int32 err = default!;
    err = pthread_attr_init(Ꮡattr);
    if (err != 0) {
        writeErrStr(failthreadcreate);
        exit(1);
    }
    // Find out OS stack size for our own stack guard.
    ref var stacksize = ref heap(new uintptr(), out var Ꮡstacksize);
    if (pthread_attr_getstacksize(Ꮡattr, Ꮡstacksize) != 0) {
        writeErrStr(failthreadcreate);
        exit(1);
    }
    mp.g0.Value.stack.hi = stacksize; // for mstart
    // Tell the pthread library we won't join with this thread.
    if (pthread_attr_setdetachstate(Ꮡattr, _PTHREAD_CREATE_DETACHED) != 0) {
        writeErrStr(failthreadcreate);
        exit(1);
    }
    // Finally, create the thread. It starts at mstart_stub, which does some low-level
    // setup and then calls mstart.
    ref var oset = ref heap(new sigset(), out var Ꮡoset);
    sigprocmask(_SIG_SETMASK, Ꮡsigset_all, Ꮡoset);
    err = retryOnEAGAIN(() => pthread_create(Ꮡattr, abi.FuncPCABI0(mstart_stub), @unsafe.Pointer.FromPinnedBox(Ꮡmp)));
    sigprocmask(_SIG_SETMASK, Ꮡoset, nil);
    if (err != 0) {
        writeErrStr(failthreadcreate);
        exit(1);
    }
}

// glue code to call mstart from pthread_create.
internal static partial void mstart_stub();

// newosproc0 is a version of newosproc that can be called before the runtime
// is initialized.
//
// This function is not safe to use after initialization as it does not pass an M as fnarg.
//
//go:nosplit
internal static void newosproc0(uintptr stacksizeʗp, uintptr fn) {
    ref var stacksize = ref heap(stacksizeʗp, out var Ꮡstacksize);

    // Initialize an attribute object.
    ref var attr = ref heap(new pthreadattr(), out var Ꮡattr);
    int32 err = default!;
    err = pthread_attr_init(Ꮡattr);
    if (err != 0) {
        writeErrStr(failthreadcreate);
        exit(1);
    }
    // The caller passes in a suggested stack size,
    // from when we allocated the stack and thread ourselves,
    // without libpthread. Now that we're using libpthread,
    // we use the OS default stack size instead of the suggestion.
    // Find out that stack size for our own stack guard.
    if (pthread_attr_getstacksize(Ꮡattr, Ꮡstacksize) != 0) {
        writeErrStr(failthreadcreate);
        exit(1);
    }
    g0.stack.hi = stacksize; // for mstart
    Ꮡmemstats.of(mstats.Ꮡstacks_sys).add((int64)stacksize);
    // Tell the pthread library we won't join with this thread.
    if (pthread_attr_setdetachstate(Ꮡattr, _PTHREAD_CREATE_DETACHED) != 0) {
        writeErrStr(failthreadcreate);
        exit(1);
    }
    // Finally, create the thread. It starts at mstart_stub, which does some low-level
    // setup and then calls mstart.
    ref var oset = ref heap(new sigset(), out var Ꮡoset);
    sigprocmask(_SIG_SETMASK, Ꮡsigset_all, Ꮡoset);
    err = pthread_create(Ꮡattr, fn, nil);
    sigprocmask(_SIG_SETMASK, Ꮡoset, nil);
    if (err != 0) {
        writeErrStr(failthreadcreate);
        exit(1);
    }
}

// Called to do synchronous initialization of Go code built with
// -buildmode=c-archive or -buildmode=c-shared.
// None of the Go runtime is initialized.
//
//go:nosplit
//go:nowritebarrierrec
internal static void libpreinit() {
    initsig(true);
}

// Called to initialize a new m (including the bootstrap m).
// Called on the parent thread (main thread in case of bootstrap), can allocate memory.
internal static void mpreinit(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    mp.gsignal = malg(32 * 1024); // OS X wants >= 8K
    mp.gsignal.Value.m = Ꮡmp;
    if (GOOS == "darwin"u8 && GOARCH == "arm64"u8) {
        // mlock the signal stack to work around a kernel bug where it may
        // SIGILL when the signal stack is not faulted in while a signal
        // arrives. See issue 42774.
        mlock((@unsafe.Pointer)((~mp.gsignal).stack.hi - physPageSize), physPageSize);
    }
}

// Called to initialize a new m (including the bootstrap m).
// Called on the new thread, cannot allocate memory.
internal static void minit() {
    // iOS does not support alternate signal stack.
    // The signal handler handles it directly.
    if (!(GOOS == "ios"u8 && GOARCH == "arm64"u8)) {
        minitSignalStack();
    }
    minitSignalMask();
    getg().Value.m.Value.procid = (uint64)(uintptr)pthread_self();
}

// Called from dropm to undo the effect of an minit.
//
//go:nosplit
internal static void unminit() {
    // iOS does not support alternate signal stack.
    // See minit.
    if (!(GOOS == "ios"u8 && GOARCH == "arm64"u8)) {
        unminitSignals();
    }
    getg().Value.m.Value.procid = 0;
}

// Called from mexit, but not from dropm, to undo the effect of thread-owned
// resources in minit, semacreate, or elsewhere. Do not take locks after calling this.
//
// This always runs without a P, so //go:nowritebarrierrec is required.
//go:nowritebarrierrec
internal static void mdestroy(ref m mp) {
}

//go:nosplit
internal static void osyield_no_g() {
    usleep_no_g(1);
}

//go:nosplit
internal static void osyield() {
    usleep(1);
}

internal static UntypedInt _NSIG => 32;
internal static UntypedInt _SI_USER => 0; /* empirically true, but not what headers say */
internal static UntypedInt _SIG_BLOCK => 1;
internal static UntypedInt _SIG_UNBLOCK => 2;
internal static UntypedInt _SIG_SETMASK => 3;
internal static UntypedInt _SS_DISABLE => 4;

partial struct sigset /*num:uint32*/;

//extern SigTabTT runtime·sigtab[];
internal static ж<sigset> Ꮡsigset_all = new StandardBox<sigset>(~((sigset)((sigset)0)));
internal static ref sigset sigset_all => ref Ꮡsigset_all.Value;

//go:nosplit
//go:nowritebarrierrec
internal static void setsig(uint32 i, uintptr fn) {
    ref var sa = ref heap(new usigactiont(), out var Ꮡsa);
    sa.sa_flags = (int32)((UntypedInt)(_SA_SIGINFO | _SA_ONSTACK) | (int32)_SA_RESTART);
    sa.sa_mask = ~(uint32)0;
    if (fn == abi.FuncPCABIInternal(sighandler)) {
        // abi.FuncPCABIInternal(sighandler) matches the callers in signal_unix.go
        if (iscgo){
            fn = abi.FuncPCABI0(cgoSigtramp);
        } else {
            fn = abi.FuncPCABI0(sigtramp);
        }
    }
    (Ꮡsa.of(usigactiont.Ꮡ__sigaction_u).Reinterpret<array<byte>, uintptr>()).Value = fn;
    sigaction(i, Ꮡsa, nil);
}

// sigtramp is the callback from libc when a signal is received.
// It is called with the C calling convention.
internal static partial void sigtramp();

internal static partial void cgoSigtramp();

//go:nosplit
//go:nowritebarrierrec
internal static void setsigstack(uint32 i) {
    ref var osa = ref heap(new usigactiont(), out var Ꮡosa);
    sigaction(i, nil, Ꮡosa);
    var handler = ~Ꮡosa.of(usigactiont.Ꮡ__sigaction_u).Reinterpret<array<byte>, uintptr>();
    if ((int32)(osa.sa_flags & (int32)_SA_ONSTACK) != 0) {
        return;
    }
    ref var sa = ref heap(new usigactiont(), out var Ꮡsa);
    (Ꮡsa.of(usigactiont.Ꮡ__sigaction_u).Reinterpret<array<byte>, uintptr>()).Value = handler;
    sa.sa_mask = osa.sa_mask;
    sa.sa_flags = (int32)(osa.sa_flags | (int32)_SA_ONSTACK);
    sigaction(i, Ꮡsa, nil);
}

//go:nosplit
//go:nowritebarrierrec
internal static uintptr getsig(uint32 i) {
    ref var sa = ref heap(new usigactiont(), out var Ꮡsa);
    sigaction(i, nil, Ꮡsa);
    return ~Ꮡsa.of(usigactiont.Ꮡ__sigaction_u).Reinterpret<array<byte>, uintptr>();
}

// setSignalstackSP sets the ss_sp field of a stackt.
//
//go:nosplit
internal static void setSignalstackSP(ж<stackt> Ꮡs, uintptr sp) {
    (Ꮡs.of(stackt.Ꮡss_sp).Reinterpret<ж<byte>, uintptr>()).Value = sp;
}

//go:nosplit
//go:nowritebarrierrec
internal static void sigaddset(ref sigset mask, nint i) {
    mask |= (sigset)((sigset)((uint32)1 << (int)(((uint32)i - 1))));
}

internal static void sigdelset(ref sigset mask, nint i) {
    mask &= unchecked((sigset)~(sigset)((sigset)((uint32)1 << (int)(((uint32)i - 1)))));
}

// go2cs generated this placeholder — func setProcessCPUProfiler is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func setThreadCPUProfiler is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:nosplit
internal static bool validSIGPROF(ref m mp, ref sigctxt c) {
    return true;
}

//go:linkname executablePath os.executablePath
public static @string executablePath;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string executablePathˢ = "executable_path="u8;

internal static void sysargs(int32 argc, ж<ж<byte>> Ꮡargv) {
    ref var argv = ref Ꮡargv.DerefOrNull();

    // skip over argv, envv and the first string will be the path
    var n = argc + 1;
    while (argv_index(Ꮡargv, n) != nil) {
        n++;
    }
    executablePath = gostringnocopy(argv_index(Ꮡargv, n + 1));
    // strip "executable_path=" prefix if available, it's added after OS X 10.11.
    executablePath = stringslite.TrimPrefix(executablePath, executablePathˢ);
}

internal static void signalM(ref m mp, nint sig) {
    pthread_kill(((pthread)(uintptr)mp.procid), (uint32)sig);
}

// sigPerThreadSyscall is only used on linux, so we assign a bogus signal
// number.
internal static UntypedInt sigPerThreadSyscall => /* 1 << 31 */ 2147483648;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runPerThreadSyscallOnlyˢ = "runPerThreadSyscall only valid on linux"u8;

//go:nosplit
internal static void runPerThreadSyscall() {
    @throw(runPerThreadSyscallOnlyˢ);
}

} // end runtime_package
