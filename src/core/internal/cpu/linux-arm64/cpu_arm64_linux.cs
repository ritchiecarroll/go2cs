// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build arm64 && linux && !android
namespace go.@internal;

partial class cpu_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string linuxˢ => ref linuxˢᵇ;
private static readonly @string linuxˢᵇ = "linux"u8;

internal static void osInit() {
    hwcapInit(linuxˢ);
}

} // end cpu_package
