// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux || (darwin && !ios) || dragonfly || freebsd || solaris
namespace go;

partial class net_package {

// Always true except for workstation and client versions of Windows
internal static bool supportsSendfile() {
    return true;
}

} // end net_package
