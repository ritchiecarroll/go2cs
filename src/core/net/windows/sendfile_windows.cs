// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using poll = @internal.poll_package;
using Δio = io_package;
using os = os_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

internal const bool supportsSendfile = true;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string transmitfileˢ = "transmitfile"u8;

// sendFile copies the contents of r to c using the TransmitFile
// system call to minimize copies.
//
// if handled == true, sendFile returns the number of bytes copied and any
// non-EOF error.
//
// if handled == false, sendFile performed no work.
internal static (int64 written, error err, bool handled) sendFile(ж<netFD> Ꮡfd, Δio.Reader r) {
    int64 written = default!;
    error err = default!;
    bool handled = default!;

    int64 n = 0;     // by default, copy until EOF.

    var (lr, ok) = r._<ж<Δio.LimitedReader>>(ᐧ);
    if (ok) {
        (n, r) = (lr.Value.N, lr.Value.R);
        if (n <= 0) {
            return (0, default!, true);
        }
    }

    (var f, ok) = r._<ж<os.File>>(ᐧ);
    if (!ok) {
        return (0, default!, false);
    }

    (written, err) = poll.SendFile(Ꮡfd.of(netFD.Ꮡpfd), ((syscallꓸHandle)f.Fd()), n);
    if (err != default!) {
        err = wrapSyscallError(transmitfileˢ, err);
    }

    // If any byte was copied, regardless of any error
    // encountered mid-way, handled must be set to true.
    handled = written > 0;

    return (written, err, handled);
}

} // end net_package
