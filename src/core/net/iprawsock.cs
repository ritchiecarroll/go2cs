// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using syscall = syscall_package;

partial class net_package {

// BUG(mikio): On every POSIX platform, reads from the "ip4" network
// using the ReadFrom or ReadFromIP method might not return a complete
// IPv4 packet, including its header, even if there is space
// available. This can occur even in cases where Read or ReadMsgIP
// could return a complete packet. For this reason, it is recommended
// that you do not use these methods if it is important to receive a
// full packet.
//
// The Go 1 compatibility guidelines make it impossible for us to
// change the behavior of these methods; use Read or ReadMsgIP
// instead.
// BUG(mikio): On JS and Plan 9, methods and functions related
// to IPConn are not implemented.
// BUG(mikio): On Windows, the File method of IPConn is not
// implemented.

// IPAddr represents the address of an IP end point.
partial struct IPAddr {
    public IP IP;
    public @string Zone; // IPv6 scoped addressing zone
}

// Network returns the address's network name, "ip".
public static @string Network(this ref IPAddr a) {
    return "ip"u8;
}

public static @string String(this ж<IPAddr> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil) {
        return nilˢ;
    }
    @string ip = ipEmptyString(a.IP);
    if (a.Zone != ""u8) {
        return ip + "%"u8 + a.Zone;
    }
    return ip;
}

internal static bool isWildcard(this ж<IPAddr> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil || a.IP == default!) {
        return true;
    }
    return a.IP.IsUnspecified();
}

internal static ΔAddr opAddr(this ж<IPAddr> Ꮡa) {
    if (Ꮡa == nil) {
        return default!;
    }
    return new IPAddrжΔAddr(Ꮡa);
}

// ResolveIPAddr returns an address of IP end point.
//
// The network must be an IP network name.
//
// If the host in the address parameter is not a literal IP address,
// ResolveIPAddr resolves the address to an address of IP end point.
// Otherwise, it parses the address as a literal IP address.
// The address parameter can use a host name, but this is not
// recommended, because it will return at most one of the host name's
// IP addresses.
//
// See func [Dial] for a description of the network and address
// parameters.
public static (ж<IPAddr>, error) ResolveIPAddr(@string network, @string address) {
    if (network == ""u8) {
        // a hint wildcard for Go 1.0 undocumented behavior
        network = "ip"u8;
    }
    var (afnet, _, err) = parseNetwork(context.Background(), network, false);
    if (err != default!) {
        return (default!, err);
    }
    var exprᴛ1 = afnet;
    if (exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8) {
    }
    else { /* default: */
        return (default!, ((UnknownNetworkError)network));
    }

    (var addrs, err) = DefaultResolver.internetAddrList(context.Background(), afnet, address);
    if (err != default!) {
        return (default!, err);
    }
    return (addrs.forResolve(network, address)._<ж<IPAddr>>(), default!);
}

// IPConn is the implementation of the [Conn] and [PacketConn] interfaces
// for IP network connections.
partial struct IPConn {
    internal partial ref conn conn { get; }
}

// SyscallConn returns a raw network connection.
// This implements the [syscall.Conn] interface.
public static (syscall.RawConn, error) SyscallConn(this ж<IPConn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(IPConn.Ꮡconn).ok()) {
        return (default!, syscall.EINVAL);
    }
    return (new rawConnжRawConn(newRawConn(c.fd)), default!);
}

// ReadFromIP acts like ReadFrom but returns an IPAddr.
public static (nint, ж<IPAddr>, error) ReadFromIP(this ж<IPConn> Ꮡc, slice<byte> b) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(IPConn.Ꮡconn).ok()) {
        return (0, default!, syscall.EINVAL);
    }
    var (n, addr, err) = c.readFrom(b);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "read"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: (~c.fd).raddr, Err: err)));
    }
    return (n, addr, err);
}

// ReadFrom implements the [PacketConn] ReadFrom method.
public static (nint, ΔAddr, error) ReadFrom(this ж<IPConn> Ꮡc, slice<byte> b) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(IPConn.Ꮡconn).ok()) {
        return (0, default!, syscall.EINVAL);
    }
    var (n, addr, err) = c.readFrom(b);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "read"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: (~c.fd).raddr, Err: err)));
    }
    if (addr == nil) {
        return (n, default!, err);
    }
    return (n, new IPAddrжΔAddr(addr), err);
}

