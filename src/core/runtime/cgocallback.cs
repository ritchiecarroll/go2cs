// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

// These functions are called from C code via cgo/callbacks.go.
// Panic.
internal static void _cgo_panic_internal(ж<byte> Δp) {
    throw panic(gostringnocopy(Δp));
}

} // end runtime_package
