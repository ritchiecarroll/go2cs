// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1 || windows
namespace go;

using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// Integrated network poller (platform-independent part).
// A particular implementation (epoll/kqueue/port/AIX/Windows)
// must define the following functions:
//
// func netpollinit()
//     Initialize the poller. Only called once.
//
// func netpollopen(fd uintptr, pd *pollDesc) int32
//     Arm edge-triggered notifications for fd. The pd argument is to pass
//     back to netpollready when fd is ready. Return an errno value.
//
// func netpollclose(fd uintptr) int32
//     Disable notifications for fd. Return an errno value.
//
// func netpoll(delta int64) (gList, int32)
//     Poll the network. If delta < 0, block indefinitely. If delta == 0,
//     poll without blocking. If delta > 0, block for up to delta nanoseconds.
//     Return a list of goroutines built by calling netpollready,
//     and a delta to add to netpollWaiters when all goroutines are ready.
//     This must never return an empty list with a non-zero delta.
//
// func netpollBreak()
//     Wake up the network poller, assumed to be blocked in netpoll.
//
// func netpollIsPollDescriptor(fd uintptr) bool
//     Reports whether fd is a file descriptor used by the poller.

// Error codes returned by runtime_pollReset and runtime_pollWait.
// These must match the values in internal/poll/fd_poll_runtime.go.
internal static UntypedInt pollNoError => 0; // no error

internal static UntypedInt pollErrClosing => 1; // descriptor is closed

internal static UntypedInt pollErrTimeout => 2; // I/O timeout

internal static UntypedInt pollErrNotPollable => 3; // general error polling descriptor

// pollDesc contains 2 binary semaphores, rg and wg, to park reader and writer
// goroutines respectively. The semaphore can be in the following states:
//
//	pdReady - io readiness notification is pending;
//	          a goroutine consumes the notification by changing the state to pdNil.
//	pdWait - a goroutine prepares to park on the semaphore, but not yet parked;
//	         the goroutine commits to park by changing the state to G pointer,
//	         or, alternatively, concurrent io notification changes the state to pdReady,
//	         or, alternatively, concurrent timeout/close changes the state to pdNil.
//	G pointer - the goroutine is blocked on the semaphore;
//	            io notification or timeout/close changes the state to pdReady or pdNil respectively
//	            and unparks the goroutine.
//	pdNil - none of the above.
internal static uintptr pdNil => 0;

internal static uintptr pdReady => 1;

internal static uintptr pdWait => 2;

internal static UntypedInt pollBlockSize => /* 4 * 1024 */ 4096;

// Network poller descriptor.
//
// No heap pointers.
partial struct pollDesc {
    internal sys.NotInHeap _;
    internal ж<pollDesc> link;   // in pollcache, protected by pollcache.lock
    internal uintptr fd;        // constant for pollDesc usage lifetime
    internal atomic.Uintptr fdseq; // protects against stale pollDesc
    // atomicInfo holds bits from closing, rd, and wd,
    // which are only ever written while holding the lock,
    // summarized for use by netpollcheckerr,
    // which cannot acquire the lock.
    // After writing these fields under lock in a way that
    // might change the summary, code must call publishInfo
    // before releasing the lock.
    // Code that changes fields and then calls netpollunblock
    // (while still holding the lock) must call publishInfo
    // before calling netpollunblock, because publishInfo is what
    // stops netpollblock from blocking anew
    // (by changing the result of netpollcheckerr).
    // atomicInfo also holds the eventErr bit,
    // recording whether a poll event on the fd got an error;
    // atomicInfo is the only source of truth for that bit.
    internal atomic.Uint32 atomicInfo; // atomic pollInfo
    // rg, wg are accessed atomically and hold g pointers.
    // (Using atomic.Uintptr here is similar to using guintptr elsewhere.)
    internal atomic.Uintptr rg; // pdReady, pdWait, G waiting for read or pdNil
    internal atomic.Uintptr wg; // pdReady, pdWait, G waiting for write or pdNil
    internal mutex @lock; // protects the following fields
    internal bool closing;
    internal bool rrun;      // whether rt is running
    internal bool wrun;      // whether wt is running
    internal uint32 user;    // user settable cookie
    internal uintptr rseq;   // protects from stale read timers
    internal timer rt;     // read deadline timer
    internal int64 rd;     // read deadline (a nanotime in the future, -1 when expired)
    internal uintptr wseq;   // protects from stale write timers
    internal timer wt;     // write deadline timer
    internal int64 wd;     // write deadline (a nanotime in the future, -1 when expired)
    internal ж<pollDesc> self; // storage for indirect interface. See (*pollDesc).makeArg.
}

