// Copyright 2026 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux
namespace go.@internal.syscall;

using strconv = go.@internal.strconv_package;
using syscall = syscall_package;
using go.@internal;

partial class unix_package {

public static error Fchmodat(nint dirfd, @string path, uint32 mode, nint flags) {
    GoFrame ᒐ = default;
    try {
        // On Linux, the fchmodat syscall silently ignores the AT_SYMLINK_NOFOLLOW flag.
        // We need to use fchmodat2 instead.
        // syscall.Fchmodat handles this.
        {
            var errΔ1 = syscall.Fchmodat(dirfd, path, mode, flags); if (!AreEqual(errΔ1, syscall.EOPNOTSUPP)) {
                return errΔ1;
            }
        }

        // This kernel doesn't appear to support fchmodat2 (added in Linux 6.6).
        // We can't fall back to Fchmod, because it requires write permissions on the file.
        // Instead, use the same workaround as GNU libc and musl, which is to open the file
        // and then fchmodat the FD in /proc/self/fd.
        // See: https://lwn.net/Articles/939217/
        var (fd, err) = Openat(dirfd, path, (nint)((nint)(UntypedInt)(O_PATH | syscall.O_NOFOLLOW) | (nint)syscall.O_CLOEXEC), 0);
        if (err != default!) {
            return err;
        }
        defer(syscall.Close, fd, ref ᒐ);
        @string procPath = "/proc/self/fd/"u8 + strconv.Itoa(fd);

        // Check to see if this file is a symlink.
        // (We passed O_NOFOLLOW above, but O_PATH|O_NOFOLLOW will open a symlink.)
        ref var st = ref heap(new syscall.Stat_t(), out var Ꮡst);
        {
            var errΔ2 = syscall.Stat(procPath, Ꮡst); if (errΔ2 != default!) {
                if (AreEqual(errΔ2, syscall.ENOENT)) {
                    // /proc has probably not been mounted. Give up.
                    return syscall.EOPNOTSUPP;
                }
                return errΔ2;
            }
        }
        if ((st.Mode & (uint32)syscall.S_IFMT) == syscall.S_IFLNK) {
            // fchmodat on the proc FD for a symlink apparently gives inconsistent
            // results, so just refuse to try.
            return syscall.EOPNOTSUPP;
        }

        return syscall.Fchmodat(AT_FDCWD, procPath, mode, (nint)(flags & ~(nint)(nint)AT_SYMLINK_NOFOLLOW));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end unix_package
