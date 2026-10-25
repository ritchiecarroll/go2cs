// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using static go.io.fs_package;
using testing = testing_package;
using fstest = go.testing.fstest_package;
using fs = go.io.fs_package;
using go.io;
using go.testing;

partial class fs_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string fooˢ = "foo"u8;
private static readonly @string barˢ = "bar"u8;
private static readonly @string dirParentlinkˢ = "dir/parentlink"u8;
private static readonly @string barˢ2 = "../bar"u8;
private static readonly @string dirLinkˢ = "dir/link"u8;
private static readonly @string fileˢ = "file"u8;
private static readonly @string dirˢ = "dir"u8;
private static readonly @string linkˢ = "link"u8;
private static readonly @string parentlinkˢ = "parentlink"u8;

public static void TestReadLink(ж<testing.T> Ꮡt) {
    var testFS = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["foo"u8] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("bar"u8),
            Mode: (fs.FileMode)(ModeSymlink | 511))),
        ["bar"u8] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("Hello, World!\n"u8),
            Mode: 420)),

        ["dir/parentlink"u8] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("../bar"u8),
            Mode: (fs.FileMode)(ModeSymlink | 511))),
        ["dir/link"u8] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("file"u8),
            Mode: (fs.FileMode)(ModeSymlink | 511))),
        ["dir/file"u8] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("Hello, World!\n"u8),
            Mode: 420))
    });

    void check(fs.FS fsys, @string name, @string want) {
        Ꮡt.Helper();
        var (got, errΔ1) = ReadLink(fsys, name);
        if (got != want || errΔ1 != default!) {
            Ꮡt.Errorf("ReadLink(%q) = %q, %v; want %q, <nil>"u8, name, got, errΔ1, want);
        }
    }

    check(new fstest_MapFSᴠFS(testFS), fooˢ, barˢ);
    check(new fstest_MapFSᴠFS(testFS), dirParentlinkˢ, barˢ2);
    check(new fstest_MapFSᴠFS(testFS), dirLinkˢ, fileˢ);

    // Test that ReadLink on Sub works.
    var (sub, err) = Sub(new fstest_MapFSᴠFS(testFS), dirˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    check(sub, linkˢ, fileˢ);
    check(sub, parentlinkˢ, barˢ2);
}

public static void TestLstat(ж<testing.T> Ꮡt) {
    var testFS = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["foo"u8] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("bar"u8),
            Mode: (fs.FileMode)(ModeSymlink | 511))),
        ["bar"u8] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("Hello, World!\n"u8),
            Mode: 420)),

        ["dir/parentlink"u8] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("../bar"u8),
            Mode: (fs.FileMode)(ModeSymlink | 511))),
        ["dir/link"u8] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("file"u8),
            Mode: (fs.FileMode)(ModeSymlink | 511))),
        ["dir/file"u8] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("Hello, World!\n"u8),
            Mode: 420))
    });

    void check(fs.FS fsys, @string name, fs.FileMode want) {
        Ꮡt.Helper();
        var (info, errΔ1) = Lstat(fsys, name);
        fs.FileMode got = default!;
        if (errΔ1 == default!) {
            got = info.Mode();
        }
        if (got != want || errΔ1 != default!) {
            Ꮡt.Errorf("Lstat(%q) = %v, %v; want %v, <nil>"u8, name, got, errΔ1, want);
        }
    }

    check(new fstest_MapFSᴠFS(testFS), fooˢ, (fs.FileMode)(ModeSymlink | 511));
    check(new fstest_MapFSᴠFS(testFS), barˢ, 420);

    // Test that Lstat on Sub works.
    var (sub, err) = Sub(new fstest_MapFSᴠFS(testFS), dirˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    check(sub, linkˢ, (fs.FileMode)(ModeSymlink | 511));
}

} // end fs_test_package
