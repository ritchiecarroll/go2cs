// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using fs = go.io.fs_package;
using static go.io.ioutil_package;
using os = os_package;
using filepath = path.filepath_package;
using regexp = regexp_package;
using strings = strings_package;
using testing = testing_package;
using go.io;
using path;

partial class ioutil_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testTempFileBadDirˢ = "TestTempFile_BadDir"u8;
private static readonly @string notExistsˢ = "_not_exists_"u8;
private static readonly @string fooˢ = "foo"u8;

public static void TestTempFile(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (dir, err) = TempDir(""u8, testTempFileBadDirˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(os.RemoveAll, dir, ref ᒐ);
        @string nonexistentDir = filepath.Join(dir, notExistsˢ);
        (var f, err) = TempFile(nonexistentDir, fooˢ);
        if (f != nil || err == default!) {
            Ꮡt.Errorf("TempFile(%q, `foo`) = %v, %v"u8, nonexistentDir, f.OrTypedNil(), err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestTempFile_pattern_tests {
    internal @string pattern, prefix, suffix;
}

public static void TestTempFile_pattern(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var tests = new TestTempFile_pattern_tests[]{
            new("ioutil_test"u8, "ioutil_test"u8, ""u8),
            new("ioutil_test*"u8, "ioutil_test"u8, ""u8),
            new("ioutil_test*xyz"u8, "ioutil_test"u8, "xyz"u8)
        }.slice();
        foreach (var (_, test) in tests) {
            var (f, err) = TempFile(""u8, test.pattern);
            if (err != default!) {
                Ꮡt.Errorf("TempFile(..., %q) error: %v"u8, test.pattern, err);
                continue;
            }
            defer(os.Remove, f.Name(), ref ᒐ);
            @string @base = filepath.Base(f.Name());
            f.Close();
            if (!(strings.HasPrefix(@base, test.prefix) && strings.HasSuffix(@base, test.suffix))) {
                Ꮡt.Errorf("TempFile pattern %q created bad name %q; want prefix %q & suffix %q"u8,
                    test.pattern, @base, test.prefix, test.suffix);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// This string is from os.errPatternHasSeparator.
internal static readonly @string patternHasSeparator = "pattern contains path separator"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string sepᶜ = "\\";

[GoType("dyn")] internal partial struct TestTempFile_BadPattern_tests {
    internal @string pattern;
    internal bool wantErr;
}

public static void TestTempFile_BadPattern(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (tmpDir, err) = TempDir(""u8, Ꮡt.Name());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(os.RemoveAll, tmpDir, ref ᒐ);
        @string sep = sepᶜ;
        var tests = new TestTempFile_BadPattern_tests[]{
            new("ioutil*test"u8, false),
            new("ioutil_test*foo"u8, false),
            new("ioutil_test"u8 + sep + "foo"u8, true),
            new("ioutil_test*"u8 + sep + "foo"u8, true),
            new("ioutil_test"u8 + sep + "*foo"u8, true),
            new(sep + "ioutil_test"u8 + sep + "*foo"u8, true),
            new("ioutil_test*foo"u8 + sep, true)
        }.slice();
        foreach (var (_, vᴛ1) in tests) {
            ref var tt = ref heap(new TestTempFile_BadPattern_tests(), out var Ꮡtt);
            tt = vᴛ1;

            var ttʗ1 = tt;
            Ꮡt.Run(tt.pattern, (ж<testing.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    var (tmpfile, errΔ1) = TempFile(tmpDir, ttʗ1.pattern);
                    var tmpfileʗ1 = tmpfile;
                    defer(() => {
                        if (tmpfileʗ1 != nil) {
                            tmpfileʗ1.Close();
                        }
                    }, ref ᒐ);
                    if (ttʗ1.wantErr){
                        if (errΔ1 == default!){
                            tΔ1.Errorf("Expected an error for pattern %q"u8, ttʗ1.pattern);
                        } else 
                        if (!strings.Contains(errΔ1.Error(), patternHasSeparator)) {
                            tΔ1.Errorf("Error mismatch: got %#v, want %q for pattern %q"u8, errΔ1, patternHasSeparator, ttʗ1.pattern);
                        }
                    } else 
                    if (errΔ1 != default!) {
                        tΔ1.Errorf("Unexpected error %v for pattern %q"u8, errΔ1, ttʗ1.pattern);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string notExistsˢ2 = "/_not_exists_"u8;
private static readonly @string xyzˢ = "*xyz"u8;

[GoType("dyn")] internal partial struct TestTempDir_tests {
    internal @string pattern;
    internal @string wantPrefix, wantSuffix;
}

public static void TestTempDir(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (name, err) = TempDir(notExistsˢ2, fooˢ);
    if (name != ""u8 || err == default!) {
        Ꮡt.Errorf("TempDir(`/_not_exists_`, `foo`) = %v, %v"u8, name, err);
    }
    var tests = new TestTempDir_tests[]{
        new("ioutil_test"u8, "ioutil_test"u8, ""u8),
        new("ioutil_test*"u8, "ioutil_test"u8, ""u8),
        new("ioutil_test*xyz"u8, "ioutil_test"u8, "xyz"u8)
    }.slice();
    @string dir = os.TempDir();
    void runTestTempDir(ж<testing.T> tΔ1, @string pattern, @string wantRePat) {
        GoFrame ᒐ = default;
        try {
            var (nameΔ1, errΔ1) = TempDir(dir, pattern);
            if (nameΔ1 == ""u8 || errΔ1 != default!) {
                tΔ1.Fatalf("TempDir(dir, `ioutil_test`) = %v, %v"u8, nameΔ1, errΔ1);
            }
            defer(os.Remove, nameΔ1, ref ᒐ);
            var re = regexp.MustCompile(wantRePat);
            if (!re.MatchString(nameΔ1)) {
                tΔ1.Errorf("TempDir(%q, %q) created bad name\n\t%q\ndid not match pattern\n\t%q"u8, dir, pattern, nameΔ1, wantRePat);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestTempDir_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var runTestTempDirʗ1 = runTestTempDir;
        var ttʗ1 = tt;
        Ꮡt.Run(tt.pattern, (ж<testing.T> tΔ2) => {
            @string wantRePat = "^"u8 + regexp.QuoteMeta(filepath.Join(dir, ttʗ1.wantPrefix)) + "[0-9]+"u8 + regexp.QuoteMeta(ttʗ1.wantSuffix) + "$"u8;
            runTestTempDirʗ1(tΔ2, ttʗ1.pattern, wantRePat);
        });
    }
    // Separately testing "*xyz" (which has no prefix). That is when constructing the
    // pattern to assert on, as in the previous loop, using filepath.Join for an empty
    // prefix filepath.Join(dir, ""), produces the pattern:
    //     ^<DIR>[0-9]+xyz$
    // yet we just want to match
    //     "^<DIR>/[0-9]+xyz"
    var runTestTempDirʗ2 = runTestTempDir;
    Ꮡt.Run(xyzˢ, (ж<testing.T> tΔ3) => {
        @string wantRePat = "^"u8 + regexp.QuoteMeta(filepath.Join(dir)) + regexp.QuoteMeta(((@string)(rune)filepath.Separator)) + "[0-9]+xyz$"u8;
        runTestTempDirʗ2(tΔ3, xyzˢ, wantRePat);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testTempDirBadDirˢ = "TestTempDir_BadDir"u8;
private static readonly @string notExistˢ = "not-exist"u8;

// test that we return a nice error message if the dir argument to TempDir doesn't
// exist (or that it's empty and os.TempDir doesn't exist)
public static void TestTempDir_BadDir(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (dir, err) = TempDir(""u8, testTempDirBadDirˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(os.RemoveAll, dir, ref ᒐ);
        @string badDir = filepath.Join(dir, notExistˢ);
        (_, err) = TempDir(badDir, fooˢ);
        {
            var (pe, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok || !os.IsNotExist(err) || (~pe).Path != badDir) {
                Ꮡt.Errorf("TempDir error = %#v; want PathError for path %q satisfying os.IsNotExist"u8, err, badDir);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string sepᶜ1 = "\\";

[GoType("dyn")] internal partial struct TestTempDir_BadPattern_tests {
    internal @string pattern;
    internal bool wantErr;
}

public static void TestTempDir_BadPattern(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (tmpDir, err) = TempDir(""u8, Ꮡt.Name());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(os.RemoveAll, tmpDir, ref ᒐ);
        @string sep = sepᶜ1;
        var tests = new TestTempDir_BadPattern_tests[]{
            new("ioutil*test"u8, false),
            new("ioutil_test*foo"u8, false),
            new("ioutil_test"u8 + sep + "foo"u8, true),
            new("ioutil_test*"u8 + sep + "foo"u8, true),
            new("ioutil_test"u8 + sep + "*foo"u8, true),
            new(sep + "ioutil_test"u8 + sep + "*foo"u8, true),
            new("ioutil_test*foo"u8 + sep, true)
        }.slice();
        foreach (var (_, vᴛ1) in tests) {
            ref var tt = ref heap(new TestTempDir_BadPattern_tests(), out var Ꮡtt);
            tt = vᴛ1;

            var ttʗ1 = tt;
            Ꮡt.Run(tt.pattern, (ж<testing.T> tΔ1) => {
                var (_, errΔ1) = TempDir(tmpDir, ttʗ1.pattern);
                if (ttʗ1.wantErr){
                    if (errΔ1 == default!){
                        tΔ1.Errorf("Expected an error for pattern %q"u8, ttʗ1.pattern);
                    } else 
                    if (!strings.Contains(errΔ1.Error(), patternHasSeparator)) {
                        tΔ1.Errorf("Error mismatch: got %#v, want %q for pattern %q"u8, errΔ1, patternHasSeparator, ttʗ1.pattern);
                    }
                } else 
                if (errΔ1 != default!) {
                    tΔ1.Errorf("Unexpected error %v for pattern %q"u8, errΔ1, ttʗ1.pattern);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end ioutil_test_package
