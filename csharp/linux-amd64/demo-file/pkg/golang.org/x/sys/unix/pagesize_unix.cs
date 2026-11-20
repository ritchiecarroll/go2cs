// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using syscall = syscall_package;

partial class unix_package {

public static nint Getpagesize() {
    return syscall.Getpagesize();
}

} // end unix_package
