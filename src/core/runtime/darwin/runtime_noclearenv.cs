// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux
namespace go;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname

partial class runtime_package {

//go:linkname syscall_runtimeClearenv syscall.runtimeClearenv
internal static void syscall_runtimeClearenv(map<@string, nint> env) {
    // The system doesn't have clearenv(3) so emulate it by unsetting all of
    // the variables manually.
    foreach (var (k, _) in env) {
        syscall_runtimeUnsetenv(k);
    }
}

} // end runtime_package
