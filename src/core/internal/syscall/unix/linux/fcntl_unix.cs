// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go.@internal.syscall;

using syscall = syscall_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname

partial class unix_package {

// Implemented in the runtime package.
//
//go:linkname fcntl runtime.fcntl
[global::System.Diagnostics.StackTraceHidden] internal static (int32, int32) fcntl(int32 fd, int32 cmd, int32 arg) {
    var (ᴛ1, ᴛ2) = go.runtime_package.fcntl(fd, cmd, arg);
    return (ᴛ1, ᴛ2);
}

public static (nint, error) Fcntl(nint fd, nint cmd, nint arg) {
    var (val, errno) = fcntl((int32)fd, (int32)cmd, (int32)arg);
    if (val == -1) {
        return ((nint)val, ((syscall.Errno)(uintptr)errno));
    }
    return ((nint)val, default!);
}

} // end unix_package
