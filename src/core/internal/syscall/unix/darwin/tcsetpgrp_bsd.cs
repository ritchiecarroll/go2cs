// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go.@internal.syscall;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class unix_package {

//go:linkname ioctlPtr syscall.ioctlPtr
/*linkname*/ internal static partial error /*err*/ ioctlPtr(nint fd, nuint req, @unsafe.Pointer arg) {
    return syscall.ioctlPtr(fd, req, arg);
}

// Note that pgid should really be pid_t, however _C_int (aka int32) is
// generally equivalent.
public static error /*err*/ Tcsetpgrp(nint fd, int32 pgidʗp) {
    ref var pgid = ref heap(pgidʗp, out var Ꮡpgid);

    return ioctlPtr(fd, syscall.TIOCSPGRP, @unsafe.Pointer.FromPinnedBox(Ꮡpgid));
}

} // end unix_package
