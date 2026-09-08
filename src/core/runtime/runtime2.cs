// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using itab = go.@internal.abi_package.ITab;

// go2cs HAND-OWNED. Unlike the companion pattern (runtime2_impl.cs supplies the bodyless
// guintptr/muintptr/… partials this file declares), FOUR edits here modify REGENERATED content a
// companion cannot supply. The list was TWO until the 1.24.13 re-derive; the other two were found
// by the three-way merge as genuine conflicts, i.e. this header under-documented its own delta and
// nothing could have noticed until something merged against it:
//
//   1. efaceOf's body — a raw-metal reinterpret stubbed to an inert eface.
//   2. the gomaxprocs/ncpu field-initializer seed — bootstrap-populated in Go.
//   3. the waitReason block declared as `static readonly` FIELDS where the converter emits
//      expression-bodied PROPERTIES. ⚠ The REASON for this is unrecorded; it is preserved because
//      re-applying a hand-own's delta means re-applying it, not judging it, and it is flagged here
//      so its owner can confirm or retire it rather than it surviving by silence a second time.
//   4. waitReasonStrings' terminator as `}.array();` where the converter emits `}.array(38);`.
//      Almost certainly the same change as (3) — an explicit length beside non-constant indices —
//      but that is an INFERENCE and no compiler has been run against it here.
//
// This marker drops runtime2.go from a -stdlib reconvert so those edits — and the frozen bodyless
// partials the companion implements — survive. THE TRADE-OFF IS SHARPER AT A RELEASE HOP THAN THE
// ORIGINAL WORDING ADMITTED: a frozen file does not merely stop receiving converter improvements,
// it stops receiving GO'S OWN CHANGES. At 1.24.13 seven declarations and three fields arrive that
// fifteen emitted siblings read (g.syncGroup in ten files, m.mWaitList, m.fipsIndicator,
// isIdleInSynctest and six waitReason constants), which is why this file is now RE-DERIVED from the
// 1.24.13 emission with the delta above re-applied, rather than surgically patched.
[module: go.GoManualConversion]

namespace go;

using abi = @internal.abi_package;
using chacha8rand = @internal.chacha8rand_package;
using goarch = @internal.goarch_package;
using goexperiment = @internal.goexperiment_package;
using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// G status
//
// Beyond indicating the general state of a G, the G status
// acts like a lock on the goroutine's stack (and hence its
// ability to execute user code).
//
// If you add to this list, add to the list
// of "okay during garbage collection" status
// in mgcmark.go too.
//
// TODO(austin): The _Gscan bit could be much lighter-weight.
// For example, we could choose not to run _Gscanrunnable
// goroutines found in the run queue, rather than CAS-looping
// until they become _Grunnable. And transitions like
// _Gscanwaiting -> _Gscanrunnable are actually okay because
// they don't affect stack ownership.
// defined constants
internal static readonly UntypedInt _Gidle = iota; // 0

internal static readonly UntypedInt _Grunnable = 1; // 1

internal static readonly UntypedInt _Grunning = 2; // 2

internal static readonly UntypedInt _Gsyscall = 3; // 3

internal static readonly UntypedInt _Gwaiting = 4; // 4

internal static readonly UntypedInt _Gmoribund_unused = 5; // 5

internal static readonly UntypedInt _Gdead = 6; // 6

internal static readonly UntypedInt _Genqueue_unused = 7; // 7

internal static readonly UntypedInt _Gcopystack = 8; // 8

internal static readonly UntypedInt _Gpreempted = 9; // 9

internal static readonly UntypedInt _Gscan = 0x1000;

internal static readonly UntypedInt _Gscanrunnable = /* _Gscan + _Grunnable */ 4097; // 0x1001

internal static readonly UntypedInt _Gscanrunning = /* _Gscan + _Grunning */ 4098; // 0x1002

internal static readonly UntypedInt _Gscansyscall = /* _Gscan + _Gsyscall */ 4099; // 0x1003

internal static readonly UntypedInt _Gscanwaiting = /* _Gscan + _Gwaiting */ 4100; // 0x1004

internal static readonly UntypedInt _Gscanpreempted = /* _Gscan + _Gpreempted */ 4105; // 0x1009

// P status
internal static readonly UntypedInt _Pidle = iota;
internal static readonly UntypedInt _Prunning = 1;
internal static readonly UntypedInt _Psyscall = 2;
internal static readonly UntypedInt _Pgcstop = 3;
internal static readonly UntypedInt _Pdead = 4;

// Mutual exclusion locks.  In the uncontended case,
// as fast as spin locks (just a few user-level instructions),
// but on the contention path they sleep in the kernel.
// A zeroed Mutex is unlocked (no need to initialize each lock).
// Initialization is helpful for static lock ranking, but not required.
[GoType] partial struct mutex {
    // Empty struct if lock ranking is disabled, otherwise includes the lock rank
    internal partial ref lockRankStruct lockRankStruct { get; }
    // Futex-based impl treats it as uint32 key,
    // while sema-based impl as M* waitm.
    // Used to be a union, but unions break precise GC.
    internal uintptr key;
}

[GoType] partial struct funcval {
    internal uintptr fn;
}

// variable-size, fn-specific data here
[GoType] partial struct iface {
    internal ж<itab> tab;
    internal @unsafe.Pointer data;
}

[GoType] partial struct eface {
    internal ж<_type> _type;
    internal @unsafe.Pointer data;
}

internal static ж<eface> efaceOf(ж<any> Ꮡep) {
    // Hand-adjusted (this file carries the GoManualConversion marker above so a reconvert keeps
    // this edit): Go reinterprets the
    // interface value's own memory as an eface{_type, data} to walk its type descriptor —
    // raw-metal on a non-native type, meaningless against a managed `any` box (the reinterpret
    // panicked on first touch, taking the whole runtime_package type initializer down with it:
    // iface.cs's uint16Type/…/sliceType and netpoll.cs's pdType field initializers all run
    // (~efaceOf(…))._type at class-init). Return an inert default eface (nil _type, nil data)
    // instead — the descriptor-walking consumers are themselves vestigial in converted code
    // (go2cs replaces itab dispatch with C# interfaces + source generators).
    return Ꮡ(new eface());
}

