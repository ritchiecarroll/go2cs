// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux
namespace go;

using errors = errors_package;
using stringslite = @internal.stringslite_package;
using runtime = runtime_package;
using @internal;

partial class os_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string procSelfExeˢ => ref procSelfExeˢᵇ;
private static readonly @string procSelfExeˢᵇ = "/proc/self/exe"u8;
internal static ref readonly @string deletedˢ => ref deletedˢᵇ;
private static readonly @string deletedˢᵇ = " (deleted)"u8;

internal static (@string, error) executable() {
    @string procfn = default;
    var exprᴛ1 = runtime.GOOS;
    switch (ᐧ) {
    default: {
        return ("", errors.New("Executable not implemented for "u8 + runtime.GOOS));
    }
    case {} when exprᴛ1 == "linux"u8 || exprᴛ1 == "android"u8: {
        procfn = procSelfExeˢ;
        break;
    }}
    var (path, err) = Readlink(procfn);

    // When the executable has been deleted then Readlink returns a
    // path appended with " (deleted)".
    return (stringslite.TrimSuffix(path, deletedˢ), err);
}

} // end os_package
