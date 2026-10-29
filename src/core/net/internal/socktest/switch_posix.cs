// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !plan9
namespace go.net.@internal;

using fmt = fmt_package;
using syscall = syscall_package;

partial class socktest_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string inet4ˢ = "inet4"u8;
private static readonly @string inet6ˢ = "inet6"u8;
private static readonly @string localˢ = "local"u8;

internal static @string familyString(nint family) {
    switch (family) {
    case var ᴛ when ᴛ == syscall.AF_INET: {
        return inet4ˢ;
    }
    case var ᴛ when ᴛ == syscall.AF_INET6: {
        return inet6ˢ;
    }
    case var ᴛ when ᴛ == syscall.AF_UNIX: {
        return localˢ;
    }
    default: {
        return fmt.Sprintf("%d"u8, family);
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string streamˢ = "stream"u8;
private static readonly @string datagramˢ = "datagram"u8;
private static readonly @string rawˢ = "raw"u8;
private static readonly @string seqpacketˢ = "seqpacket"u8;

internal static @string typeString(nint sotype) {
    @string s = default;
    switch ((sotype & 0xff)) {
    case var ᴛ when ᴛ == syscall.SOCK_STREAM: {
        s = streamˢ;
        break;
    }
    case var ᴛ when ᴛ == syscall.SOCK_DGRAM: {
        s = datagramˢ;
        break;
    }
    case var ᴛ when ᴛ == syscall.SOCK_RAW: {
        s = rawˢ;
        break;
    }
    case var ᴛ when ᴛ == syscall.SOCK_SEQPACKET: {
        s = seqpacketˢ;
        break;
    }
    default: {
        s = fmt.Sprintf("%d"u8, sotype & 0xff);
        break;
    }}
    {
        nuint flags = (nuint)((nuint)sotype & (nuint)~(nuint)0xff); if (flags != 0) {
            s += fmt.Sprintf("|%#x"u8, flags);
        }
    }
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string defaultˢ = "default"u8;
private static readonly @string tcpˢ = "tcp"u8;
private static readonly @string udpˢ = "udp"u8;

internal static @string protocolString(nint proto) {
    switch (proto) {
    case 0: {
        return defaultˢ;
    }
    case var ᴛ when ᴛ == syscall.IPPROTO_TCP: {
        return tcpˢ;
    }
    case var ᴛ when ᴛ == syscall.IPPROTO_UDP: {
        return udpˢ;
    }
    default: {
        return fmt.Sprintf("%d"u8, proto);
    }}
}

} // end socktest_package
