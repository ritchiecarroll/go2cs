// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.path;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using os = os_package;
using static go.path.filepath_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using fs = io.fs_package;
using static go.path.filepath_internal_test_package;

partial class filepath_test_package {

partial struct MatchTest {
    internal @string pattern, s;
    internal bool match;
    internal error err;
}

internal static slice<MatchTest> matchTests = new MatchTest[]{
    new("abc"u8, "abc"u8, true, default!),
    new("*"u8, "abc"u8, true, default!),
    new("*c"u8, "abc"u8, true, default!),
    new("a*"u8, "a"u8, true, default!),
    new("a*"u8, "abc"u8, true, default!),
    new("a*"u8, "ab/c"u8, false, default!),
    new("a*/b"u8, "abc/b"u8, true, default!),
    new("a*/b"u8, "a/c/b"u8, false, default!),
    new("a*b*c*d*e*/f"u8, "axbxcxdxe/f"u8, true, default!),
    new("a*b*c*d*e*/f"u8, "axbxcxdxexxx/f"u8, true, default!),
    new("a*b*c*d*e*/f"u8, "axbxcxdxe/xxx/f"u8, false, default!),
    new("a*b*c*d*e*/f"u8, "axbxcxdxexxx/fff"u8, false, default!),
    new("a*b?c*x"u8, "abxbbxdbxebxczzx"u8, true, default!),
    new("a*b?c*x"u8, "abxbbxdbxebxczzy"u8, false, default!),
    new("ab[c]"u8, "abc"u8, true, default!),
    new("ab[b-d]"u8, "abc"u8, true, default!),
    new("ab[e-g]"u8, "abc"u8, false, default!),
    new("ab[^c]"u8, "abc"u8, false, default!),
    new("ab[^b-d]"u8, "abc"u8, false, default!),
    new("ab[^e-g]"u8, "abc"u8, true, default!),
    new("a\\*b"u8, "a*b"u8, true, default!),
    new("a\\*b"u8, "ab"u8, false, default!),
    new("a?b"u8, "a☺b"u8, true, default!),
    new("a[^a]b"u8, "a☺b"u8, true, default!),
    new("a???b"u8, "a☺b"u8, false, default!),
    new("a[^a][^a][^a]b"u8, "a☺b"u8, false, default!),
    new("[a-ζ]*"u8, "α"u8, true, default!),
    new("*[a-ζ]"u8, "A"u8, false, default!),
    new("a?b"u8, "a/b"u8, false, default!),
    new("a*b"u8, "a/b"u8, false, default!),
    new("[\\]a]"u8, "]"u8, true, default!),
    new("[\\-]"u8, "-"u8, true, default!),
    new("[x\\-]"u8, "x"u8, true, default!),
    new("[x\\-]"u8, "-"u8, true, default!),
    new("[x\\-]"u8, "z"u8, false, default!),
    new("[\\-x]"u8, "x"u8, true, default!),
    new("[\\-x]"u8, "-"u8, true, default!),
    new("[\\-x]"u8, "a"u8, false, default!),
    new("[]a]"u8, "]"u8, false, ErrBadPattern),
    new("[-]"u8, "-"u8, false, ErrBadPattern),
    new("[x-]"u8, "x"u8, false, ErrBadPattern),
    new("[x-]"u8, "-"u8, false, ErrBadPattern),
    new("[x-]"u8, "z"u8, false, ErrBadPattern),
    new("[-x]"u8, "x"u8, false, ErrBadPattern),
    new("[-x]"u8, "-"u8, false, ErrBadPattern),
    new("[-x]"u8, "a"u8, false, ErrBadPattern),
    new("\\"u8, "a"u8, false, ErrBadPattern),
    new("[a-b-c]"u8, "a"u8, false, ErrBadPattern),
    new("["u8, "a"u8, false, ErrBadPattern),
    new("[^"u8, "a"u8, false, ErrBadPattern),
    new("[^bc"u8, "a"u8, false, ErrBadPattern),
    new("a["u8, "a"u8, false, ErrBadPattern),
    new("a["u8, "ab"u8, false, ErrBadPattern),
    new("a["u8, "x"u8, false, ErrBadPattern),
    new("a/b["u8, "x"u8, false, ErrBadPattern),
    new("*x"u8, "xxx"u8, true, default!)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "<nil>"u8;

internal static @string errp(error e) {
    if (e == default!) {
        return nilˢ;
    }
    return e.Error();
}

public static void TestMatch(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in matchTests) {
        @string pattern = tt.pattern;
        @string s = tt.s;
        if (runtime.GOOS == "windows"u8) {
            if (strings.Contains(pattern, "\\"u8)) {
                // no escape allowed on windows.
                continue;
            }
            pattern = Clean(pattern);
            s = Clean(s);
        }
        var (ok, err) = Match(pattern, s);
        if (ok != tt.match || !AreEqual(err, tt.err)) {
            Ꮡt.Errorf("Match(%#q, %#q) = %v, %q want %v, %q"u8, pattern, s, ok, errp(err), tt.match, errp(tt.err));
        }
    }
}


partial struct globTestsᴛ1 /*dyn*/ {
    internal @string pattern, result;
}
internal static slice<globTestsᴛ1> globTests = new globTestsᴛ1[]{
    new("match.go"u8, "match.go"u8),
    new("mat?h.go"u8, "match.go"u8),
    new("*"u8, "match.go"u8),
    new("../*/match.go"u8, "../filepath/match.go"u8)
}.slice();

public static void TestGlob(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in globTests) {
        @string pattern = tt.pattern;
        @string result = tt.result;
        if (runtime.GOOS == "windows"u8) {
            pattern = Clean(pattern);
            result = Clean(result);
        }
        var (matches, err) = Glob(pattern);
        if (err != default!) {
            Ꮡt.Errorf("Glob error for %q: %s"u8, pattern, err);
            continue;
        }
        if (!slices.Contains(matches, result)) {
            Ꮡt.Errorf("Glob(%#q) = %#v want %v"u8, pattern, matches, result);
        }
    }
    foreach (var (_, pattern) in new @string[]{"no_match"u8, "../*/no_match"u8}.slice()) {
        var (matches, err) = Glob(pattern);
        if (err != default!) {
            Ꮡt.Errorf("Glob error for %q: %s"u8, pattern, err);
            continue;
        }
        if (len(matches) != 0) {
            Ꮡt.Errorf("Glob(%#q) = %#v want []"u8, pattern, matches);
        }
    }
}

