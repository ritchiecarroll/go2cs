// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class unix_package {

public static UntypedInt P_PID => 1;
public static UntypedInt P_PIDFD => 3;

public static error Waitid(nint idType, nint id, ж<SiginfoChild> info, nint options, ж<syscall.Rusage> rusage) {
    var ᴋ0 = info;
    var ᴋ1 = rusage;
        var (_, _, errno) = syscall.Syscall6(syscall.SYS_WAITID, (uintptr)idType, (uintptr)id, (uintptr)ᴋ0, (uintptr)options, (uintptr)ᴋ1, 0);
    System.GC.KeepAlive(ᴋ0);
    System.GC.KeepAlive(ᴋ1);
    if (errno != 0) {
        return errno;
    }
    return default;
}

} // end unix_package
