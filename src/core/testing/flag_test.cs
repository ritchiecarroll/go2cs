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

internal static ж<@string> testFlagArg = flag.String("test_flag_arg"u8, ""u8, "TestFlag: passing -v option"u8);

internal static readonly @string flagTestEnv = "GO_WANT_FLAG_HELPER_PROCESS"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testRunTestFlagˢ = "-test.run=^TestFlag$"u8;

public static void TestFlag(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Δos.Getenv(flagTestEnv) == "1"u8) {
        testFlagHelper(Ꮡt);
        return;
    }
    testenv.MustHaveExec(new testing_TжTB(Ꮡt));
    foreach (var (_, flag) in new @string[]{""u8, "-test.v"u8, "-test.v=test2json"u8}.slice()) {
        @string flagΔ1 = flag;
        Ꮡt.Run(flagΔ1, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var cmd = exec.Command(testenv.Executable(new testing_TжTB(tΔ1)), testRunTestFlagˢ, "-test_flag_arg=" + flagΔ1);
            if (flagΔ1 != ""u8) {
                cmd.Value.Args = append((~cmd).Args, flagΔ1);
            }
            cmd.Value.Env = append(cmd.Environ(), flagTestEnv + "=1");
            var (b, err) = cmd.CombinedOutput();
            if (len(b) > 0) {
                // When we set -test.v=test2json, we need to escape the ^V control
                // character used for JSON framing so that the JSON parser doesn't
                // misinterpret the subprocess output as output from the parent test.
                tΔ1.Logf("%q"u8, b);
            }
            if (err != default!) {
                tΔ1.Error(err);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testVˢ = "test.v"u8;
private static readonly object flagLookupTestVFailedˢ = (@string)@"flag.Lookup(""test.v"") failed"u8;
private static readonly object testVIsBoolFlagReturnedˢ = (@string)"test.v IsBoolFlag() returned false"u8;
private static readonly object test2jsonˢ = (@string)"test2json"u8;

internal partial interface testFlagHelper_type /*dyn*/ {
    bool IsBoolFlag();
}

// testFlagHelper is called by the TestFlagHelper subprocess.
internal static void testFlagHelper(ж<testing.T> Ꮡt) {
    var f = flag.Lookup(testVˢ);
    if (f == nil) {
        Ꮡt.Fatal(flagLookupTestVFailedˢ);
    }
    var (bf, ok) = (~f).Value._<testFlagHelper_type>(ᐧ);
    if (!ok){
        Ꮡt.Errorf("test.v flag (type %T) does not have IsBoolFlag method"u8, f.OrTypedNil());
    } else 
    if (!bf.IsBoolFlag()) {
        Ꮡt.Error(testVIsBoolFlagReturnedˢ);
    }
    (var gf, ok) = (~f).Value._<flag.Getter>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("test.v flag (type %T) does not have Get method"u8, f.OrTypedNil());
    }
    var v = gf.Get();
    any want = default!;
    var exprᴛ1 = testFlagArg.Value;
    if (exprᴛ1 == ""u8) {
        want = false;
    }
    else if (exprᴛ1 == "-test.v"u8) {
        want = true;
    }
    else if (exprᴛ1 == "-test.v=test2json"u8) {
        want = test2jsonˢ;
    }
    else { /* default: */
        Ꮡt.Fatalf("unexpected test_flag_arg %q"u8, testFlagArg.Value);
    }

    if (!AreEqual(v, want)) {
        Ꮡt.Errorf("test.v is %v want %v"u8, v, want);
    }
}

} // end testing_test_package
