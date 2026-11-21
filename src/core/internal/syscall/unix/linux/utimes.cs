// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix && !wasip1
namespace go.@internal.syscall;

using syscall = syscall_package;
/* import _ "unsafe" (nothing to initialize) */ // for //go:linkname

partial class unix_package {

//go:linkname Utimensat syscall.utimensat
/*linkname*/ public static partial error Utimensat(nint dirfd, @string path, /*[2]*/ ж<array<syscall.Timespec>> times, nint flag) {
    return syscall.utimensat(dirfd, path, times, flag);
}

} // end unix_package
