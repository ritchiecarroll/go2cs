// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using os = os_package;
using regexp = regexp_package;
using testing = testing_package;
using synctest = go.testing.synctest_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using context = context_package;
using exec = go.os.exec_package;
using go.os;
using go.testing;

partial class synctest_test_package {

// Tests for interactions between synctest bubbles and the testing package.
// Other bubble behaviors are tested in internal/synctest.
public static void TestSuccess(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fatalˢ = (@string)"fatal"u8;
private static readonly @string failTestFatalSynctestˢ = """
^--- FAIL: TestFatal.*
    synctest_test.go:.* fatal
FAIL
$
"""u8.Raw();

public static void TestFatal(ж<testing.T> Ꮡt) {
    runTest(Ꮡt, default!, () => {
        synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
            tΔ1.Fatal(fatalˢ);
        });
    }, failTestFatalSynctestˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object errorˢ = (@string)"error"u8;
private static readonly @string failTestErrorSynctestˢ = """
^--- FAIL: TestError.*
    synctest_test.go:.* error
FAIL
$
"""u8.Raw();

public static void TestError(ж<testing.T> Ꮡt) {
    runTest(Ꮡt, default!, () => {
        synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
            tΔ1.Error(errorˢ);
        });
    }, failTestErrorSynctestˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string runTestVerboseErrorˢ = """
^=== RUN   TestVerboseError
    synctest_test.go:.* error
--- FAIL: TestVerboseError.*
FAIL
$
"""u8.Raw();

public static void TestVerboseError(ж<testing.T> Ꮡt) {
    runTest(Ꮡt, new @string[]{"-test.v"u8}.slice(), () => {
        synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
            tΔ1.Error(errorˢ);
        });
    }, runTestVerboseErrorˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skipˢ = (@string)"skip"u8;
private static readonly @string passˢ = """
^PASS
$
"""u8.Raw();

public static void TestSkip(ж<testing.T> Ꮡt) {
    runTest(Ꮡt, default!, () => {
        synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
            tΔ1.Skip(skipˢ);
        });
    }, passˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string runTestVerboseSkipˢ = """
^=== RUN   TestVerboseSkip
    synctest_test.go:.* skip
--- PASS: TestVerboseSkip.*
PASS
$
"""u8.Raw();

public static void TestVerboseSkip(ж<testing.T> Ꮡt) {
    runTest(Ꮡt, new @string[]{"-test.v"u8}.slice(), () => {
        synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
            tΔ1.Skip(skipˢ);
        });
    }, runTestVerboseSkipˢ);
}

public static void TestCleanup(ж<testing.T> Ꮡt) {
    var done = false;
    synctest.Test(Ꮡt, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        var ch = new channel<EmptyStruct>(0);
        var chʗ1 = ch;
        tΔ1.Cleanup(() => {
            // This cleanup function should execute inside the test's bubble.
            // (If it doesn't the runtime will panic.)
            close(chʗ1);
        });
        // synctest.Test will wait for this goroutine to exit before returning.
        // The cleanup function signals the goroutine to exit before the wait starts.
        var chʗ2 = ch;
        goǃ(() => {
            ᐸꟷ(chʗ2);
            done = true;
        });
    });
    if (!done) {
        Ꮡt.Fatalf("background goroutine did not return"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string notStartedˢ = "not started"u8;
private static readonly @string waitingOnContextˢ = "waiting on context"u8;
private static readonly @string doneˢ = "done"u8;

public static void TestContext(ж<testing.T> Ꮡt) {
    @string state = notStartedˢ;
    synctest.Test(Ꮡt, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        goǃ(() => {
            state = waitingOnContextˢ;
            ᐸꟷ(tΔ1.Context().Done());
            state = doneˢ;
        });
        // Wait blocks until the goroutine above is blocked on t.Context().Done().
        synctest.Wait();
        {
            @string got = state;
            @string want = waitingOnContextˢ; if (got != want) {
                tΔ1.Fatalf("state = %q, want %q"u8, got, want);
            }
        }
    });
    // t.Context() is canceled before the test completes,
    // and synctest.Test does not return until the goroutine has set its state to "done".
    {
        @string got = state;
        @string want = doneˢ; if (got != want) {
            Ꮡt.Fatalf("state = %q, want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testingTDeadlineCalledˢ = "testing: t.Deadline called inside synctest bubble"u8;

public static void TestDeadline(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            defer(wantPanic, tΔ1, testingTDeadlineCalledˢ, ref ᒐ, recovers: true);
            (_, _) = tΔ1.Deadline();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testingTParallelCalledˢ = "testing: t.Parallel called inside synctest bubble"u8;

public static void TestParallel(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            defer(wantPanic, tΔ1, testingTParallelCalledˢ, ref ᒐ, recovers: true);
            tΔ1.Parallel();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testingTRunCalledInsideˢ = "testing: t.Run called inside synctest bubble"u8;
private static readonly @string subtestˢ = "subtest"u8;

public static void TestRun(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            defer(wantPanic, tΔ1, testingTRunCalledInsideˢ, ref ᒐ, recovers: true);
            tΔ1.Run(subtestˢ, (ж<testing.T> tΔ2) => {
            });
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string logInHelperˢ = "log in helper"u8;
private static readonly @string runTestHelperSynctestˢ = """
^=== RUN   TestHelper
    synctest_test.go:.* log in helper
--- PASS: TestHelper.*
PASS
$
"""u8.Raw();

public static void TestHelper(ж<testing.T> Ꮡt) {
    runTest(Ꮡt, new @string[]{"-test.v"u8}.slice(), () => {
        synctest.Test(Ꮡt, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            helperLog(tΔ1, logInHelperˢ);
        });
    }, runTestHelperSynctestˢ);
}

internal static void wantPanic(ж<testing.T> Ꮡt, @string want) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?

        {
            var e = recover(calledByDefer); if (e != default!){
                {
                    @string got = fmt.Sprint(e); if (got != want) {
                        Ꮡt.Errorf("got panic message %q, want %q"u8, got, want);
                    }
                }
            } else {
                Ꮡt.Errorf("got no panic, want one"u8);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goWantHelperProcessˢ = "GO_WANT_HELPER_PROCESS"u8;
private static readonly @string testCount1ˢ = "-test.count=1"u8;

internal static void runTest(ж<testing.T> Ꮡt, slice<@string> args, Action f, @string pattern) {
    if (os.Getenv(goWantHelperProcessˢ) == "1"u8) {
        f();
        return;
    }
    Ꮡt.Helper();
    var re = regexp.MustCompile(pattern);
    testenv.MustHaveExec(new testing_TжTB(Ꮡt));
    var cmd = testenv.Command(new testing_TжTB(Ꮡt), testenv.Executable(new testing_TжTB(Ꮡt)), "-test.run=^"u8 + regexp.QuoteMeta(Ꮡt.Name()) + "$"u8, testCount1ˢ);
    cmd.Value.Args = appendꓸꓸꓸ((~cmd).Args, args);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Value.Env = append((~cmd).Env, "GO_WANT_HELPER_PROCESS=1"u8);
    var (@out, _) = cmd.CombinedOutput();
    if (!re.Match(@out)) {
        Ꮡt.Errorf("got output:\n%s\nwant matching:\n%s"u8, @out, pattern);
    }
}

public static void TestNow(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        {
            var (got, want) = (time.Now(), time.Date(2000, time.January, 1, 0, 0, 0, 0, time.ΔUTC)); if (!got.Equal(want)) {
                tΔ1.Errorf("time.Now() = %v, want %v"u8, got, want);
            }
        }
    });
}

} // end synctest_test_package
