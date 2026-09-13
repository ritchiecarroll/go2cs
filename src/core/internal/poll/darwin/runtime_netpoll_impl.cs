// runtime_netpoll_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of internal/poll's TEN //go:linkname entry points into the Go
// runtime's network poller (fd_poll_runtime.go) for the DARWIN flavor: the READINESS poller -- one
// kqueue, one background drain thread, and the managed descriptor state machine the Windows flavor
// built and gated (windows/runtime_netpoll_impl.cs) and the Linux flavor lifted verbatim
// (linux/runtime_netpoll_impl.cs), lifted verbatim again. The linux file is the template this one
// was cut from and the two cite each other so they cannot drift silently (the lock_sema/lock_futex
// per-GOOS-authority precedent); everything below the kernel seam is a COPY of it, and the design it
// inherits -- docs/phase4/DESIGN-linux-readiness-poller.md over
// docs/phase4/DESIGN-netpoll-managed-poller.md -- is not re-argued here.
//
// HISTORY, KEPT BECAUSE IT IS THE MEASURED BILL. Until this file existed the darwin folder declared
// the ten partials (fd_poll_runtime.cs) and carried no companion, so every one of them was
// PartialStubGenerator's throwing stub -- runtime_pollServerInit first. The first full darwin
// behavioral census (run 33787891520, 2026-09-03, both Apple silicon and Intel) measured exactly
// that: every pollable descriptor died at `serverInit.Do(runtime_pollServerInit)` with
// `NotImplementedException: runtime_pollServerInit: external (assembly or cgo) function is not
// implemented`, one door BEFORE the converted runtime.netpollinit and its kqueue() -- StatLayoutTruth
// on both architectures, LinuxSpawnBasics on Intel -- and the eight `net` importers and the pipe
// users died at seams in front of this one (increments 4 and 5). This increment opens the door.
//
// WHAT THIS IS, IN ONE PARAGRAPH. Go's netpoll_kqueue.go with gopark/goready replaced by a monitor
// gate per descriptor. kqueue() once. Two EV_ADD | EV_CLEAR registrations per descriptor -- EVFILT_READ
// and EVFILT_WRITE -- exactly as Go's netpollopen arms them: EDGE-TRIGGERED (EV_CLEAR is kqueue's
// EPOLLET), so the linux file's no-lost-edge argument (design §4.5) carries over unchanged: the
// consumer (darwin/fd_unix.cs) waits only after the kernel has answered EAGAIN, so the buffer is
// empty/full at the wait and any future readiness is a new transition; prepare (pollReset) clears a
// stale Ready and a transition in that window is observed by the syscall itself; fdMutex admits one
// waiter per mode. udata carries an opaque TOKEN, never a pointer and never the fd (the kernel
// reissues fd numbers; a stale event's token no longer resolves). ONE background drain thread blocks
// in kevent(kq, NULL, 0, events, 64, NULL) forever, retries EINTR, and per event applies Go's own
// mapping (netpoll_kqueue.go): EVFILT_READ -> the read mode, PLUS the write mode when EV_EOF is set
// ("when the read end of a pipe is closed the write end will not get a _EVFILT_WRITE event, but will
// get a _EVFILT_READ event with EV_EOF set"), EVFILT_WRITE -> the write mode; the eventErr bit is
// `flags == EV_ERROR`; then Ready on the named modes and a pulse. Deadlines are the Windows §5
// machinery -- sticky per-mode expiry, generation-checked System.Threading.Timer, "a deadline set in
// the past fires NOW against the current waiter", wake-without-ready for unblock -- minus the
// cancel-and-harvest dimension, exactly as on linux. No wakeup event (Go's EVFILT_USER
// netpollBreak): nothing needs to interrupt the drain thread -- deadlines and unblocks reach waiters
// directly, and a registration made from another thread is observed by an in-progress kevent. Close
// registers no delete: "calling close() on fd will remove any kevents that reference the descriptor"
// (netpoll_kqueue.go's netpollclose is empty for that reason), and the table entry is retired last so
// no event can resolve a token that is gone. Regular files and FIFOs never reach pollOpen here:
// os.newFile's S_IFREG/S_IFIFO check (darwin/file_unix.cs, Go's own) keeps them off kqueue, so unlike
// linux there is no kernel EPERM arm to lean on and pollOpen answers 0 for whatever kqueue accepts.
//
// THE KERNEL SEAM, AND WHY IT IS A DllImport. The converted runtime.netpollopen reaches kevent through
// `libcCall(kevent_trampoline, &kq)` with a `keventt` whose `udata` is a managed reference
// (runtime/darwin/defs_darwin_amd64.cs), which the darwin keystone (runtime/darwin/libccall_impl.cs,
// golib GoLibcCall) refuses BY NAME as a reference-bearing args struct. This file never calls it: every
// byte the kernel reads or writes is a NATIVE image -- the 32-byte `struct kevent` of both 64-bit
// darwin ABIs, allocated with Marshal.AllocHGlobal and written field by field -- handed to libc's
// kqueue(2)/kevent(2) through `DllImport("libc")`, which resolves against libSystem.B.dylib on darwin
// (the os/darwin/dir_darwin_impl.cs readdir_r precedent). No ж<T> address, no generated
// address-taking wrapper, no unsafe block (internal.poll.csproj's shared <AllowUnsafeBlocks> stays
// false). kevent is not variadic, so the keystone's recorded arm64 register-style debt does not
// apply to this route either.
//
// WHAT CHANGES FOR os AND net, STATED: nothing is EDITED there. Pipes, ttys and sockets now arm
// (os.newFile's SetNonblock sticks because FD.Init succeeds), so a pipe Read parks on the gate
// instead of in read(2) and Close -> evict wakes it, SetDeadline is honored on them, and net's
// listeners and dialers reach the kernel instead of the stub. What this increment does NOT do, so the
// next census is read correctly: the seams the census measured IN FRONT of the poller -- the darwin
// syscall flavor's by-address struct writes (increment 5) and runtime.pipe's [2]int32 (increment 4) --
// are untouched, so a program that died at runtime_pollServerInit is expected to move to the NEXT
// absence in its path rather than to pass.
//
// SCOPE. Darwin amd64 AND arm64: `struct kevent` is the same 32-byte record on both 64-bit ABIs
// (ident uintptr @0, filter int16 @8, flags uint16 @10, fflags uint32 @12, data intptr @16, udata
// pointer @24), naturally aligned; pollServerInit refuses any other architecture rather than misread.
// Linux keeps its poller in linux/runtime_netpoll_impl.cs and Windows its own in
// windows/runtime_netpoll_impl.cs, both untouched: the desc machinery here is a COPY of theirs and
// the three files cite each other; a hoist into a flat shared companion is a later leveling once all
// three are measured.

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
// the marker keeps a -stdlib reconvert from touching it, and L3 routing keeps it darwin-only.
[module: go.GoManualConversion]

