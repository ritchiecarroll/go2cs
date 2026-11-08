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
internal static ref readonly @string helloˢ => ref helloˢᵇ;
private static readonly @string helloˢᵇ = "hello"u8;
internal static ref readonly @string fortuneˢ => ref fortuneˢᵇ;
private static readonly @string fortuneˢᵇ = "fortune"u8;
internal static ref readonly @string fortuneKˢ => ref fortuneKˢᵇ;
private static readonly @string fortuneKˢᵇ = "fortune/k"u8;
internal static ref readonly @string fortuneKKenTxtˢ => ref fortuneKKenTxtˢᵇ;
private static readonly @string fortuneKKenTxtˢᵇ = "fortune/k/ken.txt"u8;

public static void TestMapFS(ж<testing.T> t) {
    var m = new MapFS(new map<@string, ж<global::go.testing.fstest_package.MapFile>>{
        ["hello"] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("hello, world\n"u8))),
        ["fortune/k/ken.txt"] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("If a program is too slow, it must have a loop.\n"u8)))
    });
    var err = TestFS(new fstest_internal_test_package.fstest_MapFSᴠFS(m), helloˢ, fortuneˢ, fortuneKˢ, fortuneKKenTxtˢ); if (err != nil) {
        t.Fatal(err);
    }
}

