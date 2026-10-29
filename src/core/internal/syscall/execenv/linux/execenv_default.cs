// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows
namespace go.@internal.syscall;

using syscall = syscall_package;

partial class execenv_package {

// Default will return the default environment
// variables based on the process attributes
// provided.
//
// Defaults to syscall.Environ() on all platforms
// other than Windows.
public static (slice<@string>, error) Default(ж<syscall.SysProcAttr> sys) {
    return (syscall.Environ(), default);
}

} // end execenv_package
