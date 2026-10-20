// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class runtime_package {

internal static void initSecureMode() {
    // We have already initialized the secureMode bool in sysauxv.
}

internal static bool isSecureMode() {
    return secureMode;
}

} // end runtime_package
