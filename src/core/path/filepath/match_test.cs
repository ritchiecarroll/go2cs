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

internal static slice<MatchTest> matchTests = [
    new("abc", "abc", true, default),
    new("*", "abc", true, default),
    new("*c", "abc", true, default),
    new("a*", "a", true, default),
    new("a*", "abc", true, default),
    new("a*", "ab/c", false, default),
    new("a*/b", "abc/b", true, default),
    new("a*/b", "a/c/b", false, default),
    new("a*b*c*d*e*/f", "axbxcxdxe/f", true, default),
    new("a*b*c*d*e*/f", "axbxcxdxexxx/f", true, default),
    new("a*b*c*d*e*/f", "axbxcxdxe/xxx/f", false, default),
    new("a*b*c*d*e*/f", "axbxcxdxexxx/fff", false, default),
    new("a*b?c*x", "abxbbxdbxebxczzx", true, default),
    new("a*b?c*x", "abxbbxdbxebxczzy", false, default),
    new("ab[c]", "abc", true, default),
    new("ab[b-d]", "abc", true, default),
    new("ab[e-g]", "abc", false, default),
    new("ab[^c]", "abc", false, default),
    new("ab[^b-d]", "abc", false, default),
    new("ab[^e-g]", "abc", true, default),
    new("a\\*b", "a*b", true, default),
    new("a\\*b", "ab", false, default),
    new("a?b", "a☺b", true, default),
    new("a[^a]b", "a☺b", true, default),
    new("a???b", "a☺b", false, default),
    new("a[^a][^a][^a]b", "a☺b", false, default),
    new("[a-ζ]*", "α", true, default),
    new("*[a-ζ]", "A", false, default),
    new("a?b", "a/b", false, default),
    new("a*b", "a/b", false, default),
    new("[\\]a]", "]", true, default),
    new("[\\-]", "-", true, default),
    new("[x\\-]", "x", true, default),
    new("[x\\-]", "-", true, default),
    new("[x\\-]", "z", false, default),
    new("[\\-x]", "x", true, default),
    new("[\\-x]", "-", true, default),
    new("[\\-x]", "a", false, default),
    new("[]a]", "]", false, ErrBadPattern),
    new("[-]", "-", false, ErrBadPattern),
    new("[x-]", "x", false, ErrBadPattern),
    new("[x-]", "-", false, ErrBadPattern),
    new("[x-]", "z", false, ErrBadPattern),
    new("[-x]", "x", false, ErrBadPattern),
    new("[-x]", "-", false, ErrBadPattern),
    new("[-x]", "a", false, ErrBadPattern),
    new("\\", "a", false, ErrBadPattern),
    new("[a-b-c]", "a", false, ErrBadPattern),
    new("[", "a", false, ErrBadPattern),
    new("[^", "a", false, ErrBadPattern),
    new("[^bc", "a", false, ErrBadPattern),
    new("a[", "a", false, ErrBadPattern),
    new("a[", "ab", false, ErrBadPattern),
    new("a[", "x", false, ErrBadPattern),
    new("a/b[", "x", false, ErrBadPattern),
    new("*x", "xxx", true, default)
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilˢ => ref nilˢᵇ;
private static readonly @string nilˢᵇ = "<nil>"u8;

internal static @string errp(error e) {
    if (e == nil) {
        return nilˢ;
    }
    return e.Error();
}

public static void TestMatch(ж<testing.T> t) {
    foreach (var (_, tt) in range(matchTests)) {
        @string pattern = tt.pattern;
        @string s = tt.s;
        if (runtime.GOOS == "windows"u8) {
            if (strings.Contains(pattern, "\\")) {
                // no escape allowed on windows.
                continue;
            }
            pattern = Clean(pattern);
            s = Clean(s);
        }
        var (ok, err) = Match(pattern, s);
        if (ok != tt.match || !AreEqual(err, tt.err)) {
            t.Errorf("Match(%#q, %#q) = %v, %q want %v, %q", pattern, s, ok, errp(err), tt.match, errp(tt.err));
        }
    }
}

public static void BenchmarkMatch(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in range(matchTests)) {
        ref var tt = ref heap(new MatchTest(), out var Ꮡtt);
        tt = vᴛ1;

        @string name = fmt.Sprintf("%q %q"u8, tt.pattern, tt.s);
        var ttʗ1 = tt;
        b.Run(name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            foreach (var _ᴛ1 in range((~bΔ1).N)) {
                (bSink, errSink) = Match(ttʗ1.pattern, ttʗ1.s);
            }
        });
    }
}

