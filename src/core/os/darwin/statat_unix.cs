// Copyright 2026 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || dragonfly || freebsd || wasip1 || linux || netbsd || openbsd || solaris
namespace go;

using unix = @internal.syscall.unix_package;
using @internal;
using @internal.syscall;
using syscall = syscall_package;
using static os_package;

partial class os_package {

} partial class os_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fstatatˢᴍFile => ref fstatatˢᵇᴍFile;
private static readonly @string fstatatˢᵇᴍFile = "fstatat"u8;

[GoMethodBody("lstatatNolog")] internal static (FileInfo, error) lstatatNologᴍ(ж<File> Ꮡf, @string name) {
    ref var f = ref Ꮡf.DerefOrNull();

    ref var fs = ref heap(new fileStat(), out var Ꮡfs);
    var err = Ꮡf.of(File.Ꮡpfd).Fstatat(name, Ꮡfs.Ꮡsys, unix.AT_SYMLINK_NOFOLLOW); if (err != nil) {
        return (default, f.wrapErr(fstatatˢᴍFile, err));
    }
    fillFileStatFromSys(Ꮡfs, name);
    return (new fileStatжFileInfo(Ꮡfs), default);
}

}
