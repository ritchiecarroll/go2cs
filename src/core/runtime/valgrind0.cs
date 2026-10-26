// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Valgrind instrumentation is only available on linux amd64 and arm64.
//go:build !valgrind || !linux || (!amd64 && !arm64)
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

internal const bool valgrindenabled = false;

internal static uintptr valgrindRegisterStack(@unsafe.Pointer start, @unsafe.Pointer end) {
    return 0;
}

internal static void valgrindDeregisterStack(uintptr id) {
}

internal static void valgrindChangeStack(uintptr id, @unsafe.Pointer start, @unsafe.Pointer end) {
}

internal static void valgrindMalloc(@unsafe.Pointer addr, uintptr size) {
}

internal static void valgrindFree(@unsafe.Pointer addr) {
}

internal static void valgrindCreateMempool(@unsafe.Pointer addr) {
}

internal static void valgrindMempoolMalloc(@unsafe.Pointer pool, @unsafe.Pointer addr, uintptr size) {
}

internal static void valgrindMempoolFree(@unsafe.Pointer pool, @unsafe.Pointer addr) {
}

internal static void valgrindMakeMemUndefined(@unsafe.Pointer addr, uintptr size) {
}

internal static void valgrindMakeMemDefined(@unsafe.Pointer addr, uintptr size) {
}

internal static void valgrindMakeMemNoAccess(@unsafe.Pointer addr, uintptr size) {
}

} // end runtime_package
