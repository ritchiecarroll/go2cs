// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string checkAVXˢ = "CheckAVX"u8;

public static void TestHasAVX(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string output = runTestProg(Ꮡt, testprogˢ, checkAVXˢ);
    var ok = output == "OK\n"u8;
    if (runtime_internal_test_package.X86HasAVX.Value != ok) {
        Ꮡt.Fatalf("x86HasAVX: %v, CheckAVX got:\n%s"u8, runtime_internal_test_package.X86HasAVX.Value, output);
    }
}

} // end runtime_test_package
