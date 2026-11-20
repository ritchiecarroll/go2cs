// Copyright 2023 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

global using @long = long;
global using @ulong = ulong;

namespace go.modernc.org;

using unix = golang.org.x.sys.unix_package;
using golang.org.x.sys;
using syscall = syscall_package;

partial class libc_package {

/* [1125899906842623L]byte */
partial struct RawMem /*[1125899906842623]byte*/;

public static partial int32 Xrenameat2(ж<TLS> t, int32 olddirfd, uintptr oldpath, int32 newdirfd, uintptr newpath, int32 flags) {
    if (__ccgo_strace) {
        trc("t=%v olddirfd=%v oldpath=%v newdirfd=%v newpath=%v flags=%v, (%v:)", t.OrTypedNil(), olddirfd, oldpath, newdirfd, newpath, flags, origin(2));
    }
    var (_, _, err) = unix.Syscall6(unix.SYS_RENAMEAT2, (uintptr)olddirfd, oldpath, (uintptr)newdirfd, newpath, (uintptr)flags, 0); if (err != 0) {
        t.setErrno((int32)err);
        return -1;
    }

    return 0;
}

} // end libc_package
