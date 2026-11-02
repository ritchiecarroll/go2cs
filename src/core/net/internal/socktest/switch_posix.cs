// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !plan9
namespace go.net.@internal;

using fmt = fmt_package;
using syscall = syscall_package;

partial class socktest_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string inet4ˢ => ref inet4ˢᵇ;
private static readonly @string inet4ˢᵇ = "inet4"u8;
private static ref readonly @string inet6ˢ => ref inet6ˢᵇ;
private static readonly @string inet6ˢᵇ = "inet6"u8;
private static ref readonly @string localˢ => ref localˢᵇ;
private static readonly @string localˢᵇ = "local"u8;

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
private static ref readonly @string streamˢ => ref streamˢᵇ;
private static readonly @string streamˢᵇ = "stream"u8;
private static ref readonly @string datagramˢ => ref datagramˢᵇ;
private static readonly @string datagramˢᵇ = "datagram"u8;
private static ref readonly @string rawˢ => ref rawˢᵇ;
private static readonly @string rawˢᵇ = "raw"u8;
private static ref readonly @string seqpacketˢ => ref seqpacketˢᵇ;
private static readonly @string seqpacketˢᵇ = "seqpacket"u8;

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
private static ref readonly @string defaultˢ => ref defaultˢᵇ;
private static readonly @string defaultˢᵇ = "default"u8;
private static ref readonly @string tcpˢ => ref tcpˢᵇ;
private static readonly @string tcpˢᵇ = "tcp"u8;
private static ref readonly @string udpˢ => ref udpˢᵇ;
private static readonly @string udpˢᵇ = "udp"u8;

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
