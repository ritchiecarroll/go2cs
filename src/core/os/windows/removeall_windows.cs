// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
namespace go;

using syscall = syscall_package;

partial class os_package {

internal static (ж<File>, error) newDirFile(syscallꓸHandle fd, @string name) {
    return (newFile(fd, name, kindOpenFile, false), default!);
}

} // end os_package
