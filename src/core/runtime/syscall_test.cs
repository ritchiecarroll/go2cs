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
internal static readonly @string testsyscallˢ = "testsyscall"u8;

public static void TestSyscallArgs(ж<testing.T> Ꮡt) {
    if (Δruntime.GOOS != "darwin"u8) {
        Ꮡt.Skipf("skipping test: GOARCH=%s"u8, Δruntime.GOARCH);
    }
    testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(Ꮡt));

    var (exe, err) = buildTestProg(Ꮡt, testsyscallˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    var cmd = testenv.Command(new runtime_test_package.testing_TжTB(Ꮡt), exe);
    {
        var (@out, errΔ1) = cmd.CombinedOutput(); if (errΔ1 != default!) {
            Ꮡt.Fatalf("test program failed: %v\n%s"u8, errΔ1, @out);
        }
    }
}

} // end runtime_test_package
