// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using syscall = syscall_package;
using static go.net_package;

partial class net_internal_test_package {

internal static syscall.Errno errOpNotSupported = syscall.EOPNOTSUPP;

internal static slice<error> abortedConnRequestErrors = new error[]{syscall.ERROR_NETNAME_DELETED, syscall.WSAECONNRESET}.slice(); // see accept in fd_windows.go

internal static bool isPlatformError(error err) {
    var (_, ok) = err._<syscall.Errno>(ᐧ);
    return ok;
}

internal static bool isENOBUFS(error err) {
    // syscall.ENOBUFS is a completely made-up value on Windows: we don't expect
    // a real system call to ever actually return it. However, since it is already
    // defined in the syscall package we may as well check for it.
    return errors.Is(err, syscall.ENOBUFS);
}

} // end net_internal_test_package
