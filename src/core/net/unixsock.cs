// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using os = os_package;
using Δsync = sync_package;
using syscall = syscall_package;
using time = time_package;

partial class net_package {

// BUG(mikio): On JS, WASIP1 and Plan 9, methods and functions related
// to UnixConn and UnixListener are not implemented.
// BUG(mikio): On Windows, methods and functions related to UnixConn
// and UnixListener don't work for "unixgram" and "unixpacket".

// UnixAddr represents the address of a Unix domain socket end point.
partial struct UnixAddr {
    public @string Name;
    public @string Net;
}

// Network returns the address's network name, "unix", "unixgram" or
// "unixpacket".
public static @string Network(this ref UnixAddr a) {
    return a.Net;
}

public static @string String(this ж<UnixAddr> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil) {
        return nilˢ;
    }
    return a.Name;
}

internal static bool isWildcard(this ж<UnixAddr> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    return Ꮡa == nil || a.Name == ""u8;
}

internal static ΔAddr opAddr(this ж<UnixAddr> Ꮡa) {
    if (Ꮡa == nil) {
        return default!;
    }
    return new UnixAddrжΔAddr(Ꮡa);
}

// ResolveUnixAddr returns an address of Unix domain socket end point.
//
// The network must be a Unix network name.
//
// See func [Dial] for a description of the network and address
// parameters.
public static (ж<UnixAddr>, error) ResolveUnixAddr(@string network, @string address) {
    var exprᴛ1 = network;
    if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8) {
        return (Ꮡ(new UnixAddr(Name: address, Net: network)), default!);
    }
    { /* default: */
        return (default!, ((UnknownNetworkError)network));
    }

}

// UnixConn is an implementation of the [Conn] interface for connections
// to Unix domain sockets.
partial struct UnixConn {
    internal partial ref conn conn { get; }
}

// SyscallConn returns a raw network connection.
// This implements the [syscall.Conn] interface.
public static (syscall.RawConn, error) SyscallConn(this ж<UnixConn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(UnixConn.Ꮡconn).ok()) {
        return (default!, syscall.EINVAL);
    }
    return (new rawConnжRawConn(newRawConn(c.fd)), default!);
}

// CloseRead shuts down the reading side of the Unix domain connection.
// Most callers should just use Close.
public static error CloseRead(this ж<UnixConn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(UnixConn.Ꮡconn).ok()) {
        return syscall.EINVAL;
    }
    {
        var err = c.fd.closeRead(); if (err != default!) {
            return new OpErrorжerror(Ꮡ(new OpError(Op: "close"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: (~c.fd).raddr, Err: err)));
        }
    }
    return default!;
}

// CloseWrite shuts down the writing side of the Unix domain connection.
// Most callers should just use Close.
public static error CloseWrite(this ж<UnixConn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(UnixConn.Ꮡconn).ok()) {
        return syscall.EINVAL;
    }
    {
        var err = c.fd.closeWrite(); if (err != default!) {
            return new OpErrorжerror(Ꮡ(new OpError(Op: "close"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: (~c.fd).raddr, Err: err)));
        }
    }
    return default!;
}

// ReadFromUnix acts like [UnixConn.ReadFrom] but returns a [UnixAddr].
public static (nint, ж<UnixAddr>, error) ReadFromUnix(this ж<UnixConn> Ꮡc, slice<byte> b) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(UnixConn.Ꮡconn).ok()) {
        return (0, default!, syscall.EINVAL);
    }
    var (n, addr, err) = c.readFrom(b);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "read"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: (~c.fd).raddr, Err: err)));
    }
    return (n, addr, err);
}

// ReadFrom implements the [PacketConn] ReadFrom method.
public static (nint, ΔAddr, error) ReadFrom(this ж<UnixConn> Ꮡc, slice<byte> b) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(UnixConn.Ꮡconn).ok()) {
        return (0, default!, syscall.EINVAL);
    }
    var (n, addr, err) = c.readFrom(b);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "read"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: (~c.fd).raddr, Err: err)));
    }
    if (addr == nil) {
        return (n, default!, err);
    }
    return (n, new UnixAddrжΔAddr(addr), err);
}