// type Δguintptr is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// func ptr is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// func set is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// func cas is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// func guintptr is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// func setGNoWB is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// type puintptr is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// func ptr is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// func set is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// type muintptr is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// func ptr is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// func set is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// func setMNoWB is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// The guintptr, muintptr, and puintptr are all used to bypass write barriers.
// It is particularly important to avoid write barriers when the current P has
// been released, because the GC thinks the world is stopped, and an
// unexpected write barrier would not be synchronized with the GC,
// which can lead to a half-executed write barrier that has marked the object
// but not queued it. If the GC skips the object and completes before the
// queuing can occur, it will incorrectly free the object.
//
// We tried using special assignment functions invoked only when not
// holding a running P, but then some updates to a particular memory
// word went through write barriers and some did not. This breaks the
// write barrier shadow checking mode, and it is also scary: better to have
// a word that is completely ignored by the GC than to have one for which
// only a few updates are ignored.
//
// Gs and Ps are always reachable via true pointers in the
// allgs and allp lists or (during allocation before they reach those lists)
// from stack variables.
//
// Ms are always reachable via true pointers either from allm or
// freem. Unlike Gs and Ps we do free Ms, so it's important that
// nothing ever hold an muintptr across a safe point.
[GoType] partial struct gobuf {
    // The offsets of sp, pc, and g are known to (hard-coded in) libmach.
    //
    // ctxt is unusual with respect to GC: it may be a
    // heap-allocated funcval, so GC needs to track it, but it
    // needs to be set and cleared from assembly, where it's
    // difficult to have write barriers. However, ctxt is really a
    // saved, live register, and we only ever exchange it between
    // the real register and the gobuf. Hence, we treat it as a
    // root during stack scanning, which means assembly that saves
    // and restores it doesn't need write barriers. It's still
    // typed as a pointer so that any other writes from Go get
    // write barriers.
    internal uintptr sp;
    internal uintptr pc;
    internal Δguintptr g;
    internal @unsafe.Pointer ctxt;
    internal uintptr ret;
    internal uintptr lr;
    internal uintptr bp; // for framepointer-enabled architectures
}

// sudog (pseudo-g) represents a g in a wait list, such as for sending/receiving
// on a channel.
//
// sudog is necessary because the g ↔ synchronization object relation
// is many-to-many. A g can be on many wait lists, so there may be
// many sudogs for one g; and many gs may be waiting on the same
// synchronization object, so there may be many sudogs for one object.
//
// sudogs are allocated from a special pool. Use acquireSudog and
// releaseSudog to allocate and free them.
[GoType] partial struct sudog {
// The following fields are protected by the hchan.lock of the
// channel this sudog is blocking on. shrinkstack depends on
// this for sudogs involved in channel ops.
    internal ж<g> g;
    internal ж<sudog> next;
    internal ж<sudog> prev;
    internal @unsafe.Pointer elem; // data element (may point to stack)
// The following fields are never accessed concurrently.
// For channels, waitlink is only accessed by g.
// For semaphores, all fields (including the ones above)
// are only accessed when holding a semaRoot lock.
    internal int64 acquiretime;
    internal int64 releasetime;
    internal uint32 ticket;
    // isSelect indicates g is participating in a select, so
    // g.selectDone must be CAS'd to win the wake-up race.
    internal bool isSelect;
    // success indicates whether communication over channel c
    // succeeded. It is true if the goroutine was awoken because a
    // value was delivered over channel c, and false if awoken
    // because c was closed.
    internal bool success;
    // waiters is a count of semaRoot waiting list other than head of list,
    // clamped to a uint16 to fit in unused space.
    // Only meaningful at the head of the list.
    // (If we wanted to be overly clever, we could store a high 16 bits
    // in the second entry in the list.)
    internal uint16 waiters;
    internal ж<sudog> parent; // semaRoot binary tree
    internal ж<sudog> waitlink; // g.waiting list or semaRoot
    internal ж<sudog> waittail; // semaRoot
    internal ж<Δhchan> c; // channel
}

[GoType] partial struct libcall {
    internal uintptr fn;
    internal uintptr n; // number of parameters
    internal uintptr args; // parameters
    internal uintptr r1; // return values
    internal uintptr r2;
    internal uintptr err; // error number
}

// Stack describes a Go execution stack.
// The bounds of the stack are exactly [lo, hi),
// with no implicit data structures on either side.
[GoType] partial struct Δstack {
    internal uintptr lo;
    internal uintptr hi;
}

// heldLockInfo gives info on a held lock and the rank of that lock
[GoType] partial struct heldLockInfo {
    internal uintptr lockAddr;
    internal lockRank rank;
}

