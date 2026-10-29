// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1
namespace go;

using syscall = syscall_package;
using @internal;
using fs = go.io.fs_package;

partial class os_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string statˢ = "stat"u8;

// Stat returns the [FileInfo] structure describing file.
// If there is an error, it will be of type [*PathError].
public static (FileInfo, error) Stat(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (Ꮡf == nil) {
        return (default, ErrInvalid);
    }
    ref var fs = ref heap(new fileStat(), out var Ꮡfs);
    var err = Ꮡf.of(File.Ꮡpfd).Fstat(Ꮡfs.of(fileStat.Ꮡsys));
    if (err != nil) {
        return (default, f.wrapErr(statˢ, err));
    }
    fillFileStatFromSys(Ꮡfs, f.name);
    return (new fileStatжFileInfo(Ꮡfs), default);
}

// statNolog stats a file with no test logging.
internal static (FileInfo, error) statNolog(@string name) {
    ref var fs = ref heap(new fileStat(), out var Ꮡfs);
    var err = ignoringEINTR(() => syscall.Stat(name, Ꮡfs.of(fileStat.Ꮡsys)));
    if (err != nil) {
        return (default, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "stat"u8, Path: name, Err: err))));
    }
    fillFileStatFromSys(Ꮡfs, name);
    return (new fileStatжFileInfo(Ꮡfs), default);
}

// lstatNolog lstats a file with no test logging.
internal static (FileInfo, error) lstatNolog(@string name) {
    ref var fs = ref heap(new fileStat(), out var Ꮡfs);
    var err = ignoringEINTR(() => syscall.Lstat(name, Ꮡfs.of(fileStat.Ꮡsys)));
    if (err != nil) {
        return (default, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "lstat"u8, Path: name, Err: err))));
    }
    fillFileStatFromSys(Ꮡfs, name);
    return (new fileStatжFileInfo(Ꮡfs), default);
}

} // end os_package
