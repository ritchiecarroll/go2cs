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
    if (err != default!) {
        return err;
    }

    var ᴋ0 = p;
        var (_, _, errno) = syscall.Syscall(unlinkatTrap, (uintptr)dirfd, (uintptr)ᴋ0, (uintptr)flags);
    System.GC.KeepAlive(ᴋ0);
    if (errno != 0) {
        return errno;
    }

    return default!;
}

public static (nint, error) Openat(nint dirfd, @string path, nint flags, uint32 perm) {
    var (p, err) = syscall.BytePtrFromString(path);
    if (err != default!) {
        return (0, err);
    }

    var ᴋ1 = p;
        var (fd, _, errno) = syscall.Syscall6(openatTrap, (uintptr)dirfd, (uintptr)ᴋ1, (uintptr)flags, (uintptr)perm, 0, 0);
    System.GC.KeepAlive(ᴋ1);
    if (errno != 0) {
        return (0, errno);
    }

    return ((nint)fd, default!);
}

public static (nint, error) Readlinkat(nint dirfd, @string path, slice<byte> buf) {
    var (p0, err) = syscall.BytePtrFromString(path);
    if (err != default!) {
        return (0, err);
    }
    @unsafe.Pointer p1 = default!;
    if (len(buf) > 0){
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

    return ((nint)n, default!);
}

public static error Mkdirat(nint dirfd, @string path, uint32 mode) {
    var (p, err) = syscall.BytePtrFromString(path);
    if (err != default!) {
        return err;
    }

    var ᴋ4 = p;
        var (_, _, errno) = syscall.Syscall6(mkdiratTrap, (uintptr)dirfd, (uintptr)ᴋ4, (uintptr)mode, 0, 0, 0);
    System.GC.KeepAlive(ᴋ4);
    if (errno != 0) {
        return errno;
    }
    return default!;
}

} // end unix_package
