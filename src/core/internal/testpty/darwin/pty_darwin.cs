// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using unix = go.@internal.syscall.unix_package;
using os = os_package;
using Δsyscall = syscall_package;
using go.@internal.syscall;

partial class testpty_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string posixOpenptˢ => ref posixOpenptˢᵇ;
private static readonly @string posixOpenptˢᵇ = "posix_openpt"u8;
private static ref readonly @string grantptˢ => ref grantptˢᵇ;
private static readonly @string grantptˢᵇ = "grantpt"u8;
private static ref readonly @string unlockptˢ => ref unlockptˢᵇ;
private static readonly @string unlockptˢᵇ = "unlockpt"u8;
private static ref readonly @string ptsnameˢ => ref ptsnameˢᵇ;
private static readonly @string ptsnameˢᵇ = "ptsname"u8;
private static ref readonly @string ptyˢ => ref ptyˢᵇ;
private static readonly @string ptyˢᵇ = "pty"u8;

internal static (ж<os.File> pty, @string processTTY, error err) open() {
    @string processTTY = default;
    error err = default;

    (var m, err) = unix.PosixOpenpt(Δsyscall.O_RDWR);
    if (err != nil) {
        return (default, "", new PtyErrorжerror(ptyError(posixOpenptˢ, err)));
    }
    var errΔ1 = unix.Grantpt(m); if (errΔ1 != nil) {
        Δsyscall.Close(m);
        return (default, "", new PtyErrorжerror(ptyError(grantptˢ, errΔ1)));
    }
    var errΔ2 = unix.Unlockpt(m); if (errΔ2 != nil) {
        Δsyscall.Close(m);
        return (default, "", new PtyErrorжerror(ptyError(unlockptˢ, errΔ2)));
    }
    (processTTY, err) = unix.Ptsname(m);
    if (err != nil) {
        Δsyscall.Close(m);
        return (default, "", new PtyErrorжerror(ptyError(ptsnameˢ, err)));
    }
    return (os.NewFile((uintptr)m, ptyˢ), processTTY, default);
}

} // end testpty_package
