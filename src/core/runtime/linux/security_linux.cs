// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// import _ "unsafe" (nothing to initialize)

partial class runtime_package {

internal static void initSecureMode() {
    // We have already initialized the secureMode bool in sysauxv.
}

internal static bool isSecureMode() {
    return secureMode;
}

} // end runtime_package