// ReadMsgIP reads a message from c, copying the payload into b and
// the associated out-of-band data into oob. It returns the number of
// bytes copied into b, the number of bytes copied into oob, the flags
// that were set on the message and the source address of the message.
//
// The packages golang.org/x/net/ipv4 and golang.org/x/net/ipv6 can be
// used to manipulate IP-level socket options in oob.
public static (nint n, nint oobn, nint flags, ж<IPAddr> addr, error err) ReadMsgIP(this ж<IPConn> Ꮡc, slice<byte> b, slice<byte> oob) {
    nint n = default!;
    nint oobn = default!;
    nint flags = default!;
    ж<IPAddr> addr = default!;
    error err = default!;

    ref var c = ref Ꮡc.DerefOrNull();
    if (!Ꮡc.of(IPConn.Ꮡconn).ok()) {
        return (0, 0, 0, default!, syscall.EINVAL);
    }
    (n, oobn, flags, addr, err) = c.readMsg(b, oob);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "read"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: (~c.fd).raddr, Err: err)));
    }
    return (n, oobn, flags, addr, err);
}

// WriteToIP acts like [IPConn.WriteTo] but takes an [IPAddr].
public static (nint, error) WriteToIP(this ж<IPConn> Ꮡc, slice<byte> b, ж<IPAddr> Ꮡaddr) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(IPConn.Ꮡconn).ok()) {
        return (0, syscall.EINVAL);
    }
    var (n, err) = c.writeTo(b, Ꮡaddr);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "write"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: Ꮡaddr.opAddr(), Err: err)));
    }
    return (n, err);
}

// WriteTo implements the [PacketConn] WriteTo method.
public static (nint, error) WriteTo(this ж<IPConn> Ꮡc, slice<byte> b, ΔAddr addr) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(IPConn.Ꮡconn).ok()) {
        return (0, syscall.EINVAL);
    }
    var (a, ok) = addr._<ж<IPAddr>>(ᐧ);
    if (!ok) {
        return (0, new OpErrorжerror(Ꮡ(new OpError(Op: "write"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: addr, Err: syscall.EINVAL))));
    }
    var (n, err) = c.writeTo(b, a);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "write"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: a.opAddr(), Err: err)));
    }
    return (n, err);
}

// WriteMsgIP writes a message to addr via c, copying the payload from
// b and the associated out-of-band data from oob. It returns the
// number of payload and out-of-band bytes written.
//
// The packages golang.org/x/net/ipv4 and golang.org/x/net/ipv6 can be
// used to manipulate IP-level socket options in oob.
public static (nint n, nint oobn, error err) WriteMsgIP(this ж<IPConn> Ꮡc, slice<byte> b, slice<byte> oob, ж<IPAddr> Ꮡaddr) {
    nint n = default!;
    nint oobn = default!;
    error err = default!;

    ref var c = ref Ꮡc.DerefOrNull();
    if (!Ꮡc.of(IPConn.Ꮡconn).ok()) {
        return (0, 0, syscall.EINVAL);
    }
    (n, oobn, err) = c.writeMsg(b, oob, Ꮡaddr);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "write"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: Ꮡaddr.opAddr(), Err: err)));
    }
    return (n, oobn, err);
}

internal static ж<IPConn> newIPConn(ж<netFD> Ꮡfd) {
    return Ꮡ(new IPConn(new conn(Ꮡfd)));
}

// DialIP acts like [Dial] for IP networks.
//
// The network must be an IP network name; see func Dial for details.
//
// If laddr is nil, a local address is automatically chosen.
// If the IP field of raddr is nil or an unspecified IP address, the
// local system is assumed.
public static (ж<IPConn>, error) DialIP(@string network, ж<IPAddr> Ꮡladdr, ж<IPAddr> Ꮡraddr) {
    if (Ꮡraddr == nil) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "dial"u8, Net: network, Source: Ꮡladdr.opAddr(), Addr: default!, Err: errMissingAddress))));
    }
    var sd = Ꮡ(new sysDialer(network: network, address: Ꮡraddr.String()));
    var (c, err) = sd.dialIP(context.Background(), Ꮡladdr, Ꮡraddr);
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "dial"u8, Net: network, Source: Ꮡladdr.opAddr(), Addr: Ꮡraddr.opAddr(), Err: err))));
    }
    return (c, default!);
}

// ListenIP acts like [ListenPacket] for IP networks.
//
// The network must be an IP network name; see func Dial for details.
//
// If the IP field of laddr is nil or an unspecified IP address,
// ListenIP listens on all available IP addresses of the local system
// except multicast IP addresses.
public static (ж<IPConn>, error) ListenIP(@string network, ж<IPAddr> Ꮡladdr) {
    ref var laddr = ref Ꮡladdr.DerefOrNull();

    if (Ꮡladdr == nil) {
        Ꮡladdr = Ꮡ(new IPAddr(nil)); laddr = ref Ꮡladdr.DerefOrNull();
    }
    var sl = Ꮡ(new sysListener(network: network, address: Ꮡladdr.String()));
    var (c, err) = sl.listenIP(context.Background(), Ꮡladdr);
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: network, Source: default!, Addr: Ꮡladdr.opAddr(), Err: err))));
    }
    return (c, default!);
}

} // end net_package
