// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using poll = @internal.poll_package;
using Δio = io_package;
using os = os_package;
using @internal;
using syscall = syscall_package;

partial class net_package {

internal const bool supportsSendfile = true;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sendfileˢ = "sendfile"u8;

// sendFile copies the contents of r to c using the sendfile
// system call to minimize copies.
//
// if handled == true, sendFile returns the number (potentially zero) of bytes
// copied and any non-EOF error.
//
// if handled == false, sendFile performed no work.
internal static (int64 written, error err, bool handled) sendFile(ж<netFD> Ꮡc, Δio.Reader r) {
    int64 written = default!;
    error err = default!;
    bool handled = default!;

    int64 remain = 0;     // 0 indicates sending until EOF

    var (lr, ok) = r._<ж<Δio.LimitedReader>>(ᐧ);
    if (ok) {
        (remain, r) = (lr.Value.N, lr.Value.R);
        if (remain <= 0) {
            return (0, default!, true);
        }
    }
    (var f, ok) = r._<ж<os.File>>(ᐧ);
    if (!ok) {
        return (0, default!, false);
    }

    (var sc, err) = f.SyscallConn();
    if (err != default!) {
        return (0, default!, false);
    }

    ref var werr = ref heap<error>(out var Ꮡwerr);
    err = sc.Read((uintptr fd) => {
        (written, Ꮡwerr.ValueSlot, handled) = poll.SendFile(Ꮡc.of(netFD.Ꮡpfd), (nint)fd, remain);
        return true;
    });
    if (err == default!) {
        err = werr;
    }

    if (lr != nil) {
        lr.Value.N = remain - written;
    }
    return (written, wrapSyscallError(sendfileˢ, err), handled);
}

} // end net_package
