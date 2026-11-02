// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin
namespace go.@internal.syscall;

using abi = go.@internal.abi_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class unix_package {

internal static partial void libc_readlinkat_trampoline();

//go:cgo_import_dynamic libc_readlinkat readlinkat "/usr/lib/libSystem.B.dylib"
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
    var (n, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_readlinkat_trampoline),
        (uintptr)dirfd,
        (uintptr)p0,
        (uintptr)p1,
        (uintptr)len(buf),
        0,
        0);
    if (errno != 0) {
        return (0, errno);
    }
    return ((nint)n, default);
}

internal static partial void libc_mkdirat_trampoline();

//go:cgo_import_dynamic libc_mkdirat mkdirat "/usr/lib/libSystem.B.dylib"
public static error Mkdirat(nint dirfd, @string path, uint32 mode) {
    var (p, err) = syscall.BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, errno) = syscall_syscall(abi.FuncPCABI0(libc_mkdirat_trampoline),
        (uintptr)dirfd,
        (uintptr)p,
        (uintptr)mode);
    if (errno != 0) {
        return errno;
    }
    return default;
}

internal static partial void libc_fchmodat_trampoline();

//go:cgo_import_dynamic libc_fchmodat fchmodat "/usr/lib/libSystem.B.dylib"
public static error Fchmodat(nint dirfd, @string path, uint32 mode, nint flags) {
    var (p, err) = syscall.BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_fchmodat_trampoline),
        (uintptr)dirfd,
        (uintptr)p,
        (uintptr)mode,
        (uintptr)flags,
        0,
        0);
    if (errno != 0) {
        return errno;
    }
    return default;
}

internal static partial void libc_fchownat_trampoline();

//go:cgo_import_dynamic libc_fchownat fchownat "/usr/lib/libSystem.B.dylib"
public static error Fchownat(nint dirfd, @string path, nint uid, nint gid, nint flags) {
    var (p, err) = syscall.BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_fchownat_trampoline),
        (uintptr)dirfd,
        (uintptr)p,
        (uintptr)uid,
        (uintptr)gid,
        (uintptr)flags,
        0);
    if (errno != 0) {
        return errno;
    }
    return default;
}

internal static partial void libc_renameat_trampoline();

//go:cgo_import_dynamic libc_renameat renameat "/usr/lib/libSystem.B.dylib"
public static error Renameat(nint olddirfd, @string oldpath, nint newdirfd, @string newpath) {
    var (oldp, err) = syscall.BytePtrFromString(oldpath);
    if (err != nil) {
        return err;
    }
    (var newp, err) = syscall.BytePtrFromString(newpath);
    if (err != nil) {
        return err;
    }
    var (_, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_renameat_trampoline),
        (uintptr)olddirfd,
        (uintptr)oldp,
        (uintptr)newdirfd,
        (uintptr)newp,
        0,
        0);
    if (errno != 0) {
        return errno;
    }
    return default;
}

internal static partial void libc_linkat_trampoline();

//go:cgo_import_dynamic libc_linkat linkat "/usr/lib/libSystem.B.dylib"
public static error Linkat(nint olddirfd, @string oldpath, nint newdirfd, @string newpath, nint flag) {
    var (oldp, err) = syscall.BytePtrFromString(oldpath);
    if (err != nil) {
        return err;
    }
    (var newp, err) = syscall.BytePtrFromString(newpath);
    if (err != nil) {
        return err;
    }
    var (_, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_linkat_trampoline),
        (uintptr)olddirfd,
        (uintptr)oldp,
        (uintptr)newdirfd,
        (uintptr)newp,
        (uintptr)flag,
        0);
    if (errno != 0) {
        return errno;
    }
    return default;
}

internal static partial void libc_symlinkat_trampoline();

//go:cgo_import_dynamic libc_symlinkat symlinkat "/usr/lib/libSystem.B.dylib"
public static error Symlinkat(@string oldpath, nint newdirfd, @string newpath) {
    var (oldp, err) = syscall.BytePtrFromString(oldpath);
    if (err != nil) {
        return err;
    }
    (var newp, err) = syscall.BytePtrFromString(newpath);
    if (err != nil) {
        return err;
    }
    var (_, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_symlinkat_trampoline),
        (uintptr)oldp,
        (uintptr)newdirfd,
        (uintptr)newp,
        0,
        0,
        0);
    if (errno != 0) {
        return errno;
    }
    return default;
}

} // end unix_package