[GoType] partial struct g {
    // Stack parameters.
    // stack describes the actual stack memory: [stack.lo, stack.hi).
    // stackguard0 is the stack pointer compared in the Go stack growth prologue.
    // It is stack.lo+StackGuard normally, but can be StackPreempt to trigger a preemption.
    // stackguard1 is the stack pointer compared in the //go:systemstack stack growth prologue.
    // It is stack.lo+StackGuard on g0 and gsignal stacks.
    // It is ~0 on other goroutine stacks, to trigger a call to morestackc (and crash).
    internal Δstack stack; // offset known to runtime/cgo
    internal uintptr stackguard0; // offset known to liblink
    internal uintptr stackguard1; // offset known to liblink
    internal ж<_panic> _panic; // innermost panic - offset known to liblink
    internal ж<_defer> _defer; // innermost defer
    internal ж<m> m;   // current m; offset known to arm liblink
    internal gobuf sched;
    internal uintptr syscallsp; // if status==Gsyscall, syscallsp = sched.sp to use during gc
    internal uintptr syscallpc; // if status==Gsyscall, syscallpc = sched.pc to use during gc
    internal uintptr syscallbp; // if status==Gsyscall, syscallbp = sched.bp to use in fpTraceback
    internal uintptr stktopsp; // expected sp at top of stack, to check in traceback
    // param is a generic pointer parameter field used to pass
    // values in particular contexts where other storage for the
    // parameter would be difficult to find. It is currently used
    // in four ways:
    // 1. When a channel operation wakes up a blocked goroutine, it sets param to
    //    point to the sudog of the completed blocking operation.
    // 2. By gcAssistAlloc1 to signal back to its caller that the goroutine completed
    //    the GC cycle. It is unsafe to do so in any other way, because the goroutine's
    //    stack may have moved in the meantime.
    // 3. By debugCallWrap to pass parameters to a new goroutine because allocating a
    //    closure in the runtime is forbidden.
    // 4. When a panic is recovered and control returns to the respective frame,
    //    param may point to a savedOpenDeferState.
    internal @unsafe.Pointer param;
    internal atomic.Uint32 atomicstatus;
    internal uint32 stackLock; // sigprof/scang lock; TODO: fold in to atomicstatus
    internal uint64 goid;
    internal Δguintptr schedlink;
    internal int64 waitsince;      // approx time when the g become blocked
    internal waitReason waitreason; // if status==Gwaiting
    internal bool preempt; // preemption signal, duplicates stackguard0 = stackpreempt
    internal bool preemptStop; // transition to _Gpreempted on preemption; otherwise, just deschedule
    internal bool preemptShrink; // shrink stack at synchronous safe point
    // asyncSafePoint is set if g is stopped at an asynchronous
    // safe point. This means there are frames on the stack
    // without precise pointer information.
    internal bool asyncSafePoint;
    internal bool paniconfault; // panic (instead of crash) on unexpected fault address
    internal bool gcscandone; // g has scanned stack; protected by _Gscan bit in status
    internal bool throwsplit; // must not split stack
    // activeStackChans indicates that there are unlocked channels
    // pointing into this goroutine's stack. If true, stack
    // copying needs to acquire channel locks to protect these
    // areas of the stack.
    internal bool activeStackChans;
    // parkingOnChan indicates that the goroutine is about to
    // park on a chansend or chanrecv. Used to signal an unsafe point
    // for stack shrinking.
    internal atomic.Bool parkingOnChan;
    // inMarkAssist indicates whether the goroutine is in mark assist.
    // Used by the execution tracer.
    internal bool inMarkAssist;
    internal bool coroexit; // argument to coroswitch_m
    internal int8 raceignore;  // ignore race detection events
    internal bool nocgocallback;  // whether disable callback from C
    internal bool tracking;  // whether we're tracking this G for sched latency statistics
    internal uint8 trackingSeq; // used to decide whether to track this G
    internal int64 trackingStamp; // timestamp of when the G last started being tracked
    internal int64 runnableTime; // the amount of time spent runnable, cleared when running, only used when tracking
    internal muintptr lockedm;
    internal uint8 fipsIndicator;
    internal uint32 sig;
    internal slice<byte> writebuf;
    internal uintptr sigcode0;
    internal uintptr sigcode1;
    internal uintptr sigpc;
    internal uint64 parentGoid;          // goid of goroutine that created this goroutine
    internal uintptr gopc;         // pc of go statement that created this goroutine
    internal ж<slice<ancestorInfo>> ancestors; // ancestor information goroutine(s) that created this goroutine (only used if debug.tracebackancestors)
    internal uintptr startpc;         // pc of goroutine function
    internal uintptr racectx;
    internal ж<sudog> waiting;      // sudog structures this g is waiting on (that have a valid elem ptr); in lock order
    internal slice<uintptr> cgoCtxt; // cgo traceback context
    internal @unsafe.Pointer labels; // profiler labels
    internal ж<timer> timer;      // cached timer for time.Sleep
    internal int64 sleepWhen;          // when to sleep until
    internal atomic.Uint32 selectDone;  // are we participating in a select and did someone win the race?
    // goroutineProfiled indicates the status of this goroutine's stack for the
    // current in-progress goroutine profile
    internal goroutineProfileStateHolder goroutineProfiled;
    internal ж<coro> coroarg; // argument during coroutine transfers
    internal ж<synctestGroup> syncGroup;
    // Per-G tracer state.
    internal gTraceState trace;
// Per-G GC state

    // gcAssistBytes is this G's GC assist credit in terms of
    // bytes allocated. If this is positive, then the G has credit
    // to allocate gcAssistBytes bytes without assisting. If this
    // is negative, then the G must correct this by performing
    // scan work. We track this in bytes to make it fast to update
    // and check for debt in the malloc hot path. The assist ratio
    // determines how this corresponds to scan work debt.
    internal int64 gcAssistBytes;
}

// gTrackingPeriod is the number of transitions out of _Grunning between
// latency tracking runs.
internal static readonly UntypedInt gTrackingPeriod = 8;

internal static readonly UntypedInt tlsSlots = 6;
internal static readonly UntypedInt tlsSize = /* tlsSlots * goarch.PtrSize */ 48;

// Values for m.freeWait.
internal static readonly UntypedInt freeMStack = 0; // M done, free stack and reference.

internal static readonly UntypedInt freeMRef = 1; // M done, free reference.

internal static readonly UntypedInt freeMWait = 2; // M still in use.