internal static bool bSink;
internal static error errSink;

partial struct globTestsᴛ1 /*dyn*/ {
    internal @string pattern, result;
}
internal static slice<globTestsᴛ1> globTests = [
    new("match.go", "match.go"),
    new("mat?h.go", "match.go"),
    new("*", "match.go"),
    new("../*/match.go", "../filepath/match.go")
];

public static void TestGlob(ж<testing.T> t) {
    foreach (var (_, tt) in range(globTests)) {
        @string pattern = tt.pattern;
        @string result = tt.result;
        if (runtime.GOOS == "windows"u8) {
            pattern = Clean(pattern);
            result = Clean(result);
        }
        var (matches, err) = Glob(pattern);
        if (err != nil) {
            t.Errorf("Glob error for %q: %s", pattern, err);
            continue;
        }
        if (!slices.Contains(matches, result)) {
            t.Errorf("Glob(%#q) = %#v want %v", pattern, matches, result);
        }
    }
    foreach (var (_, pattern) in range(slice<@string>(["no_match", "../*/no_match"]))) {
        var (matches, err) = Glob(pattern);
        if (err != nil) {
            t.Errorf("Glob error for %q: %s", pattern, err);
            continue;
        }
        if (len(matches) != 0) {
            t.Errorf("Glob(%#q) = %#v want []", pattern, matches);
        }
    }
}

public static partial void TestCVE202230632(ж<testing.T> t) {
    // Prior to CVE-2022-30632, this would cause a stack exhaustion given a
    // large number of separators (more than 4,000,000). There is now a limit
    // of 10,000.
    var (_, err) = Glob("/*"u8 + strings.Repeat("/", 10001));
    if (!AreEqual(err, ErrBadPattern)) {
        t.Fatalf("Glob returned err=%v, want ErrBadPattern", err);
    }
}

public static void TestGlobError(ж<testing.T> t) {
    var bad = slice<@string>([@"[]", @"nonexist/[]"]);
    foreach (var (_, pattern) in range(bad)) {
        var (_, err) = Glob(pattern); if (!AreEqual(err, ErrBadPattern)) {
            t.Errorf("Glob(%#q) returned err=%v, want ErrBadPattern", pattern, err);
        }
    }
}

public static void TestGlobUNC(ж<testing.T> t) {
    // Just make sure this runs without crashing for now.
    // See issue 15879.
    Glob(@"\\?\C:\*");
}

partial struct globSymlinkTestsᴛ1 /*dyn*/ {
    internal @string path, dest;
    internal bool brokenLink;
}
internal static slice<globSymlinkTestsᴛ1> globSymlinkTests = [
    new("test1", "link1", false),
    new("test2", "link2", true)
];

public static void TestGlobSymlink(ж<testing.T> t) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));

    @string tmpDir = t.TempDir();
    foreach (var (_, tt) in range(globSymlinkTests)) {
        @string path = Join(tmpDir, tt.path);
        @string dest = Join(tmpDir, tt.dest);
        var (f, err) = os.Create(path);
        if (err != nil) {
            t.Fatal(err);
        }
        var errΔ1 = f.Close(); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        err = os.Symlink(path, dest);
        if (err != nil) {
            t.Fatal(err);
        }
        if (tt.brokenLink) {
            // Break the symlink.
            os.Remove(path);
        }
        (var matches, err) = Glob(dest);
        if (err != nil) {
            t.Errorf("GlobSymlink error for %q: %s", dest, err);
        }
        if (!slices.Contains(matches, dest)) {
            t.Errorf("Glob(%#q) = %#v want %v", dest, matches, dest);
        }
    }
}

partial struct globTest {
    internal @string pattern;
    internal slice<@string> matches;
}

internal static slice<@string> buildWant(this ref globTest test, @string root) {
    var want = new slice<@string>(0);
    foreach (var (_, m) in range(test.matches)) {
        want = append(want, root + FromSlash(m));
    }
    slices.Sort<slice<@string>, @string>(want);
    return want;
}

