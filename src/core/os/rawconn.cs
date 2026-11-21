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
internal static ref readonly @string syscallConnControlˢᴍrawConn => ref syscallConnControlˢᵇᴍrawConn;
private static readonly @string syscallConnControlˢᵇᴍrawConn = "SyscallConn.Control"u8;

[GoMethodBody("Control")] internal static error Controlᴍ(ref rawConn c, Action<uintptr> f) {
    var errΔ1 = c.@file.checkValid(syscallConnControlˢᴍrawConn); if (errΔ1 != nil) {
        return errΔ1;
    }
    var err = c.@file.of(File.Ꮡpfd).RawControl(f);
    runtime.KeepAlive(c.@file.OrTypedNil());
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string syscallConnReadˢᴍrawConn => ref syscallConnReadˢᵇᴍrawConn;
private static readonly @string syscallConnReadˢᵇᴍrawConn = "SyscallConn.Read"u8;

[GoMethodBody("Read")] internal static error Readᴍ(ref rawConn c, Func<uintptr, bool> f) {
    var errΔ1 = c.@file.checkValid(syscallConnReadˢᴍrawConn); if (errΔ1 != nil) {
        return errΔ1;
    }
    var err = c.@file.of(File.Ꮡpfd).RawRead(f);
    runtime.KeepAlive(c.@file.OrTypedNil());
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string syscallConnWriteˢᴍrawConn => ref syscallConnWriteˢᵇᴍrawConn;
private static readonly @string syscallConnWriteˢᵇᴍrawConn = "SyscallConn.Write"u8;

[GoMethodBody("Write")] internal static error Writeᴍ(ref rawConn c, Func<uintptr, bool> f) {
    var errΔ1 = c.@file.checkValid(syscallConnWriteˢᴍrawConn); if (errΔ1 != nil) {
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
