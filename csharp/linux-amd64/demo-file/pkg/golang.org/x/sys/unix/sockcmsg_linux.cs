// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using @unsafe = unsafe_package;

partial class unix_package {

public static slice<byte> UnixCredentials(ж<Ucred> Ꮡucred) {
    ref var ucred = ref Ꮡucred.DerefOrNull();

    var b = new slice<byte>(CmsgSpace(SizeofUcred));
    var h = Ꮡ(b, 0).Reinterpret<byte, Cmsghdr>();
    h.Level = SOL_SOCKET;
    h.Type = SCM_CREDENTIALS;
    h.SetLen(CmsgLen(SizeofUcred));
    (@unsafe.Pointer.To<Ucred>(h.data(0))).Value = ucred;
    return b;
}

public static (ж<Ucred>, error) ParseUnixCredentials(ж<SocketControlMessage> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (m.Header.Level != SOL_SOCKET) {
        return (default, EINVAL);
    }
    if (m.Header.Type != SCM_CREDENTIALS) {
        return (default, EINVAL);
    }
    ref var ucred = ref heap<Ucred>(~Ꮡ(m.Data, 0).Reinterpret<byte, Ucred>(), out var Ꮡucred);
    return (Ꮡucred, default);
}

public static slice<byte> PktInfo4(ж<Inet4Pktinfo> Ꮡinfo) {
    ref var info = ref Ꮡinfo.DerefOrNull();

    var b = new slice<byte>(CmsgSpace(SizeofInet4Pktinfo));
    var h = Ꮡ(b, 0).Reinterpret<byte, Cmsghdr>();
    h.Level = SOL_IP;
    h.Type = IP_PKTINFO;
    h.SetLen(CmsgLen(SizeofInet4Pktinfo));
    (@unsafe.Pointer.To<Inet4Pktinfo>(h.data(0))).Value = info;
    return b;
}

public static slice<byte> PktInfo6(ж<Inet6Pktinfo> Ꮡinfo) {
    ref var info = ref Ꮡinfo.DerefOrNull();

    var b = new slice<byte>(CmsgSpace(SizeofInet6Pktinfo));
    var h = Ꮡ(b, 0).Reinterpret<byte, Cmsghdr>();
    h.Level = SOL_IPV6;
    h.Type = IPV6_PKTINFO;
    h.SetLen(CmsgLen(SizeofInet6Pktinfo));
    (@unsafe.Pointer.To<Inet6Pktinfo>(h.data(0))).Value = info;
    return b;
}

public static (Sockaddr, error) ParseOrigDstAddr(ж<SocketControlMessage> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    switch (ᐧ) {
    case {} when m.Header.Level == SOL_IP && m.Header.Type == IP_ORIGDSTADDR: {
        var pp = Ꮡ(m.Data, 0).Reinterpret<byte, RawSockaddrInet4>();
        var sa = @new<SockaddrInet4>();
        var p = @unsafe.ArrayPointer<byte>.Of(pp.ᏑPort, 2);
        sa.Port = ((nint)p.ElementRef(0) << 8) + (nint)p.ElementRef(1);
        sa.Addr.Assign(pp.Addr);
        return (new SockaddrInet4жSockaddr(sa), default);
    }
    case {} when m.Header.Level == SOL_IPV6 && m.Header.Type == IPV6_ORIGDSTADDR: {
        var pp = Ꮡ(m.Data, 0).Reinterpret<byte, RawSockaddrInet6>();
        var sa = @new<SockaddrInet6>();
        var p = @unsafe.ArrayPointer<byte>.Of(pp.ᏑPort, 2);
        sa.Port = ((nint)p.ElementRef(0) << 8) + (nint)p.ElementRef(1);
        sa.ZoneId = pp.Scope_id;
        sa.Addr.Assign(pp.Addr);
        return (new SockaddrInet6жSockaddr(sa), default);
    }
    default: {
        return (default, EINVAL);
    }}

}

} // end unix_package