public static void TestCVE202230632(ж<testing.T> Ꮡt) {
    // Prior to CVE-2022-30632, this would cause a stack exhaustion given a
    // large number of separators (more than 4,000,000). There is now a limit
    // of 10,000.
    var (_, err) = Glob("/*"u8 + strings.Repeat("/"u8, 10001));
    if (!AreEqual(err, ErrBadPattern)) {
        Ꮡt.Fatalf("Glob returned err=%v, want ErrBadPattern"u8, err);
    }
}

public static void TestGlobError(ж<testing.T> Ꮡt) {
    var bad = new @string[]{@"[]"u8, @"nonexist/[]"u8}.slice();
    foreach (var (_, pattern) in bad) {
        {
            var (_, err) = Glob(pattern); if (!AreEqual(err, ErrBadPattern)) {
                Ꮡt.Errorf("Glob(%#q) returned err=%v, want ErrBadPattern"u8, pattern, err);
            }
        }
    }
}

public static void TestGlobUNC(ж<testing.T> Ꮡt) {
    // Just make sure this runs without crashing for now.
    // See issue 15879.
    Glob(@"\\?\C:\*"u8);
}


partial struct globSymlinkTestsᴛ1 /*dyn*/ {
    internal @string path, dest;
    internal bool brokenLink;
}
internal static slice<globSymlinkTestsᴛ1> globSymlinkTests = new globSymlinkTestsᴛ1[]{
    new("test1"u8, "link1"u8, false),
    new("test2"u8, "link2"u8, true)
}.slice();

public static void TestGlobSymlink(ж<testing.T> Ꮡt) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    @string tmpDir = Ꮡt.TempDir();
    foreach (var (_, tt) in globSymlinkTests) {
        @string path = Join(tmpDir, tt.path);
        @string dest = Join(tmpDir, tt.dest);
        var (f, err) = os.Create(path);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            var errΔ1 = f.Close(); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        err = os.Symlink(path, dest);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (tt.brokenLink) {
            // Break the symlink.
            os.Remove(path);
        }
        (var matches, err) = Glob(dest);
        if (err != default!) {
            Ꮡt.Errorf("GlobSymlink error for %q: %s"u8, dest, err);
        }
        if (!slices.Contains(matches, dest)) {
            Ꮡt.Errorf("Glob(%#q) = %#v want %v"u8, dest, matches, dest);
        }
    }
}

partial struct globTest {
    internal @string pattern;
    internal slice<@string> matches;
}

internal static slice<@string> buildWant(this ref globTest test, @string root) {
    var want = new slice<@string>(0);
    foreach (var (_, m) in test.matches) {
        want = append(want, root + FromSlash(m));
    }
    slices.Sort<slice<@string>, @string>(want);
    return want;
}

