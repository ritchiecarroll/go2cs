// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux || (darwin && !ios) || dragonfly || freebsd || solaris || windows
namespace go;

using poll = @internal.poll_package;
using Δio = io_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

internal static Func<bool> testHookSupportsSendfile;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sendfileˢ = "sendfile"u8;

// sendFile copies the contents of r to c using the sendfile
// system call to minimize copies.
//
// if handled == true, sendFile returns the number (potentially zero) of bytes
// copied and any non-EOF error.
//
// if handled == false, sendFile performed no work.
internal static (int64 written, error err, bool handled) sendFile(ж<netFD> c, Δio.Reader r) {
    int64 written = default;
    error err = default;
    bool handled = default;

    if (!supportsSendfile()) {
        return (0, default, false);
    }
    int64 remain = 0; // 0 writes the entire file
    var (lr, ok) = r._<ж<Δio.LimitedReader>>(ᐧ);
    if (ok) {
        (remain, r) = (lr.N, lr.R);
        if (remain <= 0) {
            return (0, default, true);
        }
    }
    // r might be an *os.File or an os.fileWithoutWriteTo.
    // Type assert to an interface rather than *os.File directly to handle the latter case.
    (var f, ok) = r._<syscall.Conn>(ᐧ);
    if (!ok) {
        return (0, default, false);
    }

    (var sc, err) = f.SyscallConn();
    if (err != nil) {
        return (0, default, false);
    }

    ref var werr = ref heap<error>(out var Ꮡwerr);
    err = sc.Read((uintptr fd) => {
        (written, Ꮡwerr.ValueSlot, handled) = poll.SendFile(c.of(netFD.Ꮡpfd), fd, remain);
        return true;
    });
    if (err == nil) {
        err = werr;
    }

    if (lr != nil) {
        lr.N = remain - written;
    }

    return (written, wrapSyscallError(sendfileˢ, err), handled);
}

} // end net_package
