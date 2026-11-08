// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using exec = global::go.os.exec_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using global::go.os;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInShortModeˢ = (@string)"skipping test in -short mode"u8;
internal static ref readonly @string testprogˢ => ref testprogˢᵇ;
private static readonly @string testprogˢᵇ = "testprog"u8;
internal static ref readonly @string gcflagsAllDCheckptr1ˢ => ref gcflagsAllDCheckptr1ˢᵇ;
private static readonly @string gcflagsAllDCheckptr1ˢᵇ = "-gcflags=all=-d=checkptr=1"u8;

internal partial struct TestCheckPtr_testCases /*dyn*/ {
    internal @string cmd;
    internal @string want;
}

public static void TestCheckPtr(ж<testing.T> t) {
    // This test requires rebuilding packages with -d=checkptr=1,
    // so it's somewhat slow.
    if (testing.Short()) {
        t.Skip(skippingTestInShortModeˢ);
    }

    t.Parallel();
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(t));

    var (exe, err) = buildTestProg(t, testprogˢ, gcflagsAllDCheckptr1ˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    var testCases = slice<TestCheckPtr_testCases>([
        new("CheckPtrAlignmentPtr", "fatal error: checkptr: misaligned pointer conversion\n"),
        new("CheckPtrAlignmentNoPtr", ""),
        new("CheckPtrAlignmentNilPtr", ""),
        new("CheckPtrArithmetic", "fatal error: checkptr: pointer arithmetic result points to invalid allocation\n"),
        new("CheckPtrArithmetic2", "fatal error: checkptr: pointer arithmetic result points to invalid allocation\n"),
        new("CheckPtrArithmeticUnsafeAdd", "fatal error: checkptr: pointer arithmetic result points to invalid allocation\n"),
        new("CheckPtrSize", "fatal error: checkptr: converted pointer straddles multiple allocations\n"),
        new("CheckPtrSmall", "fatal error: checkptr: pointer arithmetic computed bad pointer value\n"),
        new("CheckPtrSliceOK", ""),
        new("CheckPtrSliceFail", "fatal error: checkptr: unsafe.Slice result straddles multiple allocations\n"),
        new("CheckPtrStringOK", ""),
        new("CheckPtrStringFail", "fatal error: checkptr: unsafe.String result straddles multiple allocations\n")
    ]);

    foreach (var (_, vᴛ1) in range(testCases)) {
        ref var tc = ref heap(new TestCheckPtr_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.cmd, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var (got, errΔ1) = testenv.CleanCmdEnv(exec.Command(exe, tcʗ1.cmd)).CombinedOutput();
            if (errΔ1 != nil) {
                tΔ1.Log(errΔ1);
            }
            if (tcʗ1.want == ""u8) {
                if (len(got) > 0) {
                    tΔ1.Errorf("output:\n%s\nwant no output", got);
                }
                return;
            }
            if (!strings.HasPrefix((@string)got, tcʗ1.want)) {
                tΔ1.Errorf("output:\n%s\n\nwant output starting with: %s", got, tcʗ1.want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gcflagsAllDCheckptr2ˢ => ref gcflagsAllDCheckptr2ˢᵇ;
private static readonly @string gcflagsAllDCheckptr2ˢᵇ = "-gcflags=all=-d=checkptr=2"u8;

internal partial struct TestCheckPtr2_testCases /*dyn*/ {
    internal @string cmd;
    internal @string want;
}

public static void TestCheckPtr2(ж<testing.T> t) {
    // This test requires rebuilding packages with -d=checkptr=2,
    // so it's somewhat slow.
    if (testing.Short()) {
        t.Skip(skippingTestInShortModeˢ);
    }

    t.Parallel();
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(t));

    var (exe, err) = buildTestProg(t, testprogˢ, gcflagsAllDCheckptr2ˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    var testCases = slice<TestCheckPtr2_testCases>([
        new("CheckPtrAlignmentNested", "fatal error: checkptr: converted pointer straddles multiple allocations\n")
    ]);

    foreach (var (_, vᴛ1) in range(testCases)) {
        ref var tc = ref heap(new TestCheckPtr2_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.cmd, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var (got, errΔ1) = testenv.CleanCmdEnv(exec.Command(exe, tcʗ1.cmd)).CombinedOutput();
            if (errΔ1 != nil) {
                tΔ1.Log(errΔ1);
            }
            if (tcʗ1.want == ""u8) {
                if (len(got) > 0) {
                    tΔ1.Errorf("output:\n%s\nwant no output", got);
                }
                return;
            }
            if (!strings.HasPrefix((@string)got, tcʗ1.want)) {
                tΔ1.Errorf("output:\n%s\n\nwant output starting with: %s", got, tcʗ1.want);
            }
        });
    }
}

} // end runtime_test_package
