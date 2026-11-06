// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using syscall = syscall_package;

partial class net_package {

internal static bool isConnError(error err) {
    var (se, ok) = err._<syscall.Errno>(ᐧ); if (ok) {
        return se == syscall.WSAECONNRESET || se == syscall.WSAECONNABORTED;
    }
    return false;
}

} // end net_package
