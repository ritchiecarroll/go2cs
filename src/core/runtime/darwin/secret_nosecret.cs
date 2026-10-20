// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !(amd64 || arm64) || !linux
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

// Stubs for platforms that do not implement runtime/secret

//go:linkname secret_count runtime/secret.count
internal static int32 secret_count() {
    return 0;
}

//go:linkname secret_inc runtime/secret.inc
internal static void secret_inc() {
}

//go:linkname secret_dec runtime/secret.dec
internal static void secret_dec() {
}

//go:linkname secret_eraseSecrets runtime/secret.eraseSecrets
internal static void secret_eraseSecrets() {
}

internal static void addSecret(@unsafe.Pointer Δp, uintptr size) {
}

//go:linkname secret_getStack runtime/secret.getStack
internal static (uintptr, uintptr) secret_getStack() {
    return (0, 0);
}

internal static void noopSignal(ref m mp) {
}

} // end runtime_package
