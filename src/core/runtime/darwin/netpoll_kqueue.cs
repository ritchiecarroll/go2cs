// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go;

// Integrated network poller (kqueue-based implementation).
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

internal static int32 kq = -1;
internal static ж<atomic.Uint32> ᏑnetpollWakeSig = new StaticBox<atomic.Uint32>(default(atomic.Uint32));
internal static ref atomic.Uint32 netpollWakeSig => ref ᏑnetpollWakeSig.Value; // used to avoid duplicate calls of netpollBreak

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeNetpollinitFailedˢ = "runtime: netpollinit failed"u8;

internal static void netpollinit() {
    kq = kqueue();
    if (kq < 0) {
        println((@string)"runtime: kqueue failed with"u8, -kq);
        @throw(runtimeNetpollinitFailedˢ);
    }
    closeonexec(kq);
    addWakeupEvent(kq);
}

internal static int32 netpollopen(uintptr fd, ж<pollDesc> Ꮡpd) {
    // Arm both EVFILT_READ and EVFILT_WRITE in edge-triggered mode (EV_CLEAR)
    // for the whole fd lifetime. The notifications are automatically unregistered
    // when fd is closed.
    ref var ev = ref heap(new array<keventt>(2), out var Ꮡev);
    (Ꮡev.at<keventt>(0).of(keventt.Ꮡident).Reinterpret<uint64, uintptr>()).Value = fd;
    ev[0].filter = _EVFILT_READ;
    ev[0].flags = (uint16)((uint16)_EV_ADD | (uint16)_EV_CLEAR);
    ev[0].fflags = 0;
    ev[0].data = 0;

    if (goarch.PtrSize == 4){
        // We only have a pointer-sized field to store into,
        // so on a 32-bit system we get no sequence protection.
        // TODO(iant): If we notice any problems we could at least
        // steal the low-order 2 bits for a tiny sequence number.
        ev[0].udata = Ꮡpd.Reinterpret<pollDesc, byte>();
    } else {
        var tp = taggedPointerPack(@unsafe.Pointer.FromPinnedBox(Ꮡpd), Ꮡpd.of(pollDesc.Ꮡfdseq).Load());
        ev[0].udata = (ж<byte>)(uintptr)((@unsafe.Pointer)(uintptr)(uint64)tp);
    }
    ev[1] = ev[0];
    ev[1].filter = _EVFILT_WRITE;
    var n = kevent(kq, Ꮡev.at<keventt>(0), 2, nil, 0, nil);
    if (n < 0) {
        return -n;
    }
    return 0;
}

internal static int32 netpollclose(uintptr fd) {
    // Don't need to unregister because calling close()
    // on fd will remove any kevents that reference the descriptor.
    return 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeUnusedˢ = "runtime: unused"u8;

internal static void netpollarm(ref pollDesc pd, nint mode) {
    @throw(runtimeUnusedˢ);
}

// netpollBreak interrupts a kevent.
internal static void netpollBreak() {
    // Failing to cas indicates there is an in-flight wakeup, so we're done here.
    if (!ᏑnetpollWakeSig.CompareAndSwap(0, 1)) {
        return;
    }

    wakeNetpoll(kq);
}

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
    if (kq == -1) {
        return (new gList(nil), 0);
    }
    ж<timespec> tp = default!;
    ref var ts = ref heap(new timespec(), out var Ꮡts);
    if (delay < 0){
        tp = default!;
    } else 
    if (delay == 0){
        tp = Ꮡts;
    } else {
        ts.setNsec(delay);
        if (ts.tv_sec > 1000000) {
            // Darwin returns EINVAL if the sleep time is too long.
            ts.tv_sec = 1000000;
        }
        tp = Ꮡts;
    }
    ref var events = ref heap(new array<keventt>(64), out var Ꮡevents);
retry:
    var n = kevent(kq, nil, 0, Ꮡevents.at<keventt>(0), (int32)len(events), tp);
    if (n < 0) {
        // Ignore the ETIMEDOUT error for now, but try to dive deep and
        // figure out what really happened with n == ETIMEOUT,
        // see https://go.dev/issue/59679 for details.
        if (n != (int32)(-_EINTR) && n != (int32)(-_ETIMEDOUT)) {
            println((@string)"runtime: kevent on fd"u8, kq, (@string)"failed with"u8, -n);
            @throw(runtimeNetpollFailedˢ);
        }
        // If a timed sleep was interrupted, just return to
        // recalculate how long we should sleep now.
        if (delay > 0) {
            return (new gList(nil), 0);
        }
        goto retry;
    }
    ref var toRun = ref heap(new gList(), out var ᏑtoRun);
    var delta = (int32)0;
    for (nint i = 0; i < (nint)n; i++) {
        var ev = Ꮡevents.at<keventt>(i);

        if (isWakeup(ref (ev).DerefOrNull())) {
            var isBlocking = delay != 0;
            processWakeupEvent(kq, isBlocking);
            if (isBlocking) {
                // netpollBreak could be picked up by a nonblocking poll.
                // Only reset the netpollWakeSig if blocking.
                ᏑnetpollWakeSig.Store(0);
            }
            continue;
        }

        int32 mode = default!;
        var exprᴛ1 = (~ev).filter;
        if (exprᴛ1 == _EVFILT_READ) {
            mode += (rune)'r';

            // On some systems when the read end of a pipe
            // is closed the write end will not get a
            // _EVFILT_WRITE event, but will get a
            // _EVFILT_READ event with EV_EOF set.
            // Note that setting 'w' here just means that we
            // will wake up a goroutine waiting to write;
            // that goroutine will try the write again,
            // and the appropriate thing will happen based
            // on what that write returns (success, EPIPE, EAGAIN).
            if ((uint16)((~ev).flags & (uint16)_EV_EOF) != 0) {
                mode += (rune)'w';
            }
        }
        else if (exprᴛ1 == _EVFILT_WRITE) {
            mode += (rune)'w';
        }
        if (mode != 0) {
            ж<pollDesc> pd = default!;
            uintptr tag = default!;
            if (goarch.PtrSize == 4){
                // No sequence protection on 32-bit systems.
                // See netpollopen for details.
                pd = (~ev).udata.Reinterpret<byte, pollDesc>();
                tag = 0;
            } else {
                var tpΔ1 = (taggedPointer)(uint64)(uintptr)(~ev).udata;
                pd = (ж<pollDesc>)(uintptr)(tpΔ1.pointer());
                tag = tpΔ1.tag();
                if (pd.of(pollDesc.Ꮡfdseq).Load() != tag) {
                    continue;
                }
            }
            pd.setEventErr((~ev).flags == _EV_ERROR, tag);
            delta += netpollready(ᏑtoRun, pd, mode);
        }
    }
    return (toRun, delta);
}

} // end runtime_package