// ReadMsgUnix reads a message from c, copying the payload into b and
// the associated out-of-band data into oob. It returns the number of
// bytes copied into b, the number of bytes copied into oob, the flags
// that were set on the message and the source address of the message.
//
// Note that if len(b) == 0 and len(oob) > 0, this function will still
// read (and discard) 1 byte from the connection.
public static (nint n, nint oobn, nint flags, ж<UnixAddr> addr, error err) ReadMsgUnix(this ж<UnixConn> Ꮡc, slice<byte> b, slice<byte> oob) {
    nint n = default!;
    nint oobn = default!;
    nint flags = default!;
    ж<UnixAddr> addr = default!;
    error err = default!;

    ref var c = ref Ꮡc.DerefOrNull();
    if (!Ꮡc.of(UnixConn.Ꮡconn).ok()) {
        return (0, 0, 0, default!, syscall.EINVAL);
    }
    (n, oobn, flags, addr, err) = c.readMsg(b, oob);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "read"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: (~c.fd).raddr, Err: err)));
    }
    return (n, oobn, flags, addr, err);
}

// WriteToUnix acts like [UnixConn.WriteTo] but takes a [UnixAddr].
public static (nint, error) WriteToUnix(this ж<UnixConn> Ꮡc, slice<byte> b, ж<UnixAddr> Ꮡaddr) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(UnixConn.Ꮡconn).ok()) {
        return (0, syscall.EINVAL);
    }
    var (n, err) = c.writeTo(b, Ꮡaddr);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "write"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: Ꮡaddr.opAddr(), Err: err)));
    }
    return (n, err);
}

// WriteTo implements the [PacketConn] WriteTo method.
public static (nint, error) WriteTo(this ж<UnixConn> Ꮡc, slice<byte> b, ΔAddr addr) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.of(UnixConn.Ꮡconn).ok()) {
        return (0, syscall.EINVAL);
    }
    var (a, ok) = addr._<ж<UnixAddr>>(ᐧ);
    if (!ok) {
        return (0, new OpErrorжerror(Ꮡ(new OpError(Op: "write"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: addr, Err: syscall.EINVAL))));
    }
    var (n, err) = c.writeTo(b, a);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "write"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: a.opAddr(), Err: err)));
    }
    return (n, err);
}

// WriteMsgUnix writes a message to addr via c, copying the payload
// from b and the associated out-of-band data from oob. It returns the
// number of payload and out-of-band bytes written.
//
// Note that if len(b) == 0 and len(oob) > 0, this function will still
// write 1 byte to the connection.
public static (nint n, nint oobn, error err) WriteMsgUnix(this ж<UnixConn> Ꮡc, slice<byte> b, slice<byte> oob, ж<UnixAddr> Ꮡaddr) {
    nint n = default!;
    nint oobn = default!;
    error err = default!;

    ref var c = ref Ꮡc.DerefOrNull();
    if (!Ꮡc.of(UnixConn.Ꮡconn).ok()) {
        return (0, 0, syscall.EINVAL);
    }
    (n, oobn, err) = c.writeMsg(b, oob, Ꮡaddr);
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "write"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: Ꮡaddr.opAddr(), Err: err)));
    }
    return (n, oobn, err);
}

internal static ж<UnixConn> newUnixConn(ж<netFD> Ꮡfd) {
    return Ꮡ(new UnixConn(new conn(Ꮡfd)));
}

// DialUnix acts like [Dial] for Unix networks.
//
// The network must be a Unix network name; see func Dial for details.
//
// If laddr is non-nil, it is used as the local address for the
// connection.
public static (ж<UnixConn>, error) DialUnix(@string network, ж<UnixAddr> Ꮡladdr, ж<UnixAddr> Ꮡraddr) {
    var exprᴛ1 = network;
    if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8) {
    }
    else { /* default: */
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "dial"u8, Net: network, Source: Ꮡladdr.opAddr(), Addr: Ꮡraddr.opAddr(), Err: ((UnknownNetworkError)network)))));
    }

    var sd = Ꮡ(new sysDialer(network: network, address: Ꮡraddr.String()));
    var (c, err) = sd.dialUnix(context.Background(), Ꮡladdr, Ꮡraddr);
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "dial"u8, Net: network, Source: Ꮡladdr.opAddr(), Addr: Ꮡraddr.opAddr(), Err: err))));
    }
    return (c, default!);
}

// UnixListener is a Unix domain socket listener. Clients should
// typically use variables of type [Listener] instead of assuming Unix
// domain sockets.
partial struct UnixListener {
    internal ж<netFD> fd;
    internal @string path;
    internal bool unlink;
    internal Δsync.Once unlinkOnce;
}

internal static bool ok(this ж<UnixListener> Ꮡln) {
    ref var ln = ref Ꮡln.DerefOrNull();

    return Ꮡln != nil && ln.fd != nil;
}

// SyscallConn returns a raw network connection.
// This implements the [syscall.Conn] interface.
//
// The returned RawConn only supports calling Control. Read and
// Write return an error.
public static (syscall.RawConn, error) SyscallConn(this ж<UnixListener> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (!Ꮡl.ok()) {
        return (default!, syscall.EINVAL);
    }
    return (new rawListenerжRawConn(newRawListener(l.fd)), default!);
}