[GoType] [GoValueClone("tls", "createstack", "trace", "pcvalueCache", "chacha8", "locksHeld")] partial struct m {
    internal ж<g> g0;  // goroutine with scheduling stack
    internal gobuf morebuf;  // gobuf arg to morestack
    internal uint32 divmod; // div/mod denominator for arm - known to liblink
    internal uint32 _; // align next field to 8 bytes
    // Fields not known to debuggers.
    internal uint64 procid;            // for debuggers, but offset not hard-coded
    internal ж<g> gsignal;             // signal-handling g
    internal gsignalStack goSigStack;      // Go-allocated signal handling stack
    internal sigset sigmask;            // storage for saved signal mask
    internal array<uintptr> tls = new(tlsSlots); // thread-local storage (for x86 extern register)
    internal Action mstartfn;
    internal ж<g> curg;    // current running goroutine
    internal Δguintptr caughtsig; // goroutine running during fatal signal
    internal puintptr p; // attached p for executing go code (nil if not executing go code)
    internal puintptr nextp;
    internal puintptr oldp; // the p that was attached before executing a syscall
    internal int64 id;
    internal bool g0StackAccurate; // whether the g0 stack has accurate bounds
    internal slice<ж<Δp>> allpSnapshot; // Snapshot of allp for use after dropping P in findRunnable, nil otherwise.
    internal int32 mallocing;
    internal throwType throwing;
    internal @string preemptoff; // if != "", keep curg running on this m
    internal int32 locks;
    internal int32 dying;
    internal int32 profilehz;
    internal bool spinning; // m is out of work and is actively looking for work
    internal bool blocked; // m is blocked on a note
    internal bool newSigstack; // minit on C thread called sigaltstack
    internal int8 printlock;
    internal bool incgo;          // m is executing a cgo call
    internal bool isextra;          // m is an extra m
    internal bool isExtraInC;          // m is an extra m that is not executing Go code
    internal bool isExtraInSig;          // m is an extra m in a signal handler
    internal atomic.Uint32 freeWait; // Whether it is safe to free g0 and delete m (one of freeMRef, freeMStack, freeMWait)
    internal bool needextram;
    internal uint8 traceback;
    internal uint64 ncgocall;        // number of cgo calls in total
    internal int32 ncgo;         // number of cgo calls currently in progress
    internal atomic.Uint32 cgoCallersUse; // if non-zero, cgoCallers in use temporarily
    internal ж<ΔcgoCallers> cgoCallers; // cgo traceback if crashing in cgo call
    internal note park;
    internal ж<m> alllink; // on allm
    internal muintptr schedlink;
    internal Δguintptr lockedg;
    internal array<uintptr> createstack = new(32); // stack that created this thread, it's used for StackRecord.Stack0, so it must align with it.
    internal uint32 lockedExt;      // tracking for external LockOSThread
    internal uint32 lockedInt;      // tracking for internal lockOSThread
    internal mWaitList mWaitList;   // list of runtime lock waiters
    internal mLockProfile mLockProfile; // fields relating to runtime.lock contention
    internal slice<uintptr> profStack; // used for memory/block/mutex stack traces
    // wait* are used to carry arguments from gopark into park_m, because
    // there's no stack to put them on. That is their sole purpose.
    internal Func<ж<g>, @unsafe.Pointer, bool> waitunlockf;
    internal @unsafe.Pointer waitlock;
    internal nint waitTraceSkip;
    internal traceBlockReason waitTraceBlockReason;
    internal uint32 syscalltick;
    internal ж<m> freelink; // on sched.freem
    internal mTraceState trace;
    // these are here because they are too large to be on the stack
    // of low-level NOSPLIT functions.
    internal libcall libcall;
    internal uintptr libcallpc; // for cpu profiler
    internal uintptr libcallsp;
    internal Δguintptr libcallg;
    internal winlibcall winsyscall; // stores syscall parameters on windows
    internal uintptr vdsoSP; // SP for traceback while in VDSO call (0 if not in call)
    internal uintptr vdsoPC; // PC for traceback while in VDSO call
    // preemptGen counts the number of completed preemption
    // signals. This is used to detect when a preemption is
    // requested, but fails.
    internal atomic.Uint32 preemptGen;
    // Whether this is a pending preemption signal on this M.
    internal atomic.Uint32 signalPending;
    // pcvalue lookup cache
    internal pcvalueCache pcvalueCache;
    internal partial ref dlogPerM dlogPerM { get; }
    internal partial ref mOS mOS { get; }
    internal chacha8rand.State chacha8;
    internal uint64 cheaprand;
    // Up to 10 locks held by this m, maintained by the lock ranking code.
    internal nint locksHeldLen;
    internal array<heldLockInfo> locksHeld = new(10);
    // Size the runtime.m structure so it fits in the 2048-byte size class, and
    // not in the next-smallest (1792-byte) size class. That leaves the 11 low
    // bits of muintptr values available for flags, as required for
    // GOEXPERIMENT=spinbitmutex.
    internal array<byte> __ = new(goexperiment.SpinbitMutexInt * 700 * (2 - goarch.PtrSize / 4));
}

[GoType("dyn")] partial struct p_gFree {
    internal partial ref gList gList { get; }
    internal int32 n;
}

[GoType("dyn")] [GoValueClone("buf")] partial struct p_mspancache {
    // We need an explicit length here because this field is used
    // in allocation codepaths where write barriers are not allowed,
    // and eliminating the write barrier/keeping it eliminated from
    // slice updates is tricky, more so than just managing the length
    // ourselves.
    internal nint len;
    internal array<ж<mspan>> buf = new(128);
}

[GoType] [GoValueClone("deferpoolbuf", "runq", "sudogbuf", "mspancache", "wbBuf")] partial struct Δp {
    internal int32 id;
    internal uint32 status; // one of pidle/prunning/...
    internal puintptr link;
    internal uint32 schedtick;     // incremented on every scheduler call
    internal uint32 syscalltick;     // incremented on every system call
    internal sysmontick sysmontick; // last tick observed by sysmon
    internal muintptr m;   // back-link to associated m (nil if idle)
    internal ж<mcache> mcache;
    internal pageCache pcache;
    internal uintptr raceprocctx;
    internal slice<ж<_defer>> deferpool; // pool of available defer structs (see panic.go)
    internal array<ж<_defer>> deferpoolbuf = new(32);
    // Cache of goroutine ids, amortizes accesses to runtime·sched.goidgen.
    internal uint64 goidcache;
    internal uint64 goidcacheend;
    // Queue of runnable goroutines. Accessed without lock.
    internal uint32 runqhead;
    internal uint32 runqtail;
    internal array<Δguintptr> runq = new(256);
    // runnext, if non-nil, is a runnable G that was ready'd by
    // the current G and should be run next instead of what's in
    // runq if there's time remaining in the running G's time
    // slice. It will inherit the time left in the current time
    // slice. If a set of goroutines is locked in a
    // communicate-and-wait pattern, this schedules that set as a
    // unit and eliminates the (potentially large) scheduling
    // latency that otherwise arises from adding the ready'd
    // goroutines to the end of the run queue.
    //
    // Note that while other P's may atomically CAS this to zero,
    // only the owner P can CAS it to a valid G.
    internal Δguintptr runnext;
    // Available G's (status == Gdead)
    internal p_gFree gFree;
    internal slice<ж<sudog>> sudogcache;
    internal array<ж<sudog>> sudogbuf = new(128);
    // Cache of mspan objects from the heap.
    internal p_mspancache mspancache;
    // Cache of a single pinner object to reduce allocations from repeated
    // pinner creation.
    internal ж<pinner> pinnerCache;
    internal pTraceState trace;
    internal persistentAlloc palloc; // per-P to avoid mutex
    // Per-P GC state
    internal int64 gcAssistTime; // Nanoseconds in assistAlloc
    internal int64 gcFractionalMarkTime; // Nanoseconds in fractional mark worker (atomic)
    // limiterEvent tracks events for the GC CPU limiter.
    internal limiterEvent limiterEvent;
    // gcMarkWorkerMode is the mode for the next mark worker to run in.
    // That is, this is used to communicate with the worker goroutine
    // selected for immediate execution by
    // gcController.findRunnableGCWorker. When scheduling other goroutines,
    // this field must be set to gcMarkWorkerNotWorker.
    internal gcMarkWorkerMode gcMarkWorkerMode;
    // gcMarkWorkerStartTime is the nanotime() at which the most recent
    // mark worker started.
    internal int64 gcMarkWorkerStartTime;
    // gcw is this P's GC work buffer cache. The work buffer is
    // filled by write barriers, drained by mutator assists, and
    // disposed on certain GC state transitions.
    internal gcWork gcw;
    // wbBuf is this P's GC write barrier buffer.
    //
    // TODO: Consider caching this in the running G.
    internal wbBuf wbBuf;
    internal uint32 runSafePointFn; // if 1, run sched.safePointFn at next safe point
    // statsSeq is a counter indicating whether this P is currently
    // writing any stats. Its value is even when not, odd when it is.
    internal atomic.Uint32 statsSeq;
    // Timer heap.
    internal timers timers;
    // maxStackScanDelta accumulates the amount of stack space held by
    // live goroutines (i.e. those eligible for stack scanning).
    // Flushed to gcController.maxStackScan once maxStackScanSlack
    // or -maxStackScanSlack is reached.
    internal int64 maxStackScanDelta;
    // gc-time statistics about current goroutines
    // Note that this differs from maxStackScan in that this
    // accumulates the actual stack observed to be used at GC time (hi - sp),
    // not an instantaneous measure of the total stack size that might need
    // to be scanned (hi - lo).
    internal uint64 scannedStackSize; // stack size of goroutines scanned by this P
    internal uint64 scannedStacks; // number of goroutines scanned by this P
    // preempt is set to indicate that this P should be enter the
    // scheduler ASAP (regardless of what G is running on it).
    internal bool preempt;
    // gcStopTime is the nanotime timestamp that this P last entered _Pgcstop.
    internal int64 gcStopTime;
}

