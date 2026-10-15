// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

// setVMAName isn’t implemented
internal static void setVMAName(@unsafe.Pointer start, uintptr len, @string name) {
}

internal static bool setVMANameSupported() {
    return false;
}

} // end runtime_package
