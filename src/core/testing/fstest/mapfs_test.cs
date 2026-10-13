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
        var err = TestFS(m, helloˢ, fortuneˢ, fortuneKˢ, fortuneKKenTxtˢ); if (err != default!) {
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
    fs.WalkDir(m, "."u8, error (@string path, fs.DirEntry d, error err) => {
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

} // end fstest_internal_test_package
