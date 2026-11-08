// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = global::go.os.exec_package;
using testing = testing_package;
using @internal;
using global::go.os;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string runˢ => ref runˢᵇ;
private static readonly @string runˢᵇ = "run"u8;
internal static ref readonly @string mklockrankGoˢ => ref mklockrankGoˢᵇ;
private static readonly @string mklockrankGoˢᵇ = "mklockrank.go"u8;
internal static ref readonly @string lockrankGoˢ => ref lockrankGoˢᵇ;
private static readonly @string lockrankGoˢᵇ = "lockrank.go"u8;

// Test that the generated code for the lock rank graph is up-to-date.
public static void TestLockRankGenerated(ж<testing.T> t) {
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(t));
    var cmd = testenv.CleanCmdEnv(testenv.Command(new runtime_test_package.testing_TжTB(t), testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), runˢ, mklockrankGoˢ));
    var (want, err) = cmd.Output();
    if (err != nil) {
        var (ee, ok) = err._<ж<exec.ExitError>>(ᐧ); if (ok && len(ee.Stderr) > 0) {
            t.Fatalf("%v: %v\n%s", cmd.OrTypedNil(), err, ee.Stderr);
        }
        t.Fatalf("%v: %v", cmd.OrTypedNil(), err);
    }
    (var got, err) = Δos.ReadFile(lockrankGoˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!bytes.Equal(want, got)) {
        t.Fatalf("lockrank.go is out of date. Please run go generate.");
    }
}

} // end runtime_test_package