partial struct pollInfo /*num:uint32*/;

internal static UntypedInt pollClosing => /* 1 << iota */ 1;
internal static UntypedInt pollEventErr => 2;
internal static UntypedInt pollExpiredReadDeadline => 4;
internal static UntypedInt pollExpiredWriteDeadline => 8;
internal static UntypedInt pollFDSeq => 16; // 20 bit field, low 20 bits of fdseq field

internal static UntypedInt pollFDSeqBits => 20;    // number of bits in pollFDSeq
internal static UntypedInt pollFDSeqMask => /* 1<<pollFDSeqBits - 1 */ 1048575; // mask for pollFDSeq

internal static bool closing(this pollInfo i) {
    return (pollInfo)(i & (uint32)pollClosing) != 0;
}

internal static bool eventErr(this pollInfo i) {
    return (pollInfo)(i & (uint32)pollEventErr) != 0;
}

internal static bool expiredReadDeadline(this pollInfo i) {
    return (pollInfo)(i & (uint32)pollExpiredReadDeadline) != 0;
}

internal static bool expiredWriteDeadline(this pollInfo i) {
    return (pollInfo)(i & (uint32)pollExpiredWriteDeadline) != 0;
}

// info returns the pollInfo corresponding to pd.
internal static pollInfo info(this ж<pollDesc> Ꮡpd) {
    return ((pollInfo)Ꮡpd.of(pollDesc.ᏑatomicInfo).Load());
}

// publishInfo updates pd.atomicInfo (returned by pd.info)
// using the other values in pd.
// It must be called while holding pd.lock,
// and it must be called after changing anything
// that might affect the info bits.
// In practice this means after changing closing
// or changing rd or wd from < 0 to >= 0.
internal static void publishInfo(this ж<pollDesc> Ꮡpd) {
    ref var pd = ref Ꮡpd.DerefOrNull();

    uint32 info = default!;
    if (pd.closing) {
        info |= (uint32)(pollClosing);
    }
    if (pd.rd < 0) {
        info |= (uint32)(pollExpiredReadDeadline);
    }
    if (pd.wd < 0) {
        info |= (uint32)(pollExpiredWriteDeadline);
    }
    info |= (uint32)(((uint32)((uintptr)(Ꮡpd.of(pollDesc.Ꮡfdseq).Load() & (uintptr)pollFDSeqMask)) << (int)(pollFDSeq)));
    // Set all of x except the pollEventErr bit.
    var x = Ꮡpd.of(pollDesc.ᏑatomicInfo).Load();
    while (!Ꮡpd.of(pollDesc.ᏑatomicInfo).CompareAndSwap(x, (uint32)(((uint32)(x & (uint32)pollEventErr)) | info))) {
        x = Ꮡpd.of(pollDesc.ᏑatomicInfo).Load();
    }
}

// setEventErr sets the result of pd.info().eventErr() to b.
// We only change the error bit if seq == 0 or if seq matches pollFDSeq
// (issue #59545).
internal static void setEventErr(this ж<pollDesc> Ꮡpd, bool b, uintptr seq) {
    var mSeq = (uint32)((uintptr)(seq & (uintptr)pollFDSeqMask));
    var x = Ꮡpd.of(pollDesc.ᏑatomicInfo).Load();
    var xSeq = (uint32)(((x >> (int)(pollFDSeq))) & (uint32)pollFDSeqMask);
    if (seq != 0 && xSeq != mSeq) {
        return;
    }
    while (((uint32)(x & (uint32)pollEventErr) != 0) != b && !Ꮡpd.of(pollDesc.ᏑatomicInfo).CompareAndSwap(x, (uint32)(x ^ (uint32)pollEventErr))) {
        x = Ꮡpd.of(pollDesc.ᏑatomicInfo).Load();
        var xSeqΔ1 = (uint32)(((x >> (int)(pollFDSeq))) & (uint32)pollFDSeqMask);
        if (seq != 0 && xSeqΔ1 != mSeq) {
            return;
        }
    }
}