internal static error globAbs(this ref globTest test, @string root, @string rootPattern) {
    @string p = FromSlash(rootPattern + @"\"u8 + test.pattern);
    var (have, err) = Glob(p);
    if (err != nil) {
        return err;
    }
    slices.Sort<slice<@string>, @string>(have);
    var want = test.buildWant(root + @"\"u8);
    if (slices.Equal<slice<@string>, @string>(want, have)) {
        return default;
    }
    return fmt.Errorf("Glob(%q) returns %q, but %q expected"u8, p, have, want);
}

internal static error globRel(this ref globTest test, @string root) {
    @string p = root + FromSlash(test.pattern);
    var (have, err) = Glob(p);
    if (err != nil) {
        return err;
    }
    slices.Sort<slice<@string>, @string>(have);
    var want = test.buildWant(root);
    if (slices.Equal<slice<@string>, @string>(want, have)) {
        return default;
    }
    // try also matching version without root prefix
    var wantWithNoRoot = test.buildWant(""u8);
    if (slices.Equal<slice<@string>, @string>(wantWithNoRoot, have)) {
        return default;
    }
    return fmt.Errorf("Glob(%q) returns %q, but %q expected"u8, p, have, want);
}

public static void TestWindowsGlob(ж<testing.T> t) {
    if (runtime.GOOS != "windows"u8) {
        t.Skipf("skipping windows specific test");
    }

    @string tmpDir = tempDirCanonical(t);
    if (len(tmpDir) < 3) {
        t.Fatalf("tmpDir path %q is too short", tmpDir);
    }
    if (tmpDir[1] != ':') {
        t.Fatalf("tmpDir path %q must have drive letter in it", tmpDir);
    }

    var dirs = slice<@string>([
        "a",
        "b",
        "dir/d/bin"
    ]);
    var files = slice<@string>([
        "dir/d/bin/git.exe"
    ]);
    foreach (var (_, dir) in range(dirs)) {
        var err = os.MkdirAll(Join(tmpDir, dir), 511);
        if (err != nil) {
            t.Fatal(err);
        }
    }
    foreach (var (_, @file) in range(files)) {
        var err = os.WriteFile(Join(tmpDir, @file), default, 438);
        if (err != nil) {
            t.Fatal(err);
        }
    }

    var tests = slice<globTest>([
        new("a", ["a"]),
        new("b", ["b"]),
        new("c", []),
        new("*", ["a", "b", "dir"]),
        new("d*", ["dir"]),
        new("*i*", ["dir"]),
        new("*r", ["dir"]),
        new("?ir", ["dir"]),
        new("?r", []),
        new("d*/*/bin/git.exe", ["dir/d/bin/git.exe"])
    ]);

    // test absolute paths
    foreach (var (_, vᴛ1) in range(tests)) {
        var test = vᴛ1;

        @string p = default;
        {
            var err = test.globAbs(tmpDir, tmpDir); if (err != nil) {
                t.Error(err);
            }
        }
        // test C:\*Documents and Settings\...
        p = tmpDir;
        p = strings.Replace(p, @":\", @":\*", 1);
        {
            var err = test.globAbs(tmpDir, p); if (err != nil) {
                t.Error(err);
            }
        }
        // test C:\Documents and Settings*\...
        p = tmpDir;
        p = strings.Replace(p, @":\", @":", 1);
        p = strings.Replace(p, @"\", @"*\", 1);
        p = strings.Replace(p, @":", @":\", 1);
        {
            var err = test.globAbs(tmpDir, p); if (err != nil) {
                t.Error(err);
            }
        }
    }

    // test relative paths
    t.Chdir(tmpDir);
    foreach (var (_, vᴛ2) in range(tests)) {
        var test = vᴛ2;

        var err = test.globRel(""u8);
        if (err != nil) {
            t.Error(err);
        }
        err = test.globRel(@".\");
        if (err != nil) {
            t.Error(err);
        }
        err = test.globRel(tmpDir[..2]); // C:
        if (err != nil) {
            t.Error(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string matchGoˢ => ref matchGoˢᵇ;
private static readonly @string matchGoˢᵇ = @"\match.go"u8;

public static void TestNonWindowsGlobEscape(ж<testing.T> t) {
    if (runtime.GOOS == "windows"u8) {
        t.Skipf("skipping non-windows specific test");
    }
    @string pattern = matchGoˢ;
    var want = slice<@string>(["match.go"]);
    var (matches, err) = Glob(pattern);
    if (err != nil) {
        t.Fatalf("Glob error for %q: %s", pattern, err);
    }
    if (!slices.Equal<slice<@string>, @string>(matches, want)) {
        t.Fatalf("Glob(%#q) = %v want %v", pattern, matches, want);
    }
}

} // end filepath_test_package
