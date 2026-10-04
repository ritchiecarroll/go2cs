// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using static go.@internal.godebug_package;
using race = go.@internal.race_package;
using testenv = go.@internal.testenv_package;
using os = os_package;
using exec = go.os.exec_package;
using metrics = go.runtime.metrics_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using go.@internal;
using go.os;
using go.runtime;
using godebug = go.@internal.godebug_package;

partial class godebug_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string fooˢ = "#foo"u8;
private static readonly @string loooooooongˢ = "#loooooooong"u8;
private static readonly @string godebugˢ = "GODEBUG"u8;

[GoType("dyn")] internal partial struct TestGet_tests {
    internal @string godebug;
    internal ж<godebug.Setting> setting;
    internal @string want;
}

public static void TestGet(ж<testing.T> Ꮡt) {
    var foo = New(fooˢ);
    var tests = new TestGet_tests[]{
        new(""u8, New("#"u8), ""u8),
        new(""u8, foo, ""u8),
        new("foo=bar"u8, foo, "bar"u8),
        new("foo=bar,after=x"u8, foo, "bar"u8),
        new("before=x,foo=bar,after=x"u8, foo, "bar"u8),
        new("before=x,foo=bar"u8, foo, "bar"u8),
        new(",,,foo=bar,,,"u8, foo, "bar"u8),
        new("foodecoy=wrong,foo=bar"u8, foo, "bar"u8),
        new("foo="u8, foo, ""u8),
        new("foo"u8, foo, ""u8),
        new(",foo"u8, foo, ""u8),
        new("foo=bar,baz"u8, New(loooooooongˢ), ""u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        Ꮡt.Setenv(godebugˢ, tt.godebug);
        @string got = tt.setting.Value();
        if (got != tt.want) {
            Ꮡt.Errorf("get(%q, %q) = %q; want %q"u8, tt.godebug, tt.setting.Name(), got, tt.want);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string nameᶜ = "http2client"u8;

public static void TestMetrics(ж<testing.T> Ꮡt) {
    @string name = nameᶜ; // must be a real name so runtime will accept it
    array<metrics.Sample> m = new(1);
    m[0].Name = "/godebug/non-default-behavior/" + name + ":events";
    metrics.Read(m[..]);
    {
        metrics.ValueKind kind = m[0].Value.Kind(); if (kind != metrics.KindUint64) {
            Ꮡt.Fatalf("NonDefault kind = %v, want uint64"u8, kind);
        }
    }
    var s = New(name);
    s.Value();
    s.IncNonDefault();
    s.IncNonDefault();
    s.IncNonDefault();
    metrics.Read(m[..]);
    {
        metrics.ValueKind kind = m[0].Value.Kind(); if (kind != metrics.KindUint64) {
            Ꮡt.Fatalf("NonDefault kind = %v, want uint64"u8, kind);
        }
    }
    {
        var count = m[0].Value.Uint64(); if (count != 3) {
            Ꮡt.Fatalf("NonDefault value = %d, want 3"u8, count);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingTestIntendedForˢ = (@string)"Skipping test intended for use with -race."u8;
private static readonly @string testRunTestPanicNilRaceˢ = "-test.run=^TestPanicNilRace$"u8;
private static readonly @string testVˢ = "-test.v"u8;
private static readonly @string testParallel2ˢ = "-test.parallel=2"u8;
private static readonly @string testCount1ˢ = "-test.count=1"u8;
private static readonly @string oneˢ = "One"u8;
private static readonly @string twoˢ = "Two"u8;

// TestPanicNilRace checks for a race in the runtime caused by use of runtime
// atomics (not visible to usual race detection) to install the counter for
// non-default panic(nil) semantics.  For #64649.
public static void TestPanicNilRace(ж<testing.T> Ꮡt) {
    if (!race.Enabled) {
        Ꮡt.Skip(skippingTestIntendedForˢ);
    }
    if (os.Getenv(godebugˢ) != "panicnil=1"u8) {
        var cmd = testenv.CleanCmdEnv(testenv.Command(new testing_TжTB(Ꮡt), os.Args[0], testRunTestPanicNilRaceˢ, testVˢ, testParallel2ˢ, testCount1ˢ));
        cmd.Value.Env = append((~cmd).Env, "GODEBUG=panicnil=1"u8);
        var (@out, err) = cmd.CombinedOutput();
        Ꮡt.Logf("output:\n%s"u8, @out);
        if (err != default!) {
            Ꮡt.Errorf("Was not expecting a crash"u8);
        }
        return;
    }
    var test = (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            tΔ1.Parallel();
            defer(() => {
                recover();
            }, ref ᒐ);
            throw panic(default!);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
    Ꮡt.Run(oneˢ, test);
    Ꮡt.Run(twoˢ, test);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string runˢ = "run"u8;
private static readonly @string cmdVendorGolangOrgXToolsˢ = "cmd/vendor/golang.org/x/tools/cmd/bisect"u8;
private static readonly @string godebugBuggy1Patternˢ = "GODEBUG=buggy=1#PATTERN"u8;
private static readonly @string testRunˢ = "-test.run=^TestBisectTestCase$"u8;
private static readonly @string godebugTestGoˢ = "godebug_test.go"u8;
private static readonly @string godebugTestGoˢ2 = "godebug_test.go:"u8;

public static void TestCmdBisect(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));
    var (@out, err) = exec.Command("go"u8, runˢ, cmdVendorGolangOrgXToolsˢ, godebugBuggy1Patternˢ, os.Args[0], testRunˢ).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("exec bisect: %v\n%s"u8, err, @out);
    }
    slice<@string> want = default!;
    (var src, err) = os.ReadFile(godebugTestGoˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (i, line) in strings.Split(((@string)src), "\n"u8)) {
        if (strings.Contains(line, "BISECT"u8 + " "u8 + "BUG"u8)) {
            want = append(want, fmt.Sprintf("godebug_test.go:%d"u8, i + 1));
        }
    }
    slices.Sort<slice<@string>, @string>(want);
    slice<@string> have = default!;
    foreach (var (_, line) in strings.Split(((@string)@out), "\n"u8)) {
        if (strings.Contains(line, godebugTestGoˢ2)) {
            have = append(have, line.slice(strings.LastIndex(line, godebugTestGoˢ2)));
        }
    }
    slices.Sort<slice<@string>, @string>(have);
    if (!slices.Equal<slice<@string>, @string>(have, want)) {
        Ꮡt.Errorf("bad bisect output:\nhave %v\nwant %v\ncomplete output:\n%s"u8, have, want, ((@string)@out));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string buggyˢ = "#buggy"u8;
private static readonly object bugˢ = (@string)"bug"u8;

// This test does nothing by itself, but you can run
//
//	bisect 'GODEBUG=buggy=1#PATTERN' go test -run='^TestBisectTestCase$'
//
// to see that the GODEBUG bisect support is working.
// TestCmdBisect above does exactly that.
public static void TestBisectTestCase(ж<testing.T> Ꮡt) {
    var s = New(buggyˢ);
    for (nint i = 0; i < 10; i++) {
        var a = s.Value() == "1"u8;
        var b = s.Value() == "1"u8;
        var c = s.Value() == "1"u8; // BISECT BUG
        var d = s.Value() == "1"u8; // BISECT BUG
        var e = s.Value() == "1"u8; // BISECT BUG
        if (a) {
            Ꮡt.Log((@string)"ok"u8);
        }
        if (b) {
            Ꮡt.Log((@string)"ok"u8);
        }
        if (c) {
            Ꮡt.Error(bugˢ);
        }
        if (d && e) {
            Ꮡt.Error(bugˢ);
        }
    }
}

} // end godebug_test_package
