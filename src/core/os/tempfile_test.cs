// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fs = go.io.fs_package;
using static os_package;
using filepath = go.path.filepath_package;
using Δregexp = regexp_package;
using strings = strings_package;
using Δtesting = testing_package;
using go.io;
using go.path;
using static go.os_internal_test_package;
using Δos = os_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notExistsˢ2 = "_not_exists_"u8;

public static void TestCreateTemp(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string nonexistentDir = filepath.Join(Ꮡt.TempDir(), notExistsˢ2);
    var (f, err) = CreateTemp(nonexistentDir, fooˢ);
    if (f != nil || err == default!) {
        Ꮡt.Errorf("CreateTemp(%q, `foo`) = %v, %v"u8, nonexistentDir, f.OrTypedNil(), err);
    }
}

internal partial struct TestCreateTempPattern_tests /*dyn*/ {
    internal @string pattern, prefix, suffix;
}

public static void TestCreateTempPattern(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var tests = new TestCreateTempPattern_tests[]{
            new("tempfile_test"u8, "tempfile_test"u8, ""u8),
            new("tempfile_test*"u8, "tempfile_test"u8, ""u8),
            new("tempfile_test*xyz"u8, "tempfile_test"u8, "xyz"u8)
        }.slice();
        foreach (var (_, test) in tests) {
            var (f, err) = CreateTemp(""u8, test.pattern);
            if (err != default!) {
                Ꮡt.Errorf("CreateTemp(..., %q) error: %v"u8, test.pattern, err);
                continue;
            }
            defer(Remove, f.Name(), ref ᒐ);
            @string @base = filepath.Base(f.Name());
            f.Close();
            if (!(strings.HasPrefix(@base, test.prefix) && strings.HasSuffix(@base, test.suffix))) {
                Ꮡt.Errorf("CreateTemp pattern %q created bad name %q; want prefix %q & suffix %q"u8,
                    test.pattern, @base, test.prefix, test.suffix);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string sepᶜ1 = "\\";

internal partial struct TestCreateTempBadPattern_tests /*dyn*/ {
    internal @string pattern;
    internal bool wantErr;
}

public static void TestCreateTempBadPattern(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string tmpDir = Ꮡt.TempDir();
    @string sep = sepᶜ1;
    var tests = new TestCreateTempBadPattern_tests[]{
        new("ioutil*test"u8, false),
        new("tempfile_test*foo"u8, false),
        new("tempfile_test"u8 + sep + "foo"u8, true),
        new("tempfile_test*"u8 + sep + "foo"u8, true),
        new("tempfile_test"u8 + sep + "*foo"u8, true),
        new(sep + "tempfile_test"u8 + sep + "*foo"u8, true),
        new("tempfile_test*foo"u8 + sep, true)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestCreateTempBadPattern_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.pattern, (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var (tmpfile, err) = CreateTemp(tmpDir, ttʗ1.pattern);
                if (tmpfile != nil) {
                    var tmpfileʗ1 = tmpfile;
                    defer(() => tmpfileʗ1.Close(), ref ᒐ);
                }
                if (ttʗ1.wantErr){
                    if (err == default!) {
                        tΔ1.Errorf("CreateTemp(..., %#q) succeeded, expected error"u8, ttʗ1.pattern);
                    }
                    if (!errors.Is(err, os_internal_test_package.ErrPatternHasSeparator)) {
                        tΔ1.Errorf("CreateTemp(..., %#q): %v, expected ErrPatternHasSeparator"u8, ttʗ1.pattern, err);
                    }
                } else 
                if (err != default!) {
                    tΔ1.Errorf("CreateTemp(..., %#q): %v"u8, ttʗ1.pattern, err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notExistsˢ3 = "/_not_exists_"u8;
internal static readonly @string xyzˢ = "*xyz"u8;

internal partial struct TestMkdirTemp_tests /*dyn*/ {
    internal @string pattern;
    internal @string wantPrefix, wantSuffix;
}

public static void TestMkdirTemp(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    var (name, err) = MkdirTemp(notExistsˢ3, fooˢ);
    if (name != ""u8 || err == default!) {
        Ꮡt.Errorf("MkdirTemp(`/_not_exists_`, `foo`) = %v, %v"u8, name, err);
    }
    var tests = new TestMkdirTemp_tests[]{
        new("tempfile_test"u8, "tempfile_test"u8, ""u8),
        new("tempfile_test*"u8, "tempfile_test"u8, ""u8),
        new("tempfile_test*xyz"u8, "tempfile_test"u8, "xyz"u8)
    }.slice();
    @string dir = filepath.Clean(TempDir());
    void runTestMkdirTemp(ж<Δtesting.T> tΔ1, @string pattern, @string wantRePat) {
        GoFrame ᒐ = default;
        try {
            var (nameΔ1, errΔ1) = MkdirTemp(dir, pattern);
            if (nameΔ1 == ""u8 || errΔ1 != default!) {
                tΔ1.Fatalf("MkdirTemp(dir, `tempfile_test`) = %v, %v"u8, nameΔ1, errΔ1);
            }
            defer(Remove, nameΔ1, ref ᒐ);
            var re = Δregexp.MustCompile(wantRePat);
            if (!re.MatchString(nameΔ1)) {
                tΔ1.Errorf("MkdirTemp(%q, %q) created bad name\n\t%q\ndid not match pattern\n\t%q"u8, dir, pattern, nameΔ1, wantRePat);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestMkdirTemp_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var runTestMkdirTempʗ1 = runTestMkdirTemp;
        var ttʗ1 = tt;
        Ꮡt.Run(tt.pattern, (ж<Δtesting.T> tΔ2) => {
            @string wantRePat = "^"u8 + Δregexp.QuoteMeta(filepath.Join(dir, ttʗ1.wantPrefix)) + "[0-9]+"u8 + Δregexp.QuoteMeta(ttʗ1.wantSuffix) + "$"u8;
            runTestMkdirTempʗ1(tΔ2, ttʗ1.pattern, wantRePat);
        });
    }
    // Separately testing "*xyz" (which has no prefix). That is when constructing the
    // pattern to assert on, as in the previous loop, using filepath.Join for an empty
    // prefix filepath.Join(dir, ""), produces the pattern:
    //     ^<DIR>[0-9]+xyz$
    // yet we just want to match
    //     "^<DIR>/[0-9]+xyz"
    var runTestMkdirTempʗ2 = runTestMkdirTemp;
    Ꮡt.Run(xyzˢ, (ж<Δtesting.T> tΔ3) => {
        @string wantRePat = "^"u8 + Δregexp.QuoteMeta(filepath.Join(dir)) + Δregexp.QuoteMeta(((@string)(rune)filepath.Separator)) + "[0-9]+xyz$"u8;
        runTestMkdirTempʗ2(tΔ3, xyzˢ, wantRePat);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notExistˢ = "not-exist"u8;

// test that we return a nice error message if the dir argument to TempDir doesn't
// exist (or that it's empty and TempDir doesn't exist)
public static void TestMkdirTempBadDir(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string badDir = filepath.Join(Ꮡt.TempDir(), notExistˢ);
    var (_, err) = MkdirTemp(badDir, fooˢ);
    {
        var (pe, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok || !IsNotExist(err) || (~pe).Path != badDir) {
            Ꮡt.Errorf("TempDir error = %#v; want PathError for path %q satisfying IsNotExist"u8, err, badDir);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string sepᶜ2 = "\\";

internal partial struct TestMkdirTempBadPattern_tests /*dyn*/ {
    internal @string pattern;
    internal bool wantErr;
}

public static void TestMkdirTempBadPattern(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string tmpDir = Ꮡt.TempDir();
    @string sep = sepᶜ2;
    var tests = new TestMkdirTempBadPattern_tests[]{
        new("ioutil*test"u8, false),
        new("tempfile_test*foo"u8, false),
        new("tempfile_test"u8 + sep + "foo"u8, true),
        new("tempfile_test*"u8 + sep + "foo"u8, true),
        new("tempfile_test"u8 + sep + "*foo"u8, true),
        new(sep + "tempfile_test"u8 + sep + "*foo"u8, true),
        new("tempfile_test*foo"u8 + sep, true)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestMkdirTempBadPattern_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.pattern, (ж<Δtesting.T> tΔ1) => {
            var (_, err) = MkdirTemp(tmpDir, ttʗ1.pattern);
            if (ttʗ1.wantErr){
                if (err == default!) {
                    tΔ1.Errorf("MkdirTemp(..., %#q) succeeded, expected error"u8, ttʗ1.pattern);
                }
                if (!errors.Is(err, os_internal_test_package.ErrPatternHasSeparator)) {
                    tΔ1.Errorf("MkdirTemp(..., %#q): %v, expected ErrPatternHasSeparator"u8, ttʗ1.pattern, err);
                }
            } else 
            if (err != default!) {
                tΔ1.Errorf("MkdirTemp(..., %#q): %v"u8, ttʗ1.pattern, err);
            }
        });
    }
}

} // end os_test_package
