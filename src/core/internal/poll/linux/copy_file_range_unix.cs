// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build freebsd || linux
namespace go.@internal;

using unix = go.@internal.syscall.unix_package;
using go.@internal.syscall;

partial class poll_package {

// CopyFileRange copies at most remain bytes of data from src to dst, using
// the copy_file_range system call. dst and src must refer to regular files.
public static (int64 written, bool handled, error err) CopyFileRange(ж<FD> Ꮡdst, ж<FD> Ꮡsrc, int64 remain) {
    int64 written = default!;
    bool handled = default!;
    error err = default!;

    if (!supportCopyFileRange()) {
        return (0, false, default!);
    }

    while (remain > 0) {
        var max = min(remain, (int64)(maxCopyFileRangeRound));
        var (n, e) = copyFileRange(Ꮡdst, Ꮡsrc, (nint)max);
        if (n > 0) {
            remain -= n;
            written += n;
        }
        (handled, err) = handleCopyFileRangeErr(e, n, written);
        if (n == 0 || !handled || err != default!) {
            return (written, handled, err);
        }
    }

    return (written, true, default!);
}

// copyFileRange performs one round of copy_file_range(2).
internal static (int64 written, error err) copyFileRange(ж<FD> Ꮡdst, ж<FD> Ꮡsrc, nint max) {
    int64 written = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        // For Linux, the signature of copy_file_range(2) is:
        //
        // ssize_t copy_file_range(int fd_in, loff_t *off_in,
        //                         int fd_out, loff_t *off_out,
        //                         size_t len, unsigned int flags);
        //
        // For FreeBSD, the signature of copy_file_range(2) is:
        //
        // ssize_t
        // copy_file_range(int infd, off_t *inoffp, int outfd, off_t *outoffp,
        //                 size_t len, unsigned int flags);
        //
        // Note that in the call to unix.CopyFileRange below, we use nil
        // values for off_in/off_out and inoffp/outoffp, which means "the file
        // offset for infd(fd_in) or outfd(fd_out) respectively will be used and
        // updated by the number of bytes copied".
        //
        // That is why we must acquire locks for both file descriptors (and why
        // this whole machinery is in the internal/poll package to begin with).
        {
            var errΔ1 = Ꮡdst.writeLock(); if (errΔ1 != default!) {
                (written, err) = (0, errΔ1); goto ᒐdone;
            }
        }
        defer(Ꮡdst.writeUnlock, ref ᒐ);
        {
            var errΔ2 = Ꮡsrc.readLock(); if (errΔ2 != default!) {
                (written, err) = (0, errΔ2); goto ᒐdone;
            }
        }
        defer(Ꮡsrc.readUnlock, ref ᒐ);
        (written, err) = ignoringEINTR2((int64, error) () => {
            var (n, errΔ3) = unix.CopyFileRange(Ꮡsrc.Value.Sysfd, nil, Ꮡdst.Value.Sysfd, nil, max, 0);
            return ((int64)n, errΔ3);
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (written, err);
}

} // end poll_package
