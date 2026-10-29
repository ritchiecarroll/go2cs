// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || wasip1

// sysfdType is the native type of a file handle
// (int on Unix, syscall.Handle on Windows),
// permitting helper functions to be written portably.
global using sysfdType = nint;

namespace go;

using errors = errors_package;
using unix = @internal.syscall.unix_package;
using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @internal.syscall;
using fs = go.io.fs_package;
using go.io;
using poll = @internal.poll_package;

partial class os_package {

// openRootNolog is OpenRoot.
internal static (ж<Root>, error) openRootNolog(@string name) {
    nint fd = default;
    var err = ignoringEINTR(() => {
        error errΔ1 = default;
        (fd, _, errΔ1) = open(name, syscall.O_CLOEXEC, 0);
        return errΔ1;
    });
    if (err != nil) {
        return (default, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: err))));
    }
    return newRoot(fd, name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notADirectoryˢ = "not a directory"u8;

// newRoot returns a new Root.
// If fd is not a directory, it closes it and returns an error.
internal static (ж<Root>, error) newRoot(nint fd, @string name) {
    ref var fs = ref heap(new fileStat(), out var Ꮡfs);
    var err = ignoringEINTR(() => syscall.Fstat(fd, Ꮡfs.of(fileStat.Ꮡsys)));
    fillFileStatFromSys(Ꮡfs, name);
    if (err == nil && !fs.IsDir()) {
        syscall.Close(fd);
        return (default, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: errors.New(notADirectoryˢ)))));
    }

    // There's a race here with fork/exec, which we are
    // content to live with. See ../syscall/exec_unix.go.
    if (!supportsCloseOnExec) {
        syscall.CloseOnExec(fd);
    }

    var r = Ꮡ(new Root(Ꮡ(new root(
        fd: fd,
        name: name
    ))
    ));
    runtime.SetFinalizer(r.root.OrTypedNil(), ((Func<ж<root>, error>)(Close)));
    return (r, default);
}

// openRootInRoot is Root.OpenRoot.
internal static (ж<Root>, error) openRootInRoot(ж<Root> Ꮡr, @string name) {
    ref var r = ref Ꮡr.DerefOrNull();

    var (fd, err) = doInRoot(ref (Ꮡr).DerefOrNull(), name, 0, default, (nint fd, error err) (nint parent, @string nameΔ1, bool endsInSlash) => {
        nint fdΔ1 = default;
        error errΔ1 = default;
        ignoringEINTR(() => {
            (fdΔ1, errΔ1) = unix.Openat(parent, nameΔ1, (nint)((nint)syscall.O_NOFOLLOW | (nint)syscall.O_CLOEXEC), 0);
            if (isNoFollowErr(errΔ1)) {
                errΔ1 = checkSymlink(parent, nameΔ1, errΔ1);
            }
            return errΔ1;
        });
        return (fdΔ1, errΔ1);
    });
    if (err != nil) {
        return (default, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "openat"u8, Path: name, Err: err))));
    }
    return newRoot(fd, joinPath(r.Name(), name));
}

// rootOpenFileNolog is Root.OpenFile.
internal static (ж<File>, error) rootOpenFileNolog(ж<Root> Ꮡroot, @string name, nint flag, FileMode perm) {
    ref var root = ref Ꮡroot.DerefOrNull();

    var (fd, err) = doInRoot(ref (Ꮡroot).DerefOrNull(), name, 0, default, (nint fd, error err) (nint parent, @string nameΔ1, bool endsInSlash) => {
        nint fdΔ1 = default;
        error errΔ1 = default;
        ignoringEINTR(() => {
            nint openFlag = (nint)((nint)(nint)((nint)syscall.O_NOFOLLOW | (nint)syscall.O_CLOEXEC) | flag);
            (fdΔ1, errΔ1) = unix.Openat(parent, nameΔ1, openFlag, (uint32)perm);
            if (errΔ1 != nil) {
                // Never follow symlinks when O_CREATE|O_EXCL, no matter
                // what error the OS returns.
                var isCreateExcl = (flag & (nint)((nint)(O_CREATE | O_EXCL))) == ((nint)(O_CREATE | O_EXCL));
                if (!isCreateExcl && (isNoFollowErr(errΔ1) || AreEqual(errΔ1, syscall.ENOTDIR))) {
                    errΔ1 = checkSymlink(parent, nameΔ1, errΔ1);
                }
                // AIX returns ELOOP instead of EEXIST for a dangling symlink.
                // Convert this to EEXIST so it matches ErrExists.
                if (isCreateExcl && AreEqual(errΔ1, syscall.ELOOP)) {
                    errΔ1 = syscall.EEXIST;
                }
            }
            return errΔ1;
        });
        return (fdΔ1, errΔ1);
    });
    if (err != nil) {
        return (default, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "openat"u8, Path: name, Err: err))));
    }
    var f = newFile(fd, joinPath(root.Name(), name), kindOpenFile, unix.HasNonblockFlag(flag));
    f.Value.inRoot = true;
    return (f, default);
}

