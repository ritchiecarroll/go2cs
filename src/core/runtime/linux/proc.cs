// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using cpu = @internal.cpu_package;
using goarch = @internal.goarch_package;
using goos = @internal.goos_package;
using atomic = @internal.runtime.atomic_package;
using exithook = @internal.runtime.exithook_package;
using stringslite = @internal.stringslite_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// set using cmd/go/internal/modload.ModInfoProg
internal static @string modinfo;

// Goroutine scheduler
// The scheduler's job is to distribute ready-to-run goroutines over worker threads.
//
// The main concepts are:
// G - goroutine.
// M - worker thread, or machine.
// P - processor, a resource that is required to execute Go code.
//     M must have an associated P to execute Go code, however it can be
//     blocked or in a syscall w/o an associated P.
//
// Design doc at https://golang.org/s/go11sched.
// Worker thread parking/unparking.
// We need to balance between keeping enough running worker threads to utilize
// available hardware parallelism and parking excessive running worker threads
// to conserve CPU resources and power. This is not simple for two reasons:
// (1) scheduler state is intentionally distributed (in particular, per-P work
// queues), so it is not possible to compute global predicates on fast paths;
// (2) for optimal thread management we would need to know the future (don't park
// a worker thread when a new goroutine will be readied in near future).
//
// Three rejected approaches that would work badly:
// 1. Centralize all scheduler state (would inhibit scalability).
// 2. Direct goroutine handoff. That is, when we ready a new goroutine and there
//    is a spare P, unpark a thread and handoff it the thread and the goroutine.
//    This would lead to thread state thrashing, as the thread that readied the
//    goroutine can be out of work the very next moment, we will need to park it.
//    Also, it would destroy locality of computation as we want to preserve
//    dependent goroutines on the same thread; and introduce additional latency.
// 3. Unpark an additional thread whenever we ready a goroutine and there is an
//    idle P, but don't do handoff. This would lead to excessive thread parking/
//    unparking as the additional threads will instantly park without discovering
//    any work to do.
//
// The current approach:
//
// This approach applies to three primary sources of potential work: readying a
// goroutine, new/modified-earlier timers, and idle-priority GC. See below for
// additional details.
//
// We unpark an additional thread when we submit work if (this is wakep()):
// 1. There is an idle P, and
// 2. There are no "spinning" worker threads.
//
// A worker thread is considered spinning if it is out of local work and did
// not find work in the global run queue or netpoller; the spinning state is
// denoted in m.spinning and in sched.nmspinning. Threads unparked this way are
// also considered spinning; we don't do goroutine handoff so such threads are
// out of work initially. Spinning threads spin on looking for work in per-P
// run queues and timer heaps or from the GC before parking. If a spinning
// thread finds work it takes itself out of the spinning state and proceeds to
// execution. If it does not find work it takes itself out of the spinning
// state and then parks.
//
// If there is at least one spinning thread (sched.nmspinning>1), we don't
// unpark new threads when submitting work. To compensate for that, if the last
// spinning thread finds work and stops spinning, it must unpark a new spinning
// thread. This approach smooths out unjustified spikes of thread unparking,
// but at the same time guarantees eventual maximal CPU parallelism
// utilization.
//
// The main implementation complication is that we need to be very careful
// during spinning->non-spinning thread transition. This transition can race
// with submission of new work, and either one part or another needs to unpark
// another worker thread. If they both fail to do that, we can end up with
// semi-persistent CPU underutilization.
//
// The general pattern for submission is:
// 1. Submit work to the local or global run queue, timer heap, or GC state.
// 2. #StoreLoad-style memory barrier.
// 3. Check sched.nmspinning.
//
// The general pattern for spinning->non-spinning transition is:
// 1. Decrement nmspinning.
// 2. #StoreLoad-style memory barrier.
// 3. Check all per-P work queues and GC for new work.
//
// Note that all this complexity does not apply to global run queue as we are
// not sloppy about thread unparking when submitting to global queue. Also see
// comments for nmspinning manipulation.
//
// How these different sources of work behave varies, though it doesn't affect
// the synchronization approach:
// * Ready goroutine: this is an obvious source of work; the goroutine is
//   immediately ready and must run on some thread eventually.
// * New/modified-earlier timer: The current timer implementation (see time.go)
//   uses netpoll in a thread with no work available to wait for the soonest
//   timer. If there is no thread waiting, we want a new spinning thread to go
//   wait.
// * Idle-priority GC: The GC wakes a stopped idle thread to contribute to
//   background GC work (note: currently disabled per golang.org/issue/19112).
//   Also see golang.org/issue/44313, as this should be extended to all GC
//   workers.
internal static ж<m> Ꮡm0 = new StandardBox<m>(new m(nil));
internal static ref m m0 => ref Ꮡm0.Value;
internal static g g0 = new();
internal static ж<mcache> mcache0;
internal static uintptr raceprocctx0;
internal static mutex raceFiniLock = new(nil);

// This slice records the initializing tasks that need to be
// done to start up the runtime. It is built by the linker.
internal static slice<ж<initTask>> runtime_inittasks;

// main_init_done is a signal used by cgocallbackg that initialization
// has been completed. It is made before _cgo_notify_runtime_init_done,
// so all cgo calls can rely on it existing. When main_init is complete,
// it is closed, meaning cgocallbackg can reliably receive from it.
internal static channel<bool> main_init_done;

//go:linkname main_main main.main
internal static partial void main_main();

// mainStarted indicates that the main M has started.
internal static bool mainStarted;

// runtimeInitTime is the nanotime() at which the runtime started.
internal static int64 runtimeInitTime;

// Value to use for signal mask for newly created M's.
internal static sigset initSigmask;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeMainNotOnM0ˢ = "runtime.main not on m0"u8;
internal static readonly @string nanotimeReturningZeroˢ = "nanotime returning zero"u8;
internal static readonly @string cgoPthreadKeyCreatedˢ = "_cgo_pthread_key_created missing"u8;
internal static readonly @string cgoThreadStartMissingˢ = "_cgo_thread_start missing"u8;
internal static readonly @string cgoSetenvMissingˢ = "_cgo_setenv missing"u8;
internal static readonly @string cgoUnsetenvMissingˢ = "_cgo_unsetenv missing"u8;
internal static readonly @string cgoNotifyRuntimeInitDoneˢ = "_cgo_notify_runtime_init_done missing"u8;
internal static readonly @string setCrosscall2Missingˢ = "set_crosscall2 missing"u8;

