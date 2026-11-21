// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows && !darwin
namespace go.runtime;

using errors = errors_package;
using os = os_package;

partial class pprof_package {

// go2cs generated this placeholder — func readMapping is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string notImplementedˢ => ref notImplementedˢᵇ;
private static readonly @string notImplementedˢᵇ = "not implemented"u8;

internal static (uint64 start, uint64 end, @string exe, @string buildID, error err) readMainModuleMapping() {
    return (0, 0, "", "", errors.New(notImplementedˢ));
}

} // end pprof_package
