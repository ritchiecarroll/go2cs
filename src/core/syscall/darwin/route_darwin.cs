// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class syscall_package {

internal static RoutingMessage toRoutingMessage(this ж<anyMessage> Ꮡany, slice<byte> b) {
    ref var any = ref Ꮡany.DerefOrNull();

    switch (any.Type) {
    case var ᴛ when ᴛ == RTM_ADD || ᴛ == RTM_DELETE || ᴛ == RTM_CHANGE || ᴛ == RTM_GET || ᴛ == RTM_LOSING || ᴛ == RTM_REDIRECT || ᴛ == RTM_MISS || ᴛ == RTM_LOCK || ᴛ == RTM_RESOLVE: {
        var p = Ꮡany.Reinterpret<anyMessage, RouteMessage>();
        return new RouteMessageжRoutingMessage(Ꮡ(new RouteMessage(Header: p.Header.ΔClone(), Data: b.slice(SizeofRtMsghdr, any.Msglen))));
    }
    case var ᴛ when ᴛ == RTM_IFINFO: {
        var p = Ꮡany.Reinterpret<anyMessage, InterfaceMessage>();
        return new InterfaceMessageжRoutingMessage(Ꮡ(new InterfaceMessage(Header: p.Header.ΔClone(), Data: b.slice(SizeofIfMsghdr, any.Msglen))));
    }
    case var ᴛ when ᴛ == RTM_NEWADDR || ᴛ == RTM_DELADDR: {
        var p = Ꮡany.Reinterpret<anyMessage, InterfaceAddrMessage>();
        return new InterfaceAddrMessageжRoutingMessage(Ꮡ(new InterfaceAddrMessage(Header: p.Header.ΔClone(), Data: b.slice(SizeofIfaMsghdr, any.Msglen))));
    }
    case var ᴛ when ᴛ == RTM_NEWMADDR2 || ᴛ == RTM_DELMADDR: {
        var p = Ꮡany.Reinterpret<anyMessage, InterfaceMulticastAddrMessage>();
        return new InterfaceMulticastAddrMessageжRoutingMessage(Ꮡ(new InterfaceMulticastAddrMessage(Header: p.Header.ΔClone(), Data: b.slice(SizeofIfmaMsghdr2, any.Msglen))));
    }}
    return default;
}

// InterfaceMulticastAddrMessage represents a routing message
// containing network interface address entries.
//
// Deprecated: Use golang.org/x/net/route instead.
partial struct InterfaceMulticastAddrMessage {
    public IfmaMsghdr2 Header;
    public slice<byte> Data;
}

internal static (slice<Sockaddr>, error) sockaddr(this ref InterfaceMulticastAddrMessage m) {
    array<Sockaddr> sas = new(8); /* RTAX_MAX */
    var b = m.Data[..];
    for (nuint i = (nuint)0; i < RTAX_MAX && len(b) >= minRoutingSockaddrLen; i++) {
        if ((m.Header.Addrs & (((int32)1).Lsh(i))) == 0) {
            continue;
        }
        var rsa = Ꮡ(b, 0).Reinterpret<byte, RawSockaddr>();
        switch (rsa.Family) {
        case var ᴛ when ᴛ == AF_LINK: {
            var (sa, err) = parseSockaddrLink(b);
            if (err != nil) {
                return (default, err);
            }
            sas[i] = new SockaddrDatalinkжSockaddr(sa);
            b = b.slice(rsaAlignOf((nint)rsa.Len));
            break;
        }
        case var ᴛ when ᴛ == AF_INET || ᴛ == AF_INET6: {
            var (sa, err) = parseSockaddrInet(b, rsa.Family);
            if (err != nil) {
                return (default, err);
            }
            sas[i] = sa;
            b = b.slice(rsaAlignOf((nint)rsa.Len));
            break;
        }
        default: {
            var (sa, l, err) = parseLinkLayerAddr(b);
            if (err != nil) {
                return (default, err);
            }
            sas[i] = new SockaddrDatalinkжSockaddr(sa);
            b = b.slice(l);
            break;
        }}
    }
    return (sas[..], default);
}

} // end syscall_package
