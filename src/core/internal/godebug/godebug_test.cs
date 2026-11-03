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
private static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "#foo"u8;
private static ref readonly @string loooooooongˢ => ref loooooooongˢᵇ;
private static readonly @string loooooooongˢᵇ = "#loooooooong"u8;
private static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;

internal partial struct TestGet_tests /*dyn*/ {
    internal @string godebug;
    internal ж<godebug.Setting> setting;
    internal @string want;
}

public static void TestGet(ж<testing.T> t) {
    var foo = New(fooˢ);
    var tests = slice<TestGet_tests>([
        new("", New("#"), ""),
        new("", foo, ""),
        new("foo=bar", foo, "bar"),
        new("foo=bar,after=x", foo, "bar"),
        new("before=x,foo=bar,after=x", foo, "bar"),
        new("before=x,foo=bar", foo, "bar"),
        new(",,,foo=bar,,,", foo, "bar"),
        new("foodecoy=wrong,foo=bar", foo, "bar"),
        new("foo=", foo, ""),
        new("foo", foo, ""),
        new(",foo", foo, ""),
        new("foo=bar,baz", New(loooooooongˢ), "")
    ]);
    foreach (var (_, tt) in tests) {
        t.Setenv(godebugˢ, tt.godebug);
        @string got = tt.setting.Value();
        if (got != tt.want) {
            t.Errorf("get(%q, %q) = %q; want %q", tt.godebug, tt.setting.Name(), got, tt.want);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static ref readonly @string nameᶜ => ref nameᶜᵇ;
private static readonly @string nameᶜᵇ = "http2client"u8;

public static void TestMetrics(ж<testing.T> t) {
    @string name = nameᶜ; // must be a real name so runtime will accept it

    array<metrics.Sample> m = new(1);
    m[0].Name = "/godebug/non-default-behavior/" + name + ":events";
    metrics.Read(m[..]);
    {
        metrics.ValueKind kind = m[0].Value.Kind(); if (kind != metrics.KindUint64) {
            t.Fatalf("NonDefault kind = %v, want uint64", kind);
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
            t.Fatalf("NonDefault kind = %v, want uint64", kind);
        }
    }
    {
        var count = m[0].Value.Uint64(); if (count != 3) {
            t.Fatalf("NonDefault value = %d, want 3", count);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingTestIntendedForˢ = (@string)"Skipping test intended for use with -race."u8;
private static ref readonly @string testRunTestPanicNilRaceˢ => ref testRunTestPanicNilRaceˢᵇ;
private static readonly @string testRunTestPanicNilRaceˢᵇ = "-test.run=^TestPanicNilRace$"u8;
private static ref readonly @string testVˢ => ref testVˢᵇ;
private static readonly @string testVˢᵇ = "-test.v"u8;
private static ref readonly @string testParallel2ˢ => ref testParallel2ˢᵇ;
private static readonly @string testParallel2ˢᵇ = "-test.parallel=2"u8;
private static ref readonly @string testCount1ˢ => ref testCount1ˢᵇ;
private static readonly @string testCount1ˢᵇ = "-test.count=1"u8;
private static ref readonly @string oneˢ => ref oneˢᵇ;
private static readonly @string oneˢᵇ = "One"u8;
private static ref readonly @string twoˢ => ref twoˢᵇ;
private static readonly @string twoˢᵇ = "Two"u8;

// TestPanicNilRace checks for a race in the runtime caused by use of runtime
// atomics (not visible to usual race detection) to install the counter for
// non-default panic(nil) semantics.  For #64649.
public static void TestPanicNilRace(ж<testing.T> t) {
    if (!race.Enabled) {
        t.Skip(skippingTestIntendedForˢ);
    }
    if (os.Getenv(godebugˢ) != "panicnil=1"u8) {
        var cmd = testenv.CleanCmdEnv(testenv.Command(new testing_TжTB(t), testenv.Executable(new testing_TжTB(t)), testRunTestPanicNilRaceˢ, testVˢ, testParallel2ˢ, testCount1ˢ));
        cmd.Env = append(cmd.Env, (@string)"GODEBUG=panicnil=1");
        var (@out, err) = cmd.CombinedOutput();
        t.Logf("output:\n%s", @out);

        if (err != nil) {
            t.Errorf("Was not expecting a crash");
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
            throw panic(default);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
    t.Run(oneˢ, test);
    t.Run(twoˢ, test);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string runˢ => ref runˢᵇ;
private static readonly @string runˢᵇ = "run"u8;
private static ref readonly @string cmdVendorGolangOrgXToolsˢ => ref cmdVendorGolangOrgXToolsˢᵇ;
private static readonly @string cmdVendorGolangOrgXToolsˢᵇ = "cmd/vendor/golang.org/x/tools/cmd/bisect"u8;
private static ref readonly @string godebugBuggy1Patternˢ => ref godebugBuggy1Patternˢᵇ;
private static readonly @string godebugBuggy1Patternˢᵇ = "GODEBUG=buggy=1#PATTERN"u8;
private static ref readonly @string testRunˢ => ref testRunˢᵇ;
private static readonly @string testRunˢᵇ = "-test.run=^TestBisectTestCase$"u8;
private static ref readonly @string godebugTestGoˢ => ref godebugTestGoˢᵇ;
private static readonly @string godebugTestGoˢᵇ = "godebug_test.go"u8;
private static ref readonly @string godebugTestGoˢ2 => ref godebugTestGoˢ2ᵇ;
private static readonly @string godebugTestGoˢ2ᵇ = "godebug_test.go:"u8;

public static void TestCmdBisect(ж<testing.T> t) {
    testenv.MustHaveGoRun(new testing_TжTB(t));
    var (@out, err) = exec.Command(testenv.GoToolPath(new testing_TжTB(t)), runˢ, cmdVendorGolangOrgXToolsˢ, godebugBuggy1Patternˢ, os.Args[0], testRunˢ).CombinedOutput();
    if (err != nil) {
        t.Fatalf("exec bisect: %v\n%s", err, @out);
    }

    slice<@string> want = default;
    (var src, err) = os.ReadFile(godebugTestGoˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    foreach (var (i, line) in strings.Split((@string)src, "\n")) {
        if (strings.Contains(line, "BISECT"u8 + " "u8 + "BUG"u8)) {
            want = append(want, fmt.Sprintf("godebug_test.go:%d"u8, i + 1));
        }
    }
    slices.Sort<slice<@string>, @string>(want);

    slice<@string> have = default;
    foreach (var (_, line) in strings.Split((@string)@out, "\n")) {
        if (strings.Contains(line, godebugTestGoˢ2)) {
            have = append(have, line.slice(strings.LastIndex(line, godebugTestGoˢ2)));
        }
    }
    slices.Sort<slice<@string>, @string>(have);

    if (!slices.Equal<slice<@string>, @string>(have, want)) {
        t.Errorf("bad bisect output:\nhave %v\nwant %v\ncomplete output:\n%s", have, want, (@string)@out);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string buggyˢ => ref buggyˢᵇ;
private static readonly @string buggyˢᵇ = "#buggy"u8;
private static readonly object bugˢ = (@string)"bug"u8;

// This test does nothing by itself, but you can run
//
//	bisect 'GODEBUG=buggy=1#PATTERN' go test -run='^TestBisectTestCase$'
//
// to see that the GODEBUG bisect support is working.
// TestCmdBisect above does exactly that.
public static void TestBisectTestCase(ж<testing.T> t) {
    var s = New(buggyˢ);
    for (nint i = 0; i < 10; i++) {
        var a = s.Value() == "1"u8;
        var b = s.Value() == "1"u8;
        var c = s.Value() == "1"u8; // BISECT BUG
        var d = s.Value() == "1"u8; // BISECT BUG
        var e = s.Value() == "1"u8; // BISECT BUG

        if (a) {
            t.Log((@string)"ok");
        }
        if (b) {
            t.Log((@string)"ok");
        }
        if (c) {
            t.Error(bugˢ);
        }
        if (d && e) {
            t.Error(bugˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string fips140ˢ => ref fips140ˢᵇ;
private static readonly @string fips140ˢᵇ = "fips140"u8;
private static ref readonly @string fips140Offˢ => ref fips140Offˢᵇ;
private static readonly @string fips140Offˢᵇ = "fips140=off"u8;
private static ref readonly @string fips140Onˢ => ref fips140Onˢᵇ;
private static readonly @string fips140Onˢᵇ = "fips140=on"u8;
private static ref readonly @string fips140ˢ2 => ref fips140ˢ2ᵇ;
private static readonly @string fips140ˢ2ᵇ = "fips140="u8;

public static void TestImmutable(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer((@string godebug) => {
            os.Setenv(godebugˢ, godebug);
        }, os.Getenv(godebugˢ), ref ᒐ);

        var setting = New(fips140ˢ);
        @string value = setting.Value();

        os.Setenv(godebugˢ, fips140Offˢ);
        if (setting.Value() != value) {
            t.Errorf("Value() changed after setting GODEBUG=fips140=off");
        }

        os.Setenv(godebugˢ, fips140Onˢ);
        if (setting.Value() != value) {
            t.Errorf("Value() changed after setting GODEBUG=fips140=on");
        }

        os.Setenv(godebugˢ, fips140ˢ2);
        if (setting.Value() != value) {
            t.Errorf("Value() changed after setting GODEBUG=fips140=");
        }

        os.Setenv(godebugˢ, ""u8);
        if (setting.Value() != value) {
            t.Errorf("Value() changed after setting GODEBUG=");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end godebug_test_package