partial struct pollCache {
    internal mutex @lock;
    internal ж<pollDesc> first;
}

// PollDesc objects must be type-stable,
// because we can get ready notification from epoll/kqueue
// after the descriptor is closed/reused.
// Stale notifications are detected using seq variable,
// seq is incremented when deadlines are changed or descriptor is reused.
internal static ж<mutex> ᏑnetpollInitLock = new StandardBox<mutex>(new mutex(nil));
internal static ref mutex netpollInitLock => ref ᏑnetpollInitLock.Value;
internal static ж<atomic.Uint32> ᏑnetpollInited = new StandardBox<atomic.Uint32>(default(atomic.Uint32));
internal static ref atomic.Uint32 netpollInited => ref ᏑnetpollInited.Value;
internal static ж<pollCache> Ꮡpollcache = new StandardBox<pollCache>(new pollCache());
internal static ref pollCache pollcache => ref Ꮡpollcache.Value;
public static ж<atomic.Uint32> ᏑnetpollWaiters = new StandardBox<atomic.Uint32>(default(atomic.Uint32));
public static ref atomic.Uint32 netpollWaiters => ref ᏑnetpollWaiters.Value;

// netpollWaiters is accessed in tests
//go:linkname netpollWaiters

//go:linkname poll_runtime_pollServerInit internal/poll.runtime_pollServerInit
internal static void poll_runtime_pollServerInit() {
    netpollGenericInit();
}

// go2cs generated this placeholder — func netpollGenericInit is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static bool netpollinited() {
    return ᏑnetpollInited.Load() != 0;
}

//go:linkname poll_runtime_isPollServerDescriptor internal/poll.runtime_isPollServerDescriptor

