// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build dragonfly || (linux && !(loong64 || mips64 || mips64le)) || netbsd || (openbsd && mips64)
namespace go.@internal.syscall;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class unix_package {

public static error Fstatat(nint dirfd, @string path, ж<syscall.Stat_t> Ꮡstat, nint flags) {
    ж<byte> p = default!;
    (p, var err) = syscall.BytePtrFromString(path);
    if (err != default!) {
        return err;
    }

    var ᴋ0 = p;
    var ᴋ1 = Ꮡstat;
        var (_, _, errno) = syscall.Syscall6(fstatatTrap, (uintptr)dirfd, (uintptr)ᴋ0, (uintptr)ᴋ1, (uintptr)flags, 0, 0);
    System.GC.KeepAlive(ᴋ0);
    System.GC.KeepAlive(ᴋ1);
    if (errno != 0) {
        return errno;
    }

    return default!;
}

} // end unix_package
