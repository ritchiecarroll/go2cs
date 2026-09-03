// runtime_netpoll_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Hand-written implementations of internal/poll's TEN //go:linkname entry points into the Go
// runtime's network poller (fd_poll_runtime.go) for the LINUX flavor: the READINESS poller -- one
// epoll instance, one background drain thread, and the managed descriptor state machine the Windows
// flavor built and gated (windows/runtime_netpoll_impl.cs), lifted verbatim. Design:
// docs/phase4/DESIGN-linux-readiness-poller.md (RATIFIED 2026-08-22, all nine OQs as recommended);
// its §2 contract inventory and §5 deadline story are inherited from
// docs/phase4/DESIGN-netpoll-managed-poller.md and not re-argued here.
//
// HISTORY, KEPT BECAUSE IT IS THE MEASURED BILL. This file was first the FALLBACK poller (the
// poll-seam lane, 2026-08-22): pollServerInit a no-op and pollOpen answering EPERM for EVERY
// descriptor, so files, pipes and sockets all degraded to the blocking path -- Go's own regular-file
// behavior applied to everything. That flipped 28 Linux roster rows (every file open had died in the
// PartialStubGenerator's throwing stub before it), and it left sockets returning `operation not
// permitted` from FD.Init once the sockaddr mirror let Bind/Connect through -- measured on
// encoding/json's TestHTTPDecoding (`httptest: failed to listen on a port: listen tcp6 [::1]:0:
// operation not permitted`) and on crypto/tls's TestMain listener at e7800600d. The poller below
// makes pollOpen's EPERM arm what it is in Go: the KERNEL's answer for the descriptors epoll refuses
// (regular files and directories -- os.newFile discards it and restores blocking mode, exactly as
// before), and every other descriptor pollable.
//
// WHAT THIS IS, IN ONE PARAGRAPH (design §0). Go's netpoll_epoll.go with gopark/goready replaced by
// a monitor gate per descriptor. epoll_create1(EPOLL_CLOEXEC) once (§4.2). EPOLL_CTL_ADD with
// EPOLLIN|EPOLLOUT|EPOLLRDHUP|EPOLLET exactly as Go arms it -- EDGE-TRIGGERED, and §4.5 is the
// no-lost-edge argument in full: the consumer (linux/fd_unix.cs) waits only after the kernel has
// answered EAGAIN, so the buffer is empty/full at the wait and any future readiness is a new
// transition; prepare (pollReset) clears a stale Ready and a transition in that window is observed
// by the syscall itself; fdMutex admits one waiter per mode. epoll_event.data carries an opaque
// TOKEN, never a pointer and never the fd (§4.6: the kernel reissues fd numbers; a stale event's
// token no longer resolves). ONE background drain thread blocks in epoll_wait(-1) forever, retries
// EINTR, and per event sets Ready on the modes Go's mapping names and pulses the gate. Deadlines are
// the Windows §5 machinery -- sticky per-mode expiry, generation-checked System.Threading.Timer,
// "a deadline set in the past fires NOW against the current waiter", wake-without-ready for
// unblock -- MINUS the cancel-and-harvest dimension, because a Linux timeout wakes a thread that owns
// no in-flight kernel operation (§4.7). Every byte the kernel reads or writes is a NATIVE image
// (Marshal over AllocHGlobal) handed through the keystone syscall(2) binding as a uintptr -- no ж<T>
// address, no generated address-taking wrapper (§4.1; OQ-9: the safe form, so
// internal.poll.csproj's shared <AllowUnsafeBlocks> stays false and no regen is owed). No break
// eventfd (OQ-2): nothing needs to interrupt the drain thread -- deadlines and unblocks reach
// waiters directly, and EPOLL_CTL_ADD/DEL take effect inside an in-progress epoll_wait (S0 measured
// an ADD during a wait delivered 1 ms later with no break write). Regular files are refused by the
// kernel's EPERM alone (OQ-4; S0: EPERM for a regular file and a directory, 0 for a pipe and a
// socket). pollWaitCanceled has no Linux caller and is the shared wait loop anyway (OQ-5). A failed
// epoll_wait other than EINTR is Go's throw("runtime: netpoll failed") -- an unhandled exception on
// the drain thread, never catch-and-continue (OQ-3).
//
// WHAT CHANGES FOR os AND net, STATED (design §5): nothing is EDITED there. Pipes, FIFOs, ttys and
// sockets now arm (os.newFile's SetNonblock sticks because FD.Init succeeds), so a pipe Read parks on
// the gate instead of in read(2) and Close -> evict wakes it (the PipeCloseUnblocksRead behavioral
// program prints Go's `read unblocked: read |0: file already closed`), SetDeadline is honored on
// them, and net.Listen/Dial stop returning EPERM. Regular files and directories behave exactly as
// under the fallback (the same errno, now from the kernel). os/exec's child-side pipe ends stay
// blocking through os.File.Fd() -> SetBlocking (each pipe end is its own open file description); the
// epoll descriptor is CLOEXEC so no child inherits it; isPollServerDescriptor answers truthfully for
// it, which os/exec's TestExtraFiles relies on when it enumerates the parent's descriptors.
//
// SCOPE. Linux amd64: the struct epoll_event image below is amd64's PACKED 12 bytes (uint32 events
// at 0, uint64 data at 4), and pollServerInit refuses any other architecture rather than misread
// (arm64's record is 16 bytes unpacked). darwin keeps the pre-poller fallback as its remedy when that
// corpus builds -- kqueue is a separate design. Windows keeps its own poller in
// windows/runtime_netpoll_impl.cs, untouched: the desc machinery here is a COPY of it (OQ-7, the
// lock_sema/lock_futex per-GOOS-authority precedent) and the two files cite each other so they
// cannot drift silently; a hoist into a flat shared companion is a later leveling once both are
// measured.

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
// Aliased rather than imported wholesale: this file needs exactly two golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using Goroutine = go.golib.Goroutine;
using WaitReason = go.golib.WaitReason;
using Stopwatch = System.Diagnostics.Stopwatch;

