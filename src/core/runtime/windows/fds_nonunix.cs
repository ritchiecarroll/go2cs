// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !unix
namespace go;

partial class runtime_package {

internal static void checkfds() {
    // Nothing to do on non-Unix platforms.
}

} // end runtime_package
