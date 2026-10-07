// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using race = @internal.race_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using Δregexp = regexp_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = io.fs_package;
using go.os;
using io;
using path;

partial class testing_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goWantRaceBeforeTestsˢ = "GO_WANT_RACE_BEFORE_TESTS"u8;

// This is exactly what a test would do without a TestMain.
// It's here only so that there is at least one package in the
// standard library with a TestMain, so that code is executed.
public static void TestMain(ж<testing.M> Ꮡm) {
    if (Δos.Getenv(goWantRaceBeforeTestsˢ) == "1"u8) {
        doRace();
    }
    Ꮡm.Run();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testˢ = "test"u8;

// Note: m.Run currently prints the final "PASS" line, so if any race is
// reported here (after m.Run but before the process exits), it will print
// "PASS", then print the stack traces for the race, then exit with nonzero
// status.
//
// This is a somewhat fundamental race: because the race detector hooks into
// the runtime at a very low level, no matter where we put the printing it
// would be possible to report a race that occurs afterward. However, we could
// theoretically move the printing after TestMain, which would at least do a
// better job of diagnosing races in cleanup functions within TestMain itself.
public static void TestTempDirInCleanup(ж<testing.T> Ꮡt) {
    @string dir = default!;
    Ꮡt.Run(testˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Cleanup(() => {
            dir = tΔ1.TempDir();
        });
        _ = tΔ1.TempDir();
    });
    var (fi, err) = Δos.Stat(dir);
    if (fi != default!) {
        Ꮡt.Fatalf("Directory %q from user Cleanup still exists"u8, dir);
    }
    if (!Δos.IsNotExist(err)) {
        Ꮡt.Fatalf("Unexpected error: %v"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object subTestFailureInAˢ = (@string)"Sub test failure in a benchmark"u8;

public static void TestTempDirInBenchmark(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testing.Benchmark((ж<testing.B> b) => {
        if (!b.Run(testˢ, (ж<testing.B> bΔ1) => {
            // Add a loop so that the test won't fail. See issue 38677.
            for (nint i = 0; i < (~bΔ1).N; i++) {
                _ = bΔ1.TempDir();
            }
        })) {
            Ꮡt.Fatal(subTestFailureInAˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string inSubtestˢ = "InSubtest"u8;
private static readonly @string testSubtestˢ = "test/subtest"u8;
private static readonly @string testSubtestˢ2 = "test\\subtest"u8;
private static readonly @string testSubtestˢ3 = "test:subtest"u8;
private static readonly @string testˢ2 = "test/.."u8;
private static readonly @string testˢ3 = "../test"u8;
private static readonly @string testˢ4 = "test[]"u8;
private static readonly @string testˢ5 = "test*"u8;

public static void TestTempDir(ж<testing.T> Ꮡt) {
    testTempDir(Ꮡt);
    Ꮡt.Run(inSubtestˢ, testTempDir);
    Ꮡt.Run(testSubtestˢ, testTempDir);
    Ꮡt.Run(testSubtestˢ2, testTempDir);
    Ꮡt.Run(testSubtestˢ3, testTempDir);
    Ꮡt.Run(testˢ2, testTempDir);
    Ꮡt.Run(testˢ3, testTempDir);
    Ꮡt.Run(testˢ4, testTempDir);
    Ꮡt.Run(testˢ5, testTempDir);
    Ꮡt.Run("äöüéè"u8, testTempDir);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object neverReceivedDirChannelˢ = (@string)"never received dir channel"u8;
private static readonly object expectedDirˢ = (@string)"expected dir"u8;
private static readonly object subsequentCallsToTempDirˢ = (@string)"subsequent calls to TempDir returned the same directory"u8;
private static readonly @string txtˢ = "*.txt"u8;

internal static void testTempDir(ж<testing.T> Ꮡt) {
    var dirCh = new channel<@string>(1);
    var dirChʗ1 = dirCh;
    Ꮡt.Cleanup(() => {
        // Verify directory has been removed.
        var selᴛ1 = dirChʗ1;
        switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out var dirΔ1): {
            var (fiΔ1, errΔ1) = Δos.Stat(dirΔ1);
            if (Δos.IsNotExist(errΔ1)) {
                // All good
                return;
            }
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            Ꮡt.Errorf("directory %q still exists: %v, isDir=%v"u8, dirΔ1, fiΔ1, fiΔ1.IsDir());
            break;
        }
        default: {
            if (!Ꮡt.Failed()) {
                Ꮡt.Fatal(neverReceivedDirChannelˢ);
            }
            break;
        }}
    });
    @string dir = Ꮡt.TempDir();
    if (dir == ""u8) {
        Ꮡt.Fatal(expectedDirˢ);
    }
    @string dir2 = Ꮡt.TempDir();
    if (dir == dir2) {
        Ꮡt.Fatal(subsequentCallsToTempDirˢ);
    }
    if (filepath.Dir(dir) != filepath.Dir(dir2)) {
        Ꮡt.Fatalf("calls to TempDir do not share a parent; got %q, %q"u8, dir, dir2);
    }
    dirCh.ᐸꟷ(dir);
    var (fi, err) = Δos.Stat(dir);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!fi.IsDir()) {
        Ꮡt.Errorf("dir %q is not a dir"u8, dir);
    }
    (var files, err) = Δos.ReadDir(dir);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (len(files) > 0) {
        Ꮡt.Errorf("unexpected %d files in TempDir: %v"u8, len(files), files);
    }
    @string glob = filepath.Join(dir, txtˢ);
    {
        var (_, errΔ2) = filepath.Glob(glob); if (errΔ2 != default!) {
            Ꮡt.Error(errΔ2);
        }
    }
}

[GoType("dyn")] internal partial struct TestSetenv_tests {
    internal @string name;
    internal @string key;
    internal bool initialValueExists;
    internal @string initialValue;
    internal @string newValue;
}

public static void TestSetenv(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestSetenv_tests[]{
        new(
            name: "initial value exists"u8,
            key: "GO_TEST_KEY_1"u8,
            initialValueExists: true,
            initialValue: "111"u8,
            newValue: "222"u8
        ),
        new(
            name: "initial value exists but empty"u8,
            key: "GO_TEST_KEY_2"u8,
            initialValueExists: true,
            initialValue: ""u8,
            newValue: "222"u8
        ),
        new(
            name: "initial value is not exists"u8,
            key: "GO_TEST_KEY_3"u8,
            initialValueExists: false,
            initialValue: ""u8,
            newValue: "222"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestSetenv_tests(), out var Ꮡtest);
        test = vᴛ1;

        if (test.initialValueExists){
            {
                var err = Δos.Setenv(test.key, test.initialValue); if (err != default!) {
                    Ꮡt.Fatalf("unable to set env: got %v"u8, err);
                }
            }
        } else {
            Δos.Unsetenv(test.key);
        }
        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            tΔ1.Setenv(testʗ1.key, testʗ1.newValue);
            if (Δos.Getenv(testʗ1.key) != testʗ1.newValue) {
                tΔ1.Fatalf("unexpected value after t.Setenv: got %s, want %s"u8, Δos.Getenv(testʗ1.key), testʗ1.newValue);
            }
        });
        var (got, exists) = Δos.LookupEnv(test.key);
        if (got != test.initialValue) {
            Ꮡt.Fatalf("unexpected value after t.Setenv cleanup: got %s, want %s"u8, got, test.initialValue);
        }
        if (exists != test.initialValueExists) {
            Ꮡt.Fatalf("unexpected value after t.Setenv cleanup: got %t, want %t"u8, exists, test.initialValueExists);
        }
    }
}

internal static void expectParallelConflict(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string want = testing.ParallelConflict;
        {
            var got = recover(); if (!AreEqual(got, want)) {
                Ꮡt.Fatalf("expected panic; got %#v want %q"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testWithParallelAfter(ж<testing.T> Ꮡt, Action<ж<testing.T>> fn) {
    GoFrame ᒐ = default;
    try {
        defer(expectParallelConflict, Ꮡt, ref ᒐ);
        fn(Ꮡt);
        Ꮡt.Parallel();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testWithParallelBefore(ж<testing.T> Ꮡt, Action<ж<testing.T>> fn) {
    GoFrame ᒐ = default;
    try {
        defer(expectParallelConflict, Ꮡt, ref ᒐ);
        Ꮡt.Parallel();
        fn(Ꮡt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string childˢ = "child"u8;

internal static void testWithParallelParentBefore(ж<testing.T> Ꮡt, Action<ж<testing.T>> fn) {
    Ꮡt.Parallel();
    Ꮡt.Run(childˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            defer(expectParallelConflict, tΔ1, ref ᒐ);
            fn(tΔ1);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string grandChildˢ = "grand-child"u8;

internal static void testWithParallelGrandParentBefore(ж<testing.T> Ꮡt, Action<ж<testing.T>> fn) {
    Ꮡt.Parallel();
    Ꮡt.Run(childˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Run(grandChildˢ, (ж<testing.T> tΔ2) => {
            GoFrame ᒐ = default;
            try {
                defer(expectParallelConflict, tΔ2, ref ᒐ);
                fn(tΔ2);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goTestKey1ˢ = "GO_TEST_KEY_1"u8;
private static readonly @string valueˢ = "value"u8;

internal static void tSetenv(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(goTestKey1ˢ, valueˢ);
}

public static void TestSetenvWithParallelAfter(ж<testing.T> Ꮡt) {
    testWithParallelAfter(Ꮡt, tSetenv);
}

public static void TestSetenvWithParallelBefore(ж<testing.T> Ꮡt) {
    testWithParallelBefore(Ꮡt, tSetenv);
}

public static void TestSetenvWithParallelParentBefore(ж<testing.T> Ꮡt) {
    testWithParallelParentBefore(Ꮡt, tSetenv);
}

public static void TestSetenvWithParallelGrandParentBefore(ж<testing.T> Ꮡt) {
    testWithParallelGrandParentBefore(Ꮡt, tSetenv);
}

internal static void tChdir(ж<testing.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir());
}

public static void TestChdirWithParallelAfter(ж<testing.T> Ꮡt) {
    testWithParallelAfter(Ꮡt, tChdir);
}

public static void TestChdirWithParallelBefore(ж<testing.T> Ꮡt) {
    testWithParallelBefore(Ꮡt, tChdir);
}

public static void TestChdirWithParallelParentBefore(ж<testing.T> Ꮡt) {
    testWithParallelParentBefore(Ꮡt, tChdir);
}

public static void TestChdirWithParallelGrandParentBefore(ж<testing.T> Ꮡt) {
    testWithParallelGrandParentBefore(Ꮡt, tChdir);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string skipˢ = "skip"u8;
private static readonly @string pwdˢ = "PWD"u8;

[GoType("dyn")] internal partial struct TestChdir_type {
    internal @string name, dir, pwd;
    internal bool extraChdir;
}

public static void TestChdir(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (oldDir, err) = Δos.Getwd();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(Δos.Chdir, oldDir, ref ᒐ);
        // The "relative" test case relies on tmp not being a symlink.
        (var tmp, err) = filepath.EvalSymlinks(Ꮡt.TempDir());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var rel, err) = filepath.Rel(oldDir, tmp);
        if (err != default!) {
            // If GOROOT is on C: volume and tmp is on the D: volume, there
            // is no relative path between them, so skip that test case.
            rel = skipˢ;
        }
        foreach (var (_, vᴛ1) in new TestChdir_type[]{
            new(
                name: "absolute"u8,
                dir: tmp,
                pwd: tmp
            ),
            new(
                name: "relative"u8,
                dir: rel,
                pwd: tmp
            ),
            new(
                name: "current (absolute)"u8,
                dir: oldDir,
                pwd: oldDir
            ),
            new(
                name: "current (relative) with extra os.Chdir"u8,
                dir: "."u8,
                pwd: oldDir,
                extraChdir: true
            )
        }.slice()) {
            ref var tc = ref heap(new TestChdir_type(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
                if (tcʗ1.dir == "skip"u8) {
                    tΔ1.Skipf("skipping test because there is no relative path between %s and %s"u8, oldDir, tmp);
                }
                if (!filepath.IsAbs(tcʗ1.pwd)) {
                    tΔ1.Fatalf("Bad tc.pwd: %q (must be absolute)"u8, tcʗ1.pwd);
                }
                tΔ1.Chdir(tcʗ1.dir);
                var (newDirΔ1, errΔ1) = Δos.Getwd();
                if (errΔ1 != default!) {
                    tΔ1.Fatal(errΔ1);
                }
                if (newDirΔ1 != tcʗ1.pwd) {
                    tΔ1.Fatalf("failed to chdir to %q: getwd: got %q, want %q"u8, tcʗ1.dir, newDirΔ1, tcʗ1.pwd);
                }
                var exprᴛ1 = Δruntime.GOOS;
                if (exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8) {
                }
                else { /* default: */
                    {
                        @string pwd = Δos.Getenv(pwdˢ); if (pwd != tcʗ1.pwd) {
                            // Windows and Plan 9 do not use the PWD variable.
                            tΔ1.Fatalf("PWD: got %q, want %q"u8, pwd, tcʗ1.pwd);
                        }
                    }
                }

                if (tcʗ1.extraChdir) {
                    Δos.Chdir(".."u8);
                }
            });
            var (newDir, errΔ2) = Δos.Getwd();
            if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
            if (newDir != oldDir) {
                Ꮡt.Fatalf("failed to restore wd to %s: getwd: %s"u8, oldDir, newDir);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// testingTrueInInit is part of TestTesting.
internal static bool testingTrueInInit = false;

// testingTrueInPackageVarInit is part of TestTesting.
internal static bool testingTrueInPackageVarInit = testing.Testing();

// init is part of TestTesting.
[GoInit] internal static void init() {
    if (testing.Testing()) {
        testingTrueInInit = true;
    }
}

internal static @string testingProg = """

package main

import (
	"fmt"
	"testing"
)

func main() {
	fmt.Println(testing.Testing())
}

"""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingBuildingABinaryˢ = (@string)"skipping building a binary in short mode"u8;
private static readonly @string xGoˢ = "x.go"u8;
private static readonly @string runˢ = "run"u8;
private static readonly object falseˢ = (@string)"false"u8;

public static void TestTesting(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (!testing.Testing()) {
        Ꮡt.Errorf("testing.Testing() == %t, want %t"u8, testing.Testing(), true);
    }
    if (!testingTrueInInit) {
        Ꮡt.Errorf("testing.Testing() called by init function == %t, want %t"u8, testingTrueInInit, true);
    }
    if (!testingTrueInPackageVarInit) {
        Ꮡt.Errorf("testing.Testing() variable initialized as %t, want %t"u8, testingTrueInPackageVarInit, true);
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingBuildingABinaryˢ);
    }
    testenv.MustHaveGoRun(new testing_TжTB(Ꮡt));
    @string fn = filepath.Join(Ꮡt.TempDir(), xGoˢ);
    {
        var errΔ1 = Δos.WriteFile(fn, slice<byte>(testingProg), 420); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var cmd = testenv.Command(new testing_TжTB(Ꮡt), testenv.GoToolPath(new testing_TжTB(Ꮡt)), runˢ, fn);
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("%v failed: %v\n%s"u8, cmd.OrTypedNil(), err, @out);
    }
    @string s = ((@string)bytes.TrimSpace(@out));
    if (s != "false"u8) {
        Ꮡt.Errorf("in non-test testing.Test() returned %q, want %q"u8, s, falseˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testVˢ2 = "-test.v"u8;
private static readonly @string testParallel2ˢ = "-test.parallel=2"u8;
private static readonly @string testBenchtime2xˢ = "-test.benchtime=2x"u8;

// runTest runs a helper test with -test.v, ignoring its exit status.
// runTest both logs and returns the test output.
internal static slice<byte> runTest(ж<testing.T> Ꮡt, @string test) {
    Ꮡt.Helper();
    testenv.MustHaveExec(new testing_TжTB(Ꮡt));
    var cmd = testenv.Command(new testing_TжTB(Ꮡt), testenv.Executable(new testing_TжTB(Ꮡt)), "-test.run=^"u8 + test + "$"u8, "-test.bench=" + test, testVˢ2, testParallel2ˢ, testBenchtime2xˢ);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Value.Env = append((~cmd).Env, "GO_WANT_HELPER_PROCESS=1"u8);
    var (@out, err) = cmd.CombinedOutput();
    Ꮡt.Logf("%v: %v\n%s"u8, cmd.OrTypedNil(), err, @out);
    return @out;
}

// doRace provokes a data race that generates a race detector report if run
// under the race detector and is otherwise benign.
internal static partial void doRace() {
    nint x = default!;
    var c1 = new channel<bool>(0);
    var c1ʗ1 = c1;
    goǃ(() => {
        x = 1; // racy write
        c1ʗ1.ᐸꟷ(true);
    });
    _ = x; // racy read
    ᐸꟷ(c1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string subˢ2 = "Sub"u8;
private static readonly @string testRaceReportsˢ = "TestRaceReports"u8;

public static void TestRaceReports(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        // Generate a race detector report in a sub test.
        Ꮡt.Run(subˢ2, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            doRace();
        });
        return;
    }
    var @out = runTest(Ꮡt, testRaceReportsˢ);
    // We should see at most one race detector report.
    nint c = bytes.Count(@out, slice<byte>("race detected"u8));
    nint want = 0;
    if (race.Enabled) {
        want = 1;
    }
    if (c != want) {
        Ꮡt.Errorf("got %d race reports, want %d"u8, c, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testRaceNameˢ = "TestRaceName"u8;
private static readonly @string nameSˢ = @"=== NAME\s*$"u8;

// Issue #60083. This used to fail on the race builder.
public static void TestRaceName(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        doRace();
        return;
    }
    var @out = runTest(Ꮡt, testRaceNameˢ);
    if (Δregexp.MustCompile(nameSˢ).Match(@out)) {
        Ꮡt.Errorf("incorrectly reported test with no name"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string subsub1ˢ = "subsub1"u8;
private static readonly @string subsub2ˢ = "subsub2"u8;
private static readonly @string testRaceSubReportsˢ = "TestRaceSubReports"u8;

public static void TestRaceSubReports(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        Ꮡt.Parallel();
        var c1 = new channel<bool>(1);
        var c1ʗ1 = c1;
        Ꮡt.Run(subˢ, (ж<testing.T> tΔ1) => {
            var c1ʗ2 = c1ʗ1;
            tΔ1.Run(subsub1ˢ, (ж<testing.T> tΔ2) => {
                tΔ2.Parallel();
                doRace();
                c1ʗ2.ᐸꟷ(true);
            });
            var c1ʗ3 = c1ʗ1;
            tΔ1.Run(subsub2ˢ, (ж<testing.T> tΔ3) => {
                tΔ3.Parallel();
                doRace();
                ᐸꟷ(c1ʗ3);
            });
        });
        doRace();
        return;
    }
    var @out = runTest(Ꮡt, testRaceSubReportsˢ);
    // There should be three race reports: one for each subtest, and one for the
    // race after the subtests complete. Note that because the subtests run in
    // parallel, the race stacks may both be printed in with one or the other
    // test's logs.
    nint cReport = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));
    nint wantReport = 0;
    if (race.Enabled) {
        wantReport = 3;
    }
    if (cReport != wantReport) {
        Ꮡt.Errorf("got %d race reports, want %d"u8, cReport, wantReport);
    }
    // Regardless of when the stacks are printed, we expect each subtest to be
    // marked as failed, and that failure should propagate up to the parents.
    nint cFail = bytes.Count(@out, slice<byte>("--- FAIL:"u8));
    nint wantFail = 0;
    if (race.Enabled) {
        wantFail = 4;
    }
    if (cFail != wantFail) {
        Ꮡt.Errorf(@"got %d ""--- FAIL:"" lines, want %d"u8, cReport, wantReport);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testRaceInCleanupˢ = "TestRaceInCleanup"u8;

public static void TestRaceInCleanup(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        Ꮡt.Cleanup(doRace);
        Ꮡt.Parallel();
        Ꮡt.Run(subˢ, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
        });
        // No race should be reported for sub.
        return;
    }
    var @out = runTest(Ꮡt, testRaceInCleanupˢ);
    // There should be one race report, for the parent test only.
    nint cReport = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));
    nint wantReport = 0;
    if (race.Enabled) {
        wantReport = 1;
    }
    if (cReport != wantReport) {
        Ꮡt.Errorf("got %d race reports, want %d"u8, cReport, wantReport);
    }
    // Only the parent test should be marked as failed.
    // (The subtest does not race, and should pass.)
    nint cFail = bytes.Count(@out, slice<byte>("--- FAIL:"u8));
    nint wantFail = 0;
    if (race.Enabled) {
        wantFail = 1;
    }
    if (cFail != wantFail) {
        Ꮡt.Errorf(@"got %d ""--- FAIL:"" lines, want %d"u8, cReport, wantReport);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string subsubˢ = "subsub"u8;
private static readonly @string subsubsubˢ = "subsubsub"u8;
private static readonly @string testDeepSubtestRaceˢ = "TestDeepSubtestRace"u8;

public static void TestDeepSubtestRace(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        Ꮡt.Run(subˢ, (ж<testing.T> tΔ1) => {
            tΔ1.Run(subsubˢ, (ж<testing.T> tΔ2) => {
                tΔ2.Run(subsubsubˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ3) => {
                    doRace();
                });
            });
            doRace();
        });
        return;
    }
    var @out = runTest(Ꮡt, testDeepSubtestRaceˢ);
    nint c = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));
    nint want = 0;
    // There should be two race reports.
    if (race.Enabled) {
        want = 2;
    }
    if (c != want) {
        Ꮡt.Errorf("got %d race reports, want %d"u8, c, want);
    }
}

public static partial void TestRaceDuringParallelFailsAllSubtests(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        ref var ready = ref heap(new Δsync.WaitGroup(), out var Ꮡready);
        Ꮡready.Add(2);
        var done = new channel<EmptyStruct>(0);
        var doneʗ1 = done;
        goǃ(() => {
            Ꮡready.Wait();
            doRace(); // This race happens while both subtests are running.
            close(doneʗ1);
        });
        var doneʗ2 = done;
        Ꮡt.Run(subˢ, (ж<testing.T> tΔ1) => {
            var doneʗ3 = doneʗ2;
            tΔ1.Run(subsub1ˢ, (ж<testing.T> tΔ2) => {
                tΔ2.Parallel();
                Ꮡready.Done();
                ᐸꟷ(doneʗ3);
            });
            var doneʗ4 = doneʗ2;
            tΔ1.Run(subsub2ˢ, (ж<testing.T> tΔ3) => {
                tΔ3.Parallel();
                Ꮡready.Done();
                ᐸꟷ(doneʗ4);
            });
        });
        return;
    }
    var @out = runTest(Ꮡt, "TestRaceDuringParallelFailsAllSubtests"u8);
    nint c = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));
    nint want = 0;
    // Each subtest should report the race independently.
    if (race.Enabled) {
        want = 2;
    }
    if (c != want) {
        Ꮡt.Errorf("got %d race reports, want %d"u8, c, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testRaceBeforeParallelˢ = "TestRaceBeforeParallel"u8;

public static void TestRaceBeforeParallel(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        Ꮡt.Run(subˢ, (ж<testing.T> tΔ1) => {
            doRace();
            tΔ1.Parallel();
        });
        return;
    }
    var @out = runTest(Ꮡt, testRaceBeforeParallelˢ);
    nint c = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));
    nint want = 0;
    // We should see one race detector report.
    if (race.Enabled) {
        want = 1;
    }
    if (c != want) {
        Ꮡt.Errorf("got %d race reports, want %d"u8, c, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testRunˢ2 = "-test.run=^$"u8;

public static void TestRaceBeforeTests(ж<testing.T> Ꮡt) {
    var cmd = testenv.Command(new testing_TжTB(Ꮡt), testenv.Executable(new testing_TжTB(Ꮡt)), testRunˢ2);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Value.Env = append((~cmd).Env, "GO_WANT_RACE_BEFORE_TESTS=1"u8);
    var (@out, _) = cmd.CombinedOutput();
    Ꮡt.Logf("%s"u8, @out);
    nint c = bytes.Count(@out, slice<byte>("race detected outside of test execution"u8));
    nint want = 0;
    if (race.Enabled) {
        want = 1;
    }
    if (c != want) {
        Ꮡt.Errorf("got %d race reports; want %d"u8, c, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string benchmarkRacyˢ = "BenchmarkRacy"u8;

public static void TestBenchmarkRace(ж<testing.T> Ꮡt) {
    var @out = runTest(Ꮡt, benchmarkRacyˢ);
    nint c = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));
    nint want = 0;
    // We should see one race detector report.
    if (race.Enabled) {
        want = 1;
    }
    if (c != want) {
        Ꮡt.Errorf("got %d race reports; want %d"u8, c, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string benchmarkBLoopRacyˢ = "BenchmarkBLoopRacy"u8;

public static void TestBenchmarkRaceBLoop(ж<testing.T> Ꮡt) {
    var @out = runTest(Ꮡt, benchmarkBLoopRacyˢ);
    nint c = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));
    nint want = 0;
    // We should see one race detector report.
    if (race.Enabled) {
        want = 1;
    }
    if (c != want) {
        Ꮡt.Errorf("got %d race reports; want %d"u8, c, want);
    }
}

public static void BenchmarkRacy(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (Δos.Getenv(goWantHelperProcessˢ) != "1"u8) {
        Ꮡb.Skipf("skipping intentionally-racy benchmark"u8);
    }
    for (nint i = 0; i < b.N; i++) {
        doRace();
    }
}

public static void BenchmarkBLoopRacy(ж<testing.B> Ꮡb) {
    if (Δos.Getenv(goWantHelperProcessˢ) != "1"u8) {
        Ꮡb.Skipf("skipping intentionally-racy benchmark"u8);
    }
    while (Ꮡb.Loop()) {
        doRace();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string benchmarkSubRacyˢ = "BenchmarkSubRacy"u8;

public static void TestBenchmarkSubRace(ж<testing.T> Ꮡt) {
    var @out = runTest(Ꮡt, benchmarkSubRacyˢ);
    nint c = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));
    nint want = 0;
    // We should see 3 race detector reports:
    // one in the sub-bencmark, one in the parent afterward,
    // and one in b.Loop.
    if (race.Enabled) {
        want = 3;
    }
    if (c != want) {
        Ꮡt.Errorf("got %d race reports; want %d"u8, c, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nonRacyˢ = "non-racy"u8;
private static readonly @string racyˢ = "racy"u8;
private static readonly @string racyBLoopˢ = "racy-bLoop"u8;

public static void BenchmarkSubRacy(ж<testing.B> Ꮡb) {
    if (Δos.Getenv(goWantHelperProcessˢ) != "1"u8) {
        Ꮡb.Skipf("skipping intentionally-racy benchmark"u8);
    }
    Ꮡb.Run(nonRacyˢ, (ж<testing.B> bΔ1) => {
        nint tot = 0;
        for (nint i = 0; i < (~bΔ1).N; i++) {
            tot++;
        }
        _ = tot;
    });
    Ꮡb.Run(racyˢ, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            doRace();
        }
    });
    Ꮡb.Run(racyBLoopˢ, (ж<testing.B> bΔ3) => {
        while (bΔ3.Loop()) {
            doRace();
        }
    });
    doRace(); // should be reported separately
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testParallel4ˢ = "-test.parallel=4"u8;

public static void TestRunningTests(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    // Regression test for https://go.dev/issue/64404:
    // on timeout, the "running tests" message should not include
    // tests that are waiting on parked subtests.
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        for (nint i = 0; i < 2; i++) {
            Ꮡt.Run(fmt.Sprintf("outer%d"u8, i), (ж<testing.T> tΔ1) => {
                tΔ1.Parallel();
                for (nint j = 0; j < 2; j++) {
                    tΔ1.Run(fmt.Sprintf("inner%d"u8, j), (ж<testing.T> tΔ2) => {
                        tΔ2.Parallel();
                        while (ᐧ) {
                            time.Sleep(1 * time.Millisecond);
                        }
                    });
                }
            });
        }
    }
    var timeout = 10 * time.Millisecond;
    while (ᐧ) {
        var cmd = testenv.Command(new testing_TжTB(Ꮡt), Δos.Args[0], "-test.run=^"u8 + Ꮡt.Name() + "$"u8, "-test.timeout=" + timeout.String(), testParallel4ˢ);
        cmd.Value.Env = append(cmd.Environ(), "GO_WANT_HELPER_PROCESS=1"u8);
        var (@out, err) = cmd.CombinedOutput();
        Ꮡt.Logf("%v:\n%s"u8, cmd.OrTypedNil(), @out);
        {
            var (_, okΔ1) = err._<ж<exec.ExitError>>(ᐧ); if (!okΔ1) {
                Ꮡt.Fatal(err);
            }
        }
        // Because the outer subtests (and TestRunningTests itself) are marked as
        // parallel, their test functions return (and are no longer “running”)
        // before the inner subtests are released to run and hang.
        // Only those inner subtests should be reported as running.
        var want = new @string[]{
            "TestRunningTests/outer0/inner0"u8,
            "TestRunningTests/outer0/inner1"u8,
            "TestRunningTests/outer1/inner0"u8,
            "TestRunningTests/outer1/inner1"u8
        }.slice();
        var (got, ok) = parseRunningTests(@out);
        if (slices.Equal<slice<@string>, @string>(got, want)) {
            break;
        }
        if (ok){
            Ꮡt.Logf("found running tests:\n%s\nwant:\n%s"u8, strings.Join(got, "\n"u8), strings.Join(want, "\n"u8));
        } else {
            Ꮡt.Logf("no running tests found"u8);
        }
        Ꮡt.Logf("retrying with longer timeout"u8);
        timeout *= 2;
    }
}

public static void TestRunningTestsInCleanup(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        for (nint i = 0; i < 2; i++) {
            Ꮡt.Run(fmt.Sprintf("outer%d"u8, i), (ж<testing.T> tΔ1) => {
                // Not parallel: we expect to see only one outer test,
                // stuck in cleanup after its subtest finishes.
                tΔ1.Cleanup(() => {
                    while (ᐧ) {
                        time.Sleep(1 * time.Millisecond);
                    }
                });
                for (nint j = 0; j < 2; j++) {
                    tΔ1.Run(fmt.Sprintf("inner%d"u8, j), (ж<testing.T> tΔ2) => {
                        tΔ2.Parallel();
                    });
                }
            });
        }
    }
    var timeout = 10 * time.Millisecond;
    while (ᐧ) {
        var cmd = testenv.Command(new testing_TжTB(Ꮡt), Δos.Args[0], "-test.run=^"u8 + Ꮡt.Name() + "$"u8, "-test.timeout=" + timeout.String());
        cmd.Value.Env = append(cmd.Environ(), "GO_WANT_HELPER_PROCESS=1"u8);
        var (@out, err) = cmd.CombinedOutput();
        Ꮡt.Logf("%v:\n%s"u8, cmd.OrTypedNil(), @out);
        {
            var (_, okΔ1) = err._<ж<exec.ExitError>>(ᐧ); if (!okΔ1) {
                Ꮡt.Fatal(err);
            }
        }
        // TestRunningTestsInCleanup is blocked in the call to t.Run,
        // but its test function has not yet returned so it should still
        // be considered to be running.
        // outer1 hasn't even started yet, so only outer0 and the top-level
        // test function should be reported as running.
        var want = new @string[]{
            "TestRunningTestsInCleanup"u8,
            "TestRunningTestsInCleanup/outer0"u8
        }.slice();
        var (got, ok) = parseRunningTests(@out);
        if (slices.Equal<slice<@string>, @string>(got, want)) {
            break;
        }
        if (ok){
            Ꮡt.Logf("found running tests:\n%s\nwant:\n%s"u8, strings.Join(got, "\n"u8), strings.Join(want, "\n"u8));
        } else {
            Ꮡt.Logf("no running tests found"u8);
        }
        Ꮡt.Logf("retrying with longer timeout"u8);
        timeout *= 2;
    }
}

internal static (slice<@string> runningTests, bool ok) parseRunningTests(slice<byte> @out) {
    slice<@string> runningTests = default!;

    var inRunningTests = false;
    foreach (var (_, line) in strings.Split(((@string)@out), "\n"u8)) {
        if (inRunningTests) {
            // Package testing adds one tab, the panic printer adds another.
            {
                var (trimmed, okΔ1) = strings.CutPrefix(line, "\t\t"u8); if (okΔ1) {
                    {
                        var (name, _, okΔ2) = strings.Cut(trimmed, " "u8); if (okΔ2) {
                            runningTests = append(runningTests, name);
                            continue;
                        }
                    }
                }
            }
            // This line is not the name of a running test.
            return (runningTests, true);
        }
        if (strings.TrimSpace(line) == "running tests:"u8) {
            inRunningTests = true;
        }
    }
    return (default!, false);
}

public static partial void TestConcurrentRun(ж<testing.T> Ꮡt) {
    // Regression test for https://go.dev/issue/64402:
    // this deadlocked after https://go.dev/cl/506755.
    var block = new channel<EmptyStruct>(0);
    ref var ready = ref heap(new Δsync.WaitGroup(), out var Ꮡready);
    ref var done = ref heap(new Δsync.WaitGroup(), out var Ꮡdone);
    for (nint i = 0; i < 2; i++) {
        Ꮡready.Add(1);
        Ꮡdone.Add(1);
        var blockʗ1 = block;
        goǃ((ᴛ1, ᴛ2) => Ꮡt.Run(ᴛ1, ᴛ2), (@string)"", (ж<testing.T> _) => {
            Ꮡready.Done();
            ᐸꟷ(blockʗ1);
            Ꮡdone.Done();
        });
    }
    Ꮡready.Wait();
    close(block);
    Ꮡdone.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string outerˢ = "outer"u8;
private static readonly object helloOuterˢ = (@string)"Hello outer!"u8;
private static readonly @string notInnerˢ = "not_inner"u8;
private static readonly object helloInnerˢ = (@string)"Hello inner!"u8;

public static void TestParentRun(ж<testing.T> Ꮡt1) {
    ref var t1 = ref Ꮡt1.DerefOrNull();

    // Regression test for https://go.dev/issue/64402:
    // this deadlocked after https://go.dev/cl/506755.
    Ꮡt1.Run(outerˢ, (ж<testing.T> t2) => {
        t2.Log(helloOuterˢ);
        Ꮡt1.Run(notInnerˢ, (ж<testing.T> t3) => {
            // Note: this is t1.Run, not t2.Run.
            t3.Log(helloInnerˢ);
        });
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string innerˢ = "inner"u8;
private static readonly @string inner2ˢ = "inner2"u8;
private static readonly object expectedContextOfSiblingˢ = (@string)"expected context of sibling test to be canceled after its test function finished"u8;
private static readonly object expectedContextCanceledˢ = (@string)"expected context canceled before cleanup"u8;

public static void TestContext(ж<testing.T> Ꮡt) {
    var ctx = Ꮡt.Context();
    {
        var err = ctx.Err(); if (err != default!) {
            Ꮡt.Fatalf("expected non-canceled context, got %v"u8, err);
        }
    }
    ref var innerCtx = ref heap<context.Context>(out var ᏑinnerCtx);
    Ꮡt.Run(innerˢ, (ж<testing.T> tΔ1) => {
        ᏑinnerCtx.ValueSlot = tΔ1.Context();
        {
            var err = ᏑinnerCtx.ValueSlot.Err(); if (err != default!) {
                tΔ1.Fatalf("expected inner test to not inherit canceled context, got %v"u8, err);
            }
        }
    });
    Ꮡt.Run(inner2ˢ, (ж<testing.T> tΔ2) => {
        if (!errors.Is(ᏑinnerCtx.ValueSlot.Err(), context.Canceled)) {
            tΔ2.Fatal(expectedContextOfSiblingˢ);
        }
    });
    var ctxʗ1 = ctx;
    Ꮡt.Cleanup(() => {
        if (!errors.Is(ctxʗ1.Err(), context.Canceled)) {
            Ꮡt.Fatal(expectedContextCanceledˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string benchmarkBLoopPrintˢ = "BenchmarkBLoopPrint"u8;
private static readonly @string benchmarkBLoopPrint09S2Sˢ = @"BenchmarkBLoopPrint(-[0-9]+)?\s+2\s+[0-9]+\s+ns/op"u8;
private static readonly object missingBenchmarkOutputˢ = (@string)"missing benchmark output"u8;

public static void TestBenchmarkBLoopIterationCorrect(ж<testing.T> Ꮡt) {
    var @out = runTest(Ꮡt, benchmarkBLoopPrintˢ);
    nint c = bytes.Count(@out, slice<byte>("Printing from BenchmarkBLoopPrint"u8));
    nint want = 2;
    if (c != want) {
        Ꮡt.Errorf("got %d loop iterations; want %d"u8, c, want);
    }
    // b.Loop() will only rampup once.
    c = bytes.Count(@out, slice<byte>("Ramping up from BenchmarkBLoopPrint"u8));
    want = 1;
    if (c != want) {
        Ꮡt.Errorf("got %d loop rampup; want %d"u8, c, want);
    }
    var re = Δregexp.MustCompile(benchmarkBLoopPrint09S2Sˢ);
    if (!re.Match(@out)) {
        Ꮡt.Error(missingBenchmarkOutputˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string benchmarkBNPrintˢ = "BenchmarkBNPrint"u8;

public static void TestBenchmarkBNIterationCorrect(ж<testing.T> Ꮡt) {
    var @out = runTest(Ꮡt, benchmarkBNPrintˢ);
    nint c = bytes.Count(@out, slice<byte>("Printing from BenchmarkBNPrint"u8));
    // runTest sets benchtime=2x, with semantics specified in #32051 it should
    // run 3 times.
    nint want = 3;
    if (c != want) {
        Ꮡt.Errorf("got %d loop iterations; want %d"u8, c, want);
    }
    // b.N style fixed iteration loop will rampup twice:
    // One in run1(), the other in launch
    c = bytes.Count(@out, slice<byte>("Ramping up from BenchmarkBNPrint"u8));
    want = 2;
    if (c != want) {
        Ꮡt.Errorf("got %d loop rampup; want %d"u8, c, want);
    }
}

public static void BenchmarkBLoopPrint(ж<testing.B> Ꮡb) {
    Ꮡb.Logf("Ramping up from BenchmarkBLoopPrint"u8);
    while (Ꮡb.Loop()) {
        Ꮡb.Logf("Printing from BenchmarkBLoopPrint"u8);
    }
}

public static void BenchmarkBNPrint(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    Ꮡb.Logf("Ramping up from BenchmarkBNPrint"u8);
    for (nint i = 0; i < b.N; i++) {
        Ꮡb.Logf("Printing from BenchmarkBNPrint"u8);
    }
}

} // end testing_test_package
