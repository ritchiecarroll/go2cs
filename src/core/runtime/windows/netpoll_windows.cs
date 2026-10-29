// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using windows = @internal.runtime.syscall.windows_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using @internal.runtime.syscall;

partial class runtime_package {

// Sources are used to identify the event that created an overlapped entry.
// The source values are arbitrary. There is no risk of collision with user
// defined values because the only way to set the key of an overlapped entry
// is using the iocphandle, which is not accessible to user code.
internal static UntypedInt netpollSourceReady => /* iota + 1 */ 1;
internal static UntypedInt netpollSourceBreak => 2;
internal static UntypedInt netpollSourceTimer => 3;

// sourceBits is the number of bits needed to represent a source.
// 4 bits can hold 16 different sources, which is more than enough.
// It is set to a low value so the overlapped entry key can
// contain as much bits as possible for the pollDesc pointer.
internal static UntypedInt sourceBits => 4; // 4 bits can hold 16 different sources, which is more than enough.
internal static UntypedInt sourceMasks => /* 1<<sourceBits - 1 */ 15;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeSourceValueIsTooˢ = "runtime: source value is too large"u8;

// packNetpollKey creates a key from a source and a tag.
// Bits that don't fit in the result are discarded.
internal static uintptr packNetpollKey(uint8 source, ж<pollDesc> pd) {
    // TODO: Consider combining the source with pd.fdseq to detect stale pollDescs.
    if (source > (uint8)((1 << (int)(sourceBits)) - 1)) {
        // Also fail on 64-bit systems, even though it can hold more bits.
        @throw(runtimeSourceValueIsTooˢ);
    }
    if (goarch.PtrSize == 4) {
        return (uintptr)((uintptr)pd << (int)(sourceBits) | (uintptr)source);
    }
    return (uintptr)taggedPointerPack(@unsafe.Pointer.FromPinnedBox(pd), (uintptr)source);
}

// unpackNetpollSource returns the source packed key.
internal static uint8 unpackNetpollSource(uintptr key) {
    if (goarch.PtrSize == 4) {
        return (uint8)((uintptr)(key & (uintptr)sourceMasks));
    }
    return (uint8)((taggedPointer)(uint64)key).tag();
}

// pollOperation must be the same as beginning of internal/poll.operation.
// Keep these in sync.
partial struct pollOperation {
    // used by windows
    internal windows.Overlapped _;
    // used by netpoll
    internal ж<pollDesc> pd;
    internal int32 mode;
}

// pollOperationFromOverlappedEntry returns the pollOperation contained in
// e. It can return nil if the entry is not from internal/poll.
// See go.dev/issue/58870
internal static ж<pollOperation> pollOperationFromOverlappedEntry(ref overlappedEntry e) {
    if (e.ov == nil) {
        return default;
    }
    var op = e.ov.Reinterpret<windows.Overlapped, pollOperation>();
    // Check that the key matches the pollDesc pointer.
    bool keyMatch = default;
    if (goarch.PtrSize == 4) {
        keyMatch = (uintptr)(e.key & ~(uintptr)(uintptr)sourceMasks) == (uintptr)op.pd << (int)(sourceBits);
    } else {
        keyMatch = (ж<pollDesc>)(uintptr)(((taggedPointer)(uint64)e.key).pointer()) == op.pd;
    }
    if (!keyMatch) {
        return default;
    }
    return op;
}

// overlappedEntry contains the information returned by a call to GetQueuedCompletionStatusEx.
// https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-overlapped_entry
partial struct overlappedEntry {
    internal uintptr key;
    internal ж<windows.Overlapped> ov;
    internal uintptr @internal;
    internal uint32 qty;
}

internal static uintptr iocphandle = windows.INVALID_HANDLE_VALUE; // completion port io handle

internal static readonly ж<atomic.Uint32> ᏑnetpollWakeSig = new StaticBox<atomic.Uint32>(default(atomic.Uint32));
internal static ref atomic.Uint32 netpollWakeSig => ref ((StaticBox<atomic.Uint32>)ᏑnetpollWakeSig).ValueSlot; // used to avoid duplicate calls of netpollBreak

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeNetpollinitFailedˢ = "runtime: netpollinit failed"u8;

internal static void netpollinit() {
    iocphandle = stdcall(_CreateIoCompletionPort, windows.INVALID_HANDLE_VALUE, 0, 0, windows.DWORD_MAX);
    if (iocphandle == 0) {
        println((@string)"runtime: CreateIoCompletionPort failed (errno="u8, getlasterror(), (@string)")"u8);
        @throw(runtimeNetpollinitFailedˢ);
    }
}

internal static bool netpollIsPollDescriptor(uintptr fd) {
    return fd == iocphandle;
}

internal static int32 netpollopen(uintptr fd, ж<pollDesc> pd) {
    var key = packNetpollKey(netpollSourceReady, pd);
    if (stdcall(_CreateIoCompletionPort, fd, iocphandle, key, 0) == 0) {
        return (int32)getlasterror();
    }
    return 0;
}

internal static int32 netpollclose(uintptr fd) {
    // nothing to do
    return 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeUnusedˢ = "runtime: unused"u8;

internal static void netpollarm(ref pollDesc pd, nint mode) {
    @throw(runtimeUnusedˢ);
}

// go2cs generated this placeholder — func netpollBreak is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeNetpollFailedˢ = "runtime: netpoll failed"u8;

// netpoll checks for ready network connections.
// Returns a list of goroutines that become runnable,
// and a delta to add to netpollWaiters.
// This must never return an empty list with a non-zero delta.
//
// delay < 0: blocks indefinitely
// delay == 0: does not block, just polls
// delay > 0: block for up to that many nanoseconds
internal static (gList, int32) netpoll(int64 delay) {
    if (iocphandle == windows.INVALID_HANDLE_VALUE) {
        return (new gList(), 0);
    }

    ref var entries = ref heap(new array<overlappedEntry>(64), out var Ꮡentries);
    uint32 wait = default;
    ref var toRun = ref heap(new gList(), out var ᏑtoRun);
    var mp = getg().Value.m;

    if (delay >= 1000000000000000) {
        // An arbitrary cap on how long to wait for a timer.
        // 1e15 ns == ~11.5 days.
        delay = 1000000000000000;
    }

    if (delay > 0 && (~mp).waitIocpHandle != 0) {
        // GetQueuedCompletionStatusEx doesn't use a high resolution timer internally,
        // so we use a separate higher resolution timer associated with a wait completion
        // packet to wake up the poller. Note that the completion packet can be delivered
        // to another thread, and the Go scheduler expects netpoll to only block up to delay,
        // so we still need to use a timeout with GetQueuedCompletionStatusEx.
        // TODO: Improve the Go scheduler to support non-blocking timers.
        var signaled = netpollQueueTimer(delay);
        if (signaled) {
            // There is a small window between the SetWaitableTimer and the NtAssociateWaitCompletionPacket
            // where the timer can expire. We can return immediately in this case.
            return (new gList(), 0);
        }
    }
    if (delay < 0) {
        wait = windows.INFINITE;
    } else if (delay == 0) {
        wait = 0;
    } else if (delay < 1000000) {
        wait = 1;
    } else {
        wait = (uint32)(delay / 1000000);
    }
    ref var n = ref heap<nint>(len(entries) / (nint)gomaxprocs, out var Ꮡn);
    if (n < 8) {
        n = 8;
    }
    if (delay != 0) {
        mp.Value.blocked = true;
    }
    if (stdcall(_GetQueuedCompletionStatusEx, iocphandle, (uintptr)Ꮡentries.at<overlappedEntry>(0), (uintptr)n, (uintptr)Ꮡn, (uintptr)wait, 0) == 0) {
        mp.Value.blocked = false;
        var errno = getlasterror();
        if (errno == windows.WAIT_TIMEOUT) {
            return (new gList(), 0);
        }
        println((@string)"runtime: GetQueuedCompletionStatusEx failed (errno="u8, errno, (@string)")"u8);
        @throw(runtimeNetpollFailedˢ);
    }
    mp.Value.blocked = false;
    var delta = (int32)0;
    for (nint i = 0; i < n; i++) {
        var e = Ꮡentries.at<overlappedEntry>(i);
        switch (unpackNetpollSource(e.key)) {
        case var ᴛ when ᴛ == netpollSourceReady: {
            var op = pollOperationFromOverlappedEntry(ref (e).DerefOrNull());
            if (op == nil) {
                // Entry from outside the Go runtime and internal/poll, ignore.
                continue;
            }
            // Entry from internal/poll.
            var mode = op.mode;
            if (mode != 'r' && mode != 'w') {
                println((@string)"runtime: GetQueuedCompletionStatusEx returned net_op with invalid mode="u8, mode);
                @throw(runtimeNetpollFailedˢ);
            }
            delta += netpollready(ᏑtoRun, op.pd, mode);
            break;
        }
        case var ᴛ when ᴛ == netpollSourceBreak: {
            ᏑnetpollWakeSig.Store(0);
            if (delay == 0) {
                // Forward the notification to the blocked poller.
                netpollBreak();
            }
            break;
        }
        case var ᴛ when ᴛ == netpollSourceTimer: {
            // TODO: We could avoid calling NtCancelWaitCompletionPacket for expired wait completion packets.
            break;
        }
        default: {
            println((@string)"runtime: GetQueuedCompletionStatusEx returned net_op with invalid key="u8, e.key);
            @throw(runtimeNetpollFailedˢ);
            break;
        }}
    }
    return (toRun, delta);
}

// netpollQueueTimer queues a timer to wake up the poller after the given delay.
// It returns true if the timer expired during this call.
internal static bool /*signaled*/ netpollQueueTimer(int64 delay) {
    ref var signaled = ref heap(new bool(), out var Ꮡsignaled);

    var mp = getg().Value.m;
    // A wait completion packet can only be associated with one timer at a time,
    // so we need to cancel the previous one if it exists. This wouldn't be necessary
    // if the poller would only be woken up by the timer, in which case the association
    // would be automatically canceled, but it can also be woken up by other events,
    // such as a netpollBreak, so we can get to this point with a timer that hasn't
    // expired yet. In this case, the completion packet can still be picked up by
    // another thread, so defer the cancellation until it is really necessary.
    var errno = stdcall(_NtCancelWaitCompletionPacket, (~mp).waitIocpHandle, 1);
    var exprᴛ1 = errno;
    var matchᴛ1 = false;
    if (exprᴛ1 == windows.STATUS_CANCELLED) { matchᴛ1 = true;
        // STATUS_CANCELLED is returned when the associated timer has already expired,
        // in which automatically cancels the wait completion packet.
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == windows.STATUS_SUCCESS) { matchᴛ1 = true;
        ref var dt = ref heap<int64>(-delay / 100, out var Ꮡdt); // relative sleep (negative), 100ns units
        if (stdcall(_SetWaitableTimer, (~mp).waitIocpTimer, (uintptr)Ꮡdt, 0, 0, 0, 0) == 0) {
            println((@string)"runtime: SetWaitableTimer failed; errno="u8, getlasterror());
            @throw(runtimeNetpollFailedˢ);
        }
        var key = packNetpollKey(netpollSourceTimer, nil);
        {
            var errnoΔ2 = stdcall(_NtAssociateWaitCompletionPacket, (~mp).waitIocpHandle, iocphandle, (~mp).waitIocpTimer, key, 0, 0, 0, (uintptr)Ꮡsignaled); if (errnoΔ2 != 0) {
                println((@string)"runtime: NtAssociateWaitCompletionPacket failed; errno="u8, errnoΔ2);
                @throw(runtimeNetpollFailedˢ);
            }
        }
    }
    else if (exprᴛ1 == windows.STATUS_PENDING) {
        // STATUS_PENDING is returned if the wait operation can't be canceled yet.
        // This can happen if this thread was woken up by another event, such as a netpollBreak,
        // and the timer expired just while calling NtCancelWaitCompletionPacket, in which case
        // this call fails to cancel the association to avoid a race condition.
        // This is a rare case, so we can just avoid using the high resolution timer this time.
    }
    else if (!matchᴛ1) { /* default: */
        println((@string)"runtime: NtCancelWaitCompletionPacket failed; errno="u8, errno);
        @throw(runtimeNetpollFailedˢ);
    }
    return signaled;
}

} // end runtime_package
