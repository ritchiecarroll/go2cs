// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using static go.@internal.cpu_package;
using godebug = go.@internal.godebug_package;
using testenv = go.@internal.testenv_package;
using os = os_package;
using exec = go.os.exec_package;
using testing = testing_package;
using go.@internal;
using go.os;
using static go.@internal.cpu_internal_test_package;

partial class cpu_test_package {

public static void MustHaveDebugOptionsSupport(ж<testing.T> Ꮡt) {
    if (!DebugOptions) {
        Ꮡt.Skipf("skipping test: cpu feature options not supported by OS"u8);
    }
}

public static void MustSupportFeatureDetection(ж<testing.T> Ꮡt) {
    // TODO: add platforms that do not have CPU feature detection support.
}

internal static void runDebugOptionsTest(ж<testing.T> Ꮡt, @string test, @string options) {
    MustHaveDebugOptionsSupport(Ꮡt);

    testenv.MustHaveExec(new cpu_test_package.testing_TжTB(Ꮡt));

    @string env = "GODEBUG="u8 + options;

    var cmd = exec.Command(os.Args[0], "-test.run=^"u8 + test + "$"u8);
    cmd.Value.Env = append((~cmd).Env, env);

    var (output, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("%s with %s: run failed: %v output:\n%s\n"u8,
            test, env, err, (@string)output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cpuAllOffˢ = "cpu.all=off"u8;

public static void TestDisableAllCapabilities(ж<testing.T> Ꮡt) {
    MustSupportFeatureDetection(Ꮡt);
    runDebugOptionsTest(Ꮡt, "TestAllCapabilitiesDisabled"u8, cpuAllOffˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cpuAllˢ = "#cpu.all"u8;

public static void TestAllCapabilitiesDisabled(ж<testing.T> Ꮡt) {
    MustHaveDebugOptionsSupport(Ꮡt);

    if (godebug.New(cpuAllˢ).Value() != "off"u8) {
        Ꮡt.Skipf("skipping test: GODEBUG=cpu.all=off not set"u8);
    }

    foreach (var (_, o) in cpu_internal_test_package.Options) {
        var want = false;
        {
            var got = o.Feature.Value; if (got != want) {
                Ꮡt.Errorf("%v: expected %v, got %v"u8, o.Name, want, got);
            }
        }
    }
}

} // end cpu_test_package
