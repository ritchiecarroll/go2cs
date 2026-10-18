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

public static void TestCoroLockOSThread(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new @string[]{
        "CoroLockOSThreadIterLock"u8,
        "CoroLockOSThreadIterLockYield"u8,
        "CoroLockOSThreadLock"u8,
        "CoroLockOSThreadLockIterNested"u8,
        "CoroLockOSThreadLockIterLock"u8,
        "CoroLockOSThreadLockIterLockYield"u8,
        "CoroLockOSThreadLockIterYieldNewG"u8,
        "CoroLockOSThreadLockAfterPull"u8,
        "CoroLockOSThreadStopLocked"u8,
        "CoroLockOSThreadStopLockedIterNested"u8
    }.slice()) {
        Ꮡt.Run(test, (ж<testing.T> tΔ1) => {
            checkCoroTestProgOutput(tΔ1, runTestProg(tΔ1, testprogˢ, test));
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object coroCgoCallbackTestsNotˢ = (@string)"coro cgo callback tests not supported on Windows"u8;
internal static readonly @string testprogcgoˢ = "testprogcgo"u8;

public static void TestCoroCgoCallback(ж<testing.T> Ꮡt) {
    testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(Ꮡt));
    if (Δruntime.GOOS == "windows"u8) {
        Ꮡt.Skip(coroCgoCallbackTestsNotˢ);
    }
    if (Δruntime.GOOS == "freebsd"u8 && race.Enabled) {
        Ꮡt.Skipf("race + cgo freebsd not supported. See https://go.dev/issue/73788."u8);
    }
    foreach (var (_, test) in new @string[]{
        "CoroCgoIterCallback"u8,
        "CoroCgoIterCallbackYield"u8,
        "CoroCgoCallback"u8,
        "CoroCgoCallbackIterNested"u8,
        "CoroCgoCallbackIterCallback"u8,
        "CoroCgoCallbackIterCallbackYield"u8,
        "CoroCgoCallbackAfterPull"u8,
        "CoroCgoStopCallback"u8,
        "CoroCgoStopCallbackIterNested"u8
    }.slice()) {
        Ꮡt.Run(test, (ж<testing.T> tΔ1) => {
            checkCoroTestProgOutput(tΔ1, runTestProg(tΔ1, testprogcgoˢ, test));
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expectˢ = "expect: "u8;

internal static void checkCoroTestProgOutput(ж<testing.T> Ꮡt, @string output) {
    Ꮡt.Helper();

    var c = strings.SplitN(output, "\n"u8, 2);
    if (len(c) == 1) {
        Ꮡt.Fatalf("expected at least one complete line in the output, got:\n%s"u8, output);
    }
    var (expect, ok) = strings.CutPrefix(c[0], expectˢ);
    if (!ok) {
        Ꮡt.Fatalf("expected first line of output to start with \"expect: \", got: %q"u8, c[0]);
    }
    @string rest = c[1];
    if (expect == "OK"u8 && rest != "OK\n"u8) {
        Ꮡt.Fatalf("expected just 'OK' in the output, got:\n%s"u8, rest);
    }
    if (!strings.Contains(rest, expect)) {
        Ꮡt.Fatalf("expected %q in the output, got:\n%s"u8, expect, rest);
    }
}

} // end runtime_test_package
