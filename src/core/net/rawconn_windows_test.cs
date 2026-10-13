// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using static go.net_package;

partial class net_internal_test_package {

internal static (nint, error) readRawConn(syscall.RawConn c, slice<byte> b) {
    ref var operr = ref heap<error>(out var Ꮡoperr);
    nint n = default!;
    var bʗ1 = b;
    var err = c.Read((uintptr s) => {
        ref var read = ref heap(new uint32(), out var Ꮡread);
        ref var flags = ref heap(new uint32(), out var Ꮡflags);
        ref var buf = ref heap(new syscall.WSABuf(), out var Ꮡbuf);
        buf.Buf = Ꮡ(bʗ1, 0);
        buf.Len = (uint32)len(bʗ1);
        Ꮡoperr.ValueSlot = syscall.WSARecv(((syscallꓸHandle)s), Ꮡbuf, 1, Ꮡread, Ꮡflags, nil, nil);
        n = (nint)read;
        return true;
    });
    if (err != default!) {
        return (n, err);
    }
    return (n, operr);
}

internal static error writeRawConn(syscall.RawConn c, slice<byte> b) {
    ref var operr = ref heap<error>(out var Ꮡoperr);
    var bʗ1 = b;
    var err = c.Write((uintptr s) => {
        ref var written = ref heap(new uint32(), out var Ꮡwritten);
        ref var buf = ref heap(new syscall.WSABuf(), out var Ꮡbuf);
        buf.Buf = Ꮡ(bʗ1, 0);
        buf.Len = (uint32)len(bʗ1);
        Ꮡoperr.ValueSlot = syscall.WSASend(((syscallꓸHandle)s), Ꮡbuf, 1, Ꮡwritten, 0, nil, nil);
        return true;
    });
    if (err != default!) {
        return err;
    }
    return operr;
}

internal static error controlRawConn(syscall.RawConn c, global::go.net_package.ΔAddr addr) {
    ref var operr = ref heap<error>(out var Ꮡoperr);
    var fn = (uintptr s) => {
        ref var v = ref heap(new int32(), out var Ꮡv);
        ref var l = ref heap(new int32(), out var Ꮡl);
        l = (int32)/* unsafe.Sizeof(v) */ (uintptr)4;
        Ꮡoperr.ValueSlot = syscall.Getsockopt(((syscallꓸHandle)s), syscall.SOL_SOCKET, syscall.SO_REUSEADDR, Ꮡv.Reinterpret<int32, byte>(), Ꮡl);
        if (Ꮡoperr.ValueSlot != default!) {
            return;
        }
        switch (addr.type()) {
        case ж<global::go.net_package.TCPAddr> addrΔ1: {
            // There's no guarantee that IP-level socket
            // options work well with dual stack sockets.
            // A simple solution would be to take a look
            // at the bound address to the raw connection
            // and to classify the address family of the
            // underlying socket by the bound address:
            //
            // - When IP.To16() != nil and IP.To4() == nil,
            //   we can assume that the raw connection
            //   consists of an IPv6 socket using only
            //   IPv6 addresses.
            //
            // - When IP.To16() == nil and IP.To4() != nil,
            //   the raw connection consists of an IPv4
            //   socket using only IPv4 addresses.
            //
            // - Otherwise, the raw connection is a dual
            //   stack socket, an IPv6 socket using IPv6
            //   addresses including IPv4-mapped or
            //   IPv4-embedded IPv6 addresses.
            if ((~addrΔ1).IP.To16() != default! && (~addrΔ1).IP.To4() == default!){
                Ꮡoperr.ValueSlot = syscall.SetsockoptInt(((syscallꓸHandle)s), syscall.IPPROTO_IPV6, syscall.IPV6_UNICAST_HOPS, 1);
            } else 
            if ((~addrΔ1).IP.To16() == default! && (~addrΔ1).IP.To4() != default!) {
                Ꮡoperr.ValueSlot = syscall.SetsockoptInt(((syscallꓸHandle)s), syscall.IPPROTO_IP, syscall.IP_TTL, 1);
            }
            break;
        }}
    };
    {
        var err = c.Control(fn); if (err != default!) {
            return err;
        }
    }
    return operr;
}

internal static error controlOnConnSetup(@string network, @string address, syscall.RawConn c) {
    ref var operr = ref heap<error>(out var Ꮡoperr);
    Action<uintptr> fn = default!;
    var exprᴛ1 = network;
    if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "udp"u8 || exprᴛ1 == "ip"u8) {
        return errors.New("ambiguous network: "u8 + network);
    }
    { /* default: */
        switch (network[len(network) - 1]) {
        case (rune)'4': {
            fn = (uintptr s) => {
                Ꮡoperr.ValueSlot = syscall.SetsockoptInt(((syscallꓸHandle)s), syscall.IPPROTO_IP, syscall.IP_TTL, 1);
            };
            break;
        }
        case (rune)'6': {
            fn = (uintptr s) => {
                Ꮡoperr.ValueSlot = syscall.SetsockoptInt(((syscallꓸHandle)s), syscall.IPPROTO_IPV6, syscall.IPV6_UNICAST_HOPS, 1);
            };
            break;
        }
        default: {
            return errors.New("unknown network: "u8 + network);
        }}
    }
    {
        var err = c.Control(fn); if (err != default!) {
            return err;
        }
    }
    return operr;
}

} // end net_internal_test_package
