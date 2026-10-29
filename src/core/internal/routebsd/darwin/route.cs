// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd

// Package routebsd supports reading interface addresses on BSD systems.
// This is a very stripped down version of x/net/route,
// for use by the net package in the standard library.
namespace go.@internal;

using errors = errors_package;
using syscall = syscall_package;

partial class routebsd_package {

internal static error errMessageMismatch = errors.New("message mismatch"u8);
internal static error errMessageTooShort = errors.New("message too short"u8);
internal static error errInvalidMessage = errors.New("invalid message"u8);
internal static error errInvalidAddr = errors.New("invalid address"u8);

// fetchRIB fetches a routing information base from the operating
// system.
//
// The arg is an interface index or 0 for all.
internal static (slice<byte>, error) fetchRIB(nint typ, nint arg) {
    nint @try = 0;
    for (;;) {
        @try++;
        var (b, err) = syscall.RouteRIB(typ, arg);

        // If the sysctl failed because the data got larger
        // between the two sysctl calls, try a few times
        // before failing (issue #45736).
        const nint maxTries = 3;
        if (AreEqual(err, syscall.ENOMEM) && @try < maxTries) {
            continue;
        }

        return (b, err);
    }
}

// FetchRIBMessages fetches a list of addressing messages for an interface.
// The typ argument is something like syscall.NET_RT_IFLIST.
// The argument is an interface index or 0 for all.
public static (slice<Message>, error) FetchRIBMessages(nint typ, nint arg) {
    var (b, err) = fetchRIB(typ, arg);
    if (err != nil) {
        return (default, err);
    }
    (var ms, err) = parseRIB(b);
    if (err != nil) {
        return (default, err);
    }
    return (ms, default);
}

} // end routebsd_package
