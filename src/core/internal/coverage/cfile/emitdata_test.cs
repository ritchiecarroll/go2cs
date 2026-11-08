// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using fmt = fmt_package;
using coverage = go.@internal.coverage_package;
using platform = go.@internal.platform_package;
using testenv = go.@internal.testenv_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using fs = go.io.fs_package;
using go.@internal;
using go.io;
using go.os;
using path;
using static go.@internal.coverage.cfile_package;

partial class cfile_internal_test_package {

// Set to true for debugging (linux only).
internal const bool fixedTestDir = false;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tmpQqqzzzˢ => ref tmpQqqzzzˢᵇ;
private static readonly @string tmpQqqzzzˢᵇ = "/tmp/qqqzzz"u8;
internal static ref readonly @string build1ˢ => ref build1ˢᵇ;
private static readonly @string build1ˢᵇ = "build1"u8;
internal static ref readonly @string setˢ => ref setˢᵇ;
private static readonly @string setˢᵇ = "set"u8;
internal static ref readonly @string build2ˢ => ref build2ˢᵇ;
private static readonly @string build2ˢᵇ = "build2"u8;
internal static ref readonly @string emitToDirˢ => ref emitToDirˢᵇ;
private static readonly @string emitToDirˢᵇ = "emitToDir"u8;
internal static ref readonly @string emitToWriterˢ => ref emitToWriterˢᵇ;
private static readonly @string emitToWriterˢᵇ = "emitToWriter"u8;
internal static ref readonly @string emitToNonexistentDirˢ => ref emitToNonexistentDirˢᵇ;
private static readonly @string emitToNonexistentDirˢᵇ = "emitToNonexistentDir"u8;
internal static ref readonly @string emitToNilWriterˢ => ref emitToNilWriterˢᵇ;
private static readonly @string emitToNilWriterˢᵇ = "emitToNilWriter"u8;
internal static ref readonly @string emitToFailingWriterˢ => ref emitToFailingWriterˢᵇ;
private static readonly @string emitToFailingWriterˢᵇ = "emitToFailingWriter"u8;
internal static ref readonly @string emitWithCounterClearˢ => ref emitWithCounterClearˢᵇ;
private static readonly @string emitWithCounterClearˢᵇ = "emitWithCounterClear"u8;
internal static ref readonly @string emitToDirNonAtomicˢ => ref emitToDirNonAtomicˢᵇ;
private static readonly @string emitToDirNonAtomicˢᵇ = "emitToDirNonAtomic"u8;
internal static ref readonly @string emitToWriterNonAtomicˢ => ref emitToWriterNonAtomicˢᵇ;
private static readonly @string emitToWriterNonAtomicˢᵇ = "emitToWriterNonAtomic"u8;

public static void TestCoverageApis(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skipf("skipping test: too long for short mode");
    }
    testenv.MustHaveGoBuild(new cfile_internal_test_package.testing_TжTB(t));
    @string dir = t.TempDir();
    if (fixedTestDir) {
        dir = tmpQqqzzzˢ;
        os.RemoveAll(dir);
        mkdir(t, dir);
    }

    // Build harness. We need two copies of the harness, one built
    // with -covermode=atomic and one built non-atomic.
    @string bdir1 = mkdir(t, filepath.Join(dir, build1ˢ));
    var hargs1 = slice<@string>(["-covermode=atomic", "-coverpkg=all"]);
    @string atomicHarnessPath = buildHarness(t, bdir1, hargs1);
    @string nonAtomicMode = testing.CoverMode();
    if (testing.CoverMode() == "atomic"u8) {
        nonAtomicMode = setˢ;
    }
    @string bdir2 = mkdir(t, filepath.Join(dir, build2ˢ));
    var hargs2 = slice<@string>(["-coverpkg=all", "-covermode="u8 + nonAtomicMode]);
    @string nonAtomicHarnessPath = buildHarness(t, bdir2, hargs2);

    t.Logf("atomic harness path is %s", atomicHarnessPath);
    t.Logf("non-atomic harness path is %s", nonAtomicHarnessPath);

