// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using fmt = fmt_package;
using coverage = go.@internal.coverage_package;
using goexperiment = go.@internal.goexperiment_package;
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
internal static readonly @string tmpQqqzzzˢ = "/tmp/qqqzzz"u8;
internal static readonly @string build1ˢ = "build1"u8;
internal static readonly @string setˢ = "set"u8;
internal static readonly @string build2ˢ = "build2"u8;
internal static readonly @string emitToDirˢ = "emitToDir"u8;
internal static readonly @string emitToWriterˢ = "emitToWriter"u8;
internal static readonly @string emitToNonexistentDirˢ = "emitToNonexistentDir"u8;
internal static readonly @string emitToNilWriterˢ = "emitToNilWriter"u8;
internal static readonly @string emitToFailingWriterˢ = "emitToFailingWriter"u8;
internal static readonly @string emitWithCounterClearˢ = "emitWithCounterClear"u8;
internal static readonly @string emitToDirNonAtomicˢ = "emitToDirNonAtomic"u8;
internal static readonly @string emitToWriterNonAtomicˢ = "emitToWriterNonAtomic"u8;

public static void TestCoverageApis(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skipf("skipping test: too long for short mode"u8);
    }
    if (!goexperiment.CoverageRedesign) {
        Ꮡt.Skipf("skipping new coverage tests (experiment not enabled)"u8);
    }
    testenv.MustHaveGoBuild(new cfile_internal_test_package.testing_TжTB(Ꮡt));
    @string dir = Ꮡt.TempDir();
    if (fixedTestDir) {
        dir = tmpQqqzzzˢ;
        os.RemoveAll(dir);
        mkdir(Ꮡt, dir);
    }
    // Build harness. We need two copies of the harness, one built
    // with -covermode=atomic and one built non-atomic.
    @string bdir1 = mkdir(Ꮡt, filepath.Join(dir, build1ˢ));
    var hargs1 = new @string[]{"-covermode=atomic"u8, "-coverpkg=all"u8}.slice();
    @string atomicHarnessPath = buildHarness(Ꮡt, bdir1, hargs1);
    @string nonAtomicMode = testing.CoverMode();
    if (testing.CoverMode() == "atomic"u8) {
        nonAtomicMode = setˢ;
    }
    @string bdir2 = mkdir(Ꮡt, filepath.Join(dir, build2ˢ));
    var hargs2 = new @string[]{"-coverpkg=all"u8, "-covermode="u8 + nonAtomicMode}.slice();
    @string nonAtomicHarnessPath = buildHarness(Ꮡt, bdir2, hargs2);
    Ꮡt.Logf("atomic harness path is %s"u8, atomicHarnessPath);
    Ꮡt.Logf("non-atomic harness path is %s"u8, nonAtomicHarnessPath);
    // Sub-tests for each API we want to inspect, plus
    // extras for error testing.
    Ꮡt.Run(emitToDirˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Parallel();
        testEmitToDir(tΔ1, atomicHarnessPath, dir);
    });
    Ꮡt.Run(emitToWriterˢ, (ж<testing.T> tΔ2) => {
        tΔ2.Parallel();
        testEmitToWriter(tΔ2, atomicHarnessPath, dir);
    });
    Ꮡt.Run(emitToNonexistentDirˢ, (ж<testing.T> tΔ3) => {
        tΔ3.Parallel();
        testEmitToNonexistentDir(tΔ3, atomicHarnessPath, dir);
    });
    Ꮡt.Run(emitToNilWriterˢ, (ж<testing.T> tΔ4) => {
        tΔ4.Parallel();
        testEmitToNilWriter(tΔ4, atomicHarnessPath, dir);
    });
    Ꮡt.Run(emitToFailingWriterˢ, (ж<testing.T> tΔ5) => {
        tΔ5.Parallel();
        testEmitToFailingWriter(tΔ5, atomicHarnessPath, dir);
    });
    Ꮡt.Run(emitWithCounterClearˢ, (ж<testing.T> tΔ6) => {
        tΔ6.Parallel();
        testEmitWithCounterClear(tΔ6, atomicHarnessPath, dir);
    });
    Ꮡt.Run(emitToDirNonAtomicˢ, (ж<testing.T> tΔ7) => {
        tΔ7.Parallel();
        testEmitToDirNonAtomic(tΔ7, nonAtomicHarnessPath, nonAtomicMode, dir);
    });
    Ꮡt.Run(emitToWriterNonAtomicˢ, (ж<testing.T> tΔ8) => {
        tΔ8.Parallel();
        testEmitToWriterNonAtomic(tΔ8, nonAtomicHarnessPath, nonAtomicMode, dir);
    });
    Ꮡt.Run("emitWithCounterClearNonAtomic"u8, (ж<testing.T> tΔ9) => {
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
internal static void upmergeCoverData(ж<testing.T> Ꮡt, @string gocoverdir, @string mode) {
    if (testing.CoverMode() != mode) {
        return;
    }
    @string testGoCoverDir = os.Getenv(gocoverdirˢ);
    if (testGoCoverDir == ""u8) {
        return;
    }
    var args = new @string[]{"tool"u8, "covdata"u8, "merge"u8, "-pkg=runtime/coverage"u8,
        "-o"u8, testGoCoverDir, "-i"u8, gocoverdir}.slice();
    Ꮡt.Logf("up-merge of covdata from %s to %s"u8, gocoverdir, testGoCoverDir);
    Ꮡt.Logf("executing: go %+v"u8, args);
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(Ꮡt)), args.ꓸꓸꓸ);
    {
        var (b, err) = cmd.CombinedOutput(); if (err != default!) {
            Ꮡt.Fatalf("covdata merge failed (%v): %s"u8, err, b);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string harnessExeˢ = "harness.exe"u8;
internal static readonly @string testdataˢ = "testdata"u8;
internal static readonly @string harnessGoˢ = "harness.go"u8;

// buildHarness builds the helper program "harness.exe".
internal static @string buildHarness(ж<testing.T> Ꮡt, @string dir, slice<@string> opts) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string harnessPath = filepath.Join(dir, harnessExeˢ);
    @string harnessSrc = filepath.Join(testdataˢ, harnessGoˢ);
    var args = new @string[]{"build"u8, "-o"u8, harnessPath}.slice();
    args = appendꓸꓸꓸ(args, opts);
    args = append(args, harnessSrc);
    //t.Logf("harness build: go %+v\n", args)
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(Ꮡt)), args.ꓸꓸꓸ);
    {
        var (b, err) = cmd.CombinedOutput(); if (err != default!) {
            Ꮡt.Fatalf("build failed (%v): %s"u8, err, b);
        }
    }
    return harnessPath;
}

