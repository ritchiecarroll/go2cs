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
using fs = go.io.fs_package;
using go.io;
using go.testing;

partial class fs_test_package {

partial struct readDirOnly {
    /*embed*/ public go.io.fs_package.ReadDirFS ReadDirFS;
}

internal static (fs.File, error) Open(this readDirOnly _, @string name) {
    return (default!, ErrNotExist);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string subˢ = "sub(.)"u8;

public static void TestReadDir(ж<testing.T> Ꮡt) {
    void check(@string desc, slice<fs.DirEntry> dirsΔ1, error errΔ1) {
        Ꮡt.Helper();
        if (errΔ1 != default! || len(dirsΔ1) != 2 || dirsΔ1[0].Name() != "hello.txt"u8 || dirsΔ1[1].Name() != "sub"u8) {
            slice<@string> names = default!;
            foreach (var (_, d) in dirsΔ1) {
                names = append(names, d.Name());
            }
            Ꮡt.Errorf("ReadDir(%s) = %v, %v, want %v, nil"u8, desc, names, errΔ1, new @string[]{"hello.txt"u8, "sub"u8}.slice());
        }
    }
    // Test that ReadDir uses the method when present.
    var (dirs, err) = ReadDir(new readDirOnly(new fstest_MapFSᴠReadDirFS(testFsys)), "."u8);
    check(readDirOnlyˢ, dirs, err);
    // Test that ReadDir uses Open when the method is not present.
    (dirs, err) = ReadDir(new openOnly(testFsys), "."u8);
    check(openOnlyˢ, dirs, err);
    // Test that ReadDir on Sub of . works (sub_test checks non-trivial subs).
    (var sub, err) = Sub(testFsys, "."u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (dirs, err) = ReadDir(sub, "."u8);
    check(subˢ, dirs, err);
}

internal partial struct TestFileInfoToDirEntry_tests /*dyn*/ {
    internal @string path;
    internal fs.FileMode wantMode;
    internal bool wantDir;
}

public static void TestFileInfoToDirEntry(ж<testing.T> Ꮡt) {
    var testFs = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["notadir.txt"u8] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("hello, world"u8),
            Mode: 0,
            ModTime: time.Now(),
            Sys: ᏑsysValue)),
        ["adir"u8] = Ꮡ(new fstest.MapFile(
            Data: default!,
            Mode: os.ModeDir,
            ModTime: time.Now(),
            Sys: ᏑsysValue))
    });
    var tests = new TestFileInfoToDirEntry_tests[]{
        new(path: "notadir.txt"u8, wantMode: 0, wantDir: false),
        new(path: "adir"u8, wantMode: os.ModeDir, wantDir: true)
    }.slice();
    foreach (var (_, test) in tests) {
        ref var testΔ1 = ref heap<TestFileInfoToDirEntry_tests>(out var ᏑtestΔ1);
        testΔ1 = test;
        var testʗ1 = testΔ1;
        var testFsʗ1 = testFs;
        Ꮡt.Run(testΔ1.path, (ж<testing.T> tΔ1) => {
            var (fi, err) = Stat(testFsʗ1, testʗ1.path);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var dirEntry = FileInfoToDirEntry(fi);
            {
                var (g, w) = (dirEntry.Type(), testʗ1.wantMode); if (g != w) {
                    tΔ1.Errorf("FileMode mismatch: got=%v, want=%v"u8, g, w);
                }
            }
            {
                @string g = dirEntry.Name();
                @string w = testʗ1.path; if (g != w) {
                    tΔ1.Errorf("Name mismatch: got=%v, want=%v"u8, g, w);
                }
            }
            {
                var (g, w) = (dirEntry.IsDir(), testʗ1.wantDir); if (g != w) {
                    tΔ1.Errorf("IsDir mismatch: got=%v, want=%v"u8, g, w);
                }
            }
        });
    }
}

internal static @string errorPath(error err) {
    ref var perr = ref heap<ж<fs.PathError>>(out var Ꮡperr);
    if (!errors.As(err, Ꮡperr)) {
        return ""u8;
    }
    return (~perr).Path;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nonExistentˢ = "non-existent"u8;

internal partial struct TestReadDirPath_fsys /*dyn*/ {
    /*embed*/ public go.io.fs_package.FS FS;
}

public static void TestReadDirPath(ж<testing.T> Ꮡt) {
    var fsys = os.DirFS(Ꮡt.TempDir());
    var (_, err1) = ReadDir(fsys, nonExistentˢ);
    var (_, err2) = ReadDir(new TestReadDirPath_fsys(fsys), nonExistentˢ);
    {
        @string s1 = errorPath(err1);
        @string s2 = errorPath(err2); if (s1 != s2) {
            Ꮡt.Fatalf("s1: %s != s2: %s"u8, s1, s2);
        }
    }
}

} // end fs_test_package