    // Sub-tests for each API we want to inspect, plus
    // extras for error testing.
    t.Run(emitToDirˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Parallel();
        testEmitToDir(tΔ1, atomicHarnessPath, dir);
    });
    t.Run(emitToWriterˢ, (ж<testing.T> tΔ2) => {
        tΔ2.Parallel();
        testEmitToWriter(tΔ2, atomicHarnessPath, dir);
    });
    t.Run(emitToNonexistentDirˢ, (ж<testing.T> tΔ3) => {
        tΔ3.Parallel();
        testEmitToNonexistentDir(tΔ3, atomicHarnessPath, dir);
    });
    t.Run(emitToNilWriterˢ, (ж<testing.T> tΔ4) => {
        tΔ4.Parallel();
        testEmitToNilWriter(tΔ4, atomicHarnessPath, dir);
    });
    t.Run(emitToFailingWriterˢ, (ж<testing.T> tΔ5) => {
        tΔ5.Parallel();
        testEmitToFailingWriter(tΔ5, atomicHarnessPath, dir);
    });
    t.Run(emitWithCounterClearˢ, (ж<testing.T> tΔ6) => {
        tΔ6.Parallel();
        testEmitWithCounterClear(tΔ6, atomicHarnessPath, dir);
    });
    t.Run(emitToDirNonAtomicˢ, (ж<testing.T> tΔ7) => {
        tΔ7.Parallel();
        testEmitToDirNonAtomic(tΔ7, nonAtomicHarnessPath, nonAtomicMode, dir);
    });
    t.Run(emitToWriterNonAtomicˢ, (ж<testing.T> tΔ8) => {
        tΔ8.Parallel();
        testEmitToWriterNonAtomic(tΔ8, nonAtomicHarnessPath, nonAtomicMode, dir);
    });
    t.Run("emitWithCounterClearNonAtomic", (ж<testing.T> tΔ9) => {
        tΔ9.Parallel();
        testEmitWithCounterClearNonAtomic(tΔ9, nonAtomicHarnessPath, nonAtomicMode, dir);
    });
}

// upmergeCoverData helps improve coverage data for this package
// itself. If this test itself is being invoked with "-cover", then
// what we'd like is for package coverage data (that is, coverage for
// routines in "runtime/coverage") to be incorporated into the test
// run from the "harness.exe" runs we've just done. We can accomplish
// this by doing a merge from the harness gocoverdir's to the test
// gocoverdir.
internal static void upmergeCoverData(ж<testing.T> t, @string gocoverdir, @string mode) {
    if (testing.CoverMode() != mode) {
        return;
    }
    @string testGoCoverDir = os.Getenv(gocoverdirˢ);
    if (testGoCoverDir == ""u8) {
        return;
    }
    var args = slice<@string>(["tool", "covdata", "merge", "-pkg=runtime/coverage",
        "-o", testGoCoverDir, "-i", gocoverdir]);
    t.Logf("up-merge of covdata from %s to %s", gocoverdir, testGoCoverDir);
    t.Logf("executing: go %+v", args);
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(t)), args.ꓸꓸꓸ);
    var (b, err) = cmd.CombinedOutput(); if (err != nil) {
        t.Fatalf("covdata merge failed (%v): %s", err, b);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string harnessExeˢ => ref harnessExeˢᵇ;
private static readonly @string harnessExeˢᵇ = "harness.exe"u8;
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;
internal static ref readonly @string harnessGoˢ => ref harnessGoˢᵇ;
private static readonly @string harnessGoˢᵇ = "harness.go"u8;

// buildHarness builds the helper program "harness.exe".
internal static @string buildHarness(ж<testing.T> t, @string dir, slice<@string> opts) {
    var argsᴷ = new stackAppend<@string>();

    @string harnessPath = filepath.Join(dir, harnessExeˢ);
    @string harnessSrc = filepath.Join(testdataˢ, harnessGoˢ);
    var args = slice<@string>(["build", "-o", harnessPath]);
    args = appendꓸꓸꓸ(args, opts);
    args = append(argsᴷ, args, harnessSrc);
    //t.Logf("harness build: go %+v\n", args)
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(t)), args.ꓸꓸꓸ);
    var (b, err) = cmd.CombinedOutput(); if (err != nil) {
        t.Fatalf("build failed (%v): %s", err, b);
    }
    return harnessPath;
}