[GoType("dyn")] partial struct schedt_disable {
    // user disables scheduling of user goroutines.
    internal bool user;
    internal gQueue runnable; // pending runnable Gs
    internal int32 n;  // length of runnable
}

[GoType("dyn")] partial struct schedt_gFree {
    internal mutex @lock;
    internal gList stack; // Gs with stacks
    internal gList noStack; // Gs without stacks
    internal int32 n;
}

// Padding is no longer needed. False sharing is now not a worry because p is large enough
// that its size class is an integer multiple of the cache line size (for any of our architectures).
[GoType] [GoValueClone("timeToRun", "stwStoppingTimeGC", "stwStoppingTimeOther", "stwTotalTimeGC", "stwTotalTimeOther")] partial struct schedt {
    internal atomic.Uint64 goidgen;
    internal atomic.Int64 lastpoll; // time of last network poll, 0 if currently polling
    internal atomic.Int64 pollUntil; // time to which current poll is sleeping
    internal mutex @lock;
// When increasing nmidle, nmidlelocked, nmsys, or nmfreed, be
// sure to call checkdead().
    internal muintptr midle; // idle m's waiting for work
    internal int32 nmidle;    // number of idle m's waiting for work
    internal int32 nmidlelocked;    // number of locked m's waiting for work
    internal int64 mnext;    // number of m's that have been created and next M ID
    internal int32 maxmcount;    // maximum number of m's allowed (or die)
    internal int32 nmsys;    // number of system m's not counted for deadlock
    internal int64 nmfreed;    // cumulative number of freed m's
    internal atomic.Int32 ngsys; // number of system goroutines
    internal puintptr pidle; // idle p's
    internal atomic.Int32 npidle;
    internal atomic.Int32 nmspinning;  // See "Worker thread parking/unparking" comment in proc.go.
    internal atomic.Uint32 needspinning; // See "Delicate dance" comment in proc.go. Boolean. Must hold sched.lock to set to 1.
    // Global runnable queue.
    internal gQueue runq;
    internal int32 runqsize;
    // disable controls selective disabling of the scheduler.
    //
    // Use schedEnableUser to control this.
    //
    // disable is protected by sched.lock.
    internal schedt_disable disable;
    // Global cache of dead G's.
    internal schedt_gFree gFree;
    // Central cache of sudog structs.
    internal mutex sudoglock;
    internal ж<sudog> sudogcache;
    // Central pool of available defer structs.
    internal mutex deferlock;
    internal ж<_defer> deferpool;
    // freem is the list of m's waiting to be freed when their
    // m.exited is set. Linked through m.freelink.
    internal ж<m> freem;
    internal atomic.Bool gcwaiting; // gc is waiting to run
    internal int32 stopwait;
    internal note stopnote;
    internal atomic.Bool sysmonwait;
    internal note sysmonnote;
    // safePointFn should be called on each P at the next GC
    // safepoint if p.runSafePointFn is set.
    internal Action<ж<Δp>> safePointFn;
    internal int32 safePointWait;
    internal note safePointNote;
    internal int32 profilehz; // cpu profiling rate
    internal int64 procresizetime; // nanotime() of last change to gomaxprocs
    internal int64 totaltime; // ∫gomaxprocs dt up to procresizetime
    // sysmonlock protects sysmon's actions on the runtime.
    //
    // Acquire and hold this mutex to block sysmon from interacting
    // with the rest of the runtime.
    internal mutex sysmonlock;
    // timeToRun is a distribution of scheduling latencies, defined
    // as the sum of time a G spends in the _Grunnable state before
    // it transitions to _Grunning.
    internal timeHistogram timeToRun;
    // idleTime is the total CPU time Ps have "spent" idle.
    //
    // Reset on each GC cycle.
    internal atomic.Int64 idleTime;
    // totalMutexWaitTime is the sum of time goroutines have spent in _Gwaiting
    // with a waitreason of the form waitReasonSync{RW,}Mutex{R,}Lock.
    internal atomic.Int64 totalMutexWaitTime;
    // stwStoppingTimeGC/Other are distributions of stop-the-world stopping
    // latencies, defined as the time taken by stopTheWorldWithSema to get
    // all Ps to stop. stwStoppingTimeGC covers all GC-related STWs,
    // stwStoppingTimeOther covers the others.
    internal timeHistogram stwStoppingTimeGC;
    internal timeHistogram stwStoppingTimeOther;
    // stwTotalTimeGC/Other are distributions of stop-the-world total
    // latencies, defined as the total time from stopTheWorldWithSema to
    // startTheWorldWithSema. This is a superset of
    // stwStoppingTimeGC/Other. stwTotalTimeGC covers all GC-related STWs,
    // stwTotalTimeOther covers the others.
    internal timeHistogram stwTotalTimeGC;
    internal timeHistogram stwTotalTimeOther;
    // totalRuntimeLockWaitTime (plus the value of lockWaitTime on each M in
    // allm) is the sum of time goroutines have spent in _Grunnable and with an
    // M, but waiting for locks within the runtime. This field stores the value
    // for Ms that have exited.
    internal atomic.Int64 totalRuntimeLockWaitTime;
}

// Values for the flags field of a sigTabT.
internal static readonly UntypedInt _SigNotify = /* 1 << iota */ 1; // let signal.Notify have signal, even if from kernel

