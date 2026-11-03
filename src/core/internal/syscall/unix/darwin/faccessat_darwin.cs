// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using abi = go.@internal.abi_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class unix_package {

internal static partial void libc_faccessat_trampoline();

//go:cgo_import_dynamic libc_faccessat faccessat "/usr/lib/libSystem.B.dylib"
internal static error faccessat(nint dirfd, @string path, uint32 mode, nint flags) {
    var (p, err) = syscall.BytePtrFromString(path);
    if (err != nil) {
        return err;
    }
    var (_, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_faccessat_trampoline), (uintptr)dirfd, (uintptr)p, (uintptr)mode, (uintptr)flags, 0, 0);
    if (errno != 0) {
        return errno;
    }
    return default;
}

} // end unix_package
