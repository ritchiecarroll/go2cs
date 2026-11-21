// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !plan9
namespace go;

using runtime = runtime_package;
using @internal;
using static os_package;

partial class os_package {

// rawConn implements syscall.RawConn.
partial struct rawConn {
    internal ж<File> @file;
}

} partial class os_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string syscallConnControlˢ⁔rawConn => ref syscallConnControlˢᵇ⁔rawConn;
private static readonly @string syscallConnControlˢᵇ⁔rawConn = "SyscallConn.Control"u8;

internal static error Control⁔(/*this*/ ref rawConn c, Action<uintptr> f) {
    var errΔ1 = c.@file.checkValid(syscallConnControlˢ⁔rawConn); if (errΔ1 != nil) {
        return errΔ1;
    }
    var err = c.@file.of(File.Ꮡpfd).RawControl(f);
    runtime.KeepAlive(c.@file.OrTypedNil());
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string syscallConnReadˢ⁔rawConn => ref syscallConnReadˢᵇ⁔rawConn;
private static readonly @string syscallConnReadˢᵇ⁔rawConn = "SyscallConn.Read"u8;

internal static error Read⁔(/*this*/ ref rawConn c, Func<uintptr, bool> f) {
    var errΔ1 = c.@file.checkValid(syscallConnReadˢ⁔rawConn); if (errΔ1 != nil) {
        return errΔ1;
    }
    var err = c.@file.of(File.Ꮡpfd).RawRead(f);
    runtime.KeepAlive(c.@file.OrTypedNil());
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string syscallConnWriteˢ⁔rawConn => ref syscallConnWriteˢᵇ⁔rawConn;
private static readonly @string syscallConnWriteˢᵇ⁔rawConn = "SyscallConn.Write"u8;

internal static error Write⁔(/*this*/ ref rawConn c, Func<uintptr, bool> f) {
    var errΔ1 = c.@file.checkValid(syscallConnWriteˢ⁔rawConn); if (errΔ1 != nil) {
        return errΔ1;
    }
    var err = c.@file.of(File.Ꮡpfd).RawWrite(f);
    runtime.KeepAlive(c.@file.OrTypedNil());
    return err;
}

} partial class os_package {

internal static (ж<rawConn>, error) newRawConn(ж<File> @file) {
    return (Ꮡ(new rawConn(@file: @file)), default);
}

} // end os_package
