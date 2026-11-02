// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using os = os_package;
using syscall = syscall_package;

partial class net_package {

internal static error setDefaultSockopts(syscallꓸHandle s, nint family, nint sotype, bool ipv6only) {
    if (family == syscall.AF_INET6 && sotype != syscall.SOCK_RAW) {
        // Allow both IP versions even if the OS default
        // is otherwise. Note that some operating systems
        // never admit this option.
        syscall.SetsockoptInt(s, syscall.IPPROTO_IPV6, syscall.IPV6_V6ONLY, boolint(ipv6only));
    }
    if ((sotype == syscall.SOCK_DGRAM || sotype == syscall.SOCK_RAW) && family != syscall.AF_UNIX && family != syscall.AF_INET6) {
        // Allow broadcast.
        return os.NewSyscallError(setsockoptˢ, syscall.SetsockoptInt(s, syscall.SOL_SOCKET, syscall.SO_BROADCAST, 1));
    }
    return default;
}

internal static error setDefaultListenerSockopts(syscallꓸHandle s) {
    // Windows will reuse recently-used addresses by default.
    // SO_REUSEADDR should not be used here, as it allows
    // a socket to forcibly bind to a port in use by another socket.
    // This could lead to a non-deterministic behavior, where
    // connection requests over the port cannot be guaranteed
    // to be handled by the correct socket.
    return default;
}

internal static error setDefaultMulticastSockopts(syscallꓸHandle s) {
    // Allow multicast UDP and raw IP datagram sockets to listen
    // concurrently across multiple listeners.
    return os.NewSyscallError(setsockoptˢ, syscall.SetsockoptInt(s, syscall.SOL_SOCKET, syscall.SO_REUSEADDR, 1));
}

} // end net_package
