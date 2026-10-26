// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux && !windows
namespace go;

partial class os_package {

internal static void closeHandle(this ref processHandle ph) {
    throw panic("internal error: unexpected call to closeHandle");
}

} // end os_package
