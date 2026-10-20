// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using startlinetest = @internal.runtime.startlinetest_package;
using testing = testing_package;
using @internal.runtime;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// TestStartLineAsm tests the start line metadata of an assembly function. This
// is only tested on amd64 to avoid the need for a proliferation of per-arch
// copies of this function.
public static void TestStartLineAsm(ж<testing.T> Ꮡt) {
    startlinetest.CallerStartLine = callerStartLine;

    const nint wantLine = 23;
    nint got = startlinetest.AsmFunc();
    if (got != wantLine) {
        Ꮡt.Errorf("start line got %d want %d"u8, got, (nint)(wantLine));
    }
}

} // end runtime_test_package
