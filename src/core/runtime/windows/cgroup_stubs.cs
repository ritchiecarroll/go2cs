// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux
namespace go;

partial class runtime_package {

internal static void defaultGOMAXPROCSInit() {
}

internal static void defaultGOMAXPROCSUpdateGODEBUG() {
}

internal static int32 defaultGOMAXPROCS(int32 ncpu) {
    // Use the total number of logical CPUs available now, as CPU affinity
    // may change after start.
    //
    // TODO(prattmic): On some GOOS getCPUCount can never change. Don't
    // bother calling over and over.

    var procs = ncpu;
    if (procs <= 0) {
        procs = getCPUCount();
    }
    return procs;
}

} // end runtime_package