// Hand-owned (no runtime_netpoll_impl.go exists, so a reconvert never regenerates this file);
// marked per the hand-own rules so a -stdlib run cannot emit a Go version over it.
[module: go.GoManualConversion]

namespace go.@internal;

partial class poll_package
{
    // ---- the kernel surface: private copies of the numbers and flags this file uses (design §3) ---
    // Kept local rather than read from the converted syscall flavor so the file's kernel contract is
    // visible in one place; each value is Linux amd64's (syscall/linux/zsysnum_linux_amd64.cs,
    // zerrors_linux_amd64.cs, internal/runtime/syscall/linux/defs_linux.cs).
    private const nuint SYS_EPOLL_WAIT = 232;
    private const nuint SYS_EPOLL_CTL = 233;
    private const nuint SYS_EPOLL_CREATE1 = 291;
    private const uint EPOLL_CLOEXEC = 0x80000;
    private const nuint EPOLL_CTL_ADD = 1;
    private const nuint EPOLL_CTL_DEL = 2;
    private const uint EPOLLIN = 0x1;
    private const uint EPOLLOUT = 0x4;
    private const uint EPOLLERR = 0x8;
    private const uint EPOLLHUP = 0x10;
    private const uint EPOLLRDHUP = 0x2000;
    private const uint EPOLLET = 0x80000000;
    private const nuint EINTR = 4;

    // struct epoll_event on linux/amd64: __attribute__((packed)) { uint32_t events; uint64_t data; }.
    // 12 bytes; data is at offset 4 and UNALIGNED, which Marshal.ReadInt64/WriteInt64 are specified
    // for (S0 measured the round trip through a 2-slot buffer).
    private const int epollEventSize = 12;
    private const int epollEventsOffset = 0;
    private const int epollDataOffset = 4;

    // Go's batch: `var events [128]syscall.EpollEvent` (runtime/netpoll_epoll.go).
    private const int drainBatch = 128;

    // The registration Go's netpollopen makes (netpoll_epoll.go:56): read, write, peer half-close,
    // edge-triggered. EPOLLHUP and EPOLLERR are always reported and need no registration.
    private const uint armEvents = EPOLLIN | EPOLLOUT | EPOLLRDHUP | EPOLLET;