internal static @string mkdir(ж<testing.T> Ꮡt, @string d) {
    Ꮡt.Helper();
    {
        var err = os.Mkdir(d, 511); if (err != default!) {
            Ꮡt.Fatalf("mkdir failed: %v"u8, err);
        }
    }
    return d;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gocoverdirˢ2 = "GOCOVERDIR="u8;

// updateGoCoverDir updates the specified environment 'env' to set
// GOCOVERDIR to 'gcd' (if setGoCoverDir is TRUE) or removes
// GOCOVERDIR from the environment (if setGoCoverDir is false).
internal static slice<@string> updateGoCoverDir(slice<@string> env, @string gcd, bool setGoCoverDir) {
    var rv = new @string[]{}.slice();
    var found = false;
    foreach (var (_, vᴛ1) in env) {
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

internal static (@string, error) runHarness(ж<testing.T> Ꮡt, @string harnessPath, @string tp, bool setGoCoverDir, @string rdir, @string edir) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Logf("running: %s -tp %s -o %s with rdir=%s and GOCOVERDIR=%v"u8, harnessPath, tp, edir, rdir, setGoCoverDir);
    var cmd = exec.Command(harnessPath, "-tp"u8, tp, "-o", edir);
    cmd.Value.Dir = rdir;
    cmd.Value.Env = updateGoCoverDir(os.Environ(), rdir, setGoCoverDir);
    var (b, err) = cmd.CombinedOutput();
    //t.Logf("harness run output: %s\n", string(b))
    return (((@string)b), err);
}

internal static @string testForSpecificFunctions(ж<testing.T> Ꮡt, @string dir, slice<@string> want, slice<@string> avoid) {
    var args = new @string[]{"tool"u8, "covdata"u8, "debugdump"u8,
        "-live"u8, "-pkg=command-line-arguments"u8, "-i="u8 + dir}.slice();
    Ꮡt.Logf("running: go %v\n"u8, args);
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(Ꮡt)), args.ꓸꓸꓸ);
    var (b, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("'go tool covdata failed (%v): %s"u8, err, b);
    }
    @string output = ((@string)b);
    @string rval = ""u8;
    foreach (var (_, f) in want) {
        @string wf = "Func: "u8 + f + "\n"u8;
        if (strings.Contains(output, wf)) {
            continue;
        }
        rval += fmt.Sprintf("error: output should contain %q but does not\n"u8, wf);
    }
    foreach (var (_, f) in avoid) {
        @string wf = "Func: "u8 + f + "\n"u8;
        if (strings.Contains(output, wf)) {
            rval += fmt.Sprintf("error: output should not contain %q but does\n"u8, wf);
        }
    }
    if (rval != ""u8) {
        Ꮡt.Logf("=-= begin output:\n%s\n=-= end output\n"u8, output);
    }
    return rval;
}

