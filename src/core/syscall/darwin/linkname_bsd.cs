// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go;

// import _ "unsafe" (nothing to initialize)

partial class syscall_package {

// used by internal/syscall/unix
//go:linkname ioctlPtr
// golang.org/x/net linknames sysctl.
// Do not remove or change the type signature.
//
//go:linkname sysctl

} // end syscall_package
