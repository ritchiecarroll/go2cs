// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using @unsafe = unsafe_package;
using syscall = syscall_package;

partial class unix_package {

internal static uintptr fcntl64Syscall = SYS_FCNTL;

internal static (nint, error) fcntl(nint fd, nint cmd, nint arg) {
    var (valptr, _, errno) = Syscall(fcntl64Syscall, (uintptr)fd, (uintptr)cmd, (uintptr)arg);
    error err = default;
    if (errno != 0) {
        err = errno;
    }
    return ((nint)valptr, err);
}

public static (nint, error) FcntlInt(uintptr fd, nint cmd, nint arg) {
    return fcntl((nint)fd, cmd, arg);
}

public static error FcntlFlock(uintptr fd, nint cmd, ж<Flock_t> lk) {
    var (_, _, errno) = Syscall(fcntl64Syscall, fd, (uintptr)cmd, (uintptr)lk);
    if (errno == 0) {
        return default;
    }
    return errno;
}

} // end unix_package
