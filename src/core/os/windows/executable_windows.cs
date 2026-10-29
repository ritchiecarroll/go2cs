// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using syscall = syscall_package;
using @internal.syscall;

partial class os_package {

internal static (@string, error) getModuleFileName(syscallꓸHandle handle) {
    var n = (uint32)1024;
    slice<uint16> buf = default;
    for (;;) {
        buf = new slice<uint16>((nint)(n));
        var (r, err) = windows.GetModuleFileName(handle, Ꮡ(buf, 0), n);
        if (err != nil) {
            return ("", err);
        }
        if (r < n) {
            break;
        }
        // r == n means n not big enough
        n += 1024;
    }
    return (syscall.UTF16ToString(buf), default);
}

internal static (@string, error) executable() {
    return getModuleFileName(0);
}

} // end os_package
