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
using fs = go.io.fs_package;

partial class fs_test_package {


[GoType("dyn")] partial struct globTestsᴛ1 {
    internal fs.FS fs;
    internal @string pattern, result;
}
internal static slice<globTestsᴛ1> globTests = new globTestsᴛ1[]{
    new(os.DirFS("."u8), "glob.go"u8, "glob.go"u8),
    new(os.DirFS("."u8), "gl?b.go"u8, "glob.go"u8),
    new(os.DirFS("."u8), @"gl\ob.go"u8, "glob.go"u8),
    new(os.DirFS("."u8), "*"u8, "glob.go"u8),
    new(os.DirFS(".."u8), "*/glob.go"u8, "fs/glob.go"u8)
}.slice();

public static void TestGlob(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in globTests) {
        var (matches, err) = Glob(tt.fs, tt.pattern);
        if (err != default!) {
            Ꮡt.Errorf("Glob error for %q: %s"u8, tt.pattern, err);
            continue;
        }
        if (!slices.Contains(matches, tt.result)) {
            Ꮡt.Errorf("Glob(%#q) = %#v want %v"u8, tt.pattern, matches, tt.result);
        }
    }
    foreach (var (_, pattern) in new @string[]{"no_match"u8, "../*/no_match"u8, @"\*"u8}.slice()) {
        var (matches, err) = Glob(os.DirFS("."u8), pattern);
        if (err != default!) {
            Ꮡt.Errorf("Glob error for %q: %s"u8, pattern, err);
            continue;
        }
        if (len(matches) != 0) {
            Ꮡt.Errorf("Glob(%#q) = %#v want []"u8, pattern, matches);
        }
    }
}

public static void TestGlobError(ж<testing.T> Ꮡt) {
    var bad = new @string[]{@"[]"u8, @"nonexist/[]"u8}.slice();
    foreach (var (_, pattern) in bad) {
        var (_, err) = Glob(os.DirFS("."u8), pattern);
        if (!AreEqual(err, path.ErrBadPattern)) {
            Ꮡt.Errorf("Glob(fs, %#q) returned err=%v, want path.ErrBadPattern"u8, pattern, err);
        }
    }
}

public static void TestCVE202230630(ж<testing.T> Ꮡt) {
    // Prior to CVE-2022-30630, a stack exhaustion would occur given a large
    // number of separators. There is now a limit of 10,000.
    var (_, err) = Glob(os.DirFS("."u8), "/*"u8 + strings.Repeat("/"u8, 10001));
    if (!AreEqual(err, path.ErrBadPattern)) {
        Ꮡt.Fatalf("Glob returned err=%v, want %v"u8, err, path.ErrBadPattern);
    }
}

[GoType] partial struct globOnly {
    [GoEmbedded] public go.io.fs_package.GlobFS GlobFS;
}

internal static (fs.File, error) Open(this globOnly _, @string name) {
    return (default!, ErrNotExist);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string txtˢ = "*.txt"u8;
private static readonly @string readDirOnlyˢ = "readDirOnly"u8;
private static readonly @string openOnlyˢ = "openOnly"u8;

public static void TestGlobMethod(ж<testing.T> Ꮡt) {
    void check(@string desc, slice<@string> namesΔ1, error errΔ1) {
        Ꮡt.Helper();
        if (errΔ1 != default! || len(namesΔ1) != 1 || namesΔ1[0] != "hello.txt") {
            Ꮡt.Errorf("Glob(%s) = %v, %v, want %v, nil"u8, desc, namesΔ1, errΔ1, new @string[]{"hello.txt"u8}.slice());
        }
    }
    // Test that ReadDir uses the method when present.
    var (names, err) = Glob(new globOnly(new fstest_MapFSᴠGlobFS(testFsys)), txtˢ);
    check(readDirOnlyˢ, names, err);
    // Test that ReadDir uses Open when the method is not present.
    (names, err) = Glob(new openOnly(testFsys), txtˢ);
    check(openOnlyˢ, names, err);
}

} // end fs_test_package
