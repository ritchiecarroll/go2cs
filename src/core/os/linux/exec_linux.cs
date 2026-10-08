// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using syscall = syscall_package;

partial class os_package {

internal static void closeHandle(this ref Process p) {
    syscall.Close((nint)p.handle);
}

} // end os_package
