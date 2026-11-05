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
using iter = iter_package;
using path;
using ꓸꓸꓸstring = Span<@string>;

partial class testing_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string goWantRaceBeforeTestsˢ => ref goWantRaceBeforeTestsˢᵇ;
private static readonly @string goWantRaceBeforeTestsˢᵇ = "GO_WANT_RACE_BEFORE_TESTS"u8;

// This is exactly what a test would do without a TestMain.
// It's here only so that there is at least one package in the
// standard library with a TestMain, so that code is executed.
public static void TestMain(ж<testing.M> m) {
    if (Δos.Getenv(goWantRaceBeforeTestsˢ) == "1"u8) {
        doRace();
    }

    m.Run();

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
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "test"u8;

public static void TestTempDirInCleanup(ж<testing.T> t) {
    @string dir = default;

    t.Run(testˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Cleanup(() => {
            dir = tΔ1.TempDir();
        });
        _ = tΔ1.TempDir();
    });

    var (fi, err) = Δos.Stat(dir);
    if (fi != nil) {
        t.Fatalf("Directory %q from user Cleanup still exists", dir);
    }
    if (!Δos.IsNotExist(err)) {
        t.Fatalf("Unexpected error: %v", err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object subTestFailureInAˢ = (@string)"Sub test failure in a benchmark"u8;

public static void TestTempDirInBenchmark(ж<testing.T> t) {
    testing.Benchmark((ж<testing.B> b) => {
        if (!b.Run(testˢ, (ж<testing.B> bΔ1) => {
            // Add a loop so that the test won't fail. See issue 38677.
            for (nint i = 0; i < (~bΔ1).N; i++) {
                _ = bΔ1.TempDir();
            }
        })) {
            t.Fatal(subTestFailureInAˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string inSubtestˢ => ref inSubtestˢᵇ;
private static readonly @string inSubtestˢᵇ = "InSubtest"u8;
private static ref readonly @string testSubtestˢ => ref testSubtestˢᵇ;
private static readonly @string testSubtestˢᵇ = "test/subtest"u8;
private static ref readonly @string testSubtestˢ2 => ref testSubtestˢ2ᵇ;
private static readonly @string testSubtestˢ2ᵇ = "test\\subtest"u8;
private static ref readonly @string testSubtestˢ3 => ref testSubtestˢ3ᵇ;
private static readonly @string testSubtestˢ3ᵇ = "test:subtest"u8;
private static ref readonly @string testˢ2 => ref testˢ2ᵇ;
private static readonly @string testˢ2ᵇ = "test/.."u8;
private static ref readonly @string testˢ3 => ref testˢ3ᵇ;
private static readonly @string testˢ3ᵇ = "../test"u8;
private static ref readonly @string testˢ4 => ref testˢ4ᵇ;
private static readonly @string testˢ4ᵇ = "test[]"u8;
private static ref readonly @string testˢ5 => ref testˢ5ᵇ;
private static readonly @string testˢ5ᵇ = "test*"u8;

public static void TestTempDir(ж<testing.T> t) {
    testTempDir(t);
    t.Run(inSubtestˢ, testTempDir);
    t.Run(testSubtestˢ, testTempDir);
    t.Run(testSubtestˢ2, testTempDir);
    t.Run(testSubtestˢ3, testTempDir);
    t.Run(testˢ2, testTempDir);
    t.Run(testˢ3, testTempDir);
    t.Run(testˢ4, testTempDir);
    t.Run(testˢ5, testTempDir);
    t.Run("äöüéè", testTempDir);
    t.Run(strings.Repeat("a", 300), testTempDir);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object neverReceivedDirChannelˢ = (@string)"never received dir channel"u8;
private static readonly object expectedDirˢ = (@string)"expected dir"u8;
private static readonly object subsequentCallsToTempDirˢ = (@string)"subsequent calls to TempDir returned the same directory"u8;
private static ref readonly @string txtˢ => ref txtˢᵇ;
private static readonly @string txtˢᵇ = "*.txt"u8;

internal static void testTempDir(ж<testing.T> t) {
    var dirCh = new channel<@string>(1);
    var dirChʗ1 = dirCh;
    t.Cleanup(() => {
        // Verify directory has been removed.
        var selᴛ1 = dirChʗ1;
        switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out var dirΔ1): {
            var (fiΔ1, errΔ1) = Δos.Stat(dirΔ1);
            if (Δos.IsNotExist(errΔ1)) {
                // All good
                return;
            }
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            t.Errorf("directory %q still exists: %v, isDir=%v", dirΔ1, fiΔ1, fiΔ1.IsDir());
            break;
        }
        default: {
            if (!t.Failed()) {
                t.Fatal(neverReceivedDirChannelˢ);
            }
            break;
        }}
    });

    @string dir = t.TempDir();
    if (dir == ""u8) {
        t.Fatal(expectedDirˢ);
    }
    @string dir2 = t.TempDir();
    if (dir == dir2) {
        t.Fatal(subsequentCallsToTempDirˢ);
    }
    if (filepath.Dir(dir) != filepath.Dir(dir2)) {
        t.Fatalf("calls to TempDir do not share a parent; got %q, %q", dir, dir2);
    }
    dirCh.ᐸꟷ(dir);
    var (fi, err) = Δos.Stat(dir);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!fi.IsDir()) {
        t.Errorf("dir %q is not a dir", dir);
    }
    (var files, err) = Δos.ReadDir(dir);
    if (err != nil) {
        t.Fatal(err);
    }
    if (len(files) > 0) {
        t.Errorf("unexpected %d files in TempDir: %v", len(files), files);
    }

    @string glob = filepath.Join(dir, txtˢ);
    {
        var (_, errΔ2) = filepath.Glob(glob); if (errΔ2 != nil) {
            t.Error(errΔ2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string customGotmpdirTestˢ => ref customGotmpdirTestˢᵇ;
private static readonly @string customGotmpdirTestˢᵇ = "custom-gotmpdir-test"u8;
private static ref readonly @string gotmpdirˢ => ref gotmpdirˢᵇ;
private static readonly @string gotmpdirˢᵇ = "GOTMPDIR"u8;

public static void TestTempDirGOTMPDIR(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // The first call to t.TempDir will create a parent temporary directory
        // that will contain all temporary directories created by TempDir.
        //
        // Use os.TempDir (not t.TempDir) to get a temporary directory,
        // set GOTMPDIR to that directory,
        // and then verify that t.TempDir creates a directory in GOTMPDIR.
        @string customTmpDir = filepath.Join(Δos.TempDir(), customGotmpdirTestˢ);
        {
            var errΔ1 = Δos.MkdirAll(customTmpDir, 511); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }
        defer(Δos.RemoveAll, customTmpDir, ref ᒐ);

        t.Setenv(gotmpdirˢ, customTmpDir);

        @string dir = t.TempDir();
        if (dir == ""u8) {
            t.Fatal(expectedDirˢ);
        }

        if (!strings.HasPrefix(dir, customTmpDir)) {
            t.Errorf("TempDir did not use GOTMPDIR: got %q, want prefix %q", dir, customTmpDir);
        }

        var (fi, err) = Δos.Stat(dir);
        if (err != nil) {
            t.Fatal(err);
        }
        if (!fi.IsDir()) {
            t.Errorf("dir %q is not a dir", dir);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestSetenv_tests /*dyn*/ {
    internal @string name;
    internal @string key;
    internal bool initialValueExists;
    internal @string initialValue;
    internal @string newValue;
}

public static void TestSetenv(ж<testing.T> t) {
    var tests = slice<TestSetenv_tests>([
        new(
            name: "initial value exists",
            key: "GO_TEST_KEY_1",
            initialValueExists: true,
            initialValue: "111",
            newValue: "222"
        ),
        new(
            name: "initial value exists but empty",
            key: "GO_TEST_KEY_2",
            initialValueExists: true,
            initialValue: "",
            newValue: "222"
        ),
        new(
            name: "initial value is not exists",
            key: "GO_TEST_KEY_3",
            initialValueExists: false,
            initialValue: "",
            newValue: "222"
        )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new TestSetenv_tests(), out var Ꮡtest);
        test = vᴛ1;

        if (test.initialValueExists) {
            {
                var err = Δos.Setenv(test.key, test.initialValue); if (err != nil) {
                    t.Fatalf("unable to set env: got %v", err);
                }
            }
        } else {
            Δos.Unsetenv(test.key);
        }

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            tΔ1.Setenv(testʗ1.key, testʗ1.newValue);
            if (Δos.Getenv(testʗ1.key) != testʗ1.newValue) {
                tΔ1.Fatalf("unexpected value after t.Setenv: got %s, want %s", Δos.Getenv(testʗ1.key), testʗ1.newValue);
            }
        });

        var (got, exists) = Δos.LookupEnv(test.key);
        if (got != test.initialValue) {
            t.Fatalf("unexpected value after t.Setenv cleanup: got %s, want %s", got, test.initialValue);
        }
        if (exists != test.initialValueExists) {
            t.Fatalf("unexpected value after t.Setenv cleanup: got %t, want %t", exists, test.initialValueExists);
        }
    }
}

internal static void expectParallelConflict(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?

        @string want = testing.ParallelConflict;
        {
            var got = recover(calledByDefer); if (!AreEqual(got, want)) {
                t.Fatalf("expected panic; got %#v want %q", got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testWithParallelAfter(ж<testing.T> t, Action<ж<testing.T>> fn) {
    GoFrame ᒐ = default;
    try {
        defer(expectParallelConflict, t, ref ᒐ, recovers: true);

        fn(t);
        t.Parallel();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testWithParallelBefore(ж<testing.T> t, Action<ж<testing.T>> fn) {
    GoFrame ᒐ = default;
    try {
        defer(expectParallelConflict, t, ref ᒐ, recovers: true);

        t.Parallel();
        fn(t);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string childˢ => ref childˢᵇ;
private static readonly @string childˢᵇ = "child"u8;

internal static void testWithParallelParentBefore(ж<testing.T> t, Action<ж<testing.T>> fn) {
    t.Parallel();

    t.Run(childˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            defer(expectParallelConflict, tΔ1, ref ᒐ, recovers: true);

            fn(tΔ1);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string grandChildˢ => ref grandChildˢᵇ;
private static readonly @string grandChildˢᵇ = "grand-child"u8;

internal static void testWithParallelGrandParentBefore(ж<testing.T> t, Action<ж<testing.T>> fn) {
    t.Parallel();

    t.Run(childˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Run(grandChildˢ, (ж<testing.T> tΔ2) => {
            GoFrame ᒐ = default;
            try {
                defer(expectParallelConflict, tΔ2, ref ᒐ, recovers: true);

                fn(tΔ2);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string goTestKey1ˢ => ref goTestKey1ˢᵇ;
private static readonly @string goTestKey1ˢᵇ = "GO_TEST_KEY_1"u8;
private static ref readonly @string valueˢ => ref valueˢᵇ;
private static readonly @string valueˢᵇ = "value"u8;

internal static void tSetenv(ж<testing.T> t) {
    t.Setenv(goTestKey1ˢ, valueˢ);
}

public static void TestSetenvWithParallelAfter(ж<testing.T> t) {
    testWithParallelAfter(t, tSetenv);
}

public static void TestSetenvWithParallelBefore(ж<testing.T> t) {
    testWithParallelBefore(t, tSetenv);
}

public static void TestSetenvWithParallelParentBefore(ж<testing.T> t) {
    testWithParallelParentBefore(t, tSetenv);
}

public static void TestSetenvWithParallelGrandParentBefore(ж<testing.T> t) {
    testWithParallelGrandParentBefore(t, tSetenv);
}

internal static void tChdir(ж<testing.T> t) {
    t.Chdir(t.TempDir());
}

public static void TestChdirWithParallelAfter(ж<testing.T> t) {
    testWithParallelAfter(t, tChdir);
}

public static void TestChdirWithParallelBefore(ж<testing.T> t) {
    testWithParallelBefore(t, tChdir);
}

public static void TestChdirWithParallelParentBefore(ж<testing.T> t) {
    testWithParallelParentBefore(t, tChdir);
}

public static void TestChdirWithParallelGrandParentBefore(ж<testing.T> t) {
    testWithParallelGrandParentBefore(t, tChdir);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string skipˢ => ref skipˢᵇ;
private static readonly @string skipˢᵇ = "skip"u8;
private static ref readonly @string pwdˢ => ref pwdˢᵇ;
private static readonly @string pwdˢᵇ = "PWD"u8;

internal partial struct TestChdir_type /*dyn*/ {
    internal @string name, dir, pwd;
    internal bool extraChdir;
}

public static void TestChdir(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (oldDir, err) = Δos.Getwd();
        if (err != nil) {
            t.Fatal(err);
        }
        defer(Δos.Chdir, oldDir, ref ᒐ);

        // The "relative" test case relies on tmp not being a symlink.
        (var tmp, err) = filepath.EvalSymlinks(t.TempDir());
        if (err != nil) {
            t.Fatal(err);
        }
        (var rel, err) = filepath.Rel(oldDir, tmp);
        if (err != nil) {
            // If GOROOT is on C: volume and tmp is on the D: volume, there
            // is no relative path between them, so skip that test case.
            rel = skipˢ;
        }

        foreach (var (_, vᴛ1) in range(slice<TestChdir_type>([
            new(
                name: "absolute",
                dir: tmp,
                pwd: tmp
            ),
            new(
                name: "relative",
                dir: rel,
                pwd: tmp
            ),
            new(
                name: "current (absolute)",
                dir: oldDir,
                pwd: oldDir
            ),
            new(
                name: "current (relative) with extra os.Chdir",
                dir: ".",
                pwd: oldDir,

                extraChdir: true
            )
        ]))) {
            ref var tc = ref heap(new TestChdir_type(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            t.Run(tc.name, (ж<testing.T> tΔ1) => {
                if (tcʗ1.dir == "skip"u8) {
                    tΔ1.Skipf("skipping test because there is no relative path between %s and %s", oldDir, tmp);
                }
                if (!filepath.IsAbs(tcʗ1.pwd)) {
                    tΔ1.Fatalf("Bad tc.pwd: %q (must be absolute)", tcʗ1.pwd);
                }

                tΔ1.Chdir(tcʗ1.dir);

                var (newDirΔ1, errΔ1) = Δos.Getwd();
                if (errΔ1 != nil) {
                    tΔ1.Fatal(errΔ1);
                }
                if (newDirΔ1 != tcʗ1.pwd) {
                    tΔ1.Fatalf("failed to chdir to %q: getwd: got %q, want %q", tcʗ1.dir, newDirΔ1, tcʗ1.pwd);
                }

                var exprᴛ1 = Δruntime.GOOS;
                switch (ᐧ) {
                case {} when exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8: {
                    // Windows and Plan 9 do not use the PWD variable.
                    break;
                }
                default: {
                    {
                        @string pwd = Δos.Getenv(pwdˢ); if (pwd != tcʗ1.pwd) {
                            tΔ1.Fatalf("PWD: got %q, want %q", pwd, tcʗ1.pwd);
                        }
                    }
                    break;
                }}

                if (tcʗ1.extraChdir) {
                    Δos.Chdir("..");
                }
            });

            var (newDir, errΔ2) = Δos.Getwd();
            if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }
            if (newDir != oldDir) {
                t.Fatalf("failed to restore wd to %s: getwd: %s", oldDir, newDir);
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

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingBuildingABinaryˢ = (@string)"skipping building a binary in short mode"u8;
private static ref readonly @string xGoˢ => ref xGoˢᵇ;
private static readonly @string xGoˢᵇ = "x.go"u8;
private static ref readonly @string runˢ => ref runˢᵇ;
private static readonly @string runˢᵇ = "run"u8;
private static readonly object falseˢ = (@string)"false"u8;

public static void TestTesting(ж<testing.T> t) {
    if (!testing.Testing()) {
        t.Errorf("testing.Testing() == %t, want %t", testing.Testing(), true);
    }
    if (!testingTrueInInit) {
        t.Errorf("testing.Testing() called by init function == %t, want %t", testingTrueInInit, true);
    }
    if (!testingTrueInPackageVarInit) {
        t.Errorf("testing.Testing() variable initialized as %t, want %t", testingTrueInPackageVarInit, true);
    }

    if (testing.Short()) {
        t.Skip(skippingBuildingABinaryˢ);
    }
    testenv.MustHaveGoRun(new testing_TжTB(t));

    @string fn = filepath.Join(t.TempDir(), xGoˢ);
    {
        var errΔ1 = Δos.WriteFile(fn, slice<byte>(testingProg), 420); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }

    var cmd = testenv.Command(new testing_TжTB(t), testenv.GoToolPath(new testing_TжTB(t)), runˢ, fn);
    var (@out, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("%v failed: %v\n%s", cmd.OrTypedNil(), err, @out);
    }

    @string s = (@string)bytes.TrimSpace(@out);
    if (s != "false"u8) {
        t.Errorf("in non-test testing.Test() returned %q, want %q", s, falseˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testVˢ2 => ref testVˢ2ᵇ;
private static readonly @string testVˢ2ᵇ = "-test.v"u8;
private static ref readonly @string testParallel2ˢ => ref testParallel2ˢᵇ;
private static readonly @string testParallel2ˢᵇ = "-test.parallel=2"u8;
private static ref readonly @string testBenchtime2xˢ => ref testBenchtime2xˢᵇ;
private static readonly @string testBenchtime2xˢᵇ = "-test.benchtime=2x"u8;

// runTest runs a helper test with -test.v, ignoring its exit status.
// runTest both logs and returns the test output.
internal static slice<byte> runTest(ж<testing.T> t, @string test, params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    t.Helper();

    testenv.MustHaveExec(new testing_TжTB(t));

    var cmd = testenv.Command(new testing_TжTB(t), testenv.Executable(new testing_TжTB(t)), "-test.run=^"u8 + test + "$"u8, "-test.bench=" + test, testVˢ2, testParallel2ˢ, testBenchtime2xˢ);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Env = append(cmd.Env, (@string)"GO_WANT_HELPER_PROCESS=1");
    cmd.Args = appendꓸꓸꓸ(cmd.Args, args);
    var (@out, err) = cmd.CombinedOutput();
    t.Logf("%v: %v\n%s", cmd.OrTypedNil(), err, @out);

    return @out;
}

// doRace provokes a data race that generates a race detector report if run
// under the race detector and is otherwise benign.
internal static partial void doRace() {
    nint x = default;
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
private static ref readonly @string subˢ2 => ref subˢ2ᵇ;
private static readonly @string subˢ2ᵇ = "Sub"u8;
private static ref readonly @string testRaceReportsˢ => ref testRaceReportsˢᵇ;
private static readonly @string testRaceReportsˢᵇ = "TestRaceReports"u8;

public static void TestRaceReports(ж<testing.T> t) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        // Generate a race detector report in a sub test.
        t.Run(subˢ2, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            doRace();
        });
        return;
    }

    var @out = runTest(t, testRaceReportsˢ);

    // We should see at most one race detector report.
    nint c = bytes.Count(@out, slice<byte>("race detected"u8));
    nint want = 0;
    if (race.Enabled) {
        want = 1;
    }
    if (c != want) {
        t.Errorf("got %d race reports, want %d", c, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testRaceNameˢ => ref testRaceNameˢᵇ;
private static readonly @string testRaceNameˢᵇ = "TestRaceName"u8;
private static ref readonly @string nameSˢ => ref nameSˢᵇ;
private static readonly @string nameSˢᵇ = @"=== NAME\s*$"u8;

// Issue #60083. This used to fail on the race builder.
public static void TestRaceName(ж<testing.T> t) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        doRace();
        return;
    }

    var @out = runTest(t, testRaceNameˢ);

    if (Δregexp.MustCompile(nameSˢ).Match(@out)) {
        t.Errorf("incorrectly reported test with no name");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string subsub1ˢ => ref subsub1ˢᵇ;
private static readonly @string subsub1ˢᵇ = "subsub1"u8;
private static ref readonly @string subsub2ˢ => ref subsub2ˢᵇ;
private static readonly @string subsub2ˢᵇ = "subsub2"u8;
private static ref readonly @string testRaceSubReportsˢ => ref testRaceSubReportsˢᵇ;
private static readonly @string testRaceSubReportsˢᵇ = "TestRaceSubReports"u8;

public static void TestRaceSubReports(ж<testing.T> t) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        t.Parallel();
        var c1 = new channel<bool>(1);
        var c1ʗ1 = c1;
        t.Run(subˢ, (ж<testing.T> tΔ1) => {
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

    var @out = runTest(t, testRaceSubReportsˢ);

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
        t.Errorf("got %d race reports, want %d", cReport, wantReport);
    }

    // Regardless of when the stacks are printed, we expect each subtest to be
    // marked as failed, and that failure should propagate up to the parents.
    nint cFail = bytes.Count(@out, slice<byte>("--- FAIL:"u8));
    nint wantFail = 0;
    if (race.Enabled) {
        wantFail = 4;
    }
    if (cFail != wantFail) {
        t.Errorf(@"got %d ""--- FAIL:"" lines, want %d", cReport, wantReport);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testRaceInCleanupˢ => ref testRaceInCleanupˢᵇ;
private static readonly @string testRaceInCleanupˢᵇ = "TestRaceInCleanup"u8;

public static void TestRaceInCleanup(ж<testing.T> t) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        t.Cleanup(doRace);
        t.Parallel();
        t.Run(subˢ, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            // No race should be reported for sub.
        });
        return;
    }

    var @out = runTest(t, testRaceInCleanupˢ);

    // There should be one race report, for the parent test only.
    nint cReport = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));
    nint wantReport = 0;
    if (race.Enabled) {
        wantReport = 1;
    }
    if (cReport != wantReport) {
        t.Errorf("got %d race reports, want %d", cReport, wantReport);
    }

    // Only the parent test should be marked as failed.
    // (The subtest does not race, and should pass.)
    nint cFail = bytes.Count(@out, slice<byte>("--- FAIL:"u8));
    nint wantFail = 0;
    if (race.Enabled) {
        wantFail = 1;
    }
    if (cFail != wantFail) {
        t.Errorf(@"got %d ""--- FAIL:"" lines, want %d", cReport, wantReport);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string subsubˢ => ref subsubˢᵇ;
private static readonly @string subsubˢᵇ = "subsub"u8;
private static ref readonly @string subsubsubˢ => ref subsubsubˢᵇ;
private static readonly @string subsubsubˢᵇ = "subsubsub"u8;
private static ref readonly @string testDeepSubtestRaceˢ => ref testDeepSubtestRaceˢᵇ;
private static readonly @string testDeepSubtestRaceˢᵇ = "TestDeepSubtestRace"u8;

public static void TestDeepSubtestRace(ж<testing.T> t) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        t.Run(subˢ, (ж<testing.T> tΔ1) => {
            tΔ1.Run(subsubˢ, (ж<testing.T> tΔ2) => {
                tΔ2.Run(subsubsubˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ3) => {
                    doRace();
                });
            });
            doRace();
        });
        return;
    }

    var @out = runTest(t, testDeepSubtestRaceˢ);

    nint c = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));
    nint want = 0;
    // There should be two race reports.
    if (race.Enabled) {
        want = 2;
    }
    if (c != want) {
        t.Errorf("got %d race reports, want %d", c, want);
    }
}

public static partial void TestRaceDuringParallelFailsAllSubtests(ж<testing.T> t) {
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
        t.Run(subˢ, (ж<testing.T> tΔ1) => {
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

    var @out = runTest(t, "TestRaceDuringParallelFailsAllSubtests");

    nint c = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));
    nint want = 0;
    // Each subtest should report the race independently.
    if (race.Enabled) {
        want = 2;
    }
    if (c != want) {
        t.Errorf("got %d race reports, want %d", c, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testRaceBeforeParallelˢ => ref testRaceBeforeParallelˢᵇ;
private static readonly @string testRaceBeforeParallelˢᵇ = "TestRaceBeforeParallel"u8;

public static void TestRaceBeforeParallel(ж<testing.T> t) {
    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        t.Run(subˢ, (ж<testing.T> tΔ1) => {
            doRace();
            tΔ1.Parallel();
        });
        return;
    }

    var @out = runTest(t, testRaceBeforeParallelˢ);

    nint c = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));
    nint want = 0;
    // We should see one race detector report.
    if (race.Enabled) {
        want = 1;
    }
    if (c != want) {
        t.Errorf("got %d race reports, want %d", c, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testRunˢ2 => ref testRunˢ2ᵇ;
private static readonly @string testRunˢ2ᵇ = "-test.run=^$"u8;

public static void TestRaceBeforeTests(ж<testing.T> t) {
    var cmd = testenv.Command(new testing_TжTB(t), testenv.Executable(new testing_TжTB(t)), testRunˢ2);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Env = append(cmd.Env, (@string)"GO_WANT_RACE_BEFORE_TESTS=1");
    var (@out, _) = cmd.CombinedOutput();
    t.Logf("%s", @out);

    nint c = bytes.Count(@out, slice<byte>("race detected outside of test execution"u8));

    nint want = 0;
    if (race.Enabled) {
        want = 1;
    }
    if (c != want) {
        t.Errorf("got %d race reports; want %d", c, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string benchmarkRacyˢ => ref benchmarkRacyˢᵇ;
private static readonly @string benchmarkRacyˢᵇ = "BenchmarkRacy"u8;

public static void TestBenchmarkRace(ж<testing.T> t) {
    var @out = runTest(t, benchmarkRacyˢ);
    nint c = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));

    nint want = 0;
    // We should see one race detector report.
    if (race.Enabled) {
        want = 1;
    }
    if (c != want) {
        t.Errorf("got %d race reports; want %d", c, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string benchmarkBLoopRacyˢ => ref benchmarkBLoopRacyˢᵇ;
private static readonly @string benchmarkBLoopRacyˢᵇ = "BenchmarkBLoopRacy"u8;

public static void TestBenchmarkRaceBLoop(ж<testing.T> t) {
    var @out = runTest(t, benchmarkBLoopRacyˢ);
    nint c = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));

    nint want = 0;
    // We should see one race detector report.
    if (race.Enabled) {
        want = 1;
    }
    if (c != want) {
        t.Errorf("got %d race reports; want %d", c, want);
    }
}

public static void BenchmarkRacy(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (Δos.Getenv(goWantHelperProcessˢ) != "1"u8) {
        Ꮡb.Skipf("skipping intentionally-racy benchmark");
    }
    for (nint i = 0; i < b.N; i++) {
        doRace();
    }
}

public static void BenchmarkBLoopRacy(ж<testing.B> b) {
    if (Δos.Getenv(goWantHelperProcessˢ) != "1"u8) {
        b.Skipf("skipping intentionally-racy benchmark");
    }
    while (b.Loop()) {
        doRace();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string benchmarkSubRacyˢ => ref benchmarkSubRacyˢᵇ;
private static readonly @string benchmarkSubRacyˢᵇ = "BenchmarkSubRacy"u8;

public static void TestBenchmarkSubRace(ж<testing.T> t) {
    var @out = runTest(t, benchmarkSubRacyˢ);
    nint c = bytes.Count(@out, slice<byte>("race detected during execution of test"u8));

    nint want = 0;
    // We should see 3 race detector reports:
    // one in the sub-bencmark, one in the parent afterward,
    // and one in b.Loop.
    if (race.Enabled) {
        want = 3;
    }
    if (c != want) {
        t.Errorf("got %d race reports; want %d", c, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string nonRacyˢ => ref nonRacyˢᵇ;
private static readonly @string nonRacyˢᵇ = "non-racy"u8;
private static ref readonly @string racyˢ => ref racyˢᵇ;
private static readonly @string racyˢᵇ = "racy"u8;
private static ref readonly @string racyBLoopˢ => ref racyBLoopˢᵇ;
private static readonly @string racyBLoopˢᵇ = "racy-bLoop"u8;

public static void BenchmarkSubRacy(ж<testing.B> b) {
    if (Δos.Getenv(goWantHelperProcessˢ) != "1"u8) {
        b.Skipf("skipping intentionally-racy benchmark");
    }

    b.Run(nonRacyˢ, (ж<testing.B> bΔ1) => {
        nint tot = 0;
        for (nint i = 0; i < (~bΔ1).N; i++) {
            tot++;
        }
        _ = tot;
    });

    b.Run(racyˢ, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            doRace();
        }
    });

    b.Run(racyBLoopˢ, (ж<testing.B> bΔ3) => {
        while (bΔ3.Loop()) {
            doRace();
        }
    });

    doRace(); // should be reported separately
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testParallel4ˢ => ref testParallel4ˢᵇ;
private static readonly @string testParallel4ˢᵇ = "-test.parallel=4"u8;

public static void TestRunningTests(ж<testing.T> t) {
    t.Parallel();

    // Regression test for https://go.dev/issue/64404:
    // on timeout, the "running tests" message should not include
    // tests that are waiting on parked subtests.

    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        for (nint i = 0; i < 2; i++) {
            t.Run(fmt.Sprintf("outer%d"u8, i), (ж<testing.T> tΔ1) => {
                tΔ1.Parallel();
                for (nint j = 0; j < 2; j++) {
                    tΔ1.Run(fmt.Sprintf("inner%d"u8, j), (ж<testing.T> tΔ2) => {
                        tΔ2.Parallel();
                        for (;;) {
                            time.Sleep(1 * time.Millisecond);
                        }
                    });
                }
            });
        }
    }

    var timeout = 10 * time.Millisecond;
    for (;;) {
        var cmd = testenv.Command(new testing_TжTB(t), testenv.Executable(new testing_TжTB(t)), "-test.run=^"u8 + t.Name() + "$"u8, "-test.timeout=" + timeout.String(), testParallel4ˢ);
        cmd.Env = append(cmd.Environ(), (@string)"GO_WANT_HELPER_PROCESS=1");
        var (@out, err) = cmd.CombinedOutput();
        t.Logf("%v:\n%s", cmd.OrTypedNil(), @out);
        {
            var (_, okΔ1) = err._<ж<exec.ExitError>>(ᐧ); if (!okΔ1) {
                t.Fatal(err);
            }
        }

        // Because the outer subtests (and TestRunningTests itself) are marked as
        // parallel, their test functions return (and are no longer “running”)
        // before the inner subtests are released to run and hang.
        // Only those inner subtests should be reported as running.
        var want = slice<@string>([
            "TestRunningTests/outer0/inner0",
            "TestRunningTests/outer0/inner1",
            "TestRunningTests/outer1/inner0",
            "TestRunningTests/outer1/inner1"
        ]);

        var (got, ok) = parseRunningTests(@out);
        if (slices.Equal<slice<@string>, @string>(got, want)) {
            break;
        }
        if (ok) {
            t.Logf("found running tests:\n%s\nwant:\n%s", strings.Join(got, "\n"), strings.Join(want, "\n"));
        } else {
            t.Logf("no running tests found");
        }
        t.Logf("retrying with longer timeout");
        timeout *= 2;
    }
}

public static void TestRunningTestsInCleanup(ж<testing.T> t) {
    t.Parallel();

    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        for (nint i = 0; i < 2; i++) {
            t.Run(fmt.Sprintf("outer%d"u8, i), (ж<testing.T> tΔ1) => {
                // Not parallel: we expect to see only one outer test,
                // stuck in cleanup after its subtest finishes.

                tΔ1.Cleanup(() => {
                    for (;;) {
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
    for (;;) {
        var cmd = testenv.Command(new testing_TжTB(t), testenv.Executable(new testing_TжTB(t)), "-test.run=^"u8 + t.Name() + "$"u8, "-test.timeout=" + timeout.String());
        cmd.Env = append(cmd.Environ(), (@string)"GO_WANT_HELPER_PROCESS=1");
        var (@out, err) = cmd.CombinedOutput();
        t.Logf("%v:\n%s", cmd.OrTypedNil(), @out);
        {
            var (_, okΔ1) = err._<ж<exec.ExitError>>(ᐧ); if (!okΔ1) {
                t.Fatal(err);
            }
        }

        // TestRunningTestsInCleanup is blocked in the call to t.Run,
        // but its test function has not yet returned so it should still
        // be considered to be running.
        // outer1 hasn't even started yet, so only outer0 and the top-level
        // test function should be reported as running.
        var want = slice<@string>([
            "TestRunningTestsInCleanup",
            "TestRunningTestsInCleanup/outer0"
        ]);

        var (got, ok) = parseRunningTests(@out);
        if (slices.Equal<slice<@string>, @string>(got, want)) {
            break;
        }
        if (ok) {
            t.Logf("found running tests:\n%s\nwant:\n%s", strings.Join(got, "\n"), strings.Join(want, "\n"));
        } else {
            t.Logf("no running tests found");
        }
        t.Logf("retrying with longer timeout");
        timeout *= 2;
    }
}

internal static (slice<@string> runningTests, bool ok) parseRunningTests(slice<byte> @out) {
    slice<@string> runningTests = default;

    var inRunningTests = false;
    foreach (var line in range<@string>(strings.SplitSeq((@string)@out, "\n").Invoke)) {
        if (inRunningTests) {
            // Package testing adds one tab, the panic printer adds another.
            {
                var (trimmed, okΔ1) = strings.CutPrefix(line, "\t\t"); if (okΔ1) {
                    {
                        var (name, _, okΔ2) = strings.Cut(trimmed, " "); if (okΔ2) {
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

    return (default, false);
}

public static partial void TestConcurrentRun(ж<testing.T> t) {
    // Regression test for https://go.dev/issue/64402:
    // this deadlocked after https://go.dev/cl/506755.

    var block = new channel<EmptyStruct>(0);
    ref var ready = ref heap(new Δsync.WaitGroup(), out var Ꮡready);
    ref var done = ref heap(new Δsync.WaitGroup(), out var Ꮡdone);
    for (nint i = 0; i < 2; i++) {
        Ꮡready.Add(1);
        Ꮡdone.Add(1);
        var blockʗ1 = block;
        goǃ((ᴛ1, ᴛ2) => t.Run(ᴛ1, ᴛ2), (@string)"", (ж<testing.T> _) => {
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
private static ref readonly @string outerˢ => ref outerˢᵇ;
private static readonly @string outerˢᵇ = "outer"u8;
private static readonly object helloOuterˢ = (@string)"Hello outer!"u8;
private static ref readonly @string notInnerˢ => ref notInnerˢᵇ;
private static readonly @string notInnerˢᵇ = "not_inner"u8;
private static readonly object helloInnerˢ = (@string)"Hello inner!"u8;

public static void TestParentRun(ж<testing.T> t1) {
    // Regression test for https://go.dev/issue/64402:
    // this deadlocked after https://go.dev/cl/506755.

    t1.Run(outerˢ, (ж<testing.T> t2) => {
        t2.Log(helloOuterˢ);
        t1.Run(notInnerˢ, (ж<testing.T> t3) => { // Note: this is t1.Run, not t2.Run.
            t3.Log(helloInnerˢ);
        });
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string innerˢ => ref innerˢᵇ;
private static readonly @string innerˢᵇ = "inner"u8;
private static ref readonly @string inner2ˢ => ref inner2ˢᵇ;
private static readonly @string inner2ˢᵇ = "inner2"u8;
private static readonly object expectedContextOfSiblingˢ = (@string)"expected context of sibling test to be canceled after its test function finished"u8;
private static readonly object expectedContextCanceledˢ = (@string)"expected context canceled before cleanup"u8;

public static void TestContext(ж<testing.T> t) {
    var ctx = t.Context();
    {
        var err = ctx.Err(); if (err != nil) {
            t.Fatalf("expected non-canceled context, got %v", err);
        }
    }

    ref var innerCtx = ref heap<context.Context>(out var ᏑinnerCtx);
    t.Run(innerˢ, (ж<testing.T> tΔ1) => {
        ᏑinnerCtx.ValueSlot = tΔ1.Context();
        {
            var err = ᏑinnerCtx.ValueSlot.Err(); if (err != nil) {
                tΔ1.Fatalf("expected inner test to not inherit canceled context, got %v", err);
            }
        }
    });
    t.Run(inner2ˢ, (ж<testing.T> tΔ2) => {
        if (!errors.Is(ᏑinnerCtx.ValueSlot.Err(), context.Canceled)) {
            tΔ2.Fatal(expectedContextOfSiblingˢ);
        }
    });

    var ctxʗ1 = ctx;
    t.Cleanup(() => {
        if (!errors.Is(ctxʗ1.Err(), context.Canceled)) {
            t.Fatal(expectedContextCanceledˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string keyˢ => ref keyˢᵇ;
private static readonly @string keyˢᵇ = "key"u8;

// TestAttrExample is used by TestAttrSet,
// and also serves as a convenient test to run that sets an attribute.
public static void TestAttrExample(ж<testing.T> t) {
    t.Attr(keyˢ, valueˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testAttrExampleˢ => ref testAttrExampleˢᵇ;
private static readonly @string testAttrExampleˢᵇ = "TestAttrExample"u8;
private static ref readonly @string attrTestAttrExampleKeyˢ => ref attrTestAttrExampleKeyˢᵇ;
private static readonly @string attrTestAttrExampleKeyˢᵇ = "=== ATTR  TestAttrExample key value\n"u8;

public static void TestAttrSet(ж<testing.T> t) {
    @string @out = (@string)runTest(t, testAttrExampleˢ);

    @string want = attrTestAttrExampleKeyˢ;
    if (!strings.Contains(@out, want)) {
        t.Errorf("expected output containing %q, got:\n%q", want, @out);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testAttrInvalidˢ => ref testAttrInvalidˢᵇ;
private static readonly @string testAttrInvalidˢᵇ = "TestAttrInvalid"u8;

internal partial struct TestAttrInvalid_tests /*dyn*/ {
    internal @string key;
    internal @string value;
}

public static void TestAttrInvalid(ж<testing.T> t) {
    var tests = slice<TestAttrInvalid_tests>([
        new("k ey", "value"),
        new("k\tey", "value"),
        new("k\rey", "value"),
        new("k\ney", "value"),
        new("key", "val\rue"),
        new("key", "val\nue")
    ]);

    if (Δos.Getenv(goWantHelperProcessˢ) == "1"u8) {
        foreach (var (i, vᴛ1) in range(tests)) {
            ref var test = ref heap(new TestAttrInvalid_tests(), out var Ꮡtest);
            test = vᴛ1;

            var testʗ1 = test;
            t.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
                tΔ1.Attr(testʗ1.key, testʗ1.value);
            });
        }
        return;
    }

    @string @out = (@string)runTest(t, testAttrInvalidˢ);

    foreach (var i in range(len(tests))) {
        @string want = fmt.Sprintf("--- FAIL: TestAttrInvalid/%v "u8, i);
        if (!strings.Contains(@out, want)) {
            t.Errorf("expected output containing %q, got:\n%q", want, @out);
        }
    }
}

internal static ref readonly @string artifactContent => ref artifactContentᵇ;
private static readonly @string artifactContentᵇ = "It belongs in a museum.\n"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string artifactˢ => ref artifactˢᵇ;
private static readonly @string artifactˢᵇ = "artifact"u8;

public static void TestArtifactDirExample(ж<testing.T> t) {
    Δos.WriteFile(filepath.Join(t.ArtifactDir(), artifactˢ), slice<byte>(artifactContent), 438);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testArtifactDirExampleˢ => ref testArtifactDirExampleˢᵇ;
private static readonly @string testArtifactDirExampleˢᵇ = "TestArtifactDirExample"u8;
private static ref readonly @string testArtifactsˢ => ref testArtifactsˢᵇ;
private static readonly @string testArtifactsˢᵇ = "-test.artifacts"u8;

public static void TestArtifactDirDefault(ж<testing.T> t) {
    @string tempDir = t.TempDir();
    t.Chdir(tempDir);
    var @out = runTest(t, testArtifactDirExampleˢ, testArtifactsˢ);
    checkArtifactDir(t, @out, testArtifactDirExampleˢ, tempDir);
}

public static void TestArtifactDirSpecified(ж<testing.T> t) {
    @string tempDir = t.TempDir();
    var @out = runTest(t, testArtifactDirExampleˢ, testArtifactsˢ, "-test.outputdir=" + tempDir);
    checkArtifactDir(t, @out, testArtifactDirExampleˢ, tempDir);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string artifactsˢ => ref artifactsˢᵇ;
private static readonly @string artifactsˢᵇ = "=== ARTIFACTS"u8;

public static void TestArtifactDirNoArtifacts(ж<testing.T> t) {
    t.Chdir(t.TempDir());
    @string @out = (@string)runTest(t, testArtifactDirExampleˢ);
    if (strings.Contains(@out, artifactsˢ)) {
        t.Errorf("expected output with no === ARTIFACTS, got\n%q", @out);
    }
    var (ents, err) = Δos.ReadDir(".");
    if (err != nil) {
        t.Fatal(err);
    }
    foreach (var (_, e) in range(ents)) {
        t.Errorf("unexpected file in current directory after test: %v", e.Name());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string subtestˢ => ref subtestˢᵇ;
private static readonly @string subtestˢᵇ = "Subtest"u8;

public static void TestArtifactDirSubtestExample(ж<testing.T> t) {
    t.Run(subtestˢ, (ж<testing.T> tΔ1) => {
        Δos.WriteFile(filepath.Join(tΔ1.ArtifactDir(), artifactˢ), slice<byte>(artifactContent), 438);
    });
}

public static void TestArtifactDirInSubtest(ж<testing.T> t) {
    @string tempDir = t.TempDir();
    var @out = runTest(t, "TestArtifactDirSubtestExample/Subtest", testArtifactsˢ, "-test.outputdir=" + tempDir);
    checkArtifactDir(t, @out, "TestArtifactDirSubtestExample/Subtest", tempDir);
}

public static void TestArtifactDirLongTestNameExample(ж<testing.T> t) {
    @string name = strings.Repeat("x", 256);
    t.Run(name, (ж<testing.T> tΔ1) => {
        Δos.WriteFile(filepath.Join(tΔ1.ArtifactDir(), artifactˢ), slice<byte>(artifactContent), 438);
    });
}

public static void TestArtifactDirWithLongTestName(ж<testing.T> t) {
    @string tempDir = t.TempDir();
    var @out = runTest(t, "TestArtifactDirLongTestNameExample", testArtifactsˢ, "-test.outputdir=" + tempDir);
    checkArtifactDir(t, @out, @"TestArtifactDirLongTestNameExample/\w+", tempDir);
}

public static void TestArtifactDirConsistent(ж<testing.T> t) {
    @string a = t.ArtifactDir();
    @string b = t.ArtifactDir();
    if (a != b) {
        t.Errorf("t.ArtifactDir is not consistent between calls: %q, %q", a, b);
    }
}

internal static void checkArtifactDir(ж<testing.T> t, slice<byte> @out, @string testName, @string outputDir) {
    t.Helper();

    var re = Δregexp.MustCompile(@"=== ARTIFACTS "u8 + testName + @" ([^\n]+)"u8);
    var match = re.FindSubmatch(@out);
    if (match == nil) {
        t.Fatalf("expected output matching %q, got\n%q", re.OrTypedNil(), @out);
    }
    @string artifactDir = (@string)match[1];

    // Verify that the artifact directory is contained in the expected output directory.
    var (relDir, err) = filepath.Rel(outputDir, artifactDir);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!filepath.IsLocal(relDir)) {
        t.Fatalf("want artifact directory contained in %q, got %q", outputDir, artifactDir);
    }

    foreach (var (_, part) in range(strings.Split(relDir, (@string)(rune)Δos.PathSeparator))) {
        const nint maxSize = 64;
        if (len(part) > maxSize) {
            t.Errorf("artifact directory %q contains component >%v characters long: %q", relDir, (nint)(maxSize), part);
        }
    }

    (var got, err) = Δos.ReadFile(filepath.Join(artifactDir, artifactˢ));
    if (err != nil || ((sstring)got) != artifactContent) {
        t.Errorf("reading artifact in %q: got %q, %v; want %q", artifactDir, got, err, artifactContent);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string benchmarkBLoopPrintˢ => ref benchmarkBLoopPrintˢᵇ;
private static readonly @string benchmarkBLoopPrintˢᵇ = "BenchmarkBLoopPrint"u8;
private static ref readonly @string benchmarkBLoopPrint09S2Sˢ => ref benchmarkBLoopPrint09S2Sˢᵇ;
private static readonly @string benchmarkBLoopPrint09S2Sˢᵇ = @"BenchmarkBLoopPrint(-[0-9]+)?\s+2\s+[0-9]+\s+ns/op"u8;
private static readonly object missingBenchmarkOutputˢ = (@string)"missing benchmark output"u8;

public static void TestBenchmarkBLoopIterationCorrect(ж<testing.T> t) {
    var @out = runTest(t, benchmarkBLoopPrintˢ);
    nint c = bytes.Count(@out, slice<byte>("Printing from BenchmarkBLoopPrint"u8));

    nint want = 2;
    if (c != want) {
        t.Errorf("got %d loop iterations; want %d", c, want);
    }

    // b.Loop() will only rampup once.
    c = bytes.Count(@out, slice<byte>("Ramping up from BenchmarkBLoopPrint"u8));
    want = 1;
    if (c != want) {
        t.Errorf("got %d loop rampup; want %d", c, want);
    }

    var re = Δregexp.MustCompile(benchmarkBLoopPrint09S2Sˢ);
    if (!re.Match(@out)) {
        t.Error(missingBenchmarkOutputˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string benchmarkBNPrintˢ => ref benchmarkBNPrintˢᵇ;
private static readonly @string benchmarkBNPrintˢᵇ = "BenchmarkBNPrint"u8;

public static void TestBenchmarkBNIterationCorrect(ж<testing.T> t) {
    var @out = runTest(t, benchmarkBNPrintˢ);
    nint c = bytes.Count(@out, slice<byte>("Printing from BenchmarkBNPrint"u8));

    // runTest sets benchtime=2x, with semantics specified in #32051 it should
    // run 3 times.
    nint want = 3;
    if (c != want) {
        t.Errorf("got %d loop iterations; want %d", c, want);
    }

    // b.N style fixed iteration loop will rampup twice:
    // One in run1(), the other in launch
    c = bytes.Count(@out, slice<byte>("Ramping up from BenchmarkBNPrint"u8));
    want = 2;
    if (c != want) {
        t.Errorf("got %d loop rampup; want %d", c, want);
    }
}

public static void BenchmarkBLoopPrint(ж<testing.B> b) {
    b.Logf("Ramping up from BenchmarkBLoopPrint");
    while (b.Loop()) {
        b.Logf("Printing from BenchmarkBLoopPrint");
    }
}

public static void BenchmarkBNPrint(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    Ꮡb.Logf("Ramping up from BenchmarkBNPrint");
    for (nint i = 0; i < b.N; i++) {
        Ꮡb.Logf("Printing from BenchmarkBNPrint");
    }
}

public static partial void TestArtifactDir(ж<testing.T> t) {
    t.Log(t.ArtifactDir());
}

} // end testing_test_package
