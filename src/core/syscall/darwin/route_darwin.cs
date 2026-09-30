// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class syscall_package {

internal static RoutingMessage toRoutingMessage(this ж<anyMessage> Ꮡany, slice<byte> b) {
    ref var any = ref Ꮡany.DerefOrNull();

    var exprᴛ1 = any.Type;
    if (exprᴛ1 == RTM_ADD || exprᴛ1 == RTM_DELETE || exprᴛ1 == RTM_CHANGE || exprᴛ1 == RTM_GET || exprᴛ1 == RTM_LOSING || exprᴛ1 == RTM_REDIRECT || exprᴛ1 == RTM_MISS || exprᴛ1 == RTM_LOCK || exprᴛ1 == RTM_RESOLVE) {
        var p = Ꮡany.Reinterpret<anyMessage, RouteMessage>();
        return new RouteMessageжRoutingMessage(Ꮡ(new RouteMessage(Header: (~p).Header.ΔClone(), Data: b.slice(SizeofRtMsghdr, any.Msglen))));
    }
    if (exprᴛ1 == RTM_IFINFO) {
        var p = Ꮡany.Reinterpret<anyMessage, InterfaceMessage>();
        return new InterfaceMessageжRoutingMessage(Ꮡ(new InterfaceMessage(Header: (~p).Header.ΔClone(), Data: b.slice(SizeofIfMsghdr, any.Msglen))));
    }
    if (exprᴛ1 == RTM_NEWADDR || exprᴛ1 == RTM_DELADDR) {
        var p = Ꮡany.Reinterpret<anyMessage, InterfaceAddrMessage>();
        return new InterfaceAddrMessageжRoutingMessage(Ꮡ(new InterfaceAddrMessage(Header: (~p).Header.ΔClone(), Data: b.slice(SizeofIfaMsghdr, any.Msglen))));
    }
    if (exprᴛ1 == RTM_NEWMADDR2 || exprᴛ1 == RTM_DELMADDR) {
        var p = Ꮡany.Reinterpret<anyMessage, InterfaceMulticastAddrMessage>();
        return new InterfaceMulticastAddrMessageжRoutingMessage(Ꮡ(new InterfaceMulticastAddrMessage(Header: (~p).Header.ΔClone(), Data: b.slice(SizeofIfmaMsghdr2, any.Msglen))));
    }

    return default!;
}

// InterfaceMulticastAddrMessage represents a routing message
// containing network interface address entries.
//
// Deprecated: Use golang.org/x/net/route instead.
[GoType] partial struct InterfaceMulticastAddrMessage {
    public IfmaMsghdr2 Header;
    public slice<byte> Data;
}

[GoRecv] internal static (slice<Sockaddr>, error) sockaddr(this ref InterfaceMulticastAddrMessage m) {
    array<Sockaddr> sas = new(8); /* RTAX_MAX */
    var b = m.Data[..];
    for (nuint i = (nuint)0; i < RTAX_MAX && len(b) >= minRoutingSockaddrLen; i++) {
        if ((int32)(m.Header.Addrs & (((int32)1).Lsh(i))) == 0) {
            continue;
        }
        var rsa = Ꮡ(b, 0).Reinterpret<byte, RawSockaddr>();
        var exprᴛ1 = (~rsa).Family;
        if (exprᴛ1 == AF_LINK) {
            var (sa, err) = parseSockaddrLink(b);
            if (err != default!) {
                return (default!, err);
            }
            sas[i] = new SockaddrDatalinkжSockaddr(sa);
            b = b.slice(rsaAlignOf((nint)(~rsa).Len));
        }
        else if (exprᴛ1 == AF_INET || exprᴛ1 == AF_INET6) {
            var (sa, err) = parseSockaddrInet(b, (~rsa).Family);
            if (err != default!) {
                return (default!, err);
            }
            sas[i] = sa;
            b = b.slice(rsaAlignOf((nint)(~rsa).Len));
        }
        else { /* default: */
            var (sa, l, err) = parseLinkLayerAddr(b);
            if (err != default!) {
                return (default!, err);
            }
            sas[i] = new SockaddrDatalinkжSockaddr(sa);
            b = b.slice(l);
        }

    }
    return (sas[..], default!);
}

} // end syscall_package
