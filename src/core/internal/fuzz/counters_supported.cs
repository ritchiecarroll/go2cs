// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (darwin || linux || windows || freebsd || openbsd) && (amd64 || arm64 || loong64)
namespace go.@internal;

using @unsafe = unsafe_package;

partial class fuzz_package {

// coverage returns a []byte containing unique 8-bit counters for each edge of
// the instrumented source code. This coverage data will only be generated if
// `-d=libfuzzer` is set at build time. This can be used to understand the code
// coverage of a test execution.
internal static slice<byte> coverage() {
    @unsafe.Pointer addr = @unsafe.Pointer.FromPinnedBox(Ꮡ_counters);
    var size = (uintptr)Ꮡ_ecounters - (uintptr)addr;
    return @unsafe.Slice((ж<byte>)(uintptr)(addr), (nint)size);
}

} // end fuzz_package