namespace go.@internal;

partial class poll_package
{
    // ---- the kernel surface: private copies of the numbers and flags this file uses -------------
    // Kept local rather than read from the converted runtime flavor so the file's kernel contract is
    // visible in one place; each value is darwin's (runtime/darwin/defs_darwin_amd64.cs, and the
    // same on arm64: sys/event.h is architecture-neutral).
    private const short EVFILT_READ = -1;
    private const short EVFILT_WRITE = -2;
    private const ushort EV_ADD = 0x1;
    private const ushort EV_CLEAR = 0x20;
    private const ushort EV_ERROR = 0x4000;
    private const ushort EV_EOF = 0x8000;
    private const int EINTR = 4;

    // struct kevent on 64-bit darwin: { uintptr_t ident; int16_t filter; uint16_t flags; uint32_t
    // fflags; intptr_t data; void *udata; } -- 32 bytes, every field naturally aligned.
    private const int keventSize = 32;
    private const int keventIdentOffset = 0;
    private const int keventFilterOffset = 8;
    private const int keventFlagsOffset = 10;
    private const int keventFflagsOffset = 12;
    private const int keventDataOffset = 16;
    private const int keventUdataOffset = 24;

    // Go's batch: `var events [64]keventt` (runtime/netpoll_kqueue.go).
    private const int drainBatch = 64;

