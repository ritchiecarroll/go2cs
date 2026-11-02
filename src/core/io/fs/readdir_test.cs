// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using errors = errors_package;
using static go.io.fs_package;
using os = os_package;
using testing = testing_package;
using fstest = go.testing.fstest_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = go.io.fs_package;
using go.io;
using go.testing;

partial class fs_test_package {

partial struct readDirOnly {
    /*embed*/ public go.io.fs_package.ReadDirFS ReadDirFS;
}

internal static (fs.File, error) Open(this readDirOnly _, @string name) {
    return (default, ErrNotExist);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string subˢ => ref subˢᵇ;
private static readonly @string subˢᵇ = "sub(.)"u8;

public static partial void TestReadDir(ж<testing.T> t) {
    void check(@string desc, slice<fs.DirEntry> dirsΔ1, error errΔ1) {
        t.Helper();
        if (errΔ1 != nil || len(dirsΔ1) != 2 || dirsΔ1[0].Name() != "hello.txt"u8 || dirsΔ1[1].Name() != "sub"u8) {
            slice<@string> names = default;
            foreach (var (_, d) in dirsΔ1) {
                names = append(names, d.Name());
            }
            t.Errorf("ReadDir(%s) = %v, %v, want %v, nil", desc, names, errΔ1, slice<@string>(["hello.txt", "sub"]));
        }
    }

    // Test that ReadDir uses the method when present.
    var (dirs, err) = ReadDir(new readDirOnly(new fstest_MapFSᴠReadDirFS(testFsys)), ".");
    check(readDirOnlyˢ, dirs, err);

    // Test that ReadDir uses Open when the method is not present.
    (dirs, err) = ReadDir(new openOnly(new fstest_MapFSᴠFS(testFsys)), ".");
    check(openOnlyˢ, dirs, err);

    // Test that ReadDir on Sub of . works (sub_test checks non-trivial subs).
    (var sub, err) = Sub(new fstest_MapFSᴠFS(testFsys), ".");
    if (err != nil) {
        t.Fatal(err);
    }
    (dirs, err) = ReadDir(sub, ".");
    check(subˢ, dirs, err);
}

internal partial struct TestFileInfoToDirEntry_tests /*dyn*/ {
    internal @string path;
    internal fs.FileMode wantMode;
    internal bool wantDir;
}

public static void TestFileInfoToDirEntry(ж<testing.T> t) {
    var testFs = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["notadir.txt"] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("hello, world"u8),
            Mode: 0,
            ModTime: time.Now(),
            Sys: ᏑsysValue)),
        ["adir"] = Ꮡ(new fstest.MapFile(
            Data: default,
            Mode: os.ModeDir,
            ModTime: time.Now(),
            Sys: ᏑsysValue))
    });

    var tests = slice<TestFileInfoToDirEntry_tests>([
        new(path: "notadir.txt", wantMode: 0, wantDir: false),
        new(path: "adir", wantMode: os.ModeDir, wantDir: true)
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestFileInfoToDirEntry_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var testFsʗ1 = testFs;
        t.Run(test.path, (ж<testing.T> tΔ1) => {
            var (fi, err) = Stat(new fstest_MapFSᴠFS(testFsʗ1), testʗ1.path);
            if (err != nil) {
                tΔ1.Fatal(err);
            }

            var dirEntry = FileInfoToDirEntry(fi);
            {
                var (g, w) = (dirEntry.Type(), testʗ1.wantMode); if (g != w) {
                    tΔ1.Errorf("FileMode mismatch: got=%v, want=%v", g, w);
                }
            }
            {
                @string g = dirEntry.Name();
                @string w = testʗ1.path; if (g != w) {
                    tΔ1.Errorf("Name mismatch: got=%v, want=%v", g, w);
                }
            }
            {
                var (g, w) = (dirEntry.IsDir(), testʗ1.wantDir); if (g != w) {
                    tΔ1.Errorf("IsDir mismatch: got=%v, want=%v", g, w);
                }
            }
        });
    }
}

internal static @string errorPath(error err) {
    var (perr, ok) = errors.AsType<ж<fs.PathError>>(err);
    if (!ok) {
        return ""u8;
    }
    return perr.Path;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string nonExistentˢ => ref nonExistentˢᵇ;
private static readonly @string nonExistentˢᵇ = "non-existent"u8;

internal partial struct TestReadDirPath_fsys /*dyn*/ {
    /*embed*/ public go.io.fs_package.FS FS;
}

public static void TestReadDirPath(ж<testing.T> t) {
    var fsys = os.DirFS(t.TempDir());
    var (_, err1) = ReadDir(fsys, nonExistentˢ);
    var (_, err2) = ReadDir(new TestReadDirPath_fsys(fsys), nonExistentˢ);
    {
        @string s1 = errorPath(err1);
        @string s2 = errorPath(err2); if (s1 != s2) {
            t.Fatalf("s1: %s != s2: %s", s1, s2);
        }
    }
}

} // end fs_test_package
