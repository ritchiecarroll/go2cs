// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux
namespace go;

using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

internal static @unsafe.Pointer _cgo_clearenv; // pointer to C function

// Clear the C environment if cgo is loaded.
internal static void clearenv_c() {
    if (_cgo_clearenv == nil) {
        return;
    }
    asmcgocall(_cgo_clearenv, nil);
}

//go:linkname syscall_runtimeClearenv syscall.runtimeClearenv
internal static void syscall_runtimeClearenv(map<@string, nint> env) {
    clearenv_c();
    // Did we just unset GODEBUG?
    {
        var (_, ok) = env[godebugˢ, ꟷ]; if (ok) {
            ᏑgodebugEnv.Store(nil);
            godebugNotify(true);
        }
    }
}

} // end runtime_package