internal static void withAndWithoutRunner(Action<bool, @string> f) {
    // Run 'f' with and without GOCOVERDIR set.
    for (nint i = 0; i < 2; i++) {
        @string tag = "x"u8;
        var setGoCoverDir = true;
        if (i == 0) {
            setGoCoverDir = false;
            tag = "y"u8;
        }
        f(setGoCoverDir, tag);
    }
}

internal static (@string, @string) mktestdirs(ж<testing.T> Ꮡt, @string tag, @string tp, @string dir) {
    Ꮡt.Helper();
    @string rdir = mkdir(Ꮡt, filepath.Join(dir, tp + "-rdir-" + tag));
    @string edir = mkdir(Ꮡt, filepath.Join(dir, tp + "-edir-" + tag));
    return (rdir, edir);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string atomicˢ = "atomic"u8;

internal static void testEmitToDir(ж<testing.T> Ꮡt, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        @string tp = emitToDirˢ;
        var (rdir, edir) = mktestdirs(Ꮡt, tag, tp, dir);
        var (output, err) = runHarness(Ꮡt, harnessPath, tp,
            setGoCoverDir, rdir, edir);
        if (err != default!) {
            Ꮡt.Logf("%s"u8, output);
            Ꮡt.Fatalf("running 'harness -tp emitDir': %v"u8, err);
        }
        // Just check to make sure meta-data file and counter data file were
        // written. Another alternative would be to run "go tool covdata"
        // or equivalent, but for now, this is what we've got.
        (var dents, err) = os.ReadDir(edir);
        if (err != default!) {
            Ꮡt.Fatalf("os.ReadDir(%s) failed: %v"u8, edir, err);
        }
        nint mfc = 0;
        nint cdc = 0;
        foreach (var (_, e) in dents) {
            if (e.IsDir()) {
                continue;
            }
            if (strings.HasPrefix(e.Name(), coverage.MetaFilePref)){
                mfc++;
            } else 
            if (strings.HasPrefix(e.Name(), coverage.CounterFilePref)) {
                cdc++;
            }
        }
        nint wantmf = 1;
        nint wantcf = 1;
        if (mfc != wantmf) {
            Ꮡt.Errorf("EmitToDir: want %d meta-data files, got %d\n"u8, wantmf, mfc);
        }
        if (cdc != wantcf) {
            Ꮡt.Errorf("EmitToDir: want %d counter-data files, got %d\n"u8, wantcf, cdc);
        }
        upmergeCoverData(Ꮡt, edir, atomicˢ);
        upmergeCoverData(Ꮡt, rdir, atomicˢ);
    });
}

internal static void testEmitToWriter(ж<testing.T> Ꮡt, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        @string tp = emitToWriterˢ;
        var (rdir, edir) = mktestdirs(Ꮡt, tag, tp, dir);
        var (output, err) = runHarness(Ꮡt, harnessPath, tp, setGoCoverDir, rdir, edir);
        if (err != default!) {
            Ꮡt.Logf("%s"u8, output);
            Ꮡt.Fatalf("running 'harness -tp %s': %v"u8, tp, err);
        }
        var want = new @string[]{"main"u8, tp}.slice();
        var avoid = new @string[]{"final"u8}.slice();
        {
            @string msg = testForSpecificFunctions(Ꮡt, edir, want, avoid); if (msg != ""u8) {
                Ꮡt.Errorf("coverage data from %q output match failed: %s"u8, tp, msg);
            }
        }
        upmergeCoverData(Ꮡt, edir, atomicˢ);
        upmergeCoverData(Ꮡt, rdir, atomicˢ);
    });
}

