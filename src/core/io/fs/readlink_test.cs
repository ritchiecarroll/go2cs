// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using static go.io.fs_package;
using testing = testing_package;
using fstest = go.testing.fstest_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = go.io.fs_package;
using go.io;
using go.testing;

partial class fs_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
private static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;
private static ref readonly @string dirParentlinkˢ => ref dirParentlinkˢᵇ;
private static readonly @string dirParentlinkˢᵇ = "dir/parentlink"u8;
private static ref readonly @string barˢ2 => ref barˢ2ᵇ;
private static readonly @string barˢ2ᵇ = "../bar"u8;
private static ref readonly @string dirLinkˢ => ref dirLinkˢᵇ;
private static readonly @string dirLinkˢᵇ = "dir/link"u8;
private static ref readonly @string fileˢ => ref fileˢᵇ;
private static readonly @string fileˢᵇ = "file"u8;
private static ref readonly @string dirˢ => ref dirˢᵇ;
private static readonly @string dirˢᵇ = "dir"u8;
private static ref readonly @string linkˢ => ref linkˢᵇ;
private static readonly @string linkˢᵇ = "link"u8;
private static ref readonly @string parentlinkˢ => ref parentlinkˢᵇ;
private static readonly @string parentlinkˢᵇ = "parentlink"u8;

public static partial void TestReadLink(ж<testing.T> t) {
    var testFS = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["foo"] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("bar"u8),
            Mode: (fs.FileMode)(ModeSymlink | 511))),
        ["bar"] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("Hello, World!\n"u8),
            Mode: 420)),

        ["dir/parentlink"] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("../bar"u8),
            Mode: (fs.FileMode)(ModeSymlink | 511))),
        ["dir/link"] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("file"u8),
            Mode: (fs.FileMode)(ModeSymlink | 511))),
        ["dir/file"] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("Hello, World!\n"u8),
            Mode: 420))
    });

    void check(fs.FS fsys, @string name, @string want) {
        t.Helper();
        var (got, errΔ1) = ReadLink(fsys, name);
        if (got != want || errΔ1 != nil) {
            t.Errorf("ReadLink(%q) = %q, %v; want %q, <nil>", name, got, errΔ1, want);
        }
    }

    check(new fstest_MapFSᴠFS(testFS), fooˢ, barˢ);
    check(new fstest_MapFSᴠFS(testFS), dirParentlinkˢ, barˢ2);
    check(new fstest_MapFSᴠFS(testFS), dirLinkˢ, fileˢ);

    // Test that ReadLink on Sub works.
    var (sub, err) = Sub(new fstest_MapFSᴠFS(testFS), dirˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    check(sub, linkˢ, fileˢ);
    check(sub, parentlinkˢ, barˢ2);
}

public static partial void TestLstat(ж<testing.T> t) {
    var testFS = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["foo"] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("bar"u8),
            Mode: (fs.FileMode)(ModeSymlink | 511))),
        ["bar"] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("Hello, World!\n"u8),
            Mode: 420)),

        ["dir/parentlink"] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("../bar"u8),
            Mode: (fs.FileMode)(ModeSymlink | 511))),
        ["dir/link"] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("file"u8),
            Mode: (fs.FileMode)(ModeSymlink | 511))),
        ["dir/file"] = Ꮡ(new fstest.MapFile(
            Data: slice<byte>("Hello, World!\n"u8),
            Mode: 420))
    });

    void check(fs.FS fsys, @string name, fs.FileMode want) {
        t.Helper();
        var (info, errΔ1) = Lstat(fsys, name);
        fs.FileMode got = default;
        if (errΔ1 == nil) {
            got = info.Mode();
        }
        if (got != want || errΔ1 != nil) {
            t.Errorf("Lstat(%q) = %v, %v; want %v, <nil>", name, got, errΔ1, want);
        }
    }

    check(new fstest_MapFSᴠFS(testFS), fooˢ, (fs.FileMode)(ModeSymlink | 511));
    check(new fstest_MapFSᴠFS(testFS), barˢ, 420);

    // Test that Lstat on Sub works.
    var (sub, err) = Sub(new fstest_MapFSᴠFS(testFS), dirˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    check(sub, linkˢ, (fs.FileMode)(ModeSymlink | 511));
}

} // end fs_test_package