internal static readonly UntypedInt _SigKill = 2; // if signal.Notify doesn't take it, exit quietly

internal static readonly UntypedInt _SigThrow = 4; // if signal.Notify doesn't take it, exit loudly

internal static readonly UntypedInt _SigPanic = 8; // if the signal is from the kernel, panic

internal static readonly UntypedInt _SigDefault = 16; // if the signal isn't explicitly requested, don't monitor it

internal static readonly UntypedInt _SigGoExit = 32; // cause all runtime procs to exit (only used on Plan 9).

internal static readonly UntypedInt _SigSetStack = 64; // Don't explicitly install handler, but add SA_ONSTACK to existing libc handler

internal static readonly UntypedInt _SigUnblock = 128; // always unblock; see blockableSig

internal static readonly UntypedInt _SigIgn = 256; // _SIG_DFL action is to ignore the signal

// Layout of in-memory per-function information prepared by linker
// See https://golang.org/s/go12symtab.
// Keep in sync with linker (../cmd/link/internal/ld/pcln.go:/pclntab)
// and with package debug/gosym and with symtab.go in package runtime.
[GoType] partial struct _func {
    public partial ref @internal.runtime.sys_package.NotInHeap NotInHeap { get; } // Only in static data
    internal uint32 entryOff; // start pc, as offset from moduledata.text/pcHeader.textStart
    internal int32 nameOff;  // function name, as index into moduledata.funcnametab.
    internal int32 args;  // in/out args size
    internal uint32 deferreturn; // offset of start of a deferreturn call instruction from entry, if any.
    internal uint32 pcsp;
    internal uint32 pcfile;
    internal uint32 pcln;
    internal uint32 npcdata;
    internal uint32 cuOffset;     // runtime.cutab offset of this function's CU
    internal int32 startLine;      // line number of start of function (func keyword/TEXT directive)
    internal abi.FuncID funcID; // set for certain special runtime functions
    internal abi.FuncFlag flag;
    internal array<byte> _ = new(1); // pad
    internal uint8 nfuncdata;   // must be last, must end on a uint32-aligned boundary
}

// The end of the struct is followed immediately by two variable-length
// arrays that reference the pcdata and funcdata locations for this
// function.
// pcdata contains the offset into moduledata.pctab for the start of
// that index's table. e.g.,
// &moduledata.pctab[_func.pcdata[_PCDATA_UnsafePoint]] is the start of
// the unsafe point table.
//
// An offset of 0 indicates that there is no table.
//
// pcdata [npcdata]uint32
// funcdata contains the offset past moduledata.gofunc which contains a
// pointer to that index's funcdata. e.g.,
// *(moduledata.gofunc +  _func.funcdata[_FUNCDATA_ArgsPointerMaps]) is
// the argument pointer map.
//
// An offset of ^uint32(0) indicates that there is no entry.
//
// funcdata [nfuncdata]uint32

// Pseudo-Func that is returned for PCs that occur in inlined code.
// A *Func can be either a *_func or a *funcinl, and they are distinguished
// by the first uintptr.
//
// TODO(austin): Can we merge this with inlinedCall?
[GoType] partial struct funcinl {
    internal uint32 ones;  // set to ^0 to distinguish from _func
    internal uintptr entry; // entry of the real (the "outermost") frame
    internal @string name;
    internal @string @file;
    internal int32 line;
    internal int32 startLine;
}

// Lock-free stack node.
// Also known to export_test.go.
[GoType] partial struct lfnode {
    internal uint64 next;
    internal uintptr pushcnt;
}

[GoType] partial struct forcegcstate {
    internal mutex @lock;
    internal ж<g> g;
    internal atomic.Bool idle;
}

// A _defer holds an entry on the list of deferred calls.
// If you add a field here, add code to clear it in deferProcStack.
// This struct must match the code in cmd/compile/internal/ssagen/ssa.go:deferstruct
// and cmd/compile/internal/ssagen/ssa.go:(*state).call.
// Some defers will be allocated on the stack and some on the heap.
// All defers are logically part of the stack, so write barriers to
// initialize them are not required. All defers must be manually scanned,
// and for heap defers, marked.
[GoType] partial struct _defer {
    internal bool heap;
    internal bool rangefunc;    // true for rangefunc list
    internal uintptr sp; // sp at time of defer
    internal uintptr pc; // pc at time of defer
    internal Action fn;  // can be nil for open-coded defers
    internal ж<_defer> link; // next defer on G; can point to either heap or stack!
    // If rangefunc is true, *head is the head of the atomic linked list
    // during a range-over-func execution.
    internal ж<atomic.Pointer<_defer>> head;
}

// A _panic holds information about an active panic.
//
// A _panic value must only ever live on the stack.
//
// The argp and link fields are stack pointers, but don't need special
// handling during stack growth: because they are pointer-typed and
// _panic values only live on the stack, regular stack pointer
// adjustment takes care of them.
[GoType] partial struct _panic {
    internal @unsafe.Pointer argp; // pointer to arguments of deferred call run during panic; cannot move - known to liblink
    internal any arg;            // argument to panic
    internal ж<_panic> link;     // link to earlier panic
    // startPC and startSP track where _panic.start was called.
    internal uintptr startPC;
    internal @unsafe.Pointer startSP;
    // The current stack frame that we're running deferred calls for.
    internal @unsafe.Pointer sp;
    internal uintptr lr;
    internal @unsafe.Pointer fp;
    // retpc stores the PC where the panic should jump back to, if the
    // function last returned by _panic.next() recovers the panic.
    internal uintptr retpc;
    // Extra state for handling open-coded defers.
    internal ж<uint8> deferBitsPtr;
    internal @unsafe.Pointer slotsPtr;
    internal bool recovered; // whether this panic has been recovered
    internal bool goexit;
    internal bool deferreturn;
}

// savedOpenDeferState tracks the extra state from _panic that's
// necessary for deferreturn to pick up where gopanic left off,
// without needing to unwind the stack.
[GoType] partial struct savedOpenDeferState {
    internal uintptr retpc;
    internal uintptr deferBitsOffset;
    internal uintptr slotsOffset;
}

// ancestorInfo records details of where a goroutine was started.
[GoType] partial struct ancestorInfo {
    internal slice<uintptr> pcs; // pcs from the stack of this goroutine
    internal uint64 goid;    // goroutine id of this goroutine; original goroutine possibly dead
    internal uintptr gopc;   // pc of go statement that created this goroutine
}