internal static @string mkdir(ж<testing.T> t, @string d) {
    t.Helper();
    var err = os.Mkdir(d, 511); if (err != nil) {
        t.Fatalf("mkdir failed: %v", err);
    }
    return d;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gocoverdirˢ2 => ref gocoverdirˢ2ᵇ;
private static readonly @string gocoverdirˢ2ᵇ = "GOCOVERDIR="u8;

// updateGoCoverDir updates the specified environment 'env' to set
// GOCOVERDIR to 'gcd' (if setGoCoverDir is TRUE) or removes
// GOCOVERDIR from the environment (if setGoCoverDir is false).
internal static slice<@string> updateGoCoverDir(slice<@string> env, @string gcd, bool setGoCoverDir) {
    var rv = slice<@string>([]);
    var found = false;
    foreach (var (_, vᴛ1) in range(env)) {
        var v = vᴛ1;

        if (strings.HasPrefix(v, gocoverdirˢ2)) {
            if (!setGoCoverDir) {
                continue;
            }
            v = "GOCOVERDIR="u8 + gcd;
            found = true;
        }
        rv = append(rv, v);
    }
    if (!found && setGoCoverDir) {
        rv = append(rv, "GOCOVERDIR="u8 + gcd);
    }
    return rv;
}

internal static (@string, error) runHarness(ж<testing.T> t, @string harnessPath, @string tp, bool setGoCoverDir, @string rdir, @string edir) {
    t.Logf("running: %s -tp %s -o %s with rdir=%s and GOCOVERDIR=%v", harnessPath, tp, edir, rdir, setGoCoverDir);
    var cmd = exec.Command(harnessPath, "-tp", tp, "-o", edir);
    cmd.Dir = rdir;
    cmd.Env = updateGoCoverDir(os.Environ(), rdir, setGoCoverDir);
    var (b, err) = cmd.CombinedOutput();
    //t.Logf("harness run output: %s\n", string(b))
    return ((@string)b, err);
}

internal static @string testForSpecificFunctions(ж<testing.T> t, @string dir, slice<@string> want, slice<@string> avoid) {
    var args = slice<@string>(["tool", "covdata", "debugdump",
        "-live", "-pkg=command-line-arguments", "-i="u8 + dir]);
    t.Logf("running: go %v\n", args);
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(t)), args.ꓸꓸꓸ);
    var (b, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("'go tool covdata failed (%v): %s", err, b);
    }
    @string output = (@string)b;
    @string rval = ""u8;
    foreach (var (_, f) in range(want)) {
        @string wf = "Func: "u8 + f + "\n"u8;
        if (strings.Contains(output, wf)) {
            continue;
        }
        rval += fmt.Sprintf("error: output should contain %q but does not\n"u8, wf);
    }
    foreach (var (_, f) in range(avoid)) {
        @string wf = "Func: "u8 + f + "\n"u8;
        if (strings.Contains(output, wf)) {
            rval += fmt.Sprintf("error: output should not contain %q but does\n"u8, wf);
        }
    }
    if (rval != ""u8) {
        t.Logf("=-= begin output:\n%s\n=-= end output\n", output);
    }
    return rval;
}

internal static void withAndWithoutRunner(Action<bool, @string> f) {
    // Run 'f' with and without GOCOVERDIR set.
    for (nint i = 0; i < 2; i++) {
        @string tag = "x";
        var setGoCoverDir = true;
        if (i == 0) {
            setGoCoverDir = false;
            tag = "y";
        }
        f(setGoCoverDir, tag);
    }
}