internal static error globAbs(this ref globTest test, @string root, @string rootPattern) {
    @string p = FromSlash(rootPattern + @"\"u8 + test.pattern);
    var (have, err) = Glob(p);
    if (err != default!) {
        return err;
    }
    slices.Sort<slice<@string>, @string>(have);
    var want = test.buildWant(root + @"\"u8);
    if (strings.Join(want, "_"u8) == strings.Join(have, "_"u8)) {
        return default!;
    }
    return fmt.Errorf("Glob(%q) returns %q, but %q expected"u8, p, have, want);
}

internal static error globRel(this ref globTest test, @string root) {
    @string p = root + FromSlash(test.pattern);
    var (have, err) = Glob(p);
    if (err != default!) {
        return err;
    }
    slices.Sort<slice<@string>, @string>(have);
    var want = test.buildWant(root);
    if (strings.Join(want, "_"u8) == strings.Join(have, "_"u8)) {
        return default!;
    }
    // try also matching version without root prefix
    var wantWithNoRoot = test.buildWant(""u8);
    if (strings.Join(wantWithNoRoot, "_"u8) == strings.Join(have, "_"u8)) {
        return default!;
    }
    return fmt.Errorf("Glob(%q) returns %q, but %q expected"u8, p, have, want);
}

public static void TestWindowsGlob(ж<testing.T> Ꮡt) {
    if (runtime.GOOS != "windows"u8) {
        Ꮡt.Skipf("skipping windows specific test"u8);
    }
    @string tmpDir = tempDirCanonical(Ꮡt);
    if (len(tmpDir) < 3) {
        Ꮡt.Fatalf("tmpDir path %q is too short"u8, tmpDir);
    }
    if (tmpDir[1] != (rune)':') {
        Ꮡt.Fatalf("tmpDir path %q must have drive letter in it"u8, tmpDir);
    }
    var dirs = new @string[]{
        "a"u8,
        "b"u8,
        "dir/d/bin"u8
    }.slice();
    var files = new @string[]{
        "dir/d/bin/git.exe"u8
    }.slice();
    foreach (var (_, dir) in dirs) {
        var err = os.MkdirAll(Join(tmpDir, dir), 511);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    foreach (var (_, @file) in files) {
        var err = os.WriteFile(Join(tmpDir, @file), default!, 438);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    var tests = new globTest[]{
        new("a"u8, new @string[]{"a"u8}.slice()),
        new("b"u8, new @string[]{"b"u8}.slice()),
        new("c"u8, new @string[]{}.slice()),
        new("*"u8, new @string[]{"a"u8, "b"u8, "dir"u8}.slice()),
        new("d*"u8, new @string[]{"dir"u8}.slice()),
        new("*i*"u8, new @string[]{"dir"u8}.slice()),
        new("*r"u8, new @string[]{"dir"u8}.slice()),
        new("?ir"u8, new @string[]{"dir"u8}.slice()),
        new("?r"u8, new @string[]{}.slice()),
        new("d*/*/bin/git.exe"u8, new @string[]{"dir/d/bin/git.exe"u8}.slice())
    }.slice();
    // test absolute paths
    foreach (var (_, vᴛ1) in tests) {
        var test = vᴛ1;

        @string p = default!;
        {
            var err = test.globAbs(tmpDir, tmpDir); if (err != default!) {
                Ꮡt.Error(err);
            }
        }
        // test C:\*Documents and Settings\...
        p = tmpDir;
        p = strings.Replace(p, @":\"u8, @":\*"u8, 1);
        {
            var err = test.globAbs(tmpDir, p); if (err != default!) {
                Ꮡt.Error(err);
            }
        }
        // test C:\Documents and Settings*\...
        p = tmpDir;
        p = strings.Replace(p, @":\"u8, @":"u8, 1);
        p = strings.Replace(p, @"\"u8, @"*\"u8, 1);
        p = strings.Replace(p, @":"u8, @":\"u8, 1);
        {
            var err = test.globAbs(tmpDir, p); if (err != default!) {
                Ꮡt.Error(err);
            }
        }
    }
    // test relative paths
    Ꮡt.Chdir(tmpDir);
    foreach (var (_, vᴛ2) in tests) {
        var test = vᴛ2;

        var err = test.globRel(""u8);
        if (err != default!) {
            Ꮡt.Error(err);
        }
        err = test.globRel(@".\"u8);
        if (err != default!) {
            Ꮡt.Error(err);
        }
        err = test.globRel(tmpDir[..2]); // C:
        if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string matchGoˢ = @"\match.go"u8;

public static void TestNonWindowsGlobEscape(ж<testing.T> Ꮡt) {
    if (runtime.GOOS == "windows"u8) {
        Ꮡt.Skipf("skipping non-windows specific test"u8);
    }
    @string pattern = matchGoˢ;
    var want = new @string[]{"match.go"u8}.slice();
    var (matches, err) = Glob(pattern);
    if (err != default!) {
        Ꮡt.Fatalf("Glob error for %q: %s"u8, pattern, err);
    }
    if (!slices.Equal<slice<@string>, @string>(matches, want)) {
        Ꮡt.Fatalf("Glob(%#q) = %v want %v"u8, pattern, matches, want);
    }
}

} // end filepath_test_package