    // The registration Go's netpollopen makes (netpoll_kqueue.go): both filters, EV_ADD | EV_CLEAR
    // -- edge-triggered for the whole fd lifetime. EV_EOF and EV_ERROR are reported without being
    // requested.
    private const ushort armFlags = EV_ADD | EV_CLEAR;

    // The one descriptor this poller owns (no wakeup event), and the drain buffer -- both
    // process-lifetime, created once in pollServerInit. -1 until then.
    private static int kq = -1;
    private static nint drainBuffer;

    // Per-mode state. Go splits the equivalent across pd.rg/pd.wg (the waiter slot), pd.rd/pd.wd (the
    // deadline), pd.rseq/pd.wseq (the stale-timer guard) and pd.rt/pd.wt (the runtime timer); the
    // fields below are the same five facts under one lock. COPIED from linux/runtime_netpoll_impl.cs
    // (itself a copy of the Windows flavor); keep the three in step.
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
        // Go's pdEventErr info bit: the last event for this descriptor carried EXACTLY EV_ERROR in its
        // flags. Set AND cleared on every event by the drain thread (netpoll_kqueue.go:
        // pd.setEventErr(ev.flags == _EV_ERROR, tag)); consulted by netpollcheckerr for mode 'r' only
        // -> pollErrNotPollable.
        internal bool EventErr;
        internal readonly ManagedPollMode Read = new();
        internal readonly ManagedPollMode Write = new();

        internal ManagedPollDesc()
        {
            Read.Owner = this;
            Write.Owner = this;
        }

        // The descriptor this desc was opened for -- what the two kevent registrations name.
        internal int Fd;
        // The ctx internal/poll holds and the udata the kernel carries back.
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

    // ---- the kernel route, and the one shape every kernel call here takes ----------------------
    // libc's kqueue(2) and kevent(2), taken directly rather than through the generated trampolines
    // (see the header). `DllImport("libc")` resolves against libSystem.B.dylib on darwin. Every
    // address handed through them is a native image owned by this file; SetLastError captures errno
    // for Marshal.GetLastPInvokeError, which is the only errno reader a DllImport call has.
    [DllImport("libc", EntryPoint = "kqueue", SetLastError = true)]
    private static extern int kqueue_native();

    [DllImport("libc", EntryPoint = "kevent", SetLastError = true)]
    private static extern int kevent_native(int kq, nint changelist, int nchanges, nint eventlist, int nevents, nint timeout);

    // Writes one struct kevent record at `record`.
    private static void writeKevent(nint record, int fd, short filter, ushort flags, uintptr token)
    {
        Marshal.WriteInt64(record, keventIdentOffset, (long)fd);
        Marshal.WriteInt16(record, keventFilterOffset, filter);
        Marshal.WriteInt16(record, keventFlagsOffset, unchecked((short)flags));
        Marshal.WriteInt32(record, keventFflagsOffset, 0);
        Marshal.WriteInt64(record, keventDataOffset, 0);
        Marshal.WriteInt64(record, keventUdataOffset, unchecked((long)(ulong)(nuint)token));
    }

    // Go's netpollopen registration: EVFILT_READ and EVFILT_WRITE, EV_ADD | EV_CLEAR, in ONE kevent
    // call with a two-record changelist and no eventlist -- on failure the call returns -1 and errno
    // names the reason (Go: `n := kevent(kq, &ev[0], 2, nil, 0, nil); if n < 0 { return -n }`).
    // Returns errno, 0 on success.
    private static int keventArm(int fd, uintptr token)
    {
        nint changes = Marshal.AllocHGlobal(2 * keventSize);

        try
        {
            writeKevent(changes, fd, EVFILT_READ, armFlags, token);
            writeKevent(changes + keventSize, fd, EVFILT_WRITE, armFlags, token);

            int n = kevent_native(kq, changes, 2, 0, 0, 0);

            return n < 0 ? Marshal.GetLastPInvokeError() : 0;
        }
        finally
        {
            Marshal.FreeHGlobal(changes);
        }
    }

    // ---- 1. runtime_pollServerInit ---------------------------------------------------------------