internal static (nint, error) rootOpenDir(nint parent, @string name) {
    nint fd = default;
    ref var err = ref heap<error>(out var Ꮡerr);
    ignoringEINTR(() => {
        (fd, Ꮡerr.ValueSlot) = unix.Openat(parent, name, (nint)((nint)(UntypedInt)(syscall.O_NOFOLLOW | syscall.O_CLOEXEC) | (nint)syscall.O_DIRECTORY), 0);
        if (isNoFollowErr(Ꮡerr.ValueSlot) || AreEqual(Ꮡerr.ValueSlot, syscall.ENOTDIR)) {
            Ꮡerr.ValueSlot = checkSymlink(parent, name, Ꮡerr.ValueSlot);
        } else if (AreEqual(Ꮡerr.ValueSlot, syscall.ENOTSUP) || AreEqual(Ꮡerr.ValueSlot, syscall.EOPNOTSUPP)) {
            // ENOTSUP and EOPNOTSUPP are often, but not always, the same errno.
            // Translate both to ENOTDIR, since this indicates a non-terminal
            // path component was not a directory.
            Ꮡerr.ValueSlot = syscall.ENOTDIR;
        }
        return Ꮡerr.ValueSlot;
    });
    return (fd, err);
}

internal static (FileInfo, error) rootStat(ref Root r, @string name, bool lstat) {
    var (fi, err) = doInRoot(ref r, name, 0, default, (FileInfo, error) (sysfdType parent, @string n, bool endsInSlash) => {
        var (fiΔ1, errΔ1) = lstatatWithName(parent, name, n);
        if (errΔ1 != nil) {
            return (default, errΔ1);
        }
        if (!lstat && (fs.FileMode)(fiΔ1.Mode() & ModeSymlink) != 0) {
            return (default, checkSymlink(parent, n, syscall.ELOOP));
        }
        return (fiΔ1, default);
    });
    if (err != nil) {
        return (default, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "statat"u8, Path: name, Err: err))));
    }
    return (fi, default);
}

internal static error rootSymlink(ref Root r, @string oldname, @string newname) {
    var (_, err) = doInRoot(ref r, newname, 0, default, (EmptyStruct, error) (sysfdType parent, @string name, bool endsInSlash) => (new EmptyStruct(), symlinkat(oldname, parent, name)));
    if (err != nil) {
        return new LinkErrorжerror(Ꮡ(new LinkError("symlinkat"u8, oldname, newname, err)));
    }
    return default;
}

// On systems which use fchmodat, fchownat, etc., we have a race condition:
// When "name" is a symlink, Root.Chmod("name") should act on the target of that link.
// However, fchmodat doesn't allow us to chmod a file only if it is not a symlink;
// the AT_SYMLINK_NOFOLLOW parameter causes the operation to act on the symlink itself.
//
// We do the best we can by first checking to see if the target of the operation is a symlink,
// and only attempting the fchmodat if it is not. If the target is replaced between the check
// and the fchmodat, we will chmod the symlink rather than following it.
//
// This race condition is unfortunate, but does not permit escaping a root:
// We may act on the wrong file, but that file will be contained within the root.
internal static error afterResolvingSymlink(nint parent, @string name, Func<error> f) {
    {
        var err = checkSymlink(parent, name, default); if (err != nil) {
            return err;
        }
    }
    return f();
}

internal static error chmodat(nint parent, @string name, FileMode mode) {
    return afterResolvingSymlink(parent, name, () => ignoringEINTR(() => unix.Fchmodat(parent, name, syscallMode(mode), unix.AT_SYMLINK_NOFOLLOW)));
}

internal static error chownat(nint parent, @string name, nint uid, nint gid) {
    return afterResolvingSymlink(parent, name, () => ignoringEINTR(() => unix.Fchownat(parent, name, uid, gid, unix.AT_SYMLINK_NOFOLLOW)));
}

