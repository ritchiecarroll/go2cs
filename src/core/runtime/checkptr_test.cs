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
internal static readonly @string testprogˢ = "testprog"u8;
internal static readonly @string gcflagsAllDCheckptr1ˢ = "-gcflags=all=-d=checkptr=1"u8;

internal partial struct TestCheckPtr_testCases /*dyn*/ {
    internal @string cmd;
    internal @string want;
}

public static void TestCheckPtr(ж<testing.T> Ꮡt) {
    // This test requires rebuilding packages with -d=checkptr=1,
    // so it's somewhat slow.
    if (testing.Short()) {
        Ꮡt.Skip(skippingTestInShortModeˢ);
    }
    Ꮡt.Parallel();
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(Ꮡt));
    var (exe, err) = buildTestProg(Ꮡt, testprogˢ, gcflagsAllDCheckptr1ˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var testCases = new TestCheckPtr_testCases[]{
        new("CheckPtrAlignmentPtr"u8, "fatal error: checkptr: misaligned pointer conversion\n"u8),
        new("CheckPtrAlignmentNoPtr"u8, ""u8),
        new("CheckPtrAlignmentNilPtr"u8, ""u8),
        new("CheckPtrArithmetic"u8, "fatal error: checkptr: pointer arithmetic result points to invalid allocation\n"u8),
        new("CheckPtrArithmetic2"u8, "fatal error: checkptr: pointer arithmetic result points to invalid allocation\n"u8),
        new("CheckPtrSize"u8, "fatal error: checkptr: converted pointer straddles multiple allocations\n"u8),
        new("CheckPtrSmall"u8, "fatal error: checkptr: pointer arithmetic computed bad pointer value\n"u8),
        new("CheckPtrSliceOK"u8, ""u8),
        new("CheckPtrSliceFail"u8, "fatal error: checkptr: unsafe.Slice result straddles multiple allocations\n"u8),
        new("CheckPtrStringOK"u8, ""u8),
        new("CheckPtrStringFail"u8, "fatal error: checkptr: unsafe.String result straddles multiple allocations\n"u8)
    }.slice();
    foreach (var (_, tc) in testCases) {
        ref var tcΔ1 = ref heap<TestCheckPtr_testCases>(out var ᏑtcΔ1);
        tcΔ1 = tc;
        var tcʗ1 = tcΔ1;
        Ꮡt.Run(tcΔ1.cmd, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var (got, errΔ1) = testenv.CleanCmdEnv(exec.Command(exe, tcʗ1.cmd)).CombinedOutput();
            if (errΔ1 != default!) {
                tΔ1.Log(errΔ1);
            }
            if (tcʗ1.want == ""u8) {
                if (len(got) > 0) {
                    tΔ1.Errorf("output:\n%s\nwant no output"u8, got);
                }
                return;
            }
            if (!strings.HasPrefix(((@string)got), tcʗ1.want)) {
                tΔ1.Errorf("output:\n%s\n\nwant output starting with: %s"u8, got, tcʗ1.want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcflagsAllDCheckptr2ˢ = "-gcflags=all=-d=checkptr=2"u8;

internal partial struct TestCheckPtr2_testCases /*dyn*/ {
    internal @string cmd;
    internal @string want;
}

public static void TestCheckPtr2(ж<testing.T> Ꮡt) {
    // This test requires rebuilding packages with -d=checkptr=2,
    // so it's somewhat slow.
    if (testing.Short()) {
        Ꮡt.Skip(skippingTestInShortModeˢ);
    }
    Ꮡt.Parallel();
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(Ꮡt));
    var (exe, err) = buildTestProg(Ꮡt, testprogˢ, gcflagsAllDCheckptr2ˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var testCases = new TestCheckPtr2_testCases[]{
        new("CheckPtrAlignmentNested"u8, "fatal error: checkptr: converted pointer straddles multiple allocations\n"u8)
    }.slice();
    foreach (var (_, tc) in testCases) {
        ref var tcΔ1 = ref heap<TestCheckPtr2_testCases>(out var ᏑtcΔ1);
        tcΔ1 = tc;
        var tcʗ1 = tcΔ1;
        Ꮡt.Run(tcΔ1.cmd, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var (got, errΔ1) = testenv.CleanCmdEnv(exec.Command(exe, tcʗ1.cmd)).CombinedOutput();
            if (errΔ1 != default!) {
                tΔ1.Log(errΔ1);
            }
            if (tcʗ1.want == ""u8) {
                if (len(got) > 0) {
                    tΔ1.Errorf("output:\n%s\nwant no output"u8, got);
                }
                return;
            }
            if (!strings.HasPrefix(((@string)got), tcʗ1.want)) {
                tΔ1.Errorf("output:\n%s\n\nwant output starting with: %s"u8, got, tcʗ1.want);
            }
        });
    }
}

} // end runtime_test_package