// AcceptUnix accepts the next incoming call and returns the new
// connection.
public static (ж<UnixConn>, error) AcceptUnix(this ж<UnixListener> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (!Ꮡl.ok()) {
        return (default!, syscall.EINVAL);
    }
    var (c, err) = l.accept();
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "accept"u8, Net: (~l.fd).net, Source: default!, Addr: (~l.fd).laddr, Err: err))));
    }
    return (c, default!);
}

// Accept implements the Accept method in the [Listener] interface.
// Returned connections will be of type [*UnixConn].
public static (Conn, error) Accept(this ж<UnixListener> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (!Ꮡl.ok()) {
        return (default!, syscall.EINVAL);
    }
    var (c, err) = l.accept();
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "accept"u8, Net: (~l.fd).net, Source: default!, Addr: (~l.fd).laddr, Err: err))));
    }
    return (new UnixConnжConn(c), default!);
}

// Close stops listening on the Unix address. Already accepted
// connections are not closed.
public static error Close(this ж<UnixListener> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (!Ꮡl.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = Ꮡl.close(); if (err != default!) {
            return new OpErrorжerror(Ꮡ(new OpError(Op: "close"u8, Net: (~l.fd).net, Source: default!, Addr: (~l.fd).laddr, Err: err)));
        }
    }
    return default!;
}

// Addr returns the listener's network address.
// The Addr returned is shared by all invocations of Addr, so
// do not modify it.
public static ΔAddr Addr(this ref UnixListener l) {
    return (~l.fd).laddr;
}

// SetDeadline sets the deadline associated with the listener.
// A zero time value disables the deadline.
public static error SetDeadline(this ж<UnixListener> Ꮡl, time.Time t) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (!Ꮡl.ok()) {
        return syscall.EINVAL;
    }
    return l.fd.SetDeadline(t);
}

// File returns a copy of the underlying [os.File].
// It is the caller's responsibility to close f when finished.
// Closing l does not affect f, and closing f does not affect l.
//
// The returned os.File's file descriptor is different from the
// connection's. Attempting to change properties of the original
// using this duplicate may or may not have the desired effect.
public static (ж<os.File> f, error err) File(this ж<UnixListener> Ꮡl) {
    ж<os.File> f = default!;
    error err = default!;

    ref var l = ref Ꮡl.DerefOrNull();
    if (!Ꮡl.ok()) {
        return (default!, syscall.EINVAL);
    }
    (f, err) = l.@file();
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "file"u8, Net: (~l.fd).net, Source: default!, Addr: (~l.fd).laddr, Err: err)));
    }
    return (f, err);
}

// ListenUnix acts like [Listen] for Unix networks.
//
// The network must be "unix" or "unixpacket".
public static (ж<UnixListener>, error) ListenUnix(@string network, ж<UnixAddr> Ꮡladdr) {
    var exprᴛ1 = network;
    if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixpacket"u8) {
    }
    else { /* default: */
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: network, Source: default!, Addr: Ꮡladdr.opAddr(), Err: ((UnknownNetworkError)network)))));
    }

    if (Ꮡladdr == nil) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: network, Source: default!, Addr: Ꮡladdr.opAddr(), Err: errMissingAddress))));
    }
    var sl = Ꮡ(new sysListener(network: network, address: Ꮡladdr.String()));
    var (ln, err) = sl.listenUnix(context.Background(), Ꮡladdr);
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: network, Source: default!, Addr: Ꮡladdr.opAddr(), Err: err))));
    }
    return (ln, default!);
}

// ListenUnixgram acts like [ListenPacket] for Unix networks.
//
// The network must be "unixgram".
public static (ж<UnixConn>, error) ListenUnixgram(@string network, ж<UnixAddr> Ꮡladdr) {
    var exprᴛ1 = network;
    if (exprᴛ1 == "unixgram"u8) {
    }
    else { /* default: */
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: network, Source: default!, Addr: Ꮡladdr.opAddr(), Err: ((UnknownNetworkError)network)))));
    }

    if (Ꮡladdr == nil) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: network, Source: default!, Addr: default!, Err: errMissingAddress))));
    }
    var sl = Ꮡ(new sysListener(network: network, address: Ꮡladdr.String()));
    var (c, err) = sl.listenUnixgram(context.Background(), Ꮡladdr);
    if (err != default!) {
        return (default!, new OpErrorжerror(Ꮡ(new OpError(Op: "listen"u8, Net: network, Source: default!, Addr: Ꮡladdr.opAddr(), Err: err))));
    }
    return (c, default!);
}

} // end net_package
