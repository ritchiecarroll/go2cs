// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string checkAVXˢ => ref checkAVXˢᵇ;
private static readonly @string checkAVXˢᵇ = "CheckAVX"u8;

public static void TestHasAVX(ж<testing.T> t) {
    t.Parallel();
    @string output = runTestProg(t, testprogˢ, checkAVXˢ);
    var ok = output == "OK\n"u8;
    if (runtime_internal_test_package.X86HasAVX.Value != ok) {
        t.Fatalf("x86HasAVX: %v, CheckAVX got:\n%s", runtime_internal_test_package.X86HasAVX.Value, output);
    }
}

} // end runtime_test_package
