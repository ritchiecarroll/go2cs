// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using errors = errors_package;
using static go.io.fs_package;
using testing = testing_package;
using fs = go.io.fs_package;
using go.io;

partial class fs_test_package {

[GoType] partial struct subOnly {
    [GoEmbedded] public go.io.fs_package.SubFS SubFS;
}

internal static (fs.File, error) Open(this subOnly _, @string name) {
    return (default!, ErrNotExist);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goodbyeTxtˢ = "goodbye.txt"u8;
private static readonly object goodbyeWorldˢ = (@string)"goodbye, world"u8;
private static readonly @string subˢ2 = "sub"u8;
private static readonly @string subOnlyˢ = "subOnly"u8;
private static readonly @string nonexistˢ = "nonexist"u8;
private static readonly object openNonexistSucceededˢ = (@string)"Open(nonexist): succeeded"u8;

public static void TestSub(ж<testing.T> Ꮡt) {
    void check(@string desc, fs.FS subΔ1, error errΔ1) {
        Ꮡt.Helper();
        if (errΔ1 != default!) {
            Ꮡt.Errorf("Sub(sub): %v"u8, errΔ1);
            return;
        }
        (var data, errΔ1) = ReadFile(subΔ1, goodbyeTxtˢ);
        if (((sstring)data) != "goodbye, world"u8 || errΔ1 != default!) {
            Ꮡt.Errorf(@"ReadFile(%s, ""goodbye.txt"" = %q, %v, want %q, nil"u8, desc, ((@string)data), errΔ1, goodbyeWorldˢ);
        }
        (var dirs, errΔ1) = ReadDir(subΔ1, "."u8);
        if (errΔ1 != default! || len(dirs) != 1 || dirs[0].Name() != "goodbye.txt"u8) {
            slice<@string> names = default!;
            foreach (var (_, d) in dirs) {
                names = append(names, d.Name());
            }
            Ꮡt.Errorf(@"ReadDir(%s, ""."") = %v, %v, want %v, nil"u8, desc, names, errΔ1, new @string[]{"goodbye.txt"u8}.slice());
        }
    }
    // Test that Sub uses the method when present.
    var (sub, err) = Sub(new subOnly(new fstest_MapFSᴠSubFS(testFsys)), subˢ2);
    check(subOnlyˢ, sub, err);
    // Test that Sub uses Open when the method is not present.
    (sub, err) = Sub(new openOnly(testFsys), subˢ2);
    check(openOnlyˢ, sub, err);
    (_, err) = sub.Open(nonexistˢ);
    if (err == default!) {
        Ꮡt.Fatal(openNonexistSucceededˢ);
    }
    var (pe, ok) = err._<ж<fs.PathError>>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("Open(nonexist): error is %T, want *PathError"u8, err);
    }
    if ((~pe).Path != "nonexist"u8) {
        Ꮡt.Fatalf("Open(nonexist): err.Path = %q, want %q"u8, (~pe).Path, nonexistˢ);
    }
    (_, err) = sub.Open("./"u8);
    if (!errors.Is(err, ErrInvalid)) {
        Ꮡt.Fatalf("Open(./): error is %v, want %v"u8, err, ErrInvalid);
    }
}

} // end fs_test_package