public static void TestMapFSChmodDot(ж<testing.T> t) {
    var m = new MapFS(new map<@string, ж<global::go.testing.fstest_package.MapFile>>{
        ["a/b.txt"] = Ꮡ(new MapFile(Mode: 438)),
        ["."] = Ꮡ(new MapFile(Mode: (fs.FileMode)(511 | fs.ModeDir)))
    });
    var buf = @new<strings.Builder>();
    var bufʗ1 = buf;
    fs.WalkDir(new fstest_internal_test_package.fstest_MapFSᴠFS(m), ".", error (@string path, fs.DirEntry d, error err) => {
        (var fi, err) = d.Info();
        if (err != nil) {
            return err;
        }
        fmt.Fprintf(new fstest_internal_test_package.strings_BuilderжWriter(bufʗ1), "%s: %v\n"u8, path, fi.Mode());
        return default;
    });
    @string want = """

.: drwxrwxrwx
a: dr-xr-xr-x
a/b.txt: -rw-rw-rw-

"""u8.Raw()[1..];
    @string got = buf.String();
    if (want != got) {
        t.Errorf("MapFS modes want:\n%s\ngot:\n%s\n", want, got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pathToBTxtˢ => ref pathToBTxtˢᵇ;
private static readonly @string pathToBTxtˢᵇ = "path/to/b.txt"u8;
internal static ref readonly @string bTxtˢ => ref bTxtˢᵇ;
private static readonly @string bTxtˢᵇ = "b.txt"u8;

public static void TestMapFSFileInfoName(ж<testing.T> t) {
    var m = new MapFS(new map<@string, ж<global::go.testing.fstest_package.MapFile>>{
        ["path/to/b.txt"] = Ꮡ(new MapFile())
    });
    var (info, _) = m.Stat(pathToBTxtˢ);
    @string want = bTxtˢ;
    @string got = info.Name();
    if (want != got) {
        t.Errorf("MapFS FileInfo.Name want:\n%s\ngot:\n%s\n", want, got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dirlinkˢ => ref dirlinkˢᵇ;
private static readonly @string dirlinkˢᵇ = "dirlink"u8;
internal static ref readonly @string kenTxtˢ => ref kenTxtˢᵇ;
private static readonly @string kenTxtˢᵇ = "ken.txt"u8;
internal static ref readonly @string linklinkˢ => ref linklinkˢᵇ;
private static readonly @string linklinkˢᵇ = "linklink"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string fileContentᶜ => ref fileContentᶜᵇ;
private static readonly @string fileContentᶜᵇ = "If a program is too slow, it must have a loop.\n"u8;

public static void TestMapFSSymlink(ж<testing.T> t) {
    @string fileContent = fileContentᶜ;
    var m = new MapFS(new map<@string, ж<global::go.testing.fstest_package.MapFile>>{
        ["fortune/k/ken.txt"] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>(fileContent))),
        ["dirlink"] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("fortune/k"u8), Mode: fs.ModeSymlink)),
        ["linklink"] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("dirlink"u8), Mode: fs.ModeSymlink)),
        ["ken.txt"] = Ꮡ(new global::go.testing.fstest_package.MapFile(Data: slice<byte>("dirlink/ken.txt"u8), Mode: fs.ModeSymlink))
    });
    var errΔ1 = TestFS(new fstest_internal_test_package.fstest_MapFSᴠFS(m), fortuneKKenTxtˢ, dirlinkˢ, kenTxtˢ, linklinkˢ); if (errΔ1 != nil) {
        t.Error(errΔ1);
    }

    var (gotData, err) = fs.ReadFile(new fstest_internal_test_package.fstest_MapFSᴠFS(m), kenTxtˢ);
    if (((sstring)gotData) != fileContent || err != nil) {
        t.Errorf("fs.ReadFile(m, \"ken.txt\") = %q, %v; want %q, <nil>", gotData, err, fileContent);
    }
    (var gotLink, err) = fs.ReadLink(new fstest_internal_test_package.fstest_MapFSᴠFS(m), dirlinkˢ);
    {
        @string want = fortuneKˢ; if (gotLink != want || err != nil) {
            t.Errorf("fs.ReadLink(m, \"dirlink\") = %q, %v; want %q, <nil>", gotLink, err, fileContent);
        }
    }
    (var gotInfo, err) = fs.Lstat(new fstest_internal_test_package.fstest_MapFSᴠFS(m), dirlinkˢ);
    if (err != nil) {
        t.Errorf("fs.Lstat(m, \"dirlink\") = _, %v; want _, <nil>", err);
    } else {
        {
            @string got = gotInfo.Name();
            @string want = dirlinkˢ; if (got != want) {
                t.Errorf("fs.Lstat(m, \"dirlink\").Name() = %q; want %q", got, want);
            }
        }
        {
            var (got, want) = (gotInfo.Mode(), fs.ModeSymlink); if (got != want) {
                t.Errorf("fs.Lstat(m, \"dirlink\").Mode() = %v; want %v", got, want);
            }
        }
    }
    (gotInfo, err) = fs.Stat(new fstest_internal_test_package.fstest_MapFSᴠFS(m), dirlinkˢ);
    if (err != nil) {
        t.Errorf("fs.Stat(m, \"dirlink\") = _, %v; want _, <nil>", err);
    } else {
        {
            @string got = gotInfo.Name();
            @string want = dirlinkˢ; if (got != want) {
                t.Errorf("fs.Stat(m, \"dirlink\").Name() = %q; want %q", got, want);
            }
        }
        {
            var (got, want) = (gotInfo.Mode(), (fs.FileMode)(fs.ModeDir | 365)); if (got != want) {
                t.Errorf("fs.Stat(m, \"dirlink\").Mode() = %v; want %v", got, want);
            }
        }
    }
    (gotInfo, err) = fs.Lstat(new fstest_internal_test_package.fstest_MapFSᴠFS(m), linklinkˢ);
    if (err != nil) {
        t.Errorf("fs.Lstat(m, \"linklink\") = _, %v; want _, <nil>", err);
    } else {
        {
            @string got = gotInfo.Name();
            @string want = linklinkˢ; if (got != want) {
                t.Errorf("fs.Lstat(m, \"linklink\").Name() = %q; want %q", got, want);
            }
        }
        {
            var (got, want) = (gotInfo.Mode(), fs.ModeSymlink); if (got != want) {
                t.Errorf("fs.Lstat(m, \"linklink\").Mode() = %v; want %v", got, want);
            }
        }
    }
    (gotInfo, err) = fs.Stat(new fstest_internal_test_package.fstest_MapFSᴠFS(m), linklinkˢ);
    if (err != nil) {
        t.Errorf("fs.Stat(m, \"linklink\") = _, %v; want _, <nil>", err);
    } else {
        {
            @string got = gotInfo.Name();
            @string want = linklinkˢ; if (got != want) {
                t.Errorf("fs.Stat(m, \"linklink\").Name() = %q; want %q", got, want);
            }
        }
        {
            var (got, want) = (gotInfo.Mode(), (fs.FileMode)(fs.ModeDir | 365)); if (got != want) {
                t.Errorf("fs.Stat(m, \"linklink\").Mode() = %v; want %v", got, want);
            }
        }
    }
}

} // end fstest_internal_test_package