    // Go's netpollinit: create the kqueue. The caller wraps it in serverInit.Do (fd_poll_runtime.cs),
    // so it runs once per process, on the first pollable FD.Init. A failure is what it is in Go --
    // throw("runtime: netpollinit failed") -- and there is deliberately no fallback to "un-armable for
    // everyone": that would silently re-introduce the blocking degradation this file exists to
    // remove. Go also marks the kqueue close-on-exec; kqueue descriptors are not inherited across
    // fork/exec on darwin at all ("The queue is not inherited by a child created with fork(2)"), so
    // there is no closeonexec step to reproduce.
    internal static partial void runtime_pollServerInit()
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64 && RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            throw new PlatformNotSupportedException(
                "runtime: netpoll (darwin flavor) mirrors the 64-bit darwin ABIs' 32-byte struct kevent; " +
                RuntimeInformation.ProcessArchitecture + " is not supported by this hand-own");
        }

        int fd = kqueue_native();

        if (fd < 0)
            throw new InvalidOperationException("runtime: netpollinit failed (kqueue errno " + Marshal.GetLastPInvokeError() + ")");

        kq = fd;
        drainBuffer = Marshal.AllocHGlobal(drainBatch * keventSize);

        // Background: the process does not wait for it at exit, and it needs no shutdown signal.
        // Started AFTER kq and the buffer exist, BEFORE pollServerInitialized is published.
        Thread drain = new(drainLoop)
        {
            IsBackground = true,
            Name = "go2cs-netpoll"
        };
        drain.Start();

        Volatile.Write(ref pollServerInitialized, true);
    }

    private static bool pollServerInitialized;

    // Go's netpoll(delta) with delta = -1 forever and no scheduler to hand the ready list to. The
    // per-event body: Go's mode mapping (netpoll_kqueue.go), the eventErr bit, Ready on the named
    // modes, and a pulse -- netpollready -> netpollunblock(ioready) -> goready collapsed to a
    // Monitor.PulseAll under the desc's gate.
    private static void drainLoop()
    {
        while (true)
        {
            // timeout NULL: block until at least one event is pending.
            int n = kevent_native(kq, 0, 0, drainBuffer, drainBatch, 0);

            if (n < 0)
            {
                int errno = Marshal.GetLastPInvokeError();

                // kevent is not restarted after a signal handler, so EINTR is retried
                // unconditionally -- Go: `if errno == _EINTR { goto retry }`.
                if (errno == EINTR)
                    continue;

                // Any other errno on a valid kq is a process-level invariant failure (EBADF: someone
                // closed the poller's descriptor; EFAULT: the buffer moved -- it cannot, it is
                // native). Go throws "runtime: netpoll failed"; here an unhandled exception on this
                // background thread terminates the process through the crash-report path.
                // Catch-and-continue would be strictly worse: every future waiter would park forever
                // on a gate nobody pulses.
                throw new InvalidOperationException("runtime: netpoll failed (kevent errno " + errno + ")");
            }

            for (int i = 0; i < n; i++)
            {
                nint record = drainBuffer + i * keventSize;
                short filter = Marshal.ReadInt16(record, keventFilterOffset);
                ushort flags = unchecked((ushort)Marshal.ReadInt16(record, keventFlagsOffset));
                uintptr token = (uintptr)unchecked((ulong)Marshal.ReadInt64(record, keventUdataOffset));

                // A token that no longer resolves is an event for a descriptor closed under us
                // (pollClose retired it); under EV_CLEAR it is not repeated and needs nothing.
                ManagedPollDesc? desc = descFor(token);

                if (desc is null)
                    continue;

                bool readable = false, writable = false;

                if (filter == EVFILT_READ)
                {
                    readable = true;

                    // Go: "On some systems when the read end of a pipe is closed the write end will not
                    // get a _EVFILT_WRITE event, but will get a _EVFILT_READ event with EV_EOF set."
                    // Waking the writer is harmless: it retries and gets EPIPE/EAGAIN/success.
                    if ((flags & EV_EOF) != 0)
                        writable = true;
                }
                else if (filter == EVFILT_WRITE)
                {
                    writable = true;
                }

                if (!readable && !writable)
                    continue;

                lock (desc.Gate)
                {
                    desc.EventErr = flags == EV_ERROR;

                    if (readable)
                        desc.Read.Ready = true;

                    if (writable)
                        desc.Write.Ready = true;

                    Monitor.PulseAll(desc.Gate);
                }
            }
        }
    }

    // ---- 2. runtime_pollOpen ---------------------------------------------------------------------

    // Go's netpollopen: the two EV_ADD | EV_CLEAR registrations (netpoll_kqueue.go). Returns (ctx, 0)
    // or (0, errno); pollDesc.init converts a nonzero errno with errnoErr(syscall.Errno(errno))
    // (fd_poll_runtime.cs), and net.netFD.init propagates it, so any errno here surfaces from
    // net.Listen/Dial as its real name. Regular files and FIFOs do not arrive here (see the header).
    //
    // ORDER MATTERS: the table entry is inserted BEFORE the registration, because a readable
    // descriptor (an accepted connection with data already in flight) can deliver its first -- under
    // EV_CLEAR, only -- edge before kevent returns, and the drain thread must be able to resolve the
    // token at that instant. On failure the entry is removed again.
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

        int errno = keventArm(desc.Fd, ctx);

        if (errno != 0)
        {
            pollTable.TryRemove(ctx, out _);
            return (0, (nint)errno);
        }

        return (ctx, 0);
    }

    // ---- 3. runtime_pollClose --------------------------------------------------------------------

    // Go's netpollclose is EMPTY on kqueue: "Don't need to unregister because calling close() on fd
    // will remove any kevents that reference the descriptor." Legal only after unblock -- Go throws
    // "runtime: close polldesc w/o unblock" (runtime/netpoll.cs), and that assert guards
    // internal/poll's OWN sequencing, which is unchanged converted code: FD.Close calls pd.evict()
    // before decref -> destroy -> pd.close(). Kept as an InvalidOperationException so a future
    // sequencing regression is loud. The table entry is removed LAST, so no window exists in which the
    // kernel can still deliver an event for a token that is gone.
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

        pollTable.TryRemove(ctx, out _);
    }

    // ---- 4. runtime_pollWait ---------------------------------------------------------------------

    // Block until an edge arrives in mode, or return the closing/deadline/eventErr code. Called by
    // every I/O wrapper in darwin/fd_unix.cs after the kernel answered EAGAIN (design §2.1).
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
    // path is its only caller in Go; nothing in darwin/fd_unix.cs reaches it. The shared loop rather
    // than a throw -- it costs nothing, and it keeps the flavors' machinery identical.
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
                // what a traceback prints as [IO wait] for a goroutine blocked in the poller.
                using (Goroutine.Park(WaitReason.IOWait))
                    Monitor.Wait(desc.Gate);
            }
        }
    }

    // ---- 6. runtime_pollReset --------------------------------------------------------------------

    // Clear consumed readiness; fail fast if closing, expired, or (read side) in event error. Called
    // by every I/O wrapper before its syscall via prepareRead/prepareWrite (fd_poll_runtime.cs) --
    // the "prepare clears" half of the edge-trigger argument. Order is netpollcheckerr's.
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
    // timer -- a resource the managed side is not short of. COPIED from the Windows flavor.
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
            // there is nothing to harvest afterwards -- the consumer simply returns
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
    // kqueue (no wakeup event). os/exec's TestExtraFiles enumerates the parent's descriptors and
    // skips the ones this names, which is why the answer has to be truthful rather than the stub's
    // constant false.
    internal static partial bool runtime_isPollServerDescriptor(uintptr fd) =>
        kq >= 0 && (nuint)fd == (nuint)(uint)kq;

    // ---- 10. runtimeNano -------------------------------------------------------------------------

    // Monotonic nanoseconds; the one clock deadlines (DueNanos) and their Timers share.
    private static readonly long nanotimeBase = Stopwatch.GetTimestamp();

    internal static partial int64 runtimeNano() =>
        unchecked((long)((Stopwatch.GetTimestamp() - nanotimeBase) * (1_000_000_000.0 / Stopwatch.Frequency)));
}