internal static (@string, @string) mktestdirs(ж<testing.T> t, @string tag, @string tp, @string dir) {
    t.Helper();
    @string rdir = mkdir(t, filepath.Join(dir, tp + "-rdir-" + tag));
    @string edir = mkdir(t, filepath.Join(dir, tp + "-edir-" + tag));
    return (rdir, edir);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string atomicˢ => ref atomicˢᵇ;
private static readonly @string atomicˢᵇ = "atomic"u8;

internal static void testEmitToDir(ж<testing.T> t, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        @string tp = emitToDirˢ;
        var (rdir, edir) = mktestdirs(t, tag, tp, dir);
        var (output, err) = runHarness(t, harnessPath, tp,
            setGoCoverDir, rdir, edir);
        if (err != nil) {
            t.Logf("%s", output);
            t.Fatalf("running 'harness -tp emitDir': %v", err);
        }

        // Just check to make sure meta-data file and counter data file were
        // written. Another alternative would be to run "go tool covdata"
        // or equivalent, but for now, this is what we've got.
        (var dents, err) = os.ReadDir(edir);
        if (err != nil) {
            t.Fatalf("os.ReadDir(%s) failed: %v", edir, err);
        }
        nint mfc = 0;
        nint cdc = 0;
        foreach (var (_, e) in range(dents)) {
            if (e.IsDir()) {
                continue;
            }
            if (strings.HasPrefix(e.Name(), coverage.MetaFilePref)) {
                mfc++;
            } else if (strings.HasPrefix(e.Name(), coverage.CounterFilePref)) {
                cdc++;
            }
        }
        nint wantmf = 1;
        nint wantcf = 1;
        if (mfc != wantmf) {
            t.Errorf("EmitToDir: want %d meta-data files, got %d\n", wantmf, mfc);
        }
        if (cdc != wantcf) {
            t.Errorf("EmitToDir: want %d counter-data files, got %d\n", wantcf, cdc);
        }
        upmergeCoverData(t, edir, atomicˢ);
        upmergeCoverData(t, rdir, atomicˢ);
    });
}

internal static void testEmitToWriter(ж<testing.T> t, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        @string tp = emitToWriterˢ;
        var (rdir, edir) = mktestdirs(t, tag, tp, dir);
        var (output, err) = runHarness(t, harnessPath, tp, setGoCoverDir, rdir, edir);
        if (err != nil) {
            t.Logf("%s", output);
            t.Fatalf("running 'harness -tp %s': %v", tp, err);
        }
        var want = slice<@string>(["main", tp]);
        var avoid = slice<@string>(["final"]);
        @string msg = testForSpecificFunctions(t, edir, want, avoid); if (msg != ""u8) {
            t.Errorf("coverage data from %q output match failed: %s", tp, msg);
        }
        upmergeCoverData(t, edir, atomicˢ);
        upmergeCoverData(t, rdir, atomicˢ);
    });
}

