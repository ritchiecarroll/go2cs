// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || dragonfly || freebsd || linux || openbsd || solaris || wasip1
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;

partial class os_package {

// isNoFollowErr reports whether err may result from O_NOFOLLOW blocking an open operation.
internal static bool isNoFollowErr(error err) {
    var exprᴛ1 = err;
    if (AreEqual(exprᴛ1, syscall.ELOOP) || AreEqual(exprᴛ1, syscall.EMLINK)) {
        return true;
    }
    if (runtime.GOOS == "dragonfly"u8) {
        // Dragonfly appears to return EINVAL from openat in this case.
        if (AreEqual(err, syscall.EINVAL)) {
            return true;
        }
    }
    return false;
}

} // end os_package