    // The one descriptor this poller owns (OQ-2: no break eventfd), and the drain buffer -- both
    // process-lifetime, created once in pollServerInit. -1 until then.
    private static int epfd = -1;
    private static nint drainBuffer;

    // Per-mode state. Go splits the equivalent across pd.rg/pd.wg (the waiter slot), pd.rd/pd.wd (the
    // deadline), pd.rseq/pd.wseq (the stale-timer guard) and pd.rt/pd.wt (the runtime timer); the
    // fields below are the same five facts under one lock. COPIED from windows/runtime_netpoll_impl.cs
    // (OQ-7); keep the two in step.
    private sealed class ManagedPollMode
    {
        // An edge arrived for this mode and has not been consumed yet. Go's pdReady.
        internal bool Ready;
        // The deadline for this mode passed and has not since been re-set. STICKY: Go models it as
        // rd < 0 published into the info bits, and every subsequent prepare/wait in the mode returns
        // pollErrTimeout until a LATER SetDeadline rewrites the mode's deadline -- to zero (clears),
        // to the future (re-arms), or to the past (re-expires).
        internal bool Expired;
        // Invalidates timer callbacks that lost a race. Go's rseq/wseq, bumped on every deadline
        // change and on unblock, checked by the fired callback under the desc lock. Not optional in
        // managed land: Timer.Change/Dispose do not synchronize with an in-flight callback.
        internal long Generation;
        // The Generation the currently-armed Timer was armed under; a callback is inert unless the two
        // still agree.
        internal long ArmedGeneration = -1;
        // The desc this mode belongs to, so a timer callback can lock the gate its setters hold.
        internal ManagedPollDesc Owner = null!;
        // Absolute due time in the runtimeNano epoch, meaningful only while a deadline is armed. Kept
        // because System.Threading.Timer's due time is a ~49.7-day uint of milliseconds while Go's
        // ceiling is ~292 years: a longer deadline is armed at the ceiling and re-armed on fire.
        internal long DueNanos;
        // Created lazily on first arm, reused by Change (Go's pd.rt.modify), disposed at pollClose.
        internal Timer? Deadline;
    }

    private sealed class ManagedPollDesc
    {
        // ONE lock. The Windows design's §4.1 prices why lock-free is not owed here.
        internal readonly object Gate = new();
        // pollUnblock ran. Sticky for the desc's lifetime -- a fresh desc is allocated per pollOpen.
        internal bool Closing;
        // Go's pdEventErr info bit: the last event for this descriptor was EXACTLY EPOLLERR. Set AND
        // cleared on every event by the drain thread (netpoll_epoll.go: pd.setEventErr(ev.events ==
        // _EPOLLERR, tag)); consulted by netpollcheckerr for mode 'r' only -> pollErrNotPollable.
        // Absent from the Windows flavor because netpoll_windows.go never sets it.
        internal bool EventErr;
        internal readonly ManagedPollMode Read = new();
        internal readonly ManagedPollMode Write = new();

        internal ManagedPollDesc()
        {
            Read.Owner = this;
            Write.Owner = this;
        }

        // The descriptor this desc was opened for -- what EPOLL_CTL_ADD/DEL name.
        internal int Fd;
        // The ctx internal/poll holds and the epoll_event.data the kernel carries back (§4.6).
        internal uintptr Token;
    }

    // ctx token -> desc. Go returns the *pollDesc itself as a uintptr and defends staleness with
    // fdseq, because it REUSES pollDescs out of a pollcache. The managed side allocates a fresh desc
    // per open and retires the token at close, so there is no reuse for an ABA to exploit. Tokens
    // start at 1; 0 is internal/poll's own "no ctx" sentinel (pollDesc.runtimeCtx == 0 short-circuits
    // every pd call, fd_poll_runtime.cs), so it must never be minted.
    private static readonly ConcurrentDictionary<uintptr, ManagedPollDesc> pollTable = new();
    private static long nextPollToken;

    private static ManagedPollDesc? descFor(uintptr ctx) =>
        pollTable.TryGetValue(ctx, out ManagedPollDesc? desc) ? desc : null;

    // internal/poll's mode argument is the rune 'r', 'w', or 'r'+'w' (fd_poll_runtime.cs).
    private const nint pollModeRead = 'r';
    private const nint pollModeWrite = 'w';
    private const nint pollModeBoth = pollModeRead + pollModeWrite;

