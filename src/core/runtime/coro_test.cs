// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using race = @internal.race_package;
using testenv = @internal.testenv_package;
using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

public static void TestCoroLockOSThread(ж<testing.T> t) {
    foreach (var (_, test) in slice<@string>([
        "CoroLockOSThreadIterLock",
        "CoroLockOSThreadIterLockYield",
        "CoroLockOSThreadLock",
        "CoroLockOSThreadLockIterNested",
        "CoroLockOSThreadLockIterLock",
        "CoroLockOSThreadLockIterLockYield",
        "CoroLockOSThreadLockIterYieldNewG",
        "CoroLockOSThreadLockAfterPull",
        "CoroLockOSThreadStopLocked",
        "CoroLockOSThreadStopLockedIterNested"
    ])) {
        t.Run(test, (ж<testing.T> tΔ1) => {
            checkCoroTestProgOutput(tΔ1, runTestProg(tΔ1, testprogˢ, test));
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object coroCgoCallbackTestsNotˢ = (@string)"coro cgo callback tests not supported on Windows"u8;
internal static ref readonly @string testprogcgoˢ => ref testprogcgoˢᵇ;
private static readonly @string testprogcgoˢᵇ = "testprogcgo"u8;

public static void TestCoroCgoCallback(ж<testing.T> t) {
    testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(t));
    if (Δruntime.GOOS == "windows"u8) {
        t.Skip(coroCgoCallbackTestsNotˢ);
    }
    if (Δruntime.GOOS == "freebsd"u8 && race.Enabled) {
        t.Skipf("race + cgo freebsd not supported. See https://go.dev/issue/73788.");
    }
    foreach (var (_, test) in slice<@string>([
        "CoroCgoIterCallback",
        "CoroCgoIterCallbackYield",
        "CoroCgoCallback",
        "CoroCgoCallbackIterNested",
        "CoroCgoCallbackIterCallback",
        "CoroCgoCallbackIterCallbackYield",
        "CoroCgoCallbackAfterPull",
        "CoroCgoStopCallback",
        "CoroCgoStopCallbackIterNested"
    ])) {
        t.Run(test, (ж<testing.T> tΔ1) => {
            checkCoroTestProgOutput(tΔ1, runTestProg(tΔ1, testprogcgoˢ, test));
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string expectˢ => ref expectˢᵇ;
private static readonly @string expectˢᵇ = "expect: "u8;

internal static void checkCoroTestProgOutput(ж<testing.T> t, @string output) {
    t.Helper();

    var c = strings.SplitN(output, "\n", 2);
    if (len(c) == 1) {
        t.Fatalf("expected at least one complete line in the output, got:\n%s", output);
    }
    var (expect, ok) = strings.CutPrefix(c[0], expectˢ);
    if (!ok) {
        t.Fatalf("expected first line of output to start with \"expect: \", got: %q", c[0]);
    }
    @string rest = c[1];
    if (expect == "OK"u8 && rest != "OK\n"u8) {
        t.Fatalf("expected just 'OK' in the output, got:\n%s", rest);
    }
    if (!strings.Contains(rest, expect)) {
        t.Fatalf("expected %q in the output, got:\n%s", expect, rest);
    }
}

} // end runtime_test_package
