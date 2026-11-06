// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !unix
namespace go;

partial class runtime_package {

internal static bool isSecureMode() {
    return false;
}

internal static void secure() { }

} // end runtime_package
