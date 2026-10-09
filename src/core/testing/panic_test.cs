// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using flag = flag_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = go.os.exec_package;
using Δregexp = regexp_package;
using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using go.os;

partial class testing_test_package {

internal static ж<@string> testPanicTest = flag.String("test_panic_test"u8, ""u8, "TestPanic: indicates which test should panic"u8);

internal static ж<bool> testPanicParallel = flag.Bool("test_panic_parallel"u8, false, "TestPanic: run subtests in parallel"u8);

internal static ж<bool> testPanicCleanup = flag.Bool("test_panic_cleanup"u8, false, "TestPanic: indicates whether test should call Cleanup"u8);

internal static ж<@string> testPanicCleanupPanic = flag.String("test_panic_cleanup_panic"u8, ""u8, "TestPanic: indicate whether test should call Cleanup function that panics"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testRunTestPanicHelperˢ = "-test.run=^TestPanicHelper$"u8;

internal partial struct TestPanic_testCases /*dyn*/ {
    internal @string desc;
    internal slice<@string> flags;
    internal @string want;
}

public static void TestPanic(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new testing_TжTB(Ꮡt));
    var testCases = new TestPanic_testCases[]{new(
        desc: "root test panics"u8,
        flags: new @string[]{"-test_panic_test=TestPanicHelper"u8}.slice(),
        want: """

--- FAIL: TestPanicHelper (N.NNs)
    panic_test.go:NNN: TestPanicHelper

"""u8
    ), new(
        desc: "subtest panics"u8,
        flags: new @string[]{"-test_panic_test=TestPanicHelper/1"u8}.slice(),
        want: """

--- FAIL: TestPanicHelper (N.NNs)
    panic_test.go:NNN: TestPanicHelper
    --- FAIL: TestPanicHelper/1 (N.NNs)
        panic_test.go:NNN: TestPanicHelper/1

"""u8
    ), new(
        desc: "subtest panics with cleanup"u8,
        flags: new @string[]{"-test_panic_test=TestPanicHelper/1"u8, "-test_panic_cleanup"u8}.slice(),
        want: """

ran inner cleanup 1
ran middle cleanup 1
ran outer cleanup
--- FAIL: TestPanicHelper (N.NNs)
    panic_test.go:NNN: TestPanicHelper
    --- FAIL: TestPanicHelper/1 (N.NNs)
        panic_test.go:NNN: TestPanicHelper/1

"""u8
    ), new(
        desc: "subtest panics with outer cleanup panic"u8,
        flags: new @string[]{"-test_panic_test=TestPanicHelper/1"u8, "-test_panic_cleanup"u8, "-test_panic_cleanup_panic=outer"u8}.slice(),
        want: """

ran inner cleanup 1
ran middle cleanup 1
ran outer cleanup
--- FAIL: TestPanicHelper (N.NNs)
    panic_test.go:NNN: TestPanicHelper

"""u8
    ), new(
        desc: "subtest panics with middle cleanup panic"u8,
        flags: new @string[]{"-test_panic_test=TestPanicHelper/1"u8, "-test_panic_cleanup"u8, "-test_panic_cleanup_panic=middle"u8}.slice(),
        want: """

ran inner cleanup 1
ran middle cleanup 1
ran outer cleanup
--- FAIL: TestPanicHelper (N.NNs)
    panic_test.go:NNN: TestPanicHelper
    --- FAIL: TestPanicHelper/1 (N.NNs)
        panic_test.go:NNN: TestPanicHelper/1

"""u8
    ), new(
        desc: "subtest panics with inner cleanup panic"u8,
        flags: new @string[]{"-test_panic_test=TestPanicHelper/1"u8, "-test_panic_cleanup"u8, "-test_panic_cleanup_panic=inner"u8}.slice(),
        want: """

ran inner cleanup 1
ran middle cleanup 1
ran outer cleanup
--- FAIL: TestPanicHelper (N.NNs)
    panic_test.go:NNN: TestPanicHelper
    --- FAIL: TestPanicHelper/1 (N.NNs)
        panic_test.go:NNN: TestPanicHelper/1

"""u8
    ), new(
        desc: "parallel subtest panics with cleanup"u8,
        flags: new @string[]{"-test_panic_test=TestPanicHelper/1"u8, "-test_panic_cleanup"u8, "-test_panic_parallel"u8}.slice(),
        want: """

ran inner cleanup 1
ran middle cleanup 1
ran outer cleanup
--- FAIL: TestPanicHelper (N.NNs)
    panic_test.go:NNN: TestPanicHelper
    --- FAIL: TestPanicHelper/1 (N.NNs)
        panic_test.go:NNN: TestPanicHelper/1

"""u8
    ), new(
        desc: "parallel subtest panics with outer cleanup panic"u8,
        flags: new @string[]{"-test_panic_test=TestPanicHelper/1"u8, "-test_panic_cleanup"u8, "-test_panic_cleanup_panic=outer"u8, "-test_panic_parallel"u8}.slice(),
        want: """

ran inner cleanup 1
ran middle cleanup 1
ran outer cleanup
--- FAIL: TestPanicHelper (N.NNs)
    panic_test.go:NNN: TestPanicHelper

"""u8
    ), new(
        desc: "parallel subtest panics with middle cleanup panic"u8,
        flags: new @string[]{"-test_panic_test=TestPanicHelper/1"u8, "-test_panic_cleanup"u8, "-test_panic_cleanup_panic=middle"u8, "-test_panic_parallel"u8}.slice(),
        want: """

ran inner cleanup 1
ran middle cleanup 1
ran outer cleanup
--- FAIL: TestPanicHelper (N.NNs)
    panic_test.go:NNN: TestPanicHelper
    --- FAIL: TestPanicHelper/1 (N.NNs)
        panic_test.go:NNN: TestPanicHelper/1

"""u8
    ), new(
        desc: "parallel subtest panics with inner cleanup panic"u8,
        flags: new @string[]{"-test_panic_test=TestPanicHelper/1"u8, "-test_panic_cleanup"u8, "-test_panic_cleanup_panic=inner"u8, "-test_panic_parallel"u8}.slice(),
        want: """

ran inner cleanup 1
ran middle cleanup 1
ran outer cleanup
--- FAIL: TestPanicHelper (N.NNs)
    panic_test.go:NNN: TestPanicHelper
    --- FAIL: TestPanicHelper/1 (N.NNs)
        panic_test.go:NNN: TestPanicHelper/1

"""u8
    )
    }.slice();
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestPanic_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.desc, (ж<testing.T> tΔ1) => {
            var cmd = exec.Command(Δos.Args[0], testRunTestPanicHelperˢ);
            cmd.Value.Args = appendꓸꓸꓸ((~cmd).Args, tcʗ1.flags);
            cmd.Value.Env = append(Δos.Environ(), "GO_WANT_HELPER_PROCESS=1"u8);
            var (b, _) = cmd.CombinedOutput();
            @string got = ((@string)b);
            @string want = strings.TrimSpace(tcʗ1.want);
            @string re = makeRegexp(want);
            {
                var (ok, err) = Δregexp.MatchString(re, got); if (!ok || err != default!) {
                    tΔ1.Errorf("output:\ngot:\n%s\nwant:\n%s"u8, got, want);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nnnˢ = ":NNN:"u8;
private static readonly @string nNNsˢ = "N\\.NNs"u8;
private static readonly @string dDSˢ = @"\d*\.\d*s"u8;

internal static @string makeRegexp(@string s) {
    s = Δregexp.QuoteMeta(s);
    s = strings.ReplaceAll(s, nnnˢ, @":\d+:"u8);
    s = strings.ReplaceAll(s, nNNsˢ, dDSˢ);
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object ranOuterCleanupˢ = (@string)"ran outer cleanup"u8;

public static void TestPanicHelper(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) != "1"u8) {
        return;
    }
    Ꮡt.Log(Ꮡt.Name());
    if (Ꮡt.Name() == testPanicTest.Value) {
        throw panic("panic");
    }
    var exprᴛ1 = testPanicCleanupPanic.Value;
    if (exprᴛ1 == ""u8 || exprᴛ1 == "outer"u8 || exprᴛ1 == "middle"u8 || exprᴛ1 == "inner"u8) {
    }
    else { /* default: */
        Ꮡt.Fatalf("bad -test_panic_cleanup_panic: %s"u8, testPanicCleanupPanic.Value);
    }

    Ꮡt.Cleanup(() => {
        fmt.Println(ranOuterCleanupˢ);
        if (testPanicCleanupPanic.Value == "outer"u8) {
            throw panic("outer cleanup");
        }
    });
    for (nint i = 0; i < 3; i++) {
        nint iΔ1 = i;
        Ꮡt.Run(fmt.Sprintf("%v"u8, iΔ1), (ж<testing.T> tΔ1) => {
            var chosen = tΔ1.Name() == testPanicTest.Value;
            if (chosen && testPanicCleanup.Value) {
                tΔ1.Cleanup(() => {
                    fmt.Printf("ran middle cleanup %d\n"u8, iΔ1);
                    if (testPanicCleanupPanic.Value == "middle"u8) {
                        throw panic("middle cleanup");
                    }
                });
            }
            if (chosen && testPanicParallel.Value) {
                tΔ1.Parallel();
            }
            tΔ1.Log(tΔ1.Name());
            if (chosen) {
                if (testPanicCleanup.Value) {
                    tΔ1.Cleanup(() => {
                        fmt.Printf("ran inner cleanup %d\n"u8, iΔ1);
                        if (testPanicCleanupPanic.Value == "inner"u8) {
                            throw panic("inner cleanup");
                        }
                    });
                }
                throw panic("panic");
            }
        });
    }
}

internal partial struct TestMorePanic_testCases /*dyn*/ {
    internal @string desc;
    internal slice<@string> flags;
    internal @string want;
}

public static void TestMorePanic(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveExec(new testing_TжTB(Ꮡt));
    var testCases = new TestMorePanic_testCases[]{
        new(
            desc: "Issue 48502: call runtime.Goexit in t.Cleanup after panic"u8,
            flags: new @string[]{"-test.run=^TestGoexitInCleanupAfterPanicHelper$"u8}.slice(),
            want: """
panic: die
	panic: test executed panic(nil) or runtime.Goexit
"""u8
        ),
        new(
            desc: "Issue 48515: call t.Run in t.Cleanup should trigger panic"u8,
            flags: new @string[]{"-test.run=^TestCallRunInCleanupHelper$"u8}.slice(),
            want: @"panic: testing: t.Run called during t.Cleanup"u8
        )
    }.slice();
    foreach (var (_, tc) in testCases) {
        var cmd = exec.Command(Δos.Args[0], tc.flags.ꓸꓸꓸ);
        cmd.Value.Env = append(Δos.Environ(), "GO_WANT_HELPER_PROCESS=1"u8);
        var (b, _) = cmd.CombinedOutput();
        @string got = ((@string)b);
        @string want = tc.want;
        @string re = makeRegexp(want);
        {
            var (ok, err) = Δregexp.MatchString(re, got); if (!ok || err != default!) {
                Ꮡt.Errorf("output:\ngot:\n%s\nwant:\n%s"u8, got, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string inCleanupˢ = "in-cleanup"u8;
private static readonly object mustNotBeExecutedˢ = (@string)"must not be executed"u8;

public static void TestCallRunInCleanupHelper(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) != "1"u8) {
        return;
    }
    Ꮡt.Cleanup(() => {
        Ꮡt.Run(inCleanupˢ, (ж<testing.T> tΔ1) => {
            tΔ1.Log(mustNotBeExecutedˢ);
        });
    });
}

public static void TestGoexitInCleanupAfterPanicHelper(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goWantHelperProcessˢ) != "1"u8) {
        return;
    }
    Ꮡt.Cleanup(() => {
        Δruntime.Goexit();
    });
    Ꮡt.Parallel();
    throw panic("die");
}

} // end testing_test_package
