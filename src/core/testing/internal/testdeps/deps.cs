// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package testdeps provides access to dependencies needed by test execution.
//
// This package is imported by the generated main package, which passes
// TestDeps into testing.Main. This allows tests to use packages at run time
// without making those packages direct dependencies of package testing.
// Direct dependencies of package testing are harder to write tests for.
namespace go.testing.@internal;

using bufio = bufio_package;
using context = context_package;
using fuzz = global::go.@internal.fuzz_package;
using testlog = global::go.@internal.testlog_package;
using io = io_package;
using os = os_package;
using signal = global::go.os.signal_package;
using reflect = reflect_package;
using regexp = regexp_package;
using pprof = runtime.pprof_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using global::go.@internal;
using global::go.os;
using runtime;

partial class testdeps_package {

// Cover indicates whether coverage is enabled.
public static bool Cover;

// TestDeps is an implementation of the testing.testDeps interface,
// suitable for passing to [testing.MainStart].
partial struct TestDeps {
}

internal static @string matchPat;

internal static ж<regexp.Regexp> matchRe;

public static (bool result, error err) MatchString(this TestDeps _, @string pat, @string str) {
    bool result = default!;
    error err = default!;

    if (matchRe == nil || matchPat != pat) {
        matchPat = pat;
        (matchRe, err) = regexp.Compile(matchPat);
        if (err != default!) {
            return (result, err);
        }
    }
    return (matchRe.MatchString(str), default!);
}

public static error StartCPUProfile(this TestDeps _, io.Writer w) {
    return pprof.StartCPUProfile(w);
}

public static void StopCPUProfile(this TestDeps _) {
    pprof.StopCPUProfile();
}

public static error WriteProfileTo(this TestDeps _, @string name, io.Writer w, nint debug) {
    return pprof.Lookup(name).WriteTo(w, debug);
}

// ImportPath is the import path of the testing binary, set by the generated main function.
public static @string ΔImportPath;

public static @string ImportPath(this TestDeps _) {
    return ΔImportPath;
}

// testLog implements testlog.Interface, logging actions by package os.
partial struct testLog {
    internal sync.Mutex mu;
    internal ж<bufio.Writer> w;
    internal bool set;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string getenvˢ = "getenv"u8;

internal static void Getenv(this ж<testLog> Ꮡl, @string key) {
    Ꮡl.add(getenvˢ, key);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string openˢ = "open"u8;

internal static void Open(this ж<testLog> Ꮡl, @string name) {
    Ꮡl.add(openˢ, name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string statˢ = "stat"u8;

internal static void Stat(this ж<testLog> Ꮡl, @string name) {
    Ꮡl.add(statˢ, name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string chdirˢ = "chdir"u8;

internal static void Chdir(this ж<testLog> Ꮡl, @string name) {
    Ꮡl.add(chdirˢ, name);
}

// add adds the (op, name) pair to the test log.
internal static void add(this ж<testLog> Ꮡl, @string op, @string name) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var l = ref Ꮡl.DerefOrNull();

        if (strings.Contains(name, "\n"u8) || name == ""u8) {
            return;
        }
        l.mu.Lock();
        ᒐd1 = true;
        if (l.w == nil) {
            return;
        }
        l.w.WriteString(op);
        l.w.WriteByte((rune)' ');
        l.w.WriteString(name);
        l.w.WriteByte((rune)'\n');
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡl.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static ж<testLog> Ꮡlog = new StandardBox<testLog>(default(testLog));
internal static ref testLog log => ref Ꮡlog.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testLogˢ = "# test log\n"u8;

public static void StartTestLog(this TestDeps _, io.Writer w) {
    Ꮡlog.of(testLog.Ꮡmu).Lock();
    log.w = bufio.NewWriter(w);
    if (!log.set) {
        // Tests that define TestMain and then run m.Run multiple times
        // will call StartTestLog/StopTestLog multiple times.
        // Checking log.set avoids calling testlog.SetLogger multiple times
        // (which will panic) and also avoids writing the header multiple times.
        log.set = true;
        testlog.SetLogger(new testLogжInterface(Ꮡlog));
        log.w.WriteString(testLogˢ); // known to cmd/go/internal/test/test.go
    }
    Ꮡlog.of(testLog.Ꮡmu).Unlock();
}

public static error StopTestLog(this TestDeps _) {
    GoFrame ᒐ = default;
    try {
        Ꮡlog.of(testLog.Ꮡmu).Lock();
        defer(Ꮡlog.of(testLog.Ꮡmu).Unlock, ref ᒐ);
        var err = log.w.Flush();
        log.w = default!;
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// SetPanicOnExit0 tells the os package whether to panic on os.Exit(0).
public static void SetPanicOnExit0(this TestDeps _, bool v) {
    testlog.SetPanicOnExit0(v);
}

public static error /*err*/ CoordinateFuzzing(this TestDeps _, time.Duration timeout, int64 limit, time.Duration minimizeTimeout, int64 minimizeLimit, nint parallel, slice<fuzzꓸCorpusEntry> seed, slice<reflectꓸType> types, @string corpusDir, @string cacheDir) {
    error err = default!;
    GoFrame ᒐ = default;
    try {
        // Fuzzing may be interrupted with a timeout or if the user presses ^C.
        // In either case, we'll stop worker processes gracefully and save
        // crashers and interesting values.
        var (ctx, cancel) = signal.NotifyContext(context.Background(), os.Interrupt);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        err = fuzz.CoordinateFuzzing(ctx, new fuzz.CoordinateFuzzingOpts(
            Log: new os.FileжWriter(os.Stderr),
            Timeout: timeout,
            Limit: limit,
            MinimizeTimeout: minimizeTimeout,
            MinimizeLimit: minimizeLimit,
            Parallel: parallel,
            Seed: seed,
            Types: types,
            CorpusDir: corpusDir,
            CacheDir: cacheDir
        ));
        if (AreEqual(err, ctx.Err())) {
            err = default!; goto ᒐdone;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

public static error RunFuzzWorker(this TestDeps _, Func<fuzzꓸCorpusEntry, error> fn) {
    GoFrame ᒐ = default;
    try {
        // Worker processes may or may not receive a signal when the user presses ^C
        // On POSIX operating systems, a signal sent to a process group is delivered
        // to all processes in that group. This is not the case on Windows.
        // If the worker is interrupted, return quickly and without error.
        // If only the coordinator process is interrupted, it tells each worker
        // process to stop by closing its "fuzz_in" pipe.
        var (ctx, cancel) = signal.NotifyContext(context.Background(), os.Interrupt);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var err = fuzz.RunFuzzWorker(ctx, fn);
        if (AreEqual(err, ctx.Err())) {
            return default!;
        }
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

public static (slice<fuzzꓸCorpusEntry>, error) ReadCorpus(this TestDeps _, @string dir, slice<reflectꓸType> types) {
    return fuzz.ReadCorpus(dir, types);
}

public static error CheckCorpus(this TestDeps _, slice<any> vals, slice<reflectꓸType> types) {
    return fuzz.CheckCorpus(vals, types);
}

public static void ResetCoverage(this TestDeps _) {
    fuzz.ResetCoverage();
}

public static void SnapshotCoverage(this TestDeps _) {
    fuzz.SnapshotCoverage();
}

public static @string CoverMode;

public static @string Covered;

public static slice<@string> CoverSelectedPackages;

// These variables below are set at runtime (via code in testmain) to point
// to the equivalent functions in package internal/coverage/cfile; doing
// things this way allows us to have tests import internal/coverage/cfile
// only when -cover is in effect (as opposed to importing for all tests).
public static Func<float64> CoverSnapshotFunc;

public static Func<@string, @string, @string, @string, io.Writer, slice<@string>, error> CoverProcessTestDirFunc;

public static Action<bool> CoverMarkProfileEmittedFunc;

public static (@string mode, Func<@string, @string, (@string, error)> tearDown, Func<float64> snapcov) InitRuntimeCoverage(this TestDeps _) {
    @string mode = default!;
    Func<@string, @string, (@string, error)> tearDown = default!;
    Func<float64> snapcov = default!;

    if (CoverMode == ""u8) {
        return (mode, tearDown, snapcov);
    }
    return (CoverMode, coverTearDown, CoverSnapshotFunc);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string gocoverdirˢ = "gocoverdir"u8;
private static readonly @string errorSettingGocoverdirˢ = "error setting GOCOVERDIR: bad os.MkdirTemp return"u8;
private static readonly @string errorGeneratingCoverageˢ = "error generating coverage report"u8;

internal static (@string, error) coverTearDown(@string coverprofile, @string gocoverdir) {
    GoFrame ᒐ = default;
    try {
        error err = default!;
        if (gocoverdir == ""u8) {
            (gocoverdir, err) = os.MkdirTemp(""u8, gocoverdirˢ);
            if (err != default!) {
                return (errorSettingGocoverdirˢ, err);
            }
            defer(os.RemoveAll, gocoverdir, ref ᒐ);
        }
        CoverMarkProfileEmittedFunc(true);
        @string cmode = CoverMode;
        {
            var errΔ1 = CoverProcessTestDirFunc(gocoverdir, coverprofile, cmode, Covered, new os.FileжWriter(os.Stdout), CoverSelectedPackages); if (errΔ1 != default!) {
                return (errorGeneratingCoverageˢ, errΔ1);
            }
        }
        return ("", default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end testdeps_package
