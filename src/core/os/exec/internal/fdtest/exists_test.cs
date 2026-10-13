// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os.exec.@internal;

using os = os_package;
using runtime = runtime_package;
using testing = testing_package;
using static go.os.exec.@internal.fdtest_package;

partial class fdtest_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object existsNotImplementedForˢ = (@string)"Exists not implemented for windows"u8;

public static void TestExists(ж<testing.T> Ꮡt) {
    if (runtime.GOOS == "windows"u8) {
        Ꮡt.Skip(existsNotImplementedForˢ);
    }

    if (!Exists(os.Stdout.Fd())) {
        Ꮡt.Errorf("Exists(%d) got false want true"u8, os.Stdout.Fd());
    }
}

} // end fdtest_internal_test_package
