// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux
namespace go;

using atomic = @internal.runtime.atomic_package;
using syscall = @internal.runtime.syscall_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

internal static int32 epfd = -1;           // epoll descriptor
internal static ж<uintptr> ᏑnetpollEventFd = new StandardBox<uintptr>(default(uintptr));
internal static ref uintptr netpollEventFd => ref ᏑnetpollEventFd.Value; // eventfd for netpollBreak
internal static ж<atomic.Uint32> ᏑnetpollWakeSig = new StandardBox<atomic.Uint32>(default(atomic.Uint32));
internal static ref atomic.Uint32 netpollWakeSig => ref ᏑnetpollWakeSig.Value;     // used to avoid duplicate calls of netpollBreak

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeNetpollinitFailedˢ = "runtime: netpollinit failed"u8;
internal static readonly @string runtimeEventfdFailedˢ = "runtime: eventfd failed"u8;
internal static readonly @string runtimeEpollctlFailedˢ = "runtime: epollctl failed"u8;

internal static void netpollinit() {
    uintptr errno = default!;
    (epfd, errno) = syscall.EpollCreate1(syscall.EPOLL_CLOEXEC);
    if (errno != 0) {
        println((@string)"runtime: epollcreate failed with"u8, errno);
        @throw(runtimeNetpollinitFailedˢ);
    }
    (var efd, errno) = syscall.Eventfd(0, (int32)((int32)syscall.EFD_CLOEXEC | (int32)syscall.EFD_NONBLOCK));
    if (errno != 0) {
        println((@string)"runtime: eventfd failed with"u8, ((uintptr)0 - errno));
        @throw(runtimeEventfdFailedˢ);
    }
    ref var ev = ref heap<syscall.EpollEvent>(out var Ꮡev);
    ev = new syscall.EpollEvent(
        Events: syscall.EPOLLIN
    );
    (Ꮡev.of(syscall.EpollEvent.ᏑData).Reinterpret<array<byte>, ж<uintptr>>()).ValueSlot = ᏑnetpollEventFd;
    errno = syscall.EpollCtl(epfd, syscall.EPOLL_CTL_ADD, efd, Ꮡev);
    if (errno != 0) {
        println((@string)"runtime: epollctl failed with"u8, errno);
        @throw(runtimeEpollctlFailedˢ);
    }
    netpollEventFd = (uintptr)efd;
}

internal static bool netpollIsPollDescriptor(uintptr fd) {
    return fd == (uintptr)epfd || fd == netpollEventFd;
}

internal static uintptr netpollopen(uintptr fd, ж<pollDesc> Ꮡpd) {
    ref var ev = ref heap(new syscall.EpollEvent(), out var Ꮡev);
    ev.Events = (uint32)((UntypedInt)((UntypedInt)(syscall.EPOLLIN | syscall.EPOLLOUT) | syscall.EPOLLRDHUP) | (uint32)syscall.EPOLLET);
    var tp = taggedPointerPack(@unsafe.Pointer.FromPinnedBox(Ꮡpd), Ꮡpd.of(pollDesc.Ꮡfdseq).Load());
    (Ꮡev.of(syscall.EpollEvent.ᏑData).Reinterpret<array<byte>, taggedPointer>()).Value = tp;
    return syscall.EpollCtl(epfd, syscall.EPOLL_CTL_ADD, (int32)fd, Ꮡev);
}

