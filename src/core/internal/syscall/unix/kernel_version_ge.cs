// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

partial class unix_package {

// KernelVersionGE checks if the running kernel version
// is greater than or equal to the provided version.
public static bool KernelVersionGE(nint x, nint y) {
    var (xx, yy) = KernelVersion();

    return xx > x || (xx == x && yy >= y);
}

} // end unix_package
