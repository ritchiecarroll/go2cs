// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using poll = @internal.poll_package;
using Δio = io_package;
using @internal;

partial class net_package {

internal static Func<ж<poll.FD>, ж<poll.FD>, int64, (int64, bool, error)> pollSplice = poll.Splice;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string spliceˢ = "splice"u8;

// spliceFrom transfers data from r to c using the splice system call to minimize
// copies from and to userspace. c must be a TCP connection.
// Currently, spliceFrom is only enabled if r is a TCP or a stream-oriented Unix connection.
//
// If spliceFrom returns handled == false, it has performed no work.
internal static (int64 written, error err, bool handled) spliceFrom(ж<netFD> c, Δio.Reader r) {
    int64 written = default;
    error err = default;
    bool handled = default;

    int64 remain = 9223372036854775807L; // by default, copy until EOF
    var (lr, ok) = r._<ж<Δio.LimitedReader>>(ᐧ);
    if (ok) {
        (remain, r) = (lr.N, lr.R);
        if (remain <= 0) {
            return (0, default, true);
        }
    }

    ж<netFD> s = default;
    switch (r.type()) {
    case ж<TCPConn> v: {
        s = v.Value.fd;
        break;
    }
    case tcpConnWithoutWriteTo v: {
        s = v.fd;
        break;
    }
    case ж<UnixConn> v: {
        if ((~v).fd.net != "unix"u8) {
            return (0, default, false);
        }
        s = v.Value.fd;
        break;
    }
    default: {
        var v = r;
        return (0, default, false);
    }}

    (written, handled, err) = pollSplice(c.of(netFD.Ꮡpfd), s.of(netFD.Ꮡpfd), remain);
    if (lr != nil) {
        lr.N -= written;
    }
    return (written, wrapSyscallError(spliceˢ, err), handled);
}

// spliceTo transfers data from c to w using the splice system call to minimize
// copies from and to userspace. c must be a TCP connection.
// Currently, spliceTo is only enabled if w is a stream-oriented Unix connection.
//
// If spliceTo returns handled == false, it has performed no work.
internal static (int64 written, error err, bool handled) spliceTo(Δio.Writer w, ж<netFD> c) {
    int64 written = default;
    error err = default;
    bool handled = default;

    var (uc, ok) = w._<ж<UnixConn>>(ᐧ);
    if (!ok || (~uc).fd.net != "unix"u8) {
        return (written, err, handled);
    }

    (written, handled, err) = pollSplice((~uc).fd.of(netFD.Ꮡpfd), c.of(netFD.Ꮡpfd), 9223372036854775807L);
    return (written, wrapSyscallError(spliceˢ, err), handled);
}

} // end net_package
