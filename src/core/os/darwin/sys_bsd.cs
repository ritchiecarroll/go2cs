// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || (js && wasm) || netbsd || openbsd || wasip1
namespace go;

using syscall = syscall_package;

partial class os_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string kernHostnameˢ = "kern.hostname"u8;
internal static readonly @string sysctlKernHostnameˢ = "sysctl kern.hostname"u8;

internal static (@string name, error err) hostname() {
    @string name = default;
    error err = default;

    (name, err) = syscall.Sysctl(kernHostnameˢ);
    if (err != nil) {
        return ("", NewSyscallError(sysctlKernHostnameˢ, err));
    }
    return (name, default);
}

} // end os_package
