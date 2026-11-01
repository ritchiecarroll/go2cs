// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using static go.@internal.cpu_package;
using godebug = go.@internal.godebug_package;
using testenv = go.@internal.testenv_package;
using exec = os.exec_package;
using Δruntime = runtime_package;
using testing = testing_package;
using go.@internal;
using os;
using static go.@internal.cpu_internal_test_package;

partial class cpu_test_package {

public static void MustHaveDebugOptionsSupport(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "aix"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "illumos"u8 || exprᴛ1 == "solaris"u8 || exprᴛ1 == "linux"u8: {
        break;
    }
    default: {
        t.Skipf("skipping test: cpu feature options not supported by OS");
        break;
    }}
}

public static void MustSupportFeatureDetection(ж<testing.T> t) {
    // TODO: add platforms that do not have CPU feature detection support.
}

internal static void runDebugOptionsTest(ж<testing.T> t, @string test, @string options) {
    MustHaveDebugOptionsSupport(t);

    @string env = "GODEBUG="u8 + options;

    var cmd = exec.Command(testenv.Executable(new cpu_test_package.testing_TжTB(t)), "-test.run=^"u8 + test + "$"u8);
    cmd.Env = append(cmd.Env, env);

    var (output, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("%s with %s: run failed: %v output:\n%s\n",
            test, env, err, (@string)output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cpuAllOffˢ => ref cpuAllOffˢᵇ;
private static readonly @string cpuAllOffˢᵇ = "cpu.all=off"u8;

public static void TestDisableAllCapabilities(ж<testing.T> t) {
    MustSupportFeatureDetection(t);
    runDebugOptionsTest(t, "TestAllCapabilitiesDisabled", cpuAllOffˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cpuAllˢ => ref cpuAllˢᵇ;
private static readonly @string cpuAllˢᵇ = "#cpu.all"u8;

public static void TestAllCapabilitiesDisabled(ж<testing.T> t) {
    MustHaveDebugOptionsSupport(t);

    if (godebug.New(cpuAllˢ).Value() != "off"u8) {
        t.Skipf("skipping test: GODEBUG=cpu.all=off not set");
    }

    foreach (var (_, o) in cpu_internal_test_package.Options) {
        var want = false;
        {
            var got = o.Feature.Value; if (got != want) {
                t.Errorf("%v: expected %v, got %v", o.Name, want, got);
            }
        }
    }
}

} // end cpu_test_package
