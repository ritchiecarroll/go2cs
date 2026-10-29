// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build freebsd || linux
namespace go.@internal.syscall;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class unix_package {

public static (nint n, error err) CopyFileRange(nint rfd, ж<int64> roff, nint wfd, ж<int64> woff, nint len, nint flags) {
    nint n = default;
    error err = default;

    var ᴋ0 = roff;
    var ᴋ1 = woff;
        var (r1, _, errno) = syscall.Syscall6(copyFileRangeTrap, (uintptr)rfd, (uintptr)ᴋ0, (uintptr)wfd, (uintptr)ᴋ1, (uintptr)len, (uintptr)flags);
    System.GC.KeepAlive(ᴋ0);
    System.GC.KeepAlive(ᴋ1);
    n = (nint)r1;
    if (errno != 0) {
        err = errno;
    }
    return (n, err);
}

} // end unix_package
