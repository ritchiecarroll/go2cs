// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || (openbsd && !mips64) || solaris
namespace go;

// import _ "unsafe" (nothing to initialize)

partial class syscall_package {

// used by internal/poll
//go:linkname writev

} // end syscall_package
