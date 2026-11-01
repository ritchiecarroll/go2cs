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
    switch (fd.family) {
    case var ᴛ when ᴛ == syscall.AF_INET || ᴛ == syscall.AF_INET6: {
        switch (fd.sotype) {
        case var ᴛ2 when ᴛ2 == syscall.SOCK_STREAM: {
            return sockaddrToTCP;
        }
        case var ᴛ2 when ᴛ2 == syscall.SOCK_DGRAM: {
            return sockaddrToUDP;
        }
        case var ᴛ2 when ᴛ2 == syscall.SOCK_RAW: {
            return sockaddrToIP;
        }}
        break;
    }
    case var ᴛ when ᴛ == syscall.AF_UNIX: {
        switch (fd.sotype) {
        case var ᴛ2 when ᴛ2 == syscall.SOCK_STREAM: {
            return sockaddrToUnix;
        }
        case var ᴛ2 when ᴛ2 == syscall.SOCK_DGRAM: {
            return sockaddrToUnixgram;
        }
        case var ᴛ2 when ᴛ2 == syscall.SOCK_SEQPACKET: {
            return sockaddrToUnixpacket;
        }}
        break;
    }}
    return (syscallꓸSockaddr _) => default;
}

} // end net_package
