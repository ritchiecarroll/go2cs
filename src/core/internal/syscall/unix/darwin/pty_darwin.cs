// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using abi = go.@internal.abi_package;
using @unsafe = unsafe_package;
using go.@internal;
using syscall = syscall_package;

partial class unix_package {

//go:cgo_import_dynamic libc_grantpt grantpt "/usr/lib/libSystem.B.dylib"
internal static partial void libc_grantpt_trampoline();

public static error Grantpt(nint fd) {
    var (_, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_grantpt_trampoline), (uintptr)fd, 0, 0, 0, 0, 0);
    if (errno != 0) {
        return errno;
    }
    return default!;
}

//go:cgo_import_dynamic libc_unlockpt unlockpt "/usr/lib/libSystem.B.dylib"
internal static partial void libc_unlockpt_trampoline();

public static error Unlockpt(nint fd) {
    var (_, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_unlockpt_trampoline), (uintptr)fd, 0, 0, 0, 0, 0);
    if (errno != 0) {
        return errno;
    }
    return default!;
}

//go:cgo_import_dynamic libc_ptsname_r ptsname_r "/usr/lib/libSystem.B.dylib"
internal static partial void libc_ptsname_r_trampoline();

public static (@string, error) Ptsname(nint fd) {
    var buf = new slice<byte>(256);
    var (_, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_ptsname_r_trampoline),
        (uintptr)fd,
        (uintptr)Ꮡ(buf, 0),
        (uintptr)(len(buf) - 1),
        0, 0, 0);
    if (errno != 0) {
        return ("", errno);
    }
    foreach (var (i, c) in buf) {
        if (c == 0) {
            buf = buf.slice(0, i);
            break;
        }
    }
    return (((@string)buf), default!);
}

//go:cgo_import_dynamic libc_posix_openpt posix_openpt "/usr/lib/libSystem.B.dylib"
internal static partial void libc_posix_openpt_trampoline();

public static (nint fd, error err) PosixOpenpt(nint flag) {
    var (ufd, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_posix_openpt_trampoline), (uintptr)flag, 0, 0, 0, 0, 0);
    if (errno != 0) {
        return (-1, errno);
    }
    return ((nint)ufd, default!);
}

} // end unix_package
