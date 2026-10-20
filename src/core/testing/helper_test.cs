// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using Δos = os_package;
using Δregexp = regexp_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using exec = go.os.exec_package;
using go.os;

partial class testing_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goWantHelperProcessˢ = "GO_WANT_HELPER_PROCESS"u8;
private static readonly @string testRunTestTBHelperˢ = "-test.run=^TestTBHelper$"u8;
private static readonly @string failTestTBHelperˢ = """
--- FAIL: TestTBHelper \([^)]+\)
    helperfuncs_test.go:15: 0
    helperfuncs_test.go:47: 1
    helperfuncs_test.go:24: 2
    helperfuncs_test.go:49: 3
    helperfuncs_test.go:56: 4
    --- FAIL: TestTBHelper/sub \([^)]+\)
        helperfuncs_test.go:59: 5
        helperfuncs_test.go:24: 6
        helperfuncs_test.go:58: 7
    --- FAIL: TestTBHelper/sub2 \([^)]+\)
        helperfuncs_test.go:80: 11
    helperfuncs_test.go:84: recover 12
    helperfuncs_test.go:86: GenericFloat64
    helperfuncs_test.go:87: GenericInt
    helper_test.go:22: 8
    helperfuncs_test.go:73: 9
    helperfuncs_test.go:69: 10

"""u8.Raw();

public static void TestTBHelper(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        testTestHelper(Ꮡt);

        // Check that calling Helper from inside a top-level test function
        // has no effect.
        Ꮡt.Helper();
        Ꮡt.Error((@string)"8"u8);
        return;
    }

    Ꮡt.Parallel();

    var cmd = testenv.Command(new testing_TжTB(Ꮡt), testenv.Executable(new testing_TжTB(Ꮡt)), testRunTestTBHelperˢ);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Value.Env = append((~cmd).Env, "GO_WANT_HELPER_PROCESS=1"u8);
    var (@out, _) = cmd.CombinedOutput();

    @string want = failTestTBHelperˢ;
    if (!Δregexp.MustCompile(want).Match(@out)) {
        Ꮡt.Errorf("got output:\n\n%s\nwant matching:\n\n%s"u8, @out, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testRunˢ = "-test.run=^TestTBHelperParallel$"u8;
private static readonly @string helperfuncsTestGo24ˢ = "helperfuncs_test.go:24: parallel"u8;

public static void TestTBHelperParallel(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        parallelTestHelper(Ꮡt);
        return;
    }

    Ꮡt.Parallel();

    var cmd = testenv.Command(new testing_TжTB(Ꮡt), testenv.Executable(new testing_TжTB(Ꮡt)), testRunˢ);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Value.Env = append((~cmd).Env, "GO_WANT_HELPER_PROCESS=1"u8);
    var (@out, _) = cmd.CombinedOutput();

    Ꮡt.Logf("output:\n%s"u8, @out);

    var lines = strings.Split(strings.TrimSpace((@string)@out), "\n"u8);

    // We expect to see one "--- FAIL" line at the start
    // of the log, five lines of "parallel" logging,
    // and a final "FAIL" line at the end of the test.
    const nint wantLines = 7;

    if (len(lines) != wantLines) {
        Ꮡt.Fatalf("parallelTestHelper gave %d lines of output; want %d"u8, len(lines), (nint)(wantLines));
    }
    @string want = helperfuncsTestGo24ˢ;
    {
        @string got = strings.TrimSpace(lines[1]); if (got != want) {
            Ꮡt.Errorf("got second output line %q; want %q"u8, got, want);
        }
    }
}

public static void BenchmarkTBHelper(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    void f1() {
        Ꮡb.Helper();
    }
    void f2() {
        Ꮡb.Helper();
    }
    b.ResetTimer();
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        if ((i & 1) == 0){
            f1();
        } else {
            f2();
        }
    }
}

} // end testing_test_package
