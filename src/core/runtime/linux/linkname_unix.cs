// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go;

// import _ "unsafe" (nothing to initialize)

partial class runtime_package {

// used in internal/syscall/unix
//go:linkname fcntl

} // end runtime_package
