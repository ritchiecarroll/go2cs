// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris || wasip1
global using sysfdType = nint;

namespace go;

using errors = errors_package;
using unix = @internal.syscall.unix_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @internal.syscall;
using fs = go.io.fs_package;
using poll = @internal.poll_package;

partial class os_package {

// openRootNolog is OpenRoot.
internal static (ж<Root>, error) openRootNolog(@string name) {
    nint fd = default!;
    var err = ignoringEINTR(() => {
        error errΔ1 = default!;
        (fd, _, errΔ1) = open(name, syscall.O_CLOEXEC, 0);
        return errΔ1;
    });
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: err))));
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
    if (err == default! && !fs.IsDir()) {
        syscall.Close(fd);
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: errors.New(notADirectoryˢ)))));
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
    runtime.SetFinalizer((~r).root.OrTypedNil(), ((Func<ж<root>, error>)(Close)));
    return (r, default!);
}

// openRootInRoot is Root.OpenRoot.
internal static (ж<Root>, error) openRootInRoot(ж<Root> Ꮡr, @string name) {
    ref var r = ref Ꮡr.DerefOrNull();

    var (fd, err) = doInRoot(ref (Ꮡr).DerefOrNull(), name, (nint fd, error err) (nint parent, @string nameΔ1) => {
        nint fdΔ1 = default!;
        error errΔ1 = default!;
        ignoringEINTR(() => {
            (fdΔ1, errΔ1) = unix.Openat(parent, nameΔ1, (nint)((nint)syscall.O_NOFOLLOW | (nint)syscall.O_CLOEXEC), 0);
            if (isNoFollowErr(errΔ1)) {
                errΔ1 = checkSymlink(parent, nameΔ1, errΔ1);
            }
            return errΔ1;
        });
        return (fdΔ1, errΔ1);
    });
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "openat"u8, Path: name, Err: err))));
    }
    return newRoot(fd, joinPath(r.Name(), name));
}

// rootOpenFileNolog is Root.OpenFile.
internal static (ж<File>, error) rootOpenFileNolog(ж<Root> Ꮡroot, @string name, nint flag, FileMode perm) {
    ref var root = ref Ꮡroot.DerefOrNull();

    var (fd, err) = doInRoot(ref (Ꮡroot).DerefOrNull(), name, (nint fd, error err) (nint parent, @string nameΔ1) => {
        nint fdΔ1 = default!;
        error errΔ1 = default!;
        ignoringEINTR(() => {
            (fdΔ1, errΔ1) = unix.Openat(parent, nameΔ1, (nint)((nint)(nint)((nint)syscall.O_NOFOLLOW | (nint)syscall.O_CLOEXEC) | flag), (uint32)perm);
            if (isNoFollowErr(errΔ1) || AreEqual(errΔ1, syscall.ENOTDIR)) {
                errΔ1 = checkSymlink(parent, nameΔ1, errΔ1);
            }
            return errΔ1;
        });
        return (fdΔ1, errΔ1);
    });
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "openat"u8, Path: name, Err: err))));
    }
    var f = newFile(fd, joinPath(root.Name(), name), kindOpenFile, unix.HasNonblockFlag(flag));
    return (f, default!);
}

internal static (nint, error) rootOpenDir(nint parent, @string name) {
    nint fd = default!;
    ref var err = ref heap<error>(out var Ꮡerr);
    ignoringEINTR(() => {
        (fd, Ꮡerr.ValueSlot) = unix.Openat(parent, name, (nint)((nint)(UntypedInt)(syscall.O_NOFOLLOW | syscall.O_CLOEXEC) | (nint)syscall.O_DIRECTORY), 0);
        if (isNoFollowErr(Ꮡerr.ValueSlot) || AreEqual(Ꮡerr.ValueSlot, syscall.ENOTDIR)){
            Ꮡerr.ValueSlot = checkSymlink(parent, name, Ꮡerr.ValueSlot);
        } else 
        if (AreEqual(Ꮡerr.ValueSlot, syscall.ENOTSUP) || AreEqual(Ꮡerr.ValueSlot, syscall.EOPNOTSUPP)) {
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
    var (fi, err) = doInRoot(ref r, name, (FileInfo, error) (sysfdType parent, @string n) => {
        ref var fs = ref heap(new fileStat(), out var Ꮡfs);
        {
            var errΔ1 = unix.Fstatat(parent, n, Ꮡfs.of(fileStat.Ꮡsys), unix.AT_SYMLINK_NOFOLLOW); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        fillFileStatFromSys(Ꮡfs, name);
        if (!lstat && (FileMode)(fs.Mode() & ModeSymlink) != 0) {
            return (default!, checkSymlink(parent, n, syscall.ELOOP));
        }
        return (new fileStatжFileInfo(Ꮡfs), default!);
    });
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "statat"u8, Path: name, Err: err))));
    }
    return (fi, default!);
}

internal static error mkdirat(nint fd, @string name, FileMode perm) {
    return ignoringEINTR(() => unix.Mkdirat(fd, name, syscallMode(perm)));
}

internal static error removeat(nint fd, @string name) {
    // The system call interface forces us to know whether
    // we are removing a file or directory. Try both.
    var e = ignoringEINTR(() => unix.Unlinkat(fd, name, 0));
    if (e == default!) {
        return default!;
    }
    var e1 = ignoringEINTR(() => unix.Unlinkat(fd, name, unix.AT_REMOVEDIR));
    if (e1 == default!) {
        return default!;
    }
    // Both failed. See comment in Remove for how we decide which error to return.
    if (!AreEqual(e1, syscall.ENOTDIR)) {
        return e1;
    }
    return e;
}

// checkSymlink resolves the symlink name in parent,
// and returns errSymlink with the link contents.
//
// If name is not a symlink, return origError.
internal static error checkSymlink(nint parent, @string name, error origError) {
    var (link, err) = readlinkat(parent, name);
    if (err != default!) {
        return origError;
    }
    return ((errSymlink)link);
}

internal static (@string, error) readlinkat(nint fd, @string name) {
    for (nint len = 128; ᐧ ; len *= 2) {
        var b = new slice<byte>(len);
        nint n = default!;
        ref var e = ref heap<error>(out var Ꮡe);
        var bʗ1 = b;
        ignoringEINTR(() => {
            (n, Ꮡe.ValueSlot) = unix.Readlinkat(fd, name, bʗ1);
            return Ꮡe.ValueSlot;
        });
        if (AreEqual(e, syscall.ERANGE)) {
            continue;
        }
        if (e != default!) {
            return ("", e);
        }
        if (n < 0) {
            n = 0;
        }
        if (n < len) {
            return (((@string)(b.slice(0, n))), default!);
        }
    }
}

} // end os_package
