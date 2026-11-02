// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using exec = global::go.os.exec_package;
using strings = strings_package;
using testing = testing_package;
using global::go.os;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingDueToShortˢ = (@string)"skipping due to -short"u8;
internal static ref readonly @string testexithooksˢ => ref testexithooksˢᵇ;
private static readonly @string testexithooksˢᵇ = "testexithooks"u8;

internal partial struct TestExitHooks_scenarios /*dyn*/ {
    internal @string mode;
    internal @string expected;
    internal slice<@string> musthave;
}

public static void TestExitHooks(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingDueToShortˢ);
    }

    var scenarios = slice<TestExitHooks_scenarios>([
        new(
            mode: "simple",
            expected: "bar foo"
        ),
        new(
            mode: "goodexit",
            expected: "orange apple"
        ),
        new(
            mode: "badexit",
            expected: "blub blix"
        ),
        new(
            mode: "panics",
            musthave: slice<@string>([
                "fatal error: exit hook invoked panic",
                "main.testPanics"
            ])
        ),
        new(
            mode: "callsexit",
            musthave: slice<@string>([
                "fatal error: exit hook invoked exit"
            ])
        ),
        new(
            mode: "exit2",
            expected: ""
        )
    ]);

    var (exe, err) = buildTestProg(t, testexithooksˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    foreach (var (_, s) in scenarios) {
        var cmd = exec.Command(exe, slice<@string>(["-mode", s.mode]).ꓸꓸꓸ);
        var (@out, _) = cmd.CombinedOutput();
        @string outs = strings.ReplaceAll((@string)@out, "\n", " ");
        outs = strings.TrimSpace(outs);
        if (s.expected != ""u8 && s.expected != outs) {
            t.Fatalf("failed %s: wanted %q\noutput:\n%s",
                s.mode, s.expected, outs);
        }
        foreach (var (_, need) in s.musthave) {
            if (!strings.Contains(outs, need)) {
                t.Fatalf("failed mode %s: output does not contain %q\noutput:\n%s",
                    s.mode, need, outs);
            }
        }
        if (s.expected == ""u8 && s.musthave == nil && outs != ""u8) {
            t.Errorf("failed mode %s: wanted no output\noutput:\n%s", s.mode, outs);
        }
    }
}

} // end runtime_test_package