    private static ManagedPollMode modeState(ManagedPollDesc desc, nint mode) =>
        mode == pollModeWrite ? desc.Write : desc.Read;

    // ---- the keystone, and the one shape every kernel call here takes --------------------------
    // syscall.RawSyscall6 is the converted syscall flavor's public entry to the keystone
    // [LibraryImport("libc", "syscall")] binding (internal/runtime/syscall/linux/syscall_linux_impl.cs).
    // "Raw" because there is no scheduler to bracket with entersyscall/exitsyscall (both are no-ops
    // on this flavor); a thread blocked inside it is in preemptive GC mode and holds no managed
    // reference (§4.2). Every address handed through it is a native image owned by this file.
    private static nuint rawSyscall6(nuint trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, out uintptr r1)
    {
        (r1, _, go.syscall_package.Errno err) = go.syscall_package.RawSyscall6(trap, a1, a2, a3, a4, 0, 0);
        return (nuint)err;
    }

    private static uintptr fdArg(int fd) => (uintptr)(nuint)(uint)fd;

    // EPOLL_CTL_ADD/DEL with a 12-byte native epoll_event image (§4.1). DEL ignores the image but a
    // valid one is passed anyway -- Go passes &ev for both, and pre-2.6.9 kernels required it.
    private static nuint epollCtl(nuint op, int fd, uint events, uintptr token)
    {
        nint ev = Marshal.AllocHGlobal(epollEventSize);

        try
        {
            Marshal.WriteInt32(ev, epollEventsOffset, unchecked((int)events));
            Marshal.WriteInt64(ev, epollDataOffset, unchecked((long)(ulong)(nuint)token));
            return rawSyscall6(SYS_EPOLL_CTL, fdArg(epfd), op, fdArg(fd), (uintptr)ev, out _);
        }
        finally
        {
            Marshal.FreeHGlobal(ev);
        }
    }

    // ---- 1. runtime_pollServerInit ---------------------------------------------------------------