internal static void testEmitToNonexistentDir(ж<testing.T> Ꮡt, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        @string tp = emitToNonexistentDirˢ;
        var (rdir, edir) = mktestdirs(Ꮡt, tag, tp, dir);
        var (output, err) = runHarness(Ꮡt, harnessPath, tp, setGoCoverDir, rdir, edir);
        if (err != default!) {
            Ꮡt.Logf("%s"u8, output);
            Ꮡt.Fatalf("running 'harness -tp %s': %v"u8, tp, err);
        }
        upmergeCoverData(Ꮡt, edir, atomicˢ);
        upmergeCoverData(Ꮡt, rdir, atomicˢ);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string emitToUnwritableDirˢ = "emitToUnwritableDir"u8;

internal static void testEmitToUnwritableDir(ж<testing.T> Ꮡt, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        GoFrame ᒐ = default;
        try {
            @string tp = emitToUnwritableDirˢ;
            var (rdir, edir) = mktestdirs(Ꮡt, tag, tp, dir);
            // Make edir unwritable.
            {
                var errΔ1 = os.Chmod(edir, 365); if (errΔ1 != default!) {
                    Ꮡt.Fatalf("chmod failed: %v"u8, errΔ1);
                }
            }
            defer(os.Chmod, edir, (fs.FileMode)(511), ref ᒐ);
            var (output, err) = runHarness(Ꮡt, harnessPath, tp, setGoCoverDir, rdir, edir);
            if (err != default!) {
                Ꮡt.Logf("%s"u8, output);
                Ꮡt.Fatalf("running 'harness -tp %s': %v"u8, tp, err);
            }
            upmergeCoverData(Ꮡt, edir, atomicˢ);
            upmergeCoverData(Ꮡt, rdir, atomicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

internal static void testEmitToNilWriter(ж<testing.T> Ꮡt, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        @string tp = emitToNilWriterˢ;
        var (rdir, edir) = mktestdirs(Ꮡt, tag, tp, dir);
        var (output, err) = runHarness(Ꮡt, harnessPath, tp, setGoCoverDir, rdir, edir);
        if (err != default!) {
            Ꮡt.Logf("%s"u8, output);
            Ꮡt.Fatalf("running 'harness -tp %s': %v"u8, tp, err);
        }
        upmergeCoverData(Ꮡt, edir, atomicˢ);
        upmergeCoverData(Ꮡt, rdir, atomicˢ);
    });
}

internal static void testEmitToFailingWriter(ж<testing.T> Ꮡt, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        @string tp = emitToFailingWriterˢ;
        var (rdir, edir) = mktestdirs(Ꮡt, tag, tp, dir);
        var (output, err) = runHarness(Ꮡt, harnessPath, tp, setGoCoverDir, rdir, edir);
        if (err != default!) {
            Ꮡt.Logf("%s"u8, output);
            Ꮡt.Fatalf("running 'harness -tp %s': %v"u8, tp, err);
        }
        upmergeCoverData(Ꮡt, edir, atomicˢ);
        upmergeCoverData(Ꮡt, rdir, atomicˢ);
    });
}

