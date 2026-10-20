// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using @internal.syscall;

partial class net_package {

// Workstation and client versions of Windows limit the number
// of concurrent TransmitFile operations allowed on the system
// to a maximum of two. Please see:
// https://learn.microsoft.com/en-us/windows/win32/api/mswsock/nf-mswsock-transmitfile
// https://golang.org/issue/73746
internal static bool supportsSendfile() {
    if (testHookSupportsSendfile != default!) {
        return testHookSupportsSendfile();
    }
    return windows.SupportUnlimitedTransmitFile();
}

} // end net_package