internal static void testEmitToNonexistentDir(ж<testing.T> t, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        @string tp = emitToNonexistentDirˢ;
        var (rdir, edir) = mktestdirs(t, tag, tp, dir);
        var (output, err) = runHarness(t, harnessPath, tp, setGoCoverDir, rdir, edir);
        if (err != nil) {
            t.Logf("%s", output);
            t.Fatalf("running 'harness -tp %s': %v", tp, err);
        }
        upmergeCoverData(t, edir, atomicˢ);
        upmergeCoverData(t, rdir, atomicˢ);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string emitToUnwritableDirˢ => ref emitToUnwritableDirˢᵇ;
private static readonly @string emitToUnwritableDirˢᵇ = "emitToUnwritableDir"u8;

internal static void testEmitToUnwritableDir(ж<testing.T> t, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        GoFrame ᒐ = default;
        try {
            @string tp = emitToUnwritableDirˢ;
            var (rdir, edir) = mktestdirs(t, tag, tp, dir);

            // Make edir unwritable.
            var errΔ1 = os.Chmod(edir, 365); if (errΔ1 != nil) {
                t.Fatalf("chmod failed: %v", errΔ1);
            }
            defer(os.Chmod, edir, (fs.FileMode)511, ref ᒐ);

            var (output, err) = runHarness(t, harnessPath, tp, setGoCoverDir, rdir, edir);
            if (err != nil) {
                t.Logf("%s", output);
                t.Fatalf("running 'harness -tp %s': %v", tp, err);
            }
            upmergeCoverData(t, edir, atomicˢ);
            upmergeCoverData(t, rdir, atomicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

internal static void testEmitToNilWriter(ж<testing.T> t, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        @string tp = emitToNilWriterˢ;
        var (rdir, edir) = mktestdirs(t, tag, tp, dir);
        var (output, err) = runHarness(t, harnessPath, tp, setGoCoverDir, rdir, edir);
        if (err != nil) {
            t.Logf("%s", output);
            t.Fatalf("running 'harness -tp %s': %v", tp, err);
        }
        upmergeCoverData(t, edir, atomicˢ);
        upmergeCoverData(t, rdir, atomicˢ);
    });
}

internal static void testEmitToFailingWriter(ж<testing.T> t, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        @string tp = emitToFailingWriterˢ;
        var (rdir, edir) = mktestdirs(t, tag, tp, dir);
        var (output, err) = runHarness(t, harnessPath, tp, setGoCoverDir, rdir, edir);
        if (err != nil) {
            t.Logf("%s", output);
            t.Fatalf("running 'harness -tp %s': %v", tp, err);
        }
        upmergeCoverData(t, edir, atomicˢ);
        upmergeCoverData(t, rdir, atomicˢ);
    });
}

