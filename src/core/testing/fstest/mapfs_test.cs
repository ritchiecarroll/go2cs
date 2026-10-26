// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using fmt = fmt_package;
using fs = go.io.fs_package;
using strings = strings_package;
using testing = testing_package;
using go.io;
using io = io_package;
using static go.testing.fstest_package;

partial class fstest_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloˢ = "hello"u8;
internal static readonly @string fortuneˢ = "fortune"u8;
internal static readonly @string fortuneKˢ = "fortune/k"u8;
internal static readonly @string fortuneKKenTxtˢ = "fortune/k/ken.txt"u8;

public static void TestMapFS(ж<testing.T> Ꮡt) {
    var m = new MapFS(new map<@string, ж<global::go.testing.fstest_package.MapFile>>{
        ["hello"u8] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("hello, world\n"u8))),
        ["fortune/k/ken.txt"u8] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("If a program is too slow, it must have a loop.\n"u8)))
    });
    {
        var err = TestFS(new fstest_internal_test_package.fstest_MapFSᴠFS(m), helloˢ, fortuneˢ, fortuneKˢ, fortuneKKenTxtˢ); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

public static void TestMapFSChmodDot(ж<testing.T> Ꮡt) {
    var m = new MapFS(new map<@string, ж<global::go.testing.fstest_package.MapFile>>{
        ["a/b.txt"u8] = Ꮡ(new MapFile(Mode: 438)),
        ["."u8] = Ꮡ(new MapFile(Mode: (fs.FileMode)(511 | fs.ModeDir)))
    });
    var buf = @new<strings.Builder>();
    var bufʗ1 = buf;
    fs.WalkDir(new fstest_internal_test_package.fstest_MapFSᴠFS(m), "."u8, error (@string path, fs.DirEntry d, error err) => {
        (var fi, err) = d.Info();
        if (err != default!) {
            return err;
        }
        fmt.Fprintf(new fstest_internal_test_package.strings_BuilderжWriter(bufʗ1), "%s: %v\n"u8, path, fi.Mode());
        return default!;
    });
    @string want = """

.: drwxrwxrwx
a: dr-xr-xr-x
a/b.txt: -rw-rw-rw-

"""u8.Raw()[1..];
    @string got = buf.String();
    if (want != got) {
        Ꮡt.Errorf("MapFS modes want:\n%s\ngot:\n%s\n"u8, want, got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pathToBTxtˢ = "path/to/b.txt"u8;
internal static readonly @string bTxtˢ = "b.txt"u8;

public static void TestMapFSFileInfoName(ж<testing.T> Ꮡt) {
    var m = new MapFS(new map<@string, ж<global::go.testing.fstest_package.MapFile>>{
        ["path/to/b.txt"u8] = Ꮡ(new MapFile(nil))
    });
    var (info, _) = m.Stat(pathToBTxtˢ);
    @string want = bTxtˢ;
    @string got = info.Name();
    if (want != got) {
        Ꮡt.Errorf("MapFS FileInfo.Name want:\n%s\ngot:\n%s\n"u8, want, got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dirlinkˢ = "dirlink"u8;
internal static readonly @string kenTxtˢ = "ken.txt"u8;
internal static readonly @string linklinkˢ = "linklink"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fileContentᶜ = "If a program is too slow, it must have a loop.\n"u8;

public static void TestMapFSSymlink(ж<testing.T> Ꮡt) {
    @string fileContent = fileContentᶜ;
    var m = new MapFS(new map<@string, ж<global::go.testing.fstest_package.MapFile>>{
        ["fortune/k/ken.txt"u8] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>(fileContent))),
        ["dirlink"u8] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("fortune/k"u8), Mode: fs.ModeSymlink)),
        ["linklink"u8] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("dirlink"u8), Mode: fs.ModeSymlink)),
        ["ken.txt"u8] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("dirlink/ken.txt"u8), Mode: fs.ModeSymlink))
    });
    {
        var errΔ1 = TestFS(new fstest_internal_test_package.fstest_MapFSᴠFS(m), fortuneKKenTxtˢ, dirlinkˢ, kenTxtˢ, linklinkˢ); if (errΔ1 != default!) {
            Ꮡt.Error(errΔ1);
        }
    }

    var (gotData, err) = fs.ReadFile(new fstest_internal_test_package.fstest_MapFSᴠFS(m), kenTxtˢ);
    if (((sstring)gotData) != fileContent || err != default!) {
        Ꮡt.Errorf("fs.ReadFile(m, \"ken.txt\") = %q, %v; want %q, <nil>"u8, gotData, err, fileContent);
    }
    (var gotLink, err) = fs.ReadLink(new fstest_internal_test_package.fstest_MapFSᴠFS(m), dirlinkˢ);
    {
        @string want = fortuneKˢ; if (gotLink != want || err != default!) {
            Ꮡt.Errorf("fs.ReadLink(m, \"dirlink\") = %q, %v; want %q, <nil>"u8, gotLink, err, fileContent);
        }
    }
    (var gotInfo, err) = fs.Lstat(new fstest_internal_test_package.fstest_MapFSᴠFS(m), dirlinkˢ);
    if (err != default!){
        Ꮡt.Errorf("fs.Lstat(m, \"dirlink\") = _, %v; want _, <nil>"u8, err);
    } else {
        {
            @string got = gotInfo.Name();
            @string want = dirlinkˢ; if (got != want) {
                Ꮡt.Errorf("fs.Lstat(m, \"dirlink\").Name() = %q; want %q"u8, got, want);
            }
        }
        {
            var (got, want) = (gotInfo.Mode(), fs.ModeSymlink); if (got != want) {
                Ꮡt.Errorf("fs.Lstat(m, \"dirlink\").Mode() = %v; want %v"u8, got, want);
            }
        }
    }
    (gotInfo, err) = fs.Stat(new fstest_internal_test_package.fstest_MapFSᴠFS(m), dirlinkˢ);
    if (err != default!){
        Ꮡt.Errorf("fs.Stat(m, \"dirlink\") = _, %v; want _, <nil>"u8, err);
    } else {
        {
            @string got = gotInfo.Name();
            @string want = dirlinkˢ; if (got != want) {
                Ꮡt.Errorf("fs.Stat(m, \"dirlink\").Name() = %q; want %q"u8, got, want);
            }
        }
        {
            var (got, want) = (gotInfo.Mode(), (fs.FileMode)(fs.ModeDir | 365)); if (got != want) {
                Ꮡt.Errorf("fs.Stat(m, \"dirlink\").Mode() = %v; want %v"u8, got, want);
            }
        }
    }
    (gotInfo, err) = fs.Lstat(new fstest_internal_test_package.fstest_MapFSᴠFS(m), linklinkˢ);
    if (err != default!){
        Ꮡt.Errorf("fs.Lstat(m, \"linklink\") = _, %v; want _, <nil>"u8, err);
    } else {
        {
            @string got = gotInfo.Name();
            @string want = linklinkˢ; if (got != want) {
                Ꮡt.Errorf("fs.Lstat(m, \"linklink\").Name() = %q; want %q"u8, got, want);
            }
        }
        {
            var (got, want) = (gotInfo.Mode(), fs.ModeSymlink); if (got != want) {
                Ꮡt.Errorf("fs.Lstat(m, \"linklink\").Mode() = %v; want %v"u8, got, want);
            }
        }
    }
    (gotInfo, err) = fs.Stat(new fstest_internal_test_package.fstest_MapFSᴠFS(m), linklinkˢ);
    if (err != default!){
        Ꮡt.Errorf("fs.Stat(m, \"linklink\") = _, %v; want _, <nil>"u8, err);
    } else {
        {
            @string got = gotInfo.Name();
            @string want = linklinkˢ; if (got != want) {
                Ꮡt.Errorf("fs.Stat(m, \"linklink\").Name() = %q; want %q"u8, got, want);
            }
        }
        {
            var (got, want) = (gotInfo.Mode(), (fs.FileMode)(fs.ModeDir | 365)); if (got != want) {
                Ꮡt.Errorf("fs.Stat(m, \"linklink\").Mode() = %v; want %v"u8, got, want);
            }
        }
    }
}

} // end fstest_internal_test_package
