// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using static go.io.fs_package;
using os = os_package;
using path = path_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = go.io.fs_package;

partial class fs_test_package {

partial struct globTestsᴛ1 /*dyn*/ {
    internal fs.FS fs;
    internal @string pattern, result;
}
internal static slice<globTestsᴛ1> globTests = slice<globTestsᴛ1>([
    new(os.DirFS("."), "glob.go", "glob.go"),
    new(os.DirFS("."), "gl?b.go", "glob.go"),
    new(os.DirFS("."), @"gl\ob.go", "glob.go"),
    new(os.DirFS("."), "*", "glob.go"),
    new(os.DirFS(".."), "*/glob.go", "fs/glob.go")
]);

public static void TestGlob(ж<testing.T> t) {
    foreach (var (_, tt) in globTests) {
        var (matches, err) = Glob(tt.fs, tt.pattern);
        if (err != nil) {
            t.Errorf("Glob error for %q: %s", tt.pattern, err);
            continue;
        }
        if (!slices.Contains(matches, tt.result)) {
            t.Errorf("Glob(%#q) = %#v want %v", tt.pattern, matches, tt.result);
        }
    }
    foreach (var (_, pattern) in slice<@string>(["no_match", "../*/no_match", @"\*"])) {
        var (matches, err) = Glob(os.DirFS("."), pattern);
        if (err != nil) {
            t.Errorf("Glob error for %q: %s", pattern, err);
            continue;
        }
        if (len(matches) != 0) {
            t.Errorf("Glob(%#q) = %#v want []", pattern, matches);
        }
    }
}

public static void TestGlobError(ж<testing.T> t) {
    var bad = slice<@string>([@"[]", @"nonexist/[]"]);
    foreach (var (_, pattern) in bad) {
        var (_, err) = Glob(os.DirFS("."), pattern);
        if (!AreEqual(err, path.ErrBadPattern)) {
            t.Errorf("Glob(fs, %#q) returned err=%v, want path.ErrBadPattern", pattern, err);
        }
    }
}

public static partial void TestCVE202230630(ж<testing.T> t) {
    // Prior to CVE-2022-30630, a stack exhaustion would occur given a large
    // number of separators. There is now a limit of 10,000.
    var (_, err) = Glob(os.DirFS("."), "/*"u8 + strings.Repeat("/", 10001));
    if (!AreEqual(err, path.ErrBadPattern)) {
        t.Fatalf("Glob returned err=%v, want %v", err, path.ErrBadPattern);
    }
}

partial struct globOnly {
    /*embed*/ public go.io.fs_package.GlobFS GlobFS;
}

internal static (fs.File, error) Open(this globOnly _, @string name) {
    return (default, ErrNotExist);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string txtˢ => ref txtˢᵇ;
private static readonly @string txtˢᵇ = "*.txt"u8;
private static ref readonly @string readDirOnlyˢ => ref readDirOnlyˢᵇ;
private static readonly @string readDirOnlyˢᵇ = "readDirOnly"u8;
private static ref readonly @string openOnlyˢ => ref openOnlyˢᵇ;
private static readonly @string openOnlyˢᵇ = "openOnly"u8;

public static partial void TestGlobMethod(ж<testing.T> t) {
    void check(@string desc, slice<@string> namesΔ1, error errΔ1) {
        t.Helper();
        if (errΔ1 != nil || len(namesΔ1) != 1 || namesΔ1[0] != "hello.txt") {
            t.Errorf("Glob(%s) = %v, %v, want %v, nil", desc, namesΔ1, errΔ1, slice<@string>(["hello.txt"]));
        }
    }

    // Test that ReadDir uses the method when present.
    var (names, err) = Glob(new globOnly(new fstest_MapFSᴠGlobFS(testFsys)), txtˢ);
    check(readDirOnlyˢ, names, err);

    // Test that ReadDir uses Open when the method is not present.
    (names, err) = Glob(new openOnly(new fstest_MapFSᴠFS(testFsys)), txtˢ);
    check(openOnlyˢ, names, err);
}

} // end fs_test_package
