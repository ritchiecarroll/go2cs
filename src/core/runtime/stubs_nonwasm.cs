// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !wasm
namespace go;

partial class runtime_package {

// pause is only used on wasm.
internal static void pause(uintptr newsp) { throw panic("unreachable"); }

} // end runtime_package