internal static void testEmitWithCounterClear(ж<testing.T> Ꮡt, @string harnessPath, @string dir) {
    withAndWithoutRunner((bool setGoCoverDir, @string tag) => {
        @string tp = emitWithCounterClearˢ;
        var (rdir, edir) = mktestdirs(Ꮡt, tag, tp, dir);
        var (output, err) = runHarness(Ꮡt, harnessPath, tp,
            setGoCoverDir, rdir, edir);
        if (err != default!) {
            Ꮡt.Logf("%s"u8, output);
            Ꮡt.Fatalf("running 'harness -tp %s': %v"u8, tp, err);
        }
        var want = new @string[]{tp, "postClear"u8}.slice();
        var avoid = new @string[]{"preClear"u8, "main"u8, "final"u8}.slice();
        {
            @string msg = testForSpecificFunctions(Ꮡt, edir, want, avoid); if (msg != ""u8) {
                Ꮡt.Logf("%s"u8, output);
                Ꮡt.Errorf("coverage data from %q output match failed: %s"u8, tp, msg);
            }
        }
        upmergeCoverData(Ꮡt, edir, atomicˢ);
        upmergeCoverData(Ꮡt, rdir, atomicˢ);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonatomdirˢ = "nonatomdir"u8;
internal static readonly @string writeCountersDirInvokedˢ = "WriteCountersDir invoked for program built"u8;

internal static void testEmitToDirNonAtomic(ж<testing.T> Ꮡt, @string harnessPath, @string naMode, @string dir) {
    @string tp = emitToDirˢ;
    @string tag = nonatomdirˢ;
    var (rdir, edir) = mktestdirs(Ꮡt, tag, tp, dir);
    var (output, err) = runHarness(Ꮡt, harnessPath, tp,
        true, rdir, edir);
    // We expect an error here.
    if (err == default!) {
        Ꮡt.Logf("%s"u8, output);
        Ꮡt.Fatalf("running 'harness -tp %s': did not get expected error"u8, tp);
    }
    @string got = strings.TrimSpace(((@string)output));
    @string want = writeCountersDirInvokedˢ;
    if (!strings.Contains(got, want)) {
        Ꮡt.Errorf("running 'harness -tp %s': got:\n%s\nwant: %s"u8,
            tp, got, want);
    }
    upmergeCoverData(Ꮡt, edir, naMode);
    upmergeCoverData(Ꮡt, rdir, naMode);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonatomwˢ = "nonatomw"u8;
internal static readonly @string writeCountersInvokedForˢ = "WriteCounters invoked for program built"u8;

internal static void testEmitToWriterNonAtomic(ж<testing.T> Ꮡt, @string harnessPath, @string naMode, @string dir) {
    @string tp = emitToWriterˢ;
    @string tag = nonatomwˢ;
    var (rdir, edir) = mktestdirs(Ꮡt, tag, tp, dir);
    var (output, err) = runHarness(Ꮡt, harnessPath, tp,
        true, rdir, edir);
    // We expect an error here.
    if (err == default!) {
        Ꮡt.Logf("%s"u8, output);
        Ꮡt.Fatalf("running 'harness -tp %s': did not get expected error"u8, tp);
    }
    @string got = strings.TrimSpace(((@string)output));
    @string want = writeCountersInvokedForˢ;
    if (!strings.Contains(got, want)) {
        Ꮡt.Errorf("running 'harness -tp %s': got:\n%s\nwant: %s"u8,
            tp, got, want);
    }
    upmergeCoverData(Ꮡt, edir, naMode);
    upmergeCoverData(Ꮡt, rdir, naMode);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cclearˢ = "cclear"u8;
internal static readonly @string clearCountersInvokedForˢ = "ClearCounters invoked for program built"u8;

internal static void testEmitWithCounterClearNonAtomic(ж<testing.T> Ꮡt, @string harnessPath, @string naMode, @string dir) {
    @string tp = emitWithCounterClearˢ;
    @string tag = cclearˢ;
    var (rdir, edir) = mktestdirs(Ꮡt, tag, tp, dir);
    var (output, err) = runHarness(Ꮡt, harnessPath, tp,
        true, rdir, edir);
    // We expect an error here.
    if (err == default!) {
        Ꮡt.Logf("%s"u8, output);
        Ꮡt.Fatalf("running 'harness -tp %s' nonatomic: did not get expected error"u8, tp);
    }
    @string got = strings.TrimSpace(((@string)output));
    @string want = clearCountersInvokedForˢ;
    if (!strings.Contains(got, want)) {
        Ꮡt.Errorf("running 'harness -tp %s': got:\n%s\nwant: %s"u8,
            tp, got, want);
    }
    upmergeCoverData(Ꮡt, edir, naMode);
    upmergeCoverData(Ꮡt, rdir, naMode);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nocoverˢ = "nocover"u8;
internal static readonly @string emitDirNoˢ = "emitDirNo"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "not built with -cover"u8;

public static void TestApisOnNocoverBinary(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skipf("skipping test: too long for short mode"u8);
    }
    testenv.MustHaveGoBuild(new cfile_internal_test_package.testing_TжTB(Ꮡt));
    @string dir = Ꮡt.TempDir();
    // Build harness with no -cover.
    @string bdir = mkdir(Ꮡt, filepath.Join(dir, nocoverˢ));
    @string edir = mkdir(Ꮡt, filepath.Join(dir, emitDirNoˢ));
    @string harnessPath = buildHarness(Ꮡt, bdir, default!);
    var (output, err) = runHarness(Ꮡt, harnessPath, emitToDirˢ, false, edir, edir);
    if (err == default!) {
        Ꮡt.Fatalf("expected error on TestApisOnNocoverBinary harness run"u8);
    }
    @string want = wantᶜ;
    if (!strings.Contains(output, want)) {
        Ꮡt.Errorf("error output does not contain %q: %s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippedDueToLackOfRaceˢ = (@string)"skipped due to lack of race detector support / CGO"u8;
internal static readonly @string testˢ = "test"u8;
internal static readonly @string coverˢ = "-cover"u8;
internal static readonly @string raceˢ = "-race"u8;
internal static readonly @string issue56006ˢ = "issue56006"u8;

public static void TestIssue56006EmitDataRaceCoverRunningGoroutine(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (testing.Short()) {
        Ꮡt.Skipf("skipping test: too long for short mode"u8);
    }
    if (!goexperiment.CoverageRedesign) {
        Ꮡt.Skipf("skipping new coverage tests (experiment not enabled)"u8);
    }
    // This test requires "go test -race -cover", meaning that we need
    // go build, go run, and "-race" support.
    testenv.MustHaveGoRun(new cfile_internal_test_package.testing_TжTB(Ꮡt));
    if (!platform.RaceDetectorSupported(runtime.GOOS, runtime.GOARCH) || !testenv.HasCGO()) {
        Ꮡt.Skip(skippedDueToLackOfRaceˢ);
    }
    // This will run a program with -cover and -race where we have a
    // goroutine still running (and updating counters) at the point where
    // the test runtime is trying to write out counter data.
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(Ꮡt)), testˢ, coverˢ, raceˢ);
    cmd.Value.Dir = filepath.Join(testdataˢ, issue56006ˢ);
    var (b, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go test -cover -race failed: %v\n%s"u8, err, b);
    }
    // Don't want to see any data races in output.
    var avoid = new @string[]{"DATA RACE"u8}.slice();
    foreach (var (_, no) in avoid) {
        if (strings.Contains(((@string)b), no)) {
            Ꮡt.Logf("%s\n"u8, ((@string)b));
            Ꮡt.Fatalf("found %s in test output, not permitted"u8, no);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooCovˢ = "foo.cov"u8;
internal static readonly @string coverpkgAllˢ = "-coverpkg=all"u8;
internal static readonly @string issue59563ˢ = "issue59563"u8;
internal static readonly @string toolˢ = "tool"u8;
internal static readonly @string coverˢ2 = "cover"u8;
internal static readonly @string internalCoverageCfileˢ = "internal/coverage/cfile/testdata/issue59563/repro.go"u8;
internal static readonly @string largeˢ = "large"u8;

public static void TestIssue59563TruncatedCoverPkgAll(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skipf("skipping test: too long for short mode"u8);
    }
    testenv.MustHaveGoRun(new cfile_internal_test_package.testing_TжTB(Ꮡt));
    @string tmpdir = Ꮡt.TempDir();
    @string ppath = filepath.Join(tmpdir, fooCovˢ);
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(Ꮡt)), testˢ, coverpkgAllˢ, "-coverprofile=" + ppath);
    cmd.Value.Dir = filepath.Join(testdataˢ, issue59563ˢ);
    var (b, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go test -cover failed: %v\n%s"u8, err, b);
    }
    cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(Ꮡt)), toolˢ, coverˢ2, "-func=" + ppath);
    (b, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go tool cover -func failed: %v"u8, err);
    }
    var lines = strings.Split(((@string)b), "\n"u8);
    nint nfound = 0;
    var bad = false;
    foreach (var (_, line) in lines) {
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
        @string want = "100.0%"u8;
        if (f[len(f) - 1] != want) {
            Ꮡt.Errorf("wanted %s got: %q\n"u8, want, line);
            bad = true;
        }
    }
    if (nfound != 1) {
        Ꮡt.Errorf("wanted 1 found, got %d\n"u8, nfound);
        bad = true;
    }
    if (bad) {
        Ꮡt.Logf("func output:\n%s\n"u8, ((@string)b));
    }
}

} // end cfile_internal_test_package
