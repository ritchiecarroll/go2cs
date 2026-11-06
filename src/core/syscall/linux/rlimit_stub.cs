// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix && !darwin
namespace go;

partial class syscall_package {

// adjustFileLimit adds per-OS limitations on the Rlimit used for RLIMIT_NOFILE. See rlimit.go.
internal static void adjustFileLimit(ref Rlimit lim) { }

} // end syscall_package
