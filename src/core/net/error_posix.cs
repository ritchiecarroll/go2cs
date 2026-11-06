// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || js || wasip1 || windows
namespace go;

using os = os_package;
using syscall = syscall_package;

partial class net_package {

// wrapSyscallError takes an error and a syscall name. If the error is
// a syscall.Errno, it wraps it in an os.SyscallError using the syscall name.
internal static error wrapSyscallError(@string name, error err) {
    var (_, ok) = err._<syscall.Errno>(ᐧ); if (ok) {
        err = os.NewSyscallError(name, err);
    }
    return err;
}

} // end net_package