internal static error lchownat(nint parent, @string name, nint uid, nint gid) {
    return ignoringEINTR(() => unix.Fchownat(parent, name, uid, gid, unix.AT_SYMLINK_NOFOLLOW));
}

internal static error chtimesat(nint parent, @string name, time.Time atime, time.Time mtime) {
    var atimeʗ1 = atime;
    var mtimeʗ1 = mtime;
    return afterResolvingSymlink(parent, name, () => {
        var atimeʗ2 = atimeʗ1;
        var mtimeʗ2 = mtimeʗ1;
        return ignoringEINTR(() => {
            ref var utimes = ref heap<array<syscall.Timespec>>(chtimesUtimes(atimeʗ2, mtimeʗ2), out var Ꮡutimes);
            return unix.Utimensat(parent, name, Ꮡutimes, unix.AT_SYMLINK_NOFOLLOW);
        });
    });
}

internal static error mkdirat(nint fd, @string name, FileMode perm) {
    return ignoringEINTR(() => unix.Mkdirat(fd, name, syscallMode(perm)));
}

internal static error removeat(nint fd, @string name) {
    // The system call interface forces us to know whether
    // we are removing a file or directory. Try both.
    var e = ignoringEINTR(() => unix.Unlinkat(fd, name, 0));
    if (e == nil) {
        return default;
    }
    var e1 = ignoringEINTR(() => unix.Unlinkat(fd, name, unix.AT_REMOVEDIR));
    if (e1 == nil) {
        return default;
    }
    // Both failed. See comment in Remove for how we decide which error to return.
    if (!AreEqual(e1, syscall.ENOTDIR)) {
        return e1;
    }
    return e;
}

internal static error removefileat(nint fd, @string name) {
    return ignoringEINTR(() => unix.Unlinkat(fd, name, 0));
}

internal static error removedirat(nint fd, @string name) {
    return ignoringEINTR(() => unix.Unlinkat(fd, name, unix.AT_REMOVEDIR));
}

internal static error renameat(nint oldfd, @string oldname, nint newfd, @string newname) {
    return unix.Renameat(oldfd, oldname, newfd, newname);
}

internal static error linkat(nint oldfd, @string oldname, nint newfd, @string newname) {
    return unix.Linkat(oldfd, oldname, newfd, newname, 0);
}

internal static error symlinkat(@string oldname, nint newfd, @string newname) {
    return unix.Symlinkat(oldname, newfd, newname);
}

internal static (FileInfo, error) lstatat(nint parent, @string name) {
    return lstatatWithName(parent, name, name);
}

internal static (FileInfo, error) lstatatWithName(nint parent, @string origName, @string name) {
    ref var fs = ref heap(new fileStat(), out var Ꮡfs);
    {
        var err = unix.Fstatat(parent, name, Ꮡfs.of(fileStat.Ꮡsys), unix.AT_SYMLINK_NOFOLLOW); if (err != nil) {
            return (default, err);
        }
    }
    fillFileStatFromSys(Ꮡfs, origName);
    return (new fileStatжFileInfo(Ꮡfs), default);
}

// checkSymlink resolves the symlink name in parent,
// and returns errSymlink with the link contents.
//
// If name is not a symlink, return origError.
internal static error checkSymlink(nint parent, @string name, error origError) {
    var (link, err) = readlinkat(parent, name);
    if (err != nil) {
        return origError;
    }
    return ((errSymlink)link);
}

internal static (@string, error) readlinkat(nint fd, @string name) {
    for (nint len = 128; ; len *= 2) {
        var b = new slice<byte>(len);
        nint n = default;
        ref var e = ref heap<error>(out var Ꮡe);
        var bʗ1 = b;
        ignoringEINTR(() => {
            (n, Ꮡe.ValueSlot) = unix.Readlinkat(fd, name, bʗ1);
            return Ꮡe.ValueSlot;
        });
        if (AreEqual(e, syscall.ERANGE)) {
            continue;
        }
        if (e != nil) {
            return ("", e);
        }
        n = max(n, 0);
        if (n < len) {
            return ((@string)(b.slice(0, n)), default);
        }
    }
}

internal static bool isLink(FileInfo fi) {
    return (fs.FileMode)(fi.Mode() & ModeSymlink) != 0;
}

// isDirectoryLink always returns false, because Unix systems don't have separate
// symlink types for files and directories.
// (See the Windows version of this function for more details.)
internal static bool isDirectoryLink(FileInfo fi) {
    return false;
}

} // end os_package
