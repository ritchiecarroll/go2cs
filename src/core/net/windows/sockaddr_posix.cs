// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || js || wasip1 || windows
namespace go;

using syscall = syscall_package;

partial class net_package {

// A sockaddr represents a TCP, UDP, IP or Unix network endpoint
// address that can be converted into a syscall.Sockaddr.
partial interface Δsockaddr :
    ΔAddr
{
    // family returns the platform-dependent address family
    // identifier.
    nint family();
    // isWildcard reports whether the address is a wildcard
    // address.
    bool isWildcard();
    // sockaddr returns the address converted into a syscall
    // sockaddr type that implements syscall.Sockaddr
    // interface. It returns a nil interface when the address is
    // nil.
    (syscallꓸSockaddr, error) sockaddr(nint family);
    // toLocal maps the zero address to a local system address (127.0.0.1 or ::1)
    Δsockaddr toLocal(@string net);
}

internal static Func<syscallꓸSockaddr, ΔAddr> addrFunc(this ref netFD fd) {
    var exprᴛ1 = fd.family;
    if (exprᴛ1 == syscall.AF_INET || exprᴛ1 == syscall.AF_INET6) {
        var exprᴛ2 = fd.sotype;
        if (exprᴛ2 == syscall.SOCK_STREAM) {
            return sockaddrToTCP;
        }
        if (exprᴛ2 == syscall.SOCK_DGRAM) {
            return sockaddrToUDP;
        }
        if (exprᴛ2 == syscall.SOCK_RAW) {
            return sockaddrToIP;
        }

    }
    else if (exprᴛ1 == syscall.AF_UNIX) {
        var exprᴛ3 = fd.sotype;
        if (exprᴛ3 == syscall.SOCK_STREAM) {
            return sockaddrToUnix;
        }
        if (exprᴛ3 == syscall.SOCK_DGRAM) {
            return sockaddrToUnixgram;
        }
        if (exprᴛ3 == syscall.SOCK_SEQPACKET) {
            return sockaddrToUnixpacket;
        }

    }

    return (syscallꓸSockaddr _) => default!;
}

} // end net_package
