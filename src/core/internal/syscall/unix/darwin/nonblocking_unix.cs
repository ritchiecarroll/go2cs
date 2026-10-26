// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go.@internal.syscall;

using syscall = syscall_package;

partial class unix_package {

public static (bool nonblocking, error err) IsNonblock(nint fd) {
    var (flag, e1) = Fcntl(fd, syscall.F_GETFL, 0);
    if (e1 != default!) {
        return (false, e1);
    }
    return ((flag & (nint)syscall.O_NONBLOCK) != 0, default!);
}

public static bool HasNonblockFlag(nint flag) {
    return (flag & (nint)syscall.O_NONBLOCK) != 0;
}

} // end unix_package
