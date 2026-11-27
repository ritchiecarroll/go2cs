// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !386 && !amd64 && !ppc64 && !ppc64le
namespace go.@internal;

partial class cpu_package {

// Name returns the CPU name given by the vendor
// if it can be read directly from memory or by CPU instructions.
// If the CPU name can not be determined an empty string is returned.
//
// Implementations that use the Operating System (e.g. sysctl or /sys/)
// to gather CPU information for display should be placed in internal/sysinfo.
public static @string Name() {
    // "A CPU has no name".
    return ""u8;
}

} // end cpu_package
