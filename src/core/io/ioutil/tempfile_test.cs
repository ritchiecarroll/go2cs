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
private static ref readonly @string testTempFileBadDirˢ => ref testTempFileBadDirˢᵇ;
private static readonly @string testTempFileBadDirˢᵇ = "TestTempFile_BadDir"u8;
private static ref readonly @string notExistsˢ => ref notExistsˢᵇ;
private static readonly @string notExistsˢᵇ = "_not_exists_"u8;
private static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;

public static void TestTempFile(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (dir, err) = TempDir(""u8, testTempFileBadDirˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        defer(os.RemoveAll, dir, ref ᒐ);

        @string nonexistentDir = filepath.Join(dir, notExistsˢ);
        (var f, err) = TempFile(nonexistentDir, fooˢ);
        if (f != nil || err == nil) {
            t.Errorf("TempFile(%q, `foo`) = %v, %v", nonexistentDir, f.OrTypedNil(), err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestTempFile_pattern_tests /*dyn*/ {
    internal @string pattern, prefix, suffix;
}

public static void TestTempFile_pattern(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var tests = slice<TestTempFile_pattern_tests>([
            new("ioutil_test", "ioutil_test", ""),
            new("ioutil_test*", "ioutil_test", ""),
            new("ioutil_test*xyz", "ioutil_test", "xyz")
        ]);
        foreach (var (_, test) in range(tests)) {
            var (f, err) = TempFile(""u8, test.pattern);
            if (err != nil) {
                t.Errorf("TempFile(..., %q) error: %v", test.pattern, err);
                continue;
            }
            defer(os.Remove, f.Name(), ref ᒐ);
            @string @base = filepath.Base(f.Name());
            f.Close();
            if (!(strings.HasPrefix(@base, test.prefix) && strings.HasSuffix(@base, test.suffix))) {
                t.Errorf("TempFile pattern %q created bad name %q; want prefix %q & suffix %q",
                    test.pattern, @base, test.prefix, test.suffix);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// This string is from os.errPatternHasSeparator.
internal static ref readonly @string patternHasSeparator => ref patternHasSeparatorᵇ;
private static readonly @string patternHasSeparatorᵇ = "pattern contains path separator"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static ref readonly @string sepᶜ => ref sepᶜᵇ;
private static readonly @string sepᶜᵇ = "\\";

internal partial struct TestTempFile_BadPattern_tests /*dyn*/ {
    internal @string pattern;
    internal bool wantErr;
}

public static void TestTempFile_BadPattern(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (tmpDir, err) = TempDir(""u8, t.Name());
        if (err != nil) {
            t.Fatal(err);
        }
        defer(os.RemoveAll, tmpDir, ref ᒐ);

        @string sep = sepᶜ;
        var tests = slice<TestTempFile_BadPattern_tests>([
            new("ioutil*test", false),
            new("ioutil_test*foo", false),
            new("ioutil_test"u8 + sep + "foo"u8, true),
            new("ioutil_test*"u8 + sep + "foo"u8, true),
            new("ioutil_test"u8 + sep + "*foo"u8, true),
            new(sep + "ioutil_test"u8 + sep + "*foo"u8, true),
            new("ioutil_test*foo"u8 + sep, true)
        ]);
        foreach (var (_, vᴛ1) in range(tests)) {
            ref var tt = ref heap(new TestTempFile_BadPattern_tests(), out var Ꮡtt);
            tt = vᴛ1;

            var ttʗ1 = tt;
            t.Run(tt.pattern, (ж<testing.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    var (tmpfile, errΔ1) = TempFile(tmpDir, ttʗ1.pattern);
                    var tmpfileʗ1 = tmpfile;
                    defer(() => {
                        if (tmpfileʗ1 != nil) {
                            tmpfileʗ1.Close();
                        }
                    }, ref ᒐ);
                    if (ttʗ1.wantErr) {
                        if (errΔ1 == nil) {
                            tΔ1.Errorf("Expected an error for pattern %q", ttʗ1.pattern);
                        } else if (!strings.Contains(errΔ1.Error(), patternHasSeparator)) {
                            tΔ1.Errorf("Error mismatch: got %#v, want %q for pattern %q", errΔ1, patternHasSeparator, ttʗ1.pattern);
                        }
                    } else if (errΔ1 != nil) {
                        tΔ1.Errorf("Unexpected error %v for pattern %q", errΔ1, ttʗ1.pattern);
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
private static ref readonly @string notExistsˢ2 => ref notExistsˢ2ᵇ;
private static readonly @string notExistsˢ2ᵇ = "/_not_exists_"u8;
private static ref readonly @string xyzˢ => ref xyzˢᵇ;
private static readonly @string xyzˢᵇ = "*xyz"u8;

internal partial struct TestTempDir_tests /*dyn*/ {
    internal @string pattern;
    internal @string wantPrefix, wantSuffix;
}

public static void TestTempDir(ж<testing.T> t) {
    var (name, err) = TempDir(notExistsˢ2, fooˢ);
    if (name != ""u8 || err == nil) {
        t.Errorf("TempDir(`/_not_exists_`, `foo`) = %v, %v", name, err);
    }

    var tests = slice<TestTempDir_tests>([
        new("ioutil_test", "ioutil_test", ""),
        new("ioutil_test*", "ioutil_test", ""),
        new("ioutil_test*xyz", "ioutil_test", "xyz")
    ]);

    @string dir = os.TempDir();

    void runTestTempDir(ж<testing.T> tΔ1, @string pattern, @string wantRePat) {
        GoFrame ᒐ = default;
        try {
            var (nameΔ1, errΔ1) = TempDir(dir, pattern);
            if (nameΔ1 == ""u8 || errΔ1 != nil) {
                tΔ1.Fatalf("TempDir(dir, `ioutil_test`) = %v, %v", nameΔ1, errΔ1);
            }
            defer(os.Remove, nameΔ1, ref ᒐ);

            var re = regexp.MustCompile(wantRePat);
            if (!re.MatchString(nameΔ1)) {
                tΔ1.Errorf("TempDir(%q, %q) created bad name\n\t%q\ndid not match pattern\n\t%q", dir, pattern, nameΔ1, wantRePat);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestTempDir_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var runTestTempDirʗ1 = runTestTempDir;
        var ttʗ1 = tt;
        t.Run(tt.pattern, (ж<testing.T> tΔ2) => {
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
    t.Run(xyzˢ, (ж<testing.T> tΔ3) => {
        @string wantRePat = "^"u8 + regexp.QuoteMeta(filepath.Join(dir)) + regexp.QuoteMeta((@string)(rune)filepath.Separator) + "[0-9]+xyz$"u8;
        runTestTempDirʗ2(tΔ3, xyzˢ, wantRePat);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string testTempDirBadDirˢ => ref testTempDirBadDirˢᵇ;
private static readonly @string testTempDirBadDirˢᵇ = "TestTempDir_BadDir"u8;
private static ref readonly @string notExistˢ => ref notExistˢᵇ;
private static readonly @string notExistˢᵇ = "not-exist"u8;

// test that we return a nice error message if the dir argument to TempDir doesn't
// exist (or that it's empty and os.TempDir doesn't exist)
public static void TestTempDir_BadDir(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (dir, err) = TempDir(""u8, testTempDirBadDirˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        defer(os.RemoveAll, dir, ref ᒐ);

        @string badDir = filepath.Join(dir, notExistˢ);
        (_, err) = TempDir(badDir, fooˢ);
        var (pe, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok || !os.IsNotExist(err) || pe.Path != badDir) {
            t.Errorf("TempDir error = %#v; want PathError for path %q satisfying os.IsNotExist", err, badDir);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static ref readonly @string sepᶜ1 => ref sepᶜ1ᵇ;
private static readonly @string sepᶜ1ᵇ = "\\";

internal partial struct TestTempDir_BadPattern_tests /*dyn*/ {
    internal @string pattern;
    internal bool wantErr;
}

public static void TestTempDir_BadPattern(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (tmpDir, err) = TempDir(""u8, t.Name());
        if (err != nil) {
            t.Fatal(err);
        }
        defer(os.RemoveAll, tmpDir, ref ᒐ);

        @string sep = sepᶜ1;
        var tests = slice<TestTempDir_BadPattern_tests>([
            new("ioutil*test", false),
            new("ioutil_test*foo", false),
            new("ioutil_test"u8 + sep + "foo"u8, true),
            new("ioutil_test*"u8 + sep + "foo"u8, true),
            new("ioutil_test"u8 + sep + "*foo"u8, true),
            new(sep + "ioutil_test"u8 + sep + "*foo"u8, true),
            new("ioutil_test*foo"u8 + sep, true)
        ]);
        foreach (var (_, vᴛ1) in range(tests)) {
            ref var tt = ref heap(new TestTempDir_BadPattern_tests(), out var Ꮡtt);
            tt = vᴛ1;

            var ttʗ1 = tt;
            t.Run(tt.pattern, (ж<testing.T> tΔ1) => {
                var (_, errΔ1) = TempDir(tmpDir, ttʗ1.pattern);
                if (ttʗ1.wantErr) {
                    if (errΔ1 == nil) {
                        tΔ1.Errorf("Expected an error for pattern %q", ttʗ1.pattern);
                    } else if (!strings.Contains(errΔ1.Error(), patternHasSeparator)) {
                        tΔ1.Errorf("Error mismatch: got %#v, want %q for pattern %q", errΔ1, patternHasSeparator, ttʗ1.pattern);
                    }
                } else if (errΔ1 != nil) {
                    tΔ1.Errorf("Unexpected error %v for pattern %q", errΔ1, ttʗ1.pattern);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end ioutil_test_package
