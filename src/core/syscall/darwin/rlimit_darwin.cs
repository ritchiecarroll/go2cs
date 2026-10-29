// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin
namespace go;

partial class syscall_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string kernMaxfilesperprocˢ = "kern.maxfilesperproc"u8;

// adjustFileLimit adds per-OS limitations on the Rlimit used for RLIMIT_NOFILE. See rlimit.go.
internal static void adjustFileLimit(ref Rlimit lim) {
    // On older macOS, setrlimit(RLIMIT_NOFILE, lim) with lim.Cur = infinity fails.
    // Set to the value of kern.maxfilesperproc instead.
    var (n, err) = SysctlUint32(kernMaxfilesperprocˢ);
    if (err != nil) {
        return;
    }
    if (lim.Cur > (uint64)n) {
        lim.Cur = (uint64)n;
    }
}

} // end syscall_package
