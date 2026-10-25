// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using @internal.syscall;
using static go.net_package;

partial class net_internal_test_package {

internal static bool supportsUnixSocket() {
    return windows.SupportUnixSocket();
}

} // end net_internal_test_package
