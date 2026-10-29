// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using errors = errors_package;
using unix = go.@internal.syscall.unix_package;
using Δsyscall = syscall_package;
using go.@internal.syscall;

partial class poll_package {

// Fsync invokes SYS_FCNTL with SYS_FULLFSYNC because
// on OS X, SYS_FSYNC doesn't fully flush contents to disk.
// See Issue #26650 as well as the man page for fsync on OS X.
public static error Fsync(this ж<FD> Ꮡfd) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        {
            var err = Ꮡfd.incref(); if (err != nil) {
                return err;
            }
        }
        ᒐd1 = true;
        return ignoringEINTR(() => {
            var (_, err) = unix.Fcntl(Ꮡfd.Value.Sysfd, Δsyscall.F_FULLFSYNC, 0);

            // There are scenarios such as SMB mounts where fcntl will fail
            // with ENOTSUP. In those cases fallback to fsync.
            // See #64215
            if (err != nil && errors.Is(err, Δsyscall.ENOTSUP)) {
                err = Δsyscall.Fsync(Ꮡfd.Value.Sysfd);
            }
            return err;
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

} // end poll_package
