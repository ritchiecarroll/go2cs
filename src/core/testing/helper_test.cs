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
private static ref readonly @string goWantHelperProcessˢ => ref goWantHelperProcessˢᵇ;
private static readonly @string goWantHelperProcessˢᵇ = "GO_WANT_HELPER_PROCESS"u8;
private static ref readonly @string testRunTestTBHelperˢ => ref testRunTestTBHelperˢᵇ;
private static readonly @string testRunTestTBHelperˢᵇ = "-test.run=^TestTBHelper$"u8;
private static ref readonly @string failTestTBHelperˢ => ref failTestTBHelperˢᵇ;
private static readonly @string failTestTBHelperˢᵇ = """
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

public static void TestTBHelper(ж<testing.T> t) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        testTestHelper(t);

        // Check that calling Helper from inside a top-level test function
        // has no effect.
        t.Helper();
        t.Error((@string)"8");
        return;
    }

    t.Parallel();

    var cmd = testenv.Command(new testing_TжTB(t), testenv.Executable(new testing_TжTB(t)), testRunTestTBHelperˢ);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Env = append(cmd.Env, (@string)"GO_WANT_HELPER_PROCESS=1");
    var (@out, _) = cmd.CombinedOutput();

    @string want = failTestTBHelperˢ;
    if (!Δregexp.MustCompile(want).Match(@out)) {
        t.Errorf("got output:\n\n%s\nwant matching:\n\n%s", @out, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testRunˢ => ref testRunˢᵇ;
private static readonly @string testRunˢᵇ = "-test.run=^TestTBHelperParallel$"u8;
private static ref readonly @string helperfuncsTestGo24ˢ => ref helperfuncsTestGo24ˢᵇ;
private static readonly @string helperfuncsTestGo24ˢᵇ = "helperfuncs_test.go:24: parallel"u8;

public static void TestTBHelperParallel(ж<testing.T> t) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        parallelTestHelper(t);
        return;
    }

    t.Parallel();

    var cmd = testenv.Command(new testing_TжTB(t), testenv.Executable(new testing_TжTB(t)), testRunˢ);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Env = append(cmd.Env, (@string)"GO_WANT_HELPER_PROCESS=1");
    var (@out, _) = cmd.CombinedOutput();

    t.Logf("output:\n%s", @out);

    var lines = strings.Split(strings.TrimSpace((@string)@out), "\n");

    // We expect to see one "--- FAIL" line at the start
    // of the log, five lines of "parallel" logging,
    // and a final "FAIL" line at the end of the test.
    const nint wantLines = 7;

    if (len(lines) != wantLines) {
        t.Fatalf("parallelTestHelper gave %d lines of output; want %d", len(lines), (nint)(wantLines));
    }
    @string want = helperfuncsTestGo24ˢ;
    @string got = strings.TrimSpace(lines[1]); if (got != want) {
        t.Errorf("got second output line %q; want %q", got, want);
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
        if ((i & 1) == 0) {
            f1();
        } else {
            f2();
        }
    }
}

} // end testing_test_package