internal static uintptr netpollclose(uintptr fd) {
    ref var ev = ref heap(new syscall.EpollEvent(), out var Ꮡev);
    return syscall.EpollCtl(epfd, syscall.EPOLL_CTL_DEL, (int32)fd, Ꮡev);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeUnusedˢ = "runtime: unused"u8;

internal static void netpollarm(ref pollDesc pd, nint mode) {
    @throw(runtimeUnusedˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeNetpollBreakWriteˢ = "runtime: netpollBreak write failed"u8;

// netpollBreak interrupts an epollwait.
internal static void netpollBreak() {
    // Failing to cas indicates there is an in-flight wakeup, so we're done here.
    if (!ᏑnetpollWakeSig.CompareAndSwap(0, 1)) {
        return;
    }
    ref var one = ref heap(new uint64(), out var Ꮡone);
    one = 1;
    var oneSize = (int32)/* unsafe.Sizeof(one) */ (uintptr)8;
    while (ᐧ) {
        var n = write(netpollEventFd, (uintptr)noescape(@unsafe.Pointer.FromPinnedBox(Ꮡone)), oneSize);
        System.GC.KeepAlive(Ꮡone);
        if (n == oneSize) {
            break;
        }
        if (n == (int32)(-_EINTR)) {
            continue;
        }
        if (n == (int32)(-_EAGAIN)) {
            return;
        }
        println((@string)"runtime: netpollBreak write failed with"u8, -n);
        @throw(runtimeNetpollBreakWriteˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeNetpollFailedˢ = "runtime: netpoll failed"u8;
internal static readonly @string runtimeNetpollEventfdˢ = "runtime: netpoll: eventfd ready for something unexpected"u8;

// netpoll checks for ready network connections.
// Returns list of goroutines that become runnable.
// delay < 0: blocks indefinitely
// delay == 0: does not block, just polls
// delay > 0: block for up to that many nanoseconds
internal static (gList, int32) netpoll(int64 delay) {
    if (epfd == -1) {
        return (new gList(nil), 0);
    }
    int32 waitms = default!;
    if (delay < 0){
        waitms = -1;
    } else 
    if (delay == 0){
        waitms = 0;
    } else 
    if (delay < 1000000){
        waitms = 1;
    } else 
    if (delay < 1000000000000000){
        waitms = (int32)(delay / 1000000);
    } else {
        // An arbitrary cap on how long to wait for a timer.
        // 1e9 ms == ~11.5 days.
        waitms = 1000000000;
    }
    array<syscall.EpollEvent> events = new(128, () => new());
retry:
    var (n, errno) = syscall.EpollWait(epfd, events[..], (int32)len(events), waitms);
    if (errno != 0) {
        if (errno != _EINTR) {
            println((@string)"runtime: epollwait on fd"u8, epfd, (@string)"failed with"u8, errno);
            @throw(runtimeNetpollFailedˢ);
        }
        // If a timed sleep was interrupted, just return to
        // recalculate how long we should sleep now.
        if (waitms > 0) {
            return (new gList(nil), 0);
        }
        goto retry;
    }
    ref var toRun = ref heap(new gList(), out var ᏑtoRun);
    var delta = (int32)0;
    for (var i = (int32)0; i < n; i++) {
        ref var ev = ref heap<syscall.EpollEvent>(out var Ꮡev);
        ev = events[i].ΔClone();
        if (ev.Events == 0) {
            continue;
        }
        if (~Ꮡev.of(syscall.EpollEvent.ᏑData).Reinterpret<array<byte>, ж<uintptr>>() == ᏑnetpollEventFd) {
            if (ev.Events != syscall.EPOLLIN) {
                println((@string)"runtime: netpoll: eventfd ready for"u8, ev.Events);
                @throw(runtimeNetpollEventfdˢ);
            }
            if (delay != 0) {
                // netpollBreak could be picked up by a
                // nonblocking poll. Only read the 8-byte
                // integer if blocking.
                // Since EFD_SEMAPHORE was not specified,
                // the eventfd counter will be reset to 0.
                ref var one = ref heap(new uint64(), out var Ꮡone);
                read((int32)netpollEventFd, (uintptr)noescape(@unsafe.Pointer.FromPinnedBox(Ꮡone)), (int32)/* unsafe.Sizeof(one) */ (uintptr)8);
                System.GC.KeepAlive(Ꮡone);
                ᏑnetpollWakeSig.Store(0);
            }
            continue;
        }
        int32 mode = default!;
        if ((uint32)(ev.Events & ((uint32)((UntypedInt)((UntypedInt)(syscall.EPOLLIN | syscall.EPOLLRDHUP) | syscall.EPOLLHUP) | (uint32)syscall.EPOLLERR))) != 0) {
            mode += (rune)'r';
        }
        if ((uint32)(ev.Events & ((uint32)((UntypedInt)(syscall.EPOLLOUT | syscall.EPOLLHUP) | (uint32)syscall.EPOLLERR))) != 0) {
            mode += (rune)'w';
        }
        if (mode != 0) {
            var tp = ~Ꮡev.of(syscall.EpollEvent.ᏑData).Reinterpret<array<byte>, taggedPointer>();
            var pd = (ж<pollDesc>)(uintptr)(tp.pointer());
            var tag = tp.tag();
            if (pd.of(pollDesc.Ꮡfdseq).Load() == tag) {
                pd.setEventErr(ev.Events == syscall.EPOLLERR, tag);
                delta += netpollready(ᏑtoRun, pd, mode);
            }
        }
    }
    return (toRun, delta);
}

} // end runtime_package
