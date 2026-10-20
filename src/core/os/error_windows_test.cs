// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
namespace go;

using fs = go.io.fs_package;
using Δos = os_package;
using syscall = syscall_package;
using go.io;
using static go.os_internal_test_package;

partial class os_test_package {

[GoInit] internal static void init() {
    syscall.Errno _ERROR_BAD_NETPATH = /* syscall.Errno(53) */ 53;

    isExistTests = append(isExistTests,
        new isExistTest(err: new fs.PathErrorжerror(Ꮡ(new fs.PathError(Err: syscall.ERROR_FILE_NOT_FOUND))), @is: false, isnot: true),
        new isExistTest(err: new Δos.LinkErrorжerror(Ꮡ(new Δos.LinkError(Err: syscall.ERROR_FILE_NOT_FOUND))), @is: false, isnot: true),
        new isExistTest(err: new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Err: syscall.ERROR_FILE_NOT_FOUND))), @is: false, isnot: true),
        new isExistTest(err: new fs.PathErrorжerror(Ꮡ(new fs.PathError(Err: _ERROR_BAD_NETPATH))), @is: false, isnot: true),
        new isExistTest(err: new Δos.LinkErrorжerror(Ꮡ(new Δos.LinkError(Err: _ERROR_BAD_NETPATH))), @is: false, isnot: true),
        new isExistTest(err: new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Err: _ERROR_BAD_NETPATH))), @is: false, isnot: true),
        new isExistTest(err: new fs.PathErrorжerror(Ꮡ(new fs.PathError(Err: syscall.ERROR_PATH_NOT_FOUND))), @is: false, isnot: true),
        new isExistTest(err: new Δos.LinkErrorжerror(Ꮡ(new Δos.LinkError(Err: syscall.ERROR_PATH_NOT_FOUND))), @is: false, isnot: true),
        new isExistTest(err: new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Err: syscall.ERROR_PATH_NOT_FOUND))), @is: false, isnot: true),
        new isExistTest(err: new fs.PathErrorжerror(Ꮡ(new fs.PathError(Err: syscall.ERROR_DIR_NOT_EMPTY))), @is: true, isnot: false),
        new isExistTest(err: new Δos.LinkErrorжerror(Ꮡ(new Δos.LinkError(Err: syscall.ERROR_DIR_NOT_EMPTY))), @is: true, isnot: false),
        new isExistTest(err: new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Err: syscall.ERROR_DIR_NOT_EMPTY))), @is: true, isnot: false));
    isPermissionTests = append(isPermissionTests,
        new isPermissionTest(err: new fs.PathErrorжerror(Ꮡ(new fs.PathError(Err: syscall.ERROR_ACCESS_DENIED))), want: true),
        new isPermissionTest(err: new Δos.LinkErrorжerror(Ꮡ(new Δos.LinkError(Err: syscall.ERROR_ACCESS_DENIED))), want: true),
        new isPermissionTest(err: new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Err: syscall.ERROR_ACCESS_DENIED))), want: true));
}

} // end os_test_package