internal static void testEmitWithCounterClear(ж<testing.T> t, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        @string tp = emitWithCounterClearˢ;
        var (rdir, edir) = mktestdirs(t, tag, tp, dir);
        var (output, err) = runHarness(t, harnessPath, tp,
            setGoCoverDir, rdir, edir);
        if (err != nil) {
            t.Logf("%s", output);
            t.Fatalf("running 'harness -tp %s': %v", tp, err);
        }
        var want = slice<@string>([tp, "postClear"]);
        var avoid = slice<@string>(["preClear", "main", "final"]);
        @string msg = testForSpecificFunctions(t, edir, want, avoid); if (msg != ""u8) {
            t.Logf("%s", output);
            t.Errorf("coverage data from %q output match failed: %s", tp, msg);
        }
        upmergeCoverData(t, edir, atomicˢ);
        upmergeCoverData(t, rdir, atomicˢ);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nonatomdirˢ => ref nonatomdirˢᵇ;
private static readonly @string nonatomdirˢᵇ = "nonatomdir"u8;
internal static ref readonly @string writeCountersDirInvokedˢ => ref writeCountersDirInvokedˢᵇ;
private static readonly @string writeCountersDirInvokedˢᵇ = "WriteCountersDir invoked for program built"u8;

internal static void testEmitToDirNonAtomic(ж<testing.T> t, @string harnessPath, @string naMode, @string dir) {
    @string tp = emitToDirˢ;
    @string tag = nonatomdirˢ;
    var (rdir, edir) = mktestdirs(t, tag, tp, dir);
    var (output, err) = runHarness(t, harnessPath, tp,
        true, rdir, edir);

    // We expect an error here.
    if (err == nil) {
        t.Logf("%s", output);
        t.Fatalf("running 'harness -tp %s': did not get expected error", tp);
    }

    @string got = strings.TrimSpace((@string)output);
    @string want = writeCountersDirInvokedˢ;
    if (!strings.Contains(got, want)) {
        t.Errorf("running 'harness -tp %s': got:\n%s\nwant: %s",
            tp, got, want);
    }
    upmergeCoverData(t, edir, naMode);
    upmergeCoverData(t, rdir, naMode);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nonatomwˢ => ref nonatomwˢᵇ;
private static readonly @string nonatomwˢᵇ = "nonatomw"u8;
internal static ref readonly @string writeCountersInvokedForˢ => ref writeCountersInvokedForˢᵇ;
private static readonly @string writeCountersInvokedForˢᵇ = "WriteCounters invoked for program built"u8;

internal static void testEmitToWriterNonAtomic(ж<testing.T> t, @string harnessPath, @string naMode, @string dir) {
    @string tp = emitToWriterˢ;
    @string tag = nonatomwˢ;
    var (rdir, edir) = mktestdirs(t, tag, tp, dir);
    var (output, err) = runHarness(t, harnessPath, tp,
        true, rdir, edir);

    // We expect an error here.
    if (err == nil) {
        t.Logf("%s", output);
        t.Fatalf("running 'harness -tp %s': did not get expected error", tp);
    }

    @string got = strings.TrimSpace((@string)output);
    @string want = writeCountersInvokedForˢ;
    if (!strings.Contains(got, want)) {
        t.Errorf("running 'harness -tp %s': got:\n%s\nwant: %s",
            tp, got, want);
    }

    upmergeCoverData(t, edir, naMode);
    upmergeCoverData(t, rdir, naMode);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cclearˢ => ref cclearˢᵇ;
private static readonly @string cclearˢᵇ = "cclear"u8;
internal static ref readonly @string clearCountersInvokedForˢ => ref clearCountersInvokedForˢᵇ;
private static readonly @string clearCountersInvokedForˢᵇ = "ClearCounters invoked for program built"u8;

internal static void testEmitWithCounterClearNonAtomic(ж<testing.T> t, @string harnessPath, @string naMode, @string dir) {
    @string tp = emitWithCounterClearˢ;
    @string tag = cclearˢ;
    var (rdir, edir) = mktestdirs(t, tag, tp, dir);
    var (output, err) = runHarness(t, harnessPath, tp,
        true, rdir, edir);

    // We expect an error here.
    if (err == nil) {
        t.Logf("%s", output);
        t.Fatalf("running 'harness -tp %s' nonatomic: did not get expected error", tp);
    }

    @string got = strings.TrimSpace((@string)output);
    @string want = clearCountersInvokedForˢ;
    if (!strings.Contains(got, want)) {
        t.Errorf("running 'harness -tp %s': got:\n%s\nwant: %s",
            tp, got, want);
    }

    upmergeCoverData(t, edir, naMode);
    upmergeCoverData(t, rdir, naMode);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nocoverˢ => ref nocoverˢᵇ;
private static readonly @string nocoverˢᵇ = "nocover"u8;
internal static ref readonly @string emitDirNoˢ => ref emitDirNoˢᵇ;
private static readonly @string emitDirNoˢᵇ = "emitDirNo"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = "not built with -cover"u8;

public static void TestApisOnNocoverBinary(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skipf("skipping test: too long for short mode");
    }
    testenv.MustHaveGoBuild(new cfile_internal_test_package.testing_TжTB(t));
    @string dir = t.TempDir();

    // Build harness with no -cover.
    @string bdir = mkdir(t, filepath.Join(dir, nocoverˢ));
    @string edir = mkdir(t, filepath.Join(dir, emitDirNoˢ));
    @string harnessPath = buildHarness(t, bdir, default);
    var (output, err) = runHarness(t, harnessPath, emitToDirˢ, false, edir, edir);
    if (err == nil) {
        t.Fatalf("expected error on TestApisOnNocoverBinary harness run");
    }
    @string want = wantᶜ;
    if (!strings.Contains(output, want)) {
        t.Errorf("error output does not contain %q: %s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippedDueToLackOfRaceˢ = (@string)"skipped due to lack of race detector support / CGO"u8;
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "test"u8;
internal static ref readonly @string coverˢ => ref coverˢᵇ;
private static readonly @string coverˢᵇ = "-cover"u8;
internal static ref readonly @string raceˢ => ref raceˢᵇ;
private static readonly @string raceˢᵇ = "-race"u8;
internal static ref readonly @string issue56006ˢ => ref issue56006ˢᵇ;
private static readonly @string issue56006ˢᵇ = "issue56006"u8;

public static void TestIssue56006EmitDataRaceCoverRunningGoroutine(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skipf("skipping test: too long for short mode");
    }

    // This test requires "go test -race -cover", meaning that we need
    // go build, go run, and "-race" support.
    testenv.MustHaveGoRun(new cfile_internal_test_package.testing_TжTB(t));
    if (!platform.RaceDetectorSupported(runtime.GOOS, runtime.GOARCH) || !testenv.HasCGO()) {
        t.Skip(skippedDueToLackOfRaceˢ);
    }

    // This will run a program with -cover and -race where we have a
    // goroutine still running (and updating counters) at the point where
    // the test runtime is trying to write out counter data.
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(t)), testˢ, coverˢ, raceˢ);
    cmd.Dir = filepath.Join(testdataˢ, issue56006ˢ);
    var (b, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("go test -cover -race failed: %v\n%s", err, b);
    }

    // Don't want to see any data races in output.
    var avoid = slice<@string>(["DATA RACE"]);
    foreach (var (_, no) in range(avoid)) {
        if (strings.Contains((@string)b, no)) {
            t.Logf("%s\n", (@string)b);
            t.Fatalf("found %s in test output, not permitted", no);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooCovˢ => ref fooCovˢᵇ;
private static readonly @string fooCovˢᵇ = "foo.cov"u8;
internal static ref readonly @string coverpkgAllˢ => ref coverpkgAllˢᵇ;
private static readonly @string coverpkgAllˢᵇ = "-coverpkg=all"u8;
internal static ref readonly @string issue59563ˢ => ref issue59563ˢᵇ;
private static readonly @string issue59563ˢᵇ = "issue59563"u8;
internal static ref readonly @string toolˢ => ref toolˢᵇ;
private static readonly @string toolˢᵇ = "tool"u8;
internal static ref readonly @string coverˢ2 => ref coverˢ2ᵇ;
private static readonly @string coverˢ2ᵇ = "cover"u8;
internal static ref readonly @string internalCoverageCfileˢ => ref internalCoverageCfileˢᵇ;
private static readonly @string internalCoverageCfileˢᵇ = "internal/coverage/cfile/testdata/issue59563/repro.go"u8;
internal static ref readonly @string largeˢ => ref largeˢᵇ;
private static readonly @string largeˢᵇ = "large"u8;

public static void TestIssue59563TruncatedCoverPkgAll(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skipf("skipping test: too long for short mode");
    }
    testenv.MustHaveGoRun(new cfile_internal_test_package.testing_TжTB(t));

    @string tmpdir = t.TempDir();
    @string ppath = filepath.Join(tmpdir, fooCovˢ);

    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(t)), testˢ, coverpkgAllˢ, "-coverprofile=" + ppath);
    cmd.Dir = filepath.Join(testdataˢ, issue59563ˢ);
    var (b, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("go test -cover failed: %v\n%s", err, b);
    }

    cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(t)), toolˢ, coverˢ2, "-func=" + ppath);
    (b, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("go tool cover -func failed: %v", err);
    }

    var lines = strings.Split((@string)b, "\n");
    nint nfound = 0;
    var bad = false;
    foreach (var (_, line) in range(lines)) {
        var f = strings.Fields(line);
        if (len(f) == 0) {
            continue;
        }
        // We're only interested in the specific function "large" for
        // the testcase being built. See the #59563 for details on why
        // size matters.
        if (!(strings.HasPrefix(f[0], internalCoverageCfileˢ) && strings.Contains(line, largeˢ))) {
            continue;
        }
        nfound++;
        @string want = "100.0%";
        if (f[len(f) - 1] != want) {
            t.Errorf("wanted %s got: %q\n", want, line);
            bad = true;
        }
    }
    if (nfound != 1) {
        t.Errorf("wanted 1 found, got %d\n", nfound);
        bad = true;
    }
    if (bad) {
        t.Logf("func output:\n%s\n", (@string)b);
    }
}

} // end cfile_internal_test_package