// poll_runtime_isPollServerDescriptor reports whether fd is a
// descriptor being used by netpoll.
internal static bool poll_runtime_isPollServerDescriptor(uintptr fd) {
    return netpollIsPollDescriptor(fd);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeBlockedWriteOnˢ = "runtime: blocked write on free polldesc"u8;
internal static readonly @string runtimeBlockedReadOnFreeˢ = "runtime: blocked read on free polldesc"u8;

//go:linkname poll_runtime_pollOpen internal/poll.runtime_pollOpen
internal static (ж<pollDesc>, nint) poll_runtime_pollOpen(uintptr fd) {
    var pd = Ꮡpollcache.alloc();
    @lock(pd.of(pollDesc.Ꮡlock));
    var wg = pd.of(pollDesc.Ꮡwg).Load();
    if (wg != pdNil && wg != pdReady) {
        @throw(runtimeBlockedWriteOnˢ);
    }
    var rg = pd.of(pollDesc.Ꮡrg).Load();
    if (rg != pdNil && rg != pdReady) {
        @throw(runtimeBlockedReadOnFreeˢ);
    }
    pd.Value.fd = fd;
    if (pd.of(pollDesc.Ꮡfdseq).Load() == 0) {
        // The value 0 is special in setEventErr, so don't use it.
        pd.of(pollDesc.Ꮡfdseq).Store(1);
    }
    pd.Value.closing = false;
    pd.setEventErr(false, 0);
    pd.Value.rseq++;
    pd.of(pollDesc.Ꮡrg).Store(pdNil);
    pd.Value.rd = 0;
    pd.Value.wseq++;
    pd.of(pollDesc.Ꮡwg).Store(pdNil);
    pd.Value.wd = 0;
    pd.Value.self = pd;
    pd.publishInfo();
    unlock(pd.of(pollDesc.Ꮡlock));
    var errno = netpollopen(fd, pd);
    if (errno != 0) {
        Ꮡpollcache.free(pd);
        return (default!, (nint)errno);
    }
    return (pd, 0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeClosePolldescWOˢ = "runtime: close polldesc w/o unblock"u8;
internal static readonly @string runtimeBlockedWriteOnˢ2 = "runtime: blocked write on closing polldesc"u8;
internal static readonly @string runtimeBlockedReadOnˢ = "runtime: blocked read on closing polldesc"u8;

//go:linkname poll_runtime_pollClose internal/poll.runtime_pollClose
internal static void poll_runtime_pollClose(ж<pollDesc> Ꮡpd) {
    ref var pd = ref Ꮡpd.DerefOrNull();

    if (!pd.closing) {
        @throw(runtimeClosePolldescWOˢ);
    }
    var wg = Ꮡpd.of(pollDesc.Ꮡwg).Load();
    if (wg != pdNil && wg != pdReady) {
        @throw(runtimeBlockedWriteOnˢ2);
    }
    var rg = Ꮡpd.of(pollDesc.Ꮡrg).Load();
    if (rg != pdNil && rg != pdReady) {
        @throw(runtimeBlockedReadOnˢ);
    }
    netpollclose(pd.fd);
    Ꮡpollcache.free(Ꮡpd);
}

internal static void free(this ж<pollCache> Ꮡc, ж<pollDesc> Ꮡpd) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var pd = ref Ꮡpd.DerefOrNull();

    // pd can't be shared here, but lock anyhow because
    // that's what publishInfo documents.
    @lock(Ꮡpd.of(pollDesc.Ꮡlock));
    // Increment the fdseq field, so that any currently
    // running netpoll calls will not mark pd as ready.
    var fdseq = Ꮡpd.of(pollDesc.Ꮡfdseq).Load();
    fdseq = (uintptr)((fdseq + 1) & (uintptr)((1 << (int)(taggedPointerBits)) - 1));
    Ꮡpd.of(pollDesc.Ꮡfdseq).Store(fdseq);
    Ꮡpd.publishInfo();
    unlock(Ꮡpd.of(pollDesc.Ꮡlock));
    @lock(Ꮡc.of(pollCache.Ꮡlock));
    pd.link = c.first;
    c.first = Ꮡpd;
    unlock(Ꮡc.of(pollCache.Ꮡlock));
}

// poll_runtime_pollReset, which is internal/poll.runtime_pollReset,
// prepares a descriptor for polling in mode, which is 'r' or 'w'.
// This returns an error code; the codes are defined above.
//
//go:linkname poll_runtime_pollReset internal/poll.runtime_pollReset
internal static nint poll_runtime_pollReset(ж<pollDesc> Ꮡpd, nint mode) {
    nint errcode = netpollcheckerr(Ꮡpd, (int32)mode);
    if (errcode != pollNoError) {
        return errcode;
    }
    if (mode == (rune)'r'){
        Ꮡpd.of(pollDesc.Ꮡrg).Store(pdNil);
    } else 
    if (mode == (rune)'w') {
        Ꮡpd.of(pollDesc.Ꮡwg).Store(pdNil);
    }
    return pollNoError;
}

// poll_runtime_pollWait, which is internal/poll.runtime_pollWait,
// waits for a descriptor to be ready for reading or writing,
// according to mode, which is 'r' or 'w'.
// This returns an error code; the codes are defined above.
//
//go:linkname poll_runtime_pollWait internal/poll.runtime_pollWait
internal static nint poll_runtime_pollWait(ж<pollDesc> Ꮡpd, nint mode) {
    nint errcode = netpollcheckerr(Ꮡpd, (int32)mode);
    if (errcode != pollNoError) {
        return errcode;
    }
    // As for now only Solaris, illumos, AIX and wasip1 use level-triggered IO.
    if (GOOS == "solaris"u8 || GOOS == "illumos"u8 || GOOS == "aix"u8 || GOOS == "wasip1"u8) {
        netpollarm(ref (Ꮡpd).DerefOrNull(), mode);
    }
    while (!netpollblock(Ꮡpd, (int32)mode, false)) {
        errcode = netpollcheckerr(Ꮡpd, (int32)mode);
        if (errcode != pollNoError) {
            return errcode;
        }
    }
    // Can happen if timeout has fired and unblocked us,
    // but before we had a chance to run, timeout has been reset.
    // Pretend it has not happened and retry.
    return pollNoError;
}

//go:linkname poll_runtime_pollWaitCanceled internal/poll.runtime_pollWaitCanceled
internal static void poll_runtime_pollWaitCanceled(ж<pollDesc> Ꮡpd, nint mode) {
    // This function is used only on windows after a failed attempt to cancel
    // a pending async IO operation. Wait for ioready, ignore closing or timeouts.
    while (!netpollblock(Ꮡpd, (int32)mode, true)) {
    }
}

//go:linkname poll_runtime_pollSetDeadline internal/poll.runtime_pollSetDeadline
internal static void poll_runtime_pollSetDeadline(ж<pollDesc> Ꮡpd, int64 d, nint mode) {
    ref var pd = ref Ꮡpd.DerefOrNull();

    @lock(Ꮡpd.of(pollDesc.Ꮡlock));
    if (pd.closing) {
        unlock(Ꮡpd.of(pollDesc.Ꮡlock));
        return;
    }
    var (rd0, wd0) = (pd.rd, pd.wd);
    var combo0 = rd0 > 0 && rd0 == wd0;
    if (d > 0) {
        d += nanotime();
        if (d <= 0) {
            // If the user has a deadline in the future, but the delay calculation
            // overflows, then set the deadline to the maximum possible value.
            d = 9223372036854775807L;
        }
    }
    if (mode == (rune)'r' || mode == (rune)'r' + (rune)'w') {
        pd.rd = d;
    }
    if (mode == (rune)'w' || mode == (rune)'r' + (rune)'w') {
        pd.wd = d;
    }
    Ꮡpd.publishInfo();
    var combo = pd.rd > 0 && pd.rd == pd.wd;
    var rtf = netpollReadDeadline;
    if (combo) {
        rtf = netpollDeadline;
    }
    if (!pd.rrun){
        if (pd.rd > 0) {
            // Copy current seq into the timer arg.
            // Timer func will check the seq against current descriptor seq,
            // if they differ the descriptor was reused or timers were reset.
            Ꮡpd.of(pollDesc.Ꮡrt).modify(pd.rd, 0, rtf, Ꮡpd.makeArg(), pd.rseq);
            pd.rrun = true;
        }
    } else 
    if (pd.rd != rd0 || combo != combo0) {
        pd.rseq++; // invalidate current timers
        if (pd.rd > 0){
            Ꮡpd.of(pollDesc.Ꮡrt).modify(pd.rd, 0, rtf, Ꮡpd.makeArg(), pd.rseq);
        } else {
            Ꮡpd.of(pollDesc.Ꮡrt).stop();
            pd.rrun = false;
        }
    }
    if (!pd.wrun){
        if (pd.wd > 0 && !combo) {
            Ꮡpd.of(pollDesc.Ꮡwt).modify(pd.wd, 0, netpollWriteDeadline, Ꮡpd.makeArg(), pd.wseq);
            pd.wrun = true;
        }
    } else 
    if (pd.wd != wd0 || combo != combo0) {
        pd.wseq++; // invalidate current timers
        if (pd.wd > 0 && !combo){
            Ꮡpd.of(pollDesc.Ꮡwt).modify(pd.wd, 0, netpollWriteDeadline, Ꮡpd.makeArg(), pd.wseq);
        } else {
            Ꮡpd.of(pollDesc.Ꮡwt).stop();
            pd.wrun = false;
        }
    }
    // If we set the new deadline in the past, unblock currently pending IO if any.
    // Note that pd.publishInfo has already been called, above, immediately after modifying rd and wd.
    var delta = (int32)0;
    ж<g> rg = default!;
    ж<g> wg = default!;
    if (pd.rd < 0) {
        rg = netpollunblock(Ꮡpd, (rune)'r', false, ref delta);
    }
    if (pd.wd < 0) {
        wg = netpollunblock(Ꮡpd, (rune)'w', false, ref delta);
    }
    unlock(Ꮡpd.of(pollDesc.Ꮡlock));
    if (rg != nil) {
        netpollgoready(rg, 3);
    }
    if (wg != nil) {
        netpollgoready(wg, 3);
    }
    netpollAdjustWaiters(delta);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeUnblockOnClosingˢ = "runtime: unblock on closing polldesc"u8;

//go:linkname poll_runtime_pollUnblock internal/poll.runtime_pollUnblock
internal static void poll_runtime_pollUnblock(ж<pollDesc> Ꮡpd) {
    ref var pd = ref Ꮡpd.DerefOrNull();

    @lock(Ꮡpd.of(pollDesc.Ꮡlock));
    if (pd.closing) {
        @throw(runtimeUnblockOnClosingˢ);
    }
    pd.closing = true;
    pd.rseq++;
    pd.wseq++;
    ж<g> rg = default!;
    ж<g> wg = default!;
    Ꮡpd.publishInfo();
    var delta = (int32)0;
    rg = netpollunblock(Ꮡpd, (rune)'r', false, ref delta);
    wg = netpollunblock(Ꮡpd, (rune)'w', false, ref delta);
    if (pd.rrun) {
        Ꮡpd.of(pollDesc.Ꮡrt).stop();
        pd.rrun = false;
    }
    if (pd.wrun) {
        Ꮡpd.of(pollDesc.Ꮡwt).stop();
        pd.wrun = false;
    }
    unlock(Ꮡpd.of(pollDesc.Ꮡlock));
    if (rg != nil) {
        netpollgoready(rg, 3);
    }
    if (wg != nil) {
        netpollgoready(wg, 3);
    }
    netpollAdjustWaiters(delta);
}

// netpollready is called by the platform-specific netpoll function.
// It declares that the fd associated with pd is ready for I/O.
// The toRun argument is used to build a list of goroutines to return
// from netpoll. The mode argument is 'r', 'w', or 'r'+'w' to indicate
// whether the fd is ready for reading or writing or both.
//
// This returns a delta to apply to netpollWaiters.
//
// This may run while the world is stopped, so write barriers are not allowed.
//
//go:nowritebarrier
internal static int32 netpollready(ж<gList> ᏑtoRun, ж<pollDesc> Ꮡpd, int32 mode) {
    ref var toRun = ref ᏑtoRun.DerefOrNull();

    var delta = (int32)0;
    ж<g> rg = default!;
    ж<g> wg = default!;
    if (mode == (rune)'r' || mode == (rune)'r' + (rune)'w') {
        rg = netpollunblock(Ꮡpd, (rune)'r', true, ref delta);
    }
    if (mode == (rune)'w' || mode == (rune)'r' + (rune)'w') {
        wg = netpollunblock(Ꮡpd, (rune)'w', true, ref delta);
    }
    if (rg != nil) {
        toRun.push(rg);
    }
    if (wg != nil) {
        toRun.push(wg);
    }
    return delta;
}

internal static nint netpollcheckerr(ж<pollDesc> Ꮡpd, int32 mode) {
    var info = Ꮡpd.info();
    if (info.closing()) {
        return pollErrClosing;
    }
    if ((mode == (rune)'r' && info.expiredReadDeadline()) || (mode == (rune)'w' && info.expiredWriteDeadline())) {
        return pollErrTimeout;
    }
    // Report an event scanning error only on a read event.
    // An error on a write event will be captured in a subsequent
    // write call that is able to report a more specific error.
    if (mode == (rune)'r' && info.eventErr()) {
        return pollErrNotPollable;
    }
    return pollNoError;
}

internal static bool netpollblockcommit(ж<g> Ꮡgp, @unsafe.Pointer gpp) {
    var r = atomic.Casuintptr((ж<uintptr>)(uintptr)(gpp), pdWait, (uintptr)Ꮡgp);
    if (r) {
        // Bump the count of goroutines waiting for the poller.
        // The scheduler uses this to decide whether to block
        // waiting for the poller if there is nothing else to do.
        netpollAdjustWaiters(1);
    }
    return r;
}

internal static void netpollgoready(ж<g> Ꮡgp, nint traceskip) {
    goready(Ꮡgp, traceskip + 1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeDoubleWaitˢ = "runtime: double wait"u8;
internal static readonly @string runtimeCorruptedPolldescˢ = "runtime: corrupted polldesc"u8;

// returns true if IO is ready, or false if timed out or closed
// waitio - wait only for completed IO, ignore errors
// Concurrent calls to netpollblock in the same mode are forbidden, as pollDesc
// can hold only a single waiting goroutine for each mode.
internal static bool netpollblock(ж<pollDesc> Ꮡpd, int32 mode, bool waitio) {
    var gpp = Ꮡpd.of(pollDesc.Ꮡrg);
    if (mode == (rune)'w') {
        gpp = Ꮡpd.of(pollDesc.Ꮡwg);
    }
    // set the gpp semaphore to pdWait
    while (ᐧ) {
        // Consume notification if already ready.
        if (gpp.CompareAndSwap(pdReady, pdNil)) {
            return true;
        }
        if (gpp.CompareAndSwap(pdNil, pdWait)) {
            break;
        }
        // Double check that this isn't corrupt; otherwise we'd loop
        // forever.
        {
            var v = gpp.Load(); if (v != pdReady && v != pdNil) {
                @throw(runtimeDoubleWaitˢ);
            }
        }
    }
    // need to recheck error states after setting gpp to pdWait
    // this is necessary because runtime_pollUnblock/runtime_pollSetDeadline/deadlineimpl
    // do the opposite: store to closing/rd/wd, publishInfo, load of rg/wg
    if (waitio || netpollcheckerr(Ꮡpd, mode) == pollNoError) {
        gopark(netpollblockcommit, @unsafe.Pointer.FromPinnedBox(gpp), waitReasonIOWait, traceBlockNet, 5);
    }
    // be careful to not lose concurrent pdReady notification
    var old = gpp.Swap(pdNil);
    if (old > pdWait) {
        @throw(runtimeCorruptedPolldescˢ);
    }
    return old == pdReady;
}

// netpollunblock moves either pd.rg (if mode == 'r') or
// pd.wg (if mode == 'w') into the pdReady state.
// This returns any goroutine blocked on pd.{rg,wg}.
// It adds any adjustment to netpollWaiters to *delta;
// this adjustment should be applied after the goroutine has
// been marked ready.
internal static ж<g> netpollunblock(ж<pollDesc> Ꮡpd, int32 mode, bool ioready, ref int32 delta) {
    var gpp = Ꮡpd.of(pollDesc.Ꮡrg);
    if (mode == (rune)'w') {
        gpp = Ꮡpd.of(pollDesc.Ꮡwg);
    }
    while (ᐧ) {
        var old = gpp.Load();
        if (old == pdReady) {
            return default!;
        }
        if (old == pdNil && !ioready) {
            // Only set pdReady for ioready. runtime_pollWait
            // will check for timeout/cancel before waiting.
            return default!;
        }
        var @new = pdNil;
        if (ioready) {
            @new = pdReady;
        }
        if (gpp.CompareAndSwap(old, @new)) {
            if (old == pdWait){
                old = pdNil;
            } else 
            if (old != pdNil) {
                delta -= 1;
            }
            return (ж<g>)(uintptr)((@unsafe.Pointer)old);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeInconsistentReadˢ = "runtime: inconsistent read deadline"u8;
internal static readonly @string runtimeInconsistentWriteˢ = "runtime: inconsistent write deadline"u8;

internal static void netpolldeadlineimpl(ж<pollDesc> Ꮡpd, uintptr seq, bool read, bool write) {
    ref var pd = ref Ꮡpd.DerefOrNull();

    @lock(Ꮡpd.of(pollDesc.Ꮡlock));
    // Seq arg is seq when the timer was set.
    // If it's stale, ignore the timer event.
    var currentSeq = pd.rseq;
    if (!read) {
        currentSeq = pd.wseq;
    }
    if (seq != currentSeq) {
        // The descriptor was reused or timers were reset.
        unlock(Ꮡpd.of(pollDesc.Ꮡlock));
        return;
    }
    var delta = (int32)0;
    ж<g> rg = default!;
    if (read) {
        if (pd.rd <= 0 || !pd.rrun) {
            @throw(runtimeInconsistentReadˢ);
        }
        pd.rd = -1;
        Ꮡpd.publishInfo();
        rg = netpollunblock(Ꮡpd, (rune)'r', false, ref delta);
    }
    ж<g> wg = default!;
    if (write) {
        if (pd.wd <= 0 || !pd.wrun && !read) {
            @throw(runtimeInconsistentWriteˢ);
        }
        pd.wd = -1;
        Ꮡpd.publishInfo();
        wg = netpollunblock(Ꮡpd, (rune)'w', false, ref delta);
    }
    unlock(Ꮡpd.of(pollDesc.Ꮡlock));
    if (rg != nil) {
        netpollgoready(rg, 0);
    }
    if (wg != nil) {
        netpollgoready(wg, 0);
    }
    netpollAdjustWaiters(delta);
}

internal static void netpollDeadline(any arg, uintptr seq, int64 delta) {
    netpolldeadlineimpl(arg._<ж<pollDesc>>(), seq, true, true);
}

internal static void netpollReadDeadline(any arg, uintptr seq, int64 delta) {
    netpolldeadlineimpl(arg._<ж<pollDesc>>(), seq, true, false);
}

internal static void netpollWriteDeadline(any arg, uintptr seq, int64 delta) {
    netpolldeadlineimpl(arg._<ж<pollDesc>>(), seq, false, true);
}

// netpollAnyWaiters reports whether any goroutines are waiting for I/O.
internal static bool netpollAnyWaiters() {
    return ᏑnetpollWaiters.Load() > 0;
}

// netpollAdjustWaiters adds delta to netpollWaiters.
internal static void netpollAdjustWaiters(int32 delta) {
    if (delta != 0) {
        ᏑnetpollWaiters.Add(delta);
    }
}

internal static ж<pollDesc> alloc(this ж<pollCache> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    @lock(Ꮡc.of(pollCache.Ꮡlock));
    if (c.first == nil) {
        uintptr pdSize = /* unsafe.Sizeof(pollDesc{}) */ 280;
        var n = (uintptr)pollBlockSize / pdSize;
        if (n == 0) {
            n = 1;
        }
        // Must be in non-GC memory because can be referenced
        // only from epoll/kqueue internals.
        @unsafe.Pointer mem = (uintptr)persistentalloc(n * pdSize, 0, Ꮡmemstats.of(mstats.Ꮡother_sys));
        for (var i = (uintptr)0; i < n; i++) {
            var pdΔ1 = (ж<pollDesc>)(uintptr)(add(mem, i * pdSize));
            lockInit(pdΔ1.of(pollDesc.Ꮡlock), lockRankPollDesc);
            pdΔ1.of(pollDesc.Ꮡrt).init(default!, default!);
            pdΔ1.of(pollDesc.Ꮡwt).init(default!, default!);
            pdΔ1.Value.link = c.first;
            c.first = pdΔ1;
        }
    }
    var pd = c.first;
    c.first = pd.Value.link;
    unlock(Ꮡc.of(pollCache.Ꮡlock));
    return pd;
}

// makeArg converts pd to an interface{}.
// makeArg does not do any allocation. Normally, such
// a conversion requires an allocation because pointers to
// types which embed internal/runtime/sys.NotInHeap (which pollDesc is)
// must be stored in interfaces indirectly. See issue 42076.
internal static any /*i*/ makeArg(this ж<pollDesc> Ꮡpd) {
    ref var i = ref heap<any>(out var Ꮡi);

    var x = Ꮡi.Reinterpret<any, eface>();
    x.Value._type = pdType;
    x.Value.data = @unsafe.Pointer.FromBox(Ꮡpd.of(pollDesc.Ꮡself));
    return i;
}

internal static ж<any> ᏑpdEface = new StandardBox<any>(((ж<pollDesc>)nil));
internal static ref any pdEface => ref ᏑpdEface.ValueSlot;
internal static ж<_type> pdType = (~efaceOf(ᏑpdEface))._type;

} // end runtime_package