[GoType("num:uint8")] partial struct waitReason;

internal static readonly waitReason waitReasonZero = /* iota */ 0;                 // ""
internal static readonly waitReason waitReasonGCAssistMarking = 1;      // "GC assist marking"
internal static readonly waitReason waitReasonIOWait = 2;               // "IO wait"
internal static readonly waitReason waitReasonChanReceiveNilChan = 3;   // "chan receive (nil chan)"
internal static readonly waitReason waitReasonChanSendNilChan = 4;      // "chan send (nil chan)"
internal static readonly waitReason waitReasonDumpingHeap = 5;          // "dumping heap"
internal static readonly waitReason waitReasonGarbageCollection = 6;    // "garbage collection"
internal static readonly waitReason waitReasonGarbageCollectionScan = 7; // "garbage collection scan"
internal static readonly waitReason waitReasonPanicWait = 8;            // "panicwait"
internal static readonly waitReason waitReasonSelect = 9;               // "select"
internal static readonly waitReason waitReasonSelectNoCases = 10;        // "select (no cases)"
internal static readonly waitReason waitReasonGCAssistWait = 11;         // "GC assist wait"
internal static readonly waitReason waitReasonGCSweepWait = 12;          // "GC sweep wait"
internal static readonly waitReason waitReasonGCScavengeWait = 13;       // "GC scavenge wait"
internal static readonly waitReason waitReasonChanReceive = 14;          // "chan receive"
internal static readonly waitReason waitReasonChanSend = 15;             // "chan send"
internal static readonly waitReason waitReasonFinalizerWait = 16;        // "finalizer wait"
internal static readonly waitReason waitReasonForceGCIdle = 17;          // "force gc (idle)"
internal static readonly waitReason waitReasonSemacquire = 18;           // "semacquire"
internal static readonly waitReason waitReasonSleep = 19;                // "sleep"
internal static readonly waitReason waitReasonSyncCondWait = 20;         // "sync.Cond.Wait"
internal static readonly waitReason waitReasonSyncMutexLock = 21;        // "sync.Mutex.Lock"
internal static readonly waitReason waitReasonSyncRWMutexRLock = 22;     // "sync.RWMutex.RLock"
internal static readonly waitReason waitReasonSyncRWMutexLock = 23;      // "sync.RWMutex.Lock"
internal static readonly waitReason waitReasonSyncWaitGroupWait = 24;    // "sync.WaitGroup.Wait"
internal static readonly waitReason waitReasonTraceReaderBlocked = 25;   // "trace reader (blocked)"
internal static readonly waitReason waitReasonWaitForGCCycle = 26;       // "wait for GC cycle"
internal static readonly waitReason waitReasonGCWorkerIdle = 27;         // "GC worker (idle)"
internal static readonly waitReason waitReasonGCWorkerActive = 28;       // "GC worker (active)"
internal static readonly waitReason waitReasonPreempted = 29;            // "preempted"
internal static readonly waitReason waitReasonDebugCall = 30;            // "debug call"
internal static readonly waitReason waitReasonGCMarkTermination = 31;    // "GC mark termination"
internal static readonly waitReason waitReasonStoppingTheWorld = 32;     // "stopping the world"
internal static readonly waitReason waitReasonFlushProcCaches = 33;      // "flushing proc caches"
internal static readonly waitReason waitReasonTraceGoroutineStatus = 34; // "trace goroutine status"
internal static readonly waitReason waitReasonTraceProcStatus = 35;      // "trace proc status"
internal static readonly waitReason waitReasonPageTraceFlush = 36;       // "page trace flush"
internal static readonly waitReason waitReasonCoroutine = 37;            // "coroutine"
internal static readonly waitReason waitReasonGCWeakToStrongWait = 38;   // "GC weak to strong wait"
internal static readonly waitReason waitReasonSynctestRun = 39;          // "synctest.Run"
internal static readonly waitReason waitReasonSynctestWait = 40;         // "synctest.Wait"
internal static readonly waitReason waitReasonSynctestChanReceive = 41;  // "chan receive (synctest)"
internal static readonly waitReason waitReasonSynctestChanSend = 42;     // "chan send (synctest)"
internal static readonly waitReason waitReasonSynctestSelect = 43;       // "select (synctest)"

internal static array<@string> waitReasonStrings = new golib.SparseArray<@string>{
    [waitReasonZero] = ""u8,
    [waitReasonGCAssistMarking] = "GC assist marking"u8,
    [waitReasonIOWait] = "IO wait"u8,
    [waitReasonChanReceiveNilChan] = "chan receive (nil chan)"u8,
    [waitReasonChanSendNilChan] = "chan send (nil chan)"u8,
    [waitReasonDumpingHeap] = "dumping heap"u8,
    [waitReasonGarbageCollection] = "garbage collection"u8,
    [waitReasonGarbageCollectionScan] = "garbage collection scan"u8,
    [waitReasonPanicWait] = "panicwait"u8,
    [waitReasonSelect] = "select"u8,
    [waitReasonSelectNoCases] = "select (no cases)"u8,
    [waitReasonGCAssistWait] = "GC assist wait"u8,
    [waitReasonGCSweepWait] = "GC sweep wait"u8,
    [waitReasonGCScavengeWait] = "GC scavenge wait"u8,
    [waitReasonChanReceive] = "chan receive"u8,
    [waitReasonChanSend] = "chan send"u8,
    [waitReasonFinalizerWait] = "finalizer wait"u8,
    [waitReasonForceGCIdle] = "force gc (idle)"u8,
    [waitReasonSemacquire] = "semacquire"u8,
    [waitReasonSleep] = "sleep"u8,
    [waitReasonSyncCondWait] = "sync.Cond.Wait"u8,
    [waitReasonSyncMutexLock] = "sync.Mutex.Lock"u8,
    [waitReasonSyncRWMutexRLock] = "sync.RWMutex.RLock"u8,
    [waitReasonSyncRWMutexLock] = "sync.RWMutex.Lock"u8,
    [waitReasonSyncWaitGroupWait] = "sync.WaitGroup.Wait"u8,
    [waitReasonTraceReaderBlocked] = "trace reader (blocked)"u8,
    [waitReasonWaitForGCCycle] = "wait for GC cycle"u8,
    [waitReasonGCWorkerIdle] = "GC worker (idle)"u8,
    [waitReasonGCWorkerActive] = "GC worker (active)"u8,
    [waitReasonPreempted] = "preempted"u8,
    [waitReasonDebugCall] = "debug call"u8,
    [waitReasonGCMarkTermination] = "GC mark termination"u8,
    [waitReasonStoppingTheWorld] = "stopping the world"u8,
    [waitReasonFlushProcCaches] = "flushing proc caches"u8,
    [waitReasonTraceGoroutineStatus] = "trace goroutine status"u8,
    [waitReasonTraceProcStatus] = "trace proc status"u8,
    [waitReasonPageTraceFlush] = "page trace flush"u8,
    [waitReasonCoroutine] = "coroutine"u8,
    [waitReasonGCWeakToStrongWait] = "GC weak to strong wait"u8,
    [waitReasonSynctestRun] = "synctest.Run"u8,
    [waitReasonSynctestWait] = "synctest.Wait"u8,
    [waitReasonSynctestChanReceive] = "chan receive (synctest)"u8,
    [waitReasonSynctestChanSend] = "chan send (synctest)"u8,
    [waitReasonSynctestSelect] = "select (synctest)"u8
}.array();

