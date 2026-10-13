// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go;

partial class runtime_package {

// These values are the same on all known Unix systems.
// If we find a discrepancy some day, we can split them out.
internal static UntypedInt _F_SETFD => 2;
internal static UntypedInt _FD_CLOEXEC => 1;

//go:nosplit
internal static void closeonexec(int32 fd) {
    fcntl(fd, _F_SETFD, _FD_CLOEXEC);
}

} // end runtime_package
