// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using Δruntime = runtime_package;
using testing = testing_package;
using @internal;
using exec = global::go.os.exec_package;
using global::go.os;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testsyscallˢ => ref testsyscallˢᵇ;
private static readonly @string testsyscallˢᵇ = "testsyscall"u8;

public static void TestSyscallArgs(ж<testing.T> t) {
    if (Δruntime.GOOS != "darwin"u8) {
        t.Skipf("skipping test: GOARCH=%s", Δruntime.GOARCH);
    }
    testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(t));

    var (exe, err) = buildTestProg(t, testsyscallˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    var cmd = testenv.Command(new runtime_test_package.testing_TжTB(t), exe);
    {
        var (@out, errΔ1) = cmd.CombinedOutput(); if (errΔ1 != nil) {
            t.Fatalf("test program failed: %v\n%s", errΔ1, @out);
        }
    }
}

} // end runtime_test_package
