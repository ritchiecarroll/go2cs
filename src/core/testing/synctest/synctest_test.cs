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
public static void TestSuccess(ж<testing.T> t) {
    synctest.Test(t, (ж<testing.T> tΔ1) => {
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fatalˢ = (@string)"fatal"u8;
private static ref readonly @string failTestFatalSynctestˢ => ref failTestFatalSynctestˢᵇ;
private static readonly @string failTestFatalSynctestˢᵇ = """
^--- FAIL: TestFatal.*
    synctest_test.go:.* fatal
FAIL
$
"""u8.Raw();

public static void TestFatal(ж<testing.T> t) {
    runTest(t, default, () => {
        synctest.Test(t, (ж<testing.T> tΔ1) => {
            tΔ1.Fatal(fatalˢ);
        });
    }, failTestFatalSynctestˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object errorˢ = (@string)"error"u8;
private static ref readonly @string failTestErrorSynctestˢ => ref failTestErrorSynctestˢᵇ;
private static readonly @string failTestErrorSynctestˢᵇ = """
^--- FAIL: TestError.*
    synctest_test.go:.* error
FAIL
$
"""u8.Raw();

public static void TestError(ж<testing.T> t) {
    runTest(t, default, () => {
        synctest.Test(t, (ж<testing.T> tΔ1) => {
            tΔ1.Error(errorˢ);
        });
    }, failTestErrorSynctestˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string runTestVerboseErrorˢ => ref runTestVerboseErrorˢᵇ;
private static readonly @string runTestVerboseErrorˢᵇ = """
^=== RUN   TestVerboseError
    synctest_test.go:.* error
--- FAIL: TestVerboseError.*
FAIL
$
"""u8.Raw();

public static void TestVerboseError(ж<testing.T> t) {
    runTest(t, slice<@string>(["-test.v"]), () => {
        synctest.Test(t, (ж<testing.T> tΔ1) => {
            tΔ1.Error(errorˢ);
        });
    }, runTestVerboseErrorˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skipˢ = (@string)"skip"u8;
private static ref readonly @string passˢ => ref passˢᵇ;
private static readonly @string passˢᵇ = """
^PASS
$
"""u8.Raw();

public static void TestSkip(ж<testing.T> t) {
    runTest(t, default, () => {
        synctest.Test(t, (ж<testing.T> tΔ1) => {
            tΔ1.Skip(skipˢ);
        });
    }, passˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string runTestVerboseSkipˢ => ref runTestVerboseSkipˢᵇ;
private static readonly @string runTestVerboseSkipˢᵇ = """
^=== RUN   TestVerboseSkip
    synctest_test.go:.* skip
--- PASS: TestVerboseSkip.*
PASS
$
"""u8.Raw();

public static void TestVerboseSkip(ж<testing.T> t) {
    runTest(t, slice<@string>(["-test.v"]), () => {
        synctest.Test(t, (ж<testing.T> tΔ1) => {
            tΔ1.Skip(skipˢ);
        });
    }, runTestVerboseSkipˢ);
}

public static void TestCleanup(ж<testing.T> t) {
    var done = false;
    synctest.Test(t, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
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
        }, GoStack.Bounded);
    });
    if (!done) {
        t.Fatalf("background goroutine did not return");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string notStartedˢ => ref notStartedˢᵇ;
private static readonly @string notStartedˢᵇ = "not started"u8;
private static ref readonly @string waitingOnContextˢ => ref waitingOnContextˢᵇ;
private static readonly @string waitingOnContextˢᵇ = "waiting on context"u8;
private static ref readonly @string doneˢ => ref doneˢᵇ;
private static readonly @string doneˢᵇ = "done"u8;

public static void TestContext(ж<testing.T> t) {
    @string state = notStartedˢ;
    synctest.Test(t, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
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
                tΔ1.Fatalf("state = %q, want %q", got, want);
            }
        }
    });
    // t.Context() is canceled before the test completes,
    // and synctest.Test does not return until the goroutine has set its state to "done".
    {
        @string got = state;
        @string want = doneˢ; if (got != want) {
            t.Fatalf("state = %q, want %q", got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testingTDeadlineCalledˢ => ref testingTDeadlineCalledˢᵇ;
private static readonly @string testingTDeadlineCalledˢᵇ = "testing: t.Deadline called inside synctest bubble"u8;

public static void TestDeadline(ж<testing.T> t) {
    synctest.Test(t, (ж<testing.T> tΔ1) => {
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
private static ref readonly @string testingTParallelCalledˢ => ref testingTParallelCalledˢᵇ;
private static readonly @string testingTParallelCalledˢᵇ = "testing: t.Parallel called inside synctest bubble"u8;

public static void TestParallel(ж<testing.T> t) {
    synctest.Test(t, (ж<testing.T> tΔ1) => {
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
private static ref readonly @string testingTRunCalledInsideˢ => ref testingTRunCalledInsideˢᵇ;
private static readonly @string testingTRunCalledInsideˢᵇ = "testing: t.Run called inside synctest bubble"u8;
private static ref readonly @string subtestˢ => ref subtestˢᵇ;
private static readonly @string subtestˢᵇ = "subtest"u8;

public static void TestRun(ж<testing.T> t) {
    synctest.Test(t, (ж<testing.T> tΔ1) => {
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
private static ref readonly @string logInHelperˢ => ref logInHelperˢᵇ;
private static readonly @string logInHelperˢᵇ = "log in helper"u8;
private static ref readonly @string runTestHelperSynctestˢ => ref runTestHelperSynctestˢᵇ;
private static readonly @string runTestHelperSynctestˢᵇ = """
^=== RUN   TestHelper
    synctest_test.go:.* log in helper
--- PASS: TestHelper.*
PASS
$
"""u8.Raw();

public static void TestHelper(ж<testing.T> t) {
    runTest(t, slice<@string>(["-test.v"]), () => {
        synctest.Test(t, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            helperLog(tΔ1, logInHelperˢ);
        });
    }, runTestHelperSynctestˢ);
}

internal static void wantPanic(ж<testing.T> t, @string want) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?

        {
            var e = recover(calledByDefer); if (e != nil) {
                {
                    @string got = fmt.Sprint(e); if (got != want) {
                        t.Errorf("got panic message %q, want %q", got, want);
                    }
                }
            } else {
                t.Errorf("got no panic, want one");
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string goWantHelperProcessˢ => ref goWantHelperProcessˢᵇ;
private static readonly @string goWantHelperProcessˢᵇ = "GO_WANT_HELPER_PROCESS"u8;
private static ref readonly @string testCount1ˢ => ref testCount1ˢᵇ;
private static readonly @string testCount1ˢᵇ = "-test.count=1"u8;

internal static void runTest(ж<testing.T> t, slice<@string> args, Action f, @string pattern) {
    if (os.Getenv(goWantHelperProcessˢ) == "1"u8) {
        f();
        return;
    }
    t.Helper();
    var re = regexp.MustCompile(pattern);
    testenv.MustHaveExec(new testing_TжTB(t));
    var cmd = testenv.Command(new testing_TжTB(t), testenv.Executable(new testing_TжTB(t)), "-test.run=^"u8 + regexp.QuoteMeta(t.Name()) + "$"u8, testCount1ˢ);
    cmd.Args = appendꓸꓸꓸ(cmd.Args, args);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Env = append(cmd.Env, (@string)"GO_WANT_HELPER_PROCESS=1");
    var (@out, _) = cmd.CombinedOutput();
    if (!re.Match(@out)) {
        t.Errorf("got output:\n%s\nwant matching:\n%s", @out, pattern);
    }
}

public static void TestNow(ж<testing.T> t) {
    synctest.Test(t, (ж<testing.T> tΔ1) => {
        {
            var (got, want) = (time.Now(), time.Date(2000, time.January, 1, 0, 0, 0, 0, time.ΔUTC)); if (!got.Equal(want)) {
                tΔ1.Errorf("time.Now() = %v, want %v", got, want);
            }
        }
    });
}

} // end synctest_test_package
