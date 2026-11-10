// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cpu = @internal.cpu_package;
using @internal;

partial class runtime_package {

internal static bool arm64UseAlignedLoads;

/* [GoInit] runtime bootstrap init - not run; .NET is the runtime */ internal static void initΔ1() {
    if (cpu.ARM64.IsNeoverse) {
        arm64UseAlignedLoads = true;
    }
}

} // end runtime_package