    // Go's netpollinit: create the epoll instance. The caller wraps it in serverInit.Do
    // (fd_poll_runtime.cs), so it runs once per process, on the first pollable FD.Init. A failure is
    // what it is in Go -- throw("runtime: epollcreate failed") -- and there is deliberately no
    // fallback to "un-armable for everyone": that would silently re-introduce the blocking
    // degradation this file exists to remove.
    internal static partial void runtime_pollServerInit()
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw new PlatformNotSupportedException(
                "runtime: netpoll (linux flavor) mirrors linux/amd64's packed 12-byte struct epoll_event; " +
                RuntimeInformation.ProcessArchitecture + " is not supported by this hand-own");
        }

        nuint errno = rawSyscall6(SYS_EPOLL_CREATE1, EPOLL_CLOEXEC, 0, 0, 0, out uintptr r1);

        if (errno != 0)
            throw new InvalidOperationException("runtime: epollcreate failed (errno " + errno + ")");

        epfd = (int)(nuint)r1;
        drainBuffer = Marshal.AllocHGlobal(drainBatch * epollEventSize);

        // Background: the process does not wait for it at exit, and it needs no shutdown signal
        // (OQ-2). Started AFTER epfd and the buffer exist, BEFORE pollServerInitialized is published.
        Thread drain = new(drainLoop)
        {
            IsBackground = true,
            Name = "go2cs-netpoll"
        };
        drain.Start();

        Volatile.Write(ref pollServerInitialized, true);
    }

    private static bool pollServerInitialized;

    // Go's netpoll(delta) with delta = -1 forever and no scheduler to hand the ready list to
    // (design §4.2). The per-event body: Go's mode mapping (netpoll_epoll.go:173-177), the eventErr
    // bit, Ready on the named modes, and a pulse -- netpollready -> netpollunblock(ioready) -> goready
    // collapsed to a Monitor.PulseAll under the desc's gate.
    private static void drainLoop()
    {
        // msec = -1: the kernel reads an int from the register, so all-ones is the value Go passes.
        uintptr forever = unchecked((nuint)(nint)(-1));

        while (true)
        {
            nuint errno = rawSyscall6(SYS_EPOLL_WAIT, fdArg(epfd), (uintptr)drainBuffer, (uintptr)(nuint)drainBatch, forever, out uintptr r1);

            // epoll_wait is NEVER restarted after a signal handler, SA_RESTART or not (signal(7)), so
            // EINTR is retried unconditionally -- Go: `if errno == _EINTR { goto retry }`. S0 measured
            // it RARE under the CLR (0 in 20 s of slices while the process spawned 22,819 children and
            // ran 7,758 gen0 GCs -- the runtime routes signals away from arbitrary threads), but a
            // never-restarted syscall is owed the retry regardless.
            if (errno == EINTR)
                continue;

            // OQ-3: any other errno on a valid epfd is a process-level invariant failure (EBADF: someone
            // closed the poller's descriptor; EINVAL: epfd is not an epoll fd; EFAULT: the buffer
            // moved -- it cannot, it is native). Go throws "runtime: netpoll failed"; here an unhandled
            // exception on this background thread terminates the process through the crash-report
            // path. Catch-and-continue would be strictly worse: every future waiter would park forever
            // on a gate nobody pulses.
            if (errno != 0)
                throw new InvalidOperationException("runtime: netpoll failed (epoll_wait errno " + errno + ")");

            int n = (int)(nuint)r1;

            for (int i = 0; i < n; i++)
            {
                uint events = unchecked((uint)Marshal.ReadInt32(drainBuffer, i * epollEventSize + epollEventsOffset));
                uintptr token = (uintptr)unchecked((ulong)Marshal.ReadInt64(drainBuffer, i * epollEventSize + epollDataOffset));

                // A token that no longer resolves is an event for a descriptor closed under us
                // (pollClose removed it); under ET it is not repeated and needs nothing (§4.6).
                ManagedPollDesc? desc = descFor(token);

                if (desc is null)
                    continue;

                bool readable = (events & (EPOLLIN | EPOLLRDHUP | EPOLLHUP | EPOLLERR)) != 0;
                bool writable = (events & (EPOLLOUT | EPOLLHUP | EPOLLERR)) != 0;

                lock (desc.Gate)
                {
                    desc.EventErr = events == EPOLLERR;

                    if (readable)
                        desc.Read.Ready = true;

                    if (writable)
                        desc.Write.Ready = true;

                    if (readable || writable)
                        Monitor.PulseAll(desc.Gate);
                }
            }
        }
    }

    // ---- 2. runtime_pollOpen ---------------------------------------------------------------------

    // Go's netpollopen: one epoll_ctl(EPOLL_CTL_ADD) (netpoll_epoll.go:49-59). Returns (ctx, 0) or
    // (0, errno); pollDesc.init converts a nonzero errno with errnoErr(syscall.Errno(errno))
    // (fd_poll_runtime.cs). For a regular file or a directory the kernel answers EPERM -- the exact
    // errno the fallback used to answer for everything -- and os.newFile discards it and restores
    // blocking mode; for a socket, net.netFD.init propagates it, so any OTHER errno here surfaces
    // from net.Listen/Dial as its real name.
    //
    // ORDER MATTERS (§4.6): the table entry is inserted BEFORE the ADD, because a readable descriptor
    // (an accepted connection with data already in flight) can deliver its first -- under ET, only --
    // edge before epoll_ctl returns, and the drain thread must be able to resolve the token at that
    // instant. On failure the entry is removed again.
    internal static partial (uintptr, nint) runtime_pollOpen(uintptr fd)
    {
        // internal/poll reaches pollOpen only through serverInit.Do(runtime_pollServerInit), so a
        // false here is a sequencing regression in converted code, not a user-reachable state.
        if (!Volatile.Read(ref pollServerInitialized))
            throw new InvalidOperationException("runtime: pollOpen before pollServerInit");

        ManagedPollDesc desc = new() { Fd = (int)(nuint)fd };

        // Interlocked, not the dictionary's own count: tokens must never be reused within a run.
        uintptr ctx = (uintptr)(nuint)(ulong)Interlocked.Increment(ref nextPollToken);
        desc.Token = ctx;
        pollTable[ctx] = desc;

        nuint errno = epollCtl(EPOLL_CTL_ADD, desc.Fd, armEvents, ctx);

        if (errno != 0)
        {
            pollTable.TryRemove(ctx, out _);
            return (0, (nint)errno);
        }

        return (ctx, 0);
    }

    // ---- 3. runtime_pollClose --------------------------------------------------------------------

    // Go's netpollclose: unregister. Legal only after unblock -- Go throws "runtime: close polldesc w/o
    // unblock" (runtime/netpoll.cs), and that assert guards internal/poll's OWN sequencing, which is
    // unchanged converted code: FD.Close calls pd.evict() before decref -> destroy -> pd.close().
    // Kept as an InvalidOperationException so a future sequencing regression is loud.
    //
    // The EPOLL_CTL_DEL is explicit and runs BEFORE close(2) by FD.destroy's own ordering ("Poller may
    // want to unregister fd in readiness notification mechanism, so this must be executed before
    // CloseFunc"). It is not optional: the kernel drops a closed fd from an epoll set only when its
    // LAST reference closes, and os.File.Fd()/os/exec dup descriptors into children, so without the
    // DEL a parent's closed socket could keep reporting edges through a child's copy. Its errno is
    // ignored, as poll_runtime_pollClose ignores netpollclose's. The table entry is removed LAST, so
    // no window exists in which the kernel can still deliver an event for a token that is gone.
    internal static partial void runtime_pollClose(uintptr ctx)
    {
        ManagedPollDesc? desc = descFor(ctx);

        if (desc is null)
            return;

        lock (desc.Gate)
        {
            if (!desc.Closing)
                throw new InvalidOperationException("runtime: close polldesc w/o unblock");

            stopDeadlineLocked(desc.Read);
            stopDeadlineLocked(desc.Write);

            desc.Read.Deadline?.Dispose();
            desc.Write.Deadline?.Dispose();
            desc.Read.Deadline = null;
            desc.Write.Deadline = null;
        }

        epollCtl(EPOLL_CTL_DEL, desc.Fd, 0, ctx);

        pollTable.TryRemove(ctx, out _);
    }

    // ---- 4. runtime_pollWait ---------------------------------------------------------------------

    // Block until an edge arrives in mode, or return the closing/deadline/eventErr code. Called by
    // every I/O wrapper in linux/fd_unix.cs after the kernel answered EAGAIN (design §2.1).
    internal static partial nint runtime_pollWait(uintptr ctx, nint mode)
    {
        ManagedPollDesc? desc = descFor(ctx);

        // The FD was closed under this caller. Go's fdseq answers a stale ctx the same way its
        // closing check does; pollErrClosing is the code the consumer is prepared for.
        if (desc is null)
            return pollErrClosing;

        return pollBlock(desc, mode, ignoreErrors: false);
    }

    // ---- 5. runtime_pollWaitCanceled -------------------------------------------------------------

    // Block until an edge arrives IGNORING deadline and closing. Windows' execIO cancel-and-harvest
    // path is its only caller in Go; nothing in linux/fd_unix.cs reaches it. OQ-5: the shared loop
    // rather than a throw -- it costs nothing, and it keeps the two flavors' machinery identical.
    internal static partial void runtime_pollWaitCanceled(uintptr ctx, nint mode)
    {
        ManagedPollDesc? desc = descFor(ctx);

        if (desc is null)
            return;

        pollBlock(desc, mode, ignoreErrors: true);
    }

    // The shared wait loop. Mirrors netpollblock + poll_runtime_pollWait's retry
    // (runtime/netpoll.cs) with the parking replaced by Monitor.Wait.
    private static nint pollBlock(ManagedPollDesc desc, nint mode, bool ignoreErrors)
    {
        ManagedPollMode m = modeState(desc, mode);

        lock (desc.Gate)
        {
            while (true)
            {
                // READINESS IS CONSUMED FIRST, ahead of every error check. Go does the same
                // (netpollblock consumes pdReady before netpollcheckerr) and it is a real behavior:
                // an edge that RACED the deadline is still delivered to the caller, matching Go's
                // preference for returning real IO over a same-instant timeout.
                if (m.Ready)
                {
                    m.Ready = false;
                    return pollNoError;
                }

                if (!ignoreErrors)
                {
                    // Check order is fixed (netpollcheckerr): closing, then timeout, then -- on the
                    // read side only -- the event scanning error. "An error on a write event will be
                    // captured in a subsequent write call that is able to report a more specific
                    // error."
                    if (desc.Closing)
                        return pollErrClosing;

                    if (m.Expired)
                        return pollErrTimeout;

                    if (mode == pollModeRead && desc.EventErr)
                        return pollErrNotPollable;
                }

                // Woken without readiness by a deadline, an unblock, or a deadline RESET that
                // superseded the one that woke us -- Go's comment for the same retry: "Can happen if
                // timeout has fired and unblocked us, but before we had a chance to run, timeout has
                // been reset. Pretend it has not happened and retry."
                //
                // Park accounting only -- Go's netpollblock parks with waitReasonIOWait, which is
                // what a traceback prints as [IO wait] for a goroutine blocked in the poller. §6
                // row 9 of DESIGN-cooperative-scheduler.md left this adoption to the netpoll arc's
                // option; taken here because the reason has a real park site and a real reader.
                using (Goroutine.Park(WaitReason.IOWait))
                    Monitor.Wait(desc.Gate);
            }
        }
    }

    // ---- 6. runtime_pollReset --------------------------------------------------------------------

    // Clear consumed readiness; fail fast if closing, expired, or (read side) in event error. Called
    // by every I/O wrapper before its syscall via prepareRead/prepareWrite (fd_poll_runtime.cs) --
    // the "prepare clears" half of the ET argument (§4.5). Order is netpollcheckerr's.
    internal static partial nint runtime_pollReset(uintptr ctx, nint mode)
    {
        ManagedPollDesc? desc = descFor(ctx);

        if (desc is null)
            return pollErrClosing;

        ManagedPollMode m = modeState(desc, mode);

        lock (desc.Gate)
        {
            if (desc.Closing)
                return pollErrClosing;

            if (m.Expired)
                return pollErrTimeout;

            if (mode == pollModeRead && desc.EventErr)
                return pollErrNotPollable;

            m.Ready = false;
            return pollNoError;
        }
    }

    // ---- 7. runtime_pollSetDeadline --------------------------------------------------------------

    // Arm, replace or clear the read and/or write deadline. `d` is a RELATIVE ns duration, already
    // normalized by setDeadlineImpl (fd_poll_runtime.cs): d > 0 arms, d == 0 clears, and d < 0
    // means already expired -- setDeadlineImpl rewrites an exactly-now deadline to -1 so that
    // "right now" is never confused with "no deadline". mode is 'r', 'w', or 'r'+'w'.
    //
    // Go's single-combo-timer optimization (netpoll.cs) is NOT reproduced: two timers with the same
    // due time are observationally equivalent, and the combo machinery exists to save a RUNTIME
    // timer -- a resource the managed side is not short of. COPIED from the Windows flavor (OQ-7).
    internal static partial void runtime_pollSetDeadline(uintptr ctx, int64 d, nint mode)
    {
        ManagedPollDesc? desc = descFor(ctx);

        if (desc is null)
            return;

        lock (desc.Gate)
        {
            // Go returns without touching anything once the desc is closing.
            if (desc.Closing)
                return;

            bool wake = false;

            if (mode == pollModeRead || mode == pollModeBoth)
                wake |= applyDeadlineLocked(desc.Read, d);

            if (mode == pollModeWrite || mode == pollModeBoth)
                wake |= applyDeadlineLocked(desc.Write, d);

            // A deadline set in the PAST fires NOW, against the CURRENT waiter: wake the blocked mode
            // without setting Ready, so its loop re-checks and returns pollErrTimeout. On this flavor
            // there is nothing to harvest afterwards (§4.7) -- the consumer simply returns
            // ErrDeadlineExceeded.
            if (wake)
                Monitor.PulseAll(desc.Gate);
        }
    }

    // Returns true when this call expired the mode immediately (a deadline in the past), which is
    // the only case that needs a wake. Caller holds the gate.
    private static bool applyDeadlineLocked(ManagedPollMode mode, int64 d)
    {
        // Every deadline change invalidates whatever timer is in flight for this mode, whether it is
        // being replaced, cleared or expired. This is Go's rseq/wseq bump, and here it is what makes
        // "deadline REPLACED while blocked -- old never fires, new does" hold.
        mode.Generation++;
        stopDeadlineLocked(mode);

        if (d > 0)
        {
            // Re-set to the future: clears a previous expiry (the sticky flag's only exit besides a
            // clear) and arms.
            mode.Expired = false;
            mode.DueNanos = runtimeNano() + d;
            armDeadlineLocked(mode);
            return false;
        }

        if (d == 0)
        {
            // No deadline. Clears a previous expiry -- this is what SetReadDeadline(time.Time{}) means.
            mode.Expired = false;
            return false;
        }

        // d < 0: already expired. Sticky until the next SetDeadline on this mode.
        mode.Expired = true;
        return true;
    }

    private static void stopDeadlineLocked(ManagedPollMode mode)
    {
        mode.ArmedGeneration = -1;
        mode.Deadline?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
    }

    // System.Threading.Timer's due time is a uint of milliseconds minus the two reserved values.
    private const long maxTimerMillis = uint.MaxValue - 2;

    private static void armDeadlineLocked(ManagedPollMode mode)
    {
        long remainingNanos = mode.DueNanos - runtimeNano();
        long dueMillis = remainingNanos <= 0 ? 0 : Math.Min(remainingNanos / 1_000_000, maxTimerMillis);

        mode.Deadline ??= new Timer(static state => deadlineFired((ManagedPollMode)state!), mode, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        mode.ArmedGeneration = mode.Generation;
        mode.Deadline.Change(dueMillis, System.Threading.Timeout.Infinite);
    }

    // The timer callback. Re-validates its generation under the gate (a stale callback must be
    // inert), re-arms if it fired early against a clamped due time, else expires the mode STICKILY
    // and wakes the waiter WITHOUT readiness.
    private static void deadlineFired(ManagedPollMode mode)
    {
        ManagedPollDesc desc = mode.Owner;

        lock (desc.Gate)
        {
            if (mode.ArmedGeneration != mode.Generation || desc.Closing)
                return;

            long remainingNanos = mode.DueNanos - runtimeNano();

            if (remainingNanos > 0)
            {
                armDeadlineLocked(mode);
                return;
            }

            mode.ArmedGeneration = -1;
            mode.Expired = true;
            Monitor.PulseAll(desc.Gate);
        }
    }

    // ---- 8. runtime_pollUnblock ------------------------------------------------------------------

    // Go's poll_runtime_pollUnblock, called by pd.evict() from FD.Close before the last reference
    // drops: mark closing, invalidate both modes' timers, wake every waiter WITHOUT readiness so each
    // returns pollErrClosing. After this, pollClose is legal.
    internal static partial void runtime_pollUnblock(uintptr ctx)
    {
        ManagedPollDesc? desc = descFor(ctx);

        if (desc is null)
            return;

        lock (desc.Gate)
        {
            if (desc.Closing)
                return;

            desc.Closing = true;
            desc.Read.Generation++;
            desc.Write.Generation++;
            stopDeadlineLocked(desc.Read);
            stopDeadlineLocked(desc.Write);
            Monitor.PulseAll(desc.Gate);
        }
    }

    // ---- 9. runtime_isPollServerDescriptor -------------------------------------------------------

    // Go's netpollIsPollDescriptor: true for the descriptors the poller itself owns -- here the one
    // epoll fd (OQ-2: no break eventfd). os/exec's TestExtraFiles enumerates the parent's descriptors
    // and skips the ones this names, which is why the answer has to be truthful rather than the
    // fallback's constant false.
    internal static partial bool runtime_isPollServerDescriptor(uintptr fd) =>
        epfd >= 0 && (nuint)fd == (nuint)(uint)epfd;

    // ---- 10. runtimeNano -------------------------------------------------------------------------

    // Monotonic nanoseconds; the one clock deadlines (DueNanos) and their Timers share.
    private static readonly long nanotimeBase = Stopwatch.GetTimestamp();

    internal static partial int64 runtimeNano() =>
        unchecked((long)((Stopwatch.GetTimestamp() - nanotimeBase) * (1_000_000_000.0 / Stopwatch.Frequency)));
}
