// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using poll = @internal.poll_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

// BUG(tmm1): On Windows, the Write method of syscall.RawConn
// does not integrate with the runtime's network poller. It cannot
// wait for the connection to become writeable, and does not respect
// deadlines. If the user-provided callback returns false, the Write
// method will fail immediately.
// BUG(mikio): On JS and Plan 9, the Control, Read and Write
// methods of syscall.RawConn are not implemented.
partial struct rawConn {
    internal ж<netFD> fd;
}

internal static bool ok(this ж<rawConn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    return Ꮡc != nil && c.fd != nil;
}

internal static error Control(this ж<rawConn> Ꮡc, Action<uintptr> f) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.ok()) {
        return syscall.EINVAL;
    }
    var err = c.fd.of(netFD.Ꮡpfd).RawControl(f);
    runtime.KeepAlive(c.fd.OrTypedNil());
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "raw-control"u8, Net: (~c.fd).net, Source: default!, Addr: (~c.fd).laddr, Err: err)));
    }
    return err;
}

internal static error Read(this ж<rawConn> Ꮡc, Func<uintptr, bool> f) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.ok()) {
        return syscall.EINVAL;
    }
    var err = c.fd.of(netFD.Ꮡpfd).RawRead(f);
    runtime.KeepAlive(c.fd.OrTypedNil());
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "raw-read"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: (~c.fd).raddr, Err: err)));
    }
    return err;
}

internal static error Write(this ж<rawConn> Ꮡc, Func<uintptr, bool> f) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.ok()) {
        return syscall.EINVAL;
    }
    var err = c.fd.of(netFD.Ꮡpfd).RawWrite(f);
    runtime.KeepAlive(c.fd.OrTypedNil());
    if (err != default!) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "raw-write"u8, Net: (~c.fd).net, Source: (~c.fd).laddr, Addr: (~c.fd).raddr, Err: err)));
    }
    return err;
}

// PollFD returns the poll.FD of the underlying connection.
//
// Other packages in std that also import [internal/poll] (such as os)
// can use a type assertion to access this extension method so that
// they can pass the *poll.FD to functions like poll.Splice.
//
// PollFD is not intended for use outside the standard library.
internal static ж<poll.FD> PollFD(this ж<rawConn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.ok()) {
        return default!;
    }
    return c.fd.of(netFD.Ꮡpfd);
}

internal static ж<rawConn> newRawConn(ж<netFD> Ꮡfd) {
    return Ꮡ(new rawConn(fd: Ꮡfd));
}

// Network returns the network type of the underlying connection.
//
// Other packages in std that import internal/poll and are unable to
// import net (such as os) can use a type assertion to access this
// extension method so that they can distinguish different socket types.
//
// Network is not intended for use outside the standard library.
internal static poll.String Network(this ref rawConn c) {
    return ((poll.String)(~c.fd).net);
}

partial struct rawListener {
    internal partial ref rawConn rawConn { get; }
}

internal static error Read(this ref rawListener l, Func<uintptr, bool> _) {
    return syscall.EINVAL;
}

internal static error Write(this ref rawListener l, Func<uintptr, bool> _) {
    return syscall.EINVAL;
}

internal static ж<rawListener> newRawListener(ж<netFD> Ꮡfd) {
    return Ꮡ(new rawListener(new rawConn(fd: Ꮡfd)));
}

} // end net_package
