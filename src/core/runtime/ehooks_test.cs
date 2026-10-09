// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using platform = @internal.platform_package;
using testenv = @internal.testenv_package;
using exec = global::go.os.exec_package;
using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using global::go.os;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingDueToShortˢ = (@string)"skipping due to -short"u8;
internal static readonly @string testexithooksˢ = "testexithooks"u8;

internal partial struct TestExitHooks_scenarios /*dyn*/ {
    internal @string mode;
    internal @string expected;
    internal slice<@string> musthave;
}

public static void TestExitHooks(ж<testing.T> Ꮡt) {
    var bmodes = new @string[]{""u8}.slice();
    if (testing.Short()) {
        Ꮡt.Skip(skippingDueToShortˢ);
    }
    // Note the HasCGO() test below; this is to prevent the test
    // running if CGO_ENABLED=0 is in effect.
    var haverace = platform.RaceDetectorSupported(Δruntime.GOOS, Δruntime.GOARCH);
    if (haverace && testenv.HasCGO()) {
        bmodes = append(bmodes, "-race"u8);
    }
    foreach (var (_, bmode) in bmodes) {
        var scenarios = new TestExitHooks_scenarios[]{
            new(
                mode: "simple"u8,
                expected: "bar foo"u8
            ),
            new(
                mode: "goodexit"u8,
                expected: "orange apple"u8
            ),
            new(
                mode: "badexit"u8,
                expected: "blub blix"u8
            ),
            new(
                mode: "panics"u8,
                musthave: new @string[]{
                    "fatal error: exit hook invoked panic"u8,
                    "main.testPanics"u8
                }.slice()
            ),
            new(
                mode: "callsexit"u8,
                musthave: new @string[]{
                    "fatal error: exit hook invoked exit"u8
                }.slice()
            ),
            new(
                mode: "exit2"u8,
                expected: ""u8
            )
        }.slice();
        var (exe, err) = buildTestProg(Ꮡt, testexithooksˢ, bmode);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string bt = ""u8;
        if (bmode != ""u8) {
            bt = " bmode: "u8 + bmode;
        }
        foreach (var (_, s) in scenarios) {
            var cmd = exec.Command(exe, new @string[]{"-mode"u8, s.mode}.slice().ꓸꓸꓸ);
            var (@out, _) = cmd.CombinedOutput();
            @string outs = strings.ReplaceAll(((@string)@out), "\n"u8, " "u8);
            outs = strings.TrimSpace(outs);
            if (s.expected != ""u8 && s.expected != outs) {
                Ꮡt.Fatalf("failed%s mode %s: wanted %q\noutput:\n%s"u8, bt,
                    s.mode, s.expected, outs);
            }
            foreach (var (_, need) in s.musthave) {
                if (!strings.Contains(outs, need)) {
                    Ꮡt.Fatalf("failed mode %s: output does not contain %q\noutput:\n%s"u8,
                        s.mode, need, outs);
                }
            }
            if (s.expected == ""u8 && s.musthave == default! && outs != ""u8) {
                Ꮡt.Errorf("failed mode %s: wanted no output\noutput:\n%s"u8, s.mode, outs);
            }
        }
    }
}

} // end runtime_test_package
