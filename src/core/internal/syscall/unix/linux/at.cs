// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build dragonfly || freebsd || linux || netbsd || (openbsd && mips64)
namespace go.@internal.syscall;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class unix_package {

public static error Unlinkat(nint dirfd, @string path, nint flags) {
    var (p, err) = syscall.BytePtrFromString(path);
    if (err != nil) {
        return err;
    }

    var ᴋ0 = p;
        var (_, _, errno) = syscall.Syscall(unlinkatTrap, (uintptr)dirfd, (uintptr)ᴋ0, (uintptr)flags);
    System.GC.KeepAlive(ᴋ0);
    if (errno != 0) {
        return errno;
    }

    return default;
}

public static (nint, error) Openat(nint dirfd, @string path, nint flags, uint32 perm) {
    var (p, err) = syscall.BytePtrFromString(path);
    if (err != nil) {
        return (0, err);
    }

    var ᴋ1 = p;
        var (fd, _, errno) = syscall.Syscall6(openatTrap, (uintptr)dirfd, (uintptr)ᴋ1, (uintptr)flags, (uintptr)perm, 0, 0);
    System.GC.KeepAlive(ᴋ1);
    if (errno != 0) {
        return (0, errno);
    }

    return ((nint)fd, default);
}

public static (nint, error) Readlinkat(nint dirfd, @string path, slice<byte> buf) {
    var (p0, err) = syscall.BytePtrFromString(path);
    if (err != nil) {
        return (0, err);
    }
    @unsafe.Pointer p1 = default;
    if (len(buf) > 0) {
        p1 = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0));
    } else {
        p1 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var ᴋ2 = p0;
    var ᴋ3 = p1;
        var (n, _, errno) = syscall.Syscall6(readlinkatTrap, (uintptr)dirfd, (uintptr)ᴋ2, (uintptr)ᴋ3, (uintptr)len(buf), 0, 0);
    System.GC.KeepAlive(ᴋ2);
    System.GC.KeepAlive(ᴋ3);
    if (errno != 0) {
        return (0, errno);
    }

    return ((nint)n, default);
}

public static error Mkdirat(nint dirfd, @string path, uint32 mode) {
    var (p, err) = syscall.BytePtrFromString(path);
    if (err != nil) {
        return err;
    }

    var ᴋ4 = p;
        var (_, _, errno) = syscall.Syscall6(mkdiratTrap, (uintptr)dirfd, (uintptr)ᴋ4, (uintptr)mode, 0, 0, 0);
    System.GC.KeepAlive(ᴋ4);
    if (errno != 0) {
        return errno;
    }
    return default;
}

public static error Fchownat(nint dirfd, @string path, nint uid, nint gid, nint flags) {
    var (p, err) = syscall.BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var ᴋ5 = p;
        var (_, _, errno) = syscall.Syscall6(fchownatTrap, (uintptr)dirfd, (uintptr)ᴋ5, (uintptr)uid, (uintptr)gid, (uintptr)flags, 0);
    System.GC.KeepAlive(ᴋ5);
    if (errno != 0) {
        return errno;
    }
    return default;
}

public static error Renameat(nint olddirfd, @string oldpath, nint newdirfd, @string newpath) {
    var (oldp, err) = syscall.BytePtrFromString(oldpath);
    if (err != nil) {
        return err;
    }
    (var newp, err) = syscall.BytePtrFromString(newpath);
    if (err != nil) {
        return err;
    }
    var ᴋ6 = oldp;
    var ᴋ7 = newp;
        var (_, _, errno) = syscall.Syscall6(renameatTrap, (uintptr)olddirfd, (uintptr)ᴋ6, (uintptr)newdirfd, (uintptr)ᴋ7, 0, 0);
    System.GC.KeepAlive(ᴋ6);
    System.GC.KeepAlive(ᴋ7);
    if (errno != 0) {
        return errno;
    }
    return default;
}

public static error Linkat(nint olddirfd, @string oldpath, nint newdirfd, @string newpath, nint flag) {
    var (oldp, err) = syscall.BytePtrFromString(oldpath);
    if (err != nil) {
        return err;
    }
    (var newp, err) = syscall.BytePtrFromString(newpath);
    if (err != nil) {
        return err;
    }
    var ᴋ8 = oldp;
    var ᴋ9 = newp;
        var (_, _, errno) = syscall.Syscall6(linkatTrap, (uintptr)olddirfd, (uintptr)ᴋ8, (uintptr)newdirfd, (uintptr)ᴋ9, (uintptr)flag, 0);
    System.GC.KeepAlive(ᴋ8);
    System.GC.KeepAlive(ᴋ9);
    if (errno != 0) {
        return errno;
    }
    return default;
}

public static error Symlinkat(@string oldpath, nint newdirfd, @string newpath) {
    var (oldp, err) = syscall.BytePtrFromString(oldpath);
    if (err != nil) {
        return err;
    }
    (var newp, err) = syscall.BytePtrFromString(newpath);
    if (err != nil) {
        return err;
    }
    var ᴋ10 = oldp;
    var ᴋ11 = newp;
        var (_, _, errno) = syscall.Syscall(symlinkatTrap, (uintptr)ᴋ10, (uintptr)newdirfd, (uintptr)ᴋ11);
    System.GC.KeepAlive(ᴋ10);
    System.GC.KeepAlive(ᴋ11);
    if (errno != 0) {
        return errno;
    }
    return default;
}

} // end unix_package