internal static @string String(this waitReason w) {
    if (w < 0 || w >= ((waitReason)(uint8)len(waitReasonStrings))) {
        return "unknown wait reason"u8;
    }
    return waitReasonStrings[w];
}

internal static bool isMutexWait(this waitReason w) {
    return w == waitReasonSyncMutexLock || w == waitReasonSyncRWMutexRLock || w == waitReasonSyncRWMutexLock;
}

internal static bool isWaitingForSuspendG(this waitReason w) {
    return ΔisWaitingForSuspendG[w];
}

// isWaitingForSuspendG indicates that a goroutine is only entering _Gwaiting and
// setting a waitReason because it needs to be able to let the suspendG
// (used by the GC and the execution tracer) take ownership of its stack.
// The G is always actually executing on the system stack in these cases.
//
// TODO(mknyszek): Consider replacing this with a new dedicated G status.
internal static array<bool> ΔisWaitingForSuspendG = new golib.SparseArray<bool>{
    [waitReasonStoppingTheWorld] = true,
    [waitReasonGCMarkTermination] = true,
    [waitReasonGarbageCollection] = true,
    [waitReasonGarbageCollectionScan] = true,
    [waitReasonTraceGoroutineStatus] = true,
    [waitReasonTraceProcStatus] = true,
    [waitReasonPageTraceFlush] = true,
    [waitReasonGCAssistMarking] = true,
    [waitReasonGCWorkerActive] = true,
    [waitReasonFlushProcCaches] = true
}.array();

internal static bool isIdleInSynctest(this waitReason w) {
    return ΔisIdleInSynctest[w];
}

// isIdleInSynctest indicates that a goroutine is considered idle by synctest.Wait.
internal static array<bool> ΔisIdleInSynctest = new golib.SparseArray<bool>{
    [waitReasonChanReceiveNilChan] = true,
    [waitReasonChanSendNilChan] = true,
    [waitReasonSelectNoCases] = true,
    [waitReasonSleep] = true,
    [waitReasonSyncCondWait] = true,
    [waitReasonSyncWaitGroupWait] = true,
    [waitReasonCoroutine] = true,
    [waitReasonSynctestRun] = true,
    [waitReasonSynctestWait] = true,
    [waitReasonSynctestChanReceive] = true,
    [waitReasonSynctestChanSend] = true,
    [waitReasonSynctestSelect] = true
}.array(44);

internal static ж<ж<m>> Ꮡallm = new StandardBox<ж<m>>(default(ж<m>));
internal static ref ж<m> allm => ref Ꮡallm.ValueSlot;
// Hand-adjusted (this file carries the GoManualConversion marker above so a reconvert keeps this
// seed): in Go these are populated by
// the runtime bootstrap (osinit/schedinit), which converted code does not run — .NET is the
// runtime. Left zero they break consumers at runtime while compiling clean (sync.Pool's pinSlow
// sizes its poolLocal array from runtime.GOMAXPROCS(0) → make(…, 0) → index out of range on the
// first fmt.Println). Seed them from the real environment instead.
internal static ж<int32> Ꮡgomaxprocs = new StandardBox<int32>((int32)Environment.ProcessorCount);
internal static ref int32 gomaxprocs => ref Ꮡgomaxprocs.Value;
internal static int32 ncpu = (int32)Environment.ProcessorCount;
internal static ж<forcegcstate> Ꮡforcegc = new StandardBox<forcegcstate>(default(forcegcstate));
internal static ref forcegcstate forcegc => ref Ꮡforcegc.Value;
internal static ж<schedt> Ꮡsched = new StandardBox<schedt>(default(schedt));
internal static ref schedt sched => ref Ꮡsched.Value;
internal static int32 newprocs;

internal static ж<mutex> ᏑallpLock = new StandardBox<mutex>(new mutex(nil));
internal static ref mutex allpLock => ref ᏑallpLock.Value;
internal static ж<slice<ж<Δp>>> Ꮡallp = new StandardBox<slice<ж<Δp>>>(default(slice<ж<Δp>>));
internal static ref slice<ж<Δp>> allp => ref Ꮡallp.ValueSlot;
internal static pMask idlepMask;
internal static pMask timerpMask;

// goarmsoftfp is used by runtime/cgo assembly.
//
//go:linkname goarmsoftfp
internal static ж<lfstack> ᏑgcBgMarkWorkerPool = new StandardBox<lfstack>(default(lfstack));
internal static ref lfstack gcBgMarkWorkerPool => ref ᏑgcBgMarkWorkerPool.Value;
internal static int32 gcBgMarkWorkerCount;
internal static uint32 processorVersionInfo;
internal static bool isIntel;

// set by cmd/link on arm systems
// accessed using linkname by internal/runtime/atomic.
//
// goarm should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/creativeprojects/go-selfupdate
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname goarm
internal static uint8 goarm;

internal static uint8 goarmsoftfp;

// Set by the linker so the runtime can determine the buildmode.
internal static bool islibrary; // -buildmode=c-shared

internal static bool isarchive; // -buildmode=c-archive

// Must agree with internal/buildcfg.FramePointerEnabled.
internal const bool framepointer_enabled = /* GOARCH == "amd64" || GOARCH == "arm64" */ true;

// getcallerfp returns the frame pointer of the caller of the caller
// of this function.
//
//go:nosplit
//go:noinline
internal static uintptr getcallerfp() {
    var fp = getfp();
    // This frame's FP.
    if (fp != 0) {
        fp = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)fp);
        // The caller's FP.
        fp = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)fp);
    }
    // The caller's caller's FP.
    return fp;
}

} // end runtime_package