// The main goroutine.
internal static void Main() {
    GoFrame ᒐ = default;
    try {
        var mp = getg().Value.m;
        // Racectx of m0->g0 is used only as the parent of the main goroutine.
        // It must not be used for anything else.
        mp.Value.g0.Value.racectx = 0;
        // Max stack size is 1 GB on 64-bit, 250 MB on 32-bit.
        // Using decimal instead of binary GB and MB because
        // they look nicer in the stack overflow failure message.
        if (goarch.PtrSize == 8){
            maxstacksize = 1000000000;
        } else {
            maxstacksize = 250000000;
        }
        // An upper limit for max stack size. Used to avoid random crashes
        // after calling SetMaxStack and trying to allocate a stack that is too big,
        // since stackalloc works with 32-bit sizes.
        maxstackceiling = 2 * maxstacksize;
        // Allow newproc to start new Ms.
        mainStarted = true;
        if (haveSysmon) {
            systemstack(() => {
                newm(sysmon, nil, -1);
            });
        }
        // Lock the main goroutine onto this, the main OS thread,
        // during initialization. Most programs won't care, but a few
        // do require certain calls to be made by the main thread.
        // Those can arrange for main.main to run in the main thread
        // by calling runtime.LockOSThread during initialization
        // to preserve the lock.
        lockOSThread();
        if (mp != Ꮡm0) {
            @throw(runtimeMainNotOnM0ˢ);
        }
        // Record when the world started.
        // Must be before doInit for tracing init.
        runtimeInitTime = nanotime();
        if (runtimeInitTime == 0) {
            @throw(nanotimeReturningZeroˢ);
        }
        if (debug.inittrace != 0) {
            inittrace.id = getg().Value.goid;
            inittrace.active = true;
        }
        doInit(runtime_inittasks); // Must be before defer.
        // Defer unlock so that runtime.Goexit during init does the unlock too.
        var needUnlock = true;
        defer(() => {
            if (needUnlock) {
                unlockOSThread();
            }
        }, ref ᒐ);
        gcenable();
        main_init_done = new channel<bool>(0);
        if (iscgo) {
            if (_cgo_pthread_key_created == nil) {
                @throw(cgoPthreadKeyCreatedˢ);
            }
            if (_cgo_thread_start == nil) {
                @throw(cgoThreadStartMissingˢ);
            }
            if (GOOS != "windows"u8) {
                if (_cgo_setenv == nil) {
                    @throw(cgoSetenvMissingˢ);
                }
                if (_cgo_unsetenv == nil) {
                    @throw(cgoUnsetenvMissingˢ);
                }
            }
            if (_cgo_notify_runtime_init_done == nil) {
                @throw(cgoNotifyRuntimeInitDoneˢ);
            }
            // Set the x_crosscall2_ptr C function pointer variable point to crosscall2.
            if (set_crosscall2 == default!) {
                @throw(setCrosscall2Missingˢ);
            }
            set_crosscall2();
            // Start the template thread in case we enter Go from
            // a C-created thread and need to create a new thread.
            startTemplateThread();
            cgocall(_cgo_notify_runtime_init_done, nil);
        }
        // Run the initializing tasks. Depending on build mode this
        // list can arrive a few different ways, but it will always
        // contain the init tasks computed by the linker for all the
        // packages in the program (excluding those added at runtime
        // by package plugin). Run through the modules in dependency
        // order (the order they are initialized by the dynamic
        // loader, i.e. they are added to the moduledata linked list).
        for (var m = Ꮡfirstmoduledata; m != nil; m = m.Value.next) {
            doInit((~m).inittasks);
        }
        // Disable init tracing after main init done to avoid overhead
        // of collecting statistics in malloc and newproc
        inittrace.active = false;
        builtin.close(main_init_done);
        needUnlock = false;
        unlockOSThread();
        if (isarchive || islibrary) {
            // A program compiled with -buildmode=c-archive or c-shared
            // has a main, but it is not executed.
            return;
        }
        var fn = main_main; // make an indirect call, as the linker doesn't know the address of the main package when laying down the runtime
        fn();
        if (raceenabled) {
            runExitHooks(0); // run hooks now, since racefini does not return
            racefini();
        }
        // Make racy client program work: if panicking on
        // another goroutine at the same time as main returns,
        // let the other goroutine finish printing the panic trace.
        // Once it does, it will exit. See issues 3934 and 20018.
        if (ᏑrunningPanicDefers.Load() != 0) {
            // Running deferred functions should not take long.
            for (nint c = 0; c < 1000; c++) {
                if (ᏑrunningPanicDefers.Load() == 0) {
                    break;
                }
                Gosched();
            }
        }
        if (Ꮡpanicking.Load() != 0) {
            gopark(default!, nil, waitReasonPanicWait, traceBlockForever, 1);
        }
        runExitHooks(0);
        exit(0);
        while (ᐧ) {
            ж<int32> x = default!;
            x.Value = 0;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// os_beforeExit is called from os.Exit(0).
//
//go:linkname os_beforeExit os.runtime_beforeExit
internal static void os_beforeExit(nint exitCode) {
    runExitHooks(exitCode);
    if (exitCode == 0 && raceenabled) {
        racefini();
    }
}

/* [GoInit] runtime bootstrap init - not run; .NET is the runtime */ internal static void initΔ6() {
    exithook.Gosched = Gosched;
    exithook.Goid = () => (~getg()).goid;
    exithook.Throw = @throw;
}

internal static void runExitHooks(nint code) {
    exithook.Run(code);
}

// start forcegc helper goroutine
/* [GoInit] runtime bootstrap init - not run; .NET is the runtime */ internal static void initΔ7() {
    goǃ(forcegchelper);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string forcegcPhaseErrorˢ = "forcegc: phase error"u8;

internal static void forcegchelper() {
    forcegc.g = getg();
    lockInit(ref forcegc.@lock, lockRankForcegc);
    while (ᐧ) {
        @lock(Ꮡforcegc.of(forcegcstate.Ꮡlock));
        if (Ꮡforcegc.of(forcegcstate.Ꮡidle).Load()) {
            @throw(forcegcPhaseErrorˢ);
        }
        Ꮡforcegc.of(forcegcstate.Ꮡidle).Store(true);
        goparkunlock(Ꮡforcegc.of(forcegcstate.Ꮡlock), waitReasonForceGCIdle, traceBlockSystemGoroutine, 1);
        // this goroutine is explicitly resumed by sysmon
        if (debug.gctrace > 0) {
            println((@string)"GC forced"u8);
        }
        // Time-triggered, fully concurrent.
        gcStart(new gcTrigger(kind: gcTriggerTime, now: nanotime()));
    }
}

// go2cs generated this placeholder — func Gosched is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// goschedguarded yields the processor like gosched, but also checks
// for forbidden states and opts out of the yield in those cases.
//
//go:nosplit
internal static void goschedguarded() {
    mcall(goschedguarded_m);
}

// goschedIfBusy yields the processor like gosched, but only does so if
// there are no idle Ps or if we're on the only P and there's nothing in
// the run queue. In both cases, there is freely available idle time.
//
//go:nosplit
internal static void goschedIfBusy() {
    var gp = getg();
    // Call gosched if gp.preempt is set; we may be in a tight loop that
    // doesn't otherwise yield.
    if (!(~gp).preempt && Ꮡsched.of(schedt.Ꮡnpidle).Load() > 0) {
        return;
    }
    mcall(gosched_m);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goparkBadGStatusˢ = "gopark: bad g status"u8;

// Puts the current goroutine into a waiting state and calls unlockf on the
// system stack.
//
// If unlockf returns false, the goroutine is resumed.
//
// unlockf must not access this G's stack, as it may be moved between
// the call to gopark and the call to unlockf.
//
// Note that because unlockf is called after putting the G into a waiting
// state, the G may have already been readied by the time unlockf is called
// unless there is external synchronization preventing the G from being
// readied. If unlockf returns false, it must guarantee that the G cannot be
// externally readied.
//
// Reason explains why the goroutine has been parked. It is displayed in stack
// traces and heap dumps. Reasons should be unique and descriptive. Do not
// re-use reasons, add new ones.
//
// gopark should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gvisor.dev/gvisor
//   - github.com/sagernet/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname gopark
internal static void gopark(Func<ж<g>, @unsafe.Pointer, bool> unlockf, @unsafe.Pointer @lock, waitReason reason, traceBlockReason traceReason, nint traceskip) {
    if (reason != waitReasonSleep) {
        checkTimeouts(); // timeouts may expire while two goroutines keep the scheduler busy
    }
    var mp = acquirem();
    var gp = mp.Value.curg;
    var status = readgstatus(gp);
    if (status != _Grunning && status != _Gscanrunning) {
        @throw(goparkBadGStatusˢ);
    }
    mp.Value.waitlock = @lock;
    mp.Value.waitunlockf = unlockf;
    gp.Value.waitreason = reason;
    mp.Value.waitTraceBlockReason = traceReason;
    mp.Value.waitTraceSkip = traceskip;
    releasem(ref (mp).DerefOrNull());
    // can't do anything that might move the G between Ms here.
    mcall(park_m);
}

// Puts the current goroutine into a waiting state and unlocks the lock.
// The goroutine can be made runnable again by calling goready(gp).
internal static void goparkunlock(ж<mutex> Ꮡlock, waitReason reason, traceBlockReason traceReason, nint traceskip) {
    gopark(parkunlock_c, @unsafe.Pointer.FromPinnedBox(Ꮡlock), reason, traceReason, traceskip);
}

// goready should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gvisor.dev/gvisor
//   - github.com/sagernet/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname goready
internal static void goready(ж<g> Ꮡgp, nint traceskip) {
    systemstack(() => {
        ready(Ꮡgp, traceskip, true);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string acquireSudogFoundSElemˢ = "acquireSudog: found s.elem != nil in cache"u8;

//go:nosplit
internal static ж<sudog> acquireSudog() {
    // Delicate dance: the semaphore implementation calls
    // acquireSudog, acquireSudog calls new(sudog),
    // new calls malloc, malloc can call the garbage collector,
    // and the garbage collector calls the semaphore implementation
    // in stopTheWorld.
    // Break the cycle by doing acquirem/releasem around new(sudog).
    // The acquirem/releasem increments m.locks during new(sudog),
    // which keeps the garbage collector from being invoked.
    var mp = acquirem();
    var pp = (~mp).p.ptr();
    if (len((~pp).sudogcache) == 0) {
        @lock(Ꮡsched.of(schedt.Ꮡsudoglock));
        // First, try to grab a batch from central cache.
        while (len((~pp).sudogcache) < cap((~pp).sudogcache) / 2 && sched.sudogcache != nil) {
            var sΔ1 = sched.sudogcache;
            sched.sudogcache = sΔ1.Value.next;
            sΔ1.Value.next = default!;
            pp.Value.sudogcache = append((~pp).sudogcache, sΔ1);
        }
        unlock(Ꮡsched.of(schedt.Ꮡsudoglock));
        // If the central cache is empty, allocate a new one.
        if (len((~pp).sudogcache) == 0) {
            pp.Value.sudogcache = append((~pp).sudogcache, @new<sudog>());
        }
    }
    nint n = len((~pp).sudogcache);
    var s = (~pp).sudogcache[n - 1];
    pp.Value.sudogcache[n - 1] = default!;
    pp.Value.sudogcache = (~pp).sudogcache[..(int)(n - 1)];
    if ((~s).elem != nil) {
        @throw(acquireSudogFoundSElemˢ);
    }
    releasem(ref (mp).DerefOrNull());
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeSudogWithNonNilˢ = "runtime: sudog with non-nil elem"u8;
internal static readonly @string runtimeSudogWithNonFalseˢ = "runtime: sudog with non-false isSelect"u8;
internal static readonly @string runtimeSudogWithNonNilˢ2 = "runtime: sudog with non-nil next"u8;
internal static readonly @string runtimeSudogWithNonNilˢ3 = "runtime: sudog with non-nil prev"u8;
internal static readonly @string runtimeSudogWithNonNilˢ4 = "runtime: sudog with non-nil waitlink"u8;
internal static readonly @string runtimeSudogWithNonNilCˢ = "runtime: sudog with non-nil c"u8;
internal static readonly @string runtimeReleaseSudogWithˢ = "runtime: releaseSudog with non-nil gp.param"u8;

//go:nosplit
internal static void releaseSudog(ж<sudog> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (s.elem != nil) {
        @throw(runtimeSudogWithNonNilˢ);
    }
    if (s.isSelect) {
        @throw(runtimeSudogWithNonFalseˢ);
    }
    if (s.next != nil) {
        @throw(runtimeSudogWithNonNilˢ2);
    }
    if (s.prev != nil) {
        @throw(runtimeSudogWithNonNilˢ3);
    }
    if (s.waitlink != nil) {
        @throw(runtimeSudogWithNonNilˢ4);
    }
    if (s.c != nil) {
        @throw(runtimeSudogWithNonNilCˢ);
    }
    var gp = getg();
    if ((~gp).param != nil) {
        @throw(runtimeReleaseSudogWithˢ);
    }
    var mp = acquirem(); // avoid rescheduling to another P
    var pp = (~mp).p.ptr();
    if (len((~pp).sudogcache) == cap((~pp).sudogcache)) {
        // Transfer half of local cache to the central cache.
        ж<sudog> first = default!;
        ж<sudog> last = default!;
        while (len((~pp).sudogcache) > cap((~pp).sudogcache) / 2) {
            nint n = len((~pp).sudogcache);
            var Δp = (~pp).sudogcache[n - 1];
            pp.Value.sudogcache[n - 1] = default!;
            pp.Value.sudogcache = (~pp).sudogcache[..(int)(n - 1)];
            if (first == nil){
                first = Δp;
            } else {
                last.Value.next = Δp;
            }
            last = Δp;
        }
        @lock(Ꮡsched.of(schedt.Ꮡsudoglock));
        last.Value.next = sched.sudogcache;
        sched.sudogcache = first;
        unlock(Ꮡsched.of(schedt.Ꮡsudoglock));
    }
    pp.Value.sudogcache = append((~pp).sudogcache, Ꮡs);
    releasem(ref (mp).DerefOrNull());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeMcallCalledOnMG0ˢ = "runtime: mcall called on m->g0 stack"u8;

// called from assembly.
internal static void badmcall(Action<ж<g>> fn) {
    @throw(runtimeMcallCalledOnMG0ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeMcallFunctionˢ = "runtime: mcall function returned"u8;

internal static void badmcall2(Action<ж<g>> fn) {
    @throw(runtimeMcallFunctionˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string argSizeToReflectCallMoreˢ = "arg size to reflect.call more than 1GB"u8;

internal static void badreflectcall() {
    throw panic(((plainError)(@string)argSizeToReflectCallMoreˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fatalMorestackOnG0ˢ = "fatal: morestack on g0\n"u8;
internal static readonly @string morestackOnG0ˢ = "morestack on g0"u8;

//go:nosplit
//go:nowritebarrierrec
internal static void badmorestackg0() {
    if (!crashStackImplemented) {
        writeErrStr(fatalMorestackOnG0ˢ);
        return;
    }
    var g = getg();
    var gʗ1 = g;
    switchToCrashStack(() => {
        print((@string)"runtime: morestack on g0, stack ["u8, ((Δhex)(uint64)(~gʗ1).stack.lo), (@string)" "u8, ((Δhex)(uint64)(~gʗ1).stack.hi), (@string)"], sp="u8, ((Δhex)(uint64)(~gʗ1).sched.sp), (@string)", called from\n"u8);
        gʗ1.Value.m.Value.traceback = 2; // include pc and sp in stack trace
        traceback1((~gʗ1).sched.pc, (~gʗ1).sched.sp, (~gʗ1).sched.lr, gʗ1, 0);
        print((@string)"\n"u8);
        @throw(morestackOnG0ˢ);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fatalMorestackOnGsignalˢ = "fatal: morestack on gsignal\n"u8;

//go:nosplit
//go:nowritebarrierrec
internal static void badmorestackgsignal() {
    writeErrStr(fatalMorestackOnGsignalˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ctxt0ˢ = "ctxt != 0"u8;

//go:nosplit
internal static void badctxt() {
    @throw(ctxt0ˢ);
}

// gcrash is a fake g that can be used when crashing due to bad
// stack conditions.
internal static g gcrash = new();

internal static ж<atomic.Pointer<g>> ᏑcrashingG = new StandardBox<atomic.Pointer<g>>(default(atomic.Pointer<g>));
internal static ref atomic.Pointer<g> crashingG => ref ᏑcrashingG.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fatalRecursiveˢ = "fatal: recursive switchToCrashStack\n"u8;
internal static readonly @string fatalConcurrentˢ = "fatal: concurrent switchToCrashStack\n"u8;

// Switch to crashstack and call fn, with special handling of
// concurrent and recursive cases.
//
// Nosplit as it is called in a bad stack condition (we know
// morestack would fail).
//
//go:nosplit
//go:nowritebarrierrec
internal static void switchToCrashStack(Action fn) {
    var me = getg();
    if (ᏑcrashingG.CompareAndSwapNoWB(nil, me)) {
        switchToCrashStack0(fn); // should never return
        abort();
    }
    if (ᏑcrashingG.Load() == me) {
        // recursive crashing. too bad.
        writeErrStr(fatalRecursiveˢ);
        abort();
    }
    // Another g is crashing. Give it some time, hopefully it will finish traceback.
    usleep_no_g(100);
    writeErrStr(fatalConcurrentˢ);
    abort();
}

// Disable crash stack on Windows for now. Apparently, throwing an exception
// on a non-system-allocated crash stack causes EXCEPTION_STACK_OVERFLOW and
// hangs the process (see issue 63938).
internal const bool crashStackImplemented = /* GOOS != "windows" */ true;

//go:noescape
internal static partial void switchToCrashStack0(Action fn);

// in assembly
internal static bool lockedOSThread() {
    var gp = getg();
    return (~gp).lockedm != 0 && (~(~gp).m).lockedg != 0;
}

internal static ж<mutex> Ꮡallglock = new StandardBox<mutex>(new mutex(nil));
internal static ref mutex allglock => ref Ꮡallglock.Value;
internal static ж<slice<ж<g>>> Ꮡallgs = new StandardBox<slice<ж<g>>>(default(slice<ж<g>>));
internal static ref slice<ж<g>> allgs => ref Ꮡallgs.ValueSlot;
internal static ж<uintptr> Ꮡallglen = new StandardBox<uintptr>(default(uintptr));
internal static ref uintptr allglen => ref Ꮡallglen.Value;
internal static ж<ж<ж<g>>> Ꮡallgptr = new StandardBox<ж<ж<g>>>(default(ж<ж<g>>));
internal static ref ж<ж<g>> allgptr => ref Ꮡallgptr.ValueSlot;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string allgaddBadStatusGidleˢ = "allgadd: bad status Gidle"u8;

internal static void allgadd(ж<g> Ꮡgp) {
    if (readgstatus(Ꮡgp) == _Gidle) {
        @throw(allgaddBadStatusGidleˢ);
    }
    @lock(Ꮡallglock);
    allgs = append(allgs, Ꮡgp);
    if (Ꮡ(allgs, 0) != allgptr) {
        atomicstorep(@unsafe.Pointer.FromBox(Ꮡallgptr), @unsafe.Pointer.FromBox(Ꮡ(allgs, 0)));
    }
    atomic.Storeuintptr(Ꮡallglen, (uintptr)len(allgs));
    unlock(Ꮡallglock);
}

// allGsSnapshot returns a snapshot of the slice of all Gs.
//
// The world must be stopped or allglock must be held.
internal static slice<ж<g>> allGsSnapshot() {
    assertWorldStoppedOrLockHeld(ref allglock);
    // Because the world is stopped or allglock is held, allgadd
    // cannot happen concurrently with this. allgs grows
    // monotonically and existing entries never change, so we can
    // simply return a copy of the slice header. For added safety,
    // we trim everything past len because that can still change.
    return allgs.slice(-1, len(allgs), len(allgs));
}

// atomicAllG returns &allgs[0] and len(allgs) for use with atomicAllGIndex.
internal static (ж<ж<g>>, uintptr) atomicAllG() {
    var length = atomic.Loaduintptr(Ꮡallglen);
    var ptr = (ж<ж<g>>)(uintptr)(atomic.Loadp(@unsafe.Pointer.FromBox(Ꮡallgptr)));
    return (ptr, length);
}

// atomicAllGIndex returns ptr[i] with the allgptr returned from atomicAllG.
internal static ж<g> atomicAllGIndex(ж<ж<g>> Ꮡptr, uintptr i) {
    return ~(ж<ж<g>>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(Ꮡptr), i * (uintptr)goarch.PtrSize));
}

// forEachG calls fn on every G from allgs.
//
// forEachG takes a lock to exclude concurrent addition of new Gs.
internal static void forEachG(Action<ж<g>> fn) {
    @lock(Ꮡallglock);
    foreach (var (_, gp) in allgs) {
        fn(gp);
    }
    unlock(Ꮡallglock);
}

// forEachGRace calls fn on every G from allgs.
//
// forEachGRace avoids locking, but does not exclude addition of new Gs during
// execution, which may be missed.
internal static void forEachGRace(Action<ж<g>> fn) {
    var (ptr, length) = atomicAllG();
    for (var i = (uintptr)0; i < length; i++) {
        var gp = atomicAllGIndex(ptr, i);
        fn(gp);
    }
    return;
}

internal static UntypedInt _GoidCacheBatch => 16;

// cpuinit sets up CPU feature flags and calls internal/cpu.Initialize. env should be the complete
// value of the GODEBUG environment variable.
internal static void cpuinit(@string env) {
    var exprᴛ1 = GOOS;
    if (exprᴛ1 == "aix"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "illumos"u8 || exprᴛ1 == "solaris"u8 || exprᴛ1 == "linux"u8) {
        cpu.DebugOptions = true;
    }

    cpu.Initialize(env);
    // Support cpu feature variables are used in code generated by the compiler
    // to guard execution of instructions that can not be assumed to be always supported.
    var exprᴛ2 = GOARCH;
    if (exprᴛ2 == "386"u8 || exprᴛ2 == "amd64"u8) {
        x86HasPOPCNT = cpu.X86.HasPOPCNT;
        x86HasSSE41 = cpu.X86.HasSSE41;
        x86HasFMA = cpu.X86.HasFMA;
    }
    else if (exprᴛ2 == "arm"u8) {
        armHasVFPv4 = cpu.ARM.HasVFPv4;
    }
    else if (exprᴛ2 == "arm64"u8) {
        arm64HasATOMICS = cpu.ARM64.HasATOMICS;
    }

}

// getGodebugEarly extracts the environment variable GODEBUG from the environment on
// Unix-like operating systems and returns it. This function exists to extract GODEBUG
// early before much of the runtime is initialized.
internal static @string getGodebugEarly() {
    @string prefix = "GODEBUG="u8;
    @string env = default!;
    var exprᴛ1 = GOOS;
    if (exprᴛ1 == "aix"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "illumos"u8 || exprᴛ1 == "solaris"u8 || exprᴛ1 == "linux"u8) {
        var n = (int32)0;
        while (argv_index(argv, // Similar to goenv_unix but extracts the environment value for
 // GODEBUG directly.
 // TODO(moehrmann): remove when general goenvs() can be called before cpuinit()
 argc + 1 + n) != nil) {
            n++;
        }
        for (var i = (int32)0; i < n; i++) {
            var Δp = argv_index(argv, argc + 1 + i);
            @string s = @unsafe.String(Δp, findnull(Δp));
            if (stringslite.HasPrefix(s, prefix)) {
                env = gostring(Δp)[(int)(len(prefix))..];
                break;
            }
        }
    }

    return env;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gomaxprocsˢ = "GOMAXPROCS"u8;
internal static readonly @string unknownRunnableGoroutineˢ = "unknown runnable goroutine during bootstrap"u8;
internal static readonly @string unknownˢ2 = "unknown"u8;

// The bootstrap sequence is:
//
//	call osinit
//	call schedinit
//	make & queue new G
//	call runtime·mstart
//
// The new G calls runtime·main.
internal static void schedinit() {
    lockInit(ref sched.@lock, lockRankSched);
    lockInit(ref sched.sysmonlock, lockRankSysmon);
    lockInit(ref sched.deferlock, lockRankDefer);
    lockInit(ref sched.sudoglock, lockRankSudog);
    lockInit(ref deadlock, lockRankDeadlock);
    lockInit(ref paniclk, lockRankPanic);
    lockInit(ref allglock, lockRankAllg);
    lockInit(ref allpLock, lockRankAllp);
    lockInit(ref reflectOffs.@lock, lockRankReflectOffs);
    lockInit(ref finlock, lockRankFin);
    lockInit(ref cpuprof.@lock, lockRankCpuprof);
    ᏑallocmLock.init(lockRankAllocmR, lockRankAllocmRInternal, lockRankAllocmW);
    ᏑexecLock.init(lockRankExecR, lockRankExecRInternal, lockRankExecW);
    traceLockInit();
    // Enforce that this lock is always a leaf lock.
    // All of this lock's critical sections should be
    // extremely short.
    lockInit(ref memstats.heapStats.noPLock, lockRankLeafRank);
    // raceinit must be the first call to race detector.
    // In particular, it must be done before mallocinit below calls racemapshadow.
    var gp = getg();
    if (raceenabled) {
        (gp.Value.racectx, raceprocctx0) = raceinit();
    }
    sched.maxmcount = 10000;
    ᏑcrashFD.Store(~(uintptr)0);
    // The world starts stopped.
    worldStopped();
    ticks.init(); // run as early as possible
    moduledataverify();
    stackinit();
    mallocinit();
    @string godebug = getGodebugEarly();
    cpuinit(godebug); // must run before alginit
    randinit(); // must run before alginit, mcommoninit
    alginit(); // maps, hash, rand must not be used before this call
    mcommoninit((~gp).m, -1);
    modulesinit(); // provides activeModules
    typelinksinit(); // uses maps, activeModules
    itabsinit(); // uses activeModules
    stkobjinit(); // must run before GC starts
    sigsave((~gp).m.of(m.Ꮡsigmask));
    initSigmask = gp.Value.m.Value.sigmask.Clone();
    goargs();
    goenvs();
    secure();
    checkfds();
    parsedebugvars();
    gcinit();
    // Allocate stack space that can be used when crashing due to bad stack
    // conditions, e.g. morestack on g0.
    gcrash.stack = @stackalloc(16384);
    gcrash.stackguard0 = gcrash.stack.lo + 1000;
    gcrash.stackguard1 = gcrash.stack.lo + 1000;
    // if disableMemoryProfiling is set, update MemProfileRate to 0 to turn off memprofile.
    // Note: parsedebugvars may update MemProfileRate, but when disableMemoryProfiling is
    // set to true by the linker, it means that nothing is consuming the profile, it is
    // safe to set MemProfileRate to 0.
    if (disableMemoryProfiling) {
        MemProfileRate = 0;
    }
    // mcommoninit runs before parsedebugvars, so init profstacks again.
    mProfStackInit(ref ((~gp).m).DerefOrNull());
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    Ꮡsched.of(schedt.Ꮡlastpoll).Store(nanotime());
    var procs = ncpu;
    {
        var (n, ok) = atoi32(gogetenv(gomaxprocsˢ)); if (ok && n > 0) {
            procs = n;
        }
    }
    if (procresize(procs) != nil) {
        @throw(unknownRunnableGoroutineˢ);
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    // World is effectively started now, as P's can run.
    worldStarted();
    if (buildVersion == ""u8) {
        // Condition should never trigger. This code just serves
        // to ensure runtime·buildVersion is kept in the resulting binary.
        buildVersion = unknownˢ2;
    }
    if (len(modinfo) == 1) {
        // Condition should never trigger. This code just serves
        // to ensure runtime·modinfo is kept in the resulting binary.
        modinfo = ""u8;
    }
}

internal static void dumpgstatus(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    var thisg = getg();
    print((@string)"runtime:   gp: gp="u8, Ꮡgp.OrTypedNil(), (@string)", goid="u8, gp.goid, (@string)", gp->atomicstatus="u8, readgstatus(Ꮡgp), (@string)"\n"u8);
    print((@string)"runtime: getg:  g="u8, thisg.OrTypedNil(), (@string)", goid="u8, (~thisg).goid, (@string)",  g->atomicstatus="u8, readgstatus(thisg), (@string)"\n"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string threadExhaustionˢ = "thread exhaustion"u8;

// sched.lock must be held.
internal static void checkmcount() {
    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    // Exclude extra M's, which are used for cgocallback from threads
    // created in C.
    //
    // The purpose of the SetMaxThreads limit is to avoid accidental fork
    // bomb from something like millions of goroutines blocking on system
    // calls, causing the runtime to create millions of threads. By
    // definition, this isn't a problem for threads created in C, so we
    // exclude them from the limit. See https://go.dev/issue/60004.
    var count = mcount() - (int32)ᏑextraMInUse.Load() - (int32)ᏑextraMLength.Load();
    if (count > sched.maxmcount) {
        print((@string)"runtime: program exceeds "u8, sched.maxmcount, (@string)"-thread limit\n"u8);
        @throw(threadExhaustionˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeThreadIdOverflowˢ = "runtime: thread ID overflow"u8;

// mReserveID returns the next ID to use for a new m. This new m is immediately
// considered 'running' by checkdead.
//
// sched.lock must be held.
internal static int64 mReserveID() {
    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    if (sched.mnext + 1 < sched.mnext) {
        @throw(runtimeThreadIdOverflowˢ);
    }
    var id = sched.mnext;
    sched.mnext++;
    checkmcount();
    return id;
}

// Pre-allocated ID may be passed as 'id', or omitted by passing -1.
internal static void mcommoninit(ж<m> Ꮡmp, int64 id) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    var gp = getg();
    // g0 stack won't make sense for user (and is not necessary unwindable).
    if (gp != (~(~gp).m).g0) {
        callers(1, mp.createstack[..]);
    }
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    if (id >= 0){
        mp.id = id;
    } else {
        mp.id = mReserveID();
    }
    mrandinit(Ꮡmp);
    mpreinit(Ꮡmp);
    if (mp.gsignal != nil) {
        mp.gsignal.Value.stackguard1 = (~mp.gsignal).stack.lo + (uintptr)stackGuard;
    }
    // Add to allm so garbage collector doesn't free g->m
    // when it is just in a register or thread-local storage.
    mp.alllink = allm;
    // NumCgoCall() and others iterate over allm w/o schedlock,
    // so we need to publish it safely.
    atomicstorep(@unsafe.Pointer.FromBox(Ꮡallm), @unsafe.Pointer.FromPinnedBox(Ꮡmp));
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    // Allocate memory to hold a cgo traceback if the cgo call crashes.
    if (iscgo || GOOS == "solaris"u8 || GOOS == "illumos"u8 || GOOS == "windows"u8) {
        mp.cgoCallers = @new<ΔcgoCallers>();
    }
    mProfStackInit(ref (Ꮡmp).DerefOrNull());
}

// mProfStackInit is used to eagerly initialize stack trace buffers for
// profiling. Lazy allocation would have to deal with reentrancy issues in
// malloc and runtime locks for mLockProfile.
// TODO(mknyszek): Implement lazy allocation if this becomes a problem.
internal static void mProfStackInit(ref m mp) {
    if (debug.profstackdepth == 0) {
        // debug.profstack is set to 0 by the user, or we're being called from
        // schedinit before parsedebugvars.
        return;
    }
    mp.profStack = makeProfStackFP();
    mp.mLockProfile.stack = makeProfStackFP();
}

// makeProfStackFP creates a buffer large enough to hold a maximum-sized stack
// trace as well as any additional frames needed for frame pointer unwinding
// with delayed inline expansion.
internal static slice<uintptr> makeProfStackFP() {
    // The "1" term is to account for the first stack entry being
    // taken up by a "skip" sentinel value for profilers which
    // defer inline frame expansion until the profile is reported.
    // The "maxSkip" term is for frame pointer unwinding, where we
    // want to end up with debug.profstackdebth frames but will discard
    // some "physical" frames to account for skipping.
    return new slice<uintptr>((int32)(1 + maxSkip) + debug.profstackdepth);
}

// makeProfStack returns a buffer large enough to hold a maximum-sized stack
// trace.
internal static slice<uintptr> makeProfStack() {
    return new slice<uintptr>(debug.profstackdepth);
}

//go:linkname pprof_makeProfStack
public static slice<uintptr> pprof_makeProfStack() {
    return makeProfStack();
}

[GoRecv] internal static void becomeSpinning(this ref m mp) {
    mp.spinning = true;
    Ꮡsched.of(schedt.Ꮡnmspinning).Add(1);
    Ꮡsched.of(schedt.Ꮡneedspinning).Store(0);
}

// Take a snapshot of allp, for use after dropping the P.
//
// Must be called with a P, but the returned slice may be used after dropping
// the P. The M holds a reference on the snapshot to keep the backing array
// alive.
//
//go:yeswritebarrierrec
[GoRecv] internal static slice<ж<Δp>> snapshotAllp(this ref m mp) {
    mp.allpSnapshot = allp;
    return mp.allpSnapshot;
}

// Clear the saved allp snapshot. Should be called as soon as the snapshot is
// no longer required.
//
// Must be called after reacquiring a P, as it requires a write barrier.
//
//go:yeswritebarrierrec
[GoRecv] internal static void clearAllpSnapshot(this ref m mp) {
    mp.allpSnapshot = default!;
}

[GoRecv] internal static bool hasCgoOnStack(this ref m mp) {
    return mp.ncgo > 0 || mp.isextra;
}

internal const bool osHasLowResTimer = /* GOOS == "windows" || GOOS == "openbsd" || GOOS == "netbsd" */ false;
internal static UntypedInt osHasLowResClockInt => /* goos.IsWindows */ 0;
internal const bool osHasLowResClock = /* osHasLowResClockInt > 0 */ false;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badGStatusInReadyˢ = "bad g->status in ready"u8;

// Mark gp ready to run.
internal static void ready(ж<g> Ꮡgp, nint traceskip, bool next) {
    var status = readgstatus(Ꮡgp);
    // Mark runnable.
    var mp = acquirem(); // disable preemption because it can be holding p in a local var
    if ((uint32)(status & ~(uint32)_Gscan) != _Gwaiting) {
        dumpgstatus(Ꮡgp);
        @throw(badGStatusInReadyˢ);
    }
    // status is Gwaiting or Gscanwaiting, make Grunnable and put on runq
    var Δtrace = traceAcquire();
    casgstatus(Ꮡgp, _Gwaiting, _Grunnable);
    if (Δtrace.ok()) {
        Δtrace.GoUnpark(Ꮡgp, traceskip);
        traceRelease(Δtrace);
    }
    runqput((~mp).p.ptr(), Ꮡgp, next);
    wakep();
    releasem(ref (mp).DerefOrNull());
}

// freezeStopWait is a large value that freezetheworld sets
// sched.stopwait to in order to request that all Gs permanently stop.
internal static UntypedInt freezeStopWait => 0x7fffffff;

// freezing is set to non-zero if the runtime is trying to freeze the
// world.
internal static ж<atomic.Bool> Ꮡfreezing = new StandardBox<atomic.Bool>(default(atomic.Bool));
internal static ref atomic.Bool freezing => ref Ꮡfreezing.Value;

// Similar to stopTheWorld but best-effort and can be called several times.
// There is no reverse operation, used during crashing.
// This function must not lock any mutexes.
internal static void freezetheworld() {
    Ꮡfreezing.Store(true);
    if (debug.dontfreezetheworld > 0) {
        // Don't prempt Ps to stop goroutines. That will perturb
        // scheduler state, making debugging more difficult. Instead,
        // allow goroutines to continue execution.
        //
        // fatalpanic will tracebackothers to trace all goroutines. It
        // is unsafe to trace a running goroutine, so tracebackothers
        // will skip running goroutines. That is OK and expected, we
        // expect users of dontfreezetheworld to use core files anyway.
        //
        // However, allowing the scheduler to continue running free
        // introduces a race: a goroutine may be stopped when
        // tracebackothers checks its status, and then start running
        // later when we are in the middle of traceback, potentially
        // causing a crash.
        //
        // To mitigate this, when an M naturally enters the scheduler,
        // schedule checks if freezing is set and if so stops
        // execution. This guarantees that while Gs can transition from
        // running to stopped, they can never transition from stopped
        // to running.
        //
        // The sleep here allows racing Ms that missed freezing and are
        // about to run a G to complete the transition to running
        // before we start traceback.
        usleep(1000);
        return;
    }
    // stopwait and preemption requests can be lost
    // due to races with concurrently executing threads,
    // so try several times
    for (nint i = 0; i < 5; i++) {
        // this should tell the scheduler to not start any new goroutines
        sched.stopwait = freezeStopWait;
        Ꮡsched.of(schedt.Ꮡgcwaiting).Store(true);
        // this should stop running goroutines
        if (!preemptall()) {
            break; // no running goroutines
        }
        usleep(1000);
    }
    // to be sure
    usleep(1000);
    preemptall();
    usleep(1000);
}

// All reads and writes of g's status go through readgstatus, casgstatus
// castogscanstatus, casfrom_Gscanstatus.
//
//go:nosplit
internal static uint32 readgstatus(ж<g> Ꮡgp) {
    return Ꮡgp.of(g.Ꮡatomicstatus).Load();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string casfromGscanstatusTopGpˢ = "casfrom_Gscanstatus:top gp->status is not in scan state"u8;
internal static readonly @string casfromGscanstatusGpˢ = "casfrom_Gscanstatus: gp->status is not in scan state"u8;

// The Gscanstatuses are acting like locks and this releases them.
// If it proves to be a performance hit we should be able to make these
// simple atomic stores but for now we are going to throw if
// we see an inconsistent state.
internal static void casfrom_Gscanstatus(ж<g> Ꮡgp, uint32 oldval, uint32 newval) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    var success = false;
    // Check that transition is valid.
    var exprᴛ1 = oldval;
    if (exprᴛ1 == _Gscanrunnable || exprᴛ1 == _Gscanwaiting || exprᴛ1 == _Gscanrunning || exprᴛ1 == _Gscansyscall || exprᴛ1 == _Gscanpreempted) {
        if (newval == (uint32)(oldval & ~(uint32)_Gscan)) {
            success = Ꮡgp.of(g.Ꮡatomicstatus).CompareAndSwap(oldval, newval);
        }
    }
    else { /* default: */
        print((@string)"runtime: casfrom_Gscanstatus bad oldval gp="u8, Ꮡgp.OrTypedNil(), (@string)", oldval="u8, ((Δhex)(uint64)oldval), (@string)", newval="u8, ((Δhex)(uint64)newval), (@string)"\n"u8);
        dumpgstatus(Ꮡgp);
        @throw(casfromGscanstatusTopGpˢ);
    }

    if (!success) {
        print((@string)"runtime: casfrom_Gscanstatus failed gp="u8, Ꮡgp.OrTypedNil(), (@string)", oldval="u8, ((Δhex)(uint64)oldval), (@string)", newval="u8, ((Δhex)(uint64)newval), (@string)"\n"u8);
        dumpgstatus(Ꮡgp);
        @throw(casfromGscanstatusGpˢ);
    }
    releaseLockRankAndM(lockRankGscan);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string castogscanstatusˢ = "castogscanstatus"u8;

// This will return false if the gp is not in the expected status and the cas fails.
// This acts like a lock acquire while the casfromgstatus acts like a lock release.
internal static bool castogscanstatus(ж<g> Ꮡgp, uint32 oldval, uint32 newval) {
    var exprᴛ1 = oldval;
    if (exprᴛ1 == _Grunnable || exprᴛ1 == _Grunning || exprᴛ1 == _Gwaiting || exprᴛ1 == _Gsyscall) {
        if (newval == (uint32)(oldval | (uint32)_Gscan)) {
            var r = Ꮡgp.of(g.Ꮡatomicstatus).CompareAndSwap(oldval, newval);
            if (r) {
                acquireLockRankAndM(lockRankGscan);
            }
            return r;
        }
    }

    print((@string)"runtime: castogscanstatus oldval="u8, ((Δhex)(uint64)oldval), (@string)" newval="u8, ((Δhex)(uint64)newval), (@string)"\n"u8);
    @throw(castogscanstatusˢ);
    throw panic("not reached");
}

// casgstatusAlwaysTrack is a debug flag that causes casgstatus to always track
// various latencies on every transition instead of sampling them.
internal static ж<bool> ᏑcasgstatusAlwaysTrack = new StandardBox<bool>(false);
internal static ref bool casgstatusAlwaysTrack => ref ᏑcasgstatusAlwaysTrack.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string casgstatusBadIncomingˢ = "casgstatus: bad incoming values"u8;
internal static readonly @string casgstatusWaitingForˢ = "casgstatus: waiting for Gwaiting but is Grunnable"u8;

// If asked to move to or from a Gscanstatus this will throw. Use the castogscanstatus
// and casfrom_Gscanstatus instead.
// casgstatus will loop if the g->atomicstatus is in a Gscan status until the routine that
// put it in the Gscan state is finished.
//
//go:nosplit
internal static void casgstatus(ж<g> Ꮡgp, uint32 oldval, uint32 newval) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    if (((uint32)(oldval & (uint32)_Gscan) != 0) || ((uint32)(newval & (uint32)_Gscan) != 0) || oldval == newval) {
        systemstack(() => {
            // Call on the systemstack to prevent print and throw from counting
            // against the nosplit stack reservation.
            print((@string)"runtime: casgstatus: oldval="u8, ((Δhex)(uint64)oldval), (@string)" newval="u8, ((Δhex)(uint64)newval), (@string)"\n"u8);
            @throw(casgstatusBadIncomingˢ);
        });
    }
    lockWithRankMayAcquire(nil, lockRankGscan);
    // See https://golang.org/cl/21503 for justification of the yield delay.
    UntypedInt yieldDelay = /* 5 * 1000 */ 5000;
    int64 nextYield = default!;
    // loop if gp->atomicstatus is in a scan state giving
    // GC time to finish and change the state to oldval.
    for (nint i = 0; !Ꮡgp.of(g.Ꮡatomicstatus).CompareAndSwap(oldval, newval); i++) {
        if (oldval == _Gwaiting && Ꮡgp.of(g.Ꮡatomicstatus).Load() == _Grunnable) {
            systemstack(() => {
                // Call on the systemstack to prevent throw from counting
                // against the nosplit stack reservation.
                @throw(casgstatusWaitingForˢ);
            });
        }
        if (i == 0) {
            nextYield = nanotime() + (int64)yieldDelay;
        }
        if (nanotime() < nextYield){
            for (nint x = 0; x < 10 && Ꮡgp.of(g.Ꮡatomicstatus).Load() != oldval; x++) {
                procyield(1);
            }
        } else {
            osyield();
            nextYield = nanotime() + (int64)(yieldDelay / 2);
        }
    }
    if (oldval == _Grunning) {
        // Track every gTrackingPeriod time a goroutine transitions out of running.
        if (casgstatusAlwaysTrack || (uint8)(gp.trackingSeq % (uint8)gTrackingPeriod) == 0) {
            gp.tracking = true;
        }
        gp.trackingSeq++;
    }
    if (!gp.tracking) {
        return;
    }
    // Handle various kinds of tracking.
    //
    // Currently:
    // - Time spent in runnable.
    // - Time spent blocked on a sync.Mutex or sync.RWMutex.
    var exprᴛ1 = oldval;
    if (exprᴛ1 == _Grunnable) {
        var now = nanotime();
        gp.runnableTime += now - gp.trackingStamp;
        gp.trackingStamp = 0;
    }
    else if (exprᴛ1 == _Gwaiting) {
        do {
            if (!gp.waitreason.isMutexWait()) {
                // We transitioned out of runnable, so measure how much
                // time we spent in this state and add it to
                // runnableTime.
                // Not blocking on a lock.
                break;
            }
            var now = nanotime();
            Ꮡsched.of(schedt.ᏑtotalMutexWaitTime).Add((now - gp.trackingStamp) * (int64)gTrackingPeriod);
            gp.trackingStamp = 0;
        } while (false);
    }

    // Blocking on a lock, measure it. Note that because we're
    // sampling, we have to multiply by our sampling period to get
    // a more representative estimate of the absolute value.
    // gTrackingPeriod also represents an accurate sampling period
    // because we can only enter this state from _Grunning.
    var exprᴛ2 = newval;
    if (exprᴛ2 == _Gwaiting) {
        do {
            if (!gp.waitreason.isMutexWait()) {
                // Not blocking on a lock.
                break;
            }
            var now = nanotime();
            gp.trackingStamp = now;
        } while (false);
    }
    else if (exprᴛ2 == _Grunnable) {
        var now = nanotime();
        gp.trackingStamp = now;
    }
    else if (exprᴛ2 == _Grunning) {
        gp.tracking = false;
        Ꮡsched.of(schedt.ᏑtimeToRun).record(gp.runnableTime);
        gp.runnableTime = 0;
    }

}

// Blocking on a lock. Write down the timestamp.
// We just transitioned into runnable, so record what
// time that happened.
// We're transitioning into running, so turn off
// tracking and record how much time we spent in
// runnable.

// casGToWaiting transitions gp from old to _Gwaiting, and sets the wait reason.
//
// Use this over casgstatus when possible to ensure that a waitreason is set.
internal static void casGToWaiting(ж<g> Ꮡgp, uint32 old, waitReason reason) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // Set the wait reason before calling casgstatus, because casgstatus will use it.
    gp.waitreason = reason;
    casgstatus(Ꮡgp, old, _Gwaiting);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string casGToWaitingForSuspendGˢ = "casGToWaitingForSuspendG with non-isWaitingForSuspendG wait reason"u8;

// casGToWaitingForSuspendG transitions gp from old to _Gwaiting, and sets the wait reason.
// The wait reason must be a valid isWaitingForSuspendG wait reason.
//
// Use this over casgstatus when possible to ensure that a waitreason is set.
internal static void casGToWaitingForSuspendG(ж<g> Ꮡgp, uint32 old, waitReason reason) {
    if (!reason.isWaitingForSuspendG()) {
        @throw(casGToWaitingForSuspendGˢ);
    }
    casGToWaiting(Ꮡgp, old, reason);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string copystackBadStatusNotˢ = "copystack: bad status, not Gwaiting or Grunnable"u8;

// casgstatus(gp, oldstatus, Gcopystack), assuming oldstatus is Gwaiting or Grunnable.
// Returns old status. Cannot call casgstatus directly, because we are racing with an
// async wakeup that might come in from netpoll. If we see Gwaiting from the readgstatus,
// it might have become Grunnable by the time we get to the cas. If we called casgstatus,
// it would loop waiting for the status to go back to Gwaiting, which it never will.
//
//go:nosplit
internal static uint32 casgcopystack(ж<g> Ꮡgp) {
    while (ᐧ) {
        var oldstatus = (uint32)(readgstatus(Ꮡgp) & ~(uint32)_Gscan);
        if (oldstatus != _Gwaiting && oldstatus != _Grunnable) {
            @throw(copystackBadStatusNotˢ);
        }
        if (Ꮡgp.of(g.Ꮡatomicstatus).CompareAndSwap(oldstatus, _Gcopystack)) {
            return oldstatus;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badGTransitionˢ = "bad g transition"u8;

// casGToPreemptScan transitions gp from _Grunning to _Gscan|_Gpreempted.
//
// TODO(austin): This is the only status operation that both changes
// the status and locks the _Gscan bit. Rethink this.
internal static void casGToPreemptScan(ж<g> Ꮡgp, uint32 old, uint32 @new) {
    if (old != _Grunning || @new != (uint32)((uint32)_Gscan | (uint32)_Gpreempted)) {
        @throw(badGTransitionˢ);
    }
    acquireLockRankAndM(lockRankGscan);
    while (!Ꮡgp.of(g.Ꮡatomicstatus).CompareAndSwap(_Grunning, (uint32)((uint32)_Gscan | (uint32)_Gpreempted))) {
    }
}

// casGFromPreempted attempts to transition gp from _Gpreempted to
// _Gwaiting. If successful, the caller is responsible for
// re-scheduling gp.
internal static bool casGFromPreempted(ж<g> Ꮡgp, uint32 old, uint32 @new) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    if (old != _Gpreempted || @new != _Gwaiting) {
        @throw(badGTransitionˢ);
    }
    gp.waitreason = waitReasonPreempted;
    return Ꮡgp.of(g.Ꮡatomicstatus).CompareAndSwap(_Gpreempted, _Gwaiting);
}

[GoType("num:uint8")] partial struct stwReason;

// Reasons to stop-the-world.
//
// Avoid reusing reasons and add new ones instead.
internal static stwReason stwUnknown => /* iota */ 0;                    // "unknown"

internal static stwReason stwGCMarkTerm => 1;                 // "GC mark termination"

internal static stwReason stwGCSweepTerm => 2;                // "GC sweep termination"

internal static stwReason stwWriteHeapDump => 3;              // "write heap dump"

internal static stwReason stwGoroutineProfile => 4;           // "goroutine profile"

internal static stwReason stwGoroutineProfileCleanup => 5;    // "goroutine profile cleanup"

internal static stwReason stwAllGoroutinesStack => 6;         // "all goroutines stack trace"

internal static stwReason stwReadMemStats => 7;               // "read mem stats"

internal static stwReason stwAllThreadsSyscall => 8;          // "AllThreadsSyscall"

internal static stwReason stwGOMAXPROCS => 9;                 // "GOMAXPROCS"

internal static stwReason stwStartTrace => 10;                 // "start trace"

internal static stwReason stwStopTrace => 11;                  // "stop trace"

internal static stwReason stwForTestCountPagesInUse => 12;     // "CountPagesInUse (test)"

internal static stwReason stwForTestReadMetricsSlow => 13;     // "ReadMetricsSlow (test)"

internal static stwReason stwForTestReadMemStatsSlow => 14;    // "ReadMemStatsSlow (test)"

internal static stwReason stwForTestPageCachePagesLeaked => 15; // "PageCachePagesLeaked (test)"

internal static stwReason stwForTestResetDebugLog => 16;       // "ResetDebugLog (test)"

internal static @string String(this stwReason r) {
    return stwReasonStrings[r];
}

internal static bool isGC(this stwReason r) {
    return r == stwGCMarkTerm || r == stwGCSweepTerm;
}

// If you add to this list, also add it to src/internal/trace/parser.go.
// If you change the values of any of the stw* constants, bump the trace
// version number and make a copy of this.
internal static array<@string> stwReasonStrings = new golib.SparseArray<@string>{
    [stwUnknown] = "unknown"u8,
    [stwGCMarkTerm] = "GC mark termination"u8,
    [stwGCSweepTerm] = "GC sweep termination"u8,
    [stwWriteHeapDump] = "write heap dump"u8,
    [stwGoroutineProfile] = "goroutine profile"u8,
    [stwGoroutineProfileCleanup] = "goroutine profile cleanup"u8,
    [stwAllGoroutinesStack] = "all goroutines stack trace"u8,
    [stwReadMemStats] = "read mem stats"u8,
    [stwAllThreadsSyscall] = "AllThreadsSyscall"u8,
    [stwGOMAXPROCS] = "GOMAXPROCS"u8,
    [stwStartTrace] = "start trace"u8,
    [stwStopTrace] = "stop trace"u8,
    [stwForTestCountPagesInUse] = "CountPagesInUse (test)"u8,
    [stwForTestReadMetricsSlow] = "ReadMetricsSlow (test)"u8,
    [stwForTestReadMemStatsSlow] = "ReadMemStatsSlow (test)"u8,
    [stwForTestPageCachePagesLeaked] = "PageCachePagesLeaked (test)"u8,
    [stwForTestResetDebugLog] = "ResetDebugLog (test)"u8
}.array();

// worldStop provides context from the stop-the-world required by the
// start-the-world.
[GoType] partial struct worldStop {
    internal stwReason reason;
    internal int64 startedStopping;
    internal int64 finishedStopping;
    internal int64 stoppingCPUTime;
}

// Temporary variable for stopTheWorld, when it can't write to the stack.
//
// Protected by worldsema.
internal static worldStop stopTheWorldContext;

// stopTheWorld stops all P's from executing goroutines, interrupting
// all goroutines at GC safe points and records reason as the reason
// for the stop. On return, only the current goroutine's P is running.
// stopTheWorld must not be called from a system stack and the caller
// must not hold worldsema. The caller must call startTheWorld when
// other P's should resume execution.
//
// stopTheWorld is safe for multiple goroutines to call at the
// same time. Each will execute its own stop, and the stops will
// be serialized.
//
// This is also used by routines that do stack dumps. If the system is
// in panic or being exited, this may not reliably stop all
// goroutines.
//
// Returns the STW context. When starting the world, this context must be
// passed to startTheWorld.
internal static worldStop stopTheWorld(stwReason reason) {
    semacquire(Ꮡworldsema);
    var gp = getg();
    gp.Value.m.Value.preemptoff = reason.String();
    systemstack(() => {
        stopTheWorldContext = stopTheWorldWithSema(reason); // avoid write to stack
    });
    return stopTheWorldContext;
}

// startTheWorld undoes the effects of stopTheWorld.
//
// w must be the worldStop returned by stopTheWorld.
internal static void startTheWorld(worldStop w) {
    var wʗ1 = w;
    systemstack(() => {
        startTheWorldWithSema(0, wʗ1);
    });
    // worldsema must be held over startTheWorldWithSema to ensure
    // gomaxprocs cannot change while worldsema is held.
    //
    // Release worldsema with direct handoff to the next waiter, but
    // acquirem so that semrelease1 doesn't try to yield our time.
    //
    // Otherwise if e.g. ReadMemStats is being called in a loop,
    // it might stomp on other attempts to stop the world, such as
    // for starting or ending GC. The operation this blocks is
    // so heavy-weight that we should just try to be as fair as
    // possible here.
    //
    // We don't want to just allow us to get preempted between now
    // and releasing the semaphore because then we keep everyone
    // (including, for example, GCs) waiting longer.
    var mp = acquirem();
    mp.Value.preemptoff = ""u8;
    semrelease1(Ꮡworldsema, true, 0);
    releasem(ref (mp).DerefOrNull());
}

// stopTheWorldGC has the same effect as stopTheWorld, but blocks
// until the GC is not running. It also blocks a GC from starting
// until startTheWorldGC is called.
internal static worldStop stopTheWorldGC(stwReason reason) {
    semacquire(Ꮡgcsema);
    return stopTheWorld(reason);
}

// startTheWorldGC undoes the effects of stopTheWorldGC.
//
// w must be the worldStop returned by stopTheWorld.
internal static void startTheWorldGC(worldStop w) {
    startTheWorld(w);
    semrelease(Ꮡgcsema);
}

// Holding worldsema grants an M the right to try to stop the world.
internal static ж<uint32> Ꮡworldsema = new StandardBox<uint32>(1);
internal static ref uint32 worldsema => ref Ꮡworldsema.Value;

// Holding gcsema grants the M the right to block a GC, and blocks
// until the current GC is done. In particular, it prevents gomaxprocs
// from changing concurrently.
//
// TODO(mknyszek): Once gomaxprocs and the execution tracer can handle
// being changed/enabled during a GC, remove this.
internal static ж<uint32> Ꮡgcsema = new StandardBox<uint32>(1);
internal static ref uint32 gcsema => ref Ꮡgcsema.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stopTheWorldHoldingLocksˢ = "stopTheWorld: holding locks"u8;
internal static readonly @string stopTheWorldNotStoppedˢ = "stopTheWorld: not stopped (stopwait != 0)"u8;
internal static readonly @string stopTheWorldNotStoppedˢ2 = "stopTheWorld: not stopped (status != _Pgcstop)"u8;
internal static readonly @string stopTheWorldBrokenCpuˢ = "stopTheWorld: broken CPU time accounting"u8;

// stopTheWorldWithSema is the core implementation of stopTheWorld.
// The caller is responsible for acquiring worldsema and disabling
// preemption first and then should stopTheWorldWithSema on the system
// stack:
//
//	semacquire(&worldsema, 0)
//	m.preemptoff = "reason"
//	var stw worldStop
//	systemstack(func() {
//		stw = stopTheWorldWithSema(reason)
//	})
//
// When finished, the caller must either call startTheWorld or undo
// these three operations separately:
//
//	m.preemptoff = ""
//	systemstack(func() {
//		now = startTheWorldWithSema(stw)
//	})
//	semrelease(&worldsema)
//
// It is allowed to acquire worldsema once and then execute multiple
// startTheWorldWithSema/stopTheWorldWithSema pairs.
// Other P's are able to execute between successive calls to
// startTheWorldWithSema and stopTheWorldWithSema.
// Holding worldsema causes any other goroutines invoking
// stopTheWorld to block.
//
// Returns the STW context. When starting the world, this context must be
// passed to startTheWorldWithSema.
//
//go:systemstack
internal static worldStop stopTheWorldWithSema(stwReason reason) {
    // Mark the goroutine which called stopTheWorld preemptible so its
    // stack may be scanned by the GC or observed by the execution tracer.
    //
    // This lets a mark worker scan us or the execution tracer take our
    // stack while we try to stop the world since otherwise we could get
    // in a mutual preemption deadlock.
    //
    // We must not modify anything on the G stack because a stack shrink
    // may occur, now that we switched to _Gwaiting, specifically if we're
    // doing this during the mark phase (mark termination excepted, since
    // we know that stack scanning is done by that point). A stack shrink
    // is otherwise OK though because in order to return from this function
    // (and to leave the system stack) we must have preempted all
    // goroutines, including any attempting to scan our stack, in which
    // case, any stack shrinking will have already completed by the time we
    // exit.
    //
    // N.B. The execution tracer is not aware of this status transition and
    // andles it specially based on the wait reason.
    casGToWaitingForSuspendG((~(~getg()).m).curg, _Grunning, waitReasonStoppingTheWorld);
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.STWStart(reason);
        traceRelease(Δtrace);
    }
    var gp = getg();
    // If we hold a lock, then we won't be able to stop another M
    // that is blocked trying to acquire the lock.
    if ((~(~gp).m).locks > 0) {
        @throw(stopTheWorldHoldingLocksˢ);
    }
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    var start = nanotime(); // exclude time waiting for sched.lock from start and total time metrics.
    sched.stopwait = gomaxprocs;
    Ꮡsched.of(schedt.Ꮡgcwaiting).Store(true);
    preemptall();
    // stop current P
    (~(~gp).m).p.ptr().Value.status = _Pgcstop; // Pgcstop is only diagnostic.
    (~(~gp).m).p.ptr().Value.gcStopTime = start;
    sched.stopwait--;
    // try to retake all P's in Psyscall status
    Δtrace = traceAcquire();
    foreach (var (_, pp) in allp) {
        var s = pp.Value.status;
        if (s == _Psyscall && atomic.Cas(pp.of(runtime_package.Δp.Ꮡstatus), s, _Pgcstop)) {
            if (Δtrace.ok()) {
                Δtrace.ProcSteal(pp, false);
            }
            pp.Value.syscalltick++;
            pp.Value.gcStopTime = nanotime();
            sched.stopwait--;
        }
    }
    if (Δtrace.ok()) {
        traceRelease(Δtrace);
    }
    // stop idle P's
    var now = nanotime();
    while (ᐧ) {
        var (pp, _) = pidleget(now);
        if (pp == nil) {
            break;
        }
        pp.Value.status = _Pgcstop;
        pp.Value.gcStopTime = nanotime();
        sched.stopwait--;
    }
    var wait = sched.stopwait > 0;
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    // wait for remaining P's to stop voluntarily
    if (wait) {
        while (ᐧ) {
            // wait for 100us, then try to re-preempt in case of any races
            if (notetsleep(Ꮡsched.of(schedt.Ꮡstopnote), 100 * 1000)) {
                noteclear(Ꮡsched.of(schedt.Ꮡstopnote));
                break;
            }
            preemptall();
        }
    }
    var finish = nanotime();
    var startTime = finish - start;
    if (reason.isGC()){
        Ꮡsched.of(schedt.ᏑstwStoppingTimeGC).record(startTime);
    } else {
        Ꮡsched.of(schedt.ᏑstwStoppingTimeOther).record(startTime);
    }
    // Double-check we actually stopped everything, and all the invariants hold.
    // Also accumulate all the time spent by each P in _Pgcstop up to the point
    // where everything was stopped. This will be accumulated into the total pause
    // CPU time by the caller.
    var stoppingCPUTime = (int64)0;
    @string bad = ""u8;
    if (sched.stopwait != 0){
        bad = stopTheWorldNotStoppedˢ;
    } else {
        foreach (var (_, pp) in allp) {
            if ((~pp).status != _Pgcstop) {
                bad = stopTheWorldNotStoppedˢ2;
            }
            if ((~pp).gcStopTime == 0 && bad == ""u8) {
                bad = stopTheWorldBrokenCpuˢ;
            }
            stoppingCPUTime += finish - (~pp).gcStopTime;
            pp.Value.gcStopTime = 0;
        }
    }
    if (Ꮡfreezing.Load()) {
        // Some other thread is panicking. This can cause the
        // sanity checks above to fail if the panic happens in
        // the signal handler on a stopped thread. Either way,
        // we should halt this thread.
        @lock(Ꮡdeadlock);
        @lock(Ꮡdeadlock);
    }
    if (bad != ""u8) {
        @throw(bad);
    }
    worldStopped();
    // Switch back to _Grunning, now that the world is stopped.
    casgstatus((~(~getg()).m).curg, _Gwaiting, _Grunning);
    return new worldStop(
        reason: reason,
        startedStopping: start,
        finishedStopping: finish,
        stoppingCPUTime: stoppingCPUTime
    );
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string startTheWorldˢ = "startTheWorld: inconsistent mp->nextp"u8;

// reason is the same STW reason passed to stopTheWorld. start is the start
// time returned by stopTheWorld.
//
// now is the current time; prefer to pass 0 to capture a fresh timestamp.
//
// stattTheWorldWithSema returns now.
internal static int64 startTheWorldWithSema(int64 now, worldStop w) {
    assertWorldStopped();
    var mp = acquirem(); // disable preemption because it can be holding p in a local var
    if (netpollinited()) {
        ref var list = ref heap<gList>(out var Ꮡlist);
        (list, var delta) = netpoll(0); // non-blocking
        injectglist(Ꮡlist);
        netpollAdjustWaiters(delta);
    }
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    var procs = gomaxprocs;
    if (newprocs != 0) {
        procs = newprocs;
        newprocs = 0;
    }
    var p1 = procresize(procs);
    Ꮡsched.of(schedt.Ꮡgcwaiting).Store(false);
    if (Ꮡsched.of(schedt.Ꮡsysmonwait).Load()) {
        Ꮡsched.of(schedt.Ꮡsysmonwait).Store(false);
        notewakeup(Ꮡsched.of(schedt.Ꮡsysmonnote));
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    worldStarted();
    while (p1 != nil) {
        var Δp = p1;
        p1 = (~p1).link.ptr();
        if ((~Δp).m != 0){
            var mpΔ1 = (~Δp).m.ptr();
            Δp.Value.m = 0;
            if ((~mpΔ1).nextp != 0) {
                @throw(startTheWorldˢ);
            }
            mpΔ1.of(m.Ꮡnextp).set(Δp);
            notewakeup(mpΔ1.of(m.Ꮡpark));
        } else {
            // Start M to run P.  Do not start another M below.
            newm(default!, Δp, -1);
        }
    }
    // Capture start-the-world time before doing clean-up tasks.
    if (now == 0) {
        now = nanotime();
    }
    var totalTime = now - w.startedStopping;
    if (w.reason.isGC()){
        Ꮡsched.of(schedt.ᏑstwTotalTimeGC).record(totalTime);
    } else {
        Ꮡsched.of(schedt.ᏑstwTotalTimeOther).record(totalTime);
    }
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.STWDone();
        traceRelease(Δtrace);
    }
    // Wakeup an additional proc in case we have excessive runnable goroutines
    // in local queues or in the global queue. If we don't, the proc will park itself.
    // If we have lots of excessive work, resetspinning will unpark additional procs as necessary.
    wakep();
    releasem(ref (mp).DerefOrNull());
    return now;
}

// usesLibcall indicates whether this runtime performs system calls
// via libcall.
internal static bool usesLibcall() {
    var exprᴛ1 = GOOS;
    if (exprᴛ1 == "aix"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "illumos"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "solaris"u8 || exprᴛ1 == "windows"u8) {
        return true;
    }
    if (exprᴛ1 == "openbsd"u8) {
        return GOARCH != "mips64"u8;
    }

    return false;
}

// mStackIsSystemAllocated indicates whether this runtime starts on a
// system-allocated stack.
internal static bool mStackIsSystemAllocated() {
    var exprᴛ1 = GOOS;
    if (exprᴛ1 == "aix"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "plan9"u8 || exprᴛ1 == "illumos"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "solaris"u8 || exprᴛ1 == "windows"u8) {
        return true;
    }
    if (exprᴛ1 == "openbsd"u8) {
        return GOARCH != "mips64"u8;
    }

    return false;
}

// mstart is the entry-point for new Ms.
// It is written in assembly, uses ABI0, is marked TOPFRAME, and calls mstart0.
internal static partial void mstart();

// mstart0 is the Go entry-point for new Ms.
// This must not split the stack because we may not even have stack
// bounds set up yet.
//
// May run during STW (because it doesn't have a P yet), so write
// barriers are not allowed.
//
//go:nosplit
//go:nowritebarrierrec
internal static void mstart0() {
    var gp = getg();
    var osStack = (~gp).stack.lo == 0;
    if (osStack) {
        // Initialize stack bounds from system stack.
        // Cgo may have left stack size in stack.hi.
        // minit may update the stack bounds.
        //
        // Note: these bounds may not be very accurate.
        // We set hi to &size, but there are things above
        // it. The 1024 is supposed to compensate this,
        // but is somewhat arbitrary.
        ref var size = ref heap<uintptr>(out var Ꮡsize);
        size = gp.Value.stack.hi;
        if (size == 0) {
            size = 16384 * sys.StackGuardMultiplier;
        }
        gp.Value.stack.hi = (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡsize));
        gp.Value.stack.lo = (~gp).stack.hi - size + 1024;
    }
    // Initialize stack guard so that we can start calling regular
    // Go code.
    gp.Value.stackguard0 = (~gp).stack.lo + (uintptr)stackGuard;
    // This is the g0, so we can also call go:systemstack
    // functions, which check stackguard1.
    gp.Value.stackguard1 = gp.Value.stackguard0;
    mstart1();
    // Exit this thread.
    if (mStackIsSystemAllocated()) {
        // Windows, Solaris, illumos, Darwin, AIX and Plan 9 always system-allocate
        // the stack, but put it in gp.stack before mstart,
        // so the logic above hasn't set osStack yet.
        osStack = true;
    }
    mexit(osStack);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badRuntimeMstartˢ = "bad runtime·mstart"u8;

// The go:noinline is to guarantee the getcallerpc/getcallersp below are safe,
// so that we can set up g0.sched to return to the call of mstart1 above.
//
//go:noinline
internal static void mstart1() {
    var gp = getg();
    if (gp != (~(~gp).m).g0) {
        @throw(badRuntimeMstartˢ);
    }
    // Set up m.g0.sched as a label returning to just
    // after the mstart1 call in mstart0 above, for use by goexit0 and mcall.
    // We're never coming back to mstart1 after we call schedule,
    // so other calls can reuse the current frame.
    // And goexit0 does a gogo that needs to return from mstart1
    // and let mstart0 exit the thread.
    gp.Value.sched.g = new Δguintptr(gp);
    gp.Value.sched.pc = getcallerpc();
    gp.Value.sched.sp = getcallersp();
    asminit();
    minit();
    // Install signal handlers; after minit so that minit can
    // prepare the thread to be able to handle the signals.
    if ((~gp).m == Ꮡm0) {
        mstartm0();
    }
    {
        var fn = gp.Value.m.Value.mstartfn; if (fn != default!) {
            fn();
        }
    }
    if ((~gp).m != Ꮡm0) {
        acquirep((~(~gp).m).nextp.ptr());
        gp.Value.m.Value.nextp = 0;
    }
    schedule();
}

// mstartm0 implements part of mstart1 that only runs on the m0.
//
// Write barriers are allowed here because we know the GC can't be
// running yet, so they'll be no-ops.
//
//go:yeswritebarrierrec
internal static void mstartm0() {
    // Create an extra M for callbacks on threads not created by Go.
    // An extra M is also needed on Windows for callbacks created by
    // syscall.NewCallback. See issue #6751 for details.
    if ((iscgo || GOOS == "windows"u8) && !cgoHasExtraM) {
        cgoHasExtraM = true;
        newextram();
    }
    initsig(false);
}

// mPark causes a thread to park itself, returning once woken.
//
//go:nosplit
internal static void mPark() {
    var gp = getg();
    notesleep((~gp).m.of(m.Ꮡpark));
    noteclear((~gp).m.of(m.Ꮡpark));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lockedM0WokeUpˢ = "locked m0 woke up"u8;
internal static readonly @string mNotFoundInAllmˢ = "m not found in allm"u8;

// mexit tears down and exits the current thread.
//
// Don't call this directly to exit the thread, since it must run at
// the top of the thread stack. Instead, use gogo(&gp.m.g0.sched) to
// unwind the stack to the point that exits the thread.
//
// It is entered with m.p != nil, so write barriers are allowed. It
// will release the P before exiting.
//
//go:yeswritebarrierrec
internal static void mexit(bool osStack) {
    var mp = getg().Value.m;
    if (mp == Ꮡm0) {
        // This is the main thread. Just wedge it.
        //
        // On Linux, exiting the main thread puts the process
        // into a non-waitable zombie state. On Plan 9,
        // exiting the main thread unblocks wait even though
        // other threads are still running. On Solaris we can
        // neither exitThread nor return from mstart. Other
        // bad things probably happen on other platforms.
        //
        // We could try to clean up this M more before wedging
        // it, but that complicates signal handling.
        handoffp(releasep());
        @lock(Ꮡsched.of(schedt.Ꮡlock));
        sched.nmfreed++;
        checkdead();
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        mPark();
        @throw(lockedM0WokeUpˢ);
    }
    sigblock(true);
    unminit();
    // Free the gsignal stack.
    if ((~mp).gsignal != nil) {
        stackfree((~(~mp).gsignal).stack);
        // On some platforms, when calling into VDSO (e.g. nanotime)
        // we store our g on the gsignal stack, if there is one.
        // Now the stack is freed, unlink it from the m, so we
        // won't write to it when calling VDSO code.
        mp.Value.gsignal = default!;
    }
    // Remove m from allm.
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    for (var pprev = Ꮡallm; pprev.ValueSlot != nil; pprev = (pprev.ValueSlot).of(m.Ꮡalllink)) {
        if (pprev.ValueSlot == mp) {
            pprev.ValueSlot = mp.Value.alllink;
            goto found;
        }
    }
    @throw(mNotFoundInAllmˢ);
found:
    mp.of(m.ᏑfreeWait).Store(freeMWait);
    // Events must not be traced after this point.
    // Delay reaping m until it's done with the stack.
    //
    // Put mp on the free list, though it will not be reaped while freeWait
    // is freeMWait. mp is no longer reachable via allm, so even if it is
    // on an OS stack, we must keep a reference to mp alive so that the GC
    // doesn't free mp while we are still using it.
    //
    // Note that the free list must not be linked through alllink because
    // some functions walk allm without locking, so may be using alllink.
    //
    // N.B. It's important that the M appears on the free list simultaneously
    // with it being removed so that the tracer can find it.
    mp.Value.freelink = sched.freem;
    sched.freem = mp;
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    atomic.Xadd64(Ꮡncgocall, (int64)(~mp).ncgocall);
    Ꮡsched.of(schedt.ᏑtotalRuntimeLockWaitTime).Add(mp.of(m.ᏑmLockProfile).of(mLockProfile.ᏑwaitTime).Load());
    // Release the P.
    handoffp(releasep());
    // After this point we must not have write barriers.
    // Invoke the deadlock detector. This must happen after
    // handoffp because it may have started a new M to take our
    // P's work.
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    sched.nmfreed++;
    checkdead();
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    if (GOOS == "darwin"u8 || GOOS == "ios"u8) {
        // Make sure pendingPreemptSignals is correct when an M exits.
        // For #41702.
        if (mp.of(m.ᏑsignalPending).Load() != 0) {
            ᏑpendingPreemptSignals.Add(-1);
        }
    }
    // Destroy all allocated resources. After this is called, we may no
    // longer take any locks.
    mdestroy(ref (mp).DerefOrNull());
    if (osStack) {
        // No more uses of mp, so it is safe to drop the reference.
        mp.of(m.ᏑfreeWait).Store(freeMRef);
        // Return from mstart and let the system thread
        // library free the g0 stack and terminate the thread.
        return;
    }
    // mstart is the thread's entry point, so there's nothing to
    // return to. Exit the thread directly. exitThread will clear
    // m.freeWait when it's done with the stack and the m can be
    // reaped.
    exitThread(mp.of(m.ᏑfreeWait));
}

// forEachP calls fn(p) for every P p when p reaches a GC safe point.
// If a P is currently executing code, this will bring the P to a GC
// safe point and execute fn on that P. If the P is not executing code
// (it is idle or in a syscall), this will call fn(p) directly while
// preventing the P from exiting its state. This does not ensure that
// fn will run on every CPU executing Go code, but it acts as a global
// memory barrier. GC uses this as a "ragged barrier."
//
// The caller must hold worldsema. fn must not refer to any
// part of the current goroutine's stack, since the GC may move it.
internal static void forEachP(waitReason reason, Action<ж<Δp>> fn) {
    systemstack(() => {
        var gp = getg().Value.m.Value.curg;
        // Mark the user stack as preemptible so that it may be scanned
        // by the GC or observed by the execution tracer. Otherwise, our
        // attempt to force all P's to a safepoint could result in a
        // deadlock as we attempt to preempt a goroutine that's trying
        // to preempt us (e.g. for a stack scan).
        //
        // We must not modify anything on the G stack because a stack shrink
        // may occur. A stack shrink is otherwise OK though because in order
        // to return from this function (and to leave the system stack) we
        // must have preempted all goroutines, including any attempting
        // to scan our stack, in which case, any stack shrinking will
        // have already completed by the time we exit.
        //
        // N.B. The execution tracer is not aware of this status
        // transition and handles it specially based on the
        // wait reason.
        casGToWaitingForSuspendG(gp, _Grunning, reason);
        forEachPInternal(fn);
        casgstatus(gp, _Gwaiting, _Grunning);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string forEachPSchedˢ = "forEachP: sched.safePointWait != 0"u8;
internal static readonly @string forEachPNotDoneˢ = "forEachP: not done"u8;
internal static readonly @string forEachPPDidNotRunFnˢ = "forEachP: P did not run fn"u8;

// forEachPInternal calls fn(p) for every P p when p reaches a GC safe point.
// It is the internal implementation of forEachP.
//
// The caller must hold worldsema and either must ensure that a GC is not
// running (otherwise this may deadlock with the GC trying to preempt this P)
// or it must leave its goroutine in a preemptible state before it switches
// to the systemstack. Due to these restrictions, prefer forEachP when possible.
//
//go:systemstack
internal static void forEachPInternal(Action<ж<Δp>> fn) {
    var mp = acquirem();
    var pp = (~(~getg()).m).p.ptr();
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    if (sched.safePointWait != 0) {
        @throw(forEachPSchedˢ);
    }
    sched.safePointWait = gomaxprocs - 1;
    sched.safePointFn = fn;
    // Ask all Ps to run the safe point function.
    foreach (var (_, p2) in allp) {
        if (p2 != pp) {
            atomic.Store(p2.of(runtime_package.Δp.ᏑrunSafePointFn), 1);
        }
    }
    preemptall();
    // Any P entering _Pidle or _Psyscall from now on will observe
    // p.runSafePointFn == 1 and will call runSafePointFn when
    // changing its status to _Pidle/_Psyscall.
    // Run safe point function for all idle Ps. sched.pidle will
    // not change because we hold sched.lock.
    for (var Δp = sched.pidle.ptr(); Δp != nil; Δp = (~Δp).link.ptr()) {
        if (atomic.Cas(Δp.of(runtime_package.Δp.ᏑrunSafePointFn), 1, 0)) {
            fn(Δp);
            sched.safePointWait--;
        }
    }
    var wait = sched.safePointWait > 0;
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    // Run fn for the current P.
    fn(pp);
    // Force Ps currently in _Psyscall into _Pidle and hand them
    // off to induce safe point function execution.
    foreach (var (_, p2) in allp) {
        var s = p2.Value.status;
        // We need to be fine-grained about tracing here, since handoffp
        // might call into the tracer, and the tracer is non-reentrant.
        var Δtrace = traceAcquire();
        if (s == _Psyscall && (~p2).runSafePointFn == 1 && atomic.Cas(p2.of(runtime_package.Δp.Ꮡstatus), s, _Pidle)){
            if (Δtrace.ok()) {
                // It's important that we traceRelease before we call handoffp, which may also traceAcquire.
                Δtrace.ProcSteal(p2, false);
                traceRelease(Δtrace);
            }
            p2.Value.syscalltick++;
            handoffp(p2);
        } else 
        if (Δtrace.ok()) {
            traceRelease(Δtrace);
        }
    }
    // Wait for remaining Ps to run fn.
    if (wait) {
        while (ᐧ) {
            // Wait for 100us, then try to re-preempt in
            // case of any races.
            //
            // Requires system stack.
            if (notetsleep(Ꮡsched.of(schedt.ᏑsafePointNote), 100 * 1000)) {
                noteclear(Ꮡsched.of(schedt.ᏑsafePointNote));
                break;
            }
            preemptall();
        }
    }
    if (sched.safePointWait != 0) {
        @throw(forEachPNotDoneˢ);
    }
    foreach (var (_, p2) in allp) {
        if ((~p2).runSafePointFn != 0) {
            @throw(forEachPPDidNotRunFnˢ);
        }
    }
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    sched.safePointFn = default!;
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    releasem(ref (mp).DerefOrNull());
}

// runSafePointFn runs the safe point function, if any, for this P.
// This should be called like
//
//	if getg().m.p.runSafePointFn != 0 {
//	    runSafePointFn()
//	}
//
// runSafePointFn must be checked on any transition in to _Pidle or
// _Psyscall to avoid a race where forEachP sees that the P is running
// just before the P goes into _Pidle/_Psyscall and neither forEachP
// nor the P run the safe-point function.
internal static void runSafePointFn() {
    var Δp = (~(~getg()).m).p.ptr();
    // Resolve the race between forEachP running the safe-point
    // function on this P's behalf and this P running the
    // safe-point function directly.
    if (!atomic.Cas(Δp.of(runtime_package.Δp.ᏑrunSafePointFn), 1, 0)) {
        return;
    }
    sched.safePointFn(Δp);
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    sched.safePointWait--;
    if (sched.safePointWait == 0) {
        notewakeup(Ꮡsched.of(schedt.ᏑsafePointNote));
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock));
}

// When running with cgo, we call _cgo_thread_start
// to start threads for us so that we can play nicely with
// foreign code.
internal static @unsafe.Pointer cgoThreadStart;

[GoType] partial struct cgothreadstart {
    internal Δguintptr g;
    internal ж<uint64> tls;
    internal @unsafe.Pointer fn;
}

// Allocate a new m unassociated with any thread.
// Can use p for allocation context if needed.
// fn is recorded as the new m's m.mstartfn.
// id is optional pre-allocated m ID. Omit by passing -1.
//
// This function is allowed to have write barriers even if the caller
// isn't because it borrows pp.
//
//go:yeswritebarrierrec
internal static ж<m> allocm(ж<Δp> Ꮡpp, Action fn, int64 id) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    ᏑallocmLock.rlock();
    // The caller owns pp, but we may borrow (i.e., acquirep) it. We must
    // disable preemption to ensure it is not stolen, which would make the
    // caller lose ownership.
    acquirem();
    var gp = getg();
    if ((~(~gp).m).p == 0) {
        acquirep(Ꮡpp); // temporarily borrow p for mallocs in this function
    }
    // Release the free M list. We need to do this somewhere and
    // this may free up a stack we can use.
    if (sched.freem != nil) {
        @lock(Ꮡsched.of(schedt.Ꮡlock));
        ж<m> newList = default!;
        for (var freemᴛ1 = sched.freem; freemᴛ1 != nil; ) {
            var freem = freemᴛ1;
            // Wait for freeWait to indicate that freem's stack is unused.
            var wait = freem.of(m.ᏑfreeWait).Load();
            if (wait == freeMWait) {
                var next = freem.Value.freelink;
                freem.Value.freelink = newList;
                newList = freem;
                freem = next;
                freemᴛ1 = freem;
                continue;
            }
            // Drop any remaining trace resources.
            // Ms can continue to emit events all the way until wait != freeMWait,
            // so it's only safe to call traceThreadDestroy at this point.
            if (traceEnabled() || traceShuttingDown()) {
                traceThreadDestroy(freem);
            }
            // Free the stack if needed. For freeMRef, there is
            // nothing to do except drop freem from the sched.freem
            // list.
            if (wait == freeMStack) {
                // stackfree must be on the system stack, but allocm is
                // reachable off the system stack transitively from
                // startm.
                var freemʗ1 = freem;
                systemstack(() => {
                    stackfree((~(~freemʗ1).g0).stack);
                });
            }
            freem = freem.Value.freelink;
            freemᴛ1 = freem;
        }
        sched.freem = newList;
        unlock(Ꮡsched.of(schedt.Ꮡlock));
    }
    var mp = @new<m>();
    mp.Value.mstartfn = fn;
    mcommoninit(mp, id);
    // In case of cgo or Solaris or illumos or Darwin, pthread_create will make us a stack.
    // Windows and Plan 9 will layout sched stack on OS stack.
    if (iscgo || mStackIsSystemAllocated()){
        mp.Value.g0 = malg(-1);
    } else {
        mp.Value.g0 = malg(16384 * sys.StackGuardMultiplier);
    }
    mp.Value.g0.Value.m = mp;
    if (Ꮡpp == (~(~gp).m).p.ptr()) {
        releasep();
    }
    releasem(ref ((~gp).m).DerefOrNull());
    ᏑallocmLock.runlock();
    return mp;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fatalErrorCgoCallbackˢ = "fatal error: cgo callback before cgo call\n"u8;

// needm is called when a cgo callback happens on a
// thread without an m (a thread not created by Go).
// In this case, needm is expected to find an m to use
// and return with m, g initialized correctly.
// Since m and g are not set now (likely nil, but see below)
// needm is limited in what routines it can call. In particular
// it can only call nosplit functions (textflag 7) and cannot
// do any scheduling that requires an m.
//
// In order to avoid needing heavy lifting here, we adopt
// the following strategy: there is a stack of available m's
// that can be stolen. Using compare-and-swap
// to pop from the stack has ABA races, so we simulate
// a lock by doing an exchange (via Casuintptr) to steal the stack
// head and replace the top pointer with MLOCKED (1).
// This serves as a simple spin lock that we can use even
// without an m. The thread that locks the stack in this way
// unlocks the stack by storing a valid stack head pointer.
//
// In order to make sure that there is always an m structure
// available to be stolen, we maintain the invariant that there
// is always one more than needed. At the beginning of the
// program (if cgo is in use) the list is seeded with a single m.
// If needm finds that it has taken the last m off the list, its job
// is - once it has installed its own m so that it can do things like
// allocate memory - to create a spare m and put it on the list.
//
// Each of these extra m's also has a g0 and a curg that are
// pressed into service as the scheduling stack and current
// goroutine for the duration of the cgo callback.
//
// It calls dropm to put the m back on the list,
// 1. when the callback is done with the m in non-pthread platforms,
// 2. or when the C thread exiting on pthread platforms.
//
// The signal argument indicates whether we're called from a signal
// handler.
//
//go:nosplit
internal static void needm(bool signal) {
    if ((iscgo || GOOS == "windows"u8) && !cgoHasExtraM) {
        // Can happen if C/C++ code calls Go from a global ctor.
        // Can also happen on Windows if a global ctor uses a
        // callback created by syscall.NewCallback. See issue #6751
        // for details.
        //
        // Can not throw, because scheduler is not initialized yet.
        writeErrStr(fatalErrorCgoCallbackˢ);
        exit(1);
    }
    // Save and block signals before getting an M.
    // The signal handler may call needm itself,
    // and we must avoid a deadlock. Also, once g is installed,
    // any incoming signals will try to execute,
    // but we won't have the sigaltstack settings and other data
    // set up appropriately until the end of minit, which will
    // unblock the signals. This is the same dance as when
    // starting a new m to run Go code via newosproc.
    ref var sigmask = ref heap(new sigset(), out var Ꮡsigmask);
    sigsave(Ꮡsigmask);
    sigblock(false);
    // getExtraM is safe here because of the invariant above,
    // that the extra list always contains or will soon contain
    // at least one m.
    var (mp, last) = getExtraM();
    // Set needextram when we've just emptied the list,
    // so that the eventual call into cgocallbackg will
    // allocate a new m for the extra list. We delay the
    // allocation until then so that it can be done
    // after exitsyscall makes sure it is okay to be
    // running at all (that is, there's no garbage collection
    // running right now).
    mp.Value.needextram = last;
    // Store the original signal mask for use by minit.
    mp.Value.sigmask = sigmask.Clone();
    // Install TLS on some platforms (previously setg
    // would do this if necessary).
    osSetupTLS(ref (mp).DerefOrNull());
    // Install g (= m->g0) and set the stack bounds
    // to match the current stack.
    setg((~mp).g0);
    var sp = getcallersp();
    callbackUpdateSystemStack(ref (mp).DerefOrNull(), sp, signal);
    // Should mark we are already in Go now.
    // Otherwise, we may call needm again when we get a signal, before cgocallbackg1,
    // which means the extram list may be empty, that will cause a deadlock.
    mp.Value.isExtraInC = false;
    // Initialize this thread to use the m.
    asminit();
    minit();
    // Emit a trace event for this dead -> syscall transition,
    // but only if we're not in a signal handler.
    //
    // N.B. the tracer can run on a bare M just fine, we just have
    // to make sure to do this before setg(nil) and unminit.
    traceLocker Δtrace = default!;
    if (!signal) {
        Δtrace = traceAcquire();
    }
    // mp.curg is now a real goroutine.
    casgstatus((~mp).curg, _Gdead, _Gsyscall);
    Ꮡsched.of(schedt.Ꮡngsys).Add(-1);
    if (!signal) {
        if (Δtrace.ok()) {
            Δtrace.GoCreateSyscall((~mp).curg);
            traceRelease(Δtrace);
        }
    }
    mp.Value.isExtraInSig = signal;
}

// Acquire an extra m and bind it to the C thread when a pthread key has been created.
//
//go:nosplit
internal static void needAndBindM() {
    needm(false);
    if (_cgo_pthread_key_created != nil && ~(ж<uintptr>)(uintptr)(_cgo_pthread_key_created) != 0) {
        cgoBindM();
    }
}

// newextram allocates m's and puts them on the extra list.
// It is called with a working local m, so that it can do things
// like call schedlock and allocate.
internal static void newextram() {
    var c = ᏑextraMWaiters.Swap(0);
    if (c > 0){
        for (var i = (uint32)0; i < c; i++) {
            oneNewExtraM();
        }
    } else 
    if (ᏑextraMLength.Load() == 0) {
        // Make sure there is at least one extra M.
        oneNewExtraM();
    }
}

// oneNewExtraM allocates an m and puts it on the extra list.
internal static void oneNewExtraM() {
    // Create extra goroutine locked to extra m.
    // The goroutine is the context in which the cgo callback will run.
    // The sched.pc will never be returned to, but setting it to
    // goexit makes clear to the traceback routines where
    // the goroutine stack ends.
    var mp = allocm(nil, default!, -1);
    var gp = malg(4096);
    gp.Value.sched.pc = abi.FuncPCABI0(goexit) + (uintptr)sys.PCQuantum;
    gp.Value.sched.sp = gp.Value.stack.hi;
    gp.Value.sched.sp -= 4 * goarch.PtrSize; // extra space in case of reads slightly beyond frame
    gp.Value.sched.lr = 0;
    gp.Value.sched.g = new Δguintptr(gp);
    gp.Value.syscallpc = gp.Value.sched.pc;
    gp.Value.syscallsp = gp.Value.sched.sp;
    gp.Value.stktopsp = gp.Value.sched.sp;
    // malg returns status as _Gidle. Change to _Gdead before
    // adding to allg where GC can see it. We use _Gdead to hide
    // this from tracebacks and stack scans since it isn't a
    // "real" goroutine until needm grabs it.
    casgstatus(gp, _Gidle, _Gdead);
    gp.Value.m = mp;
    mp.Value.curg = gp;
    mp.Value.isextra = true;
    // mark we are in C by default.
    mp.Value.isExtraInC = true;
    mp.Value.lockedInt++;
    mp.of(m.Ꮡlockedg).set(gp);
    gp.of(g.Ꮡlockedm).set(mp);
    gp.Value.goid = Ꮡsched.of(schedt.Ꮡgoidgen).Add(1);
    if (raceenabled) {
        gp.Value.racectx = racegostart(abi.FuncPCABIInternal(newextram) + (uintptr)sys.PCQuantum);
    }
    // put on allg for garbage collector
    allgadd(gp);
    // gp is now on the allg list, but we don't want it to be
    // counted by gcount. It would be more "proper" to increment
    // sched.ngfree, but that requires locking. Incrementing ngsys
    // has the same effect.
    Ꮡsched.of(schedt.Ꮡngsys).Add(1);
    // Add m to the extra list.
    addExtraM(mp);
}

// dropm puts the current m back onto the extra list.
//
// 1. On systems without pthreads, like Windows
// dropm is called when a cgo callback has called needm but is now
// done with the callback and returning back into the non-Go thread.
//
// The main expense here is the call to signalstack to release the
// m's signal stack, and then the call to needm on the next callback
// from this thread. It is tempting to try to save the m for next time,
// which would eliminate both these costs, but there might not be
// a next time: the current thread (which Go does not control) might exit.
// If we saved the m for that thread, there would be an m leak each time
// such a thread exited. Instead, we acquire and release an m on each
// call. These should typically not be scheduling operations, just a few
// atomics, so the cost should be small.
//
// 2. On systems with pthreads
// dropm is called while a non-Go thread is exiting.
// We allocate a pthread per-thread variable using pthread_key_create,
// to register a thread-exit-time destructor.
// And store the g into a thread-specific value associated with the pthread key,
// when first return back to C.
// So that the destructor would invoke dropm while the non-Go thread is exiting.
// This is much faster since it avoids expensive signal-related syscalls.
//
// This always runs without a P, so //go:nowritebarrierrec is required.
//
// This may run with a different stack than was recorded in g0 (there is no
// call to callbackUpdateSystemStack prior to dropm), so this must be
// //go:nosplit to avoid the stack bounds check.
//
//go:nowritebarrierrec
//go:nosplit
internal static void dropm() {
    // Clear m and g, and return m to the extra list.
    // After the call to setg we can only call nosplit functions
    // with no pointer manipulation.
    var mp = getg().Value.m;
    // Emit a trace event for this syscall -> dead transition.
    //
    // N.B. the tracer can run on a bare M just fine, we just have
    // to make sure to do this before setg(nil) and unminit.
    traceLocker Δtrace = default!;
    if (!(~mp).isExtraInSig) {
        Δtrace = traceAcquire();
    }
    // Return mp.curg to dead state.
    casgstatus((~mp).curg, _Gsyscall, _Gdead);
    mp.Value.curg.Value.preemptStop = false;
    Ꮡsched.of(schedt.Ꮡngsys).Add(1);
    if (!(~mp).isExtraInSig) {
        if (Δtrace.ok()) {
            Δtrace.GoDestroySyscall();
            traceRelease(Δtrace);
        }
    }
    // Trash syscalltick so that it doesn't line up with mp.old.syscalltick anymore.
    //
    // In the new tracer, we model needm and dropm and a goroutine being created and
    // destroyed respectively. The m then might get reused with a different procid but
    // still with a reference to oldp, and still with the same syscalltick. The next
    // time a G is "created" in needm, it'll return and quietly reacquire its P from a
    // different m with a different procid, which will confuse the trace parser. By
    // trashing syscalltick, we ensure that it'll appear as if we lost the P to the
    // tracer parser and that we just reacquired it.
    //
    // Trash the value by decrementing because that gets us as far away from the value
    // the syscall exit code expects as possible. Setting to zero is risky because
    // syscalltick could already be zero (and in fact, is initialized to zero).
    mp.Value.syscalltick--;
    // Reset trace state unconditionally. This goroutine is being 'destroyed'
    // from the perspective of the tracer.
    (~mp).curg.of(g.Ꮡtrace).reset();
    // Flush all the M's buffers. This is necessary because the M might
    // be used on a different thread with a different procid, so we have
    // to make sure we don't write into the same buffer.
    if (traceEnabled() || traceShuttingDown()) {
        // Acquire sched.lock across thread destruction. One of the invariants of the tracer
        // is that a thread cannot disappear from the tracer's view (allm or freem) without
        // it noticing, so it requires that sched.lock be held over traceThreadDestroy.
        //
        // This isn't strictly necessary in this case, because this thread never leaves allm,
        // but the critical section is short and dropm is rare on pthread platforms, so just
        // take the lock and play it safe. traceThreadDestroy also asserts that the lock is held.
        @lock(Ꮡsched.of(schedt.Ꮡlock));
        traceThreadDestroy(mp);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
    }
    mp.Value.isExtraInSig = false;
    // Block signals before unminit.
    // Unminit unregisters the signal handling stack (but needs g on some systems).
    // Setg(nil) clears g, which is the signal handler's cue not to run Go handlers.
    // It's important not to try to handle a signal between those two steps.
    var sigmask = mp.Value.sigmask.Clone();
    sigblock(false);
    unminit();
    setg(nil);
    // Clear g0 stack bounds to ensure that needm always refreshes the
    // bounds when reusing this M.
    var g0 = mp.Value.g0;
    g0.Value.stack.hi = 0;
    g0.Value.stack.lo = 0;
    g0.Value.stackguard0 = 0;
    g0.Value.stackguard1 = 0;
    mp.Value.g0StackAccurate = false;
    putExtraM(mp);
    msigrestore(sigmask);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bindmInUnexpectedGoosˢ = "bindm in unexpected GOOS"u8;
internal static readonly @string theCurrentGIsNotG0ˢ = "the current g is not g0"u8;

// bindm store the g0 of the current m into a thread-specific value.
//
// We allocate a pthread per-thread variable using pthread_key_create,
// to register a thread-exit-time destructor.
// We are here setting the thread-specific value of the pthread key, to enable the destructor.
// So that the pthread_key_destructor would dropm while the C thread is exiting.
//
// And the saved g will be used in pthread_key_destructor,
// since the g stored in the TLS by Go might be cleared in some platforms,
// before the destructor invoked, so, we restore g by the stored g, before dropm.
//
// We store g0 instead of m, to make the assembly code simpler,
// since we need to restore g0 in runtime.cgocallback.
//
// On systems without pthreads, like Windows, bindm shouldn't be used.
//
// NOTE: this always runs without a P, so, nowritebarrierrec required.
//
//go:nosplit
//go:nowritebarrierrec
internal static void cgoBindM() {
    if (GOOS == "windows"u8 || GOOS == "plan9"u8) {
        fatal(bindmInUnexpectedGoosˢ);
    }
    var g = getg();
    if ((~(~g).m).g0 != g) {
        fatal(theCurrentGIsNotG0ˢ);
    }
    if (_cgo_bindm != nil) {
        asmcgocall(_cgo_bindm, @unsafe.Pointer.FromPinnedBox(g));
    }
}

// A helper function for EnsureDropM.
//
// getm should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - fortio.org/log
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname getm
internal static uintptr getm() {
    return (uintptr)(~getg()).m;
}

internal static ж<atomic.Uintptr> ᏑextraM = new StandardBox<atomic.Uintptr>(default(atomic.Uintptr));
internal static ref atomic.Uintptr extraM => ref ᏑextraM.Value;
internal static ж<atomic.Uint32> ᏑextraMLength = new StandardBox<atomic.Uint32>(default(atomic.Uint32));
internal static ref atomic.Uint32 extraMLength => ref ᏑextraMLength.Value;
internal static ж<atomic.Uint32> ᏑextraMWaiters = new StandardBox<atomic.Uint32>(default(atomic.Uint32));
internal static ref atomic.Uint32 extraMWaiters => ref ᏑextraMWaiters.Value;
public static ж<atomic.Uint32> ᏑextraMInUse = new StandardBox<atomic.Uint32>(default(atomic.Uint32));
public static ref atomic.Uint32 extraMInUse => ref ᏑextraMInUse.Value;

// lockextra locks the extra list and returns the list head.
// The caller must unlock the list by storing a new list head
// to extram. If nilokay is true, then lockextra will
// return a nil list head if that's what it finds. If nilokay is false,
// lockextra will keep waiting until the list head is no longer nil.
//
//go:nosplit
internal static ж<m> lockextra(bool nilokay) {
    uintptr locked = 1;
    var incr = false;
    while (ᐧ) {
        var old = ᏑextraM.Load();
        if (old == locked) {
            osyield_no_g();
            continue;
        }
        if (old == 0 && !nilokay) {
            if (!incr) {
                // Add 1 to the number of threads
                // waiting for an M.
                // This is cleared by newextram.
                ᏑextraMWaiters.Add(1);
                incr = true;
            }
            usleep_no_g(1);
            continue;
        }
        if (ᏑextraM.CompareAndSwap(old, locked)) {
            return (ж<m>)(uintptr)((@unsafe.Pointer)old);
        }
        osyield_no_g();
        continue;
    }
}

//go:nosplit
internal static void unlockextra(ж<m> Ꮡmp, int32 delta) {
    ᏑextraMLength.Add(delta);
    ᏑextraM.Store((uintptr)Ꮡmp);
}

// Return an M from the extra M list. Returns last == true if the list becomes
// empty because of this call.
//
// Spins waiting for an extra M, so caller must ensure that the list always
// contains or will soon contain at least one M.
//
//go:nosplit
internal static (ж<m> mp, bool last) getExtraM() {
    ж<m> mp = default!;

    mp = lockextra(false);
    ᏑextraMInUse.Add(1);
    unlockextra((~mp).schedlink.ptr(), -1);
    return (mp, (~mp).schedlink.ptr() == nil);
}

// Returns an extra M back to the list. mp must be from getExtraM. Newly
// allocated M's should use addExtraM.
//
//go:nosplit
internal static void putExtraM(ж<m> Ꮡmp) {
    ᏑextraMInUse.Add(-1);
    addExtraM(Ꮡmp);
}

// Adds a newly allocated M to the extra M list.
//
//go:nosplit
internal static void addExtraM(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    var mnext = lockextra(true);
    mp.schedlink.set(mnext);
    unlockextra(Ꮡmp, 1);
}

internal static ж<rwmutex> ᏑallocmLock = new StandardBox<rwmutex>(new rwmutex());
internal static ref rwmutex allocmLock => ref ᏑallocmLock.Value;
internal static ж<rwmutex> ᏑexecLock = new StandardBox<rwmutex>(new rwmutex());
internal static ref rwmutex execLock => ref ᏑexecLock.Value;

// These errors are reported (via writeErrStr) by some OS-specific
// versions of newosproc and newosproc0.
internal static readonly @string failthreadcreate = "runtime: failed to create new OS thread\n"u8;

internal static readonly @string failallocatestack = "runtime: failed to allocate stack for the new OS thread\n"u8;

// newmHandoff contains a list of m structures that need new OS threads.
// This is used by newm in situations where newm itself can't safely
// start an OS thread.

[GoType("dyn")] partial struct newmHandoffᴛ1 {
    internal mutex @lock;
    // newm points to a list of M structures that need new OS
    // threads. The list is linked through m.schedlink.
    internal muintptr newm;
    // waiting indicates that wake needs to be notified when an m
    // is put on the list.
    internal bool waiting;
    internal note wake;
    // haveTemplateThread indicates that the templateThread has
    // been started. This is not protected by lock. Use cas to set
    // to 1.
    internal uint32 haveTemplateThread;
}
internal static ж<newmHandoffᴛ1> ᏑnewmHandoff = new StandardBox<newmHandoffᴛ1>(new newmHandoffᴛ1());
internal static ref newmHandoffᴛ1 newmHandoff => ref ᏑnewmHandoff.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string onALockedThreadWithNoˢ = "on a locked thread with no template thread"u8;

// Create a new m. It will start off with a call to fn, or else the scheduler.
// fn needs to be static and not a heap allocated closure.
// May run with m.p==nil, so write barriers are not allowed.
//
// id is optional pre-allocated m ID. Omit by passing -1.
//
//go:nowritebarrierrec
internal static void newm(Action fn, ж<Δp> Ꮡpp, int64 id) {
    // allocm adds a new M to allm, but they do not start until created by
    // the OS in newm1 or the template thread.
    //
    // doAllThreadsSyscall requires that every M in allm will eventually
    // start and be signal-able, even with a STW.
    //
    // Disable preemption here until we start the thread to ensure that
    // newm is not preempted between allocm and starting the new thread,
    // ensuring that anything added to allm is guaranteed to eventually
    // start.
    acquirem();
    var mp = allocm(Ꮡpp, fn, id);
    mp.of(m.Ꮡnextp).set(Ꮡpp);
    mp.Value.sigmask = initSigmask.Clone();
    {
        var gp = getg(); if (gp != nil && (~gp).m != nil && ((~(~gp).m).lockedExt != 0 || (~(~gp).m).incgo) && GOOS != "plan9"u8) {
            // We're on a locked M or a thread that may have been
            // started by C. The kernel state of this thread may
            // be strange (the user may have locked it for that
            // purpose). We don't want to clone that into another
            // thread. Instead, ask a known-good thread to create
            // the thread for us.
            //
            // This is disabled on Plan 9. See golang.org/issue/22227.
            //
            // TODO: This may be unnecessary on Windows, which
            // doesn't model thread creation off fork.
            @lock(ᏑnewmHandoff.of(newmHandoffᴛ1.Ꮡlock));
            if (newmHandoff.haveTemplateThread == 0) {
                @throw(onALockedThreadWithNoˢ);
            }
            mp.Value.schedlink = newmHandoff.newm;
            ᏑnewmHandoff.of(newmHandoffᴛ1.Ꮡnewm).set(mp);
            if (newmHandoff.waiting) {
                newmHandoff.waiting = false;
                notewakeup(ᏑnewmHandoff.of(newmHandoffᴛ1.Ꮡwake));
            }
            unlock(ᏑnewmHandoff.of(newmHandoffᴛ1.Ꮡlock));
            // The M has not started yet, but the template thread does not
            // participate in STW, so it will always process queued Ms and
            // it is safe to releasem.
            releasem(ref ((~getg()).m).DerefOrNull());
            return;
        }
    }
    newm1(mp);
    releasem(ref ((~getg()).m).DerefOrNull());
}

internal static void newm1(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    if (iscgo) {
        ref var ts = ref heap(new cgothreadstart(), out var Ꮡts);
        if (_cgo_thread_start == nil) {
            @throw(cgoThreadStartMissingˢ);
        }
        ts.g.set(mp.g0);
        ts.tls = Ꮡmp.at(m.Ꮡtls, 0).Reinterpret<uintptr, uint64>();
        ts.fn = (@unsafe.Pointer)abi.FuncPCABI0(mstart);
        if (msanenabled) {
            msanwrite(@unsafe.Pointer.FromPinnedBox(Ꮡts), /* unsafe.Sizeof(ts) */ (uintptr)24);
        }
        if (asanenabled) {
            asanwrite(@unsafe.Pointer.FromPinnedBox(Ꮡts), /* unsafe.Sizeof(ts) */ (uintptr)24);
        }
        ᏑexecLock.rlock(); // Prevent process clone.
        asmcgocall(_cgo_thread_start, @unsafe.Pointer.FromPinnedBox(Ꮡts));
        ᏑexecLock.runlock();
        return;
    }
    ᏑexecLock.rlock(); // Prevent process clone.
    newosproc(Ꮡmp);
    ᏑexecLock.runlock();
}

// startTemplateThread starts the template thread if it is not already
// running.
//
// The calling thread must itself be in a known-good state.
internal static void startTemplateThread() {
    if (GOARCH == "wasm"u8) {
        // no threads on wasm yet
        return;
    }
    // Disable preemption to guarantee that the template thread will be
    // created before a park once haveTemplateThread is set.
    var mp = acquirem();
    if (!atomic.Cas(ᏑnewmHandoff.of(newmHandoffᴛ1.ᏑhaveTemplateThread), 0, 1)) {
        releasem(ref (mp).DerefOrNull());
        return;
    }
    newm(templateThread, nil, -1);
    releasem(ref (mp).DerefOrNull());
}

// templateThread is a thread in a known-good state that exists solely
// to start new threads in known-good states when the calling thread
// may not be in a good state.
//
// Many programs never need this, so templateThread is started lazily
// when we first enter a state that might lead to running on a thread
// in an unknown state.
//
// templateThread runs on an M without a P, so it must not have write
// barriers.
//
//go:nowritebarrierrec
internal static void templateThread() {
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    sched.nmsys++;
    checkdead();
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    while (ᐧ) {
        @lock(ᏑnewmHandoff.of(newmHandoffᴛ1.Ꮡlock));
        while (newmHandoff.newm != 0) {
            var newm = newmHandoff.newm.ptr();
            newmHandoff.newm = 0;
            unlock(ᏑnewmHandoff.of(newmHandoffᴛ1.Ꮡlock));
            while (newm != nil) {
                var next = (~newm).schedlink.ptr();
                newm.Value.schedlink = 0;
                newm1(newm);
                newm = next;
            }
            @lock(ᏑnewmHandoff.of(newmHandoffᴛ1.Ꮡlock));
        }
        newmHandoff.waiting = true;
        noteclear(ᏑnewmHandoff.of(newmHandoffᴛ1.Ꮡwake));
        unlock(ᏑnewmHandoff.of(newmHandoffᴛ1.Ꮡlock));
        notesleep(ᏑnewmHandoff.of(newmHandoffᴛ1.Ꮡwake));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stopmHoldingLocksˢ = "stopm holding locks"u8;
internal static readonly @string stopmHoldingPˢ = "stopm holding p"u8;
internal static readonly @string stopmSpinningˢ = "stopm spinning"u8;

// Stops execution of the current m until new work is available.
// Returns with acquired P.
internal static void stopm() {
    var gp = getg();
    if ((~(~gp).m).locks != 0) {
        @throw(stopmHoldingLocksˢ);
    }
    if ((~(~gp).m).p != 0) {
        @throw(stopmHoldingPˢ);
    }
    if ((~(~gp).m).spinning) {
        @throw(stopmSpinningˢ);
    }
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    mput((~gp).m);
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    mPark();
    acquirep((~(~gp).m).nextp.ptr());
    gp.Value.m.Value.nextp = 0;
}

internal static void mspinning() {
    // startm's caller incremented nmspinning. Set the new M's spinning.
    getg().Value.m.Value.spinning = true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string startmPRequiredForˢ = "startm: P required for spinning=true"u8;
internal static readonly @string startmMIsSpinningˢ = "startm: m is spinning"u8;
internal static readonly @string startmMHasPˢ = "startm: m has p"u8;
internal static readonly @string startmPHasRunnableGsˢ = "startm: p has runnable gs"u8;

// Schedules some M to run the p (creates an M if necessary).
// If p==nil, tries to get an idle P, if no idle P's does nothing.
// May run with m.p==nil, so write barriers are not allowed.
// If spinning is set, the caller has incremented nmspinning and must provide a
// P. startm will set m.spinning in the newly started M.
//
// Callers passing a non-nil P must call from a non-preemptible context. See
// comment on acquirem below.
//
// Argument lockheld indicates whether the caller already acquired the
// scheduler lock. Callers holding the lock when making the call must pass
// true. The lock might be temporarily dropped, but will be reacquired before
// returning.
//
// Must not have write barriers because this may be called without a P.
//
//go:nowritebarrierrec
internal static void startm(ж<Δp> Ꮡpp, bool spinning, bool lockheld) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    // Disable preemption.
    //
    // Every owned P must have an owner that will eventually stop it in the
    // event of a GC stop request. startm takes transient ownership of a P
    // (either from argument or pidleget below) and transfers ownership to
    // a started M, which will be responsible for performing the stop.
    //
    // Preemption must be disabled during this transient ownership,
    // otherwise the P this is running on may enter GC stop while still
    // holding the transient P, leaving that P in limbo and deadlocking the
    // STW.
    //
    // Callers passing a non-nil P must already be in non-preemptible
    // context, otherwise such preemption could occur on function entry to
    // startm. Callers passing a nil P may be preemptible, so we must
    // disable preemption before acquiring a P from pidleget below.
    var mp = acquirem();
    if (!lockheld) {
        @lock(Ꮡsched.of(schedt.Ꮡlock));
    }
    if (Ꮡpp == nil) {
        if (spinning) {
            // TODO(prattmic): All remaining calls to this function
            // with _p_ == nil could be cleaned up to find a P
            // before calling startm.
            @throw(startmPRequiredForˢ);
        }
        (Ꮡpp, _) = pidleget(0); pp = ref Ꮡpp.DerefOrNull();
        if (Ꮡpp == nil) {
            if (!lockheld) {
                unlock(Ꮡsched.of(schedt.Ꮡlock));
            }
            releasem(ref (mp).DerefOrNull());
            return;
        }
    }
    var nmp = mget();
    if (nmp == nil) {
        // No M is available, we must drop sched.lock and call newm.
        // However, we already own a P to assign to the M.
        //
        // Once sched.lock is released, another G (e.g., in a syscall),
        // could find no idle P while checkdead finds a runnable G but
        // no running M's because this new M hasn't started yet, thus
        // throwing in an apparent deadlock.
        // This apparent deadlock is possible when startm is called
        // from sysmon, which doesn't count as a running M.
        //
        // Avoid this situation by pre-allocating the ID for the new M,
        // thus marking it as 'running' before we drop sched.lock. This
        // new M will eventually run the scheduler to execute any
        // queued G's.
        var id = mReserveID();
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        Action fn = default!;
        if (spinning) {
            // The caller incremented nmspinning, so set m.spinning in the new M.
            fn = mspinning;
        }
        newm(fn, Ꮡpp, id);
        if (lockheld) {
            @lock(Ꮡsched.of(schedt.Ꮡlock));
        }
        // Ownership transfer of pp committed by start in newm.
        // Preemption is now safe.
        releasem(ref (mp).DerefOrNull());
        return;
    }
    if (!lockheld) {
        unlock(Ꮡsched.of(schedt.Ꮡlock));
    }
    if ((~nmp).spinning) {
        @throw(startmMIsSpinningˢ);
    }
    if ((~nmp).nextp != 0) {
        @throw(startmMHasPˢ);
    }
    if (spinning && !runqempty(Ꮡpp)) {
        @throw(startmPHasRunnableGsˢ);
    }
    // The caller incremented nmspinning, so set m.spinning in the new M.
    nmp.Value.spinning = spinning;
    nmp.of(m.Ꮡnextp).set(Ꮡpp);
    notewakeup(nmp.of(m.Ꮡpark));
    // Ownership transfer of pp committed by wakeup. Preemption is now
    // safe.
    releasem(ref (mp).DerefOrNull());
}

// Hands off P from syscall or locked M.
// Always runs without a P, so write barriers are not allowed.
//
//go:nowritebarrierrec
internal static void handoffp(ж<Δp> Ꮡpp) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    // handoffp must start an M in any situation where
    // findrunnable would return a G to run on pp.
    // if it has local work, start it straight away
    if (!runqempty(Ꮡpp) || sched.runqsize != 0) {
        startm(Ꮡpp, false, false);
        return;
    }
    // if there's trace work to do, start it straight away
    if ((traceEnabled() || traceShuttingDown()) && traceReaderAvailable() != nil) {
        startm(Ꮡpp, false, false);
        return;
    }
    // if it has GC work, start it straight away
    if (gcBlackenEnabled != 0 && gcMarkWorkAvailable(Ꮡpp)) {
        startm(Ꮡpp, false, false);
        return;
    }
    // no local work, check that there are no spinning/idle M's,
    // otherwise our help is not required
    if (Ꮡsched.of(schedt.Ꮡnmspinning).Load() + Ꮡsched.of(schedt.Ꮡnpidle).Load() == 0 && Ꮡsched.of(schedt.Ꮡnmspinning).CompareAndSwap(0, 1)) {
        // TODO: fast atomic
        Ꮡsched.of(schedt.Ꮡneedspinning).Store(0);
        startm(Ꮡpp, true, false);
        return;
    }
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    if (Ꮡsched.of(schedt.Ꮡgcwaiting).Load()) {
        pp.status = _Pgcstop;
        pp.gcStopTime = nanotime();
        sched.stopwait--;
        if (sched.stopwait == 0) {
            notewakeup(Ꮡsched.of(schedt.Ꮡstopnote));
        }
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        return;
    }
    if (pp.runSafePointFn != 0 && atomic.Cas(Ꮡpp.of(runtime_package.Δp.ᏑrunSafePointFn), 1, 0)) {
        sched.safePointFn(Ꮡpp);
        sched.safePointWait--;
        if (sched.safePointWait == 0) {
            notewakeup(Ꮡsched.of(schedt.ᏑsafePointNote));
        }
    }
    if (sched.runqsize != 0) {
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        startm(Ꮡpp, false, false);
        return;
    }
    // If this is the last running P and nobody is polling network,
    // need to wakeup another M to poll network.
    if (Ꮡsched.of(schedt.Ꮡnpidle).Load() == gomaxprocs - 1 && Ꮡsched.of(schedt.Ꮡlastpoll).Load() != 0) {
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        startm(Ꮡpp, false, false);
        return;
    }
    // The scheduler lock cannot be held when calling wakeNetPoller below
    // because wakeNetPoller may call wakep which may call startm.
    var when = Ꮡpp.of(runtime_package.Δp.Ꮡtimers).wakeTime();
    pidleput(Ꮡpp, 0);
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    if (when != 0) {
        wakeNetPoller(when);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wakepNegativeNmspinningˢ = "wakep: negative nmspinning"u8;

// Tries to add one more P to execute G's.
// Called when a G is made runnable (newproc, ready).
// Must be called with a P.
//
// wakep should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gvisor.dev/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname wakep
internal static void wakep() {
    // Be conservative about spinning threads, only start one if none exist
    // already.
    if (Ꮡsched.of(schedt.Ꮡnmspinning).Load() != 0 || !Ꮡsched.of(schedt.Ꮡnmspinning).CompareAndSwap(0, 1)) {
        return;
    }
    // Disable preemption until ownership of pp transfers to the next M in
    // startm. Otherwise preemption here would leave pp stuck waiting to
    // enter _Pgcstop.
    //
    // See preemption comment on acquirem in startm for more details.
    var mp = acquirem();
    ж<Δp> pp = default!;
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    (pp, _) = pidlegetSpinning(0);
    if (pp == nil) {
        if (Ꮡsched.of(schedt.Ꮡnmspinning).Add(-1) < 0) {
            @throw(wakepNegativeNmspinningˢ);
        }
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        releasem(ref (mp).DerefOrNull());
        return;
    }
    // Since we always have a P, the race in the "No M is available"
    // comment in startm doesn't apply during the small window between the
    // unlock here and lock in startm. A checkdead in between will always
    // see at least one running M (ours).
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    startm(pp, true, false);
    releasem(ref (mp).DerefOrNull());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stoplockedmInconsistentˢ = "stoplockedm: inconsistent locking"u8;
internal static readonly @string stoplockedmNotRunnableˢ = "stoplockedm: not runnable"u8;

// Stops execution of the current m that is locked to a g until the g is runnable again.
// Returns with acquired P.
internal static void stoplockedm() {
    var gp = getg();
    if ((~(~gp).m).lockedg == 0 || (~(~(~gp).m).lockedg.ptr()).lockedm.ptr() != (~gp).m) {
        @throw(stoplockedmInconsistentˢ);
    }
    if ((~(~gp).m).p != 0) {
        // Schedule another M to run this p.
        var pp = releasep();
        handoffp(pp);
    }
    incidlelocked(1);
    // Wait until another thread schedules lockedg again.
    mPark();
    var status = readgstatus((~(~gp).m).lockedg.ptr());
    if ((uint32)(status & ~(uint32)_Gscan) != _Grunnable) {
        print((@string)"runtime:stoplockedm: lockedg (atomicstatus="u8, status, (@string)") is not Grunnable or Gscanrunnable\n"u8);
        dumpgstatus((~(~gp).m).lockedg.ptr());
        @throw(stoplockedmNotRunnableˢ);
    }
    acquirep((~(~gp).m).nextp.ptr());
    gp.Value.m.Value.nextp = 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string startlockedmLockedToMeˢ = "startlockedm: locked to me"u8;
internal static readonly @string startlockedmMHasPˢ = "startlockedm: m has p"u8;

// Schedules the locked m to run the locked gp.
// May run during STW, so write barriers are not allowed.
//
//go:nowritebarrierrec
internal static void startlockedm(ref g gp) {
    var mp = gp.lockedm.ptr();
    if (mp == (~getg()).m) {
        @throw(startlockedmLockedToMeˢ);
    }
    if ((~mp).nextp != 0) {
        @throw(startlockedmMHasPˢ);
    }
    // directly handoff current P to the locked m
    incidlelocked(-1);
    var pp = releasep();
    mp.of(m.Ꮡnextp).set(pp);
    notewakeup(mp.of(m.Ꮡpark));
    stopm();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcstopmNotWaitingForGcˢ = "gcstopm: not waiting for gc"u8;
internal static readonly @string gcstopmNegativeˢ = "gcstopm: negative nmspinning"u8;

// Stops the current m for stopTheWorld.
// Returns when the world is restarted.
internal static void gcstopm() {
    var gp = getg();
    if (!Ꮡsched.of(schedt.Ꮡgcwaiting).Load()) {
        @throw(gcstopmNotWaitingForGcˢ);
    }
    if ((~(~gp).m).spinning) {
        gp.Value.m.Value.spinning = false;
        // OK to just drop nmspinning here,
        // startTheWorld will unpark threads as necessary.
        if (Ꮡsched.of(schedt.Ꮡnmspinning).Add(-1) < 0) {
            @throw(gcstopmNegativeˢ);
        }
    }
    var pp = releasep();
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    pp.Value.status = _Pgcstop;
    pp.Value.gcStopTime = nanotime();
    sched.stopwait--;
    if (sched.stopwait == 0) {
        notewakeup(Ꮡsched.of(schedt.Ꮡstopnote));
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    stopm();
}

// Schedules gp to run on the current M.
// If inheritTime is true, gp inherits the remaining time in the
// current time slice. Otherwise, it starts a new time slice.
// Never returns.
//
// Write barriers are allowed because this is called immediately after
// acquiring a P in several places.
//
//go:yeswritebarrierrec
internal static void execute(ж<g> Ꮡgp, bool inheritTime) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    var mp = getg().Value.m;
    if (goroutineProfile.active) {
        // Make sure that gp has had its stack written out to the goroutine
        // profile, exactly as it was when the goroutine profiler first stopped
        // the world.
        tryRecordGoroutineProfile(Ꮡgp, default!, osyield);
    }
    // Assign gp.m before entering _Grunning so running Gs have an
    // M.
    mp.Value.curg = Ꮡgp;
    gp.m = mp;
    casgstatus(Ꮡgp, _Grunnable, _Grunning);
    gp.waitsince = 0;
    gp.preempt = false;
    gp.stackguard0 = gp.stack.lo + (uintptr)stackGuard;
    if (!inheritTime) {
        (~mp).p.ptr().Value.schedtick++;
    }
    // Check whether the profiler needs to be turned on or off.
    var hz = sched.profilehz;
    if ((~mp).profilehz != hz) {
        setThreadCPUProfiler(hz);
    }
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GoStart();
        traceRelease(Δtrace);
    }
    gogo(Ꮡgp.of(g.Ꮡsched));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string findrunnableWrongPˢ = "findrunnable: wrong p"u8;
internal static readonly @string findrunnableNegativeˢ = "findrunnable: negative nmspinning"u8;
internal static readonly @string globalRunqEmptyWithNonˢ = "global runq empty with non-zero runqsize"u8;
internal static readonly @string findrunnableNetpollWithPˢ = "findrunnable: netpoll with p"u8;
internal static readonly @string findrunnableNetpollWithˢ = "findrunnable: netpoll with spinning"u8;

// Finds a runnable goroutine to execute.
// Tries to steal from other P's, get g from local or global queue, poll network.
// tryWakeP indicates that the returned goroutine is not normal (GC worker, trace
// reader) so the caller should try to wake a P.
internal static (ж<g> gp, bool inheritTime, bool tryWakeP) findRunnable() {
    ж<g> gp = default!;

    var mp = getg().Value.m;
    // The conditions here and in handoffp must agree: if
    // findrunnable would return a G to run, handoffp must start
    // an M.
top:
    mp.clearAllpSnapshot();
    // We may have collected an allp snapshot below. The snapshot is only
    // required in each loop iteration. Clear it to all GC to collect the
    // slice.
    var pp = (~mp).p.ptr();
    if (Ꮡsched.of(schedt.Ꮡgcwaiting).Load()) {
        gcstopm();
        goto top;
    }
    if ((~pp).runSafePointFn != 0) {
        runSafePointFn();
    }
    // now and pollUntil are saved for work stealing later,
    // which may steal timers. It's important that between now
    // and then, nothing blocks, so these numbers remain mostly
    // relevant.
    var (now, pollUntil, _) = pp.of(runtime_package.Δp.Ꮡtimers).check(0);
    // Try to schedule the trace reader.
    if (traceEnabled() || traceShuttingDown()) {
        var gpΔ1 = traceReader();
        if (gpΔ1 != nil) {
            var Δtrace = traceAcquire();
            casgstatus(gpΔ1, _Gwaiting, _Grunnable);
            if (Δtrace.ok()) {
                Δtrace.GoUnpark(gpΔ1, 0);
                traceRelease(Δtrace);
            }
            return (gpΔ1, false, true);
        }
    }
    // Try to schedule a GC worker.
    if (gcBlackenEnabled != 0) {
        var (gpΔ2, tnow) = ᏑgcController.findRunnableGCWorker(pp, now);
        if (gpΔ2 != nil) {
            return (gpΔ2, false, true);
        }
        now = tnow;
    }
    // Check the global runnable queue once in a while to ensure fairness.
    // Otherwise two goroutines can completely occupy the local runqueue
    // by constantly respawning each other.
    if ((~pp).schedtick % 61 == 0 && sched.runqsize > 0) {
        @lock(Ꮡsched.of(schedt.Ꮡlock));
        var gpΔ3 = globrunqget(pp, 1);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        if (gpΔ3 != nil) {
            return (gpΔ3, false, false);
        }
    }
    // Wake up the finalizer G.
    if ((uint32)(ᏑfingStatus.Load() & ((uint32)(fingWait | fingWake))) == (uint32)(fingWait | fingWake)) {
        {
            var gpΔ4 = wakefing(); if (gpΔ4 != nil) {
                ready(gpΔ4, 0, true);
            }
        }
    }
    if (cgo_yield.Value != nil) {
        asmcgocall(cgo_yield.Value, nil);
    }
    // local runq
    {
        var (gpΔ5, inheritTimeΔ1) = runqget(pp); if (gpΔ5 != nil) {
            return (gpΔ5, inheritTimeΔ1, false);
        }
    }
    // global runq
    if (sched.runqsize != 0) {
        @lock(Ꮡsched.of(schedt.Ꮡlock));
        var gpΔ6 = globrunqget(pp, 0);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        if (gpΔ6 != nil) {
            return (gpΔ6, false, false);
        }
    }
    // Poll network.
    // This netpoll is only an optimization before we resort to stealing.
    // We can safely skip it if there are no waiters or a thread is blocked
    // in netpoll already. If there is any kind of logical race with that
    // blocked thread (e.g. it has already returned from netpoll, but does
    // not set lastpoll yet), this thread will do blocking netpoll below
    // anyway.
    if (netpollinited() && netpollAnyWaiters() && Ꮡsched.of(schedt.Ꮡlastpoll).Load() != 0) {
        {
            ref var list = ref heap<gList>(out var Ꮡlist);
            (list, var delta) = netpoll(0); if (!list.empty()) {
                // non-blocking
                var gpΔ7 = list.pop();
                injectglist(Ꮡlist);
                netpollAdjustWaiters(delta);
                var Δtrace = traceAcquire();
                casgstatus(gpΔ7, _Gwaiting, _Grunnable);
                if (Δtrace.ok()) {
                    Δtrace.GoUnpark(gpΔ7, 0);
                    traceRelease(Δtrace);
                }
                return (gpΔ7, false, false);
            }
        }
    }
    // Spinning Ms: steal work from other Ps.
    //
    // Limit the number of spinning Ms to half the number of busy Ps.
    // This is necessary to prevent excessive CPU consumption when
    // GOMAXPROCS>>1 but the program parallelism is low.
    if ((~mp).spinning || 2 * Ꮡsched.of(schedt.Ꮡnmspinning).Load() < gomaxprocs - Ꮡsched.of(schedt.Ꮡnpidle).Load()) {
        if (!(~mp).spinning) {
            mp.becomeSpinning();
        }
        var (gpΔ8, inheritTimeΔ2, tnow, w, newWork) = stealWork(now);
        if (gpΔ8 != nil) {
            // Successfully stole.
            return (gpΔ8, inheritTimeΔ2, false);
        }
        if (newWork) {
            // There may be new timer or GC work; restart to
            // discover.
            goto top;
        }
        now = tnow;
        if (w != 0 && (pollUntil == 0 || w < pollUntil)) {
            // Earlier timer to wait for.
            pollUntil = w;
        }
    }
    // We have nothing to do.
    //
    // If we're in the GC mark phase, can safely scan and blacken objects,
    // and have work to do, run idle-time marking rather than give up the P.
    if (gcBlackenEnabled != 0 && gcMarkWorkAvailable(pp) && ᏑgcController.addIdleMarkWorker()) {
        var node = (ж<gcBgMarkWorkerNode>)(uintptr)(ᏑgcBgMarkWorkerPool.pop());
        if (node != nil) {
            pp.Value.gcMarkWorkerMode = gcMarkWorkerIdleMode;
            var gpΔ9 = (~node).gp.ptr();
            var Δtrace = traceAcquire();
            casgstatus(gpΔ9, _Gwaiting, _Grunnable);
            if (Δtrace.ok()) {
                Δtrace.GoUnpark(gpΔ9, 0);
                traceRelease(Δtrace);
            }
            return (gpΔ9, false, false);
        }
        ᏑgcController.removeIdleMarkWorker();
    }
    // wasm only:
    // If a callback returned and no other goroutine is awake,
    // then wake event handler goroutine which pauses execution
    // until a callback was triggered.
    (gp, var otherReady) = beforeIdle(now, pollUntil);
    if (gp != nil) {
        var Δtrace = traceAcquire();
        casgstatus(gp, _Gwaiting, _Grunnable);
        if (Δtrace.ok()) {
            Δtrace.GoUnpark(gp, 0);
            traceRelease(Δtrace);
        }
        return (gp, false, false);
    }
    if (otherReady) {
        goto top;
    }
    // Before we drop our P, make a snapshot of the allp slice,
    // which can change underfoot once we no longer block
    // safe-points. We don't need to snapshot the contents because
    // everything up to cap(allp) is immutable.
    //
    // We clear the snapshot from the M after return via
    // mp.clearAllpSnapshop (in schedule) and on each iteration of the top
    // loop.
    var allpSnapshot = mp.snapshotAllp();
    // Also snapshot masks. Value changes are OK, but we can't allow
    // len to change out from under us.
    var idlepMaskSnapshot = idlepMask;
    var timerpMaskSnapshot = timerpMask;
    // return P and block
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    if (Ꮡsched.of(schedt.Ꮡgcwaiting).Load() || (~pp).runSafePointFn != 0) {
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        goto top;
    }
    if (sched.runqsize != 0) {
        var gpΔ10 = globrunqget(pp, 0);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        return (gpΔ10, false, false);
    }
    if (!(~mp).spinning && Ꮡsched.of(schedt.Ꮡneedspinning).Load() == 1) {
        // See "Delicate dance" comment below.
        mp.becomeSpinning();
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        goto top;
    }
    if (releasep() != pp) {
        @throw(findrunnableWrongPˢ);
    }
    now = pidleput(pp, now);
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    // Delicate dance: thread transitions from spinning to non-spinning
    // state, potentially concurrently with submission of new work. We must
    // drop nmspinning first and then check all sources again (with
    // #StoreLoad memory barrier in between). If we do it the other way
    // around, another thread can submit work after we've checked all
    // sources but before we drop nmspinning; as a result nobody will
    // unpark a thread to run the work.
    //
    // This applies to the following sources of work:
    //
    // * Goroutines added to the global or a per-P run queue.
    // * New/modified-earlier timers on a per-P timer heap.
    // * Idle-priority GC work (barring golang.org/issue/19112).
    //
    // If we discover new work below, we need to restore m.spinning as a
    // signal for resetspinning to unpark a new worker thread (because
    // there can be more than one starving goroutine).
    //
    // However, if after discovering new work we also observe no idle Ps
    // (either here or in resetspinning), we have a problem. We may be
    // racing with a non-spinning M in the block above, having found no
    // work and preparing to release its P and park. Allowing that P to go
    // idle will result in loss of work conservation (idle P while there is
    // runnable work). This could result in complete deadlock in the
    // unlikely event that we discover new work (from netpoll) right as we
    // are racing with _all_ other Ps going idle.
    //
    // We use sched.needspinning to synchronize with non-spinning Ms going
    // idle. If needspinning is set when they are about to drop their P,
    // they abort the drop and instead become a new spinning M on our
    // behalf. If we are not racing and the system is truly fully loaded
    // then no spinning threads are required, and the next thread to
    // naturally become spinning will clear the flag.
    //
    // Also see "Worker thread parking/unparking" comment at the top of the
    // file.
    var wasSpinning = mp.Value.spinning;
    if ((~mp).spinning) {
        mp.Value.spinning = false;
        if (Ꮡsched.of(schedt.Ꮡnmspinning).Add(-1) < 0) {
            @throw(findrunnableNegativeˢ);
        }
        // Note the for correctness, only the last M transitioning from
        // spinning to non-spinning must perform these rechecks to
        // ensure no missed work. However, the runtime has some cases
        // of transient increments of nmspinning that are decremented
        // without going through this path, so we must be conservative
        // and perform the check on all spinning Ms.
        //
        // See https://go.dev/issue/43997.
        // Check global and P runqueues again.
        @lock(Ꮡsched.of(schedt.Ꮡlock));
        if (sched.runqsize != 0) {
            var (ppΔ1, _) = pidlegetSpinning(0);
            if (ppΔ1 != nil) {
                var gpΔ11 = globrunqget(ppΔ1, 0);
                if (gpΔ11 == nil) {
                    @throw(globalRunqEmptyWithNonˢ);
                }
                unlock(Ꮡsched.of(schedt.Ꮡlock));
                acquirep(ppΔ1);
                mp.becomeSpinning();
                return (gpΔ11, false, false);
            }
        }
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        var ppΔ2 = checkRunqsNoP(allpSnapshot, idlepMaskSnapshot);
        if (ppΔ2 != nil) {
            acquirep(ppΔ2);
            mp.becomeSpinning();
            goto top;
        }
        // Check for idle-priority GC work again.
        (ppΔ2, var gpΔ12) = checkIdleGCNoP();
        if (ppΔ2 != nil) {
            acquirep(ppΔ2);
            mp.becomeSpinning();
            // Run the idle worker.
            ppΔ2.Value.gcMarkWorkerMode = gcMarkWorkerIdleMode;
            var Δtrace = traceAcquire();
            casgstatus(gpΔ12, _Gwaiting, _Grunnable);
            if (Δtrace.ok()) {
                Δtrace.GoUnpark(gpΔ12, 0);
                traceRelease(Δtrace);
            }
            return (gpΔ12, false, false);
        }
        // Finally, check for timer creation or expiry concurrently with
        // transitioning from spinning to non-spinning.
        //
        // Note that we cannot use checkTimers here because it calls
        // adjusttimers which may need to allocate memory, and that isn't
        // allowed when we don't have an active P.
        pollUntil = checkTimersNoP(allpSnapshot, timerpMaskSnapshot, pollUntil);
    }
    // We don't need allp anymore at this pointer, but can't clear the
    // snapshot without a P for the write barrier..
    // Poll network until next timer.
    if (netpollinited() && (netpollAnyWaiters() || pollUntil != 0) && Ꮡsched.of(schedt.Ꮡlastpoll).Swap(0) != 0){
        Ꮡsched.of(schedt.ᏑpollUntil).Store(pollUntil);
        if ((~mp).p != 0) {
            @throw(findrunnableNetpollWithPˢ);
        }
        if ((~mp).spinning) {
            @throw(findrunnableNetpollWithˢ);
        }
        var delay = (int64)(-1);
        if (pollUntil != 0) {
            if (now == 0) {
                now = nanotime();
            }
            delay = pollUntil - now;
            if (delay < 0) {
                delay = 0;
            }
        }
        if (faketime != 0) {
            // When using fake time, just poll.
            delay = 0;
        }
        ref var list = ref heap<gList>(out var Ꮡlist);
        (list, var delta) = netpoll(delay); // block until new work is available
        // Refresh now again, after potentially blocking.
        now = nanotime();
        Ꮡsched.of(schedt.ᏑpollUntil).Store(0);
        Ꮡsched.of(schedt.Ꮡlastpoll).Store(now);
        if (faketime != 0 && list.empty()) {
            // Using fake time and nothing is ready; stop M.
            // When all M's stop, checkdead will call timejump.
            stopm();
            goto top;
        }
        @lock(Ꮡsched.of(schedt.Ꮡlock));
        var (ppΔ3, _) = pidleget(now);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        if (ppΔ3 == nil){
            injectglist(Ꮡlist);
            netpollAdjustWaiters(delta);
        } else {
            acquirep(ppΔ3);
            if (!list.empty()) {
                var gpΔ13 = list.pop();
                injectglist(Ꮡlist);
                netpollAdjustWaiters(delta);
                var Δtrace = traceAcquire();
                casgstatus(gpΔ13, _Gwaiting, _Grunnable);
                if (Δtrace.ok()) {
                    Δtrace.GoUnpark(gpΔ13, 0);
                    traceRelease(Δtrace);
                }
                return (gpΔ13, false, false);
            }
            if (wasSpinning) {
                mp.becomeSpinning();
            }
            goto top;
        }
    } else 
    if (pollUntil != 0 && netpollinited()) {
        var pollerPollUntil = Ꮡsched.of(schedt.ᏑpollUntil).Load();
        if (pollerPollUntil == 0 || pollerPollUntil > pollUntil) {
            netpollBreak();
        }
    }
    stopm();
    goto top;
}

// pollWork reports whether there is non-background work this P could
// be doing. This is a fairly lightweight check to be used for
// background work loops, like idle GC. It checks a subset of the
// conditions checked by the actual scheduler.
internal static bool pollWork() {
    if (sched.runqsize != 0) {
        return true;
    }
    var Δp = (~(~getg()).m).p.ptr();
    if (!runqempty(Δp)) {
        return true;
    }
    if (netpollinited() && netpollAnyWaiters() && Ꮡsched.of(schedt.Ꮡlastpoll).Load() != 0) {
        {
            ref var list = ref heap<gList>(out var Ꮡlist);
            (list, var delta) = netpoll(0); if (!list.empty()) {
                injectglist(Ꮡlist);
                netpollAdjustWaiters(delta);
                return true;
            }
        }
    }
    return false;
}

// stealWork attempts to steal a runnable goroutine or timer from any P.
//
// If newWork is true, new work may have been readied.
//
// If now is not 0 it is the current time. stealWork returns the passed time or
// the current time if now was passed as 0.
internal static (ж<g> gp, bool inheritTime, int64 rnow, int64 pollUntil, bool newWork) stealWork(int64 now) {
    int64 pollUntil = default!;

    var pp = (~(~getg()).m).p.ptr();
    var ranTimer = false;
    UntypedInt stealTries = 4;
    for (nint i = 0; i < stealTries; i++) {
        var stealTimersOrRunNextG = i == (nint)(stealTries - 1);
        for (var @enum = stealOrder.start(cheaprand()); !@enum.done(); @enum.next()) {
            if (Ꮡsched.of(schedt.Ꮡgcwaiting).Load()) {
                // GC work may be available.
                return (default!, false, now, pollUntil, true);
            }
            var p2 = allp[(nint)(@enum.position())];
            if (pp == p2) {
                continue;
            }
            // Steal timers from p2. This call to checkTimers is the only place
            // where we might hold a lock on a different P's timers. We do this
            // once on the last pass before checking runnext because stealing
            // from the other P's runnext should be the last resort, so if there
            // are timers to steal do that first.
            //
            // We only check timers on one of the stealing iterations because
            // the time stored in now doesn't change in this loop and checking
            // the timers for each P more than once with the same value of now
            // is probably a waste of time.
            //
            // timerpMask tells us whether the P may have timers at all. If it
            // can't, no need to check at all.
            if (stealTimersOrRunNextG && timerpMask.read(@enum.position())) {
                var (tnow, w, ran) = p2.of(runtime_package.Δp.Ꮡtimers).check(now);
                now = tnow;
                if (w != 0 && (pollUntil == 0 || w < pollUntil)) {
                    pollUntil = w;
                }
                if (ran) {
                    // Running the timers may have
                    // made an arbitrary number of G's
                    // ready and added them to this P's
                    // local run queue. That invalidates
                    // the assumption of runqsteal
                    // that it always has room to add
                    // stolen G's. So check now if there
                    // is a local G to run.
                    {
                        var (gpΔ1, inheritTimeΔ1) = runqget(pp); if (gpΔ1 != nil) {
                            return (gpΔ1, inheritTimeΔ1, now, pollUntil, ranTimer);
                        }
                    }
                    ranTimer = true;
                }
            }
            // Don't bother to attempt to steal if p2 is idle.
            if (!idlepMask.read(@enum.position())) {
                {
                    var gpΔ2 = runqsteal(pp, p2, stealTimersOrRunNextG); if (gpΔ2 != nil) {
                        return (gpΔ2, false, now, pollUntil, ranTimer);
                    }
                }
            }
        }
    }
    // No goroutines found to steal. Regardless, running a timer may have
    // made some goroutine ready that we missed. Indicate the next timer to
    // wait for.
    return (default!, false, now, pollUntil, ranTimer);
}

// Check all Ps for a runnable G to steal.
//
// On entry we have no P. If a G is available to steal and a P is available,
// the P is returned which the caller should acquire and attempt to steal the
// work to.
internal static ж<Δp> checkRunqsNoP(slice<ж<Δp>> allpSnapshot, pMask idlepMaskSnapshot) {
    foreach (var (id, p2) in allpSnapshot) {
        if (!idlepMaskSnapshot.read((uint32)id) && !runqempty(p2)) {
            @lock(Ꮡsched.of(schedt.Ꮡlock));
            var (pp, _) = pidlegetSpinning(0);
            if (pp == nil) {
                // Can't get a P, don't bother checking remaining Ps.
                unlock(Ꮡsched.of(schedt.Ꮡlock));
                return default!;
            }
            unlock(Ꮡsched.of(schedt.Ꮡlock));
            return pp;
        }
    }
    // No work available.
    return default!;
}

// Check all Ps for a timer expiring sooner than pollUntil.
//
// Returns updated pollUntil value.
internal static int64 checkTimersNoP(slice<ж<Δp>> allpSnapshot, pMask timerpMaskSnapshot, int64 pollUntil) {
    foreach (var (id, p2) in allpSnapshot) {
        if (timerpMaskSnapshot.read((uint32)id)) {
            var w = p2.of(runtime_package.Δp.Ꮡtimers).wakeTime();
            if (w != 0 && (pollUntil == 0 || w < pollUntil)) {
                pollUntil = w;
            }
        }
    }
    return pollUntil;
}

// Check for idle-priority GC, without a P on entry.
//
// If some GC work, a P, and a worker G are all available, the P and G will be
// returned. The returned P has not been wired yet.
internal static (ж<Δp>, ж<g>) checkIdleGCNoP() {
    // N.B. Since we have no P, gcBlackenEnabled may change at any time; we
    // must check again after acquiring a P. As an optimization, we also check
    // if an idle mark worker is needed at all. This is OK here, because if we
    // observe that one isn't needed, at least one is currently running. Even if
    // it stops running, its own journey into the scheduler should schedule it
    // again, if need be (at which point, this check will pass, if relevant).
    if (atomic.Load(ᏑgcBlackenEnabled) == 0 || !ᏑgcController.needIdleMarkWorker()) {
        return (default!, default!);
    }
    if (!gcMarkWorkAvailable(nil)) {
        return (default!, default!);
    }
    // Work is available; we can start an idle GC worker only if there is
    // an available P and available worker G.
    //
    // We can attempt to acquire these in either order, though both have
    // synchronization concerns (see below). Workers are almost always
    // available (see comment in findRunnableGCWorker for the one case
    // there may be none). Since we're slightly less likely to find a P,
    // check for that first.
    //
    // Synchronization: note that we must hold sched.lock until we are
    // committed to keeping it. Otherwise we cannot put the unnecessary P
    // back in sched.pidle without performing the full set of idle
    // transition checks.
    //
    // If we were to check gcBgMarkWorkerPool first, we must somehow handle
    // the assumption in gcControllerState.findRunnableGCWorker that an
    // empty gcBgMarkWorkerPool is only possible if gcMarkDone is running.
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    var (pp, now) = pidlegetSpinning(0);
    if (pp == nil) {
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        return (default!, default!);
    }
    // Now that we own a P, gcBlackenEnabled can't change (as it requires STW).
    if (gcBlackenEnabled == 0 || !ᏑgcController.addIdleMarkWorker()) {
        pidleput(pp, now);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        return (default!, default!);
    }
    var node = (ж<gcBgMarkWorkerNode>)(uintptr)(ᏑgcBgMarkWorkerPool.pop());
    if (node == nil) {
        pidleput(pp, now);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        ᏑgcController.removeIdleMarkWorker();
        return (default!, default!);
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    return (pp, (~node).gp.ptr());
}

// wakeNetPoller wakes up the thread sleeping in the network poller if it isn't
// going to wake up before the when argument; or it wakes an idle P to service
// timers and the network poller if there isn't one already.
internal static void wakeNetPoller(int64 when) {
    if (Ꮡsched.of(schedt.Ꮡlastpoll).Load() == 0){
        // In findrunnable we ensure that when polling the pollUntil
        // field is either zero or the time to which the current
        // poll is expected to run. This can have a spurious wakeup
        // but should never miss a wakeup.
        var pollerPollUntil = Ꮡsched.of(schedt.ᏑpollUntil).Load();
        if (pollerPollUntil == 0 || pollerPollUntil > when) {
            netpollBreak();
        }
    } else {
        // There are no threads in the network poller, try to get
        // one there so it can handle new timers.
        if (GOOS != "plan9"u8) {
            // Temporary workaround - see issue #42303.
            wakep();
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string resetspinningNotAˢ = "resetspinning: not a spinning m"u8;

internal static void resetspinning() {
    var gp = getg();
    if (!(~(~gp).m).spinning) {
        @throw(resetspinningNotAˢ);
    }
    gp.Value.m.Value.spinning = false;
    var nmspinning = Ꮡsched.of(schedt.Ꮡnmspinning).Add(-1);
    if (nmspinning < 0) {
        @throw(findrunnableNegativeˢ);
    }
    // M wakeup policy is deliberately somewhat conservative, so check if we
    // need to wakeup another P here. See "Worker thread parking/unparking"
    // comment at the top of the file for details.
    wakep();
}

// injectglist adds each runnable G on the list to some run queue,
// and clears glist. If there is no current P, they are added to the
// global queue, and up to npidle M's are started to run them.
// Otherwise, for each idle P, this adds a G to the global queue
// and starts an M. Any remaining G's are added to the current P's
// local run queue.
// This may temporarily acquire sched.lock.
// Can run concurrently with GC.
internal static void injectglist(ж<gList> Ꮡglist) {
    ref var glist = ref Ꮡglist.DerefOrNull();

    if (glist.empty()) {
        return;
    }
    // Mark all the goroutines as runnable before we put them
    // on the run queues.
    var head = glist.head.ptr();
    ж<g> tail = default!;
    nint qsize = 0;
    var Δtrace = traceAcquire();
    for (var gp = head; gp != nil; gp = (~gp).schedlink.ptr()) {
        tail = gp;
        qsize++;
        casgstatus(gp, _Gwaiting, _Grunnable);
        if (Δtrace.ok()) {
            Δtrace.GoUnpark(gp, 0);
        }
    }
    if (Δtrace.ok()) {
        traceRelease(Δtrace);
    }
    // Turn the gList into a gQueue.
    ref var q = ref heap(new gQueue(), out var Ꮡq);
    q.head.set(head);
    q.tail.set(tail);
    glist = new gList(nil);
    void startIdle(nint nΔ1) {
        for (nint i = 0; i < nΔ1; i++) {
            var mp = acquirem(); // See comment in startm.
            @lock(Ꮡsched.of(schedt.Ꮡlock));
            var (ppΔ1, _) = pidlegetSpinning(0);
            if (ppΔ1 == nil) {
                unlock(Ꮡsched.of(schedt.Ꮡlock));
                releasem(ref (mp).DerefOrNull());
                break;
            }
            startm(ppΔ1, false, true);
            unlock(Ꮡsched.of(schedt.Ꮡlock));
            releasem(ref (mp).DerefOrNull());
        }
    }
    var pp = (~(~getg()).m).p.ptr();
    if (pp == nil) {
        @lock(Ꮡsched.of(schedt.Ꮡlock));
        globrunqputbatch(ref q, (int32)qsize);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        startIdle(qsize);
        return;
    }
    nint npidle = (nint)Ꮡsched.of(schedt.Ꮡnpidle).Load();
    gQueue globq = default!;
    nint n = default!;
    for (n = 0; n < npidle && !q.empty(); n++) {
        var g = q.pop();
        globq.pushBack(g);
    }
    if (n > 0) {
        @lock(Ꮡsched.of(schedt.Ꮡlock));
        globrunqputbatch(ref globq, (int32)n);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        startIdle(n);
        qsize -= n;
    }
    if (!q.empty()) {
        runqputbatch(pp, Ꮡq, qsize);
    }
    // Some P's might have become idle after we loaded `sched.npidle`
    // but before any goroutines were added to the queue, which could
    // lead to idle P's when there is work available in the global queue.
    // That could potentially last until other goroutines become ready
    // to run. That said, we need to find a way to hedge
    //
    // Calling wakep() here is the best bet, it will do nothing in the
    // common case (no racing on `sched.npidle`), while it could wake one
    // more P to execute G's, which might end up with >1 P's: the first one
    // wakes another P and so forth until there is no more work, but this
    // ought to be an extremely rare case.
    //
    // Also see "Worker thread parking/unparking" comment at the top of the file for details.
    wakep();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string scheduleHoldingLocksˢ = "schedule: holding locks"u8;
internal static readonly @string scheduleInCgoˢ = "schedule: in cgo"u8;
internal static readonly @string scheduleSpinningWithˢ = "schedule: spinning with local work"u8;

// One round of scheduler: find a runnable goroutine and execute it.
// Never returns.
internal static void schedule() {
    var mp = getg().Value.m;
    if ((~mp).locks != 0) {
        @throw(scheduleHoldingLocksˢ);
    }
    if ((~mp).lockedg != 0) {
        stoplockedm();
        execute((~mp).lockedg.ptr(), false); // Never returns.
    }
    // We should not schedule away from a g that is executing a cgo call,
    // since the cgo call is using the m's g0 stack.
    if ((~mp).incgo) {
        @throw(scheduleInCgoˢ);
    }
top:
    var pp = (~mp).p.ptr();
    pp.Value.preempt = false;
    // Safety check: if we are spinning, the run queue should be empty.
    // Check this before calling checkTimers, as that might call
    // goready to put a ready goroutine on the local run queue.
    if ((~mp).spinning && ((~pp).runnext != 0 || (~pp).runqhead != (~pp).runqtail)) {
        @throw(scheduleSpinningWithˢ);
    }
    var (gp, inheritTime, tryWakeP) = findRunnable(); // blocks until work is available
    // findRunnable may have collected an allp snapshot. The snapshot is
    // only required within findRunnable. Clear it to all GC to collect the
    // slice.
    mp.clearAllpSnapshot();
    if (debug.dontfreezetheworld > 0 && Ꮡfreezing.Load()) {
        // See comment in freezetheworld. We don't want to perturb
        // scheduler state, so we didn't gcstopm in findRunnable, but
        // also don't want to allow new goroutines to run.
        //
        // Deadlock here rather than in the findRunnable loop so if
        // findRunnable is stuck in a loop we don't perturb that
        // either.
        @lock(Ꮡdeadlock);
        @lock(Ꮡdeadlock);
    }
    // This thread is going to run a goroutine and is not spinning anymore,
    // so if it was marked as spinning we need to reset it now and potentially
    // start a new spinning M.
    if ((~mp).spinning) {
        resetspinning();
    }
    if (sched.disable.user && !schedEnabled(ref (gp).DerefOrNull())) {
        // Scheduling of this goroutine is disabled. Put it on
        // the list of pending runnable goroutines for when we
        // re-enable user scheduling and look again.
        @lock(Ꮡsched.of(schedt.Ꮡlock));
        if (schedEnabled(ref (gp).DerefOrNull())){
            // Something re-enabled scheduling while we
            // were acquiring the lock.
            unlock(Ꮡsched.of(schedt.Ꮡlock));
        } else {
            Ꮡsched.of(schedt.Ꮡdisable).of(schedt_disable.Ꮡrunnable).pushBack(gp);
            sched.disable.n++;
            unlock(Ꮡsched.of(schedt.Ꮡlock));
            goto top;
        }
    }
    // If about to schedule a not-normal goroutine (a GCworker or tracereader),
    // wake a P if there is one.
    if (tryWakeP) {
        wakep();
    }
    if ((~gp).lockedm != 0) {
        // Hands off own p to the locked m,
        // then blocks waiting for a new p.
        startlockedm(ref (gp).DerefOrNull());
        goto top;
    }
    execute(gp, inheritTime);
}

// dropg removes the association between m and the current goroutine m->curg (gp for short).
// Typically a caller sets gp's status away from Grunning and then
// immediately calls dropg to finish the job. The caller is also responsible
// for arranging that gp will be restarted using ready at an
// appropriate time. After calling dropg and arranging for gp to be
// readied later, the caller can do other work but eventually should
// call schedule to restart the scheduling of goroutines on this m.
internal static void dropg() {
    var gp = getg();
    setMNoWB((~(~gp).m).curg.of(g.Ꮡm), nil);
    setGNoWB((~gp).m.of(m.Ꮡcurg), nil);
}

internal static bool parkunlock_c(ж<g> Ꮡgp, @unsafe.Pointer @lock) {
    unlock((ж<mutex>)(uintptr)(@lock));
    return true;
}

// park continuation on g0.
internal static void park_m(ж<g> Ꮡgp) {
    var mp = getg().Value.m;
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        // Trace the event before the transition. It may take a
        // stack trace, but we won't own the stack after the
        // transition anymore.
        Δtrace.GoPark((~mp).waitTraceBlockReason, (~mp).waitTraceSkip);
    }
    // N.B. Not using casGToWaiting here because the waitreason is
    // set by park_m's caller.
    casgstatus(Ꮡgp, _Grunning, _Gwaiting);
    if (Δtrace.ok()) {
        traceRelease(Δtrace);
    }
    dropg();
    {
        var fn = mp.Value.waitunlockf; if (fn != default!) {
            var ok = fn(Ꮡgp, (~mp).waitlock);
            mp.Value.waitunlockf = default!;
            mp.Value.waitlock = default!;
            if (!ok) {
                var traceΔ1 = traceAcquire();
                casgstatus(Ꮡgp, _Gwaiting, _Grunnable);
                if (traceΔ1.ok()) {
                    traceΔ1.GoUnpark(Ꮡgp, 2);
                    traceRelease(traceΔ1);
                }
                execute(Ꮡgp, true); // Schedule it back, never returns.
            }
        }
    }
    schedule();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badGStatusˢ = "bad g status"u8;

internal static void goschedImpl(ж<g> Ꮡgp, bool preempted) {
    var Δtrace = traceAcquire();
    var status = readgstatus(Ꮡgp);
    if ((uint32)(status & ~(uint32)_Gscan) != _Grunning) {
        dumpgstatus(Ꮡgp);
        @throw(badGStatusˢ);
    }
    if (Δtrace.ok()) {
        // Trace the event before the transition. It may take a
        // stack trace, but we won't own the stack after the
        // transition anymore.
        if (preempted){
            Δtrace.GoPreempt();
        } else {
            Δtrace.GoSched();
        }
    }
    casgstatus(Ꮡgp, _Grunning, _Grunnable);
    if (Δtrace.ok()) {
        traceRelease(Δtrace);
    }
    dropg();
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    globrunqput(Ꮡgp);
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    if (mainStarted) {
        wakep();
    }
    schedule();
}

// Gosched continuation on g0.
internal static void gosched_m(ж<g> Ꮡgp) {
    goschedImpl(Ꮡgp, false);
}

// goschedguarded is a forbidden-states-avoided version of gosched_m.
internal static void goschedguarded_m(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    if (!canPreemptM(ref (gp.m).DerefOrNull())) {
        gogo(Ꮡgp.of(g.Ꮡsched)); // never return
    }
    goschedImpl(Ꮡgp, false);
}

internal static void gopreempt_m(ж<g> Ꮡgp) {
    goschedImpl(Ꮡgp, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string preemptAtUnknownPcˢ = "preempt at unknown pc"u8;
internal static readonly @string preemptSpwriteˢ = "preempt SPWRITE"u8;

// preemptPark parks gp and puts it in _Gpreempted.
//
//go:systemstack
internal static void preemptPark(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    var status = readgstatus(Ꮡgp);
    if ((uint32)(status & ~(uint32)_Gscan) != _Grunning) {
        dumpgstatus(Ꮡgp);
        @throw(badGStatusˢ);
    }
    if (gp.asyncSafePoint) {
        // Double-check that async preemption does not
        // happen in SPWRITE assembly functions.
        // isAsyncSafePoint must exclude this case.
        var f = findfunc(gp.sched.pc);
        if (!f.valid()) {
            @throw(preemptAtUnknownPcˢ);
        }
        if ((abi.FuncFlag)(f.flag & abi.FuncFlagSPWrite) != 0) {
            println((@string)"runtime: unexpected SPWRITE function"u8, funcname(f), (@string)"in async preempt"u8);
            @throw(preemptSpwriteˢ);
        }
    }
    // Transition from _Grunning to _Gscan|_Gpreempted. We can't
    // be in _Grunning when we dropg because then we'd be running
    // without an M, but the moment we're in _Gpreempted,
    // something could claim this G before we've fully cleaned it
    // up. Hence, we set the scan bit to lock down further
    // transitions until we can dropg.
    casGToPreemptScan(Ꮡgp, _Grunning, (uint32)((uint32)_Gscan | (uint32)_Gpreempted));
    dropg();
    // Be careful about how we trace this next event. The ordering
    // is subtle.
    //
    // The moment we CAS into _Gpreempted, suspendG could CAS to
    // _Gwaiting, do its work, and ready the goroutine. All of
    // this could happen before we even get the chance to emit
    // an event. The end result is that the events could appear
    // out of order, and the tracer generally assumes the scheduler
    // takes care of the ordering between GoPark and GoUnpark.
    //
    // The answer here is simple: emit the event while we still hold
    // the _Gscan bit on the goroutine. We still need to traceAcquire
    // and traceRelease across the CAS because the tracer could be
    // what's calling suspendG in the first place, and we want the
    // CAS and event emission to appear atomic to the tracer.
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GoPark(traceBlockPreempted, 0);
    }
    casfrom_Gscanstatus(Ꮡgp, (uint32)((uint32)_Gscan | (uint32)_Gpreempted), _Gpreempted);
    if (Δtrace.ok()) {
        traceRelease(Δtrace);
    }
    schedule();
}

// goyield is like Gosched, but it:
// - emits a GoPreempt trace event instead of a GoSched trace event
// - puts the current G on the runq of the current P instead of the globrunq
//
// goyield should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gvisor.dev/gvisor
//   - github.com/sagernet/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname goyield
internal static void goyield() {
    checkTimeouts();
    mcall(goyield_m);
}

internal static void goyield_m(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    var Δtrace = traceAcquire();
    var pp = (~gp.m).p.ptr();
    if (Δtrace.ok()) {
        // Trace the event before the transition. It may take a
        // stack trace, but we won't own the stack after the
        // transition anymore.
        Δtrace.GoPreempt();
    }
    casgstatus(Ꮡgp, _Grunning, _Grunnable);
    if (Δtrace.ok()) {
        traceRelease(Δtrace);
    }
    dropg();
    runqput(pp, Ꮡgp, false);
    schedule();
}

// Finishes execution of the current goroutine.
internal static void goexit1() {
    if (raceenabled) {
        racegoend();
    }
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GoEnd();
        traceRelease(Δtrace);
    }
    mcall(goexit0);
}

// goexit continuation on g0.
internal static void goexit0(ж<g> Ꮡgp) {
    gdestroy(Ꮡgp);
    schedule();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exitedAGoroutineˢ = "exited a goroutine internally locked to the OS thread"u8;

internal static void gdestroy(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    var mp = getg().Value.m;
    var pp = (~mp).p.ptr();
    casgstatus(Ꮡgp, _Grunning, _Gdead);
    ᏑgcController.addScannableStack(pp, -(int64)(gp.stack.hi - gp.stack.lo));
    if (isSystemGoroutine(ref (Ꮡgp).DerefOrNull(), false)) {
        Ꮡsched.of(schedt.Ꮡngsys).Add(-1);
    }
    gp.m = default!;
    var locked = gp.lockedm != 0;
    gp.lockedm = 0;
    mp.Value.lockedg = 0;
    gp.preemptStop = false;
    gp.paniconfault = false;
    gp._defer = default!; // should be true already but just in case.
    gp._panic = default!; // non-nil for Goexit during panic. points at stack-allocated data.
    gp.writebuf = default!;
    gp.waitreason = waitReasonZero;
    gp.param = default!;
    gp.labels = default!;
    gp.timer = default!;
    if (gcBlackenEnabled != 0 && gp.gcAssistBytes > 0) {
        // Flush assist credit to the global pool. This gives
        // better information to pacing if the application is
        // rapidly creating an exiting goroutines.
        var assistWorkPerByte = ᏑgcController.of(gcControllerState.ᏑassistWorkPerByte).Load();
        var scanCredit = (int64)(assistWorkPerByte * (float64)gp.gcAssistBytes);
        ᏑgcController.of(gcControllerState.ᏑbgScanCredit).Add(scanCredit);
        gp.gcAssistBytes = 0;
    }
    dropg();
    if (GOARCH == "wasm"u8) {
        // no threads yet on wasm
        gfput(pp, Ꮡgp);
        return;
    }
    if (locked && (~mp).lockedInt != 0) {
        print((@string)"runtime: mp.lockedInt = "u8, (~mp).lockedInt, (@string)"\n"u8);
        @throw(exitedAGoroutineˢ);
    }
    gfput(pp, Ꮡgp);
    if (locked) {
        // The goroutine may have locked this thread because
        // it put it in an unusual kernel state. Kill it
        // rather than returning it to the thread pool.
        // Return to mstart, which will release the P and exit
        // the thread.
        if (GOOS != "plan9"u8){
            // See golang.org/issue/22227.
            gogo((~mp).g0.of(g.Ꮡsched));
        } else {
            // Clear lockedExt on plan9 since we may end up re-using
            // this thread.
            mp.Value.lockedExt = 0;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string saveOnSystemGNotAllowedˢ = "save on system g not allowed"u8;

// save updates getg().sched to refer to pc and sp so that a following
// gogo will restore pc and sp.
//
// save must not have write barriers because invoking a write barrier
// can clobber getg().sched.
//
//go:nosplit
//go:nowritebarrierrec
internal static void save(uintptr pc, uintptr sp, uintptr bp) {
    var gp = getg();
    if (gp == (~(~gp).m).g0 || gp == (~(~gp).m).gsignal) {
        // m.g0.sched is special and must describe the context
        // for exiting the thread. mstart1 writes to it directly.
        // m.gsignal.sched should not be used at all.
        // This check makes sure save calls do not accidentally
        // run in contexts where they'd write to system g's.
        @throw(saveOnSystemGNotAllowedˢ);
    }
    gp.Value.sched.pc = pc;
    gp.Value.sched.sp = sp;
    gp.Value.sched.lr = 0;
    gp.Value.sched.ret = 0;
    gp.Value.sched.bp = bp;
    // We need to ensure ctxt is zero, but can't have a write
    // barrier here. However, it should always already be zero.
    // Assert that.
    if ((~gp).sched.ctxt != nil) {
        badctxt();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string entersyscallˢ = "entersyscall"u8;

// The goroutine g is about to enter a system call.
// Record that it's not using the cpu anymore.
// This is called only from the go syscall library and cgocall,
// not from the low-level system calls used by the runtime.
//
// Entersyscall cannot split the stack: the save must
// make g->sched refer to the caller's stack segment, because
// entersyscall is going to return immediately after.
//
// Nothing entersyscall calls can split the stack either.
// We cannot safely move the stack during an active call to syscall,
// because we do not know which of the uintptr arguments are
// really pointers (back into the stack).
// In practice, this means that we make the fast path run through
// entersyscall doing no-split things, and the slow path has to use systemstack
// to run bigger things on the system stack.
//
// reentersyscall is the entry point used by cgo callbacks, where explicitly
// saved SP and PC are restored. This is needed when exitsyscall will be called
// from a function further up in the call stack than the parent, as g->syscallsp
// must always point to a valid stack frame. entersyscall below is the normal
// entry point for syscalls, which obtains the SP and PC from the caller.
//
//go:nosplit
internal static void reentersyscall(uintptr pc, uintptr sp, uintptr bp) {
    ref var Δtrace = ref heap<traceLocker>(out var Ꮡtrace);
    Δtrace = traceAcquire();
    var gp = getg();
    // Disable preemption because during this function g is in Gsyscall status,
    // but can have inconsistent g->sched, do not let GC observe it.
    gp.Value.m.Value.locks++;
    // Entersyscall must not call any function that might split/grow the stack.
    // (See details in comment above.)
    // Catch calls that might, by replacing the stack guard with something that
    // will trip any stack check and leaving a flag to tell newstack to die.
    gp.Value.stackguard0 = stackPreempt;
    gp.Value.throwsplit = true;
    // Leave SP around for GC and traceback.
    save(pc, sp, bp);
    gp.Value.syscallsp = sp;
    gp.Value.syscallpc = pc;
    gp.Value.syscallbp = bp;
    casgstatus(gp, _Grunning, _Gsyscall);
    if (staticLockRanking) {
        // When doing static lock ranking casgstatus can call
        // systemstack which clobbers g.sched.
        save(pc, sp, bp);
    }
    if ((~gp).syscallsp < (~gp).stack.lo || (~gp).stack.hi < (~gp).syscallsp) {
        var gpʗ1 = gp;
        systemstack(() => {
            print((@string)"entersyscall inconsistent sp "u8, ((Δhex)(uint64)(~gpʗ1).syscallsp), (@string)" ["u8, ((Δhex)(uint64)(~gpʗ1).stack.lo), (@string)","u8, ((Δhex)(uint64)(~gpʗ1).stack.hi), (@string)"]\n"u8);
            @throw(entersyscallˢ);
        });
    }
    if ((~gp).syscallbp != 0 && (~gp).syscallbp < (~gp).stack.lo || (~gp).stack.hi < (~gp).syscallbp) {
        var gpʗ2 = gp;
        systemstack(() => {
            print((@string)"entersyscall inconsistent bp "u8, ((Δhex)(uint64)(~gpʗ2).syscallbp), (@string)" ["u8, ((Δhex)(uint64)(~gpʗ2).stack.lo), (@string)","u8, ((Δhex)(uint64)(~gpʗ2).stack.hi), (@string)"]\n"u8);
            @throw(entersyscallˢ);
        });
    }
    if (Δtrace.ok()) {
        var traceʗ1 = Δtrace;
        systemstack(() => {
            traceʗ1.GoSysCall();
            traceRelease(traceʗ1);
        });
        // systemstack itself clobbers g.sched.{pc,sp} and we might
        // need them later when the G is genuinely blocked in a
        // syscall
        save(pc, sp, bp);
    }
    if (Ꮡsched.of(schedt.Ꮡsysmonwait).Load()) {
        systemstack(entersyscall_sysmon);
        save(pc, sp, bp);
    }
    if ((~(~(~gp).m).p.ptr()).runSafePointFn != 0) {
        // runSafePointFn may stack split if run on this stack
        systemstack(runSafePointFn);
        save(pc, sp, bp);
    }
    gp.Value.m.Value.syscalltick = (~(~gp).m).p.ptr().Value.syscalltick;
    var pp = (~(~gp).m).p.ptr();
    pp.Value.m = 0;
    (~gp).m.of(m.Ꮡoldp).set(pp);
    gp.Value.m.Value.p = 0;
    atomic.Store(pp.of(runtime_package.Δp.Ꮡstatus), _Psyscall);
    if (Ꮡsched.of(schedt.Ꮡgcwaiting).Load()) {
        systemstack(entersyscall_gcwait);
        save(pc, sp, bp);
    }
    gp.Value.m.Value.locks--;
}

// Standard syscall entry used by the go syscall library and normal cgo calls.
//
// This is exported via linkname to assembly in the syscall package and x/sys.
//
// Other packages should not be accessing entersyscall directly,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gvisor.dev/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:nosplit
//go:linkname entersyscall
internal static void entersyscall() {
    // N.B. getcallerfp cannot be written directly as argument in the call
    // to reentersyscall because it forces spilling the other arguments to
    // the stack. This results in exceeding the nosplit stack requirements
    // on some platforms.
    var fp = getcallerfp();
    reentersyscall(getcallerpc(), getcallersp(), fp);
}

internal static void entersyscall_sysmon() {
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    if (Ꮡsched.of(schedt.Ꮡsysmonwait).Load()) {
        Ꮡsched.of(schedt.Ꮡsysmonwait).Store(false);
        notewakeup(Ꮡsched.of(schedt.Ꮡsysmonnote));
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock));
}

internal static void entersyscall_gcwait() {
    var gp = getg();
    var pp = (~(~gp).m).oldp.ptr();
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    var Δtrace = traceAcquire();
    if (sched.stopwait > 0 && atomic.Cas(pp.of(runtime_package.Δp.Ꮡstatus), _Psyscall, _Pgcstop)){
        if (Δtrace.ok()) {
            // This is a steal in the new tracer. While it's very likely
            // that we were the ones to put this P into _Psyscall, between
            // then and now it's totally possible it had been stolen and
            // then put back into _Psyscall for us to acquire here. In such
            // case ProcStop would be incorrect.
            //
            // TODO(mknyszek): Consider emitting a ProcStop instead when
            // gp.m.syscalltick == pp.syscalltick, since then we know we never
            // lost the P.
            Δtrace.ProcSteal(pp, true);
            traceRelease(Δtrace);
        }
        pp.Value.gcStopTime = nanotime();
        pp.Value.syscalltick++;
        {
            sched.stopwait--; if (sched.stopwait == 0) {
                notewakeup(Ꮡsched.of(schedt.Ꮡstopnote));
            }
        }
    } else 
    if (Δtrace.ok()) {
        traceRelease(Δtrace);
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string entersyscallblockˢ = "entersyscallblock"u8;

// The same as entersyscall(), but with a hint that the syscall is blocking.

// entersyscallblock should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gvisor.dev/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname entersyscallblock
//go:nosplit
internal static void entersyscallblock() {
    var gp = getg();
    gp.Value.m.Value.locks++; // see comment in entersyscall
    gp.Value.throwsplit = true;
    gp.Value.stackguard0 = stackPreempt; // see comment in entersyscall
    gp.Value.m.Value.syscalltick = (~(~gp).m).p.ptr().Value.syscalltick;
    (~(~gp).m).p.ptr().Value.syscalltick++;
    // Leave SP around for GC and traceback.
    var pc = getcallerpc();
    var sp = getcallersp();
    var bp = getcallerfp();
    save(pc, sp, bp);
    gp.Value.syscallsp = gp.Value.sched.sp;
    gp.Value.syscallpc = gp.Value.sched.pc;
    gp.Value.syscallbp = gp.Value.sched.bp;
    if ((~gp).syscallsp < (~gp).stack.lo || (~gp).stack.hi < (~gp).syscallsp) {
        var sp1 = sp;
        var sp2 = gp.Value.sched.sp;
        var sp3 = gp.Value.syscallsp;
        var gpʗ1 = gp;
        systemstack(() => {
            print((@string)"entersyscallblock inconsistent sp "u8, ((Δhex)(uint64)sp1), (@string)" "u8, ((Δhex)(uint64)sp2), (@string)" "u8, ((Δhex)(uint64)sp3), (@string)" ["u8, ((Δhex)(uint64)(~gpʗ1).stack.lo), (@string)","u8, ((Δhex)(uint64)(~gpʗ1).stack.hi), (@string)"]\n"u8);
            @throw(entersyscallblockˢ);
        });
    }
    casgstatus(gp, _Grunning, _Gsyscall);
    if ((~gp).syscallsp < (~gp).stack.lo || (~gp).stack.hi < (~gp).syscallsp) {
        var gpʗ2 = gp;
        systemstack(() => {
            print((@string)"entersyscallblock inconsistent sp "u8, ((Δhex)(uint64)sp), (@string)" "u8, ((Δhex)(uint64)(~gpʗ2).sched.sp), (@string)" "u8, ((Δhex)(uint64)(~gpʗ2).syscallsp), (@string)" ["u8, ((Δhex)(uint64)(~gpʗ2).stack.lo), (@string)","u8, ((Δhex)(uint64)(~gpʗ2).stack.hi), (@string)"]\n"u8);
            @throw(entersyscallblockˢ);
        });
    }
    if ((~gp).syscallbp != 0 && (~gp).syscallbp < (~gp).stack.lo || (~gp).stack.hi < (~gp).syscallbp) {
        var gpʗ3 = gp;
        systemstack(() => {
            print((@string)"entersyscallblock inconsistent bp "u8, ((Δhex)(uint64)bp), (@string)" "u8, ((Δhex)(uint64)(~gpʗ3).sched.bp), (@string)" "u8, ((Δhex)(uint64)(~gpʗ3).syscallbp), (@string)" ["u8, ((Δhex)(uint64)(~gpʗ3).stack.lo), (@string)","u8, ((Δhex)(uint64)(~gpʗ3).stack.hi), (@string)"]\n"u8);
            @throw(entersyscallblockˢ);
        });
    }
    systemstack(entersyscallblock_handoff);
    // Resave for traceback during blocked call.
    save(getcallerpc(), getcallersp(), getcallerfp());
    gp.Value.m.Value.locks--;
}

internal static void entersyscallblock_handoff() {
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GoSysCall();
        traceRelease(Δtrace);
    }
    handoffp(releasep());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exitsyscallSyscallFrameˢ = "exitsyscall: syscall frame is no longer valid"u8;

// The goroutine g exited its system call.
// Arrange for it to run on a cpu again.
// This is called only from the go syscall library, not
// from the low-level system calls used by the runtime.
//
// Write barriers are not allowed because our P may have been stolen.
//
// This is exported via linkname to assembly in the syscall package.
//
// exitsyscall should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gvisor.dev/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:nosplit
//go:nowritebarrierrec
//go:linkname exitsyscall
internal static void exitsyscall() {
    var gp = getg();
    gp.Value.m.Value.locks++; // see comment in entersyscall
    if (getcallersp() > (~gp).syscallsp) {
        @throw(exitsyscallSyscallFrameˢ);
    }
    gp.Value.waitsince = 0;
    var oldp = (~(~gp).m).oldp.ptr();
    gp.Value.m.Value.oldp = 0;
    if (exitsyscallfast(oldp)) {
        // When exitsyscallfast returns success, we have a P so can now use
        // write barriers
        if (goroutineProfile.active) {
            // Make sure that gp has had its stack written out to the goroutine
            // profile, exactly as it was when the goroutine profiler first
            // stopped the world.
            var gpʗ1 = gp;
            systemstack(() => {
                tryRecordGoroutineProfileWB(gpʗ1);
            });
        }
        ref var Δtrace = ref heap<traceLocker>(out var Ꮡtrace);
        Δtrace = traceAcquire();
        if (Δtrace.ok()) {
            var lostP = oldp != (~(~gp).m).p.ptr() || (~(~gp).m).syscalltick != (~(~(~gp).m).p.ptr()).syscalltick;
            var traceʗ1 = Δtrace;
            systemstack(() => {
                // Write out syscall exit eagerly.
                //
                // It's important that we write this *after* we know whether we
                // lost our P or not (determined by exitsyscallfast).
                traceʗ1.GoSysExit(lostP);
                if (lostP) {
                    // We lost the P at some point, even though we got it back here.
                    // Trace that we're starting again, because there was a traceGoSysBlock
                    // call somewhere in exitsyscallfast (indicating that this goroutine
                    // had blocked) and we're about to start running again.
                    traceʗ1.GoStart();
                }
            });
        }
        // There's a cpu for us, so we can run.
        (~(~gp).m).p.ptr().Value.syscalltick++;
        // We need to cas the status and scan before resuming...
        casgstatus(gp, _Gsyscall, _Grunning);
        if (Δtrace.ok()) {
            traceRelease(Δtrace);
        }
        // Garbage collector isn't running (since we are),
        // so okay to clear syscallsp.
        gp.Value.syscallsp = 0;
        gp.Value.m.Value.locks--;
        if ((~gp).preempt){
            // restore the preemption request in case we've cleared it in newstack
            gp.Value.stackguard0 = stackPreempt;
        } else {
            // otherwise restore the real stackGuard, we've spoiled it in entersyscall/entersyscallblock
            gp.Value.stackguard0 = (~gp).stack.lo + (uintptr)stackGuard;
        }
        gp.Value.throwsplit = false;
        if (sched.disable.user && !schedEnabled(ref (gp).DerefOrNull())) {
            // Scheduling of this goroutine is disabled.
            Gosched();
        }
        return;
    }
    gp.Value.m.Value.locks--;
    // Call the scheduler.
    mcall(exitsyscall0);
    // Scheduler returned, so we're allowed to run now.
    // Delete the syscallsp information that we left for
    // the garbage collector during the system call.
    // Must wait until now because until gosched returns
    // we don't know for sure that the garbage collector
    // is not running.
    gp.Value.syscallsp = 0;
    (~(~gp).m).p.ptr().Value.syscalltick++;
    gp.Value.throwsplit = false;
}

//go:nosplit
internal static bool exitsyscallfast(ж<Δp> Ꮡoldp) {
    ref var oldp = ref Ꮡoldp.DerefOrNull();

    // Freezetheworld sets stopwait but does not retake P's.
    if (sched.stopwait == freezeStopWait) {
        return false;
    }
    // Try to re-acquire the last P.
    var Δtrace = traceAcquire();
    if (Ꮡoldp != nil && oldp.status == _Psyscall && atomic.Cas(Ꮡoldp.of(runtime_package.Δp.Ꮡstatus), _Psyscall, _Pidle)) {
        // There's a cpu for us, so we can run.
        wirep(Ꮡoldp);
        exitsyscallfast_reacquired(Δtrace);
        if (Δtrace.ok()) {
            traceRelease(Δtrace);
        }
        return true;
    }
    if (Δtrace.ok()) {
        traceRelease(Δtrace);
    }
    // Try to get any other idle P.
    if (sched.pidle != 0) {
        bool ok = default!;
        systemstack(() => {
            ok = exitsyscallfast_pidle();
        });
        if (ok) {
            return true;
        }
    }
    return false;
}

// exitsyscallfast_reacquired is the exitsyscall path on which this G
// has successfully reacquired the P it was running on before the
// syscall.
//
//go:nosplit
internal static void exitsyscallfast_reacquired(traceLocker Δtrace) {
    var gp = getg();
    if ((~(~gp).m).syscalltick != (~(~(~gp).m).p.ptr()).syscalltick) {
        if (Δtrace.ok()) {
            // The p was retaken and then enter into syscall again (since gp.m.syscalltick has changed).
            // traceGoSysBlock for this syscall was already emitted,
            // but here we effectively retake the p from the new syscall running on the same p.
            var gpʗ1 = gp;
            var traceʗ1 = Δtrace;
            systemstack(() => {
                // We're stealing the P. It's treated
                // as if it temporarily stopped running. Then, start running.
                traceʗ1.ProcSteal((~(~gpʗ1).m).p.ptr(), true);
                traceʗ1.ProcStart();
            });
        }
        (~(~gp).m).p.ptr().Value.syscalltick++;
    }
}

internal static bool exitsyscallfast_pidle() {
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    var (pp, _) = pidleget(0);
    if (pp != nil && Ꮡsched.of(schedt.Ꮡsysmonwait).Load()) {
        Ꮡsched.of(schedt.Ꮡsysmonwait).Store(false);
        notewakeup(Ꮡsched.of(schedt.Ꮡsysmonnote));
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    if (pp != nil) {
        acquirep(pp);
        return true;
    }
    return false;
}

// exitsyscall slow path on g0.
// Failed to acquire P, enqueue gp as runnable.
//
// Called via mcall, so gp is the calling g from this M.
//
//go:nowritebarrierrec
internal static void exitsyscall0(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    traceLocker Δtrace = default!;
    traceExitingSyscall();
    Δtrace = traceAcquire();
    casgstatus(Ꮡgp, _Gsyscall, _Grunnable);
    traceExitedSyscall();
    if (Δtrace.ok()) {
        // Write out syscall exit eagerly.
        //
        // It's important that we write this *after* we know whether we
        // lost our P or not (determined by exitsyscallfast).
        Δtrace.GoSysExit(true);
        traceRelease(Δtrace);
    }
    dropg();
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    ж<Δp> pp = default!;
    if (schedEnabled(ref (Ꮡgp).DerefOrNull())) {
        (pp, _) = pidleget(0);
    }
    bool locked = default!;
    if (pp == nil){
        globrunqput(Ꮡgp);
        // Below, we stoplockedm if gp is locked. globrunqput releases
        // ownership of gp, so we must check if gp is locked prior to
        // committing the release by unlocking sched.lock, otherwise we
        // could race with another M transitioning gp from unlocked to
        // locked.
        locked = gp.lockedm != 0;
    } else 
    if (Ꮡsched.of(schedt.Ꮡsysmonwait).Load()) {
        Ꮡsched.of(schedt.Ꮡsysmonwait).Store(false);
        notewakeup(Ꮡsched.of(schedt.Ꮡsysmonnote));
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    if (pp != nil) {
        acquirep(pp);
        execute(Ꮡgp, false); // Never returns.
    }
    if (locked) {
        // Wait until another thread schedules gp and so m again.
        //
        // N.B. lockedm must be this M, as this g was running on this M
        // before entersyscall.
        stoplockedm();
        execute(Ꮡgp, false); // Never returns.
    }
    stopm();
    schedule(); // Never returns.
}

// Called from syscall package before fork.
//
// syscall_runtime_BeforeFork is for package syscall,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/containerd/containerd
//   - gvisor.dev/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname syscall_runtime_BeforeFork syscall.runtime_BeforeFork
//go:nosplit
internal static void syscall_runtime_BeforeFork() {
    var gp = getg().Value.m.Value.curg;
    // Block signals during a fork, so that the child does not run
    // a signal handler before exec if a signal is sent to the process
    // group. See issue #18600.
    gp.Value.m.Value.locks++;
    sigsave((~gp).m.of(m.Ꮡsigmask));
    sigblock(false);
    // This function is called before fork in syscall package.
    // Code between fork and exec must not allocate memory nor even try to grow stack.
    // Here we spoil g.stackguard0 to reliably detect any attempts to grow stack.
    // runtime_AfterFork will undo this in parent process, but not in child.
    gp.Value.stackguard0 = stackFork;
}

// Called from syscall package after fork in parent.
//
// syscall_runtime_AfterFork is for package syscall,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/containerd/containerd
//   - gvisor.dev/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname syscall_runtime_AfterFork syscall.runtime_AfterFork
//go:nosplit
internal static void syscall_runtime_AfterFork() {
    var gp = getg().Value.m.Value.curg;
    // See the comments in beforefork.
    gp.Value.stackguard0 = (~gp).stack.lo + (uintptr)stackGuard;
    msigrestore((~(~gp).m).sigmask);
    gp.Value.m.Value.locks--;
}

// inForkedChild is true while manipulating signals in the child process.
// This is used to avoid calling libc functions in case we are using vfork.
internal static bool inForkedChild;

// Called from syscall package after fork in child.
// It resets non-sigignored signals to the default handler, and
// restores the signal mask in preparation for the exec.
//
// Because this might be called during a vfork, and therefore may be
// temporarily sharing address space with the parent process, this must
// not change any global variables or calling into C code that may do so.
//
// syscall_runtime_AfterForkInChild is for package syscall,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/containerd/containerd
//   - gvisor.dev/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname syscall_runtime_AfterForkInChild syscall.runtime_AfterForkInChild
//go:nosplit
//go:nowritebarrierrec
internal static void syscall_runtime_AfterForkInChild() {
    // It's OK to change the global variable inForkedChild here
    // because we are going to change it back. There is no race here,
    // because if we are sharing address space with the parent process,
    // then the parent process can not be running concurrently.
    inForkedChild = true;
    clearSignalHandlers();
    // When we are the child we are the only thread running,
    // so we know that nothing else has changed gp.m.sigmask.
    msigrestore((~(~getg()).m).sigmask);
    inForkedChild = false;
}

// pendingPreemptSignals is the number of preemption signals
// that have been sent but not received. This is only used on Darwin.
// For #41702.
internal static ж<atomic.Int32> ᏑpendingPreemptSignals = new StandardBox<atomic.Int32>(default(atomic.Int32));
internal static ref atomic.Int32 pendingPreemptSignals => ref ᏑpendingPreemptSignals.Value;

// Called from syscall package before Exec.
//
//go:linkname syscall_runtime_BeforeExec syscall.runtime_BeforeExec
internal static void syscall_runtime_BeforeExec() {
    // Prevent thread creation during exec.
    ᏑexecLock.@lock();
    // On Darwin, wait for all pending preemption signals to
    // be received. See issue #41702.
    if (GOOS == "darwin"u8 || GOOS == "ios"u8) {
        while (ᏑpendingPreemptSignals.Load() > 0) {
            osyield();
        }
    }
}

// Called from syscall package after Exec.
//
//go:linkname syscall_runtime_AfterExec syscall.runtime_AfterExec
internal static void syscall_runtime_AfterExec() {
    ᏑexecLock.unlock();
}

// Allocate a new g, with a stack big enough for stacksize bytes.
internal static ж<g> malg(int32 stacksize) {
    var newg = @new<g>();
    if (stacksize >= 0) {
        stacksize = round2((int32)stackSystem + stacksize);
        var newgʗ1 = newg;
        systemstack(() => {
            newgʗ1.Value.stack = @stackalloc((uint32)stacksize);
        });
        newg.Value.stackguard0 = (~newg).stack.lo + (uintptr)stackGuard;
        newg.Value.stackguard1 = ~(uintptr)0;
        // Clear the bottom word of the stack. We record g
        // there on gsignal stack during VDSO on ARM and ARM64.
        ((ж<uintptr>)(uintptr)((@unsafe.Pointer)(~newg).stack.lo)).Value = 0;
    }
    return newg;
}

// Create a new g running fn.
// Put it on the queue of g's waiting to run.
// The compiler turns a go statement into a call to this.
internal static void newproc(ж<funcval> Ꮡfn) {
    var gp = getg();
    var pc = getcallerpc();
    var gpʗ1 = gp;
    systemstack(() => {
        var newg = newproc1(Ꮡfn, gpʗ1, pc, false, waitReasonZero);
        var pp = (~(~getg()).m).p.ptr();
        runqput(pp, newg, true);
        if (mainStarted) {
            wakep();
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goOfNilFuncValueˢ = "go of nil func value"u8;
internal static readonly @string newproc1NewgMissingStackˢ = "newproc1: newg missing stack"u8;
internal static readonly @string newproc1NewGIsNotGdeadˢ = "newproc1: new g is not Gdead"u8;

// Create a new g in state _Grunnable (or _Gwaiting if parked is true), starting at fn.
// callerpc is the address of the go statement that created this. The caller is responsible
// for adding the new g to the scheduler. If parked is true, waitreason must be non-zero.
internal static ж<g> newproc1(ж<funcval> Ꮡfn, ж<g> Ꮡcallergp, uintptr callerpc, bool parked, waitReason waitreason) {
    ref var fn = ref Ꮡfn.DerefOrNull();
    ref var callergp = ref Ꮡcallergp.DerefOrNull();

    if (Ꮡfn == nil) {
        fatal(goOfNilFuncValueˢ);
    }
    var mp = acquirem(); // disable preemption because we hold M and P in local vars.
    var pp = (~mp).p.ptr();
    var newg = gfget(pp);
    if (newg == nil) {
        newg = malg(stackMin);
        casgstatus(newg, _Gidle, _Gdead);
        allgadd(newg); // publishes with a g->status of Gdead so GC scanner doesn't look at uninitialized stack.
    }
    if ((~newg).stack.hi == 0) {
        @throw(newproc1NewgMissingStackˢ);
    }
    if (readgstatus(newg) != _Gdead) {
        @throw(newproc1NewGIsNotGdeadˢ);
    }
    var totalSize = (uintptr)(4 * goarch.PtrSize + sys.MinFrameSize); // extra space in case of reads slightly beyond frame
    totalSize = alignUp(totalSize, sys.StackAlign);
    var sp = (~newg).stack.hi - totalSize;
    if (usesLR) {
        // caller's LR
        ((ж<uintptr>)(uintptr)((@unsafe.Pointer)sp)).Value = 0;
        prepGoExitFrame(sp);
    }
    if (GOARCH == "arm64"u8) {
        // caller's FP
        ((ж<uintptr>)(uintptr)((@unsafe.Pointer)(sp - (uintptr)goarch.PtrSize))).Value = 0;
    }
    memclrNoHeapPointers(@unsafe.Pointer.FromPinnedBox(newg.of(g.Ꮡsched)), /* unsafe.Sizeof(newg.sched) */ (uintptr)56);
    newg.Value.sched.sp = sp;
    newg.Value.stktopsp = sp;
    newg.Value.sched.pc = abi.FuncPCABI0(goexit) + (uintptr)sys.PCQuantum; // +PCQuantum so that previous instruction is in same function
    newg.Value.sched.g = new Δguintptr(newg);
    gostartcallfn(ref (newg.of(g.Ꮡsched)).DerefOrNull(), Ꮡfn);
    newg.Value.parentGoid = callergp.goid;
    newg.Value.gopc = callerpc;
    newg.Value.ancestors = saveAncestors(Ꮡcallergp);
    newg.Value.startpc = fn.fn;
    if (isSystemGoroutine(ref (newg).DerefOrNull(), false)){
        Ꮡsched.of(schedt.Ꮡngsys).Add(1);
    } else {
        // Only user goroutines inherit pprof labels.
        if ((~mp).curg != nil) {
            newg.Value.labels = mp.Value.curg.Value.labels;
        }
        if (goroutineProfile.active) {
            // A concurrent goroutine profile is running. It should include
            // exactly the set of goroutines that were alive when the goroutine
            // profiler first stopped the world. That does not include newg, so
            // mark it as not needing a profile before transitioning it from
            // _Gdead.
            newg.of(g.ᏑgoroutineProfiled).Store(goroutineProfileSatisfied);
        }
    }
    // Track initial transition?
    newg.Value.trackingSeq = (uint8)cheaprand();
    if ((uint8)((~newg).trackingSeq % (uint8)gTrackingPeriod) == 0) {
        newg.Value.tracking = true;
    }
    ᏑgcController.addScannableStack(pp, (int64)((~newg).stack.hi - (~newg).stack.lo));
    // Get a goid and switch to runnable. Make all this atomic to the tracer.
    var Δtrace = traceAcquire();
    uint32 status = _Grunnable;
    if (parked) {
        status = _Gwaiting;
        newg.Value.waitreason = waitreason;
    }
    casgstatus(newg, _Gdead, status);
    if ((~pp).goidcache == (~pp).goidcacheend) {
        // Sched.goidgen is the last allocated id,
        // this batch must be [sched.goidgen+1, sched.goidgen+GoidCacheBatch].
        // At startup sched.goidgen=0, so main goroutine receives goid=1.
        pp.Value.goidcache = Ꮡsched.of(schedt.Ꮡgoidgen).Add(_GoidCacheBatch);
        pp.Value.goidcache -= _GoidCacheBatch - 1;
        pp.Value.goidcacheend = (~pp).goidcache + (uint64)_GoidCacheBatch;
    }
    newg.Value.goid = pp.Value.goidcache;
    pp.Value.goidcache++;
    newg.of(g.Ꮡtrace).reset();
    if (Δtrace.ok()) {
        Δtrace.GoCreate(newg, (~newg).startpc, parked);
        traceRelease(Δtrace);
    }
    // Set up race context.
    if (raceenabled) {
        newg.Value.racectx = racegostart(callerpc);
        newg.Value.raceignore = 0;
        if ((~newg).labels != nil) {
            // See note in proflabel.go on labelSync's role in synchronizing
            // with the reads in the signal handler.
            racereleasemergeg(ref (newg).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑlabelSync));
        }
    }
    releasem(ref (mp).DerefOrNull());
    return newg;
}

// saveAncestors copies previous ancestors of the given caller g and
// includes info for the current caller into a new set of tracebacks for
// a g being created.
internal static ж<slice<ancestorInfo>> saveAncestors(ж<g> Ꮡcallergp) {
    ref var callergp = ref Ꮡcallergp.DerefOrNull();

    // Copy all prior info, except for the root goroutine (goid 0).
    if (debug.tracebackancestors <= 0 || callergp.goid == 0) {
        return default!;
    }
    slice<ancestorInfo> callerAncestors = default!;
    if (callergp.ancestors != nil) {
        callerAncestors = callergp.ancestors.ValueSlot;
    }
    var n = (int32)len(callerAncestors) + 1;
    if (n > debug.tracebackancestors) {
        n = debug.tracebackancestors;
    }
    var ancestors = new slice<ancestorInfo>(n);
    copy(ancestors[1..], callerAncestors);
    array<uintptr> pcs = new(50); /* tracebackInnerFrames */
    nint npcs = gcallers(Ꮡcallergp, 0, pcs[..]);
    var ipcs = new slice<uintptr>(npcs);
    copy(ipcs, pcs[..]);
    ancestors[0] = new ancestorInfo(
        pcs: ipcs,
        goid: callergp.goid,
        gopc: callergp.gopc
    );
    var ancestorsp = @new<slice<ancestorInfo>>();
    ancestorsp.ValueSlot = ancestors;
    return ancestorsp;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gfputBadStatusNotGdeadˢ = "gfput: bad status (not Gdead)"u8;

// Put on gfree list.
// If local list is too long, transfer a batch to the global list.
internal static void gfput(ж<Δp> Ꮡpp, ж<g> Ꮡgp) {
    ref var pp = ref Ꮡpp.DerefOrNull();
    ref var gp = ref Ꮡgp.DerefOrNull();

    if (readgstatus(Ꮡgp) != _Gdead) {
        @throw(gfputBadStatusNotGdeadˢ);
    }
    var stksize = gp.stack.hi - gp.stack.lo;
    if (stksize != (uintptr)startingStackSize) {
        // non-standard stack size - free it.
        stackfree(gp.stack);
        gp.stack.lo = 0;
        gp.stack.hi = 0;
        gp.stackguard0 = 0;
    }
    Ꮡpp.of(runtime_package.Δp.ᏑgFree).of(p_gFree.ᏑgList).push(Ꮡgp);
    pp.gFree.n++;
    if (pp.gFree.n >= 64) {
        int32 inc = default!;
        gQueue stackQ = default!;
        gQueue noStackQ = default!;
        while (pp.gFree.n >= 32) {
            var gpΔ1 = Ꮡpp.of(runtime_package.Δp.ᏑgFree).of(p_gFree.ᏑgList).pop();
            pp.gFree.n--;
            if ((~gpΔ1).stack.lo == 0){
                noStackQ.push(gpΔ1);
            } else {
                stackQ.push(gpΔ1);
            }
            inc++;
        }
        @lock(Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡlock));
        Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.ᏑnoStack).pushAll(noStackQ);
        Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡstack).pushAll(stackQ);
        sched.gFree.n += inc;
        unlock(Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡlock));
    }
}

// Get from gfree list.
// If local list is empty, grab a batch from global list.
internal static ж<g> gfget(ж<Δp> Ꮡpp) {
    ref var pp = ref Ꮡpp.DerefOrNull();

retry:
    if (Ꮡpp.of(runtime_package.Δp.ᏑgFree).of(p_gFree.ᏑgList).empty() && (!Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡstack).empty() || !Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.ᏑnoStack).empty())) {
        @lock(Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡlock));
        // Move a batch of free Gs to the P.
        while (pp.gFree.n < 32) {
            // Prefer Gs with stacks.
            var gpΔ1 = Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡstack).pop();
            if (gpΔ1 == nil) {
                gpΔ1 = Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.ᏑnoStack).pop();
                if (gpΔ1 == nil) {
                    break;
                }
            }
            sched.gFree.n--;
            Ꮡpp.of(runtime_package.Δp.ᏑgFree).of(p_gFree.ᏑgList).push(gpΔ1);
            pp.gFree.n++;
        }
        unlock(Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡlock));
        goto retry;
    }
    var gp = Ꮡpp.of(runtime_package.Δp.ᏑgFree).of(p_gFree.ᏑgList).pop();
    if (gp == nil) {
        return default!;
    }
    pp.gFree.n--;
    if ((~gp).stack.lo != 0 && (~gp).stack.hi - (~gp).stack.lo != (uintptr)startingStackSize) {
        // Deallocate old stack. We kept it in gfput because it was the
        // right size when the goroutine was put on the free list, but
        // the right size has changed since then.
        var gpʗ1 = gp;
        systemstack(() => {
            stackfree((~gpʗ1).stack);
            gpʗ1.Value.stack.lo = 0;
            gpʗ1.Value.stack.hi = 0;
            gpʗ1.Value.stackguard0 = 0;
        });
    }
    if ((~gp).stack.lo == 0){
        // Stack was deallocated in gfput or just above. Allocate a new one.
        var gpʗ2 = gp;
        systemstack(() => {
            gpʗ2.Value.stack = @stackalloc(startingStackSize);
        });
        gp.Value.stackguard0 = (~gp).stack.lo + (uintptr)stackGuard;
    } else {
        if (raceenabled) {
            racemalloc((@unsafe.Pointer)(~gp).stack.lo, (~gp).stack.hi - (~gp).stack.lo);
        }
        if (msanenabled) {
            msanmalloc((@unsafe.Pointer)(~gp).stack.lo, (~gp).stack.hi - (~gp).stack.lo);
        }
        if (asanenabled) {
            asanunpoison((@unsafe.Pointer)(~gp).stack.lo, (~gp).stack.hi - (~gp).stack.lo);
        }
    }
    return gp;
}

// Purge all cached G's from gfree list to the global list.
internal static void gfpurge(ж<Δp> Ꮡpp) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    int32 inc = default!;
    gQueue stackQ = default!;
    gQueue noStackQ = default!;
    while (!Ꮡpp.of(runtime_package.Δp.ᏑgFree).of(p_gFree.ᏑgList).empty()) {
        var gp = Ꮡpp.of(runtime_package.Δp.ᏑgFree).of(p_gFree.ᏑgList).pop();
        pp.gFree.n--;
        if ((~gp).stack.lo == 0){
            noStackQ.push(gp);
        } else {
            stackQ.push(gp);
        }
        inc++;
    }
    @lock(Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡlock));
    Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.ᏑnoStack).pushAll(noStackQ);
    Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡstack).pushAll(stackQ);
    sched.gFree.n += inc;
    unlock(Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡlock));
}

// Breakpoint executes a breakpoint trap.
public static void Breakpoint() {
    breakpoint();
}

// dolockOSThread is called by LockOSThread and lockOSThread below
// after they modify m.locked. Do not allow preemption during this call,
// or else the m might be different in this function than in the caller.
//
//go:nosplit
internal static void dolockOSThread() {
    if (GOARCH == "wasm"u8) {
        return; // no threads on wasm yet
    }
    var gp = getg();
    (~gp).m.of(m.Ꮡlockedg).set(gp);
    gp.of(g.Ꮡlockedm).set((~gp).m);
}

// go2cs generated this placeholder — func LockOSThread is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func lockOSThread is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// dounlockOSThread is called by UnlockOSThread and unlockOSThread below
// after they update m->locked. Do not allow preemption during this call,
// or else the m might be in different in this function than in the caller.
//
//go:nosplit
internal static void dounlockOSThread() {
    if (GOARCH == "wasm"u8) {
        return; // no threads on wasm yet
    }
    var gp = getg();
    if ((~(~gp).m).lockedInt != 0 || (~(~gp).m).lockedExt != 0) {
        return;
    }
    gp.Value.m.Value.lockedg = 0;
    gp.Value.lockedm = 0;
}

// go2cs generated this placeholder — func UnlockOSThread is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func unlockOSThread is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeInternalErrorˢ2 = "runtime: internal error: misuse of lockOSThread/unlockOSThread"u8;

internal static void badunlockosthread() {
    @throw(runtimeInternalErrorˢ2);
}

// go2cs generated this placeholder — func gcount is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static int32 mcount() {
    return (int32)(sched.mnext - sched.nmfreed);
}


[GoType("dyn")] partial struct profᴛ1 {
    internal atomic.Uint32 signalLock;
    // Must hold signalLock to write. Reads may be lock-free, but
    // signalLock should be taken to synchronize with changes.
    internal atomic.Int32 hz;
}
internal static ж<profᴛ1> Ꮡprof = new StandardBox<profᴛ1>(default(profᴛ1));
internal static ref profᴛ1 prof => ref Ꮡprof.Value;

internal static void _System() {
    _System();
}

internal static void _ExternalCode() {
    _ExternalCode();
}

internal static void _LostExternalCode() {
    _LostExternalCode();
}

internal static void _GC() {
    _GC();
}

internal static void _LostSIGPROFDuringAtomic64() {
    _LostSIGPROFDuringAtomic64();
}

internal static void _LostContendedRuntimeLock() {
    _LostContendedRuntimeLock();
}

internal static void _VDSO() {
    _VDSO();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string internalRuntimeAtomicˢ = "internal/runtime/atomic"u8;

// Called if we receive a SIGPROF signal.
// Called by the signal handler, may run during STW.
//
//go:nowritebarrierrec
internal static void sigprof(uintptr pc, uintptr sp, uintptr lr, ж<g> Ꮡgp, ж<m> Ꮡmp) {
    ref var gp = ref Ꮡgp.DerefOrNull();
    ref var mp = ref Ꮡmp.DerefOrNull();

    if (Ꮡprof.of(profᴛ1.Ꮡhz).Load() == 0) {
        return;
    }
    // If mp.profilehz is 0, then profiling is not enabled for this thread.
    // We must check this to avoid a deadlock between setcpuprofilerate
    // and the call to cpuprof.add, below.
    if (Ꮡmp != nil && mp.profilehz == 0) {
        return;
    }
    // On mips{,le}/arm, 64bit atomics are emulated with spinlocks, in
    // internal/runtime/atomic. If SIGPROF arrives while the program is inside
    // the critical section, it creates a deadlock (when writing the sample).
    // As a workaround, create a counter of SIGPROFs while in critical section
    // to store the count, and pass it to sigprof.add() later when SIGPROF is
    // received from somewhere else (with _LostSIGPROFDuringAtomic64 as pc).
    if (GOARCH == "mips"u8 || GOARCH == "mipsle"u8 || GOARCH == "arm"u8) {
        {
            var f = findfunc(pc); if (f.valid()) {
                if (stringslite.HasPrefix(funcname(f), internalRuntimeAtomicˢ)) {
                    cpuprof.lostAtomic++;
                    return;
                }
            }
        }
        if (GOARCH == "arm"u8 && goarm < 7 && GOOS == "linux"u8 && (uintptr)(pc & (uintptr)0xffff0000U) == 0xffff0000U) {
            // internal/runtime/atomic functions call into kernel
            // helpers on arm < 7. See
            // internal/runtime/atomic/sys_linux_arm.s.
            cpuprof.lostAtomic++;
            return;
        }
    }
    // Profiling runs concurrently with GC, so it must not allocate.
    // Set a trap in case the code does allocate.
    // Note that on windows, one thread takes profiles of all the
    // other threads, so mp is usually not getg().m.
    // In fact mp may not even be stopped.
    // See golang.org/issue/17165.
    getg().Value.m.Value.mallocing++;
    ref var u = ref heap(new unwinder(), out var Ꮡu);
    array<uintptr> stk = new(64); /* maxCPUProfStack */
    nint n = 0;
    if (mp.ncgo > 0 && mp.curg != nil && (~mp.curg).syscallpc != 0 && (~mp.curg).syscallsp != 0){
        nint cgoOff = 0;
        // Check cgoCallersUse to make sure that we are not
        // interrupting other code that is fiddling with
        // cgoCallers.  We are running in a signal handler
        // with all signals blocked, so we don't have to worry
        // about any other code interrupting us.
        if (Ꮡmp.of(m.ᏑcgoCallersUse).Load() == 0 && mp.cgoCallers != nil && mp.cgoCallers.Value[0] != 0) {
            while (cgoOff < 32 && mp.cgoCallers.Value[cgoOff] != 0) {
                cgoOff++;
            }
            n += copy(stk[..], (~mp.cgoCallers).Value[..(int)(cgoOff)]);
            mp.cgoCallers.Value[0] = 0;
        }
        // Collect Go stack that leads to the cgo call.
        Ꮡu.initAt((~mp.curg).syscallpc, (~mp.curg).syscallsp, 0, mp.curg, unwindSilentErrors);
    } else 
    if (usesLibcall() && mp.libcallg != 0 && mp.libcallpc != 0 && mp.libcallsp != 0){
        // Libcall, i.e. runtime syscall on windows.
        // Collect Go stack that leads to the call.
        Ꮡu.initAt(mp.libcallpc, mp.libcallsp, 0, mp.libcallg.ptr(), unwindSilentErrors);
    } else 
    if (Ꮡmp != nil && mp.vdsoSP != 0){
        // VDSO call, e.g. nanotime1 on Linux.
        // Collect Go stack that leads to the call.
        Ꮡu.initAt(mp.vdsoPC, mp.vdsoSP, 0, Ꮡgp, (unwindFlags)(unwindSilentErrors | unwindJumpStack));
    } else {
        Ꮡu.initAt(pc, sp, lr, Ꮡgp, (unwindFlags)((unwindFlags)(unwindSilentErrors | unwindTrap) | unwindJumpStack));
    }
    n += tracebackPCs(Ꮡu, 0, stk[(int)(n)..]);
    if (n <= 0) {
        // Normal traceback is impossible or has failed.
        // Account it against abstract "System" or "GC".
        n = 2;
        if (inVDSOPage(pc)){
            pc = abi.FuncPCABIInternal(_VDSO) + (uintptr)sys.PCQuantum;
        } else 
        if (pc > firstmoduledata.etext) {
            // "ExternalCode" is better than "etext".
            pc = abi.FuncPCABIInternal(_ExternalCode) + (uintptr)sys.PCQuantum;
        }
        stk[0] = pc;
        if (mp.preemptoff != ""u8){
            stk[1] = abi.FuncPCABIInternal(_GC) + (uintptr)sys.PCQuantum;
        } else {
            stk[1] = abi.FuncPCABIInternal(_System) + (uintptr)sys.PCQuantum;
        }
    }
    if (Ꮡprof.of(profᴛ1.Ꮡhz).Load() != 0) {
        // Note: it can happen on Windows that we interrupted a system thread
        // with no g, so gp could nil. The other nil checks are done out of
        // caution, but not expected to be nil in practice.
        ж<@unsafe.Pointer> tagPtr = default!;
        if (Ꮡgp != nil && gp.m != nil && (~gp.m).curg != nil) {
            tagPtr = (~gp.m).curg.of(g.Ꮡlabels);
        }
        cpuprof.add(tagPtr, stk[..(int)(n)]);
        var gprof = Ꮡgp;
        ж<m> mpΔ1 = default!;
        ж<Δp> pp = default!;
        if (Ꮡgp != nil && gp.m != nil) {
            if ((~gp.m).curg != nil) {
                gprof = gp.m.Value.curg;
            }
            mpΔ1 = gp.m;
            pp = (~gp.m).p.ptr();
        }
        traceCPUSample(gprof, mpΔ1, pp, stk[..(int)(n)]);
    }
    getg().Value.m.Value.mallocing--;
}

// setcpuprofilerate sets the CPU profiling rate to hz times per second.
// If hz <= 0, setcpuprofilerate turns off CPU profiling.
internal static void setcpuprofilerate(int32 hz) {
    // Force sane arguments.
    if (hz < 0) {
        hz = 0;
    }
    // Disable preemption, otherwise we can be rescheduled to another thread
    // that has profiling enabled.
    var gp = getg();
    gp.Value.m.Value.locks++;
    // Stop profiler on this thread so that it is safe to lock prof.
    // if a profiling signal came in while we had prof locked,
    // it would deadlock.
    setThreadCPUProfiler(0);
    while (!Ꮡprof.of(profᴛ1.ᏑsignalLock).CompareAndSwap(0, 1)) {
        osyield();
    }
    if (Ꮡprof.of(profᴛ1.Ꮡhz).Load() != hz) {
        setProcessCPUProfiler(hz);
        Ꮡprof.of(profᴛ1.Ꮡhz).Store(hz);
    }
    Ꮡprof.of(profᴛ1.ᏑsignalLock).Store(0);
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    sched.profilehz = hz;
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    if (hz != 0) {
        setThreadCPUProfiler(hz);
    }
    gp.Value.m.Value.locks--;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingMcacheˢ = "missing mcache?"u8;

// init initializes pp, which may be a freshly allocated p or a
// previously destroyed p, and transitions it to status _Pgcstop.
internal static void init(this ж<Δp> Ꮡpp, int32 id) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    pp.id = id;
    pp.status = _Pgcstop;
    pp.sudogcache = pp.sudogbuf[..0];
    pp.deferpool = pp.deferpoolbuf[..0];
    pp.wbBuf.reset();
    if (pp.mcache == nil) {
        if (id == 0){
            if (mcache0 == nil) {
                @throw(missingMcacheˢ);
            }
            // Use the bootstrap mcache0. Only one P will get
            // mcache0: the one with ID 0.
            pp.mcache = mcache0;
        } else {
            pp.mcache = allocmcache();
        }
    }
    if (raceenabled && pp.raceprocctx == 0) {
        if (id == 0){
            pp.raceprocctx = raceprocctx0;
            raceprocctx0 = 0; // bootstrap
        } else {
            pp.raceprocctx = raceproccreate();
        }
    }
    lockInit(ref nonnil(ref pp).timers.mu, lockRankTimers);
    // This P may get timers when it starts running. Set the mask here
    // since the P may not go through pidleget (notably P 0 on startup).
    timerpMask.set(id);
    // Similarly, we may not go through pidleget before this P starts
    // running if it is P 0 on startup.
    idlepMask.clear(id);
}

// destroy releases all of the resources associated with pp and
// transitions it to status _Pdead.
//
// sched.lock must be held and the world must be stopped.
internal static void destroy(this ж<Δp> Ꮡpp) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    assertWorldStopped();
    // Move all runnable goroutines to the global queue
    while (pp.runqhead != pp.runqtail) {
        // Pop from tail of local queue
        pp.runqtail--;
        var gp = pp.runq[(nint)(pp.runqtail % (uint32)len(pp.runq))].ptr();
        // Push onto head of global queue
        globrunqputhead(gp);
    }
    if (pp.runnext != 0) {
        globrunqputhead(pp.runnext.ptr());
        pp.runnext = 0;
    }
    // Move all timers to the local P.
    (~(~getg()).m).p.ptr().of(runtime_package.Δp.Ꮡtimers).take(Ꮡpp.of(runtime_package.Δp.Ꮡtimers));
    // Flush p's write barrier buffer.
    if (gcphase != _GCoff) {
        wbBufFlush1(Ꮡpp);
        pp.gcw.dispose();
    }
    foreach (var (i, _) in pp.sudogbuf) {
        pp.sudogbuf[i] = default!;
    }
    pp.sudogcache = pp.sudogbuf[..0];
    pp.pinnerCache = default!;
    foreach (var (j, _) in pp.deferpoolbuf) {
        pp.deferpoolbuf[j] = default!;
    }
    pp.deferpool = pp.deferpoolbuf[..0];
    systemstack(() => {
        for (nint i = 0; i < Ꮡpp.Value.mspancache.len; i++) {
            // Safe to call since the world is stopped.
            Ꮡmheap_.of(mheap.Ꮡspanalloc).free(@unsafe.Pointer.FromPinnedBox(Ꮡpp.Value.mspancache.buf[i]));
        }
        Ꮡpp.Value.mspancache.len = 0;
        @lock(Ꮡmheap_.of(mheap.Ꮡlock));
        Ꮡpp.Value.pcache.flush(Ꮡmheap_.of(mheap.Ꮡpages));
        unlock(Ꮡmheap_.of(mheap.Ꮡlock));
    });
    freemcache(pp.mcache);
    pp.mcache = default!;
    gfpurge(Ꮡpp);
    if (raceenabled) {
        if (pp.timers.raceCtx != 0) {
            // The race detector code uses a callback to fetch
            // the proc context, so arrange for that callback
            // to see the right thing.
            // This hack only works because we are the only
            // thread running.
            var mp = getg().Value.m;
            var phold = (~mp).p.ptr();
            mp.of(m.Ꮡp).set(Ꮡpp);
            racectxend(pp.timers.raceCtx);
            pp.timers.raceCtx = 0;
            mp.of(m.Ꮡp).set(phold);
        }
        raceprocdestroy(pp.raceprocctx);
        pp.raceprocctx = 0;
    }
    pp.gcAssistTime = 0;
    pp.status = _Pdead;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string procresizeInvalidArgˢ = "procresize: invalid arg"u8;

// Change number of processors.
//
// sched.lock must be held, and the world must be stopped.
//
// gcworkbufs must not be being modified by either the GC or the write barrier
// code, so the GC must not be running if the number of Ps actually changes.
//
// Returns list of Ps with local work, they need to be scheduled by the caller.
internal static ж<Δp> procresize(int32 nprocs) {
    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    assertWorldStopped();
    var old = gomaxprocs;
    if (old < 0 || nprocs <= 0) {
        @throw(procresizeInvalidArgˢ);
    }
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.Gomaxprocs(nprocs);
        traceRelease(Δtrace);
    }
    // update statistics
    var now = nanotime();
    if (sched.procresizetime != 0) {
        sched.totaltime += (int64)old * (now - sched.procresizetime);
    }
    sched.procresizetime = now;
    var maskWords = (nprocs + 31) / 32;
    // Grow allp if necessary.
    if (nprocs > (int32)len(allp)) {
        // Synchronize with retake, which could be running
        // concurrently since it doesn't run on a P.
        @lock(ᏑallpLock);
        if (nprocs <= (int32)cap(allp)){
            allp = allp[..(int)(nprocs)];
        } else {
            var nallp = new slice<ж<Δp>>(nprocs);
            // Copy everything up to allp's cap so we
            // never lose old allocated Ps.
            copy(nallp, allp[..(int)(cap(allp))]);
            allp = nallp;
        }
        if (maskWords <= (int32)cap(idlepMask)){
            idlepMask = idlepMask[..(int)(maskWords)];
            timerpMask = timerpMask[..(int)(maskWords)];
        } else {
            var nidlepMask = new slice<uint32>(maskWords);
            // No need to copy beyond len, old Ps are irrelevant.
            copy(nidlepMask, idlepMask);
            idlepMask = nidlepMask;
            var ntimerpMask = new slice<uint32>(maskWords);
            copy(ntimerpMask, timerpMask);
            timerpMask = ntimerpMask;
        }
        unlock(ᏑallpLock);
    }
    // initialize new P's
    for (var i = old; i < nprocs; i++) {
        var pp = allp[i];
        if (pp == nil) {
            pp = @new<Δp>();
        }
        pp.init(i);
        atomicstorep(@unsafe.Pointer.FromBox(Ꮡ(allp, i)), @unsafe.Pointer.FromPinnedBox(pp));
    }
    var gp = getg();
    if ((~(~gp).m).p != 0 && (~(~(~gp).m).p.ptr()).id < nprocs){
        // continue to use the current P
        (~(~gp).m).p.ptr().Value.status = _Prunning;
        (~(~(~gp).m).p.ptr()).mcache.prepareForSweep();
    } else {
        // release the current P and acquire allp[0].
        //
        // We must do this before destroying our current P
        // because p.destroy itself has write barriers, so we
        // need to do that from a valid P.
        if ((~(~gp).m).p != 0) {
            var traceΔ1 = traceAcquire();
            if (traceΔ1.ok()) {
                // Pretend that we were descheduled
                // and then scheduled again to keep
                // the trace consistent.
                traceΔ1.GoSched();
                traceΔ1.ProcStop((~(~gp).m).p.ptr());
                traceRelease(traceΔ1);
            }
            (~(~gp).m).p.ptr().Value.m = 0;
        }
        gp.Value.m.Value.p = 0;
        var pp = allp[0];
        pp.Value.m = 0;
        pp.Value.status = _Pidle;
        acquirep(pp);
        var traceΔ2 = traceAcquire();
        if (traceΔ2.ok()) {
            traceΔ2.GoStart();
            traceRelease(traceΔ2);
        }
    }
    // g.m.p is now set, so we no longer need mcache0 for bootstrapping.
    mcache0 = default!;
    // release resources from unused P's
    for (var i = nprocs; i < old; i++) {
        var pp = allp[i];
        pp.destroy();
    }
    // can't free P itself because it can be referenced by an M in syscall
    // Trim allp.
    if ((int32)len(allp) != nprocs) {
        @lock(ᏑallpLock);
        allp = allp[..(int)(nprocs)];
        idlepMask = idlepMask[..(int)(maskWords)];
        timerpMask = timerpMask[..(int)(maskWords)];
        unlock(ᏑallpLock);
    }
    ж<Δp> runnablePs = default!;
    for (var i = nprocs - 1; i >= 0; i--) {
        var pp = allp[i];
        if ((~(~gp).m).p.ptr() == pp) {
            continue;
        }
        pp.Value.status = _Pidle;
        if (runqempty(pp)){
            pidleput(pp, now);
        } else {
            pp.of(runtime_package.Δp.Ꮡm).set(mget());
            pp.of(runtime_package.Δp.Ꮡlink).set(runnablePs);
            runnablePs = pp;
        }
    }
    stealOrder.reset((uint32)nprocs);
    ж<int32> int32p = Ꮡgomaxprocs;          // make compiler check that gomaxprocs is an int32
    atomic.Store(int32p.Reinterpret<int32, uint32>(), (uint32)nprocs);
    if (old != nprocs) {
        // Notify the limiter that the amount of procs has changed.
        ᏑgcCPULimiter.resetCapacity(now, nprocs);
    }
    return runnablePs;
}

// Associate p and the current m.
//
// This function is allowed to have write barriers even if the caller
// isn't because it immediately acquires pp.
//
//go:yeswritebarrierrec
internal static void acquirep(ж<Δp> Ꮡpp) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    // Do the part that isn't allowed to have write barriers.
    wirep(Ꮡpp);
    // Have p; write barriers now allowed.
    // Perform deferred mcache flush before this P can allocate
    // from a potentially stale mcache.
    pp.mcache.prepareForSweep();
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.ProcStart();
        traceRelease(Δtrace);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wirepAlreadyInGoˢ = "wirep: already in go"u8;
internal static readonly @string wirepInvalidPStateˢ = "wirep: invalid p state"u8;

// wirep is the first step of acquirep, which actually associates the
// current M to pp. This is broken out so we can disallow write
// barriers for this part, since we don't yet have a P.
//
//go:nowritebarrierrec
//go:nosplit
internal static void wirep(ж<Δp> Ꮡpp) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    var gp = getg();
    if ((~(~gp).m).p != 0) {
        // Call on the systemstack to avoid a nosplit overflow build failure
        // on some platforms when built with -N -l. See #64113.
        systemstack(() => {
            @throw(wirepAlreadyInGoˢ);
        });
    }
    if (pp.m != 0 || pp.status != _Pidle) {
        // Call on the systemstack to avoid a nosplit overflow build failure
        // on some platforms when built with -N -l. See #64113.
        systemstack(() => {
            var id = (int64)0;
            if (Ꮡpp.Value.m != 0) {
                id = Ꮡpp.Value.m.ptr().Value.id;
            }
            print((@string)"wirep: p->m="u8, Ꮡpp.Value.m, (@string)"("u8, id, (@string)") p->status="u8, Ꮡpp.Value.status, (@string)"\n"u8);
            @throw(wirepInvalidPStateˢ);
        });
    }
    (~gp).m.of(m.Ꮡp).set(Ꮡpp);
    pp.m.set((~gp).m);
    pp.status = _Prunning;
}

// Disassociate p and the current m.
internal static ж<Δp> releasep() {
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.ProcStop((~(~getg()).m).p.ptr());
        traceRelease(Δtrace);
    }
    return releasepNoTrace();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string releasepInvalidArgˢ = "releasep: invalid arg"u8;
internal static readonly @string releasepInvalidPStateˢ = "releasep: invalid p state"u8;

// Disassociate p and the current m without tracing an event.
internal static ж<Δp> releasepNoTrace() {
    var gp = getg();
    if ((~(~gp).m).p == 0) {
        @throw(releasepInvalidArgˢ);
    }
    var pp = (~(~gp).m).p.ptr();
    if ((~pp).m.ptr() != (~gp).m || (~pp).status != _Prunning) {
        print((@string)"releasep: m="u8, (~gp).m.OrTypedNil(), (@string)" m->p="u8, (~(~gp).m).p.ptr().OrTypedNil(), (@string)" p->m="u8, ((Δhex)(uint64)(uintptr)(~pp).m), (@string)" p->status="u8, (~pp).status, (@string)"\n"u8);
        @throw(releasepInvalidPStateˢ);
    }
    gp.Value.m.Value.p = 0;
    pp.Value.m = 0;
    pp.Value.status = _Pidle;
    return pp;
}

internal static void incidlelocked(int32 v) {
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    sched.nmidlelocked += v;
    if (v > 0) {
        checkdead();
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string checkdeadInconsistentˢ = "checkdead: inconsistent counts"u8;
internal static readonly @string checkdeadRunnableGˢ = "checkdead: runnable g"u8;
internal static readonly @string noGoroutinesMainCalledˢ = "no goroutines (main called runtime.Goexit) - deadlock!"u8;
internal static readonly @string checkdeadNoPForTimerˢ = "checkdead: no p for timer"u8;
internal static readonly @string checkdeadNoMForTimerˢ = "checkdead: no m for timer"u8;
internal static readonly @string allGoroutinesAreAsleepˢ = "all goroutines are asleep - deadlock!"u8;

// Check for deadlock situation.
// The check is based on number of running M's, if 0 -> deadlock.
// sched.lock must be held.
internal static void checkdead() {
    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    // For -buildmode=c-shared or -buildmode=c-archive it's OK if
    // there are no running goroutines. The calling program is
    // assumed to be running.
    if (islibrary || isarchive) {
        return;
    }
    // If we are dying because of a signal caught on an already idle thread,
    // freezetheworld will cause all running threads to block.
    // And runtime will essentially enter into deadlock state,
    // except that there is a thread that will call exit soon.
    if (Ꮡpanicking.Load() > 0) {
        return;
    }
    // If we are not running under cgo, but we have an extra M then account
    // for it. (It is possible to have an extra M on Windows without cgo to
    // accommodate callbacks created by syscall.NewCallback. See issue #6751
    // for details.)
    int32 run0 = default!;
    if (!iscgo && cgoHasExtraM && ᏑextraMLength.Load() > 0) {
        run0 = 1;
    }
    var run = mcount() - sched.nmidle - sched.nmidlelocked - sched.nmsys;
    if (run > run0) {
        return;
    }
    if (run < 0) {
        print((@string)"runtime: checkdead: nmidle="u8, sched.nmidle, (@string)" nmidlelocked="u8, sched.nmidlelocked, (@string)" mcount="u8, mcount(), (@string)" nmsys="u8, sched.nmsys, (@string)"\n"u8);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        @throw(checkdeadInconsistentˢ);
    }
    nint grunning = 0;
    forEachG((ж<g> gp) => {
        if (isSystemGoroutine(ref (gp).DerefOrNull(), false)) {
            return;
        }
        var s = readgstatus(gp);
        var exprᴛ1 = (uint32)(s & ~(uint32)_Gscan);
        if (exprᴛ1 == _Gwaiting || exprᴛ1 == _Gpreempted) {
            grunning++;
        }
        else if (exprᴛ1 == _Grunnable || exprᴛ1 == _Grunning || exprᴛ1 == _Gsyscall) {
            print((@string)"runtime: checkdead: find g "u8, (~gp).goid, (@string)" in status "u8, s, (@string)"\n"u8);
            unlock(Ꮡsched.of(schedt.Ꮡlock));
            @throw(checkdeadRunnableGˢ);
        }

    });
    if (grunning == 0) {
        // possible if main goroutine calls runtime·Goexit()
        unlock(Ꮡsched.of(schedt.Ꮡlock)); // unlock so that GODEBUG=scheddetail=1 doesn't hang
        fatal(noGoroutinesMainCalledˢ);
    }
    // Maybe jump time forward for playground.
    if (faketime != 0) {
        {
            var when = timeSleepUntil(); if (when < maxWhen) {
                faketime = when;
                // Start an M to steal the timer.
                var (pp, _) = pidleget(faketime);
                if (pp == nil) {
                    // There should always be a free P since
                    // nothing is running.
                    unlock(Ꮡsched.of(schedt.Ꮡlock));
                    @throw(checkdeadNoPForTimerˢ);
                }
                var mp = mget();
                if (mp == nil) {
                    // There should always be a free M since
                    // nothing is running.
                    unlock(Ꮡsched.of(schedt.Ꮡlock));
                    @throw(checkdeadNoMForTimerˢ);
                }
                // M must be spinning to steal. We set this to be
                // explicit, but since this is the only M it would
                // become spinning on its own anyways.
                Ꮡsched.of(schedt.Ꮡnmspinning).Add(1);
                mp.Value.spinning = true;
                mp.of(m.Ꮡnextp).set(pp);
                notewakeup(mp.of(m.Ꮡpark));
                return;
            }
        }
    }
    // There are no goroutines running, so we can look at the P's.
    foreach (var (_, pp) in allp) {
        if (len((~pp).timers.heap) > 0) {
            return;
        }
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock)); // unlock so that GODEBUG=scheddetail=1 doesn't hang
    fatal(allGoroutinesAreAsleepˢ);
}

// forcegcperiod is the maximum time in nanoseconds between garbage
// collections. If we go this long without a garbage collection, one
// is forced to run.
//
// This is a variable for testing purposes. It normally doesn't change.
internal static ж<int64> Ꮡforcegcperiod = new StandardBox<int64>(120000000000L);
internal static ref int64 forcegcperiod => ref Ꮡforcegcperiod.Value;

// needSysmonWorkaround is true if the workaround for
// golang.org/issue/42515 is needed on NetBSD.
internal static bool needSysmonWorkaround = false;

// haveSysmon indicates whether there is sysmon thread support.
//
// No threads on wasm yet, so no sysmon.
internal const bool haveSysmon = /* GOARCH != "wasm" */ true;

// Always runs without a P, so write barriers are not allowed.
//
//go:nowritebarrierrec
internal static void sysmon() {
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    sched.nmsys++;
    checkdead();
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    var lasttrace = (int64)0;
    nint idle = 0; // how many cycles in succession we had not wokeup somebody
    var delay = (uint32)0;
    while (ᐧ) {
        if (idle == 0){
            // start with 20us sleep...
            delay = 20;
        } else 
        if (idle > 50) {
            // start doubling the sleep after 1ms...
            delay *= 2;
        }
        if (delay > 10 * 1000) {
            // up to 10ms
            delay = 10 * 1000;
        }
        usleep(delay);
        // sysmon should not enter deep sleep if schedtrace is enabled so that
        // it can print that information at the right time.
        //
        // It should also not enter deep sleep if there are any active P's so
        // that it can retake P's from syscalls, preempt long running G's, and
        // poll the network if all P's are busy for long stretches.
        //
        // It should wakeup from deep sleep if any P's become active either due
        // to exiting a syscall or waking up due to a timer expiring so that it
        // can resume performing those duties. If it wakes from a syscall it
        // resets idle and delay as a bet that since it had retaken a P from a
        // syscall before, it may need to do it again shortly after the
        // application starts work again. It does not reset idle when waking
        // from a timer to avoid adding system load to applications that spend
        // most of their time sleeping.
        var now = nanotime();
        if (debug.schedtrace <= 0 && (Ꮡsched.of(schedt.Ꮡgcwaiting).Load() || Ꮡsched.of(schedt.Ꮡnpidle).Load() == gomaxprocs)) {
            @lock(Ꮡsched.of(schedt.Ꮡlock));
            if (Ꮡsched.of(schedt.Ꮡgcwaiting).Load() || Ꮡsched.of(schedt.Ꮡnpidle).Load() == gomaxprocs) {
                var syscallWake = false;
                var next = timeSleepUntil();
                if (next > now) {
                    Ꮡsched.of(schedt.Ꮡsysmonwait).Store(true);
                    unlock(Ꮡsched.of(schedt.Ꮡlock));
                    // Make wake-up period small enough
                    // for the sampling to be correct.
                    var sleep = forcegcperiod / 2;
                    if (next - now < sleep) {
                        sleep = next - now;
                    }
                    var shouldRelax = sleep >= osRelaxMinNS;
                    if (shouldRelax) {
                        osRelax(true);
                    }
                    syscallWake = notetsleep(Ꮡsched.of(schedt.Ꮡsysmonnote), sleep);
                    if (shouldRelax) {
                        osRelax(false);
                    }
                    @lock(Ꮡsched.of(schedt.Ꮡlock));
                    Ꮡsched.of(schedt.Ꮡsysmonwait).Store(false);
                    noteclear(Ꮡsched.of(schedt.Ꮡsysmonnote));
                }
                if (syscallWake) {
                    idle = 0;
                    delay = 20;
                }
            }
            unlock(Ꮡsched.of(schedt.Ꮡlock));
        }
        @lock(Ꮡsched.of(schedt.Ꮡsysmonlock));
        // Update now in case we blocked on sysmonnote or spent a long time
        // blocked on schedlock or sysmonlock above.
        now = nanotime();
        // trigger libc interceptors if needed
        if (cgo_yield.Value != nil) {
            asmcgocall(cgo_yield.Value, nil);
        }
        // poll network if not polled for more than 10ms
        var lastpoll = Ꮡsched.of(schedt.Ꮡlastpoll).Load();
        if (netpollinited() && lastpoll != 0 && lastpoll + 10 * 1000 * 1000 < now) {
            Ꮡsched.of(schedt.Ꮡlastpoll).CompareAndSwap(lastpoll, now);
            ref var list = ref heap<gList>(out var Ꮡlist);
            (list, var delta) = netpoll(0); // non-blocking - returns list of goroutines
            if (!list.empty()) {
                // Need to decrement number of idle locked M's
                // (pretending that one more is running) before injectglist.
                // Otherwise it can lead to the following situation:
                // injectglist grabs all P's but before it starts M's to run the P's,
                // another M returns from syscall, finishes running its G,
                // observes that there is no work to do and no other running M's
                // and reports deadlock.
                incidlelocked(-1);
                injectglist(Ꮡlist);
                incidlelocked(1);
                netpollAdjustWaiters(delta);
            }
        }
        if (GOOS == "netbsd"u8 && needSysmonWorkaround) {
            // netpoll is responsible for waiting for timer
            // expiration, so we typically don't have to worry
            // about starting an M to service timers. (Note that
            // sleep for timeSleepUntil above simply ensures sysmon
            // starts running again when that timer expiration may
            // cause Go code to run again).
            //
            // However, netbsd has a kernel bug that sometimes
            // misses netpollBreak wake-ups, which can lead to
            // unbounded delays servicing timers. If we detect this
            // overrun, then startm to get something to handle the
            // timer.
            //
            // See issue 42515 and
            // https://gnats.netbsd.org/cgi-bin/query-pr-single.pl?number=50094.
            {
                var next = timeSleepUntil(); if (next < now) {
                    startm(nil, false, false);
                }
            }
        }
        if (Ꮡscavenger.of(scavengerState.ᏑsysmonWake).Load() != 0) {
            // Kick the scavenger awake if someone requested it.
            Ꮡscavenger.wake();
        }
        // retake P's blocked in syscalls
        // and preempt long running G's
        if (retake(now) != 0){
            idle = 0;
        } else {
            idle++;
        }
        // check if we need to force a GC
        {
            var t = (new gcTrigger(kind: gcTriggerTime, now: now)); if (t.test() && Ꮡforcegc.of(forcegcstate.Ꮡidle).Load()) {
                @lock(Ꮡforcegc.of(forcegcstate.Ꮡlock));
                Ꮡforcegc.of(forcegcstate.Ꮡidle).Store(false);
                ref var list = ref heap(new gList(), out var Ꮡlist);
                list.push(forcegc.g);
                injectglist(Ꮡlist);
                unlock(Ꮡforcegc.of(forcegcstate.Ꮡlock));
            }
        }
        if (debug.schedtrace > 0 && lasttrace + (int64)debug.schedtrace * 1000000 <= now) {
            lasttrace = now;
            schedtrace(debug.scheddetail > 0);
        }
        unlock(Ꮡsched.of(schedt.Ꮡsysmonlock));
    }
}

[GoType] partial struct sysmontick {
    internal uint32 schedtick;
    internal uint32 syscalltick;
    internal int64 schedwhen;
    internal int64 syscallwhen;
}

// forcePreemptNS is the time slice given to a G before it is
// preempted.
internal static UntypedInt forcePreemptNS => /* 10 * 1000 * 1000 */ 10000000; // 10ms

internal static uint32 retake(int64 now) {
    nint n = 0;
    // Prevent allp slice changes. This lock will be completely
    // uncontended unless we're already stopping the world.
    @lock(ᏑallpLock);
    // We can't use a range loop over allp because we may
    // temporarily drop the allpLock. Hence, we need to re-fetch
    // allp each time around the loop.
    for (nint i = 0; i < len(allp); i++) {
        var pp = allp[i];
        if (pp == nil) {
            // This can happen if procresize has grown
            // allp but not yet created new Ps.
            continue;
        }
        var pd = pp.of(runtime_package.Δp.Ꮡsysmontick);
        var s = pp.Value.status;
        var sysretake = false;
        if (s == _Prunning || s == _Psyscall) {
            // Preempt G if it's running on the same schedtick for
            // too long. This could be from a single long-running
            // goroutine or a sequence of goroutines run via
            // runnext, which share a single schedtick time slice.
            var t = (int64)(~pp).schedtick;
            if ((int64)(~pd).schedtick != t){
                pd.Value.schedtick = (uint32)t;
                pd.Value.schedwhen = now;
            } else 
            if ((~pd).schedwhen + (int64)forcePreemptNS <= now) {
                preemptone(ref (pp).DerefOrNull());
                // In case of syscall, preemptone() doesn't
                // work, because there is no M wired to P.
                sysretake = true;
            }
        }
        if (s == _Psyscall) {
            // Retake P from syscall if it's there for more than 1 sysmon tick (at least 20us).
            var t = (int64)(~pp).syscalltick;
            if (!sysretake && (int64)(~pd).syscalltick != t) {
                pd.Value.syscalltick = (uint32)t;
                pd.Value.syscallwhen = now;
                continue;
            }
            // On the one hand we don't want to retake Ps if there is no other work to do,
            // but on the other hand we want to retake them eventually
            // because they can prevent the sysmon thread from deep sleep.
            if (runqempty(pp) && Ꮡsched.of(schedt.Ꮡnmspinning).Load() + Ꮡsched.of(schedt.Ꮡnpidle).Load() > 0 && (~pd).syscallwhen + 10 * 1000 * 1000 > now) {
                continue;
            }
            // Drop allpLock so we can take sched.lock.
            unlock(ᏑallpLock);
            // Need to decrement number of idle locked M's
            // (pretending that one more is running) before the CAS.
            // Otherwise the M from which we retake can exit the syscall,
            // increment nmidle and report deadlock.
            incidlelocked(-1);
            var Δtrace = traceAcquire();
            if (atomic.Cas(pp.of(runtime_package.Δp.Ꮡstatus), s, _Pidle)){
                if (Δtrace.ok()) {
                    Δtrace.ProcSteal(pp, false);
                    traceRelease(Δtrace);
                }
                n++;
                pp.Value.syscalltick++;
                handoffp(pp);
            } else 
            if (Δtrace.ok()) {
                traceRelease(Δtrace);
            }
            incidlelocked(1);
            @lock(ᏑallpLock);
        }
    }
    unlock(ᏑallpLock);
    return (uint32)n;
}

// Tell all goroutines that they have been preempted and they should stop.
// This function is purely best-effort. It can fail to inform a goroutine if a
// processor just started running it.
// No locks need to be held.
// Returns true if preemption request was issued to at least one goroutine.
internal static bool preemptall() {
    var res = false;
    foreach (var (_, pp) in allp) {
        if ((~pp).status != _Prunning) {
            continue;
        }
        if (preemptone(ref (pp).DerefOrNull())) {
            res = true;
        }
    }
    return res;
}

// Tell the goroutine running on processor P to stop.
// This function is purely best-effort. It can incorrectly fail to inform the
// goroutine. It can inform the wrong goroutine. Even if it informs the
// correct goroutine, that goroutine might ignore the request if it is
// simultaneously executing newstack.
// No lock needs to be held.
// Returns true if preemption request was issued.
// The actual preemption will happen at some point in the future
// and will be indicated by the gp->status no longer being
// Grunning
internal static bool preemptone(ref Δp pp) {
    var mp = pp.m.ptr();
    if (mp == nil || mp == (~getg()).m) {
        return false;
    }
    var gp = mp.Value.curg;
    if (gp == nil || gp == (~mp).g0) {
        return false;
    }
    gp.Value.preempt = true;
    // Every call in a goroutine checks for stack overflow by
    // comparing the current stack pointer to gp->stackguard0.
    // Setting gp->stackguard0 to StackPreempt folds
    // preemption into the normal stack overflow check.
    gp.Value.stackguard0 = stackPreempt;
    // Request an async preemption of this P.
    if (preemptMSupported && debug.asyncpreemptoff == 0) {
        pp.preempt = true;
        preemptM(mp);
    }
    return true;
}

internal static int64 starttime;

internal static void schedtrace(bool detailed) {
    var now = nanotime();
    if (starttime == 0) {
        starttime = now;
    }
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    print((@string)"SCHED "u8, (now - starttime) / 1000000, (@string)"ms: gomaxprocs="u8, gomaxprocs, (@string)" idleprocs="u8, Ꮡsched.of(schedt.Ꮡnpidle).Load(), (@string)" threads="u8, mcount(), (@string)" spinningthreads="u8, Ꮡsched.of(schedt.Ꮡnmspinning).Load(), (@string)" needspinning="u8, Ꮡsched.of(schedt.Ꮡneedspinning).Load(), (@string)" idlethreads="u8, sched.nmidle, (@string)" runqueue="u8, sched.runqsize);
    if (detailed) {
        print((@string)" gcwaiting="u8, Ꮡsched.of(schedt.Ꮡgcwaiting).Load(), (@string)" nmidlelocked="u8, sched.nmidlelocked, (@string)" stopwait="u8, sched.stopwait, (@string)" sysmonwait="u8, Ꮡsched.of(schedt.Ꮡsysmonwait).Load(), (@string)"\n"u8);
    }
    // We must be careful while reading data from P's, M's and G's.
    // Even if we hold schedlock, most data can be changed concurrently.
    // E.g. (p->m ? p->m->id : -1) can crash if p->m changes from non-nil to nil.
    foreach (var (i, pp) in allp) {
        var mp = (~pp).m.ptr();
        var h = atomic.Load(pp.of(runtime_package.Δp.Ꮡrunqhead));
        var t = atomic.Load(pp.of(runtime_package.Δp.Ꮡrunqtail));
        if (detailed){
            print((@string)"  P"u8, i, (@string)": status="u8, (~pp).status, (@string)" schedtick="u8, (~pp).schedtick, (@string)" syscalltick="u8, (~pp).syscalltick, (@string)" m="u8);
            if (mp != nil){
                print((~mp).id);
            } else {
                print((@string)"nil"u8);
            }
            print((@string)" runqsize="u8, t - h, (@string)" gfreecnt="u8, (~pp).gFree.n, (@string)" timerslen="u8, len((~pp).timers.heap), (@string)"\n"u8);
        } else {
            // In non-detailed mode format lengths of per-P run queues as:
            // [len1 len2 len3 len4]
            print((@string)" "u8);
            if (i == 0) {
                print((@string)"["u8);
            }
            print(t - h);
            if (i == len(allp) - 1) {
                print((@string)"]\n"u8);
            }
        }
    }
    if (!detailed) {
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        return;
    }
    for (var mp = allm; mp != nil; mp = mp.Value.alllink) {
        var pp = (~mp).p.ptr();
        print((@string)"  M"u8, (~mp).id, (@string)": p="u8);
        if (pp != nil){
            print((~pp).id);
        } else {
            print((@string)"nil"u8);
        }
        print((@string)" curg="u8);
        if ((~mp).curg != nil){
            print((~(~mp).curg).goid);
        } else {
            print((@string)"nil"u8);
        }
        print((@string)" mallocing="u8, (~mp).mallocing, (@string)" throwing="u8, (~mp).throwing, (@string)" preemptoff="u8, (~mp).preemptoff, (@string)" locks="u8, (~mp).locks, (@string)" dying="u8, (~mp).dying, (@string)" spinning="u8, (~mp).spinning, (@string)" blocked="u8, (~mp).blocked, (@string)" lockedg="u8);
        {
            var lockedg = (~mp).lockedg.ptr(); if (lockedg != nil){
                print((~lockedg).goid);
            } else {
                print((@string)"nil"u8);
            }
        }
        print((@string)"\n"u8);
    }
    forEachG((ж<g> gp) => {
        print((@string)"  G"u8, (~gp).goid, (@string)": status="u8, readgstatus(gp), (@string)"("u8, (~gp).waitreason.String(), (@string)") m="u8);
        if ((~gp).m != nil){
            print((~(~gp).m).id);
        } else {
            print((@string)"nil"u8);
        }
        print((@string)" lockedm="u8);
        {
            var lockedm = (~gp).lockedm.ptr(); if (lockedm != nil){
                print((~lockedm).id);
            } else {
                print((@string)"nil"u8);
            }
        }
        print((@string)"\n"u8);
    });
    unlock(Ꮡsched.of(schedt.Ꮡlock));
}

// schedEnableUser enables or disables the scheduling of user
// goroutines.
//
// This does not stop already running user goroutines, so the caller
// should first stop the world when disabling user goroutines.
internal static void schedEnableUser(bool enable) {
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    if (sched.disable.user == !enable) {
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        return;
    }
    sched.disable.user = !enable;
    if (enable){
        var n = sched.disable.n;
        sched.disable.n = 0;
        globrunqputbatch(ref sched.disable.runnable, n);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
        for (; n != 0 && Ꮡsched.of(schedt.Ꮡnpidle).Load() != 0; n--) {
            startm(nil, false, false);
        }
    } else {
        unlock(Ꮡsched.of(schedt.Ꮡlock));
    }
}

// schedEnabled reports whether gp should be scheduled. It returns
// false is scheduling of gp is disabled.
//
// sched.lock must be held.
internal static bool schedEnabled(ref g gp) {
    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    if (sched.disable.user) {
        return isSystemGoroutine(ref gp, true);
    }
    return true;
}

// Put mp on midle list.
// sched.lock must be held.
// May run during STW, so write barriers are not allowed.
//
//go:nowritebarrierrec
internal static void mput(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    mp.schedlink = sched.midle;
    Ꮡsched.of(schedt.Ꮡmidle).set(Ꮡmp);
    sched.nmidle++;
    checkdead();
}

// Try to get an m from midle list.
// sched.lock must be held.
// May run during STW, so write barriers are not allowed.
//
//go:nowritebarrierrec
internal static ж<m> mget() {
    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    var mp = sched.midle.ptr();
    if (mp != nil) {
        sched.midle = mp.Value.schedlink;
        sched.nmidle--;
    }
    return mp;
}

// Put gp on the global runnable queue.
// sched.lock must be held.
// May run during STW, so write barriers are not allowed.
//
//go:nowritebarrierrec
internal static void globrunqput(ж<g> Ꮡgp) {
    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    Ꮡsched.of(schedt.Ꮡrunq).pushBack(Ꮡgp);
    sched.runqsize++;
}

// Put gp at the head of the global runnable queue.
// sched.lock must be held.
// May run during STW, so write barriers are not allowed.
//
//go:nowritebarrierrec
internal static void globrunqputhead(ж<g> Ꮡgp) {
    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    Ꮡsched.of(schedt.Ꮡrunq).push(Ꮡgp);
    sched.runqsize++;
}

// Put a batch of runnable goroutines on the global runnable queue.
// This clears *batch.
// sched.lock must be held.
// May run during STW, so write barriers are not allowed.
//
//go:nowritebarrierrec
internal static void globrunqputbatch(ref gQueue batch, int32 n) {
    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    Ꮡsched.of(schedt.Ꮡrunq).pushBackAll(batch);
    sched.runqsize += n;
    batch = new gQueue(nil);
}

// Try get a batch of G's from the global runnable queue.
// sched.lock must be held.
internal static ж<g> globrunqget(ж<Δp> Ꮡpp, int32 max) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    if (sched.runqsize == 0) {
        return default!;
    }
    var n = sched.runqsize / gomaxprocs + 1;
    if (n > sched.runqsize) {
        n = sched.runqsize;
    }
    if (max > 0 && n > max) {
        n = max;
    }
    if (n > (int32)len(pp.runq) / 2) {
        n = (int32)len(pp.runq) / 2;
    }
    sched.runqsize -= n;
    var gp = Ꮡsched.of(schedt.Ꮡrunq).pop();
    n--;
    for (; n > 0; n--) {
        var gp1 = Ꮡsched.of(schedt.Ꮡrunq).pop();
        runqput(Ꮡpp, gp1, false);
    }
    return gp;
}

[GoType("[]uint32")] partial struct pMask;

// read returns true if P id's bit is set.
internal static bool read(this pMask Δp, uint32 id) {
    var word = id / 32;
    var mask = ((uint32)1 << (int)((id % 32)));
    return ((uint32)(atomic.Load(Ꮡ(Δp, (int)(word))) & mask)) != 0;
}

// set sets P id's bit.
internal static void set(this pMask Δp, int32 id) {
    var word = id / 32;
    var mask = ((uint32)1 << (int)((id % 32)));
    atomic.Or(Ꮡ(Δp, word), mask);
}

// clear clears P id's bit.
internal static void clear(this pMask Δp, int32 id) {
    var word = id / 32;
    var mask = ((uint32)1 << (int)((id % 32)));
    atomic.And(Ꮡ(Δp, word), ~mask);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pidleputPHasNonEmptyRunˢ = "pidleput: P has non-empty run queue"u8;
internal static readonly @string mustBeAbleToTrackIdleˢ = "must be able to track idle limiter event"u8;

// pidleput puts p on the _Pidle list. now must be a relatively recent call
// to nanotime or zero. Returns now or the current time if now was zero.
//
// This releases ownership of p. Once sched.lock is released it is no longer
// safe to use p.
//
// sched.lock must be held.
//
// May run during STW, so write barriers are not allowed.
//
//go:nowritebarrierrec
internal static int64 pidleput(ж<Δp> Ꮡpp, int64 now) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    if (!runqempty(Ꮡpp)) {
        @throw(pidleputPHasNonEmptyRunˢ);
    }
    if (now == 0) {
        now = nanotime();
    }
    if (Ꮡpp.of(runtime_package.Δp.Ꮡtimers).of(timers.Ꮡlen).Load() == 0) {
        timerpMask.clear(pp.id);
    }
    idlepMask.set(pp.id);
    pp.link = sched.pidle;
    Ꮡsched.of(schedt.Ꮡpidle).set(Ꮡpp);
    Ꮡsched.of(schedt.Ꮡnpidle).Add(1);
    if (!Ꮡpp.of(runtime_package.Δp.ᏑlimiterEvent).start(limiterEventIdle, now)) {
        @throw(mustBeAbleToTrackIdleˢ);
    }
    return now;
}

// pidleget tries to get a p from the _Pidle list, acquiring ownership.
//
// sched.lock must be held.
//
// May run during STW, so write barriers are not allowed.
//
//go:nowritebarrierrec
internal static (ж<Δp>, int64) pidleget(int64 now) {
    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    var pp = sched.pidle.ptr();
    if (pp != nil) {
        // Timer may get added at any time now.
        if (now == 0) {
            now = nanotime();
        }
        timerpMask.set((~pp).id);
        idlepMask.clear((~pp).id);
        sched.pidle = pp.Value.link;
        Ꮡsched.of(schedt.Ꮡnpidle).Add(-1);
        pp.of(runtime_package.Δp.ᏑlimiterEvent).stop(limiterEventIdle, now);
    }
    return (pp, now);
}

// pidlegetSpinning tries to get a p from the _Pidle list, acquiring ownership.
// This is called by spinning Ms (or callers than need a spinning M) that have
// found work. If no P is available, this must synchronized with non-spinning
// Ms that may be preparing to drop their P without discovering this work.
//
// sched.lock must be held.
//
// May run during STW, so write barriers are not allowed.
//
//go:nowritebarrierrec
internal static (ж<Δp>, int64) pidlegetSpinning(int64 now) {
    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    (var pp, now) = pidleget(now);
    if (pp == nil) {
        // See "Delicate dance" comment in findrunnable. We found work
        // that we cannot take, we must synchronize with non-spinning
        // Ms that may be preparing to drop their P.
        Ꮡsched.of(schedt.Ꮡneedspinning).Store(1);
        return (default!, now);
    }
    return (pp, now);
}

// runqempty reports whether pp has no Gs on its local run queue.
// It never returns true spuriously.
internal static bool runqempty(ж<Δp> Ꮡpp) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    // Defend against a race where 1) pp has G1 in runqnext but runqhead == runqtail,
    // 2) runqput on pp kicks G1 to the runq, 3) runqget on pp empties runqnext.
    // Simply observing that runqhead == runqtail and then observing that runqnext == nil
    // does not mean the queue is empty.
    while (ᐧ) {
        var head = atomic.Load(Ꮡpp.of(runtime_package.Δp.Ꮡrunqhead));
        var tail = atomic.Load(Ꮡpp.of(runtime_package.Δp.Ꮡrunqtail));
        var runnext = atomic.Loaduintptr(Ꮡpp.of(runtime_package.Δp.Ꮡrunnext).Reinterpret<Δguintptr, uintptr>());
        if (tail == atomic.Load(Ꮡpp.of(runtime_package.Δp.Ꮡrunqtail))) {
            return head == tail && runnext == 0;
        }
    }
}

// To shake out latent assumptions about scheduling order,
// we introduce some randomness into scheduling decisions
// when running with the race detector.
// The need for this was made obvious by changing the
// (deterministic) scheduling order in Go 1.5 and breaking
// many poorly-written tests.
// With the randomness here, as long as the tests pass
// consistently with -race, they shouldn't have latent scheduling
// assumptions.
internal const bool randomizeScheduler = /* raceenabled */ false;

// runqput tries to put g on the local runnable queue.
// If next is false, runqput adds g to the tail of the runnable queue.
// If next is true, runqput puts g in the pp.runnext slot.
// If the run queue is full, runnext puts g on the global queue.
// Executed only by the owner P.
internal static void runqput(ж<Δp> Ꮡpp, ж<g> Ꮡgp, bool next) {
    ref var pp = ref Ꮡpp.DerefOrNull();
    ref var gp = ref Ꮡgp.DerefOrNull();

    if (!haveSysmon && next) {
        // A runnext goroutine shares the same time slice as the
        // current goroutine (inheritTime from runqget). To prevent a
        // ping-pong pair of goroutines from starving all others, we
        // depend on sysmon to preempt "long-running goroutines". That
        // is, any set of goroutines sharing the same time slice.
        //
        // If there is no sysmon, we must avoid runnext entirely or
        // risk starvation.
        next = false;
    }
    if (randomizeScheduler && next && randn(2) == 0) {
        next = false;
    }
    if (next) {
retryNext:
        var oldnext = pp.runnext;
        if (!Ꮡpp.of(runtime_package.Δp.Ꮡrunnext).cas(oldnext, new Δguintptr(gp))) {
            goto retryNext;
        }
        if (oldnext == 0) {
            return;
        }
        // Kick the old runnext out to the regular run queue.
        Ꮡgp = oldnext.ptr(); gp = ref Ꮡgp.DerefOrNull();
    }
retry:
    var h = atomic.LoadAcq(Ꮡpp.of(runtime_package.Δp.Ꮡrunqhead)); // load-acquire, synchronize with consumers
    var t = pp.runqtail;
    if (t - h < (uint32)len(pp.runq)) {
        pp.runq[(nint)(t % (uint32)len(pp.runq))].set(Ꮡgp);
        atomic.StoreRel(Ꮡpp.of(runtime_package.Δp.Ꮡrunqtail), t + 1); // store-release, makes the item available for consumption
        return;
    }
    if (runqputslow(Ꮡpp, Ꮡgp, h, t)) {
        return;
    }
    // the queue is not full, now the put above must succeed
    goto retry;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runqputslowQueueIsNotˢ = "runqputslow: queue is not full"u8;

// Put g and a batch of work from local runnable queue on global queue.
// Executed only by the owner P.
internal static bool runqputslow(ж<Δp> Ꮡpp, ж<g> Ꮡgp, uint32 h, uint32 t) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    array<ж<g>> batch = new(129); /* len(pp.runq) / 2 + 1 */
    // First, grab a batch from local queue.
    var n = t - h;
    n = n / 2;
    if (n != (uint32)(len(pp.runq) / 2)) {
        @throw(runqputslowQueueIsNotˢ);
    }
    for (var i = (uint32)0; i < n; i++) {
        batch[(nint)(i)] = pp.runq[(nint)((h + i) % (uint32)len(pp.runq))].ptr();
    }
    if (!atomic.CasRel(Ꮡpp.of(runtime_package.Δp.Ꮡrunqhead), h, h + n)) {
        // cas-release, commits consume
        return false;
    }
    batch[(nint)(n)] = Ꮡgp;
    if (randomizeScheduler) {
        for (var i = (uint32)1; i <= n; i++) {
            var j = cheaprandn(i + 1);
            (batch[(nint)(i)], batch[(nint)(j)]) = (batch[(nint)(j)], batch[(nint)(i)]);
        }
    }
    // Link the goroutines.
    for (var i = (uint32)0; i < n; i++) {
        batch[(nint)(i)].of(g.Ꮡschedlink).set(batch[(nint)(i + 1)]);
    }
    gQueue q = default!;
    q.head.set(batch[0]);
    q.tail.set(batch[(nint)(n)]);
    // Now put the batch on global queue.
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    globrunqputbatch(ref q, (int32)(n + 1));
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    return true;
}

// runqputbatch tries to put all the G's on q on the local runnable queue.
// If the queue is full, they are put on the global queue; in that case
// this will temporarily acquire the scheduler lock.
// Executed only by the owner P.
internal static void runqputbatch(ж<Δp> Ꮡpp, ж<gQueue> Ꮡq, nint qsize) {
    ref var pp = ref Ꮡpp.DerefOrNull();
    ref var q = ref Ꮡq.DerefOrNull();

    var h = atomic.LoadAcq(Ꮡpp.of(runtime_package.Δp.Ꮡrunqhead));
    var t = pp.runqtail;
    var n = (uint32)0;
    while (!q.empty() && t - h < (uint32)len(pp.runq)) {
        var gp = q.pop();
        pp.runq[(nint)(t % (uint32)len(pp.runq))].set(gp);
        t++;
        n++;
    }
    qsize -= (nint)n;
    if (randomizeScheduler) {
        uint32 off(uint32 o) => (Ꮡpp.Value.runqtail + o) % (uint32)len(Ꮡpp.Value.runq);
        for (var i = (uint32)1; i < n; i++) {
            var j = cheaprandn(i + 1);
            (pp.runq[(nint)(off(i))], pp.runq[(nint)(off(j))]) = (pp.runq[(nint)(off(j))], pp.runq[(nint)(off(i))]);
        }
    }
    atomic.StoreRel(Ꮡpp.of(runtime_package.Δp.Ꮡrunqtail), t);
    if (!q.empty()) {
        @lock(Ꮡsched.of(schedt.Ꮡlock));
        globrunqputbatch(ref (Ꮡq).DerefOrNull(), (int32)qsize);
        unlock(Ꮡsched.of(schedt.Ꮡlock));
    }
}

// Get g from local runnable queue.
// If inheritTime is true, gp should inherit the remaining time in the
// current time slice. Otherwise, it should start a new time slice.
// Executed only by the owner P.
internal static (ж<g> gp, bool inheritTime) runqget(ж<Δp> Ꮡpp) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    // If there's a runnext, it's the next G to run.
    var next = pp.runnext;
    // If the runnext is non-0 and the CAS fails, it could only have been stolen by another P,
    // because other Ps can race to set runnext to 0, but only the current P can set it to non-0.
    // Hence, there's no need to retry this CAS if it fails.
    if (next != 0 && Ꮡpp.of(runtime_package.Δp.Ꮡrunnext).cas(next, 0)) {
        return (next.ptr(), true);
    }
    while (ᐧ) {
        var h = atomic.LoadAcq(Ꮡpp.of(runtime_package.Δp.Ꮡrunqhead)); // load-acquire, synchronize with other consumers
        var t = pp.runqtail;
        if (t == h) {
            return (default!, false);
        }
        var gpΔ1 = pp.runq[(nint)(h % (uint32)len(pp.runq))].ptr();
        if (atomic.CasRel(Ꮡpp.of(runtime_package.Δp.Ꮡrunqhead), h, h + 1)) {
            // cas-release, commits consume
            return (gpΔ1, false);
        }
    }
}

// runqdrain drains the local runnable queue of pp and returns all goroutines in it.
// Executed only by the owner P.
internal static (gQueue drainQ, uint32 n) runqdrain(ж<Δp> Ꮡpp) {
    gQueue drainQ = default!;
    uint32 n = default!;

    ref var pp = ref Ꮡpp.DerefOrNull();
    var oldNext = pp.runnext;
    if (oldNext != 0 && Ꮡpp.of(runtime_package.Δp.Ꮡrunnext).cas(oldNext, 0)) {
        drainQ.pushBack(oldNext.ptr());
        n++;
    }
retry:
    var h = atomic.LoadAcq(Ꮡpp.of(runtime_package.Δp.Ꮡrunqhead)); // load-acquire, synchronize with other consumers
    var t = pp.runqtail;
    var qn = t - h;
    if (qn == 0) {
        return (drainQ, n);
    }
    if (qn > (uint32)len(pp.runq)) {
        // read inconsistent h and t
        goto retry;
    }
    if (!atomic.CasRel(Ꮡpp.of(runtime_package.Δp.Ꮡrunqhead), h, h + qn)) {
        // cas-release, commits consume
        goto retry;
    }
    // We've inverted the order in which it gets G's from the local P's runnable queue
    // and then advances the head pointer because we don't want to mess up the statuses of G's
    // while runqdrain() and runqsteal() are running in parallel.
    // Thus we should advance the head pointer before draining the local P into a gQueue,
    // so that we can update any gp.schedlink only after we take the full ownership of G,
    // meanwhile, other P's can't access to all G's in local P's runnable queue and steal them.
    // See https://groups.google.com/g/golang-dev/c/0pTKxEKhHSc/m/6Q85QjdVBQAJ for more details.
    for (var i = (uint32)0; i < qn; i++) {
        var gp = pp.runq[(nint)((h + i) % (uint32)len(pp.runq))].ptr();
        drainQ.pushBack(gp);
        n++;
    }
    return (drainQ, n);
}

// Grabs a batch of goroutines from pp's runnable queue into batch.
// Batch is a ring buffer starting at batchHead.
// Returns number of grabbed goroutines.
// Can be executed by any P.
internal static uint32 runqgrab(ж<Δp> Ꮡpp, [GoArrayDims(256)] ж<array<Δguintptr>> Ꮡbatch, uint32 batchHead, bool stealRunNextG) {
    ref var pp = ref Ꮡpp.DerefOrNull();
    ref var batch = ref Ꮡbatch.DerefOrNull();

    while (ᐧ) {
        var h = atomic.LoadAcq(Ꮡpp.of(runtime_package.Δp.Ꮡrunqhead)); // load-acquire, synchronize with other consumers
        var t = atomic.LoadAcq(Ꮡpp.of(runtime_package.Δp.Ꮡrunqtail)); // load-acquire, synchronize with the producer
        var n = t - h;
        n = n - n / 2;
        if (n == 0) {
            if (stealRunNextG) {
                // Try to steal from pp.runnext.
                {
                    var next = pp.runnext; if (next != 0) {
                        if (pp.status == _Prunning) {
                            // Sleep to ensure that pp isn't about to run the g
                            // we are about to steal.
                            // The important use case here is when the g running
                            // on pp ready()s another g and then almost
                            // immediately blocks. Instead of stealing runnext
                            // in this window, back off to give pp a chance to
                            // schedule runnext. This will avoid thrashing gs
                            // between different Ps.
                            // A sync chan send/recv takes ~50ns as of time of
                            // writing, so 3us gives ~50x overshoot.
                            if (!osHasLowResTimer){
                                usleep(3);
                            } else {
                                // On some platforms system timer granularity is
                                // 1-15ms, which is way too much for this
                                // optimization. So just yield.
                                osyield();
                            }
                        }
                        if (!Ꮡpp.of(runtime_package.Δp.Ꮡrunnext).cas(next, 0)) {
                            continue;
                        }
                        batch[batchHead % (uint32)256] = next;
                        return 1;
                    }
                }
            }
            return 0;
        }
        if (n > (uint32)(len(pp.runq) / 2)) {
            // read inconsistent h and t
            continue;
        }
        for (var i = (uint32)0; i < n; i++) {
            var g = pp.runq[(nint)((h + i) % (uint32)len(pp.runq))];
            batch[(batchHead + i) % (uint32)256] = g;
        }
        if (atomic.CasRel(Ꮡpp.of(runtime_package.Δp.Ꮡrunqhead), h, h + n)) {
            // cas-release, commits consume
            return n;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runqstealRunqOverflowˢ = "runqsteal: runq overflow"u8;

// Steal half of elements from local runnable queue of p2
// and put onto local runnable queue of p.
// Returns one of the stolen elements (or nil if failed).
internal static ж<g> runqsteal(ж<Δp> Ꮡpp, ж<Δp> Ꮡp2, bool stealRunNextG) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    var t = pp.runqtail;
    var n = runqgrab(Ꮡp2, Ꮡpp.of(runtime_package.Δp.Ꮡrunq), t, stealRunNextG);
    if (n == 0) {
        return default!;
    }
    n--;
    var gp = pp.runq[(nint)((t + n) % (uint32)len(pp.runq))].ptr();
    if (n == 0) {
        return gp;
    }
    var h = atomic.LoadAcq(Ꮡpp.of(runtime_package.Δp.Ꮡrunqhead)); // load-acquire, synchronize with consumers
    if (t - h + n >= (uint32)len(pp.runq)) {
        @throw(runqstealRunqOverflowˢ);
    }
    atomic.StoreRel(Ꮡpp.of(runtime_package.Δp.Ꮡrunqtail), t + n); // store-release, makes the item available for consumption
    return gp;
}

// A gQueue is a dequeue of Gs linked through g.schedlink. A G can only
// be on one gQueue or gList at a time.
[GoType] partial struct gQueue {
    internal Δguintptr head;
    internal Δguintptr tail;
}

// empty reports whether q is empty.
[GoRecv] internal static bool empty(this ref gQueue q) {
    return q.head == 0;
}

// push adds gp to the head of q.
[GoRecv] internal static void push(this ref gQueue q, ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    gp.schedlink = q.head;
    q.head.set(Ꮡgp);
    if (q.tail == 0) {
        q.tail.set(Ꮡgp);
    }
}

// pushBack adds gp to the tail of q.
[GoRecv] internal static void pushBack(this ref gQueue q, ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    gp.schedlink = 0;
    if (q.tail != 0){
        q.tail.ptr().of(g.Ꮡschedlink).set(Ꮡgp);
    } else {
        q.head.set(Ꮡgp);
    }
    q.tail.set(Ꮡgp);
}

// pushBackAll adds all Gs in q2 to the tail of q. After this q2 must
// not be used.
[GoRecv] internal static void pushBackAll(this ref gQueue q, gQueue q2) {
    if (q2.tail == 0) {
        return;
    }
    q2.tail.ptr().Value.schedlink = 0;
    if (q.tail != 0){
        q.tail.ptr().Value.schedlink = q2.head;
    } else {
        q.head = q2.head;
    }
    q.tail = q2.tail;
}

// pop removes and returns the head of queue q. It returns nil if
// q is empty.
[GoRecv] internal static ж<g> pop(this ref gQueue q) {
    var gp = q.head.ptr();
    if (gp != nil) {
        q.head = gp.Value.schedlink;
        if (q.head == 0) {
            q.tail = 0;
        }
    }
    return gp;
}

// popList takes all Gs in q and returns them as a gList.
[GoRecv] internal static gList popList(this ref gQueue q) {
    var Δstack = new gList(q.head);
    q = new gQueue(nil);
    return Δstack;
}

// A gList is a list of Gs linked through g.schedlink. A G can only be
// on one gQueue or gList at a time.
[GoType] partial struct gList {
    internal Δguintptr head;
}

// empty reports whether l is empty.
[GoRecv] internal static bool empty(this ref gList l) {
    return l.head == 0;
}

// push adds gp to the head of l.
[GoRecv] internal static void push(this ref gList l, ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    gp.schedlink = l.head;
    l.head.set(Ꮡgp);
}

// pushAll prepends all Gs in q to l.
[GoRecv] internal static void pushAll(this ref gList l, gQueue q) {
    if (!q.empty()) {
        q.tail.ptr().Value.schedlink = l.head;
        l.head = q.head;
    }
}

// pop removes and returns the head of l. If l is empty, it returns nil.
[GoRecv] internal static ж<g> pop(this ref gList l) {
    var gp = l.head.ptr();
    if (gp != nil) {
        l.head = gp.Value.schedlink;
    }
    return gp;
}

//go:linkname setMaxThreads runtime/debug.setMaxThreads
internal static nint /*out*/ setMaxThreads(nint @in) {
    nint @out = default!;

    @lock(Ꮡsched.of(schedt.Ꮡlock));
    @out = (nint)sched.maxmcount;
    if (@in > 0x7fffffff){
        // MaxInt32
        sched.maxmcount = 0x7fffffff;
    } else {
        sched.maxmcount = (int32)@in;
    }
    checkmcount();
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    return @out;
}

// procPin should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/gopkg
//   - github.com/choleraehyq/pid
//   - github.com/songzhibin97/gkit
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname procPin
//go:nosplit
internal static nint procPin() {
    var gp = getg();
    var mp = gp.Value.m;
    mp.Value.locks++;
    return (nint)(~(~mp).p.ptr()).id;
}

// procUnpin should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/gopkg
//   - github.com/choleraehyq/pid
//   - github.com/songzhibin97/gkit
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname procUnpin
//go:nosplit
internal static void procUnpin() {
    var gp = getg();
    gp.Value.m.Value.locks--;
}

//go:linkname sync_runtime_procPin sync.runtime_procPin
//go:nosplit
internal static nint sync_runtime_procPin() {
    return procPin();
}

//go:linkname sync_runtime_procUnpin sync.runtime_procUnpin
//go:nosplit
internal static void sync_runtime_procUnpin() {
    procUnpin();
}

//go:linkname sync_atomic_runtime_procPin sync/atomic.runtime_procPin
//go:nosplit
internal static nint sync_atomic_runtime_procPin() {
    return procPin();
}

//go:linkname sync_atomic_runtime_procUnpin sync/atomic.runtime_procUnpin
//go:nosplit
internal static void sync_atomic_runtime_procUnpin() {
    procUnpin();
}

// Active spinning for sync.Mutex.
//
// sync_runtime_canSpin should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/livekit/protocol
//   - github.com/sagernet/gvisor
//   - gvisor.dev/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname sync_runtime_canSpin sync.runtime_canSpin
//go:nosplit
internal static bool sync_runtime_canSpin(nint i) {
    // sync.Mutex is cooperative, so we are conservative with spinning.
    // Spin only few times and only if running on a multicore machine and
    // GOMAXPROCS>1 and there is at least one other running P and local runq is empty.
    // As opposed to runtime mutex we don't do passive spinning here,
    // because there can be work on global runq or on other Ps.
    if (i >= active_spin || ncpu <= 1 || gomaxprocs <= Ꮡsched.of(schedt.Ꮡnpidle).Load() + Ꮡsched.of(schedt.Ꮡnmspinning).Load() + 1) {
        return false;
    }
    {
        var Δp = (~(~getg()).m).p.ptr(); if (!runqempty(Δp)) {
            return false;
        }
    }
    return true;
}

// sync_runtime_doSpin should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/livekit/protocol
//   - github.com/sagernet/gvisor
//   - gvisor.dev/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname sync_runtime_doSpin sync.runtime_doSpin
//go:nosplit
internal static void sync_runtime_doSpin() {
    procyield(active_spin_cnt);
}

internal static ж<randomOrder> ᏑstealOrder = new StandardBox<randomOrder>(default(randomOrder));
internal static ref randomOrder stealOrder => ref ᏑstealOrder.Value;

// randomOrder/randomEnum are helper types for randomized work stealing.
// They allow to enumerate all Ps in different pseudo-random orders without repetitions.
// The algorithm is based on the fact that if we have X such that X and GOMAXPROCS
// are coprime, then a sequences of (i + X) % GOMAXPROCS gives the required enumeration.
[GoType] partial struct randomOrder {
    internal uint32 count;
    internal slice<uint32> coprimes;
}

[GoType] partial struct randomEnum {
    internal uint32 i;
    internal uint32 count;
    internal uint32 pos;
    internal uint32 inc;
}

[GoRecv] internal static void reset(this ref randomOrder ord, uint32 count) {
    ord.count = count;
    ord.coprimes = ord.coprimes[..0];
    for (var i = (uint32)1; i <= count; i++) {
        if (gcd(i, count) == 1) {
            ord.coprimes = append(ord.coprimes, i);
        }
    }
}

[GoRecv] internal static randomEnum start(this ref randomOrder ord, uint32 i) {
    return new randomEnum(
        count: ord.count,
        pos: i % ord.count,
        inc: ord.coprimes[(nint)(i / ord.count % (uint32)len(ord.coprimes))]
    );
}

[GoRecv] internal static bool done(this ref randomEnum @enum) {
    return @enum.i == @enum.count;
}

[GoRecv] internal static void next(this ref randomEnum @enum) {
    @enum.i++;
    @enum.pos = (@enum.pos + @enum.inc) % @enum.count;
}

[GoRecv] internal static uint32 position(this ref randomEnum @enum) {
    return @enum.pos;
}

internal static uint32 gcd(uint32 a, uint32 b) {
    while (b != 0) {
        (a, b) = (b, a % b);
    }
    return a;
}

// An initTask represents the set of initializations that need to be done for a package.
// Keep in sync with ../../test/noinit.go:initTask
[GoType] partial struct initTask {
    internal uint32 state; // 0 = uninitialized, 1 = in progress, 2 = done
    internal uint32 nfns;
}

// followed by nfns pcs, uintptr sized, one per init function to run

// inittrace stores statistics for init functions which are
// updated by malloc and newproc when active is true.
internal static tracestat inittrace;

[GoType] partial struct tracestat {
    internal bool active;   // init tracing activation status
    internal uint64 id; // init goroutine id
    internal uint64 allocs; // heap allocations
    internal uint64 bytes; // heap allocated bytes
}

internal static void doInit(slice<ж<initTask>> ts) {
    foreach (var (_, t) in ts) {
        doInit1(t);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string recursiveCallDuringˢ = "recursive call during initialization - linker skew"u8;
internal static readonly @string inittaskWithNoFunctionsˢ = "inittask with no functions"u8;

internal static void doInit1(ж<initTask> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    switch (t.state) {
    case 2: {
        return;
    }
    case 1: {
        @throw(recursiveCallDuringˢ);
        break;
    }
    default: {
        t.state = 1; // initialization in progress
// fully initialized
// initialization in progress
// not initialized yet
        int64 start = default!;
        tracestat before = default!;
        if (inittrace.active) {
            start = nanotime();
            // Load stats non-atomically since tracinit is updated only by this init goroutine.
            before = inittrace;
        }
        if (t.nfns == 0) {
            // We should have pruned all of these in the linker.
            @throw(inittaskWithNoFunctionsˢ);
        }
        ref var firstFunc = ref heap<@unsafe.Pointer>(out var ᏑfirstFunc);
        firstFunc = (uintptr)add(@unsafe.Pointer.FromPinnedBox(Ꮡt), 8);
        for (var i = (uint32)0; i < t.nfns; i++) {
            ref var Δp = ref heap<@unsafe.Pointer>(out var Ꮡp);
            Δp = (uintptr)add(firstFunc, (uintptr)i * (uintptr)goarch.PtrSize);
            var f = (Ꮡp.Reinterpret<@unsafe.Pointer, Action>()).ValueSlot;
            f();
        }
        if (inittrace.active) {
            var end = nanotime();
            // Load stats non-atomically since tracinit is updated only by this init goroutine.
            var after = inittrace;
            var f = (ᏑfirstFunc.Reinterpret<@unsafe.Pointer, Action>()).ValueSlot;
            @string pkg = funcpkgpath(findfunc(abi.FuncPCABIInternal((f).OrTypedNilFunc())));
            array<byte> sbuf = new(24);
            print((@string)"init "u8, pkg, (@string)" @"u8);
            print(((@string)fmtNSAsMS(sbuf[..], (uint64)(start - runtimeInitTime))), (@string)" ms, "u8);
            print(((@string)fmtNSAsMS(sbuf[..], (uint64)(end - start))), (@string)" ms clock, "u8);
            print(((@string)itoa(sbuf[..], after.bytes - before.bytes)), (@string)" bytes, "u8);
            print(((@string)itoa(sbuf[..], after.allocs - before.allocs)), (@string)" allocs"u8);
            print((@string)"\n"u8);
        }
        t.state = 2; // initialization done
        break;
    }}

}

} // end runtime_package
