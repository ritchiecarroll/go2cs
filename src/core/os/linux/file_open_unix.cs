// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm)
namespace go;

using poll = @internal.poll_package;
using syscall = syscall_package;
using @internal;

partial class os_package {

internal static (nint, poll.SysFile, error) open(@string path, nint flag, uint32 perm) {
    var (fd, err) = syscall.Open(path, flag, perm);
    return (fd, new poll.SysFile(), err);
}

} // end os_package
