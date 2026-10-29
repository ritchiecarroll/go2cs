// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd
namespace go;

partial class runtime_package {

// Magic number of identifier used for EVFILT_USER.
// This number had zero Google results when it's created.
// That way, people will be directed here when this number
// get printed somehow and they search for it.
internal static UntypedInt kqIdent => 0xee1eb9f4;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeKeventFailedˢ = "runtime: kevent failed"u8;

internal static void addWakeupEvent(int32 kq) {
    ref var ev = ref heap<keventt>(new keventt(
        ident: kqIdent,
        filter: _EVFILT_USER,
        flags: (uint16)((uint16)_EV_ADD | (uint16)_EV_CLEAR)
    ), out var Ꮡev);
    for (;;) {
        var n = kevent(kq, Ꮡev, 1, nil, 0, nil);
        if (n == 0) {
            break;
        }
        if (n == (int32)(-_EINTR)) {
            // All changes contained in the changelist should have been applied
            // before returning EINTR. But let's be skeptical and retry it anyway,
            // to make a 100% commitment.
            continue;
        }
        println((@string)"runtime: kevent for EVFILT_USER failed with"u8, -n);
        @throw(runtimeKeventFailedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeNetpollBreakWriteˢ = "runtime: netpollBreak write failed"u8;

internal static void wakeNetpoll(int32 kq) {
    ref var ev = ref heap<keventt>(new keventt(
        ident: kqIdent,
        filter: _EVFILT_USER,
        fflags: _NOTE_TRIGGER
    ), out var Ꮡev);
    for (;;) {
        var n = kevent(kq, Ꮡev, 1, nil, 0, nil);
        if (n == 0) {
            break;
        }
        if (n == (int32)(-_EINTR)) {
            // Check out the comment in addWakeupEvent.
            continue;
        }
        println((@string)"runtime: netpollBreak write failed with"u8, -n);
        @throw(runtimeNetpollBreakWriteˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeNetpollBreakFdˢ = "runtime: netpoll: break fd ready for something unexpected"u8;

internal static bool isWakeup(ref keventt ev) {
    if (ev.filter == _EVFILT_USER) {
        if (ev.ident == kqIdent) {
            return true;
        }
        println((@string)"runtime: netpoll: break fd ready for"u8, ev.ident);
        @throw(runtimeNetpollBreakFdˢ);
    }
    return false;
}

internal static void processWakeupEvent(int32 kq, bool isBlocking) {
    if (!isBlocking) {
        // Got a wrong thread, relay
        wakeNetpoll(kq);
    }
}

internal static bool netpollIsPollDescriptor(uintptr fd) {
    return fd == (uintptr)kq;
}

} // end runtime_package
