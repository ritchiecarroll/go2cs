// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !unix || wasm
namespace go.crypto;

using static go.crypto.tls_package;

partial class tls_internal_test_package {

internal static void pauseProcess() {
    throw panic("-wait-for-debugger not supported on this OS");
}

} // end tls_internal_test_package
