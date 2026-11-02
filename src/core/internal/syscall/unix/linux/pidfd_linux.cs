// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;

partial class unix_package {

public static error PidFDSendSignal(uintptr pidfd, syscallꓸSignal s) {
    var (_, _, errno) = syscall.Syscall(pidfdSendSignalTrap, pidfd, (uintptr)s, 0);
    if (errno != 0) {
        return errno;
    }
    return default;
}

public static (uintptr, error) PidFDOpen(nint pid, nint flags) {
    var (pidfd, _, errno) = syscall.Syscall(pidfdOpenTrap, (uintptr)pid, (uintptr)flags, 0);
    if (errno != 0) {
        return (~(uintptr)0, errno);
    }
    return ((uintptr)pidfd, default);
}

} // end unix_package
