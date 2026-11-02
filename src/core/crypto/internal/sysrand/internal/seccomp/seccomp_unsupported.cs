// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux || !cgo
namespace go.crypto.@internal.sysrand.@internal;

using errors = errors_package;

partial class seccomp_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string disablingGetrandomIsNotˢ => ref disablingGetrandomIsNotˢᵇ;
private static readonly @string disablingGetrandomIsNotˢᵇ = "disabling getrandom is not supported on this system"u8;

public static error DisableGetrandom() {
    return errors.New(disablingGetrandomIsNotˢ);
}

} // end seccomp_package
