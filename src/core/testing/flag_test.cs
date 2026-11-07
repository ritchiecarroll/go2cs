// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using flag = flag_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = go.os.exec_package;
using testing = testing_package;
using @internal;
using go.os;

partial class testing_test_package {

internal static ж<@string> testFlagArg = flag.String("test_flag_arg", "", "TestFlag: passing -v option");

internal static ref readonly @string flagTestEnv => ref flagTestEnvᵇ;
private static readonly @string flagTestEnvᵇ = "GO_WANT_FLAG_HELPER_PROCESS"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testRunTestFlagˢ => ref testRunTestFlagˢᵇ;
private static readonly @string testRunTestFlagˢᵇ = "-test.run=^TestFlag$"u8;

public static void TestFlag(ж<testing.T> t) {
    if (Δos.Getenv(flagTestEnv) == "1"u8) {
        testFlagHelper(t);
        return;
    }

    testenv.MustHaveExec(new testing_TжTB(t));

    foreach (var (_, flag) in range(slice<@string>(["", "-test.v", "-test.v=test2json"]))) {
        t.Run(flag, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var cmd = exec.Command(testenv.Executable(new testing_TжTB(tΔ1)), testRunTestFlagˢ, "-test_flag_arg=" + flag);
            if (flag != ""u8) {
                cmd.Args = append(cmd.Args, flag);
            }
            cmd.Env = append(cmd.Environ(), flagTestEnv + "=1");
            var (b, err) = cmd.CombinedOutput();
            if (len(b) > 0) {
                // When we set -test.v=test2json, we need to escape the ^V control
                // character used for JSON framing so that the JSON parser doesn't
                // misinterpret the subprocess output as output from the parent test.
                tΔ1.Logf("%q", b);
            }
            if (err != nil) {
                tΔ1.Error(err);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testVˢ => ref testVˢᵇ;
private static readonly @string testVˢᵇ = "test.v"u8;
private static readonly object flagLookupTestVFailedˢ = (@string)@"flag.Lookup(""test.v"") failed"u8;
private static readonly object testVIsBoolFlagReturnedˢ = (@string)"test.v IsBoolFlag() returned false"u8;
private static readonly object test2jsonˢ = (@string)"test2json"u8;

internal partial interface testFlagHelper_type /*dyn*/ {
    bool IsBoolFlag();
}

// testFlagHelper is called by the TestFlagHelper subprocess.
internal static void testFlagHelper(ж<testing.T> t) {
    var f = flag.Lookup(testVˢ);
    if (f == nil) {
        t.Fatal(flagLookupTestVFailedˢ);
    }

    var (bf, ok) = (~f).Value._<testFlagHelper_type>(ᐧ);
    if (!ok) {
        t.Errorf("test.v flag (type %T) does not have IsBoolFlag method", f.OrTypedNil());
    } else if (!bf.IsBoolFlag()) {
        t.Error(testVIsBoolFlagReturnedˢ);
    }

    (var gf, ok) = (~f).Value._<flag.Getter>(ᐧ);
    if (!ok) {
        t.Fatalf("test.v flag (type %T) does not have Get method", f.OrTypedNil());
    }
    var v = gf.Get();

    any want = default;
    var exprᴛ1 = testFlagArg.Value;
    switch (ᐧ) {
    case {} when exprᴛ1 == ""u8: {
        want = false;
        break;
    }
    case {} when exprᴛ1 == "-test.v"u8: {
        want = true;
        break;
    }
    case {} when exprᴛ1 == "-test.v=test2json"u8: {
        want = test2jsonˢ;
        break;
    }
    default: {
        t.Fatalf("unexpected test_flag_arg %q", testFlagArg.Value);
        break;
    }}

    if (!AreEqual(v, want)) {
        t.Errorf("test.v is %v want %v", v, want);
    }
}

} // end testing_test_package
